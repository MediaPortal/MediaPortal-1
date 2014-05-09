#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.ChannelLinkage;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// Base class for all tuners.
  /// </summary>
  internal abstract class TvCardBase : ITunerInternal, ITVCard, IDisposable
  {
    #region events

    /// <summary>
    /// New subchannel delegate, invoked when a new subchannel is created.
    /// </summary>
    private OnNewSubChannelDelegate _newSubChannelEventDelegate = null;

    /// <summary>
    /// Set the tuner's new subchannel event handler.
    /// </summary>
    /// <value>the delegate</value>
    public event OnNewSubChannelDelegate OnNewSubChannelEvent
    {
      add
      {
        _newSubChannelEventDelegate = null;
        _newSubChannelEventDelegate += value;
      }
      remove
      {
        _newSubChannelEventDelegate = null;
      }
    }

    /// <summary>
    /// Fire the new subchannel observer event.
    /// </summary>
    /// <param name="subChannelId">The ID of the new subchannel.</param>
    protected void FireNewSubChannelEvent(int subChannelId)
    {
      if (_newSubChannelEventDelegate != null)
      {
        _newSubChannelEventDelegate(subChannelId);
      }
    }

    /// <summary>
    /// After tune delegate, fired after tuning is complete.
    /// </summary>
    private OnAfterTuneDelegate _afterTuneEventDelegate;

    /// <summary>
    /// Set the tuner's after tune event handler.
    /// </summary>
    /// <value>the delegate</value>
    public event OnAfterTuneDelegate OnAfterTuneEvent
    {
      add
      {
        _afterTuneEventDelegate = null;
        _afterTuneEventDelegate += value;
      }
      remove
      {
        _afterTuneEventDelegate = null;
      }
    }

    /// <summary>
    /// Fire the after tune observer event.
    /// </summary>
    private void FireAfterTuneEvent()
    {
      if (_afterTuneEventDelegate != null)
      {
        _afterTuneEventDelegate();
      }
    }

    #endregion

    #region variables

    /// <summary>
    /// Scanning Paramters
    /// </summary>
    private ScanParameters _parameters = new ScanParameters();

    /// <summary>
    /// Dictionary of the corresponding sub channels
    /// </summary>
    private Dictionary<int, ITvSubChannel> _mapSubChannels = new Dictionary<int, ITvSubChannel>();

    /// <summary>
    /// Context reference
    /// </summary>
    private object _context;

    /// <summary>
    /// Indicates, if the tuner is locked
    /// </summary>
    protected volatile bool _isSignalLocked;

    /// <summary>
    /// Value of the signal level
    /// </summary>
    protected int _signalLevel;

    /// <summary>
    /// Value of the signal quality
    /// </summary>
    protected int _signalQuality;

    /// <summary>
    /// The unique external identifier for the tuner.
    /// </summary>
    /// <remarks>
    /// The source of this identifier varies from class to class. DirectShow
    /// tuners may use the IMoniker display name (AKA device path).
    /// </remarks>
    private string _externalId;

    /// <summary>
    /// Name of the tv card
    /// </summary>
    protected string _name;

    /// <summary>
    /// Indicator: is the tuner scanning?
    /// </summary>
    protected bool _isScanning = false;

    /// <summary>
    /// The tuner type (eg. DVB-S, DVB-T... etc.).
    /// </summary>
    protected CardType _tunerType;

    /// <summary>
    /// Date and time of the last signal update
    /// </summary>
    protected DateTime _lastSignalUpdate = DateTime.MinValue;

    /// <summary>
    /// Indicates, if the signal is present
    /// </summary>
    protected bool _isSignalPresent;

    /// <summary>
    /// The db card id
    /// </summary>
    protected int _tunerId;

    /// <summary>
    /// The action that will be taken when the tuner is no longer being actively used.
    /// </summary>
    private IdleMode _idleMode = IdleMode.Stop;

    /// <summary>
    /// A list containing the extension interfaces supported by this tuner. The list is ordered by
    /// descending extension priority.
    /// </summary>
    protected List<ICustomDevice> _extensions = new List<ICustomDevice>();

    /// <summary>
    /// A list containing the conditional access provider extensions supported by this tuner. The
    /// list is ordered by descending extension priority.
    /// </summary>
    private List<IConditionalAccessProvider> _caProviders = new List<IConditionalAccessProvider>();

    /// <summary>
    /// Enable or disable the use of conditional access interface(s).
    /// </summary>
    private bool _useConditionalAccessInterface = true;

    /// <summary>
    /// The type of conditional access module available to the conditional access interface.
    /// </summary>
    /// <remarks>
    /// Certain conditional access modules require specific handling to ensure compatibility.
    /// </remarks>
    private CamType _camType = CamType.Default;

    /// <summary>
    /// The number of channels that the device is capable of or permitted to decrypt simultaneously. Zero means
    /// there is no limit.
    /// </summary>
    private int _decryptLimit = 0;

    /// <summary>
    /// The method that should be used to communicate the set of channels that the tuner's conditional access
    /// interface needs to manage.
    /// </summary>
    /// <remarks>
    /// Multi-channel decrypt is *not* the same as Digital Devices' multi-transponder decrypt (MTD). MCD is a
    /// implmented using standard CA PMT commands; MTD is implemented in the Digital Devices drivers.
    /// Disabled = Always send Only. In most cases this will result in only one channel being decrypted. If other
    ///         methods are not working reliably then this one should at least allow decrypting one channel
    ///         reliably.
    /// List = Explicit management using Only, First, More and Last. This is the most widely supported set
    ///         of commands, however they are not suitable for some interfaces (such as the Digital Devices
    ///         interface).
    /// Changes = Use Add, Update and Remove to pass changes to the interface. The full channel list is never
    ///         passed. Most interfaces don't support these commands.
    /// </remarks>
    private MultiChannelDecryptMode _multiChannelDecryptMode = MultiChannelDecryptMode.List;

    /// <summary>
    /// Enable or disable waiting for the conditional interface to be ready before sending commands.
    /// </summary>
    private bool _waitUntilCaInterfaceReady = true;

    /// <summary>
    /// The number of times to re-attempt decrypting the current service set when one or more services are
    /// not able to be decrypted for whatever reason.
    /// </summary>
    /// <remarks>
    /// Each available CA interface will be tried in order of priority. If decrypting is not started
    /// successfully, all interfaces are retried until each interface has been tried
    /// _decryptFailureRetryCount + 1 times, or until decrypting is successful.
    /// </remarks>
    private int _decryptFailureRetryCount = 2;

    /// <summary>
    /// The device's current tuning parameter values or null if the device is not tuned.
    /// </summary>
    protected IChannel _currentTuningDetail = null;

    /// <summary>
    /// Enable or disable the use of extensions for tuning.
    /// </summary>
    /// <remarks>
    /// Custom/direct tuning *may* be faster or more reliable than regular tuning methods. It might
    /// be slower (eg. TeVii) or more limiting (eg. Digital Everywhere) than regular tuning
    /// methods. User gets to choose which method to use.
    /// </remarks>
    private bool _useCustomTuning = false;

    /// <summary>
    /// A flag used by the TV service as a signal to abort the tuning process before it is completed.
    /// </summary>
    private volatile bool _cancelTune = false;

    /// <summary>
    /// The current state of the tuner.
    /// </summary>
    protected TunerState _state = TunerState.NotLoaded;

    /// <summary>
    /// Does the tuner support receiving more than one service simultaneously?
    /// </summary>
    /// <remarks>
    /// This may seem obvious and unnecessary, especially for modern tuners. However even today
    /// there are tuners that cannot receive more than one service simultaneously. CableCARD tuners
    /// are a good example.
    /// </remarks>
    protected bool _supportsSubChannels = true;

    /// <summary>
    /// A shared identifier for all tuner instances derived from a [multi-tuner] product.
    /// </summary>
    protected string _productInstanceId = null;

    /// <summary>
    /// A shared identifier for all tuner instances derived from a single physical tuner.
    /// </summary>
    protected string _tunerInstanceId = null;

    /// <summary>
    /// The tuner group that this tuner is a member of, if any.
    /// </summary>
    private ITunerGroup _group = null;

    /// <summary>
    /// The tuner's channel scanning interface.
    /// </summary>
    protected ITVScanning _channelScanner = null;

    /// <summary>
    /// The tuner's EPG grabber interface.
    /// </summary>
    protected IEpgGrabber _epgGrabber = null;

    /// <summary>
    /// The tuner's DiSEqC control interface.
    /// </summary>
    protected IDiseqcController _diseqcController = null;

    /// <summary>
    /// The tuner's encoder control interface.
    /// </summary>
    protected IQuality _encoderController = null;

    #endregion

    #region constructor

    /// <summary>
    /// Base constructor
    /// </summary>
    /// <param name="name">The device name.</param>
    /// <param name="externalId">An identifier for the device. Useful for distinguishing this instance from other devices of the same type.</param>
    protected TvCardBase(string name, string externalId)
    {
      _name = name;
      _externalId = externalId;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public virtual void Dispose()
    {
      Unload();
    }

    #endregion

    #region properties

    /// <summary>
    /// Get the tuner's unique identifier.
    /// </summary>
    public int TunerId
    {
      get
      {
        return _tunerId;
      }
    }

    /// <summary>
    /// Get or set the tuner's group.
    /// </summary>
    public ITunerGroup Group
    {
      get
      {
        return _group;
      }
      set
      {
        _group = value;
      }
    }

    /// <summary>
    /// Gets or sets the parameters.
    /// </summary>
    /// <value>A set of timeout parameters used for tuning and scanning.</value>
    public ScanParameters Parameters
    {
      get { return _parameters; }
      set
      {
        _parameters = value;
        Dictionary<int, ITvSubChannel>.Enumerator en = _mapSubChannels.GetEnumerator();
        while (en.MoveNext())
        {
          en.Current.Value.Parameters = value;
        }
      }
    }

    /// <summary>
    /// Get the tuner's name.
    /// </summary>
    public string Name
    {
      get
      {
        return _name;
      }
    }

    /// <summary>
    /// Get the tuner's unique external identifier.
    /// </summary>
    public string ExternalId
    {
      get
      {
        return _externalId;
      }
    }

    /// <summary>
    /// Get the tuner's product instance identifier.
    /// </summary>
    public string ProductInstanceId
    {
      get
      {
        return _productInstanceId;
      }
    }

    /// <summary>
    /// Get the tuner's instance identifier.
    /// </summary>
    public string TunerInstanceId
    {
      get
      {
        return _tunerInstanceId;
      }
    }

    /// <summary>
    /// Gets the device tuner type.
    /// </summary>
    public CardType CardType
    {
      get { return _tunerType; }
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      return _name;
    }

    #region conditional access properties

    /// <summary>
    /// Get/set the type of conditional access module available to the conditional access interface.
    /// </summary>
    /// <value>The type of the cam.</value>
    public CamType CamType
    {
      get
      {
        return _camType;
      }
    }

    /// <summary>
    /// Get the device's conditional access interface decrypt limit. This is usually the number of channels
    /// that the interface is able to decrypt simultaneously. A value of zero indicates that the limit is
    /// to be ignored.
    /// </summary>
    public int DecryptLimit
    {
      get
      {
        return _decryptLimit;
      }
    }

    /// <summary>
    /// Does the tuner support conditional access?
    /// </summary>
    /// <value><c>true</c> if the tuner supports conditional access, otherwise <c>false</c></value>
    public bool IsConditionalAccessSupported
    {
      get
      {
        if (!_useConditionalAccessInterface)
        {
          return false;
        }
        if (_state == TunerState.NotLoaded)
        {
          Load();
        }
        return _caProviders.Count > 0;
      }
    }

    /// <summary>
    /// Get the device's conditional access menu interaction interface. This interface is only applicable if
    /// conditional access is supported.
    /// </summary>
    /// <value><c>null</c> if the device does not support conditional access</value>
    public IConditionalAccessMenuActions CaMenuInterface
    {
      get
      {
        if (!_useConditionalAccessInterface)
        {
          return null;
        }
        // Return the first extension that implements CA menu access.
        foreach (ICustomDevice extension in _extensions)
        {
          IConditionalAccessMenuActions caMenuInterface = extension as IConditionalAccessMenuActions;
          if (caMenuInterface != null)
          {
            return caMenuInterface;
          }
        }
        return null;
      }
    }

    /// <summary>
    /// Get a count of the number of services that the device is currently decrypting.
    /// </summary>
    /// <value>The number of services currently being decrypted.</value>
    public int NumberOfChannelsDecrypting
    {
      get
      {
        // If not decrypting any channels or the limit is diabled then return zero.
        if (_mapSubChannels == null || _mapSubChannels.Count == 0 || _decryptLimit == 0)
        {
          return 0;
        }

        HashSet<long> decryptedServices = new HashSet<long>();
        Dictionary<int, ITvSubChannel>.Enumerator en = _mapSubChannels.GetEnumerator();
        while (en.MoveNext())
        {
          IChannel service = en.Current.Value.CurrentChannel;
          DVBBaseChannel digitalService = service as DVBBaseChannel;
          if (digitalService != null)
          {
            decryptedServices.Add(digitalService.ServiceId);
          }
          else
          {
            AnalogChannel analogService = service as AnalogChannel;
            if (analogService != null)
            {
              decryptedServices.Add(analogService.Frequency);
            }
            else
            {
              throw new TvException("TvCardBase: service type not recognised, unable to count number of services being decrypted\r\n" + service.ToString());
            }
          }
        }

        return decryptedServices.Count;
      }
    }

    #endregion

    /// <summary>
    /// Get the tuner's DiSEqC control interface. This interface is only
    /// applicable for satellite tuners. It is used for controlling switch,
    /// positioner and LNB settings.
    /// </summary>
    /// <value><c>null</c> if the tuner is not a satellite tuner or the tuner
    /// does not support sending/receiving DiSEqC commands</value>
    public IDiseqcController DiseqcController
    {
      get
      {
        return _diseqcController;
      }
    }

    /// <summary>
    /// Get an indicator to determine whether the tuner is locked on signal.
    /// </summary>
    public bool IsTunerLocked
    {
      get
      {
        UpdateSignalStatus();
        return _isSignalLocked;
      }
    }

    /// <summary>
    /// Get the tuner input signal quality.
    /// </summary>
    public int SignalQuality
    {
      get
      {
        UpdateSignalStatus();
        return _signalQuality;
      }
    }

    /// <summary>
    /// Get the tuner input signal level.
    /// </summary>
    public int SignalLevel
    {
      get
      {
        UpdateSignalStatus();
        return _signalLevel;
      }
    }

    /// <summary>
    /// Reset the signal update timer.
    /// </summary>
    /// <remarks>
    /// Calling this function will force us to update signal
    /// information (rather than return cached values) next time
    /// a signal information query is received.
    /// </remarks>
    public void ResetSignalUpdate()
    {
      _lastSignalUpdate = DateTime.MinValue;
    }

    /// <summary>
    /// Gets or sets the context.
    /// </summary>
    /// <value>The context.</value>
    public object Context
    {
      get { return _context; }
      set { _context = value; }
    }

    /// <summary>
    /// Get or set a value indicating whether this tuner is scanning for channels.
    /// </summary>
    /// <value><c>true</c> if the tuner is currently scanning, otherwise <c>false</c></value>
    public virtual bool IsScanning
    {
      get
      {
        return _isScanning;
      }
      set
      {
        _isScanning = value;
      }
    }

    /// <summary>
    /// Get the tuning parameters that have been applied to the hardware.
    /// This property returns null when the device is not in use.
    /// </summary>
    public IChannel CurrentTuningDetail
    {
      get
      {
        return _currentTuningDetail;
      }
    }

    #endregion

    /// <summary>
    /// Load the <see cref="T:TvLibrary.Interfaces.ICustomDevice">extensions</see> for this tuner.
    /// </summary>
    /// <param name="context">Any context required to initialise supported extensions.</param>
    protected void LoadExtensions(object context)
    {
      this.LogDebug("TvCardBase: load tuner extensions");

      TunerExtensionLoader tunerExtensionLoader = new TunerExtensionLoader();
      IEnumerable<ICustomDevice> extensions = tunerExtensionLoader.Load();

      this.LogDebug("TvCardBase: checking for supported extensions");
      HashSet<string> foundInterfaces = new HashSet<string>();
      foreach (ICustomDevice extension in extensions)
      {
        // We only support one implementation of each interface, unless the
        // extension is a DirectShow add-on.
        Type[] interfaces = new Type[0];
        if (!(extension is IDirectShowAddOnDevice))
        {
          bool foundInterface = false;
          interfaces = extension.GetType().GetInterfaces();
          foreach (Type i in interfaces)
          {
            // TODO this could pick up interfaces that we don't care about...
            // but we don't want an explicit list of interfaces we care about.
            // Need to be smarter!
            if (i != typeof(ICustomDevice) &&
              i != typeof(IRemoteControlListener) &&
              i != typeof(IDisposable) &&
              foundInterfaces.Contains(i.Name))
            {
              this.LogDebug("TvCardBase: extension \"{0}\" supports already found interface {1}, won't use", extension.Name, i.Name);
              foundInterface = true;
              break;
            }
          }
          if (foundInterface)
          {
            continue;
          }
        }

        this.LogDebug("TvCardBase: try extension \"{0}\"", extension.Name);
        if (!extension.Initialise(ExternalId, _tunerType, context))
        {
          extension.Dispose();
          continue;
        }

        _extensions.Add(extension);
      }

      this.LogDebug("TvCardBase: {0} extension(s) supported", _extensions.Count);
    }

    /// <summary>
    /// Open any <see cref="T:TvLibrary.Interfaces.ICustomDevice"/> extensions loaded for this tuner.
    /// </summary>
    /// <remarks>
    /// We separate this from the loading because some extensions (for example, the NetUP extension)
    /// can't be opened until the graph has finished being built.
    /// </remarks>
    protected void OpenExtensions()
    {
      this.LogDebug("TvCardBase: open tuner extensions");

      foreach (ICustomDevice extension in _extensions)
      {
        if (_useConditionalAccessInterface)
        {
          IConditionalAccessProvider caProvider = extension as IConditionalAccessProvider;
          if (caProvider != null)
          {
            this.LogDebug("TvCardBase: found conditional access provider \"{0}\"", extension.Name);
            if (caProvider.OpenConditionalAccessInterface())
            {
              _caProviders.Add(caProvider);
            }
            else
            {
              this.LogDebug("TvCardBase: provider will not be used");
            }
          }
        }
        if (_diseqcController == null)
        {
          IDiseqcDevice diseqcDevice = extension as IDiseqcDevice;
          if (diseqcDevice != null)
          {
            this.LogDebug("TvCardBase: found DiSEqC control interface \"{0}\"", extension.Name);
            _diseqcController = new DiseqcController(_tunerId, diseqcDevice);
          }
        }
        if (_encoderController == null)
        {
          IEncoder encoder = extension as IEncoder;
          if (encoder != null)
          {
            this.LogDebug("TvCardBase: found encoder control interface \"{0}\"", extension.Name);
            _encoderController = new EncoderController(_extensions);
          }
        }
        IRemoteControlListener rcListener = extension as IRemoteControlListener;
        if (rcListener != null)
        {
          this.LogDebug("TvCardBase: found remote control interface \"{0}\"", extension.Name);
          if (!rcListener.OpenRemoteControlInterface())
          {
            this.LogDebug("TvCardBase: interface will not be used");
          }
        }
      }
    }

    #region abstract and virtual methods

    /// <summary>
    /// Load the tuner.
    /// </summary>
    public void Load()
    {
      if (_state == TunerState.Loading)
      {
        this.LogWarn("TvCardBase: the tuner is already loading");
        return;
      }
      if (_state != TunerState.NotLoaded)
      {
        this.LogWarn("TvCardBase: the tuner is already loaded");
        return;
      }
      _state = TunerState.Loading;

      // Related tuners must be unloaded before this tuner can be loaded.
      if (_group != null)
      {
        this.LogDebug("TvCardBase: unload tuners in group");
        foreach (ITVCard tuner in _group.Tuners)
        {
          if (tuner.TunerId != _tunerId)
          {
            TvCardBase tunerBase = tuner as TvCardBase;
            if (tunerBase != null)
            {
              tunerBase.Unload();
            }
          }
        }
      }

      this.LogDebug("TvCardBase: load tuner {0}", _name);
      try
      {
        ReloadConfiguration();
        PerformLoading();

        _state = TunerState.Stopped;

        // Open any plugins that were detected during loading. This is separated from loading because some
        // plugins can't be opened until the tuner has fully loaded.
        OpenExtensions();

        // Plugins can request to pause or start the tuner - other actions don't make sense here. The started
        // state is considered more compatible than the paused state, so start takes precedence.
        TunerAction actualAction = TunerAction.Default;
        foreach (ICustomDevice extension in _extensions)
        {
          TunerAction action;
          extension.OnLoaded(this, out action);
          if (action == TunerAction.Pause)
          {
            if (actualAction == TunerAction.Default)
            {
              this.LogDebug("TvCardBase: plugin \"{0}\" will cause device pause", extension.Name);
              actualAction = TunerAction.Pause;
            }
            else
            {
              this.LogDebug("TvCardBase: plugin \"{0}\" wants to pause the tuner, overriden", extension.Name);
            }
          }
          else if (action == TunerAction.Start)
          {
            this.LogDebug("TvCardBase: plugin \"{0}\" will cause tuner start", extension.Name);
            actualAction = action;
          }
          else if (action != TunerAction.Default)
          {
            this.LogDebug("TvCardBase: plugin \"{0}\" wants unsupported action {1}", extension.Name, action);
          }
        }

        if (actualAction == TunerAction.Default && _idleMode == IdleMode.AlwaysOn)
        {
          this.LogDebug("TvCardBase: tuner is configured as always on");
          actualAction = TunerAction.Start;
        }

        if (actualAction != TunerAction.Default)
        {
          PerformTunerAction(actualAction);
        }
      }
      catch (TvExceptionSWEncoderMissing)
      {
        Unload();
        throw;
      }
      catch (Exception ex)
      {
        Unload();
        throw new TvExceptionTunerLoadFailed("Failed to load tuner.", ex);
      }
    }

    /// <summary>
    /// Unload the tuner.
    /// </summary>
    private void Unload()
    {
      this.LogDebug("TvCardBase: unload tuner");
      FreeAllSubChannels();
      PerformTunerAction(TunerAction.Stop);

      // Dispose extensions.
      if (_extensions != null)
      {
        foreach (ICustomDevice extension in _extensions)
        {
          // Avoid recursive loop for ITVCard implementations that also implement ICustomDevice.
          if (!(extension is ITVCard))
          {
            extension.Dispose();
          }
        }
      }
      _caProviders.Clear();
      _extensions.Clear();

      try
      {
        PerformUnloading();
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "TvCardBase: failed to completely unload the tuner");
      }

      _epgGrabber = null;
      _channelScanner = null;
      _diseqcController = null;
      _encoderController = null;

      _state = TunerState.NotLoaded;
    }

    /// <summary>
    /// Wait for the tuner to acquire signal lock.
    /// </summary>
    private void LockInOnSignal()
    {
      this.LogDebug("TvCardBase: lock in on signal");
      _isSignalLocked = false;
      DateTime timeStart = DateTime.Now;
      TimeSpan ts = timeStart - timeStart;
      while (!_isSignalLocked && ts.TotalSeconds < _parameters.TimeOutTune)
      {
        ThrowExceptionIfTuneCancelled();
        PerformSignalStatusUpdate(true);
        if (!_isSignalLocked)
        {
          ts = DateTime.Now - timeStart;
          this.LogDebug("  waiting 20ms");
          System.Threading.Thread.Sleep(20);
        }
      }

      if (!_isSignalLocked)
      {
        throw new TvExceptionNoSignal("TvCardBase: failed to lock signal");
      }

      this.LogDebug("TvCardBase: locked");
    }

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    public virtual void ReloadConfiguration()
    {
      if (ExternalId != null)
      {
        this.LogDebug("tuner base: reload configuration");
        Card t = CardManagement.GetCardByDevicePath(ExternalId, CardIncludeRelationEnum.None);
        if (t != null)
        {
          _tunerId = t.IdCard;
          _name = t.Name;   // We prefer to use the name that can be set via configuration for more readable logs...
          _idleMode = (IdleMode)t.IdleMode;
          _pidFilterMode = (PidFilterMode)t.PidFilterMode;
          _useCustomTuning = t.UseCustomTuning;

          // Conditional access...
          _useConditionalAccessInterface = t.UseConditionalAccess;
          _camType = (CamType)t.CamType;
          _decryptLimit = t.DecryptLimit;
          _multiChannelDecryptMode = (MultiChannelDecryptMode)t.MultiChannelDecryptMode;

          if (_state == TunerState.NotLoaded && t.PreloadCard)
          {
            Load();
          }
        }
      }

      if (_epgGrabber != null)
      {
        _epgGrabber.ReloadConfiguration();
      }
      if (_diseqcController != null)
      {
        _diseqcController.ReloadConfiguration();
      }
    }

    /// <summary>
    /// Get the tuner's electronic programme guide data grabbing interface.
    /// </summary>
    public IEpgGrabber EpgGrabberInterface
    {
      get
      {
        return _epgGrabber;
      }
    }

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public ITVScanning ScanningInterface
    {
      get
      {
        return _channelScanner;
      }
    }

    /// <summary>
    /// Update tuner signal status measurements.
    /// </summary>
    private void UpdateSignalStatus()
    {
      TimeSpan ts = DateTime.Now - _lastSignalUpdate;
      if (ts.TotalMilliseconds < 5000)
      {
        return;
      }
      if (_currentTuningDetail == null || _state != TunerState.Started)
      {
        _isSignalPresent = false;
        _isSignalLocked = false;
        _signalLevel = 0;
        _signalQuality = 0;
      }
      else
      {
        PerformSignalStatusUpdate(false);
        _lastSignalUpdate = DateTime.Now;

        if (_signalLevel < 0)
        {
          _signalLevel = 0;
        }
        else if (_signalLevel > 100)
        {
          _signalLevel = 100;
        }
        if (_signalQuality < 0)
        {
          _signalQuality = 0;
        }
        else if (_signalQuality > 100)
        {
          _signalQuality = 100;
        }
      }
    }

    /// <summary>
    /// Stop the tuner. The actual result of this function depends on tuner configuration.
    /// </summary>
    public virtual void Stop()
    {
      this.LogDebug("TvCardBase: stop, idle mode = {0}", _idleMode);
      TunerAction action = TunerAction.Stop;
      try
      {
        if (_epgGrabber != null)
        {
          _epgGrabber.AbortGrabbing();
        }
        IsScanning = false;
        FreeAllSubChannels();

        switch (_idleMode)
        {
          case IdleMode.Pause:
            action = TunerAction.Pause;
            break;
          case IdleMode.Unload:
            action = TunerAction.Unload;
            break;
          case IdleMode.AlwaysOn:
            action = TunerAction.Start;
            break;
        }

        // Plugins may want to prevent or direct actions to ensure compatibility and smooth device operation.
        TunerAction pluginAction = action;
        foreach (ICustomDevice extension in _extensions)
        {
          extension.OnStop(this, ref pluginAction);
          if (pluginAction > action)
          {
            this.LogDebug("TvCardBase: plugin \"{0}\" overrides action {1} with {2}", extension.Name, action, pluginAction);
            action = pluginAction;
          }
          else if (action != pluginAction)
          {
            this.LogDebug("TvCardBase: plugin \"{0}\" wants to perform action {1}, overriden", extension.Name, pluginAction);
          }
        }

        PerformTunerAction(action);

        // Turn off the device power.
        foreach (ICustomDevice extension in _extensions)
        {
          IPowerDevice powerDevice = extension as IPowerDevice;
          if (powerDevice != null)
          {
            powerDevice.SetPowerState(PowerState.Off);
          }
        }
      }
      finally
      {
        if (action != TunerAction.Start && action != TunerAction.Pause)
        {
          // This forces a full retune. There are potential tuner compatibility
          // considerations here. Most tuners should remember current settings
          // when paused; some do when stopped as well, but the majority don't.
          _currentTuningDetail = null;
          if (_diseqcController != null)
          {
            _diseqcController.SwitchToChannel(null);
          }
        }
      }
    }

    /// <summary>
    /// Perform a specific tuner action. For example, stop the tuner.
    /// </summary>
    /// <param name="action">The action to perform with the device.</param>
    private void PerformTunerAction(TunerAction action)
    {
      this.LogDebug("TvCardBase: perform tuner action, action = {0}", action);
      try
      {
        if (action == TunerAction.Reset)
        {
          Unload();
          Load();
        }
        else if (action == TunerAction.Unload)
        {
          Unload();
        }
        else if (action == TunerAction.Pause)
        {
          SetTunerState(TunerState.Paused);
        }
        else if (action == TunerAction.Stop)
        {
          SetTunerState(TunerState.Stopped);
        }
        else if (action == TunerAction.Start)
        {
          SetTunerState(TunerState.Started);
        }
        else if (action == TunerAction.Restart)
        {
          SetTunerState(TunerState.Stopped);
          SetTunerState(TunerState.Started);
        }
        else
        {
          this.LogWarn("TvCardBase: unhandled action {0}", action);
          return;
        }

        this.LogDebug("TvCardBase: action succeeded");
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TvCardBase: action {0} failed", action);
      }
    }

    /// <summary>
    /// Set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    public abstract void SetTunerState(TunerState state);

    #endregion

    #region scan/tune

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public abstract bool CanTune(IChannel channel);

    /// <summary>
    /// Tune to a specific channel.
    /// </summary>
    /// <param name="subChannelId">The ID of the subchannel associated with the channel that is being tuned.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the subchannel associated with the tuned channel</returns>
    public virtual ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      this.LogDebug("TvCardBase: tune channel, {0}", channel);
      ITvSubChannel subChannel = null;
      try
      {
        // The tuner must be loaded before a channel can be tuned.
        if (_state == TunerState.NotLoaded)
        {
          Load();
          ThrowExceptionIfTuneCancelled();
        }
        // Some tuners (for example: CableCARD tuners) are only able to
        // deliver one service... full stop.
        else if (!_supportsSubChannels && _mapSubChannels.Count > 0)
        {
          if (_mapSubChannels.TryGetValue(subChannelId, out subChannel))
          {
            // Existing sub channel.
            if (_mapSubChannels.Count != 1)
            {
              // If this is not the only sub channel then by definition this
              // must be an attempt to tune a new service. Not allowed.
              throw new TvException("Tuner is not able to receive more than one service.");
            }
          }
          else
          {
            // New sub channel.
            Dictionary<int, ITvSubChannel>.ValueCollection.Enumerator en = _mapSubChannels.Values.GetEnumerator();
            en.MoveNext();
            if (en.Current.CurrentChannel != channel)
            {
              // The tuner is currently streaming a different service.
              throw new TvException("Tuner is not able to receive more than one service.");
            }
          }
        }

        // Get a subchannel for the service.
        string description;
        if (subChannel == null && !_mapSubChannels.TryGetValue(subChannelId, out subChannel))
        {
          description = "creating new subchannel";
          subChannel = CreateNewSubChannel(subChannelId);
          subChannel.Parameters = Parameters;
          _mapSubChannels[subChannelId] = subChannel;
          FireNewSubChannelEvent(subChannelId);
        }
        else
        {
          description = "using existing subchannel";
          // If reusing a subchannel and our multi-channel decrypt mode is "changes", tell the plugin to stop
          // decrypting the previous service before we lose access to the PMT and CAT.
          if (_multiChannelDecryptMode == MultiChannelDecryptMode.Changes)
          {
            UpdateDecryptList(subChannelId, CaPmtListManagementAction.Last);
          }
        }
        this.LogInfo("TvCardBase: {0}, ID = {1}, count = {2}", description, subChannelId, _mapSubChannels.Count);
        subChannel.CurrentChannel = channel;

        // Subchannel OnBeforeTune().
        subChannel.OnBeforeTune();

        // Do we need to tune?
        if (_currentTuningDetail == null || _currentTuningDetail.IsDifferentTransponder(channel))
        {
          // Stop the EPG grabber. We're going to move to a different channel. Any EPG data that
          // has been grabbed but not stored is thrown away.
          if (_epgGrabber != null)
          {
            _epgGrabber.AbortGrabbing();
          }

          // When we call ICustomDevice.OnBeforeTune(), the ICustomDevice may modify the tuning parameters.
          // However, the original channel object *must not* be modified otherwise IsDifferentTransponder()
          // will sometimes returns true when it shouldn't. See mantis 0002979.
          IChannel tuneChannel = channel.GetTuningChannel();

          // Plugin OnBeforeTune().
          TunerAction action = TunerAction.Default;
          foreach (ICustomDevice extension in _extensions)
          {
            TunerAction pluginAction;
            extension.OnBeforeTune(this, _currentTuningDetail, ref tuneChannel, out pluginAction);
            if (pluginAction != TunerAction.Unload && pluginAction != TunerAction.Default)
            {
              // Valid action requested...
              if (pluginAction > action)
              {
                this.LogDebug("TvCardBase: plugin \"{0}\" overrides action {1} with {2}", extension.Name, action, pluginAction);
                action = pluginAction;
              }
              else if (pluginAction != action)
              {
                this.LogDebug("TvCardBase: plugin \"{0}\" wants to perform action {1}, overriden", extension.Name, pluginAction);
              }
            }

            // Turn on power. This usually needs to happen before tuning.
            IPowerDevice powerDevice = extension as IPowerDevice;
            if (powerDevice != null)
            {
              powerDevice.SetPowerState(PowerState.On);
            }
          }
          ThrowExceptionIfTuneCancelled();
          if (action != TunerAction.Default)
          {
            PerformTunerAction(action);
          }

          // Send DiSEqC commands (if necessary) before actually tuning in case the driver applies the commands
          // during the tuning process.
          if (_diseqcController != null)
          {
            _diseqcController.SwitchToChannel(channel as DVBSChannel);
          }

          // Apply tuning parameters.
          ThrowExceptionIfTuneCancelled();
          if (_useCustomTuning)
          {
            foreach (ICustomDevice extension in _extensions)
            {
              ICustomTuner customTuner = extension as ICustomTuner;
              if (customTuner != null && customTuner.CanTuneChannel(channel))
              {
                this.LogDebug("TvCardBase: using custom tuning");
                if (!customTuner.Tune(tuneChannel))
                {
                  ThrowExceptionIfTuneCancelled();
                  this.LogWarn("TvCardBase: custom tuning failed, falling back to default tuning");
                  PerformTuning(tuneChannel);
                }
              }
            }
          }
          else
          {
            PerformTuning(tuneChannel);
          }
          ThrowExceptionIfTuneCancelled();

          // Plugin OnAfterTune().
          foreach (ICustomDevice extension in _extensions)
          {
            extension.OnAfterTune(this, channel);
          }
        }

        // Subchannel OnAfterTune().
        _lastSignalUpdate = DateTime.MinValue;
        subChannel.OnAfterTune();

        _currentTuningDetail = channel;

        // Start the tuner.
        ThrowExceptionIfTuneCancelled();
        PerformTunerAction(TunerAction.Start);
        ThrowExceptionIfTuneCancelled();

        // Plugin OnStarted().
        foreach (ICustomDevice extension in _extensions)
        {
          extension.OnStarted(this, channel);
        }

        LockInOnSignal();

        subChannel.AfterTuneEvent -= FireAfterTuneEvent;
        subChannel.AfterTuneEvent += FireAfterTuneEvent;

        // Ensure that data/streams which are required to detect the service will pass through the device's
        // PID filter.
        ConfigurePidFilter();
        ThrowExceptionIfTuneCancelled();

        subChannel.OnGraphRunning();

        // At this point we should know which data/streams form the service(s) that are being accessed. We need to
        // ensure those streams will pass through the device's PID filter.
        ThrowExceptionIfTuneCancelled();
        ConfigurePidFilter();
        ThrowExceptionIfTuneCancelled();

        // If the service is encrypted, start decrypting it.
        UpdateDecryptList(subChannelId, CaPmtListManagementAction.Add);
      }
      catch (Exception ex)
      {
        if (!(ex is TvException))
        {
          this.LogError(ex);
        }

        // One potential reason for getting here is that signal could not be locked, and the reason for
        // that may be that tuning failed. We always want to force a retune on the next tune request in
        // this situation.
        _currentTuningDetail = null;
        if (subChannel != null)
        {
          FreeSubChannel(subChannelId);
        }
        throw;
      }
      finally
      {
        _cancelTune = false;
      }

      return subChannel;
    }

    /// <summary>
    /// Cancel the current tuning process.
    /// </summary>
    /// <param name="subChannelId">The ID of the subchannel associated with the channel that is being cancelled.</param>
    public void CancelTune(int subChannelId)
    {
      this.LogDebug("TvCardBase: subchannel {0} cancel tune", subChannelId);
      _cancelTune = true;
      ITvSubChannel subChannel;
      if (_mapSubChannels.TryGetValue(subChannelId, out subChannel))
      {
        subChannel.CancelTune();
      }
    }

    /// <summary>
    /// Check if the current tuning process has been cancelled and throw an exception if it has.
    /// </summary>
    private void ThrowExceptionIfTuneCancelled()
    {
      if (_cancelTune)
      {
        throw new TvExceptionTuneCancelled();
      }
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public abstract void PerformTuning(IChannel channel);

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    public abstract void PerformLoading();

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    public abstract void PerformUnloading();

    /// <summary>
    /// Actually update tuner signal status statistics.
    /// </summary>
    /// <param name="onlyUpdateLock"><c>True</c> to only update lock status.</param>
    public abstract void PerformSignalStatusUpdate(bool onlyUpdateLock);

    #endregion

    #region quality control

    /// <summary>
    /// Get the device's quality control interface.
    /// </summary>
    public virtual IQuality Quality
    {
      get
      {
        return _encoderController;
      }
    }

    #endregion

    #region channel linkages

    /// <summary>
    /// Starts scanning for linkages.
    /// </summary>
    /// <param name="callBack">The delegate to call when scanning is complete or canceled.</param>
    public virtual void StartLinkageScanner(BaseChannelLinkageScanner callBack)
    {
    }

    /// <summary>
    /// Stop/reset the linkage scanner.
    /// </summary>
    public virtual void ResetLinkageScanner()
    {
    }

    /// <summary>
    /// Get the portal channels found by the linkage scanner.
    /// </summary>
    public virtual List<PortalChannel> ChannelLinkages
    {
      get
      {
        return null;
      }
    }

    #endregion

    #region subchannel management

    /// <summary>
    /// Allocate a new subchannel instance.
    /// </summary>
    /// <param name="id">The identifier for the subchannel.</param>
    /// <returns>the new subchannel instance</returns>
    public abstract ITvSubChannel CreateNewSubChannel(int id);

    /// <summary>
    /// Free a subchannel.
    /// </summary>
    /// <param name="id">The subchannel identifier.</param>
    public void FreeSubChannel(int id)
    {
      this.LogDebug("TvCardBase: free subchannel, ID = {0}, count = {1}", id, _mapSubChannels.Count);
      ITvSubChannel subChannel;
      if (_mapSubChannels.TryGetValue(id, out subChannel))
      {
        if (subChannel.IsTimeShifting)
        {
          this.LogError("TvCardBase: asked to free subchannel that is still timeshifting!");
          return;
        }
        if (subChannel.IsRecording)
        {
          this.LogError("TvCardBase: asked to free subchannel that is still recording!");
          return;
        }

        try
        {
          UpdateDecryptList(id, CaPmtListManagementAction.Last);
          subChannel.Decompose();
        }
        finally
        {
          _mapSubChannels.Remove(id);
        }
        // PID filters are configured according to the PIDs that need to be passed, so reconfigure the PID
        // filter *after* we removed the subchannel (otherwise PIDs for the subchannel that we're freeing
        // won't be removed).
        ConfigurePidFilter();
      }
      else
      {
        this.LogWarn("TvCardBase: subchannel not found!");
      }
      if (_mapSubChannels.Count == 0)
      {
        this.LogDebug("TvCardBase: no subchannels present, stopping tuner");
        Stop();
      }
      else
      {
        this.LogDebug("TvCardBase: subchannels still present, leave tuner running");
      }
    }

    /// <summary>
    /// Free all subchannels.
    /// </summary>
    private void FreeAllSubChannels()
    {
      this.LogInfo("TvCardBase: free all subchannels, count = {0}", _mapSubChannels.Count);
      Dictionary<int, ITvSubChannel>.Enumerator en = _mapSubChannels.GetEnumerator();
      while (en.MoveNext())
      {
        en.Current.Value.Decompose();
      }
      _mapSubChannels.Clear();
    }

    /// <summary>
    /// Get a specific subchannel.
    /// </summary>
    /// <param name="id">The ID of the subchannel.</param>
    /// <returns></returns>
    public ITvSubChannel GetSubChannel(int id)
    {
      ITvSubChannel subChannel = null;
      if (_mapSubChannels != null)
      {
        _mapSubChannels.TryGetValue(id, out subChannel);
      }
      return subChannel;
    }

    /// <summary>
    /// Get the tuner's subchannels.
    /// </summary>
    /// <value>An array containing the subchannels.</value>
    public ITvSubChannel[] SubChannels
    {
      get
      {
        int count = 0;
        ITvSubChannel[] channels = new ITvSubChannel[_mapSubChannels.Count];
        Dictionary<int, ITvSubChannel>.Enumerator en = _mapSubChannels.GetEnumerator();
        while (en.MoveNext())
        {
          channels[count++] = en.Current.Value;
        }
        return channels;
      }
    }

    #endregion
  }
}
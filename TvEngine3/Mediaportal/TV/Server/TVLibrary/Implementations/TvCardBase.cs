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
using System.IO;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using DirectShowLib;
using DirectShowLib.BDA;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.ChannelLinkage;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Epg;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// Base class for all devices.
  /// </summary>
  public abstract class TvCardBase : ITVCard, IDisposable
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
    protected ScanParameters _parameters;

    /// <summary>
    /// Dictionary of the corresponding sub channels
    /// </summary>
    protected Dictionary<int, ITvSubChannel> _mapSubChannels;

    /// <summary>
    /// Context reference
    /// </summary>
    protected object m_context;

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
    /// Device Path of the tv card
    /// </summary>
    protected string _devicePath;

    /// <summary>
    /// Indicates, if the card is grabbing epg
    /// </summary>
    protected bool _epgGrabbing;

    /// <summary>
    /// Name of the tv card
    /// </summary>
    protected string _name;

    /// <summary>
    /// Indicates, if the card is scanning
    /// </summary>
    protected bool _isScanning;

    /// <summary>
    /// The tuner type (eg. DVB-S, DVB-T... etc.).
    /// </summary>
    protected CardType _tunerType;

    /// <summary>
    /// Date and time of the last signal update
    /// </summary>
    protected volatile DateTime _lastSignalUpdate = DateTime.MinValue;

    /// <summary>
    /// Indicates, if the signal is present
    /// </summary>
    protected bool _isSignalPresent;

    /// <summary>
    /// Indicates, if the card is present
    /// </summary>
    protected bool _cardPresent = true;

    /// <summary>
    /// The db card id
    /// </summary>
    protected int _cardId;

    /// <summary>
    /// The action that will be taken when the tuner is no longer being actively used.
    /// </summary>
    protected IdleMode _idleMode = IdleMode.Stop;

    /// <summary>
    /// A list containing the custom device interfaces supported by this tuner. The list is ordered by
    /// interface priority.
    /// </summary>
    protected List<ICustomDevice> _customDeviceInterfaces = null;

    /// <summary>
    /// Enable or disable the use of conditional access interface(s).
    /// </summary>
    protected bool _useConditionalAccessInterface = true;

    /// <summary>
    /// The type of conditional access module available to the conditional access interface.
    /// </summary>
    /// <remarks>
    /// Certain conditional access modules require specific handling to ensure compatibility.
    /// </remarks>
    protected CamType _camType = CamType.Default;

    /// <summary>
    /// The number of channels that the device is capable of or permitted to decrypt simultaneously. Zero means
    /// there is no limit.
    /// </summary>
    protected int _decryptLimit = 0;

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
    protected MultiChannelDecryptMode _multiChannelDecryptMode = MultiChannelDecryptMode.List;

    /// <summary>
    /// Enable or disable waiting for the conditional interface to be ready before sending commands.
    /// </summary>
    protected bool _waitUntilCaInterfaceReady = true;

    /// <summary>
    /// The number of times to re-attempt decrypting the current service set when one or more services are
    /// not able to be decrypted for whatever reason.
    /// </summary>
    /// <remarks>
    /// Each available CA interface will be tried in order of priority. If decrypting is not started
    /// successfully, all interfaces are retried until each interface has been tried
    /// _decryptFailureRetryCount + 1 times, or until decrypting is successful.
    /// </remarks>
    protected int _decryptFailureRetryCount = 2;

    /// <summary>
    /// The device's current tuning parameter values or null if the device is not tuned.
    /// </summary>
    protected IChannel _currentTuningDetail = null;

    /// <summary>
    /// Enable or disable the use of custom device interfaces for tuning.
    /// </summary>
    /// <remarks>
    /// Custom/direct tuning may be faster or more reliable than regular tuning methods. Equally, it can
    /// also be slower (eg. TeVii) or more limiting (eg. Digital Everywhere) than regular tuning methods.
    /// </remarks>
    protected bool _useCustomTuning = false;

    /// <summary>
    /// A flag used by the TV service as a signal to abort the tuning process before it is completed.
    /// </summary>
    protected volatile bool _cancelTune = false;

    /// <summary>
    /// The current state of the tuner.
    /// </summary>
    protected TunerState _state = TunerState.NotLoaded;

    #endregion

    #region constructor

    /// <summary>
    /// Base constructor
    /// </summary>
    /// <param name="name">The device name.</param>
    /// <param name="externalId">An identifier for the device. Useful for distinguishing this instance from other devices of the same type.</param>
    protected TvCardBase(string name, string externalIdentifier)
    {
      _mapSubChannels = new Dictionary<int, ITvSubChannel>();
      _parameters = new ScanParameters();
      _epgGrabbing = false;   // EPG grabbing not supported by default.
      _customDeviceInterfaces = new List<ICustomDevice>();
      _name = name;
      _devicePath = externalIdentifier;

      if (DevicePath != null)
      {
        Card d = CardManagement.GetCardByDevicePath(DevicePath, CardIncludeRelationEnum.None);
        if (d != null)
        {
          _cardId = d.IdCard;
          _name = d.Name;   // We prefer to use the name that can be set in TV Server configuration for more readable logs...
          _idleMode = (IdleMode)d.IdleMode;
          _pidFilterMode = (PidFilterMode)d.PidFilterMode;
          _useCustomTuning = d.UseCustomTuning;

          // Conditional access...
          _useConditionalAccessInterface = d.UseConditionalAccess;
          _camType = (CamType)d.CamType;
          _decryptLimit = d.DecryptLimit;
          _multiChannelDecryptMode = (MultiChannelDecryptMode)d.MultiChannelDecryptMode;

          // Preload the device if configured to do so.
          // TODO preloading shouldn't be done here - it should happen in TunerDetector (in case required components can't be loaded until the super constructor is executed)
          if (d.Enabled && d.PreloadCard)
          {
            this.LogInfo("TvCardBase: preloading device {0}", Name);
            Load();
          }
        }
      }
    }

    #endregion

    #region properties

    /// <summary>
    /// Gets or sets the unique id of this card
    /// </summary>
    public int CardId
    {
      get { return _cardId; }
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
    /// Gets/sets the card name
    /// </summary>
    /// <value></value>
    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    /// <summary>
    /// returns true if card is currently present
    /// </summary>
    public bool CardPresent
    {
      get { return _cardPresent; }
      set { _cardPresent = value; }
    }

    /// <summary>
    /// Gets/sets the card device
    /// </summary>
    public virtual string DevicePath
    {
      get { return _devicePath; }
    }

    /// <summary>
    /// Gets the device tuner type.
    /// </summary>
    public virtual CardType CardType
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
      set
      {
        _camType = value;
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
        // Return true if any interface implements IConditionalAccessProvider.
        foreach (ICustomDevice d in _customDeviceInterfaces)
        {
          if (d is IConditionalAccessProvider)
          {
            return true;
          }
        }
        return false;
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
        // Return the first interface that implements ICiMenuActions.
        foreach (ICustomDevice d in _customDeviceInterfaces)
        {
          IConditionalAccessMenuActions caMenuInterface = d as IConditionalAccessMenuActions;
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
    /// Get the tuner's DiSEqC control interface. This interface is only applicable for satellite tuners.
    /// It is used for controlling switch, positioner and LNB settings.
    /// </summary>
    /// <value><c>null</c> if the tuner is not a satellite tuner or the tuner does not support sending/receiving
    /// DiSEqC commands</value>
    public virtual IDiseqcController DiseqcController
    {
      get
      {
        return null;
      }
    }

    /// <summary>
    /// Get an indicator to determine whether the tuner is locked on signal.
    /// </summary>
    public virtual bool IsTunerLocked
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
      get { return m_context; }
      set { m_context = value; }
    }

    /// <summary>
    /// returns true if card is currently grabbing the epg
    /// </summary>
    public bool IsEpgGrabbing
    {
      get { return _epgGrabbing; }
      set
      {
        UpdateEpgGrabber(value);
        _epgGrabbing = value;
      }
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
    /// Load the <see cref="T:TvLibrary.Interfaces.ICustomDevice"/> plugins for this device.
    /// </summary>
    /// <remarks>
    /// It is expected that this function will be called at some stage during the DirectShow graph building process.
    /// This function may update the lastFilter reference parameter to insert filters for <see cref="IDirectShowAddOnDevice"/>
    /// plugins.
    /// </remarks>
    /// <param name="mainFilter">The main device source filter. Usually a tuner filter.</param>
    /// <param name="graph">The tuner graph.</param>
    /// <param name="lastFilter">The source filter (usually either a tuner or capture/receiver filter) to
    ///   connect the [first] device filter to.</param>
    protected void LoadPlugins(IBaseFilter mainFilter, IFilterGraph2 graph, ref IBaseFilter lastFilter)
    {
      this.LogDebug("TvCardBase: load custom device plugins");

      if (mainFilter == null)
      {
        this.LogDebug("TvCardBase: the main filter is null");
        return;
      }

      CustomDeviceLoader customDeviceLoader = new CustomDeviceLoader();

      customDeviceLoader.Load();
      IEnumerable<ICustomDevice> plugins = customDeviceLoader.Plugins;

      this.LogDebug("TvCardBase: checking for supported plugins");
      _customDeviceInterfaces = new List<ICustomDevice>();      
      foreach (ICustomDevice d in plugins)
      {
        if (!d.Initialise(DevicePath, _tunerType, mainFilter))
        {
          d.Dispose();
          continue;
        }

        // The plugin is supported. If the plugin is an add on plugin, we attempt to add it to the graph.
        bool isAddOn = false;
        if (lastFilter != null)
        {
          IDirectShowAddOnDevice addOn = d as IDirectShowAddOnDevice;
          if (addOn != null)
          {
            this.LogDebug("TvCardBase: DirectShow add-on plugin found");
            if (!addOn.AddToGraph(graph, ref lastFilter))
            {
              this.LogDebug("TvCardBase: failed to add filters to graph");
              addOn.Dispose();
              continue;
            }
            isAddOn = true;
          }
        }

        try
        {
          // When we find the main plugin, then we stop searching...
          if (!isAddOn)
          {
            this.LogDebug("TvCardBase: primary plugin found");
            break;
          }
        }
        finally
        {
          _customDeviceInterfaces.Add(d);
        }
      }
      if (_customDeviceInterfaces.Count == 0)
      {
        this.LogDebug("TvCardBase: no plugins supported");
      }
    }

    /// <summary>
    /// Open any <see cref="T:TvLibrary.Interfaces.ICustomDevice"/> plugins loaded for this device by LoadPlugins().
    /// </summary>
    /// <remarks>
    /// We separate this from the loading because some plugins (for example, the NetUP plugin) can't be opened
    /// until the graph has finished being built.
    /// </remarks>
    protected void OpenPlugins()
    {
      this.LogDebug("TvCardBase: open custom device plugins");
      if (_useConditionalAccessInterface)
      {
        foreach (ICustomDevice plugin in _customDeviceInterfaces)
        {
          IConditionalAccessProvider caProvider = plugin as IConditionalAccessProvider;
          if (caProvider != null)
          {
            caProvider.OpenInterface();
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
      if (_state != TunerState.NotLoaded)
      {
        this.LogWarn("TvCardBase: the tuner is already loaded");
        return;
      }

      this.LogDebug("TvCardBase: load tuner {0}", _name);
      try
      {
        ReloadConfiguration();
        PerformLoading();

        _state = TunerState.Stopped;

        // Open any plugins that were detected during loading. This is separated from loading because some
        // plugins can't be opened until the tuner has fully loaded.
        OpenPlugins();

        // Plugins can request to pause or start the tuner - other actions don't make sense here. The started
        // state is considered more compatible than the paused state, so start takes precedence.
        TunerAction actualAction = TunerAction.Default;
        foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
        {
          TunerAction action;
          deviceInterface.OnLoaded(this, out action);
          if (action == TunerAction.Pause)
          {
            if (actualAction == TunerAction.Default)
            {
              this.LogDebug("TvCardBase: plugin \"{0}\" will cause device pause", deviceInterface.Name);
              actualAction = TunerAction.Pause;
            }
            else
            {
              this.LogDebug("TvCardBase: plugin \"{0}\" wants to pause the tuner, overriden", deviceInterface.Name);
            }
          }
          else if (action == TunerAction.Start)
          {
            this.LogDebug("TvCardBase: plugin \"{0}\" will cause tuner start", deviceInterface.Name);
            actualAction = action;
          }
          else if (action != TunerAction.Default)
          {
            this.LogDebug("TvCardBase: plugin \"{0}\" wants unsupported action {1}", deviceInterface.Name, action);
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
    public void Unload()
    {
      this.LogDebug("TvCardBase: unload tuner");
      FreeAllSubChannels();
      PerformTunerAction(TunerAction.Stop);

      // Dispose plugins.
      if (_customDeviceInterfaces != null)
      {
        foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
        {
          // Avoid recursive loop for ITVCard implementations that also implement ICustomDevice... like TunerB2c2Base.
          if (!(deviceInterface is ITVCard))
          {
            deviceInterface.Dispose();
          }
        }
      }
      _customDeviceInterfaces = new List<ICustomDevice>();

      try
      {
        PerformUnloading();
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "TvCardBase: failed to completely unload the tuner");
      }

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
    /// Reload configuration.
    /// </summary>
    public virtual void ReloadConfiguration()
    {
    }

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public virtual ITVScanning ScanningInterface
    {
      get
      {
        return null;
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
      try
      {
        UpdateEpgGrabber(false);  // Stop grabbing EPG.
        IsScanning = false;
        FreeAllSubChannels();

        TunerAction action = TunerAction.Stop;
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
        foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
        {
          deviceInterface.OnStop(this, ref pluginAction);
          if (pluginAction > action)
          {
            this.LogDebug("TvCardBase: plugin \"{0}\" overrides action {1} with {2}", deviceInterface.Name, action, pluginAction);
            action = pluginAction;
          }
          else if (action != pluginAction)
          {
            this.LogDebug("TvCardBase: plugin \"{0}\" wants to perform action {1}, overriden", deviceInterface.Name, pluginAction);
          }
        }

        PerformTunerAction(action);

        // Turn off the device power.
        foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
        {
          IPowerDevice powerDevice = deviceInterface as IPowerDevice;
          if (powerDevice != null)
          {
            powerDevice.SetPowerState(PowerState.Off);
          }
        }
      }
      finally
      {
        // We always want to force a full retune on the next tune request in this situation.
        // TODO: consider the implications for pausing the graph (advantages of pause vs. stop may be reduced). Do we really want to do this?
        _currentTuningDetail = null;
      }
    }

    /// <summary>
    /// Perform a specific tuner action. For example, stop the tuner.
    /// </summary>
    /// <param name="action">The action to perform with the device.</param>
    protected virtual void PerformTunerAction(TunerAction action)
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
          this.LogDebug("TvCardBase: unhandled action");
          return;
        }

        this.LogDebug("TvCardBase: action succeeded");
      }
      catch (Exception ex)
      {
        this.LogError(ex, "TvCardBase: action failed");
      }
    }

    /// <summary>
    /// Set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    protected abstract void SetTunerState(TunerState state);

    #endregion

    #region scan/tune

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public abstract bool CanTune(IChannel channel);

    /// <summary>
    /// Scan a specific channel.
    /// </summary>
    /// <param name="subChannelId">The ID of the subchannel associated with the channel that is being scanned.</param>
    /// <param name="channel">The channel to scan.</param>
    /// <returns>the subchannel associated with the scanned channel</returns>
    public virtual ITvSubChannel Scan(int subChannelId, IChannel channel)
    {
      return Tune(subChannelId, channel);
    }

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

        // Get a subchannel for the service.
        string description;
        if (!_mapSubChannels.TryGetValue(subChannelId, out subChannel))
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
          // Stop the EPG grabber. We're going to move to a different channel so any EPG data that has been
          // grabbed for the previous channel should be stored.
          UpdateEpgGrabber(false);

          // When we call ICustomDevice.OnBeforeTune(), the ICustomDevice may modify the tuning parameters.
          // However, the original channel object *must not* be modified otherwise IsDifferentTransponder()
          // will sometimes returns true when it shouldn't. See mantis 0002979.
          IChannel tuneChannel = channel.GetTuningChannel();

          // Plugin OnBeforeTune().
          TunerAction action = TunerAction.Default;
          foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
          {
            TunerAction pluginAction;
            deviceInterface.OnBeforeTune(this, _currentTuningDetail, ref tuneChannel, out pluginAction);
            if (pluginAction != TunerAction.Unload && pluginAction != TunerAction.Default)
            {
              // Valid action requested...
              if (pluginAction > action)
              {
                this.LogDebug("TvCardBase: plugin \"{0}\" overrides action {1} with {2}", deviceInterface.Name, action, pluginAction);
                action = pluginAction;
              }
              else if (pluginAction != action)
              {
                this.LogDebug("TvCardBase: plugin \"{0}\" wants to perform action {1}, overriden", deviceInterface.Name, pluginAction);
              }
            }

            // Turn on power. This usually needs to happen before tuning.
            IPowerDevice powerDevice = deviceInterface as IPowerDevice;
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

          // Apply tuning parameters.
          ThrowExceptionIfTuneCancelled();
          if (_useCustomTuning)
          {
            foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
            {
              ICustomTuner customTuner = deviceInterface as ICustomTuner;
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
          foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
          {
            deviceInterface.OnAfterTune(this, channel);
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
        foreach (ICustomDevice deviceInterface in _customDeviceInterfaces)
        {
          deviceInterface.OnStarted(this, channel);
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
    protected void ThrowExceptionIfTuneCancelled()
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
    protected abstract void PerformTuning(IChannel channel);

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    protected abstract void PerformLoading();

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    protected abstract void PerformUnloading();

    /// <summary>
    /// Actually update tuner signal status statistics.
    /// </summary>
    /// <param name="onlyUpdateLock"><c>True</c> to only update lock status.</param>
    protected abstract void PerformSignalStatusUpdate(bool onlyUpdateLock);

    #endregion

    #region quality control

    /// <summary>
    /// Check if the device supports stream quality control.
    /// </summary>
    /// <value></value>
    public virtual bool SupportsQualityControl
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Get the device's quality control interface.
    /// </summary>
    public virtual IQuality Quality
    {
      get
      {
        return null;
      }
    }

    #endregion

    #region EPG

    /// <summary>
    /// Get the device's ITVEPG interface, used for grabbing electronic programme guide data.
    /// </summary>
    public virtual ITVEPG EpgInterface
    {
      get
      {
        return null;
      }
    }

    /// <summary>
    /// Abort grabbing electronic programme guide data.
    /// </summary>
    public virtual void AbortGrabbing()
    {
    }

    /// <summary>
    /// Get the electronic programme guide data found in a grab session.
    /// </summary>
    /// <value>EPG data if the device supports EPG grabbing and grabbing is complete, otherwise <c>null</c></value>
    public virtual List<EpgChannel> Epg
    {
      get
      {
        return null;
      }
    }

    /// <summary>
    /// Start grabbing electronic programme guide data (idle EPG grabber).
    /// </summary>
    /// <param name="callBack">The delegate to call when grabbing is complete or canceled.</param>
    public virtual void GrabEpg(BaseEpgGrabber callBack)
    {
    }

    /// <summary>
    /// Start grabbing electronic programme guide data (timeshifting/recording EPG grabber).
    /// </summary>
    public virtual void GrabEpg()
    {
    }

    /// <summary>
    /// Activate or deactivate the EPG grabber.
    /// </summary>
    /// <param name="value"><c>True</c> to enable EPG grabbing.</param>
    protected virtual void UpdateEpgGrabber(bool value)
    {
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

    #region IDisposable members

    #endregion

    #region subchannel management

    /// <summary>
    /// Allocate a new subchannel instance.
    /// </summary>
    /// <param name="id">The identifier for the subchannel.</param>
    /// <returns>the new subchannel instance</returns>
    protected abstract ITvSubChannel CreateNewSubChannel(int id);

    /// <summary>
    /// Free a subchannel.
    /// </summary>
    /// <param name="id">The subchannel identifier.</param>
    public virtual void FreeSubChannel(int id)
    {
      this.LogDebug("TvCardBase: free subchannel, ID = {0}, count = {1}", id, _mapSubChannels.Count);
      ITvSubChannel subChannel;
      if (_mapSubChannels.TryGetValue(id, out subChannel))
      {
        if (subChannel.IsTimeShifting)
        {
          this.LogError("TvCardBase: subchannel is still timeshifting!");
          return;
        }
        if (subChannel.IsRecording)
        {
          this.LogError("TvCardBase: subchannel is still recording!");
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
    protected void FreeAllSubChannels()
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

    // TODO implement IDisposable here
  }
}
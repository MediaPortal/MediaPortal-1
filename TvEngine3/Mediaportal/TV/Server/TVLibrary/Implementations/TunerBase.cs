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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// A base class for all tuners, independent of tuner implementation and stream format.
  /// </summary>
  internal abstract class TunerBase : ITunerInternal, ITuner, IDisposable
  {
    #region events

    /// <summary>
    /// New sub-channel delegate, invoked when a new sub-channel is created.
    /// </summary>
    private OnNewSubChannelDelegate _newSubChannelEventDelegate = null;

    /// <summary>
    /// Set the tuner's new sub-channel event handler.
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
    /// Fire the new sub-channel observer event.
    /// </summary>
    /// <param name="subChannelId">The ID of the new sub-channel.</param>
    private void FireNewSubChannelEvent(int subChannelId)
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
    /// Dictionary of sub-channels.
    /// </summary>
    private Dictionary<int, ISubChannelInternal> _mapSubChannels = new Dictionary<int, ISubChannelInternal>();

    /// <summary>
    /// The ID to use for the next new sub-channel.
    /// </summary>
    private int _nextSubChannelId = 0;

    /// <summary>
    /// Context reference
    /// </summary>
    private object _context = null;

    #region identification

    /// <summary>
    /// The tuner's unique identifier.
    /// </summary>
    /// <remarks>
    /// This is the identifier for the database record which holds the tuner's
    /// settings.
    /// </remarks>
    private int _tunerId = -1;

    /// <summary>
    /// The tuner's unique external identifier.
    /// </summary>
    /// <remarks>
    /// The source of this identifier varies from implementation to
    /// implementation. For example, implementations based on DirectShow
    /// may use the IMoniker display name (AKA device path).
    /// </remarks>
    private readonly string _externalId = string.Empty;

    /// <summary>
    /// A shared identifier for all tuner instances derived from a
    /// [multi-tuner] product.
    /// </summary>
    private string _productInstanceId = null;

    /// <summary>
    /// A shared identifier for all tuner instances derived from a single
    /// physical tuner.
    /// </summary>
    private string _tunerInstanceId = null;

    /// <summary>
    /// The tuner's name.
    /// </summary>
    private string _name = string.Empty;

    /// <summary>
    /// The broadcast standards supported by the tuner hardware.
    /// </summary>
    private BroadcastStandard _supportedBroadcastStandards = BroadcastStandard.Unknown;

    #endregion

    #region signal status

    /// <summary>
    /// Indicates if the tuner is locked onto signal.
    /// </summary>
    private volatile bool _isSignalLocked = false;

    /// <summary>
    /// Indicates if the tuner has detected signal.
    /// </summary>
    private bool _isSignalPresent = false;

    /// <summary>
    /// The signal strength. Range: 0 to 100.
    /// </summary>
    private int _signalStrength = 0;

    /// <summary>
    /// The signal quality. Range: 0 to 100.
    /// </summary>
    private int _signalQuality = 0;

    /// <summary>
    /// Date and time of the last signal status update.
    /// </summary>
    private DateTime _lastSignalStatusUpdate = DateTime.MinValue;

    #endregion

    private bool _isEnabled = true;

    private bool _useForEpgGrabbing = false;

    /// <summary>
    /// The action that will be taken when the tuner is no longer being
    /// actively used.
    /// </summary>
    private TunerIdleMode _idleMode = TunerIdleMode.Stop;

    /// <summary>
    /// A list containing the extension interfaces supported by this tuner. The
    /// list is ordered by descending extension priority.
    /// </summary>
    private IList<ITunerExtension> _extensions = new List<ITunerExtension>();

    #region conditional access

    /// <summary>
    /// A list containing the conditional access provider extensions supported
    /// by this tuner. The list is ordered by descending extension priority.
    /// </summary>
    private List<IConditionalAccessProvider> _caProviders = new List<IConditionalAccessProvider>();

    /// <summary>
    /// Enable or disable the use of conditional access interface(s).
    /// </summary>
    private bool _useConditionalAccessInterface = true;

    private ICollection<string> _conditionalAccessProviders = new List<string>(20);

    /// <summary>
    /// The type of conditional access module available to the conditional
    /// access interface.
    /// </summary>
    /// <remarks>
    /// Certain conditional access modules require specific handling to ensure
    /// compatibility.
    /// </remarks>
    private CamType _camType = CamType.Default;

    /// <summary>
    /// The method that should be used to communicate the set of channels that
    /// the tuner's conditional access interface needs to manage.
    /// </summary>
    /// <remarks>
    /// Multi-channel decrypt is *not* the same as Digital Devices'
    /// multi-transponder decrypt (MTD). MCD is implmented using standard CA
    /// PMT commands; MTD is implemented in the Digital Devices drivers.
    /// Disabled = Always send Only. In most cases this will result in only one
    ///         channel being decrypted. If other methods are not working
    ///         reliably then this one should at least allow decrypting one
    ///         channel reliably.
    /// List = Explicit management using Only, First, More and Last. This is
    ///         the most widely supported set of commands, however they are not
    ///         suitable for some interfaces (such as the Digital Devices
    ///         interface).
    /// Changes = Use Add, Update and Remove to pass changes to the interface.
    ///         The full channel list is never passed. Most interfaces don't
    ///         support these commands.
    /// </remarks>
    private MultiChannelDecryptMode _multiChannelDecryptMode = MultiChannelDecryptMode.List;

    /// <summary>
    /// The number of channels that the tuner is capable of or permitted to
    /// decrypt simultaneously. Zero means there is no limit.
    /// </summary>
    private int _decryptLimit = 0;

    /// <summary>
    /// Enable or disable waiting for the conditional interface to be ready
    /// before sending commands.
    /// </summary>
    private bool _waitUntilCaInterfaceReady = true;

    /// <summary>
    /// The number of times to re-attempt decrypting the current service set
    /// when one or more services are not able to be decrypted for whatever
    /// reason.
    /// </summary>
    /// <remarks>
    /// Each available CA interface will be tried in order of priority. If
    /// decrypting is not started successfully, all interfaces are retried
    /// until each interface has been tried _decryptFailureRetryCount + 1
    /// times, or until decrypting is successful.
    /// </remarks>
    private int _decryptFailureRetryCount = 2;

    /// <summary>
    /// Is the MPEG 2 conditional access table required in order for any or all
    /// of the conditional access providers to decrypt programs.
    /// </summary>
    private bool _isConditionalAccessTableRequired = false;

    #endregion

    /// <summary>
    /// The tuner's current tuning parameter values or null if the tuner is not
    /// tuned.
    /// </summary>
    private IChannel _currentTuningDetail = null;

    /// <summary>
    /// Enable or disable the use of extensions for tuning.
    /// </summary>
    /// <remarks>
    /// Custom/direct tuning *may* be faster or more reliable than regular
    /// tuning methods. It might be slower (eg. TeVii) or more limiting (eg.
    /// Digital Everywhere) than regular tuning methods. User gets to choose
    /// which method to use.
    /// </remarks>
    private bool _useCustomTuning = false;

    /// <summary>
    /// A flag used by the TV service as a signal to abort the tuning process
    /// before it is completed.
    /// </summary>
    private volatile bool _cancelTune = false;

    /// <summary>
    /// The current state of the tuner.
    /// </summary>
    private TunerState _state = TunerState.NotLoaded;

    /// <summary>
    /// Does the tuner support receiving more than one service simultaneously?
    /// </summary>
    /// <remarks>
    /// This may seem obvious and unnecessary, especially for modern tuners.
    /// However even today there are tuners that cannot receive more than one
    /// service simultaneously. CableCARD tuners are a good example.
    /// </remarks>
    protected bool _supportsSubChannels = true;

    /// <summary>
    /// The tuner group that this tuner is a member of, if any.
    /// </summary>
    private ITunerGroup _group = null;

    /// <summary>
    /// The tuner's DiSEqC control interface.
    /// </summary>
    private IDiseqcController _diseqcController = null;

    /// <summary>
    /// The tuner's encoder control interface.
    /// </summary>
    private IQuality _encoderController = null;

    /// <summary>
    /// The maximum length of time to wait for signal detection after tuning.
    /// </summary>
    private int _timeOutWaitForSignal = 2500;   // unit = milliseconds

    #endregion

    #region constructor & finaliser

    /// <summary>
    /// Base constructor.
    /// </summary>
    /// <param name="name">The tuner's name.</param>
    /// <param name="externalId">The tuner's unique external identifier.</param>
    /// <param name="tunerInstanceId">The identifier shared by all <see cref="ITuner"/> instances derived from a single tuner.</param>
    /// <param name="productInstanceId">The identifier shared by all <see cref="ITuner"/> instances derived from a single product.</param>
    /// <param name="supportedBroadcastStandards">The broadcast standards supported by the tuner hardware.</param>
    protected TunerBase(string name, string externalId, string tunerInstanceId, string productInstanceId, BroadcastStandard supportedBroadcastStandards)
    {
      _name = name;
      _externalId = externalId;
      _tunerInstanceId = tunerInstanceId;
      _productInstanceId = productInstanceId;
      _supportedBroadcastStandards = supportedBroadcastStandards;
    }

    ~TunerBase()
    {
      Dispose(false);
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the tuner is being disposed.</param>
    protected virtual void Dispose(bool isDisposing)
    {
      Unload(!isDisposing);
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
    /// Get the broadcast standards supported by the tuner hardware.
    /// </summary>
    public BroadcastStandard SupportedBroadcastStandards
    {
      get
      {
        return _supportedBroadcastStandards;
      }
    }

    /// <summary>
    /// Get the broadcast standards supported by the tuner code/class/type implementation.
    /// </summary>
    public virtual BroadcastStandard PossibleBroadcastStandards
    {
      get
      {
        return _supportedBroadcastStandards;
      }
    }

    #region conditional access properties

    /// <summary>
    /// Get the set of conditional access providers that the tuner's
    /// conditional access interface is able to decrypt.
    /// </summary>
    public ICollection<string> ConditionalAccessProviders
    {
      get
      {
        return _conditionalAccessProviders;
      }
    }

    /// <summary>
    /// Get the type of conditional access module available to the tuner's
    /// conditional access interface.
    /// </summary>
    public CamType CamType
    {
      get
      {
        return _camType;
      }
    }

    /// <summary>
    /// Get the tuner's conditional access interface decrypt limit.
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
        return _useConditionalAccessInterface;
      }
    }

    /// <summary>
    /// Get the tuner's conditional access menu interaction interface.
    /// </summary>
    /// <value><c>null</c> if the tuner does not support conditional access</value>
    public IConditionalAccessMenuActions CaMenuInterface
    {
      get
      {
        if (!_useConditionalAccessInterface)
        {
          return null;
        }
        if (_state == TunerState.NotLoaded)
        {
          Load();
        }

        // Return the first extension that implements CA menu access.
        foreach (ITunerExtension extension in _extensions)
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
    /// Get a count of the number of services that the tuner is currently decrypting.
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
        Dictionary<int, ISubChannelInternal>.Enumerator en = _mapSubChannels.GetEnumerator();
        while (en.MoveNext())
        {
          IChannel service = en.Current.Value.CurrentChannel;
          if (service.IsEncrypted)
          {
            ChannelMpeg2Base mpeg2Service = service as ChannelMpeg2Base;
            if (mpeg2Service != null)
            {
              decryptedServices.Add(mpeg2Service.ProgramNumber);
            }
            else
            {
              ChannelAnalogTv analogTvService = service as ChannelAnalogTv;
              if (analogTvService != null)
              {
                decryptedServices.Add(analogTvService.Frequency);
              }
              else
              {
                throw new TvException("tuner base: service type not recognised, unable to count number of services being decrypted\r\n" + service.ToString());
              }
            }
          }
        }

        return decryptedServices.Count;
      }
    }

    #endregion

    /// <summary>
    /// Gets or sets the context.
    /// </summary>
    /// <value>The context.</value>
    public object Context
    {
      get
      {
        return _context;
      }
      set
      {
        _context = value;
      }
    }

    /// <summary>
    /// Get the tuning parameters that have been applied to the hardware.
    /// </summary>
    public IChannel CurrentTuningDetail
    {
      get
      {
        return _currentTuningDetail;
      }
    }

    /// <summary>
    /// Get the state of the tuner's enable/disable setting.
    /// </summary>
    public bool IsEnabled
    {
      get
      {
        return _isEnabled;
      }
    }

    /// <summary>
    /// Does configuration allow the tuner to be used for EPG grabbing?
    /// </summary>
    /// <value><c>true</c> if the tuner configuration allows EPG grabbing, otherwise <c>false</c></value>
    public bool IsEpgGrabbingAllowed
    {
      get
      {
        return _useForEpgGrabbing;
      }
    }

    #region interfaces

    /// <summary>
    /// Get the tuner's channel linkage scanning interface.
    /// </summary>
    public IChannelLinkageScanner ChannelLinkageScanningInterface
    {
      get
      {
        if (_state == TunerState.NotLoaded)
        {
          Load();
        }
        return InternalChannelLinkageScanningInterface;
      }
    }

    /// <summary>
    /// Get the tuner's channel linkage scanning interface.
    /// </summary>
    public virtual IChannelLinkageScanner InternalChannelLinkageScanningInterface
    {
      get
      {
        return null;
      }
    }

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public IChannelScanner ChannelScanningInterface
    {
      get
      {
        if (_state == TunerState.NotLoaded)
        {
          Load();
        }
        return InternalChannelScanningInterface;
      }
    }

    /// <summary>
    /// Get the tuner's channel scanning interface.
    /// </summary>
    public abstract IChannelScannerInternal InternalChannelScanningInterface
    {
      get;
    }

    /// <summary>
    /// Get the tuner's DiSEqC control interface.
    /// </summary>
    /// <value><c>null</c> if the tuner is not a satellite tuner or the tuner
    /// does not support sending/receiving DiSEqC commands</value>
    public IDiseqcController DiseqcController
    {
      get
      {
        if (_state == TunerState.NotLoaded)
        {
          Load();
        }
        return _diseqcController;
      }
    }

    /// <summary>
    /// Get the tuner's electronic programme guide data grabbing interface.
    /// </summary>
    public IEpgGrabber EpgGrabberInterface
    {
      get
      {
        if (_state == TunerState.NotLoaded)
        {
          Load();
        }
        return InternalEpgGrabberInterface;
      }
    }

    /// <summary>
    /// Get the tuner's electronic programme guide data grabbing interface.
    /// </summary>
    public virtual IEpgGrabber InternalEpgGrabberInterface
    {
      get
      {
        return null;
      }
    }

    /// <summary>
    /// Get the tuner's quality control interface.
    /// </summary>
    public virtual IQuality QualityControlInterface
    {
      get
      {
        if (_state == TunerState.NotLoaded)
        {
          Load();
        }
        return _encoderController;
      }
    }

    #endregion

    #endregion

    #region configuration

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    public void ReloadConfiguration()
    {
      this.LogDebug("tuner base: reload configuration");

      Tuner config = null;
      if (ExternalId != null)
      {
        config = TunerManagement.GetTunerByExternalId(ExternalId, TunerIncludeRelationEnum.AnalogTunerSettings | TunerIncludeRelationEnum.TunerProperties);
        if (config != null)
        {
          this.LogDebug("  ID                  = {0}", config.IdTuner);
          this.LogDebug("  name                = {0}", config.Name);
          this.LogDebug("  external ID         = {0}", config.ExternalId);
          this.LogDebug("  standards           = {0}", (BroadcastStandard)config.SupportedBroadcastStandards);
          this.LogDebug("  tuner group ID      = {0}", config.IdTunerGroup == null ? "[null]" : config.IdTunerGroup.ToString());
          this.LogDebug("  enabled?            = {0}", config.IsEnabled);
          this.LogDebug("  priority            = {0}", config.Priority);
          this.LogDebug("  EPG grabbing?       = {0}", config.UseForEpgGrabbing);
          this.LogDebug("  preload?            = {0}", config.Preload);
          this.LogDebug("  conditional access? = {0}", config.UseConditionalAccess);
          this.LogDebug("    providers         = {0}", config.ConditionalAccessProviders);
          this.LogDebug("    CAM type          = {0}", (CamType)config.CamType);
          this.LogDebug("    decrypt limit     = {0}", config.DecryptLimit);
          this.LogDebug("    MCD mode          = {0}", (MultiChannelDecryptMode)config.MultiChannelDecryptMode);
          this.LogDebug("  idle mode           = {0}", (TunerIdleMode)config.IdleMode);
          this.LogDebug("  PID filter mode     = {0}", (PidFilterMode)config.PidFilterMode);
          this.LogDebug("  custom tuning?      = {0}", config.UseCustomTuning);

          _tunerId = config.IdTuner;
          _name = config.Name;
          _isEnabled = config.IsEnabled;
          _idleMode = (TunerIdleMode)config.IdleMode;
          _pidFilterMode = (PidFilterMode)config.PidFilterMode;
          _useCustomTuning = config.UseCustomTuning;

          BroadcastStandard configuredStandards = (BroadcastStandard)config.SupportedBroadcastStandards;
          BroadcastStandard impossibleStandards = configuredStandards & ~PossibleBroadcastStandards;
          if (impossibleStandards != BroadcastStandard.Unknown)
          {
            this.LogWarn("tuner base: configuration attempts to enable support for broadcast standard(s) not supported by code, impossible standards = [{0}]", impossibleStandards);
            configuredStandards &= PossibleBroadcastStandards;
            config.SupportedBroadcastStandards = (int)configuredStandards;
            TunerManagement.SaveTuner(config);
          }
          if (configuredStandards != SupportedBroadcastStandards)
          {
            this.LogDebug("tuner base: configuration enables/disables support for broadcast standard(s)...");
            this.LogDebug("  detected      = {0}", SupportedBroadcastStandards);
            this.LogDebug("  configuration = {0}", configuredStandards);
            _supportedBroadcastStandards = configuredStandards;
          }

          if (_useForEpgGrabbing && !config.UseForEpgGrabbing && InternalEpgGrabberInterface != null && InternalEpgGrabberInterface.IsGrabbing)
          {
            this.LogDebug("tuner base: EPG grabbing disabled, cancelling grab");
            InternalEpgGrabberInterface.AbortGrabbing();
          }
          _useForEpgGrabbing = config.UseForEpgGrabbing;

          // Conditional access...
          if (!_useConditionalAccessInterface && config.UseConditionalAccess && _state != TunerState.NotLoaded && _caProviders.Count == 0)
          {
            this.LogDebug("tuner base: conditional access enabled, opening provider(s)");
            _isConditionalAccessTableRequired = false;
            foreach (ITunerExtension extension in _extensions)
            {
              IConditionalAccessProvider caProvider = extension as IConditionalAccessProvider;
              if (caProvider != null)
              {
                this.LogDebug("tuner base: found conditional access provider \"{0}\"", extension.Name);
                if (caProvider.Open())
                {
                  _caProviders.Add(caProvider);
                  _isConditionalAccessTableRequired |= caProvider.IsConditionalAccessTableRequiredForDecryption();
                }
                else
                {
                  this.LogDebug("tuner base: provider will not be used");
                }
              }
            }
          }
          _useConditionalAccessInterface = config.UseConditionalAccess;
          foreach (string provider in config.ConditionalAccessProviders.Split(','))
          {
            _conditionalAccessProviders.Add(provider.Trim());
          }
          _camType = (CamType)config.CamType;
          _decryptLimit = config.DecryptLimit;
          _multiChannelDecryptMode = (MultiChannelDecryptMode)config.MultiChannelDecryptMode;

          if (_state == TunerState.NotLoaded && config.Preload)
          {
            Load();
          }
        }
      }

      _timeOutWaitForSignal = SettingsManagement.GetValue("timeOutSignal", 2500);

      foreach (ISubChannelInternal subChannel in _mapSubChannels.Values)
      {
        subChannel.ReloadConfiguration();
      }

      if (InternalEpgGrabberInterface != null)
      {
        InternalEpgGrabberInterface.ReloadConfiguration();
      }
      if (_diseqcController != null)
      {
        _diseqcController.ReloadConfiguration();
      }

      ReloadConfiguration(config);
    }

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    public abstract void ReloadConfiguration(TVDatabase.Entities.Tuner configuration);

    #endregion

    /// <summary>
    /// Get a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/></returns>
    public override string ToString()
    {
      return _name;
    }

    #region loading and unloading

    /// <summary>
    /// Load the tuner.
    /// </summary>
    private void Load()
    {
      if (_state == TunerState.Loading)
      {
        this.LogWarn("tuner base: the tuner is already loading");
        return;
      }
      if (_state != TunerState.NotLoaded)
      {
        this.LogWarn("tuner base: the tuner is already loaded");
        return;
      }
      _state = TunerState.Loading;

      // Related tuners must be unloaded before this tuner can be loaded.
      if (_group != null)
      {
        this.LogDebug("tuner base: unload tuners in group");
        foreach (ITuner tuner in _group.Tuners)
        {
          if (tuner.TunerId != TunerId)
          {
            TunerBase tunerBase = tuner as TunerBase;
            if (tunerBase != null)
            {
              tunerBase.Unload();
            }
          }
        }
      }

      this.LogDebug("tuner base: load tuner");
      try
      {
        ReloadConfiguration();
        _extensions = PerformLoading();

        _state = TunerState.Stopped;

        // Open any extensions that were detected during loading. This is
        // separated from loading because some extensions can't be opened until
        // the tuner has fully loaded.
        OpenExtensions();

        // Extensions can request to pause or start the tuner - other actions
        // don't make sense here. The started state is considered more
        // compatible than the paused state, so start takes precedence.
        TunerAction actualAction = TunerAction.Default;
        foreach (ITunerExtension extension in _extensions)
        {
          TunerAction action;
          extension.OnLoaded(this, out action);
          if (action == TunerAction.Pause)
          {
            if (actualAction == TunerAction.Default)
            {
              this.LogDebug("tuner base: extension \"{0}\" will cause tuner pause", extension.Name);
              actualAction = TunerAction.Pause;
            }
            else
            {
              this.LogDebug("tuner base: extension \"{0}\" wants to pause the tuner, overriden", extension.Name);
            }
          }
          else if (action == TunerAction.Start)
          {
            this.LogDebug("tuner base: extension \"{0}\" will cause tuner start", extension.Name);
            actualAction = action;
          }
          else if (action != TunerAction.Default)
          {
            this.LogDebug("tuner base: extension \"{0}\" wants unsupported action {1}", extension.Name, action);
          }
        }

        if (actualAction == TunerAction.Default && _idleMode == TunerIdleMode.AlwaysOn)
        {
          this.LogDebug("tuner base: tuner is configured as always on");
          actualAction = TunerAction.Start;
        }

        if (actualAction != TunerAction.Default)
        {
          PerformTunerAction(actualAction);
        }
      }
      catch (TvExceptionNeedSoftwareEncoder)
      {
        Unload();
        throw;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "tuner base: failed to load tuner {0}", TunerId);
        Unload();
        throw new TvExceptionTunerLoadFailed(TunerId, ex);
      }
    }

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public abstract IList<ITunerExtension> PerformLoading();

    /// <summary>
    /// Open any <see cref="ITunerExtension">extensions</see> loaded for this
    /// tuner.
    /// </summary>
    /// <remarks>
    /// We separate this from the loading because some extensions (for example, the NetUP extension)
    /// can't be opened until the graph has finished being built.
    /// </remarks>
    private void OpenExtensions()
    {
      this.LogDebug("tuner base: open tuner extensions");

      _isConditionalAccessTableRequired = false;
      foreach (ITunerExtension extension in _extensions)
      {
        if (_useConditionalAccessInterface)
        {
          IConditionalAccessProvider caProvider = extension as IConditionalAccessProvider;
          if (caProvider != null)
          {
            this.LogDebug("tuner base: found conditional access provider \"{0}\"", extension.Name);
            if (caProvider.Open())
            {
              _caProviders.Add(caProvider);
              _isConditionalAccessTableRequired |= caProvider.IsConditionalAccessTableRequiredForDecryption();
            }
            else
            {
              this.LogDebug("tuner base: provider will not be used");
            }
          }
        }
        if (_diseqcController == null)
        {
          IDiseqcDevice diseqcDevice = extension as IDiseqcDevice;
          if (diseqcDevice != null)
          {
            this.LogDebug("tuner base: found DiSEqC control interface \"{0}\"", extension.Name);
            _diseqcController = new DiseqcController(TunerId, diseqcDevice);
          }
        }
        if (_encoderController == null)
        {
          IEncoder encoder = extension as IEncoder;
          if (encoder != null)
          {
            this.LogDebug("tuner base: found encoder control interface \"{0}\"", extension.Name);
            _encoderController = new EncoderController(_extensions);
          }
        }
        IRemoteControlListener rcListener = extension as IRemoteControlListener;
        if (rcListener != null)
        {
          this.LogDebug("tuner base: found remote control interface \"{0}\"", extension.Name);
          if (!rcListener.Open())
          {
            this.LogDebug("tuner base: interface will not be used");
          }
        }
      }
    }

    /// <summary>
    /// Unload the tuner.
    /// </summary>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    private void Unload(bool isFinalising = false)
    {
      this.LogDebug("tuner base: unload tuner");
      if (!isFinalising)
      {
        FreeAllSubChannels();
      }
      try
      {
        PerformTunerAction(TunerAction.Stop, isFinalising);
      }
      catch (Exception ex)
      {
        this.LogError(ex, "tuner base: failed to stop tuner before unloading");
      }

      // Dispose extensions.
      if (!isFinalising)
      {
        if (_extensions != null)
        {
          foreach (ITunerExtension extension in _extensions)
          {
            // Avoid recursive loop for ITVCard implementations that also implement ITunerExtension.
            if (!(extension is ITuner))
            {
              IDisposable d = extension as IDisposable;
              if (d != null)
              {
                d.Dispose();
              }
            }
          }
        }
        _caProviders.Clear();
        _extensions.Clear();
      }

      try
      {
        PerformUnloading(isFinalising);
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "tuner base: failed to completely unload the tuner");
      }

      if (!isFinalising)
      {
        _diseqcController = null;
        _encoderController = null;

        _state = TunerState.NotLoaded;
      }
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public abstract void PerformUnloading(bool isFinalising = false);

    #endregion

    #region tuner state control

    /// <summary>
    /// Stop the tuner.
    /// </summary>
    /// <remarks>
    /// The actual result of this function depends on tuner configuration.
    /// </remarks>
    public void Stop()
    {
      this.LogDebug("tuner base: stop, idle mode = {0}", _idleMode);
      TunerAction action = TunerAction.Stop;
      try
      {
        if (InternalEpgGrabberInterface != null && InternalEpgGrabberInterface.IsGrabbing)
        {
          InternalEpgGrabberInterface.AbortGrabbing();
        }
        if (InternalChannelScanningInterface != null && InternalChannelScanningInterface.IsScanning)
        {
          InternalChannelScanningInterface.AbortScanning();
        }
        FreeAllSubChannels();

        switch (_idleMode)
        {
          case TunerIdleMode.Pause:
            action = TunerAction.Pause;
            break;
          case TunerIdleMode.Unload:
            action = TunerAction.Unload;
            break;
          case TunerIdleMode.AlwaysOn:
            action = TunerAction.Start;
            break;
        }

        // Extensions may want to prevent or direct actions to ensure
        // compatibility and smooth tuner operation.
        TunerAction extensionAction = action;
        foreach (ITunerExtension extension in _extensions)
        {
          extension.OnStop(this, ref extensionAction);
          if (extensionAction > action)
          {
            this.LogDebug("tuner base: extension \"{0}\" overrides action {1} with {2}", extension.Name, action, extensionAction);
            action = extensionAction;
          }
          else if (action != extensionAction)
          {
            this.LogWarn("tuner base: extension \"{0}\" wants to perform action {1}, overriden", extension.Name, extensionAction);
          }
        }

        try
        {
          PerformTunerAction(action);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "tuner base: failed to stop tuner with action {0}", action);
        }

        // Turn off the tuner power.
        foreach (ITunerExtension extension in _extensions)
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
    /// <param name="action">The action to perform.</param>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    private void PerformTunerAction(TunerAction action, bool isFinalising = false)
    {
      // Don't do anything if the tuner is not loaded.
      if (_state == TunerState.NotLoaded)
      {
        return;
      }
      this.LogDebug("tuner base: perform tuner action, action = {0}", action);

      if (action == TunerAction.Reset)
      {
        Unload(isFinalising);
        Load();
      }
      else if (action == TunerAction.Unload)
      {
        Unload(isFinalising);
      }
      else if (action == TunerAction.Pause)
      {
        SetTunerState(TunerState.Paused, isFinalising);
      }
      else if (action == TunerAction.Stop)
      {
        SetTunerState(TunerState.Stopped, isFinalising);
      }
      else if (action == TunerAction.Start)
      {
        SetTunerState(TunerState.Started, isFinalising);
      }
      else if (action == TunerAction.Restart)
      {
        SetTunerState(TunerState.Stopped, isFinalising);
        SetTunerState(TunerState.Started, isFinalising);
      }
      else
      {
        this.LogWarn("tuner base: unhandled action {0}", action);
        return;
      }

      this.LogDebug("tuner base: action succeeded");
    }

    /// <summary>
    /// Set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    private void SetTunerState(TunerState state, bool isFinalising = false)
    {
      this.LogDebug("tuner base: set tuner state, current state = {0}, requested state = {1}", _state, state);

      if (state == _state)
      {
        this.LogDebug("tuner base: tuner already in required state");
        return;
      }

      PerformSetTunerState(state, isFinalising);
      _state = state;
    }

    /// <summary>
    /// Actually set the state of the tuner.
    /// </summary>
    /// <param name="state">The state to apply to the tuner.</param>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public abstract void PerformSetTunerState(TunerState state, bool isFinalising = false);

    #endregion

    #region tuning

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public virtual bool CanTune(IChannel channel)
    {
      if (
        (channel is ChannelAnalogTv && _supportedBroadcastStandards.HasFlag(BroadcastStandard.AnalogTelevision)) ||
        (channel is ChannelAtsc && _supportedBroadcastStandards.HasFlag(BroadcastStandard.Atsc)) ||
        (channel is ChannelCapture && _supportedBroadcastStandards.HasFlag(BroadcastStandard.ExternalInput)) ||
        (channel is ChannelDigiCipher2 && _supportedBroadcastStandards.HasFlag(BroadcastStandard.DigiCipher2)) ||
        (channel is ChannelDvbC && _supportedBroadcastStandards.HasFlag(BroadcastStandard.DvbC)) ||
        (channel is ChannelDvbC2 && _supportedBroadcastStandards.HasFlag(BroadcastStandard.DvbC2)) ||
        (channel is ChannelDvbS && _supportedBroadcastStandards.HasFlag(BroadcastStandard.DvbS)) ||
        (channel is ChannelDvbS2 && _supportedBroadcastStandards.HasFlag(BroadcastStandard.DvbS2)) ||
        (channel is ChannelDvbT && _supportedBroadcastStandards.HasFlag(BroadcastStandard.DvbT)) ||
        (channel is ChannelDvbT2 && _supportedBroadcastStandards.HasFlag(BroadcastStandard.DvbT2)) ||
        (channel is ChannelFmRadio && _supportedBroadcastStandards.HasFlag(BroadcastStandard.FmRadio)) ||
        (channel is ChannelSatelliteTurboFec && _supportedBroadcastStandards.HasFlag(BroadcastStandard.SatelliteTurboFec)) ||
        (channel is ChannelScte && _supportedBroadcastStandards.HasFlag(BroadcastStandard.Scte)) ||
        (channel is ChannelStream && _supportedBroadcastStandards.HasFlag(BroadcastStandard.DvbIp))
      )
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Tune to a specific channel.
    /// </summary>
    /// <param name="subChannelId">The ID of the sub-channel associated with the channel that is being tuned.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the sub-channel associated with the tuned channel</returns>
    public virtual ISubChannel Tune(int subChannelId, IChannel channel)
    {
      this.LogDebug("tuner base: tune channel, {0}", channel);
      _cancelTune = false;
      ISubChannelInternal subChannel = null;
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
            // Existing sub-channel.
            if (_mapSubChannels.Count != 1)
            {
              // If this is not the only sub-channel then by definition this
              // must be an attempt to tune a new service. Not allowed.
              throw new TvException("Tuner is not able to receive more than one service.");
            }
          }
          else
          {
            // New sub-channel.
            Dictionary<int, ISubChannelInternal>.ValueCollection.Enumerator en = _mapSubChannels.Values.GetEnumerator();
            en.MoveNext();
            if (en.Current.CurrentChannel != channel)
            {
              // The tuner is currently streaming a different service.
              throw new TvException("Tuner is not able to receive more than one service.");
            }
          }
        }

        // Get a sub-channel for the service.
        string description;
        if (subChannel == null && !_mapSubChannels.TryGetValue(subChannelId, out subChannel))
        {
          description = "creating new sub-channel";
          subChannelId = _nextSubChannelId++;
          subChannel = CreateNewSubChannel(subChannelId);
          _mapSubChannels[subChannelId] = subChannel;
          FireNewSubChannelEvent(subChannelId);
        }
        else
        {
          description = "using existing sub-channel";
          // If reusing a sub-channel and our multi-channel decrypt mode is
          // "changes", tell the extension to stop decrypting the previous
          // service before we lose access to the PMT and CAT.
          if (_multiChannelDecryptMode == MultiChannelDecryptMode.Changes)
          {
            UpdateDecryptList(subChannelId, CaPmtListManagementAction.Last);
          }
        }
        this.LogInfo("tuner base: {0}, ID = {1}, count = {2}", description, subChannelId, _mapSubChannels.Count);
        subChannel.CurrentChannel = channel;
        SubChannelMpeg2Ts mpeg2TsSubChannel = subChannel as SubChannelMpeg2Ts;
        if (mpeg2TsSubChannel != null)
        {
          mpeg2TsSubChannel.IsConditionalAccessTableRequired = _isConditionalAccessTableRequired;
        }

        // Sub-channel OnBeforeTune().
        subChannel.OnBeforeTune();

        // Do we need to tune?
        bool tuned = false;
        if (_currentTuningDetail == null || _currentTuningDetail.IsDifferentTransmitter(channel))
        {
          tuned = true;
          // Stop the EPG grabber. We're going to move to a different channel.
          // Any EPG data that has been grabbed but not stored is thrown away.
          if (InternalEpgGrabberInterface != null && InternalEpgGrabberInterface.IsGrabbing)
          {
            InternalEpgGrabberInterface.AbortGrabbing();
          }

          // When we call ITunerExtension.OnBeforeTune(), the extension may
          // modify the tuning parameters. However, the original channel object
          // *must not* be modified otherwise IsDifferentTransponder() will
          // sometimes returns true when it shouldn't. See mantis 0002979.
          IChannel tuneChannel = (IChannel)channel.Clone();

          // Extension OnBeforeTune().
          TunerAction action = TunerAction.Default;
          foreach (ITunerExtension extension in _extensions)
          {
            if (_currentTuningDetail == null)
            {
              // Turn on power. This usually needs to happen before tuning.
              IPowerDevice powerDevice = extension as IPowerDevice;
              if (powerDevice != null)
              {
                powerDevice.SetPowerState(PowerState.On);
              }
            }

            TunerAction extensionAction;
            extension.OnBeforeTune(this, _currentTuningDetail, ref tuneChannel, out extensionAction);
            if (extensionAction != TunerAction.Unload && extensionAction != TunerAction.Default)
            {
              // Valid action requested...
              if (extensionAction > action)
              {
                this.LogDebug("tuner base: extension \"{0}\" overrides action {1} with {2}", extension.Name, action, extensionAction);
                action = extensionAction;
              }
              else if (extensionAction != action)
              {
                this.LogWarn("tuner base: extension \"{0}\" wants to perform action {1}, overriden", extension.Name, extensionAction);
              }
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
            _diseqcController.SwitchToChannel(channel as IChannelSatellite);
          }

          // Apply tuning parameters.
          ThrowExceptionIfTuneCancelled();
          _isSignalLocked = false;
          if (_useCustomTuning)
          {
            foreach (ITunerExtension extension in _extensions)
            {
              ICustomTuner customTuner = extension as ICustomTuner;
              if (customTuner != null && customTuner.CanTuneChannel(channel))
              {
                this.LogDebug("tuner base: using custom tuning");
                if (!customTuner.Tune(tuneChannel))
                {
                  ThrowExceptionIfTuneCancelled();
                  this.LogWarn("tuner base: custom tuning failed, falling back to default tuning");
                  PerformTuning(tuneChannel);
                }
                break;
              }
            }
          }
          else
          {
            PerformTuning(tuneChannel);
          }
          _lastSignalStatusUpdate = DateTime.MinValue;
          ThrowExceptionIfTuneCancelled();

          // Extension OnAfterTune().
          foreach (ITunerExtension extension in _extensions)
          {
            extension.OnAfterTune(this, channel);
          }
        }

        // Sub-channel OnAfterTune().
        subChannel.OnAfterTune();

        _currentTuningDetail = channel;

        // Start the tuner.
        ThrowExceptionIfTuneCancelled();
        PerformTunerAction(TunerAction.Start);
        ThrowExceptionIfTuneCancelled();

        // Extension OnStarted().
        foreach (ITunerExtension extension in _extensions)
        {
          extension.OnStarted(this, channel);
        }

        LockInOnSignal();
        this.LogDebug("tuner base: signal, locked = {0}, present = {1}, strength = {2} %, quality = {3} %", _isSignalLocked, _isSignalPresent, _signalStrength, _signalQuality);

        subChannel.AfterTuneEvent -= FireAfterTuneEvent;
        subChannel.AfterTuneEvent += FireAfterTuneEvent;

        // Ensure that data/streams which are required to detect the service will pass through the
        // tuner's PID filter.
        ConfigurePidFilter(tuned);
        ThrowExceptionIfTuneCancelled();

        subChannel.OnGraphRunning();

        // At this point we should know which data/streams form the service(s) that are being
        // accessed. We need to ensure those streams will pass through the tuner's PID filter.
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
    /// Wait for the tuner to acquire signal lock.
    /// </summary>
    private void LockInOnSignal()
    {
      this.LogDebug("tuner base: lock in on signal");
      DateTime timeStart = DateTime.Now;
      TimeSpan ts = timeStart - timeStart;
      bool isLocked;
      while (ts.TotalMilliseconds < _timeOutWaitForSignal)
      {
        ThrowExceptionIfTuneCancelled();
        GetSignalStatus(out isLocked, out _isSignalPresent, out _signalStrength, out _signalQuality, true);
        _isSignalLocked = isLocked;
        if (isLocked)
        {
          this.LogDebug("tuner base: locked");
          return;
        }
        ts = DateTime.Now - timeStart;
        System.Threading.Thread.Sleep(20);
      }

      throw new TvExceptionNoSignal(TunerId, _currentTuningDetail);
    }

    /// <summary>
    /// Cancel the current tuning process.
    /// </summary>
    /// <param name="subChannelId">The ID of the sub-channel associated with the channel that is being cancelled.</param>
    public void CancelTune(int subChannelId)
    {
      this.LogDebug("tuner base: sub-channel {0} cancel tune", subChannelId);
      _cancelTune = true;
      ISubChannelInternal subChannel;
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

    #endregion

    #region signal status

    /// <summary>
    /// Get the tuner's signal status information.
    /// </summary>
    /// <param name="forceUpdate"><c>True</c> to force the signal status to be updated, and not use cached information.</param>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    public void GetSignalStatus(bool forceUpdate, out bool isLocked, out bool isPresent, out int strength, out int quality)
    {
      try
      {
        if (!forceUpdate)
        {
          TimeSpan ts = DateTime.Now - _lastSignalStatusUpdate;
          if (ts.TotalMilliseconds < 5000)
          {
            return;
          }
        }
        if (_currentTuningDetail == null || _state != TunerState.Started)
        {
          _isSignalLocked = false;
          _isSignalPresent = false;
          _signalStrength = 0;
          _signalQuality = 0;
        }
        else
        {
          GetSignalStatus(out isLocked, out _isSignalPresent, out _signalStrength, out _signalQuality, false);
          _isSignalLocked = isLocked;
          _lastSignalStatusUpdate = DateTime.Now;
        }
      }
      finally
      {
        isLocked = _isSignalLocked;
        isPresent = _isSignalPresent;
        strength = _signalStrength;
        quality = _signalQuality;
      }
    }

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <remarks>
    /// The <paramref name="onlyGetLock"/> parameter exists as a speed
    /// optimisation. Getting strength and quality readings can be slow.
    /// </remarks>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    /// <param name="onlyGetLock"><c>True</c> to only get lock status.</param>
    public abstract void GetSignalStatus(out bool isLocked, out bool isPresent, out int strength, out int quality, bool onlyGetLock);

    #endregion

    #region sub-channel management

    /// <summary>
    /// Allocate a new sub-channel instance.
    /// </summary>
    /// <param name="id">The identifier for the sub-channel.</param>
    /// <returns>the new sub-channel instance</returns>
    public abstract ISubChannelInternal CreateNewSubChannel(int id);

    /// <summary>
    /// Free a sub-channel.
    /// </summary>
    /// <param name="id">The sub-channel identifier.</param>
    public void FreeSubChannel(int id)
    {
      this.LogDebug("tuner base: free sub-channel, ID = {0}, count = {1}", id, _mapSubChannels.Count);
      ISubChannelInternal subChannel;
      if (_mapSubChannels.TryGetValue(id, out subChannel))
      {
        if (subChannel.IsTimeShifting)
        {
          this.LogError("tuner base: asked to free sub-channel that is still timeshifting!");
          return;
        }
        if (subChannel.IsRecording)
        {
          this.LogError("tuner base: asked to free sub-channel that is still recording!");
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
        // filter *after* we removed the sub-channel (otherwise PIDs for the sub-channel that we're freeing
        // won't be removed).
        ConfigurePidFilter();
      }
      else
      {
        this.LogWarn("tuner base: sub-channel not found!");
      }
      if (_mapSubChannels.Count == 0)
      {
        this.LogDebug("tuner base: no sub-channels present, stopping tuner");
        _nextSubChannelId = 0;
        Stop();
      }
      else
      {
        this.LogDebug("tuner base: sub-channels still present, leave tuner running");
      }
    }

    /// <summary>
    /// Free all sub-channels.
    /// </summary>
    private void FreeAllSubChannels()
    {
      this.LogInfo("tuner base: free all sub-channels, count = {0}", _mapSubChannels.Count);
      Dictionary<int, ISubChannelInternal>.Enumerator en = _mapSubChannels.GetEnumerator();
      while (en.MoveNext())
      {
        en.Current.Value.Decompose();
      }
      _mapSubChannels.Clear();
      _nextSubChannelId = 0;
    }

    /// <summary>
    /// Get a specific sub-channel.
    /// </summary>
    /// <param name="id">The ID of the sub-channel.</param>
    /// <returns></returns>
    public ISubChannel GetSubChannel(int id)
    {
      ISubChannelInternal subChannel = null;
      if (_mapSubChannels != null)
      {
        _mapSubChannels.TryGetValue(id, out subChannel);
      }
      return subChannel;
    }

    /// <summary>
    /// Get the tuner's sub-channels.
    /// </summary>
    /// <value>An array containing the sub-channels.</value>
    public ISubChannel[] SubChannels
    {
      get
      {
        int count = 0;
        ISubChannel[] channels = new ISubChannel[_mapSubChannels.Count];
        Dictionary<int, ISubChannelInternal>.Enumerator en = _mapSubChannels.GetEnumerator();
        while (en.MoveNext())
        {
          channels[count++] = en.Current.Value;
        }
        return channels;
      }
    }

    #endregion

    #region move me to sub-channel manager

    /// <summary>
    /// The mode to use for controlling tuner PID filter(s).
    /// </summary>
    /// <remarks>
    /// This setting can be used to enable or disable the tuner's PID filter even when the tuning context
    /// (for example, DVB-S vs. DVB-S2) would usually result in different behaviour. Note that it is usually
    /// not ideal to have to manually enable or disable a PID filter as it can affect tuning reliability.
    /// </remarks>
    private PidFilterMode _pidFilterMode = PidFilterMode.Automatic;
    private HashSet<ushort> _previousPids = new HashSet<ushort>();

    /// <summary>
    /// Configure the tuner's PID filter(s) to enable receiving the PIDs for each of the current sub-channels.
    /// </summary>
    private void ConfigurePidFilter(bool isTune = false)
    {
      this.LogDebug("Mpeg2TunerController: configure PID filter, mode = {0}", _pidFilterMode);

      if (_mapSubChannels == null || _mapSubChannels.Count == 0)
      {
        this.LogDebug("Mpeg2TunerController: no sub-channels");
        return;
      }

      HashSet<ushort> pidSet = null;
      HashSet<ushort> pidsOverflow = new HashSet<ushort>();
      HashSet<ushort> pidsToAdd = null;
      HashSet<ushort> pidsToRemove = null;
      foreach (ITunerExtension e in _extensions)
      {
        IMpeg2PidFilter filter = e as IMpeg2PidFilter;
        if (filter == null)
        {
          continue;
        }

        this.LogDebug("Mpeg2TunerController: found PID filter controller interface \"{0}\"", filter.Name);

        if (_pidFilterMode == PidFilterMode.Disabled || (_pidFilterMode == PidFilterMode.Automatic && !filter.ShouldEnable(_currentTuningDetail)))
        {
          filter.Disable();
          _previousPids.Clear();
          return;
        }

        if (isTune)
        {
          _previousPids.Clear();
        }
        this.LogDebug("Mpeg2TunerController: current, count = {0}, PIDs = {1}", _previousPids.Count, string.Join(", ", _previousPids));
        pidSet = new HashSet<ushort>();
        foreach (ISubChannel subChannel in _mapSubChannels.Values)
        {
          SubChannelMpeg2Ts dvbChannel = subChannel as SubChannelMpeg2Ts;
          if (dvbChannel != null && dvbChannel.Pids != null)
          {
            // Build a distinct super-set of PIDs used by the sub-channels.
            pidSet.UnionWith(dvbChannel.Pids);
          }
        }
        this.LogDebug("Mpeg2TunerController: required, count = {0}, PIDs = {1}", pidSet.Count, string.Join(", ", pidSet));

        bool tooManyPids = (filter.MaximumPidCount > 0 && pidSet.Count > filter.MaximumPidCount);
        if (tooManyPids && _pidFilterMode == PidFilterMode.Automatic)
        {
          this.LogDebug("Mpeg2TunerController: PID count exceeds filter limit {0}, disabling filter", filter.MaximumPidCount);
          filter.Disable();
          _previousPids.Clear();
          return;
        }

        pidsToAdd = new HashSet<ushort>(pidSet);
        pidsToAdd.ExceptWith(_previousPids);
        pidsToRemove = new HashSet<ushort>(_previousPids);
        pidsToRemove.ExceptWith(pidSet);
        if (pidsToAdd.Count == 0 && pidsToRemove.Count == 0)
        {
          this.LogDebug("Mpeg2TunerController: nothing to do");
          return;
        }

        if (pidsToRemove.Count > 0)
        {
          this.LogDebug("Mpeg2TunerController: remove, count = {0}, PIDs = {1}", pidsToRemove.Count, string.Join(", ", pidsToRemove));
          filter.AllowStreams(pidsToRemove);
          _previousPids.ExceptWith(pidsToRemove);
          if (pidsToAdd.Count == 0)
          {
            filter.ApplyConfiguration();
            return;
          }
        }

        if (tooManyPids)
        {
          HashSet<ushort> pidsAdded = new HashSet<ushort>();
          foreach (ushort pid in pidsToAdd)
          {
            if (_previousPids.Count >= filter.MaximumPidCount)
            {
              break;
            }
            pidsAdded.Add(pid);
          }
          this.LogDebug("Mpeg2TunerController: add, count = {0}, PIDs = {1}", pidsAdded.Count, string.Join(", ", pidsAdded));
          filter.AllowStreams(pidsAdded);
          _previousPids.UnionWith(pidsAdded);
          pidsToAdd.ExceptWith(pidsAdded);
          this.LogDebug("Mpeg2TunerController: overflow, count = {0}, PIDs = {1}", pidsToAdd.Count, string.Join(", ", pidsToAdd));
        }
        else
        {
          this.LogDebug("Mpeg2TunerController: add, count = {0}, PIDs = {1}", pidsToAdd.Count, string.Join(", ", pidsToAdd));
          filter.AllowStreams(pidsToAdd);
          _previousPids.UnionWith(pidsToAdd);
        }
        filter.ApplyConfiguration();
        return;
      }
    }

    /// <summary>
    /// Update the list of services being decrypted by the device's conditional access interfaces(s).
    /// </summary>
    /// <remarks>
    /// The strategy here is usually to only send commands to the CAM when we need an *additional* service
    /// to be decrypted. The *only* exception is when we have to stop decrypting services in "changes" mode.
    /// We don't send "not selected" commands for "list" or "only" mode because this can disrupt the other
    /// services that still need to be decrypted. We also don't send "keep decrypting" commands (alternative
    /// to "not selected") because that will almost certainly cause glitches in streams.
    /// </remarks>
    /// <param name="subChannelId">The ID of the sub-channel causing this update.</param>
    /// <param name="updateAction"><c>Add</c> if the sub-channel is being tuned, <c>update</c> if the PMT for the
    ///   sub-channel has changed, or <c>last</c> if the sub-channel is being disposed.</param>
    private void UpdateDecryptList(int subChannelId, CaPmtListManagementAction updateAction)
    {
      if (!_useConditionalAccessInterface)
      {
        this.LogWarn("Mpeg2TunerController: CA disabled");
        return;
      }
      if (_caProviders.Count == 0)
      {
        this.LogWarn("Mpeg2TunerController: no CA providers identified");
        return;
      }
      this.LogDebug("Mpeg2TunerController: sub-channel {0} update decrypt list, mode = {1}, update action = {2}", subChannelId, _multiChannelDecryptMode, updateAction);

      if (_mapSubChannels == null || _mapSubChannels.Count == 0 || !_mapSubChannels.ContainsKey(subChannelId))
      {
        this.LogDebug("Mpeg2TunerController: sub-channel not found");
        return;
      }
      if (!_mapSubChannels[subChannelId].CurrentChannel.IsEncrypted)
      {
        this.LogDebug("Mpeg2TunerController: service is not encrypted");
        return;
      }
      if (updateAction == CaPmtListManagementAction.Last && _multiChannelDecryptMode != MultiChannelDecryptMode.Changes)
      {
        this.LogDebug("Mpeg2TunerController: \"not selected\" command acknowledged, no action required");
        return;
      }

      // First build a distinct list of the services that we need to handle.
      this.LogDebug("Mpeg2TunerController: assembling service list");
      List<ISubChannel> distinctServices = new List<ISubChannel>();
      Dictionary<int, ISubChannelInternal>.ValueCollection.Enumerator en = _mapSubChannels.Values.GetEnumerator();
      ChannelMpeg2Base updatedMpeg2Service = _mapSubChannels[subChannelId].CurrentChannel as ChannelMpeg2Base;
      ChannelAnalogTv updatedAnalogTvService = _mapSubChannels[subChannelId].CurrentChannel as ChannelAnalogTv;
      ChannelFmRadio updatedFmRadioService = _mapSubChannels[subChannelId].CurrentChannel as ChannelFmRadio;
      while (en.MoveNext())
      {
        IChannel service = en.Current.CurrentChannel;
        // We don't care about FTA services here.
        if (!service.IsEncrypted)
        {
          continue;
        }

        // Keep an eye out - if there is another sub-channel accessing the same service as the sub-channel that
        // is being updated then we always do *nothing* unless this is specifically an update request. In any other
        // situation, if we were to stop decrypting the service it would be wrong; if we were to start decrypting
        // the service it would be unnecessary and possibly cause stream interruptions.
        if (en.Current.SubChannelId != subChannelId && updateAction != CaPmtListManagementAction.Update)
        {
          if (updatedMpeg2Service != null)
          {
            ChannelMpeg2Base mpeg2Service = service as ChannelMpeg2Base;
            if (mpeg2Service != null && mpeg2Service.ProgramNumber == updatedMpeg2Service.ProgramNumber)
            {
              this.LogDebug("Mpeg2TunerController: the service for this sub-channel is a duplicate, no action required");
              return;
            }
          }
          else if (updatedAnalogTvService != null)
          {
            ChannelAnalogTv analogTvService = service as ChannelAnalogTv;
            if (analogTvService != null && analogTvService.PhysicalChannelNumber == updatedAnalogTvService.PhysicalChannelNumber)
            {
              this.LogDebug("Mpeg2TunerController: the service for this sub-channel is a duplicate, no action required");
              return;
            }
          }
          else if (updatedFmRadioService != null)
          {
            ChannelFmRadio fmRadioService = service as ChannelFmRadio;
            if (fmRadioService != null && fmRadioService.Frequency == updatedFmRadioService.Frequency)
            {
              this.LogDebug("Mpeg2TunerController: the service for this sub-channel is a duplicate, no action required");
              return;
            }
          }
          else
          {
            throw new TvException("Mpeg2TunerController: service type not recognised, unable to assemble decrypt service list\r\n" + service.ToString());
          }
        }

        if (_multiChannelDecryptMode == MultiChannelDecryptMode.List)
        {
          // Check for "list" mode: have we already go this service in our distinct list? If so, don't add it
          // again...
          bool exists = false;
          foreach (ISubChannel serviceToDecrypt in distinctServices)
          {
            ChannelMpeg2Base mpeg2Service = service as ChannelMpeg2Base;
            if (mpeg2Service != null)
            {
              if (mpeg2Service.ProgramNumber == ((ChannelMpeg2Base)serviceToDecrypt.CurrentChannel).ProgramNumber)
              {
                exists = true;
                break;
              }
            }
            else
            {
              ChannelAnalogTv analogTvService = service as ChannelAnalogTv;
              if (analogTvService != null)
              {
                if (analogTvService.PhysicalChannelNumber == ((ChannelAnalogTv)serviceToDecrypt.CurrentChannel).PhysicalChannelNumber)
                {
                  exists = true;
                  break;
                }
              }
              else
              {
                ChannelFmRadio fmRadioService = service as ChannelFmRadio;
                if (fmRadioService != null)
                {
                  if (fmRadioService.Frequency == ((ChannelFmRadio)serviceToDecrypt.CurrentChannel).Frequency)
                  {
                    exists = true;
                    break;
                  }
                }
                else
                {
                  throw new TvException("Mpeg2TunerController: service type not recognised, unable to assemble decrypt service list\r\n" + service.ToString());
                }
              }
            }
          }
          if (!exists)
          {
            distinctServices.Add(en.Current);
          }
        }
        else if (en.Current.SubChannelId == subChannelId)
        {
          // For "changes" and "only" modes: we only send one command and that relates to the service being updated.
          distinctServices.Add(en.Current);
        }
      }

      if (distinctServices.Count == 0)
      {
        this.LogDebug("Mpeg2TunerController: no services to update");
        return;
      }

      // Send the service list or changes to the CA providers.
      for (int attempt = 1; attempt <= _decryptFailureRetryCount + 1; attempt++)
      {
        ThrowExceptionIfTuneCancelled();
        if (attempt > 1)
        {
          this.LogDebug("Mpeg2TunerController: attempt {0}...", attempt);
        }

        foreach (IConditionalAccessProvider caProvider in _caProviders)
        {
          this.LogDebug("Mpeg2TunerController: CA provider {0}...", caProvider.Name);

          if (_waitUntilCaInterfaceReady && !caProvider.IsReady())
          {
            this.LogDebug("Mpeg2TunerController: provider is not ready, waiting for up to 15 seconds", caProvider.Name);
            DateTime startWait = DateTime.Now;
            TimeSpan waitTime = new TimeSpan(0);
            while (waitTime.TotalMilliseconds < 15000)
            {
              ThrowExceptionIfTuneCancelled();
              System.Threading.Thread.Sleep(200);
              waitTime = DateTime.Now - startWait;
              if (caProvider.IsReady())
              {
                this.LogDebug("Mpeg2TunerController: provider ready after {0} ms", waitTime.TotalMilliseconds);
                break;
              }
            }
          }

          // Ready or not, we send commands now.
          this.LogDebug("Mpeg2TunerController: sending command(s)");
          bool success = true;
          SubChannelMpeg2Ts digitalService;
          // The default action is "more" - this will be changed below if necessary.
          CaPmtListManagementAction action = CaPmtListManagementAction.More;

          // The command is "start/continue descrambling" unless we're removing services.
          CaPmtCommand command = CaPmtCommand.OkDescrambling;
          if (updateAction == CaPmtListManagementAction.Last)
          {
            command = CaPmtCommand.NotSelected;
          }
          for (int i = 0; i < distinctServices.Count; i++)
          {
            ThrowExceptionIfTuneCancelled();
            if (i == 0)
            {
              if (distinctServices.Count == 1)
              {
                if (_multiChannelDecryptMode == MultiChannelDecryptMode.Changes)
                {
                  // Remove a service...
                  if (updateAction == CaPmtListManagementAction.Last)
                  {
                    action = CaPmtListManagementAction.Only;
                  }
                  // Add or update a service...
                  else
                  {
                    action = updateAction;
                  }
                }
                else
                {
                  action = CaPmtListManagementAction.Only;
                }
              }
              else
              {
                action = CaPmtListManagementAction.First;
              }
            }
            else if (i == distinctServices.Count - 1)
            {
              action = CaPmtListManagementAction.Last;
            }
            else
            {
              action = CaPmtListManagementAction.More;
            }

            this.LogDebug("  command = {0}, action = {1}, service = {2}", command, action, distinctServices[i].CurrentChannel.Name);
            digitalService = distinctServices[i] as SubChannelMpeg2Ts;
            if (digitalService == null)
            {
              success &= caProvider.SendCommand(action, command, null, null, distinctServices[i].CurrentChannel.Provider);
            }
            else
            {
              // TODO need to PatchPmtForCam() sometime before now, and in such a way that the patched PMT is not propagated to TsWriter etc.
              success &= caProvider.SendCommand(action, command, digitalService.ProgramMapTable, digitalService.ConditionalAccessTable, distinctServices[i].CurrentChannel.Provider);
            }
          }

          // Are we done?
          if (success)
          {
            return;
          }
        }
      }
    }

    #endregion
  }
}
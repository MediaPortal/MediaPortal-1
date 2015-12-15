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
using System.Collections.ObjectModel;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// A base class for all <see cref="ITuner"/> implementations, independent of
  /// tuner interface and stream format.
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
    /// <param name="subChannelId">The new sub-channel's identifier.</param>
    private void FireNewSubChannelEvent(int subChannelId)
    {
      if (_newSubChannelEventDelegate != null)
      {
        _newSubChannelEventDelegate(subChannelId);
      }
    }

    #endregion

    #region variables

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
    /// The broadcast standards supported by the tuner.
    /// </summary>
    private BroadcastStandard _supportedBroadcastStandards = BroadcastStandard.Unknown;

    /// <summary>
    /// The broadcast standards supported by the tuner code/class/type implementation.
    /// </summary>
    private readonly BroadcastStandard _possibleBroadcastStandards = BroadcastStandard.Unknown;

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
    private int _priority = 1;
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
    /// Enable or disable the use of conditional access interface(s).
    /// </summary>
    private bool _useConditionalAccessInterface = true;

    /// <summary>
    /// Have we attempted to open the conditional access interface(s)?
    /// </summary>
    private bool _areConditionalAccessInterfacesOpened = false;

    /// <summary>
    /// The tuner's conditional access interface(s) can decrypt channels from
    /// all of these providers.
    /// </summary>
    /// <remarks>
    /// If this collection is empty it is assumed that all channels can be
    /// decrypted.
    /// </remarks>
    private IList<string> _conditionalAccessProviders = new List<string>(20);

    /// <summary>
    /// The type of conditional access module available to the conditional
    /// access interface(s).
    /// </summary>
    /// <remarks>
    /// Certain conditional access modules require specific handling to ensure
    /// compatibility.
    /// </remarks>
    private CamType _camType = CamType.Default;

    /// <summary>
    /// The number of channels that the conditional access interface(s) can
    /// decrypt simultaneously.
    /// </summary>
    private int _decryptLimit = 1;

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
    /// Should the current tuning process be aborted immediately?
    /// </summary>
    private volatile bool _cancelTune = false;

    /// <summary>
    /// The current state of the tuner.
    /// </summary>
    private TunerState _state = TunerState.NotLoaded;

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
    /// The maximum length of time to wait for signal lock/detection after tuning.
    /// </summary>
    private int _timeLimitSignalLock = 2500;    // unit = milli-seconds

    #endregion

    #region constructor & finaliser

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerBase"/> class.
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
      _possibleBroadcastStandards = supportedBroadcastStandards;
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
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
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
        return _possibleBroadcastStandards;
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
        return new ReadOnlyCollection<string>(_conditionalAccessProviders);
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
            IConditionalAccessProvider caProviderInterface = extension as IConditionalAccessProvider;
            if (caProviderInterface != null && !caProviderInterface.IsOpen)
            {
              continue;
            }
            return caMenuInterface;
          }
        }
        return null;
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
        if (SubChannelManager != null && SubChannelManager.SubChannelCount > 0)
        {
          return _currentTuningDetail;
        }
        return null;
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
    /// Get the tuner's priority.
    /// </summary>
    public int Priority
    {
      get
      {
        return _priority;
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
    /// Get the tuner's sub-channel manager.
    /// </summary>
    public abstract ISubChannelManager SubChannelManager
    {
      get;
    }

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
          _priority = config.Priority;
          _idleMode = (TunerIdleMode)config.IdleMode;
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
          if (
            !_useConditionalAccessInterface &&
            config.UseConditionalAccess &&
            _state != TunerState.NotLoaded &&
            _state != TunerState.Loading &&
            !_areConditionalAccessInterfacesOpened
          )
          {
            this.LogDebug("tuner base: conditional access enabled, opening provider(s)");
            foreach (ITunerExtension extension in _extensions)
            {
              IConditionalAccessProvider caProvider = extension as IConditionalAccessProvider;
              if (caProvider != null)
              {
                this.LogDebug("tuner base: found conditional access provider \"{0}\"", extension.Name);
                if (!caProvider.Open())
                {
                  this.LogDebug("tuner base: provider will not be used");
                }
              }
            }
            _areConditionalAccessInterfacesOpened = true;
          }
          _useConditionalAccessInterface = config.UseConditionalAccess;
          _conditionalAccessProviders.Clear();
          foreach (string provider in config.ConditionalAccessProviders.Split(','))
          {
            _conditionalAccessProviders.Add(provider.Trim());
          }
          _camType = (CamType)config.CamType;
          _decryptLimit = config.DecryptLimit;

          if (_state == TunerState.NotLoaded && config.Preload)
          {
            Load();
          }
        }
      }

      _timeLimitSignalLock = SettingsManagement.GetValue("timeLimitSignalLock", 2500);

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
        _extensions = PerformLoading(StreamFormat.Default);

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

        SubChannelManager.SetExtensions(_extensions);
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
    /// <param name="streamFormat">The format(s) of the streams that the tuner is expected to support.</param>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public abstract IList<ITunerExtension> PerformLoading(StreamFormat streamFormat = StreamFormat.Default);

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

      foreach (ITunerExtension extension in _extensions)
      {
        if (_useConditionalAccessInterface)
        {
          IConditionalAccessProvider caProvider = extension as IConditionalAccessProvider;
          if (caProvider != null)
          {
            this.LogDebug("tuner base: found conditional access provider \"{0}\"", extension.Name);
            if (!caProvider.Open())
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

      _areConditionalAccessInterfacesOpened = _useConditionalAccessInterface;
    }

    /// <summary>
    /// Unload the tuner.
    /// </summary>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    private void Unload(bool isFinalising = false)
    {
      this.LogDebug("tuner base: unload tuner");
      if (!isFinalising && SubChannelManager != null)
      {
        SubChannelManager.Decompose();
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
        foreach (ITunerExtension extension in _extensions)
        {
          // Avoid recursive loop for ITuner implementations that also implement ITunerExtension.
          if (!(extension is ITuner))
          {
            IDisposable d = extension as IDisposable;
            if (d != null)
            {
              d.Dispose();
            }
          }
        }
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
    private void Stop()
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
            _diseqcController.Tune(null);
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
        _isEnabled &&
        (
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
      )
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Tune to a specific channel.
    /// </summary>
    /// <param name="subChannelId">The identifier of the sub-channel associated with the channel that is being tuned.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the sub-channel associated with the tuned channel</returns>
    public virtual ISubChannel Tune(int subChannelId, IChannel channel)
    {
      if (!IsEnabled)
      {
        return null;
      }
      this.LogDebug("tuner base: tune channel, {0}", channel);
      _cancelTune = false;
      try
      {
        // The tuner must be loaded before a channel can be tuned.
        if (_state == TunerState.NotLoaded)
        {
          Load();
          ThrowExceptionIfTuneCancelled();
        }

        // Do we need to tune?
        if (_currentTuningDetail == null || _currentTuningDetail.IsDifferentTransmitter(channel))
        {
          SubChannelManager.OnBeforeTune();

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
            ThrowExceptionIfTuneCancelled();
          }

          // Send DiSEqC commands (if necessary) before actually tuning in case the driver applies the commands
          // during the tuning process.
          if (_diseqcController != null)
          {
            _diseqcController.Tune(channel as IChannelSatellite);
            ThrowExceptionIfTuneCancelled();
          }

          // Apply tuning parameters.
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

        _currentTuningDetail = channel;

        // Start the tuner.
        PerformTunerAction(TunerAction.Start);
        ThrowExceptionIfTuneCancelled();

        // Extension OnStarted().
        foreach (ITunerExtension extension in _extensions)
        {
          extension.OnStarted(this, channel);
        }

        LockInOnSignal();
        this.LogDebug("tuner base: signal, locked = {0}, present = {1}, strength = {2} %, quality = {3} %", _isSignalLocked, _isSignalPresent, _signalStrength, _signalQuality);

        bool isNewSubChannel;
        ISubChannel subChannel = SubChannelManager.Tune(subChannelId, channel, out isNewSubChannel);
        if (isNewSubChannel)
        {
          FireNewSubChannelEvent(subChannel.SubChannelId);
        }
        return subChannel;
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
        throw;
      }
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
      while (ts.TotalMilliseconds < _timeLimitSignalLock)
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
    /// <param name="subChannelId">The identifier of the sub-channel associated with the tuning process that is being cancelled.</param>
    public void CancelTune(int subChannelId)
    {
      this.LogDebug("tuner base: cancel tune, sub-channel ID = {0}", subChannelId);
      _cancelTune = true;
      if (_diseqcController != null)
      {
        _diseqcController.CancelTune();
      }
      if (SubChannelManager != null)
      {
        SubChannelManager.CancelTune(subChannelId);
      }
    }

    /// <summary>
    /// Check if the current tuning process has been cancelled and throw an
    /// exception if it has.
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
    /// Can the tuner receive all sub-channels from the current transmitter simultaneously?
    /// </summary>
    public bool CanSimultaneouslyReceiveTransmitterSubChannels
    {
      get
      {
        if (SubChannelManager == null)
        {
          return true;
        }
        return SubChannelManager.CanSimultaneouslyReceiveTransmitterSubChannels;
      }
    }

    /// <summary>
    /// Free a sub-channel.
    /// </summary>
    /// <param name="id">The sub-channel's identifier.</param>
    public void FreeSubChannel(int id)
    {
      if (SubChannelManager != null)
      {
        try
        {
          SubChannelManager.FreeSubChannel(id);
          if (SubChannelManager.SubChannelCount == 0)
          {
            this.LogDebug("tuner base: no sub-channels present, stopping tuner");
            Stop();
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex);
        }
      }
    }

    /// <summary>
    /// Get a sub-channel.
    /// </summary>
    /// <param name="id">The sub-channel's identifier.</param>
    /// <returns>the sub-channel if it exists, otherwise <c>null</c></returns>
    public ISubChannel GetSubChannel(int id)
    {
      if (SubChannelManager == null)
      {
        return null;
      }
      return SubChannelManager.GetSubChannel(id);
    }

    /// <summary>
    /// Get the count of sub-channels.
    /// </summary>
    public int SubChannelCount
    {
      get
      {
        if (SubChannelManager == null)
        {
          return 0;
        }
        return SubChannelManager.SubChannelCount;
      }
    }

    /// <summary>
    /// Get the set of sub-channel identifiers for each channel the tuner is currently decrypting.
    /// </summary>
    /// <returns>a collection of sub-channel identifier lists</returns>
    public ICollection<IList<int>> GetDecryptedSubChannelDetails()
    {
      if (SubChannelManager == null)
      {
        return new List<IList<int>>(0);
      }
      return SubChannelManager.GetDecryptedSubChannelDetails();
    }

    /// <summary>
    /// Determine whether a sub-channel is being decrypted.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the sub-channel is being decrypted, otherwise <c>false</c></returns>
    public bool IsDecrypting(IChannel channel)
    {
      if (SubChannelManager == null || SubChannelManager.SubChannelCount == 0)
      {
        return false;
      }
      return SubChannelManager.IsDecrypting(channel);
    }

    #endregion
  }
}
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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Config;
using Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Enum;
using Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Service;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster
{
  public class MicrosoftBlaster : BaseTunerExtension, IDisposable, IPowerDevice, ITvServerPlugin, ITvServerPluginCommunication
  {
    #region variables

    private static bool _isPluginEnabled = false;
    private static MicrosoftBlasterConfigService _service = new MicrosoftBlasterConfigService();

    private bool _isMicrosoftBlaster = false;
    private string _tunerExternalId = null;
    private object _configLock = new object();
    private SetTopBoxConfig _config = null;

    #endregion

    private void OnBlasterConfigChange()
    {
      lock (_configLock)
      {
        SetTopBoxConfig config = _service.GetSetTopBoxConfigurationForTuner(_tunerExternalId);
        if (
          _config == null ||
          !string.Equals(_config.TransceiverDevicePath, config.TransceiverDevicePath) ||
          _config.TransmitPort != config.TransmitPort ||
          !string.Equals(_config.ProfileName, config.ProfileName) ||
          _config.IsPowerControlEnabled != config.IsPowerControlEnabled
        )
        {
          this.LogDebug("Microsoft blaster: config change");
          this.LogDebug("  tuner external ID = {0}", _tunerExternalId);
          this.LogDebug("  transceiver       = {0}", _config.TransceiverDevicePath);
          this.LogDebug("  transmit port     = {0}", _config.TransmitPort);
          this.LogDebug("  STB profile       = {0}", _config.ProfileName);
          this.LogDebug("  power control?    = {0}", _config.IsPowerControlEnabled);
        }
      }
    }

    #region ITunerExtension members

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Microsoft blaster";
      }
    }

    /// <summary>
    /// Does the extension control some part of the tuner hardware?
    /// </summary>
    public override bool ControlsTunerHardware
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context)
    {
      this.LogDebug("Microsoft blaster: initialising");

      if (_isMicrosoftBlaster)
      {
        this.LogWarn("Microsoft blaster: extension already initialised");
        return true;
      }
      if (string.IsNullOrEmpty(tunerExternalId))
      {
        this.LogDebug("Microsoft blaster: tuner external identifier is not set");
        return false;
      }
      if (!tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.AnalogTelevision) && !tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.ExternalInput))
      {
        this.LogDebug("Microsoft blaster: tuner type not supported");
        return false;
      }

      this.LogInfo("Microsoft blaster: extension supported");
      _isMicrosoftBlaster = true;
      _tunerExternalId = tunerExternalId;
      _config = _service.GetSetTopBoxConfigurationForTuner(tunerExternalId);
      this.LogDebug("  transceiver    = {0}", _config.TransceiverDevicePath);
      this.LogDebug("  transmit port  = {0}", _config.TransmitPort);
      this.LogDebug("  STB profile    = {0}", _config.ProfileName);
      this.LogDebug("  power control? = {0}", _config.IsPowerControlEnabled);
      _service.OnConfigChange += OnBlasterConfigChange;
      return true;
    }

    #region tuner state change call backs

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITuner tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      this.LogDebug("Microsoft blaster: on before tune call back");
      action = TunerAction.Default;

      if (!_isMicrosoftBlaster)
      {
        this.LogWarn("Microsoft blaster: not initialised or interface not supported");
        return;
      }
      if (!_isPluginEnabled)
      {
        return;
      }
      if (!(channel is ChannelAnalogTv) && !(channel is ChannelCapture))
      {
        this.LogDebug("Microsoft blaster: not tuning a capture channel");
        return;
      }

      lock (_configLock)
      {
        if (string.IsNullOrEmpty(_config.TransceiverDevicePath) || _config.TransmitPort == TransmitPort.None)
        {
          this.LogDebug("Microsoft blaster: transceiver not configured for tuner");
          return;
        }
        _service.Transmit(_config.TransceiverDevicePath, _config.TransmitPort, _config.ProfileName, channel.LogicalChannelNumber);
      }
    }

    #endregion

    #endregion

    #region IPowerDevice member

    /// <summary>
    /// Set the tuner power state.
    /// </summary>
    /// <param name="state">The power state to apply.</param>
    /// <returns><c>true</c> if the power state is set successfully, otherwise <c>false</c></returns>
    public bool SetPowerState(PowerState state)
    {
      this.LogDebug("Microsoft blaster: set power state, state = {0}", state);

      if (!_isMicrosoftBlaster)
      {
        this.LogWarn("Microsoft blaster: not initialised or interface not supported");
        return false;
      }
      if (!_isPluginEnabled)
      {
        return true;
      }

      lock (_configLock)
      {
        if (string.IsNullOrEmpty(_config.TransceiverDevicePath) || _config.TransmitPort == TransmitPort.None)
        {
          this.LogDebug("Microsoft blaster: transceiver not configured for tuner");
          return true;
        }
        if (!_config.IsPowerControlEnabled)
        {
          this.LogDebug("Microsoft blaster: power control disabled");
          return true;
        }

        string channelNumber = MicrosoftBlasterConfigService.CHANNEL_NUMBER_POWER_OFF;
        if (state == PowerState.On)
        {
          channelNumber = MicrosoftBlasterConfigService.CHANNEL_NUMBER_POWER_ON;
        }
        TransmitResult result = _service.Transmit(_config.TransceiverDevicePath, _config.TransmitPort, _config.ProfileName, channelNumber);
        if (result != TransmitResult.Success)
        {
          this.LogError("Microsoft blaster: failed to set power state, result = {0}", result);
          return false;
        }

        this.LogDebug("Microsoft blaster: result = success");
        return true;
      }
    }

    #endregion

    #region ITvServerPlugin members

    /// <summary>
    /// The version of this TV Server plugin.
    /// </summary>
    public string Version
    {
      get
      {
        return "1.0.0.0";
      }
    }

    /// <summary>
    /// The author of this TV Server plugin.
    /// </summary>
    public string Author
    {
      get
      {
        return "mm1352000";
      }
    }

    /// <summary>
    /// Get an instance of the configuration section for use in TV Server configuration (SetupTv).
    /// </summary>
    public SectionSettings Setup
    {
      get
      {
        return new MicrosoftBlasterConfig();
      }
    }

    /// <summary>
    /// Start this TV Server plugin.
    /// </summary>
    public void Start(IInternalControllerService controllerService)
    {
      this.LogDebug("Microsoft blaster: plugin enabled");
      _isPluginEnabled = true;
      _service.Start();
    }

    /// <summary>
    /// Stop this TV Server plugin.
    /// </summary>
    public void Stop()
    {
      this.LogDebug("Microsoft blaster: plugin disabled");
      _isPluginEnabled = false;
      _service.Stop();
    }

    #endregion

    #region ITvServerPluginCommunication members

    /// <summary>
    /// Supply a service class implementation for client-server plugin communication.
    /// </summary>
    public object GetServiceInstance
    {
      get
      {
        return _service;
      }
    }

    /// <summary>
    /// Supply a service class interface for client-server plugin communication.
    /// </summary>
    public Type GetServiceInterfaceForContractType
    {
      get
      {
        return typeof(IMicrosoftBlasterConfigService);
      }
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

    ~MicrosoftBlaster()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing && _isMicrosoftBlaster)
      {
        _service.OnConfigChange -= OnBlasterConfigChange;
        _isMicrosoftBlaster = false;
      }
    }

    #endregion
  }
}
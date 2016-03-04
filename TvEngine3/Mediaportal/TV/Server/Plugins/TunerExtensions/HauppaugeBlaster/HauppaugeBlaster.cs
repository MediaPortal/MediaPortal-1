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
using Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeBlaster.Config;
using Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeBlaster.Service;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeBlaster
{
  /// <summary>
  /// A class that implements support for the blaster included with various
  /// Hauppauge products.
  /// </summary>
  /// <remarks>
  /// Hauppauge's IRBlast application must be installed. IRBlast installs
  /// hcwIRblast.dll - the DLL that this class depends on - into the Windows
  /// system32 directory. It also installs BlastCfg, which is used for blaster
  /// configuration and key code learning.
  /// 
  /// Due to limitations in the Hauppauge drivers and software, this
  /// implementation is only capable of controlling the blaster(s) on a single
  /// card. That card is selected automatically by hcwIRblast.dll. We have
  /// little or no control over the selection process.
  /// 
  /// Selection seems to depend on card installation order and maybe also which
  /// PCI/PCIe slot they're seated in.
  /// 
  /// There are 2 registry keys that may force hcwIRblast.dll to open a blaster
  /// on a particular family of cards:
  /// HKEY_LOCAL_MACHINE\SOFTWARE\Hauppauge\IR\IRBlasterPort
  /// HKEY_LOCAL_MACHINE\SOFTWARE\Hauppauge\IR\BlasterI2CType
  /// 
  /// These are undocumented and subject to change. Modify at your own risk!!!
  /// </remarks>
  public class HauppaugeBlaster : BaseTunerExtension, IDisposable, ITvServerPlugin, ITvServerPluginCommunication
  {
    #region variables

    private static bool _isPluginEnabled = false;
    private static HauppaugeBlasterConfigService _service = new HauppaugeBlasterConfigService();

    private bool _isHauppaugeBlaster = false;
    private string _tunerExternalId = null;
    private object _configLock = new object();
    private int _blasterPort = 0;

    #endregion

    private void OnBlasterConfigChange(string tunerExternalIdPort1, string tunerExternalIdPort2)
    {
      lock (_configLock)
      {
        if (string.Equals(_tunerExternalId, tunerExternalIdPort1))
        {
          this.LogDebug("Hauppauge blaster: config change, port = 1, tuner external ID = {0}", _tunerExternalId);
          _blasterPort = 1;
        }
        else if (string.Equals(_tunerExternalId, tunerExternalIdPort2))
        {
          this.LogDebug("Hauppauge blaster: config change, port = 2, tuner external ID = {0}", _tunerExternalId);
          _blasterPort = 2;
        }
        else if (_blasterPort != 0)
        {
          this.LogDebug("Hauppauge blaster: config change, port = [none], tuner external ID = {0}", _tunerExternalId);
          _blasterPort = 0;
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
        return "Hauppauge blaster";
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
      this.LogDebug("Hauppauge blaster: initialising");

      if (_isHauppaugeBlaster)
      {
        this.LogWarn("Hauppauge blaster: extension already initialised");
        return true;
      }
      if (string.IsNullOrEmpty(tunerExternalId))
      {
        this.LogDebug("Hauppauge blaster: tuner external identifier is not set");
        return false;
      }
      if (!tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.AnalogTelevision) && !tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.ExternalInput))
      {
        this.LogDebug("Hauppauge blaster: tuner type not supported");
        return false;
      }

      this.LogInfo("Hauppauge blaster: extension supported");
      _isHauppaugeBlaster = true;
      _tunerExternalId = tunerExternalId;
      string tunerExternalIdPort1;
      string tunerExternalIdPort2;
      _service.GetBlasterTunerExternalIds(out tunerExternalIdPort1, out tunerExternalIdPort2);
      if (string.Equals(_tunerExternalId, tunerExternalIdPort1))
      {
        this.LogDebug("Hauppauge blaster: port = 1");
        _blasterPort = 1;
      }
      else if (string.Equals(_tunerExternalId, tunerExternalIdPort2))
      {
        this.LogDebug("Hauppauge blaster: port = 2");
        _blasterPort = 2;
      }
      else
      {
        this.LogDebug("Hauppauge blaster: port = [none]");
        _blasterPort = 0;
      }
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
      this.LogDebug("Hauppauge blaster: on before tune call back");
      action = TunerAction.Default;

      if (!_isHauppaugeBlaster)
      {
        this.LogWarn("Hauppauge blaster: not initialised or interface not supported");
        return;
      }
      if (!_isPluginEnabled)
      {
        return;
      }
      if (!(channel is ChannelAnalogTv) && !(channel is ChannelCapture))
      {
        this.LogDebug("Hauppauge blaster: not tuning a capture channel");
        return;
      }

      lock (_configLock)
      {
        if (_blasterPort == 0)
        {
          this.LogDebug("Hauppauge blaster: not blaster tuner");
          return;
        }
        _service.BlastChannelNumber(channel.LogicalChannelNumber, _blasterPort);
      }
    }

    #endregion

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
        return new HauppaugeBlasterConfig();
      }
    }

    /// <summary>
    /// Start this TV Server plugin.
    /// </summary>
    public void Start(IInternalControllerService controllerService)
    {
      this.LogDebug("Hauppauge blaster: plugin enabled");
      _isPluginEnabled = true;
      _service.OpenBlaster();
    }

    /// <summary>
    /// Stop this TV Server plugin.
    /// </summary>
    public void Stop()
    {
      this.LogDebug("Hauppauge blaster: plugin disabled");
      _isPluginEnabled = false;
      _service.CloseBlaster();
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
        return typeof(IHauppaugeBlasterConfigService);
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

    ~HauppaugeBlaster()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing && _isHauppaugeBlaster)
      {
        _service.OnConfigChange -= OnBlasterConfigChange;
        _isHauppaugeBlaster = false;
      }
    }

    #endregion
  }
}
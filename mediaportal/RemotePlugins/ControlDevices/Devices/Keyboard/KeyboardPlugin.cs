using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.ControlDevices.Keyboard
{
  public class KeyboardPlugin : AbstractControlPlugin, IControlPlugin
  {
    public string DeviceName { get { return "Keyboard"; } }
    public Uri VendorUri { get { return null; } }
    public IControlInput InputInterface { get { return null; } }
    public IControlOutput OutputInterface { get { return null; } }
    public bool DriverInstalled { get { return true; } }
    public string DriverVersion { get { return null; } }
    public string DeviceDescription { get { return "lalala"; } }
    public string DevicePrefix { get { return _settings.Prefix; } }
    public bool HardwareInstalled { get { return true; } }
    public string HardwareVersion { get { return null; } }
    public bool Capability(EControlCapabilities capability)
    {
      switch (capability)
      {
        case EControlCapabilities.CAP_INPUT:
          return true;
        case EControlCapabilities.CAP_OUTPUT:
          return false;
        case EControlCapabilities.CAP_VERBOSELOG:
          return false;
        default:
          return false;
      }
    }
    public IControlSettings Settings { get { return _settings; } }

    public KeyboardPlugin()
    {
      _settings = new KeyboardSettings(this);
    }

  }
}

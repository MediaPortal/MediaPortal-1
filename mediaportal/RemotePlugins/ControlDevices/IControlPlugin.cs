using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.ControlDevices
{
  public enum EControlCapabilities
  {
    CAP_INPUT = 1,
    CAP_OUTPUT = 2,
    CAP_VERBOSELOG = 3,
    CAP_INPUTMAPPING = 4,
    CAP_SETUP_PANEL = 101,
    CAP_SETUP_ADVANCED = 102,
    CAP_SETUP_DEFAULT = 103
  }

  public enum ControlPluginType
  {
    CPT_UNKNOWN = 0,
    CPT_REMOTE = 1,
    CPT_KEYBOARD = 2,
    CPT_NETWORK = 3
  }


  public interface IControlPlugin
  {
    string DeviceName { get; }

    string DeviceDescription { get; }

    string DevicePrefix { get; }

    Uri VendorUri { get; }

    bool DriverInstalled { get; }

    string DriverVersion { get; }

    bool HardwareInstalled { get; }

    string HardwareVersion { get; }

    bool Capability(EControlCapabilities capability);

    IControlInput InputInterface { get; }

    IControlOutput OutputInterface { get; }

    IControlSettings Settings { get; }

    

  }
}

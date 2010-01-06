#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
    string LibraryName { set; get; }

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

    void Initialize();
  }
}
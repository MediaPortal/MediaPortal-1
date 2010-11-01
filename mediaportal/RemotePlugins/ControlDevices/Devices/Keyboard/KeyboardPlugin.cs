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

namespace MediaPortal.ControlDevices.Keyboard
{
  public class KeyboardPlugin : AbstractControlPlugin, IControlPlugin
  {
    public string DeviceName
    {
      get { return "Keyboard"; }
    }

    public Uri VendorUri
    {
      get { return null; }
    }

    public IControlInput InputInterface
    {
      get { return null; }
    }

    public IControlOutput OutputInterface
    {
      get { return null; }
    }

    public bool DriverInstalled
    {
      get { return true; }
    }

    public string DriverVersion
    {
      get { return null; }
    }

    public string DeviceDescription
    {
      get { return "lalala"; }
    }

    public string DevicePrefix
    {
      get { return _settings.Prefix; }
    }

    public bool HardwareInstalled
    {
      get { return true; }
    }

    public string HardwareVersion
    {
      get { return null; }
    }

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

    public IControlSettings Settings
    {
      get { return _settings; }
    }

    public KeyboardPlugin()
    {
      _settings = new KeyboardSettings(this);
    }
  }
}
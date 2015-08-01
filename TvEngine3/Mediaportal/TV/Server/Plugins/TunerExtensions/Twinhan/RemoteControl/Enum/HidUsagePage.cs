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

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.RemoteControl.Enum
{
  /// <summary>
  /// From USB HID usage tables.
  /// http://www.usb.org/developers/hidpage#HID_Usage
  /// http://www.usb.org/developers/devclass_docs/Hut1_12v2.pdf
  /// </summary>
  internal enum HidUsagePage : ushort
  {
    Undefined = 0,
    GenericDesktopControl,
    SimulationControl,
    VirtualRealityControl,
    SportControl,
    GameControl,
    GenericDeviceControl,
    Keyboard,
    LightEmittingDiode,
    Button,
    Ordinal,
    Telephony,
    Consumer,
    Digitiser,

    PhysicalInterfaceDevice = 0x0f,
    Unicode = 0x10,
    AlphaNumericDisplay = 0x14,
    MedicalInstruments = 0x40,

    MonitorPage0 = 0x80,
    MonitorPage1,
    MonitorPage2,
    MonitorPage3,
    PowerPage0,
    PowerPage1,
    PowerPage2,
    PowerPage3,

    BarCodeScanner = 0x8c,
    Scale,
    MagneticStripeReader,
    ReservedPointOfSale,
    CameraControl,
    Arcade,

    // http://msdn.microsoft.com/en-us/library/windows/desktop/bb417079.aspx
    MceRemote = 0xffbc,
    TerraTecRemote = 0xffcc
  }
}
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

using System.Runtime.Serialization;
using Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt.Enum;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt
{
  /// <summary>
  /// Detail for a USB-UIRT.
  /// </summary>
  [DataContract]
  internal class UsbUirtDetail
  {
    /// <summary>
    /// The USB-UIRT's name.
    /// </summary>
    [DataMember]
    public string Name = string.Empty;

    /// <summary>
    /// The USB-UIRT's index.
    /// </summary>
    [DataMember]
    public int Index = -1;

    /// <summary>
    /// The transmit zones available for the USB-UIRT.
    /// </summary>
    [DataMember]
    public TransmitZone TransmitZones = TransmitZone.All;

    public override string ToString()
    {
      return Name;
    }
  }
}
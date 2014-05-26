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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.B2c2
{
  internal class Constants
  {
    public static readonly Guid B2C2_ADAPTER_CLSID = new Guid(0xe82536a0, 0x94da, 0x11d2, 0xa4, 0x63, 0x00, 0xa0, 0xc9, 0x5d, 0x30, 0x8d);

    public const int MAX_DEVICE_COUNT = 16;
    public const int MAC_ADDRESS_LENGTH = 6;
    public const int MAX_MAC_ADDRESS_COUNT = 32;
  }
}

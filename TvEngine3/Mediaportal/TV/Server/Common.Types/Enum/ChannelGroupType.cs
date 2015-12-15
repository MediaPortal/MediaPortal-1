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

namespace Mediaportal.TV.Server.Common.Types.Enum
{
  [Flags]
  public enum ChannelGroupType
  {
    Manual = 0,

    // automatic
    ChannelProvider = 0x0001,
    DvbNetwork = 0x0002,
    DvbBouquet = 0x0004,
    DvbTargetRegion = 0x0008,
    BroadcastStandard = 0x0010,
    Satellite = 0x0020,
    FreesatChannelCategory = 0x0040,      // UK satellite
    NorDigChannelList = 0x0080,
    VirginMediaChannelCategory = 0x0100,  // UK cable

    // automatic, provider-specific
    OpenTvRegion = 0x10000,               // Foxtel, Sky NZ, Sky UK
    FreesatRegion = 0x20000,              // UK satellite
    DishNetworkMarket = 0x40000,          // US satellite
    FreeviewSatellite = 0x80000           // NZ satellite
  }
}
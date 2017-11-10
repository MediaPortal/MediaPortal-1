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
    CyfrowyPolsatChannelCategory = 0x0040,  // Hotbird 13E (Poland)
    FreesatChannelCategory = 0x0080,        // Astra 28.2E (UK)
    MediaHighwayChannelCategory = 0x0100,   // Hotbird 13E (Spain)
    NorDigChannelList = 0x0200,
    OpenTvChannelCategory = 0x0400,         // Foxtel, Sky IT, Sky NZ, Sky UK
    VirginMediaChannelCategory = 0x0800,    // UK cable

    // automatic, provider-specific
    DishNetworkMarket = 0x001000,           // US satellite
    FreesatRegion = 0x002000,               // Astra 28.2E (UK)
    FreeviewSatellite = 0x004000,           // Optus D1 160E (NZ)
    OpenTvRegionFoxtel = 0x008000,          // Foxtel
    OpenTvRegionSkyNz = 0x010000,           // Sky NZ
    OpenTvRegionSkyUk = 0x020000,           // Sky UK
    OpenTvRegionOther = 0x040000            // other OpenTV providers for which the region ID mapping is not known
  }
}
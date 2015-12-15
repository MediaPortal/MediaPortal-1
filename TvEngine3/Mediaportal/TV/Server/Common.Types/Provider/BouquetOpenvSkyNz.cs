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

namespace Mediaportal.TV.Server.Common.Types.Provider
{
  public enum BouquetOpenTvSkyNz
  {
    // Sky network television - no region IDs in these bouquets.
    NdsCa1DefaultBouquet = 24577,
    SntBouquet0_UpperNorthIsland = 24656,
    SntBouquet1_LowerNorthIsland,
    SntBouquet2_SouthIsland,
    SntBouquet3_SkyMusic,
    SntBouquet4_SkyStaffTest, // 24660
    SntBouquet5_LinkingServices,
    SntBouquet6_MiddleNorthIsland,
    SntBouquet7_Missing,
    SntBouquet8_EineeringOnly,
    SntBouquet9_EngineeringDecoder,
    SntBouquet10_EngineeringFull,
    SntBouquet11_EngineeringPlusInt,
    SntBouquet12_InteractiveTest1,
    SntBouquet13_InteractiveTest2,
    SntBouquet14_InteractiveTest3,
    SntBouquet15_InteractiveDev,
    SntBouquet16_PrimeOnly,   // 24672

    // These bouquets have region IDs.
    Ca2Sd = 25280,
    Ca2SdHd,
    Ca2HdHd,
    Ca2DummyForP2LegacyMiddleNorthIsland = 25286
  }
}
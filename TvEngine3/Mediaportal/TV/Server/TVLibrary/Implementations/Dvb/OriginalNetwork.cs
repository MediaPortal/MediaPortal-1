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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dvb
{
  /// <summary>
  /// DVB original network utility functions.
  /// </summary>
  /// <remarks>
  /// Value is the original network's identifier.
  /// Refer to http://www.dvbservices.com/identifiers/original_network_id&tab=table
  /// </remarks>
  internal static class OriginalNetwork
  {
    private enum OriginalNetworkId
    {
      // UK satellite (28.2E)
      Astra28_2e = 0x0002,
      Bbc = 0x003b,             // Freesat

      // New Zealand & Australia satellite (156E & 160E)
      Tvnz = 0x002f,            // Freeview Satellite
      OptusB3_156e1 = 0x0069,   // Foxtel
      Foxtel = 0x00a8,
      SkyNz = 0x00a9,
      OptusNetworks = 0x0fff,   // VAST AU
      OptusB3_156e2 = 0x1000,   // Foxtel

      // Europe satellite (13E)
      PolsatCyfraPlusNcPlus = 0x0071,
      GroupeCanalPlus = 0x00b0, // Groupe CANAL+
      CanalPlusStart = 0x00c0,
      CanalPlusEnd = 0x00cd,
      Mediaset = 0x0110,
      Eutelsat13e1 = 0x013e,
      Eutelsat13e2 = 0x013f,
      Arabsat = 0x02be,         // Arab Satellite Communications Organization
      SkyItalia = 0xfbff,

      DishNetwork = 0x1001,
      DishNetwork61_5w = 0x1002,
      DishNetwork83w = 0x1003,
      DishNetwork119w = 0x1004,
      DishNetwork121w = 0x1005,
      DishNetwork148w = 0x1006,
      DishNetwork175w = 0x1007,
      DishNetworkW = 0x1008,
      DishNetworkX = 0x1009,
      DishNetworkY = 0x100a,
      DishNetworkZ = 0x100b,
      DishNetworkStart = DishNetwork,
      DishNetworkEnd = DishNetworkZ,

      // Australia DVB-T/T2
      AustralianBroadcastingCorporation = 0x1010,
      SpecialBroadcastingService = 0x1011,
      NineNetwork = 0x1012,
      SevenNetwork = 0x1013,
      NetworkTen = 0x1014,
      WinTv = 0x1015,
      PrimeTv = 0x1016,
      SouthernCrossBroadcasting = 0x1017,
      TelecastersAustralia = 0x1018,
      NbnTelevision = 0x1019,
      ImparjaTelevision = 0x101a,
      ReservedAustralianBroadcaster1 = 0x101b,
      ReservedAustralianBroadcaster2 = 0x101c,
      ReservedAustralianBroadcaster3 = 0x101d,
      ReservedAustralianBroadcaster4 = 0x101e,
      ReservedAustralianBroadcaster5 = 0x101f,
      AustralianBroadcasterStart = AustralianBroadcastingCorporation,
      AustralianBroadcasterEnd = ReservedAustralianBroadcaster5,

      Echostar2a = 0x1700,
      Echostar2b = 0x1701,
      Echostar2c = 0x1702,
      Echostar2d = 0x1703,
      Echostar2e = 0x1704,
      Echostar2f = 0x1705,
      Echostar2g = 0x1706,
      Echostar2h = 0x1707,
      Echostar2i = 0x1708,
      Echostar2j = 0x1709,
      Echostar2k = 0x170a,
      Echostar2l = 0x170b,
      Echostar2m = 0x170c,
      Echostar2n = 0x170d,
      Echostar2o = 0x170e,
      Echostar2p = 0x170f,
      Echostar2q = 0x1710,
      Echostar2r = 0x1711,
      Echostar2s = 0x1712,
      Echostar2t = 0x1713,
      EchostarStart = Echostar2a,
      EchostarEnd = Echostar2t,

      VirginMediaUk = 0xf020    // cable and wireless communication
    }

    public static bool IsDishNetwork(ushort originalNetworkId)
    {
      return originalNetworkId >= (ushort)OriginalNetworkId.DishNetworkStart && originalNetworkId <= (ushort)OriginalNetworkId.DishNetworkEnd;
    }

    public static bool IsEchostar(ushort originalNetworkId)
    {
      return originalNetworkId >= (ushort)OriginalNetworkId.EchostarStart && originalNetworkId <= (ushort)OriginalNetworkId.EchostarEnd;
    }

    public static bool IsOpenTvFoxtel(ushort originalNetworkId)
    {
      if (
        originalNetworkId == (ushort)OriginalNetworkId.OptusNetworks ||
        originalNetworkId == (ushort)OriginalNetworkId.OptusB3_156e1 ||
        originalNetworkId == (ushort)OriginalNetworkId.OptusB3_156e2 ||
        (
          originalNetworkId >= (ushort)OriginalNetworkId.AustralianBroadcasterStart &&
          originalNetworkId <= (ushort)OriginalNetworkId.AustralianBroadcasterEnd
        )
      )
      {
        return true;
      }
      return false;
    }

    public static bool IsOpenTvSkyItalia(ushort originalNetworkId)
    {
      if (
        originalNetworkId == (ushort)OriginalNetworkId.PolsatCyfraPlusNcPlus ||
        originalNetworkId == (ushort)OriginalNetworkId.GroupeCanalPlus ||
        (
          originalNetworkId >= (ushort)OriginalNetworkId.CanalPlusStart &&
          originalNetworkId <= (ushort)OriginalNetworkId.CanalPlusEnd
        ) ||
        originalNetworkId == (ushort)OriginalNetworkId.Mediaset ||
        originalNetworkId == (ushort)OriginalNetworkId.Eutelsat13e1 ||
        originalNetworkId == (ushort)OriginalNetworkId.Eutelsat13e2 ||
        originalNetworkId == (ushort)OriginalNetworkId.Arabsat ||
        originalNetworkId == (ushort)OriginalNetworkId.SkyItalia
      )
      {
        return true;
      }
      return false;
    }

    public static bool IsOpenTvSkyNz(ushort originalNetworkId)
    {
      return originalNetworkId == (ushort)OriginalNetworkId.Tvnz || originalNetworkId == (ushort)OriginalNetworkId.SkyNz;
    }

    public static bool IsOpenTvSkyUk(ushort originalNetworkId)
    {
      return originalNetworkId == (ushort)OriginalNetworkId.Astra28_2e || originalNetworkId == (ushort)OriginalNetworkId.Bbc;
    }

    public static bool IsVirginMediaUk(ushort originalNetworkId)
    {
      return originalNetworkId == (ushort)OriginalNetworkId.VirginMediaUk;
    }
  }
}
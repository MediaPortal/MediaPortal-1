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
  public enum BouquetOpenTvFoxtel
  {
    // AUSTAR - now defunct, bought by Foxtel; no region IDs
    Austar = 268,
    Test2 = 269,
    Sma = 555,
    AustarDigital = 666,
    AustarTest = 667,
    AustarBusiness = 668,
    HdAustarDigital = 766,    // added 5 January 2017
    ParentBouquet = 13858,

    // Foxtel and Optus TV featuring Foxtel
    Residential = 25184,
    CommercialAndPublic,
    Sports,
    CourtesyAndVips,
    Mcr,      // master control room
    MduLite,  // multi dwelling unit
    Optus,
    Website,  // 25191

    HdResidential = 25248,
    HdCommercialAndPublic,
    HdSports,
    HdCourtesyAndVips,
    // (HD MCR does not exist)
    HdMduLite = 25253,

    // removed 5 January 2017
    //HdOptus = 25254,                

    // added 5 January 2017
    SdBouquet = 25312,
    TotalUpper = 25316,
    MduSdLite = 25317,
    HdBouquet = 25376,
    HdMdu = 25381
  }
}
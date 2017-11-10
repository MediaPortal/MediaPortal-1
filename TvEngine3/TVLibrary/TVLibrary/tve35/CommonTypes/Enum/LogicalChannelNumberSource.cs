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

using System.ComponentModel;

namespace Mediaportal.TV.Server.Common.Types.Enum
{
  // Define in order of preference, from least to most preferred. It's assumed
  // that free-to-view providers are preferred over pay providers.
  public enum LogicalChannelNumberSource
  {
    [Description("SFR RED Canal+ [FR]")]
    SfrRedCanalPlus,      // FR - Astra 19.2e
    [Description("SFR RED Canalsat [FR]")]
    SfrRedCanalsat,       // FR - Astra 19.2e
    [Description("SFR RED Basic [FR]")]
    SfrRedBasic,          // FR - Astra 19.2e

    [Description("Sky [UK]")]
    SkyUk,                // UK - Astra 28.2e
    [Description("Sky [NZ]")]
    SkyNz,                // UK - Astra 28.2e
    [Description("Canalsat Suisse [CH]")]
    CanalsatSuisse,       // CH - Astra 19.2e

    [Description("TNT SAT [FR]")]
    TntSat,               // FR - Astra 19.2e
    [Description("Freeview Satellite [NZ]")]
    FreeviewSatellite,    // NZ - Optus D1 160e
    [Description("Freesat [UK]")]
    Freesat               // UK - Astra 28.2e
  }
}
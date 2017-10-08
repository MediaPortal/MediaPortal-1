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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Atsc.Enum
{
  /// <summary>
  /// ATSC service types. See A/53 part 1 table 4.1 and the ATSC code points registry "other" tab.
  /// </summary>
  internal enum ServiceType : byte
  {
    /// <summary>
    /// analog television (A/65)
    /// </summary>
    AnalogTelevision = 1,
    /// <summary>
    /// digital television service (A/53 part 3)
    /// </summary>
    DigitalTelevision = 2,
    /// <summary>
    /// audio service (A/53 part 3)
    /// </summary>
    Audio = 3,
    /// <summary>
    /// data only service (A/90)
    /// </summary>
    DataOnly = 4,
    /// <summary>
    /// software download service (A/97)
    /// </summary>
    SoftwareDownload = 5,
    /// <summary>
    /// unassociated/small screen service (A/53 part 3)
    /// </summary>
    SmallScreen = 6,
    /// <summary>
    /// parameterised service (A/71)
    /// </summary>
    Parameterised = 7,
    /// <summary>
    /// non real time service (A/103)
    /// </summary>
    Nrt = 8,
    /// <summary>
    /// extended parametised service (A/71)
    /// </summary>
    ExtendedParameterised = 9
  }
}
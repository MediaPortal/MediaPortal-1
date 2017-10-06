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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dvb.Enum
{
  /// <summary>
  /// DVB satellite roll-off factors. See EN 300 468 table 38.
  /// </summary>
  internal enum RollOffFactor : byte
  {
    /// <summary>
    /// 0.35 (35%)
    /// </summary>
    ThirtyFive = 0,
    /// <summary>
    /// 0.25 (25%)
    /// </summary>
    TwentyFive = 1,
    /// <summary>
    /// 0.20 (20%)
    /// </summary>
    Twenty = 2

    // (3 reserved)
  }
}
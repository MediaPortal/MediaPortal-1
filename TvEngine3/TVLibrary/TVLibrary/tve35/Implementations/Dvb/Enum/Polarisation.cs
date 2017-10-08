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
  /// DVB satellite polarisations. See EN 300 468 table 37.
  /// </summary>
  internal enum Polarisation : byte
  {
    /// <summary>
    /// linear horizontal
    /// </summary>
    LinearHorizontal = 0,
    /// <summary>
    /// linear vertical
    /// </summary>
    LinearVertical = 1,
    /// <summary>
    /// circular left
    /// </summary>
    CircularLeft = 2,
    /// <summary>
    /// circular right
    /// </summary>
    CircularRight = 3
  }
}
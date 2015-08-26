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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum
{
  /// <summary>
  /// DVB conditional access PMT list management actions.
  /// </summary>
  public enum CaPmtListManagementAction : byte
  {
    /// <summary>
    /// A program that is neither the first or last in a list of at least three programs.
    /// </summary>
    More = 0,
    /// <summary>
    /// First program in a list of at least two programs.
    /// </summary>
    First = 1,
    /// <summary>
    /// Last program in a list of at least two programs.
    /// </summary>
    Last = 2,
    /// <summary>
    /// The single program in a list.
    /// </summary>
    Only = 3,
    /// <summary>
    /// Add the program to the list.
    /// </summary>
    Add = 4,
    /// <summary>
    /// Update a program in the list.
    /// </summary>
    Update = 5
  }
}
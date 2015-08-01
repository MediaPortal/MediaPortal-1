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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu
{
  /// <summary>
  /// Conditional access menu state.
  /// </summary>
  public enum CiMenuState
  {
    /// menu is closed
    Closed = 0,
    /// opened
    Opened = 1,
    /// ready
    Ready = 2,
    /// request
    Request = 3,
    /// no choices
    NoChoices = 4,
    /// error
    Error = 5,
    /// open menu is to be closed
    Close = 6
  }
}
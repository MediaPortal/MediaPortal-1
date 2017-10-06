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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  /// <summary>
  /// This interface defines the capabilities of an electronic program guide
  /// data grabber controller.
  /// </summary>
  internal interface IEpgGrabberController
  {
    /// <summary>
    /// Enable the electronic programme guide data grabber.
    /// </summary>
    void Enable();

    /// <summary>
    /// Disable the electronic programme guide data grabber.
    /// </summary>
    void Disable();

    /// <summary>
    /// Indicates whether the electronic programme guide data grabber is enabled.
    /// </summary>
    /// <returns><c>true</c> if the electronic programme guide data grabber is enabled, otherwise <c>false</c></returns>
    bool IsEnabled();
  }
}
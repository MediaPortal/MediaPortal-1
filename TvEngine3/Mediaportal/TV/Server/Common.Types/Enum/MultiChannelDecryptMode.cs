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

namespace Mediaportal.TV.Server.Common.Types.Enum
{
  /// <summary>
  /// Modes for decrypting multiple services from a single transmitter
  /// simultaneously.
  /// </summary>
  /// <remarks>
  /// The mode is applied within TV library and appropriate commands are sent
  /// to conditional access extensions.
  /// </remarks>
  public enum MultiChannelDecryptMode
  {
    /// <summary>
    /// Send only one service.
    /// </summary>
    Disabled,
    /// <summary>
    /// Send the current list of services that must be decrypted. Most compatible.
    /// </summary>
    List,
    /// <summary>
    /// Send only the service that is being added to or removed from the list.
    /// </summary>
    Changes
  }
}
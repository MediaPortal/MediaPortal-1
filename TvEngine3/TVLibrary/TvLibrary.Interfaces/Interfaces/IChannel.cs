/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
namespace TvLibrary.Interfaces
{
  /// <summary>
  /// interface which describes a tv/radio channel
  /// </summary>
  public interface IChannel
  {
    /// <summary>
    /// gets/sets the channel name
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// boolean indication if this is a radio channel
    /// </summary>
    bool IsRadio { get; set; }

    /// <summary>
    /// boolean indication if this is a tv channel
    /// </summary>
    bool IsTv { get; set; }

    /// <summary>
    /// Checks if the given channel and this instance are on the different transponder
    /// </summary>
    /// <param name="channel">Channel to check</param>
    /// <returns>true, if the channels are on the same transponder</returns>
    bool IsDifferentTransponder(IChannel channel);
  }
}
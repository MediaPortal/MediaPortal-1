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
using System.Collections.Generic;

namespace TvLibrary.Interfaces
{
  /// <summary>
  /// Interface for scanning new channels
  /// </summary>
  public interface ITVScanning
  {
    /// <summary>
    /// Disposes this instance.
    /// </summary>
    void Dispose();

    /// <summary>
    /// resets the scanner
    /// </summary>
    void Reset();

    /// <summary>
    /// Tunes to the channel specified and will start scanning for any channel
    /// </summary>
    /// <param name="channel">channel to tune to</param>
    /// <param name="settings">ScanParameters to use while tuning</param>
    /// <returns>list of channels found</returns>
    List<IChannel> Scan(IChannel channel, ScanParameters settings);

    /// <summary>
    /// Tunes to channels based on the list the multiplexes that make up a DVB network.
    /// This information is obtained from the DVB NIT (Network Information Table)
    /// </summary>
    /// <param name="channel">channel to tune to</param>
    /// <param name="settings">ScanParameters to use while tuning</param>
    /// <returns></returns>
    List<IChannel> ScanNIT(IChannel channel, ScanParameters settings);

    /// <summary>
    /// returns the tv card used 
    /// </summary>
    ITVCard TvCard { get;}
  }
}

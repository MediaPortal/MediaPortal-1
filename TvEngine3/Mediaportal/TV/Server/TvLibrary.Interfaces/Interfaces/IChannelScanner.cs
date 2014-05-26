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

using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces
{
  /// <summary>
  /// Interface for scanning new channels
  /// </summary>
  public interface IChannelScanner
  {
    /// <summary>
    /// Reload the scanner's configuration.
    /// </summary>
    void ReloadConfiguration();

    /// <summary>
    /// Get the scanner's current status.
    /// </summary>
    /// <value><c>true</c> if the scanner is scanning, otherwise <c>false</c></value>
    bool IsScanning
    {
      get;
    }

    /// <summary>
    /// Tunes to the channel specified and will start scanning for any channel
    /// </summary>
    /// <param name="channel">channel to tune to</param>
    /// <returns>list of channels found</returns>
    List<IChannel> Scan(IChannel channel);

    /// <summary>
    /// Tunes to channels based on the list the multiplexes that make up a DVB network.
    /// This information is obtained from the DVB NIT (Network Information Table)
    /// </summary>
    /// <param name="channel">channel to tune to</param>
    /// <returns></returns>
    List<IChannel> ScanNIT(IChannel channel);

    /// <summary>
    /// Abort scanning for channels.
    /// </summary>
    void AbortScanning();
  }
}
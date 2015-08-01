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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.TuningDetail;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner
{
  /// <summary>
  /// Tuner channel scanner interface.
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
    /// Tune to a specified channel and scan for channel information.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the channel information found</returns>
    List<IChannel> Scan(IChannel channel);

    /// <summary>
    /// Tune to a specified channel and scan for network information.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>the network information found</returns>
    List<TuningDetail> ScanNIT(IChannel channel);

    /// <summary>
    /// Abort scanning for channels.
    /// </summary>
    void AbortScanning();
  }
}
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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  internal interface ISubChannelManager
  {
    /// <summary>
    /// Reload the manager's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    void ReloadConfiguration(TVDatabase.Entities.Tuner configuration);

    /// <summary>
    /// Set the manager's extensions.
    /// </summary>
    /// <param name="extensions">A list of the tuner's extensions, in priority order.</param>
    void SetExtensions(IList<ITunerExtension> extensions);

    /// <summary>
    /// Decompose the sub-channel manager.
    /// </summary>
    void Decompose();

    #region tuning

    /// <summary>
    /// This function should be called before the tuner is tuned to a new
    /// transmitter.
    /// </summary>
    void OnBeforeTune();

    /// <summary>
    /// Tune a sub-channel.
    /// </summary>
    /// <param name="id">The sub-channel's identifier.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <param name="isNew"><c>True</c> if the sub-channel is newly created.</param>
    /// <returns>the sub-channel</returns>
    ISubChannel Tune(int id, IChannel channel, out bool isNew);

    /// <summary>
    /// Cancel the current tuning process.
    /// </summary>
    /// <param name="id">The identifier of the sub-channel associated with the tuning process that is being cancelled.</param>
    void CancelTune(int id);

    #endregion

    #region sub-channels

    /// <summary>
    /// Can the sub-channel manager receive all sub-channels from the current transmitter simultaneously?
    /// </summary>
    bool CanReceiveAllTransmitterSubChannels
    {
      get;
    }

    /// <summary>
    /// Get a sub-channel.
    /// </summary>
    /// <param name="id">The sub-channel's identifier.</param>
    /// <returns>the sub-channel if it exists, otherwise <c>null</c></returns>
    ISubChannel GetSubChannel(int id);

    /// <summary>
    /// Free a sub-channel.
    /// </summary>
    /// <param name="id">The sub-channel's identifier.</param>
    void FreeSubChannel(int id);

    /// <summary>
    /// Get the count of sub-channels.
    /// </summary>
    int SubChannelCount
    {
      get;
    }

    /// <summary>
    /// Get the set of sub-channel identifiers for each channel the tuner is
    /// currently decrypting.
    /// </summary>
    /// <returns>a collection of sub-channel identifier lists</returns>
    ICollection<IList<int>> GetDecryptedSubChannelDetails();

    /// <summary>
    /// Determine whether a sub-channel is being decrypted.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the sub-channel is being decrypted, otherwise <c>false</c></returns>
    bool IsDecrypting(IChannel channel);

    #endregion
  }
}
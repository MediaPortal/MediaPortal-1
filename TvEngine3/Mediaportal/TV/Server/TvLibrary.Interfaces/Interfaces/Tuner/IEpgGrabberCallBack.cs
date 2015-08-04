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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner
{
  /// <summary>
  /// Call back interface for EPG grabbing progress notification.
  /// </summary>
  public interface IEpgGrabberCallBack
  {
    /// <summary>
    /// Called when electronic programme guide data grabbing is cancelled.
    /// </summary>
    void OnEpgCancelled();

    /// <summary>
    /// Called when electronic programme guide data grabbing is complete.
    /// </summary>
    /// <param name="tuningDetail">The tuning details of the transmitter from which the EPG was grabbed.</param>
    /// <param name="epg">The grabbed data.</param>
    void OnEpgReceived(IChannel tuningDetail, IDictionary<IChannel, IList<EpgProgram>> epg);
  }
}
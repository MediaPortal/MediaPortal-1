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

using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces
{
  internal interface ISubChannelInternal : ISubChannel
  {
    /// <summary>
    /// Get or set the channel which the sub-channel is tuned to.
    /// </summary>
    new IChannel CurrentChannel { get; set; }

    /// <summary>
    /// Set the sub-channel's quality control interface.
    /// </summary>
    IQualityControlInternal QualityControlInterface { set; }

    /// <summary>
    /// Cancel the current tuning process.
    /// </summary>
    void CancelTune();

    /// <summary>
    /// Decompose the sub-channel.
    /// </summary>
    void Decompose();
  }
}
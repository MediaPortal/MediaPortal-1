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
using Mediaportal.TV.Server.TVLibrary.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dvb
{
  internal class SubChannelManagerDvb : SubChannelManagerMpeg2Ts
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="SubChannelManagerAnalog"/> class.
    /// </summary>
    /// <param name="tsWriter">The TS writer instance used to perform/implement time-shifting and recording.</param>
    /// <param name="canReceiveAllTransmitterSubChannels"><c>True</c> if the tuner can simultaneously receive all sub-channels from the tuned transmitter.</param>
    public SubChannelManagerDvb(ITsWriter tsWriter, bool canReceiveAllTransmitterSubChannels = true)
      : base(tsWriter, canReceiveAllTransmitterSubChannels)
    {
      // The sub-channel manager needs the DVB SDT to determine whether the
      // services are running or not.
      AlwaysRequiredPids = new HashSet<ushort> { SubChannelManagerMpeg2Ts.PID_PAT, 0x11 };
    }
  }
}
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

using Mediaportal.TV.Server.TVLibrary.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Analog
{
  internal class SubChannelManagerAnalog : SubChannelManagerDvb
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="SubChannelManagerAnalog"/> class.
    /// </summary>
    /// <param name="tsWriter">The TS writer instance used to perform/implement time-shifting and recording.</param>
    public SubChannelManagerAnalog(ITsWriter tsWriter)
      : base(tsWriter)
    {
    }

    protected override int GetTuningProgramNumber(IChannel channel)
    {
      if (channel != null && !string.IsNullOrEmpty(channel.Name))
      {
        return 1;   // TsMuxer's fixed/static program number
      }
      return ChannelMpeg2Base.PROGRAM_NUMBER_SCANNING;
    }
  }
}
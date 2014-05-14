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
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Mpeg2Ts
{
  internal class SubChannelManagerMpeg2Ts
  {
    private Cat _cat = null;
    private HashSet<ushort> _emmPids = new HashSet<ushort>();
    private HashSet<ushort> _epgPids = new HashSet<ushort>();
    private HashSet<ushort> _channelScanningPids = new HashSet<ushort>();
    private HashSet<ushort> STATIC_PIDS = new HashSet<ushort> { 0 };
    // program number => sub-channel
    private Dictionary<ushort, SubChannel> _subChannels = new Dictionary<ushort, SubChannel>();

    private struct SubChannel
    {
      public int ProgramNumber;
      public bool IsEncrypted;
      public HashSet<ushort> VideoAudioSubtitleTeletextPids;
      public HashSet<ushort> EcmPids;
      public Pmt Pmt;
      public HashSet<int> SubChannelIds;
    }
  }
}
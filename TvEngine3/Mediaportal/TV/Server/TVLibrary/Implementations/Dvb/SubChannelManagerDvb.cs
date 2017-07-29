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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dvb
{
  internal class SubChannelManagerDvb : SubChannelManagerMpeg2Ts
  {
    private bool _scanMediaHighway2ChannelPid = false;

    /// <summary>
    /// Initialise a new instance of the <see cref="SubChannelManagerDvb"/> class.
    /// </summary>
    /// <remarks>
    /// A DVB sub-channel manager requires the service description table (SDT)
    /// in addition to the standard MPEG 2 TS program association table (PAT).
    /// The SDT is used to determine whether services are running or not.
    /// </remarks>
    /// <param name="tsWriter">The TS writer instance used to perform/implement time-shifting and recording.</param>
    public SubChannelManagerDvb(ITsWriter tsWriter)
      : base(tsWriter, new List<ushort> { SubChannelManagerMpeg2Ts.PID_PAT, 0x11  /* SDT, BAT */ })
    {
    }

    protected override HashSet<ushort> GetScanningPids()
    {
      if (_scanMediaHighway2ChannelPid)
      {
        // PID 0xc8 carries MHW2 channel and theme information.
        return new HashSet<ushort> { 0xc8 };
      }
      return new HashSet<ushort>();
    }

    #region ISubChannelManager members

    /// <summary>
    /// Reload the manager's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    public override void ReloadConfiguration(Tuner configuration)
    {
      ChannelGroupType channelGroupTypes = ChannelGroupType.FreesatChannelCategory | ChannelGroupType.MediaHighwayChannelCategory | ChannelGroupType.NorDigChannelList | ChannelGroupType.OpenTvChannelCategory | ChannelGroupType.VirginMediaChannelCategory;
      channelGroupTypes = (ChannelGroupType)SettingsManagement.GetValue("scanAutoCreateChannelGroups", (int)channelGroupTypes);
      _scanMediaHighway2ChannelPid = channelGroupTypes.HasFlag(ChannelGroupType.MediaHighwayChannelCategory);
    }

    #endregion
  }
}
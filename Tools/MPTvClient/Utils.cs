#region Copyright (C) 2005-2016 Team MediaPortal

// Copyright (C) 2005-2016 Team MediaPortal
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

using System;
using Microsoft.Win32;


namespace MPTvClient
{
    public class ReceptionDetails
  {
    public int signalLevel;
    public int signalQuality;
  }

  public class StreamingStatus
  {
    public int cardId;
    public string cardName;
    public string cardType;
    public string status;
    public string channelName;
    public string userName;
  }

  public class ProgrammInfo
  {
    public string timeInfo;
    public string description;
  }

  public class EPGInfo
  {
    public DateTime startTime;
    public DateTime endTime;
    public string title;
    public string description;
  }

  public class ChannelInfo
  {
    public string channelID;
    public string name;
    public bool isWebStream;
    public ProgrammInfo epgNow;
    public ProgrammInfo epgNext;
  }

  public class RecordingInfo
  {
    public string recordingID;
    public string title;
    public string genre;
    public string description;
    public string timeInfo;
  }

  public class ScheduleInfo
  {
    public string scheduleID;
    public DateTime startTime;
    public DateTime endTime;
    public string channelName;
    public string description;
    public string type;
  }

  public class ClientSettings
  {
    public static string playerPath = "";
    public static string playerArgs = "{0}";
    public static string serverHostname = "";
    public static bool useOverride = false;
    public static string overrideURL = "";
    public static bool alwaysPerformConnectionChecks;
    public static int frmLeft;
    public static int frmTop;
    public static int frmWidth;
    public static int frmHeight;

    public static bool IsValid()
    {
      return (System.IO.File.Exists(playerPath)) && (playerArgs != "") && (serverHostname != "");
    }

    public static void Load()
    {
            playerPath = Properties.Settings.Default.playerPath;
            playerArgs = Properties.Settings.Default.playerArgs;
            serverHostname = Properties.Settings.Default.serverHostname;
            useOverride = Properties.Settings.Default.useOverride;
            overrideURL = Properties.Settings.Default.overrideURL;
            alwaysPerformConnectionChecks = Properties.Settings.Default.alwaysPerformConnectionChecks;
            frmLeft = Properties.Settings.Default.frmLeft;
            frmTop = Properties.Settings.Default.frmTop;
            frmWidth = Properties.Settings.Default.frmWidth;
            frmHeight = Properties.Settings.Default.frmHeight;
        }

    public static void Save()
    {
            Properties.Settings.Default.playerPath = playerPath;
            Properties.Settings.Default.playerArgs = playerArgs;
            Properties.Settings.Default.serverHostname = serverHostname;
            Properties.Settings.Default.useOverride = useOverride;
            Properties.Settings.Default.overrideURL = overrideURL;
            Properties.Settings.Default.alwaysPerformConnectionChecks = alwaysPerformConnectionChecks;
            Properties.Settings.Default.frmLeft = frmLeft;
            Properties.Settings.Default.frmTop = frmTop;
            Properties.Settings.Default.frmWidth = frmWidth;
            Properties.Settings.Default.frmHeight = frmHeight;

            Properties.Settings.Default.Save();
        }
  }
}
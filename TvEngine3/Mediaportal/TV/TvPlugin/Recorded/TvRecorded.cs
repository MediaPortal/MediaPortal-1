#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

using System.IO;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;

namespace Mediaportal.TV.TvPlugin.Recorded
{
  public class TvRecorded : RecordedBase
  {
    public TvRecorded() : base()
    {
      GetID = (int)Window.WINDOW_RECORDEDTV;
    }

    protected override bool OnSelectedRecording(int iItem)
    {
      GUIListItem item;
      return OnSelectedRecording(iItem, MediaType.Television, g_Player.MediaType.Recording, out item);
    }

    protected override string GetCachedRecordingFileName(Recording recording)
    {
      return Path.Combine(Thumbs.TVRecorded, Path.ChangeExtension(Path.GetFileName(recording.FileName), Utils.GetThumbExtension()));
    }

    protected override string ThumbsType
    {
      get
      {
        return Thumbs.TVChannel;
      }
    }

    protected override string SettingsSection
    {
      get
      {
        return "tvrecorded";
      }
    }

    protected override MediaType MediaType
    {
      get
      {
        return MediaType.Television;
      }
    }

    protected override string SkinPropertyPrefix
    {
      get
      {
        return "#TV.RecordedTV";
      }
    }

    protected override string SkinFileName
    {
      get
      {
        return @"\myrecordedtv.xml";
      }
    }

    protected override int ChannelViewOptionStringId
    {
      get
      {
        return 915;
      }
    }

    #region playback events

    protected override void DoOnPlayBackStoppedOrChanged(g_Player.MediaType type, int stoptime, string filename, string caller)
    {
      if (type != g_Player.MediaType.Recording)
      {
        return;
      }
      base.DoOnPlayBackStoppedOrChanged(type, stoptime, filename, caller);
    }

    protected override void OnPlayRecordingBackEnded(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Recording)
      {
        return;
      }
      base.OnPlayRecordingBackEnded(type, filename);
    }

    protected override void OnPlayRecordingBackStarted(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Recording)
      {
        return;
      }
      base.OnPlayRecordingBackStarted(type, filename);
    }

    #endregion
  }
}
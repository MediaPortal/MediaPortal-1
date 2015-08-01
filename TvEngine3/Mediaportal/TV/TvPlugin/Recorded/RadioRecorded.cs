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

using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;

namespace Mediaportal.TV.TvPlugin.Recorded
{
  public class RadioRecorded : RecordedBase
  {
    public RadioRecorded() : base()
    {
      GetID = (int)Window.WINDOW_RECORDEDRADIO;
    }

    protected override bool OnSelectedRecording(int iItem)
    {
      GUIListItem item;
      bool toReturn = OnSelectedRecording(iItem, MediaType.Radio, g_Player.MediaType.RadioRecording, out item);
      if (item != null && toReturn)
      {
        GUIPropertyManager.RemovePlayerProperties();
        GUIPropertyManager.SetProperty("#Play.Current.ArtistThumb", item.Label);
        GUIPropertyManager.SetProperty("#Play.Current.Album", item.Label);
        GUIPropertyManager.SetProperty("#Play.Current.Thumb", item.ThumbnailImage);
      }
      return toReturn;
    }

    protected override string GetCachedRecordingFileName(Recording recording)
    {
      return Utils.GetCoverArt(Thumbs.Radio, GetChannelDisplayName(recording));
    }

    protected override string ThumbsType
    {
      get
      {
        return Thumbs.Radio;
      }
    }

    protected override string SettingsSection
    {
      get
      {
        return "radiorecorded";
      }
    }

    protected override MediaType MediaType
    {
      get
      {
        return MediaType.Radio;
      }
    }

    protected override string SkinPropertyPrefix
    {
      get
      {
        return "#Radio.Recorded";
      }
    }

    protected override string SkinFileName
    {
      get
      {
        return @"\myradiorecorded.xml";
      }
    }

    protected override int ChannelViewOptionStringId
    {
      get
      {
        return 812;
      }
    }

    #region playback events

    protected override void DoOnPlayBackStoppedOrChanged(g_Player.MediaType type, int stoptime, string filename, string caller)
    {
      if (type != g_Player.MediaType.Radio)
      {
        return;
      }
      base.DoOnPlayBackStoppedOrChanged(type, stoptime, filename, caller);
    }
      
    protected override void OnPlayRecordingBackEnded(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Radio)
      {
        return;
      }
      base.OnPlayRecordingBackEnded(type, filename);
    }

    protected override void OnPlayRecordingBackStarted(g_Player.MediaType type, string filename)
    {
      if (type != g_Player.MediaType.Radio)
      {
        return;
      }
      base.OnPlayRecordingBackStarted(type, filename);
    }

    #endregion
  }
}
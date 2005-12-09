/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
namespace MediaPortal.Playlists
{
  /// <summary>
  /// 
  /// </summary>
  public class PlayListPlayer
  {
    public enum PlayListType
    {
      PLAYLIST_NONE
    ,
      PLAYLIST_MUSIC
    ,
      PLAYLIST_MUSIC_TEMP
    ,
      PLAYLIST_VIDEO
    , PLAYLIST_VIDEO_TEMP
    };
    static int _entriesNotFound = 0;
    static bool _isChanged = false;
    static int _currentSong = -1;
    static PlayListType _currentPlayList = PlayListType.PLAYLIST_NONE;
    static PlayList _musicPlayList = new PlayList();
    static PlayList _tempMusicPlayList = new PlayList();
    static PlayList _videoPlayList = new PlayList();
    static PlayList _tempVideoPlayList = new PlayList();
    static PlayList _emptyPlayList = new PlayList();
    static bool _repeatPlayList = true;

    // singleton. Dont allow any instance of this class
    private PlayListPlayer()
    {
    }

    static public void Init()
    {
      GUIWindowManager.Receivers += new SendMessageHandler(PlayListPlayer.OnMessage);
    }
    static public void OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED:
          {
            PlayList.PlayListItem item = GetCurrentItem();
            if (item != null)
            {
              if (item.Type == PlayList.PlayListItem.PlayListItemType.Radio ||
                item.Type == PlayList.PlayListItem.PlayListItemType.AudioStream)
              {

                return;
              }
            }
            Reset();
            _currentPlayList = PlayListType.PLAYLIST_NONE;
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_ENDED:
          {
            PlayNext(true);
            if (!g_Player.Playing)
            {
              g_Player.Release();
            }
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_PLAY_FILE:
          {
            Log.Write("Playlistplayer.StartFile({0})", message.Label);
            g_Player.Play(message.Label);
            if (!g_Player.Playing) g_Player.Stop();
          }
          break;
        case GUIMessage.MessageType.GUI_MSG_STOP_FILE:
          {
            Log.Write("Playlistplayer.Stopfile");
            g_Player.Stop();
          }
          break;
        case GUIMessage.MessageType.GUI_MSG_SEEK_FILE_PERCENTAGE:
          {
            Log.Write("Playlistplayer.SeekPercent({0}%)", message.Param1);
            g_Player.SeekAsolutePercentage(message.Param1);
            Log.Write("Playlistplayer.SeekPercent({0}%) done", message.Param1);
          }
          break;
        case GUIMessage.MessageType.GUI_MSG_SEEK_FILE_END:
          {
            double duration = g_Player.Duration;
            double position = g_Player.CurrentPosition;
            if (position < duration - 1d)
            {
              Log.Write("Playlistplayer.SeekEnd({0})", duration);
              g_Player.SeekAbsolute(duration - 2d);
              Log.Write("Playlistplayer.SeekEnd({0}) done", g_Player.CurrentPosition);
            }
          }
          break;
        case GUIMessage.MessageType.GUI_MSG_SEEK_POSITION:
          {
            g_Player.SeekAbsolute(message.Param1);
          }
          break;
      }
    }

    static public string Get(int iSong)
    {
      if (_currentPlayList == PlayListType.PLAYLIST_NONE) return String.Empty;

      PlayList playlist = GetPlaylist(_currentPlayList);
      if (playlist.Count <= 0) return String.Empty;

      if (iSong >= playlist.Count)
      {
        //	Is last element of video stacking playlist?
        if (_currentPlayList == PlayListType.PLAYLIST_VIDEO_TEMP)
        {
          return String.Empty;
        }

        if (!_repeatPlayList)
        {
          return String.Empty; ;
        }
        iSong = 0;
      }

      PlayList.PlayListItem item = playlist[iSong];
      return item.FileName;
    }

    static public PlayList.PlayListItem GetCurrentItem()
    {
      if (_currentSong < 0) return null;

      PlayList playlist = GetPlaylist(_currentPlayList);
      if (playlist == null) return null;

      if (_currentSong < 0 || _currentSong >= playlist.Count)
        _currentSong = 0;

      if (_currentSong >= playlist.Count) return null;

      return playlist[_currentSong];
    }

    static public string GetNext()
    {
      if (_currentPlayList == PlayListType.PLAYLIST_NONE) return String.Empty;

      PlayList playlist = GetPlaylist(_currentPlayList);
      if (playlist.Count <= 0) return String.Empty;
      int iSong = _currentSong;
      iSong++;

      if (iSong >= playlist.Count)
      {
        //	Is last element of video stacking playlist?
        if (_currentPlayList == PlayListType.PLAYLIST_VIDEO_TEMP)
        {
          return String.Empty;
        }

        if (!_repeatPlayList)
        {
          return String.Empty; ;
        }
        iSong = 0;
      }

      PlayList.PlayListItem item = playlist[iSong];
      return item.FileName;
    }

    static public void PlayNext(bool bAutoPlay)
    {
      if (_currentPlayList == PlayListType.PLAYLIST_NONE) return;

      PlayList playlist = GetPlaylist(_currentPlayList);
      if (playlist.Count <= 0) return;
      int iSong = _currentSong;
      iSong++;

      if (iSong >= playlist.Count)
      {
        //	Is last element of video stacking playlist?
        if (_currentPlayList == PlayListType.PLAYLIST_VIDEO_TEMP)
        {
          //	Disable playlist playback
          _currentPlayList = PlayListType.PLAYLIST_NONE;
          return;
        }

        if (!_repeatPlayList)
        {
          _currentPlayList = PlayListType.PLAYLIST_NONE;
          return;
        }
        iSong = 0;
      }

      if (bAutoPlay)
      {
        PlayList.PlayListItem item = playlist[iSong];
        //TODO
        //if ( CUtil::IsShoutCast(item.GetFileName()) )
        //{
        //  return;
        //}
      }
      Play(iSong);


      if (!g_Player.Playing)
      {
        PlayNext(bAutoPlay);
      }
    }

    static public void PlayPrevious()
    {
      if (_currentPlayList == PlayListType.PLAYLIST_NONE)
        return;

      PlayList playlist = GetPlaylist(_currentPlayList);
      if (playlist.Count <= 0) return;
      int iSong = _currentSong;
      iSong--;
      if (iSong < 0)
        iSong = playlist.Count - 1;

      Play(iSong);
      if (!g_Player.Playing)
      {
        PlayPrevious();
      }
    }

    static public void Play(string filename)
    {
      if (_currentPlayList == PlayListType.PLAYLIST_NONE)
        return;

      PlayList playlist = GetPlaylist(_currentPlayList);
      if (playlist.Count <= 0) return;
      for (int i = 0; i < playlist.Count; ++i)
      {
        PlayList.PlayListItem item = playlist[i];
        if (item.FileName.Equals(filename))
        {
          Play(i);
          return;
        }
      }
    }

    static public void Play(int iSong)
    {
      if (_currentPlayList == PlayListType.PLAYLIST_NONE)
        return;

      PlayList playlist = GetPlaylist(_currentPlayList);
      if (playlist.Count <= 0) return;
      if (iSong < 0) iSong = 0;
      if (iSong >= playlist.Count) iSong = playlist.Count - 1;

      _isChanged = true;
      int iPreviousSong = _currentSong;
      _currentSong = iSong;
      PlayList.PlayListItem item = playlist[_currentSong];

      if (playlist.AllPlayed())
      {
        playlist.ResetStatus();
      }

      Log.Write("PlaylistPlayer.Play({0})", item.FileName);
      if (item.Type == PlayList.PlayListItem.PlayListItemType.Radio)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_TUNE_RADIO, 0, 0, 0, 0, 0, null);
        msg.Label = item.Description;
        GUIGraphicsContext.SendMessage(msg);
        item.Played = true;
        return;
      }

      if (!g_Player.Play(item.FileName))
      {
        //	Count entries in current playlist
        //	that couldn't be played
        _entriesNotFound++;
      }
      else
      {
        item.Played = true;
        if (Utils.IsVideo(item.FileName))
        {
          if (g_Player.HasVideo)
          {
            GUIGraphicsContext.IsFullScreenVideo = true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
          }
        }
      }
    }

    static public int CurrentSong
    {
      get { return _currentSong; }
      set
      {
        if (value >= -1 && value < GetPlaylist(CurrentPlaylist).Count)
          _currentSong = value;
      }
    }

    static public void Remove(PlayListType type, string filename)
    {
      PlayList playlist = GetPlaylist(type);
      int itemRemoved = playlist.Remove(filename);
      if (type != CurrentPlaylist)
      {
        return;
      }
      if (_currentSong >= itemRemoved) _currentSong--;
    }

    static public bool HasChanged
    {
      get
      {
        bool result = _isChanged;
        _isChanged = false;
        return result;
      }
    }

    static public Playlists.PlayListPlayer.PlayListType CurrentPlaylist
    {
      get { return _currentPlayList; }
      set
      {
        if (_currentPlayList != value)
        {
          _currentPlayList = value;
          _entriesNotFound = 0;
          _isChanged = true;
          using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
          {
            if (value == PlayListType.PLAYLIST_MUSIC || value == PlayListType.PLAYLIST_MUSIC_TEMP)
            {
              _repeatPlayList = xmlreader.GetValueAsBool("musicfiles", "repeat", true);
            }
            else
            {
              _repeatPlayList = xmlreader.GetValueAsBool("movies", "repeat", true);
            }
          }
        }
      }
    }

    static public PlayList GetPlaylist(Playlists.PlayListPlayer.PlayListType nPlayList)
    {
      switch (nPlayList)
      {
        case PlayListType.PLAYLIST_MUSIC:
          return _musicPlayList;

        case PlayListType.PLAYLIST_MUSIC_TEMP:
          return _tempMusicPlayList;
        case PlayListType.PLAYLIST_VIDEO:
          return _videoPlayList;
        case PlayListType.PLAYLIST_VIDEO_TEMP:
          return _tempVideoPlayList;
        default:
          _emptyPlayList.Clear();
          return _emptyPlayList;
      }
    }

    static public int RemoveDVDItems()
    {
      int removedDvdItems = _musicPlayList.RemoveDVDItems();
      _tempMusicPlayList.RemoveDVDItems();
      int removedVideoItems = _videoPlayList.RemoveDVDItems();
      _tempVideoPlayList.RemoveDVDItems();

      return removedDvdItems + removedVideoItems;

    }

    static public void Reset()
    {
      _currentSong = -1;
      _entriesNotFound = 0;
    }

    static public int EntriesNotFound
    {
      get
      {
        return _entriesNotFound;
      }
    }
  }
}

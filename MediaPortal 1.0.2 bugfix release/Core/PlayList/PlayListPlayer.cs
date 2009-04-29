#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
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

#endregion

using MediaPortal.GUI.Library;

namespace MediaPortal.Playlists
{
  public class PlayListPlayer
  {
    #region g_Player decoupling in work

    public interface IPlayer
    {
      bool Playing { get; }
      void Release();
      bool Play(string strFile);
      bool PlayVideoStream(string strURL, string streamName);
      bool PlayAudioStream(string strURL);
      void Stop();
      void SeekAsolutePercentage(int iPercentage);
      double Duration { get; }
      double CurrentPosition { get; }
      void SeekAbsolute(double dTime);
      bool HasVideo { get; }
      bool ShowFullScreenWindow();
    }

    private class FakePlayer : IPlayer
    {
      public bool Playing
      {
        get { return Player.g_Player.Playing; }
      }

      public void Release()
      {
        Player.g_Player.Release();
      }

      bool IPlayer.Play(string strFile)
      {
        return Player.g_Player.Play(strFile);
      }

      bool IPlayer.PlayVideoStream(string strURL, string streamName)
      {
        return Player.g_Player.PlayVideoStream(strURL, streamName);
      }

      bool IPlayer.PlayAudioStream(string strURL)
      {
        return Player.g_Player.PlayAudioStream(strURL);
      }

      public void Stop()
      {
        Player.g_Player.Stop();
      }

      public void SeekAsolutePercentage(int iPercentage)
      {
        Player.g_Player.SeekAsolutePercentage(iPercentage);
      }

      public double Duration
      {
        get { return Player.g_Player.Duration; }
      }

      public double CurrentPosition
      {
        get { return Player.g_Player.CurrentPosition; }
      }

      public void SeekAbsolute(double dTime)
      {
        Player.g_Player.SeekAbsolute(dTime);
      }

      public bool HasVideo
      {
        get { return Player.g_Player.HasVideo; }
      }

      public bool ShowFullScreenWindow()
      {
        return Player.g_Player.ShowFullScreenWindow();
      }
    }

    public IPlayer g_Player = new FakePlayer();

    #endregion

    private int _entriesNotFound = 0;
    private int _currentItem = -1;
    private PlayListType _currentPlayList = PlayListType.PLAYLIST_NONE;
    private PlayList _musicPlayList = new PlayList();
    private PlayList _tempMusicPlayList = new PlayList();
    private PlayList _videoPlayList = new PlayList();
    private PlayList _tempVideoPlayList = new PlayList();
    private PlayList _emptyPlayList = new PlayList();
    private PlayList _musicVideoPlayList = new PlayList();
    private PlayList _radioStreamPlayList = new PlayList();
    private bool _repeatPlayList = true;
    private string _currentPlaylistName = string.Empty;

    public PlayListPlayer()
    {
    }

    private static PlayListPlayer singletonPlayer = new PlayListPlayer();

    public static PlayListPlayer SingletonPlayer
    {
      get { return singletonPlayer; }
    }

    public void InitTest()
    {
      GUIGraphicsContext.Receivers += new SendMessageHandler(this.OnMessage);
    }

    public void Init()
    {
      GUIWindowManager.Receivers += new SendMessageHandler(this.OnMessage);
    }

    public void OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED:
          {
            PlayListItem item = GetCurrentItem();
            if (item != null)
            {
              if (item.Type != PlayListItem.PlayListItemType.Radio ||
                  item.Type != PlayListItem.PlayListItemType.AudioStream)
              {
                Reset();
                _currentPlayList = PlayListType.PLAYLIST_NONE;
              }
            }
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS, 0, 0, 0, -1, 0, null);
            GUIGraphicsContext.SendMessage(msg);
          }
          break;

          // SV Allows BassMusicPlayer to continuously play
        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_CROSSFADING:
          {
            // This message is only sent by BASS in gapless/crossfading mode
            PlayNext();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_ENDED:
          {
            // This message is sent by both the internal and BASS player
            // In case of gapless/crossfading it is only sent after the last song
            PlayNext();
            if (!g_Player.Playing)
            {
              g_Player.Release();

              // Clear focus when playback ended
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS, 0, 0, 0, -1, 0, null);
              GUIGraphicsContext.SendMessage(msg);
            }
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_PLAY_FILE:
          {
            Log.Info("Playlistplayer: Start file ({0})", message.Label);
            g_Player.Play(message.Label);
            if (!g_Player.Playing)
            {
              g_Player.Stop();
            }
          }
          break;
        case GUIMessage.MessageType.GUI_MSG_STOP_FILE:
          {
            Log.Info("Playlistplayer: Stop file");
            g_Player.Stop();
          }
          break;
        case GUIMessage.MessageType.GUI_MSG_SEEK_FILE_PERCENTAGE:
          {
            Log.Info("Playlistplayer: SeekPercent ({0}%)", message.Param1);
            g_Player.SeekAsolutePercentage(message.Param1);
            Log.Debug("Playlistplayer: SeekPercent ({0}%) done", message.Param1);
          }
          break;
        case GUIMessage.MessageType.GUI_MSG_SEEK_FILE_END:
          {
            double duration = g_Player.Duration;
            double position = g_Player.CurrentPosition;
            if (position < duration - 1d)
            {
              Log.Info("Playlistplayer: SeekEnd ({0})", duration);
              g_Player.SeekAbsolute(duration - 2d);
              Log.Debug("Playlistplayer: SeekEnd ({0}) done", g_Player.CurrentPosition);
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

    public string Get(int iSong)
    {
      if (_currentPlayList == PlayListType.PLAYLIST_NONE)
      {
        return string.Empty;
      }

      PlayList playlist = GetPlaylist(_currentPlayList);
      if (playlist.Count <= 0)
      {
        return string.Empty;
      }

      if (iSong >= playlist.Count)
      {
        //	Is last element of video stacking playlist?
        if (_currentPlayList == PlayListType.PLAYLIST_VIDEO_TEMP)
        {
          return string.Empty;
        }

        if (!_repeatPlayList)
        {
          return string.Empty;
          ;
        }
        iSong = 0;
      }

      PlayListItem item = playlist[iSong];
      return item.FileName;
    }

    public PlayListItem GetCurrentItem()
    {
      if (_currentItem < 0)
      {
        return null;
      }

      PlayList playlist = GetPlaylist(_currentPlayList);
      if (playlist == null)
      {
        return null;
      }

      if (_currentItem < 0 || _currentItem >= playlist.Count)
      {
        _currentItem = 0;
      }

      if (_currentItem >= playlist.Count)
      {
        return null;
      }

      return playlist[_currentItem];
    }

    public PlayListItem GetNextItem()
    {
      if (_currentPlayList == PlayListType.PLAYLIST_NONE)
      {
        return null;
      }

      PlayList playlist = GetPlaylist(_currentPlayList);
      if (playlist.Count <= 0)
      {
        return null;
      }
      int iSong = _currentItem;
      iSong++;

      if (iSong >= playlist.Count)
      {
        //	Is last element of video stacking playlist?
        if (_currentPlayList == PlayListType.PLAYLIST_VIDEO_TEMP)
        {
          return null;
        }

        if (!_repeatPlayList)
        {
          return null;
        }

        iSong = 0;
      }

      PlayListItem item = playlist[iSong];
      return item;
    }

    public string GetNext()
    {
      PlayListItem resultingItem = GetNextItem();
      if (resultingItem != null)
      {
        return resultingItem.FileName;
      }
      else
      {
        return string.Empty;
      }
    }

    public void PlayNext()
    {
      if (_currentPlayList == PlayListType.PLAYLIST_NONE)
      {
        return;
      }

      PlayList playlist = GetPlaylist(_currentPlayList);
      if (playlist.Count <= 0)
      {
        return;
      }
      int iSong = _currentItem;
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

      if (!Play(iSong))
      {
        if (!g_Player.Playing)
        {
          PlayNext();
        }
      }
    }

    public void PlayPrevious()
    {
      if (_currentPlayList == PlayListType.PLAYLIST_NONE)
      {
        return;
      }

      PlayList playlist = GetPlaylist(_currentPlayList);
      if (playlist.Count <= 0)
      {
        return;
      }
      int iSong = _currentItem;
      iSong--;
      if (iSong < 0)
      {
        iSong = playlist.Count - 1;
      }

      if (!Play(iSong))
      {
        if (!g_Player.Playing)
        {
          PlayPrevious();
        }
      }
    }

    public void Play(string filename)
    {
      if (_currentPlayList == PlayListType.PLAYLIST_NONE)
      {
        return;
      }

      PlayList playlist = GetPlaylist(_currentPlayList);
      for (int i = 0; i < playlist.Count; ++i)
      {
        PlayListItem item = playlist[i];
        if (item.FileName.Equals(filename))
        {
          Play(i);
          return;
        }
      }
    }

    public bool Play(int iSong)
    {
      // if play returns false PlayNext is called but this does not help against selecting an invalid track
      bool skipmissing = false;
      do
      {
        if (_currentPlayList == PlayListType.PLAYLIST_NONE)
        {
          Log.Debug("PlaylistPlayer.Play() no playlist selected");
          return false;
        }
        PlayList playlist = GetPlaylist(_currentPlayList);
        if (playlist.Count <= 0)
        {
          Log.Debug("PlaylistPlayer.Play() playlist is empty");
          return false;
        }
        if (iSong < 0)
        {
          iSong = 0;
        }
        if (iSong >= playlist.Count)
        {
          if (skipmissing)
          {
            return false;
          }
          else
          {
            if (_entriesNotFound < playlist.Count)
            {
              iSong = playlist.Count - 1;
            }
            else
            {
              return false;
            }
          }
        }

        //int previousItem = _currentItem;
        _currentItem = iSong;
        PlayListItem item = playlist[_currentItem];

        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS, 0, 0, 0, _currentItem, 0, null);
        msg.Label = item.FileName;
        GUIGraphicsContext.SendMessage(msg);

        if (playlist.AllPlayed())
        {
          playlist.ResetStatus();
        }

        //Log.Debug("PlaylistPlayer: Play - {0}", item.FileName);
        if (item.Type == PlayListItem.PlayListItemType.Radio)
        {
          msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_TUNE_RADIO, 0, 0, 0, 0, 0, null);
          msg.Label = item.Description;
          GUIGraphicsContext.SendMessage(msg);
          item.Played = true;
          return true;
        }

        bool playResult = false;
        if (_currentPlayList == PlayListType.PLAYLIST_MUSIC_VIDEO)
        {
          playResult = g_Player.PlayVideoStream(item.FileName, item.Description);
        }
        else if (item.Type == PlayListItem.PlayListItemType.AudioStream) // Internet Radio
        {
          playResult = g_Player.PlayAudioStream(item.FileName);
        }
        else
        {
          playResult = g_Player.Play(item.FileName);
        }
        if (!playResult)
        {
          //	Count entries in current playlist
          //	that couldn't be played
          _entriesNotFound++;
          Log.Error("PlaylistPlayer: *** unable to play - {0} - skipping track!", item.FileName);

          // do not try to play the next movie or internetstream in the list
          if (Util.Utils.IsVideo(item.FileName) || Util.Utils.IsLastFMStream(item.FileName))
          {
            skipmissing = false;
          }
          else
          {
            skipmissing = true;
          }

          iSong++;
        }
        else
        {
          item.Played = true;
          skipmissing = false;
          if (Util.Utils.IsVideo(item.FileName))
          {
            if (g_Player.HasVideo)
            {
              g_Player.ShowFullScreenWindow();
            }
          }
        }
      } while (skipmissing);
      return g_Player.Playing;
    }

    public int CurrentSong
    {
      get { return _currentItem; }
      set
      {
        if (value >= -1 && value < GetPlaylist(CurrentPlaylistType).Count)
        {
          _currentItem = value;
        }
      }
    }

    public string CurrentPlaylistName
    {
      get { return _currentPlaylistName; }
      set { _currentPlaylistName = value; }
    }

    public void Remove(PlayListType type, string filename)
    {
      PlayList playlist = GetPlaylist(type);
      int itemRemoved = playlist.Remove(filename);
      if (type != CurrentPlaylistType)
      {
        return;
      }
      if (_currentItem >= itemRemoved)
      {
        _currentItem--;
      }
    }

    public PlayListType CurrentPlaylistType
    {
      get { return _currentPlayList; }
      set
      {
        if (_currentPlayList != value)
        {
          _currentPlayList = value;
          _entriesNotFound = 0;
        }
      }
    }

    public PlayList GetPlaylist(PlayListType nPlayList)
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
        case PlayListType.PLAYLIST_MUSIC_VIDEO:
          return _musicVideoPlayList;
        case PlayListType.PLAYLIST_RADIO_STREAMS:
          return _radioStreamPlayList;
        default:
          _emptyPlayList.Clear();
          return _emptyPlayList;
      }
    }

    public int RemoveDVDItems()
    {
      int removedDvdItems = _musicPlayList.RemoveDVDItems();
      _tempMusicPlayList.RemoveDVDItems();
      int removedVideoItems = _videoPlayList.RemoveDVDItems();
      _tempVideoPlayList.RemoveDVDItems();

      return removedDvdItems + removedVideoItems;
    }

    public void Reset()
    {
      _currentItem = -1;
      _entriesNotFound = 0;
    }

    public int EntriesNotFound
    {
      get { return _entriesNotFound; }
    }

    public bool RepeatPlaylist
    {
      get { return _repeatPlayList; }
      set { _repeatPlayList = value; }
    }
  }
}
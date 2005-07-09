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
      , PLAYLIST_MUSIC		
      , PLAYLIST_MUSIC_TEMP
      , PLAYLIST_VIDEO		
      , PLAYLIST_VIDEO_TEMP
    };
    static int				  m_iEntriesNotFound=0;
    static bool				  m_bChanged=false;
    static int				  m_iCurrentSong=-1;
    static PlayListType	m_iCurrentPlayList=PlayListType.PLAYLIST_NONE;
    static PlayList		  m_PlaylistMusic=new PlayList();
    static PlayList		  m_PlaylistMusicTemp=new PlayList();
    static PlayList		  m_PlaylistVideo=new PlayList();
    static PlayList		  m_PlaylistVideoTemp=new PlayList();
    static PlayList		  m_PlaylistEmpty=new PlayList();
    static bool         repeatPlaylist=true;

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
					PlayList.PlayListItem item=GetCurrentItem();
					if (item!=null)
					{
						if (item.Type==PlayList.PlayListItem.PlayListItemType.Radio ||
							item.Type==PlayList.PlayListItem.PlayListItemType.AudioStream)
						{
							
							return;
						}
					}
          Reset();
          m_iCurrentPlayList=PlayListType.PLAYLIST_NONE;
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
          Log.Write("Playlistplayer.StartFile({0})",message.Label);
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
          Log.Write("Playlistplayer.SeekPercent({0}%)",message.Param1);
          g_Player.SeekAsolutePercentage(message.Param1);
          Log.Write("Playlistplayer.SeekPercent({0}%) done",message.Param1);
        }
        break;
				case GUIMessage.MessageType.GUI_MSG_SEEK_FILE_END:
				{
					double duration=g_Player.Duration;
					double position=g_Player.CurrentPosition;
					if (position < duration-1d)
					{
						Log.Write("Playlistplayer.SeekEnd({0})",duration);
						g_Player.SeekAbsolute(duration-0.5d);
						Log.Write("Playlistplayer.SeekEnd({0}) done",g_Player.CurrentPosition);
					}
				}
					break;
      }
    }

    static public string Get(int iSong)
    {
      if (m_iCurrentPlayList==PlayListType.PLAYLIST_NONE) return String.Empty;

      PlayList playlist = GetPlaylist(m_iCurrentPlayList);
      if (playlist.Count <= 0) return String.Empty;
      
      if (iSong >= playlist.Count )
      {
        //	Is last element of video stacking playlist?
        if (m_iCurrentPlayList==PlayListType.PLAYLIST_VIDEO_TEMP)
        {
          return String.Empty;
        }

          if (!repeatPlaylist)
          {
            return  String.Empty;;
          }
        iSong=0;
      }

      PlayList.PlayListItem item = playlist[iSong];
      return item.FileName;
    }

    static public PlayList.PlayListItem GetCurrentItem()
    {
      if (m_iCurrentSong<0) return null;

      PlayList playlist = GetPlaylist(m_iCurrentPlayList);
      if (playlist == null) return null;

      return playlist[m_iCurrentSong];
    }

    static public string GetNext()
    {
      if (m_iCurrentPlayList==PlayListType.PLAYLIST_NONE) return String.Empty;

      PlayList playlist = GetPlaylist(m_iCurrentPlayList);
      if (playlist.Count <= 0) return String.Empty;
      int iSong=m_iCurrentSong;
      iSong++;

      if (iSong >= playlist.Count )
      {
        //	Is last element of video stacking playlist?
        if (m_iCurrentPlayList==PlayListType.PLAYLIST_VIDEO_TEMP)
        {
          return String.Empty;
        }

          if (!repeatPlaylist)
          {
            return  String.Empty;;
          }
        iSong=0;
      }

      PlayList.PlayListItem item = playlist[iSong];
      return item.FileName;
    }

    static public void	PlayNext(bool bAutoPlay)
    {
      if (m_iCurrentPlayList==PlayListType.PLAYLIST_NONE) return;

      PlayList playlist = GetPlaylist(m_iCurrentPlayList);
      if (playlist.Count <= 0) return;
      int iSong=m_iCurrentSong;
      iSong++;

      if (iSong >= playlist.Count )
      {
        //	Is last element of video stacking playlist?
        if (m_iCurrentPlayList==PlayListType.PLAYLIST_VIDEO_TEMP)
        {
          //	Disable playlist playback
          m_iCurrentPlayList=PlayListType.PLAYLIST_NONE;
          return;
        }

          if (!repeatPlaylist)
          {
            m_iCurrentPlayList=PlayListType.PLAYLIST_NONE;
            return;
          }
        iSong=0;
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

    static public void			PlayPrevious()
    {
      if (m_iCurrentPlayList==PlayListType.PLAYLIST_NONE)
        return;

      PlayList playlist = GetPlaylist(m_iCurrentPlayList);
      if (playlist.Count <= 0) return;
      int iSong=m_iCurrentSong;
      iSong--;
      if (iSong < 0)
        iSong=playlist.Count-1;

      Play(iSong);
			if (!g_Player.Playing)
			{
				PlayPrevious();
			}
		}

    static public void Play(string filename)
    {
      if (m_iCurrentPlayList==PlayListType.PLAYLIST_NONE)
        return;

      PlayList playlist = GetPlaylist(m_iCurrentPlayList);
      if (playlist.Count <= 0) return;
      for (int i=0; i < playlist.Count; ++i)
      {
        PlayList.PlayListItem item=playlist[i];
        if (item.FileName.Equals(filename))
        {
          Play(i);
          return;
        }
      }
    }

    static public void Play(int iSong)
    {
      if (m_iCurrentPlayList==PlayListType.PLAYLIST_NONE)
        return;

      PlayList playlist = GetPlaylist(m_iCurrentPlayList);
      if (playlist.Count <= 0) return;
      if (iSong < 0) iSong=0;
      if (iSong >= playlist.Count ) iSong=playlist.Count-1;

      m_bChanged=true;
      int iPreviousSong=m_iCurrentSong;
      m_iCurrentSong=iSong;
      PlayList.PlayListItem item=playlist[m_iCurrentSong];

      if (playlist.AllPlayed() )
      {
        playlist.ResetStatus();
      }

			Log.Write("PlaylistPlayer.Play({0})",item.FileName);
			if (item.Type == PlayList.PlayListItem.PlayListItemType.Radio)
			{		
				GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_TUNE_RADIO,0,0,0,0,0,null);
				msg.Label=item.Description;
				GUIGraphicsContext.SendMessage(msg);
				item.Played=true;
				return;
			}

      if(!g_Player.Play(item.FileName))
      {
        //	Count entries in current playlist
        //	that couldn't be played
        m_iEntriesNotFound++;
      }
      else
      {
        item.Played=true;
        if (Utils.IsVideo(item.FileName))
        {
					if (g_Player.HasVideo)
					{
						GUIGraphicsContext.IsFullScreenVideo=true;
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
					}
        }
      }
    }

    static public int	CurrentSong 
    {
      get { return m_iCurrentSong;}
      set { 
        if (value >= -1 && value < GetPlaylist( CurrentPlaylist).Count ) 
          m_iCurrentSong=value;
      }
    }

    static public void Remove(PlayListType type, string filename)
    {
      PlayList playlist=GetPlaylist(type);
      int itemremoved=playlist.Remove(filename);
      if (type!=CurrentPlaylist)
      {
        return;
      }
      if (m_iCurrentSong>=itemremoved) m_iCurrentSong--;
    }

    static public bool			HasChanged
    {
      get { 
        bool bResult=m_bChanged;
        m_bChanged=false;
        return bResult;
      }
    }

    static public Playlists.PlayListPlayer.PlayListType CurrentPlaylist
    {
      get { return m_iCurrentPlayList; }
      set 
      { 
        if (m_iCurrentPlayList != value)
        {
          m_iCurrentPlayList=value ;
          m_iEntriesNotFound=0;
          m_bChanged=true;
          using (MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
          {
            if (value==PlayListType.PLAYLIST_MUSIC||value==PlayListType.PLAYLIST_MUSIC_TEMP)
            {
              repeatPlaylist=xmlreader.GetValueAsBool("musicfiles","repeat",true);
            }
            else
            {
              repeatPlaylist=xmlreader.GetValueAsBool("movies","repeat",true);
            }
          }
        }
      }
    }

    static public PlayList	GetPlaylist( Playlists.PlayListPlayer.PlayListType nPlayList)
    {
      switch ( nPlayList )
      {
        case PlayListType.PLAYLIST_MUSIC:
          return m_PlaylistMusic;
          
        case PlayListType.PLAYLIST_MUSIC_TEMP:
          return m_PlaylistMusicTemp;
        case PlayListType.PLAYLIST_VIDEO:
          return m_PlaylistVideo;
        case PlayListType.PLAYLIST_VIDEO_TEMP:
          return m_PlaylistVideoTemp;
        default:
          m_PlaylistEmpty.Clear();
          return m_PlaylistEmpty;
      }
    }

    static public int					RemoveDVDItems()
    {
      int nRemovedM=m_PlaylistMusic.RemoveDVDItems();
      m_PlaylistMusicTemp.RemoveDVDItems();
      int nRemovedV=m_PlaylistVideo.RemoveDVDItems();
      m_PlaylistVideoTemp.RemoveDVDItems();

      return nRemovedM+nRemovedV;

    }

    static public void        Reset()
    {
      m_iCurrentSong=-1;
      m_iEntriesNotFound=0;
    }
    
    static public int EntriesNotFound
    {
      get 
      {
        return m_iEntriesNotFound;
      }
    }
	}
}

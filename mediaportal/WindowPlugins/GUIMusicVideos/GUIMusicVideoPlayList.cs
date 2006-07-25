#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal;
using MediaPortal.GUI.Library;
using MediaPortal.MusicVideos.Database;
using MediaPortal.Player;

namespace MediaPortal.GUI.MusicVideos
{
  class MusicVideoPlaylist
  {
    private static MusicVideoPlaylist moPlayListInstance;
    private List<YahooVideo> moPlayList = new List<YahooVideo>();
    private int miPlayListIndex = -1;
    private string msCurrentUrl = "";
    private bool mbRepeat = false;
    private bool mbPLayingState = false;
    private MusicVideoPlaylist()
    {
      g_Player.PlayBackStopped += new g_Player.StoppedHandler(PlayerStopped);
      g_Player.PlayBackEnded += new g_Player.EndedHandler(SongEnded);
      //GUIWindowManager.
      //GUIWindowManager.


    }
    public static MusicVideoPlaylist getInstance()
    {

      if (moPlayListInstance == null)
      {
        moPlayListInstance = new MusicVideoPlaylist();
      }
      return moPlayListInstance;
    }
    public void SongEnded(g_Player.MediaType type, string filename)
    {
      Log.Write("MusicVideoPlaylist - EventArgs song ended received");
      if (mbPLayingState && msCurrentUrl.Equals(filename))
      {
        PlayNext();
        //Action action = new Action();
        //action.wID = Action.ActionType.ACTION_NEXT_ITEM;
        //action.fAmount1 = 0;
        //action.fAmount2 = 0;
        //GUIWindowManager.GetWindow(4734).OnAction(action);

      }
    }

    public void PlayerStopped(g_Player.MediaType type, int stoptime, string filename)
    {
      Log.Write("Playing  {0}",g_Player.CurrentFile);
      Log.Write("PlayList {0}",miPlayListIndex);
      Log.Write("MusicVideoPlaylist - EventArgs stop play received");
      mbPLayingState = false;
    }
    public void AddAllToPlayList(List<YahooVideo> foVideoList)
    {
      moPlayList.AddRange(foVideoList);
    }
    public void AddToPlayList(YahooVideo foVideo)
    {
      moPlayList.Add(foVideo);
    }
    public void Play()
    {
      Log.Write("MusicVideoPlaylist - Play()");
      if (isPlayListLoaded())
      {
        if (miPlayListIndex == -1)
        {
          if (mbPLayingState == false)
          {
            //miPlayListIndex =
            if (getNextSongIndex() == false)
            {
              return;
            }
          }
          else
          {
            return;
          }
        }

        YahooUtil loUtil = YahooUtil.getInstance();
        YahooVideo loVideo = moPlayList[miPlayListIndex];
        //Log.Write("{0}",video.countryId);
        YahooSite loSite = loUtil.getYahooSiteById(loVideo.countryId);
        
        YahooSettings loSetting = YahooSettings.getInstance();
        string lsVideoLink = loUtil.getVideoMMSUrl(loVideo, loSetting.msDefaultBitRate);
        
        lsVideoLink = lsVideoLink.Substring(0, lsVideoLink.Length - 2) + "&txe=.wmv";
        if (loSetting.mbUseVMR9)
        {
            g_Player.PlayVideoStream(lsVideoLink);
        }
        else
        {
            g_Player.PlayAudioStream(lsVideoLink);
        }
        //g_Player.Play(lsVideoLink);
        //g_Player.
        //g_Player.FullScreen = true;
        GUIGraphicsContext.IsFullScreenVideo = true;
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
        mbPLayingState = true;
        Log.Write("Now Playing index:{0}", miPlayListIndex);
        msCurrentUrl = lsVideoLink;
        Log.Write("1 - {0}",GUIPropertyManager.GetProperty("#Play.Current.Title"));
        Log.Write("2 - {0}",GUIPropertyManager.GetProperty("#Play.Current.File"));
        //GUIPropertyManager.SetProperty("#Play.Current.Title", loVideo.songName);
        //GUIPropertyManager.SetProperty("#Play.Current.File", loVideo.songName);
      }
    }
    public void Stop()
    {
      g_Player.Stop();
      mbPLayingState = false;
    }
    public void Play(int fiPlaylistIndex)
    {
      Log.Write("MusicVideoPlaylist - Play(int)");
      if (fiPlaylistIndex > -1 && fiPlaylistIndex < moPlayList.Count)
      {
        miPlayListIndex = fiPlaylistIndex;
        Play();
      }
    }
    public void Clear()
    {
      moPlayList.Clear();
      miPlayListIndex = -1;
      mbPLayingState = false;
    }
    public void PlayNext()
    {
      Log.Write("MusicVideoPlaylist - PlayNext()");
      //Log.Write("",isPlayListLoaded(),);
      if (isPlayListLoaded() && mbPLayingState)
      {

        if (getNextSongIndex())
        {
          Play();
        }
        //if (miPlayListIndex == -1)
        //{
        //    return;
        // }
        // Play();
      }
    }
    public void PlayPrevious()
    {
      Log.Write("MusicVideoPlaylist - PlayPrevious()");
      if (isPlayListLoaded() && mbPLayingState)
      {
        if (getPreviousSongIndex())
        {
          Play();
        }
        //if (miPlayListIndex == -1)
        //{
        //    return;
        //}
        //Play();
      }
    }
    public bool isPlayListLoaded()
    {
      return moPlayList.Count > 0;
    }
    private bool getNextSongIndex()
    {
       Log.Write("GetNextSongIndex - current playing index:{0}", miPlayListIndex);
      //Log.Write("PlayList Count:{0}", moPlayList.Count);
      if (miPlayListIndex >= -1 && miPlayListIndex < moPlayList.Count - 1)
      {
        miPlayListIndex++;
        return true;
      }
      else if (miPlayListIndex == moPlayList.Count - 1 && mbRepeat)
      {
        miPlayListIndex = 0;
        return true;
      }
      return false;


      //if (miPlayListIndex >= moPlayList.Count)
      //{
      //    if (mbRepeat)
      //    {
      //        miPlayListIndex = 0;
      //    }
      //    else
      //    {
      //        miPlayListIndex = -1;
      //    }
      //}
      //return miPlayListIndex;         

    }
    private bool getPreviousSongIndex()
    {
      Log.Write("GetPreviousSongIndex - current playing index:{0}", miPlayListIndex);
      //Log.Write("PlayList Count:{0}", moPlayList.Count);
      if (miPlayListIndex > 0 && miPlayListIndex < moPlayList.Count)
      {

        miPlayListIndex--;
        return true;
      }
      else if (miPlayListIndex == 0 && mbRepeat)
      {
        miPlayListIndex = moPlayList.Count - 1;
        return true;
      }
      else
      {
        return false;
      }

    }
    public List<YahooVideo> getPlayListVideos()
    {
      return moPlayList;
    }
    public void repeat(bool fbRepeat)
    {
      mbRepeat = fbRepeat;
    }
    public bool getRepeatState()
    {
      return mbRepeat;
    }
    public void shuffle()
    {

      if (moPlayList == null || moPlayList.Count == 0) { return; }
      if (mbPLayingState)
      {

      }
      int count = moPlayList.Count;
      int liPos;
      Random r = new System.Random(DateTime.Now.Millisecond);
      //bool lbCurPlayIndexFound = false;
      YahooVideo loCurVideo = null;
      if (miPlayListIndex > -1)
      {
        loCurVideo = moPlayList[miPlayListIndex];
      }


      for (int i = 0; i < count; i++)
      {
        YahooVideo loVideo = moPlayList[i];
        moPlayList.RemoveAt(i);
        liPos = r.Next(count);
        //if(i==miPlayListIndex && !lbCurPlayIndexFound){
        //    Log.Write("Playing song index changing from {0} to {1}",miPlayListIndex,liPos);
        //    miPlayListIndex = liPos;
        //    lbCurPlayIndexFound = true;
        //}
        moPlayList.Insert(liPos, loVideo);
      }
      if (miPlayListIndex > -1)
      {
        liPos = moPlayList.IndexOf(loCurVideo);
        //Log.Write("Playing song index changing from {0} to {1}", miPlayListIndex, liPos);
        miPlayListIndex = liPos;
      }

    }
    public int getPlayListIndex()
    {
      //Log.Write("MusicVideoPlaylist - getPlayListIndex()");
      return miPlayListIndex;
    }
        public YahooVideo getCurrentPlayingVideo()
        {
            if (mbPLayingState)
            {
                return moPlayList[miPlayListIndex];
            }
            return null;
        }
        public bool isPlaying()
        {
            return mbPLayingState;
        }

    }
}

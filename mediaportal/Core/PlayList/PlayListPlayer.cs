/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Player;
using MediaPortal.Playlists;
namespace MediaPortal.Playlists
{
    public static class PlayListPlayer
    {
        static NonStaticPlayListPlayer myPlayer = new NonStaticPlayListPlayer();

        static public void Init()
        {
            myPlayer.Init();
        }

        static public void OnMessage(GUIMessage message)
        {
            myPlayer.OnMessage(message);
        }

        static public string Get(int iSong)
        {
            return myPlayer.Get(iSong);
        }

        static public PlayListItem GetCurrentItem()
        {
            return myPlayer.GetCurrentItem();
        }

        static public string GetNext()
        {
            return myPlayer.GetNext();
        }

        static public void PlayNext(bool bAutoPlay)
        {
            myPlayer.PlayNext(bAutoPlay);
        }

        static public void PlayPrevious()
        {
            myPlayer.PlayPrevious();
        }

        static public void Play(string filename)
        {
            myPlayer.Play(filename);
        }

        static public void Play(int iSong)
        {
            myPlayer.Play(iSong);
        }

        static public int CurrentSong
        {
            get { return myPlayer.CurrentSong; }
            set { myPlayer.CurrentSong = value; }
        }

        static public void Remove(PlayListType type, string filename)
        {
            myPlayer.Remove(type, filename);
        }

        static public bool HasChanged
        {
            get { return myPlayer.HasChanged; }
        }

        static public PlayListType CurrentPlaylist
        {
            get { return myPlayer.CurrentPlaylistType; }
            set { myPlayer.CurrentPlaylistType = value; }
        }

        static public PlayList GetPlaylist(PlayListType nPlayList)
        {
            return myPlayer.GetPlaylist(nPlayList);
        }

        static public int RemoveDVDItems()
        {
            return myPlayer.RemoveDVDItems();
        }

        static public void Reset()
        {
            myPlayer.Reset();
        }

        static public int EntriesNotFound
        {
            get { return myPlayer.EntriesNotFound; }
        }
    }
}

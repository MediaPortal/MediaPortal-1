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

using System;
using MediaPortal.Playlists;
using NUnit.Framework;

namespace MediaPortal.Tests.Core.Playlists
{
  [TestFixture]
  public class PlayListPlayerTest : PlayListPlayer.IPlayer
  {
    [SetUp]
    public void Init() {}

    [Test]
    public void InsertItemButNotStartPlayingGivesNull()
    {
      PlayListPlayer player = new PlayListPlayer();
      player.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
      PlayList playlist = player.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      PlayListItem item1 = new PlayListItem();
      playlist.Add(item1);
      Assert.IsNull(player.GetCurrentItem());
    }

    [Test]
    public void PlayMovesCurrentToItem()
    {
      PlayListPlayer player = new PlayListPlayer();
      player.g_Player = this; //fake g_Player
      player.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
      PlayList playlist = player.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      PlayListItem item1 = new PlayListItem();
      playlist.Add(item1);
      player.PlayNext();
      Assert.AreEqual(item1, player.GetCurrentItem());
      Assert.IsTrue(hasPlayBeenCalled);
    }

    [Test]
    public void GetNextReturnsFileName()
    {
      PlayListPlayer player = new PlayListPlayer();
      player.CurrentPlaylistType = PlayListType.PLAYLIST_MUSIC;
      PlayList playlist = player.GetPlaylist(PlayListType.PLAYLIST_MUSIC);
      PlayListItem item1 = new PlayListItem("apa", "c:\\apa.mp3");
      playlist.Add(item1);
      Assert.AreEqual("c:\\apa.mp3", player.GetNext());
    }

    #region IPlayer Members

    private bool hasPlayBeenCalled = false;

    public bool Playing
    {
      get { return true; }
    }

    public void Release()
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public bool Play(string strFile)
    {
      hasPlayBeenCalled = true;
      return true;
    }

    public void Stop()
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public void SeekAsolutePercentage(int iPercentage)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public double Duration
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    public double CurrentPosition
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    public void SeekAbsolute(double dTime)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public bool HasVideo
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    public bool PlayVideoStream(string strURL, string streamName)
    {
      hasPlayBeenCalled = true;
      return true;
    }

    public bool PlayAudioStream(string strURL)
    {
      hasPlayBeenCalled = true;
      return true;
    }

    public bool ShowFullScreenWindow()
    {
      return false;
    }

    #endregion
  }
}
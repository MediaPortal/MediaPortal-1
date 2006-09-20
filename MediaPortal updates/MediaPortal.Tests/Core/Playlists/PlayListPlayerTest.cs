using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.Playlists;

namespace MediaPortal.Tests.Core.Playlists
{
  [TestFixture]
  public class PlayListPlayerTest : PlayListPlayer.IPlayer
  {
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

    #endregion
  }
}

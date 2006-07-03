using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.Playlists;

namespace MediaPortal.Tests.Core.Playlists
{
  [TestFixture]
  public class PlayListTest
  {
    [Test]
    public void NewlyAddedSongsAreNotMarkedPlayed()
    {
      PlayList pl = new PlayList();
      PlayListItem item = new PlayListItem("my song", "myfile.mp3");
      pl.Add(item);

      Assert.IsFalse(pl.AllPlayed());
    }

    [Test]
    public void RemoveReallyRemovesASong()
    {
      PlayList pl = new PlayList();
      PlayListItem item = new PlayListItem("my song", "myfile.mp3");
      pl.Add(item);

      pl.Remove("myfile.mp3");

      Assert.AreEqual(0, pl.Count);
    }

    [Test]
    public void AllPlayedReturnsTrueWhenAllArePlayed()
    {
      PlayList pl = new PlayList();
      PlayListItem item = new PlayListItem("my song", "myfile.mp3");
      item.Played = true;
      pl.Add(item);

      item = new PlayListItem("my 2:d song", "myfile2.mp3");
      item.Played = true;
      pl.Add(item);

      Assert.IsTrue(pl.AllPlayed());
    }

    [Test]
    public void ResetSetsAllItemsToFalse()
    {
      PlayList pl = new PlayList();
      PlayListItem item = new PlayListItem("my song", "myfile.mp3");
      pl.Add(item);

      PlayListItem item2 = new PlayListItem("my 2:d song", "myfile2.mp3");
      pl.Add(item2);

      pl[0].Played = true;
      pl[1].Played = true;

      pl.ResetStatus();

      Assert.IsFalse(pl[0].Played);
      Assert.IsFalse(pl[1].Played);
    }
  }
}

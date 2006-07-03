using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.Playlists;

namespace MediaPortal.Tests.Core.Playlists
{
  [TestFixture]
  public class PlayListWPLTest
  {
    [Test]
    public void LoadTest()
    {
      PlayList playlist = new PlayList();
      IPlayListIO loader = new PlayListWPLIO();
      Assert.IsTrue(loader.Load(playlist,"Core\\Playlists\\TestData\\exampleList.wpl"));

      string lastName = playlist[playlist.Count - 1].FileName;
      Assert.IsTrue(playlist[0].FileName.EndsWith("01-chant_down_babylon-rev.mp3"));
      Assert.IsTrue(playlist[1].FileName.EndsWith("06-blackman_redemption-rev.mp3"));
      Assert.IsTrue(lastName.EndsWith("satisfy_my_soul_babe_(version)-just.mp3"));
    }
  }
}

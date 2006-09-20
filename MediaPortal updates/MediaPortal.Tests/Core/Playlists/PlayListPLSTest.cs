using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.Playlists;

namespace MediaPortal.Tests.Core.Playlists
{
  [TestFixture]
  public class PlayListPLSTest
  {
    [Test]
    public void LoadPLS()
    {
      PlayList playlist = new PlayList();
      IPlayListIO loader = new PlayListPLSIO();
      Assert.IsTrue(loader.Load(playlist, "Core\\Playlists\\TestData\\exampleList.pls"));
      Assert.AreEqual(@"E:\Program Files\Winamp3\demo.mp3", playlist[0].FileName);
      Assert.AreEqual(@"E:\Program Files\Winamp3\demo2.mp3", playlist[1].FileName);
      Assert.AreEqual(2, playlist.Count);
    }
  }
}

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.Playlists;
using System.IO;

namespace MediaPortal.Tests.Core.Playlists
{
  [TestFixture]
  public class PlayListB4STest
  {
    [Test]
    public void LoadB4S()
    {
      PlayList playlist = new PlayList();
      IPlayListIO loader = new PlayListB4sIO();
      Assert.IsTrue(loader.Load(playlist, "Core\\Playlists\\TestData\\exampleList.b4s"));
      Assert.AreEqual(@"E:\Program Files\Winamp3\demo.mp3", playlist[0].FileName);
      Assert.AreEqual(@"E:\Program Files\Winamp3\demo2.mp3", playlist[1].FileName);
      Assert.AreEqual(2, playlist.Count);
    }

    [Test]
    public void SaveB4s()
    {
      PlayList playlist = new PlayList();
      IPlayListIO saver = new PlayListB4sIO();
      playlist.Add(new PlayListItem("mytuneMp3", "mytune.mp3"));
      playlist.Add(new PlayListItem("mytuneOgg", "mytune.ogg", 123));
      playlist.Add(new PlayListItem("mytuneWav", "mytune.wav"));
      playlist.Add(new PlayListItem("mytuneWav", "mytune.wav", 666));
      saver.Save(playlist,"test.b4s");

      string newXml;
      string oldXml;
      using (StreamReader reader = new StreamReader("test.b4s"))
      {
        newXml = reader.ReadToEnd();
      }

      using (StreamReader reader = new StreamReader("Core\\Playlists\\TestData\\testSave.b4s"))
      {
        oldXml = reader.ReadToEnd();
      }

      Assert.AreEqual(oldXml, newXml);
    }
  }
}

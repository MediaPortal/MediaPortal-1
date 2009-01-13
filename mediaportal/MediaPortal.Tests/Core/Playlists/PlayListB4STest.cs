#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

using System.IO;
using MediaPortal.Playlists;
using NUnit.Framework;

namespace MediaPortal.Tests.Core.Playlists
{
  [TestFixture]
  public class PlayListB4STest
  {
    [SetUp]
    public void Init()
    {
    }

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
      saver.Save(playlist, "test.b4s");

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
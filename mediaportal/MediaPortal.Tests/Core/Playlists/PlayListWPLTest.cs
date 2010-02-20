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

using MediaPortal.Playlists;
using NUnit.Framework;

namespace MediaPortal.Tests.Core.Playlists
{
  [TestFixture]
  public class PlayListWPLTest
  {
    [SetUp]
    public void Init() {}

    [Test]
    public void LoadTest()
    {
      PlayList playlist = new PlayList();
      IPlayListIO loader = new PlayListWPLIO();
      Assert.IsTrue(loader.Load(playlist, "Core\\Playlists\\TestData\\exampleList.wpl"));

      string lastName = playlist[playlist.Count - 1].FileName;
      Assert.IsTrue(playlist[0].FileName.EndsWith("01-chant_down_babylon-rev.mp3"));
      Assert.IsTrue(playlist[1].FileName.EndsWith("06-blackman_redemption-rev.mp3"));
      Assert.IsTrue(lastName.EndsWith("satisfy_my_soul_babe_(version)-just.mp3"));
    }
  }
}
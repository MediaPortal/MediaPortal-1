#region Copyright (C) 2005-2016 Team MediaPortal

// Copyright (C) 2005-2016 Team MediaPortal
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

using System.Threading;
using System.Windows.Forms;

using MediaPortal.Player;

using NUnit.Framework;

namespace MediaPortal.Tests.Core.Player
{
  [TestFixture, RequiresSTA]
  public class DvdPlayer9Tests
  {
    [Test, Explicit]
    [TestCase(@"H:\Work\My\Video\Убить дракона\VIDEO_TS\VIDEO_TS.IFO")]
    [TestCase(@"J:\Test_DVD\Delfinariy.Koktebel.2009.DVD5\VIDEO_TS\VIDEO_TS.IFO")]
    [TestCase(@"J:\Test_DVD\Video1\VIDEO_TS\VIDEO_TS.IFO")]
    public void SimpleVideoFile1Test(string path)
    {
      using (var form = new Form { Size = new System.Drawing.Size(100, 100), Visible = false })
      {
        using (new DirectShowPlayerTestHelper(form))
        {
          using (var player = new DVDPlayer9())
          {
            Assert.IsTrue(player.Play(path));
            Application.DoEvents();
            Thread.Sleep(1000);
            Application.DoEvents();
            Assert.IsNotNull(player.CurrentVideo);
            Assert.IsNotNull(player.BestVideo);
            Assert.AreEqual(1, player.VideoStreams);
            Assert.AreEqual(VideoCodec.V_MPEG2, player.CurrentVideo.Codec);
            Assert.AreEqual(720, player.CurrentVideo.Width);
            Assert.AreEqual("576P", player.CurrentVideo.Resolution);
            Assert.GreaterOrEqual(player.AudioStreams, 1);
            Assert.GreaterOrEqual(player.SubtitleStreams, 0);
          }
        }
      }
    }
  }
}
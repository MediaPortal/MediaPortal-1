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
  public class VideoPlayerVmrTests
  {
    [Test, Explicit]
    [TestCase(@"H:\Work\My\Video\Quantum Levitation.mp4")]
    [TestCase(@"C:\Users\1\My Documents\3DNews Daily 499 Motorola обновила Moto 360, а Huawei взвесила апельсин на смартфоне Mate S.mp4")]
    public void SimpleVideoFile1Test(string path)
    {
      using (var form = new Form { Size = new System.Drawing.Size(100, 100), Visible = false })
      {
        using (new DirectShowPlayerTestHelper(form))
        {
          using (var player = new VideoPlayerVMR9())
          {
            Assert.IsTrue(player.Play(path));
            Application.DoEvents();
            Thread.Sleep(1000);
            Application.DoEvents();
            Assert.IsNotNull(player.CurrentVideo);
            Assert.IsNotNull(player.BestVideo);
            Assert.AreEqual(1, player.VideoStreams);
            Assert.AreEqual(1, player.AudioStreams);
            Assert.AreEqual(0, player.SubtitleStreams);
            Assert.IsNotNull(player.CurrentAudio);
          }
        }
      }
    }

    [Test, Explicit]
    [TestCase(@"E:\Clips\Game trailers\Silicon Studio New Rendering Engine - Tech Demo Preview.mp4")]
    [TestCase(@"E:\Clips\Game trailers\The Witcher 3 Wild Hunt - 35min gameplay demo.mp4")]
    [TestCase(@"E:\Clips\Game trailers\Лучший мир танков [Tрейлер].mp4")]
    [TestCase(@"E:\Clips\Game trailers\Command Shooter\Battlefield 4\Battlefield 4 Through My Eyes - Cinematic Movie.mp4")]
    public void SimpleVideoFile2Test(string path)
    {
      using (var form = new Form { Size = new System.Drawing.Size(100, 100), Visible = false })
      {
        using (new DirectShowPlayerTestHelper(form))
        {
          using (var player = new VideoPlayerVMR9())
          {
            Assert.IsTrue(player.Play(path));
            Application.DoEvents();
            Thread.Sleep(1000);
            Application.DoEvents();
            Assert.IsNotNull(player.CurrentVideo);
            Assert.IsNotNull(player.BestVideo);
            Assert.AreEqual(1, player.VideoStreams);
            Assert.AreEqual(1, player.AudioStreams);
            Assert.AreEqual(0, player.SubtitleStreams);
            Assert.IsNotNull(player.CurrentAudio);
          }
        }
      }
    }

    [Test, Explicit]
    [TestCase(@"H:\Work\My\Video\South.Pacific.E01.Ocean.of.Islands.2009.720p.mkv")]
    public void ComplexVideoFile1Test(string path)
    {
      using (var form = new Form { Size = new System.Drawing.Size(100, 100), Visible = false })
      {
        using (new DirectShowPlayerTestHelper(form))
        {
          using (var player = new VideoPlayerVMR9())
          {
            Assert.IsTrue(player.Play(path));
            Application.DoEvents();
            Thread.Sleep(1000);
            Application.DoEvents();
            Assert.IsNotNull(player.CurrentVideo);
            Assert.IsNotNull(player.BestVideo);
            Assert.GreaterOrEqual(player.VideoStreams, 1);
            Assert.GreaterOrEqual(player.AudioStreams, 1);
            Assert.GreaterOrEqual(player.SubtitleStreams, 0);
            Assert.IsNotNull(player.CurrentAudio);
          }
        }
      }
    }

    [Test, Explicit]
    [TestCase(@"I:\Users\Край-001.mkv")]
    public void ComplexVideoFile2Test(string path)
    {
      using (var form = new Form { Size = new System.Drawing.Size(100, 100), Visible = false })
      {
        using (new DirectShowPlayerTestHelper(form))
        {
          using (var player = new VideoPlayerVMR9())
          {
            Assert.IsTrue(player.Play(path));
            Application.DoEvents();
            Thread.Sleep(1000);
            Application.DoEvents();
            Assert.IsNotNull(player.CurrentVideo);
            Assert.IsNotNull(player.BestVideo);
            Assert.GreaterOrEqual(player.VideoStreams, 1);
            Assert.GreaterOrEqual(player.AudioStreams, 1);
            Assert.GreaterOrEqual(player.SubtitleStreams, 0);
            Assert.IsNotNull(player.CurrentAudio);
          }
        }
      }
    }
  }
}
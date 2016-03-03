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

using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Player.MediaInfo;

using NUnit.Framework;

namespace MediaPortal.Tests.Core.Player
{
    [TestFixture, RequiresSTA]
    public class AudioPlayerVMRTests
    {
        [Test, Explicit]
        [TestCase(@"E:\Music\Anugama\Healing\01 - Healing Earth.flac")]
        [TestCase(@"E:\Music\Angelight\Intimland vol 1 Прикосновение\Angelight.-.[Intimland.vol.1].ape")]
        [TestCase(@"E:\Music\Blackmore's Night\Blackmore's_Night_1997_Shadow_Of_The_Moon_[Japan_BVCP-6022]_eac.wv.log.cue.covers.tags.iso.wv")]
        [TestCase(@"E:\Music\DJ Romeo\VIP Mix [04.2007]\[DJ Romeo] Track 1.mp3")]
        [TestCase(@"H:\Work\My\Music\Daft Punk - The Grid (From Tron Legacy).mp3")]
        [TestCase(@"H:\Work\My\Music\Diablo III\01-ost--and_the_heavens_shall_tremble-oma.mp3")]
        public void SimpleMusicFile1Test(string path)
        {
            using (var form = new Form()
            {
                Size = new System.Drawing.Size(10, 10),
                Visible = false
            })
            {
                GUIGraphicsContext.form = form;
                using (var player = new AudioPlayerVMR7())
                {
                    player.Play(path);
                    Application.DoEvents();
                    Thread.Sleep(1000);
                    Application.DoEvents();
                    Assert.IsNull(player.CurrentVideo);
                    Assert.IsNull(player.BestVideo);
                    Assert.AreEqual(0, player.VideoStreams);
                    Assert.AreEqual(1, player.AudioStreams);
                    Assert.AreEqual(0, player.SubtitleStreams);
                    Assert.IsNotNull(player.CurrentAudio);
                }
            }
        }

        [Test, Explicit]
        [TestCase(@"I:\Users\01. INTRO- Sueno del Cielo.mka")]
        [TestCase(@"H:\Work\My\Music\01. INTRO- Sueno del Cielo.mka")]
        public void MatroskaAudioTest(string path)
        {
            using (var form = new Form()
            {
                Size = new System.Drawing.Size(10, 10),
                Visible = false
            })
            {
                GUIGraphicsContext.form = form;
                using (var player = new AudioPlayerVMR7())
                {
                    player.Play(path);
                    Application.DoEvents();
                    Thread.Sleep(1000);
                    Application.DoEvents();
                    Assert.IsNull(player.CurrentVideo);
                    Assert.IsNull(player.BestVideo);
                    Assert.AreEqual(0, player.VideoStreams);
                    Assert.AreEqual(2, player.AudioStreams);
                    Assert.AreEqual(0, player.SubtitleStreams);
                    Assert.IsNotNull(player.CurrentAudio);
                    Assert.AreEqual(2, player.CurrentAudio.Channel);
                    Assert.AreEqual(44100, player.CurrentAudio.SamplingRate);
                    Assert.AreEqual(16, player.CurrentAudio.BitDepth);
                    Assert.AreEqual("CD1", player.CurrentAudio.Name);
                    Assert.AreEqual(AudioCodec.A_FLAC, player.CurrentAudio.Codec);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.Playlists;

namespace MediaPortal.Tests.Core.Playlists
{
    [TestFixture]
    public class PlayListM3UTests
    {
        [Test]
        public void LoadM3U()
        {
            PlayListM3U playlist = new PlayListM3U();
            Assert.IsTrue(playlist.Load("Core\\Playlists\\TestData\\exampleList.m3u"), "playlist could not even load!");
            Assert.IsTrue(playlist[0].FileName.EndsWith("Bob Marley - 01 - Judge Not.mp3"));
            Assert.IsTrue(playlist[1].FileName.EndsWith("Bob Marley - 02 - One Cup of Coffee.mp3"));
            Assert.IsTrue(playlist[2].FileName.EndsWith("Bob Marley - 03 - Simmer Down.mp3"));
            Assert.IsTrue(playlist[3].FileName.EndsWith("Bob Marley - 05 - Guava Jelly.mp3"));
        }
    }
}

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

using MediaPortal.Player.MediaInfo;
using NUnit.Framework;

namespace MediaPortal.Tests.Core.Player
{
  [TestFixture]
  public class MediaInfoWrapperTests
  {
    [Test, Explicit]
    [TestCase(@"H:\Work\My\Video\Марсианин 3D HSBS-001.mk3d")]
    public void Load3DMatroska(string fileName)
    {
      var info = new MediaInfoWrapper(fileName);
      Assert.AreEqual(1, info.VideoStreams.Count);
      Assert.AreEqual(VideoCodec.V_MPEG4_ISO_AVC, info.VideoStreams[0].Codec);
      Assert.AreEqual(StereoMode.SideBySideLeft, info.VideoStreams[0].Stereoscopic);
      Assert.AreEqual("English", info.VideoStreams[0].Language);
      Assert.AreEqual(3, info.AudioStreams.Count);
      Assert.AreEqual(6, info.AudioStreams[0].Channel);
      Assert.AreEqual("Russian", info.AudioStreams[0].Language);
    }

    [Test, Explicit]
    [TestCase(@"H:\Work\My\Video\Убить дракона\VIDEO_TS\VIDEO_TS.IFO")]
    public void LoadDvd(string fileName)
    {
      var info = new MediaInfoWrapper(fileName);
      Assert.AreEqual(1, info.VideoStreams.Count);
      Assert.AreEqual(VideoCodec.V_MPEG2, info.VideoStreams[0].Codec);
      Assert.AreEqual(StereoMode.Mono, info.VideoStreams[0].Stereoscopic);
      Assert.AreEqual("Unknown", info.VideoStreams[0].Language);
      Assert.AreEqual(2, info.AudioStreams.Count);
      Assert.AreEqual(6, info.AudioStreams[0].Channel);
      Assert.AreEqual("Russian", info.AudioStreams[0].Language);
    }

    [Test, Explicit]
    [TestCase(@"D:\Video\2012.mkv")]
    [TestCase(@"F:\Serials\С точки зрения науки\01. С точки зрения науки. Большой взрыв_2007_HDTV 1080i.ts")]
    public void LoadProblemFiles(string fileName)
    {
      var info = new MediaInfoWrapper(fileName);
      Assert.AreEqual(1, info.VideoStreams.Count);
      Assert.AreEqual(VideoCodec.V_MPEG4_ISO_AVC, info.VideoStreams[0].Codec);
    }

    [Test, Explicit]
    [TestCase(@"https://r8---sn-3c27ln7e.googlevideo.com:443/videoplayback?upn=54U34EqFLPo&requiressl=yes&sver=3&gcr=ua&expire=1453410778&ip=195.177.74.50&mt=1453389018&mv=m&dur=206.773&fexp=9410705,9415031,9416126,9416778,9419452,9419475,9420452,9422596,9423347,9423662,9424822,9425283,9426230,9426410,9427268&ms=au&ipbits=0&mm=31&mn=sn-3c27ln7e&id=o-AGYchkoHPuOeevEPMyZS_TrJQWcvYjqXi5XNGJO__g7Z&initcwndbps=2045000&sparams=dur,gcr,id,initcwndbps,ip,ipbits,itag,lmt,mime,mm,mn,ms,mv,pl,ratebypass,requiressl,source,upn,expire&itag=22&lmt=1432353561075686&ratebypass=yes&mime=video/mp4&pl=24&source=youtube&key=yt6&signature=2D4DBAD2C152193B4D01222073DCD6D35CCB334A.30E689B4B225C4D30AE453EA6D86FC4D06D55B3B&fallback_host=tc.v2.cache8.googlevideo.com&ext=.mp4")]
    public void LoadAvStream(string fileName)
    {
      var info = new MediaInfoWrapper(fileName);
      Assert.AreEqual(1, info.VideoStreams.Count);
      Assert.AreEqual(VideoCodec.V_MPEG4_ISO_AVC, info.VideoStreams[0].Codec);
      Assert.AreEqual(StereoMode.Mono, info.VideoStreams[0].Stereoscopic);
      Assert.AreEqual("English", info.VideoStreams[0].Language);
      Assert.AreEqual(1, info.AudioStreams.Count);
      Assert.AreEqual(2, info.AudioStreams[0].Channel);
      Assert.AreEqual("English", info.AudioStreams[0].Language);
    }
  }
}
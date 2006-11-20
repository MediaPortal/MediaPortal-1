using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using MediaPortal.TagReader;
using MediaPortal.Util;
using MediaPortal.Services;
using MediaPortal.Utils.Services;
using MediaPortal.Tests.MockObjects;

namespace MediaPortal.Tests.TagReader
{
  [TestFixture]
  public class TestTagReader
  {
    [SetUp]
    public void Init()
    {
      GlobalServiceProvider.Replace<ILog>(new NoLog());
    }

    [Test]
    public void TestAPE()
    {
      GetTag(@"TagReader\Music\DingDong.ape");
    }

    [Test]
    public void TestFLAC()
    {
      GetTag(@"TagReader\Music\DingDong.flac");
    }

    [Test]
    public void TestMP3v1()
    {
      GetTag(@"TagReader\Music\DingDongV1.mp3");
    }

    [Test]
    public void TestMP3v2()
    {
      GetTag(@"TagReader\Music\DingDongV2.mp3");
    }

    [Test]
    public void TestM4A()
    {
      GetTag(@"TagReader\Music\DingDong.m4a");
    }

    [Test]
    public void TestMPC()
    {
      GetTag(@"TagReader\Music\DingDong.mpc");
    }

    [Test]
    public void TestOGG()
    {
      GetTag(@"TagReader\Music\DingDong.ogg");
    }

    [Test]
    public void TestWavPack()
    {
      GetTag(@"TagReader\Music\DingDong.wv");
    }

    [Test]
    public void TestWMA()
    {
      GetTag(@"TagReader\Music\DingDong.wma");
    }

    [Test]
    public void TestFileWithoutTag()
    {
      GetFileWithoutTag(@"TagReader\Music\DingDongNoTag.mp3");
    }

    [Test]
    public void TestUnsupported()
    {
      GetUnsupportedFile(@"TagReader\Music\DingDong.xxx");
    }


    void GetTag(string filename)
    {
      MusicTag tag;
      string expectedArtist = Path.GetExtension(filename).ToLower();
      tag = MediaPortal.TagReader.TagReader.ReadTag(filename);
      Assert.IsNotNull(tag, string.Format("Failed to read tag from file: {0}", filename));
      Assert.AreNotEqual(expectedArtist,tag.Artist);
    }

    void GetUnsupportedFile(string filename)
    {
      MusicTag tag;
      tag = MediaPortal.TagReader.TagReader.ReadTag(filename);
      Assert.IsNull(tag);
    }

    void GetFileWithoutTag(string filename)
    {
      MusicTag tag;
      tag = MediaPortal.TagReader.TagReader.ReadTag(filename);
      Assert.AreNotEqual(tag.Duration, 0);
    }
  }
}

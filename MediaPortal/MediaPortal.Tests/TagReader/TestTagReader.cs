#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using MediaPortal.Services;
using MediaPortal.TagReader;
using MediaPortal.Tests.MockObjects;
using NUnit.Framework;

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


    private void GetTag(string filename)
    {
      MusicTag tag;
      string expectedArtist = Path.GetExtension(filename).ToLower();
      tag = MediaPortal.TagReader.TagReader.ReadTag(filename);
      Assert.IsNotNull(tag, string.Format("Failed to read tag from file: {0}", filename));
      Assert.AreNotEqual(expectedArtist, tag.Artist);
    }

    private void GetUnsupportedFile(string filename)
    {
      MusicTag tag;
      tag = MediaPortal.TagReader.TagReader.ReadTag(filename);
      Assert.IsNull(tag);
    }

    private void GetFileWithoutTag(string filename)
    {
      MusicTag tag;
      tag = MediaPortal.TagReader.TagReader.ReadTag(filename);
      Assert.AreNotEqual(tag.Duration, 0);
    }
  }
}
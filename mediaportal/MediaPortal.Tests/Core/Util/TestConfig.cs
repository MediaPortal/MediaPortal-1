using System;
using MediaPortal.Util;
using NUnit.Framework;

namespace MediaPortal.Tests.Core.Util
{
  [TestFixture]
  public class TestConfig
  {
    [Test]
    public void TestBaseDirName()
    {
      DoTest(Config.Dir.Base);
    }

    [Test]
    public void TestLogDirName()
    {
      DoTest(Config.Dir.Log);
    }

    [Test]
    public void TestSkinDirName()
    {
      DoTest(Config.Dir.Skin);
    }

    [Test]
    public void TestLanguageDirName()
    {
      DoTest(Config.Dir.Language);
    }

    [Test]
    public void TestDatabaseDirName()
    {
      DoTest(Config.Dir.Database);
    }

    [Test]
    public void TestPluginsDirName()
    {
      DoTest(Config.Dir.Plugins);
    }

    [Test]
    public void TestCacheDirName()
    {
      DoTest(Config.Dir.Cache);
    }

    [Test]
    public void TestWeatherDirName()
    {
      DoTest(Config.Dir.Weather);
    }

    [Test]
    public void TestCustomInputDeviceDirName()
    {
      DoTest(Config.Dir.CustomInputDevice);
    }

    [Test]
    public void TestConfigDirName()
    {
      DoTest(Config.Dir.Config);
    }

    [Test]
    public void TestThumbsDirName()
    {
      DoTest(Config.Dir.Thumbs);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestInvalidFileName()
    {
      Config.GetFile(Config.Dir.Base, @"\file.ext");
    }

    [Test]
    public void TestSpecialFileNames()
    {
      Assert.AreEqual(Config.GetFolder(Config.Dir.Base) + @"\file{0}.*", Config.GetFile(Config.Dir.Base, "file{0}.*"));
    }

    private static void DoTest(Config.Dir directory)
    {
      Assert.IsFalse(Config.GetFolder(directory).EndsWith(@"\"),
                     string.Format("Config.GetFolder({0}) returns a folder with a trailing backslash", directory));
      Assert.IsFalse(Config.GetFile(directory, "test.xml").Contains(@"\\"),
                     string.Format("Config.GetFile({0},\"test.xml\") returns a path with double backslashes", directory));
    }
  }
}
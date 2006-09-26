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
        public void TestFileWithSubFolder()
        {
            Assert.AreEqual(Config.GetFolder(Config.Dir.Base) + @"\folder\file.ext",Config.GetFile(Config.Dir.Base, @"folder\file.ext"));
        }

        [Test]
        public void TestFileWithSubFolderWithLeadingBackSlash()
        {
            Assert.AreEqual(Config.GetFolder(Config.Dir.Base) + @"\folder\file.ext",Config.GetFile(Config.Dir.Base, @"\folder\file.ext"));
        }

        [Test]
        public void TestFileWithSubFolderWithLeadingSlash()
        {
            Assert.AreEqual(Config.GetFolder(Config.Dir.Base) + @"\folder\file.ext",Config.GetFile(Config.Dir.Base, @"/folder\file.ext"));
        }

        [Test]
        public void TestFileWithLeadingBackSlash()
        {
            Assert.AreEqual(Config.GetFolder(Config.Dir.Base) + @"\file.ext",Config.GetFile(Config.Dir.Base, @"\file.ext"));
        }

        [Test]
        public void TestFileWithLeadingSlash()
        {
            Assert.AreEqual(Config.GetFolder(Config.Dir.Base) + @"\file.ext",Config.GetFile(Config.Dir.Base, @"/file.ext"));
        }


        private static void DoTest(Config.Dir directory)
        {
            Assert.IsTrue(Config.Get(directory).EndsWith(@"\"),string.Format("Config.Get({0}) returns a folder with no trailing backslash",directory));
            Assert.IsFalse(Config.GetFolder(directory).EndsWith(@"\"),string.Format("Config.GetFolder({0}) returns a folder with a trailing backslash",directory));
            Assert.IsFalse(Config.GetFile(directory, "test.xml").Contains(@"\\"), string.Format("Config.GetFile({0},\"test.xml\") returns a path with double backslashes", directory));
        }

    }
}

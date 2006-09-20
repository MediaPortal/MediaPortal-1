using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.Profile;
using System.IO;

namespace MediaPortal.Tests.Core.Profile
{
  [TestFixture]
  public class XmlSettingsPrefetchTest
  {

    [SetUp]
    public void Init()
    {
      rememberedSettings = new List<object[]>();
    }

    List<object[]> rememberedSettings;

    private void Remember(string section, string entry, object value)
    {
      rememberedSettings.Add(new object[] { section, entry, value });
    }

    [Test]
    public void Prefetch1()
    {
      string xml = 
@"<?xml version=""1.0"" encoding=""utf-8""?>
<profile>
  <section name=""capture"">
    <entry name=""tuner"">Cable</entry>
  </section>
</profile>
";
      using(TextWriter writer = File.CreateText("prefetchtest.xml"))
        writer.Write(xml);

      XmlSettingsProvider provider = new XmlSettingsProvider("prefetchtest.xml");
      provider.Prefetch(this.Remember);

      Assert.AreEqual("capture", rememberedSettings[0][0]);
      Assert.AreEqual("tuner", rememberedSettings[0][1]);
      Assert.AreEqual("Cable", rememberedSettings[0][2]);
    }
  }
}

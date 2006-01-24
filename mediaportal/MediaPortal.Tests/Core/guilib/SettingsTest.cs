using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.Profile;

namespace MediaPortal.Tests.Core.Profile
{
  [TestFixture]
  public class SettingsTest
  {

    private static Settings CreateXml()
    {
      return new Settings("Core\\guilib\\TestData\\MediaPortal.xml");
    }

    [SetUp]
    public void Init()
    {
      Settings.ClearCache();
    }

    [Test]
    public void GetValueReturnsValue()
    {
      Settings doc = CreateXml();

      string ret = doc.GetValue("capture", "country");
      Assert.AreEqual("31", ret);

    }

    [Test]
    public void GetValueAsString()
    {
      Settings doc = CreateXml();

      string ret = doc.GetValueAsString("capture", "country", "");
      Assert.AreEqual("31", ret);
    }

    [Test]
    public void GetValueAsBool()
    {
      Settings doc = CreateXml();

      bool ret = doc.GetValueAsBool("general", "startfullscreen", false);
      Assert.IsTrue(ret);
    }

    [Test]
    public void GetValueAsInt()
    {
      Settings doc = CreateXml();

      int ret = doc.GetValueAsInt("capture", "country", 0);
      Assert.AreEqual(31, ret);
    }

    [Test]
    public void GetValueAsFloat()
    {
      Settings doc = CreateXml();

      float ret = doc.GetValueAsFloat("capture", "floattest", 0);
      Assert.AreEqual(3.141592, ret);
    }

    [Test]
    public void SetValueSetsValue()
    {
      Settings doc = CreateXml();

      int prerecord = Convert.ToInt32(doc.GetValue("capture", "prerecord"));
      doc.SetValue("capture", "prerecord", prerecord + 1);
      Assert.AreEqual(prerecord + 1, Convert.ToInt32(doc.GetValue("capture", "prerecord")));
    }

    [Test]
    public void SetValueAsBoolSetsValue()
    {
      Settings doc = CreateXml();

      bool val = doc.GetValueAsBool("general", "minimizeonstartup", false);
      doc.SetValueAsBool("general", "minimizeonstartup", !val);
      Assert.AreEqual(!val, doc.GetValueAsBool("general", "minimizeonstartup", val));
    }

    [Test]
    public void RemoveEntryRemovesEntry()
    {
      Settings doc = CreateXml();

      //Make sure entry exists
      Assert.IsNotNull(doc.GetValue("capture", "postrecord"), "Got null!");

      doc.RemoveEntry("capture", "postrecord");

      Assert.IsEmpty(doc.GetValue("capture", "postrecord"), "Did not get null!");
    }
  }
}

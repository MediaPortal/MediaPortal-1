using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.Profile;

namespace MediaPortal.Tests.Core.Profile
{
  [TestFixture]
  public class CacheSettingsProviderTest : ISettingsProvider, ISettingsPrefetchable
  {
    private string section;
    private string entry;
    private int getValueHits;
    private int removeEntryHits;
    private int setValueHits;
    public object getValueReturns;

    #region ISettingsPrefetchable Members

    public void Prefetch(RememberDelegate function)
    {
      function.Invoke("1234", "5678", 42);
    }

    #endregion

    #region ISettingsProvider Members

    public string FileName
    {
      get { return "Test SettingProvider"; }
    }

    public object GetValue(string section, string entry)
    {
      this.section = section;
      this.entry = entry;
      getValueHits++;
      return getValueReturns;
    }

    public void RemoveEntry(string section, string entry)
    {
      removeEntryHits++;
    }

    public void Save()
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public void SetValue(string section, string entry, object value)
    {
      setValueHits++;
    }

    #endregion

    [SetUp]
    public void Init()
    {
      this.getValueReturns = null;
      this.getValueHits = 0;
      this.removeEntryHits = 0;
      this.setValueHits = 0;
      this.section = null;
      this.entry = null;
    }

    [Test]
    public void GetValueAsksInnerXmlDoc()
    {
      CacheSettingsProvider doc = new CacheSettingsProvider(this);
      string testSection = "foo";
      string testEntry = "bar";

      getValueReturns = 42;

      object returned = doc.GetValue(testSection, testEntry);

      Assert.AreEqual(this.getValueReturns, returned);

      Assert.AreEqual(testSection, section);
      Assert.AreEqual(testEntry, entry);
      Assert.AreEqual(1, getValueHits);
    }

    [Test]
    public void GetValueTwiceUsesCache()
    {
      CacheSettingsProvider doc = new CacheSettingsProvider(this);
      string testSection = "foo";
      string testEntry = "bar";
      getValueReturns = 42;

      object returned = doc.GetValue(testSection, testEntry);

      Assert.AreEqual(this.getValueReturns, returned);

      doc.GetValue(testSection, testEntry);
      doc.GetValue(testSection, testEntry);
      Assert.AreEqual(1, getValueHits);
    }

    [Test]
    public void RemoveValueRemovesFromCache()
    {
      CacheSettingsProvider doc = new CacheSettingsProvider(this);
      string testSection = "foo";
      string testEntry = "bar";
      getValueReturns = 42;

      doc.GetValue(testSection, testEntry);
      doc.RemoveEntry(testSection, testEntry);
      doc.GetValue(testSection, testEntry);

      Assert.AreEqual(2, getValueHits);
      Assert.AreEqual(1, removeEntryHits);
    }

    [Test]
    public void SetValueUpdatesTheCache()
    {
      CacheSettingsProvider doc = new CacheSettingsProvider(this);
      string testSection = "foo";
      string testEntry = "bar";
      getValueReturns = 42;

      Assert.AreEqual(42, doc.GetValue(testSection, testEntry));
      doc.SetValue(testSection, testEntry, 666);
      Assert.AreEqual(666, doc.GetValue(testSection, testEntry));

      Assert.AreEqual(1, getValueHits);
      Assert.AreEqual(1, setValueHits);
    }

    [Test]
    public void PrefetchLoadsCache()
    {
      CacheSettingsProvider doc = new CacheSettingsProvider(this);

      object returned = doc.GetValue("1234", "5678"); //See the Prefetch thing

      Assert.AreEqual(42, returned);
      Assert.AreEqual(0, getValueHits);
    }
  }
}

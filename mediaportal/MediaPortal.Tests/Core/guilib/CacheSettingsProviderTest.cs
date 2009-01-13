#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

using System;
using MediaPortal.Profile;
using MediaPortal.Services;
using MediaPortal.Tests.MockObjects;
using NUnit.Framework;

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
      GlobalServiceProvider.Replace<ILog>(new NoLog());
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
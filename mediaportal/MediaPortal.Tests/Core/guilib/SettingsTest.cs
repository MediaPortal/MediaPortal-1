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

using NUnit.Framework;

using MediaPortal.Profile;

namespace MediaPortal.Tests.Core.Profile
{
  [TestFixture]
  public class SettingsTest
  {

    private static Settings CreateXml()
    {
      return new MediaPortal.Profile.Settings("Core\\guilib\\TestData\\MediaPortal.xml");
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

    //[Test]
    //public void GetValueAsFloat()
    //{
    //  Settings doc = CreateXml();

    //  float ret = doc.GetValueAsFloat("capture", "floattest", 0);
    //  Assert.AreEqual(3.141592, ret);
    //}

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
    public void SetGet()
    {
      Settings doc = CreateXml();
      doc.SetValue("test", "test", "123");
      doc.Dispose();

      doc = CreateXml();
      int intValue = doc.GetValueAsInt("test", "test", 444);
      Assert.AreEqual(intValue, 123);
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

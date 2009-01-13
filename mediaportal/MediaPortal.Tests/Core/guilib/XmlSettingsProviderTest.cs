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
using NUnit.Framework;

namespace MediaPortal.Tests.Core.Profile
{
  [TestFixture]
  public class XmlSettingsProviderTest
  {
    private static XmlSettingsProvider CreateDoc()
    {
      return new XmlSettingsProvider("Core\\guilib\\TestData\\MediaPortal.xml");
    }

    [Test]
    public void XmlDocGetValueReturnsValue()
    {
      XmlSettingsProvider doc = CreateDoc();

      string ret = (string) doc.GetValue("capture", "country");
      Assert.AreEqual("31", ret);
    }

    [Test]
    public void XmlDocFilenameReturnsValue()
    {
      string fileName = "Core\\guilib\\TestData\\MediaPortal.xml";
      XmlSettingsProvider doc = new XmlSettingsProvider(fileName);

      Assert.AreEqual(fileName, doc.FileName);
    }

    [Test]
    public void XmlDocSetValueSetsValue()
    {
      XmlSettingsProvider doc = CreateDoc();

      int prerecord = Convert.ToInt32(doc.GetValue("capture", "prerecord"));
      doc.SetValue("capture", "prerecord", prerecord + 1);
      Assert.AreEqual(prerecord + 1, Convert.ToInt32(doc.GetValue("capture", "prerecord")));
    }

    [Test]
    public void XmlDocSaveSavesValue()
    {
      XmlSettingsProvider doc = CreateDoc();

      int prerecord = Convert.ToInt32(doc.GetValue("capture", "prerecord"));
      doc.SetValue("capture", "prerecord", prerecord + 1);
      doc.Save();

      doc = CreateDoc();
      Assert.AreEqual(prerecord + 1, Convert.ToInt32(doc.GetValue("capture", "prerecord")));
    }

    [Test]
    public void XmlDocRemoveEntryRemovesEntry()
    {
      XmlSettingsProvider doc = CreateDoc();

      //Make sure entry exists
      Assert.IsNotNull(doc.GetValue("capture", "postrecord"));

      doc.RemoveEntry("capture", "postrecord");

      Assert.IsNull(doc.GetValue("capture", "postrecord"));
    }
  }
}
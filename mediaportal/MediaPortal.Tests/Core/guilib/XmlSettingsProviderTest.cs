using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MediaPortal.Profile;

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

            string ret = (string)doc.GetValue("capture", "country");
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

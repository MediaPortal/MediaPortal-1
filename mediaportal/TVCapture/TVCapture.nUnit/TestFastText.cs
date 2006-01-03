using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MediaPortal.TV.Teletext;
using NUnit.Framework;

namespace TVCapture.nUnit
{
  [TestFixture]
  public class TestFastText
  {
    [Test]
    public void Decode()
    {
      Assembly assm = Assembly.GetExecutingAssembly();
      string[] names = assm.GetManifestResourceNames();
      Stream stream = assm.GetManifestResourceStream("TVCapture.nUnit.FastTextPage" );
      BinaryReader reader = new BinaryReader(stream);
      byte[] byPage = new byte[stream.Length];
      reader.Read(byPage, 0, (int)stream.Length);

      FastTextDecoder decoder = new FastTextDecoder();
      decoder.Decode(byPage);
      Assert.AreEqual(decoder.Red, 0x123);
      Assert.AreEqual(decoder.Green, 0x456);
      Assert.AreEqual(decoder.Yellow,0x678);
      Assert.AreEqual(decoder.Blue, 0x789);
      Assert.AreEqual(decoder.White, 0x874);
    }
  }
}

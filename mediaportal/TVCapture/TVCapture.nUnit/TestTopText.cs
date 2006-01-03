using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using MediaPortal.TV.Teletext;
using NUnit.Framework;

namespace TVCapture.nUnit
{
  [TestFixture]
  public class TestTopText
  {
    [Test]
    public void Decode()
    {
      TeletextPageCache cache = new TeletextPageCache();
      AddPage43(0x1f0, 0, ref cache, "TopBasic");
      AddPage43(0x1f1, 0, ref cache, "TopMulti");
      AddPage43(0x1f2, 0, ref cache, "TopAdditional");

      ToptextDecoder decoder = new ToptextDecoder();
      int red,green,blue,yellow;
      string nextGroup, nextBlock;
      decoder.GetPageLinks(cache, 0x100, out red, out green, out yellow, out blue, out nextGroup,out nextBlock);
      Assert.AreEqual(red, 888);
      Assert.AreEqual(blue, 101);
      Assert.AreEqual(yellow, 110);
      Assert.AreEqual(green, 201);
      Assert.AreEqual(nextGroup, "sec1");
      Assert.AreEqual(nextBlock, "chap2");
      decoder.GetPageLinks(cache, 0x210, out red, out green, out yellow, out blue, out nextGroup, out nextBlock);
      Assert.AreEqual(red, 201);
      Assert.AreEqual(blue, 401);
      Assert.AreEqual(yellow, 110);
      Assert.AreEqual(green, 436);
      Assert.AreEqual(nextGroup, "sec1");
      Assert.AreEqual(nextBlock, "chap3");

    }
    void AddPage43(int pageNr, int subNr, ref TeletextPageCache cache, string resourceName)
    {
      Assembly assm = Assembly.GetExecutingAssembly();
      string[] names = assm.GetManifestResourceNames();
      Stream stream = assm.GetManifestResourceStream("TVCapture.nUnit." + resourceName);
      BinaryReader reader = new BinaryReader(stream);
      byte[] byPage43 = new byte[stream.Length];
      reader.Read(byPage43, 0, (int)stream.Length);

      int maxRows = byPage43.Length / 43;
      byte[] topPage = new byte[maxRows * 42];
      for (int i = 0; i < maxRows; ++i)
      {
        for (int c = 0; c <= 41; c++)
          topPage[i * 42 + c] = byPage43[i * 43 + c + 1];
      }

      cache.SetPage(pageNr, subNr, topPage);
    }
  }
}

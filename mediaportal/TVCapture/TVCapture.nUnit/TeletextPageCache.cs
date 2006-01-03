using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using MediaPortal.TV.Teletext;
using NUnit.Framework;
namespace TVCapture.nUnit
{
  [TestFixture]
  [Category("Teletext")]
  public class TeletextPageCacheClass
  {
    [Test]
    public void NumberOfSubpages()
    {
      int pageNr = 0x123;
      int pageNr2 = 0x234;
      TeletextPageCache cache = new TeletextPageCache();

      Assert.AreEqual(0,cache.NumberOfSubpages(pageNr));

      cache.AllocPage(pageNr, 0x0);
      Assert.AreEqual(1, cache.NumberOfSubpages(pageNr));

      cache.AllocPage(pageNr, 0x1);
      Assert.AreEqual(2, cache.NumberOfSubpages(pageNr));

      cache.AllocPage(pageNr, 0x2);
      Assert.AreEqual(3, cache.NumberOfSubpages(pageNr));

      cache.AllocPage(pageNr2, 0x1);
      cache.AllocPage(pageNr2, 0x2);

      Assert.AreEqual(3, cache.NumberOfSubpages(pageNr));
      Assert.AreEqual(2, cache.NumberOfSubpages(pageNr2));

      cache.Clear();
      Assert.AreEqual(0, cache.NumberOfSubpages(pageNr));
      Assert.AreEqual(0, cache.NumberOfSubpages(pageNr2));
    }

    [Test]
    public void PageExists()
    {
      TeletextPageCache cache = new TeletextPageCache();

      Assert.IsFalse(cache.PageExists(0x100));
      Assert.IsFalse(cache.PageExists(0x200));
      cache.AllocPage(0x100, 0x0);
      Assert.IsTrue(cache.PageExists(0x100));
      Assert.IsFalse(cache.PageExists(0x200));
      cache.AllocPage(0x200, 0x0);
      Assert.IsTrue(cache.PageExists(0x100));
      Assert.IsTrue(cache.PageExists(0x200));

      cache.Clear();

      Assert.IsFalse(cache.PageExists(0x100));
      Assert.IsFalse(cache.PageExists(0x200));
    }

    [Test]
    public void SubPageExists()
    {
      int pageNr = 0x123;
      int pageNr2 = 0x234;
      TeletextPageCache cache = new TeletextPageCache();

      Assert.IsFalse(cache.SubPageExists(pageNr, 0));
      Assert.IsFalse(cache.SubPageExists(pageNr, 1));
      Assert.IsFalse(cache.SubPageExists(pageNr, 2));
      Assert.IsFalse(cache.SubPageExists(pageNr2, 0));
      Assert.IsFalse(cache.SubPageExists(pageNr2, 1));
      Assert.IsFalse(cache.SubPageExists(pageNr2, 2));

      cache.AllocPage(pageNr, 0x0);
      Assert.IsTrue(cache.SubPageExists(pageNr, 0));
      Assert.IsFalse(cache.SubPageExists(pageNr, 1));
      Assert.IsFalse(cache.SubPageExists(pageNr, 2));

      cache.AllocPage(pageNr, 0x1);
      Assert.IsTrue(cache.SubPageExists(pageNr, 0));
      Assert.IsTrue(cache.SubPageExists(pageNr, 1));
      Assert.IsFalse(cache.SubPageExists(pageNr, 2));

      cache.AllocPage(pageNr, 0x2);
      Assert.IsTrue(cache.SubPageExists(pageNr, 0));
      Assert.IsTrue(cache.SubPageExists(pageNr, 1));
      Assert.IsTrue(cache.SubPageExists(pageNr, 2));

      cache.AllocPage(pageNr2, 0x1);
      cache.AllocPage(pageNr2, 0x2);
      Assert.IsFalse(cache.SubPageExists(pageNr2, 0));
      Assert.IsTrue(cache.SubPageExists(pageNr2, 1));
      Assert.IsTrue(cache.SubPageExists(pageNr2, 2));

      cache.Clear();

      Assert.IsFalse(cache.SubPageExists(pageNr, 0));
      Assert.IsFalse(cache.SubPageExists(pageNr, 1));
      Assert.IsFalse(cache.SubPageExists(pageNr, 2));
      Assert.IsFalse(cache.SubPageExists(pageNr2, 0));
      Assert.IsFalse(cache.SubPageExists(pageNr2, 1));
      Assert.IsFalse(cache.SubPageExists(pageNr2, 2));
    }

    [Test]
    public void GetSetPage()
    {
      byte[] byData = new byte[50 * 42];
      TeletextPageCache cache = new TeletextPageCache();
      byte kar = 0;
      for (int pageNr = 0x100; pageNr <= 0x200; pageNr+=0x10)
      {
        if (kar < 255) kar++;
        else kar=0;
        for (int subPageNr = 0x0; subPageNr <= 0x70; subPageNr+=0x10)
        {
          if (kar < 255) kar++;
          else kar = 0;
          for (int i=0; i < byData.Length;++i)
          {
            if (kar < 255) kar++;
            else kar = 0;
            byData[i] = kar;
          }
          cache.SetPage(pageNr, subPageNr, byData);
        }
      }

      //now check it!
      kar = 0;
      for (int pageNr = 0x100; pageNr <= 0x200; pageNr+=0x10)
      {
        if (kar < 255) kar++;
        else kar = 0;
        for (int subPageNr = 0x0; subPageNr <= 0x70; subPageNr+=0x10)
        {
          byData = cache.GetPage(pageNr, subPageNr);
          if (kar < 255) kar++;
          else kar = 0;
          for (int i = 0; i < byData.Length; ++i)
          {
            if (kar < 255) kar++;
            else kar = 0;
            Assert.AreEqual(byData[i] , kar);
          }
        }
      }
    }

    [Test]
    public void ClearPage()
    {
      byte[] byData = new byte[50 * 42];
      TeletextPageCache cache = new TeletextPageCache();
      for (int i = 0; i < byData.Length; ++i)
        byData[i] = 0xea;

      cache.SetPage(0x100, 0x1, byData);
      cache.ClearPage(0x100, 0x1);

      byData = cache.GetPage(0x100, 0x1);
      for (int i = 0; i < byData.Length; ++i)
      {
        Assert.AreEqual(32, byData[i]);
      }
    }
    [Test]
    public void GetPagePtr()
    {
      byte[] byData = new byte[50 * 42];
      TeletextPageCache cache = new TeletextPageCache();
      for (int i = 0; i < byData.Length; ++i)
        byData[i] = 0xea;

      cache.SetPage(0x100, 0x1, byData);
      byData = new byte[50 * 42];

      IntPtr ptrPage=cache.GetPagePtr(0x100, 0x1);
      Marshal.Copy(ptrPage, byData, 0, byData.Length);

      for (int i = 0; i < byData.Length; ++i)
      {
        Assert.AreEqual(0xea, byData[i]);
      }
    }
  }
}

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

using MediaPortal.TV.Teletext;
using NUnit.Framework;

namespace MediaPortal.Tests.Teletext
{
  [TestFixture]
  [Category("Teletext")]
  public class TeletextDecoderTest
  {
    [Test]
    public void DecodeSinglePage()
    {
      TeletextPageCache cache = new TeletextPageCache();
      TeletextDecoder decoder = new TeletextDecoder(ref cache);

      byte[] page100 = CreatePage(0x100, 0);
      byte[] page200 = CreatePage(0x200, 1);
      decoder.Decode((byte[]) page100.Clone(), 24);
      decoder.Decode((byte[]) page200.Clone(), 24);

      Assert.IsTrue(cache.PageExists(0x100));
      Assert.IsTrue(cache.SubPageExists(0x100, 0));
      Assert.AreEqual(1, cache.NumberOfSubpages(0x100));

      byte[] decodedPage = cache.GetPage(0x100, 0);
      Assert.IsTrue(ComparePage(page100, decodedPage));


      Assert.IsTrue(cache.PageExists(0x200));
      Assert.IsTrue(cache.SubPageExists(0x200, 1));
      Assert.AreEqual(1, cache.NumberOfSubpages(0x200));


      decodedPage = cache.GetPage(0x200, 1);
      Assert.IsTrue(ComparePage(page200, decodedPage));
      Assert.IsFalse(ComparePage(page100, decodedPage));
    }

    [Test]
    public void DecodeParallelMode()
    {
      int[] transmission = new int[]
                             {
                               100, 101, 102, 103, 104, 205, 206, 207, 208, 209, 309, 310, 311, 312, 313, 314, 315,
                               105, 106, 107, 108, 109, 210, 211, 212, 213, 214, 316, 317, 318, 319, 320, 321, 322,
                               110, 111, 112, 113, 114, 215, 216, 217, 218, 219, 323, 300, 301, 302, 303, 304, 305,
                               115, 116, 117, 118, 119, 220, 221, 222, 223, 200, 306, 307, 308, 309, 310, 311, 312,
                               120, 121, 122, 123, 100, 201, 202, 203, 204, 205, 313, 314, 315, 316, 317, 318, 319,
                               101, 102, 103, 104, 105, 206, 207, 208, 209, 210, 320, 321, 322, 323
                             };
      TeletextPageCache cache = new TeletextPageCache();
      TeletextDecoder decoder = new TeletextDecoder(ref cache);

      for (int i = 0; i < transmission.Length; ++i)
      {
        int magazine = transmission[i]/100;
        int packetNumber = transmission[i]%100;
        if (packetNumber == 0)
        {
          byte[] header = CreateHeader(0x100*magazine, 0);
          decoder.Decode(header, 1);
        }
        else
        {
          byte[] row = CreateRow(0x100*magazine, packetNumber);
          decoder.Decode(row, 1);
        }
      }

      Assert.IsTrue(cache.PageExists(0x100));
      Assert.IsTrue(cache.PageExists(0x200));
      Assert.IsTrue(cache.PageExists(0x300));
    }

    [Test]
    public void DecodeStreamMode()
    {
      int[] transmission = new int[]
                             {
                               0x1FF, 0x1FF, 0x1FF, 0x1FF, 0x100, 0x205, 0x206, 0x207, 0x208, 0x209, 0x309, 0x310, 0x311
                               , 0x312, 0x313, 0x314, 0x315,
                               0x101, 0x102, 0x103, 0x104, 0x105, 0x210, 0x211, 0x212, 0x213, 0x214, 0x316, 0x317, 0x318
                               , 0x319, 0x320, 0x321, 0x322,
                               0x106, 0x107, 0x108, 0x109, 0x110, 0x215, 0x216, 0x217, 0x218, 0x219, 0x323, 0x3FF, 0x3FF
                               , 0x3FF, 0x830, 0x3FF, 0x300,
                               0x111, 0x112, 0x113, 0x114, 0x115, 0x220, 0x221, 0x222, 0x223, 0x200, 0x301, 0x302, 0x303
                               , 0x304, 0x305, 0x306, 0x307,
                               0x116, 0x117, 0x118, 0x119, 0x120, 0x201, 0x202, 0x203, 0x204, 0x205, 0x308, 0x309, 0x310
                               , 0x311, 0x312, 0x313, 0x314,
                               0x121, 0x122, 0x123, 0x1FF, 0x100, 0x206, 0x207, 0x208, 0x209, 0x210, 0x315, 0x316, 0x317
                               , 0x318, 0x319, 0x320, 0x321,
                               0x101, 0x102, 0x103, 0x104, 0x105, 0x211, 0x212, 0x213, 0x214, 0x215, 0x322, 0x323, 0x3FF
                               , 0x3ff, 0x3ff, 0x3ff, 0x3ff,
                               0x1ff, 0x1ff, 0x1ff, 0x1ff, 0x1ff, 0x216, 0x217, 0x218, 0x219, 0x220, 0x3ff, 0x3ff, 0x3FF
                               , 0x3ff, 0x3ff, 0x3ff, 0x3ff,
                               0x1ff, 0x1ff, 0x1ff, 0x1ff, 0x1ff, 0x221, 0x222, 0x223, 0x2ff, 0x2ff, 0x3ff, 0x3ff, 0x3FF
                               , 0x3ff, 0x3ff, 0x3ff, 0x3ff
                             };
      TeletextPageCache cache = new TeletextPageCache();
      TeletextDecoder decoder = new TeletextDecoder(ref cache);

      for (int i = 0; i < transmission.Length; ++i)
      {
        int magazine = transmission[i]/0x100;
        int pageNr = 0x100*magazine;
        int packetNumber = (transmission[i] & 0xf) + (((transmission[i] & 0xf0) >> 4)*10);
        if ((transmission[i] & 0xff) == 0xff)
        {
          pageNr += 0xff;
          packetNumber = 0;
        }
        if (packetNumber == 0)
        {
          byte[] header = CreateHeader(pageNr, 0);
          decoder.Decode(header, 1);
        }
        else
        {
          byte[] row = CreateRow(pageNr, packetNumber);
          decoder.Decode(row, 1);
        }
      }

      Assert.IsTrue(cache.PageExists(0x100));
      Assert.IsTrue(cache.PageExists(0x200));
      Assert.IsTrue(cache.PageExists(0x300));
      byte[] decodedPage = cache.GetPage(0x100, 0);
      VerifyRows(ref decodedPage, false);

      decodedPage = cache.GetPage(0x200, 0);
      VerifyRows(ref decodedPage, false);

      decodedPage = cache.GetPage(0x300, 0);
      VerifyRows(ref decodedPage, false);
    }

    [Test]
    public void FilterOutIllegalRows()
    {
      TeletextPageCache cache = new TeletextPageCache();
      TeletextDecoder decoder = new TeletextDecoder(ref cache);

      for (int i = 0; i <= 31; ++i)
      {
        byte[] row;
        if (i == 0)
        {
          row = CreateHeader(0x100, 0);
        }
        else
        {
          row = CreateRow(0x100, i);
        }
        decoder.Decode(row, 1);
      }

      Assert.IsTrue(cache.PageExists(0x100));
      byte[] decodedPage = cache.GetPage(0x100, 0);
      VerifyRows(ref decodedPage, true);
    }


    [Test]
    public void DecodeSubPages()
    {
      TeletextPageCache cache = new TeletextPageCache();
      TeletextDecoder decoder = new TeletextDecoder(ref cache);

      for (int sub = 0x0; sub < 0x3; sub++)
      {
        for (int page = 0x100; page <= 0x109; page++)
        {
          byte[] pageData = CreatePage(page, sub);
          decoder.Decode(pageData, 24);
        }
      }
      for (int page = 0x100; page <= 0x109; page++)
      {
        Assert.IsTrue(cache.PageExists(page));
        Assert.AreEqual(3, cache.NumberOfSubpages(page));
        Assert.IsTrue(cache.SubPageExists(page, 0x0));
        Assert.IsTrue(cache.SubPageExists(page, 0x1));
        Assert.IsTrue(cache.SubPageExists(page, 0x2));
        Assert.IsFalse(cache.SubPageExists(page, 0x3));
      }
    }

    private void VerifyRows(ref byte[] pageData, bool fastText)
    {
      for (int i = 0; i <= 31; ++i)
      {
        int rowNr = Hamming.GetPacketNumber(i*42, ref pageData);
        if (i < 25 || i == 27)
        {
          if ((i == 24 || i == 27))
          {
            if (fastText)
            {
              Assert.AreEqual(i, rowNr);
            }
            else
            {
              Assert.AreEqual(-1, rowNr);
            }
          }
          else
          {
            Assert.AreEqual(i, rowNr);
          }
        }
        else
        {
          Assert.AreEqual(-1, rowNr);
        }
      }
    }

    private bool ComparePage(byte[] page1, byte[] page2)
    {
      for (int x = 0; x < page1.Length; ++x)
      {
        if (page1[x] != page2[x])
        {
          return false;
        }
      }
      return true;
    }

    private byte[] CreateHeader(int pageNr, int subNr)
    {
      byte[] rows = new byte[42];
      for (int i = 0; i < rows.Length; ++i)
      {
        rows[i] = (byte) (i & 0x7f);
      }
      Hamming.SetHeader(0, ref rows, pageNr, subNr);
      return rows;
    }

    private byte[] CreateRow(int pageNr, int packetNumber)
    {
      byte[] rows = new byte[42];
      for (int i = 0; i < rows.Length; ++i)
      {
        rows[i] = (byte) (i & 0x7f);
      }
      Hamming.SetPacketNumber(0, ref rows, pageNr, packetNumber);
      return rows;
    }

    private byte[] CreatePage(int pageNr, int subNr)
    {
      byte[] rows = new byte[42*24];
      for (int i = 0; i < rows.Length; ++i)
      {
        rows[i] = (byte) (i & 0x7f);
      }
      Hamming.SetHeader(0, ref rows, pageNr, subNr);
      for (int row = 0; row < 24; row++)
      {
        Hamming.SetPacketNumber(row*42, ref rows, pageNr, row);
      }
      return rows;
    }
  }
}
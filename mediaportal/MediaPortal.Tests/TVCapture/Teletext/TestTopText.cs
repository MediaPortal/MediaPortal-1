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
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using MediaPortal.TV.Teletext;
using NUnit.Framework;

namespace MediaPortal.Tests.Teletext
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

      Assert.IsTrue(decoder.Decode(cache, 0x100));
      Assert.AreEqual(decoder.Red, 0x888);
      Assert.AreEqual(decoder.Blue, 0x101);
      Assert.AreEqual(decoder.Yellow, 0x110);
      Assert.AreEqual(decoder.Green, 0x201);
    }

    void AddPage43(int pageNr, int subNr, ref TeletextPageCache cache, string resourceName)
    {
      Assembly assm = Assembly.GetExecutingAssembly();
      string[] names = assm.GetManifestResourceNames();
      Stream stream = assm.GetManifestResourceStream("MediaPortal.Tests.TVCapture.Teletext." + resourceName);
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

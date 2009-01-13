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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using MediaPortal.TV.Teletext;
using NUnit.Framework;

namespace MediaPortal.Tests.Teletext
{
  [TestFixture]
  [Category("Teletext")]
  public class RendererTest
  {
    [Test]
    public void RenderPages()
    {
      RenderPage("100", 0x100, 0x0, "100.png");
      RenderPage("199", 0x199, 0x0, "199.png");
      RenderPage("234", 0x234, 0x0, "234.png");
      RenderPage("333", 0x333, 0x0, "333.png");
      RenderPage("503", 0x503, 0x0, "503.png");
      RenderPage("541", 0x541, 0x0, "541.png");
      RenderPage("FastTextPage", 0x100, 0x0, "FastText.png");
    }

    private void RenderPage(string resourceName, int pageNr, int subNr, string filename)
    {
      try
      {
        File.Delete(filename);
      }
      catch (Exception)
      {
      }
      Assembly assm = Assembly.GetExecutingAssembly();
      string[] names = assm.GetManifestResourceNames();
      Stream stream = assm.GetManifestResourceStream("MediaPortal.Tests.TVCapture.Teletext." + resourceName);
      BinaryReader reader = new BinaryReader(stream);
      byte[] byPage = new byte[stream.Length];
      reader.Read(byPage, 0, (int) stream.Length);

      TeletextPageRenderer renderer = new TeletextPageRenderer();
      renderer.Width = 800;
      renderer.Height = 600;
      using (Bitmap bmp = renderer.RenderPage(byPage, pageNr, subNr))
      {
        bmp.Save(filename, ImageFormat.Png);
      }
      renderer.Clear();
      Assert.AreEqual(true, true);
    }
  }
}
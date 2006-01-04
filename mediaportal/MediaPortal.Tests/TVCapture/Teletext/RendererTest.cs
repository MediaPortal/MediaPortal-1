using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MediaPortal.TV.Teletext;
using NUnit.Framework;

namespace TVCapture.nUnit
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
    
    void RenderPage(string resourceName, int pageNr, int subNr, string filename)
    {
      try
      {
        System.IO.File.Delete(filename);
      }
      catch (Exception)
      { }
      Assembly assm = Assembly.GetExecutingAssembly();
      string[] names=assm.GetManifestResourceNames();
      Stream stream = assm.GetManifestResourceStream("MediaPortal.Tests.TVCapture.Teletext." + resourceName);
      BinaryReader reader = new BinaryReader(stream);
      byte[] byPage = new byte[stream.Length];
      reader.Read(byPage, 0, (int)stream.Length);

      TeletextPageRenderer renderer = new TeletextPageRenderer();
      renderer.Width = 800;
      renderer.Height = 600;
      using (Bitmap bmp = renderer.RenderPage(byPage, pageNr, subNr))
      {
        bmp.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
      }
      renderer.Clear();
      Assert.AreEqual(true, true);
    }
  }
}

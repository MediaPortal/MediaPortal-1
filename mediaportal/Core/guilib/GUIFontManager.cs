#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Xml;
using DShowNET.Helper;
using MediaPortal.Util;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Filter = Microsoft.DirectX.Direct3D.Filter;
using Font = Microsoft.DirectX.Direct3D.Font;
using Matrix = Microsoft.DirectX.Matrix;
using MediaPortal.ExtensionMethods;

// ReSharper disable CheckNamespace
namespace MediaPortal.GUI.Library
// ReSharper restore CheckNamespace
{
  /// <summary>
  /// The class responsible for keeping track of the used fonts.
  /// </summary>
  public class GUIFontManager
  {
    #region Constructors

    // singleton. Dont allow any instance of this class
    private GUIFontManager() {}

    static GUIFontManager() {}

    #endregion

    #region Private structs

    private struct FontManagerDrawText
    {
      public Font Fnt;
      public float Xpos;
      public float Ypos;
      public int Color;
      public string Text;
      public float[,] Matrix;
      public Viewport Viewport;
      public int FontHeight;
    };

    #endregion

    private static readonly object RenderLock = new object();
    protected static List<GUIFont> ListFonts = new List<GUIFont>();
    protected static Dictionary<string, string> DictFontAlias = new Dictionary<string, string>();
    private static Sprite _sprite;
    private static bool _spriteUsed;
    private const int MaxCachedTextures = 250;
    private static List<FontManagerDrawText> _listDrawText = new List<FontManagerDrawText>();
    private static readonly List<FontTexture> ListFontTextures = new List<FontTexture>();
    private static readonly List<FontObject> ListFontObjects = new List<FontObject>();


    public static int Count
    {
      get { return ListFonts.Count; }
    }

    public static object Renderlock
    {
      get { return RenderLock; }
    }

    /// <summary>
    /// Loads the fonts from a file.
    /// </summary>
    /// <param name="strFilename">The filename from where the fonts are loaded.</param>
    /// <returns>true if loaded else false</returns>
    public static bool LoadFonts(string strFilename)
    {
      // Clear current set of fonts
      Dispose();
      lock (Renderlock)
      {
        int counter = 0;
        Log.Info("Load fonts from: {0}", strFilename);
        ListFonts.DisposeAndClear();

        // Load the debug font
        var fontDebug = new GUIFont("debug", "Arial", 12) {ID = counter++};
        fontDebug.Load();
        ListFonts.Add(fontDebug);

        try
        {
          // Load the XML document
          var doc = new XmlDocument();
          doc.Load(strFilename);
          // Check the root element
          if (doc.DocumentElement == null)
          {
            return false;
          }
          string strRoot = doc.DocumentElement.Name;
          if (strRoot != "fonts")
          {
            return false;
          }
          // Select the list of fonts
          XmlNodeList list = doc.DocumentElement.SelectNodes("/fonts/font");
          if (list != null)
          {
            foreach (XmlNode node in list)
            {
              XmlNode nodeStart = node.SelectSingleNodeFast("startchar");
              XmlNode nodeEnd = node.SelectSingleNodeFast("endchar");
              XmlNode nodeName = node.SelectSingleNodeFast("name");
              XmlNode nodeFileName = node.SelectSingleNodeFast("filename");
              XmlNode nodeHeight = node.SelectSingleNodeFast("height");
              XmlNode nodeBold = node.SelectSingleNodeFast("bold");
              XmlNode nodeItalics = node.SelectSingleNodeFast("italic");
              if (nodeHeight != null && nodeName != null && nodeFileName != null)
              {
                bool bold = false;
                bool italic = false;
                if (nodeBold != null && nodeBold.InnerText.Equals("yes"))
                {
                  bold = true;
                }
                if (nodeItalics != null && nodeItalics.InnerText.Equals("yes"))
                {
                  italic = true;
                }
                string strName = nodeName.InnerText;
                string strFileName = nodeFileName.InnerText;
                int iHeight = Int32.Parse(nodeHeight.InnerText);

                // font height is based on legacy hard coded resolution of 720x576
                float baseSize = 576;

                // adjust for different DPI settings (96dpi = 100%)
                Graphics graphics = GUIGraphicsContext.form.CreateGraphics();
                // With DPIAware setting baseSize need to be kept
                if (Environment.OSVersion.Version.Major >= 6 && graphics.DpiY == 96.0)
                {
                  baseSize = 576;
                }
                else
                {
                  baseSize *= graphics.DpiY / 96;
                }

                float fPercent = (GUIGraphicsContext.Height * GUIGraphicsContext.ZoomVertical) / baseSize;
                fPercent *= iHeight;
                iHeight = (int)fPercent;
                var style = FontStyle.Regular;
                if (bold)
                {
                  style |= FontStyle.Bold;
                }
                if (italic)
                {
                  style |= FontStyle.Italic;
                }
                var font = new GUIFont(strName, strFileName, iHeight, style) {ID = counter++};

                // .NET's LocalisationProvider should give the correct amount of chars.
                if (nodeStart != null && nodeStart.InnerText != "" && nodeEnd != null && nodeEnd.InnerText != "")
                {
                  int start = Int32.Parse(nodeStart.InnerText);
                  int end = Int32.Parse(nodeEnd.InnerText);
                  font.SetRange(start, end);
                }
                else
                {
                  font.SetRange(0, GUIGraphicsContext.CharsInCharacterSet);
                }

                font.Load();
                ListFonts.Add(font);
              }
            }
          }

          // Select the list of aliases
          DictFontAlias.Clear();
          XmlNodeList listAlias = doc.DocumentElement.SelectNodes("/fonts/alias");
          if (listAlias != null)
          {
            foreach (XmlNode node in listAlias)
            {
              XmlNode nodeName = node.SelectSingleNodeFast("name");
              XmlNode nodeFontName = node.SelectSingleNodeFast("fontname");
              DictFontAlias.Add(nodeName.InnerText, nodeFontName.InnerText);
            }
          }
          return true;
        }
        catch (Exception ex)
        {
          Log.Warn("GUIFontManager: Exception loading fonts {0} err:{1} stack:{2}", strFilename, ex.Message, ex.StackTrace);
        }

        return false;
      }
    }


    /// <summary>
    /// Gets a GUIFont.
    /// </summary>
    /// <param name="iFont">The font number</param>
    /// <returns>A GUIFont instance representing the fontnumber or a default GUIFont if the number does not exists.</returns>
    public static GUIFont GetFont(int iFont)
    {
      lock (Renderlock)
      {
        if (iFont >= 0 && iFont < ListFonts.Count)
        {
          return ListFonts[iFont];
        }
        Log.Warn("GUIFontManager: could load load font with index '{0}'", iFont);
        return GetFont("debug");
      }
    }


    /// <summary>
    /// Gets a GUIFont.
    /// </summary>
    /// <param name="strFontName">The name of the font</param>
    /// <returns>A GUIFont instance representing the strFontName or a default GUIFont if the strFontName does not exists.</returns>
    public static GUIFont GetFont(string strFontName)
    {
      // do nothing if no fonts are loaded
      if (ListFonts.Count == 0)
      {
        return null;
      }

      lock (Renderlock)
      {
        if (!string.IsNullOrEmpty(strFontName))
        {
          // Try to interpret the font name as an alias before searching for the font.
          string fn;
          if (!DictFontAlias.TryGetValue(strFontName, out fn))
          {
            fn = strFontName;
          }

          foreach (GUIFont font in ListFonts.Where(font => font.FontName == fn))
          {
            return font;
          }
          Log.Warn("GUIFontManager: Font with the name '{0}' does not exist", strFontName);
        }

        // prevent infinite recursion in case we are already trying to load the default font
        if (strFontName != "debug")
        {
          // return default font
          return GetFont("debug");
        }
        Log.Error("GUIFontManager: could load default font");
        return null;
      }
    }

    public static void MeasureText(Font fnt, string text, ref float textwidth, ref float textheight, int fontSize)
    {
      if (text[0] == ' ') // anti-trim
      {
        text = "_" + text.Substring(1);
      }
      if (text[text.Length - 1] == ' ')
      {
        text = text.Substring(0, text.Length - 1) + '_';
      }

      // Text drawing doesnt work with DX9Ex & sprite
      if (GUIGraphicsContext.IsDirectX9ExUsed())
      {
        MeasureText(text, ref textwidth, ref textheight, fontSize);
      }
      else
      {
        if (_sprite == null)
        {
          _sprite = new Sprite(GUIGraphicsContext.DX9Device);
        }
        Rectangle rect = fnt.MeasureString(_sprite, text, DrawTextFormat.NoClip, Color.Black);
        textwidth = rect.Width;
        textheight = rect.Height;
      }
    }

    /// <summary>
    /// Uses GDI+ Graphics to render a given text and measure the it's size
    /// </summary>
    /// <param name="text"></param>
    /// <param name="textwidth"></param>
    /// <param name="textheight"></param>
    /// <param name="fontSize"></param>
    public static void MeasureText(string text, ref float textwidth, ref float textheight, int fontSize)
    {
      try
      {
        using (var bmp = new Bitmap(1, 1, PixelFormat.Format32bppArgb))
        {
          using (Graphics g = Graphics.FromImage(bmp))
          {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.TextContrast = 0;

            Size size = g.MeasureString(text, CachedSystemFont(fontSize)).ToSize();
            textwidth = size.Width;
            textheight = size.Height;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUIFontManager: Error in MeasureText - {0}", ex.Message);
      }
    }

    private static System.Drawing.Font CachedSystemFont(int fontSize)
    {
      bool fontCached = false;
      int cacheSlot = 0;
      foreach (FontObject cachedFont in ListFontObjects)
      {
        if (cachedFont.size == fontSize)
        {
          fontCached = true;
          break;
        }
        cacheSlot++;
      }

      if (!fontCached)
      {
        var systemFont = new System.Drawing.Font("Arial", fontSize);
        var newFont = new FontObject {size = fontSize, font = systemFont};
        ListFontObjects.Add(newFont);
      }

      return ListFontObjects[cacheSlot].font;
    }

    public static void DrawText(Font fnt, float xpos, float ypos, Color color, string text, int maxWidth, int fontHeight)
    {
      FontManagerDrawText draw;
      draw.FontHeight = fontHeight;
      draw.Fnt = fnt;
      draw.Xpos = xpos;
      draw.Ypos = ypos;
      draw.Color = color.ToArgb();
      draw.Text = text;
      draw.Matrix = (float[,])GUIGraphicsContext.GetFinalMatrix().Clone();
      draw.Viewport = GUIGraphicsContext.DX9Device.Viewport;
      if (maxWidth >= 0)
      {
        draw.Viewport.Width = ((int)GUIGraphicsContext.GetFinalTransform().TransformXCoord(xpos + maxWidth, 0, 0)) -
                              draw.Viewport.X;
      }
      _listDrawText.Add(draw);
      _spriteUsed = true;
    }

    private static void DrawTextUsingTexture(FontManagerDrawText draw, int fontSize)
    {
      bool textureCached = false;
      int cacheSlot = 0;
      var drawingTexture = new FontTexture();
      foreach (FontTexture cachedTexture in ListFontTextures)
      {
        if (cachedTexture.text == draw.Text && cachedTexture.size == fontSize)
        {
          textureCached = true;
          drawingTexture = cachedTexture;
          break;
        }
        cacheSlot++;
      }

      var size = new Size(0, 0);
      if (textureCached)
      {
        //keep commonly used textures at the top of the pile
        ListFontTextures.RemoveAt(cacheSlot);
        ListFontTextures.Add(drawingTexture);
      }
      else // texture needs to be cached
      {
        Texture texture = null;
        float textwidth = 0, textheight = 0;

        MeasureText(draw.Text, ref textwidth, ref textheight, fontSize);
        size.Width = (int)textwidth;
        size.Height = (int)textheight;

        try
        {
          // The MemoryStream must be kept open for the lifetime of the bitmap
          using (var imageStream = new MemoryStream())
          {
            using (var bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb))
            {
              using (Graphics g = Graphics.FromImage(bitmap))
              {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.TextContrast = 0;
                g.DrawString(draw.Text, CachedSystemFont(fontSize), Brushes.White, new Point(0, 0),
                             StringFormat.GenericTypographic);

                bitmap.Save(imageStream, ImageFormat.Bmp);

                imageStream.Position = 0;
                var format = Format.Dxt3;
                if (GUIGraphicsContext.GetTexturePoolType() == Pool.Default)
                {
                  format = Format.Unknown;
                }

                var info = new ImageInformation();
                try
                {
                  texture = TextureLoader.FromStream(GUIGraphicsContext.DX9Device,
                                                     imageStream, (int)imageStream.Length,
                                                     0, 0,
                                                     1,
                                                     0,
                                                     format,
                                                     GUIGraphicsContext.GetTexturePoolType(),
                                                     Filter.None,
                                                     Filter.None,
                                                     0,
                                                     ref info);
                }
                catch (OutOfVideoMemoryException oovme)
                {
                  Log.Error("GUIFontManager: OutOfVideoMemory in DrawTextUsingTexture - {0}", oovme.Message);
                }
                catch (OutOfMemoryException oome)
                {
                  Log.Error("GUIFontManager: OutOfMemory in DrawTextUsingTexture - {0}", oome.Message);
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("GUIFontManager: Error in DrawTextUsingTexture - {0}", ex.Message);
        }

        MeasureText(draw.Text, ref textwidth, ref textheight, fontSize);
        size.Width = (int)textwidth;
        size.Height = (int)textheight;

        var newTexture = new FontTexture {text = draw.Text, texture = texture, size = fontSize};

        if (ListFontTextures.Count >= MaxCachedTextures)
        {
          //need to clear this and not rely on the finalizer
          FontTexture disposableFont = ListFontTextures[0];
          ListFontTextures.RemoveAt(0);
          disposableFont.Dispose();
        }
        ListFontTextures.Add(newTexture);
        drawingTexture = newTexture;
      }

      _sprite.Draw(drawingTexture.texture, new Rectangle(0, 0, size.Width, size.Height),
                       Vector3.Empty,
                       new Vector3((int)draw.Xpos, (int)draw.Ypos, 0), draw.Color);
    }

    public static void Present()
    {
      lock (Renderlock)
      {
        DXNative.FontEnginePresentTextures();
        foreach (GUIFont font in ListFonts)
        {
          font.Present();
        }

        if (_spriteUsed)
        {
          if (_sprite == null)
          {
            _sprite = new Sprite(GUIGraphicsContext.DX9Device);
          }
          _sprite.Begin(SpriteFlags.AlphaBlend | SpriteFlags.SortTexture);
          Viewport orgView = GUIGraphicsContext.DX9Device.Viewport;
          Matrix orgProj = GUIGraphicsContext.DX9Device.Transform.View;
          Matrix projm = orgProj;

          foreach (FontManagerDrawText draw in _listDrawText)
          {
            Matrix finalm;
            finalm.M11 = draw.Matrix[0, 0];
            finalm.M21 = draw.Matrix[0, 1];
            finalm.M31 = draw.Matrix[0, 2];
            finalm.M41 = draw.Matrix[0, 3];
            finalm.M12 = draw.Matrix[1, 0];
            finalm.M22 = draw.Matrix[1, 1];
            finalm.M32 = draw.Matrix[1, 2];
            finalm.M42 = draw.Matrix[1, 3];
            finalm.M13 = draw.Matrix[2, 0];
            finalm.M23 = draw.Matrix[2, 1];
            finalm.M33 = draw.Matrix[2, 2];
            finalm.M43 = draw.Matrix[2, 3];
            finalm.M14 = 0;
            finalm.M24 = 0;
            finalm.M34 = 0;
            finalm.M44 = 1.0f;
            _sprite.Transform = finalm;
            GUIGraphicsContext.DX9Device.Viewport = draw.Viewport;
            float wfactor = (float)orgView.Width / draw.Viewport.Width;
            float hfactor = (float)orgView.Height / draw.Viewport.Height;
            float xoffset = (float)orgView.X - draw.Viewport.X;
            float yoffset = (float)orgView.Y - draw.Viewport.Y;
            projm.M11 = (orgProj.M11 + orgProj.M14 * xoffset) * wfactor;
            projm.M21 = (orgProj.M21 + orgProj.M24 * xoffset) * wfactor;
            projm.M31 = (orgProj.M31 + orgProj.M34 * xoffset) * wfactor;
            projm.M41 = (orgProj.M41 + orgProj.M44 * xoffset) * wfactor;
            projm.M12 = (orgProj.M12 + orgProj.M14 * yoffset) * hfactor;
            projm.M22 = (orgProj.M22 + orgProj.M24 * yoffset) * hfactor;
            projm.M32 = (orgProj.M32 + orgProj.M34 * yoffset) * hfactor;
            projm.M42 = (orgProj.M42 + orgProj.M44 * yoffset) * hfactor;
            GUIGraphicsContext.DX9Device.Transform.View = projm;
            if (GUIGraphicsContext.IsDirectX9ExUsed())
            {
              DrawTextUsingTexture(draw, draw.FontHeight);
            }
            else
            {
              draw.Fnt.DrawText(_sprite, draw.Text, new Rectangle((int)draw.Xpos,
                                                                      (int)draw.Ypos, 0, 0), DrawTextFormat.NoClip,
                                draw.Color);
            }

            _sprite.Flush();
          }

          GUIGraphicsContext.DX9Device.Viewport = orgView;
          GUIGraphicsContext.DX9Device.Transform.View = orgProj;
          _sprite.End();
          _listDrawText = new List<FontManagerDrawText>();
          _spriteUsed = false;
        }
      }
    }

    /// <summary>
    /// Disposes all GUIFonts.
    /// </summary>
    public static void Dispose()
    {
      lock (Renderlock)
      {
        Log.Debug("GUIFontManager: SafeDispose()");
        //_listFonts.DisposeAndClear();
        foreach (GUIFont font in ListFonts)
        {
          font.Dispose(null, null);
        }

        if (_sprite != null)
        {
          _sprite.SafeDispose();
          _sprite = null;
          _spriteUsed = false;
        }
        ListFontTextures.DisposeAndClear();
        ListFontObjects.DisposeAndClear();
      }
    }

    public static void ClearFontCache()
    {
      try
      {
        string fontCache = String.Format(@"{0}\fonts", GUIGraphicsContext.SkinCacheFolder);
        MediaPortal.Util.Utils.DirectoryDelete(fontCache, true);
      }
      // ReSharper disable EmptyGeneralCatchClause
      catch (Exception) {}
      // ReSharper restore EmptyGeneralCatchClause
    }

    /// <summary>
    /// Sets the device and the FVF.
    /// </summary>
    public static void SetDevice()
    {
      Log.Debug("GUIFontManager SetDevice()");
      IntPtr upDevice = DirectShowUtil.GetUnmanagedDevice(GUIGraphicsContext.DX9Device);

      unsafe
      {
        DXNative.FontEngineSetDevice(upDevice.ToPointer());
      }
    }

    public static void SetDeviceNull()
    {
      Log.Debug("GUIFontManager SetDeviceNull()");
      unsafe
      {
        DXNative.FontEngineSetDevice(null);
      }
    }

    /// <summary>
    /// Initializes the device objects of the GUIFonts.
    /// </summary>
    public static void InitializeDeviceObjects()
    {
      lock (Renderlock)
      {
        Log.Debug("GUIFontManager InitializeDeviceObjects()");
        IntPtr upDevice = DirectShowUtil.GetUnmanagedDevice(GUIGraphicsContext.DX9Device);

        unsafe
        {
          DXNative.FontEngineSetDevice(upDevice.ToPointer());
        }
        foreach (GUIFont font in ListFonts)
        {
          font.InitializeDeviceObjects();
        }
      }
    }
  }
}
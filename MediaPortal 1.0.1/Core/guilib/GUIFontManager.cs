#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using DShowNET.Helper;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Filter=Microsoft.DirectX.Direct3D.Filter;
using Font=Microsoft.DirectX.Direct3D.Font;
using Matrix=Microsoft.DirectX.Matrix;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// The class responsible for keeping track of the used fonts.
  /// </summary>
  public class GUIFontManager
  {
    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEnginePresentTextures();

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineSetDevice(void* device);

    #region Constructors

    // singleton. Dont allow any instance of this class
    private GUIFontManager()
    {
    }

    static GUIFontManager()
    {
    }

    #endregion

    #region Private structs

    private struct FontManagerDrawText
    {
      public Font fnt;
      public float xpos;
      public float ypos;
      public int color;
      public string text;
      public float[,] matrix;
      public Viewport viewport;
      public int fontHeight;
    } ;

    // This is used for caching font textures (non-latin char support)
    private struct FontTexture
    {
      public int size;
      public string text;
      public Texture texture;
    } ;

    // This is used for caching system fonts (non-latin char support)
    private struct FontObject
    {
      public int size;
      public System.Drawing.Font font;
    } ;

    #endregion

    protected static List<GUIFont> _listFonts = new List<GUIFont>();
    private static Sprite _d3dxSprite;
    private static bool _d3dxSpriteUsed;
    private static int _maxCachedTextures = 500;
    private static List<FontManagerDrawText> _listDrawText = new List<FontManagerDrawText>();
    private static List<FontTexture> _listFontTextures = new List<FontTexture>();
    private static List<FontObject> _listFontObjects = new List<FontObject>();


    public static int Count
    {
      get { return _listFonts.Count; }
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
      int counter = 0;
      Log.Info("  Load fonts from {0}", strFilename);
      _listFonts.Clear();

      // Load the debug font
      GUIFont fontDebug = new GUIFont("debug", "Arial", 12);
      fontDebug.ID = counter++;
      fontDebug.Load();
      _listFonts.Add(fontDebug);

      try
      {
        // Load the XML document
        XmlDocument doc = new XmlDocument();
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
        foreach (XmlNode node in list)
        {
          XmlNode nodeStart = node.SelectSingleNode("startchar");
          XmlNode nodeEnd = node.SelectSingleNode("endchar");
          XmlNode nodeName = node.SelectSingleNode("name");
          XmlNode nodeFileName = node.SelectSingleNode("filename");
          XmlNode nodeHeight = node.SelectSingleNode("height");
          XmlNode nodeBold = node.SelectSingleNode("bold");
          XmlNode nodeItalics = node.SelectSingleNode("italic");
          if (nodeHeight != null && nodeName != null && nodeFileName != null)
          {
            bool bold = false;
            bool italic = false;
            if (nodeBold != null && nodeBold.InnerText != null && nodeBold.InnerText.Equals("yes"))
            {
              bold = true;
            }
            if (nodeItalics != null && nodeItalics.InnerText != null && nodeItalics.InnerText.Equals("yes"))
            {
              italic = true;
            }
            string strName = nodeName.InnerText;
            string strFileName = nodeFileName.InnerText;
            int iHeight = Int32.Parse(nodeHeight.InnerText);

            // height is based on 720x576
            float fPercent = ((float) GUIGraphicsContext.Height)/576.0f;
            fPercent *= iHeight;
            iHeight = (int) fPercent;
            FontStyle style = new FontStyle();
            style = FontStyle.Regular;
            if (bold)
            {
              style |= FontStyle.Bold;
            }
            if (italic)
            {
              style |= FontStyle.Italic;
            }
            GUIFont font = new GUIFont(strName, strFileName, iHeight, style);
            font.ID = counter++;

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
            _listFonts.Add(font);
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Info("GUIFontManager: Exception loading fonts {0} err:{1} stack:{2}", strFilename, ex.Message, ex.StackTrace);
      }

      return false;
    }

    /// <summary>
    /// Gets a GUIFont.
    /// </summary>
    /// <param name="iFont">The font number</param>
    /// <returns>A GUIFont instance representing the fontnumber or a default GUIFont if the number does not exists.</returns>
    public static GUIFont GetFont(int iFont)
    {
      if (iFont >= 0 && iFont < _listFonts.Count)
      {
        return _listFonts[iFont];
      }
      return GetFont("debug");
    }

    /// <summary>
    /// Gets a GUIFont.
    /// </summary>
    /// <param name="strFontName">The name of the font</param>
    /// <returns>A GUIFont instance representing the strFontName or a default GUIFont if the strFontName does not exists.</returns>
    public static GUIFont GetFont(string strFontName)
    {
      for (int i = 0; i < _listFonts.Count; ++i)
      {
        GUIFont font = _listFonts[i];
        if (font.FontName == strFontName)
        {
          return font;
        }
      }

      // just return a font
      return GetFont("debug");
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
        if (_d3dxSprite == null)
        {
          _d3dxSprite = new Sprite(GUIGraphicsContext.DX9Device);
        }
        Rectangle rect = fnt.MeasureString(_d3dxSprite, text, DrawTextFormat.NoClip, Color.Black);
        textwidth = rect.Width;
        textheight = rect.Height;
      }
      return;
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
        using (Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format32bppArgb))
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
      foreach (FontObject cachedFont in _listFontObjects)
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
        System.Drawing.Font systemFont = new System.Drawing.Font("Arial", fontSize);
        FontObject newFont;
        newFont.size = fontSize;
        newFont.font = systemFont;
        _listFontObjects.Add(newFont);
      }

      return _listFontObjects[cacheSlot].font;
    }

    public static void DrawText(Font fnt, float xpos, float ypos, Color color, string text, int maxWidth, int fontHeight)
    {
      FontManagerDrawText draw;
      draw.fontHeight = fontHeight;
      draw.fnt = fnt;
      draw.xpos = xpos;
      draw.ypos = ypos;
      draw.color = color.ToArgb();
      draw.text = text;
      draw.matrix = (float[,]) GUIGraphicsContext.GetFinalMatrix().Clone();
      draw.viewport = GUIGraphicsContext.DX9Device.Viewport;
      if (maxWidth >= 0)
      {
        draw.viewport.Width = ((int) xpos) + maxWidth - draw.viewport.X;
      }
      _listDrawText.Add(draw);
      _d3dxSpriteUsed = true;
    }

    private static void DrawTextUsingTexture(FontManagerDrawText draw, int fontSize)
    {
      bool textureCached = false;
      int cacheSlot = 0;
      foreach (FontTexture cachedTexture in _listFontTextures)
      {
        if (cachedTexture.text == draw.text && cachedTexture.size == fontSize)
        {
          textureCached = true;
          break;
        }
        cacheSlot++;
      }

      Size size = new Size(0, 0);

      if (!textureCached)
      {
        Texture texture = null;
        float textwidth = 0, textheight = 0;

        MeasureText(draw.text, ref textwidth, ref textheight, fontSize);
        size.Width = (int) textwidth;
        size.Height = (int) textheight;

        try
        {
          // The MemoryStream must be kept open for the lifetime of the bitmap
          using (MemoryStream imageStream = new MemoryStream())
          {
            using (Bitmap bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb))
            {
              using (Graphics g = Graphics.FromImage(bitmap))
              {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.TextContrast = 0;
                g.DrawString(draw.text, CachedSystemFont(fontSize), Brushes.White, new Point((int) 0, (int) 0),
                             StringFormat.GenericTypographic);

                bitmap.Save(imageStream, ImageFormat.Bmp);

                imageStream.Position = 0;
                Format format = Format.Dxt3;
                if (GUIGraphicsContext.GetTexturePoolType() == Pool.Default)
                {
                  format = Format.Unknown;
                }

                ImageInformation info = new ImageInformation();
                try
                {
                  texture = TextureLoader.FromStream(GUIGraphicsContext.DX9Device,
                                                     imageStream, (int) imageStream.Length,
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

        MeasureText(draw.text, ref textwidth, ref textheight, fontSize);
        size.Width = (int) textwidth;
        size.Height = (int) textheight;

        FontTexture newTexture;
        newTexture.text = draw.text;
        newTexture.texture = texture;
        newTexture.size = fontSize;

        if (_listFontTextures.Count >= _maxCachedTextures)
        {
          _listFontTextures.RemoveAt(0);
        }
        _listFontTextures.Add(newTexture);
      }

      _d3dxSprite.Draw(_listFontTextures[cacheSlot].texture, new Rectangle(0, 0, size.Width, size.Height),
                       Vector3.Empty,
                       new Vector3((int) draw.xpos, (int) draw.ypos, 0), draw.color);
    }

    public static void Present()
    {
      FontEnginePresentTextures();
      for (int i = 0; i < _listFonts.Count; ++i)
      {
        GUIFont font = _listFonts[i];
        font.Present();
      }

      if (_d3dxSpriteUsed)
      {
        if (_d3dxSprite == null)
        {
          _d3dxSprite = new Sprite(GUIGraphicsContext.DX9Device);
        }
        _d3dxSprite.Begin(SpriteFlags.AlphaBlend | SpriteFlags.SortTexture);
        Viewport orgView = GUIGraphicsContext.DX9Device.Viewport;
        Matrix orgProj = GUIGraphicsContext.DX9Device.Transform.View;
        Matrix projm = orgProj;
        Matrix finalm;

        foreach (FontManagerDrawText draw in _listDrawText)
        {
          finalm.M11 = draw.matrix[0, 0];
          finalm.M12 = draw.matrix[0, 1];
          finalm.M13 = draw.matrix[0, 2];
          finalm.M14 = draw.matrix[0, 3];
          finalm.M21 = draw.matrix[1, 0];
          finalm.M22 = draw.matrix[1, 1];
          finalm.M23 = draw.matrix[1, 2];
          finalm.M24 = draw.matrix[1, 3];
          finalm.M31 = draw.matrix[2, 0];
          finalm.M32 = draw.matrix[2, 1];
          finalm.M33 = draw.matrix[2, 2];
          finalm.M34 = draw.matrix[2, 3];
          finalm.M41 = 0;
          finalm.M42 = 0;
          finalm.M43 = 0;
          finalm.M44 = 1.0f;
          _d3dxSprite.Transform = finalm;
          GUIGraphicsContext.DX9Device.Viewport = draw.viewport;
          float wfactor = ((float) orgView.Width)/(float) draw.viewport.Width;
          float hfactor = ((float) orgView.Height)/(float) draw.viewport.Height;
          float xoffset = (float) (orgView.X - draw.viewport.X);
          float yoffset = (float) (orgView.Y - draw.viewport.Y);
          projm.M11 = (orgProj.M11 + orgProj.M14*xoffset)*wfactor;
          projm.M21 = (orgProj.M21 + orgProj.M24*xoffset)*wfactor;
          projm.M31 = (orgProj.M31 + orgProj.M34*xoffset)*wfactor;
          projm.M41 = (orgProj.M41 + orgProj.M44*xoffset)*wfactor;
          projm.M12 = (orgProj.M12 + orgProj.M14*yoffset)*hfactor;
          projm.M22 = (orgProj.M22 + orgProj.M24*yoffset)*hfactor;
          projm.M32 = (orgProj.M32 + orgProj.M34*yoffset)*hfactor;
          projm.M42 = (orgProj.M42 + orgProj.M44*yoffset)*hfactor;
          GUIGraphicsContext.DX9Device.Transform.View = projm;
          if (GUIGraphicsContext.IsDirectX9ExUsed())
          {
            DrawTextUsingTexture(draw, draw.fontHeight);
          }
          else
          {
            draw.fnt.DrawText(_d3dxSprite, draw.text, new Rectangle((int) draw.xpos,
                                                                    (int) draw.ypos, 0, 0), DrawTextFormat.NoClip,
                              draw.color);
          }

          _d3dxSprite.Flush();
        }

        GUIGraphicsContext.DX9Device.Viewport = orgView;
        GUIGraphicsContext.DX9Device.Transform.View = orgProj;
        _d3dxSprite.End();
        _listDrawText = new List<FontManagerDrawText>();
        _d3dxSpriteUsed = false;
      }
    }

    /// <summary>
    /// Disposes all GUIFonts.
    /// </summary>
    public static void Dispose()
    {
      Log.Info("  fonts.Dispose()");
      foreach (GUIFont font in _listFonts)
      {
        font.Dispose(null, null);
      }

      if (_d3dxSprite != null)
      {
        _d3dxSprite.Dispose();
        _d3dxSprite = null;
        _d3dxSpriteUsed = false;
      }
      _listFontTextures.Clear();
      _listFontObjects.Clear();
    }

    /// <summary>
    /// Sets the device and the FVF.
    /// </summary>
    public static void SetDevice()
    {
      Log.Info("  fonts.SetDevice()");
      IntPtr upDevice = DirectShowUtil.GetUnmanagedDevice(GUIGraphicsContext.DX9Device);

      unsafe
      {
        FontEngineSetDevice(upDevice.ToPointer());
      }
    }

    /// <summary>
    /// Initializes the device objects of the GUIFonts.
    /// </summary>
    public static void InitializeDeviceObjects()
    {
      Log.Info("  fonts.InitializeDeviceObjects()");
      IntPtr upDevice = DirectShowUtil.GetUnmanagedDevice(GUIGraphicsContext.DX9Device);

      unsafe
      {
        FontEngineSetDevice(upDevice.ToPointer());
      }
      foreach (GUIFont font in _listFonts)
      {
        font.InitializeDeviceObjects();
      }
    }
  }
}
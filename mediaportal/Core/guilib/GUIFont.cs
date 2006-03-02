/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using System.Runtime.InteropServices;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// An implementation of the GUIFont class (renders text using DirectX textures).  This implementation generates the necessary textures for rendering the fonts in DirectX in the @skin\skinname\fonts directory.
  /// </summary>
  public class GUIFont
  {
    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineInitialize(int iScreenWidth, int iScreenHeight);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineAddFont(int fontNumber, void* fontTexture, int firstChar, int endChar, float textureScale, float textureWidth, float textureHeight, float fSpacingPerChar, int maxVertices);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineRemoveFont(int fontNumber);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineSetCoordinate(int fontNumber, int index, int subindex, float fValue);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineDrawText3D(int fontNumber, void* text, int xposStart, int yposStart, uint intColor, int maxWidth);


    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEnginePresent3D(int fontNumber);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineSetDevice(void* device);

    // Font rendering flags
    [System.Flags]
    public enum RenderFlags
    {
      Centered = 0x0001,
      TwoSided = 0x0002,
      Filtered = 0x0004,
      DontDiscard = 0x0008
    }
    private System.Drawing.Font systemFont;
    int m_iFontHeight;
    private float[,] textureCoords = null;
    private int spacingPerChar = 0;
    private Direct3D.Texture fontTexture;
    private int textureWidth; // Texture dimensions
    private int textureHeight;
    private float textureScale;
    private FontStyle m_FontStyle = FontStyle.Regular;
    int m_ID = -1;
    bool FontAdded = false;
    private string _fontName;
    private string m_strFileName;
    public const int MaxNumfontVertices = 100 * 6;
    private int _StartCharacter = 32;
    private int _EndCharacter = 255;
    private static bool logfonts = false;
    /// <summary>
    /// Constructor of the GUIFont class.
    /// </summary>
    /// <param name="strName">The name of the font used in the skin. (E.g., debug)</param>
    /// <param name="strFileName">The system name of the font (E.g., Arial)</param>
    /// <param name="iHeight">The height of the font.</param>
    public GUIFont(string strName, string strFileName, int iHeight)
    {
      if (logfonts) Log.Write("GUIFont:ctor({0}) fontengine: Initialize()", strName);
      FontEngineInitialize(GUIGraphicsContext.Width, GUIGraphicsContext.Height);
      _fontName = strName;
      m_strFileName = strFileName;
      m_iFontHeight = iHeight;
    }
    public int ID
    {
      get { return m_ID; }
      set { m_ID = value; }

    }

    /// <summary>
    /// Constructor of the GUIFont class.
    /// </summary>
    /// <param name="strName">The name of the font used in the skin (E.g., debug).</param>
    /// <param name="strFileName">The system name of the font (E.g., Arial).</param>
    /// <param name="iHeight">The height of the font.</param>
    /// <param name="style">The style of the font (E.g., Bold)</param>
    public GUIFont(string strName, string strFileName, int iHeight, FontStyle style)
    {
      if (logfonts) Log.Write("GUIFont:ctor({0}) fontengine: Initialize()", strName);
      FontEngineInitialize(GUIGraphicsContext.Width, GUIGraphicsContext.Height);
      _fontName = strName;
      m_strFileName = strFileName;
      m_FontStyle = style;
      m_iFontHeight = iHeight;
    }

    public void SetRange(int start, int end)
    {
      _StartCharacter = start;
      _EndCharacter = end + 1;
      if (_StartCharacter < 32) _StartCharacter = 32;
    }

    /// <summary>
    /// Get/set the name of the font used in the skin (E.g., debug).
    /// </summary>
    public string FontName
    {
      get { return _fontName; }
      set { _fontName = value; }
    }

    /// <summary>
    /// Get/set the system name of the font (E.g., Arial).
    /// </summary>
    public string FileName
    {
      get { return m_strFileName; }
      set { m_strFileName = value; }
    }

    /// <summary>
    /// Get/set the height of the font.
    /// </summary>
    public int FontSize
    {
      get { return m_iFontHeight; }
      set { m_iFontHeight = value; }
    }

    /// <summary>
    /// Creates a system font.
    /// </summary>
    /// <param name="strFileName">The system font name (E.g., Arial).</param>
    /// <param name="style">The font style.</param>
    /// <param name="Size">The size.</param>
    public void Create(string strFileName, FontStyle style, int Size)
    {
      Dispose(null, null);
      m_strFileName = strFileName;
      m_iFontHeight = Size;
      systemFont = new System.Drawing.Font(m_strFileName, (float)m_iFontHeight, style);
    }

    /// <summary>
    /// Draws text with a maximum width.
    /// </summary>
    /// <param name="xpos">The X position.</param>
    /// <param name="ypos">The Y position.</param>
    /// <param name="color">The font color.</param>
    /// <param name="strLabel">The actual text.</param>
    /// <param name="fMaxWidth">The maximum width.</param>
    public void DrawTextWidth(float xpos, float ypos, long color, string strLabel, float fMaxWidth, GUIControl.Alignment alignment)
    {
      if (fMaxWidth <= 0) return;
      if (xpos <= 0) return;
      if (ypos <= 0) return;
      if (strLabel == null) return;
      if (strLabel.Length == 0) return;
      float fTextWidth = 0, fTextHeight = 0;
      GetTextExtent(strLabel, ref fTextWidth, ref fTextHeight);
      if (fTextWidth <= fMaxWidth)
      {
        DrawText(xpos, ypos, color, strLabel, alignment, (int)fMaxWidth);
        return;
      }
      while (fTextWidth >= fMaxWidth && strLabel.Length > 1)
      {
        if (alignment == GUICheckMarkControl.Alignment.ALIGN_RIGHT)
          strLabel = strLabel.Substring(1);
        else
          strLabel = strLabel.Substring(0, strLabel.Length - 1);
        GetTextExtent(strLabel, ref fTextWidth, ref fTextHeight);
      }
      GetTextExtent(strLabel, ref fTextWidth, ref fTextHeight);
      if (fTextWidth <= fMaxWidth)
      {
        DrawText(xpos, ypos, color, strLabel, alignment, -1);
      }
    }

    /// <summary>
    /// Draws aligned text.
    /// </summary>
    /// <param name="xpos">The X position.</param>
    /// <param name="ypos">The Y position.</param>
    /// <param name="color">The font color.</param>
    /// <param name="strLabel">The actual text.</param>
    /// <param name="alignment">The alignment of the text.</param>
    public void DrawText(float xpos, float ypos, long color, string strLabel, GUIControl.Alignment alignment, int maxWidth)
    {
      if (strLabel == null) return;
      if (strLabel == String.Empty) return;
      if (xpos <= 0) return;
      if (ypos <= 0) return;
      int alpha = (int)((color >> 24) & 0xff);
      int red = (int)((color >> 16) & 0xff);
      int green = (int)((color >> 8) & 0xff);
      int blue = (int)(color & 0xff);


      if (alignment == GUIControl.Alignment.ALIGN_LEFT)
      {
        DrawText(xpos, ypos, Color.FromArgb(alpha, red, green, blue), strLabel, RenderFlags.Filtered, maxWidth);
      }
      else if (alignment == GUIControl.Alignment.ALIGN_RIGHT)
      {
        float fW = 0, fH = 0;
        GetTextExtent(strLabel, ref fW, ref fH);
        DrawText(xpos - fW, ypos, Color.FromArgb(alpha, red, green, blue), strLabel, RenderFlags.Filtered, maxWidth);
      }
    }

    /// <summary>
    /// Draw shadowed text.
    /// </summary>
    /// <param name="fOriginX">The X position.</param>
    /// <param name="fOriginY">The Y position.</param>
    /// <param name="dwColor">The font color.</param>
    /// <param name="strText">The actual text.</param>
    /// <param name="alignment">The alignment of the text.</param>
    /// <param name="iShadowWidth">The width parameter of the shadow.</param>
    /// <param name="iShadowHeight">The height parameter of the shadow.</param>
    /// <param name="dwShadowColor">The shadow color.</param>
    public void DrawShadowText(float fOriginX, float fOriginY, long dwColor,
                                string strText,
                                GUIControl.Alignment alignment,
                                int iShadowWidth,
                                int iShadowHeight,
                                long dwShadowColor)
    {

      for (int x = -iShadowWidth; x < iShadowWidth; x++)
      {
        for (int y = -iShadowHeight; y < iShadowHeight; y++)
        {
          DrawText((float)x + fOriginX, (float)y + fOriginY, dwShadowColor, strText, alignment, -1);
        }
      }
      DrawText(fOriginX, fOriginY, dwColor, strText, alignment, -1);
    }

    public void Present()
    {
      if (!FontAdded)
      {
        if (logfonts) Log.Write("GUIFont:Present() Fontengine  ERROR font not added:" + ID.ToString());
      }
      else if (ID >= 0)
      {
        FontEnginePresent3D(ID);
      }
    }
    /// <summary>
    /// Draw some text on the screen.
    /// </summary>
    /// <param name="xpos">The X position.</param>
    /// <param name="ypos">The Y position.</param>
    /// <param name="color">The font color.</param>
    /// <param name="text">The actual text.</param>
    /// <param name="flags">Font render flags.</param>
    protected void DrawText(float xpos, float ypos, Color color, string text, RenderFlags flags, int maxWidth)
    {
      if (!FontAdded) return;
      if (text == null) return;
      if (text == String.Empty) return;
      if (xpos <= 0) return;
      if (ypos <= 0) return;


      GUIGraphicsContext.Correct(ref xpos, ref ypos);
      if (GUIGraphicsContext.graphics != null)
      {
        GUIGraphicsContext.graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        GUIGraphicsContext.graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;//.AntiAlias;
        GUIGraphicsContext.graphics.DrawString(text, systemFont, new SolidBrush(color), xpos, ypos);
        return;
      }

      int intColor = color.ToArgb();

      if (!FontAdded)
      {
        if (logfonts) Log.Write("GUIFont:DrawText Fontengine ERROR font not added:" + ID.ToString());
        return;
      }
      else if (ID >= 0)
      {
        unsafe
        {
          IntPtr ptrStr = Marshal.StringToCoTaskMemUni(text); //SLOW
          FontEngineDrawText3D(ID, (void*)(ptrStr.ToPointer()), (int)xpos, (int)ypos, (uint)intColor, maxWidth);
          Marshal.FreeCoTaskMem(ptrStr);
          return;
        }
      }

    }

    /// <summary>
    /// Measure the width of a string on the display.
    /// </summary>
    /// <param name="graphics">The graphics context.</param>
    /// <param name="text">The string that needs to be measured.</param>
    /// <param name="font">The font that needs to be used.</param>
    /// <returns>The width of the string.</returns>
    static public int MeasureDisplayStringWidth(Graphics graphics, string text, System.Drawing.Font font)
    {
      const int width = 32;

      System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width, 1, graphics);
      System.Drawing.SizeF size = graphics.MeasureString(text, font);
      System.Drawing.Graphics anagra = System.Drawing.Graphics.FromImage(bitmap);

      int measured_width = (int)size.Width;

      if (anagra != null)
      {
        anagra.Clear(Color.White);
        anagra.DrawString(text + "|", font, Brushes.Black,
          width - measured_width, -font.Height / 2);

        for (int i = width - 1; i >= 0; i--)
        {
          measured_width--;
          if (bitmap.GetPixel(i, 0).R != 255)    // found a non-white pixel ?
            break;
        }
      }
      return measured_width;
    }

    /// <summary>
    /// Get the dimensions of a text string.
    /// </summary>
    /// <param name="text">The actual text.</param>
    /// <returns>The size of the rendered text.</returns>
    public void GetTextExtent(string text, ref float textwidth, ref float textheight)
    {
      textwidth = 0.0f;
      textheight = 0.0f;
      if (!FontAdded) return;
      if (null == text || text == String.Empty) return;

      float fRowWidth = 0.0f;
      float fRowHeight = (textureCoords[0, 3] - textureCoords[0, 1]) * textureHeight;
      textheight = fRowHeight;

      for (int i = 0; i < text.Length; ++i)
      {
        char c = text[i];
        if (c == '\n')
        {
          if (fRowWidth > textwidth)
            textwidth = fRowWidth;
          fRowWidth = 0.0f;
          textheight += fRowHeight;
        }

        if (c < _StartCharacter || c >= _EndCharacter)
          continue;

        float tx1 = textureCoords[c - _StartCharacter, 0];
        float tx2 = textureCoords[c - _StartCharacter, 2];

        fRowWidth += (tx2 - tx1) * textureWidth - 2 * spacingPerChar;
      }

      if (fRowWidth > textwidth)
        textwidth = fRowWidth;
    }

    /// <summary>
    /// Cleanup any resources being used.
    /// </summary>
    public void Dispose(object sender, EventArgs e)
    {
      if (systemFont != null)
        systemFont.Dispose();

      if (fontTexture != null)
        fontTexture.Dispose();

      fontTexture = null;
      systemFont = null;
      if (FontAdded)
      {
        if (logfonts) Log.Write("GUIFont:Dispose({0}) fontengine: Remove font:{1}", _fontName, ID.ToString());
        if (ID >= 0) FontEngineRemoveFont(ID);
      }
      FontAdded = false;
    }

    /// <summary>
    /// Loads a font.
    /// </summary>
    /// <returns>True if loaded succesful.</returns>
    public bool Load()
    {
      Create(m_strFileName, m_FontStyle, m_iFontHeight);
      return true;
    }

    /// <summary>
    /// Initialize the device objects.
    /// </summary>
    public void InitializeDeviceObjects()
    {
      ReloadFont();
    }

    void ReloadFont()
    {
      textureScale = 1.0f; // Draw fonts into texture without scaling


      // Create a directory to cache the font bitmaps
      string strCache = String.Format(@"{0}\fonts\", GUIGraphicsContext.Skin);
      try
      {
        System.IO.Directory.CreateDirectory(strCache);
      }
      catch (Exception) { }
      strCache = String.Format(@"{0}\fonts\{1}_{2}.png", GUIGraphicsContext.Skin, _fontName, m_iFontHeight);

      // If the cached bitmap file exists load from file.
      if (System.IO.File.Exists(strCache))
      {
        bool bExists = true;

        if (File.Exists(strCache + "_2.xml"))
        {
          try
          {
            using (Stream r = File.Open(strCache + "_2.xml", FileMode.Open, FileAccess.Read))
            {
              // deserialize persons
              SoapFormatter c = new SoapFormatter();
              try
              {
                textureCoords = (float[,])c.Deserialize(r);
              }
              catch (Exception)
              {
                bExists = false;
              }
              int iLen = textureCoords.GetLength(0);
              if (iLen != 10 + _EndCharacter - _StartCharacter)
              {
                bExists = false;
              }
              r.Close();
            }
          }
          catch (Exception)
          {
            bExists = false;
          }
        }
        if (bExists && textureCoords != null)
        {
          bool SupportsCompressedTextures = Manager.CheckDeviceFormat(GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal,
                                                                    GUIGraphicsContext.DX9Device.DeviceCaps.DeviceType,
                                                                    GUIGraphicsContext.DX9Device.DisplayMode.Format,
                                                                    Usage.None,
                                                                    ResourceType.Textures,
                                                                    Format.Dxt3);
          Format fmt = Format.Unknown;
          if (SupportsCompressedTextures) fmt = Format.Dxt3;
          spacingPerChar = (int)textureCoords[_EndCharacter - _StartCharacter, 0];

          // load coords
          ImageInformation info = new ImageInformation();
          fontTexture = TextureLoader.FromFile(GUIGraphicsContext.DX9Device,
                                            strCache,
                                            0, 0, //width/height
                                            1,//miplevels
                                            0,
                                            fmt,
                                            Pool.Managed,
                                            Filter.None,
                                            Filter.None,
                                            (int)0,
                                            ref info);


          textureHeight = info.Height;
          textureWidth = info.Width;
          RestoreDeviceObjects();
          Log.Write("  Loaded font:{0} height:{1} texture:{2}x{3} chars:[{4}-{5}] miplevels:{6}",
            _fontName, m_iFontHeight, textureWidth, textureWidth, _StartCharacter, _EndCharacter, fontTexture.LevelCount);
          SetFontEgine();
          return;
        }
      }
      // If not generate it.
      textureCoords = new float[(10 + _EndCharacter - _StartCharacter), 4];

      // Create a bitmap on which to measure the alphabet
      Bitmap bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      Graphics g = Graphics.FromImage(bmp);
      g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
      g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
      g.TextContrast = 0;

      // Establish the font and texture size
      textureScale = 1.0f; // Draw fonts into texture without scaling

      // Calculate the dimensions for the smallest power-of-two texture which
      // can hold all the printable characters
      textureWidth = textureHeight = 256;
      for (; ; )
      {
        try
        {
          // Measure the alphabet
          PaintAlphabet(g, true);
        }
        catch (System.InvalidOperationException)
        {
          // Scale up the texture size and try again
          textureWidth *= 2;
          textureHeight *= 2;
          continue;
        }

        break;
      }

      // If requested texture is too big, use a smaller texture and smaller font,
      // and scale up when rendering.
      Direct3D.Caps d3dCaps = GUIGraphicsContext.DX9Device.DeviceCaps;

      // If the needed texture is too large for the video card...
      if (textureWidth > d3dCaps.MaxTextureWidth)
      {
        // Scale the font size down to fit on the largest possible texture
        textureScale = (float)d3dCaps.MaxTextureWidth / (float)textureWidth;
        textureWidth = textureHeight = d3dCaps.MaxTextureWidth;

        for (; ; )
        {
          // Create a new, smaller font
          m_iFontHeight = (int)Math.Floor(m_iFontHeight * textureScale);
          systemFont = new System.Drawing.Font(systemFont.Name, m_iFontHeight, systemFont.Style);

          try
          {
            // Measure the alphabet
            PaintAlphabet(g, true);
          }
          catch (System.InvalidOperationException)
          {
            // If that still doesn't fit, scale down again and continue
            textureScale *= 0.9F;
            continue;
          }

          break;
        }
      }
      bmp.Dispose();

      Trace.WriteLine("font:" + _fontName + " " + m_strFileName + " height:" + m_iFontHeight.ToString() + " " + textureWidth.ToString() + "x" + textureHeight.ToString());
      // Release the bitmap used for measuring and create one for drawing

      using (bmp = new Bitmap(textureWidth, textureHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
      {
        using (g = Graphics.FromImage(bmp))
        {
          g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
          g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
          g.TextContrast = 0;

          // Draw the alphabet
          PaintAlphabet(g, false);

          // Create a new texture for the font from the bitmap we just created
          try
          {
            fontTexture = Texture.FromBitmap(GUIGraphicsContext.DX9Device, bmp, 0, Pool.Managed);
            bmp.Save(strCache);
            textureCoords[_EndCharacter - _StartCharacter, 0] = spacingPerChar;
            try
            {
              System.IO.File.Delete(strCache + "_2.xml");
            }
            catch (Exception) { }

            using (Stream s = File.Open(strCache + "_2.xml", FileMode.CreateNew, FileAccess.ReadWrite))
            {
              SoapFormatter b = new SoapFormatter();
              b.Serialize(s, (object)textureCoords);
              s.Close();
            }
          }
          catch (Exception ex)
          {
            string strLine = ex.Message;
          }
        }
      }
      SetFontEgine();
    }

    public void RestoreDeviceObjects()
    {
    }
    /// <summary>
    /// Restore the font after a device has been reset.
    /// </summary>
    public void SetFontEgine()
    {
      if (FontAdded) return;
      if (ID < 0) return;
      Surface surf = GUIGraphicsContext.DX9Device.GetRenderTarget(0);

      if (logfonts) Log.Write("GUIFont:RestoreDeviceObjects() fontengine: add font:" + ID.ToString());
      IntPtr upTexture = DShowNET.Helper.DirectShowUtil.GetUnmanagedTexture(fontTexture);
      unsafe
      {
        FontEngineAddFont(ID, upTexture.ToPointer(), _StartCharacter, _EndCharacter, textureScale, textureWidth, textureHeight, spacingPerChar, MaxNumfontVertices);
      }

      int length = textureCoords.GetLength(0);
      for (int i = 0; i < length; ++i)
      {
        FontEngineSetCoordinate(ID, i, 0, textureCoords[i, 0]);
        FontEngineSetCoordinate(ID, i, 1, textureCoords[i, 1]);
        FontEngineSetCoordinate(ID, i, 2, textureCoords[i, 2]);
        FontEngineSetCoordinate(ID, i, 3, textureCoords[i, 3]);
      }
      FontAdded = true;

    }

    /// <summary>
    /// Attempt to draw the systemFont alphabet onto the provided texture
    /// graphics.
    /// </summary>
    /// <param name="g">Graphics object on which to draw and measure the letters</param>
    /// <param name="measureOnly">If set, the method will test to see if the alphabet will fit without actually drawing</param>
    public void PaintAlphabet(Graphics g, bool measureOnly)
    {
      string str;
      float x = 0;
      float y = 0;
      Point p = new Point(0, 0);
      Size size = new Size(0, 0);

      // Calculate the spacing between characters based on line height
      size = g.MeasureString(" ", systemFont).ToSize();
      //x = spacingPerChar = (int) Math.Ceiling(size.Height * 0.3);
      spacingPerChar = (int)Math.Ceiling(size.Width * 0.4);
      x = 0;

      for (char c = (char)_StartCharacter; c < (char)_EndCharacter; c++)
      {
        str = c.ToString();
        // We need to do some things here to get the right sizes.  The default implemententation of MeasureString
        // will return a resolution independant size.  For our height, this is what we want.  However, for our width, we 
        // want a resolution dependant size.
        Size resSize = g.MeasureString(str, systemFont).ToSize();
        size.Height = resSize.Height + 1;

        // Now the Resolution independent width
        if (c != ' ') // We need the special case here because a space has a 0 width in GenericTypoGraphic stringformats
        {
          resSize = g.MeasureString(str, systemFont, p, StringFormat.GenericTypographic).ToSize();
          size.Width = resSize.Width;
        }
        else
          size.Width = resSize.Width;

        if ((x + size.Width + spacingPerChar) > textureWidth)
        {
          x = spacingPerChar;
          y += size.Height;
        }

        // Make sure we have room for the current character
        if ((y + size.Height) > textureHeight)
          throw new System.InvalidOperationException("Texture too small for alphabet");

        if (!measureOnly)
        {
          if (c != ' ') // We need the special case here because a space has a 0 width in GenericTypoGraphic stringformats
            g.DrawString(str, systemFont, Brushes.White, new Point((int)x, (int)y), StringFormat.GenericTypographic);
          else
            g.DrawString(str, systemFont, Brushes.White, new Point((int)x, (int)y));
          textureCoords[c - _StartCharacter, 0] = ((float)(x + 0 - spacingPerChar)) / textureWidth;
          textureCoords[c - _StartCharacter, 1] = ((float)(y + 0 + 0)) / textureHeight;
          textureCoords[c - _StartCharacter, 2] = ((float)(x + size.Width + spacingPerChar)) / textureWidth;
          textureCoords[c - _StartCharacter, 3] = ((float)(y + size.Height + 0)) / textureHeight;
        }

        x += size.Width + (2 * spacingPerChar);
      }
    }
  }
}

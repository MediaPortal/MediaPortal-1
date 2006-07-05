/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using System.Runtime.InteropServices;
using MediaPortal.Utils.Services;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// An implementation of the GUIFont class (renders text using DirectX textures).  This implementation generates the necessary textures for rendering the fonts in DirectX in the @skin\skinname\fonts directory.
  /// </summary>
  public class GUIFont
  {
    #region imports
    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineInitialize(int iScreenWidth, int iScreenHeight);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineAddFont(int fontNumber, void* fontTexture, int firstChar, int endChar, float textureScale, float textureWidth, float textureHeight, float fSpacingPerChar, int maxVertices);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineRemoveFont(int fontNumber);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineSetCoordinate(int fontNumber, int index, int subindex, float fValue1, float fValue2, float fValue3, float fValue4);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineDrawText3D(int fontNumber, void* text, int xposStart, int yposStart, uint intColor, int maxWidth);


    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEnginePresent3D(int fontNumber);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineSetDevice(void* device);
    #endregion

    #region enums
    // Font rendering flags
    [System.Flags]
    public enum RenderFlags
    {
      Centered = 0x0001,
      TwoSided = 0x0002,
      Filtered = 0x0004,
      DontDiscard = 0x0008
    }
    #endregion

    #region variables
    private System.Drawing.Font _systemFont;
    int _fontHeight;
    private float[,] _textureCoords = null;
    private int _spacingPerChar = 0;
    private Direct3D.Texture _textureFont;
    private int _textureWidth; // Texture dimensions
    private int _textureHeight;
    private float _textureScale;
    private FontStyle _fontStyle = FontStyle.Regular;
    int _fontId = -1;
    bool _fontAdded = false;
    private string _fontName;
    private string _fileName;
    public const int MaxNumfontVertices = 100 * 6;
    private int _StartCharacter = 32;
    private int _EndCharacter = 255;
    private static bool logfonts = false;
    private ILog _log;
    #endregion

    #region ctors
    /// <summary>
    /// Constructor of the GUIFont class.
    /// </summary>
    public GUIFont()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }
    /// <summary>
    /// Constructor of the GUIFont class.
    /// </summary>
    /// <param name="strName">The name of the font used in the skin. (E.g., debug)</param>
    /// <param name="strFileName">The system name of the font (E.g., Arial)</param>
    /// <param name="iHeight">The height of the font.</param>
    public GUIFont(string fontName, string fileName, int fontHeight)
      : this()
    {
      if (logfonts) _log.Info("GUIFont:ctor({0}) fontengine: Initialize()", fontName);
      FontEngineInitialize(GUIGraphicsContext.Width, GUIGraphicsContext.Height);
      _fontName = fontName;
      _fileName = fileName;
      _fontHeight = fontHeight;
    }

    /// <summary>
    /// Constructor of the GUIFont class.
    /// </summary>
    /// <param name="strName">The name of the font used in the skin (E.g., debug).</param>
    /// <param name="strFileName">The system name of the font (E.g., Arial).</param>
    /// <param name="iHeight">The height of the font.</param>
    /// <param name="style">The style of the font (E.g., Bold)</param>
    public GUIFont(string fontName, string fileName, int iHeight, FontStyle style)
      : this()
    {
      if (logfonts) _log.Info("GUIFont:ctor({0}) fontengine: Initialize()", fontName);
      FontEngineInitialize(GUIGraphicsContext.Width, GUIGraphicsContext.Height);
      _fontName = fontName;
      _fileName = fileName;
      _fontStyle = style;
      _fontHeight = iHeight;
    }
    #endregion

    public int ID
    {
      get { return _fontId; }
      set { _fontId = value; }

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
      get { return _fileName; }
      set { _fileName = value; }
    }

    /// <summary>
    /// Get/set the height of the font.
    /// </summary>
    public int FontSize
    {
      get { return _fontHeight; }
      set { _fontHeight = value; }
    }

    /// <summary>
    /// Creates a system font.
    /// </summary>
    /// <param name="strFileName">The system font name (E.g., Arial).</param>
    /// <param name="style">The font style.</param>
    /// <param name="Size">The size.</param>
    public void Create(string fileName, FontStyle style, int Size)
    {
      Dispose(null, null);
      _fileName = fileName;
      _fontHeight = Size;
      _systemFont = new System.Drawing.Font(_fileName, (float)_fontHeight, style);
    }

    /// <summary>
    /// Draws text with a maximum width.
    /// </summary>
    /// <param name="xpos">The X position.</param>
    /// <param name="ypos">The Y position.</param>
    /// <param name="color">The font color.</param>
    /// <param name="strLabel">The actual text.</param>
    /// <param name="fMaxWidth">The maximum width.</param>
    public void DrawTextWidth(float xpos, float ypos, long color, string label, float fMaxWidth, GUIControl.Alignment alignment)
    {
      if (fMaxWidth <= 0) return;
      if (xpos <= 0) return;
      if (ypos <= 0) return;
      if (label == null) return;
      if (label.Length == 0) return;
      float fTextWidth = 0, fTextHeight = 0;
      GetTextExtent(label, ref fTextWidth, ref fTextHeight);
      if (fTextWidth <= fMaxWidth)
      {
        DrawText(xpos, ypos, color, label, alignment, (int)fMaxWidth);
        return;
      }
      while (fTextWidth >= fMaxWidth && label.Length > 1)
      {
        if (alignment == GUICheckMarkControl.Alignment.ALIGN_RIGHT)
          label = label.Substring(1);
        else
          label = label.Substring(0, label.Length - 1);
        GetTextExtent(label, ref fTextWidth, ref fTextHeight);
      }
      GetTextExtent(label, ref fTextWidth, ref fTextHeight);
      if (fTextWidth <= fMaxWidth)
      {
        DrawText(xpos, ypos, color, label, alignment, -1);
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
    public void DrawText(float xpos, float ypos, long color, string label, GUIControl.Alignment alignment, int maxWidth)
    {
      if (label == null) return;
      if (label.Length == 0) return;
      if (xpos <= 0) return;
      if (ypos <= 0) return;
      int alpha = (int)((color >> 24) & 0xff);
      int red = (int)((color >> 16) & 0xff);
      int green = (int)((color >> 8) & 0xff);
      int blue = (int)(color & 0xff);


      if (alignment == GUIControl.Alignment.ALIGN_LEFT)
      {
        DrawText(xpos, ypos, Color.FromArgb(alpha, red, green, blue), label, RenderFlags.Filtered, maxWidth);
      }
      else if (alignment == GUIControl.Alignment.ALIGN_RIGHT)
      {
        float fW = 0, fH = 0;
        GetTextExtent(label, ref fW, ref fH);
        DrawText(xpos - fW, ypos, Color.FromArgb(alpha, red, green, blue), label, RenderFlags.Filtered, maxWidth);
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
      if (!_fontAdded)
      {
        Restore();
        return;
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
      if (!_fontAdded)
      {
        Restore();
        return;
      }
      if (text == null) return;
      if (text.Length == 0) return;
      if (xpos <= 0) return;
      if (ypos <= 0) return;
      if (maxWidth < -1) return;

      GUIGraphicsContext.Correct(ref xpos, ref ypos);
      if (GUIGraphicsContext.graphics != null)
      {
        GUIGraphicsContext.graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        GUIGraphicsContext.graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;//.AntiAlias;
        GUIGraphicsContext.graphics.DrawString(text, _systemFont, new SolidBrush(color), xpos, ypos);
        return;
      }


      if (ID >= 0)
      {
        int intColor = color.ToArgb();
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
      if (!_fontAdded)
      {
        Restore();
        return;
      }
      if (null == text || text == String.Empty) return;

      float fRowWidth = 0.0f;
      float fRowHeight = (_textureCoords[0, 3] - _textureCoords[0, 1]) * _textureHeight;
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

        float tx1 = _textureCoords[c - _StartCharacter, 0];
        float tx2 = _textureCoords[c - _StartCharacter, 2];

        fRowWidth += (tx2 - tx1) * _textureWidth - 2 * _spacingPerChar;
      }

      if (fRowWidth > textwidth)
        textwidth = fRowWidth;
    }

    /// <summary>
    /// Cleanup any resources being used.
    /// </summary>
    public void Dispose(object sender, EventArgs e)
    {
      if (_systemFont != null)
        _systemFont.Dispose();

      if (_textureFont != null)
      {
        _textureFont.Disposing -= new EventHandler(_textureFont_Disposing);
        _textureFont.Dispose();
      }
      _textureFont = null;
      _systemFont = null;
      if (_fontAdded)
      {
        if (logfonts) _log.Info("GUIFont:Dispose({0}) fontengine: Remove font:{1}", _fontName, ID.ToString());
        if (ID >= 0) FontEngineRemoveFont(ID);
      }
      _fontAdded = false;
    }

    /// <summary>
    /// Loads a font.
    /// </summary>
    /// <returns>True if loaded succesful.</returns>
    public bool Load()
    {
      Create(_fileName, _fontStyle, _fontHeight);
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
      _textureScale = 1.0f; // Draw fonts into texture without scaling

      int colorKey = 0;
      System.Drawing.Imaging.PixelFormat pixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
      // Create a directory to cache the font bitmaps
      string strCache = String.Format(@"{0}\fonts\", GUIGraphicsContext.Skin);
      try
      {
        System.IO.Directory.CreateDirectory(strCache);
      }
      catch (Exception) { }
      strCache = String.Format(@"{0}\fonts\{1}_{2}.png", GUIGraphicsContext.Skin, _fontName, _fontHeight);

      // If the cached bitmap file exists load from file.

      bool SupportsCompressedTextures = Manager.CheckDeviceFormat(GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal,
                                                                GUIGraphicsContext.DX9Device.DeviceCaps.DeviceType,
                                                                GUIGraphicsContext.DX9Device.DisplayMode.Format,
                                                                Usage.None,
                                                                ResourceType.Textures,
                                                                Format.Dxt3);
      if (System.IO.File.Exists(strCache))
      {
        bool bExists = true;

        if (File.Exists(strCache + "_2.bxml"))
        {
          try
          {
            using (Stream r = File.Open(strCache + "_2.bxml", FileMode.Open, FileAccess.Read))
            {
              // deserialize persons
              BinaryFormatter c = new BinaryFormatter();
              try
              {
                _textureCoords = (float[,])c.Deserialize(r);
              }
              catch (Exception)
              {
                bExists = false;
              }
              int iLen = _textureCoords.GetLength(0);
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
        if (bExists && _textureCoords != null)
        {
          Format fmt = Format.Unknown;
          if (SupportsCompressedTextures) fmt = Format.Dxt3;
          _spacingPerChar = (int)_textureCoords[_EndCharacter - _StartCharacter, 0];

          // load coords
          ImageInformation info = new ImageInformation();
          _textureFont = TextureLoader.FromFile(GUIGraphicsContext.DX9Device,
                                            strCache,
                                            0, 0, //width/height
                                            1,//miplevels
                                            0,
                                            fmt,
                                            Pool.Managed,
                                            Filter.None,
                                            Filter.None,
                                            colorKey,
                                            ref info);

          _textureFont.Disposing += new EventHandler(_textureFont_Disposing);
          _textureHeight = info.Height;
          _textureWidth = info.Width;
          RestoreDeviceObjects();
          _log.Info("  Loaded font:{0} height:{1} texture:{2}x{3} chars:[{4}-{5}] miplevels:{6}",
            _fontName, _fontHeight, _textureWidth, _textureWidth, _StartCharacter, _EndCharacter, _textureFont.LevelCount);
          SetFontEgine();
          return;
        }
      }
      // If not generate it.
      _textureCoords = new float[(10 + _EndCharacter - _StartCharacter), 4];

      // Create a bitmap on which to measure the alphabet
      using (Bitmap bmp = new Bitmap(1, 1, pixelFormat))
      {
        using (Graphics g = Graphics.FromImage(bmp))
        {
          g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
          g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
          g.TextContrast = 0;

          // Establish the font and texture size
          _textureScale = 1.0f; // Draw fonts into texture without scaling

          // Calculate the dimensions for the smallest power-of-two texture which
          // can hold all the printable characters
          _textureWidth = _textureHeight = 256;
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
              _textureWidth *= 2;
              _textureHeight *= 2;
              continue;
            }
            break;
          }

          // If requested texture is too big, use a smaller texture and smaller font,
          // and scale up when rendering.
          Direct3D.Caps d3dCaps = GUIGraphicsContext.DX9Device.DeviceCaps;

          // If the needed texture is too large for the video card...
          if (_textureWidth > d3dCaps.MaxTextureWidth)
          {
            // Scale the font size down to fit on the largest possible texture
            _textureScale = (float)d3dCaps.MaxTextureWidth / (float)_textureWidth;
            _textureWidth = _textureHeight = d3dCaps.MaxTextureWidth;

            for (; ; )
            {
              // Create a new, smaller font
              _fontHeight = (int)Math.Floor(_fontHeight * _textureScale);
              _systemFont = new System.Drawing.Font(_systemFont.Name, _fontHeight, _systemFont.Style);

              try
              {
                // Measure the alphabet
                PaintAlphabet(g, true);
              }
              catch (System.InvalidOperationException)
              {
                // If that still doesn't fit, scale down again and continue
                _textureScale *= 0.9F;
                continue;
              }

              break;
            }
          }
        }
      }


      Trace.WriteLine("font:" + _fontName + " " + _fileName + " height:" + _fontHeight.ToString() + " " + _textureWidth.ToString() + "x" + _textureHeight.ToString());
      // Release the bitmap used for measuring and create one for drawing

      using (Bitmap bmp = new Bitmap(_textureWidth, _textureHeight, pixelFormat))
      {
        using (Graphics g = Graphics.FromImage(bmp))
        {
          g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
          g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
          g.TextContrast = 0;

          // Draw the alphabet
          PaintAlphabet(g, false);

          // Create a new texture for the font from the bitmap we just created
          try
          {
            bmp.Save(strCache);
            Format fmt = Format.Unknown;
            if (SupportsCompressedTextures) fmt = Format.Dxt3;

            ImageInformation info = new ImageInformation();
            _textureFont = TextureLoader.FromFile(GUIGraphicsContext.DX9Device,
                                              strCache,
                                              0, 0, //width/height
                                              1,//miplevels
                                              0,
                                              fmt,
                                              Pool.Managed,
                                              Filter.None,
                                              Filter.None,
                                              (int)colorKey,
                                              ref info);
            _textureFont.Disposing += new EventHandler(_textureFont_Disposing);
            _textureCoords[_EndCharacter - _StartCharacter, 0] = _spacingPerChar;
            try
            {
              System.IO.File.Delete(strCache + "_2.bxml");
            }
            catch (Exception) { }

            using (Stream s = File.Open(strCache + "_2.bxml", FileMode.CreateNew, FileAccess.ReadWrite))
            {
              BinaryFormatter b = new BinaryFormatter();
              b.Serialize(s, (object)_textureCoords);
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

    void _textureFont_Disposing(object sender, EventArgs e)
    {
      _log.Info("GUIFont:texture disposing:{0} {1}", ID, _fontName);
      _textureFont = null;
      if (_fontAdded && ID >= 0)
      {
        FontEngineRemoveFont(ID);
      }
      _fontAdded = false;
    }
    void Restore()
    {
      _log.Info("GUIFont:restore font:{0} {1}", ID, _fontName);
      Format fmt = Format.Unknown;
      int colorKey = 0;
      string strCache = String.Format(@"{0}\fonts\{1}_{2}.png", GUIGraphicsContext.Skin, _fontName, _fontHeight);
      ImageInformation info = new ImageInformation();
      _textureFont = TextureLoader.FromFile(GUIGraphicsContext.DX9Device,
                                        strCache,
                                        0, 0, //width/height
                                        1,//miplevels
                                        0,
                                        fmt,
                                        Pool.Managed,
                                        Filter.None,
                                        Filter.None,
                                        colorKey,
                                        ref info);
      _textureFont.Disposing += new EventHandler(_textureFont_Disposing);
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
      if (_fontAdded) return;
      if (ID < 0) return;
      Surface surf = GUIGraphicsContext.DX9Device.GetRenderTarget(0);

      if (logfonts) _log.Info("GUIFont:RestoreDeviceObjects() fontengine: add font:" + ID.ToString());
      IntPtr upTexture = DShowNET.Helper.DirectShowUtil.GetUnmanagedTexture(_textureFont);
      unsafe
      {
        FontEngineAddFont(ID, upTexture.ToPointer(), _StartCharacter, _EndCharacter, _textureScale, _textureWidth, _textureHeight, _spacingPerChar, MaxNumfontVertices);
      }

      int length = _textureCoords.GetLength(0);
      for (int i = 0; i < length; ++i)
      {
        FontEngineSetCoordinate(ID, i, 0, _textureCoords[i, 0], _textureCoords[i, 1], _textureCoords[i, 2], _textureCoords[i, 3]);
      }
      _fontAdded = true;

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
      size = g.MeasureString(" ", _systemFont).ToSize();
      //x = spacingPerChar = (int) Math.Ceiling(size.Height * 0.3);
      _spacingPerChar = (int)Math.Ceiling(size.Width * 0.4);
      x = 0;

      for (char c = (char)_StartCharacter; c < (char)_EndCharacter; c++)
      {
        str = c.ToString();
        // We need to do some things here to get the right sizes.  The default implemententation of MeasureString
        // will return a resolution independant size.  For our height, this is what we want.  However, for our width, we 
        // want a resolution dependant size.
        Size resSize = g.MeasureString(str, _systemFont).ToSize();
        size.Height = resSize.Height + 1;

        // Now the Resolution independent width
        if (c != ' ') // We need the special case here because a space has a 0 width in GenericTypoGraphic stringformats
        {
          resSize = g.MeasureString(str, _systemFont, p, StringFormat.GenericTypographic).ToSize();
          size.Width = resSize.Width;
        }
        else
          size.Width = resSize.Width;

        if ((x + size.Width + _spacingPerChar) > _textureWidth)
        {
          x = _spacingPerChar;
          y += size.Height;
        }

        // Make sure we have room for the current character
        if ((y + size.Height) > _textureHeight)
          throw new System.InvalidOperationException("Texture too small for alphabet");

        if (!measureOnly)
        {
          if (c != ' ') // We need the special case here because a space has a 0 width in GenericTypoGraphic stringformats
            g.DrawString(str, _systemFont, Brushes.White, new Point((int)x, (int)y), StringFormat.GenericTypographic);
          else
            g.DrawString(str, _systemFont, Brushes.White, new Point((int)x, (int)y));
          _textureCoords[c - _StartCharacter, 0] = ((float)(x + 0 - _spacingPerChar)) / _textureWidth;
          _textureCoords[c - _StartCharacter, 1] = ((float)(y + 0 + 0)) / _textureHeight;
          _textureCoords[c - _StartCharacter, 2] = ((float)(x + size.Width + _spacingPerChar)) / _textureWidth;
          _textureCoords[c - _StartCharacter, 3] = ((float)(y + size.Height + 0)) / _textureHeight;
        }

        x += size.Width + (2 * _spacingPerChar);
      }
    }
  }
}

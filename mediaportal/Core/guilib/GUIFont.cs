#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using Microsoft.DirectX.Direct3D;
using Filter = Microsoft.DirectX.Direct3D.Filter;
using Font = System.Drawing.Font;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// An implementation of the GUIFont class (renders text using DirectX textures).  This implementation generates the necessary textures for rendering the fonts in DirectX in the @skin\skinname\fonts directory.
  /// </summary>
  public class GUIFont
  {
    #region imports

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineInitialize(int iScreenWidth, int iScreenHeight, int poolFormat);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineAddFont(int fontNumber, void* fontTexture, int firstChar, int endChar,
                                                        float textureScale, float textureWidth, float textureHeight,
                                                        float fSpacingPerChar, int maxVertices);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineRemoveFont(int fontNumber);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineSetCoordinate(int fontNumber, int index, int subindex, float fValue1,
                                                              float fValue2, float fValue3, float fValue4);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineDrawText3D(int fontNumber, void* text, int xposStart, int yposStart,
                                                           uint intColor, int maxWidth, float[,] matrix);


    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEnginePresent3D(int fontNumber);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineSetDevice(void* device);

    #endregion

    #region enums

    // Font rendering flags
    [Flags]
    public enum RenderFlags
    {
      Centered = 0x0001,
      TwoSided = 0x0002,
      Filtered = 0x0004,
      DontDiscard = 0x0008
    }

    #endregion

    #region variables

    private Font _systemFont;
    private int _fontHeight;
    private float[,] _textureCoords = null;
    private int _spacingPerChar = 0;
    private Texture _textureFont;
    private int _textureWidth; // Texture dimensions
    private int _textureHeight;
    private float _textureScale;
    private FontStyle _fontStyle = FontStyle.Regular;
    private int _fontId = -1;
    private bool _fontAdded = false;
    private string _fontName;
    private string _fileName;
    public const int MaxNumfontVertices = 100 * 6;
    private int _StartCharacter = 32;
    private int _EndCharacter = 255;
    private bool _useRTLLang = false;
    private Microsoft.DirectX.Direct3D.Font _d3dxFont;

    #endregion

    #region ctors

    /// <summary>
    /// Constructor of the GUIFont class.
    /// </summary>
    public GUIFont()
    {
      LoadSettings();
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
      //Log.Debug("GUIFont:ctor({0}) fontengine: Initialize()", fontName);
      FontEngineInitialize(GUIGraphicsContext.Width, GUIGraphicsContext.Height,
                           (int)GUIGraphicsContext.GetTexturePoolType());
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
      //Log.Debug("GUIFont:ctor({0}) fontengine: Initialize()", fontName);
      FontEngineInitialize(GUIGraphicsContext.Width, GUIGraphicsContext.Height,
                           (int)GUIGraphicsContext.GetTexturePoolType());
      _fontName = fontName;
      _fileName = fileName;
      _fontStyle = style;
      _fontHeight = iHeight;
    }

    #endregion

    private void LoadSettings()
    {
      // Some users have english systems but use RTL text.. System.Globalization.CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
      using (Settings xmlreader = new MPSettings())
      {
        _useRTLLang = xmlreader.GetValueAsBool("general", "rtllang", false);
      }
    }

    public int ID
    {
      get { return _fontId; }
      set { _fontId = value; }
    }

    public void SetRange(int start, int end)
    {
      _StartCharacter = start;
      _EndCharacter = end + 1;
      if (_StartCharacter < 32)
      {
        _StartCharacter = 32;
      }
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
    /// Get/set the style of the font.
    /// </summary>
    public FontStyle FontStyle
    {
      get { return _fontStyle; }
      set { _fontStyle = value; }
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
      _systemFont = new Font(_fileName, (float)_fontHeight, style);
    }

    /// <summary>
    /// Draws text with a maximum width.
    /// </summary>
    /// <param name="xpos">The X position.</param>
    /// <param name="ypos">The Y position.</param>
    /// <param name="color">The font color.</param>
    /// <param name="strLabel">The actual text.</param>
    /// <param name="fMaxWidth">The maximum width.</param>
    public void DrawTextWidth(float xpos, float ypos, long color, string label, float fMaxWidth,
                              GUIControl.Alignment alignment)
    {
      if (fMaxWidth <= 0)
      {
        return;
      }
      if (xpos <= 0)
      {
        return;
      }
      if (ypos <= 0)
      {
        return;
      }
      if (label == null)
      {
        return;
      }
      if (label.Length == 0)
      {
        return;
      }
      float fTextWidth = 0, fTextHeight = 0;
      GetTextExtent(label, ref fTextWidth, ref fTextHeight);
      if (fTextWidth <= fMaxWidth)
      {
        DrawText(xpos, ypos, color, label, alignment, (int)fMaxWidth);
        return;
      }
      while (fTextWidth >= fMaxWidth && label.Length > 1)
      {
        if (alignment == GUIControl.Alignment.ALIGN_RIGHT)
        {
          label = label.Substring(1);
        }
        else
        {
          label = label.Substring(0, label.Length - 1);
        }
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
      if (label == null)
      {
        return;
      }
      if (label.Length == 0)
      {
        return;
      }
      if (xpos <= 0)
      {
        return;
      }
      if (ypos <= 0)
      {
        return;
      }
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
      else if (alignment == GUIControl.Alignment.ALIGN_CENTER)
      {
        float fW = 0, fH = 0;
        GetTextExtent(label, ref fW, ref fH);
        int off = (int)((maxWidth - fW) / 2);
        if (off < 0)
        {
          off = 0;
        }
        DrawText(xpos + off, ypos, Color.FromArgb(alpha, red, green, blue), label, RenderFlags.Filtered, maxWidth);
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
    /// <param name="iShadowAngle">The angle parameter of the shadow; zero degrees along x-axis.</param>
    /// <param name="iShadowDistance">The distance parameter of the shadow.</param>
    /// <param name="dwShadowColor">The shadow color.</param>
    public void DrawShadowText(float fOriginX, float fOriginY, long dwColor,
                               string strText,
                               GUIControl.Alignment alignment,
                               int iShadowAngle,
                               int iShadowDistance,
                               long dwShadowColor)
    {
      // Draw the shadow
      float fShadowX =
        (float)Math.Round((double)iShadowDistance * Math.Cos(ConvertDegreesToRadians((double)iShadowAngle)));
      float fShadowY =
        (float)Math.Round((double)iShadowDistance * Math.Sin(ConvertDegreesToRadians((double)iShadowAngle)));
      DrawText(fOriginX + fShadowX, fOriginY + fShadowY, dwShadowColor, strText, alignment, -1);

      // Draw the text
      DrawText(fOriginX, fOriginY, dwColor, strText, alignment, -1);
    }

    public void DrawShadowTextWidth(float fOriginX, float fOriginY, long dwColor,
                                    string strText,
                                    GUIControl.Alignment alignment,
                                    int iShadowAngle,
                                    int iShadowDistance,
                                    long dwShadowColor,
                                    float fMaxWidth)
    {
      // Draw the shadow
      float fShadowX =
        (float)Math.Round((double)iShadowDistance * Math.Cos(ConvertDegreesToRadians((double)iShadowAngle)));
      float fShadowY =
        (float)Math.Round((double)iShadowDistance * Math.Sin(ConvertDegreesToRadians((double)iShadowAngle)));
      DrawTextWidth(fOriginX + fShadowX, fOriginY + fShadowY, dwShadowColor, strText, fMaxWidth, alignment);

      // Draw the text
      DrawTextWidth(fOriginX, fOriginY, dwColor, strText, fMaxWidth, alignment);
    }

    private static double ConvertDegreesToRadians(double degrees)
    {
      double radians = (Math.PI / 180) * degrees;
      return (radians);
    }

    public void Present()
    {
      if (ID >= 0)
      {
        FontEnginePresent3D(ID);
      }
    }

    #region RTL handling

    private string reverse(string a)
    {
      string temp = "";
      string flipsource = "()[]{}<>";
      string fliptarget = ")(][}{><";

      int i, j;
      for (j = 0, i = a.Length - 1; i >= 0; i--, j++)
      {
        if (flipsource.Contains(a[i].ToString()))
        {
          temp += fliptarget[flipsource.IndexOf(a[i])].ToString();
        }
        else
        {
          temp += a[i];
        }
      }
      return temp;
    }

    /// <summary>
    /// Reverse the direction of characters - Change text from logical to display order
    /// </summary>
    /// <remarks>
    /// Since doing it correct is very complex (for example numbers are written from left to right even in Hebrew). The
    /// UNICODE standard of handling bidirectional language is a very long document...
    /// http://unicode.org/reports/tr9/
    /// this code is a try to implement some of the standard.
    /// 
    /// Author: leo212 
    /// 
    /// </remarks>
    /// <param name="text">The text in logical (reading) order</param>
    /// <returns>The text in display order</returns>	
    public string HandleRTLText(string inLTRText)
    {
      try
      {
        bool rtl = isRTL(inLTRText);
        string directions = findDirections(inLTRText);
        string result = "";

        if (directions.Length > 0)
        {
          char lastDir = directions[0];
          int lastIndex = 0;

          string text;
          if (!rtl)
          {
            for (int i = 1; i <= directions.Length; i++)
            {
              if (i == directions.Length || directions[i] != lastDir)
              {
                text = inLTRText.Substring(lastIndex, i - lastIndex);
                if (lastDir == 'R')
                {
                  result += reverse(text);
                }
                else
                {
                  result += text;
                }

                if (i < directions.Length)
                {
                  lastDir = directions[i];
                }
                lastIndex = i;
              }
            }
          }
          else
          {
            lastIndex = directions.Length - 1;
            lastDir = directions[directions.Length - 1];
            for (int i = directions.Length - 2; i >= -1; i--)
            {
              if (i == -1 || directions[i] != lastDir)
              {
                text = inLTRText.Substring(i + 1, lastIndex - i);
                if (lastDir == 'R')
                {
                  result += reverse(text);
                }
                else
                {
                  result += text;
                }

                if (i >= 0)
                {
                  lastDir = directions[i];
                }
                lastIndex = i;
              }
            }
          }
        }
        return result;
      }
      catch (Exception exp)
      {
        Log.Error(exp);
        return inLTRText;
      }
    }

    private static bool isRTL(string inLTRText)
    {
      try
      {
        string strRTLChars = "";
        const int firstRTLCharacter = 0x05B0;
        const int lastRTLCharacter = 0x06F0;
        int i;
        for (i = firstRTLCharacter; i <= lastRTLCharacter; i++)
        {
          strRTLChars += Char.ConvertFromUtf32(i).ToString();
        }

        const string strNeutralChars = " ,.?:;\\|/`~!@#$%^&*-=_+*";
        const string strDelimiterChars = "[]{}()\"\"''";
        const string strNumbers = "0123456789";

        // find the first non-neutral character
        i = 0;
        bool found = false;
        bool rtl = false;

        while (i < inLTRText.Length && !found)
        {
          if (!strNumbers.Contains(inLTRText[i].ToString()) && !strNeutralChars.Contains(inLTRText[i].ToString()) &&
              !strDelimiterChars.Contains(inLTRText[i].ToString()))
          {
            found = true;
            if (strRTLChars.Contains(inLTRText[i].ToString()))
            {
              rtl = true;
            }
          }
          i++;
        }
        return rtl;
      }
      catch (Exception exp)
      {
        Log.Error(exp);
        return false;
      }
    }

    private static string findDirections(string inLTRText)
    {
      try
      {
        string strRTLChars = "";
        const int firstRTLCharacter = 0x05B0;
        const int lastRTLCharacter = 0x06F0;
        int i;
        for (i = firstRTLCharacter; i <= lastRTLCharacter; i++)
        {
          strRTLChars += Char.ConvertFromUtf32(i).ToString();
        }

        const string strNeutralChars = " ,.?:;\\|/`~!@#$%^&*-=_+*";
        const string strDelimiterChars = "[]{}()\"\"''";
        const string strStickyChars = ",.?:;\\|/`~!@#$%^&*-=_+*";
        const string strNumbers = "0123456789";

        // mark directions
        String directions = "";

        // find the first non-neutral character
        i = 0;
        bool found = false;
        bool rtl = false;

        while (i < inLTRText.Length && !found)
        {
          if (!strNumbers.Contains(inLTRText[i].ToString()) && !strNeutralChars.Contains(inLTRText[i].ToString()) &&
              !strDelimiterChars.Contains(inLTRText[i].ToString()))
          {
            found = true;
            if (strRTLChars.Contains(inLTRText[i].ToString()))
            {
              rtl = true;
            }
          }
          i++;
        }

        // mark directions of text
        for (i = 0; i < inLTRText.Length; i++)
        {
          if (strNeutralChars.Contains(inLTRText[i].ToString()))
          {
            if (strStickyChars.Contains(inLTRText[i].ToString()))
            {
              directions += "S";
            }
            else
            {
              if (rtl)
              {
                directions += "R";
              }
              else
              {
                directions += "L";
              }
            }
          }
          else if (strDelimiterChars.Contains(inLTRText[i].ToString()))
          {
            // mark its direction
            if (rtl)
            {
              directions += "R";
            }
            else
            {
              directions += "L";
            }

            // find the opposite delimiter
            int j = strDelimiterChars.IndexOf(inLTRText[i]);
            if (j % 2 == 0)
            {
              j++;
            }
            else
            {
              j--;
            }
            j = inLTRText.IndexOf(strDelimiterChars[j], i + 1);

            if (j < 0)
            {
              j = inLTRText.Length;
            }

            directions += findDirections(inLTRText.Substring(i + 1, j - i - 1));

            // mark the direction of the last delimiter
            if (j < inLTRText.Length)
            {
              if (rtl)
              {
                directions += "R";
              }
              else
              {
                directions += "L";
              }

              // jump to the next index
              i = j + 1;
            }
            else
            {
              i = j;
            }
          }
          else if (strRTLChars.Contains(inLTRText[i].ToString()))
          {
            rtl = true;
            directions += "R";
          }
          else
          {
            if (!strNumbers.Contains(inLTRText[i].ToString()))
            {
              rtl = false;
              directions += "L";
            }
            else
            {
              directions += "L";
            }
          }
        }

        // handle sticky chars
        char lastDir = '\0';
        i = 0;
        while (lastDir == '\0' && i < directions.Length)
        {
          char c = directions[i];
          if (c != 'S')
          {
            lastDir = c;
          }
          i++;
        }
        if (lastDir == '\0')
        {
          lastDir = directions[0];
        }
        char[] dircarr = directions.ToCharArray();
        for (int j = dircarr.Length - 1; j >= 0; j--)
        {
          if (dircarr[j] == 'S')
          {
            dircarr[j] = lastDir;
          }
          lastDir = dircarr[j];
        }

        for (int j = 0; j < dircarr.Length; j++)
        {
          if (dircarr[j] == 'S')
          {
            dircarr[j] = lastDir;
          }
          lastDir = dircarr[j];
        }

        return new String(dircarr);
      }
      catch (Exception exp)
      {
        Log.Error(exp);
        return "";
      }
    }

    #endregion RTL handling

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
      if (text == null)
      {
        return;
      }
      if (text.Length == 0)
      {
        return;
      }
      if (xpos <= 0)
      {
        return;
      }
      if (ypos <= 0)
      {
        return;
      }
      if (maxWidth < -1)
      {
        return;
      }

      if (GUIGraphicsContext.graphics != null)
      {
        GUIGraphicsContext.graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
        GUIGraphicsContext.graphics.SmoothingMode = SmoothingMode.HighQuality; //.AntiAlias;
        GUIGraphicsContext.graphics.DrawString(text, _systemFont, new SolidBrush(color), xpos, ypos);
        return;
      }
      if (_useRTLLang)
      {
        text = HandleRTLText(text);
      }
      if (ID >= 0)
      {
        for (int i = 0; i < text.Length; ++i)
        {
          char c = text[i];
          if (c < _StartCharacter || c >= _EndCharacter)
          {
            GUIFontManager.DrawText(_d3dxFont, xpos, ypos, color, text, maxWidth, _fontHeight);
            return;
          }
        }

        int intColor = color.ToArgb();
        unsafe
        {
          float[,] matrix = GUIGraphicsContext.GetFinalMatrix();

          IntPtr ptrStr = Marshal.StringToCoTaskMemUni(text); //SLOW
          FontEngineDrawText3D(ID, (void*)(ptrStr.ToPointer()), (int)xpos, (int)ypos, (uint)intColor, maxWidth,
                               matrix);
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
    public static int MeasureDisplayStringWidth(Graphics graphics, string text, Font font)
    {
      const int width = 32;

      Bitmap bitmap = new Bitmap(width, 1, graphics);
      SizeF size = graphics.MeasureString(text, font);
      Graphics anagra = Graphics.FromImage(bitmap);

      int measured_width = (int)size.Width;

      if (anagra != null)
      {
        anagra.Clear(Color.White);
        anagra.DrawString(text + "|", font, Brushes.Black,
                          width - measured_width, -font.Height / 2);

        for (int i = width - 1; i >= 0; i--)
        {
          measured_width--;
          if (bitmap.GetPixel(i, 0).R != 255) // found a non-white pixel ?
          {
            break;
          }
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

      if (null == text || text == string.Empty || _textureCoords == null)
      {
        return;
      }

      for (int i = 0; i < text.Length; ++i)
      {
        char c = text[i];
        if (c < _StartCharacter || c >= _EndCharacter)
        {
          GUIFontManager.MeasureText(_d3dxFont, text, ref textwidth, ref textheight, _fontHeight);
          return;
        }
      }

      float fRowWidth = 0.0f;
      float fRowHeight = (_textureCoords[0, 3] - _textureCoords[0, 1]) * _textureHeight;
      textheight = fRowHeight;

      for (int i = 0; i < text.Length; ++i)
      {
        char c = text[i];
        if (c == '\n')
        {
          if (fRowWidth > textwidth)
          {
            textwidth = fRowWidth;
          }
          fRowWidth = 0.0f;
          textheight += fRowHeight;
        }

        if (c < _StartCharacter || c >= _EndCharacter)
        {
          continue;
        }

        float tx1 = _textureCoords[c - _StartCharacter, 0];
        float tx2 = _textureCoords[c - _StartCharacter, 2];

        fRowWidth += (tx2 - tx1) * _textureWidth - 2 * _spacingPerChar;
      }

      if (fRowWidth > textwidth)
      {
        textwidth = fRowWidth;
      }
    }

    /// <summary>
    /// Cleanup any resources being used.
    /// </summary>
    public void Dispose(object sender, EventArgs e)
    {
      if (_systemFont != null)
      {
        _systemFont.Dispose();
      }

      if (_d3dxFont != null)
      {
        _d3dxFont.Dispose();
      }
      _d3dxFont = null;

      if (_textureFont != null)
      {
        _textureFont.Disposing -= new EventHandler(_textureFont_Disposing);
        _textureFont.Dispose();
      }
      _textureFont = null;
      _systemFont = null;
      _textureCoords = null;
      if (_fontAdded)
      {
        //Log.Debug("GUIFont:Dispose({0}) fontengine: Remove font:{1}", _fontName, ID.ToString());
        if (ID >= 0)
        {
          FontEngineRemoveFont(ID);
        }
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

    private Bitmap CreateFontBitmap()
    {
      // Create a bitmap on which to measure the alphabet
      Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
      Graphics g = Graphics.FromImage(bmp);
      bool width = true;

      g.SmoothingMode = SmoothingMode.AntiAlias;
      g.TextRenderingHint = TextRenderingHint.AntiAlias;
      g.TextContrast = 0;

      // Establish the font and texture size
      _textureScale = 1.0f; // Draw fonts into texture without scaling

      // Calculate the dimensions for the smallest power-of-two texture which
      // can hold all the printable characters
      _textureWidth = _textureHeight = 256;
      for (;;)
      {
        try
        {
          // Measure the alphabet
          PaintAlphabet(g, true);
        }
        catch (InvalidOperationException)
        {
          // Scale up the texture size and try again
          if (width)
          {
            _textureWidth *= 2;
          }
          else
          {
            _textureHeight *= 2;
          }
          width = !width;
          continue;
        }
        break;
      }

      // If requested texture is too big, use a smaller texture and smaller font,
      // and scale up when rendering.
      Caps d3dCaps = GUIGraphicsContext.DX9Device.DeviceCaps;

      // If the needed texture is too large for the video card...
      if (_textureWidth > d3dCaps.MaxTextureWidth)
      {
        // Scale the font size down to fit on the largest possible texture
        _textureScale = (float)d3dCaps.MaxTextureWidth / (float)_textureWidth;
        _textureWidth = _textureHeight = d3dCaps.MaxTextureWidth;

        for (;;)
        {
          // Create a new, smaller font
          _fontHeight = (int)Math.Floor(_fontHeight * _textureScale);
          _systemFont = new Font(_systemFont.Name, _fontHeight, _systemFont.Style);

          try
          {
            // Measure the alphabet
            PaintAlphabet(g, true);
          }
          catch (InvalidOperationException)
          {
            // If that still doesn't fit, scale down again and continue
            _textureScale *= 0.9F;
            continue;
          }

          break;
        }
      }
      Trace.WriteLine("font:" + _fontName + " " + _fileName + " height:" + _fontHeight.ToString() + " " +
                      _textureWidth.ToString() + "x" + _textureHeight.ToString());

      // Release the bitmap used for measuring and create one for drawing

      bmp = new Bitmap(_textureWidth, _textureHeight, PixelFormat.Format32bppArgb);
      g = Graphics.FromImage(bmp);

      g.SmoothingMode = SmoothingMode.AntiAlias;
      g.TextRenderingHint = TextRenderingHint.AntiAlias;
      g.TextContrast = 0;
      _textureCoords = new float[(10 + _EndCharacter - _StartCharacter),4];
      // Draw the alphabet
      PaintAlphabet(g, false);
      _textureCoords[_EndCharacter - _StartCharacter, 0] = _spacingPerChar;
      _textureCoords[_EndCharacter - _StartCharacter + 1, 0] = _textureScale;
      return bmp;
    }

    /// <summary>
    /// Initialize the device objects. Load the texture or if it does not exist, create it.
    /// </summary>
    public void InitializeDeviceObjects()
    {
      BinaryFormatter b = new BinaryFormatter();
      Stream s = null;
      bool needsCreation = false;

      string strCache = String.Format(@"{0}\fonts\{1}_{2}.dds", GUIGraphicsContext.SkinCacheFolder, _fontName,
                                      _fontHeight);
      try
      {
        // If file does not exist
        needsCreation = !File.Exists(strCache);
        if (!needsCreation)
        {
          try
          {
            ImageInformation info = new ImageInformation();
            _textureFont = TextureLoader.FromFile(GUIGraphicsContext.DX9Device,
                                                  strCache,
                                                  0, 0, //width/height
                                                  1, //miplevels
                                                  0,
                                                  Format.Unknown,
                                                  GUIGraphicsContext.GetTexturePoolType(),
                                                  Filter.None,
                                                  Filter.None,
                                                  0,
                                                  ref info);

            s = File.Open(strCache + ".bxml", FileMode.Open, FileAccess.Read);
            _textureCoords = (float[,])b.Deserialize(s);
            s.Close();
            _spacingPerChar = (int)_textureCoords[_EndCharacter - _StartCharacter, 0];
            _textureScale = _textureCoords[_EndCharacter - _StartCharacter + 1, 0];
            _textureHeight = info.Height;
            _textureWidth = info.Width;

            Log.Debug("  Loaded font:{0} height:{1} texture:{2}x{3} chars:[{4}-{5}] miplevels:{6}", _fontName,
                      _fontHeight,
                      _textureWidth, _textureHeight, _StartCharacter, _EndCharacter, _textureFont.LevelCount);
          }
          catch (Exception)
          {
            // Deserialisation failed. Maybe the language changed or the font cache got manipulated.
            Log.Error("GUIFont: Failed to load font {0} from cache. Trying to recreate it...", _fontName);
            MediaPortal.Util.Utils.FileDelete(strCache);
            MediaPortal.Util.Utils.FileDelete(strCache + ".bxml");
            needsCreation = true;
          }
        }

        if (needsCreation)
        {
          Log.Debug("TextureLoader.CreateFile {0}", strCache);
          // Make sure directory exists
          try
          {
            Directory.CreateDirectory(String.Format(@"{0}\fonts\", GUIGraphicsContext.SkinCacheFolder));

            // Create bitmap with the fonts
            using (Bitmap bmp = CreateFontBitmap())
            {
              // Save bitmap to stream
              using (MemoryStream imageStream = new MemoryStream())
              {
                bmp.Save(imageStream, ImageFormat.Bmp);

                // Reset and load from steam
                imageStream.Position = 0;
                Format format = Format.Dxt3;
                if (GUIGraphicsContext.GetTexturePoolType() == Pool.Default)
                {
                  format = Format.Unknown;
                }
                ImageInformation info = new ImageInformation();
                _textureFont = TextureLoader.FromStream(GUIGraphicsContext.DX9Device,
                                                        imageStream, (int)imageStream.Length,
                                                        0, 0, //width/height
                                                        1, //miplevels
                                                        0,
                                                        format,
                                                        GUIGraphicsContext.GetTexturePoolType(),
                                                        Filter.None,
                                                        Filter.None,
                                                        0,
                                                        ref info);

                // Finally save texture and texture coords to disk
                TextureLoader.Save(strCache, ImageFileFormat.Dds, _textureFont);
                s = File.Open(strCache + ".bxml", FileMode.CreateNew, FileAccess.ReadWrite);
                b.Serialize(s, (object)_textureCoords);
                s.Close();
                Log.Debug("Saving font:{0} height:{1} texture:{2}x{3} chars:[{4}-{5}] miplevels:{6}", _fontName,
                          _fontHeight,
                          _textureWidth, _textureHeight, _StartCharacter, _EndCharacter, _textureFont.LevelCount);
              }
            }
          }
          catch (Exception) {}
        }


        _textureFont.Disposing += new EventHandler(_textureFont_Disposing);
        SetFontEgine();
        _d3dxFont = new Microsoft.DirectX.Direct3D.Font(GUIGraphicsContext.DX9Device, _systemFont);
      }
      finally
      {
        if (s != null)
          s.Dispose();
      }
    }

    private void _textureFont_Disposing(object sender, EventArgs e)
    {
      Log.Debug("GUIFont:texture disposing:{0} {1}", ID, _fontName);
      _textureFont = null;
      if (_fontAdded && ID >= 0)
      {
        FontEngineRemoveFont(ID);
      }
      _fontAdded = false;
    }

    /// <summary>
    /// Load the font into the font engine
    /// </summary>
    public void SetFontEgine()
    {
      if (_fontAdded)
      {
        return;
      }
      if (ID < 0)
      {
        return;
      }

      //Log.Debug("GUIFont:RestoreDeviceObjects() fontengine: add font:" + ID.ToString());
      IntPtr upTexture = DirectShowUtil.GetUnmanagedTexture(_textureFont);
      unsafe
      {
        FontEngineAddFont(ID, upTexture.ToPointer(), _StartCharacter, _EndCharacter, _textureScale, _textureWidth,
                          _textureHeight, _spacingPerChar, MaxNumfontVertices);
      }

      int length = _textureCoords.GetLength(0);
      for (int i = 0; i < length; ++i)
      {
        FontEngineSetCoordinate(ID, i, 0, _textureCoords[i, 0], _textureCoords[i, 1], _textureCoords[i, 2],
                                _textureCoords[i, 3]);
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
        {
          size.Width = resSize.Width;
        }

        if ((x + size.Width + _spacingPerChar) > _textureWidth)
        {
          x = _spacingPerChar;
          y += size.Height;
        }

        // Make sure we have room for the current character
        if ((y + size.Height) > _textureHeight)
        {
          throw new InvalidOperationException("Texture too small for alphabet");
        }

        if (!measureOnly)
        {
          try
          {
            if (c != ' ')
              // We need the special case here because a space has a 0 width in GenericTypoGraphic stringformats
            {
              g.DrawString(str, _systemFont, Brushes.White, new Point((int)x, (int)y), StringFormat.GenericTypographic);
            }
            else
            {
              g.DrawString(str, _systemFont, Brushes.White, new Point((int)x, (int)y));
            }
          }
          catch (ExternalException)
          {
            // If GDI+ throws a generic exception (Interop ExternalException) because the requested character (str) isn't defined, ignore it and move on.
            continue;
          }
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
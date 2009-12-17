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
using System.Drawing;
using System.Globalization;
using System.Text;

namespace TvLibrary.Teletext
{
  public class TeletextPageRenderer
  {
    #region constructors

    public TeletextPageRenderer()
    {
      _isRegionalDKorNO = (RegionInfo.CurrentRegion.TwoLetterISORegionName.Equals("DK", StringComparison.InvariantCultureIgnoreCase))
                       || (RegionInfo.CurrentRegion.TwoLetterISORegionName.Equals("NO", StringComparison.InvariantCultureIgnoreCase));
      Transparent = Color.FromArgb(0, 0, 0, 0);
    }

    #endregion

    #region constants

    private const int MAX_ROWS = 50;

    #endregion

    #region variables

    //regional stuff
    private readonly bool _isRegionalDKorNO;

    private Font _fontTeletext;

    private bool _hiddenMode = true;
    private bool _transparentMode;
    private bool _fullscreenMode;

    private string _selectedPageText = "";

    private int _pageRenderWidth = 720;
    private int _pageRenderHeight = 540;
    private int _percentageOfMaximumHeight = 100;

    private Color Transparent = new Color();
    #endregion

    #region enums

    /// <summary>
    /// Enumeration of all availabel colors in teletext
    /// </summary>
    private enum TextColors
    {
      None,
      Black,
      Red,
      Green,
      Yellow,
      Blue,
      Magenta,
      Cyan,
      White,
      Trans1,
      Trans2
    }

    /// <summary>
    /// Enumeration of all possible attributes for a position in teletext
    /// </summary>
    public enum Attributes
    {
      AlphaBlack,
      AlphaRed,
      AlphaGreen,
      AlphaYellow,
      AlphaBlue,
      AlphaMagenta,
      AlphaCyan,
      AlphaWhite,
      Flash,
      Steady,
      EndBox,
      StartBox,
      NormalSize,
      DoubleHeight,
      DoubleWidth,
      DoubleSize,
      MosaicBlack,
      MosaicRed,
      MosaicGreen,
      MosaicYellow,
      MosaicBlue,
      MosaicMagenta,
      MosaicCyan,
      MosaicWhite,
      Conceal,
      ContiguousMosaic,
      SeparatedMosaic,
      Esc,
      BlackBackground,
      NewBackground,
      HoldMosaic,
      ReleaseMosaic
    }

    #endregion

    #region character and other tables for multi-language support. Referring the bits C12-C14 in the header

    private readonly char[,] m_charTableA = new char[,]
                                              {
                                                {'#', '\u016F'}, {'\u00A3', '$'},
                                                {'#', '\u00F5'}, {'\u00E9', '\u00EF'}, {'#', '$'}, {'\u00A3', '$'}, {'#', '$'},
                                                {'#', '\u0149'}, {'\u00E7', '$'}, {'#', '\u00A4'}, {'#', '\u00CB'}, {'#', '\u00A4'},
                                                {'\u00A3', '\u011F'}, {'#', '\u00A4'}
                                              };

    private readonly char[] m_charTableB = new char[]
                                             {
                                               '\u010D', '@', '\u0160', '\u00E0', '\u00A7', '\u00E9', '\u0160', '\u0105', '\u00A1', '\u0162',
                                               '\u010C', '\u00C9', '\u0130', '\u00C9'
                                             };

    private readonly char[,] m_charTableC = new char[,]
                                              {
                                                {'\u0165', '\u017E', '\u00FD', '\u00ED', '\u0159', '\u00E9'},
                                                {'\u2190', '\u00BD', '\u2192', '\u2191', '#', '\u0336'},
                                                {'\u00C4', '\u00D6', '\u017D', '\u00DC', '\u00D5', '\u0161'}, 
                                                {'\u00EB', '\u00EA', '\u00F9', '\u00EE', '#', '\u00E8'},
                                                {'\u00C4', '\u00D6', '\u00DC', '^', '_', '\u00B0'}, 
                                                {'\u00B0', '\u00E7', '\u2192', '\u2191', '#', '\u00F9'},
                                                {'\u00E9', '\u0229', '\u017D', '\u010D', '\u016B', '\u0161'},
                                                {'\u01B5', '\u015A', '\u0141', '\u0107', '\u00F3', '\u0119'},
                                                {'\u00E1', '\u00E9', '\u00ED', '\u00F3', '\u00FA', '\u00BF'},
                                                {'\u00C2', '\u015E', '\u01CD', '\u00CE', '\u0131', '\u0163'},
                                                {'\u0106', '\u017D', '\u0110', '\u0160', '\u00EB', '\u010D'},
                                                {'\u00C4', '\u00D6', '\u00C5', '\u00DC', '_', '\u00E9'},
                                                {'\u015E', '\u00D6', '\u00C7', '\u00DC', '\u01E6', '\u0131'},
                                                {'\u00C6', '\u00D8', '\u00C5', '\u00DC', '_', '\u00E9'}
                                              };

    private readonly char[,] m_charTableD = new char[,]
                                              {
                                                {'\u00E1', '\u011B', '\u00FA', '\u0161'}, {'\u00BC', '\u2016', '\u00BE', '\u00F7'},
                                                {'\u00E4', '\u00F6', '\u017E', '\u00FC'}, {'\u00E2', '\u00F4', '\u00FB', '\u00E7'},
                                                {'\u00E4', '\u00F6', '\u00FC', '\u00DF'}, {'\u00E0', '\u00F2', '\u00E8', '\u00EC'},
                                                {'\u0105', '\u0173', '\u017E', '\u012F'}, {'\u017C', '\u015B', '\u0142', '\u017A'},
                                                {'\u00FC', '\u00F1', '\u00E8', '\u00E0'}, {'\u00E2', '\u015F', '\u01CE', '\u00EE'},
                                                {'\u0107', '\u017E', '\u0111', '\u0161'}, {'\u00E4', '\u00F6', '\u00E5', '\u00FC'},
                                                {'\u015F', '\u00F6', '\u00E7', '\u00FC'}, {'\u00E6', '\u00F8', '\u00E5', '\u00FC'}
                                              };

    private readonly char[] m_charTableE = new char[]
                                             {
                                               '\u2190', '\u2192', '\u2191', '\u2193', 'O', 'K', '\u2190', '\u2190',
                                               '\u2190'
                                             };

    #endregion

    #region properties

    /// <summary>
    /// Width of the bitmap
    /// </summary>
    public int Width
    {
      get { return _pageRenderWidth; }
      set { _pageRenderWidth = value;}
    }

    /// <summary>
    /// Height of the bitmap
    /// </summary>
    public int Height
    {
      get { return _pageRenderHeight; }
      set {_pageRenderHeight = value; }
    }

    /// <summary>
    /// Draw also hidden information
    /// </summary>
    public bool HiddenMode
    {
      get { return _hiddenMode; }
      set { _hiddenMode = value; }
    }

    /// <summary>
    /// Draw the background transparent, only allowed in combination with fullscreen
    /// </summary>
    public bool TransparentMode
    {
      get { return _transparentMode; }
      set { _transparentMode = value; }
    }

    /// <summary>
    /// Draw for windowed mode or fullscreen mode
    /// </summary>
    public bool FullscreenMode
    {
      get { return _fullscreenMode; }
      set { _fullscreenMode = value; }
    }

    /// <summary>
    /// This text describe the selected page.
    /// </summary>
    public string PageSelectText
    {
      get { return _selectedPageText; }
      set
      {
        _selectedPageText = "";
        if (value.Length == 3)
        {
          _selectedPageText = value;
        }
        if (value.Length > 0 && value.Length < 3)
        {
          _selectedPageText = value + (new string('-', 3 - value.Length));
        }
      }
    }

    /// <summary>
    /// Gets/Sets  the percentage of the maximum height for the font size
    /// </summary>
    public int PercentageOfMaximumHeight
    {
      get { return _percentageOfMaximumHeight; }
      set { _percentageOfMaximumHeight = value; }
    }

    #endregion

    #region private methods

    /// <summary>
    /// Render a single position in the bitmap
    /// </summary>
    /// <param name="graph">Graphics</param>
    /// <param name="chr">Character</param>
    /// <param name="attrib">Attributes</param>
    /// <param name="x">x position</param>
    /// <param name="y">y position</param>
    /// <param name="w">width of the font</param>
    /// <param name="h">height of the font</param>
    /// <param name="txtLanguage">Teletext language</param>
    private void Render(ref Graphics graph, ref Bitmap pageBitmap, byte chr, int attrib, ref int x, ref int y, int w, int h, int txtLanguage)
    {
      bool charReady;
      char chr2 = '?';

      // Skip the character if 0xFF
      if (chr == 0xFF)
      {
        x += w;
        return;
      }
      // Generate mosaic
      int[] mosaicY = new int[4];
      mosaicY[0] = 0;
      mosaicY[1] = (h + 1)/3;
      mosaicY[2] = (h*2 + 1)/3;
      mosaicY[3] = h;

      /* get colors */
      int fColor = attrib & 0x0F;
      int bColor = (attrib >> 4) & 0x0F;
      Color bgColor = GetColor(bColor);
      // We are in transparent mode and fullscreen. Make beckground transparent
      if (_transparentMode && _fullscreenMode)
      {
        bgColor = Transparent;
      }
      Brush backBrush = new SolidBrush(bgColor);
      Brush foreBrush = new SolidBrush(GetColor(fColor));
      Pen backPen = new Pen(backBrush, 1);
      Pen forePen = new Pen(foreBrush, 1);
      // Draw the graphic
      try
      {
        if (((attrib & 0x300) > 0) && ((chr & 0xA0) == 0x20))
        {
          int w1 = w/2;
          int w2 = w - w1;
          int y1;

          chr = (byte) ((chr & 0x1f) | ((chr & 0x40) >> 1));
          if ((attrib & 0x200) > 0)
          {
            for (y1 = 0; y1 < 3; y1++)
            {
              graph.FillRectangle(backBrush, x, y + mosaicY[y1], w1, mosaicY[y1 + 1] - mosaicY[y1]);
              if ((chr & 1) > 0)
              {
                graph.FillRectangle(foreBrush, x + 1, y + mosaicY[y1] + 1, w1 - 2, mosaicY[y1 + 1] - mosaicY[y1] - 2);
              }
              graph.FillRectangle(backBrush, x + w1, y + mosaicY[y1], w2, mosaicY[y1 + 1] - mosaicY[y1]);
              if ((chr & 2) > 0)
              {
                graph.FillRectangle(foreBrush, x + w1 + 1, y + mosaicY[y1] + 1, w2 - 2,
                                    mosaicY[y1 + 1] - mosaicY[y1] - 2);
              }
              chr >>= 2;
            }
          }
          else
          {
            for (y1 = 0; y1 < 3; y1++)
            {
              if ((chr & 1) > 0)
              {
                graph.FillRectangle(foreBrush, x, y + mosaicY[y1], w1, mosaicY[y1 + 1] - mosaicY[y1]);
              }
              else
              {
                graph.FillRectangle(backBrush, x, y + mosaicY[y1], w1, mosaicY[y1 + 1] - mosaicY[y1]);
              }
              if ((chr & 2) > 0)
              {
                graph.FillRectangle(foreBrush, x + w1, y + mosaicY[y1], w2, mosaicY[y1 + 1] - mosaicY[y1]);
              }
              else
              {
                graph.FillRectangle(backBrush, x + w1, y + mosaicY[y1], w2, mosaicY[y1 + 1] - mosaicY[y1]);
              }

              chr >>= 2;
            }
          }

          x += w;
          return;
        }

        int factor = (attrib & 1 << 10) > 0 ? 2 : 1;

        charReady = false;
        // If character is still not drawn, then we analyse it again
        switch (chr)
        {
          case 0x00:
          case 0x20:
            graph.FillRectangle(backBrush, x, y, w, h);
            if (factor == 2)
            {
              graph.FillRectangle(backBrush, x, y + h, w, h);
            }
            x += w;
            charReady = true;
            break;
          case 0x23:
          case 0x24:
            chr2 = m_charTableA[txtLanguage, chr - 0x23];
            break;
          case 0x40:
            chr2 = m_charTableB[txtLanguage];
            break;
          case 0x5B:
          case 0x5C:
          case 0x5D:
          case 0x5E:
          case 0x5F:
          case 0x60:
            chr2 = m_charTableC[txtLanguage, chr - 0x5B];
            break;
          case 0x7B:
          case 0x7C:
          case 0x7D:
          case 0x7E:
            chr2 = m_charTableD[txtLanguage, chr - 0x7B];
            break;
          case 0x7F:
            graph.FillRectangle(backBrush, x, y, w, factor*h);
            graph.FillRectangle(foreBrush, x + (w/12), y + factor*(h*5/20), w*10/12, factor*(h*11/20));
            x += w;
            charReady = true;
            break;
          case 0xE0:
            graph.FillRectangle(backBrush, x + 1, y + 1, w - 1, h - 1);
            graph.DrawLine(forePen, x, y, x + w, y);
            graph.DrawLine(forePen, x, y, x, y + h);
            x += w;
            charReady = true;
            break;
          case 0xE1:
            graph.FillRectangle(backBrush, x, y + 1, w, h - 1);
            graph.DrawLine(forePen, x, y, x + w, y);
            x += w;
            charReady = true;
            break;
          case 0xE2:
            graph.FillRectangle(backBrush, x, y + 1, w - 1, h - 1);
            graph.DrawLine(forePen, x, y, x + w, y);
            graph.DrawLine(forePen, x + w - 1, y + 1, x + w - 1, y + h - 1);
            x += w;
            charReady = true;
            break;
          case 0xE3:
            graph.FillRectangle(backBrush, x + 1, y, w - 1, h);
            graph.DrawLine(forePen, x, y, x, y + h);
            x += w;
            charReady = true;
            break;
          case 0xE4:
            graph.FillRectangle(backBrush, x, y, w - 1, h);
            graph.DrawLine(forePen, x + w - 1, y, x + w - 1, y + h);
            x += w;
            charReady = true;
            break;
          case 0xE5:
            graph.FillRectangle(backBrush, x + 1, y, w - 1, h - 1);
            graph.DrawLine(forePen, x, y + h - 1, x + w, y + h - 1);
            graph.DrawLine(forePen, x, y, x, y + h - 1);
            x += w;
            charReady = true;
            break;
          case 0xE6:
            graph.FillRectangle(backBrush, x, y, w, h - 1);
            graph.DrawLine(forePen, x, y + h - 1, x + w, y + h - 1);
            x += w;
            charReady = true;
            break;
          case 0xE7:
            graph.FillRectangle(backBrush, x, y, w - 1, h - 1);
            graph.DrawLine(forePen, x, y + h - 1, x + w, y + h - 1);
            graph.DrawLine(forePen, x + w - 1, y, x + w - 1, y + h - 1);
            x += w;
            charReady = true;
            break;
          case 0xE8:
            graph.FillRectangle(backBrush, x + 1, y, w - 1, h);
            for (int r = 0; r < w/2; r++)
            {
              graph.DrawLine(forePen, x + r, y + r, x + r, y + h - r);
            }
            x += w;
            charReady = true;
            break;
          case 0xE9:
            graph.FillRectangle(backBrush, x + w/2, y, (w + 1)/2, h);
            graph.FillRectangle(foreBrush, x, y, w/2, h);
            x += w;
            charReady = true;
            break;
          case 0xEA:
            graph.FillRectangle(backBrush, x, y, w, h);
            graph.FillRectangle(foreBrush, x, y, w/2, h/2);
            x += w;
            charReady = true;
            break;
          case 0xEB:
            graph.FillRectangle(backBrush, x, y + 1, w, h - 1);
            for (int r = 0; r < w/2; r++)
            {
              graph.DrawLine(forePen, x + r, y + r, x + w - r, y + r);
            }
            x += w;
            charReady = true;
            break;
          case 0xEC:
            graph.FillRectangle(backBrush, x, y + (w/2), w, h - (w/2));
            graph.FillRectangle(foreBrush, x, y, w, h/2);
            x += w;
            charReady = true;
            break;
          case 0xED:
          case 0xEE:
          case 0xEF:
          case 0xF0:
          case 0xF1:
          case 0xF2:
          case 0xF3:
          case 0xF4:
          case 0xF5:
          case 0xF6:
            chr2 = m_charTableE[chr - 0xED];
            break;
          default:
            chr2 = (char) chr;
            break;
        }
        // If still not drawn than it's a text and we draw the string
        if (charReady == false)
        {
          string text = "" + chr2;
          graph.FillRectangle(backBrush, x, y, w, h);
          SizeF width = graph.MeasureString(text, _fontTeletext);
          PointF xyPos = new PointF((float) x + ((w - ((int) width.Width))/2), y);
          graph.DrawString(text, _fontTeletext, foreBrush, xyPos);
          if (factor == 2)
          {
            graph.FillRectangle(backBrush, x, y + h, w, h);
            Color[,] pixelColor = new Color[w + 1,h + 1];
            // save char
            for (int ypos = 0; ypos < h; ypos++)
            {
              for (int xpos = 0; xpos < w; xpos++)
              {
                pixelColor[xpos, ypos] = pageBitmap.GetPixel(xpos + x, ypos + y); // backup old line
              }
            }
            // draw doubleheight
            for (int ypos = 0; ypos < h; ypos++)
            {
              for (int xpos = 0; xpos < w; xpos++)
              {
                try
                {
                  if (y + (ypos*2) + 1 < pageBitmap.Height)
                  {
                    pageBitmap.SetPixel(x + xpos, y + (ypos*2), pixelColor[xpos, ypos]); // backup old line
                    pageBitmap.SetPixel(x + xpos, y + (ypos*2) + 1, pixelColor[xpos, ypos]);
                  }
                }
                catch
                {
                }
              }
            }
          }
          x += w;
        }
      }
      finally
      {
        foreBrush.Dispose();
        backBrush.Dispose();
        forePen.Dispose();
        backPen.Dispose();
      }
      return;
    }

    /// <summary>
    /// Converts the color of the teletext informations to a system color
    /// </summary>
    /// <param name="colorNumber">Number of the teletext color, referring to the enumeration TextColors </param>
    /// <returns>Corresponding System Color, or black if the value is not defined</returns>
    private Color GetColor(int colorNumber)
    {
      switch (colorNumber)
      {
        case (int) TextColors.Black:
          return Color.Black;
        case (int) TextColors.Red:
          return Color.Red;
        case (int) TextColors.Green:
          return Color.FromArgb(0, 255, 0);
        case (int) TextColors.Yellow:
          return Color.Yellow;
        case (int) TextColors.Blue:
          return Color.Blue;
        case (int) TextColors.Magenta:
          return Color.Magenta;
        case (int) TextColors.White:
          return Color.White;
        case (int) TextColors.Cyan:
          return Color.Cyan;
        case (int) TextColors.Trans1:
          return Transparent;
        case (int) TextColors.Trans2:
          return Transparent;
      }
      return Color.Black;
    }

    /// <summary>
    /// Checks if is a valid page to be displayed
    /// </summary>
    /// <param name="i">Pagenumber to check</param>
    /// <returns>True, if page should be displayed</returns>
    private static bool IsDecimalPage(int i)
    {
      return ((i & 0x00F) <= 9) && ((i & 0x0F0) <= 0x90);
    }

    #endregion

    #region public methods

    /// <summary>
    /// Renders a teletext page to a bitmap
    /// </summary>
    /// <param name="pageBitmap">The bitmap to render to</param>
    /// <param name="byPage">Teletext page data</param>
    /// <param name="mPage">Pagenumber</param>
    /// <param name="sPage">Subpagenumber</param>
    public void RenderPage(ref Bitmap pageBitmap, byte[] byPage, int mPage, int sPage)
    {
      int col;
      int hold;
      int foreground, background, doubleheight, charset, mosaictype;
      byte held_mosaic;
      bool flag = false;
      bool isBoxed = false;
      byte[] pageChars = new byte[31*40];
      int[] pageAttribs = new int[31*40];
      bool row24 = false;

      

      Graphics renderGraphics = Graphics.FromImage(pageBitmap);

      // Decode the page data (Hamming 8/4 or odd parity)
      for (int rowNr = 0; rowNr < MAX_ROWS; rowNr++)
      {
        if (rowNr*42 >= byPage.Length)
        {
          break;
        }
        int packetNumber = Hamming.GetPacketNumber(rowNr*42, ref byPage);
        // Only the packets 0-25 are accepted
        if (packetNumber < 0 || packetNumber > 25)
        {
          continue;
        }
        bool stripParity = true;
        // Packets 0 and 25 are hamming 8/4 encoded
        if (packetNumber == 25 || packetNumber == 0)
        {
          stripParity = false;
        }
        // Decode the whole row and remove the first two bytes
        for (col = 2; col < 42; col++)
        {
          // After pageheader in packet 0 (Bit 10) odd parity is used
          if (col >= 10 && packetNumber == 0)
          {
            stripParity = true;
          }
          byte kar = byPage[rowNr*42 + col];
          if (stripParity)
          {
            kar &= 0x7f;
          }
          pageChars[packetNumber*40 + col - 2] = kar;
        }
        // Exists a packet 24 (Toptext line)
        if (packetNumber == 24)
        {
          row24 = true;
        }
      }
      int row;
      int txtLanguage;
      // language detection. Extract the bit C12-C14 from the teletext header and set the language code
      int languageCode;
      byte byte1 = Hamming.Decode[byPage[9]];
      if (byte1 == 0xFF)
      {
        languageCode = 0;
      }
      else
      {
        languageCode = ((byte1 >> 3) & 0x01) | (((byte1 >> 2) & 0x01) << 1) | (((byte1 >> 1) & 0x01) << 2);
      }

      switch (languageCode)
      {
        case 0:
          txtLanguage = 1;
          break;
        case 1:
          txtLanguage = 4;
          break;
        case 2:
          txtLanguage = _isRegionalDKorNO ? 13 : 11;
          break;
        case 3:
          txtLanguage = 5;
          break;
        case 4:
          txtLanguage = 3;
          break;
        case 5:
          txtLanguage = 8;
          break;
        case 6:
          txtLanguage = 0;
          break;
        default:
          txtLanguage = 1;
          break;
      }
      // Detect if it's a boxed page. Boxed Page = subtitle and/or newsflash bit is set
      bool isSubtitlePage = Hamming.IsSubtitleBitSet(0, ref byPage);
      bool isNewsflash = Hamming.IsNewsflash(0, ref byPage);
      isBoxed = isNewsflash | isSubtitlePage;
      MediaPortal.GUI.Library.Log.Debug("Newsflash: " + isNewsflash);
      MediaPortal.GUI.Library.Log.Debug("Subtitle: " + isSubtitlePage);
      MediaPortal.GUI.Library.Log.Debug("Boxed: " + isBoxed);

      // Determine if the header or toptext line sould be displayed.
      bool displayHeaderAndTopText = !_fullscreenMode || !isBoxed || (isBoxed && _selectedPageText.IndexOf("-") != -1)
                                     ||
                                     (isBoxed && _selectedPageText.IndexOf("-") == -1 &&
                                      !_selectedPageText.Equals(Convert.ToString(mPage, 16)));

      // Iterate over all lines of the teletext page and prepare the rendering
      for (row = 0; row <= 24; row++)
      {
        // If row 24 and no toptext exists, then skip this row
        if (row == 24 && !row24)
        {
          continue;
        }
        // If not display the header and toptext line, then clear these two rows
        if ((row == 0 || row == 24) && !displayHeaderAndTopText)
        {
          for (int i = 0; i < 40; ++i)
          {
            pageChars[row*40 + i] = 32;
            pageAttribs[row*40 + i] = ((int) TextColors.Trans1 << 4) | ((int) TextColors.White);
          }
        }
        else
        {
          // Otherwise, analyse the information. First set the forground to white and the background to:
          // - Transparent, if transparent mode or boxed and fullscreen and not display the header and toptext line
          // - Black otherwise
          foreground = (int) TextColors.White;
          if ((isBoxed || _transparentMode) && _fullscreenMode && !displayHeaderAndTopText)
          {
            background = (int) TextColors.Trans1;
          }
          else
          {
            background = (int) TextColors.Black;
          }

          // Reset the attributes
          doubleheight = 0;
          charset = 0;
          mosaictype = 0;
          hold = 0;
          held_mosaic = 32;
          // Iterate over all columns in the row and check if a box starts
          for (int loop1 = 0; loop1 < 40; loop1++)
          {
            // Box starts in this row
            if (pageChars[(row*40) + loop1] == (int) Attributes.StartBox)
            {
              flag = true;
              break;
            }
          }

          // If boxed page and box doesn't start in this line, than set foreground and background to black or transparent
          // depending on the mode (fullscreen <-> windowed)
          if (isBoxed && flag == false)
          {
            if (_fullscreenMode)
            {
              foreground = (int) TextColors.Trans1;
              background = (int) TextColors.Trans1;
            }
            else
            {
              foreground = (int) TextColors.Black;
              background = (int) TextColors.Black;
            }
          }

          // Iterate over all columns in the row again and now analyse every byte
          for (col = 0; col < 40; col++)
          {
            int index = row*40 + col;

            // Set the attributes
            pageAttribs[index] = (doubleheight << 10 | charset << 8 | background << 4 | foreground);
            // Boxed and no flag and not row 24 than delete the characters
            if (isBoxed && !flag && row != 24)
            {
              pageChars[index] = 32;
            }
            // Analyse the attributes
            if (pageChars[index] < 32)
            {
              switch (pageChars[index])
              {
                case (int) Attributes.AlphaBlack:
                  foreground = (int) TextColors.Black;
                  charset = 0;
                  break;

                case (int) Attributes.AlphaRed:
                  foreground = (int) TextColors.Red;
                  charset = 0;
                  break;

                case (int) Attributes.AlphaGreen:
                  foreground = (int) TextColors.Green;
                  charset = 0;
                  break;

                case (int) Attributes.AlphaYellow:
                  foreground = (int) TextColors.Yellow;
                  charset = 0;
                  break;

                case (int) Attributes.AlphaBlue:
                  foreground = (int) TextColors.Blue;
                  charset = 0;
                  break;

                case (int) Attributes.AlphaMagenta:
                  foreground = (int) TextColors.Magenta;
                  charset = 0;
                  break;

                case (int) Attributes.AlphaCyan:
                  foreground = (int) TextColors.Cyan;
                  charset = 0;
                  break;

                case (int) Attributes.AlphaWhite:
                  foreground = (int) TextColors.White;
                  charset = 0;
                  break;

                case (int) Attributes.Flash:
                  break;

                case (int) Attributes.Steady:
                  break;

                case (int) Attributes.EndBox:
                  if (isBoxed)
                  {
                    if (_fullscreenMode)
                    {
                      foreground = (int) TextColors.Trans1;
                      background = (int) TextColors.Trans1;
                    }
                    else
                    {
                      foreground = (int) TextColors.Black;
                      background = (int) TextColors.Black;
                    }
                  }
                  break;

                case (int) Attributes.StartBox:
                  if (isBoxed)
                  {
                    // Clear everything until this position in the line
                    if (col > 0)
                    {
                      for (int loop1 = 0; loop1 < col; loop1++)
                      {
                        pageChars[(row*40) + loop1] = 32;
                      }
                    }
                    // Clear also the page attributes
                    for (int clear = 0; clear < col; clear++)
                    {
                      if (_fullscreenMode)
                      {
                        pageAttribs[row*40 + clear] = doubleheight << 10 | charset << 8 | (int) TextColors.Trans1 << 4 |
                                                      (int) TextColors.Trans1;
                      }
                      else
                      {
                        pageAttribs[row*40 + clear] = doubleheight << 10 | charset << 8 | (int) TextColors.Black << 4 |
                                                      (int) TextColors.Black;
                      }
                    }
                    // Set the standard background color
                    if (background == (int) TextColors.Trans1)
                    {
                      background = (int) TextColors.Black;
                    }
                  }
                  break;

                case (int) Attributes.NormalSize:
                  doubleheight = 0;
                  pageAttribs[index] = (doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  break;

                case (int) Attributes.DoubleHeight:
                  if (row < 23)
                  {
                    doubleheight = 1;
                  }
                  break;

                case (int) Attributes.MosaicBlack:
                  foreground = (int) TextColors.Black;
                  charset = 1 + mosaictype;
                  break;

                case (int) Attributes.MosaicRed:
                  foreground = (int) TextColors.Red;
                  charset = 1 + mosaictype;
                  break;

                case (int) Attributes.MosaicGreen:
                  foreground = (int) TextColors.Green;
                  charset = 1 + mosaictype;
                  break;

                case (int) Attributes.MosaicYellow:
                  foreground = (int) TextColors.Yellow;
                  charset = 1 + mosaictype;
                  break;

                case (int) Attributes.MosaicBlue:
                  foreground = (int) TextColors.Blue;
                  charset = 1 + mosaictype;
                  break;

                case (int) Attributes.MosaicMagenta:
                  foreground = (int) TextColors.Magenta;
                  charset = 1 + mosaictype;
                  break;

                case (int) Attributes.MosaicCyan:
                  foreground = (int) TextColors.Cyan;
                  charset = 1 + mosaictype;
                  break;

                case (int) Attributes.MosaicWhite:
                  foreground = (int) TextColors.White;
                  charset = 1 + mosaictype;
                  break;

                case (int) Attributes.Conceal:
                  if (_hiddenMode == false)
                  {
                    foreground = background;
                    pageAttribs[index] = (doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  }
                  break;

                case (int) Attributes.ContiguousMosaic:
                  mosaictype = 0;
                  if (charset > 0)
                  {
                    charset = 1;
                    pageAttribs[index] = (doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  }
                  break;

                case (int) Attributes.SeparatedMosaic:
                  mosaictype = 1;
                  if (charset > 0)
                  {
                    charset = 2;
                    pageAttribs[index] = (doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  }
                  break;

                case (int) Attributes.Esc:
                  break;

                case (int) Attributes.BlackBackground:
                  background = (int) TextColors.Black;
                  pageAttribs[index] = (doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  break;

                case (int) Attributes.NewBackground:
                  background = foreground;
                  pageAttribs[index] = (doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  break;

                case (int) Attributes.HoldMosaic:
                  hold = 1;
                  break;

                case (int) Attributes.ReleaseMosaic:
                  hold = 2;
                  break;
              }

              if (hold > 0 && charset > 0)
              {
                pageChars[index] = held_mosaic;
              }
              else
              {
                pageChars[index] = 32;
              }

              if (hold == 2)
              {
                hold = 0;
              }
            }
            else
            {
              if (charset > 0)
              {
                held_mosaic = pageChars[index];
              }
              // If doubleheight is selected than delete the following line
              if (doubleheight > 0)
              {
                pageChars[index + 40] = 0xFF;
              }
            }
          }
          // Check, if there is double height selected in than set the attributes for the next row and skip it
          for (int count = (row + 1)*40; count < ((row + 1)*40) + 40; count++)
          {
            if (pageChars[count] == 255)
            {
              for (int loop1 = 0; loop1 < 40; loop1++)
              {
                pageAttribs[(row + 1)*40 + loop1] = ((pageAttribs[(row*40) + loop1] & 0xF0) |
                                                     ((pageAttribs[(row*40) + loop1] & 0xF0) >> 4));
              }

              row++;
              break;
            }
          }
        }
      } //for (int rowNr = 0; rowNr < 24; rowNr++)

      // Generate header line, if it should be displayed
      if (IsDecimalPage(mPage) && displayHeaderAndTopText)
      {
        int i;
        string pageNumber;
        int lineColor;
        // Determine the state, of the header line.
        // Red=Incomplete page number
        // Yellow=Waiting for page
        // Green=Page is displayed
        if (_selectedPageText.IndexOf("-") == -1)
        {
          if (_selectedPageText.Equals(Convert.ToString(mPage, 16)))
          {
            lineColor = (int) TextColors.Green;
            pageNumber = Convert.ToString(mPage, 16) + "/" + Convert.ToString(sPage, 16);
          }
          else
          {
            lineColor = (int) TextColors.Yellow;
            pageNumber = _selectedPageText;
          }
        }
        else
        {
          lineColor = (int) TextColors.Red;
          pageNumber = _selectedPageText;
        }
        string headline = "MediaPortal P." + pageNumber;
        headline += new string((char) 32, 32 - headline.Length);
        byte[] mpText = Encoding.ASCII.GetBytes(headline);
        Array.Copy(mpText, 0, pageChars, 0, mpText.Length);
        for (i = 0; i < 11; i++)
        {
          pageAttribs[i] = ((int) TextColors.Black << 4) | lineColor;
        }
        for (i = 12; i < 40; i++)
        {
          pageAttribs[i] = ((int) TextColors.Black << 4) | ((int) TextColors.White);
        }
      }

      // Now we generate the bitmap
      int y = 0;
      int x;
      int width = _pageRenderWidth/40;
      int height = _pageRenderHeight/25;
      float fntSize = height; //Math.Min(width, height);
      float nPercentage = ((float) _percentageOfMaximumHeight/100);
      _fontTeletext = new Font("Lucida Console", fntSize, FontStyle.Regular, GraphicsUnit.Pixel);
      float fntHeight = _fontTeletext.GetHeight(renderGraphics);
      while (fntHeight > nPercentage*height) // || fntHeight > nPercentage * width)
      {
        fntSize -= 0.1f;
        _fontTeletext = new Font("Lucida Console", fntSize, FontStyle.Bold, GraphicsUnit.Pixel);
        fntHeight = _fontTeletext.GetHeight(renderGraphics);
      }
      MediaPortal.GUI.Library.Log.Debug("FONT SIZE OF TELETEXT: " + fntSize);

      try
      {
        // Select the brush, depending on the page and mode
        // Draw the base rectangle
        if ((isBoxed || _transparentMode) && _fullscreenMode)
        {
          renderGraphics.Clear(Transparent);
        }
        else
        {
          renderGraphics.FillRectangle(new SolidBrush(Color.Black), 0, 0, _pageRenderWidth, _pageRenderHeight);
        }

        // Fill the rectangle with the teletext page informations
        for (row = 0; row < 25; row++)
        {
          // If not display a toptext line than abort
          if (!displayHeaderAndTopText && row == 24)
          {
            break;
          }
          x = 0;
          // Draw a single point
          for (col = 0; col < 40; col++)
          {
            Render(ref renderGraphics, ref pageBitmap, pageChars[row * 40 + col], pageAttribs[row * 40 + col], ref x, ref y, width, height,
                   txtLanguage);
          }

          y += height + (row == 23 ? 2 : 0);
        }
      }
      finally
      {
        _fontTeletext.Dispose();
        _fontTeletext = null;
      }
    }

    #endregion
  }
}
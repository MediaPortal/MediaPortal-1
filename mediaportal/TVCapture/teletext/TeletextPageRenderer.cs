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
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.TV.Teletext
{
  public class TeletextPageRenderer
  {
    #region constants
    const int MAX_ROWS = 50;
    #endregion

    #region variables
    System.Drawing.Bitmap _pageBitmap = null;
    System.Drawing.Graphics _renderGraphics = null;
    System.Drawing.Font _fontTeletext = null;

    bool _hiddenMode = true;
    bool _transparentMode = false;
    string _selectedPageText = "";

    int _pageRenderWidth = 1920;
    int _pageRenderHeight = 1080;
    #endregion

    #region enums
    enum TextColors
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

    #region character and other tables
    //
    char[,] m_charTableA = new char[,]{{ '#', '\u016F' },{ '£', '$' }, 
	{ '#', 'õ' },{ 'é', 'ï' }, { '#', '$' }, { '£', '$' },{ '#', '$' },
	{ '#', '\u0149' },{ 'ç', '$' }, { '#', '¤' },{ '#', 'Ë' }, { '#', '¤' },{ '£', '\u011F' }
	};
    char[] m_charTableB = new char[] { '\u010D', '@', '\u0160', 'à', '§', 'é', '\u0160', '\u0105', '¡', '\u0162', '\u010C', 'É', '\u0130' };
    char[,] m_charTableC = new char[,]{{ '\u0165', '\u017E', 'ý', 'í', '\u0159', 'é' },{'\u2190', '½','\u2192','\u2191', '#', '\u0336' },
	{ 'Ä', 'Ö', '\u017D', 'Ü', 'Õ', '\u0161' },{ 'ë', 'ê', 'ù', 'î', '#', 'è' },
	{ 'Ä', 'Ö', 'Ü', '^', '_', '°' },{ '°', 'ç','\u2192','\u2191', '#', 'ù' },
	{ 'é', '\u0229', '\u017D', '\u010D', '\u016B', '\u0161' },{ '\u01B5', '\u015A', '\u0141', '\u0107', 'ó', '\u0119' },
	{ 'á', 'é', 'í', 'ó', 'ú', '¿' },{ 'Â', '\u015E', '\u01CD', 'Î', '\u0131', '\u0163' },
	{ '\u0106', '\u017D', '\u0110', '\u0160', 'ë', '\u010D' },{ 'Ä', 'Ö', 'Å', 'Ü', '_', 'é' },
	{ '\u015E', 'Ö', 'Ç', 'Ü', '\u01E6', '\u0131' }};

    char[,] m_charTableD = new char[,]{{ 'á', '\u011B', 'ú', '\u0161' },{ '¼','\u2016', '¾', '÷' },
	{ 'ä', 'ö', '\u017E', 'ü' },{ 'â', 'ô', 'û', 'ç' },{ 'ä', 'ö', 'ü', 'ß' },
	{ 'à', 'ò', 'è', 'ì' },{ '\u0105', '\u0173', '\u017E', '\u012F' },{ '\u017C', '\u015B', '\u0142', '\u017A' },
	{ 'ü', 'ñ', 'è', 'à' },{ 'â', '\u015F', '\u01CE', 'î' },{ '\u0107', '\u017E', '\u0111', '\u0161' },
	{ 'ä', 'ö', 'å', 'ü' },{ '\u015F', 'ö', 'ç', 'ü' }};
    char[] m_charTableE = new char[] { '\u2190', '\u2192', '\u2191', '\u2193', 'O', 'K', '\u2190', '\u2190', '\u2190' };

    #endregion

    public int Width
    {
      get { return _pageRenderWidth; }
      set { 
        _pageRenderWidth = value;
        Clear();
      }
    }
    public int Height
    {
      get { return _pageRenderHeight; }
      set
      {
        _pageRenderHeight = value;
        Clear();
      }
    }

    public bool HiddenMode
    {
      get
      {
        return _hiddenMode;
      }
      set
      {
        _hiddenMode = value;
      }
    }
    public bool TransparentMode
    {
      get { return _transparentMode; }
      set { _transparentMode = value; }
    }


    public string PageSelectText
    {
      get
      {
        return _selectedPageText;
      }
      set
      {
        _selectedPageText = "";
        if (value.Length > 0 && value.Length < 3)
          _selectedPageText = value + (new string('-', 3 - value.Length));
      }
    }

    public void Clear()
    {
      if (_pageBitmap != null)
        _pageBitmap.Dispose();
      _pageBitmap = null;

      if (_renderGraphics != null)
        _renderGraphics.Dispose();
      _renderGraphics = null;
    }


    void Render(System.Drawing.Graphics graph, byte chr, int attrib, ref int x, ref int y, int w, int h, bool isSubtitlePage, int txtLanguage)
    {

      bool charReady = false;
      char chr2 = '?';

      if (chr == 0xFF)
      {
        x += w;
        return;
      }
      int[] mosaicY = new int[4];
      mosaicY[0] = 0;
      mosaicY[1] = (h + 1) / 3;
      mosaicY[2] = (h * 2 + 1) / 3;
      mosaicY[3] = h;

      /* get colors */
      int fColor = attrib & 0x0F;
      int bColor = (attrib >> 4) & 0x0F;
      System.Drawing.Color bgColor = GetColor(bColor);
      //if (bgColor==System.Drawing.Color.Black && isSubtitlePage)
      //	bgColor=System.Drawing.Color.HotPink;
      if (bgColor == System.Drawing.Color.HotPink && !isSubtitlePage)
        bgColor = System.Drawing.Color.Black;
      System.Drawing.Brush backBrush = new System.Drawing.SolidBrush(bgColor);
      System.Drawing.Brush foreBrush = new System.Drawing.SolidBrush(GetColor(fColor));
      System.Drawing.Pen backPen = new System.Drawing.Pen(backBrush, 1);
      System.Drawing.Pen forePen = new System.Drawing.Pen(foreBrush, 1);
      try
      {
        if (((attrib & 0x300) > 0) && ((chr & 0xA0) == 0x20))
        {
          int w1 = w / 2;
          int w2 = w - w1;
          int y1;

          chr = (byte)((chr & 0x1f) | ((chr & 0x40) >> 1));
          if ((attrib & 0x200) > 0)
            for (y1 = 0; y1 < 3; y1++)
            {
              graph.FillRectangle(backBrush, x, y + mosaicY[y1], w1, mosaicY[y1 + 1] - mosaicY[y1]);
              if ((chr & 1) > 0)
                graph.FillRectangle(foreBrush, x + 1, y + mosaicY[y1] + 1, w1 - 2, mosaicY[y1 + 1] - mosaicY[y1] - 2);
              graph.FillRectangle(backBrush, x + w1, y + mosaicY[y1], w2, mosaicY[y1 + 1] - mosaicY[y1]);
              if ((chr & 2) > 0)
                graph.FillRectangle(foreBrush, x + w1 + 1, y + mosaicY[y1] + 1, w2 - 2, mosaicY[y1 + 1] - mosaicY[y1] - 2);
              chr >>= 2;
            }
          else
            for (y1 = 0; y1 < 3; y1++)
            {
              if ((chr & 1) > 0)
                graph.FillRectangle(foreBrush, x, y + mosaicY[y1], w1, mosaicY[y1 + 1] - mosaicY[y1]);
              else
                graph.FillRectangle(backBrush, x, y + mosaicY[y1], w1, mosaicY[y1 + 1] - mosaicY[y1]);
              if ((chr & 2) > 0)
                graph.FillRectangle(foreBrush, x + w1, y + mosaicY[y1], w2, mosaicY[y1 + 1] - mosaicY[y1]);
              else
                graph.FillRectangle(backBrush, x + w1, y + mosaicY[y1], w2, mosaicY[y1 + 1] - mosaicY[y1]);

              chr >>= 2;
            }

          x += w;
          return;
        }
        int factor = 0;

        if ((attrib & 1 << 10) > 0)
          factor = 2;
        else
          factor = 1;

        charReady = false;

        switch (chr)
        {
          case 0x00:
          case 0x20:
            graph.FillRectangle(backBrush, x, y, w, h);
            if (factor == 2)
              graph.FillRectangle(backBrush, x, y + h, w, h);
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
            graph.FillRectangle(backBrush, x, y, w, factor * h);
            graph.FillRectangle(foreBrush, x + (w / 12), y + factor * (h * 5 / 20), w * 10 / 12, factor * (h * 11 / 20));
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
            for (int r = 0; r < w / 2; r++)
              graph.DrawLine(forePen, x + r, y + r, x + r, y + h - r);
            x += w;
            charReady = true;
            break;
          case 0xE9:
            graph.FillRectangle(backBrush, x + w / 2, y, (w + 1) / 2, h);
            graph.FillRectangle(foreBrush, x, y, w / 2, h);
            x += w;
            charReady = true;
            break;
          case 0xEA:
            graph.FillRectangle(backBrush, x, y, w, h);
            graph.FillRectangle(foreBrush, x, y, w / 2, h / 2);
            x += w;
            charReady = true;
            break;
          case 0xEB:
            graph.FillRectangle(backBrush, x, y + 1, w, h - 1);
            for (int r = 0; r < w / 2; r++)
              graph.DrawLine(forePen, x + r, y + r, x + w - r, y + r);
            x += w;
            charReady = true;
            break;
          case 0xEC:
            graph.FillRectangle(backBrush, x, y + (w / 2), w, h - (w / 2));
            graph.FillRectangle(foreBrush, x, y, w, h / 2);
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
            chr2 = (char)chr;
            break;
        }
        if (charReady == false)
        {
          string text = "" + chr2;
          graph.FillRectangle(backBrush, x, y, w, h);
          System.Drawing.SizeF width = graph.MeasureString(text, _fontTeletext);
          System.Drawing.PointF xyPos = new System.Drawing.PointF((float)x + ((w - ((int)width.Width)) / 2), (float)y);
          graph.DrawString(text, _fontTeletext, foreBrush, xyPos);
          if (factor == 2)
          {
            graph.FillRectangle(backBrush, x, y + h, w, h);
            System.Drawing.Color[,] pixelColor = new System.Drawing.Color[w + 1, h + 1];
            // save char
            for (int ypos = 0; ypos < h; ypos++)
            {
              for (int xpos = 0; xpos < w; xpos++)
              {
                pixelColor[xpos, ypos] = _pageBitmap.GetPixel(xpos + x, ypos + y); // backup old line
              }
            }
            // draw doubleheight
            for (int ypos = 0; ypos < h; ypos++)
            {

              for (int xpos = 0; xpos < w; xpos++)
              {

                try
                {
                  if (y + (ypos * 2) + 1 < _pageBitmap.Height)
                  {
                    _pageBitmap.SetPixel(x + xpos, y + (ypos * 2), pixelColor[xpos, ypos]); // backup old line
                    _pageBitmap.SetPixel(x + xpos, y + (ypos * 2) + 1, pixelColor[xpos, ypos]);
                  }
                }
                catch { }
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

    System.Drawing.Color GetColor(int colorNumber)
    {

      switch (colorNumber)
      {
        case (int)TextColors.Black:
          return System.Drawing.Color.Black;
        case (int)TextColors.Red:
          return System.Drawing.Color.Red;
        case (int)TextColors.Green:
          return System.Drawing.Color.FromArgb(0, 255, 0);
        case (int)TextColors.Yellow:
          return System.Drawing.Color.Yellow;
        case (int)TextColors.Blue:
          return System.Drawing.Color.Blue;
        case (int)TextColors.Magenta:
          return System.Drawing.Color.Magenta;
        case (int)TextColors.White:
          return System.Drawing.Color.White;
        case (int)TextColors.Cyan:
          return System.Drawing.Color.Cyan;
        case (int)TextColors.Trans1:
          return System.Drawing.Color.HotPink;
        case (int)TextColors.Trans2:
          return System.Drawing.Color.HotPink;
      }
      return System.Drawing.Color.Black;
    }


    public System.Drawing.Bitmap RenderPage(byte[] byPage, int mPage, int sPage)
    {
      if (_pageBitmap == null)
      {
        _pageBitmap = new System.Drawing.Bitmap(_pageRenderWidth, _pageRenderHeight);
        _pageBitmap.MakeTransparent(System.Drawing.Color.HotPink);
      }
      if (_renderGraphics == null)
        _renderGraphics = System.Drawing.Graphics.FromImage(_pageBitmap);

      int row, col;
      int hold;
      int foreground, background, doubleheight, charset, mosaictype;
      byte held_mosaic;
      bool flag = false;
      int boxed = 0;
      byte[] pageChars = new byte[31*40];
      int[] pageAttribs = new int[31 * 40];
      bool row24 = false;
      for (int rowNr = 0; rowNr < MAX_ROWS; rowNr++)
      {
        if (rowNr * 42 >= byPage.Length) break;
        int packetNumber = Hamming.GetPacketNumber(rowNr * 42, ref byPage);
        if (packetNumber < 0 || packetNumber > 25) continue;
        bool stripParity=true;
        if (packetNumber == 25 || packetNumber == 0) 
          stripParity = false;
        for (col = 2; col < 42; col++)
        {
          byte kar= byPage[rowNr * 42 + col];
          if (stripParity) 
            kar &= 0x7f;
          pageChars[packetNumber * 40 + col-2] = kar;
        }
        if (packetNumber == 24)
          row24 = true;
      }
      row = col = 0;
      int txtLanguage=0;
      //language
      int languageCode = 0;
      byte byte1 = Hamming.Decode[byPage[9]];
      if (byte1 == 0xFF)
        languageCode = 0;
      else
        languageCode = ((byte1 >> 3) & 0x01) | (((byte1 >> 2) & 0x01) << 1) | (((byte1 >> 1) & 0x01) << 2);

      switch (languageCode)
      {
        case 0:
          txtLanguage = 1;
          break;
        case 1:
          txtLanguage = 4;
          break;
        case 2:
          txtLanguage = 11;
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

      bool isSubtitlePage = false;
      if (Hamming.IsBoxed(0,ref byPage) )
        boxed = 1;
      else
        boxed = 0;

      if (Hamming.IsSubtitleBitSet(0, ref byPage) && _transparentMode)
        isSubtitlePage = true;

      for (row = 0; row <= 24; row++)
      {
        if (row == 24 && !row24) continue;
        if ((row == 0 || row == 24) && isSubtitlePage)
        {
          for (int i = 0; i < 40; ++i)
          {
            pageChars[row * 40 + i] = 32;
            pageAttribs[row * 40 + i] = ((int)TextColors.Trans1 << 4) | ((int)TextColors.White);
          }
        }
        else
        {
          foreground = (int)TextColors.White;
          if (isSubtitlePage == false && _transparentMode == false)
            background = (int)TextColors.Black;
          else
            background = (int)TextColors.Trans1;

          doubleheight = 0;
          charset = 0;
          mosaictype = 0;
          hold = 0;
          held_mosaic = 32;

          for (int loop1 = 0; loop1 < 40; loop1++)
          {
            if (pageChars[(row * 40) + loop1] == (int)Attributes.StartBox)
            {
              flag = true;
              break;
            }
          }

          if (boxed != 0 && flag == false)
          {
            if (_transparentMode)
            {
              foreground = (int)TextColors.Trans1;
              background = (int)TextColors.Trans1;
            }
          }

          for (col = 0; col < 40; col++)
          {
            int index = row * 40 + col;

            pageAttribs[index] = (doubleheight << 10 | charset << 8 | background << 4 | foreground);

            if (pageChars[index] < 32)
            {
              switch (pageChars[index])
              {
                case (int)Attributes.AlphaBlack:
                  foreground = (int)TextColors.Black;
                  charset = 0;
                  break;

                case (int)Attributes.AlphaRed:
                  foreground = (int)TextColors.Red;
                  charset = 0;
                  break;

                case (int)Attributes.AlphaGreen:
                  foreground = (int)TextColors.Green;
                  charset = 0;
                  break;

                case (int)Attributes.AlphaYellow:
                  foreground = (int)TextColors.Yellow;
                  charset = 0;
                  break;

                case (int)Attributes.AlphaBlue:
                  foreground = (int)TextColors.Blue;
                  charset = 0;
                  break;

                case (int)Attributes.AlphaMagenta:
                  foreground = (int)TextColors.Magenta;
                  charset = 0;
                  break;

                case (int)Attributes.AlphaCyan:
                  foreground = (int)TextColors.Cyan;
                  charset = 0;
                  break;

                case (int)Attributes.AlphaWhite:
                  foreground = (int)TextColors.White;
                  charset = 0;
                  break;

                case (int)Attributes.Flash:
                  break;

                case (int)Attributes.Steady:
                  break;

                case (int)Attributes.EndBox:
                  if (boxed > 0)
                  {
                    if (_transparentMode)
                    {
                      foreground = (int)TextColors.Trans1;
                      background = (int)TextColors.Trans1;
                    }
                  }
                  break;

                case (int)Attributes.StartBox:
                  if (boxed > 0 && _transparentMode)
                  {
                    background = (int)TextColors.Black;
                    if (col > 0)
                      for (int loop1 = 0; loop1 < col; loop1++)
                        pageChars[(row * 40) + loop1] = 32;
                    for (int clear = 0; clear < col; clear++)
                      pageAttribs[row * 40 + clear] = doubleheight << 10 | charset << 8 | (int)TextColors.Black << 4 | (int)TextColors.Black;
                  }
                  break;

                case (int)Attributes.NormalSize:
                  doubleheight = 0;
                  pageAttribs[index] = (doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  break;

                case (int)Attributes.DoubleHeight:
                  if (row < 23)
                    doubleheight = 1;
                  break;

                case (int)Attributes.MosaicBlack:
                  foreground = (int)TextColors.Black;
                  charset = 1 + mosaictype;
                  break;

                case (int)Attributes.MosaicRed:
                  foreground = (int)TextColors.Red;
                  charset = 1 + mosaictype;
                  break;

                case (int)Attributes.MosaicGreen:
                  foreground = (int)TextColors.Green;
                  charset = 1 + mosaictype;
                  break;

                case (int)Attributes.MosaicYellow:
                  foreground = (int)TextColors.Yellow;
                  charset = 1 + mosaictype;
                  break;

                case (int)Attributes.MosaicBlue:
                  foreground = (int)TextColors.Blue;
                  charset = 1 + mosaictype;
                  break;

                case (int)Attributes.MosaicMagenta:
                  foreground = (int)TextColors.Magenta;
                  charset = 1 + mosaictype;
                  break;

                case (int)Attributes.MosaicCyan:
                  foreground = (int)TextColors.Cyan;
                  charset = 1 + mosaictype;
                  break;

                case (int)Attributes.MosaicWhite:
                  foreground = (int)TextColors.White;
                  charset = 1 + mosaictype;
                  break;

                case (int)Attributes.Conceal:
                  if (_hiddenMode == true)
                  {
                    foreground = background;
                    pageAttribs[index] = (doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  }
                  break;

                case (int)Attributes.ContiguousMosaic:
                  mosaictype = 0;
                  if (charset > 0)
                  {
                    charset = 1;
                    pageAttribs[index] = (doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  }
                  break;

                case (int)Attributes.SeparatedMosaic:
                  mosaictype = 1;
                  if (charset > 0)
                  {
                    charset = 2;
                    pageAttribs[index] = (doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  }
                  break;

                case (int)Attributes.Esc:
                  break;

                case (int)Attributes.BlackBackground:
                  background = (int)TextColors.Black;
                  pageAttribs[index] = (doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  break;

                case (int)Attributes.NewBackground:
                  background = foreground;
                  pageAttribs[index] = (doubleheight << 10 | charset << 8 | background << 4 | foreground);
                  break;

                case (int)Attributes.HoldMosaic:
                  hold = 1;
                  break;

                case (int)Attributes.ReleaseMosaic:
                  hold = 2;
                  break;
              }

              if (hold > 0 && charset > 0)
                pageChars[index] = held_mosaic;
              else
                pageChars[index] = 32;

              if (hold == 2)
                hold = 0;
            }
            else
            {
              if (charset > 0)
                held_mosaic = pageChars[index];

              if (doubleheight > 0)
                pageChars[index + 40] = 0xFF;
            }
          }

          for (int count = (row + 1) * 40; count < ((row + 1) * 40) + 40; count++)
          {
            if (pageChars[count] == 255)
            {
              for (int loop1 = 0; loop1 < 40; loop1++)
                pageAttribs[(row + 1) * 40 + loop1] = ((pageAttribs[(row * 40) + loop1] & 0xF0) | ((pageAttribs[(row * 40) + loop1] & 0xF0) >> 4));

              row++;
              break;
            }
          }
        }
      }//for (int rowNr = 0; rowNr < 24; rowNr++)

      if (IsDecimalPage(mPage))
      {
        if (!isSubtitlePage)
        {
          int i;
          string pageNumber = "";
          int lineColor = 0;
          if (_selectedPageText.IndexOf("-") == -1)
          {
            lineColor = (int)TextColors.Green;
            pageNumber = Convert.ToString(mPage, 16) + "/" + Convert.ToString(sPage, 16);
          }
          else
          {
            lineColor = (int)TextColors.Red;
            pageNumber = _selectedPageText;
          }
          string headline = "MediaPortal P." + pageNumber;
          headline += new string((char)32, 32 - headline.Length);
          byte[] mpText = System.Text.Encoding.ASCII.GetBytes(headline);
          System.Array.Copy(mpText, 0, pageChars, 0, mpText.Length);
          for (i = 0; i < 11; i++)
            pageAttribs[i] = ((int)TextColors.Black << 4) | lineColor;
          for (i = 12; i < 40; i++)
            pageAttribs[i] = ((int)TextColors.Black << 4) | ((int)TextColors.White);
        }
      }

      int y = 0;
      int x;
      int width = _pageRenderWidth / 40;
      int height = (_pageRenderHeight - 2) / 25;
      int fntSize = (width - 2 < 10) ? 10 : width - 2;
      _fontTeletext = new System.Drawing.Font("Courier New", fntSize, System.Drawing.FontStyle.Bold);
      System.Drawing.SolidBrush brush=null;
      try
      {
        if (isSubtitlePage)
          brush = new System.Drawing.SolidBrush(System.Drawing.Color.HotPink);
        else
          brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);

        _renderGraphics.FillRectangle(brush, 0, 0, _pageRenderWidth, _pageRenderHeight);

        if (_renderGraphics != null && _pageBitmap != null)
        {
          for (row = 0; row < 25; row++)
          {
            if (isSubtitlePage && row == 24) break;
            x = 0;

            for (col = 0; col < 40; col++)
              Render(_renderGraphics, pageChars[row * 40 + col], pageAttribs[row * 40 + col], ref x, ref y, width, height, isSubtitlePage, txtLanguage);

            y += height + (row == 23 ? 2 : 0);
          }
        }
      }
      finally
      {
        if (brush!=null)
          brush.Dispose();
        brush = null;
        _fontTeletext.Dispose();
        _fontTeletext = null;
      }
      return _pageBitmap;
      // send the bitmap to the callback
    }

    #region private members
    bool IsDecimalPage(int i)
    {
      return (bool)(((i & 0x00F) <= 9) && ((i & 0x0F0) <= 0x90));
    }

    bool IsDecimalSubPage(int i)
    {
      if (i >= 0x80) return false;

      return (bool)(((i & 0x00F) <= 9) && ((i & 0x0F0) <= 0x70));
    }
    #endregion

  }
}

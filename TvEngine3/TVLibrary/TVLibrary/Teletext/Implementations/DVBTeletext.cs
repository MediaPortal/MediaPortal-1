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
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections;
using System.Reflection;
using System.IO;

namespace TvLibrary.Teletext
{
  /// <summary>
  /// Zusammenfassung für DVBTeletext.
  /// </summary>
  public class DVBTeletext : ITeletext
  {
    #region constants
    const int MIN_PAGE = 0x100;
    const int MAX_PAGE = 0x900;
    const int MAX_SUB_PAGES = 0x80;
    const int MAX_MAGAZINE = 8;
    #endregion

    #region delegates
    public event PageEventHandler OnPageUpdated;
    public event PageEventHandler OnPageAdded;
    public event PageEventHandler OnPageDeleted;
    #endregion

    #region variables
    TeletextPageCache _pageCache = new TeletextPageCache();
    TeletextPageRenderer _renderer = new TeletextPageRenderer();
    TeletextDecoder _decoder;
    FastTextDecoder _fastTextDecoder = new FastTextDecoder();
    ToptextDecoder _topTextDecoder = new ToptextDecoder();

    int _currentPageNumber = 0x100;
    int _currentSubPageNumber = 0;

    byte[] analogBuffer = new byte[2048];
    byte[] tmpBuffer = new byte[46];

    #endregion

    #region character and other tables
    byte[] m_lutTable = new byte[] {0x00,0x08,0x04,0x0c,0x02,0x0a,0x06,0x0e,
										 0x01,0x09,0x05,0x0d,0x03,0x0b,0x07,0x0f,
										 0x00,0x80,0x40,0xc0,0x20,0xa0,0x60,0xe0,
										 0x10,0x90,0x50,0xd0,0x30,0xb0,0x70,0xf0
									 };

    #endregion

    #region ctor/dtor
    public DVBTeletext()
    {
      _decoder = new TeletextDecoder(ref _pageCache);
      _pageCache.OnPageAdded += new PageEventHandler(_pageCache_OnPageAdded);
      _pageCache.OnPageDeleted += new PageEventHandler(_pageCache_OnPageDeleted);
      _pageCache.OnPageUpdated += new PageEventHandler(_pageCache_OnPageUpdated);
    }

    ~DVBTeletext()
    {
      ClearBuffer();
    }

    #endregion

    #region event callbacks
    void _pageCache_OnPageUpdated(int pageNumber, int subPageNumber)
    {
      //  Trace.WriteLine(String.Format("{0:X}/{1:X} updated", pageNumber, subPageNumber));
      if (OnPageUpdated != null)
      {
        OnPageUpdated(pageNumber, subPageNumber);
      }
    }

    void _pageCache_OnPageDeleted(int pageNumber, int subPageNumber)
    {
      // Trace.WriteLine(String.Format("{0:X}/{1:X} deleted", pageNumber, subPageNumber));
      if (OnPageDeleted != null)
      {
        OnPageDeleted(pageNumber, subPageNumber);
      }
    }

    void _pageCache_OnPageAdded(int pageNumber, int subPageNumber)
    {
      //Trace.WriteLine(String.Format("{0:X}/{1:X} added", pageNumber, subPageNumber));
      if (OnPageAdded != null)
      {
        OnPageAdded(pageNumber, subPageNumber);
      }
    }
    #endregion

    #region fasttext
    public int PageRed
    {
      get
      {
        if (_fastTextDecoder.Red > 0) return _fastTextDecoder.Red;
        return _topTextDecoder.Red;
      }
    }
    public int PageGreen
    {
      get
      {
        if (_fastTextDecoder.Green > 0) return _fastTextDecoder.Green;
        return _topTextDecoder.Green;
      }
    }
    public int PageYellow
    {
      get
      {
        if (_fastTextDecoder.Yellow > 0) return _fastTextDecoder.Yellow;
        return _topTextDecoder.Yellow;
      }
    }
    public int PageBlue
    {
      get
      {
        if (_fastTextDecoder.Blue > 0) return _fastTextDecoder.Blue;
        return _topTextDecoder.Blue;
      }
    }
    public string PageSelectText
    {
      get
      {
        return _renderer.PageSelectText;
      }
      set
      {
        _renderer.PageSelectText = value;
      }
    }
    #endregion

    #region properties
    public int PercentageOfMaximumHeight {
      get {
        return _renderer.PercentageOfMaximumHeight;
      }
      set {
        _renderer.PercentageOfMaximumHeight = value;
      }
    }

    public bool HiddenMode
    {
      get
      {
        return _renderer.HiddenMode;
      }
      set
      {
        _renderer.HiddenMode = value;
      }
    }
    public bool TransparentMode
    {
      get { return _renderer.TransparentMode; }
      set { _renderer.TransparentMode = value; }
    }
    public bool FullscreenMode {
      get { return _renderer.FullscreenMode; }
      set { _renderer.FullscreenMode = value; }
    }
    #endregion

    #region public methods
    #region channel name
    public string GetTeletextChannelName()
    {
      return _pageCache.ChannelName;
    }
    public void ClearTeletextChannelName()
    {
      _pageCache.ChannelName = "";
    }
    #endregion

    public void ClearBuffer()
    {
      _decoder.Clear();
      _renderer.Clear();
      _pageCache.Clear();
    }

    public TimeSpan RotationTime(int currentPageNumber)
    {
      return _pageCache.RotationTime(currentPageNumber);
    }

    public void SetPageSize(int renderWidth, int renderHeight)
    {
      _renderer.Width = renderWidth;
      _renderer.Height = renderHeight;
    }

    public byte[] GetRawPage(int page, int subpage)
    {

      _currentPageNumber = page;
      _currentSubPageNumber = subpage;
      if (_currentPageNumber < MIN_PAGE)
        _currentPageNumber = MIN_PAGE;
      if (_currentPageNumber >= MAX_PAGE)
        _currentPageNumber = MAX_PAGE - 1;

      if (_pageCache.SubPageExists(_currentPageNumber, _currentSubPageNumber))
      {
        byte[] byPage = _pageCache.GetPage(_currentPageNumber, _currentSubPageNumber);
        if (byPage == null)
        {
          return null;
        }
        _fastTextDecoder.Decode(byPage);

        if (_topTextDecoder.Decode(_pageCache, _currentPageNumber))
        {
          AddTopTextRow24(ref byPage);
        }
        return byPage;
      }
      else
      {
        return null;
      }
    }

    public System.Drawing.Bitmap GetPage(int page, int subpage)
    {
      _currentPageNumber = page;
      _currentSubPageNumber = subpage;
      if (_currentPageNumber < MIN_PAGE)
        _currentPageNumber = MIN_PAGE;
      if (_currentPageNumber >= MAX_PAGE)
        _currentPageNumber = MAX_PAGE - 1;

      if (_pageCache.SubPageExists(_currentPageNumber, _currentSubPageNumber))
      {
        byte[] byPage = _pageCache.GetPage(_currentPageNumber, _currentSubPageNumber);
        _fastTextDecoder.Decode(byPage);

        if (_topTextDecoder.Decode(_pageCache, _currentPageNumber))
        {
          AddTopTextRow24(ref byPage);
        }
        return _renderer.RenderPage(byPage, _currentPageNumber, _currentSubPageNumber);
      }
      else
      {
        for (int sub = 0; sub < MAX_SUB_PAGES; sub++)
        {
          if (_pageCache.SubPageExists(_currentPageNumber, sub))//return first aval. subpage
          {
            _currentSubPageNumber = sub;
            byte[] byPage = _pageCache.GetPage(_currentPageNumber, _currentSubPageNumber);
            _fastTextDecoder.Decode(byPage);
            if (_topTextDecoder.Decode(_pageCache, _currentPageNumber))
            {
              AddTopTextRow24(ref byPage);
            }
            return _renderer.RenderPage(byPage, _currentPageNumber, _currentSubPageNumber);
          }
        }

        Assembly assm = Assembly.GetExecutingAssembly();
        string[] names = assm.GetManifestResourceNames();
        //for (int x = 0; x < names.Length; x++)
        //  Log.Write("res:{0}", names[x]);

        Stream stream = assm.GetManifestResourceStream("TVCapture.teletext.LogoPage");
        if (stream != null)
        {
          using (BinaryReader reader = new BinaryReader(stream))
          {
            byte[] logoPage = new byte[stream.Length];
            reader.Read(logoPage, 0, (int)stream.Length);
            _fastTextDecoder.Decode(logoPage);
            _topTextDecoder.Clear();
            return _renderer.RenderPage(logoPage, _currentPageNumber, 0);
          }
        }
        return null;
      }
    }

    public int NumberOfSubpages(int currentPageNumber)
    {
      return _pageCache.NumberOfSubpages(currentPageNumber);
    }
    #endregion

    #region decoding
    public void SaveData(IntPtr dataPtr)
    {
      if (dataPtr == IntPtr.Zero) return;
      int dataAdd = (int)dataPtr;
      try
      {
        for (int line = 0; line < 4; line++)
        {
          Marshal.Copy((IntPtr)((dataAdd + 4) + (line * 0x2e)), tmpBuffer, 0, 46);

          if ((tmpBuffer[0] == 0x02 || tmpBuffer[0] == 0x03) && (tmpBuffer[1] == 0x2C))
          {
            for (int b = 4; b < 46; b++)
            {
              byte upper = 0;
              byte lower = 0;
              upper = (byte)((tmpBuffer[b] >> 4) & 0xf);
              lower = (byte)(tmpBuffer[b] & 0xf);
              tmpBuffer[b - 4] = (byte)((m_lutTable[upper]) | (m_lutTable[lower + 16]));
            }//for(b=4;
            _decoder.Decode( tmpBuffer, 0, 1);
          }//if ((tmpBuffer
        }// for(line=0
      }
      catch (Exception)
      {
      }
    }
    #endregion

    #region rendering
    void AddTopTextRow24(ref byte[] byPage)
    {
      int offsetRow24 = -1;
      int maxRows = byPage.Length / 42;
      for (int row = 0; row < maxRows; ++row)
      {
        int packetNr = Hamming.GetPacketNumber(row * 42, ref byPage);
        if (packetNr == 24 || packetNr < 0)
        {
          offsetRow24 = row * 42;
          break;
        }
      }
      if (offsetRow24 < 0) return;
      byte[] row24 = _topTextDecoder.Row24;
      for (int i = 0; i < 42; ++i)
      {
        byPage[i + offsetRow24] = row24[i];
      }
    }


    #endregion

    #region helper functions
    int GetNextDecimal(int val)
    {
      int ret = val;
      ret++;

      if ((ret & 15) > 9)
        ret += 6;

      if ((ret & 240) > 144)
        ret += 96;

      if (ret > 2201)
        ret = 256;

      return ret;
    }
    bool IsText(byte val)
    {
      if (val >= ' ')
        return true;
      return false;

    }
    bool IsAlphaNumeric(byte val)
    {
      if (val >= 'A' && val <= 'Z')
        return true;
      if (val >= 'a' && val <= 'z')
        return true;
      if (val >= '0' && val <= '9')
        return true;
      return false;
    }
    int GetPreviousDecimal(int val)           /* counting down */
    {
      int ret = val;
      ret--;

      if ((ret & 15) > 0x09)
        ret -= 6;

      if ((ret & 240) > 144)
        ret -= 96;

      if (ret < 256)
        ret = 2201;

      return ret;
    }

    #endregion
  }// class
}// namespace

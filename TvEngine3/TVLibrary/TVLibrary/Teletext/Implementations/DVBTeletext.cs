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
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Drawing;

namespace TvLibrary.Teletext
{
  /// <summary>
  /// Zusammenfassung für DVBTeletext.
  /// </summary>
  public class DVBTeletext : ITeletext
  {
    #region constants

    private const int MIN_PAGE = 0x100;
    private const int MAX_PAGE = 0x900;
    private const int MAX_SUB_PAGES = 0x80;

    #endregion

    #region delegates

    /// <summary>
    /// Page update event
    /// </summary>
    public event PageEventHandler OnPageUpdated;

    /// <summary>
    /// Page added event
    /// </summary>
    public event PageEventHandler OnPageAdded;

    /// <summary>
    /// Page deleted event
    /// </summary>
    public event PageEventHandler OnPageDeleted;

    #endregion

    #region variables

    private readonly TeletextPageCache _pageCache = new TeletextPageCache();
    private readonly TeletextPageRenderer _renderer = new TeletextPageRenderer();
    private readonly TeletextDecoder _decoder;
    private readonly FastTextDecoder _fastTextDecoder = new FastTextDecoder();
    private readonly ToptextDecoder _topTextDecoder = new ToptextDecoder();

    private int _currentPageNumber = 0x100;
    private int _currentSubPageNumber;

    private readonly byte[] tmpBuffer = new byte[46];

    #endregion

    #region character and other tables

    private readonly byte[] m_lutTable = new byte[]
                                           {
                                             0x00, 0x08, 0x04, 0x0c, 0x02, 0x0a, 0x06, 0x0e,
                                             0x01, 0x09, 0x05, 0x0d, 0x03, 0x0b, 0x07, 0x0f,
                                             0x00, 0x80, 0x40, 0xc0, 0x20, 0xa0, 0x60, 0xe0,
                                             0x10, 0x90, 0x50, 0xd0, 0x30, 0xb0, 0x70, 0xf0
                                           };

    #endregion

    #region ctor/dtor

    ///<summary>
    /// Constructor
    ///</summary>
    public DVBTeletext()
    {
      _decoder = new TeletextDecoder(_pageCache);
      _pageCache.OnPageAdded += _pageCache_OnPageAdded;
      _pageCache.OnPageDeleted += _pageCache_OnPageDeleted;
      _pageCache.OnPageUpdated += _pageCache_OnPageUpdated;
    }

    /// <summary>
    /// Disposes the DVB teletext
    /// </summary>
    ~DVBTeletext()
    {
      ClearBuffer();
    }

    #endregion

    #region event callbacks

    private void _pageCache_OnPageUpdated(int pageNumber, int subPageNumber)
    {
      //  Trace.WriteLine(String.Format("{0:X}/{1:X} updated", pageNumber, subPageNumber));
      if (OnPageUpdated != null)
      {
        OnPageUpdated(pageNumber, subPageNumber);
      }
    }

    private void _pageCache_OnPageDeleted(int pageNumber, int subPageNumber)
    {
      // Trace.WriteLine(String.Format("{0:X}/{1:X} deleted", pageNumber, subPageNumber));
      if (OnPageDeleted != null)
      {
        OnPageDeleted(pageNumber, subPageNumber);
      }
    }

    private void _pageCache_OnPageAdded(int pageNumber, int subPageNumber)
    {
      //Trace.WriteLine(String.Format("{0:X}/{1:X} added", pageNumber, subPageNumber));
      if (OnPageAdded != null)
      {
        OnPageAdded(pageNumber, subPageNumber);
      }
    }

    #endregion

    #region fasttext

    /// <summary>
    /// Gets the red teletext page
    /// </summary>
    public int PageRed
    {
      get
      {
        if (_fastTextDecoder.Red > 0)
          return _fastTextDecoder.Red;
        return _topTextDecoder.Red;
      }
    }

    /// <summary>
    /// Gets the green teletext page
    /// </summary>
    public int PageGreen
    {
      get
      {
        if (_fastTextDecoder.Green > 0)
          return _fastTextDecoder.Green;
        return _topTextDecoder.Green;
      }
    }

    /// <summary>
    /// Gets the yellow teletext page
    /// </summary>
    public int PageYellow
    {
      get
      {
        if (_fastTextDecoder.Yellow > 0)
          return _fastTextDecoder.Yellow;
        return _topTextDecoder.Yellow;
      }
    }

    /// <summary>
    /// Gets the blue teletext page
    /// </summary>
    public int PageBlue
    {
      get
      {
        if (_fastTextDecoder.Blue > 0)
          return _fastTextDecoder.Blue;
        return _topTextDecoder.Blue;
      }
    }

    /// <summary>
    /// Gets the page select text.
    /// </summary>
    /// <value>The page select text.</value>
    public string PageSelectText
    {
      get { return _renderer.PageSelectText; }
      set { _renderer.PageSelectText = value; }
    }

    #endregion

    #region properties

    /// <summary>
    /// Gets/Sets  the percentage of the maximum height for the font size
    /// </summary>
    /// <value>Percentage of the maximum height of font size.</value>
    public int PercentageOfMaximumHeight
    {
      get { return _renderer.PercentageOfMaximumHeight; }
      set { _renderer.PercentageOfMaximumHeight = value; }
    }

    /// <summary>
    /// turns on/off the conceal (hidden) mode
    /// </summary>
    public bool HiddenMode
    {
      get { return _renderer.HiddenMode; }
      set { _renderer.HiddenMode = value; }
    }

    /// <summary>
    /// turns on/off transparent mode. In transparent mode the
    /// teletext page is rendered on transparent background
    /// </summary>
    public bool TransparentMode
    {
      get { return _renderer.TransparentMode; }
      set { _renderer.TransparentMode = value; }
    }

    ///<summary>
    /// Gets/Sets the fullscreen mode
    ///</summary>
    public bool FullscreenMode
    {
      get { return _renderer.FullscreenMode; }
      set { _renderer.FullscreenMode = value; }
    }

    #endregion

    #region public methods

    #region channel name

    /// <summary>
    /// Gets the teletext channel name
    /// </summary>
    /// <returns>Channel name</returns>
    public string GetTeletextChannelName()
    {
      return _pageCache.ChannelName;
    }

    /// <summary>
    /// Clears the stored teletext channel name
    /// </summary>
    public void ClearTeletextChannelName()
    {
      _pageCache.ChannelName = "";
    }

    #endregion

    /// <summary>
    /// Clears the buffers and cache
    /// </summary>
    public void ClearBuffer()
    {
      _decoder.Clear();
      _renderer.Clear();
      _pageCache.Clear();
    }

    /// <summary>
    /// returns the rotation time for the page.
    /// </summary>
    /// <param name="currentPageNumber">The current page number.</param>
    /// <returns>timespan contain the rotation time</returns>
    public TimeSpan RotationTime(int currentPageNumber)
    {
      return _pageCache.RotationTime(currentPageNumber);
    }

    /// <summary>
    /// sets the width/height of the bitmap generated by GetPage()
    /// </summary>
    /// <param name="renderWidth">width in pixels</param>
    /// <param name="renderHeight">height in pixels</param>
    public void SetPageSize(int renderWidth, int renderHeight)
    {
      _renderer.Width = renderWidth;
      _renderer.Height = renderHeight;
    }

    /// <summary>
    /// Gets the raw teletext page.
    /// </summary>
    /// <param name="page">pagenumber (0x100-0x899)</param>
    /// <param name="subpage">subpagenumber (0x0-0x79)</param>
    /// <returns>raw teletext page (or null if page is not found)</returns>
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
      return null;
    }

    /// <summary>
    /// Gets the teletext page and renders it to a Bitmap
    /// </summary>
    /// <param name="page">pagenumber (0x100-0x899)</param>
    /// <param name="subpage">subpagenumber (0x0-0x79)</param>
    /// <returns>bitmap (or null if page is not found)</returns>
    public Bitmap GetPage(int page, int subpage)
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
      for (int sub = 0; sub < MAX_SUB_PAGES; sub++)
      {
        if (_pageCache.SubPageExists(_currentPageNumber, sub)) //return first aval. subpage
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

    /// <summary>
    /// returns the total number of subpages for a pagnumber
    /// </summary>
    /// <param name="currentPageNumber">pagenumber 0x100-0x899</param>
    /// <returns>number of subpages for this pagenumber</returns>
    public int NumberOfSubpages(int currentPageNumber)
    {
      return _pageCache.NumberOfSubpages(currentPageNumber);
    }

    #endregion

    #region decoding

    ///<summary>
    /// Saves the given data in the cache and decodes
    ///</summary>
    ///<param name="dataPtr">Teletext data</param>
    public void SaveData(IntPtr dataPtr)
    {
      if (dataPtr == IntPtr.Zero)
        return;
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
              byte upper = (byte)((tmpBuffer[b] >> 4) & 0xf);
              byte lower = (byte)(tmpBuffer[b] & 0xf);
              tmpBuffer[b - 4] = (byte)((m_lutTable[upper]) | (m_lutTable[lower + 16]));
            } //for(b=4;
            _decoder.Decode(tmpBuffer, 0, 1);
          } //if ((tmpBuffer
        } // for(line=0
      }
      catch (Exception ex)
      {
        Log.Log.WriteFile("Error while saving teletext data: ", ex);
      }
    }

    #endregion

    #region rendering

    private void AddTopTextRow24(ref byte[] byPage)
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
      if (offsetRow24 < 0)
        return;
      byte[] row24 = _topTextDecoder.Row24;
      for (int i = 0; i < 42; ++i)
      {
        byPage[i + offsetRow24] = row24[i];
      }
    }

    #endregion
  }

// class
}

// namespace
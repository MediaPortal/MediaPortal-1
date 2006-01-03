using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaPortal.TV.Teletext
{
  public class TeletextDecoder
  {

    #region delegates
    public delegate void PageUpdated(int pageNumber, int subPageNumber);
    public event PageUpdated PageUpdatedEvent;
    #endregion

    #region constants
    const int MAX_MAGAZINE = 8;
    #endregion

    #region variables
    int[] _magazineCurrentSubPage = new int[MAX_MAGAZINE + 2];
    int[] _magazineCurrentPageNr = new int[MAX_MAGAZINE + 2];
    int[] _magazineLastRow = new int[MAX_MAGAZINE + 2];
    TeletextPageCache _pageCache;
    #endregion

    #region ctor
    public TeletextDecoder(ref TeletextPageCache cache)
    {
      _pageCache = cache;
      Clear();
    }
    #endregion

    #region public members
    public void Clear()
    {
      for (int i = 0; i <= MAX_MAGAZINE;++i )
      {
        _magazineCurrentPageNr[i] = -1;
        _magazineLastRow[i] = -1;
      }
      _pageCache.Clear();
    }

    public void Decode(byte[] rowData, int rows)
    {
      int line = 0;
      int b = 0, byte1 = 0, byte2 = 0, byte3 = 0, byte4 = 0;
      int packetNumber;
      byte magazine;
      for (line = 0; line < rows; line++)
      {
        int off = line * 42;
        bool copyData = false;
        byte1 = Hamming.Decode[rowData[off + 0]];
        byte2 = Hamming.Decode[rowData[off + 1]];

        //check for invalid hamming bytes
        if (byte1 == 0xFF || byte2 == 0xFF)
          continue;

        //get packet number
        packetNumber = Hamming.GetPacketNumber(off, ref rowData);

        //  get magazine 
        magazine = (byte)(Hamming.Decode[rowData[off+0]] & 7);
        _magazineLastRow[magazine] = packetNumber;

        //ignore invalid packets and packets 25,26,28,29,30,31
        if (packetNumber < 0 || packetNumber == 25 || packetNumber == 26 || packetNumber > 27) continue;
        

        if (packetNumber == 0)
        {
          if (PageUpdatedEvent != null)
          {
            if (_magazineCurrentPageNr[magazine] != -1 && _magazineCurrentSubPage[magazine] != -1)
            {
              PageUpdatedEvent(_magazineCurrentPageNr[magazine], _magazineCurrentSubPage[magazine]);
            }
          }
          // start of new teletext page...
          bool headerError = false;
          for (int i = 2; i <= 9; ++i)
          {
            if (Hamming.Decode[rowData[off + i]] == 0xFF)
            {
              headerError = true;
              break;
            }
          }
          if (headerError == true)
          {
            _magazineCurrentPageNr[magazine] = -1;
            _magazineCurrentSubPage[magazine] = -1;
            continue;
          }

          int pageNr = Hamming.GetPageNumber(off, ref rowData);
          int subPageNr = Hamming.GetSubPageNumber(off, ref rowData);

          if (!ToptextDecoder.IsTopTextPage(pageNr, subPageNr))
          {
            if (!IsDecimalPage(pageNr))
            {
              _magazineCurrentPageNr[magazine] = -1;
              _magazineCurrentSubPage[magazine] = -1;
              continue;
            }
            if (!IsDecimalSubPage(subPageNr))
            {
              _magazineCurrentPageNr[magazine] = -1;
              _magazineCurrentSubPage[magazine] = -1;
              continue;
            }
          }
          else
          {
            subPageNr = 0;
          }
          _magazineCurrentPageNr[magazine] = pageNr;
          _magazineCurrentSubPage[magazine] = subPageNr;

          //strip parity of header
          for (b = 10; b < 42; b++)
          {
            rowData[off+b] &= 0x7f;
          }

          if (Hamming.IsEraseBitSet(off, ref rowData))   /* C4 -> erase page */
          {
            _pageCache.ClearPage(_magazineCurrentPageNr[magazine], _magazineCurrentSubPage[magazine]);
          }
          copyData = true;
        }
        else if (packetNumber <= 24)
        {
          if (_magazineCurrentPageNr[magazine] == -1)
          {
            continue;
          }
          if (_magazineLastRow[magazine] != 27)
          {
            if (packetNumber < _magazineLastRow[magazine])
            {
              continue;
            }
          }

          for (b = 2; b < 42; b++)
          {
            rowData[off+b] &= 0x7f; // strip parity
          }
          copyData = true;
        }
        else if (packetNumber == 27)
        {
          if (_magazineCurrentPageNr[magazine] == -1) continue;
          if (packetNumber < _magazineLastRow[magazine]) continue;
          copyData = true;
        }

        if (copyData)
        {
          if (_magazineCurrentPageNr[magazine] != -1 && _magazineCurrentSubPage[magazine] != -1)
          {
            _pageCache.AllocPage(_magazineCurrentPageNr[magazine], _magazineCurrentSubPage[magazine]);
            IntPtr ptrPage=_pageCache.GetPagePtr(_magazineCurrentPageNr[magazine], _magazineCurrentSubPage[magazine]);
            if (ptrPage != IntPtr.Zero)
            {
              Marshal.Copy(rowData, off, new IntPtr(ptrPage.ToInt32() + (packetNumber * 42)), 42);
            }
          }
        }
      }// for (line = 0; line < rows; line++)
    }//void Decode(byte[] rowData)
    #endregion

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

  }//class TeletextDecoder
}

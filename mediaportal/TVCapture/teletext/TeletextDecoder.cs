#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

using System;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;

namespace MediaPortal.TV.Teletext
{
  public class TeletextDecoder
  {
    #region delegates

    public delegate void PageUpdated(int pageNumber, int subPageNumber);

    public event PageUpdated PageUpdatedEvent;

    #endregion

    #region constants

    private const int MAX_MAGAZINE = 8;

    #endregion

    #region variables

    private int[] _magazineCurrentSubPage = new int[MAX_MAGAZINE + 2];
    private int[] _magazineCurrentPageNr = new int[MAX_MAGAZINE + 2];
    private int[] _magazineLastRow = new int[MAX_MAGAZINE + 2];
    private TeletextPageCache _pageCache;

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
      for (int i = 0; i <= MAX_MAGAZINE; ++i)
      {
        _magazineCurrentPageNr[i] = -1;
        _magazineLastRow[i] = -1;
      }
      _pageCache.Clear();
    }

    public void Decode(byte[] rowData, int rows)
    {
      try
      {
        int line = 0;
        int b = 0, byte1 = 0, byte2 = 0;
        int packetNumber;
        byte magazine;
        for (line = 0; line < rows; line++)
        {
          int off = line*42;
          bool copyData = false;
          byte1 = Hamming.Decode[rowData[off + 0]];
          byte2 = Hamming.Decode[rowData[off + 1]];

          //check for invalid hamming bytes
          if (byte1 == 0xFF || byte2 == 0xFF)
          {
            continue;
          }

          //get packet number
          packetNumber = Hamming.GetPacketNumber(off, ref rowData);

          //  get magazine 
          magazine = (byte) (Hamming.Decode[rowData[off + 0]] & 7);
          _magazineLastRow[magazine] = packetNumber;

          if (packetNumber == 30)
          {
            byte type = (byte) (Hamming.Decode[rowData[off + 2]]);
            if ((type != 0) && (type != 2))
            {
              continue;
            }
            //Log.Info("Packet Number:{0}, type:{1}", packetNumber, type);
            string channelName = "";
            for (int i = 0; i < 20; i++)
            {
              char char1 = (char) (rowData[off + 22 + i] & 127);
              //Log.Info("{0}-{1:x}", char1, (byte)(rowData[off + 22 + i] & 127));
              channelName += char1;
            }
            int pos = channelName.LastIndexOf("teletext", StringComparison.InvariantCultureIgnoreCase);
            if (pos != -1)
            {
              channelName = channelName.Substring(0, pos);
            }
            //Some times channel name includes program name after :
            pos = channelName.LastIndexOf(":");
            if (pos != -1)
            {
              channelName = channelName.Substring(0, pos);
            }
            channelName = channelName.TrimEnd(new char[] {'\'', '\"', '´', '`'});
            channelName = channelName.Trim();
            _pageCache.ChannelName = channelName;
            continue;
          }
          //ignore invalid packets and packets 25,26,28,29,30,31
          if (packetNumber < 0 || packetNumber == 25 || packetNumber == 26 || packetNumber > 27)
          {
            continue;
          }


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
              rowData[off + b] &= 0x7f;
            }

            if (Hamming.IsEraseBitSet(off, ref rowData)) /* C4 -> erase page */
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
              rowData[off + b] &= 0x7f; // strip parity
            }
            copyData = true;
          }
          else if (packetNumber == 27)
          {
            if (_magazineCurrentPageNr[magazine] == -1)
            {
              continue;
            }
            if (packetNumber < _magazineLastRow[magazine])
            {
              continue;
            }
            copyData = true;
          }

          if (copyData)
          {
            if (_magazineCurrentPageNr[magazine] != -1 && _magazineCurrentSubPage[magazine] != -1)
            {
              _pageCache.AllocPage(_magazineCurrentPageNr[magazine], _magazineCurrentSubPage[magazine]);
              IntPtr ptrPage = _pageCache.GetPagePtr(_magazineCurrentPageNr[magazine], _magazineCurrentSubPage[magazine]);
              if (ptrPage != IntPtr.Zero)
              {
                Marshal.Copy(rowData, off, new IntPtr(ptrPage.ToInt32() + (packetNumber*42)), 42);
              }
            }
          }
        } // for (line = 0; line < rows; line++)
      }
      catch (Exception ex)
      {
        Log.Error("Exception while decoding teletext");
        Log.Error(ex);
      }
    } //void Decode(byte[] rowData)

    #endregion

    #region private members

    private bool IsDecimalPage(int i)
    {
      return (bool) (((i & 0x00F) <= 9) && ((i & 0x0F0) <= 0x90));
    }

    private bool IsDecimalSubPage(int i)
    {
      if (i >= 0x80)
      {
        return false;
      }

      return (bool) (((i & 0x00F) <= 9) && ((i & 0x0F0) <= 0x70));
    }

    #endregion
  } //class TeletextDecoder
}
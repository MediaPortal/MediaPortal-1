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
using System;
using System.Collections.Generic;

namespace TvLibrary.Teletext
{
  /// <summary>
  /// teletext decoder class
  /// </summary>
  public class TeletextDecoder
  {
    #region constants

    private const int MAX_MAGAZINE = 8;

    #endregion

    #region variables

    private readonly int[] _magazineCurrentSubPage = new int[MAX_MAGAZINE + 2];
    private readonly int[] _magazineCurrentPageNr = new int[MAX_MAGAZINE + 2];
    private readonly int[] _magazineLastRow = new int[MAX_MAGAZINE + 2];
    private readonly string[] _vbiLine = new string[MAX_MAGAZINE + 2];
    private readonly List<byte[]> _workingPage = new List<byte[]>();
    private readonly TeletextPageCache _pageCache;
    //bool[,] _rowsReceived = new bool[MAX_MAGAZINE + 2, 32];
    private string _line = String.Empty;
    private bool _isSerial = true;
    private int _prevMagazine;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="TeletextDecoder"/> class.
    /// </summary>
    /// <param name="cache">The cache.</param>
    public TeletextDecoder(TeletextPageCache cache)
    {
      _prevMagazine = 0;
      _pageCache = cache;
      for (int i = 0; i < MAX_MAGAZINE + 2; ++i)
      {
        _vbiLine[i] = "";
        byte[] page = new byte[2100];
        _workingPage.Add(page);
      }
      Clear();
    }

    #endregion

    #region public members

    /// <summary>
    /// Clears this instance.
    /// </summary>
    public void Clear()
    {
      for (int i = 0; i <= MAX_MAGAZINE; ++i)
      {
        _magazineCurrentPageNr[i] = -1;
        _magazineLastRow[i] = -1;
        _vbiLine[i] = "";
      }
      _pageCache.Clear();
    }

    /// <summary>
    /// Decodes the specified row data.
    /// </summary>
    /// <param name="rowData">The row data.</param>
    /// <param name="startOff">The start off.</param>
    /// <param name="rows">The rows.</param>
    public void Decode(byte[] rowData, int startOff, int rows)
    {
      _line = "";
      try
      {
        int line;
        for (line = 0; line < rows; line++)
        {
          int off = startOff + line * 43;
          bool copyData = false;
          int byte1 = Hamming.Decode[rowData[off + 0]];
          int byte2 = Hamming.Decode[rowData[off + 1]];

          //check for invalid hamming bytes
          if (byte1 == 0xFF || byte2 == 0xFF)
          {
            //hamming error
            _line += "HE1 ";
            continue;
          }
          byte magazine = (byte)(byte1 & 0x7);

          //get packet number
          int packetNumber = Hamming.GetPacketNumber(off, ref rowData);
          if (packetNumber < 0)
          {
            _line += "HE2 ";
            _vbiLine[magazine] += "HE2 ";
            continue;
          }

          if (packetNumber == 0)
          {
            _line += String.Format("{0:X}'", Hamming.GetPageNumber(off, ref rowData));
            _vbiLine[magazine] += String.Format(" [{0}] {1:X}'", (line), Hamming.GetPageNumber(off, ref rowData));
          }
          else if (packetNumber < 10)
          {
            _line += String.Format("{0}0{1} ", magazine, packetNumber);
            _vbiLine[magazine] += String.Format(" [{0}] {1}0{2} ", (line), magazine, packetNumber);
          }
          else
          {
            _line += String.Format("{0}{1} ", magazine, packetNumber);
            _vbiLine[magazine] += String.Format(" [{0}] {1}{2} ", (line), magazine, packetNumber);
          }


          if (packetNumber == 30 && magazine == 0)
          {
            /*
            byte type = (byte)(Hamming.Decode[rowData[off + 2]]);
            if ((type != 0) && (type != 2))
            {
              continue;
            }
             */
            //Log.Write("Packet Number:{0}, type:{1}", packetNumber, type);
            string channelName = "";
            for (int i = 0; i < 20; i++)
            {
              char char1 = (char)(rowData[off + 22 + i] & 127);
              //Log.Write("{0}-{1:x}", char1, (byte)(rowData[off + 22 + i] & 127));
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
            UpdatePage(magazine);
            _vbiLine[magazine] = String.Format(" [{0}] {1:X}'", (line), Hamming.GetPageNumber(off, ref rowData));
            _magazineCurrentPageNr[magazine] = -1;
            _magazineCurrentSubPage[magazine] = -1;

            // check if header contains errors
            bool headerError = false;
            for (int i = 2; i <= 9; ++i)
            {
              if (Hamming.Decode[rowData[off + i]] == 0xFF)
              {
                headerError = true;
                break;
              }
            }
            if (headerError)
            {
              //yes then ignore this header.
              _line += "[HER]";
              continue;
            }

            int pageNr = Hamming.GetPageNumber(off, ref rowData);
            int subPageNr = Hamming.GetSubPageNumber(off, ref rowData);
            _isSerial = Hamming.IsSerial(off, ref rowData);
            _prevMagazine = magazine;

            if (!ToptextDecoder.IsTopTextPage(pageNr, subPageNr))
            {
              if (!IsDecimalPage(pageNr))
              {
                //ignore hexadecimal pages
                _magazineCurrentPageNr[magazine] = -1;
                _magazineCurrentSubPage[magazine] = -1;
                continue;
              }

              if (!IsDecimalSubPage(subPageNr))
              {
                //ignore hexadecimal subpages
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

            copyData = true;
            _magazineLastRow[magazine] = 0;
          }
          else if (packetNumber <= 24)
          {
            if (_magazineCurrentPageNr[magazine] == -1)
            {
              //no header received for this page
              continue;
            }
            if (_isSerial && _prevMagazine != magazine)
            {
              _magazineCurrentPageNr[magazine] = -1;
              _magazineCurrentSubPage[magazine] = -1;
              _line += "[MAG1]";
              continue;
            }
            if (_magazineLastRow[magazine] != 27)
            {
/*
              if (packetNumber <= _magazineLastRow[magazine])
              {
                if (_magazineLastRow[magazine] >= 23)
                {
                  //UpdatePage(magazine);
                  //continue;
                }
                // packet number received is less then previous row
                _magazineCurrentPageNr[magazine] = -1;
                _magazineCurrentSubPage[magazine] = -1;
                //System.Diagnostics.Trace.WriteLine(_line);
                //System.Diagnostics.Trace.WriteLine("ERR2");
                _line += "[ERR2]";
                continue;
              }
              if (_rowsReceived[magazine, packetNumber])
              {
                // packet number received is 2 times
                _magazineCurrentPageNr[magazine] = -1;
                _magazineCurrentSubPage[magazine] = -1;
                //System.Diagnostics.Trace.WriteLine(_line);
                //System.Diagnostics.Trace.WriteLine("ERR3");
                _line += "[ERR3]";
                continue;
              }*/
            }

            copyData = true;
            _magazineLastRow[magazine] = packetNumber;
          }
          else if (packetNumber == 27)
          {
            if (_magazineCurrentPageNr[magazine] == -1)
            {
              continue;
            }
            if (_isSerial && _prevMagazine != magazine)
            {
              _magazineCurrentPageNr[magazine] = -1;
              _magazineCurrentSubPage[magazine] = -1;
              _line += "[MAG2]";
              continue;
            }
            /*
            if (_magazineLastRow[magazine] >= 1)
            {
              if (_magazineLastRow[magazine] >= 23)
              {
                //UpdatePage(magazine);
                //continue;
              }
              _magazineCurrentPageNr[magazine] = -1;
              _magazineCurrentSubPage[magazine] = -1;
              //System.Diagnostics.Trace.WriteLine(_line);
              //System.Diagnostics.Trace.WriteLine("ERR5");
              _line += "[ERR4]";
              continue;
            }
            if (_rowsReceived[magazine, packetNumber])
            {
              // packet number received is 2 times
              _magazineCurrentPageNr[magazine] = -1;
              _magazineCurrentSubPage[magazine] = -1;
              //System.Diagnostics.Trace.WriteLine(_line);
              //System.Diagnostics.Trace.WriteLine("ERR6");
              _line += "[ERR5]";
              continue;
            }*/
            copyData = true;
            _magazineLastRow[magazine] = packetNumber;
          }

          if (copyData)
          {
            if (_magazineCurrentPageNr[magazine] != -1 && _magazineCurrentSubPage[magazine] != -1)
            {
              //_rowsReceived[magazine, packetNumber] = true;
              int offwp = packetNumber * 42;
              for (int c = 0; c < 42; ++c)
              {
                _workingPage[magazine][offwp + c] = rowData[off + c];
              }
            }
          }
        } // for (line = 0; line < rows; line++)
      }
      catch (Exception)
      {
        System.Diagnostics.Trace.WriteLine("EXCEPTION");
        //        Log.WriteFile(Log.LogType.Error,true,"Exception while decoding teletext");
        //        Log.Write(ex);
      }
      //System.Diagnostics.Trace.WriteLine(_line);
      //Log.Log.WriteFile(_line);
    }

//void Decode(byte[] rowData)

    #endregion

    #region private members

    private void UpdatePage(int magazine)
    {
      //page header
      if (_magazineCurrentPageNr[magazine] != -1 && _magazineCurrentSubPage[magazine] != -1)
      {
        _pageCache.PageReceived(_magazineCurrentPageNr[magazine], _magazineCurrentSubPage[magazine],
                                _workingPage[magazine], _vbiLine[magazine]);
      }
      _vbiLine[magazine] = "";
      _magazineCurrentPageNr[magazine] = -1;
      _magazineCurrentSubPage[magazine] = -1;
      _magazineLastRow[magazine] = -1;

      //clear working page
      for (int i = 0; i < 2100; ++i)
      {
        _workingPage[magazine][i] = 32;
      }
    }

    private static bool IsDecimalPage(int i)
    {
      return ((i & 0x00F) <= 9) && ((i & 0x0F0) <= 0x90);
    }

    private static bool IsDecimalSubPage(int i)
    {
      if (i >= 0x80)
        return false;

      return ((i & 0x00F) <= 9) && ((i & 0x0F0) <= 0x70);
    }

    #endregion
  }

//class TeletextDecoder
}
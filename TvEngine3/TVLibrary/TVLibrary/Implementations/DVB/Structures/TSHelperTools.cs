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
using System.Runtime.InteropServices;

namespace TvLibrary.Implementations.DVB.Structures
{
  /// <summary>
  /// Zusammenfassung für TSHelperTools.
  /// </summary>
  public class TSHelperTools
  {
    /// <summary>
    /// TS Header struct
    /// </summary>
    public struct TSHeader
    {
      /// <summary>
      /// Sync byte
      /// </summary>
      public int SyncByte;
      /// <summary>
      /// Transport error
      /// </summary>
      public bool TransportError;
      /// <summary>
      /// Payload unit start
      /// </summary>
      public bool PayloadUnitStart;
      /// <summary>
      /// Transport priority
      /// </summary>
      public bool TransportPriority;
      /// <summary>
      /// Pid
      /// </summary>
      public int Pid;
      /// <summary>
      /// Transport scrambling
      /// </summary>
      public int TransportScrambling;
      /// <summary>
      /// Adaption field control
      /// </summary>
      public int AdaptionFieldControl;
      /// <summary>
      /// Continuity Counter
      /// </summary>
      public int ContinuityCounter;
      /// <summary>
      /// Adpation field
      /// </summary>
      public int AdaptionField;
      /// <summary>
      /// Table id
      /// </summary>
      public int TableID;
      /// <summary>
      /// Section Len
      /// </summary>
      public int SectionLen;
      /// <summary>
      /// Is MHW Table
      /// </summary>
      public bool IsMHWTable;
      /// <summary>
      /// MHW Indicator
      /// </summary>
      public int MHWIndicator;
    }

    ///<summary>
    /// Converts the raw data into a TsHeader
    ///</summary>
    ///<param name="streamData">Raw data</param>
    ///<returns>Ts Header struct</returns>
    public TSHeader GetHeader(IntPtr streamData)
    {
      byte[] data = new byte[8];
      Marshal.Copy(streamData, data, 0, 8);
      TSHeader header = new TSHeader();
      header.SyncByte = data[0]; // indicates header is not valid
      if (data[0] != 0x47)
        return header;// no ts-header, return
      header.SyncByte = data[0];
      header.TransportError = (data[1] & 0x80) > 0 ? true : false;
      header.PayloadUnitStart = (data[1] & 0x40) > 0 ? true : false;
      header.TransportPriority = (data[1] & 0x20) > 0 ? true : false;
      header.Pid = ((data[1] & 0x1F) << 8) + data[2];
      header.TransportScrambling = data[3] & 0xC0;
      header.AdaptionFieldControl = (data[3] >> 4) & 0x3;
      header.ContinuityCounter = data[3] & 0x0F;
      header.AdaptionField = data[4];
      header.TableID = data[5];
      header.SectionLen = ((data[6] - 0x70) << 8) + data[7];
      return header;
    }
    ///<summary>
    /// Converts the binary data into a TsHeader
    ///</summary>
    ///<param name="data">Binary data</param>
    ///<returns>Ts Header struct</returns>
    public TSHeader GetHeader(byte[] data)
    {
      TSHeader header = new TSHeader();
      header.SyncByte = data[0]; // indicates header is not valid
      if (data[0] != 0x47)
        return header;// no ts-header, return
      header.SyncByte = data[0];
      header.TransportError = (data[1] & 0x80) > 0 ? true : false;
      header.PayloadUnitStart = (data[1] & 0x40) > 0 ? true : false;
      header.TransportPriority = (data[1] & 0x20) > 0 ? true : false;
      header.Pid = ((data[1] & 0x1F) << 8) + data[2];
      header.TransportScrambling = data[3] & 0xC0;
      header.AdaptionFieldControl = (data[3] >> 4) & 0x3;
      header.ContinuityCounter = data[3] & 0x0F;
      header.AdaptionField = data[4];
      header.TableID = data[5];
      header.SectionLen = ((data[6] - 0x70) << 8) + data[7];
      return header;
    }
  }
}

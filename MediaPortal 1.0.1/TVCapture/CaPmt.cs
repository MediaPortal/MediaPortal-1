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

using System.Collections.Generic;

namespace MediaPortal.TV.Recording
{
  public enum ListManagementType : byte
  {
    More = 0,
    First = 1,
    Last = 2,
    Only = 3,
    Add = 4,
    Update = 5
  } ;

  public enum CommandIdType : byte
  {
    Descrambling = 1,
    MMI = 2,
    Query = 3,
    NotSelected = 4
  } ;

  public class CaPmtEs
  {
    public int StreamType; // 8 bit      0
    public int reserved2; // 3 bit      +1 3bit 
    public int ElementaryStreamPID; // 13 bit     +1 5bit, +2=8bit
    public int reserved3; // 4  bit
    public int ElementaryStreamInfoLength; // 12 bit
    public CommandIdType CommandId; // 8 bit
    public List<byte[]> Descriptors;

    public CaPmtEs()
    {
      Descriptors = new List<byte[]>();
    }
  } ;

  public class CaPMT
  {
    public ListManagementType CAPmt_Listmanagement; //  8 bit   0
    public int ProgramNumber; // 16 bit   1..2
    public int reserved0; //  2 bit   3
    public int VersionNumber; //  5 bit   3
    public int CurrentNextIndicator; //  1 bit   3
    public int reserved1; //  4 bit   4
    public int ProgramInfoLength; // 12 bit   4..5
    public CommandIdType CommandId; // 8  bit   6
    public List<byte[]> Descriptors; // x  bit
    public List<CaPmtEs> CaPmtEsList;

    public CaPMT()
    {
      Descriptors = new List<byte[]>();
      CaPmtEsList = new List<CaPmtEs>();
    }

    public byte[] CaPmtStruct(out int length)
    {
      byte[] data = new byte[1024];
      data[0] = (byte) CAPmt_Listmanagement;
      data[1] = (byte) ((ProgramNumber >> 8) & 0xff);
      data[2] = (byte) (ProgramNumber & 0xff);
      data[3] = (byte) ((VersionNumber << 1) + CurrentNextIndicator + 0xc0);
      data[4] = (byte) ((ProgramInfoLength >> 8) & 0xf);
      data[5] = (byte) ((ProgramInfoLength & 0xff));
      int offset = 6;
      if (ProgramInfoLength > 0)
      {
        data[offset++] = (byte) (CommandId);
        for (int i = 0; i < Descriptors.Count; ++i)
        {
          byte[] descriptor = Descriptors[i];
          for (int count = 0; count < descriptor.Length; ++count)
          {
            data[offset++] = descriptor[count];
          }
        }
      }

      for (int esPmt = 0; esPmt < CaPmtEsList.Count; esPmt++)
      {
        CaPmtEs pmtEs = CaPmtEsList[esPmt];
        data[offset++] = (byte) (pmtEs.StreamType);
        data[offset++] = (byte) (((pmtEs.ElementaryStreamPID >> 8) & 0x1f) + 0xe0);
        data[offset++] = (byte) ((pmtEs.ElementaryStreamPID & 0xff));
        data[offset++] = (byte) ((pmtEs.ElementaryStreamInfoLength >> 8) & 0xf);
        data[offset++] = (byte) ((pmtEs.ElementaryStreamInfoLength & 0xff));
        if (pmtEs.ElementaryStreamInfoLength != 0)
        {
          data[offset++] = (byte) ((pmtEs.CommandId));
          for (int i = 0; i < pmtEs.Descriptors.Count; ++i)
          {
            byte[] descriptor = pmtEs.Descriptors[i];
            for (int count = 0; count < descriptor.Length; ++count)
            {
              data[offset++] = descriptor[count];
            }
          }
        }
      }
      length = offset;
      return data;
    }
  }
}
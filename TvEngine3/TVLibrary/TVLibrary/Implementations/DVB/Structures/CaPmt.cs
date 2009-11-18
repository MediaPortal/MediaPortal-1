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

namespace TvLibrary.Implementations.DVB.Structures
{
  /// <summary>
  /// List management type
  /// </summary>
  public enum ListManagementType : byte
  {
    /// <summary>
    /// More
    /// </summary>
    More = 0,
    /// <summary>
    /// First
    /// </summary>
    First = 1,
    /// <summary>
    /// Last
    /// </summary>
    Last = 2,
    /// <summary>
    /// Only
    /// </summary>
    Only = 3,
    /// <summary>
    /// Add
    /// </summary>
    Add = 4,
    /// <summary>
    /// Update
    /// </summary>
    Update = 5
  };
  /// <summary>
  /// Command Id Type enum
  /// </summary>
  public enum CommandIdType : byte
  {
    /// <summary>
    /// Descrambling
    /// </summary>
    Descrambling = 1,
    /// <summary>
    /// MMI
    /// </summary>
    MMI = 2,
    /// <summary>
    /// Query
    /// </summary>
    Query = 3,
    /// <summary>
    /// Not selected
    /// </summary>
    NotSelected = 4
  };

  /// <summary>
  /// ECMEMM
  /// </summary>
  public class ECMEMM
  {
    /// <summary>
    /// ECMEMM Number
    /// </summary>
    public int Number;
    /// <summary>
    /// Pid
    /// </summary>
    public int Pid;
    /// <summary>
    /// CA Id
    /// </summary>
    public int CaId;
    /// <summary>
    /// Provider id
    /// </summary>
    public int ProviderId;
  }
  ///<summary>
  /// CA PMT Es class
  ///</summary>
  public class CaPmtEs
  {
    /// <summary>
    /// CA PMT ES Stream type
    /// </summary>
    public int StreamType;                          // 8 bit      0
    /// <summary>
    /// CA PMT ES reserved
    /// </summary>
    public int reserved2;                           // 3 bit      +1 3bit 
    /// <summary>
    /// CA PMT ES elementary stream PID
    /// </summary>
    public int ElementaryStreamPID;                 // 13 bit     +1 5bit, +2=8bit
    /// <summary>
    /// CA PMT ES reserved3
    /// </summary>
    public int reserved3;                           // 4  bit
    /// <summary>
    /// CA PMT ES elementary stream info length
    /// </summary>
    public int ElementaryStreamInfoLength;          // 12 bit
    /// <summary>
    /// CA PMT ES command id
    /// </summary>
    public CommandIdType CommandId;                  // 8 bit
    /// <summary>
    /// CA PMT ES descriptors
    /// </summary>
    public List<byte[]> Descriptors;
    /// <summary>
    /// Constructor
    /// </summary>
    public CaPmtEs()
    {
      Descriptors = new List<byte[]>();
    }
  };

  ///<summary>
  /// CA PMT class
  ///</summary>
  public class CaPMT
  {
    /// <summary>
    /// CA PMT listmanagement
    /// </summary>
    public ListManagementType CAPmt_Listmanagement; //  8 bit   0
    /// <summary>
    /// CA PMT program number
    /// </summary>
    public int ProgramNumber;                       // 16 bit   1..2
    /// <summary>
    /// CA PMT reserved 0
    /// </summary>
    public int reserved0;                           //  2 bit   3
    /// <summary>
    /// CA PMT version number
    /// </summary>
    public int VersionNumber;                       //  5 bit   3
    /// <summary>
    /// CA PMT current next indicator
    /// </summary>
    public int CurrentNextIndicator;                //  1 bit   3
    /// <summary>
    /// CA PMT reserved 1
    /// </summary>
    public int reserved1;                           //  4 bit   4
    /// <summary>
    /// CA PMT  program info length
    /// </summary>
    public int ProgramInfoLength;                   // 12 bit   4..5
    /// <summary>
    /// CA PMT  command id
    /// </summary>
    public CommandIdType CommandId;       // 8  bit   6
    /// <summary>
    /// CA PMT descriptors
    /// </summary>
    public List<byte[]> Descriptors;             // x  bit
    /// <summary>
    /// CA PMT  es list
    /// </summary>
    public List<CaPmtEs> CaPmtEsList;
    /// <summary>
    /// CA PMT descriptors cat
    /// </summary>
    public List<byte[]> DescriptorsCat;             // x  bit

    ///<summary>
    /// Constructor
    ///</summary>
    public CaPMT()
    {
      Descriptors = new List<byte[]>();
      DescriptorsCat = new List<byte[]>();
      CaPmtEsList = new List<CaPmtEs>();
    }

    /// <summary>
    /// Returns the EMM's found in the CAT.
    /// </summary>
    /// <returns></returns>
    public List<ECMEMM> GetEMM()
    {
      List<ECMEMM> emms = new List<ECMEMM>();
      if (DescriptorsCat == null)
        return emms;
      for (int i = 0; i < DescriptorsCat.Count; ++i)
      {
        byte[] descriptor = DescriptorsCat[i];
        string tmp = "";
        for (int x = 0; x < descriptor.Length; ++x)
          tmp += String.Format("{0:X} ", descriptor[x]);
        Log.Log.Info("emm len:{0:X} {1}", descriptor.Length, tmp);
        Parse(descriptor, emms);
      }
      return emms;
    }

    static void Add(IList<ECMEMM> list, ECMEMM newEcm)
    {
      for (int i = 0; i < list.Count; ++i)
      {
        if (list[i].ProviderId == newEcm.ProviderId &&
            list[i].Pid == newEcm.Pid &&
            list[i].CaId == newEcm.CaId)
          return;
      }
      list.Add(newEcm);
    }
    /// <summary>
    /// Returns the ECM's found in the PMT.
    /// </summary>
    /// <returns></returns>
    static void Parse(byte[] descriptor, IList<ECMEMM> newEcms)
    {
      ECMEMM ecm = new ECMEMM();
      int off = 0;
      while (off < descriptor.Length)
      {
        byte tag = descriptor[off];
        byte len = descriptor[off + 1];
        if (tag == 0x9)
        {
          int offset;
          if (ecm.Pid != 0)
            Add(newEcms, ecm);
          ecm = new ECMEMM();
          int caId = ecm.CaId = ((descriptor[off + 2]) << 8) + descriptor[off + 3];
          ecm.Pid = ((descriptor[off + 4] & 0x1f) << 8) + descriptor[off + 5];
          if (ecm.CaId == 0x100 && len >= 17)
          {
            if (descriptor[off + 8] == 0xff)
            {
              //0  1 2 3  4  5 6  7  8 9 10
              //9 11 1 0 E6 43 0 6A FF 0  0 0 0 0 0 2 14 21 8C 
              //
              //
              //0  1  2 3  4  5  6  7  8 9 10
              //9 11  1 0 E6  1C 41 1 FF FF FF FF FF FF FF FF FF 21 8C 
              int count = (len - 2) / 15;
              for (int i = 0; i < count; ++i)
              {
                offset = off + i * 15;
                if (offset >= descriptor.Length)
                  break;
                ecm = new ECMEMM();
                ecm.CaId = caId;
                ecm.Pid = ((descriptor[offset + 4] & 0x1f) << 8) + descriptor[offset + 5];
                ecm.ProviderId = ((descriptor[offset + 6]) << 8) + descriptor[offset + 7];
                Add(newEcms, ecm);
                ecm = new ECMEMM();
              }
            }
          }

          if (ecm.CaId == 0x100 && len >= 8)
          {
            if (descriptor[off + 7] == 0xe0 && descriptor[8] != 0xff)
            {
              //0  1 2 3  4  5 6  7 8  9  10
              //9 11 1 0 E0 C1 3 E0 92 41  1 E0 93 40 1 E0 C4 0 64 
              //9  D 1 0 E0 B6 2 E0 B7  0 6A E0 B9 0  6C 
              if (descriptor[off + 6] > 0 || descriptor[off + 6] <= 9)
              {
                ecm.ProviderId = ((descriptor[off + 9]) << 8) + descriptor[off + 10];
              }
            }
          }
          offset = off + 6;
          if (offset + 2 < descriptor.Length)
          {
            while (true)
            {
              byte tagInd = descriptor[offset];
              byte tagLen = descriptor[offset + 1];
              if (tagLen + off < descriptor.Length)
              {
                if (tagInd == 0x14)
                {
                  ecm.ProviderId = (descriptor[offset + 2] << 16) + (descriptor[offset + 3] << 8) + descriptor[offset + 4];
                  // Some providers sends wrong information in provider id (Boxer),
                  // so reset lower 4 bits for Via Access
                  if (ecm.CaId == 0x500)
                  {
                    ecm.ProviderId = ecm.ProviderId & 0xFFFFF0;
                  }
                }
              }
              offset += (tagLen + 2);
              if (offset >= descriptor.Length)
                break;
            }
          }
        }
        off += (len + 2);
      }
      if (ecm.Pid > 0)
        Add(newEcms, ecm);
    }

    ///<summary>
    /// Get ECM
    ///</summary>
    ///<returns>ECM</returns>
    public List<ECMEMM> GetECM()
    {
      List<ECMEMM> ecms = new List<ECMEMM>();
      if (Descriptors != null)
      {
        for (int i = 0; i < Descriptors.Count; ++i)
        {
          byte[] descriptor = Descriptors[i];
          string tmp = "";
          for (int x = 0; x < descriptor.Length; ++x)
            tmp += String.Format("{0:X} ", descriptor[x]);
          Log.Log.Info("ecm len:{0:X} {1}", descriptor.Length, tmp);
          Parse(descriptor, ecms);
        }
      }

      if (CaPmtEsList != null)
      {
        foreach (CaPmtEs pmtEs in CaPmtEsList)
        {
          if (pmtEs.Descriptors == null)
            continue;
          for (int i = 0; i < pmtEs.Descriptors.Count; ++i)
          {
            byte[] descriptor = pmtEs.Descriptors[i];
            string tmp = "";
            for (int x = 0; x < descriptor.Length; ++x)
              tmp += String.Format("{0:X} ", descriptor[x]);
            Log.Log.Info("ecm len:{0:X} {1}", descriptor.Length, tmp);
            Parse(descriptor, ecms);
          }
        }
      }
      return ecms;
    }


    /// <summary>
    /// Cas the PMT struct.
    /// </summary>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public byte[] CaPmtStruct(out int length)
    {
      byte[] data = new byte[1024];
      data[0] = (byte)CAPmt_Listmanagement;
      data[1] = (byte)((ProgramNumber >> 8) & 0xff);
      data[2] = (byte)(ProgramNumber & 0xff);
      data[3] = (byte)((VersionNumber << 1) + CurrentNextIndicator + 0xc0);
      data[4] = (byte)((ProgramInfoLength >> 8) & 0xf);
      data[5] = (byte)((ProgramInfoLength & 0xff));
      int offset = 6;
      if (ProgramInfoLength > 0)
      {
        data[offset++] = (byte)(CommandId);
        for (int i = 0; i < Descriptors.Count; ++i)
        {
          byte[] descriptor = Descriptors[i];
          for (int count = 0; count < descriptor.Length; ++count)
            data[offset++] = descriptor[count];
        }
      }

      for (int esPmt = 0; esPmt < CaPmtEsList.Count; esPmt++)
      {
        CaPmtEs pmtEs = CaPmtEsList[esPmt];
        data[offset++] = (byte)(pmtEs.StreamType);
        data[offset++] = (byte)(((pmtEs.ElementaryStreamPID >> 8) & 0x1f) + 0xe0);
        data[offset++] = (byte)((pmtEs.ElementaryStreamPID & 0xff));
        data[offset++] = (byte)((pmtEs.ElementaryStreamInfoLength >> 8) & 0xf);
        data[offset++] = (byte)((pmtEs.ElementaryStreamInfoLength & 0xff));
        if (pmtEs.ElementaryStreamInfoLength != 0)
        {
          data[offset++] = (byte)((pmtEs.CommandId));
          for (int i = 0; i < pmtEs.Descriptors.Count; ++i)
          {
            byte[] descriptor = pmtEs.Descriptors[i];
            for (int count = 0; count < descriptor.Length; ++count)
              data[offset++] = descriptor[count];
          }
        }
      }
      length = offset;
      return data;
    }
    /// <summary>
    /// Dumps ca pmt to the log file.
    /// </summary>
    public void Dump()
    {
      Log.Log.Write("Ca pmt:");
      Log.Log.Write("  program number            :{0:X}", ProgramNumber);
      Log.Log.Write("  VersionNumber             :{0}", VersionNumber);
      Log.Log.Write("  CurrentNextIndicator      :{0}", CurrentNextIndicator);
      Log.Log.Write("  ProgramInfoLength         :{0:X} {1}", ProgramInfoLength, Descriptors.Count);
      Log.Log.Write("  CAPmt_CommandID_PRG       :{0}", CommandId);
      foreach (CaPmtEs pmtes in CaPmtEsList)
      {
        Log.Log.Write("  StreamType                :{0}", pmtes.StreamType);
        Log.Log.Write("  ElementaryStreamPID       :{0:X}", pmtes.ElementaryStreamPID);
        Log.Log.Write("  ElementaryStreamInfoLength:{0:X} {1}", pmtes.ElementaryStreamInfoLength, pmtes.Descriptors.Count);
        Log.Log.Write("  CAPmt_CommandID_ES        :{0}", pmtes.CommandId);
      }
    }
  }

}

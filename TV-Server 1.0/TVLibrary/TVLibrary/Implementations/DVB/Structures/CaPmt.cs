using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Implementations.DVB.Structures
{
  public enum ListManagementType : byte
  {
    More = 0,
    First = 1,
    Last = 2,
    Only = 3,
    Add = 4,
    Update = 5
  };
  public enum CommandIdType : byte
  {
    Descrambling = 1,
    MMI = 2,
    Query = 3,
    NotSelected = 4
  };

  public class ECMEMM
  {
    public int Number;
    public int Pid;
    public int CaId;
    public int ProviderId;
  }
  public class CaPmtEs
  {
    public int StreamType;                          // 8 bit      0
    public int reserved2;                           // 3 bit      +1 3bit 
    public int ElementaryStreamPID;                 // 13 bit     +1 5bit, +2=8bit
    public int reserved3;                           // 4  bit
    public int ElementaryStreamInfoLength;          // 12 bit
    public CommandIdType CommandId;                  // 8 bit
    public List<byte[]> Descriptors;
    public CaPmtEs()
    {
      Descriptors = new List<byte[]>();
    }
  };

  public class CaPMT
  {
    public ListManagementType CAPmt_Listmanagement; //  8 bit   0
    public int ProgramNumber;                       // 16 bit   1..2
    public int reserved0;                           //  2 bit   3
    public int VersionNumber;                       //  5 bit   3
    public int CurrentNextIndicator;                //  1 bit   3
    public int reserved1;                           //  4 bit   4
    public int ProgramInfoLength;                   // 12 bit   4..5
    public CommandIdType CommandId;       // 8  bit   6
    public List<byte[]> Descriptors;             // x  bit
    public List<CaPmtEs> CaPmtEsList;
    public List<byte[]> DescriptorsCat;             // x  bit

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
      if (DescriptorsCat == null) return emms;
      for (int i = 0; i < DescriptorsCat.Count; ++i)
      {
        byte[] descriptor = DescriptorsCat[i];
        string tmp = "";
        for (int x = 0; x < descriptor.Length; ++x)
          tmp += String.Format("{0:X} ", descriptor[x]);
        Log.Log.Info("emm len:{0:X} {1}", descriptor.Length, tmp);
        Parse(descriptor, ref emms);
      }
      return emms;
    }

    void Add(ref List<ECMEMM> list, ECMEMM newEcm)
    {
      for (int i = 0; i < list.Count; ++i)
      {
        if (list[i].ProviderId == newEcm.ProviderId &&
            list[i].Pid == newEcm.Pid &&
            list[i].CaId == newEcm.CaId) return;
      }
      list.Add(newEcm);
    }
    /// <summary>
    /// Returns the ECM's found in the PMT.
    /// </summary>
    /// <returns></returns>
    void Parse(byte[] descriptor, ref List<ECMEMM> newEcms)
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
            Add(ref newEcms,ecm);
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
                if (offset >= descriptor.Length) break;
                ecm = new ECMEMM();
                ecm.CaId = caId;
                ecm.Pid = ((descriptor[offset + 4] & 0x1f) << 8) + descriptor[offset + 5];
                ecm.ProviderId = ((descriptor[offset + 6]) << 8) + descriptor[offset + 7];
                Add(ref newEcms, ecm);
                ecm = new ECMEMM();
              }
            }
          }

          if (ecm.CaId == 0x100 && len >=8)
          {
            if (descriptor[off + 7] == 0xe0 && descriptor[8]!=0xff)
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
                }
              }
              offset += (tagLen + 2);
              if (offset >= descriptor.Length) break;
            }
          }
        }
        off += (len + 2);
      }
      if (ecm.Pid > 0) Add(ref newEcms, ecm);
    }

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
          Parse(descriptor, ref ecms);
        }
      }

      if (CaPmtEsList != null)
      {
        foreach (CaPmtEs pmtEs in CaPmtEsList)
        {
          if (pmtEs.Descriptors == null) continue;
          for (int i = 0; i < pmtEs.Descriptors.Count; ++i)
          {
            byte[] descriptor = pmtEs.Descriptors[i];
            string tmp = "";
            for (int x = 0; x < descriptor.Length; ++x)
              tmp += String.Format("{0:X} ", descriptor[x]);
            Log.Log.Info("ecm len:{0:X} {1}", descriptor.Length, tmp);
            Parse(descriptor, ref ecms);
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

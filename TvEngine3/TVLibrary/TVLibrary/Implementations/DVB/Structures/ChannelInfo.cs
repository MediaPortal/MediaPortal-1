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
using System.Collections;
using System.Runtime.InteropServices;

namespace TvLibrary.Implementations.DVB.Structures
{
  /// <summary>
  /// class holding all information about a channel including pids
  /// </summary>
  public class ChannelInfo
  {
    /// <summary>
    /// program number (service id)
    /// </summary>
    public int program_number;
    /// <summary>
    /// reserved
    /// </summary>
    public int reserved;
    /// <summary>
    /// pid of the PMT
    /// </summary>
    public int network_pmt_PID;
    /// <summary>
    /// transport stream id
    /// </summary>
    public int transportStreamID;
    /// <summary>
    /// name of the provider
    /// </summary>
    public string service_provider_name;
    /// <summary>
    /// name of the service
    /// </summary>
    public string service_name;
    /// <summary>
    /// service type
    /// </summary>
    public int serviceType;
    /// <summary>
    /// eit schedule flag
    /// </summary>
    public bool eitSchedule;
    /// <summary>
    /// eit prefollow flag
    /// </summary>
    public bool eitPreFollow;
    /// <summary>
    /// indicates if channel is scrambled
    /// </summary>
    public bool scrambled;
    /// <summary>
    /// carrier frequency
    /// </summary>
    public int freq;// 12188
    /// <summary>
    /// symbol rate
    /// </summary>
    public int symb;// 27500
    /// <summary>
    /// fec
    /// </summary>
    public int fec;// 6
    /// <summary>
    /// diseqc type
    /// </summary>
    public int diseqc;// 1
    /// <summary>
    /// LNB low oscilator frequency
    /// </summary>
    public int lnb01;// 10600
    /// <summary>
    /// LNB frequency
    /// </summary>
    public int lnbkhz;// 1 = 22
    /// <summary>
    /// Polarisation
    /// </summary>
    public int pol; // 0 - h
    /// <summary>
    /// pid of the PCR
    /// </summary>
    public int pcr_pid;
    /// <summary>
    /// ArrayList of PidInfo containing all pids
    /// </summary>
    public ArrayList pids;
    /// <summary>
    /// Service Id
    /// </summary>
    public int serviceID;
    /// <summary>
    /// Network Id
    /// </summary>
    public int networkID;
    /// <summary>
    /// pidcache?
    /// </summary>
    public string pidCache;
    /// <summary>
    /// Atsc minor channel number
    /// </summary>
    public int minorChannel;
    /// <summary>
    /// atsc major channel number
    /// </summary>
    public int majorChannel;
    /// <summary>
    /// Modulation
    /// </summary>
    public int modulation;
    /// <summary>
    /// CaPmt
    /// </summary>
    public CaPMT caPMT;
    /// <summary>
    /// Logical channel number
    /// </summary>
    public int LCN;
    /// <summary>
    /// Video PID
    /// </summary>
    public int videoPid;
    /// <summary>
    /// Audio PID
    /// </summary>
    public int audioPid;


    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelInfo"/> class.
    /// </summary>
    public ChannelInfo()
    {
      pids = new ArrayList();
    }
    /// <summary>
    /// Adds a pid to the pidtable
    /// </summary>
    /// <param name="info">The info.</param>
    public void AddPid(PidInfo info)
    {
      pids.Add(info);
    }
    /// <summary>
    /// Decodes the pmt specified in data.
    /// </summary>
    /// <param name="data">The data.</param>
    public void Decode(IntPtr data)
    {
      byte[] da = new byte[600];
      Marshal.Copy(data, da, 0, 580);
      program_number = -1;
      network_pmt_PID = -1;
      transportStreamID = -1;
      service_provider_name = String.Empty;
      service_name = String.Empty;
      serviceType = -1;
      eitSchedule = false;
      eitPreFollow = false;
      scrambled = false;
      freq = -1;
      symb = -1;
      fec = -1;
      diseqc = -1;
      lnb01 = -1;
      lnbkhz = -1;
      pol = -1;
      pcr_pid = -1;
      pids = new ArrayList();
      serviceID = -1;
      networkID = -1;
      pidCache = String.Empty;
      minorChannel = -1;
      majorChannel = -1;
      modulation = -1;
      majorChannel = -1;
      minorChannel = -1;


      transportStreamID = Marshal.ReadInt32(data, 0);
      program_number = Marshal.ReadInt32(data, 4);
      network_pmt_PID = Marshal.ReadInt32(data, 8);
      pcr_pid = Marshal.ReadInt32(data, 12);
      serviceID = program_number;
      pids = new ArrayList();
      PidInfo pmt = new PidInfo();
      // video
      pmt.pid = Marshal.ReadInt16(data, 16);
      pmt.isVideo = true;
      pmt.stream_type = 1;
      pmt.language = "";
      RemoveInvalidChars(ref pmt.language);
      pids.Add(pmt);
      pmt = new PidInfo();

      // audio 1
      pmt.pid = Marshal.ReadInt16(data, 18);
      pmt.isAudio = true;
      pmt.stream_type = 3;
      pmt.language = "" + (char)Marshal.ReadByte(data, 20) + (char)Marshal.ReadByte(data, 21) + (char)Marshal.ReadByte(data, 22);
      RemoveInvalidChars(ref pmt.language);
      pids.Add(pmt);
      pmt = new PidInfo();

      // audio 2
      pmt.pid = Marshal.ReadInt16(data, 24);
      pmt.isAudio = true;
      pmt.stream_type = 3;
      pmt.language = "" + (char)Marshal.ReadByte(data, 26) + (char)Marshal.ReadByte(data, 27) + (char)Marshal.ReadByte(data, 28);
      RemoveInvalidChars(ref pmt.language);
      pids.Add(pmt);
      pmt = new PidInfo();

      // audio 3
      pmt.pid = Marshal.ReadInt16(data, 30);
      pmt.isAudio = true;
      pmt.stream_type = 3;
      pmt.language = "" + (char)Marshal.ReadByte(data, 32) + (char)Marshal.ReadByte(data, 33) + (char)Marshal.ReadByte(data, 34);
      RemoveInvalidChars(ref pmt.language);
      pids.Add(pmt);
      pmt = new PidInfo();

      // ac3
      pmt.pid = Marshal.ReadInt16(data, 36);
      pmt.isAC3Audio = true;
      pmt.stream_type = 0x81;
      pmt.language = "";
      RemoveInvalidChars(ref pmt.language);
      pids.Add(pmt);
      pmt = new PidInfo();

      // teletext
      pmt.pid = Marshal.ReadInt16(data, 38);
      pmt.isTeletext = true;
      pmt.stream_type = 0;
      pmt.language = "";
      RemoveInvalidChars(ref pmt.language);
      pids.Add(pmt);
      pmt = new PidInfo();

      // sub
      pmt.pid = Marshal.ReadInt16(data, 40);
      pmt.isDVBSubtitle = true;
      pmt.stream_type = 0;
      pmt.language = "";
      RemoveInvalidChars(ref pmt.language);
      pids.Add(pmt);

      //Marshal.Copy((IntPtr)(((int)data)+42),d,0,255);
      service_name = Marshal.PtrToStringAnsi((IntPtr)(((int)data) + 42));
      //Marshal.Copy((IntPtr)(((int)data)+297),d,0,255);
      service_provider_name = Marshal.PtrToStringAnsi((IntPtr)(((int)data) + 297));
      eitPreFollow = (Marshal.ReadInt16(data, 552)) == 1 ? true : false;
      eitSchedule = (Marshal.ReadInt16(data, 554)) == 1 ? true : false;
      scrambled = (Marshal.ReadInt16(data, 556)) == 1 ? true : false;
      serviceType = Marshal.ReadInt16(data, 558);
      networkID = Marshal.ReadInt32(data, 560);

      majorChannel = Marshal.ReadInt16(data, 568);
      minorChannel = Marshal.ReadInt16(data, 570);
      modulation = Marshal.ReadInt16(data, 572);
      freq = Marshal.ReadInt32(data, 576);
      LCN = Marshal.ReadInt32(data, 580);
      RemoveInvalidChars(ref service_name);
      RemoveInvalidChars(ref service_provider_name);

    }

    static void RemoveInvalidChars(ref string strTxt)
    {
      if (strTxt == null)
      {
        strTxt = String.Empty;
        return;
      }
      if (strTxt.Length == 0)
      {
        strTxt = String.Empty;
        return;
      }
      string strReturn = String.Empty;
      for (int i = 0; i < strTxt.Length; ++i)
      {
        char k = strTxt[i];
        if (k == '\'')
        {
          strReturn += "'";
        }
        if ((byte)k == 0)// remove 0-bytes from the string
          k = (char)32;

        strReturn += k;
      }
      strReturn = strReturn.Trim();
      strTxt = strReturn;
    }
    /// <summary>
    /// Decodes the PMT supplied in buf and fills the pid table with all pids found
    /// </summary>
    /// <param name="buf">The buf.</param>
    public void DecodePmt(byte[] buf)
    {
      if (buf.Length < 13)
      {
        //Log.Log.WriteFile("decodePMTTable() len < 13 len={0}", buf.Length);
        return;
      }
      int section_length = ((buf[1] & 0xF) << 8) + buf[2];
      int programNumber = (buf[3] << 8) + buf[4];
      int version_number = ((buf[5] >> 1) & 0x1F);
      int current_next_indicator = buf[5] & 1;
      pcr_pid = ((buf[8] & 0x1F) << 8) + buf[9];   // ! really set pcr_pid ! ( ambass )
      int program_info_length = ((buf[10] & 0xF) << 8) + buf[11];


      caPMT = new CaPMT();
      caPMT.ProgramNumber = programNumber;
      caPMT.CurrentNextIndicator = current_next_indicator;
      caPMT.VersionNumber = version_number;
      caPMT.CAPmt_Listmanagement = ListManagementType.Only;

      //if (pat.program_number != program_number)
      //{

      //Log.Write("decodePMTTable() pat program#!=program numer {0}!={1}", pat.program_number, program_number);
      //return 0;
      //}
      //pat.pid_list = new ArrayList();

      //string pidText = "";

      int pointer = 12;
      int x;
      int len1 = section_length - pointer;
      int len2 = program_info_length;
      Log.Log.Write("Decode pmt");
      while (len2 > 0)
      {
        if (pointer + 2 > buf.Length)
          break;
        int indicator = buf[pointer];
        x = buf[pointer + 1] + 2;
        byte[] data = new byte[x];

        if (pointer + x > buf.Length)
          break;
        Array.Copy(buf, pointer, data, 0, x);

        if (indicator == 0x9) //MPEG CA Descriptor
        {
          //Log.Log.Write("  descriptor1:{0:X} len:{1} {2:X} {3:X}", indicator,data.Length,buf[pointer],buf[pointer+1]);
          caPMT.Descriptors.Add(data);
          caPMT.ProgramInfoLength += data.Length;
          //string tmpString = DVB_CADescriptor(data);
          //if (pidText.IndexOf(tmpString, 0) == -1)
          // pidText += tmpString + ";";

          string tmp = "";
          for (int teller = 0; teller < x; ++teller)
            tmp += String.Format("{0:x} ", buf[pointer + teller]);
          Log.Log.Info("descr1 len:{0:X} {1}", x, tmp);
        }
        len2 -= x;
        pointer += x;
        len1 -= x;
      }
      if (caPMT.ProgramInfoLength > 0)
      {
        caPMT.CommandId = CommandIdType.Descrambling;
        caPMT.ProgramInfoLength += 1;
      }
      //byte[] b = new byte[6];
      PidInfo pidInfo;
      while (len1 > 4)
      {
        if (pointer + 5 > section_length)
          break;
        pidInfo = new PidInfo();
        //System.Array.Copy(buf, pointer, b, 0, 5);
        try
        {
          pidInfo.stream_type = buf[pointer];
          pidInfo.reserved_1 = (buf[pointer + 1] >> 5) & 7;
          pidInfo.pid = ((buf[pointer + 1] & 0x1F) << 8) + buf[pointer + 2];
          pidInfo.reserved_2 = (buf[pointer + 3] >> 4) & 0xF;
          pidInfo.ES_info_length = ((buf[pointer + 3] & 0xF) << 8) + buf[pointer + 4];
        } catch (Exception ex)
        {
          Log.Log.WriteFile("Error while decoding pmt: ", ex);
        }

        switch (pidInfo.stream_type)
        {
          case 0x1b://H.264
            pidInfo.isVideo = true;
            break;
          case 0x10://MPEG4 ISO/IEC 14496-2
            pidInfo.isVideo = true;
            break;
          case 0x1://MPEG-1 VIDEO ISO/IEC 11172 
            pidInfo.isVideo = true;
            break;
          case 0x2://MPEG-2 VIDEO ITU-T Rec. H.262 | ISO/IEC 13818-2 Video or ISO/IEC 11172-2 constrained parameter video stream
            pidInfo.isVideo = true;
            break;
          case 0x3://MPEG-1 AUDIO ISO/IEC 11172 
            pidInfo.isAudio = true;
            pidInfo.isAC3Audio = false;
            break;
          case 0x4://MPEG-3 AUDIO ISO/IEC 13818-3 
            pidInfo.isAudio = true;
            pidInfo.isAC3Audio = false;
            break;
          case 0x81://AC3 AUDIO
            pidInfo.isAudio = false;
            pidInfo.isAC3Audio = true;
            break;
          case 0x0f://AAC AUDIO
            pidInfo.isAudio = true;
            pidInfo.isAC3Audio = false;
            break;
          case 0x11://LATM AAC AUDIO
            pidInfo.isAudio = true;
            pidInfo.isAC3Audio = false;
            break;
        }
        pointer += 5;
        len1 -= 5;
        len2 = pidInfo.ES_info_length;

        CaPmtEs pmtEs = new CaPmtEs();
        pmtEs.StreamType = pidInfo.stream_type;
        pmtEs.ElementaryStreamPID = pidInfo.pid;
        pmtEs.CommandId = CommandIdType.Descrambling;

        if (len1 > 0)
        {
          while (len2 > 0)
          {
            if (pointer + 1 < buf.Length)
            {
              int indicator = buf[pointer];
              x = buf[pointer + 1] + 2;

              //Log.Log.Write("  descriptor2:{0:X}", indicator);
              if (x + pointer < buf.Length) // parse descriptor data
              {
                byte[] data = new byte[x];
                Array.Copy(buf, pointer, data, 0, x);
                if (indicator == 9)
                {
                  string tmp = "";
                  for (int teller = 0; teller < x; ++teller)
                    tmp += String.Format("{0:x} ", buf[pointer + teller]);
                  Log.Log.Info("descr2 pid:{0:X} len:{1:X} {2}", pmtEs.ElementaryStreamPID, x, tmp);
                }
                switch (indicator)
                {
                  case 0x02: // video
                    pidInfo.isAudio = false;
                    pidInfo.isVideo = true;
                    pidInfo.isTeletext = false;
                    pidInfo.isDVBSubtitle = false;
                    pidInfo.isAC3Audio = false;
                    break;
                  case 0x03: // audio
                    //Log.Write("dvbsections: indicator {1} {0} found",(indicator==0x02?"for video":"for audio"),indicator);
                    pidInfo.isAudio = true;
                    pidInfo.isVideo = false;
                    pidInfo.isTeletext = false;
                    pidInfo.isDVBSubtitle = false;
                    pidInfo.isAC3Audio = false;
                    pidInfo.stream_type = 0x03;
                    break;
                  case 0x09:
                    pmtEs.Descriptors.Add(data);
                    pmtEs.ElementaryStreamInfoLength += data.Length;
                    break;
                  case 0x0A: //MPEG_ISO639_Lang																														
                    pidInfo.language = DVB_GetMPEGISO639Lang(data);
                    pidInfo.AddDescriptorData(data); // remember the original descriptor																				
                    break;
                  case 0x52: //stream identifier
                    pidInfo.AddDescriptorData(data);
                    break;
                  case 0x6A: //AC3									
                    pidInfo.isAudio = false;
                    pidInfo.isVideo = false;
                    pidInfo.isTeletext = false;
                    pidInfo.isDVBSubtitle = false;
                    pidInfo.isAC3Audio = true;
                    pidInfo.stream_type = 0x81;
                    break;
                  case 0x56://teletext
                    pidInfo.isAC3Audio = false;
                    pidInfo.isAudio = false;
                    pidInfo.isVideo = false;
                    pidInfo.isTeletext = true;
                    pidInfo.isDVBSubtitle = false;
                    pidInfo.stream_type = 0x6; // Ziphnor
                    pidInfo.AddDescriptorData(data); // remember the original descriptor
                    pidInfo.teletextLANG = DVB_GetTeletextDescriptor(data);
                    break;
                  //case 0xc2:
                  case 0x59: // DVB ssubtitle
                    if (pidInfo.stream_type == 0x05 || pidInfo.stream_type == 0x06)
                    {
                      pidInfo.isAC3Audio = false;
                      pidInfo.isAudio = false;
                      pidInfo.isVideo = false;
                      pidInfo.isTeletext = false;
                      pidInfo.isDVBSubtitle = true;
                      pidInfo.stream_type = 0x6;
                      pidInfo.AddDescriptorData(data);
                      pidInfo.language = DVB_SubtitleDescriptior(data);
                    }
                    break;
                  default:
                    pidInfo.language = "";
                    break;
                }
              }
            }
            else
            {
              break;
            }
            len2 -= x;
            len1 -= x;
            pointer += x;
          }
        }
        if (pidInfo.isVideo || pidInfo.isAC3Audio || pidInfo.isAudio)
        {
          if (pmtEs.ElementaryStreamInfoLength > 0)
          {
            pmtEs.CommandId = CommandIdType.Descrambling;
            pmtEs.ElementaryStreamInfoLength += 1;
          }
          caPMT.CaPmtEsList.Add(pmtEs);
        }
        pids.Add(pidInfo);
      }
      //pat.pidCache = pidText;
      //caPMT.Dump();
    }

    private static string DVB_GetMPEGISO639Lang(byte[] b)
    {
      string ISO_639_language_code = "";

      int descriptor_tag = b[0];
      int descriptor_length = b[1];
      if (descriptor_length < b.Length)
        if (descriptor_tag == 0xa)
        {
          int len = descriptor_length;
          byte[] bytes = new byte[len + 1];

          int pointer = 2;

          while (len > 0)
          {
            Array.Copy(b, pointer, bytes, 0, len);
            ISO_639_language_code += System.Text.Encoding.ASCII.GetString(bytes, 0, 3);
            pointer += 4;
            len -= 4;
          }
        }

      return ISO_639_language_code;
    }

    static string DVB_SubtitleDescriptior(byte[] buf)
    {
      string ISO_639_language_code = "";

      int descriptor_tag = buf[0];
      int descriptor_length = buf[1];
      if (descriptor_length < buf.Length)
        if (descriptor_tag == 0x59)
        {
          int len = descriptor_length;
          byte[] bytes = new byte[len + 1];

          int pointer = 2;

          while (len > 0)
          {
            Array.Copy(buf, pointer, bytes, 0, len);
            ISO_639_language_code += System.Text.Encoding.ASCII.GetString(bytes, 0, 3);

            pointer += 8;
            len -= 8;
          }
        }

      return ISO_639_language_code;
    }

    private static string DVB_GetTeletextDescriptor(byte[] b)
    {
      string ISO_639_language_code = "";
      if (b.Length < 2)
        return String.Empty;
      int descriptor_tag = b[0];
      int descriptor_length = b[1];

      int len = descriptor_length;
      byte[] bytes = new byte[len + 1];
      if (len < b.Length + 2)
        if (descriptor_tag == 0x56)
        {
          int pointer = 2;

          while (len > 0 && (pointer + 3 <= b.Length))
          {
            Array.Copy(b, pointer, bytes, 0, 3);
            ISO_639_language_code += System.Text.Encoding.ASCII.GetString(bytes, 0, 3);
            pointer += 5;
            len -= 5;
          }
        }
      if (ISO_639_language_code.Length >= 3)
        return ISO_639_language_code.Substring(0, 3);
      return "";
    }

    /// <summary>
    /// Decodes the conditional access table
    /// </summary>
    /// <param name="cat">The conditional access table.</param>
    /// <param name="catLen">The length of the conditional access table.</param>
    public void DecodeCat(byte[] cat, int catLen)
    {
      if (catLen < 7)
        return;
      int pos = 8;
      while (pos + 2 < catLen)
      {
        byte descriptorTag = cat[pos];
        byte descriptorLen = cat[pos + 1];
        //Log.Log.Info("tag:0x{0:X} len:{1:X}", descriptorTag, descriptorLen);
        if (descriptorTag == 0x9)
        {
          byte[] data = new byte[2 + descriptorLen];
          for (int i = 0; i < 2 + descriptorLen; ++i)
            data[i] = cat[pos + i];
          caPMT.DescriptorsCat.Add(data);
        }
        pos += (descriptorLen + 2);
      }
    }

  }
}

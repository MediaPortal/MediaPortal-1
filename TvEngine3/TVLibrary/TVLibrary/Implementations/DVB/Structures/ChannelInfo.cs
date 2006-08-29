using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Implementations.DVB.Structures
{
  public class ChannelInfo
  {
    public int program_number;
    public int reserved;
    public int network_pmt_PID;
    public int transportStreamID;
    public string service_provider_name;
    public string service_name;
    public int serviceType;
    public bool eitSchedule;
    public bool eitPreFollow;
    public bool scrambled;
    public int freq;// 12188
    public int symb;// 27500
    public int fec;// 6
    public int diseqc;// 1
    public int lnb01;// 10600
    public int lnbkhz;// 1 = 22
    public int pol; // 0 - h
    public int pcr_pid;
    public ArrayList pids;
    public int serviceID;
    public int networkID;
    public string pidCache;
    public int minorChannel;
    public int majorChannel;
    public int modulation;
    public CaPMT caPMT;
    public int LCN;

    public ChannelInfo()
    {
      pids = new ArrayList();
    }
    public void AddPid(PidInfo info)
    {
      pids.Add(info);
    }
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
      pmt.stream_type = 0;
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
      pmt = new PidInfo();

      byte[] d = new byte[255];
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
    void RemoveInvalidChars(ref string strTxt)
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
      for (int i = 0; i < (int)strTxt.Length; ++i)
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
    public void DecodePmt(byte[] buf)
    {
      if (buf.Length < 13)
      {
        //Log.Log.WriteFile("decodePMTTable() len < 13 len={0}", buf.Length);
        return;
      }
      int table_id = buf[0];
      int section_syntax_indicator = (buf[1] >> 7) & 1;
      int section_length = ((buf[1] & 0xF) << 8) + buf[2];
      int program_number = (buf[3] << 8) + buf[4];
      int version_number = ((buf[5] >> 1) & 0x1F);
      int current_next_indicator = buf[5] & 1;
      int section_number = buf[6];
      int last_section_number = buf[7];
      int pcr_pid = ((buf[8] & 0x1F) << 8) + buf[9];
      int program_info_length = ((buf[10] & 0xF) << 8) + buf[11];


      //pat.caPMT = new CaPMT();
      //pat.caPMT.CADescriptors_ES = new ArrayList();
      //pat.caPMT.CADescriptors_PRG = new ArrayList();

      //pat.caPMT.ProgramNumber = program_number;
      //pat.caPMT.CurrentNextIndicator = current_next_indicator;
      //pat.caPMT.VersionNumber = version_number;
      //pat.caPMT.CAPmt_Listmanagement = 0x9f8032;

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

      while (len2 > 0)
      {
        if (pointer + 2 > buf.Length) break;
        int indicator = buf[pointer];
        x = 0;
        x = buf[pointer + 1] + 2;
        byte[] data = new byte[x];

        if (pointer + x > buf.Length) break;
        System.Array.Copy(buf, pointer, data, 0, x);
        if (indicator == 0x9)
        {
          //pat.caPMT.CADescriptors_PRG.Add(data);
          //pat.caPMT.ProgramInfoLength += data.Length;
          //string tmpString = DVB_CADescriptor(data);
          //if (pidText.IndexOf(tmpString, 0) == -1)
          // pidText += tmpString + ";";
        }
        //if (pat.caPMT.ProgramInfoLength > 0)
        //{
        //  pat.caPMT.CAPmt_CommandID_PRG = 1;
        // pat.caPMT.ProgramInfoLength += 1;
        //}
        len2 -= x;
        pointer += x;
        len1 -= x;
      }
      //byte[] b = new byte[6];
      PidInfo pmt;
      while (len1 > 4)
      {
        if (pointer + 5 > section_length) break;
        pmt = new PidInfo();
        //System.Array.Copy(buf, pointer, b, 0, 5);
        try
        {
          pmt.stream_type = buf[pointer];
          pmt.reserved_1 = (buf[pointer + 1] >> 5) & 7;
          pmt.pid = ((buf[pointer + 1] & 0x1F) << 8) + buf[pointer + 2];
          pmt.reserved_2 = (buf[pointer + 3] >> 4) & 0xF;
          pmt.ES_info_length = ((buf[pointer + 3] & 0xF) << 8) + buf[pointer + 4];
        }
        catch
        {
        }
        switch (pmt.stream_type)
        {
          case 0x1b://H.264
            pmt.isVideo = true;
            break;
          case 0x10://MPEG4
            pmt.isVideo = true;
            break;
          case 0x1://MPEG-2 VIDEO
            pmt.isVideo = true;
            break;
          case 0x2://MPEG-2 VIDEO
            pmt.isVideo = true;
            break;
          case 0x3://MPEG-2 AUDIO
            pmt.isAudio = true;
            break;
          case 0x4://MPEG-2 AUDIO
            pmt.isAudio = true;
            break;
        }
        pointer += 5;
        len1 -= 5;
        len2 = pmt.ES_info_length;
        if (len1 > 0)
        {
          while (len2 > 0)
          {
            x = 0;
            if (pointer + 1 < buf.Length)
            {
              int indicator = buf[pointer];
              x = buf[pointer + 1] + 2;
              if (x + pointer < buf.Length) // parse descriptor data
              {
                byte[] data = new byte[x];
                System.Array.Copy(buf, pointer, data, 0, x);
                switch (indicator)
                {
                  case 0x02: // video
                  case 0x03: // audio
                    //Log.Write("dvbsections: indicator {1} {0} found",(indicator==0x02?"for video":"for audio"),indicator);
                    break;
                  case 0x09:
                    //pat.caPMT.StreamType = pmt.stream_type;
                    //pat.caPMT.ElementaryStreamPID = pmt.elementary_PID;
                    //pat.caPMT.CAPmt_CommandID_ES = 1;
                    //pat.caPMT.CADescriptors_ES.Add(data);
                    //pat.caPMT.ElementaryStreamInfoLength = pmt.ES_info_length;
                    //pat.caData.Add(data);
                    //string tmpString = DVB_CADescriptor(data);
                    //if (pidText.IndexOf(tmpString, 0) == -1)
                    //  pidText += tmpString + ";";
                    break;
                  case 0x0A:
                    pmt.language = DVB_GetMPEGISO639Lang(data);
                    break;
                  case 0x6A:
                    pmt.isAC3Audio = true;
                    break;
                  case 0x56:
                    pmt.isTeletext = true;
                    pmt.teletextLANG = DVB_GetTeletextDescriptor(data);
                    break;
                  //case 0xc2:
                  case 0x59:
                    if (pmt.stream_type == 0x05 || pmt.stream_type == 0x06)
                    {
                      pmt.isDVBSubtitle = true;
                      pmt.language = DVB_SubtitleDescriptior(data);
                    }
                    break;
                  default:
                    pmt.language = "";
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
        pids.Add(pmt);
      }
      //pat.pidCache = pidText;
    }
    private string DVB_GetMPEGISO639Lang(byte[] b)
    {

      int descriptor_tag;
      int descriptor_length;
      string ISO_639_language_code = "";
      int audio_type;
      int len;

      descriptor_tag = b[0];
      descriptor_length = b[1];
      if (descriptor_length < b.Length)
        if (descriptor_tag == 0xa)
        {
          len = descriptor_length;
          byte[] bytes = new byte[len + 1];

          int pointer = 2;

          while (len > 0)
          {
            System.Array.Copy(b, pointer, bytes, 0, len);
            ISO_639_language_code += System.Text.Encoding.ASCII.GetString(bytes, 0, 3);
            if (bytes.Length >= 4)
              audio_type = bytes[3];
            pointer += 4;
            len -= 4;
          }
        }

      return ISO_639_language_code;
    }

    string DVB_SubtitleDescriptior(byte[] buf)
    {
      int descriptor_tag;
      int descriptor_length;
      string ISO_639_language_code = "";
      int subtitling_type;
      int composition_page_id;
      int ancillary_page_id;
      int len;

      descriptor_tag = buf[0];
      descriptor_length = buf[1];
      if (descriptor_length < buf.Length)
        if (descriptor_tag == 0x59)
        {
          len = descriptor_length;
          byte[] bytes = new byte[len + 1];

          int pointer = 2;

          while (len > 0)
          {
            System.Array.Copy(buf, pointer, bytes, 0, len);
            ISO_639_language_code += System.Text.Encoding.ASCII.GetString(bytes, 0, 3);
            if (bytes.Length >= 4)
              subtitling_type = bytes[3];
            if (bytes.Length >= 6)
              composition_page_id = (bytes[4] << 8) + bytes[5];
            if (bytes.Length >= 8)
              ancillary_page_id = (bytes[6] << 8) + bytes[7];

            pointer += 8;
            len -= 8;
          }
        }

      return ISO_639_language_code;
    }
    private string DVB_GetTeletextDescriptor(byte[] b)
    {
      int descriptor_tag;
      int descriptor_length;
      string ISO_639_language_code = "";
      int teletext_type;
      int teletext_magazine_number;
      int teletext_page_number;
      int len;
      if (b.Length < 2) return String.Empty;
      descriptor_tag = b[0];
      descriptor_length = b[1];

      len = descriptor_length;
      byte[] bytes = new byte[len + 1];
      if (len < b.Length + 2)
        if (descriptor_tag == 0x56)
        {
          int pointer = 2;

          while (len > 0 && (pointer + 3 <= b.Length))
          {
            System.Array.Copy(b, pointer, bytes, 0, 3);
            ISO_639_language_code += System.Text.Encoding.ASCII.GetString(bytes, 0, 3);
            teletext_type = (bytes[3] >> 3) & 0x1F;
            teletext_magazine_number = bytes[3] & 7;
            teletext_page_number = bytes[4];
            pointer += 5;
            len -= 5;
          }
        }
      if (ISO_639_language_code.Length >= 3)
        return ISO_639_language_code.Substring(0, 3);
      return "";
    }

  }
}

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

//#define MAKEDUMP
using System;
using System.Text;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
using MediaPortal;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
//using DirectX.Capture;
using MediaPortal.TV.Database;
using MediaPortal.Player;
using MediaPortal.Radio.Database;
using Toub.MediaCenter.Dvrms.Metadata;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using MediaPortal.TV.Teletext;

namespace MediaPortal.TV.Recording
{
  public class DVBDemuxer : ISampleGrabberCB
  {

    #region Global Arrays
    readonly static int[, ,] AudioBitrates = new int[,,]{{
		{-1,8000,16000,24000,32000,40000,48000,56000,64000,
		80000,96000,112000,128000,144000,160000,0 },		
		{-1,8000,16000,24000,32000,40000,48000,56000,64000,
		80000,96000,112000,128000,144000,160000,0 },		
		{-1,32000,48000,56000,64000,80000,96000,112000,128000,
		144000,160000,176000,192000,224000,256000,0 }		
	},{
		{-1,32000,40000,48000,56000,64000,80000,96000,
		112000,128000,160000,192000,224000,256000,320000, 0 },	
		{-1,32000,48000,56000,64000,80000,96000,112000,
		128000,160000,192000,224000,256000,320000,384000, 0 },	
		{-1,32000,64000,96000,128000,160000,192000,224000,
		256000,288000,320000,352000,384000,416000,448000,0 }	
	},{
		{-1, 6000, 8000, 10000, 12000, 16000, 20000, 24000,    
		28000, 320000, 40000, 48000, 56000, 64000, 80000, 0 },
		{-1, 6000, 8000, 10000, 12000, 16000, 20000, 24000,    
		28000, 320000, 40000, 48000, 56000, 64000, 80000, 0 },
		{-1, 8000, 12000, 16000, 20000, 24000, 32000, 40000,    
		48000, 560000, 64000, 80000, 96000, 112000, 128000, 0 }
	}};

    readonly static int[,] AudioFrequencies = new int[,]{
		{ 22050,24000,16000,0 },	
		{ 44100,48000,32000,0 },	
		{ 11025,12000,8000,0 }		
	};

    readonly static double[] AudioTimes = new double[] { 0.0, 103680000.0, 103680000.0, 34560000.0 };
    UInt32[] CRC32Data = new UInt32[]{0x00000000, 0x04c11db7, 0x09823b6e, 0x0d4326d9, 0x130476dc, 0x17c56b6b,
		 0x1a864db2, 0x1e475005, 0x2608edb8, 0x22c9f00f, 0x2f8ad6d6, 0x2b4bcb61,
		 0x350c9b64, 0x31cd86d3, 0x3c8ea00a, 0x384fbdbd, 0x4c11db70, 0x48d0c6c7,
		 0x4593e01e, 0x4152fda9, 0x5f15adac, 0x5bd4b01b, 0x569796c2, 0x52568b75,
		 0x6a1936c8, 0x6ed82b7f, 0x639b0da6, 0x675a1011, 0x791d4014, 0x7ddc5da3,
		 0x709f7b7a, 0x745e66cd, 0x9823b6e0, 0x9ce2ab57, 0x91a18d8e, 0x95609039,
		 0x8b27c03c, 0x8fe6dd8b, 0x82a5fb52, 0x8664e6e5, 0xbe2b5b58, 0xbaea46ef,
		 0xb7a96036, 0xb3687d81, 0xad2f2d84, 0xa9ee3033, 0xa4ad16ea, 0xa06c0b5d,
		 0xd4326d90, 0xd0f37027, 0xddb056fe, 0xd9714b49, 0xc7361b4c, 0xc3f706fb,
		 0xceb42022, 0xca753d95, 0xf23a8028, 0xf6fb9d9f, 0xfbb8bb46, 0xff79a6f1,
		 0xe13ef6f4, 0xe5ffeb43, 0xe8bccd9a, 0xec7dd02d, 0x34867077, 0x30476dc0,
		 0x3d044b19, 0x39c556ae, 0x278206ab, 0x23431b1c, 0x2e003dc5, 0x2ac12072,
		 0x128e9dcf, 0x164f8078, 0x1b0ca6a1, 0x1fcdbb16, 0x018aeb13, 0x054bf6a4,
		 0x0808d07d, 0x0cc9cdca, 0x7897ab07, 0x7c56b6b0, 0x71159069, 0x75d48dde,
		 0x6b93dddb, 0x6f52c06c, 0x6211e6b5, 0x66d0fb02, 0x5e9f46bf, 0x5a5e5b08,
		 0x571d7dd1, 0x53dc6066, 0x4d9b3063, 0x495a2dd4, 0x44190b0d, 0x40d816ba,
		 0xaca5c697, 0xa864db20, 0xa527fdf9, 0xa1e6e04e, 0xbfa1b04b, 0xbb60adfc,
		 0xb6238b25, 0xb2e29692, 0x8aad2b2f, 0x8e6c3698, 0x832f1041, 0x87ee0df6,
		 0x99a95df3, 0x9d684044, 0x902b669d, 0x94ea7b2a, 0xe0b41de7, 0xe4750050,
		 0xe9362689, 0xedf73b3e, 0xf3b06b3b, 0xf771768c, 0xfa325055, 0xfef34de2,
		 0xc6bcf05f, 0xc27dede8, 0xcf3ecb31, 0xcbffd686, 0xd5b88683, 0xd1799b34,
		 0xdc3abded, 0xd8fba05a, 0x690ce0ee, 0x6dcdfd59, 0x608edb80, 0x644fc637,
		 0x7a089632, 0x7ec98b85, 0x738aad5c, 0x774bb0eb, 0x4f040d56, 0x4bc510e1,
		 0x46863638, 0x42472b8f, 0x5c007b8a, 0x58c1663d, 0x558240e4, 0x51435d53,
		 0x251d3b9e, 0x21dc2629, 0x2c9f00f0, 0x285e1d47, 0x36194d42, 0x32d850f5,
		 0x3f9b762c, 0x3b5a6b9b, 0x0315d626, 0x07d4cb91, 0x0a97ed48, 0x0e56f0ff,
		 0x1011a0fa, 0x14d0bd4d, 0x19939b94, 0x1d528623, 0xf12f560e, 0xf5ee4bb9,
		 0xf8ad6d60, 0xfc6c70d7, 0xe22b20d2, 0xe6ea3d65, 0xeba91bbc, 0xef68060b,
		 0xd727bbb6, 0xd3e6a601, 0xdea580d8, 0xda649d6f, 0xc423cd6a, 0xc0e2d0dd,
		 0xcda1f604, 0xc960ebb3, 0xbd3e8d7e, 0xb9ff90c9, 0xb4bcb610, 0xb07daba7,
		 0xae3afba2, 0xaafbe615, 0xa7b8c0cc, 0xa379dd7b, 0x9b3660c6, 0x9ff77d71,
		 0x92b45ba8, 0x9675461f, 0x8832161a, 0x8cf30bad, 0x81b02d74, 0x857130c3,
		 0x5d8a9099, 0x594b8d2e, 0x5408abf7, 0x50c9b640, 0x4e8ee645, 0x4a4ffbf2,
		 0x470cdd2b, 0x43cdc09c, 0x7b827d21, 0x7f436096, 0x7200464f, 0x76c15bf8,
		 0x68860bfd, 0x6c47164a, 0x61043093, 0x65c52d24, 0x119b4be9, 0x155a565e,
		 0x18197087, 0x1cd86d30, 0x029f3d35, 0x065e2082, 0x0b1d065b, 0x0fdc1bec,
		 0x3793a651, 0x3352bbe6, 0x3e119d3f, 0x3ad08088, 0x2497d08d, 0x2056cd3a,
		 0x2d15ebe3, 0x29d4f654, 0xc5a92679, 0xc1683bce, 0xcc2b1d17, 0xc8ea00a0,
		 0xd6ad50a5, 0xd26c4d12, 0xdf2f6bcb, 0xdbee767c, 0xe3a1cbc1, 0xe760d676,
		 0xea23f0af, 0xeee2ed18, 0xf0a5bd1d, 0xf464a0aa, 0xf9278673, 0xfde69bc4,
		 0x89b8fd09, 0x8d79e0be, 0x803ac667, 0x84fbdbd0, 0x9abc8bd5, 0x9e7d9662,
		 0x933eb0bb, 0x97ffad0c, 0xafb010b1, 0xab710d06, 0xa6322bdf, 0xa2f33668,
		 0xbcb4666d, 0xb8757bda, 0xb5365d03, 0xb1f740b4};
    #endregion

    #region Structs
    public struct AudioHeader
    {
      //AudioHeader
      public int ID;
      public int Emphasis;
      public int Layer;
      public int ProtectionBit;
      public int Bitrate;
      public int SamplingFreq;
      public int PaddingBit;
      public int PrivateBit;
      public int Mode;
      public int ModeExtension;
      public int Bound;
      public int Channel;
      public int Copyright;
      public int Original;
      public int TimeLength;
      public int Size;
      public int SizeBase;
    } ;

    //
    // section header
    public struct DVBSectionHeader
    {
      public int TableID;
      public int SectionSyntaxIndicator;
      public int SectionLength;
      public int TableIDExtension;
      public int VersionNumber;
      public int CurrentNextIndicator;
      public int SectionNumber;
      public int LastSectionNumber;
      public int HeaderExtB8B9;
      public int HeaderExtB10B11;
      public int HeaderExtB12;
      public int HeaderExtB13;
    }
    #endregion

    #region Contructor/Destructor
    public DVBDemuxer()
    {
    }
    ~DVBDemuxer()
    {
    }
    #endregion

    #region global Vars
    int _pidTeletext = 0;
    int _pidSubtitle = 0;
    int _pidVideo = 0;
    int _pidMp2Audio = 0;
    int _pidPmt = 0;
    int _pidAc3;
    bool _ac3Present = false;
    DateTime _ac3Timer;
    string _channelName = "";
    int _restBufferLen = 0;
    byte[] _restBuffer = new byte[200];
    IntPtr _ptrRestBuffer = Marshal.AllocCoTaskMem(200);
#if GRABPPMT
		// pmt
		int m_currentPMTVersion=-1;
		// for pmt pid
		byte[] m_tableBufferPMT=new byte[4096];
int m_bufferPositionPMT=0;
#endif
    int _programNumber = -1;
    DateTime _packetTimer = DateTime.MinValue;
    bool _isReceivingPackets = false;
    bool _isScrambled = false;
    ulong _numberOfPacketsReceived = 0;
    // card
    static int _currentDVBCard = 0;
    static NetworkType _currentNetworkType;
    //DVBSectionHeader m_sectionHeader=new DVBSectionHeader();
    bool _grabTeletext = false;


    #endregion

    #region global Objects

    TSHelperTools.TSHeader _packetHeader = new TSHelperTools.TSHeader();
    TSHelperTools m_tsHelper = new TSHelperTools();

    DVBEPG _epgClass = new DVBEPG();
    //AudioHeader m_usedAudioFormat = new AudioHeader();
    #endregion

    #region Delegates/Events
    // audio format
    public delegate bool OnAudioChanged(AudioHeader audioFormat);

#if GRABPPMT
				// pmt handling
				public delegate void OnPMTChanged(byte[] pmtTable);
				public event OnPMTChanged OnPMTIsChanged;
#endif
    // grab table
    public delegate void OnTableReceived(int pid, int tableID, ArrayList tableList);
    #endregion

    #region public functions
    public void GetEPGSchedule(int tableID, int programID)
    {
#if GRABEPG
			if(tableID<0x50 || tableID>0x6f)
				return;
			if(m_sectionPid!=-1)
				return;
			SECTIONS_BUFFER_WIDTH=65535;
			m_eitScheduleLastTable=0x50;
			m_secTimer.Interval=10000;
			GetTable(0x12,tableID);// timeout 10 sec
			Log.Info("start getting epg for table 0x{0:X}",tableID);
#endif
    }

    public void GrabTeletext(bool yesNo)
    {
      _grabTeletext = yesNo;
    }

    public void SetCardType(int cardType, NetworkType networkType)
    {
      _currentDVBCard = cardType;
      _currentNetworkType = networkType;
      _epgClass = new DVBEPG(cardType, networkType);
    }

    public DVBSectionHeader GetSectionHeader(byte[] data)
    {
      return GetSectionHeader(data, 0);
    }

    public DVBSectionHeader GetSectionHeader(byte[] data, int offset)
    {
      if (data == null)
        return new DVBSectionHeader();

      if (data.Length < 14 || data.Length < offset + 14)
        return new DVBSectionHeader();

      DVBSectionHeader header = new DVBSectionHeader();
      header.TableID = data[offset];
      header.SectionSyntaxIndicator = (data[offset + 1] >> 7) & 1;
      header.SectionLength = ((data[offset + 1] & 0xF) << 8) + data[offset + 2];
      header.TableIDExtension = (data[offset + 3] << 8) + data[offset + 4];
      header.VersionNumber = ((data[offset + 5] >> 1) & 0x1F);
      header.CurrentNextIndicator = data[offset + 5] & 1;
      header.SectionNumber = data[offset + 6];
      header.LastSectionNumber = data[offset + 7];
      header.HeaderExtB8B9 = (data[offset + 8] << 8) + data[offset + 9];
      header.HeaderExtB10B11 = (data[offset + 10] << 8) + data[offset + 11];
      header.HeaderExtB12 = data[offset + 12];
      header.HeaderExtB13 = data[offset + 13];
      return header;
    }

    public void SetChannelData(int audio, int video, int ac3, int teletext, int subtitle, string channelName, int pmtPid, int programnumber)
    {
      _isReceivingPackets = false;
      _numberOfPacketsReceived = 0;
      _programNumber = -1;
      if (programnumber > 0)
        _programNumber = programnumber;
      // audio
      if (audio > 0x1FFF)
        _pidMp2Audio = -1;
      else
        _pidMp2Audio = audio;
      // video
      if (video > 0x1FFF)
        _pidVideo = -1;
      else
        _pidVideo = video;
      // teletext
      if (teletext > 0x1FFF)
        _pidTeletext = -1;
      else
        _pidTeletext = teletext;
      // subtitle
      if (subtitle > 0x1FFF)
        _pidSubtitle = -1;
      else
        _pidSubtitle = subtitle;
      // pmt pid
      if (pmtPid > 0x1FFF)
        _pidPmt = -1;
      else
        _pidPmt = pmtPid;
      // AC3
      if (ac3 > 0x1FFF)
        _pidAc3 = -1;
      else
        _pidAc3 = ac3;
      _ac3Present = false;
      _ac3Timer = DateTime.Now;

      // name
      _channelName = "";
      if (channelName != null)
        if (channelName != "")
        {
          _channelName = channelName;
        }
      //
      _packetTimer = DateTime.MinValue;


      Log.Info("DVBDemuxer:{0} audio:{1:X} video:{2:X} teletext:{3:X} pmt:{4:X} subtitle:{5:X} program:{6}",
        channelName, _pidMp2Audio, _pidVideo, _pidTeletext, _pidPmt, _pidSubtitle, _programNumber);

    }
    public bool ReceivingPackets
    {
      get { return _isReceivingPackets; }
    }

    public bool IsScrambled
    {
      get { return _isScrambled; }
    }

    public bool Ac3AudioPresent
    {
      get
      {
        return _ac3Present;
      }
    }
    public void OnTuneNewChannel()
    {
      _isScrambled = false;
      _isReceivingPackets = false;
      _numberOfPacketsReceived = 0;
      _ac3Present = false;
      _ac3Timer = DateTime.Now;
    }
    public void Process()
    {
      if (_isReceivingPackets)
      {
        TimeSpan ts = DateTime.Now - _packetTimer;
        if (ts.TotalMilliseconds >= 1000)
        {
          _isReceivingPackets = false;
          _numberOfPacketsReceived = 0;
          Log.Info("DVBDemuxer:stopped receiving DVB packets");
        }
      }
      if (_ac3Present)
      {
        TimeSpan ts = DateTime.Now - _ac3Timer;
        if (ts.TotalSeconds >= 2)
        {
          _ac3Present = false;
        }
      }
    }

    #endregion

    #region functions
    UInt32 GetCRC32(byte[] data)
    {
      UInt32 crc = 0xffffffff;
      for (UInt32 i = 0; i < data.Length - 4; i++)
      {
        crc = (crc << 8) ^ CRC32Data[((crc >> 24) ^ data[i]) & 0xff];
      }
      return crc;

    }
    UInt32 GetCRC32(byte[] data, UInt32 skip, UInt32 len)
    {
      UInt32 crc = 0xffffffff;
      for (UInt32 i = skip; i < len; ++i)
      {
        crc = (crc << 8) ^ CRC32Data[((crc >> 24) ^ data[i]) & 0xff];
      }
      return crc;

    }

    UInt32 GetSectionCRCValue(byte[] data, int ptr)
    {
      if (data.Length < ptr + 3)
        return (UInt32)0;


      return (UInt32)((data[ptr] << 24) + (data[ptr + 1] << 16) + (data[ptr + 2] << 8) + data[ptr + 3]);
    }
    bool ParseAudioHeader(byte[] data, ref AudioHeader header)
    {

      header = new AudioHeader();
      int limit = 32;

      if ((data[0] & 0xFF) != 0xFF || (data[1] & 0xF0) != 0xF0)
        return false;

      header.ID = ((data[1] >> 3) & 0x01);
      header.Emphasis = data[3] & 0x03;

      if (header.ID == 1 && header.Emphasis == 2)
        header.ID = 2;
      header.Layer = ((data[1] >> 1) & 0x03);

      if (header.Layer < 1)
        return false;

      header.ProtectionBit = (data[1] & 0x01) ^ 1;
      header.Bitrate = AudioBitrates[header.ID, header.Layer - 1, ((data[2] >> 4) & 0x0F)];
      if (header.Bitrate < 1)
        return false;
      header.SamplingFreq = AudioFrequencies[header.ID, ((data[2] >> 2) & 0x03)];
      if (header.SamplingFreq == 0)
        return false;

      header.PaddingBit = ((data[2] >> 1) & 0x01);
      header.PrivateBit = data[2] & 0x01;

      header.Mode = ((data[3] >> 6) & 0x03) & 0x03;
      header.ModeExtension = ((data[3] >> 4) & 0x03);
      if (header.Mode == 0)
        header.ModeExtension = 0;

      header.Bound = (header.Mode == 1) ? ((header.ModeExtension + 1) << 2) : limit;
      header.Channel = (header.Mode == 3) ? 1 : 2;
      header.Copyright = ((data[3] >> 3) & 0x01);
      header.Original = ((data[3] >> 2) & 0x01);
      header.TimeLength = (int)(AudioTimes[header.Layer] / header.SamplingFreq);

      if (header.ID == 1 && header.Layer == 2)
      {

        if (header.Bitrate / header.Channel < 32000)
          return false;
        if (header.Bitrate / header.Channel > 192000)
          return false;

        if (header.Bitrate < 56000)
        {
          if (header.SamplingFreq == 32000)
            limit = 12;
          else
            limit = 8;
        }
        else if (header.Bitrate < 96000)
          limit = 27;
        else
        {
          if (header.SamplingFreq == 48000)
            limit = 27;
          else
            limit = 30;
        }
        if (header.Bound > limit)
          header.Bound = limit;
      }
      else if (header.Layer == 2)  // MPEG-2
      {
        limit = 30;
      }

      if (header.Layer < 3)
      {
        if (header.Bound > limit)
          header.Bound = limit;
        header.Size = (header.SizeBase = 144 * header.Bitrate / header.SamplingFreq) + header.PaddingBit;
        return true;
      }
      else
      {
        limit = 32;
        header.Size = (header.SizeBase = (12 * header.Bitrate / header.SamplingFreq) * 4) + (4 * header.PaddingBit);
        return true;
      }

    }
    void SaveData(int pid, int tableID, byte[] data)
    {
      lock (data.SyncRoot)
      {
        if (pid == 0xd3)
        {
          if (tableID == 0x91)
            _epgClass.ParseChannels(data);
          else if (tableID == 0x90)
            _epgClass.ParseSummaries(data);
          else if (tableID == 0x92)
            _epgClass.ParseThemes(data);

        }
        if (pid == 0xd2)
        {
          if (tableID == 0x90)
            _epgClass.ParseTitles(data);
        }

      }
    }

    byte[] GetAudioHeader(byte[] data)
    {
      int pos = 0;
      bool found = false;
      for (; pos < data.Length; pos++)
      {
        if (data[pos] == 0 && data[pos + 1] == 0 && data[pos + 2] == 1)
        {
          found = true;
          break;
        }
      }
      if (found == false)
        return new byte[184];


      if ((data[pos + 3] & 0xE0) == 0xC0)
      {
        if ((data[pos + 3] & 0xE0) == 0xC0)
        {
          System.Array.Copy(data, pos, data, 0, data.Length - pos);
          return GetPES(data);
        }

      }
      return new byte[184];
    }
    public byte[] GetPES(byte[] data)
    {
      byte[] pesData = new byte[184];
      int ptr = 0;
      int offset = 0;
      bool isMPEG1 = false;

      int i = 0;
      for (; i < data.Length; )
      {
        ptr = (0xFF & data[i + 4]) << 8 | (0xFF & data[i + 5]);
        isMPEG1 = (0x80 & data[i + 6]) == 0 ? true : false;
        offset = i + 6 + (!isMPEG1 ? 3 + (0xFF & data[i + 8]) : 0);

        Array.Copy(data, offset, pesData, 0, data.Length - offset);
        i += 6 + ptr;
      }

      return pesData;
    }
    #endregion

    #region Properties

    #endregion

    #region ISampleGrabberCB Members
    #region Unused SampleCB()

    public int SampleCB(double SampleTime, IMediaSample pSample)
    {
      //throw new Exception("The method or operation is not implemented.");
      return 0;
    }
    #endregion

#if MAKEDUMP
		System.IO.BinaryWriter writer=null;
		System.IO.FileStream stream=null;
		ulong fileLen=0;
#endif
    public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
    {
      int off = -1;
      if (_restBufferLen > 0)
      {
        int len = 188 - _restBufferLen;	//remaining bytes of packet
        if (len > 0 && len < BufferLen)
        {
          if (_restBufferLen >= 0 && _restBufferLen + len < 200)
          {

            //copy the remaining bytes 
            Marshal.Copy(pBuffer, _restBuffer, _restBufferLen, len);
            Marshal.Copy(_restBuffer, 0, _ptrRestBuffer, 188);

            ProcessPacket(_ptrRestBuffer);

            //set offset ...
            if (Marshal.ReadByte(pBuffer, len) == 0x47 && Marshal.ReadByte(pBuffer, len + 188) == 0x47 && Marshal.ReadByte(pBuffer, len + 2 * 188) == 0x47)
            {
              off = len;
            }
          }
          else _restBufferLen = 0;
        }
        else _restBufferLen = 0;
      }
      if (off == -1)
      {
        //no then find first 3 transport packets in mediasample
        for (int i = 0; i < BufferLen - 2 * 188; ++i)
        {
          if (Marshal.ReadByte(pBuffer, i) == 0x47 && Marshal.ReadByte(pBuffer, i + 188) == 0x47 && Marshal.ReadByte(pBuffer, i + 2 * 188) == 0x47)
          {
            //found first 3 ts packets
            //set the offset
            off = i;
            break;
          }
        }
      }
      for (uint t = (uint)off; t < BufferLen; t += 188)
      {
        if (t + 188 > BufferLen) break;
        ProcessPacket((IntPtr)((int)pBuffer + t));
      }
      if (_restBufferLen > 0)
      {
        _restBufferLen /= 188;
        _restBufferLen *= 188;
        _restBufferLen = (BufferLen - off) - _restBufferLen;
        if (_restBufferLen > 0 && _restBufferLen < 188)
        {
          //copy the incomplete packet in the rest buffer
          Marshal.Copy((IntPtr)((int)pBuffer + BufferLen - _restBufferLen), _restBuffer, 0, _restBufferLen);
        }
      }
      return 0;
    }

    public void ProcessPacket(IntPtr ptr)
    {
      if (ptr == IntPtr.Zero) return;
      _packetHeader = m_tsHelper.GetHeader((IntPtr)ptr);
      if (_packetHeader.SyncByte != 0x47)
      {
        return;
      }
      if (_packetHeader.TransportError == true)
      {
        return;
      }
      if (_packetHeader.Pid == _pidAc3)
      {
        _ac3Present = true;
        _ac3Timer = DateTime.Now;
      }

      if (_packetHeader.Pid == _pidVideo)
      {
        if (_packetHeader.TransportScrambling != 0)
        {
          // if (!_isScrambled)
          //   Log.Info("demuxer:video pid:{0:X} is scrambled",_pidVideo);
          _isScrambled = true;
        }
        else
        {
          // if (_isScrambled)
          //   Log.Info("demuxer:video pid:{0:X} is unscrambled", _pidVideo);
          _isScrambled = false;
        }
      }

      // teletext
      if (_grabTeletext)
      {
        if (_packetHeader.Pid == _pidTeletext && _pidTeletext > 0)
        {
          TeletextGrabber.SaveData((IntPtr)ptr);
        }
      }

      if (!_isReceivingPackets)
      {
        _isReceivingPackets = true;
      }
      _numberOfPacketsReceived++;
      _packetTimer = DateTime.Now;

    }

    #endregion

    public void DumpPMT(byte[] pmt)
    {
      DVBSections sections = new DVBSections();
      DVBSections.ChannelInfo info = new DVBSections.ChannelInfo();
      if (!sections.GetChannelInfoFromPMT(pmt, ref info))
      {
        Log.Info("PMT:invalid");
        return;
      }
      Log.Info("PMT: program number:{0}", info.program_number);
      if (info.pid_list != null)
      {
        for (int pids = 0; pids < info.pid_list.Count; pids++)
        {
          DVBSections.PMTData data = (DVBSections.PMTData)info.pid_list[pids];
          if (data.isVideo)
            Log.Info(" video pid:0x{0:X}", data.elementary_PID);
          else if (data.isAudio)
            Log.Info(" audio pid:0x{0:X}", data.elementary_PID);
          else if (data.isAC3Audio)
            Log.Info(" ac3 pid:0x{0:X}", data.elementary_PID);
          else if (data.isDVBSubtitle)
            Log.Info(" dvb subtitle pid:0x{0:X}", data.elementary_PID);
          else if (data.isTeletext)
            Log.Info(" teletext pid:0x{0:X}", data.elementary_PID);
          else
            Log.Info(" unknown pid:0x{0:X}", data.elementary_PID);
        }
        Log.Info(" pcr pid:0x{0:X}", info.pcr_pid);

      }
    }
    //
    //
  }//class dvbdemuxer

}//namespace

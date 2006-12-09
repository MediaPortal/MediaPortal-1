#region Copyright (C) 2006 Team MediaPortal
/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using MediaPortal.TagReader;
using MediaPortal.GUI.Library;

namespace ID3
{
  #region Structs
  public struct ID3v1
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public char[] Header;           // should equal 'TAG' 

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
    public char[] Title;            // title

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
    public char[] Artist;            // artist

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
    public char[] Album;            // album

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public char[] Year;             // year

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 29)]
    public char[] Comment;          // comment

    public byte Track;        // track
    public byte Genre;        // genre
  };

  public struct ID3v2RawHeader
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] Header;       // ID3v2/file identifier    "ID3" (3 bytes)

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] Version;      // ID3v2 version            $04 00 (2 bytes)

    public byte Flags;          // ID3v2 flags              %abcd0000 (1 byte)

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Size;         // ID3v2 size               4 * %0xxxxxxx (4 bytes)
  }
  #endregion

  public class ID3Tag
  {
    #region Enums
    public enum MPEG_VERSION
    {
      UKNOWN = -1,
      MPEG_2_5 = 0,
      RESERVED = 1,
      MPEG_2 = 2,
      MPEG_1 = 3,
    };

    public enum LAYER_DESCRIPTION
    {
      UKNOWN = -1,
      RESERVED = 0,
      LAYER_III = 1,
      LAYER_II = 2,
      LAYER_I = 3,
    };
    #endregion

    #region Variables
    private static int[,] BitrateTable = new int[5, 16]
            {
                //Version1 Layer1
                {0, 32, 64, 96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448, 999},

                //Version1 Layer2
                {0, 32, 48, 56,  64,  80,  96, 112, 128, 160, 192, 224, 256, 320, 384, 999},

                //Version1 Layer3
                {0, 32, 40, 48,  56,  64,  80,  96, 112, 128, 160, 192, 224, 256, 320, 999},

                //Version2 Layer1
                {0, 32, 48, 56,  64,  80,  96, 112, 128, 144, 160, 176, 192, 224, 256, 999},

                //Version2 Layer2 or Layer3
                {0,  8, 16, 24,  32,  40,  48,  56,  64,  80,  96, 112, 128, 144, 160, 999},
            };

    private static int[,] SampleRateTable = new int[3, 3]
            {
                // MPEG1
                {44100, 48000, 32000},

                // MPEG2
                {22050, 24000, 16000},

                // MPEG3
                {11025, 12000, 8000}
            };

    private static int[,] SamplesPerFrameTable = new int[3, 3]
      {
        // MPEG1
        {384, 1152, 1152},
 
        // MPEG2
        {384, 1152, 576},

        // MPEG2
        {384, 1152, 576}
      };

    private static int[] SlotSizes = new int[3]
      {
        4,  // Layer1
        1,  // Layer2
        1   // Layer3
      };

    private string[] ImageType = new string[]
        {
            "Other",
            "File icon",
            "Other file icon",
            "Cover (front)",
            "Cover (back)",
            "Leaflet page",
            "Media (e.g. lable side of CD)",
            "Lead artist/lead performer/soloist",
            "Artist/performer",
            "Conductor",
            "Band/Orchestra",
            "Composer",
            "Lyricist/text writer",
            "Recording Location",
            "During recording",
            "During performance",
            "Movie/video screen capture",
            "A bright coloured fish",
            "Illustration",
            "Band/artist logotype",
            "Publisher/Studio logotype"
        };

    public const byte V2ExtendedHeaderFlag = 0x40;      // binary: 01000000; dec: 64
    public const byte V2FooterPresentFlag = 0x10;	    // binary: 00010000; dec: 16
    public const byte V2UnsynchronisationFlag = 128;    // binary: 10000000; dec: 128
    public const byte V2ExperimentalFlag = 0x20;	    // binary: 00100000; dec: 32

    public const string Id3v1ID = "TAG";
    public const string Id3v2ID = "ID3";
    public const int ID3_TAG_BYTES = 128;

    public ID3v1 ID3v1Tag = new ID3v1();
    public ID3v2RawHeader Id3v2RawHeader = new ID3v2RawHeader();

    private bool HasExtendedHeader = false;
    private bool HasFooter = false;
    private bool UsesUnsynchronisation = false;
    private bool IsExperimental = false;

    private int ID3v2TagSize = 0;
    private List<ID3Frame> _ID3Frames = new List<ID3Frame>();

    private MPEG_VERSION _MpegVersion = MPEG_VERSION.UKNOWN;
    private LAYER_DESCRIPTION _LayerDescription = LAYER_DESCRIPTION.UKNOWN;
    private int _AudioDataLen = 0;
    private int _BitRate = 0;
    private int _SampleRate = 0;
    private double _BytesPerFrame = 0;
    private double _DurationPerFrame = 0;
    private double _Duration = 0;
    private int _TotalFrameCount = 0;
    private int _ChannelMode = 0;
    private string _ChannelModeString = "";
    private bool _VariableBitRate = false;
    #endregion

    #region Constructors/Destructors
    public ID3Tag(FileStream s)
    {
      bool foundV1Tag = GetID3v1Tag(s);
      bool foundV2Tag = GetID3v2Tag(s);
    }
    #endregion

    #region Properties

    public bool HasV1Header
    {
      get
      {
        return (new string(ID3v1Tag.Header).CompareTo(Id3v1ID) == 0);
      }
    }

    public bool HasV2Header
    {
      get
      {
        return Id3v2RawHeader.Header[0] == (byte)'I'
            && Id3v2RawHeader.Header[1] == (byte)'D'
            && Id3v2RawHeader.Header[2] == (byte)'3';
      }
    }

    public bool IsVBR
    {
      get { return _VariableBitRate; }
    }

    public List<ID3Frame> ID3Frames
    {
      get { return _ID3Frames; }
    }

    public int Duration
    {
      get { return (int)_Duration; }
    }

    public int Channels
    {
      get
      {
        switch (_ChannelMode)
        {
          case 0:             // 00 - Stereo 
          case 1:             // 01 - Joint stereo (Stereo)
          case 2:             // 10 - Dual channel (Stereo)
            return 2;

          case 3:
            return 1;  // 11 - Mono

          default:
            return 0;
        }
      }
    }

    public int BitRate
    {
      get { return _BitRate; }
    }

    public int SampleRate
    {
      get { return _SampleRate; }
    }

    #endregion

    #region Private Methods
    private bool GetID3v1Tag(FileStream s)
    {
      s.Seek(-ID3.ID3Tag.ID3_TAG_BYTES, SeekOrigin.End);
      byte[] id3v1Bytes = Utils.RawSerializeEx(ID3v1Tag);

      s.Read(id3v1Bytes, 0, ID3.ID3Tag.ID3_TAG_BYTES);
      ID3v1Tag = (ID3v1)Utils.RawDeserializeEx(id3v1Bytes, typeof(ID3v1));

      bool hasV1Hdr = HasV1Header;

      // If the ID3V1 header wasn't found, the ID3v1Tag object contains garbage so clear it
      if (!hasV1Hdr)
        ID3v1Tag = new ID3v1();

    // Now we need to determine if this is an ID3v1 or ID3v1.1 tag
      // The v1 spec called for a 30 byte comment byte[]
      // The v1.1 spec steals a byte from the comment byte[] and uses it for track number
      // The only distinction the that a v1.1 must have the last byte of the the comment byte[] set to 0
      else
      {
        if (ID3v1Tag.Comment[28] != 0)
          ID3v1Tag.Track = 0;
      }

      return hasV1Hdr;
    }

    private bool GetID3v2Tag(FileStream s)
    {
      s.Position = 0;
      byte[] id3v2RawHeaderBytes = Utils.RawSerializeEx(Id3v2RawHeader);
      s.Read(id3v2RawHeaderBytes, 0, id3v2RawHeaderBytes.Length);
      Id3v2RawHeader = (ID3v2RawHeader)Utils.RawDeserializeEx(id3v2RawHeaderBytes, typeof(ID3v2RawHeader));
      ID3v2TagSize = Utils.ReadUnsynchronizedData(Id3v2RawHeader.Size, 0, 4);

      if (!HasV2Header)
        return GetMpegAudioFrameHeader(s, false);

      HasExtendedHeader = Utils.IsFlagSet(Id3v2RawHeader.Flags, V2ExtendedHeaderFlag);
      HasFooter = Utils.IsFlagSet(Id3v2RawHeader.Flags, V2FooterPresentFlag);
      UsesUnsynchronisation = Utils.IsFlagSet(Id3v2RawHeader.Flags, V2UnsynchronisationFlag);
      IsExperimental = Utils.IsFlagSet(Id3v2RawHeader.Flags, V2ExperimentalFlag);

      if (HasExtendedHeader)
        ReadExtendedHeader(s);

      GetFrames(s);

      return true;
    }

    // Returns size of extended header
    private bool ReadExtendedHeader(FileStream s)
    {
      // Extended header size   4 * %0xxxxxxx
      // Number of flag bytes       $01
      // Extended Flags             $xx

      try
      {
        byte[] buffer = new byte[4];
        s.Read(buffer, 0, buffer.Length);
        int extendedHdrLen = Utils.ReadUnsynchronizedData(buffer, 0, buffer.Length);
        s.Seek(extendedHdrLen, SeekOrigin.Current);
        return true;
      }

      catch
      {
        return false;
      }
    }

    private bool GetFrames(FileStream s)
    {
      bool result = true;
      bool frameReadException = false;
      int currentLength = 0;
      long startingStreamPosition = s.Position;

      while (currentLength < ID3v2TagSize)
      {
        ID3Frame frame;

        try
        {
          frame = new ID3Frame(s, (int)Id3v2RawHeader.Version[0]);
          currentLength += frame.Size;
          _ID3Frames.Add(frame);
        }

        catch (ID3.ID3FramePaddingException)
        {
          break;
        }

        catch (Exception)
        {
          Log.Warn("File contains invalid ID3 Tags. Please use a Tag Editor to correct the tags.: {0}", s.Name);
          frameReadException = true;
          break;
        }

      }

      result = GetMpegAudioFrameHeader(s, frameReadException);

      s.Position = startingStreamPosition + ID3v2TagSize;
      return result;
    }

    private bool GetMpegAudioFrameHeader(FileStream s, bool frameReadException)
    {
      bool result = true;
      int id3TagLength = HasV1Header ? 128 : 0;
      int id3v2TagLength = HasV2Header ? ID3v2TagSize + Utils.ID3_HEADERSIZE : 0;
      _AudioDataLen = (int)s.Length - (id3TagLength + id3v2TagLength);

      try
      {
        // Bit layout:                  AAAA AAAA AAAB BCCD EEEE FFGH IIJJ KLMM
        //                              =======================================
        // A: Frame sync                1111 1111 1110 0000 0000 0000 0000 0000
        // B: MPEG Audio version ID     0000 0000 0001 1000 0000 0000 0000 0000
        // C: Layer description         0000 0000 0000 0110 0000 0000 0000 0000
        // D: Protection bit            0000 0000 0000 0001 0000 0000 0000 0000
        // E: Bitrate index             0000 0000 0000 0000 1111 0000 0000 0000

        //                              AAAA AAAA AAAB BCCD EEEE FFGH IIJJ KLMM
        //                              =======================================
        // F: Sampling rate freq index  0000 0000 0000 0000 0000 1100 0000 0000
        // G: Padding bit               0000 0000 0000 0000 0000 0010 0000 0000
        // H: Private bit               0000 0000 0000 0000 0000 0001 0000 0000
        // I: Channel Mode              0000 0000 0000 0000 0000 0000 1100 0000
        // J: Mode extension (Only      0000 0000 0000 0000 0000 0000 0011 0000
        //    if Joint stereo)

        //                              AAAA AAAA AAAB BCCD EEEE FFGH IIJJ KLMM
        //                              =======================================
        // K: Copyright                 0000 0000 0000 0000 0000 0000 0000 1000
        // L: Original                  0000 0000 0000 0000 0000 0000 0000 0100
        // M: Emphasis                  0000 0000 0000 0000 0000 0000 0000 0011

        // When we got a framereadexception, we had an invalid id3v2 length and need to search from the beginning of the file
        if (!frameReadException)
          s.Seek(id3v2TagLength, SeekOrigin.Begin);

        bool doAccurateDurationCalc = false;

        byte[] mpegAudioFrameHeader = new byte[4];
        UInt32 mpegAudioFrameVal = 0;
        bool foundValidHeader = false;

        while (true)
        {
          if (IsValidHeader(mpegAudioFrameVal, mpegAudioFrameHeader))
          {
            foundValidHeader = true;
            // if we had a framereadexception the Audio data length needs to be recalculated
            if (frameReadException)
              _AudioDataLen = (int)s.Length - (int)s.Position;
            break;
          }

          //Make sure we have a way out if we've read the entire file...
          if (s.Position == s.Length)
            return false;

          if (s.Position > (id3v2TagLength + 1024 * 200))
          {
            return false;
          }

          // read in four characters
          try
          {
            s.Read(mpegAudioFrameHeader, 0, 4);
          }

          catch (Exception ex)
          {
            Log.Error("ID3.GetMpegAudioFrameHeader caused an exception: {0}", ex.Message);
            return false;
          }

          mpegAudioFrameVal = GetMpegAudioFrameHeaderValue(mpegAudioFrameHeader);
        }

        if (!foundValidHeader)
          return false;

        // Check if it is a RIFF Header
        if (IsRIFFHeader(mpegAudioFrameHeader))
        {
          if (GetRiffHeader(s))
            return true;
          else
            return false;
        }

        long firstFramePosition = s.Position;

        try { _MpegVersion = (MPEG_VERSION)getVersionId(mpegAudioFrameVal); }
        catch { }

        try { _LayerDescription = (LAYER_DESCRIPTION)getLayerDescription(mpegAudioFrameVal); }
        catch { }

        _BitRate = GetBitrate(getBitrateIndex(mpegAudioFrameVal), _MpegVersion, _LayerDescription);
        _SampleRate = GetSampleRate(getSampleRateIndex(mpegAudioFrameVal), _MpegVersion);
        _ChannelMode = (int)getChannelModeIndex(mpegAudioFrameVal);
        _ChannelModeString = GetChannelModeString(getChannelModeIndex(mpegAudioFrameVal));

        if (doAccurateDurationCalc)
        {
          return DoAccurateDurationCalc(s);
        }

        // Make the assumption that we have CBR encoding
        // We'll check for VBR encoding later...
        float size = ((float)(144 * _BitRate) / (float)_SampleRate + getPaddingBit(mpegAudioFrameVal));

        // Make sure we don't end up dividing by zero
        if (size > 0 && _AudioDataLen > 0 && _BitRate > 0)
          _Duration = (double)_AudioDataLen * 0.008 / _BitRate;

        bool foundXingHeader = ReadXingHeader(s);
        bool foundFraunhoferHeader = false;

        // If no Xing header was found look for a Fraunhofer "VBRI" header
        if (!foundXingHeader)
        {
          if (frameReadException)
            s.Position = 0;
          else
            s.Seek(id3v2TagLength, SeekOrigin.Begin);
          foundFraunhoferHeader = ReadFraunhoferHeader(s);
        }
      }

      catch (Exception ex)
      {
        Log.Error("ID3.GetMpegAudioFrameHeader caused an exception: {0}", ex.Message);
        result = false;
      }

      _BitRate = GetBitrate(s, _Duration);
      return result;
    }

    private bool DoAccurateDurationCalc(FileStream s)
    {
      byte[] mpegAudioFrameHeader = new byte[4];
      uint mpegAudioFrameVal = 0;
      _TotalFrameCount = 0;
      int sumBitrates = 0;
      int _FrameSize = 0;
      int bitrate = 0;

      // Search for a valid header
      while (s.Position < s.Length)
      {
        if (IsValidHeader(mpegAudioFrameVal, mpegAudioFrameHeader))
        {
          bitrate = GetBitrate(getBitrateIndex(mpegAudioFrameVal), _MpegVersion, _LayerDescription);
          ++_TotalFrameCount;
          sumBitrates += bitrate;

          // Calculate Framesize and skip to next header
          _FrameSize = (int)((((double)GetSamplesPerFrame(_MpegVersion, _LayerDescription) / 8 * (double)bitrate * 1000) / (double)GetSampleRate(getSampleRateIndex(mpegAudioFrameVal), _MpegVersion)) + getPaddingBit(mpegAudioFrameVal)) * GetSlotSize(_LayerDescription);
          s.Position += _FrameSize - 4;
        }
        // read in four characters
        try
        {
          s.Read(mpegAudioFrameHeader, 0, 4);
        }

        catch (Exception ex)
        {
          Log.Error("ID3.GetMpegAudioFrameHeader caused an exception: {0}", ex.Message);
          return false;
        }

        mpegAudioFrameVal = GetMpegAudioFrameHeaderValue(mpegAudioFrameHeader);
      }

      if (_TotalFrameCount > 0)
        _Duration = sumBitrates / _TotalFrameCount;

      return true;
    }

    private UInt32 GetMpegAudioFrameHeaderValue(byte[] mpegAudioFrameHeader)
    {
      if (mpegAudioFrameHeader == null || mpegAudioFrameHeader.Length < 4)
        return 0;

      UInt32 val = (UInt32)
        (UInt32)((mpegAudioFrameHeader[0] & 255) << 24)
      | (UInt32)((mpegAudioFrameHeader[1] & 255) << 16)
      | (UInt32)((mpegAudioFrameHeader[2] & 255) << 8)
      | (UInt32)((mpegAudioFrameHeader[3] & 255));

      return val;
    }

    private bool ReadXingHeader(Stream s)
    {
      byte[] buffer = new byte[4];

      // Look for the Xing header for 8kb of data stepping 4 bytes at a time
      for (int i = 0; i < 8192; i += 4)
      {
        // Read 4 bytes at a time and look for the Xing header
        s.Read(buffer, 0, 4);

        if (IsXingHeader(buffer))
        {
          //Console.WriteLine("Found Xing header!");
          _VariableBitRate = true;
          s.Read(buffer, 0, 4);

          uint flags = GetMpegAudioFrameHeaderValue(buffer);
          int vBRFramesFlag = 1;
          if ((flags & vBRFramesFlag) > 0)
          {
            // Get the frame total count value
            s.Read(buffer, 0, 4);
            _TotalFrameCount = Utils.ReadSynchronizedData(buffer, 0, 4);

            // Get the total MP3 audio size 
            s.Read(buffer, 0, 4);
            int xingAudioDataLen = Utils.ReadSynchronizedData(buffer, 0, 4);

            if (xingAudioDataLen != _AudioDataLen)
            {
              Console.WriteLine("xingAudioDataLen length wrong! xingAudioDataLen:{0}   _AudioDataLen:{1}", xingAudioDataLen, _AudioDataLen);
            }

            _BytesPerFrame = GetBytesPerFrame(_LayerDescription);

            double[] samplesPerFrameTable = new double[4] { 0, 1152.0f, 1152.0f, 384.0f };
            _DurationPerFrame = (samplesPerFrameTable[(int)_LayerDescription] / (double)_SampleRate);

            if (_MpegVersion == MPEG_VERSION.MPEG_2 || _MpegVersion == MPEG_VERSION.MPEG_2_5)
              _DurationPerFrame /= 2;

            if (_TotalFrameCount > 0 && _SampleRate > 0)
              _Duration = _TotalFrameCount * 1152 / _SampleRate;


            return true;
          }
        }
      }

      return false;
    }

    private bool ReadFraunhoferHeader(Stream s)
    {
      byte[] buffer = new byte[4];
      int sampleRate = GetFraunhoferSampleRate(s);
      //Console.WriteLine("Fraunhofer SampleRate Rate:{0}", sampleRate);

      for (int i = 0; i < 8192; i += 4)
      {
        s.Read(buffer, 0, 4);

        if (IsFrauhoferHeader(buffer))
        {
          //Console.WriteLine("Found Fraunhofer header!");
          byte[] shortBuf = new byte[2];
          byte[] intBuf = new byte[4];

          s.Read(shortBuf, 0, shortBuf.Length);
          int VbriVersion = ReadFromVBRIBuffer(shortBuf, shortBuf.Length);

          s.Read(shortBuf, 0, shortBuf.Length);
          int VbriDelay = Utils.GetInt(shortBuf, 0, shortBuf.Length);

          s.Read(shortBuf, 0, shortBuf.Length);
          int VbriQuality = ReadFromVBRIBuffer(shortBuf, shortBuf.Length);

          s.Read(intBuf, 0, intBuf.Length);
          int VbriStreamBytes = ReadFromVBRIBuffer(intBuf, intBuf.Length);

          s.Read(intBuf, 0, intBuf.Length);
          int VbriStreamFrames = ReadFromVBRIBuffer(intBuf, intBuf.Length);

          s.Read(shortBuf, 0, shortBuf.Length);
          int VbriTableSize = ReadFromVBRIBuffer(shortBuf, shortBuf.Length);

          s.Read(shortBuf, 0, shortBuf.Length);
          int VbriTableScale = ReadFromVBRIBuffer(shortBuf, shortBuf.Length);

          s.Read(shortBuf, 0, shortBuf.Length);
          int VbriEntryBytes = ReadFromVBRIBuffer(shortBuf, shortBuf.Length);

          s.Read(shortBuf, 0, shortBuf.Length);
          int VbriEntryFrames = ReadFromVBRIBuffer(shortBuf, shortBuf.Length);

          _Duration = VbriStreamFrames * 1152 / _SampleRate;

          return true;
        }
      }

      return false;
    }

    int ReadFromVBRIBuffer(byte[] HBuffer, int length)
    {
      if (HBuffer == null || HBuffer.Length < length)
        return 0;

      int b = 0;
      int number = 0;

      for (int i = 0; i < length; i++)
      {

        b = length - 1 - i;
        number = number | (int)(HBuffer[i] & 0xff) << (8 * b);
      }

      return number;
    }

    private bool IsXingHeader(byte[] mpegAudioFrameHeader)
    {
      if (mpegAudioFrameHeader == null || mpegAudioFrameHeader.Length < 4)
        return false;

      char x = (char)mpegAudioFrameHeader[0];
      char i = (char)mpegAudioFrameHeader[1];
      char n = (char)mpegAudioFrameHeader[2];
      char g = (char)mpegAudioFrameHeader[3];

      if (x == 'X' && i == 'i' && n == 'n' && g == 'g')
        return true;

    // LAME encoding uses either a "Xing" on "Info" VBR tag
      else if (x == 'I' && i == 'n' && n == 'f' && g == '0')
        return true;

      return false;
    }

    private bool IsFrauhoferHeader(byte[] mpegAudioFrameHeader)
    {
      if (mpegAudioFrameHeader == null || mpegAudioFrameHeader.Length < 4)
        return false;

      char v = (char)mpegAudioFrameHeader[0];
      char b = (char)mpegAudioFrameHeader[1];
      char r = (char)mpegAudioFrameHeader[2];
      char i = (char)mpegAudioFrameHeader[3];

      if (v == 'V' && b == 'B' && r == 'R' && i == 'I')
        return true;

      return false;
    }

    private double GetBytesPerFrame(LAYER_DESCRIPTION layer)
    {
      double bytesPerFrame = 0;

      switch (layer)
      {
        case LAYER_DESCRIPTION.LAYER_I:
          {
            bytesPerFrame = (_BitRate * 48000) / _SampleRate;
            break;
          }

        case LAYER_DESCRIPTION.LAYER_II:
        case LAYER_DESCRIPTION.LAYER_III:
          {
            bytesPerFrame = (_BitRate * 144000) / _SampleRate;
            break;
          }

        default:
          bytesPerFrame = 1;
          break;
      }

      return bytesPerFrame;
    }

    private int GetBitrate(FileStream s, double durationSecs)
    {
      try
      {
        long audioDataLength = s.Length - s.Position;
        long bitrate = (((audioDataLength * 8000) / (int)(durationSecs * 1000)) + 500) / 1000;
        return (int)bitrate;
      }

      catch (Exception ex)
      {
        Log.Error("ID3.GetBitrate caused an exception: {0}", ex.Message);
        return 0;
      }
    }

    private int GetBitrate(UInt32 bitRateIndex, MPEG_VERSION mpegVer, LAYER_DESCRIPTION layerDesc)
    {
      if (bitRateIndex <= 0 || bitRateIndex >= 15)
        return 0;

      int table = -1;

      if (mpegVer == MPEG_VERSION.MPEG_1 && layerDesc == LAYER_DESCRIPTION.LAYER_I)
        table = 0;

      else if (mpegVer == MPEG_VERSION.MPEG_1 && layerDesc == LAYER_DESCRIPTION.LAYER_II)
        table = 1;

      else if (mpegVer == MPEG_VERSION.MPEG_1 && layerDesc == LAYER_DESCRIPTION.LAYER_III)
        table = 2;

      else if ((mpegVer == MPEG_VERSION.MPEG_2 || mpegVer == MPEG_VERSION.MPEG_2_5) && layerDesc == LAYER_DESCRIPTION.LAYER_I)
        table = 3;

      else if ((mpegVer == MPEG_VERSION.MPEG_2 || mpegVer == MPEG_VERSION.MPEG_2_5) && (layerDesc == LAYER_DESCRIPTION.LAYER_II || layerDesc == LAYER_DESCRIPTION.LAYER_III))
        table = 4;

      else
        return 0;

      return BitrateTable[table, bitRateIndex];
    }

    private int GetSampleRate(UInt32 sampleRateIndex, MPEG_VERSION mpegVer)
    {
      if (sampleRateIndex < 0 || sampleRateIndex > 2)
        return 0;

      int table = -1;

      if (mpegVer == MPEG_VERSION.MPEG_1)
        table = 0;

      else if (mpegVer == MPEG_VERSION.MPEG_2)
        table = 1;

      else if (mpegVer == MPEG_VERSION.MPEG_2_5)
        table = 2;

      return SampleRateTable[table, sampleRateIndex];
    }

    private int GetSamplesPerFrame(MPEG_VERSION mpegVer, LAYER_DESCRIPTION layerDesc)
    {
      int row = -1;
      int column = -1;

      if (mpegVer == MPEG_VERSION.MPEG_1)
        row = 0;

      else if (mpegVer == MPEG_VERSION.MPEG_2)
        row = 1;

      else if (mpegVer == MPEG_VERSION.MPEG_2_5)
        row = 2;

      if (layerDesc == LAYER_DESCRIPTION.LAYER_I)
        column = 0;

      else if (layerDesc == LAYER_DESCRIPTION.LAYER_II)
        column = 1;

      else if (layerDesc == LAYER_DESCRIPTION.LAYER_III)
        column = 2;

      return SamplesPerFrameTable[row, column];
    }

    private int GetSlotSize(LAYER_DESCRIPTION layerDesc)
    {
      int table = -1;
      if (layerDesc == LAYER_DESCRIPTION.LAYER_I)
        table = 0;

      else if (layerDesc == LAYER_DESCRIPTION.LAYER_II)
        table = 1;

      else if (layerDesc == LAYER_DESCRIPTION.LAYER_III)
        table = 2;

      return SlotSizes[table];
    }

    private string GetChannelModeString(UInt32 channelMode)
    {
      if (channelMode == 0)
        return "Stereo";

      else if (channelMode == 1)
        return "Joint stereo (Stereo)";

      else if (channelMode == 2)
        return "Dual channel (Stereo)";

      else if (channelMode == 3)
        return "Mono";

      else
        return string.Empty;
    }

    private UInt32 getFrameSync(UInt32 mpegAudioFrameVal)
    {
      UInt32 frameSyncBitMask = 4292870144;
      return (mpegAudioFrameVal & frameSyncBitMask) >> 21;
    }

    private UInt32 getVersionId(UInt32 mpegAudioFrameVal)
    {
      UInt32 audioVersionIdBitMask = 1572864;
      UInt32 audioVersionId = (mpegAudioFrameVal & audioVersionIdBitMask) >> 19;
      return audioVersionId;
    }

    private UInt32 getLayerDescription(UInt32 mpegAudioFrameVal)
    {
      UInt32 layerDescriptionBitMask = 393216;
      return (mpegAudioFrameVal & layerDescriptionBitMask) >> 17;
    }

    private UInt32 getProtectionBit(UInt32 mpegAudioFrameVal)
    {
      UInt32 protectionBitMask = 65536;
      return (mpegAudioFrameVal & protectionBitMask) >> 16;
    }

    private UInt32 getBitrateIndex(UInt32 mpegAudioFrameVal)
    {
      UInt32 bitRateIndexMask = 61440;
      return (mpegAudioFrameVal & bitRateIndexMask) >> 12;
    }

    private UInt32 getSampleRateIndex(UInt32 mpegAudioFrameVal)
    {
      UInt32 sampleRateIndexBitMask = 3072;
      return (mpegAudioFrameVal & sampleRateIndexBitMask) >> 10;
    }

    private UInt32 getPaddingBit(UInt32 mpegAudioFrameVal)
    {
      UInt32 paddingBitMask = 512;
      return (mpegAudioFrameVal & paddingBitMask) >> 9;
    }

    private UInt32 getPrivateBit(UInt32 mpegAudioFrameVal)
    {
      UInt32 privateBitMask = 256;
      return (mpegAudioFrameVal & privateBitMask) >> 8;
    }

    private UInt32 getChannelModeIndex(UInt32 mpegAudioFrameVal)
    {
      UInt32 channelModeBitMask = 192;
      return (mpegAudioFrameVal & channelModeBitMask) >> 6;
    }

    private UInt32 getModeExtIndex(UInt32 mpegAudioFrameVal)
    {
      UInt32 modeExtensionBitMask = 48;
      return (mpegAudioFrameVal & modeExtensionBitMask) >> 4;
    }

    private UInt32 getCopyrightBit(UInt32 mpegAudioFrameVal)
    {
      UInt32 copyrightBitMask = 8;
      return (mpegAudioFrameVal & copyrightBitMask) >> 3;
    }

    private UInt32 getOrginalBit(UInt32 mpegAudioFrameVal)
    {
      UInt32 originalBitMask = 4;
      return (mpegAudioFrameVal & originalBitMask) >> 2;
    }

    private UInt32 getEmphasisIndex(UInt32 mpegAudioFrameVal)
    {
      UInt32 emphasisBitMask = 3;
      return (mpegAudioFrameVal & emphasisBitMask);
    }

    // Do we have a valid header?
    private bool IsValidHeader(UInt32 mpegAudioFrameVal, byte[] mpegAudioFrameHeader)
    {
      // Check if it is a RIFF Header
      if (IsRIFFHeader(mpegAudioFrameHeader))
        return true;

      // Valid MP3 Header
      return (((getFrameSync(mpegAudioFrameVal) & 2047) == 2047) &&
          ((getVersionId(mpegAudioFrameVal) & 3) != 1) &&
          ((getLayerDescription(mpegAudioFrameVal) & 3) != 0) &&
          ((getBitrateIndex(mpegAudioFrameVal) & 15) != 0) &&
          ((getBitrateIndex(mpegAudioFrameVal) & 15) != 15) &&
          ((getSampleRateIndex(mpegAudioFrameVal) & 3) != 3) &&
          ((getEmphasisIndex(mpegAudioFrameVal) & 3) != 2));
    }

    // Check for RIFF Header. The file might be a RIFF Wavefmt, altough having mp3 extension
    private bool IsRIFFHeader(byte[] mpegAudioFrameHeader)
    {
      if (mpegAudioFrameHeader == null || mpegAudioFrameHeader.Length < 4)
        return false;

      char r = (char)mpegAudioFrameHeader[0];
      char i = (char)mpegAudioFrameHeader[1];
      char f = (char)mpegAudioFrameHeader[2];
      char f2 = (char)mpegAudioFrameHeader[3];

      if (r == 'R' && i == 'I' && f == 'F' && f2 == 'F')
        return true;

      return false;
    }

    private bool GetRiffHeader(FileStream s)
    {
      // Check for RIFF, which might be hidden in the file

      // Format:
      // 0000-0003: Chunk ID  "RIFF"  in  ASCII  ("RIFX"  files  identify  the
      //            samples audio data in hi/lo format instead of  the  normal lo/hi format)

      // 0004-0007: Chunk size (lo/hi) $01CC45D4=30,164,452 is the total  size
      //            minus 8 (these first 8  bytes  are  not  included  in  the
      //            overall size)

      // 0008-000B: Chunk format "WAVE". This format requires two subchunks to
      //            exist, "fmt " and "data".

      // 000C-000F: Subchunk1 ID "fmt ". This describes the format of the next DATA subchunk.

      // 0010-0013: Subchunk size (lo/hi) $00000010 (usually  this  value  for PCM audio)

      // 0014-0015: Audio format (1=PCM, 2 and higher are custom) ($0001)
      //            - $0001 = standard PCM
      //            - $0101 = IBM mu-law (custom)
      //            - $0102 = IBM a-law (custom)
      //            - $0103 = IBM AVC ADPCM (custom)

      // 0016-0017: Number of channels (1=mono, 2=stereo, etc) ($0002)

      // 0018-001B: Sample rate per second (lo/hi) $0000AC44 = 44100

      // 001C-001F: Byte rate per second (=sample rate * number of channels  *
      //            (bits per channel/8)) $0002B110 = 176400

      // 0020-0021: Block Align (=number of  channels  *  bits  per  sample/8) ($0004).

      // 0022-0023: Bits per sample (8=8 bits, 16=16  bits)  ($0010).  Samples
      //            not using the entire bit range allocated  should  set  the
      //            unused bits off.

      // 0024-0027:  Subchunk2  ID  "data".  This  chunk  contains  the  audio
      //             samples. There can be more than one in a WAV file.

      // 0028-002B: Subchunk2 size (lo/hi) ($01CC45B0=30,164,400)

      // 002C-xxxx: Audio data (lo/hi),  stored  as  2's  complimented  signed
      //            integers in the following order:
      try
      {
        _MpegVersion = MPEG_VERSION.MPEG_1;
        _LayerDescription = LAYER_DESCRIPTION.LAYER_III;
        _BitRate = 128;

        // Get Audio Length
        byte[] chunkSize = new byte[4];
        s.Read(chunkSize, 0, 4);
        Array.Reverse(chunkSize);
        _AudioDataLen = Utils.ReadSynchronizedData(chunkSize, 0, 4);

        // Get Channel Mode
        s.Seek(14, SeekOrigin.Current);
        byte[] channels = new byte[2];
        s.Read(channels, 0, 2);
        Array.Reverse(channels);
        int channelmode = Utils.ReadSynchronizedData(channels, 0, 2);

        if (channelmode == 1)
          _ChannelModeString = "Mono";

        else
          _ChannelModeString = "Stereo";

        _ChannelMode = channelmode;

        // Get Sample Rate
        byte[] rate = new byte[4];
        s.Read(rate, 0, 4);
        Array.Reverse(rate);
        _SampleRate = Utils.ReadSynchronizedData(rate, 0, 4);

        // Get Rate / second
        s.Read(rate, 0, 4);
        Array.Reverse(rate);
        int ratePerSecond = Utils.ReadSynchronizedData(rate, 0, 4);

        // Calculate the Duration
        _Duration = (double)_AudioDataLen / (double)ratePerSecond;
        return true;
      }

      catch (Exception ex)
      {
        Log.Error("ID3.GetRiffHeader caused an exception: {0}", ex.Message);
        return false;
      }
    }

    private int GetFraunhoferSampleRate(Stream s)
    {
      int id = 0;
      int idx = 0;
      int mpeg = 0;

      byte[] buffer = new byte[4];
      s.Read(buffer, 0, buffer.Length);

      id = (0xC0 & (buffer[1] << 3)) >> 4;
      idx = (0xC0 & (buffer[2] << 4)) >> 6;

      mpeg = id | idx;

      switch (mpeg)
      {
        case 0:
          return 11025;

        case 1:
          return 12000;

        case 2:
          return 8000;

        case 8:
          return 22050;

        case 9:
          return 24000;

        case 10:
          return 16000;

        case 12:
          return 44100;

        case 13:
          return 48000;

        case 14:
          return 32000;

        default:
          return 0;
      }
    }
    #endregion
  }
}
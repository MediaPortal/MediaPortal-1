#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.IO;
using System.Text;

using Tag.MP4.MiscUtil.Conversion;
using MediaPortal.GUI.Library;

namespace Tag.MP4
{
  #region ParsedAtom class

  public class ParsedAtom
  {
    protected long m_size;
    protected string m_type;

    public ParsedAtom(long size, string type)
    {
      m_size = size;
      m_type = type;
    }

    public long Size
    {
      get
      {
        return m_size;
      }
    }

    public string Type
    {
      get
      {
        return m_type;
      }
    }
  }

  #endregion

  #region ParsedContainerAtom class

  public class ParsedContainerAtom : ParsedAtom
  {
    protected ParsedAtom[] m_children;

    #region Properties

    public ParsedAtom[] Children
    {
      get
      {
        return m_children;
      }
    }

    #endregion

    public ParsedContainerAtom(long atomSize, string atomType, ParsedAtom[] children)
      : base(atomSize, atomType)
    {
      m_children = children;
    }

    public override string ToString()
    {
      return m_type + " (" + m_size + " bytes) - " +
          m_children.Length +
          (m_children.Length == 1 ?
          " child" :
          " children");
    }
  }

  #endregion

  #region ParsedLeafAtom class

  public class ParsedLeafAtom : ParsedAtom
  {
    public ParsedLeafAtom(long size, string type, Stream s)
      : base(size, type)
    {
      init(s);
    }

    protected virtual void init(Stream s)
    {
    }

    override public string ToString()
    {
      return m_type + " (" + m_size + " bytes) ";
    }
  }

  #endregion

  #region ParsedHdlrAtom class

  public class ParsedHdlrAtom : ParsedLeafAtom
  {
    protected int m_version;
    protected string m_componentType;
    protected string m_componentSubType;
    protected string m_componentManufacturer;
    protected string m_componentName;

    #region Properties


    public int Version
    {
      get { return m_version; }
    }

    public string ComponentType
    {
      get { return m_componentType; }
    }

    public string ComponentSubType
    {
      get { return m_componentSubType; }
    }

    public string ComponentManufacturer
    {
      get { return m_componentManufacturer; }
    }

    public string ComponentName
    {
      get { return m_componentName; }
    }

    #endregion

    public ParsedHdlrAtom(long size, string type, Stream s)
      : base(size, type, s)
    {
    }

    protected override void init(Stream s)
    {
      base.init(s);
      // hdlr contains a 1-byte version, 3 bytes of (unused) flags,
      // 4-char component type, 4-char component subtype,
      // 4-byte fields for comp mfgr, comp flags, and flag mask
      // then a pascal string for component name

      byte[] buffy = new byte[4];
      s.Read(buffy, 0, 1);
      m_version = buffy[0];
      // flags are defined as 3 bytes of 0, I just read & forget
      s.Read(buffy, 0, 3);
      // component type and subtype (4-byte strings)
      s.Read(buffy, 0, 4);
      m_componentType = Encoding.Default.GetString(buffy);
      s.Read(buffy, 0, 4);
      m_componentSubType = Encoding.Default.GetString(buffy);
      // component mfgr (4 bytes, apple says "reserved- set to 0")
      s.Read(buffy, 0, 4);
      m_componentManufacturer = Encoding.Default.GetString(buffy);
      // component flags & flag mask 
      // (4 bytes each, apple says "reserved- set to 0", skip for now)
      s.Read(buffy, 0, 4);
      s.Read(buffy, 0, 4);
      // length of pascal string
      s.Read(buffy, 0, 1);
      int compNameLen = buffy[0];
      /* undocumented hack:
         in .mp4 files (as opposed to .mov's), the component name
         seems to be a C-style (null-terminated) string rather
         than Pascal-style (length-byte then run of characters).
         However, the name is the last thing in this atom, so
         if the String size is wrong, assume we're in MPEG-4
         and just read to end of the atom.  In other words, the
         string length *must* always be atomSize - 33, since there
         are 33 bytes prior to the string, and it's the last thing
         in the atom.
      */
      if (compNameLen != (m_size - 33))
      {
        // MPEG-4 case
        compNameLen = (int)m_size - 33;
        // back up one byte (since what we thought was
        // length was actually first char of string)
        s.Seek(-1, SeekOrigin.Current);
      }
      byte[] compNameBuf = new byte[compNameLen];
      s.Read(compNameBuf, 0, compNameLen);
      m_componentName = Encoding.Default.GetString(compNameBuf);
    }
  }

  #endregion

  #region ParsedWlocAtom class

  public class ParsedWlocAtom : ParsedLeafAtom
  {
    protected int m_x;
    protected int m_y;

    #region Properties

    public int X
    {
      get { return m_x; }
    }

    public int Y
    {
      get { return m_y; }
    }

    #endregion

    public ParsedWlocAtom(long size, string type, Stream s)
      : base(size, type, s)
    {
    }

    override protected void init(Stream s)
    {
      // WLOC contains 16-bit x,y values
      byte[] value = new byte[4];
      s.Read(value, 0, value.Length);
      m_x = (value[0] << 8) | value[1];
      m_y = (value[2] << 8) | value[3];
    }
  }

  #endregion

  #region ParsedElstAtom class

  public class ParsedElstAtom : ParsedLeafAtom
  {
    #region Edit class

    public class Edit
    {
      long m_trackDuration;
      long m_mediaTime;
      float m_mediaRate;

      #region Properties

      public long TrackDuration
      {
        get { return m_trackDuration; }
      }

      public long MediaTime
      {
        get { return m_mediaTime; }
      }

      public float MediaRate
      {
        get { return m_mediaRate; }
      }

      #endregion

      public Edit(long d, long t, float r)
      {
        m_trackDuration = d;
        m_mediaTime = t;
        m_mediaRate = r;
      }
    }

    #endregion

    int m_version;
    Edit[] m_edits;

    #region Properties

    public int Version
    {
      get { return m_version; }
    }

    public Edit[] Edits
    {
      get { return m_edits; }
    }

    #endregion

    public ParsedElstAtom(long size, string type, Stream s)
      : base(size, type, s)
    {
    }

    override protected void init(Stream s)
    {
      byte[] buffy = new byte[4];
      s.Read(buffy, 0, 1);
      m_version = buffy[0];
      // flags are defined as 3 bytes of 0, I just read & forget
      s.Read(buffy, 0, 3);
      // how many table entries are there?
      s.Read(buffy, 0, 4);
      int tableCount = EndianBitConverter.Big.ToInt32(buffy, 0);
      m_edits = new Edit[tableCount];
      for (int i = 0; i < tableCount; i++)
      {
        // TODO: also bounds-check that we don't go past size
        // track duration
        s.Read(buffy, 0, 4);
        long trackDuration = EndianBitConverter.Big.ToUInt32(buffy, 0);
        // media time
        s.Read(buffy, 0, 4);
        long mediaTime = EndianBitConverter.Big.ToUInt32(buffy, 0);
        // media rate
        // TODO: wrong, these 4 bytes are a fixed-point
        // float, 16-bits left of decimal, 16 - right
        // I don't get how apple does this, so I'm just reading
        // the integer part
        s.Read(buffy, 0, 2);
        float mediaRate = EndianBitConverter.Big.ToInt16(buffy, 0);
        s.Read(buffy, 0, 2);
        // make an Edit object
        m_edits[i] = new Edit(trackDuration, mediaTime, mediaRate);
      }
    }
  }

  #endregion

  #region ParsedDataAtom class

  public class ParsedDataAtom : ParsedLeafAtom
  {
    byte[] m_data;

    #region Properties

    public byte[] Data
    {
      get { return m_data; }
    }

    #endregion

    public ParsedDataAtom(long size, string type, Stream s)
      : base(size, type, s)
    {
    }

    override protected void init(Stream s)
    {
      int tagSize = ((m_size > 0xffffffffL) ? Int32.MaxValue : (int)m_size) - 16;

      m_data = new byte[tagSize];
      s.Seek(8, SeekOrigin.Current);
      s.Read(m_data, 0, tagSize);
    }
  }

  #endregion

  #region ParsedMvhdAtom class

  public class ParsedMvhdAtom : ParsedLeafAtom
  {
    uint _TimeScale = 0;
    int _DurationMS = 0;

    #region Properties

    public int DurationMS
    {
      get { return _DurationMS; }
    }

    #endregion

    public ParsedMvhdAtom(long size, string type, Stream s)
      : base(size, type, s)
    {
    }

    override protected void init(Stream s)
    {
      byte[] buffer = new byte[4];
      s.Read(buffer, 0, 1);
      int version = buffer[0];

      // Read flags
      s.Read(buffer, 0, 3);

      bool useLongDateAndDurVals = version == 1;
      int dataLen = 4;

      if (useLongDateAndDurVals)
        dataLen = 8;

      buffer = new byte[dataLen];

      // Read create Date
      s.Read(buffer, 0, dataLen);

      // Read modified Date
      s.Read(buffer, 0, dataLen);

      // Read time scale
      buffer = new byte[4];
      s.Read(buffer, 0, 4);
      _TimeScale = EndianBitConverter.Big.ToUInt32(buffer, 0);

      // Read duration
      buffer = new byte[dataLen];
      s.Read(buffer, 0, dataLen);
      uint timeUnitsDuration = 0;

      if (useLongDateAndDurVals)
        timeUnitsDuration = (uint)EndianBitConverter.Big.ToUInt64(buffer, 0);

      else
        timeUnitsDuration = EndianBitConverter.Big.ToUInt32(buffer, 0);

      if (_TimeScale > 0 && timeUnitsDuration > 0)
        _DurationMS = (int)(((double)timeUnitsDuration / (double)_TimeScale) * 1000);
    }
  }

  #endregion

  #region ParsedStsdAtom class

  public class ParsedStsdAtom : ParsedLeafAtom
  {
    private byte[] _Data = null;
    private long _DataLength = 0;

    private int _MaximumBitRate = 0;
    private int _AverageBitRate = 0;

    #region Properties

    public int MaximumBitRate
    {
      get { return _MaximumBitRate; }
    }

    public int AverageBitRate
    {
      get { return _AverageBitRate; }
    }

    #endregion

    public ParsedStsdAtom(long size, string type, Stream s)
      : base(size, type, s)
    {
      _DataLength = size;
      init(s);
    }

    override protected void init(Stream s)
    {
      ////  -> 4 bytes version/flags = byte hex version + 24-bit hex flags
      ////      (current = 0)
      //// Ignore
      //byte[] buffer = new byte[4];
      //s.Read(0, buffer.Length);

      ////  -> 4 bytes number of descriptions = long unsigned total
      ////      (default = 1)
      //// Ignore
      //byte[] buffer = new byte[4];
      //s.Read(0, buffer.Length);

      ////  -> 4 bytes description length = long unsigned length
      //// Ignore
      //byte[] buffer = new byte[4];
      //s.Read(0, buffer.Length);

      ////  -> 4 bytes description visual format = long ASCII text string 'mp4v'
      ////    - if encoded to ISO/IEC 14496-10 or 3GPP AVC standards then use:
      //// Ignore
      //byte[] buffer = new byte[4];
      //s.Read(0, buffer.Length);

      ////  -> 4 bytes description visual format = long ASCII text string 'avc1'
      ////    - if encrypted to ISO/IEC 14496-12 or 3GPP standards then use:
      //// Ignore
      //byte[] buffer = new byte[4];
      //s.Read(0, buffer.Length);

      ////  -> 4 bytes description visual format = long ASCII text string 'encv'
      ////    - if encoded to 3GPP H.263v1 standards then use:
      //// Ignore
      //byte[] buffer = new byte[4];
      //s.Read(0, buffer.Length);

      ////  -> 4 bytes description visual format = long ASCII text string 's263'
      //// Ignore
      //byte[] buffer = new byte[4];
      //s.Read(0, buffer.Length);

      ////  -> 6 bytes reserved = 48-bit value set to zero
      //// Ignore
      //byte[] buffer = new byte[6];
      //s.Read(0, buffer.Length);

      ////  -> 2 bytes data reference index
      ////      = short unsigned index from 'dref' box
      ////    - there are other sample descriptions
      ////       available in the Apple QT format dev docs
      //// Ignore
      //byte[] buffer = new byte[2];
      //s.Read(0, buffer.Length);

      ////  -> 2 bytes QUICKTIME video encoding version = short hex version
      ////    - default = 0 ; audio data size before decompression = 1
      //// Ignore
      //byte[] buffer = new byte[2];
      //s.Read(0, buffer.Length);

      ////  -> 2 bytes QUICKTIME video encoding revision level = byte hex version
      ////    - default = 0 ; video can revise this value
      //// Ignore
      //byte[] buffer = new byte[2];
      //s.Read(0, buffer.Length);

      ////  -> 4 bytes QUICKTIME video encoding vendor = long ASCII text string
      ////    - default = 0
      //// Ignore
      //byte[] buffer = new byte[4];
      //s.Read(0, buffer.Length);

      ////  -> 4 bytes QUICKTIME video temporal quality = long unsigned value (0 to 1024)
      //// Ignore
      //byte[] buffer = new byte[4];
      //s.Read(0, buffer.Length);

      ////  -> 4 bytes QUICKTIME video spatial quality = long unsigned value (0 to 1024)
      ////    - some quality values are lossless = 1024 ; maximum = 1023 ; high = 768
      ////    - some quality values are normal = 512 ; low = 256 ; minimum = 0
      //// Ignore
      //byte[] buffer = new byte[4];
      //s.Read(0, buffer.Length);

      ////  -> 4 bytes video frame pixel size
      ////      = short unsigned width + short unsigned height
      //// Ignore
      //byte[] buffer = new byte[4];
      //s.Read(0, buffer.Length);

      ////  -> 8 bytes video resolution
      ////      = long fixed point horizontal + long fixed point vertical
      ////    - defaults to 72.0 dpi
      //// Ignore
      //byte[] buffer = new byte[8];
      //s.Read(0, buffer.Length);

      ////  -> 4 bytes QUICKTIME video data size = long value set to zero
      //// Ignore
      //byte[] buffer = new byte[4];
      //s.Read(0, buffer.Length);

      ////  -> 2 bytes video frame count = short unsigned total (set to 1)
      //// Ignore
      //byte[] buffer = new byte[2];
      //s.Read(0, buffer.Length);

      ////  -> 1 byte video encoding name string length = byte unsigned length
      //// Ignore
      //byte[] buffer = new byte[1];
      //s.Read(0, buffer.Length);

      ////  -> 31 bytes video encoder name string
      ////  -> NOTE: if video encoder name string < 31 chars then pad with zeros
      //// Ignore
      //byte[] buffer = new byte[31];
      //s.Read(0, buffer.Length);

      ////  -> 2 bytes video pixel depth = short unsigned bit depth
      ////    - colors are 1 (Monochrome), 2 (4), 4 (16), 8 (256)
      ////    - colors are 16 (1000s), 24 (Ms), 32 (Ms+A)
      ////    - grays are 33 (B/W), 34 (4), 36 (16), 40(256)
      //// Ignore
      //byte[] buffer = new byte[2];
      //s.Read(0, buffer.Length);

      ////  -> 2 bytes QUICKTIME video color table id = short integer value
      ////      (no table = -1)
      ////  -> optional QUICKTIME color table data if above set to 0
      ////      (see color table atom below for layout)
      ////  OR
      ////  -> 4 bytes description length = long unsigned length
      ////  -> 4 bytes description audio format = long ASCII text string 'mp4a'
      ////    - if encrypted to ISO/IEC 14496-12 or 3GPP standards then use:
      ////  -> 4 bytes description audio format = long ASCII text string 'enca'
      ////    - if encoded to 3GPP GSM 6.10 AMR narrowband standards then use:
      ////  -> 4 bytes description audio format = long ASCII text string 'samr'
      ////    - if encoded to 3GPP GSM 6.10 AMR wideband standards then use:
      ////  -> 4 bytes description audio format = long ASCII text string 'sawb'
      ////  -> 6 bytes reserved = 48-bit value set to zero
      ////  -> 2 bytes data reference index
      ////      = short unsigned index from 'dref' box
      ////  -> 2 bytes QUICKTIME audio encoding version = short hex version
      ////    - default = 0 ; audio data size before decompression = 1
      ////  -> 2 bytes QUICKTIME audio encoding revision level
      ////      = byte hex version
      ////    - default = 0 ; video can revise this value
      ////  -> 4 bytes QUICKTIME audio encoding vendor
      ////      = long ASCII text string
      ////    - default = 0
      ////  -> 2 bytes audio channels = short unsigned count
      ////      (mono = 1 ; stereo = 2)
      ////  -> 2 bytes audio sample size = short unsigned value
      ////      (8 or 16)
      ////  -> 2 bytes QUICKTIME audio compression id = short integer value
      ////    - default = 0
      ////  -> 2 bytes QUICKTIME audio packet size = short value set to zero
      ////  -> 4 bytes audio sample rate = long unsigned fixed point rate
      ////  OR
      ////  -> 4 bytes description length = long unsigned length
      ////  -> 4 bytes description system format = long ASCII text string 'mp4s'
      ////    - if encrypted to ISO/IEC 14496-12 standards then use:
      ////  -> 4 bytes description system format = long ASCII text string 'encs'
      ////  -> 6 bytes reserved = 48-bit value set to zero
      ////  -> 2 bytes data reference index
      ////      = short unsigned index from 'dref' box

      if (_DataLength <= 0)
        return;

      if (s == null || s.Position + _DataLength > s.Length)
        return;

      _Data = new byte[_DataLength];
      s.Read(_Data, 0, _Data.Length);

      // Look for the ESDS string

      int pos = 0;
      bool esdsFound = false;

      while (pos < _Data.Length - 4)
      {
        if (_Data[pos] == 'e' && _Data[pos + 1] == 's' && _Data[pos + 2] == 'd' && _Data[pos + 3] == 's')
        {
          esdsFound = true;
          pos += 4;
          break;
        }

        ++pos;
      }

      if (!esdsFound)
        return;

      MemoryStream ms = new MemoryStream(_Data, pos, _Data.Length - pos);
      byte[] buffer = new byte[4];

      //* 8+ bytes vers. 2 ES Descriptor box
      //          = long unsigned offset + long ASCII text string 'esds'
      //       - if encoded to ISO/IEC 14496-10 AVC standards then optionally use:
      //          = long unsigned offset + long ASCII text string 'm4ds'

      //        -> 4 bytes version/flags = 8-bit hex version + 24-bit hex flags
      //            (current = 0)
      ms.Read(buffer, 0, buffer.Length);

      //        -> 1 byte ES descriptor type tag = 8-bit hex value 0x03
      int esDescType = ms.ReadByte();

      //        -> 3 bytes extended descriptor type tag string = 3 * 8-bit hex value
      //          - types are Start = 0x80 ; End = 0xFE
      //          - NOTE: the extended start tags may be left out
      buffer = new byte[3];
      ms.Read(buffer, 0, buffer.Length);

      //        -> 1 byte descriptor type length = 8-bit unsigned length
      int descType = ms.ReadByte();

      //          -> 2 bytes ES ID = 16-bit unsigned value
      buffer = new byte[2];
      ms.Read(buffer, 0, buffer.Length);

      //          -> 1 byte stream priority = 8-bit unsigned value
      //            - Defaults to 16 and ranges from 0 through to 31
      int streamPrio = ms.ReadByte();

      //            -> 1 byte decoder config descriptor type tag = 8-bit hex value 0x04
      int decoderConfig = ms.ReadByte();

      //            -> 3 bytes extended descriptor type tag string = 3 * 8-bit hex value
      //              - types are Start = 0x80 ; End = 0xFE
      //              - NOTE: the extended start tags may be left out
      buffer = new byte[3];
      ms.Read(buffer, 0, buffer.Length);

      //            -> 1 byte descriptor type length = 8-bit unsigned length
      int descTypeLen = ms.ReadByte();

      //              -> 1 byte object type ID = 8-bit unsigned value
      //                - type IDs are system v1 = 1 ; system v2 = 2
      //                - type IDs are MPEG-4 video = 32 ; MPEG-4 AVC SPS = 33
      //                - type IDs are MPEG-4 AVC PPS = 34 ; MPEG-4 audio = 64
      //                - type IDs are MPEG-2 simple video = 96
      //                - type IDs are MPEG-2 main video = 97
      //                - type IDs are MPEG-2 SNR video = 98
      //                - type IDs are MPEG-2 spatial video = 99
      //                - type IDs are MPEG-2 high video = 100
      //                - type IDs are MPEG-2 4:2:2 video = 101
      //                - type IDs are MPEG-4 ADTS main = 102
      //                - type IDs are MPEG-4 ADTS Low Complexity = 103
      //                - type IDs are MPEG-4 ADTS Scalable Sampling Rate = 104
      //                - type IDs are MPEG-2 ADTS = 105 ; MPEG-1 video = 106
      //                - type IDs are MPEG-1 ADTS = 107 ; JPEG video = 108
      //                - type IDs are private audio = 192 ; private video = 208
      //                - type IDs are 16-bit PCM LE audio = 224 ; vorbis audio = 225
      //                - type IDs are dolby v3 (AC3) audio = 226 ; alaw audio = 227
      //                - type IDs are mulaw audio = 228 ; G723 ADPCM audio = 229
      //                - type IDs are 16-bit PCM Big Endian audio = 230
      //                - type IDs are Y'CbCr 4:2:0 (YV12) video = 240 ; H264 video = 241
      //                - type IDs are H263 video = 242 ; H261 video = 243
      int objTypeID = ms.ReadByte();

      //              -> 6 bits stream type = 3/4 byte hex value
      //                - type IDs are object descript. = 1 ; clock ref. = 2
      //                - type IDs are scene descript. = 4 ; visual = 4
      //                - type IDs are audio = 5 ; MPEG-7 = 6 ; IPMP = 7
      //                - type IDs are OCI = 8 ; MPEG Java = 9
      //                - type IDs are user private = 32
      //              -> 1 bit upstream flag = 1/8 byte hex value
      //              -> 1 bit reserved flag = 1/8 byte hex value set to 1
      int streamTypeFlags = ms.ReadByte();

      //              -> 3 bytes buffer size = 24-bit unsigned value
      buffer = new byte[3];
      ms.Read(buffer, 0, buffer.Length);

      //              -> 4 bytes maximum bit rate = 32-bit unsigned value
      buffer = new byte[4];
      ms.Read(buffer, 0, buffer.Length);
      _MaximumBitRate = (int)EndianBitConverter.Big.ToUInt32(buffer, 0);

      //              -> 4 bytes average bit rate = 32-bit unsigned value
      buffer = new byte[4];
      ms.Read(buffer, 0, buffer.Length);
      _AverageBitRate = (int)EndianBitConverter.Big.ToUInt32(buffer, 0);

      //                -> 1 byte decoder specific descriptor type tag
      //                    = 8-bit hex value 0x05
      int decoderDescType = ms.ReadByte();

      //                -> 3 bytes extended descriptor type tag string
      //                    = 3 * 8-bit hex value
      //                  - types are Start = 0x80 ; End = 0xFE
      //                  - NOTE: the extended start tags may be left out
      buffer = new byte[3];
      ms.Read(buffer, 0, buffer.Length);

      //                -> 1 byte descriptor type length
      //                    = 8-bit unsigned length
      //                  -> ES header start codes = hex dump

      //            -> 1 byte SL config descriptor type tag = 8-bit hex value 0x06


      //            -> 3 bytes extended descriptor type tag string = 3 * 8-bit hex value
      //              - types are Start = 0x80 ; End = 0xFE
      //              - NOTE: the extended start tags may be left out


      //            -> 1 byte descriptor type length = 8-bit unsigned length

      //              -> 1 byte SL value = 8-bit hex value set to 0x02


    }
  }

  #endregion

  #region AtomFactory class

  public class AtomFactory
  {
    public static ParsedAtom createAtomFor(long atomSize, string atomType, Stream s)
    {
      switch (atomType)
      {
        case "WLOC":
          return new ParsedWlocAtom(atomSize, atomType, s);

        case "ELST":
          return new ParsedElstAtom(atomSize, atomType, s);

        case "HDLR":
          return new ParsedHdlrAtom(atomSize, atomType, s);

        case "DATA":
          return new ParsedDataAtom(atomSize, atomType, s);

        case "MVHD":
          return new ParsedMvhdAtom(atomSize, atomType, s);

        case "STSD":
          return new ParsedStsdAtom(atomSize, atomType, s);

        default:
          return new ParsedLeafAtom(atomSize, atomType, s);
      }
    }
  }

  #endregion
}

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

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Tag.MAC
{
  public class APE_HEADER
  {
    public struct APE_FILE_INFO
    {
      public int nVersion;                                   // file version number * 1000 (3.93 = 3930)
      public int nCompressionLevel;                          // the compression level
      public int nFormatFlags;                               // format flags
      public int nTotalFrames;                               // the total number frames (frames are used internally)
      public int nBlocksPerFrame;                            // the samples in a frame (frames are used internally)
      public int nFinalFrameBlocks;                          // the number of samples in the final frame
      public int nChannels;                                  // audio channels
      public int nSampleRate;                                // audio samples per second
      public int nBitsPerSample;                             // audio bits per sample
      public int nBytesPerSample;                            // audio bytes per sample
      public int nBlockAlign;                                // audio block align (channels * bytes per sample)
      public int nWAVHeaderBytes;                            // header bytes of the original WAV
      public int nWAVDataBytes;                              // data bytes of the original WAV
      public int nWAVTerminatingBytes;                       // terminating bytes of the original WAV
      public int nWAVTotalBytes;                             // total bytes of the original WAV
      public int nAPETotalBytes;                             // total bytes of the APE file
      public int nTotalBlocks;                               // the total number audio blocks
      public int nLengthMS;                                  // the length in milliseconds
      public int nAverageBitrate;                            // the kbps (i.e. 637 kpbs)
      public int nDecompressedBitrate;                       // the kbps of the decompressed audio (i.e. 1440 kpbs for CD audio)
      public int nJunkHeaderBytes;                           // used for ID3v2, etc.
      public int nSeekTableElements;                         // the number of elements in the seek table(s)};

      public UInt32 spSeekByteTable;                         // the seek table (byte)
      public byte spSeekBitTable;                            // the seek table (bits -- legacy)
      public byte spWaveHeaderData;                          // the pre-audio header data
      public APE_DESCRIPTOR spAPEDescriptor;                 // the descriptor (only with newer files)
    };

    public struct APE_DESCRIPTOR
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
      public char[] cID;                                // should equal 'MAC '
      public UInt16 nVersion;                           // version number * 1000 (3.81 = 3810)

      public UInt32 nDescriptorBytes;                   // the number of descriptor bytes (allows later expansion of this header)
      public UInt32 nHeaderBytes;                       // the number of header APE_HEADER bytes
      public UInt32 nSeekTableBytes;                    // the number of bytes of the seek table
      public UInt32 nHeaderDataBytes;                   // the number of header data bytes (from original file)
      public UInt32 nAPEFrameDataBytes;                 // the number of bytes of APE frame data
      public UInt32 nAPEFrameDataBytesHigh;             // the high order number of APE frame data bytes
      public UInt32 nTerminatingDataBytes;              // the terminating data of the file (not including tag data)

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] cFileMD5;                            // the MD5 hash of the file (see notes for usage... it's a littly tricky)
    };

    public struct ApeHeader
    {
      public UInt16 nCompressionLevel;                 // the compression level (see defines I.E. COMPRESSION_LEVEL_FAST)
      public UInt16 nFormatFlags;                      // any format flags (for future use)

      public UInt32 nBlocksPerFrame;                   // the number of audio blocks in one frame
      public UInt32 nFinalFrameBlocks;                 // the number of audio blocks in the final frame
      public UInt32 nTotalFrames;                      // the total number of frames

      public UInt16 nBitsPerSample;                    // the bits per sample (typically 16)
      public UInt16 nChannels;                         // the number of channels (1 or 2)
      public UInt32 nSampleRate;                       // the sample rate (typically 44100)
    };

    public struct ApeCommonHeader
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
      public char[] cID;                            // should equal 'MAC '

      public UInt16 nVersion;                        // version number * 1000 (3.81 = 3810)
    };

    public APE_FILE_INFO ApeFileInfo = new APE_FILE_INFO();
    public ApeHeader Header = new ApeHeader();
    public ApeCommonHeader CommonHeader = new ApeCommonHeader();
    public APE_DESCRIPTOR ApeDescriptor = new APE_DESCRIPTOR();

    public APE_HEADER()
    {
      CommonHeader.cID = new char[4];
    }
  }

  public class APE_TAG_FOOTER
  {
    public struct ApeFooterData
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
      public char[] m_cID;              // should equal 'APETAGEX'    

      public int m_nVersion;             // equals CURRENT_APE_TAG_VERSION
      public int m_nSize;                // the complete size of the tag, including this footer (excludes header)
      public int m_nFields;              // the number of fields in the tag
      public int m_nFlags;               // the tag flags

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
      public char[] m_cReserved;        // reserved for later use (must be zero)
    };

    public ApeFooterData TagFooter = new ApeFooterData();

    public APE_TAG_FOOTER()
      : this(0, 0)
    {
    }

    public APE_TAG_FOOTER(int nFields, int nFieldBytes)
    {
      TagFooter.m_nFields = nFields;
      TagFooter.m_nFlags = ApeTagField.APE_TAG_FLAGS_DEFAULT;
      TagFooter.m_nSize = nFieldBytes + ApeTagField.APE_TAG_FOOTER_BYTES;
      TagFooter.m_nVersion = ApeTagField.CURRENT_APE_TAG_VERSION;
    }

    public int GetTotalTagBytes()
    {
      return TagFooter.m_nSize + (GetHasHeader() ? ApeTagField.APE_TAG_FOOTER_BYTES : 0);
    }

    public int GetFieldBytes()
    {
      return TagFooter.m_nSize - ApeTagField.APE_TAG_FOOTER_BYTES;
    }

    public int GetFieldsOffset()
    {
      return GetHasHeader() ? ApeTagField.APE_TAG_FOOTER_BYTES : 0;
    }

    public int GetNumberFields()
    {
      return TagFooter.m_nFields;
    }

    public bool GetHasHeader()
    {
      return (TagFooter.m_nFlags & ApeTagField.APE_TAG_FLAG_CONTAINS_HEADER) > 0 ? true : false;
    }

    public bool GetIsHeader()
    {
      return (TagFooter.m_nFlags & ApeTagField.APE_TAG_FLAG_IS_HEADER) > 0 ? true : false;
    }

    public int GetVersion()
    {
      return TagFooter.m_nVersion;
    }

    public bool GetIsValid(bool bAllowHeader)
    {
      string sID = new string(TagFooter.m_cID);

      bool bValid = (sID.CompareTo("APETAGEX") == 0)
          && (TagFooter.m_nVersion <= ApeTagField.CURRENT_APE_TAG_VERSION)
          && (TagFooter.m_nFields <= 65536)
          && (GetFieldBytes() <= (1024 * 1024 * 16));

      if (bValid && (bAllowHeader == false) && GetIsHeader())
        bValid = false;

      return bValid;
    }
  }

  public class ApeTagField : IDisposable
  {
    #region Consts

    // The version of the APE tag
    public const int CURRENT_APE_TAG_VERSION = 2000;

    // Footer (and header) flags
    public const int APE_TAG_FLAG_CONTAINS_HEADER = (1 << 31);
    public const int APE_TAG_FLAG_CONTAINS_FOOTER = (1 << 30);
    public const int APE_TAG_FLAG_IS_HEADER = (1 << 29);

    public const int APE_TAG_FLAGS_DEFAULT = (APE_TAG_FLAG_CONTAINS_FOOTER);

    // Tag field flags
    public const int TAG_FIELD_FLAG_READ_ONLY = (1 << 0);

    public const int TAG_FIELD_FLAG_DATA_TYPE_MASK = (6);
    public const int TAG_FIELD_FLAG_DATA_TYPE_TEXT_UTF8 = (0 << 1);
    public const int TAG_FIELD_FLAG_DATA_TYPE_BINARY = (1 << 1);
    public const int TAG_FIELD_FLAG_DATA_TYPE_EXTERNAL_INFO = (2 << 1);
    public const int TAG_FIELD_FLAG_DATA_TYPE_RESERVED = (3 << 1);

    // The footer at the end of APE tagged files (can also optionally be at the front of the tag)
    public const int APE_TAG_FOOTER_BYTES = 32;
    #endregion

    #region Variables

    private int _FieldFlags = -1;
    private string _FieldName = string.Empty;
    private byte[] _FieldValue = null;

    #endregion

    #region Properties

    public int FieldFlags
    {
      get { return _FieldFlags; }
    }

    public string FieldName
    {
      get { return _FieldName; }
    }

    public byte[] FieldValue
    {
      get { return _FieldValue; }
    }

    #endregion

    public ApeTagField(string fieldName, byte[] fieldValue, int nFieldBytes, int nFlags)
    {
      _FieldName = fieldName;
      _FieldValue = fieldValue;
      _FieldFlags = nFlags;
    }

    public void Dispose()
    {
    }

    public int GetFieldSize()
    {
      int fieldBytes = _FieldValue == null ? 0 : _FieldValue.Length;
      return _FieldName.Length + 1 + fieldBytes + 4 + 4;
    }

    public string GetFieldName()
    {
      return FieldName;
    }

    public byte[] GetFieldValue()
    {
      return this.FieldValue;
    }

    public int GetFieldValueSize()
    {
      return _FieldValue == null ? 0 : _FieldValue.Length;
    }

    public int GetFieldFlags()
    {
      return _FieldFlags;
    }

    public int SaveField(byte[] buffer)
    {
      return -1;
    }

    public bool GetIsReadOnly()
    {
      return (_FieldFlags & TAG_FIELD_FLAG_READ_ONLY) > 0 ? true : false;
    }

    public bool GetIsUTF8Text()
    {
      return ((_FieldFlags & TAG_FIELD_FLAG_DATA_TYPE_MASK) == TAG_FIELD_FLAG_DATA_TYPE_TEXT_UTF8) ? true : false;
    }
  }
}

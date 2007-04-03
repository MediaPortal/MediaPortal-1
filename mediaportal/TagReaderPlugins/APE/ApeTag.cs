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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.TagReader;

namespace Tag.MAC
{
  public class ApeTag : TagBase
  {
    #region Consts

    //APETag layout

    //1) Header - APE_TAG_FOOTER (optional) (32 bytes)
    //2) Fields (array):
    //        Value Size (4 bytes)
    //        Flags (4 bytes)
    //        Field Name (? ANSI bytes -- requires NULL terminator -- in range of 0x20 (space) to 0x7E (tilde))
    //        Value ([Value Size] bytes)
    //3) Footer - APE_TAG_FOOTER (32 bytes)    

    //Notes

    //-When saving images, store the filename (no directory -- i.e. Cover.jpg) in UTF-8 followed 
    //by a null terminator, followed by the image data.

    public const int CURRENT_APE_TAG_VERSION = 2000;

    //"Standard" APE tag fields
    public const string APE_TAG_FIELD_TITLE = "Title";
    public const string APE_TAG_FIELD_ARTIST = "Artist";
    public const string APE_TAG_FIELD_ALBUM = "Album";
    public const string APE_TAG_FIELD_COMMENT = "Comment";
    public const string APE_TAG_FIELD_YEAR = "Year";
    public const string APE_TAG_FIELD_TRACK = "Track";
    public const string APE_TAG_FIELD_GENRE = "Genre";
    public const string APE_TAG_FIELD_COVER_ART_FRONT = "Cover Art (front)";
    public const string APE_TAG_FIELD_NOTES = "Notes";
    public const string APE_TAG_FIELD_LYRICS = "Lyrics";
    public const string APE_TAG_FIELD_COPYRIGHT = "Copyright";
    public const string APE_TAG_FIELD_BUY_URL = "Buy URL";
    public const string APE_TAG_FIELD_ARTIST_URL = "Artist URL";
    public const string APE_TAG_FIELD_PUBLISHER_URL = "Publisher URL";
    public const string APE_TAG_FIELD_FILE_URL = "File URL";
    public const string APE_TAG_FIELD_COPYRIGHT_URL = "Copyright URL";
    public const string APE_TAG_FIELD_MJ_METADATA = "Media Jukebox Metadata";
    public const string APE_TAG_FIELD_TOOL_NAME = "Tool Name";
    public const string APE_TAG_FIELD_TOOL_VERSION = "Tool Version";
    public const string APE_TAG_FIELD_PEAK_LEVEL = "Peak Level";
    public const string APE_TAG_FIELD_REPLAY_GAIN_RADIO = "Replay Gain (radio)";
    public const string APE_TAG_FIELD_REPLAY_GAIN_ALBUM = "Replay Gain (album)";
    public const string APE_TAG_FIELD_COMPOSER = "Composer";
    public const string APE_TAG_FIELD_KEYWORDS = "Keywords";

    public const string APE_TAG_GENRE_UNDEFINED = "Undefined";

    public const int ID3_TAG_BYTES = 128;

    public struct ID3_TAG
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      public char[] Header; // should equal 'TAG'    

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
      public char[] Title; // should equal 'APETAGEX'    

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
      public char[] Artist; // artist    

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
      public char[] Album;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
      public char[] Year;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 29)]
      public char[] Comment;

      public byte Track; // track
      public byte Genre; // genre
    } ;

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

    protected List<ApeTagField> FieldList = new List<ApeTagField>();
    private int ApeTagBytes = -1;

    protected bool HasId3Tag = false;
    protected bool HasApeTag = false;
    protected int ApeTagVersion = -1;
    protected APE_HEADER ApeHeader = new APE_HEADER();
    protected long ApeTagStreamPosition = 0;
    private ID3_TAG ID3Tag = new ID3_TAG();

    #endregion

    #region Constructors/Destructors
    public ApeTag()
      : base()
    {
    }

    ~ApeTag()
    {
      Dispose();
    }
    #endregion

    #region Properties

    public override string Album
    {
      get
      {
        string val = GetFieldString(APE_TAG_FIELD_ALBUM);

        if (val.Length == 0)
        {
          return new string(ID3Tag.Album);
        }

        return val;
      }
    }

    public override string Artist
    {
      get
      {
        string val = GetFieldString(APE_TAG_FIELD_ARTIST);

        if (val.Length == 0)
        {
          return new string(ID3Tag.Artist);
        }

        return val;
      }
    }

    public override string AlbumArtist
    {
      get
      {
        string sVal = GetFieldString("album artist");

        if (sVal.Length == 0)
        {
          sVal = GetFieldString("albumartist");
        }

        return sVal;
      }
    }

    public override string ArtistURL
    {
      get { return GetFieldString(APE_TAG_FIELD_ARTIST_URL); }
    }

    public override int AverageBitrate
    {
      get { return ApeHeader.ApeFileInfo.nAverageBitrate; }
    }

    public override int BitsPerSample
    {
      get { return ApeHeader.ApeFileInfo.nBitsPerSample; }
    }

    public override int BlocksPerFrame
    {
      get { return ApeHeader.ApeFileInfo.nBlocksPerFrame; }
    }

    public override string BuyURL
    {
      get { return GetFieldString(APE_TAG_FIELD_BUY_URL); }
    }

    public override int BytesPerSample
    {
      get { return ApeHeader.ApeFileInfo.nBytesPerSample; }
    }

    public override int Channels
    {
      get { return ApeHeader.ApeFileInfo.nChannels; }
    }

    public override string Comment
    {
      get
      {
        string val = GetFieldString(APE_TAG_FIELD_COMMENT);

        if (val.Length == 0)
        {
          return new string(ID3Tag.Comment);
        }

        return val;
      }
    }

    public override string Composer
    {
      get { return GetFieldString(APE_TAG_FIELD_COMPOSER); }
    }

    public override int CompressionLevel
    {
      get { return ApeHeader.ApeFileInfo.nCompressionLevel; }
    }

    public override string Copyright
    {
      get { return GetFieldString(APE_TAG_FIELD_COPYRIGHT); }
    }

    public override string CopyrightURL
    {
      get { return GetFieldString(APE_TAG_FIELD_COPYRIGHT_URL); }
    }

    public override byte[] CoverArtImageBytes
    {
      get
      {
        try
        {
          byte[] tempBytes = GetFieldBinary(APE_TAG_FIELD_COVER_ART_FRONT);

          if (tempBytes == null)
          {
            return null;
          }

          string imgFileName = "";
          int i;
          bool foundFileName = false;

          for (i = 0; i < tempBytes.Length; i++)
          {
            char c = (char) tempBytes[i];

            if (c == 0)
            {
              foundFileName = i > 0;
              break;
            }

            imgFileName += c;
          }

          if (foundFileName && i < tempBytes.Length)
          {
            int imgBytesLen = tempBytes.Length - ++i;
            byte[] imgBytes = new byte[imgBytesLen];
            Buffer.BlockCopy(tempBytes, i, imgBytes, 0, imgBytesLen);

            return imgBytes;
          }
        }

        catch (Exception ex)
        {
          Log.Error("ApeTag.get_CoverArtImageBytes caused an exception in file {0} : {1}", FileName, ex.Message);
        }

        return null;
      }
    }

    public override string FileURL
    {
      get { return GetFieldString(APE_TAG_FIELD_FILE_URL); }
    }

    public override int FormatFlags
    {
      get { return ApeHeader.ApeFileInfo.nFormatFlags; }
    }

    public override bool IsVBR
    {
      get { return false; }
    }

    public override string Genre
    {
      get { return GetFieldString(APE_TAG_FIELD_GENRE); }
    }

    public override string Keywords
    {
      get { return GetFieldString(APE_TAG_FIELD_KEYWORDS); }
    }

    public override string Length
    {
      get { return Utils.GetDurationString(ApeHeader.ApeFileInfo.nLengthMS); }
    }

    public override int LengthMS
    {
      get { return ApeHeader.ApeFileInfo.nLengthMS; }
    }

    public override string Lyrics
    {
      get { return GetFieldString(APE_TAG_FIELD_LYRICS); }
    }

    public override string Notes
    {
      get { return GetFieldString(APE_TAG_FIELD_NOTES); }
    }

    public override string PeakLevel
    {
      get { return GetFieldString(APE_TAG_FIELD_PEAK_LEVEL); }
    }

    public override string PublisherURL
    {
      get { return GetFieldString(APE_TAG_FIELD_PUBLISHER_URL); }
    }

    public override int Rating
    {
      get { return base.Rating; }
    }

    public override string ReplayGainAlbum
    {
      get { return GetFieldString(APE_TAG_FIELD_REPLAY_GAIN_ALBUM); }
    }

    public override string ReplayGainRadio
    {
      get { return GetFieldString(APE_TAG_FIELD_REPLAY_GAIN_RADIO); }
    }

    public override int SampleRate
    {
      get { return ApeHeader.ApeFileInfo.nSampleRate; }
    }

    public override string Title
    {
      get
      {
        string val = GetFieldString(APE_TAG_FIELD_TITLE);

        if (val.Length == 0)
        {
          return new string(ID3Tag.Title);
        }

        return val;
      }
    }

    public override string ToolName
    {
      get { return GetFieldString(APE_TAG_FIELD_TOOL_NAME); }
    }

    public override string ToolVersion
    {
      get { return GetFieldString(APE_TAG_FIELD_TOOL_VERSION); }
    }

    public override int TotalBlocks
    {
      get { return ApeHeader.ApeFileInfo.nTotalBlocks; }
    }

    public override int TotalFrames
    {
      get { return ApeHeader.ApeFileInfo.nTotalFrames; }
    }

    public override int Track
    {
      get { return GetFieldInt(APE_TAG_FIELD_TRACK); }
    }

    public override string Version
    {
      get
      {
        if (ApeHeader.ApeFileInfo.nVersion > 0)
        {
          return string.Format("{0}", ApeHeader.ApeFileInfo.nVersion/1000f);
        }

        else
        {
          return string.Empty;
        }
      }
    }

    public override int Year
    {
      get
      {
        try
        {
          string sYear = GetFieldString(APE_TAG_FIELD_YEAR);
          int year = Utils.GetYear(sYear);

          if (year == 0)
          {
            year = Utils.GetYear(new string(ID3Tag.Year));
          }

          return year;
        }

        catch (Exception ex)
        {
          Log.Error("    ApeTag.get_Year caused an exception in file {0} : {1}", FileName, ex.Message);
          return 0;
        }
      }
    }

    #endregion

    #region Public Methods
    public override bool SupportsFile(string strFileName)
    {
      if (Path.GetExtension(strFileName).ToLower() == ".ape")
      {
        return true;
      }
      return false;
    }

    public override bool Read(string fileName)
    {
      if (fileName.Length == 0)
      {
        throw new Exception("No file name specified");
      }

      if (!File.Exists(fileName))
      {
        throw new Exception("Unable to open file.  File does not exist.");
      }

      if (Path.GetExtension(fileName).ToLower() != ".ape")
      {
        throw new AudioFileTypeException("Expected APE file type.");
      }

      base.Read(fileName);
      bool result = ReadTags();

      if (!result)
      {
        return false;
      }

      result = ReadHeader();
      AudioDataStartPostion = AudioFileStream.Position;

      return result && (FieldList.Count > 0 || HasId3Tag);
    }

    protected bool ReadTags()
    {
      bool result = true;

      try
      {
        if (AudioFileStream == null)
        {
          AudioFileStream = new FileStream(AudioFilePath, FileMode.Open, FileAccess.Read);
        }

        long nOriginalPosition = AudioFileStream.Position;

        // check for a tag
        int nBytesRead;
        HasId3Tag = false;
        HasApeTag = false;
        ApeTagVersion = -1;
        AudioFileStream.Seek(-ID3_TAG_BYTES, SeekOrigin.End);

        byte[] id3TagBytes = Utils.RawSerializeEx(ID3Tag);
        int ID3TagSize = Marshal.SizeOf(ID3Tag);
        nBytesRead = AudioFileStream.Read(id3TagBytes, 0, ID3TagSize);

        ID3Tag = (ID3_TAG) Utils.RawDeserializeEx(id3TagBytes, typeof(ID3_TAG));

        if (nBytesRead == ID3TagSize && nBytesRead == id3TagBytes.Length)
        {
          if (ID3Tag.Header[0] == 'T' && ID3Tag.Header[1] == 'A' && ID3Tag.Header[2] == 'G')
          {
            HasId3Tag = true;
            ApeTagBytes += ID3_TAG_BYTES;
          }
        }

        if (!HasId3Tag)
        {
          // Clear the invalid data we read in earlier
          ID3Tag = new ID3_TAG();

          APE_TAG_FOOTER APETagFooter = new APE_TAG_FOOTER();
          AudioFileStream.Seek(-ApeTagField.APE_TAG_FOOTER_BYTES, SeekOrigin.End);

          byte[] appeTagFooterBytes = Utils.RawSerializeEx(APETagFooter.TagFooter);
          nBytesRead = AudioFileStream.Read(appeTagFooterBytes, 0, APE_TAG_FOOTER_BYTES);

          if (nBytesRead == APE_TAG_FOOTER_BYTES && nBytesRead == appeTagFooterBytes.Length)
          {
            APETagFooter.TagFooter =
              (APE_TAG_FOOTER.ApeFooterData)
              Utils.RawDeserializeEx(appeTagFooterBytes, typeof(APE_TAG_FOOTER.ApeFooterData));

            if (APETagFooter.GetIsValid(false))
            {
              ApeTagStreamPosition = AudioFileStream.Position - 4;
              HasApeTag = true;
              ApeTagVersion = APETagFooter.GetVersion();

              int nRawFieldBytes = APETagFooter.GetFieldBytes();
              ApeTagBytes += APETagFooter.GetTotalTagBytes();

              byte[] spRawTag = new byte[nRawFieldBytes];
              AudioFileStream.Seek(-(APETagFooter.GetTotalTagBytes() - APETagFooter.GetFieldsOffset()), SeekOrigin.End);
              nBytesRead = AudioFileStream.Read(spRawTag, 0, nRawFieldBytes);

              if (nRawFieldBytes == nBytesRead)
              {
                // parse out the raw fields
                int nLocation = 0;
                for (int z = 0; z < APETagFooter.GetNumberFields(); z++)
                {
                  int nMaximumFieldBytes = nRawFieldBytes - nLocation;

                  int nBytes = 0;
                  byte[] tempBytes = new byte[nMaximumFieldBytes];
                  Array.Copy(spRawTag, nLocation, tempBytes, 0, nMaximumFieldBytes);

                  if (!LoadField(tempBytes, nMaximumFieldBytes, ref nBytes))
                  {
                    // if LoadField(...) fails, it means that the tag is corrupt (accidently or intentionally)
                    // we'll just bail out -- leaving the fields we've already set
                    break;
                  }

                  nLocation += nBytes;
                }
              }
            }
          }
        }

        AudioFileStream.Seek(nOriginalPosition, SeekOrigin.Begin);
      }

      catch (Exception ex)
      {
        Log.Error("ApeTag.ReadTags cause an exception in file {0} : {1}", FileName, ex.Message);
        result = false;
      }

      return result;
    }

    protected bool HasApeV1Tags()
    {
      long currentStreamPosition = AudioFileStream.Position;
      byte[] buffer = new byte[8];
      AudioFileStream.Read(buffer, 0, 8);

      bool hasV1Tag = (buffer[0] == 'A' && buffer[1] == 'P' && buffer[2] == 'E' && buffer[3] == 'T'
                       && buffer[4] == 'A' && buffer[5] == 'G' && buffer[6] == 'E' && buffer[7] == 'X');

      AudioFileStream.Position = currentStreamPosition;
      return hasV1Tag;
    }

    protected bool ReadHeader()
    {
      // find the descriptor
      ApeHeader.ApeFileInfo.nJunkHeaderBytes = FindDescriptor(true);

      if (ApeHeader.ApeFileInfo.nJunkHeaderBytes < 0)
      {
        return false;
      }

      // read the first 8 bytes of the descriptor (ID and version)
      byte[] commonHdrBytes = Utils.RawSerializeEx(ApeHeader.CommonHeader);

      AudioFileStream.Read(commonHdrBytes, 0, commonHdrBytes.Length);
      ApeHeader.CommonHeader =
        (APE_HEADER.ApeCommonHeader) Utils.RawDeserializeEx(commonHdrBytes, typeof(APE_HEADER.ApeCommonHeader));

      // make sure we're at the ID
      if (ApeHeader.CommonHeader.cID[0] != 'M' || ApeHeader.CommonHeader.cID[1] != 'A' ||
          ApeHeader.CommonHeader.cID[2] != 'C' || ApeHeader.CommonHeader.cID[3] != ' ')
      {
        return false;
      }

      bool result;

      if (ApeHeader.CommonHeader.nVersion >= 3980)
      {
        // current header format
        result = ReadHeaderCurrent(ref ApeHeader.ApeFileInfo);
      }
      else
      {
        // legacy support
        result = ReadHeaderOld(ref ApeHeader.ApeFileInfo);
      }

      return result;
    }

    protected bool ReadHeaderCurrent(ref APE_HEADER.APE_FILE_INFO pInfo)
    {
      int nBytesRead;

      // read the descriptor
      AudioFileStream.Seek(pInfo.nJunkHeaderBytes, SeekOrigin.Begin);
      byte[] apeDescriptorBytes = Utils.RawSerializeEx(pInfo.spAPEDescriptor);

      nBytesRead = AudioFileStream.Read(apeDescriptorBytes, 0, apeDescriptorBytes.Length);
      pInfo.spAPEDescriptor =
        (APE_HEADER.APE_DESCRIPTOR) Utils.RawDeserializeEx(apeDescriptorBytes, typeof(APE_HEADER.APE_DESCRIPTOR));

      if ((pInfo.spAPEDescriptor.nDescriptorBytes - nBytesRead) > 0)
      {
        AudioFileStream.Seek(pInfo.spAPEDescriptor.nDescriptorBytes - nBytesRead, SeekOrigin.Current);
      }

      // read the header
      byte[] headerBytes = Utils.RawSerializeEx(ApeHeader.Header);
      nBytesRead = AudioFileStream.Read(headerBytes, 0, headerBytes.Length);
      ApeHeader.Header = (APE_HEADER.ApeHeader) Utils.RawDeserializeEx(headerBytes, typeof(APE_HEADER.ApeHeader));

      if ((pInfo.spAPEDescriptor.nHeaderBytes - nBytesRead) > 0)
      {
        AudioFileStream.Seek(pInfo.spAPEDescriptor.nHeaderBytes - nBytesRead, SeekOrigin.Current);
      }

      // fill the APE info structure
      pInfo.nVersion = pInfo.spAPEDescriptor.nVersion;
      pInfo.nCompressionLevel = ApeHeader.Header.nCompressionLevel;
      pInfo.nFormatFlags = ApeHeader.Header.nFormatFlags;
      pInfo.nTotalFrames = (int) (ApeHeader.Header.nTotalFrames);
      pInfo.nFinalFrameBlocks = (int) (ApeHeader.Header.nFinalFrameBlocks);
      pInfo.nBlocksPerFrame = (int) (ApeHeader.Header.nBlocksPerFrame);
      pInfo.nChannels = ApeHeader.Header.nChannels;
      pInfo.nSampleRate = (int) (ApeHeader.Header.nSampleRate);
      pInfo.nBitsPerSample = ApeHeader.Header.nBitsPerSample;
      pInfo.nBytesPerSample = pInfo.nBitsPerSample/8;
      pInfo.nBlockAlign = pInfo.nBytesPerSample*pInfo.nChannels;
      pInfo.nTotalBlocks = (ApeHeader.Header.nTotalFrames == 0)
                             ? 0
                             :
                           (int)
                           (((ApeHeader.Header.nTotalFrames - 1)*pInfo.nBlocksPerFrame) +
                            ApeHeader.Header.nFinalFrameBlocks);
      //pInfo.nWAVHeaderBytes = (ApeHeader.Header.nFormatFlags & MAC_FORMAT_FLAG_CREATE_WAV_HEADER) ? sizeof(WAVE_HEADER) : pInfo.spAPEDescriptor.nHeaderDataBytes;
      pInfo.nWAVTerminatingBytes = (int) pInfo.spAPEDescriptor.nTerminatingDataBytes;
      pInfo.nWAVDataBytes = pInfo.nTotalBlocks*pInfo.nBlockAlign;
      pInfo.nWAVTotalBytes = pInfo.nWAVDataBytes + pInfo.nWAVHeaderBytes + pInfo.nWAVTerminatingBytes;
      pInfo.nAPETotalBytes = (int) AudioFileStream.Length;
      pInfo.nLengthMS = (int) (((double) (pInfo.nTotalBlocks)*(double) (1000))/pInfo.nSampleRate);
      pInfo.nAverageBitrate = (pInfo.nLengthMS <= 0)
                                ? 0
                                : (int) (((double) (pInfo.nAPETotalBytes)*(double) (8))/pInfo.nLengthMS);
      pInfo.nDecompressedBitrate = (pInfo.nBlockAlign*pInfo.nSampleRate*8)/1000;
      pInfo.nSeekTableElements = (int) pInfo.spAPEDescriptor.nSeekTableBytes/4;

      return true;
    }

    protected bool ReadHeaderOld(ref APE_HEADER.APE_FILE_INFO pInfo)
    {
      // Not currently implemented.  We should fix this though!
      Log.Debug("    ApeTag.ReadHeaderOld not implemented!");
      return false;
    }

    protected int FindDescriptor(bool bSeek)
    {
      // store the original location and seek to the beginning
      long nOriginalFileLocation = AudioFileStream.Position;
      AudioFileStream.Seek(0, SeekOrigin.Begin);

      // set the default junk bytes to 0
      int nJunkBytes = 0;

      // skip an ID3v2 tag (which we really don't support anyway...)
      int nBytesRead;
      byte[] cID3v2Header = new byte[10];

      AudioFileStream.Read(cID3v2Header, 0, 10);
      if (cID3v2Header[0] == 'I' && cID3v2Header[1] == 'D' && cID3v2Header[2] == '3')
      {
        //// why is it so hard to figure the lenght of an ID3v2 tag ?!?
        //uint nLength = cID3v2Header[6];  JoeDalton: unused

        uint nSyncSafeLength;
        nSyncSafeLength = (uint) (cID3v2Header[6] & 127) << 21;
        nSyncSafeLength += (uint) (cID3v2Header[7] & 127) << 14;
        nSyncSafeLength += (uint) (cID3v2Header[8] & 127) << 7;
        nSyncSafeLength += (uint) (cID3v2Header[9] & 127);

        bool bHasTagFooter = false;

        if ((cID3v2Header[5] & 16) > 0)
        {
          bHasTagFooter = true;
          nJunkBytes = (int) nSyncSafeLength + 20;
        }
        else
        {
          nJunkBytes = (int) nSyncSafeLength + 10;
        }

        // error check
        if ((cID3v2Header[5] & 64) > 0)
        {
          // this ID3v2 length calculator algorithm can't cope with extended headers
          // we should be ok though, because the scan for the MAC header below should
          // really do the trick
        }

        AudioFileStream.Seek(nJunkBytes, SeekOrigin.Begin);

        // scan for padding (slow and stupid, but who cares here...)
        if (!bHasTagFooter)
        {
          byte[] cTemp = new byte[1];
          cTemp[0] = 0;
          nBytesRead = AudioFileStream.Read(cTemp, 0, 1);

          while (cTemp[0] == 0 && nBytesRead == 1)
          {
            nJunkBytes++;
            nBytesRead = AudioFileStream.Read(cTemp, 0, 1);
          }
        }
      }

      AudioFileStream.Seek(nJunkBytes, SeekOrigin.Begin);

      uint nGoalID = (' ' << 24) | ('C' << 16) | ('A' << 8) | ('M');
      byte[] readIDBytes = new byte[4];
      nBytesRead = AudioFileStream.Read(readIDBytes, 0, 4);
      int nReadID = BitConverter.ToInt32(readIDBytes, 0);

      if (nBytesRead != 4)
      {
        return -1;
      }

      nBytesRead = 1;

      int nScanBytes = 0;

      while ((nGoalID != nReadID) && (nBytesRead == 1) && (nScanBytes < (1024*1024)))
      {
        byte[] cTempBytes = new byte[1];
        nBytesRead = AudioFileStream.Read(cTempBytes, 0, 1);
        int cTemp = BitConverter.ToInt32(cTempBytes, 0);

        nReadID = (cTemp << 24) | (nReadID >> 8);
        nJunkBytes++;
        nScanBytes++;
      }

      if (nGoalID != nReadID)
      {
        nJunkBytes = -1;
      }

      // seek to the proper place (depending on result and settings)
      if (bSeek && (nJunkBytes != -1))
      {
        // successfully found the start of the file (seek to it and return)
        AudioFileStream.Seek(nJunkBytes, SeekOrigin.Begin);
      }
      else
      {
        // restore the original file pointer
        AudioFileStream.Seek(nOriginalFileLocation, SeekOrigin.Begin);
      }

      return nJunkBytes;
    }


    protected bool ClearFields()
    {
      bool result = true;

      try
      {
        foreach (ApeTagField field in FieldList)
        {
          field.Dispose();
        }
      }

      catch
      {
        result = false;
      }

      return result;
    }

    protected int GetTagBytes()
    {
      return ApeTagBytes;
    }

    protected ApeTagField GetTagField(int nIndex)
    {
      if ((nIndex >= 0) && (nIndex < FieldList.Count))
      {
        return FieldList[nIndex];
      }

      return null;
    }

    protected bool LoadField(byte[] pBuffer, int nMaximumBytes, ref int pBytes)
    {
      pBytes = 0;

      MemoryStream ms = new MemoryStream(pBuffer);
      byte[] buffer = new byte[4];
      ms.Read(buffer, 0, 4);
      int fieldSize = BitConverter.ToInt32(buffer, 0);

      buffer = new byte[4];
      ms.Read(buffer, 0, 4);
      int fieldFlags = BitConverter.ToInt32(buffer, 0);
      int nameLength;

      for (nameLength = 0; nameLength < pBuffer.Length; nameLength++)
      {
        if (pBuffer[ms.Position + nameLength] == 0)
        {
          break;
        }
      }

      if (nameLength >= pBuffer.Length)
      {
        return false;
      }

      buffer = new byte[nameLength];
      ms.Read(buffer, 0, buffer.Length);
      string fieldName = Encoding.UTF8.GetString(buffer);

      ms.Position++;

      buffer = new byte[fieldSize];
      ms.Read(buffer, 0, buffer.Length);

      SetFieldBinary(fieldName, buffer, fieldSize, fieldFlags);
      pBytes = (int) ms.Position;
      return true;
    }

    protected bool SetFieldBinary(string pFieldName, byte[] pFieldValue, int nFieldBytes, int nFieldFlags)
    {
      //Console.WriteLine(pFieldName);
      if (pFieldName.Length == 0)
      {
        return false;
      }

      //// check to see if we're trying to remove the field (by setting it to NULL or an empty string)
      //bool bRemoving = (pFieldValue == null) || (nFieldBytes <= 0);  JoeDalton: not used

      //JoeDalton: not used
      // get the index
      //int nFieldIndex = GetTagFieldIndex(pFieldName);
      //if (nFieldIndex != -1)
      //{
      //  nFieldIndex = FieldList.Count;
      //}

      // create the field and add it to the field array
      FieldList.Add(new ApeTagField(pFieldName, pFieldValue, nFieldBytes, nFieldFlags));
      return true;
    }

    protected int GetTagFieldIndex(string pFieldName)
    {
      if (pFieldName == null)
      {
        return -1;
      }

      for (int z = 0; z < FieldList.Count; z++)
      {
        if (FieldList[z].GetFieldName().CompareTo(pFieldName) == 0)
        {
          return z;
        }
      }

      return -1;
    }

    protected ApeTagField GetTagField(string sFieldName)
    {
      sFieldName = sFieldName.ToLower();

      for (int i = 0; i < FieldList.Count; i++)
      {
        ApeTagField field = FieldList[i];

        if (field.GetFieldName().ToLower().CompareTo(sFieldName) == 0)
        {
          return field;
        }
      }

      return null;
    }

    protected string GetFieldString(string sFieldName)
    {
      ApeTagField field = GetTagField(sFieldName);

      if (field == null)
      {
        return string.Empty;
      }

      byte[] val = field.FieldValue;

      if (val == null)
      {
        return string.Empty;
      }

      return Encoding.UTF8.GetString(val);
    }

    protected string GetFieldStringSearch(string sPartialFieldName)
    {
      if (sPartialFieldName.Length < 5)
      {
        return string.Empty;
      }

      sPartialFieldName = sPartialFieldName.ToLower();

      foreach (ApeTagField tagField in FieldList)
      {
        if (tagField.FieldName.ToLower().IndexOf(sPartialFieldName) != -1)
        {
          byte[] val = tagField.FieldValue;

          if (val == null)
          {
            return string.Empty;
          }

          return Encoding.UTF8.GetString(val);
        }
      }

      return string.Empty;
    }

    protected int GetFieldInt(string sFieldName)
    {
      string sVal = GetFieldString(sFieldName);

      if (sVal.Length == 0)
      {
        return 0;
      }

      try
      {
        return int.Parse(sVal);
      }

      catch
      {
        return 0;
      }
    }

    protected byte[] GetFieldBinary(string sFieldName)
    {
      ApeTagField field = GetTagField(sFieldName);

      if (field == null)
      {
        return null;
      }

      return field.FieldValue;
    }
    #endregion
  }
}
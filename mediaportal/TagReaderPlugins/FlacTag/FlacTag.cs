#region Copyright (C) 2005-2006 Team MediaPortal
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
using System.IO;
using System.Text;
using ID3;
using MediaPortal.GUI.Library;
using MediaPortal.TagReader;
using Tag.MAC;
using Tag.OGG;

namespace Tag.FLAC
{
  public class FlacTag : TagBase
  {
    #region Enums
    private enum BlockType
    {
      StreamInfo = 0,
      Padding = 1,
      Application = 2,
      SeekTable = 3,
      VorbisComment = 4,
      CueSheet = 5,
      //6-126 : reserved 
      Invalid = 127,
    } ;

    private struct StreamInfoBlock
    {
      public long StreamPosition;
      public bool LastMetadataBlock;
      public int MetadataLength;
      public int MinimumBlockSamples;
      public int MaximumBlockSamples;
      public int MinimumFrameSize;
      public int MaximumFrameSize;
      public int SampleRate;
      public byte Channels;
      public int BitsPerSample;
      public UInt64 TotalSamples;
      public byte[] Md5Signature;
      
      public static StreamInfoBlock Empty
      {
        get
        {
          StreamInfoBlock info = new StreamInfoBlock();
          info.StreamPosition = 0;
          info.LastMetadataBlock = false;
          info.MetadataLength = 0;
          info.MinimumBlockSamples = 0;
          info.MaximumBlockSamples = 0;
          info.MinimumFrameSize = 0;
          info.MaximumFrameSize = 0;
          info.SampleRate = 0;
          info.Channels = 0;
          info.BitsPerSample = 0;
          info.TotalSamples = 0;
          info.Md5Signature = new byte[0];
          return info;
        }
      }
    } 

    private struct PaddingInfoBlock
    {
      public long StreamPosition;
      public bool LastMetadataBlock;
      public int MetadataLength;
    }

    private struct ApplicationInfoBlock
    {
      public long StreamPosition;
      public bool LastMetadataBlock;
      public int MetadataLength;
      public UInt32 ApplicationID;
      public byte[] ApplicationData;
    }

    private struct VorbisCommentInfoBlock
    {
      public long StreamPosition;
      public bool LastMetadataBlock;
      public int MetadataLength;
      public int CommentsLength;

      public static VorbisCommentInfoBlock Empty
      {
        get
        {
          VorbisCommentInfoBlock info = new VorbisCommentInfoBlock();
          info.StreamPosition = 0;
          info.LastMetadataBlock = false;
          info.MetadataLength = 0;
          info.CommentsLength = 0;
          return info;
        }
      }
    }

    private struct CueSheetInfoBlock
    {
      public long StreamPosition;
      public bool LastMetadataBlock;
      public int MetadataLength;
    }
    #endregion

    #region Constants

    private const string TagPrefix = "reference libFLAC";

    #endregion

    #region Variables

    private List<VorbisComment> CommentList = new List<VorbisComment>();
    private APE_HEADER ApeHeader = new APE_HEADER();

    private StreamInfoBlock StreamInfo = new StreamInfoBlock();
    private PaddingInfoBlock PaddingBlock = new PaddingInfoBlock();
    private ApplicationInfoBlock ApplicationInfo = new ApplicationInfoBlock();
    private VorbisCommentInfoBlock VorbisCommentsInfo = new VorbisCommentInfoBlock();
    private CueSheetInfoBlock CueSheetInfo = new CueSheetInfoBlock();

    #endregion

    #region Constructors/Destructors
    public FlacTag()
      : base()
    {
    }

    ~FlacTag()
    {
      Dispose();
    }
    #endregion

    #region Properties

    public override string Album
    {
      get { return GetStringCommentValue("ALBUM"); }
    }

    public override string Artist
    {
      get { return GetStringCommentValue("ARTIST"); }
    }

    public override string AlbumArtist
    {
      get { return GetStringCommentValue("ALBUMARTIST"); }
    }

    public override string ArtistURL
    {
      get { return ""; }
    }

    public override int AverageBitrate
    {
      get
      {
        long AudioDataLength = FileLength - AudioDataStartPostion;
        long bitrate = (((AudioDataLength*8000)/LengthMS) + 500)/1000;
        return (int) bitrate;
      }
    }

    public override int BitsPerSample
    {
      get { return StreamInfo.BitsPerSample; }
    }

    public override int BlocksPerFrame
    {
      get { return StreamInfo.MaximumBlockSamples; }
    }

    public override string BuyURL
    {
      get { return base.BuyURL; }
    }

    public override int BytesPerSample
    {
      get { return StreamInfo.BitsPerSample; }
    }

    public override int Channels
    {
      get { return (int) StreamInfo.Channels; }
    }

    public override string Comment
    {
      get { return base.Comment; }
    }

    public override string Composer
    {
      get { return base.Composer; }
    }

    public override int CompressionLevel
    {
      get { return base.CompressionLevel; }
    }

    public override string Copyright
    {
      get { return base.Copyright; }
    }

    public override string CopyrightURL
    {
      get { return base.CopyrightURL; }
    }

    public override byte[] CoverArtImageBytes
    {
      get { return GetBase64StringCommentValue("COVERART"); }
    }

    public override string FileURL
    {
      get { return base.FileURL; }
    }

    public override int FormatFlags
    {
      get { return base.FormatFlags; }
    }

    public override string Genre
    {
      get { return GetStringCommentValue("GENRE"); }
    }

    public override bool IsVBR
    {
      get { return base.IsVBR; }
    }

    public override string Keywords
    {
      get { return base.Keywords; }
    }

    public override string Length
    {
      get { return Utils.GetDurationString(LengthMS); }
    }

    public override int LengthMS
    {
      get { return (int) ((StreamInfo.TotalSamples/(UInt64) StreamInfo.SampleRate)*1000); }
    }

    public override string Lyrics
    {
      get { return GetStringCommentValue("LYRICS"); }
    }

    public override string Notes
    {
      get { return base.Notes; }
    }

    public override string PeakLevel
    {
      get { return base.PeakLevel; }
    }

    public override string PublisherURL
    {
      get { return base.PublisherURL; }
    }

    public override string ReplayGainAlbum
    {
      get { return base.ReplayGainAlbum; }
    }

    public override string ReplayGainRadio
    {
      get { return base.ReplayGainRadio; }
    }

    public override int SampleRate
    {
      get { return StreamInfo.SampleRate; }
    }

    public override string Title
    {
      get { return GetStringCommentValue("TITLE"); }
    }

    public override string ToolName
    {
      get { return base.ToolName; }
    }

    public override string ToolVersion
    {
      get { return base.ToolVersion; }
    }

    public override int TotalBlocks
    {
      get { return base.TotalBlocks; }
    }

    public override int TotalFrames
    {
      get { return base.TotalFrames; }
    }

    public override int Track
    {
      get
      {
        try
        {
          string sTrack = GetStringCommentValue("TRACKNUMBER");

          if (sTrack.Length > 0)
          {
            return int.Parse(sTrack);
          }
        }

        catch (Exception ex)
        {
          Log.Error("FlacTag.get_Track caused an exception in file {0} : {1}", base.FileName, ex.Message);
        }

        return 0;
      }
    }

    public override string Version
    {
      get { return base.Version; }
    }

    public override int Year
    {
      get
      {
        try
        {
          string sYear = GetStringCommentValue("DATE");
          return Utils.GetYear(sYear);
        }

        catch (Exception ex)
        {
          Log.Error("FlacTag.get_Year caused an exception in file {0} : {1}", base.FileName, ex.Message);
          return 0;
        }
      }
    }

    #endregion

    #region Public Methods
    public override bool SupportsFile(string strFileName)
    {
      if (Path.GetExtension(strFileName).ToLower() == ".flac")
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

      if (Path.GetExtension(fileName).ToLower() != ".flac")
      {
        throw new AudioFileTypeException("Expected FLAC file type.");
      }

      base.Read(fileName);
      bool result = true;

      try
      {
        if (AudioFileStream == null)
        {
          AudioFileStream = new FileStream(AudioFilePath, FileMode.Open, FileAccess.Read);
        }

        if (!IsFlacFile())
        {
          throw new AudioFileTypeException("Invalid Flac file");
        }

        int metadataBlocksRead = 0;

        // Read all of the Metadata blocks
        while (true)
        {
          // ReadMetadataBlocks will return false once we've read the last block
          if (!ReadMetadataBlocks(ref metadataBlocksRead))
          {
            break;
          }
        }

        // Did we read at least one block
        if (metadataBlocksRead > 0)
        {
          AudioDataStartPostion = AudioFileStream.Position;
        }

        AudioDataStartPostion = AudioFileStream.Position;
      }

      catch (Exception ex)
      {
        Log.Error("FlacTag.Read cause an exception in file {0} : {1}", base.FileName, ex.Message);
        result = false;
      }

      return result;
    }
    #endregion

    #region Private Methods
    private bool IsFlacFile()
    {
      AudioFileStream.Seek(0, SeekOrigin.Begin);

      // It's possible, though rare, that the beginning of the file could contain and ID3V2.x tag
      // so we'll check for that first...

      ID3v2RawHeader id3v2RawHeader = new ID3v2RawHeader();
      byte[] id3v2RawHeaderBytes = Utils.RawSerializeEx(id3v2RawHeader);
      AudioFileStream.Read(id3v2RawHeaderBytes, 0, id3v2RawHeaderBytes.Length);
      id3v2RawHeader = (ID3v2RawHeader) Utils.RawDeserializeEx(id3v2RawHeaderBytes, typeof(ID3v2RawHeader));
      int iD3v2TagSize = Utils.ReadUnsynchronizedData(id3v2RawHeader.Size, 0, 4);

      if (id3v2RawHeader.Header[0] == (byte) 'I'
          && id3v2RawHeader.Header[1] == (byte) 'D'
          && id3v2RawHeader.Header[2] == (byte) '3')
      {
        AudioFileStream.Seek(iD3v2TagSize, SeekOrigin.Current);
      }

      else
      {
        AudioFileStream.Position = 0;
      }

      byte[] buffer = new byte[4];
      AudioFileStream.Read(buffer, 0, 4);

      return buffer[0] == 'f' && buffer[1] == 'L' && buffer[2] == 'a' && buffer[3] == 'C';
    }


    private bool ReadMetadataBlocks(ref int metadataBlocksRead)
    {
      /////////////////////////////////////////////////////////////
      // Read the block header and get the block type
      // bit  1:   Last-metadata-block flag
      // bits 2-8: Block type (see BlockType enum)
      /////////////////////////////////////////////////////////////

      byte[] buffer = new byte[4];
      AudioFileStream.Read(buffer, 0, 4);
      UInt32 hdrInfo = BitConverter.ToUInt32(buffer, 0);
      UInt16 audioFrameHeaderSync = Utils.ReadUInt16SynchronizedData(buffer, 0, 2);

      // Can't entirely rely on this as some taggers seem to ignore setting this flag :(
      bool hasMoreMetadataBlocks = (hdrInfo & 1) == 0;
      bool isLastMetadataBlock = !hasMoreMetadataBlocks;

      byte blockType = (byte) (hdrInfo & 254);

      int metadataLength = Utils.ReadSynchronizedData(buffer, 1, 3);

      if (blockType == (byte) BlockType.StreamInfo)
      {
        // Have we already read the Stream Info block?  If so, something's wrong so bail out
        if (StreamInfo.StreamPosition > 0)
        {
          return false;
        }

        ReadStreamInfoBlock(metadataLength, isLastMetadataBlock);
        metadataBlocksRead++;
        return true;
      }

      else if (blockType == (byte) BlockType.Padding)
      {
        // Have we already read the Padding block?  If so, something's wrong so bail out
        if (PaddingBlock.StreamPosition > 0)
        {
          return false;
        }

        ReadBlockPadding(metadataLength, isLastMetadataBlock);
        metadataBlocksRead++;
        return true;
      }

      else if (blockType == (byte) BlockType.Application)
      {
        // Have we already read the Application block?  If so, something's wrong so bail out
        if (ApplicationInfo.StreamPosition > 0)
        {
          return false;
        }

        ReadApplicationBlock(metadataLength, isLastMetadataBlock);
        metadataBlocksRead++;
        return true;
      }

      else if (blockType == (byte) BlockType.VorbisComment)
      {
        // Have we already read the Vorbis Comments block?  If so, something's wrong so bail out
        if (VorbisCommentsInfo.StreamPosition > 0)
        {
          return false;
        }

        ReadVorbisCommentsBlock(metadataLength, isLastMetadataBlock);
        metadataBlocksRead++;
        return true;
      }

      else if (blockType == (byte) BlockType.CueSheet)
      {
        // Have we already read the CueSheet block?  If so, something's wrong so bail out
        if (CueSheetInfo.StreamPosition > 0)
        {
          return false;
        }

        ReadCueSheetBlock(metadataLength, isLastMetadataBlock);
        metadataBlocksRead++;
        return true;
      }

      else if (blockType == (byte) BlockType.Invalid)
      {
        AudioFileStream.Seek(-4, SeekOrigin.Current);
        return false;
      }

      else // The block type wasn't a valid value type.  We must be done
      {
        AudioFileStream.Seek(-4, SeekOrigin.Current);
        return false;
      }
    }

    private void ReadStreamInfoBlock(int blockLength, bool lastMetadataBlock)
    {
      long currentStreamPosition = AudioFileStream.Position;
      StreamInfo.StreamPosition = currentStreamPosition;
      StreamInfo.LastMetadataBlock = lastMetadataBlock;

      byte[] buffer = new byte[2];
      AudioFileStream.Read(buffer, 0, 2);
      StreamInfo.MinimumBlockSamples = Utils.ReadSynchronizedData(buffer, 0, 2);

      buffer = new byte[2];
      AudioFileStream.Read(buffer, 0, 2);
      StreamInfo.MaximumBlockSamples = Utils.ReadSynchronizedData(buffer, 0, 2);

      buffer = new byte[3];
      AudioFileStream.Read(buffer, 0, 3);
      StreamInfo.MinimumFrameSize = Utils.ReadSynchronizedData(buffer, 0, 3);

      buffer = new byte[3];
      AudioFileStream.Read(buffer, 0, 3);
      StreamInfo.MaximumFrameSize = Utils.ReadSynchronizedData(buffer, 0, 3);

      buffer = new byte[8];
      AudioFileStream.Read(buffer, 0, 8);
      UInt64 sampleInfo = Utils.ReadUInt64SynchronizedData(buffer, 0, 8);

      StreamInfo.SampleRate = (int) (sampleInfo >> 44);
      StreamInfo.Channels = (byte) (((int) (sampleInfo >> 41) & 7) + 1);

      StreamInfo.BitsPerSample = ((int) (sampleInfo >> 36) & 31) + 1;
      StreamInfo.TotalSamples = sampleInfo & 68719476735;

      byte[] md5Signature = new byte[16];
      AudioFileStream.Read(md5Signature, 0, 16);
      StreamInfo.Md5Signature = md5Signature;
    }

    private void ReadBlockPadding(int blockLength, bool isLastMetadataBlock)
    {
      PaddingBlock.StreamPosition = AudioFileStream.Position;
      PaddingBlock.LastMetadataBlock = isLastMetadataBlock;
      PaddingBlock.MetadataLength = blockLength;

      // Don't care about the padding so skip over it;
      AudioFileStream.Seek(blockLength, SeekOrigin.Current);
    }

    private void ReadApplicationBlock(int blockLength, bool isLastMetadataBlock)
    {
      ApplicationInfo.StreamPosition = AudioFileStream.Position;
      ApplicationInfo.LastMetadataBlock = isLastMetadataBlock;
      ApplicationInfo.MetadataLength = blockLength;

      byte[] buffer = new byte[4];
      AudioFileStream.Read(buffer, 0, 4);
      ApplicationInfo.ApplicationID = (uint) Utils.ReadSynchronizedData(buffer, 0, 4);

      int dataLength = blockLength;

      if (dataLength > 0)
      {
        buffer = new byte[dataLength];
        AudioFileStream.Read(buffer, 0, dataLength - 4);
        ApplicationInfo.ApplicationData = buffer;
      }
    }

    private void ReadVorbisCommentsBlock(int blockLength, bool isLastMetadataBlock)
    {
      VorbisCommentsInfo.StreamPosition = AudioFileStream.Position;
      VorbisCommentsInfo.LastMetadataBlock = isLastMetadataBlock;
      VorbisCommentsInfo.MetadataLength = blockLength;

      byte[] buffer = new byte[4];
      AudioFileStream.Read(buffer, 0, 4);
      int vendorLength = BitConverter.ToInt32(buffer, 0);

      buffer = new byte[vendorLength];
      AudioFileStream.Read(buffer, 0, vendorLength);
      string vendor = Encoding.UTF8.GetString(buffer);

      buffer = new byte[4];
      AudioFileStream.Read(buffer, 0, 4);

      int commentsCount = BitConverter.ToInt32(buffer, 0);

      if (commentsCount == 0)
      {
        return;
      }

      for (int i = 0; i < commentsCount; i++)
      {
        buffer = new byte[4];
        AudioFileStream.Read(buffer, 0, 4);

        Int32 len = BitConverter.ToInt32(buffer, 0);
        buffer = new byte[len];
        long lastStreamPosition = AudioFileStream.Position;
        AudioFileStream.Read(buffer, 0, len);
        AudioFileStream.Position = lastStreamPosition;

        string comment = Encoding.UTF8.GetString(buffer);

        int pos = comment.IndexOf("=");
        if (pos == -1)
        {
          continue;
        }

        comment = comment.Substring(0, pos);
        AudioFileStream.Position += comment.Length + 1;
        int commentLength = comment.Length + 1;
        int dataLength = len - commentLength;
        buffer = new byte[dataLength];
        AudioFileStream.Read(buffer, 0, dataLength);

        VorbisComment oggComment = new VorbisComment(comment, buffer);
        CommentList.Add(oggComment);
      }
    }

    private void ReadCueSheetBlock(int blockLength, bool isLastMetadataBlock)
    {
      CueSheetInfo.StreamPosition = AudioFileStream.Position;
      CueSheetInfo.LastMetadataBlock = isLastMetadataBlock;
      CueSheetInfo.MetadataLength = blockLength;

      // Don't care about the cuesheet ATM so skip over it;
      AudioFileStream.Seek(blockLength, SeekOrigin.Current);
    }

    private byte[] GetBinaryCommentValue(string commentName)
    {
      return GetBase64StringCommentValue(commentName);
    }

    private string GetStringCommentValue(string commentName)
    {
      commentName = commentName.ToLower();

      foreach (VorbisComment comment in CommentList)
      {
        if (comment.FieldName.ToLower().CompareTo(commentName) == 0)
        {
          return Encoding.UTF8.GetString(comment.FieldValue);
        }
      }

      return string.Empty;
    }

    private byte[] GetBase64StringCommentValue(string commentName)
    {
      commentName = commentName.ToLower();

      foreach (VorbisComment comment in CommentList)
      {
        if (comment.FieldName.ToLower().CompareTo(commentName) == 0)
        {
          char[] c = Encoding.ASCII.GetChars(comment.FieldValue);
          return Convert.FromBase64CharArray(c, 0, comment.FieldValue.Length);
        }
      }

      return null;
    }
    #endregion
  }
}
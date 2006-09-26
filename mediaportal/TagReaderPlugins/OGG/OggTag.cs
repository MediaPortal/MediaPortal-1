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
using System.Collections;
using System.Text;
using System.IO;
using System.Drawing;
using MediaPortal.TagReader;

using MediaPortal.GUI.Library;

namespace Tag.OGG
{
  public class OggTag : TagBase
  {
    public struct VorbisIdHeader
    {
      public Int32 VorbisVersion;
      public byte AudioChannels;
      public UInt32 AudioSampleRate;
      public Int32 BitrateMaximum;
      public Int32 BitrateNominal;
      public Int32 BitrateNormal;
      public int BlockSize0;
      public int BlockSize1;
    }

    public class PageHeader
    {
      private long _StreamPosition = -1;
      private byte _StreamStructureVersion;
      private byte _HeaderTypeFlag;
      private UInt64 _AbsGranulePosition;
      private UInt32 _StreamSerialNumber;
      private UInt32 _PageNumber;
      private UInt32 _PageCRC;
      private byte _PageSegmentCount;
      private byte[] _SegmentTable = null;
      private int _PacketDataSize = -1;

      private bool _IsFreshPacket = true;
      private bool _IsBeginningOfStream = true;
      private bool _IsEndOfStream = true;

      #region Properties

      public long StreamPosition
      {
        get { return _StreamPosition; }
        set { _StreamPosition = value; }
      }

      public byte StreamStructureVersion
      {
        get { return _StreamStructureVersion; }
        set { _StreamStructureVersion = value; }
      }

      public byte HeaderTypeFlag
      {
        get { return _HeaderTypeFlag; }
        set
        {
          _IsFreshPacket = Utils.IsFlagSet(_HeaderTypeFlag, 1);
          _IsBeginningOfStream = Utils.IsFlagSet(_HeaderTypeFlag, 2);
          _IsEndOfStream = Utils.IsFlagSet(_HeaderTypeFlag, 4);

          _HeaderTypeFlag = value;
        }
      }

      public UInt64 AbsGranulePosition
      {
        get { return _AbsGranulePosition; }
        set { _AbsGranulePosition = value; }
      }

      public UInt32 StreamSerialNumber
      {
        get { return _StreamSerialNumber; }
        set { _StreamSerialNumber = value; }
      }

      public UInt32 PageNumber
      {
        get { return _PageNumber; }
        set { _PageNumber = value; }
      }

      public UInt32 PageCRC
      {
        get { return _PageCRC; }
        set { _PageCRC = value; }
      }

      public byte PageSegmentCount
      {
        get { return _PageSegmentCount; }
        set { _PageSegmentCount = value; }
      }

      public byte[] SegmentTable
      {
        get { return _SegmentTable; }
        set
        {
          _SegmentTable = value;

          if (_SegmentTable != null)
          {
            for (int i = 0; i < _SegmentTable.Length; i++)
            {
              _PacketDataSize += _SegmentTable[i];
            }
          }
        }
      }

      public int PacketDataSize
      {
        get { return _PacketDataSize; }
      }

      public long PacketDataBeginStreamPosition
      {
        get
        {
          return StreamPosition + _PageSegmentCount + 26 + 1;
        }
      }

      public long PacketDataEndStreamPosition
      {
        get
        {
          return PacketDataBeginStreamPosition + _PacketDataSize + 1;
        }
      }

      public bool IsFreshPacket
      {
        get { return _IsFreshPacket; }
        set { _IsFreshPacket = value; }
      }

      public int PageLength
      {
        get
        {
          int segmentTableLength = PageSegmentCount + 26;
          int pageLen = PacketDataSize + segmentTableLength + 2;

          return pageLen;
        }
      }

      public bool IsBeginningOfStream
      {
        get { return _IsBeginningOfStream; }
        set { _IsBeginningOfStream = value; }
      }

      public bool IsEndOfStream
      {
        get { return _IsEndOfStream; }
        set { _IsEndOfStream = value; }
      }

      public bool HasSamples
      {
        get { return _AbsGranulePosition > 0 && _AbsGranulePosition != ulong.MaxValue; }
      }

      #endregion

      public PageHeader()
      {
      }
    }

    #region Constants

    private const string OGG_PAGEID = "OggS";
    private const string VORBIS_ID_HEADER = "1vorbis";
    private const string VORBIS_COMMENT_HEADER = "3vorbis";
    private const string VORBIS_SETUP_HEADER = "5vorbis";
    private const string VORBIS_CODE_BOOK = "BCV";

    public const string TAG_ARTIST = "ARTIST";
    public const string TAG_ALBUM = "ALBUM";
    public const string TAG_TITLE = "TITLE";
    public const string TAG_DATE = "DATE";
    public const string TAG_TRACK = "TRACK";
    public const string TAG_GENRE = "GENRE";
    public const string TAG_COMMENT = "COMMENT";

    #endregion

    #region Variables

    private List<VorbisComment> CommentList = new List<VorbisComment>();
    private PageHeader CurrentPageHeader = null;
    private VorbisIdHeader VorbisIdHdr;
    private ulong TotalSamples = 0;
    private Hashtable PageTable = new Hashtable();


    #endregion

    #region ITag Properties

    override public string Album
    {
      get { return GetStringCommentValue("ALBUM"); }
    }

    override public string Artist
    {
      get { return GetStringCommentValue("ARTIST"); }
    }

    override public string AlbumArtist
    {
      get
      {
        string albumArtist = GetStringCommentValue("ALBUMARTIST");
        return albumArtist;
      }
    }

    override public string ArtistURL
    {
      get { return base.ArtistURL; }
    }

    override public int AverageBitrate
    {
      get
      {
        int bitRate = (int)VorbisIdHdr.BitrateNominal;

        if (bitRate > 0)
          return bitRate / 1000;

        return bitRate;
      }
    }

    override public int BitsPerSample
    {
      get { return 16; }
    }

    override public int BlocksPerFrame
    {
      get { return base.BlocksPerFrame; }
    }

    override public string BuyURL
    {
      get { return base.BuyURL; }
    }

    override public int BytesPerSample
    {
      get { return base.BytesPerSample; }
    }

    override public int Channels
    {
      get { return VorbisIdHdr.AudioChannels; }
    }

    override public string Comment
    {
      get { return base.Comment; }
    }

    override public string Composer
    {
      get { return base.Composer; }
    }

    override public int CompressionLevel
    {
      get { return base.CompressionLevel; }
    }

    override public string Copyright
    {
      get { return base.Copyright; }
    }

    override public string CopyrightURL
    {
      get { return base.CopyrightURL; }
    }

    override public byte[] CoverArtImageBytes
    {
      get { return GetBase64StringCommentValue("COVERART"); }
    }

    override public string FileURL
    {
      get { return base.FileURL; }
    }

    override public int FormatFlags
    {
      get { return base.FormatFlags; }
    }

    override public bool IsVBR
    {
      get
      {
        return VorbisIdHdr.BitrateMaximum > 0 && VorbisIdHdr.BitrateNominal > 0;
      }
    }

    override public string Genre
    {
      get { return GetStringCommentValue("GENRE"); }
    }

    override public string Keywords
    {
      get { return base.Keywords; }
    }

    override public string Length
    {
      get { return Utils.GetDurationString(LengthMS); }
    }

    override public int LengthMS
    {
      get
      {
        if (TotalSamples == 0)
          return 0;

        return (int)((TotalSamples / VorbisIdHdr.AudioSampleRate) * 1000);
      }
    }

    override public string Lyrics
    {
      get { return GetStringCommentValue("LYRICS"); }
    }

    override public string Notes
    {
      get { return base.Notes; }
    }

    override public string PeakLevel
    {
      get { return base.PeakLevel; }
    }

    override public string PublisherURL
    {
      get { return base.PublisherURL; }
    }

    override public string ReplayGainAlbum
    {
      get { return base.ReplayGainAlbum; }
    }

    override public string ReplayGainRadio
    {
      get { return base.ReplayGainRadio; }
    }

    override public int SampleRate
    {
      get { return (int)VorbisIdHdr.AudioSampleRate; }
    }

    override public string Title
    {
      get { return GetStringCommentValue("TITLE"); }
    }

    override public string ToolName
    {
      get { return base.ToolName; }
    }

    override public string ToolVersion
    {
      get { return base.ToolVersion; }
    }

    override public int TotalBlocks
    {
      get { return base.TotalBlocks; }
    }

    override public int TotalFrames
    {
      get { return base.TotalFrames; }
    }

    override public int Track
    {
      get
      {
        try
        {
          string sTrack = GetStringCommentValue("TRACKNUMBER");

          if (sTrack.Length > 0)
            return int.Parse(sTrack);
        }

        catch (Exception ex)
        {
          Log.Error("OggTag.get_Length caused an exception in file {0} : {1}", base.FileName, ex.Message);
        }

        return 0;
      }
    }

    override public string Version
    {
      get { return base.Version; }
    }

    override public int Year
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
          Log.Error("OggTag.get_Year caused an exception in file {0} : {1}", base.FileName, ex.Message);
          return 0;
        }
      }
    }

    #endregion

    public OggTag()
      : base()
    {
    }

    public OggTag(string fileName)
      : base(fileName)
    {
      Read(fileName);
    }

    ~OggTag()
    {
      Dispose();
    }

    override public bool SupportsFile(string strFileName)
    {
      if (System.IO.Path.GetExtension(strFileName).ToLower() == ".ogg") return true;
      return false;
    }

    override public bool Read(string fileName)
    {
      if (fileName.Length == 0)
        throw new Exception("No file name specified");

      if (!File.Exists(fileName))
        throw new Exception("Unable to open file.  File does not exist.");

      if (Path.GetExtension(fileName).ToLower() != ".ogg")
        throw new AudioFileTypeException("Expected OGG file type.");

      base.Read(fileName);
      bool result = true;

      if (AudioFileStream == null)
        AudioFileStream = new FileStream(this.AudioFilePath, FileMode.Open, FileAccess.Read);

      try
      {
        if (IsOggPage() == null)
          return false;

        if (!ReadVorbisIDHeader())
          return false;

        if (!ReadCommentsHeader())
          return false;

        TotalSamples = GetTotalSamples();
      }

      catch (Exception ex)
      {
        Log.Error("OggTag.Read caused an exception in file {0} : {1}", base.FileName, ex.Message);
        result = false;
      }

      return result;
    }

    private bool ReadVorbisIDHeader()
    {
      long curStreamPos = AudioFileStream.Position;

      AudioFileStream.Position = 26;

      int nB1 = AudioFileStream.ReadByte();
      int nB2 = AudioFileStream.ReadByte();

      if (nB1 == -1 || nB2 == -1)
        return false;

      byte b1 = (byte)nB1;
      byte b2 = (byte)nB2;

      int headerLength = (b1 + b2) - VORBIS_ID_HEADER.Length;

      byte[] vorbisBytes = new byte[7];
      AudioFileStream.Read(vorbisBytes, 0, 7);

      if (vorbisBytes[0] == 1 && vorbisBytes[1] == 'v' && vorbisBytes[2] == 'o'
          && vorbisBytes[3] == 'r' && vorbisBytes[4] == 'b' && vorbisBytes[5] == 'i'
          && vorbisBytes[6] == 's')
      {
        VorbisIdHdr = new VorbisIdHeader();
        byte[] buffer = new byte[4];
        AudioFileStream.Read(buffer, 0, 4);
        VorbisIdHdr.VorbisVersion = BitConverter.ToInt32(buffer, 0);

        VorbisIdHdr.AudioChannels = (byte)AudioFileStream.ReadByte();

        AudioFileStream.Read(buffer, 0, 4);
        VorbisIdHdr.AudioSampleRate = BitConverter.ToUInt32(buffer, 0);

        AudioFileStream.Read(buffer, 0, 4);
        VorbisIdHdr.BitrateMaximum = Utils.GetBigEndianInt(buffer);

        AudioFileStream.Read(buffer, 0, 4);
        VorbisIdHdr.BitrateNominal = (int)BitConverter.ToUInt32(buffer, 0);

        AudioFileStream.Read(buffer, 0, 4);
        VorbisIdHdr.BitrateNormal = BitConverter.ToInt32(buffer, 0);

        int blocks = AudioFileStream.ReadByte();
        VorbisIdHdr.BlockSize0 = (int)Math.Pow(2, blocks - (blocks >> 4 << 4));
        VorbisIdHdr.BlockSize1 = (int)Math.Pow(2, blocks >> 4);

        if (!IsValidBlockSize(VorbisIdHdr.BlockSize0))
          throw new AudioFileTypeException("Invalid Vorbis HeaderID BlockSize0");

        if (!IsValidBlockSize(VorbisIdHdr.BlockSize1))
          throw new AudioFileTypeException("Invalid Vorbis HeaderID BlockSize1");

        if (VorbisIdHdr.BlockSize0 > VorbisIdHdr.BlockSize1)
          throw new AudioFileTypeException("Invalid Vorbis HeaderID BlockSize error (BlockSize > BlcokSize1)");

        return true;
      }

      else
        return false;
    }

    private bool ReadCommentsHeader()
    {
      if (SetPage(1) == null)
        return false;

      long currentPosition = AudioFileStream.Position;

      int pageSize = CurrentPageHeader.PacketDataSize + 26 + 2;
      AudioFileStream.Seek(CurrentPageHeader.PageSegmentCount + 26 + 1, SeekOrigin.Current);
      byte[] vorbisBytes = new byte[7];
      AudioFileStream.Read(vorbisBytes, 0, 7);

      if (vorbisBytes[0] == 3 && vorbisBytes[1] == 'v' && vorbisBytes[2] == 'o'
          && vorbisBytes[3] == 'r' && vorbisBytes[4] == 'b' && vorbisBytes[5] == 'i'
          && vorbisBytes[6] == 's')
      {
        //Console.WriteLine("Found header");
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
          return false;

        for (int i = 0; i < commentsCount; i++)
        {
          buffer = new byte[4];
          AudioFileStream.Read(buffer, 0, 4);

          Int32 len = BitConverter.ToInt32(buffer, 0);
          buffer = new byte[len];
          long lastStreamPosition = AudioFileStream.Position;
          AudioFileStream.Read(buffer, 0, len);

          string comment = Encoding.UTF8.GetString(buffer);

          int pos = comment.IndexOf("=");

          if (pos == -1)
            continue;

          comment = comment.Substring(0, pos);
          AudioFileStream.Position -= (len - (comment.Length + 1));
          int commentLength = comment.Length + 1;
          int dataLength = len - commentLength;
          byte[] valueBuffer = ReadPacketData(dataLength);

          VorbisComment oggComment = new VorbisComment(comment, valueBuffer);
          CommentList.Add(oggComment);
        }
      }

      return true;
    }

    private byte[] ReadPacketData(int dataLength)
    {
      int dataBytesRead = 0;
      byte[] packetData = new byte[dataLength];
      bool multiPage = false;
      bool isNewPage = false;

      while (dataBytesRead < dataLength)
      {
        if (isNewPage)
          AudioFileStream.Position = CurrentPageHeader.PacketDataBeginStreamPosition;

        long packetBytesLeftOnPage = CurrentPageHeader.PacketDataEndStreamPosition - AudioFileStream.Position;
        long packetBytesToRead = dataLength - packetBytesLeftOnPage;

        // Is the packet spread over multiple pages?
        if (packetBytesToRead > packetBytesLeftOnPage)
        {
          multiPage = true;
          long readBytes = CurrentPageHeader.PacketDataEndStreamPosition - AudioFileStream.Position;

          if (dataBytesRead + readBytes > dataLength)
            readBytes = dataLength - dataBytesRead;

          //Console.WriteLine("dataBytesRead: {0}", dataBytesRead);

          byte[] buffer = new byte[readBytes];
          AudioFileStream.Read(buffer, 0, (int)readBytes);

          Buffer.BlockCopy(buffer, 0, packetData, dataBytesRead, buffer.Length);
          dataBytesRead += buffer.Length;
        }

        else if (multiPage)
        {
          byte[] buffer = new byte[packetBytesToRead];
          AudioFileStream.Read(buffer, 0, (int)packetBytesToRead);

          Buffer.BlockCopy(buffer, 0, packetData, dataBytesRead, buffer.Length);
          dataBytesRead += buffer.Length;
        }

        else if (!multiPage)
        {
          AudioFileStream.Read(packetData, 0, (int)packetData.Length);
          return packetData;
        }

        if (dataBytesRead == dataLength)
          break;

        this.SetNextPage(CurrentPageHeader);
        isNewPage = true;
      }

      return packetData;
    }

    private ulong GetTotalSamples()
    {
      PageHeader pgHdr = GetLastPage();

      if (pgHdr != null)
        return pgHdr.AbsGranulePosition;

      return 0;
    }

    private PageHeader GetPage(uint pageNumber)
    {
      return (PageHeader)PageTable[pageNumber];
    }

    private PageHeader GetNextPage(PageHeader pgHdr)
    {
      if (pgHdr == null)
        throw new Exception("Invalid PageHeader");

      uint pageNumber = pgHdr.PageNumber;
      PageHeader targetPgHdr = (PageHeader)PageTable[pageNumber + 1];

      if (targetPgHdr != null)
        return targetPgHdr;

      int pageLen = GetPageLength(pgHdr);

      long currentStreamPosition = AudioFileStream.Position;
      AudioFileStream.Position = pgHdr.StreamPosition + pageLen;

      targetPgHdr = IsOggPage();
      AudioFileStream.Position = currentStreamPosition;
      return targetPgHdr;
    }

    private PageHeader GetLastPage()
    {
      long currentStreamPosition = AudioFileStream.Position;
      PageHeader pgHdr = null;

      // Go to the end of the stream and look for the "OggS" itendifier one 
      // byte at a time...
      int maxSearchBytes = 1024 * 20;
      byte[] buffer = new byte[maxSearchBytes];
      AudioFileStream.Seek(-maxSearchBytes, SeekOrigin.End);
      AudioFileStream.Read(buffer, 0, maxSearchBytes);

      for (int i = buffer.Length - 1; i >= 0; i--)
      {
        try
        {
          if (buffer[i] == 'S' && buffer[i - 1] == 'g' && buffer[i - 2] == 'g' && buffer[i - 3] == 'O')
          {
            int index = i - 4;
            //Console.WriteLine("Found last page at: {0}", (AudioFileStream.Length - 1) - index);
            AudioFileStream.Seek(-(maxSearchBytes - index - 1), SeekOrigin.End);
            pgHdr = IsOggPage();
            break;
          }
        }

        catch (Exception ex)
        {
          Log.Error("OggTag.GetLastPage caused an exception in file {0} : {1}", base.FileName, ex.Message);
        }

        finally
        {
          AudioFileStream.Position = currentStreamPosition;
        }
      }

      return pgHdr;
    }

    private PageHeader SetPage(uint pageNumber)
    {
      PageHeader pgHdr = GetPage(pageNumber);

      if (pgHdr != null)
      {
        AudioFileStream.Position = pgHdr.StreamPosition;
        CurrentPageHeader = pgHdr;
        return pgHdr;
      }

      // Find closest matching page number
      uint bestMatchPageNumber = 0;

      foreach (DictionaryEntry o in PageTable)
      {
        PageHeader curPdHdr = (PageHeader)o.Value;

        if (curPdHdr.PageNumber < pageNumber && curPdHdr.PageNumber > bestMatchPageNumber)
          bestMatchPageNumber = curPdHdr.PageNumber;
      }

      PageHeader tempPgHdr = (PageHeader)PageTable[bestMatchPageNumber];

      if (tempPgHdr == null)
        throw new Exception("Invalid PageHeader");

      AudioFileStream.Position = tempPgHdr.StreamPosition;

      bool isLastPage = false;

      while (!isLastPage && tempPgHdr.PageNumber < pageNumber)
      {
        tempPgHdr = IsOggPage(true);

        if (tempPgHdr.PageNumber == pageNumber)
        {
          AudioFileStream.Position = tempPgHdr.StreamPosition;
          CurrentPageHeader = tempPgHdr;
          return tempPgHdr;
        }

        int pageLen = GetPageLength(tempPgHdr);

        isLastPage = AudioFileStream.Position + pageLen >= AudioFileStream.Length;

        if (!isLastPage)
          AudioFileStream.Position += pageLen;
      }

      return pgHdr;
    }

    private PageHeader SetNextPage(PageHeader pgHdr)
    {
      if (pgHdr == null)
        throw new Exception("Invalid PageHeader");

      uint nextPageNumber = pgHdr.PageNumber + 1;
      PageHeader newPageHeader = SetPage(nextPageNumber);

      if (pgHdr.PageNumber == newPageHeader.PageNumber)
        throw new Exception("Failed to GetNextPage");

      return newPageHeader;
    }

    private int GetPageCount(bool resetPosition)
    {
      long currentPosition = AudioFileStream.Position;

      // Set the stream position to the beginning of page 2
      AudioFileStream.Position = 58;
      PageHeader curPageHdr = IsOggPage(true);

      int pageCount = 2;
      bool isLastPage = false;

      while (!isLastPage)
      {
        int pageLen = GetPageLength(curPageHdr);
        isLastPage = curPageHdr.StreamPosition + pageLen >= AudioFileStream.Length;

        if (!isLastPage)
        {
          curPageHdr = GetNextPage(curPageHdr);

          if (curPageHdr != null)
            pageCount++;

          else
            throw new Exception("Error: couldn't find page boundry!");
        }
      }

      if (resetPosition)
        AudioFileStream.Position = currentPosition;

      return pageCount;
    }

    private uint GetPageCountFast()
    {
      PageHeader pgHdr = GetLastPage();

      if (pgHdr == null)
        return 0;

      return pgHdr.PageNumber + 1;
    }

    private int GetPageLength(PageHeader pgHdr)
    {
      if (pgHdr == null)
        throw new Exception("Invalid PageHeader");

      int segmentTableLength = pgHdr.PageSegmentCount + 26;
      int pageLen = pgHdr.PacketDataSize + segmentTableLength + 2;

      return pageLen;
    }

    private bool IsValidBlockSize(int blockSize)
    {
      return blockSize == 64 || blockSize == 128
          || blockSize == 256 || blockSize == 512
          || blockSize == 1024 || blockSize == 2048
          || blockSize == 4096 || blockSize == 8192;
    }

    private PageHeader IsOggPage()
    {
      PageHeader pgHdr = IsOggPage(true);
      return pgHdr;
    }

    private PageHeader IsOggPage(bool resetPosition)
    {
      PageHeader pgHdr = null;
      long startingPosition = AudioFileStream.Position;
      byte[] buffer = new byte[4];
      AudioFileStream.Read(buffer, 0, 4);

      // Looking for "OggS"
      if (buffer[0] == 'O' && buffer[1] == 'g' && buffer[2] == 'g' && buffer[3] == 'S')
      {
        long curPosition = AudioFileStream.Position;

        // Get the page number...
        AudioFileStream.Position += 14;
        buffer = new byte[4];
        AudioFileStream.Read(buffer, 0, 4);
        UInt32 pageNumber = BitConverter.ToUInt32(buffer, 0);
        AudioFileStream.Position = curPosition;

        pgHdr = (PageHeader)PageTable[pageNumber];

        if (pgHdr == null)
        {
          pgHdr = new PageHeader();
          pgHdr.StreamPosition = startingPosition;
          pgHdr.StreamStructureVersion = (byte)AudioFileStream.ReadByte();
          pgHdr.HeaderTypeFlag = (byte)AudioFileStream.ReadByte();

          buffer = new byte[8];
          AudioFileStream.Read(buffer, 0, 8);
          pgHdr.AbsGranulePosition = BitConverter.ToUInt64(buffer, 0);

          buffer = new byte[4];
          AudioFileStream.Read(buffer, 0, 4);
          pgHdr.StreamSerialNumber = BitConverter.ToUInt32(buffer, 0);

          buffer = new byte[4];
          AudioFileStream.Read(buffer, 0, 4);
          pgHdr.PageNumber = BitConverter.ToUInt32(buffer, 0);

          buffer = new byte[4];
          AudioFileStream.Read(buffer, 0, 4);
          pgHdr.PageCRC = BitConverter.ToUInt32(buffer, 0);

          pgHdr.PageSegmentCount = (byte)AudioFileStream.ReadByte();

          buffer = new byte[pgHdr.PageSegmentCount];
          AudioFileStream.Read(buffer, 0, buffer.Length);
          pgHdr.SegmentTable = buffer;

          pgHdr.StreamPosition = startingPosition;
          PageTable[pageNumber] = pgHdr;
        }
      }

      if (resetPosition)
        AudioFileStream.Position = startingPosition;

      return pgHdr;
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
          return System.Text.Encoding.UTF8.GetString(comment.FieldValue);
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
  }
}

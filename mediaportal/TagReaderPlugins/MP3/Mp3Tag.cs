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
using System.IO;
using Tag;
using Tag.MAC;
using ID3;
using System.Drawing;
using MediaPortal.TagReader;
using MediaPortal.GUI.Library;

namespace Tag.MP3
{
  public class Mp3TagField : IDisposable
  {
    #region Variables

    private string _FieldName = string.Empty;
    private byte[] _FieldValue = null;

    #endregion

    #region Properties

    public string FieldName
    {
      get { return _FieldName; }
    }

    public byte[] FieldValue
    {
      get { return _FieldValue; }
    }

    #endregion

    #region Constructors/Destructors
    public Mp3TagField(string fieldName, byte[] fieldValue)
    {
      _FieldName = fieldName;
      _FieldValue = fieldValue;
    }

    ~Mp3TagField()
    {
      Dispose();
    }
    #endregion


    public void Dispose()
    {
    }
  }

  public class Mp3Tag : ApeTag
  {
    #region Variables

    private ID3.ID3Tag Id3Tag = null;

    #endregion

    #region Constructors/Destructors
    public Mp3Tag()
      : base()
    {
    }

    ~Mp3Tag()
    {
      Dispose();
    }
    #endregion

    #region Properties

    public override string Album
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.Album;

          if (val.Length > 0)
            return val;
        }

        string v2Album = GetFrameValueString(FrameNames.TALB, FrameNamesV2.TAL);

        if (v2Album.Length != 0)
          return v2Album;

        if (HasId3Tag)
          return new string(Id3Tag.ID3v1Tag.Album);

        return string.Empty;
      }
    }

    public override string Artist
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.Artist;

          if (val.Length > 0)
            return val;
        }

        string v2Artist = GetFrameValueString(FrameNames.TPE1, FrameNamesV2.TP1);

        if (v2Artist.Length != 0)
          return v2Artist;

        if (HasId3Tag)
          return new string(Id3Tag.ID3v1Tag.Artist);

        else
          return string.Empty;
      }
    }

    public override string AlbumArtist
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.AlbumArtist;

          if (val.Length > 0)
            return val;
        }

        return GetFrameValueString(FrameNames.TPE2, FrameNamesV2.TP2);
      }
    }

    public override string ArtistURL
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.ArtistURL;

          if (val.Length > 0)
            return val;
        }

        return GetFrameValueString(FrameNames.WOAR, FrameNamesV2.WAR);
      }
    }

    public override int AverageBitrate
    {
      get { return Id3Tag.BitRate; }
    }

    public override int BitsPerSample
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
          return base.BitsPerSample;

        return 0;
      }
    }

    public override int BlocksPerFrame
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
          return base.BlocksPerFrame;

        return 0;
      }
    }

    public override string BuyURL
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.BuyURL;

          if (val.Length > 0)
            return val;
        }

        return GetFrameValueString(FrameNames.WPAY, FrameNamesV2.WPB);
      }
    }

    public override int BytesPerSample
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
          return base.BytesPerSample;

        return 0;
      }
    }

    public override int Channels
    {
      get { return Id3Tag.Channels; }
    }

    public override string Comment
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.Comment;

          if (val.Length > 0)
            return val;
        }

        string comment = GetFrameValueString(FrameNames.COMM, FrameNamesV2.COM);

        if (comment.Length != 0)
          return comment;

        if (HasId3Tag)
          return new string(Id3Tag.ID3v1Tag.Comment);

        else
          return string.Empty;
      }
    }

    public override string Composer
    {
      get
      {
        if (FieldList.Count > 0)
          return base.Composer;

        return GetFrameValueString(FrameNames.TCOM, FrameNamesV2.TCM);
      }
    }

    public override int CompressionLevel
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          int val = base.CompressionLevel;

          if (val > 0)
            return val;
        }

        return 0;
      }
    }

    public override string Copyright
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.Copyright;

          if (val.Length > 0)
            return val;
        }

        return GetFrameValueString(FrameNames.TCOP, FrameNamesV2.TCR);
      }
    }

    public override string CopyrightURL
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.CopyrightURL;

          if (val.Length > 0)
            return val;
        }

        return GetFrameValueString(FrameNames.WCOP, FrameNamesV2.TCR);
      }
    }

    public override byte[] CoverArtImageBytes
    {
      //<Header for 'Attached picture', ID: "APIC">
      //Text encoding      $xx
      //MIME type          <text string> $00
      //Picture type       $xx
      //Description        <text string according to encoding> $00 (00)
      //Picture data       <binary data>

      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          byte[] val = base.CoverArtImageBytes;

          if (val != null && val.Length > 0)
            return val;
        }

        byte[] tempBytes = this.GetValueBinary(FrameNames.APIC);

        if (tempBytes == null)
          return null;

        string text = "";
        int i = 0;
        bool bFoundMimeTypeString = false;
        bool bFoundDescription = false;
        bool bFoundPicType = false;
        char lastVal = 'a';
        string sMimeType = "";
        string sDesc = "";

        // Skip the "Text encoding" byte so we start at index 1
        for (i = 1; i < tempBytes.Length; i++)
        {
          if (bFoundMimeTypeString && !bFoundDescription && !bFoundPicType)
          {
            // Skip the "Picture type" byte
            bFoundPicType = true;
            continue;
          }

          if (bFoundMimeTypeString && bFoundDescription)
            break;

          char c = (char)tempBytes[i];
          text += c;

          if (!bFoundMimeTypeString)
            sMimeType += c;

          else
            sDesc += c;

          if (c == '\0')
          {
            if (!bFoundMimeTypeString)
              bFoundMimeTypeString = true;

            else
              bFoundDescription = true;
          }

          lastVal = c;
        }

        if (bFoundMimeTypeString && bFoundDescription)
        {
          try
          {
            int imgBytesLen = tempBytes.Length - (i + 1);

            // In some cases the file can contain an APIC frame and mime type 
            // string but contain 0 actual image byte.  This should catch this
            if (imgBytesLen <= 0)
              return null;

            byte[] imgBytes = new byte[imgBytesLen];
            Buffer.BlockCopy(tempBytes, i, imgBytes, 0, imgBytesLen);

            return imgBytes;
          }

          catch (Exception ex)
          {
            Log.Error("MP3Tag.get_CoverArtImageBytes caused an exception in file {0}: {1}", base.FileName, ex.Message);
            return null;
          }
        }

        return null;
      }
    }

    public override string FileURL
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.FileURL;

          if (val.Length > 0)
            return val;
        }

        return this.GetFrameValueString(FrameNames.WOAF, FrameNamesV2.WAF);
      }
    }

    public override int FormatFlags
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
          return base.FormatFlags;

        return 0;
      }
    }

    public override bool IsVBR
    {
      get { return this.Id3Tag.IsVBR; }
    }

    public override string Genre
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.Genre;

          if (val.Length > 0)
            return val;
        }

        string sGenreVal = GetFrameValueString(FrameNames.TCON, FrameNamesV2.TCO);

        if (sGenreVal.Length > 0)
        {
          sGenreVal = sGenreVal.Trim();

          int pos = sGenreVal.IndexOf(")");

          // Check is the genre value is an invalid composite of genre id and genre name such 
          // as (2)Country. if it is we'll split and use the genre id
          if (pos > 0 && pos < sGenreVal.Length - 1)
            sGenreVal = sGenreVal.Substring(0, pos);

          sGenreVal = sGenreVal.Replace("(", "");
          sGenreVal = sGenreVal.Replace(")", "");

          // Check for another variant of the composite of genre id and genre name; 02Country
          for (int i = 0; i < sGenreVal.Length; i++)
          {
            if (Utils.IsAlphaNumericValue(sGenreVal[i]))
              return Utils.GetGenre(sGenreVal.Substring(i));
          }

          return Utils.GetGenre(sGenreVal);
        }

        else
        {
          if (HasId3Tag)
            return Utils.GetGenre(Id3Tag.ID3v1Tag.Genre);

          else
            return string.Empty;
        }
      }
    }

    public override string Keywords
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.Keywords;

          if (val.Length > 0)
            return val;
        }

        return "";
      }
    }

    public override string Length
    {
      get
      {
        try
        {
          TimeSpan ts = new TimeSpan(0, 0, 0, 0, LengthMS);
          int hr = ts.Hours;
          int min = ts.Minutes;
          int sec = ts.Seconds;

          string sHr = hr > 0 ? string.Format("{0}:", hr) : "";
          return string.Format("{0}{1:D2}:{2:D2}", sHr, min, sec);
        }

        catch (Exception ex)
        {
          Log.Error("MP3Tag.get_Length caused an exception in file {0}: {1}", base.FileName, ex.Message);
        }

        return string.Empty;
      }
    }

    public override int LengthMS
    {
      get
      {
        int length = GetFrameValueInt(FrameNames.TLEN);

        if (length > 0)
          return length;

        return Id3Tag.Duration * 1000;
      }
    }

    public override string Lyrics
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.Lyrics;

          if (val.Length > 0)
            return val;
        }

        return GetUnsynchronizedLyrics();
      }
    }

    public override string Notes
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.Notes;

          if (val.Length > 0)
            return val;
        }

        return "";
      }
    }

    public override string PeakLevel
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.PeakLevel;

          if (val.Length > 0)
            return val;
        }

        return "";
      }
    }

    public override string PublisherURL
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.PublisherURL;

          if (val.Length > 0)
            return val;
        }

        return this.GetFrameValueString(FrameNames.WPUB, FrameNamesV2.WPB);
      }
    }

    public override string ReplayGainAlbum
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.ReplayGainAlbum;

          if (val.Length > 0)
            return val;
        }

        return "";
      }
    }

    public override string ReplayGainRadio
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.ReplayGainRadio;

          if (val.Length > 0)
            return val;
        }

        return "";
      }
    }

    public override int SampleRate
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
          return base.SampleRate;

        return Id3Tag.SampleRate;
      }
    }

    public override string Title
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.Title;

          if (val.Length > 0)
            return val;
        }

        string v2Title = this.GetFrameValueString(FrameNames.TIT2, FrameNamesV2.TT2);

        if (v2Title.Length != 0)
          return v2Title;

        if (HasId3Tag)
          return new string(Id3Tag.ID3v1Tag.Title);

        else
          return string.Empty;
      }
    }

    public override string ToolName
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.ToolName;

          if (val.Length > 0)
            return val;
        }

        return "";
      }
    }

    public override string ToolVersion
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.ToolVersion;

          if (val.Length > 0)
            return val;
        }

        return "";
      }
    }

    public override int TotalBlocks
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
          return base.TotalBlocks;

        return 0;
      }
    }

    public override int TotalFrames
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
          return base.TotalFrames;

        return 0;
      }
    }

    public override int Track
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          int val = base.Track;

          if (val > 0)
            return val;
        }

        string sTrack = GetFrameValueString(FrameNames.TRCK.ToString(), FrameNamesV2.TRK.ToString());

        try
        {
          if (sTrack.Length > 0)
          {
            // Handle rare cases where the data returned includes the total track count
            // such as "3/12" or "3-12" (track 3 of 12)

            char[] delims = new char[] { '/', '-' };
            string[] splitString = sTrack.Split(delims);

            if (splitString.Length > 1)
              sTrack = splitString[0];

            return int.Parse(sTrack);
          }

          else
          {
            if (HasId3Tag)
              return (int)Id3Tag.ID3v1Tag.Track;

            else
              return 0;
          }
        }

        catch (Exception ex)
        {
          Log.Error("MP3Tag.get_Track caused an exception in file {0}: {1}", base.FileName, ex.Message);
        }

        return 0;
      }
    }

    public override string Version
    {
      get
      {
        // The file could contain a mix of APE and ID3v2.x tags so
        // we'll check if there's a corresponding APE value first
        if (FieldList.Count > 0)
        {
          string val = base.Version;

          if (val.Length > 0)
            return val;
        }

        return "";
      }
    }

    public override int Year
    {
      get
      {
        try
        {
          // The file could contain a mix of APE and ID3v2.x tags so
          // we'll check if there's a corresponding APE value first
          if (FieldList.Count > 0)
          {
            int val = base.Year;

            if (val > 0)
              return val;
          }

          string sYear = GetFrameValueString(FrameNames.TYER);
          int yr = Utils.GetYear(sYear);

          if (yr == 0)
          {
            sYear = GetFrameValueString(FrameNames.TDRC);
            yr = Utils.GetYear(sYear);
          }

          if (yr > 0)
            return yr;

          if (!HasId3Tag)
            return 0;

          // Check if we have a year value in ID3V1
          sYear = new string(Id3Tag.ID3v1Tag.Year);
          return Utils.GetYear(sYear);
        }

        catch (Exception ex)
        {
          Log.Error("MP3Tag.get_Year caused an exception in file {0}: {1}", base.FileName, ex.Message);
          return 0;
        }
      }
    }

    #endregion

    #region Public Methods
    public override bool SupportsFile(string strFileName)
    {
      if (System.IO.Path.GetExtension(strFileName).ToLower() == ".mp3") return true;
      return false;
    }

    public override bool Read(string fileName)
    {
      if (fileName.Length == 0)
        throw new Exception("No file name specified");

      if (!File.Exists(fileName))
        throw new Exception("Unable to open file.  File does not exist.");

      if (Path.GetExtension(fileName).ToLower() != ".mp3")
        throw new AudioFileTypeException("Expected MP3 file type.");

      AudioFilePath = fileName;
      FileName = Path.GetFileName(fileName);
      FileLength = new FileInfo(fileName).Length;

      bool result = true;
      bool bHasID3v1Tag = false;
      bool bHasID3v2Tag = false;

      try
      {
        if (AudioFileStream == null)
          AudioFileStream = new FileStream(AudioFilePath, FileMode.Open, FileAccess.Read);

        Id3Tag = new ID3.ID3Tag(AudioFileStream);

        bHasID3v1Tag = Id3Tag.HasV1Header;
        bHasID3v2Tag = Id3Tag.HasV2Header;

        ReadTags();

        if (bHasID3v1Tag || bHasID3v2Tag || FieldList.Count > 0)
          result = true;

        else
          result = false;

        AudioDataStartPostion = AudioFileStream.Position;
      }

      catch (Exception ex)
      {
        Log.Error("MP3Tag.Read caused an exception in file {0} : {1}", base.FileName, ex.Message);
        result = false;
      }
      finally
      {
        if (AudioFileStream == null)
        {
          AudioFileStream.Close();
          AudioFileStream.Dispose();
        }
      }

      return result;
    }
    #endregion

    #region Private Methods
    private ID3Frame GetFrame(string sFrameID)
    {
      return GetFrame(sFrameID, "");
    }

    private ID3Frame GetFrame(string sFrameID, string sFrameIDv2)
    {
      foreach (ID3Frame frame in Id3Tag.ID3Frames)
      {
        if (sFrameID.CompareTo(frame.ID) == 0 || sFrameIDv2.CompareTo(frame.ID) == 0)
          return frame;
      }

      return null;
    }

    private string GetFrameValueString(string sFrameID)
    {
      return GetFrameValueString(sFrameID, "");
    }

    private string GetFrameValueString(string sFrameID, string sFrameIDv2)
    {
      ID3Frame frame = GetFrame(sFrameID, sFrameIDv2);

      if (frame == null)
        return string.Empty;

      return frame.Data;
    }

    private string GetFrameValueStringSearch(string sPartialFieldName)
    {
      if (sPartialFieldName.Length < 5)
        return string.Empty;

      sPartialFieldName = sPartialFieldName.ToLower();

      foreach (ID3Frame frame in Id3Tag.ID3Frames)
      {
      }

      return string.Empty;
    }

    private int GetFrameValueInt(string sFrameID)
    {
      return GetFrameValueInt(sFrameID, false);
    }

    private int GetFrameValueInt(string sFrameID, bool returnNegativeOnFail)
    {
      string sVal = GetFrameValueString(sFrameID);

      if (sVal.Length == 0)
        return returnNegativeOnFail ? -1 : 0;

      try
      {
        return int.Parse(sVal);
      }

      catch
      {
        return returnNegativeOnFail ? -1 : 0;
      }
    }

    private byte[] GetValueBinary(string sFrameID)
    {
      ID3Frame frame = GetFrame(sFrameID);

      if (frame == null)
        return null;

      return frame.BinaryData;
    }

    private string GetUnsynchronizedLyrics()
    {
      byte[] lyrics = GetValueBinary(FrameNames.USLT);

      if (lyrics == null || lyrics.Length == 0)
        lyrics = GetValueBinary(FrameNamesV2.ULT);

      if (lyrics == null || lyrics.Length == 0)
        return string.Empty;

      MemoryStream s = null;

      try
      {
        int textEncoding = lyrics[0];

        switch (textEncoding)
        {
          // Meaning of the encoding byte:
          // $00   ISO-8859-1 [ISO-8859-1]. Terminated with $00.
          // $01   UTF-16 [UTF-16] encoded Unicode [UNICODE] with BOM. 
          //       All strings in the same frame SHALL have the same byteorder. Terminated with $00 00.
          // $02   UTF-16BE [UTF-16] encoded Unicode [UNICODE] without BOM. Terminated with $00 00.
          // $03   UTF-8 [UTF-8] encoded Unicode [UNICODE]. Terminated with $00.

          case 0: // ISO-8859-1. Use Default Encoding
            lyrics = Encoding.Convert(Encoding.Default, Encoding.Unicode, lyrics, 1, lyrics.Length - 1);
            break;

          case 1:
            break;

          case 2:
            lyrics = Encoding.Convert(Encoding.BigEndianUnicode, Encoding.Unicode, lyrics, 1, lyrics.Length - 1);
            break;

          case 3:
            lyrics = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, lyrics, 1, lyrics.Length - 1);

            break;
        }

        s = new MemoryStream(lyrics);

        byte[] lang = new byte[6];
        s.Read(lang, 0, lang.Length);

        // Find the "Content descriptor" and ignore it
        char curChar = 'a';
        char lastChar = 'a';
        string contentDesc = string.Empty;
        int charCount = 0;
        bool isBigEndian = false;

        while (s.Position != s.Length)
        {
          charCount++;
          lastChar = curChar;
          curChar = (char)s.ReadByte();

          if (textEncoding == 0 || textEncoding == 1 || textEncoding == 3)
          {
            if (curChar == '\0' && lastChar == '\0')
            {
              // Make sure we've stopped on a character boundry
              if (charCount % 2 != 0)
                continue;

              // UTF-16 with BOM
              if (textEncoding == 1)
              {
                // Read the BOM so we know how to convert this.
                byte[] bom = new byte[2];
                bom[0] = lyrics[s.Position];
                bom[1] = lyrics[s.Position + 1];

                isBigEndian = Utils.UTF16HasBigEndianBOM(bom);
              }

              break;
            }
          }

          else
          {
            if (curChar == '\0')
              break;
          }

          contentDesc += curChar;
        }

        if (s.Position == s.Length)
          return string.Empty;

        lyrics = new byte[s.Length - s.Position];
        s.Read(lyrics, 0, lyrics.Length);

        if (isBigEndian)
          lyrics = Encoding.Convert(Encoding.BigEndianUnicode, Encoding.Unicode, lyrics, 0, lyrics.Length);

        string sLyrics = Encoding.Unicode.GetString(lyrics).Trim();
        return sLyrics;
      }

      catch (Exception ex)
      {
        Log.Error("MP3Tag.GetUnsynchronizedLyrics caused an exception in file {0} : {1}", base.FileName, ex.Message);
        return string.Empty;
      }

      finally
      {
        if (s != null)
        {
          s.Close();
          s = null;
        }
      }
    }
    #endregion
  }
}

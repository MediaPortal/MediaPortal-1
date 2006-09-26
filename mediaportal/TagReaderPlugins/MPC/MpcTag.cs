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
using System.Runtime.InteropServices;
using MediaPortal.TagReader;
using Tag.MAC;

using MediaPortal.GUI.Library;

namespace Tag.MPC
{
  public class MpcTag : ApeTag
  {
    internal struct MpcHeaderInfo
    {
      internal int SampleRate;
      internal int Channels;
      internal long HeaderPosition;
      internal long HeaderLength;
      internal int MPCStreamVersion;
      internal int Bitrate;
      internal double AverageBitrate;
      internal uint Frames;
      internal UInt64 PCMSamples;
      internal uint MaxBand;
      internal uint IntensityStereo;
      internal uint MidSideStereo;
      internal uint BlockSize;
      internal uint Profile;
      internal string ProfileName;

      internal short GainTitle;
      internal short GainAlbum;
      internal ushort PeakAlbum;
      internal ushort PeakTitle;

      internal uint IsTrueGapless;
      internal uint LastFrameSamples;

      internal uint EncoderVersion;
      internal string Encoder;
    }

    string[] ProfileNames = new string[]
            {
                "Uknown", 
                "'Unstable/Experimental'", 
                "Uknown", 
                "Uknown",
                "Uknown", 
                "Below 'Telephone'", 
                "Below 'Telephone'", 
                "'Telephone'",
                "'Thumb'", 
                "'Radio'", 
                "'Standard'", 
                "'Xtreme'",
                "'Insane'", 
                "'BrainDead'", 
                "above 'BrainDead'", 
                "above 'BrainDead'"
            };

    int[] SampleRates = new int[]
            {
                44100,
                48000,
                37800,
                32000
            };

    #region Variables

    internal MpcHeaderInfo HeaderInfo = new MpcHeaderInfo();

    #endregion

    #region Properties

    override public string Album
    {
      get { return base.Album; }
    }

    override public string Artist
    {
      get { return base.Artist; }
    }

    override public string AlbumArtist
    {
      get { return base.AlbumArtist; }
    }

    override public string ArtistURL
    {
      get { return base.ArtistURL; }
    }

    override public int AverageBitrate
    {
      get { return (int)(HeaderInfo.AverageBitrate / 1000 + .5); }
    }

    override public int BitsPerSample
    {
      get { return base.BitsPerSample; }
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
      get { return HeaderInfo.Channels; }
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
      get { return base.CoverArtImageBytes; }
    }

    override public string FileURL
    {
      get { return base.FileURL; }
    }

    override public int FormatFlags
    {
      get { return base.FormatFlags; }
    }

    override public string Genre
    {
      get { return base.Genre; }

    }

    override public bool IsVBR
    {
      get { return base.IsVBR; }
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
        int frameLength = 1152;
        double framesSize = ((double)HeaderInfo.Frames - .5) * frameLength;
        int durationSecs = (int)(framesSize / HeaderInfo.SampleRate);

        return durationSecs * 1000;
      }
    }

    override public string Lyrics
    {
      get { return base.Lyrics; }
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
      get { return HeaderInfo.GainAlbum.ToString(); }
    }

    override public string ReplayGainRadio
    {
      get { return base.ReplayGainRadio; }
    }

    override public int SampleRate
    {
      get { return HeaderInfo.SampleRate; }
    }

    override public string Title
    {
      get { return base.Title; }
    }

    override public string ToolName
    {
      get { return base.ToolName; }
    }

    override public string ToolVersion
    {
      get { return ToolVersion; }
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
      get { return base.Track; }
    }

    override public string Version
    {
      get { return base.Version; }
    }

    override public int Year
    {
      get { return base.Year; }
    }

    #endregion

    public MpcTag()
      : base()
    {
    }

    public MpcTag(string fileName)
      : base(fileName)
    {
      Read(fileName);
    }

    ~MpcTag()
    {
      Dispose();
    }

    override public bool SupportsFile(string strFileName)
    {
      if (System.IO.Path.GetExtension(strFileName).ToLower() == ".mpc") return true;
      return false;
    }

    override public bool Read(string fileName)
    {
      if (fileName.Length == 0)
        throw new Exception("No file name specified");

      if (!File.Exists(fileName))
        throw new Exception("Unable to open file.  File does not exist.");

      if (Path.GetExtension(fileName).ToLower() != ".mpc")
        throw new AudioFileTypeException("Expected MPC file type.");

      AudioFilePath = fileName;
      FileName = Path.GetFileName(fileName);
      FileLength = new FileInfo(fileName).Length;

      bool result = true;

      if (AudioFileStream == null)
        AudioFileStream = new FileStream(this.AudioFilePath, FileMode.Open, FileAccess.Read);

      try
      {
        if (!ReadMpcHeader())
          return false;

        if (!ReadTags())
          return false;
      }

      catch (Exception ex)
      {
        Log.Error("MPCTag.Read caused an exception: {0}", ex.Message);
        result = false;
      }

      return result;
    }

    private bool ReadMpcHeader()
    {
      bool result;
      AudioFileStream.Seek(0, SeekOrigin.Begin);

      byte[] buffer = new byte[3];
      AudioFileStream.Read(buffer, 0, 3);

      if (buffer[0] != 'M' || buffer[1] != 'P' || buffer[2] != '+')
        return false;

      HeaderInfo.MPCStreamVersion = AudioFileStream.ReadByte();
      result = ReadStream(HeaderInfo.MPCStreamVersion);
      HeaderInfo.Channels = 2;

      HeaderInfo.HeaderLength = AudioFileStream.Position - HeaderInfo.HeaderPosition;


      HeaderInfo.PCMSamples = 1152 * HeaderInfo.Frames - 576; // estimation, exact value takes too much time

      if (HeaderInfo.PCMSamples != 0)
        HeaderInfo.AverageBitrate = (ulong)((AudioFileStream.Length - HeaderInfo.HeaderPosition) * 8 * HeaderInfo.SampleRate) / HeaderInfo.PCMSamples;

      else
        HeaderInfo.AverageBitrate = 0;

      return result;
    }

    private bool ReadStream(int version)
    {
      if (version <= 6)
        return ReadMpcStreamV6();

      else if (version >= 7)
        return ReadMpcStreamV7();

      else return false;
    }

    private bool ReadMpcStreamV6()
    {
      Log.Error("MPCTag.ReadMpcStreamV6 not implemented!");
      bool result = false;

      try
      {
        /*
            if ( fseek ( fp, Info->simple.HeaderPosition, SEEK_SET ) != 0 )         // seek to header start
                return 1;
            if ( fread ( HeaderData, 1, sizeof HeaderData, fp ) != sizeof HeaderData )
                return 1;

            Info->simple.Bitrate          = (HeaderData[0]>>23) & 0x01FF;           // read the file-header (SV6 and below)
            Info->simple.IS               = (HeaderData[0]>>22) & 0x0001;
            Info->simple.MS               = (HeaderData[0]>>21) & 0x0001;
            Info->simple.StreamVersion    = (HeaderData[0]>>11) & 0x03FF;
            Info->simple.MaxBand          = (HeaderData[0]>> 6) & 0x001F;
            Info->simple.BlockSize        = (HeaderData[0]    ) & 0x003F;
            Info->simple.Profile          = 0;
            Info->simple.ProfileName      = "unknown";
            if ( Info->simple.StreamVersion >= 5 )
                Info->simple.Frames       =  HeaderData[1];                         // 32 bit
            else
                Info->simple.Frames       = (HeaderData[1]>>16);                    // 16 bit

            Info->simple.GainTitle        = 0;                                      // not supported
            Info->simple.PeakTitle        = 0;
            Info->simple.GainAlbum        = 0;
            Info->simple.PeakAlbum        = 0;

            Info->simple.LastFrameSamples = 0;
            Info->simple.IsTrueGapless    = 0;
            Info->simple.EncoderVersion   = 0;
            Info->simple.Encoder[0]       = '\0';

            if ( Info->simple.StreamVersion == 7 ) return 1;                        // are there any unsupported parameters used?
            if ( Info->simple.Bitrate       != 0 ) return 1;
            if ( Info->simple.IS            != 0 ) return 1;
            if ( Info->simple.BlockSize     != 1 ) return 1;

            if ( Info->simple.StreamVersion < 6 )                                   // Bugfix: last frame was invalid for up to SV5
                Info->simple.Frames -= 1;

            Info->simple.SampleFreq    = 44100;                                     // AB: used by all files up to SV7
            Info->simple.Channels      = 2;

            if ( Info->simple.StreamVersion < 4  ||  Info->simple.StreamVersion > 7 )
                return 1;

            return 0; */
      }

      catch (Exception ex)
      {
        Log.Error("MPCTag.ReadMpcStreamV6 caused an exception: {0}", ex.Message);
        result = false;
      }

      return result;
    }

    private bool ReadMpcStreamV7()
    {
      bool result = true;

      try
      {
        byte[] buffer = new byte[4];
        AudioFileStream.Read(buffer, 0, buffer.Length);
        HeaderInfo.Frames = BitConverter.ToUInt32(buffer, 0);

        HeaderInfo.Bitrate = 0;

        buffer = new byte[4];
        AudioFileStream.Read(buffer, 0, buffer.Length);
        uint flag1 = BitConverter.ToUInt32(buffer, 0);

        HeaderInfo.IntensityStereo = 0;
        HeaderInfo.MidSideStereo = (flag1 >> 30) & 0x0001;
        HeaderInfo.MaxBand = (flag1 >> 24) & 0x003F; ;
        HeaderInfo.BlockSize = 1;
        HeaderInfo.Profile = (flag1 << 8) >> 28;

        if (HeaderInfo.Profile >= 0 && HeaderInfo.Profile < ProfileNames.Length)
          HeaderInfo.ProfileName = ProfileNames[HeaderInfo.Profile];

        int sampleRateIndex = (int)((flag1 >> 16) & 0x003);

        if (sampleRateIndex >= 0 && sampleRateIndex < SampleRates.Length)
          HeaderInfo.SampleRate = SampleRates[sampleRateIndex];

        ushort EstimatedPeakTitle = (byte)(flag1 & 0xFFFF);

        buffer = new byte[4];
        AudioFileStream.Read(buffer, 0, buffer.Length);
        uint flag2 = BitConverter.ToUInt32(buffer, 0);

        HeaderInfo.GainTitle = (byte)(flag2 & 0xffff);
        HeaderInfo.PeakTitle = (byte)((flag2 >> 16) & 0xffff);

        buffer = new byte[4];
        AudioFileStream.Read(buffer, 0, buffer.Length);
        uint flag3 = BitConverter.ToUInt32(buffer, 0);

        HeaderInfo.GainAlbum = (byte)(flag3 & 0xffff);
        HeaderInfo.PeakAlbum = (byte)((flag3 >> 16) & 0xffff);

        buffer = new byte[4];
        AudioFileStream.Read(buffer, 0, buffer.Length);
        uint flag4 = BitConverter.ToUInt32(buffer, 0);

        HeaderInfo.IsTrueGapless = (flag4 >> 31) & 0x0001;
        HeaderInfo.LastFrameSamples = (flag4 >> 20) & 0x07ff;


        buffer = new byte[4];
        AudioFileStream.Read(buffer, 0, buffer.Length);
        uint flag5 = BitConverter.ToUInt32(buffer, 0);

        HeaderInfo.EncoderVersion = (flag5 >> 24) & 0x00ff;

        if (HeaderInfo.EncoderVersion == 0)
          HeaderInfo.Encoder = "Buschmann 1.7.0...9, Klemm 0.90...1.05";

        else
        {
          switch (HeaderInfo.EncoderVersion % 10)
          {
            case 0:
              HeaderInfo.Encoder = string.Format("Release {0}.{1:D2}", HeaderInfo.EncoderVersion / 100, HeaderInfo.EncoderVersion % 100);
              break;

            case 2:
            case 4:
            case 6:
            case 8:
              HeaderInfo.Encoder = string.Format("Beta {0}.{1:D2}", HeaderInfo.EncoderVersion / 100, HeaderInfo.EncoderVersion % 100);
              break;

            default:
              HeaderInfo.Encoder = string.Format("Alpha {0}.{1:D2}", HeaderInfo.EncoderVersion / 100, HeaderInfo.EncoderVersion % 100);
              break;
          }
        }

        if (HeaderInfo.PeakTitle == 0)
          HeaderInfo.PeakTitle = (ushort)(EstimatedPeakTitle * 1.18);

        if (HeaderInfo.PeakAlbum == 0)
          HeaderInfo.PeakAlbum = HeaderInfo.PeakTitle;
      }

      catch (Exception ex)
      {
        Log.Error("    MPCTag.ReadMpcStreamV7 caused an exception: {0}", ex.Message);
        result = false;
      }

      return result;
    }
  }
}
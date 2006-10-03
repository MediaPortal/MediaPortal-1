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
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.TagReader;
using Tag.MAC;

namespace Tag.MPC
{
  public class MpcTag : ApeTag
  {
    #region Variables
    internal struct MpcHeaderInfo
    {
      public int SampleRate;
      public int Channels;
      public long HeaderPosition;
      public long HeaderLength;
      public int MPCStreamVersion;
      public int Bitrate;
      public double AverageBitrate;
      public uint Frames;
      public UInt64 PCMSamples;
      public uint MaxBand;
      public uint IntensityStereo;
      public uint MidSideStereo;
      public uint BlockSize;
      public uint Profile;
      public string ProfileName;

      public short GainTitle;
      public short GainAlbum;
      public ushort PeakAlbum;
      public ushort PeakTitle;

      public uint IsTrueGapless;
      public uint LastFrameSamples;

      public uint EncoderVersion;
      public string Encoder;

      public static MpcHeaderInfo Empty
      {
        get
        {
          MpcHeaderInfo mhi = new MpcHeaderInfo();
          mhi.SampleRate = 0;
          mhi.Channels = 0;
          mhi.HeaderPosition = 0;
          mhi.HeaderLength = 0;
          mhi.MPCStreamVersion = 0;
          mhi.Bitrate = 0;
          mhi.AverageBitrate = 0;
          mhi.Frames = 0;
          mhi.PCMSamples = 0;
          mhi.MaxBand = 0;
          mhi.IntensityStereo = 0;
          mhi.MidSideStereo = 0;
          mhi.BlockSize = 0;
          mhi.Profile = 0;
          mhi.ProfileName = String.Empty;
          mhi.GainTitle = 0;
          mhi.GainAlbum = 0;
          mhi.PeakAlbum = 0;
          mhi.PeakTitle = 0;
          mhi.IsTrueGapless = 0;
          mhi.LastFrameSamples = 0;
          mhi.EncoderVersion = 0;
          mhi.Encoder = String.Empty;
          return mhi;
        }
      }
    }

    private string[] ProfileNames = new string[]
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

    private int[] SampleRates = new int[]
      {
        44100,
        48000,
        37800,
        32000
      };

    internal MpcHeaderInfo HeaderInfo = new MpcHeaderInfo();
    #endregion

    #region Constructors/Destructors
    public MpcTag()
      : base()
    {
      HeaderInfo = MpcHeaderInfo.Empty;
    }

    ~MpcTag()
    {
      Dispose();
    }
    #endregion

    #region Properties

    public override int AverageBitrate
    {
      get { return (int) (HeaderInfo.AverageBitrate/1000 + .5); }
    }

    public override int Channels
    {
      get { return HeaderInfo.Channels; }
    }

    public override string Length
    {
      get { return Utils.GetDurationString(LengthMS); }
    }

    public override int LengthMS
    {
      get
      {
        int frameLength = 1152;
        double framesSize = (HeaderInfo.Frames - .5)*frameLength;
        int durationSecs = (int) (framesSize/HeaderInfo.SampleRate);

        return durationSecs*1000;
      }
    }

    public override string ReplayGainAlbum
    {
      get { return HeaderInfo.GainAlbum.ToString(); }
    }

    public override int SampleRate
    {
      get { return HeaderInfo.SampleRate; }
    }

    #endregion

    #region Public Methods
    public override bool SupportsFile(string strFileName)
    {
      if (Path.GetExtension(strFileName).ToLower() == ".mpc")
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

      if (Path.GetExtension(fileName).ToLower() != ".mpc")
      {
        throw new AudioFileTypeException("Expected MPC file type.");
      }

      AudioFilePath = fileName;
      FileName = Path.GetFileName(fileName);
      FileLength = new FileInfo(fileName).Length;

      bool result = true;

      if (AudioFileStream == null)
      {
        AudioFileStream = new FileStream(AudioFilePath, FileMode.Open, FileAccess.Read);
      }

      try
      {
        if (!ReadMpcHeader())
        {
          return false;
        }

        if (!ReadTags())
        {
          return false;
        }
      }

      catch (Exception ex)
      {
        Log.Error("MPCTag.Read caused an exception in file {0} : {1}", FileName, ex.Message);
        result = false;
      }

      return result;
    }
    #endregion

    #region Private Methods
    private bool ReadMpcHeader()
    {
      bool result;
      AudioFileStream.Seek(0, SeekOrigin.Begin);

      byte[] buffer = new byte[3];
      AudioFileStream.Read(buffer, 0, 3);

      if (buffer[0] != 'M' || buffer[1] != 'P' || buffer[2] != '+')
      {
        return false;
      }

      HeaderInfo.MPCStreamVersion = AudioFileStream.ReadByte();
      result = ReadStream(HeaderInfo.MPCStreamVersion);
      HeaderInfo.Channels = 2;

      HeaderInfo.HeaderLength = AudioFileStream.Position - HeaderInfo.HeaderPosition;


      HeaderInfo.PCMSamples = 1152*HeaderInfo.Frames - 576; // estimation, exact value takes too much time

      if (HeaderInfo.PCMSamples != 0)
      {
        HeaderInfo.AverageBitrate =
          (ulong) ((AudioFileStream.Length - HeaderInfo.HeaderPosition)*8*HeaderInfo.SampleRate)/HeaderInfo.PCMSamples;
      }

      else
      {
        HeaderInfo.AverageBitrate = 0;
      }

      return result;
    }

    private bool ReadStream(int version)
    {
      if (version <= 6)
      {
        return ReadMpcStreamV6();
      }

      else if (version >= 7)
      {
        return ReadMpcStreamV7();
      }

      else
      {
        return false;
      }
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
        Log.Error("MPCTag.ReadMpcStreamV6 caused an exception in file {0} : {1}", FileName, ex.Message);
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
        HeaderInfo.MaxBand = (flag1 >> 24) & 0x003F;
        ;
        HeaderInfo.BlockSize = 1;
        HeaderInfo.Profile = (flag1 << 8) >> 28;

        if (HeaderInfo.Profile >= 0 && HeaderInfo.Profile < ProfileNames.Length)
        {
          HeaderInfo.ProfileName = ProfileNames[HeaderInfo.Profile];
        }

        int sampleRateIndex = (int) ((flag1 >> 16) & 0x003);

        if (sampleRateIndex >= 0 && sampleRateIndex < SampleRates.Length)
        {
          HeaderInfo.SampleRate = SampleRates[sampleRateIndex];
        }

        ushort EstimatedPeakTitle = (byte) (flag1 & 0xFFFF);

        buffer = new byte[4];
        AudioFileStream.Read(buffer, 0, buffer.Length);
        uint flag2 = BitConverter.ToUInt32(buffer, 0);

        HeaderInfo.GainTitle = (byte) (flag2 & 0xffff);
        HeaderInfo.PeakTitle = (byte) ((flag2 >> 16) & 0xffff);

        buffer = new byte[4];
        AudioFileStream.Read(buffer, 0, buffer.Length);
        uint flag3 = BitConverter.ToUInt32(buffer, 0);

        HeaderInfo.GainAlbum = (byte) (flag3 & 0xffff);
        HeaderInfo.PeakAlbum = (byte) ((flag3 >> 16) & 0xffff);

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
        {
          HeaderInfo.Encoder = "Buschmann 1.7.0...9, Klemm 0.90...1.05";
        }

        else
        {
          switch (HeaderInfo.EncoderVersion%10)
          {
            case 0:
              HeaderInfo.Encoder =
                string.Format("Release {0}.{1:D2}", HeaderInfo.EncoderVersion/100, HeaderInfo.EncoderVersion%100);
              break;

            case 2:
            case 4:
            case 6:
            case 8:
              HeaderInfo.Encoder =
                string.Format("Beta {0}.{1:D2}", HeaderInfo.EncoderVersion/100, HeaderInfo.EncoderVersion%100);
              break;

            default:
              HeaderInfo.Encoder =
                string.Format("Alpha {0}.{1:D2}", HeaderInfo.EncoderVersion/100, HeaderInfo.EncoderVersion%100);
              break;
          }
        }

        if (HeaderInfo.PeakTitle == 0)
        {
          HeaderInfo.PeakTitle = (ushort) (EstimatedPeakTitle*1.18);
        }

        if (HeaderInfo.PeakAlbum == 0)
        {
          HeaderInfo.PeakAlbum = HeaderInfo.PeakTitle;
        }
      }

      catch (Exception ex)
      {
        Log.Error("MPCTag.ReadMpcStreamV7 caused an exception in file {0} : {1}", FileName, ex.Message);
        result = false;
      }

      return result;
    }
    #endregion
  }
}
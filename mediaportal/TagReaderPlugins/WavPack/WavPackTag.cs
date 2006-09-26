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

namespace Tag.WavPack
{
  public class WavPackTag : ApeTag
  {
    int[] SampleRates = new int[] 
        { 
            6000, 
            8000, 
            9600, 
            11025, 
            12000, 
            16000, 
            22050,
            24000, 
            32000, 
            44100, 
            48000, 
            64000, 
            88200, 
            96000, 
            192000 
        };

    internal struct WavPackHeader
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
      internal char[] ID;                 // wvpk

      internal UInt32 BlockSize;          // frame length not including ID and BlockSize
      internal short Version;             // Currently 0x403
      internal byte Track;                // Unused - 0
      internal byte IndexNumber;          // Unused - 0
      internal UInt32 TotalSamples;       // Total sample count in file. -1 if unknown
      internal UInt32 BlockIndex;         // 
      internal UInt32 BlockSamples;       // Number os samples in this block
      internal UInt32 Flags;
      internal UInt32 CRC;                // CRC for decoded data
    };

    #region Constants

    private const int INITIAL_BLOCK = 0x800;    // initial block of multichannel segment
    private const int FINAL_BLOCK = 0x1000; 	// final block of multichannel segment
    private const int SRATE_LSB = 23;
    private const int SRATE_MASK = (0xf << SRATE_LSB);

    private const int BYTE_PER_SAMPLE = 0x0003;
    private const int MONO = 0x0004;
    private const int HYBRID = 0x0008;
    private const int JOINT_STEREO = 0x0010;
    private const int CROSS_DECORRELATION = 0x0020;
    private const int HYBRID_NOISESHAPE = 0x0040;
    private const int IEEE_32BIT_FLOAT = 0x0080;
    private const int INT_32BIT = 0x0100;
    private const int HYBRID_BITRATE_NOISE = 0x0200;
    private const int HYBRID_BALANCE_NOISE = 0x0400;
    private const int MULTICHANNEL_INITIAL = 0x0800;


    private int _SampleRate = 0;
    private int _BitsPerSample = 0;
    private int _Channels = 0;
    private int _BitRate = 0;
    private string _Encoding = "";
    private string _ChannelType = "";


    #endregion

    #region Variables

    private WavPackHeader FirstHeader;

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
      get
      {
        if (_BitRate > 0)
          return _BitRate / 1000;

        return 0;
      }
    }

    override public int BitsPerSample
    {
      get { return _BitsPerSample; }
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
      get { return _Channels; }
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
        try
        {
          return (int)((FirstHeader.TotalSamples / (uint)_SampleRate) * 1000);
        }

        catch (Exception ex)
        {
          Log.Error("WavPackTag.get_LengthMS caused an exception in file {0} : {1}", base.FileName, ex.Message);
          return 0;
        }
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
      get { return base.ReplayGainAlbum; }
    }

    override public string ReplayGainRadio
    {
      get { return base.ReplayGainRadio; }
    }

    override public int SampleRate
    {
      get { return _SampleRate; }
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
      get { return base.Track; }
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
          string sYear = GetFieldString(ApeTag.APE_TAG_FIELD_YEAR);
          return Utils.GetYear(sYear);
        }

        catch (Exception ex)
        {
          Log.Error("WavPackTag.get_Year caused an exception in file {0} : {1}", base.FileName, ex.Message);
          return 0;
        }
      }
    }

    #endregion

    public WavPackTag()
      : base()
    {
    }

    public WavPackTag(string fileName)
      : base(fileName)
    {
      Read(fileName);
    }

    ~WavPackTag()
    {
      Dispose();
    }

    override public bool SupportsFile(string strFileName)
    {
      if (System.IO.Path.GetExtension(strFileName).ToLower() == ".wv") return true;
      return false;
    }

    override public bool Read(string fileName)
    {
      if (fileName.Length == 0)
        throw new Exception("No file name specified");

      if (!File.Exists(fileName))
        throw new Exception("Unable to open file.  File does not exist.");

      if (Path.GetExtension(fileName).ToLower() != ".wv")
        throw new AudioFileTypeException("Expected WavPack file type.");

      AudioFilePath = fileName;
      FileName = Path.GetFileName(fileName);
      FileLength = new FileInfo(fileName).Length;

      bool result = true;

      if (AudioFileStream == null)
        AudioFileStream = new FileStream(this.AudioFilePath, FileMode.Open, FileAccess.Read);

      try
      {
        if (!ReadTags())
          return false;

        if (!ReadHeader())
          return false;

        if (HasApeV1Tags())
        {
        }

        else if (HasId3Tag)
        {
        }

        else
        {
        }
      }

      catch (Exception ex)
      {
        Log.Error("WavPackTag.Read caused an exception in file {0} : {1}", base.FileName, ex.Message);
        result = false;
      }

      return result;
    }

    new private bool ReadHeader()
    {
      bool result = true;

      try
      {
        AudioFileStream.Seek(0, SeekOrigin.Begin);
        int readChunkSize = 1024;
        int totalBytesRead = 0;
        int maxReadBytes = 1024 * 1024;
        bool headerFound = false;
        byte[] buffer;

        while (!headerFound)
        {
          int readSize = (int)Math.Min(readChunkSize, AudioFileStream.Length - AudioFileStream.Position);
          buffer = new byte[readSize];
          int bytesRead = AudioFileStream.Read(buffer, 0, readSize);
          totalBytesRead += bytesRead;

          for (int i = 0; i < buffer.Length; i += 4)
          {
            if (i + 4 > buffer.Length)
              break;

            if (buffer[i + 0] == 'w' && buffer[i + 1] == 'v'
                && buffer[i + 2] == 'p' && buffer[i + 3] == 'k')
            {
              headerFound = true;
              AudioFileStream.Seek(-(bytesRead - i), SeekOrigin.Current);
              break;
            }
          }

          if (totalBytesRead >= maxReadBytes)
            break;
        }

        if (!headerFound)
          return false;

        FirstHeader = new WavPackHeader();
        int headerSize = Marshal.SizeOf(FirstHeader);
        buffer = new byte[headerSize];
        AudioFileStream.Read(buffer, 0, headerSize);
        FirstHeader = (WavPackHeader)Utils.RawDeserializeEx(buffer, typeof(WavPackHeader));

        if (FirstHeader.BlockSamples > 0)
        {
          if ((FirstHeader.Flags & INITIAL_BLOCK) > 0 && FirstHeader.BlockIndex != 0)
          {
            //Console.WriteLine("May not be at first block!");
          }

          uint sampleRateIndex = (FirstHeader.Flags & SRATE_MASK) >> SRATE_LSB;

          if (sampleRateIndex >= 0 && sampleRateIndex < SampleRates.Length)
            _SampleRate = SampleRates[sampleRateIndex];

          _BitsPerSample = (int)(1 + (FirstHeader.Flags & BYTE_PER_SAMPLE)) * 8;
          _Channels = (FirstHeader.Flags & MONO) > 0 ? 1 : 2;
          _Encoding = (FirstHeader.Flags & HYBRID) > 0 ? "Hybrid" : "Lossless";
          _ChannelType = (FirstHeader.Flags & MONO) > 0 ? "Mono" : "Stereo";


          _BitRate = 0;

          if (FirstHeader.TotalSamples > 0)
          {
            double output_time = (double)FirstHeader.TotalSamples / _SampleRate;
            double input_size = AudioFileStream.Length - (AudioFileStream.Position + (AudioFileStream.Length - ApeTagStreamPosition));

            if (output_time >= 1.0 && input_size >= 1.0)
              _BitRate = (int)(input_size * 8.0 / output_time);
          }
        }
      }

      catch (Exception ex)
      {
        Log.Error("WavPackTag.ReadHeader caused an exception in file {0} : {1}", base.FileName, ex.Message);
        result = false;
      }

      return result;
    }
  }
}

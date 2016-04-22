#region Copyright (C) 2005-2016 Team MediaPortal

// Copyright (C) 2005-2016 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;

using MediaPortal.Drawing;

namespace MediaPortal.Player
{
  public enum AspectRatio
  {
    Opaque, // 1:1
    HighEndDataGraphics, // 5:4
    FullScreen, // 4:3
    StandartSlides, // 3:3
    DigitalSlrCameras, // 3:2
    HighDefinitionTv, // 16:9
    WideScreenDisplay, // 16:10
    WideScreen, // 1.85:1
    CinimaScope // 21:9
  }

  public enum StereoMode
  {
    Mono,
    SideBySideLeft,
    TopBottomRight,
    TopBottomLeft,
    CheckboardRight,
    CheckboardLeft,
    RowInterleavedRight,
    RowInterleavedLeft,
    ColumnInterleavedRight,
    ColumnInterleavedLeft,
    AnaglyphCyanRed,
    SideBySideRight,
    AnaglyphGreenMagenta,
    BothEyesLacedLeft,
    BothEyesLacedRight
  }

  public enum VideoCodec
  {
    V_UNDEFINED,
    V_UNCOMPRESSED,
    V_DIRAC,
    V_MPEG4,
    V_MPEG4_IS0_SP,
    V_MPEG4_IS0_ASP,
    V_MPEG4_IS0_AP,
    V_MPEG4_IS0_AVC,
    V_MPEG4_ISO_SP,
    V_MPEG4_ISO_ASP,
    V_MPEG4_ISO_AP,
    V_MPEG4_ISO_AVC,
    V_MPEGH_ISO_HEVC,
    V_MPEG4_MS_V1,
    V_MPEG4_MS_V2,
    V_MPEG4_MS_V3,
    V_VC1,
    V_MPEG1,
    V_MPEG2,
    V_PRORES,
    V_REAL_RV10,
    V_REAL_RV20,
    V_REAL_RV30,
    V_REAL_RV40,
    V_THEORA,
    V_VP6,
    V_VP8,
    V_VP9,
    V_DIVX1,
    V_DIVX2,
    V_DIVX3,
    V_DIVX4,
    V_DIVX50,
    V_XVID,
    V_SVQ1,
    V_SVQ2,
    V_SVQ3,
    V_SPRK,
    V_H260,
    V_H261,
    V_H263,
    V_AVDV,
    V_AVD1,
    V_FFV1,
    V_FFV2,
    V_IV21,
    V_IV30,
    V_IV40,
    V_IV50,
    V_FFDS,
    V_FRAPS,
    V_FFVH,
    V_MJPG,
    V_DV,
    V_HDV,
    V_DVCPRO50,
    V_DVCPRO100,
    V_WMV1,
    V_WMV2,
    V_WMV3,
    V_8BPS,
    V_BINKVIDEO,
  }

  public class VideoStream : LanguageMediaStream
  {
    #region match dictionaries

    private static readonly Dictionary<string, AspectRatio> Ratios = new Dictionary<string, AspectRatio>
    {
      { "1:1", AspectRatio.Opaque },
      { "5:4", AspectRatio.HighEndDataGraphics },
      { "1.2", AspectRatio.HighEndDataGraphics },
      { "3:3", AspectRatio.StandartSlides },
      { "4:3", AspectRatio.FullScreen },
      { "1.334", AspectRatio.FullScreen },
      { "3:2", AspectRatio.DigitalSlrCameras },
      { "1.5", AspectRatio.DigitalSlrCameras },
      { "16:9", AspectRatio.HighDefinitionTv },
      { "1.778", AspectRatio.HighDefinitionTv },
      { "16:10", AspectRatio.WideScreenDisplay },
      { "1.6", AspectRatio.WideScreenDisplay },
      { "1.85", AspectRatio.WideScreen },
      { "21:9", AspectRatio.CinimaScope },
      { "2.334", AspectRatio.CinimaScope }
    };

    private static readonly Dictionary<string, StereoMode> StereoModes = new Dictionary<string, StereoMode>
    {
      { "side by side (left eye first)", StereoMode.SideBySideLeft },
      { "top-bottom (right eye first)", StereoMode.TopBottomRight },
      { "top-bottom (left eye first)", StereoMode.TopBottomLeft },
      { "checkboard (right eye first)", StereoMode.CheckboardRight },
      { "checkboard (left eye first)", StereoMode.CheckboardLeft },
      { "row interleaved (right eye first)", StereoMode.RowInterleavedRight },
      { "row interleaved (left eye first)", StereoMode.RowInterleavedLeft },
      { "column interleaved (right eye first)", StereoMode.ColumnInterleavedRight },
      { "column interleaved (left eye first)", StereoMode.ColumnInterleavedLeft },
      { "anaglyph (cyan/red)", StereoMode.AnaglyphCyanRed },
      { "side by side (right eye first)", StereoMode.SideBySideRight },
      { "anaglyph (green/magenta)", StereoMode.AnaglyphGreenMagenta },
      { "both eyes laced in one block (left eye first)", StereoMode.BothEyesLacedLeft },
      { "both eyes laced in one block (right eye first)", StereoMode.BothEyesLacedRight }
    };

    private static readonly Dictionary<string, VideoCodec> VideoCodecs = new Dictionary<string, VideoCodec>
    {
      { "V_UNCOMPRESSED", VideoCodec.V_UNCOMPRESSED },
      { "V_DIRAC", VideoCodec.V_DIRAC },
      { "V_MPEG4/IS0/SP", VideoCodec.V_MPEG4_IS0_SP },
      { "V_MPEG4/IS0/ASP", VideoCodec.V_MPEG4_IS0_ASP },
      { "V_MPEG4/IS0/AP", VideoCodec.V_MPEG4_IS0_AP },
      { "V_MPEG4/IS0/AVC", VideoCodec.V_MPEG4_IS0_AVC },
      { "V_MPEG4/ISO/SP", VideoCodec.V_MPEG4_ISO_SP },
      { "V_MPEG4/ISO/ASP", VideoCodec.V_MPEG4_ISO_ASP },
      { "V_MPEG4/ISO/AP", VideoCodec.V_MPEG4_ISO_AP },
      { "V_MPEG4/ISO/AVC", VideoCodec.V_MPEG4_ISO_AVC },
      { "V_MPEGH/ISO/HEVC", VideoCodec.V_MPEGH_ISO_HEVC },
      { "V_MPEG4/MS/V2", VideoCodec.V_MPEG4_MS_V2 },
      { "V_MPEG4/MS/V3", VideoCodec.V_MPEG4_MS_V3 },
      { "V_MPEG1", VideoCodec.V_MPEG1 },
      { "V_MPEG2", VideoCodec.V_MPEG2 },
      { "V_PRORES", VideoCodec.V_PRORES },
      { "V_REAL/RV10", VideoCodec.V_REAL_RV10 },
      { "V_REAL/RV20", VideoCodec.V_REAL_RV20 },
      { "V_REAL/RV30", VideoCodec.V_REAL_RV30 },
      { "V_REAL/RV40", VideoCodec.V_REAL_RV40 },
      { "V_THEORA", VideoCodec.V_THEORA },
      { "V_VP8", VideoCodec.V_VP8 },
      { "V_VP9", VideoCodec.V_VP9 },
      { "AVC1", VideoCodec.V_MPEG4_ISO_AVC },
      { "AVC", VideoCodec.V_MPEG4_ISO_AVC },
      { "H264", VideoCodec.V_MPEG4_ISO_AVC },
      { "DAVC", VideoCodec.V_MPEG4_ISO_AVC },
      { "MPEG-2V", VideoCodec.V_MPEG2 },
      { "MPEG-2", VideoCodec.V_MPEG2 },
      { "MPEG-1", VideoCodec.V_MPEG1 },
      { "MPEG-1V", VideoCodec.V_MPEG1 },
      { "VC1", VideoCodec.V_VC1 },
      { "VC-1", VideoCodec.V_VC1 },
      { "OVC1", VideoCodec.V_VC1 },
      { "WVC1", VideoCodec.V_VC1 },
      { "SORENSON H263", VideoCodec.V_SPRK },
      { "SPRK", VideoCodec.V_SPRK },
      { "SVQ1", VideoCodec.V_SVQ1 },
      { "SVQ2", VideoCodec.V_SVQ2 },
      { "SVQ3", VideoCodec.V_SVQ3 },
      { "DX50", VideoCodec.V_DIVX50 },
      { "DVX1", VideoCodec.V_DIVX1 },
      { "DIV1", VideoCodec.V_DIVX1 },
      { "DVX2", VideoCodec.V_DIVX2 },
      { "DIV2", VideoCodec.V_DIVX2 },
      { "DVX3", VideoCodec.V_DIVX3 },
      { "DIV3", VideoCodec.V_DIVX3 },
      { "DIV4", VideoCodec.V_DIVX3 },
      { "DIV5", VideoCodec.V_DIVX50 },
      { "DIV6", VideoCodec.V_MPEG4_MS_V3 },
      { "DIVX", VideoCodec.V_DIVX4 },
      { "XVID", VideoCodec.V_XVID },
      { "FFV1", VideoCodec.V_FFV1 },
      { "FFV2", VideoCodec.V_FFV2 },
      { "S263", VideoCodec.V_H263 },
      { "H263", VideoCodec.V_H263 },
      { "D263", VideoCodec.V_H263 },
      { "L263", VideoCodec.V_H263 },
      { "M263", VideoCodec.V_H263 },
      { "ILVR", VideoCodec.V_H263 },
      { "S261", VideoCodec.V_H261 },
      { "H261", VideoCodec.V_H261 },
      { "D261", VideoCodec.V_H261 },
      { "L261", VideoCodec.V_H261 },
      { "M261", VideoCodec.V_H261 },
      { "IF09", VideoCodec.V_H261 },
      { "H260", VideoCodec.V_H260 },
      { "IR21", VideoCodec.V_IV21 },
      { "IV30", VideoCodec.V_IV30 },
      { "IV31", VideoCodec.V_IV30 },
      { "IV32", VideoCodec.V_IV30 },
      { "IV33", VideoCodec.V_IV30 },
      { "IV34", VideoCodec.V_IV30 },
      { "IV35", VideoCodec.V_IV30 },
      { "IV36", VideoCodec.V_IV30 },
      { "IV37", VideoCodec.V_IV30 },
      { "IV38", VideoCodec.V_IV30 },
      { "IV39", VideoCodec.V_IV30 },
      { "IV40", VideoCodec.V_IV40 },
      { "IV41", VideoCodec.V_IV40 },
      { "IV42", VideoCodec.V_IV40 },
      { "IV43", VideoCodec.V_IV40 },
      { "IV44", VideoCodec.V_IV40 },
      { "IV45", VideoCodec.V_IV40 },
      { "IV46", VideoCodec.V_IV40 },
      { "IV47", VideoCodec.V_IV40 },
      { "IV48", VideoCodec.V_IV40 },
      { "IV49", VideoCodec.V_IV40 },
      { "IAN", VideoCodec.V_IV40 },
      { "IV50", VideoCodec.V_IV50 },
      { "RV10", VideoCodec.V_REAL_RV10 },
      { "RV13", VideoCodec.V_REAL_RV10 },
      { "RV20", VideoCodec.V_REAL_RV20 },
      { "RV30", VideoCodec.V_REAL_RV30 },
      { "RV40", VideoCodec.V_REAL_RV40 },
      { "FLV1", VideoCodec.V_SPRK },
      { "FLV4", VideoCodec.V_VP6 },
      { "FFVH", VideoCodec.V_FFVH },
      { "FFDS", VideoCodec.V_FFDS },
      { "FPS1", VideoCodec.V_FRAPS },
      { "M4S2", VideoCodec.V_MPEG4_MS_V2 },
      { "COL0", VideoCodec.V_MPEG4_MS_V3 },
      { "COL1", VideoCodec.V_MPEG4_MS_V3 },
      { "THEORA", VideoCodec.V_THEORA },
      { "MJPG", VideoCodec.V_MJPG },
      { "MPEG-4V", VideoCodec.V_MPEG4 },
      { "3IV0", VideoCodec.V_MPEG4 },
      { "3IV1", VideoCodec.V_MPEG4 },
      { "3IV2", VideoCodec.V_MPEG4 },
      { "3IVD", VideoCodec.V_MPEG4 },
      { "3IVX", VideoCodec.V_MPEG4 },
      { "3VID", VideoCodec.V_MPEG4 },
      { "AP41", VideoCodec.V_MPEG4 },
      { "AP42", VideoCodec.V_MPEG4 },
      { "ATM4", VideoCodec.V_MPEG4 },
      { "BLZ0", VideoCodec.V_MPEG4 },
      { "DM4V", VideoCodec.V_MPEG4 },
      { "DP02", VideoCodec.V_MPEG4 },
      { "FMP4", VideoCodec.V_MPEG4 },
      { "M4CC", VideoCodec.V_MPEG4 },
      { "MP41", VideoCodec.V_MPEG4_MS_V1 },
      { "MP42", VideoCodec.V_MPEG4_MS_V2 },
      { "MP43", VideoCodec.V_MPEG4_MS_V3 },
      { "DV", VideoCodec.V_DV },
      { "HDV", VideoCodec.V_HDV },
      { "WMV1", VideoCodec.V_WMV1 },
      { "WMV2", VideoCodec.V_WMV2 },
      { "WMV3", VideoCodec.V_WMV3 },
    };

    #endregion

    public VideoStream(MediaInfo info, int number, int position)
        : base(info, number, position)
    {
    }

    public VideoStream(int number, int position)
        : base(null, number, position)
    {
    }

    public override MediaStreamKind Kind
    {
      get { return MediaStreamKind.Video; }
    }

    protected override StreamKind StreamKind
    {
      get { return StreamKind.Video; }
    }

    public double FrameRate { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public AspectRatio AspectRatio { get; set; }

    public bool Interlaced { get; set; }

    public StereoMode Stereoscopic { get; set; }

    public string Format { get; set; }

    public VideoCodec Codec { get; set; }

    public TimeSpan Duration { get; set; }

    public int BitDepth { get; set; }

    public string CodecName { get; set; }

    public string Resolution
    {
      get { return GetVideoResolution(); }
    }

    public Size Size
    {
      get { return new Size(Width, Height); }
    }

    protected override void AnalyzeStreamInternal(MediaInfo info)
    {
      base.AnalyzeStreamInternal(info);
      FrameRate = GetDouble(info, "FrameRate");
      Width = GetInt(info, "Width");
      Height = GetInt(info, "Height");
      AspectRatio = GetAspectRatio(GetString(info, "DisplayAspectRatio"));
      Interlaced = GetInterlaced(GetString(info, "ScanType").ToLower());
      Stereoscopic = GetInt(info, "MultiView_Count") >= 2
                       ? GetStereoscopic(GetString(info, "MultiView_Layout").ToLower())
                       : StereoMode.Mono;
      Format = GetString(info, "Format");
      Codec = GetCodecId(GetString(info, "CodecID"));
      if (Codec == VideoCodec.V_UNDEFINED)
      {
        Codec = GetCodec(GetString(info, "Codec"));
      }

      Duration = TimeSpan.FromMilliseconds(GetDouble(info, "Duration"));
      BitDepth = GetInt(info, "BitDepth");
      CodecName = GetFullCodecName(info);
    }

    private static VideoCodec GetCodecId(string codec)
    {
      VideoCodec result;
      return VideoCodecs.TryGetValue(codec.ToUpper(), out result) ? result : VideoCodec.V_UNDEFINED;
    }

    private static VideoCodec GetCodec(string codec)
    {
      VideoCodec result;
      return VideoCodecs.TryGetValue(codec.ToUpper(), out result) ? result : VideoCodec.V_UNDEFINED;
    }

    private static StereoMode GetStereoscopic(string layout)
    {
      StereoMode result;
      return StereoModes.TryGetValue(layout, out result) ? result : StereoMode.Mono;
    }

    private static bool GetInterlaced(string source)
    {
      return source.Contains("interlaced");
    }

    private static AspectRatio GetAspectRatio(string source)
    {
      AspectRatio result;
      return Ratios.TryGetValue(source, out result) ? result : AspectRatio.Opaque;
    }

    private string GetVideoResolution()
    {
      string result;

      if (Width >= 7680 || Height >= 4320)
      {
        result = "4320";
      }
      else if (Width >= 3840 || Height >= 2160)
      {
        result = "2160";
      }
      else if (Width >= 1920 || Height >= 1080)
      {
        result = "1080";
      }
      else if (Width >= 1280 || Height >= 720)
      {
        result = "720";
      }
      else if (Height >= 576)
      {
        result = "576";
      }
      else if (Height >= 480)
      {
        result = "480";
      }
      else if (Height >= 360)
      {
        result = "360";
      }
      else if (Height >= 240)
      {
        result = "240";
      }
      else
      {
        result = "SD";
      }

      if (result != "SD")
      {
        result += Interlaced ? "I" : "P";
      }

      return result;
    }

    private string GetFullCodecName(MediaInfo mediaInfo)
    {
      var strCodec = mediaInfo.Get(StreamKind.Video, StreamPosition, "Format").ToUpper();
      var strCodecVer = mediaInfo.Get(StreamKind.Video, StreamPosition, "Format_Version").ToUpper();
      if (strCodec == "MPEG-4 VISUAL")
      {
        strCodec = mediaInfo.Get(StreamKind.Video, StreamPosition, "CodecID").ToUpperInvariant();
      }
      else
      {
        if (!string.IsNullOrEmpty(strCodecVer))
        {
          strCodec = (strCodec + " " + strCodecVer).Trim();
          string strCodecProf = mediaInfo.Get(StreamKind.Video, StreamPosition, "Format_Profile").ToUpper();
          if (strCodecProf != "MAIN@MAIN")
          {
            strCodec = (strCodec + " " + strCodecProf).Trim();
          }
        }
      }

      return strCodec.Replace("+", "PLUS");
    }
  }
}
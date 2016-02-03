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

namespace MediaPortal.Player.MediaInfo
{
    public enum AspectRatio
    {
        Opaque, // 1:1
        HighEndDataGraphics, // 5:4
        Tv, // 4:3
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
        V_MPEG4_IS0_SP,
        V_MPEG4_IS0_ASP,
        V_MPEG4_IS0_AP,
        V_MPEG4_IS0_AVC,
        V_MPEG4_ISO_SP,
        V_MPEG4_ISO_ASP,
        V_MPEG4_ISO_AP,
        V_MPEG4_ISO_AVC,
        V_MPEGH_ISO_HEVC,
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
        V_VP8,
        V_VP9
    }

    public class VideoStream : LanguageMediaStream
    {
        private static readonly Dictionary<string, AspectRatio> _ratios = new Dictionary<string, AspectRatio>
        {
            { "1:1", AspectRatio.Opaque },
            { "5:4", AspectRatio.HighEndDataGraphics },
            { "1.2", AspectRatio.HighEndDataGraphics },
            { "3:3", AspectRatio.StandartSlides },
            { "4:3", AspectRatio.Tv },
            { "1.334", AspectRatio.Tv },
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

        private static readonly Dictionary<string, StereoMode> _stereoModes = new Dictionary<string, StereoMode>
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

        private static readonly Dictionary<string, VideoCodec> _videoCodecs = new Dictionary<string, VideoCodec>
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
            { "MPEG-2V", VideoCodec.V_MPEG2 },
            { "MPEG-2", VideoCodec.V_MPEG2 },
            { "MPEG-1", VideoCodec.V_MPEG1 },
            { "MPEG-1V", VideoCodec.V_MPEG1 },
            { "VC1", VideoCodec.V_VC1 },
            { "VC-1", VideoCodec.V_VC1 },
            { "OVC1", VideoCodec.V_VC1 },
        };

        public VideoStream(MediaInfo info, int number)
            : base(info, number)
        {
        }

        public VideoStream(int number)
            : base(null, number)
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

        public string Resolution
        {
            get { return GetVideoResolution(); }
        }

        protected override void AnalyzeStreamInternal(MediaInfo info)
        {
            base.AnalyzeStreamInternal(info);
            FrameRate = GetDouble(info, "FrameRate");
            Width = GetInt(info, "Width");
            Height = GetInt(info, "Height");
            AspectRatio = GetAspectRatio(GetString(info, "DisplayAspectRatio"));
            Interlaced = GetInterlaced(GetString(info, "ScanType").ToLower());
            Stereoscopic = GetInt(info, "MultiView_Count") >= 2 ? GetStereoscopic(GetString(info, "MultiView_Layout").ToLower()) : StereoMode.Mono;
            Format = GetString(info, "Format");
            Codec = GetCodecId(GetString(info, "CodecID"));
            if (Codec == VideoCodec.V_UNDEFINED)
            {
                Codec = GetCodec(GetString(info, "Codec"));
            }

            Duration = TimeSpan.FromMilliseconds(GetDouble(info, "Duration"));
            BitDepth = GetInt(info, "BitDepth");
        }

        private static VideoCodec GetCodecId(string codec)
        {
            VideoCodec result;
            return _videoCodecs.TryGetValue(codec.ToUpper(), out result) ? result : VideoCodec.V_UNDEFINED;
        }

        private static VideoCodec GetCodec(string codec)
        {
            VideoCodec result;
            return _videoCodecs.TryGetValue(codec.ToUpper(), out result) ? result : VideoCodec.V_UNDEFINED;
        }

        private static StereoMode GetStereoscopic(string layout)
        {
            StereoMode result;
            return _stereoModes.TryGetValue(layout, out result) ? result : StereoMode.Mono;
        }

        private static bool GetInterlaced(string source)
        {
            return source.Contains("interlaced");
        }

        private static AspectRatio GetAspectRatio(string source)
        {
            AspectRatio result;
            return _ratios.TryGetValue(source, out result) ? result : AspectRatio.Opaque;
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
    }
}
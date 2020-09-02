using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPortal.Util
{
  public static class CodecExtenstions
  {
    #region mappings

    private static readonly Dictionary<MediaInfo.Model.AudioCodec, string> _audioCodecs = new Dictionary<MediaInfo.Model.AudioCodec, string>()
    {
      { MediaInfo.Model.AudioCodec.Undefined, string.Empty },
      { MediaInfo.Model.AudioCodec.MpegLayer1, "MP1" },
      { MediaInfo.Model.AudioCodec.MpegLayer2, "MP2" },
      { MediaInfo.Model.AudioCodec.MpegLayer3, "MP3" },
      { MediaInfo.Model.AudioCodec.PcmIntBig, "PCM" },
      { MediaInfo.Model.AudioCodec.PcmIntLit, "PCM" },
      { MediaInfo.Model.AudioCodec.PcmFloatIeee, "PCM" },
      { MediaInfo.Model.AudioCodec.Ac3, "AC-3" },
      { MediaInfo.Model.AudioCodec.Ac3Atmos, "AC-3 ATMOS" },
      { MediaInfo.Model.AudioCodec.Ac3Bsid9, "AC-3 NET" },
      { MediaInfo.Model.AudioCodec.Ac3Bsid10, "AC-3 NET" },
      { MediaInfo.Model.AudioCodec.Eac3, "EAC3" },
      { MediaInfo.Model.AudioCodec.Eac3Atmos, "EAC3 ATMOS" },
      { MediaInfo.Model.AudioCodec.Truehd, "TRUEHD" },
      { MediaInfo.Model.AudioCodec.TruehdAtmos, "TRUEHD ATMOS" },
      { MediaInfo.Model.AudioCodec.Dts, "DTS" },
      { MediaInfo.Model.AudioCodec.DtsX, "DTSX" },
      { MediaInfo.Model.AudioCodec.DtsHdMa, "DTSHD_MA" },
      { MediaInfo.Model.AudioCodec.DtsExpress, "DTSEX" },
      { MediaInfo.Model.AudioCodec.DtsHdHra, "DTSHD_HRA" },
      { MediaInfo.Model.AudioCodec.DtsEs, "DTS ES" },
      { MediaInfo.Model.AudioCodec.DtsHd, "DTSHD" },
      { MediaInfo.Model.AudioCodec.Flac, "FLAC" },
      { MediaInfo.Model.AudioCodec.Opus, "OPUS" },
      { MediaInfo.Model.AudioCodec.Tta1, "TTA1" },
      { MediaInfo.Model.AudioCodec.Vorbis, "VORBIS" },
      { MediaInfo.Model.AudioCodec.WavPack4, "WAVPACK4" },
      { MediaInfo.Model.AudioCodec.WavPack, "WAVPACK" },
      { MediaInfo.Model.AudioCodec.Wave, "WAVE" },
      { MediaInfo.Model.AudioCodec.Wave64, "WAVE" },
      { MediaInfo.Model.AudioCodec.Real14_4, "RA" },
      { MediaInfo.Model.AudioCodec.Real28_8, "RA" },
      { MediaInfo.Model.AudioCodec.Real10, "RA" },
      { MediaInfo.Model.AudioCodec.RealCook, "RA" },
      { MediaInfo.Model.AudioCodec.RealSipr, "RA" },
      { MediaInfo.Model.AudioCodec.RealRalf, "RA" },
      { MediaInfo.Model.AudioCodec.RealAtrc, "RA" },
      { MediaInfo.Model.AudioCodec.Mlp, "MLP" },
      { MediaInfo.Model.AudioCodec.Aac, "AAC" },
      { MediaInfo.Model.AudioCodec.AacMpeg2Main, "AAC" },
      { MediaInfo.Model.AudioCodec.AacMpeg2Lc, "AAC LC" },
      { MediaInfo.Model.AudioCodec.AacMpeg2LcSbr, "AAC HE-AAC" },
      { MediaInfo.Model.AudioCodec.AacMpeg2Ssr, "AAC HE-AAC" },
      { MediaInfo.Model.AudioCodec.AacMpeg4Main, "AAC" },
      { MediaInfo.Model.AudioCodec.AacMpeg4Lc, "AAC LC" },
      { MediaInfo.Model.AudioCodec.AacMpeg4LcSbr, "AAC HE-AAC" },
      { MediaInfo.Model.AudioCodec.AacMpeg4LcSbrPs, "AAC HE-AAC" },
      { MediaInfo.Model.AudioCodec.AacMpeg4Ssr, "AAC HE-AAC" },
      { MediaInfo.Model.AudioCodec.AacMpeg4Ltp, "AAC HE-AAC" },
      { MediaInfo.Model.AudioCodec.Alac, "ALAC" },
      { MediaInfo.Model.AudioCodec.Ape, "APE" },
      { MediaInfo.Model.AudioCodec.Wma1, "WMA" },
      { MediaInfo.Model.AudioCodec.Wma2, "WMA" },
      { MediaInfo.Model.AudioCodec.Wma3, "WMA" },
      { MediaInfo.Model.AudioCodec.WmaVoice, "WMA VOICE" },
      { MediaInfo.Model.AudioCodec.WmaPro, "WMAPRO" },
      { MediaInfo.Model.AudioCodec.WmaLossless, "WMA LOSSLESS" },
      { MediaInfo.Model.AudioCodec.Adpcm, "ADPCM" },
      { MediaInfo.Model.AudioCodec.Amr, "AMR" },
      { MediaInfo.Model.AudioCodec.Atrac1, "ATRAC" },
      { MediaInfo.Model.AudioCodec.Atrac3, "ATRAC3" },
      { MediaInfo.Model.AudioCodec.Atrac3Plus, "ATRAC3 PLUS" },
      { MediaInfo.Model.AudioCodec.AtracLossless, "ATRAC LOSSLESS" },
      { MediaInfo.Model.AudioCodec.Atrac9, "ATRAC9" },
      { MediaInfo.Model.AudioCodec.Dsd, "DSD" },
      { MediaInfo.Model.AudioCodec.Mac3, "MAC3" },
      { MediaInfo.Model.AudioCodec.Mac6, "MAC6" },
      { MediaInfo.Model.AudioCodec.G_723_1, "G.723.1" },
      { MediaInfo.Model.AudioCodec.Truespeech, "TRUESPEECH" },
      { MediaInfo.Model.AudioCodec.RkAudio, "RKAUDIO" },
      { MediaInfo.Model.AudioCodec.Als, "ALS" },
      { MediaInfo.Model.AudioCodec.Iac2, "IAC2" },
    };

    private static readonly Dictionary<MediaInfo.Model.VideoCodec, string> _videoCodecs = new Dictionary<MediaInfo.Model.VideoCodec, string>()
    {
      { MediaInfo.Model.VideoCodec.Undefined, string.Empty },
      { MediaInfo.Model.VideoCodec.Uncompressed, "RAW" },
      { MediaInfo.Model.VideoCodec.Dirac, "DIRAC" },
      { MediaInfo.Model.VideoCodec.Mpeg4, "MPEG4" },
      { MediaInfo.Model.VideoCodec.Mpeg4Is0Sp, "MPEG4" },
      { MediaInfo.Model.VideoCodec.Mpeg4Is0Asp, "MPEG4VIDEO" },
      { MediaInfo.Model.VideoCodec.Mpeg4Is0Ap, "MPEG4VIDEO" },
      { MediaInfo.Model.VideoCodec.Mpeg4Is0Avc, "AVC" },
      { MediaInfo.Model.VideoCodec.Mpeg4IsoSp, "MPEG4" },
      { MediaInfo.Model.VideoCodec.Mpeg4IsoAsp, "MPEG4VIDEO" },
      { MediaInfo.Model.VideoCodec.Mpeg4IsoAp, "MPEG4VIDEO" },
      { MediaInfo.Model.VideoCodec.Mpeg4IsoAvc, "AVC" },
      { MediaInfo.Model.VideoCodec.MpeghIsoHevc, "HEVC" },
      { MediaInfo.Model.VideoCodec.Mpeg4MsV1, "MSMPEG4V1" },
      { MediaInfo.Model.VideoCodec.Mpeg4MsV2, "MSMPEG4V2" },
      { MediaInfo.Model.VideoCodec.Mpeg4MsV3, "MSMPEG4V3" },
      { MediaInfo.Model.VideoCodec.Vc1, "VC-1" },
      { MediaInfo.Model.VideoCodec.Mpeg1, "MPEG1VIDEO" },
      { MediaInfo.Model.VideoCodec.Mpeg2, "MPEG2VIDEO" },
      { MediaInfo.Model.VideoCodec.ProRes, "PRORES" },
      { MediaInfo.Model.VideoCodec.RealRv10, "REAL" },
      { MediaInfo.Model.VideoCodec.RealRv20, "REAL" },
      { MediaInfo.Model.VideoCodec.RealRv30, "RV30" },
      { MediaInfo.Model.VideoCodec.RealRv40, "RV40" },
      { MediaInfo.Model.VideoCodec.Theora, "THEORA" },
      { MediaInfo.Model.VideoCodec.Vp6, "VP6F" },
      { MediaInfo.Model.VideoCodec.Vp8, "VP8" },
      { MediaInfo.Model.VideoCodec.Vp9, "VP9" },
      { MediaInfo.Model.VideoCodec.Divx1, "DIVX" },
      { MediaInfo.Model.VideoCodec.Divx2, "DIV2" },
      { MediaInfo.Model.VideoCodec.Divx3, "DIV3" },
      { MediaInfo.Model.VideoCodec.Divx4, "DIVX 4" },
      { MediaInfo.Model.VideoCodec.Divx50, "DX50" },
      { MediaInfo.Model.VideoCodec.Xvid, "XVID" },
      { MediaInfo.Model.VideoCodec.Svq1, "SVQ1" },
      { MediaInfo.Model.VideoCodec.Svq2, "SVQ2" },
      { MediaInfo.Model.VideoCodec.Svq3, "SVQ3" },
      { MediaInfo.Model.VideoCodec.Sprk, "FLASH" },
      { MediaInfo.Model.VideoCodec.H260, "H260" },
      { MediaInfo.Model.VideoCodec.H261, "H261" },
      { MediaInfo.Model.VideoCodec.H263, "H263" },
      { MediaInfo.Model.VideoCodec.Avdv, "AVDV" },
      { MediaInfo.Model.VideoCodec.Avd1, "AVD1" },
      { MediaInfo.Model.VideoCodec.Ffv1, "FFV1" },
      { MediaInfo.Model.VideoCodec.Ffv2, "FFV2" },
      { MediaInfo.Model.VideoCodec.Iv21, "IV21" },
      { MediaInfo.Model.VideoCodec.Iv30, "IV30" },
      { MediaInfo.Model.VideoCodec.Iv40, "IV40" },
      { MediaInfo.Model.VideoCodec.Iv50, "IV50" },
      { MediaInfo.Model.VideoCodec.Ffds, "MPEG4" },
      { MediaInfo.Model.VideoCodec.Fraps, "MPEG4" },
      { MediaInfo.Model.VideoCodec.Ffvh, "MPEG4" },
      { MediaInfo.Model.VideoCodec.Mjpg, "MJPG" },
      { MediaInfo.Model.VideoCodec.Dv, "DV" },
      { MediaInfo.Model.VideoCodec.Hdv, "HDV" },
      { MediaInfo.Model.VideoCodec.DvcPro50, "DVCPRO50" },
      { MediaInfo.Model.VideoCodec.DvcProHd, "DVCPROHD" },
      { MediaInfo.Model.VideoCodec.Wmv1, "WMV" },
      { MediaInfo.Model.VideoCodec.Wmv2, "WMV2" },
      { MediaInfo.Model.VideoCodec.Wmv3, "WMV3" },
      { MediaInfo.Model.VideoCodec.Q8Bps, "Q8BPS" },
      { MediaInfo.Model.VideoCodec.BinkVideo, "BINKVIDEO" },
      { MediaInfo.Model.VideoCodec.Av1, "AV1" },
      { MediaInfo.Model.VideoCodec.HuffYUV, "HUFFYUV" },
    };

    #endregion

    public static string ToCodecString(this MediaInfo.Model.AudioCodec codec)
    {
      string result;
      return _audioCodecs.TryGetValue(codec, out result) ? result : string.Empty;
    }

    public static string ToCodecString(this MediaInfo.Model.VideoCodec codec)
    {
      string result;
      return _videoCodecs.TryGetValue(codec, out result) ? result : string.Empty;
    }
  }
}

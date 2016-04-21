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

namespace MediaPortal.Player
{
  public enum AudioCodec
  {
    A_UNDEFINED,
    A_MPEG_L1,
    A_MPEG_L2,
    A_MPEG_L3,
    A_PCM_INT_BIG,
    A_PCM_INT_LIT,
    A_PCM_FLOAT_IEEE,
    A_AC3,
    A_AC3_BSID9,
    A_AC3_BSID10,
    A_DTS,
    A_DTS_HD,
    A_EAC3,
    A_FLAC,
    A_OPUS,
    A_TTA1,
    A_VORBIS,
    A_WAVPACK4,
    A_WAVPACK,
    A_WAVE,
    A_WAVE64,
    A_REAL_14_4,
    A_REAL_28_8,
    A_REAL_COOK,
    A_REAL_SIPR,
    A_REAL_RALF,
    A_REAL_ATRC,
    A_TRUEHD,
    A_MLP,
    A_AAC,
    A_AAC_MPEG2_MAIN,
    A_AAC_MPEG2_LC,
    A_AAC_MPEG2_LC_SBR,
    A_AAC_MPEG2_SSR,
    A_AAC_MPEG4_MAIN,
    A_AAC_MPEG4_LC,
    A_AAC_MPEG4_LC_SBR,
    A_AAC_MPEG4_LC_SBR_PS,
    A_AAC_MPEG4_SSR,
    A_AAC_MPEG4_LTP,
    A_ALAC,
    A_APE,
    A_WMA1,
    A_WMA2,
    A_WMA9,
    A_ADPCM,
    A_AMR,
    A_ATRAC1,
    A_ATRAC3,
  }

  public class AudioStream : LanguageMediaStream
  {
    #region matching dictionaries

    private static readonly Dictionary<AudioCodec, string> CodecFrendlyNames = new Dictionary<AudioCodec, string>
    {
      { AudioCodec.A_UNDEFINED, "" },
      { AudioCodec.A_MPEG_L1, "MPEG Layer 1" },
      { AudioCodec.A_MPEG_L2, "MPEG Layer 2" },
      { AudioCodec.A_MPEG_L3, "MPEG Layer 3" },
      { AudioCodec.A_PCM_INT_BIG, "PCM" },
      { AudioCodec.A_PCM_INT_LIT, "PCM" },
      { AudioCodec.A_PCM_FLOAT_IEEE, "PCM" },
      { AudioCodec.A_AC3, "Dolby Digital" },
      { AudioCodec.A_AC3_BSID9, "Dolby Digital" },
      { AudioCodec.A_AC3_BSID10, "Dolby Digital" },
      { AudioCodec.A_DTS, "DTS" },
      { AudioCodec.A_DTS_HD, "DTS HD" },
      { AudioCodec.A_EAC3, "Dolby Digital Plus" },
      { AudioCodec.A_FLAC, "FLAC" },
      { AudioCodec.A_OPUS, "OPUS" },
      { AudioCodec.A_TTA1, "True Audio" },
      { AudioCodec.A_VORBIS, "Vorbis" },
      { AudioCodec.A_WAVPACK4, "WavPack" },
      { AudioCodec.A_WAVPACK, "WavPack" },
      { AudioCodec.A_WAVE, "Wave" },
      { AudioCodec.A_WAVE64, "Wave" },
      { AudioCodec.A_REAL_14_4, "Real Audio" },
      { AudioCodec.A_REAL_28_8, "Real Audio" },
      { AudioCodec.A_REAL_COOK, "Real Audio" },
      { AudioCodec.A_REAL_SIPR, "Real Audio" },
      { AudioCodec.A_REAL_RALF, "Real Audio" },
      { AudioCodec.A_REAL_ATRC, "Real Audio" },
      { AudioCodec.A_TRUEHD, "Dolby TrueHD" },
      { AudioCodec.A_MLP, "Meridian Lossless" },
      { AudioCodec.A_AAC, "AAC" },
      { AudioCodec.A_AAC_MPEG2_MAIN, "AAC" },
      { AudioCodec.A_AAC_MPEG2_LC, "AAC" },
      { AudioCodec.A_AAC_MPEG2_LC_SBR, "AAC" },
      { AudioCodec.A_AAC_MPEG2_SSR, "AAC" },
      { AudioCodec.A_AAC_MPEG4_MAIN, "AAC" },
      { AudioCodec.A_AAC_MPEG4_LC, "AAC" },
      { AudioCodec.A_AAC_MPEG4_LC_SBR, "AAC" },
      { AudioCodec.A_AAC_MPEG4_LC_SBR_PS, "AAC" },
      { AudioCodec.A_AAC_MPEG4_SSR, "AAC" },
      { AudioCodec.A_AAC_MPEG4_LTP, "AAC" },
      { AudioCodec.A_ALAC, "Apple Lossless" },
      { AudioCodec.A_APE, "Monkey's Audio" },
      { AudioCodec.A_WMA1, "Windows Audio" },
      { AudioCodec.A_WMA2, "Windows Audio" },
      { AudioCodec.A_WMA9, "Windows Audio Pro" },
      { AudioCodec.A_ADPCM, "ADPCM" },
      { AudioCodec.A_AMR, "Adaptive Multi-Rate" },
    };

    private static readonly Dictionary<string, AudioCodec> CodecIds = new Dictionary<string, AudioCodec>
    {
      { "A_MPEG/L1", AudioCodec.A_MPEG_L1 },
      { "A_MPEG/L2", AudioCodec.A_MPEG_L2 },
      { "A_MPEG/L3", AudioCodec.A_MPEG_L3 },
      { "A_PCM/INT/BIG", AudioCodec.A_PCM_INT_BIG },
      { "A_PCM/INT/LIT", AudioCodec.A_PCM_INT_LIT },
      { "A_PCM/FLOAT/IEEE", AudioCodec.A_PCM_FLOAT_IEEE },
      { "A_AC3", AudioCodec.A_AC3 },
      { "A_AC3/BSID9", AudioCodec.A_AC3_BSID9 },
      { "A_AC3/BSID10", AudioCodec.A_AC3_BSID10 },
      { "A_DTS", AudioCodec.A_DTS },
      { "A_DTS-HD", AudioCodec.A_DTS_HD },
      { "A_EAC3", AudioCodec.A_EAC3 },
      { "A_FLAC", AudioCodec.A_FLAC },
      { "A_OPUS", AudioCodec.A_OPUS },
      { "A_TTA1", AudioCodec.A_TTA1 },
      { "A_VORBIS", AudioCodec.A_VORBIS },
      { "A_WAVPACK4", AudioCodec.A_WAVPACK4 },
      { "A_WAVPACK", AudioCodec.A_WAVPACK },
      { "A_REAL/14_4", AudioCodec.A_REAL_14_4 },
      { "A_REAL/28_8", AudioCodec.A_REAL_28_8 },
      { "A_REAL/COOK", AudioCodec.A_REAL_COOK },
      { "A_REAL/SIPR", AudioCodec.A_REAL_SIPR },
      { "A_REAL/RALF", AudioCodec.A_REAL_RALF },
      { "A_REAL/ATRC", AudioCodec.A_REAL_ATRC },
      { "A_TRUEHD", AudioCodec.A_TRUEHD },
      { "A_MLP", AudioCodec.A_MLP },
      { "A_AAC", AudioCodec.A_AAC },
      { "A_AAC/MPEG2/MAIN", AudioCodec.A_AAC_MPEG2_MAIN },
      { "A_AAC/MPEG2/LC", AudioCodec.A_AAC_MPEG2_LC },
      { "A_AAC/MPEG2/LC/SBR", AudioCodec.A_AAC_MPEG2_LC_SBR },
      { "A_AAC/MPEG2/SSR", AudioCodec.A_AAC_MPEG2_SSR },
      { "A_AAC/MPEG4/MAIN", AudioCodec.A_AAC_MPEG4_MAIN },
      { "A_AAC/MPEG4/LC", AudioCodec.A_AAC_MPEG4_LC },
      { "A_AAC/MPEG4/LC/SBR", AudioCodec.A_AAC_MPEG4_LC_SBR },
      { "A_AAC/MPEG4/LC/SBR/PS", AudioCodec.A_AAC_MPEG4_LC_SBR_PS },
      { "A_AAC/MPEG4/SSR", AudioCodec.A_AAC_MPEG4_SSR },
      { "A_AAC/MPEG4/LTP", AudioCodec.A_AAC_MPEG4_LTP },
      { "A_ALAC", AudioCodec.A_ALAC },
      { "A_APE", AudioCodec.A_APE },
      { "SAMR", AudioCodec.A_AMR },
    };

    private static readonly Dictionary<string, AudioCodec> Codecs = new Dictionary<string, AudioCodec>
    {
      { "MPA1L1", AudioCodec.A_MPEG_L1 },
      { "MPA1L2", AudioCodec.A_MPEG_L2 },
      { "MPA1L3", AudioCodec.A_MPEG_L3 },
      { "PCM BIG", AudioCodec.A_PCM_INT_BIG },
      { "PCM LITTLE", AudioCodec.A_PCM_INT_LIT },
      { "PCM", AudioCodec.A_PCM_INT_LIT },
      { "PCM/FLOAT/IEEE", AudioCodec.A_PCM_FLOAT_IEEE },
      { "AC3", AudioCodec.A_AC3 },
      { "AC-3", AudioCodec.A_AC3 },
      { "AC3/BSID9", AudioCodec.A_AC3_BSID9 },
      { "AC3/BSID10", AudioCodec.A_AC3_BSID10 },
      { "DTS", AudioCodec.A_DTS },
      { "DTS-HD", AudioCodec.A_DTS_HD },
      { "EAC3", AudioCodec.A_EAC3 },
      { "EAC-3", AudioCodec.A_EAC3 },
      { "E-AC-3", AudioCodec.A_EAC3 },
      { "AC3+", AudioCodec.A_EAC3 },
      { "FLAC", AudioCodec.A_FLAC },
      { "OPUS", AudioCodec.A_OPUS },
      { "TTA1", AudioCodec.A_TTA1 },
      { "VORBIS", AudioCodec.A_VORBIS },
      { "WAVPACK4", AudioCodec.A_WAVPACK4 },
      { "WAVPACK", AudioCodec.A_WAVPACK },
      { "WAVE", AudioCodec.A_WAVE },
      { "WAVE64", AudioCodec.A_WAVE64 },
      { "REAL/14_4", AudioCodec.A_REAL_14_4 },
      { "REAL/28_8", AudioCodec.A_REAL_28_8 },
      { "REAL/COOK", AudioCodec.A_REAL_COOK },
      { "REAL/SIPR", AudioCodec.A_REAL_SIPR },
      { "REAL/RALF", AudioCodec.A_REAL_RALF },
      { "REAL/ATRC", AudioCodec.A_REAL_ATRC },
      { "TRUEHD", AudioCodec.A_TRUEHD },
      { "MLP", AudioCodec.A_MLP },
      { "AAC", AudioCodec.A_AAC },
      { "AAC LC", AudioCodec.A_AAC_MPEG4_LC },
      { "AAC LTP", AudioCodec.A_AAC_MPEG4_LTP },
      { "AAC MAIN", AudioCodec.A_AAC_MPEG4_MAIN },
      { "AAC SSR", AudioCodec.A_AAC_MPEG4_SSR },
      { "AAC/MPEG2/MAIN", AudioCodec.A_AAC_MPEG2_MAIN },
      { "AAC/MPEG2/LC", AudioCodec.A_AAC_MPEG2_LC },
      { "AAC/MPEG2/LC/SBR", AudioCodec.A_AAC_MPEG2_LC_SBR },
      { "AAC/MPEG2/SSR", AudioCodec.A_AAC_MPEG2_SSR },
      { "AAC/MPEG4/MAIN", AudioCodec.A_AAC_MPEG4_MAIN },
      { "AAC/MPEG4/LC", AudioCodec.A_AAC_MPEG4_LC },
      { "AAC/MPEG4/LC/SBR", AudioCodec.A_AAC_MPEG4_LC_SBR },
      { "AAC/MPEG4/LC/SBR/PS", AudioCodec.A_AAC_MPEG4_LC_SBR_PS },
      { "AAC/MPEG4/SSR", AudioCodec.A_AAC_MPEG4_SSR },
      { "AAC/MPEG4/LTP", AudioCodec.A_AAC_MPEG4_LTP },
      { "ALAC", AudioCodec.A_ALAC },
      { "APE", AudioCodec.A_APE },
      { "11", AudioCodec.A_ADPCM },
      { "AMR", AudioCodec.A_AMR },
      { "160", AudioCodec.A_WMA1 },
      { "161", AudioCodec.A_WMA2 },
      { "162", AudioCodec.A_WMA9 },
    };

    private static readonly Dictionary<int, string> Channels = new Dictionary<int, string>
    {
      { 1, "Mono" },
      { 2, "Stereo" },
      { 3, "2.1" },
      { 4, "4.0" },
      { 5, "5.0" },
      { 6, "5.1" },
      { 7, "6.1" },
      { 8, "7.1" },
      { 9, "7.2" },
      { 10, "7.2.1" },
    };

    #endregion

    public AudioStream(MediaInfo info, int number, int position)
        : base(info, number, position)
    {
    }

    public AudioStream(int number, int position)
        : base(null, number, position)
    {
    }

    public override MediaStreamKind Kind
    {
      get { return MediaStreamKind.Audio; }
    }

    protected override StreamKind StreamKind
    {
      get { return StreamKind.Audio; }
    }

    public AudioCodec Codec { get; set; }

    public string CodecFrendly
    {
      get
      {
        string result;
        return CodecFrendlyNames.TryGetValue(Codec, out result) ? result : string.Empty;
      }
    }

    public TimeSpan Duration { get; set; }

    public double Bitrate { get; set; }

    public int Channel { get; set; }

    public double SamplingRate { get; set; }

    public int BitDepth { get; set; }

    public string Format { get; set; }

    public string CodecName { get; set; }

    public string AudioChannelsFriendly
    {
      get { return ConvertAudioChannels(Channel); }
    }

    protected override void AnalyzeStreamInternal(MediaInfo info)
    {
      base.AnalyzeStreamInternal(info);
      Codec = GetCodecByCodecId(GetString(info, "CodecID").ToUpper());
      if (Codec == AudioCodec.A_UNDEFINED)
      {
        var codecValue = GetString(info, "Codec");
        if (codecValue.Equals("PCM", StringComparison.OrdinalIgnoreCase))
        {
          var endianness = GetString(info, "Codec_Settings_Endianness");
          codecValue = string.Format("{0}{1}", codecValue, string.IsNullOrEmpty(endianness) ? string.Empty : " " + endianness);
        }

        Codec = GetCodecByCodec(codecValue.ToUpper());
      }

      Duration = TimeSpan.FromMilliseconds(GetDouble(info, "Duration"));
      Bitrate = GetDouble(info, "BitRate");
      Channel = GetInt(info, "Channel(s)");
      SamplingRate = GetDouble(info, "SamplingRate");
      BitDepth = GetInt(info, "BitDepth");
      Format = GetString(info, "Format");
      CodecName = GetFullCodecName(info);
    }

    private static AudioCodec GetCodecByCodecId(string source)
    {
      AudioCodec result;
      return CodecIds.TryGetValue(source, out result) ? result : AudioCodec.A_UNDEFINED;
    }

    private static AudioCodec GetCodecByCodec(string source)
    {
      AudioCodec result;
      return Codecs.TryGetValue(source, out result) ? result : AudioCodec.A_UNDEFINED;
    }

    private static string ConvertAudioChannels(int channels)
    {
      string result;
      return Channels.TryGetValue(channels, out result) ? result : "Unknown";
    }

    private string GetFullCodecName(MediaInfo mediaInfo)
    {
      var strCodec = mediaInfo.Get(StreamKind.Audio, StreamPosition, "Format").ToUpper();
      var strCodecVer = mediaInfo.Get(StreamKind.Audio, StreamPosition, "Format_Version").ToUpper();
      if (strCodec == "MPEG-4 VISUAL")
      {
        strCodec = mediaInfo.Get(StreamKind.Audio, StreamPosition, "CodecID").ToUpperInvariant();
      }
      else
      {
        if (!string.IsNullOrEmpty(strCodecVer))
        {
          strCodec = (strCodec + " " + strCodecVer).Trim();
        }
      }

      strCodec = (strCodec + " " + mediaInfo.Get(StreamKind.Audio, StreamPosition, "Format_Profile").Split(new char[] { '/' })[0].ToUpper()).Trim();

      return strCodec.Replace("+", "PLUS");
    }

  }
}
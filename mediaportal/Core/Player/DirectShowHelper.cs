﻿#region Copyright (C) 2005-2016 Team MediaPortal

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
using System.Linq;
using System.Text.RegularExpressions;

using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.GUI.Library;
using MediaPortal.Localisation;
using MediaPortal.Services;

namespace MediaPortal.Player
{
  public delegate void StoreStreamAction(string filterName, string name, int lcid, int id, AMMediaType sType, DirectShowHelper.StreamType type, AMStreamSelectInfoFlags flag, IAMStreamSelect pStrm);

  public class DirectShowHelper
  {
    public enum StreamType
    {
      Unknown,
      Video,
      Audio,
      Subtitle,
      Subtitle_hidden,
      Subtitle_shown,
      Edition,
      Subtitle_file,
      PostProcessing,
    }

    private readonly StoreStreamAction _storeAction;
    private readonly ILog _logger;

    #region Match dictionaries

    private static readonly Dictionary<Guid, HashSet<AudioCodec>> MediaCodecs = new Dictionary<Guid, HashSet<AudioCodec>>
    {
        { MediaSubType.DolbyAC3, new HashSet<AudioCodec> { AudioCodec.A_AC3_BSID10, AudioCodec.A_AC3, AudioCodec.A_AC3_BSID9 } },
        { MediaSubType.DDPLUS, new HashSet<AudioCodec> { AudioCodec.A_EAC3, AudioCodec.A_AC3 } },
        { MediaSubType.DOLBY_AC3_SPDIF, new HashSet<AudioCodec> { AudioCodec.A_AC3_BSID10, AudioCodec.A_AC3, AudioCodec.A_AC3_BSID9 } },
        { MediaSubType.MPEG1Audio, new HashSet<AudioCodec> { AudioCodec.A_MPEG_L1 } },
        { MediaSubType.MPEG1AudioPayload, new HashSet<AudioCodec> { AudioCodec.A_MPEG_L1 } },
        { MediaSubType.Mpeg2Audio, new HashSet<AudioCodec> { AudioCodec.A_MPEG_L2 } },
        { MediaSubType.LATMAAC, new HashSet<AudioCodec>
        {
            AudioCodec.A_AAC, AudioCodec.A_AAC_MPEG2_LC, AudioCodec.A_AAC_MPEG2_LC_SBR, AudioCodec.A_AAC_MPEG2_MAIN, AudioCodec.A_AAC_MPEG2_SSR,
            AudioCodec.A_AAC_MPEG4_LC, AudioCodec.A_AAC_MPEG4_LC_SBR, AudioCodec.A_AAC_MPEG4_LC_SBR_PS, AudioCodec.A_AAC_MPEG4_LTP, 
            AudioCodec.A_AAC_MPEG4_MAIN, AudioCodec.A_AAC_MPEG4_SSR
        } },
        { MediaSubType.AAC, new HashSet<AudioCodec>
        {
            AudioCodec.A_AAC, AudioCodec.A_AAC_MPEG2_LC, AudioCodec.A_AAC_MPEG2_LC_SBR, AudioCodec.A_AAC_MPEG2_MAIN, AudioCodec.A_AAC_MPEG2_SSR,
            AudioCodec.A_AAC_MPEG4_LC, AudioCodec.A_AAC_MPEG4_LC_SBR, AudioCodec.A_AAC_MPEG4_LC_SBR_PS, AudioCodec.A_AAC_MPEG4_LTP, 
            AudioCodec.A_AAC_MPEG4_MAIN, AudioCodec.A_AAC_MPEG4_SSR
        } },
        { MediaSubType.DVD_LPCM_AUDIO, new HashSet<AudioCodec> { AudioCodec.A_PCM_FLOAT_IEEE, AudioCodec.A_PCM_INT_BIG, AudioCodec.A_PCM_INT_LIT } },
        { MediaSubType.IEEE_FLOAT, new HashSet<AudioCodec> { AudioCodec.A_PCM_FLOAT_IEEE } },
        { MediaSubType.PCM, new HashSet<AudioCodec> { AudioCodec.A_PCM_FLOAT_IEEE, AudioCodec.A_PCM_INT_BIG, AudioCodec.A_PCM_INT_LIT } },
        { MediaSubType.PCMAudio_Obsolete, new HashSet<AudioCodec> { AudioCodec.A_PCM_FLOAT_IEEE, AudioCodec.A_PCM_INT_BIG, AudioCodec.A_PCM_INT_LIT } },
        { MediaSubType.WAVE, new HashSet<AudioCodec> { AudioCodec.A_WAVE, AudioCodec.A_WAVE64, AudioCodec.A_WAVPACK, AudioCodec.A_WAVPACK4 } },
    };

    private static readonly Dictionary<string, string> Channels = new Dictionary<string, string>
    {
      { "1 channel", "Mono" },
      { "2 channels", "Stereo" },
      { "3 channels", "2.1" },
      { "4 channels", "4.0" },
      { "6 channels", "5.1" },
      { "7 channels", "6.1" },
      { "8 channels", "7.1" },
      { "9 channels", "7.2" },
      { "10 channels", "7.2.1" },
    };

    private static readonly Dictionary<string, int> ChannelNumbers = new Dictionary<string, int>
    {
      { "1 channel", 1 },
      { "1 chn", 1 },
      { "Mono", 1 },
      { "2 channels", 2 },
      { "2 chn", 2 },
      { "Stereo", 2 },
      { "3 channels", 3 },
      { "3 chn", 3 },
      { "2.1", 3 },
      { "4 channels", 4 },
      { "4 chn", 4 },
      { "4.0", 4 },
      { "6 channels", 6 },
      { "6 chn", 6 },
      { "5.1", 6 },
      { "7 channels", 7 },
      { "7 chn", 7 },
      { "6.1", 7 },
      { "8 channels", 8 },
      { "8 chn", 8 },
      { "7.1", 8 },
      { "9 channels", 9 },
      { "9 chn", 9 },
      { "7.2", 9 },
      { "10 channels", 10 },
      { "10 chn", 10 },
      { "7.2.1", 10 },
    };

    private static readonly Dictionary<string, string> NameEncoders = new Dictionary<string, string>
    {
      { "Mainconcept MP4 Sound Media Handler", string.Empty },
      { "Mainconcept MP4 Video Media Handler", string.Empty },
      { "SoundHandler", string.Empty },
      { "VideoHandler", string.Empty },
      { "L-SMASH Video Handler", string.Empty },
      { "L-SMASH Sound Handler", string.Empty },
      { "DataHandler", string.Empty },
      { "MediaHandler", string.Empty },
    };

    #region Video codecs

    private static readonly Dictionary<string, HashSet<VideoCodec>> VideoCodecs = new Dictionary<string, HashSet<VideoCodec>>
    {
      // H.264 High Profile
      { "H264 HIGH L1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L1.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L1b", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L1.0b", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L1.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L1.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L1.3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L2.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L2.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L2.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L3.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L3.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L3.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L4", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L4.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L4.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L4.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L5", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L5.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L5.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH L5.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },

      // H.264 High 10 Profile
      { "H264 HIGH 10 L1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L1.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L1b", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L1.0b", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L1.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L1.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L1.3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L2.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L2.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L2.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L3.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L3.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L3.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L4", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L4.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L4.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L4.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L5", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L5.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L5.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 HIGH 10 L5.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },

      // H.264 Scalable High Profile
      { "H264 SCALABLE HIGH L1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L1.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L1b", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L1.0b", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L1.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L1.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L1.3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L2.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L2.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L2.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L3.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L3.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L3.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L4", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L4.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L4.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L4.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L5", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L5.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L5.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE HIGH L5.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },

      // H.264 Main Profile
      { "H264 MAIN L1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L1.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L1b", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L1.0b", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L1.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L1.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L1.3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L2.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L2.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L2.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L3.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L3.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L3.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L4", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L4.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L4.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L4.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L5", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L5.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L5.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 MAIN L5.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      
      // H.264 Baseline Profile
      { "H264 BASELINE L1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L1.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L1b", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L1.0b", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L1.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L1.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L1.3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L2.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L2.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L2.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L3.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L3.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L3.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L4", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L4.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L4.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L4.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L5", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L5.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L5.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 BASELINE L5.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },

      // H.264 Constrained Baseline Profile
      { "H264 CONSTRAINED BASELINE L1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L1.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L1b", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L1.0b", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L1.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L1.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L1.3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L2.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L2.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L2.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L3.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L3.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L3.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L4", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L4.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L4.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L4.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L5", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L5.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L5.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 CONSTRAINED BASELINE L5.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },

      // H.264 Scalable Baseline Profile
      { "H264 SCALABLE BASELINE L1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L1.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L1b", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L1.0b", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L1.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L1.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L1.3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L2.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L2.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L2.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L3.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L3.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L3.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L4", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L4.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L4.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L4.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L5", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L5.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L5.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 SCALABLE BASELINE L5.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },

      // H.264 Extended Profile
      { "H264 EXTENDED L1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L1.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L1b", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L1.0b", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L1.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L1.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L1.3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L2.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L2.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L2.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L3.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L3.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L3.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L4", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L4.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L4.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L4.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L5", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L5.0", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L5.1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },
      { "H264 EXTENDED L5.2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_ISO_AVC } },

      // H.264 High 4:2:2 Profile
      // H.264 4:4:4 Predictive Profile

      { "8BPS", new HashSet<VideoCodec> { VideoCodec.V_8BPS } },
      { "DIVX", new HashSet<VideoCodec> { VideoCodec.V_DIVX4, VideoCodec.V_DIVX3, VideoCodec.V_DIVX2, VideoCodec.V_DIVX1 } },
      { "DX50", new HashSet<VideoCodec> { VideoCodec.V_DIVX50 } },
      { "XVID", new HashSet<VideoCodec> { VideoCodec.V_XVID } },
      { "BINKVIDEO", new HashSet<VideoCodec> { VideoCodec.V_BINKVIDEO } },
      { "DVVIDEO", new HashSet<VideoCodec> { VideoCodec.V_DV } },
      { "FLV1", new HashSet<VideoCodec> { VideoCodec.V_SPRK, VideoCodec.V_FFV1 } },
      { "FLV", new HashSet<VideoCodec> { VideoCodec.V_SPRK, VideoCodec.V_FFV2 } },
      { "FFVHUFF", new HashSet<VideoCodec> { VideoCodec.V_FFVH } },
      { "FRAPS", new HashSet<VideoCodec> { VideoCodec.V_FRAPS } },
      { "H261", new HashSet<VideoCodec> { VideoCodec.V_H261 } },
      { "H263", new HashSet<VideoCodec> { VideoCodec.V_H263 } },
      { "H263I", new HashSet<VideoCodec> { VideoCodec.V_H263 } },
      { "H263P", new HashSet<VideoCodec> { VideoCodec.V_H263 } },

      { "HEVC", new HashSet<VideoCodec> { VideoCodec.V_MPEGH_ISO_HEVC } },
      { "HEVC MAIN", new HashSet<VideoCodec> { VideoCodec.V_MPEGH_ISO_HEVC } },
      { "HEVC MAIN 10", new HashSet<VideoCodec> { VideoCodec.V_MPEGH_ISO_HEVC } },

      { "INDEO2", new HashSet<VideoCodec> { VideoCodec.V_IV21 } },
      { "INDEO3", new HashSet<VideoCodec> { VideoCodec.V_IV30 } },
      { "INDEO4", new HashSet<VideoCodec> { VideoCodec.V_IV40 } },
      { "INDEO5", new HashSet<VideoCodec> { VideoCodec.V_IV50 } },
      { "MPEG1VIDEO", new HashSet<VideoCodec> { VideoCodec.V_MPEG1 } },

      { "MPEG2VIDEO", new HashSet<VideoCodec> { VideoCodec.V_MPEG2 } },
      { "MPEG2 MAIN", new HashSet<VideoCodec> { VideoCodec.V_MPEG2 } },
      { "MPEG2 SIMPLE", new HashSet<VideoCodec> { VideoCodec.V_MPEG2 } },
      { "MPEG2 HIGH", new HashSet<VideoCodec> { VideoCodec.V_MPEG2 } },
      
      { "MPEG4", new HashSet<VideoCodec> { VideoCodec.V_MPEG4, VideoCodec.V_MPEG4_ISO_SP } },
      { "MSMPEG4V1", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_MS_V1 } },
      { "MSMPEG4V2", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_MS_V2 } },
      { "MSMPEG4V3", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_MS_V3 } },
      { "MSMPEG4", new HashSet<VideoCodec> { VideoCodec.V_MPEG4_MS_V1, VideoCodec.V_MPEG4_MS_V2, VideoCodec.V_MPEG4_MS_V3 } },

      // VC-1 
      { "VC1", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },
      { "VC-1", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },
      { "VC-1 SIMPLE LOW", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },
      { "VC-1 SIMPLE L", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },
      { "VC-1 SIMPLE MEDIUM", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },
      { "VC-1 SIMPLE M", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },
      { "VC-1 MAIN LOW", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },
      { "VC-1 MAIN L", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },
      { "VC-1 MAIN MEDIUM", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },
      { "VC-1 MAIN M", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },
      { "VC-1 MAIN HIGH", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },
      { "VC-1 MAIN H", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },
      { "VC-1 ADVANCED L0", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },
      { "VC-1 ADVANCED L1", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },
      { "VC-1 ADVANCED L2", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },
      { "VC-1 ADVANCED L3", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },
      { "VC-1 ADVANCED L4", new HashSet<VideoCodec> { VideoCodec.V_VC1 } },

      { "VP6", new HashSet<VideoCodec> { VideoCodec.V_VP6 } },
      { "VP6A", new HashSet<VideoCodec> { VideoCodec.V_VP6 } },
      { "VP6F", new HashSet<VideoCodec> { VideoCodec.V_VP6 } },
      { "VP8", new HashSet<VideoCodec> { VideoCodec.V_VP8 } },
      { "VP9", new HashSet<VideoCodec> { VideoCodec.V_VP9 } },
      { "WMV1", new HashSet<VideoCodec> { VideoCodec.V_WMV1 } },
      { "WMV2", new HashSet<VideoCodec> { VideoCodec.V_WMV2 } },
      { "WMV3", new HashSet<VideoCodec> { VideoCodec.V_WMV3 } },
      { "MPEG4 SIMPLE SCALABLE PROFILE", new HashSet<VideoCodec> { VideoCodec.V_DIVX50, VideoCodec.V_MPEG4, VideoCodec.V_MPEG4_IS0_SP, VideoCodec.V_MPEG4_ISO_SP, VideoCodec.V_XVID } },
      { "MPEG4 SIMPLE PROFILE", new HashSet<VideoCodec> { VideoCodec.V_DIVX50, VideoCodec.V_MPEG4, VideoCodec.V_MPEG4_IS0_SP, VideoCodec.V_MPEG4_ISO_SP, VideoCodec.V_XVID } },
    };

    #endregion

    #region Audio codecs

    private static readonly Dictionary<string, HashSet<AudioCodec>> AudioCodecs = new Dictionary<string, HashSet<AudioCodec>>
    {
      { "MP1", new HashSet<AudioCodec> { AudioCodec.A_MPEG_L1 } },
      { "MP2", new HashSet<AudioCodec> { AudioCodec.A_MPEG_L2 } },
      { "MP3", new HashSet<AudioCodec> { AudioCodec.A_MPEG_L3 } },
      { "MP3ADU", new HashSet<AudioCodec> { AudioCodec.A_MPEG_L3 } },
      { "MP3ON4", new HashSet<AudioCodec> { AudioCodec.A_MPEG_L3 } },
      
      { "AC3", new HashSet<AudioCodec> { AudioCodec.A_AC3, AudioCodec.A_AC3_BSID10, AudioCodec.A_AC3_BSID9 } },
      { "Dolby Digital", new HashSet<AudioCodec> { AudioCodec.A_AC3, AudioCodec.A_AC3_BSID10, AudioCodec.A_AC3_BSID9 } },
      { "EAC3", new HashSet<AudioCodec> { AudioCodec.A_EAC3, AudioCodec.A_AC3, AudioCodec.A_AC3_BSID10, AudioCodec.A_AC3_BSID9 } },
      
      { "ADTS", new HashSet<AudioCodec> { AudioCodec.A_DTS } },
      { "DTS", new HashSet<AudioCodec> { AudioCodec.A_DTS } },
      { "DTSHD", new HashSet<AudioCodec> { AudioCodec.A_DTS_HD, AudioCodec.A_DTS } },
      { "DTS-HD MA", new HashSet<AudioCodec> { AudioCodec.A_DTS_HD, AudioCodec.A_DTS } },
      { "DTS-ES", new HashSet<AudioCodec> { AudioCodec.A_DTS } },
      { "DTS 96/24", new HashSet<AudioCodec> { AudioCodec.A_DTS } },
      { "DTS-HD HRA", new HashSet<AudioCodec> { AudioCodec.A_DTS_HD, AudioCodec.A_DTS } },
      { "DTS EXPRESS", new HashSet<AudioCodec> { AudioCodec.A_DTS } },

      { "FLAC", new HashSet<AudioCodec> { AudioCodec.A_FLAC } },
      { "OPUS", new HashSet<AudioCodec> { AudioCodec.A_OPUS } },
      { "LIBOPUS", new HashSet<AudioCodec> { AudioCodec.A_OPUS } },
      { "TTA1", new HashSet<AudioCodec> { AudioCodec.A_TTA1 } },
      { "TTA", new HashSet<AudioCodec> { AudioCodec.A_TTA1 } },
      { "VORBIS", new HashSet<AudioCodec> { AudioCodec.A_VORBIS } },
      { "WAVPACK4", new HashSet<AudioCodec> { AudioCodec.A_WAVPACK4 } },
      { "WAVPACK", new HashSet<AudioCodec> { AudioCodec.A_WAVPACK } },
      { "TRUEHD", new HashSet<AudioCodec> { AudioCodec.A_TRUEHD } },
      { "MLP", new HashSet<AudioCodec> { AudioCodec.A_MLP } },

      { "AAC", new HashSet<AudioCodec> { AudioCodec.A_AAC, AudioCodec.A_AAC_MPEG2_MAIN, AudioCodec.A_AAC_MPEG4_MAIN, 
                                         AudioCodec.A_AAC_MPEG2_LC, AudioCodec.A_AAC_MPEG2_LC_SBR, AudioCodec.A_AAC_MPEG4_LC, AudioCodec.A_AAC_MPEG4_LC_SBR, AudioCodec.A_AAC_MPEG4_LC_SBR_PS,
                                         AudioCodec.A_AAC_MPEG2_SSR, AudioCodec.A_AAC_MPEG4_SSR,
                                         AudioCodec.A_AAC_MPEG4_LTP } },
      { "AAC_LATM", new HashSet<AudioCodec> { AudioCodec.A_AAC } },
      { "AAC LC", new HashSet<AudioCodec> { AudioCodec.A_AAC_MPEG4_LC, AudioCodec.A_AAC_MPEG2_LC, AudioCodec.A_AAC_MPEG2_LC_SBR, AudioCodec.A_AAC_MPEG4_LC_SBR, AudioCodec.A_AAC_MPEG4_LC_SBR_PS } },
      { "AAC LTP", new HashSet<AudioCodec> { AudioCodec.A_AAC_MPEG4_LTP } },
      { "AAC SSR", new HashSet<AudioCodec> { AudioCodec.A_AAC_MPEG4_SSR, AudioCodec.A_AAC_MPEG2_SSR } },

      { "ALAC", new HashSet<AudioCodec> { AudioCodec.A_ALAC } },
      { "ATRAC1", new HashSet<AudioCodec> { AudioCodec.A_ATRAC1 } },
      { "ATRAC3", new HashSet<AudioCodec> { AudioCodec.A_ATRAC3 } },
      { "ATRAC3P", new HashSet<AudioCodec> { AudioCodec.A_ATRAC3 } },

      { "PCM_ALAW", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_MULAW", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_BLURAY", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_DVD", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_LXF", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_F32BE", new HashSet<AudioCodec> { AudioCodec.A_PCM_FLOAT_IEEE } },
      { "PCM_F32LE", new HashSet<AudioCodec> { AudioCodec.A_PCM_FLOAT_IEEE } },
      { "PCM_F64BE", new HashSet<AudioCodec> { AudioCodec.A_PCM_FLOAT_IEEE } },
      { "PCM_F64LE", new HashSet<AudioCodec> { AudioCodec.A_PCM_FLOAT_IEEE } },
      { "PCM_S16BE", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_BIG } },
      { "PCM_S16BE_PLANAR", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_BIG } },
      { "PCM_S16LE", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_S16LE_PLANAR", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_S24BE", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_BIG } },
      { "PCM_S24DAUD", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_S24LE", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_S24LE_PLANAR", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_S32BE", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_BIG } },
      { "PCM_S32LE", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_S32LE_PLANAR", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_S8", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_S8_PLANAR", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_U16BE", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_BIG } },
      { "PCM_U16LE", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_U24BE", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_BIG } },
      { "PCM_U24LE", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_U32BE", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_BIG } },
      { "PCM_U32LE", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_U8", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM_ZORK", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },
      { "PCM", new HashSet<AudioCodec> { AudioCodec.A_PCM_INT_LIT } },

      { "APE", new HashSet<AudioCodec> { AudioCodec.A_APE } },
      { "RA_144", new HashSet<AudioCodec> { AudioCodec.A_REAL_14_4 } },
      { "RA_288", new HashSet<AudioCodec> { AudioCodec.A_REAL_28_8 } },
      { "RALF", new HashSet<AudioCodec> { AudioCodec.A_REAL_ATRC } },

      { "WMALOSSLESS", new HashSet<AudioCodec> { AudioCodec.A_WMA9 } },
      { "WMAPRO", new HashSet<AudioCodec> { AudioCodec.A_WMA9 } },
      { "WMAV1", new HashSet<AudioCodec> { AudioCodec.A_WMA1 } },
      { "WMAV2", new HashSet<AudioCodec> { AudioCodec.A_WMA2 } },
      { "WMAVOICE", new HashSet<AudioCodec> { AudioCodec.A_WMA9 } },

      { "ADPCM_IMA_WAV", new HashSet<AudioCodec> { AudioCodec.A_ADPCM } },
      { "ADPCM", new HashSet<AudioCodec> { AudioCodec.A_ADPCM } },
      { "AMRNB", new HashSet<AudioCodec> { AudioCodec.A_AMR } },
    };

    #endregion

    private static readonly Dictionary<string, int> FiltersToSkip = new Dictionary<string, int>
    {
      { "ffdshow DXVA Video Decoder", 0 },
      { "ffdshow Video Decoder", 0 },
      { "ffdshow raw video filter", 0 }
    };

    #endregion

    public double[] Chapters { get; private set; }

    public string[] ChaptersName { get; private set; }

    public DirectShowHelper(StoreStreamAction storeAction, ILog logger)
    {
      Chapters = null;
      ChaptersName = null;
      _storeAction = storeAction;
      _logger = logger;
    }

    public void AnalyzeStreamChapters(IGraphBuilder graphBuilder)
    {
      try
      {
        //RETRIEVING THE CURRENT SPLITTER
        var foundfilter = new IBaseFilter[2];
        IEnumFilters enumFilters;
        graphBuilder.EnumFilters(out enumFilters);
        if (enumFilters != null)
        {
          try
          {
            int fetched;
            enumFilters.Reset();
            while (enumFilters.Next(1, foundfilter, out fetched) == 0)
            {
              if (foundfilter[0] != null && fetched == 1)
              {
                var pEs = foundfilter[0] as IAMExtendedSeeking;
                try
                {
                  if (pEs != null)
                  {
                    int markerCount;
                    if (pEs.get_MarkerCount(out markerCount) == 0 && markerCount > 0)
                    {
                      Chapters = new double[markerCount];
                      ChaptersName = new string[markerCount];
                      for (var i = 1; i <= markerCount; ++i)
                      {
                        double markerTime;
                        var hr = pEs.GetMarkerTime(i, out markerTime);
                        if (hr == 0)
                        {
                          Chapters[i - 1] = markerTime;
                        }
                        //fill up chapter names
                        string name;
                        hr = pEs.GetMarkerName(i, out name);
                        if (hr == 0)
                        {
                          ChaptersName[i - 1] = name;
                        }
                      }
                    }
                  }
                }
                finally
                {
                  DirectShowUtil.ReleaseComObject(foundfilter[0]);
                }
              }
            }
          }
          finally
          {
            DirectShowUtil.ReleaseComObject(enumFilters);
          }
        }
      }
      catch
      {
      }
    }

    public bool AnalyseStreams(IGraphBuilder graphBuilder)
    {
      try
      {
        //RETRIEVING THE CURRENT SPLITTER
        var foundfilter = new IBaseFilter[2];
        IEnumFilters enumFilters;
        graphBuilder.EnumFilters(out enumFilters);
        if (enumFilters != null)
        {
          try
          {
            enumFilters.Reset();
            int fetched;
            while (enumFilters.Next(1, foundfilter, out fetched) == 0)
            {
              if (foundfilter[0] != null && fetched == 1)
              {
                try
                {
                  if (Chapters == null)
                  {
                    var pEs = foundfilter[0] as IAMExtendedSeeking;
                    if (pEs != null)
                    {
                      int markerCount;
                      if (pEs.get_MarkerCount(out markerCount) == 0 && markerCount > 0)
                      {
                        Chapters = new double[markerCount];
                        ChaptersName = new string[markerCount];
                        for (var i = 1; i <= markerCount; ++i)
                        {
                          double markerTime;
                          var hr = pEs.GetMarkerTime(i, out markerTime);
                          if (hr == 0)
                          {
                            Chapters[i - 1] = markerTime;
                          }
                          //fill up chapter names
                          string name;
                          hr = pEs.GetMarkerName(i, out name);
                          if (hr == 0)
                          {
                            ChaptersName[i - 1] = name;
                          }
                        }
                      }
                    }
                  }
                  var pStrm = foundfilter[0] as IAMStreamSelect;
                  if (pStrm != null)
                  {
                    FilterInfo foundfilterinfos;
                    foundfilter[0].QueryFilterInfo(out foundfilterinfos);
                    var filter = foundfilterinfos.achName;
                    int cStreams;
                    pStrm.Count(out cStreams);

                    //GET STREAMS
                    for (var istream = 0; istream < cStreams; ++istream)
                    {
                      AMMediaType sType;
                      AMStreamSelectInfoFlags sFlag;
                      int sPdwGroup, sPlcId;
                      string sName;
                      object pppunk, ppobject;
                      var type = StreamType.Unknown;
                      //STREAM INFO
                      pStrm.Info(istream, out sType, out sFlag, out sPlcId,
                                 out sPdwGroup, out sName, out pppunk, out ppobject);
                      //Avoid listing ffdshow video filter's plugins amongst subtitle and audio streams and editions.
                      if (FiltersToSkip.ContainsKey(filter) &&
                          ((sPdwGroup == 1) || (sPdwGroup == 2) || (sPdwGroup == 18) || (sPdwGroup == 4)))
                      {
                        type = StreamType.Unknown;
                      }
                      //VIDEO
                      else if (sPdwGroup == 0)
                      {
                        type = StreamType.Video;
                      }
                      //AUDIO
                      else if (sPdwGroup == 1)
                      {
                        type = StreamType.Audio;
                      }
                      //SUBTITLE
                      else if (sPdwGroup == 2 && sName.LastIndexOf("off", StringComparison.Ordinal) == -1 && sName.LastIndexOf("Hide ", StringComparison.Ordinal) == -1 &&
                                sName.LastIndexOf("No ", StringComparison.Ordinal) == -1 && sName.LastIndexOf("Miscellaneous ", StringComparison.Ordinal) == -1)
                      {
                        type = StreamType.Subtitle;
                      }
                      //NO SUBTITILE TAG
                      else if ((sPdwGroup == 2 && (sName.LastIndexOf("off", StringComparison.Ordinal) != -1 || sName.LastIndexOf("No ", StringComparison.Ordinal) != -1)) ||
                                (sPdwGroup == 6590033 && sName.LastIndexOf("Hide ", StringComparison.Ordinal) != -1))
                      {
                        type = StreamType.Subtitle_hidden;
                      }
                      //DirectVobSub SHOW SUBTITLE TAG
                      else if (sPdwGroup == 6590033 && sName.LastIndexOf("Show ", StringComparison.Ordinal) != -1)
                      {
                        type = StreamType.Subtitle_shown;
                      }
                      //EDITION
                      else if (sPdwGroup == 18)
                      {
                        type = StreamType.Edition;
                      }
                      else if (sPdwGroup == 4) //Subtitle file
                      {
                        type = StreamType.Subtitle_file;
                      }
                      else if (sPdwGroup == 10) //Postprocessing filter
                      {
                        type = StreamType.PostProcessing;
                      }
                      _logger.Debug("DirectShowHelper: FoundStreams: Type={0}; Name={1}, Filter={2}, Id={3}, PDWGroup={4}, LCID={5}, sType={{{6}}},{{{7}}}",
                                type.ToString(), sName, filter, istream.ToString(),
                                sPdwGroup.ToString(), sPlcId.ToString(), sType.majorType, sType.subType);

                      if (_storeAction != null)
                      {
                        _storeAction(filter, sName, sPlcId, istream, sType, type, sFlag, pStrm);
                      }
                    }
                  }
                }
                finally
                {
                  DirectShowUtil.ReleaseComObject(foundfilter[0]);
                }
              }
            }
          }
          finally
          {
            DirectShowUtil.ReleaseComObject(enumFilters);
          }
        }
      }
      catch { }

      return true;
    }

    private static readonly Regex LavSplitterAudio = new Regex(@"A\:\s*(((?<name>.+)\[(?<language>\w+)\]|\[(?<language>\w+)\]|(?<name>.+))\s*\()?\s*(?<codec>[a-z0-9\s\.'_\-]+),\s*(?<freq>\d+)\s*Hz,\s*(?<channels>[a-z0-9\s\.'_\-]+)(,\s*s(?<bit>\d+))?(,\s*(?<bitrate>\d+)\s*kb/s)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HaaliSplitterAudio = new Regex(@"A\:\s*((?<name>.+)\[(?<language>\w+)\]|\[(?<language>\w+)\]|(?<name>.+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex FfdshowNameAudio = new Regex(@"Audio\s*-\s*(?<codec>\w+),\s*(?<channels>[^,]+),\s*(?<freq>\d+)\s*Hz", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex MpegSlitterNameAudio = new Regex(@"Audio\s*-\s*((?<language>\w+),\s*)?(?<codec>[^,]+),\s*(?<freq>[0-9\.\+\-]+)\s*(k)?Hz,\s*(?<channels>[^,]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex MpAudioNameAudio = new Regex(@"(?<language>\w+),\s*((?<name>.*))?\(Audio\s*(\d+)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex TsReaderAudio = new Regex(@"^(?<language>\w{2,3})$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex MpcNameAudio = new Regex(@"(?<language>\w+),\s*(?<codecName>.+)\(Audio\s*(\d+)\)\s*\-\s*(?<freq>\d+)\s*Hz,\s*(?<channnel>[0-9\.]+)\s+channel(s)?\s+(?<codec>.+)\s*\((?<internalcodec>[^\(]*)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public AudioStream MatchAudioStream(MediaInfoWrapper info, string filterName, string name, int lcid, int id, AMMediaType sType)
    {
      _logger.Debug("DirectShowHelper: match audio stream Name={0}, Filter={1}, Id={2}, LCID={3}", name, filterName, id, lcid);

      var m = LavSplitterAudio.Match(name);
      if (m.Success)
      {
        return LavSplitterAudioMatch(info, m, id);
      }

      m = HaaliSplitterAudio.Match(name);
      if (m.Success)
      {
        return HaaliSplitterAudioMatch(info, m, id);
      }
      
      m = FfdshowNameAudio.Match(name);
      if (m.Success)
      {
        return FfdshowAudioMatch(info, m, id);
      }

      m = MpegSlitterNameAudio.Match(name);
      if (m.Success)
      {
        return MpegSlitterAudioMatch(info, m, id);
      }

      m = MpAudioNameAudio.Match(name);
      if (m.Success)
      {
        return MpAudioMatch(info, m, id);
      }

      m = MpcNameAudio.Match(name);
      if (m.Success)
      {
        return MpcAudioMatch(info, m, id);
      }

      m = TsReaderAudio.Match(name);
      if (m.Success)
      {
        return TsReaderAudioMatch(info, m, id, sType);
      }

      return new AudioStream(id, id)
      {
        Language = LanguageHelper.UnknownLanguage,
        Name = string.Empty,
        Format = string.Empty,
        Codec = AudioCodec.A_UNDEFINED,
        Duration = TimeSpan.FromSeconds(0)
      };
    }

    public static VideoCodec GetVideoCodecByType(string videoType)
    {
      var result = GetVideoCodec(videoType);
      return result != null && result.Any() ? result.First() : VideoCodec.V_UNDEFINED;
    }

    public static AudioCodec GetAudioCodecByType(string videoType)
    {
      var result = GetAudioCodec(videoType);
      return result != null && result.Any() ? result.First() : AudioCodec.A_UNDEFINED;
    }

    private static AudioStream MpegSlitterAudioMatch(MediaInfoWrapper info, Match m, int id)
    {
      string language;
      string name;
      GetLavMainParameters(m, out language, out name);

      var codec = GetAudioCodec(m.Groups["codec"].Value);

      double frequency;
      if (!double.TryParse(m.Groups["freq"].Value, out frequency))
      {
        frequency = 0;
      }
      if (m.Groups[2].Value == "K" || m.Groups[2].Value == "k")
      {
        frequency *= 1000;
      }

      return null;
    }

    private static AudioStream TsReaderAudioMatch(MediaInfoWrapper info, Match m, int id, AMMediaType sType)
    {
      var result = id < info.AudioStreams.Count ? info.AudioStreams[id] : null;
      if (result != null)
      {
        var codecs = GetAudioCodecByMediaType(sType);
        string language = LanguageHelper.GetLanguageByShortName(m.Groups["language"].ToString());
        if (language.Equals(LanguageHelper.UnknownLanguage, StringComparison.OrdinalIgnoreCase) || result.Language != language || !codecs.Contains(result.Codec))
        {
          result = new AudioStream(id, id) { Name = string.Empty, Codec = codecs.FirstOrDefault(), Language = m.Groups["language"].Value };
        }
      }

      return result;
    }

    private static HashSet<AudioCodec> GetAudioCodecByMediaType(AMMediaType type)
    {
      HashSet<AudioCodec> result;
      return MediaCodecs.TryGetValue(type.subType, out result) ? result : new HashSet<AudioCodec>();
    }

    private static AudioStream MpAudioMatch(MediaInfoWrapper info, Match m, int id)
    {
      return null;
    }

    private static AudioStream MpcAudioMatch(MediaInfoWrapper info, Match m, int id)
    {
      return null;
    }

    private static string CheckNameEncoder(string name)
    {
      string result;
      return NameEncoders.TryGetValue(name, out result) ? result : name;
    }

    private static AudioStream FfdshowAudioMatch(MediaInfoWrapper info, Match m, int id)
    {
      var codec = GetAudioCodec(m.Groups["codec"].Value);

      int frequency;
      if (!int.TryParse(m.Groups["freq"].Value, out frequency))
      {
        frequency = 0;
      }

      var channelFrendly = m.Groups["channels"].Value;

      var result = info.AudioStreams.FirstOrDefault(x =>
                                                    (codec == null || codec.Contains(x.Codec))
                                                    && x.AudioChannelsFriendly.Equals(channelFrendly, StringComparison.OrdinalIgnoreCase)
                                                    && (int)x.SamplingRate == frequency) 
              ??  new AudioStream(id, id)
              {
                Language = LanguageHelper.UnknownLanguage, 
                Name = string.Empty, 
                Codec = codec != null ? codec.FirstOrDefault() : AudioCodec.A_UNDEFINED, 
                SamplingRate = frequency
              };

      return result;
    }

    private static HashSet<VideoCodec> GetVideoCodec(string sourceCodecName)
    {
      HashSet<VideoCodec> result;
      return !string.IsNullOrEmpty(sourceCodecName) && VideoCodecs.TryGetValue(sourceCodecName.ToUpper(), out result)
               ? result
               : null;
    }

    private static HashSet<AudioCodec> GetAudioCodec(string sourceCodecName)
    {
      HashSet<AudioCodec> result;
      return !string.IsNullOrEmpty(sourceCodecName) && AudioCodecs.TryGetValue(sourceCodecName.ToUpper(), out result)
               ? result
               : null;
    }

    private static string EncodeChannels(string source)
    {
      string result;
      return Channels.TryGetValue(source, out result) ? result : source;
    }

    private static int GetChannelNumbers(string source)
    {
        int result;
        return ChannelNumbers.TryGetValue(source, out result) ? result : 0;
    }

    private static AudioStream LavSplitterAudioMatch(MediaInfoWrapper info, Match m, int id)
    {
      string language;
      string name;
      GetLavMainParameters(m, out language, out name);

      var codec = GetAudioCodec(m.Groups["codec"].Value);

      int frequency;
      if (!int.TryParse(m.Groups["freq"].Value, out frequency))
      {
        frequency = 0;
      }

      var channelFrendly = EncodeChannels(m.Groups["channels"].Value);

      int bit;
      if (!int.TryParse(m.Groups["bit"].Value, out bit))
      {
        bit = 0;
      }

      var result = info.AudioStreams.FirstOrDefault(x => x.StreamNumber == id);
      if (result != null && (codec == null || codec.Contains(result.Codec))
          && result.AudioChannelsFriendly.Equals(channelFrendly, StringComparison.OrdinalIgnoreCase)
          && (int)result.SamplingRate == frequency
          && (result.BitDepth == bit || bit == 0 || result.Codec == AudioCodec.A_ADPCM) // ADPCM reports 16 bit instead of 4
          && (string.IsNullOrEmpty(language) || result.Language.Equals(LanguageHelper.UnknownLanguage, StringComparison.OrdinalIgnoreCase) || result.Language.Equals(language, StringComparison.OrdinalIgnoreCase)))
      {
        return result;
      }

      return info.AudioStreams.FirstOrDefault(x =>
                                             (codec == null || codec.Contains(x.Codec))
                                             && x.AudioChannelsFriendly.Equals(channelFrendly, StringComparison.OrdinalIgnoreCase)
                                             && (int)x.SamplingRate == frequency
                                             && (x.BitDepth == bit || bit == 0 || x.Codec == AudioCodec.A_ADPCM)
                                             && (string.IsNullOrEmpty(language) || x.Language.Equals(LanguageHelper.UnknownLanguage, StringComparison.OrdinalIgnoreCase) || x.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
                                             && ((string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(language) 
                                             && language.Equals(name, StringComparison.OrdinalIgnoreCase)) || x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
        ?? new AudioStream(id, id)
        {
            Codec = codec != null ? codec.FirstOrDefault() : AudioCodec.A_UNDEFINED, 
            Name = name, 
            Language = language, 
            Channel = GetChannelNumbers(m.Groups["channels"].Value), 
            BitDepth = bit, 
            SamplingRate = frequency
        };
    }

    private static AudioStream HaaliSplitterAudioMatch(MediaInfoWrapper info, Match m, int id)
    {
      string language;
      string name;
      GetLavMainParameters(m, out language, out name);

      var result = info.AudioStreams.FirstOrDefault(x => x.StreamNumber == id);
      if (result != null 
          && (string.IsNullOrEmpty(language) || result.Language.Equals(LanguageHelper.UnknownLanguage, StringComparison.OrdinalIgnoreCase) || result.Language.Equals(language, StringComparison.OrdinalIgnoreCase)))
      {
        return result;
      }

      return info.AudioStreams.FirstOrDefault(x =>
                                             (string.IsNullOrEmpty(language) || x.Language.Equals(LanguageHelper.UnknownLanguage, StringComparison.OrdinalIgnoreCase) || x.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
                                             && ((string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(language) 
                                             && language.Equals(name, StringComparison.OrdinalIgnoreCase)) || x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
        ?? new AudioStream(id, id)
        {
          Codec = AudioCodec.A_UNDEFINED, 
          Name = name, 
          Language = language, 
        };
    }

    private static void GetLavMainParameters(Match m, out string language, out string name)
    {
      name = CheckNameEncoder(m.Groups["name"].Value.TrimEnd());
      if (name.Equals(m.Groups["language"].Value, StringComparison.OrdinalIgnoreCase))
      {
        name = string.Empty;
      }

      language = LanguageHelper.GetLanguageByShortName(m.Groups["language"].Value);

      if (name.Equals(language, StringComparison.OrdinalIgnoreCase))
      {
        name = string.Empty;
      }
    }

    private static readonly Regex LavSplitterVideo = new Regex(@"V\:\s*(((?<name>.+)\[(?<language>\w+)\]|\[(?<language>\w+)\]|(?<name>.+))\s*\()?\s*(?<codec>[a-z0-9\s\.'_\-]+),\s*(?<output>[a-z0-9\s\.'_\-]+),\s*(?<width>\d+)x(?<height>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HaaliSplitterVideo = new Regex(@"V\:((\s*(?<name>.+)\[(?<language>\w+)\]|\[(?<language>\w+)\]))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public VideoStream MatchVideoStream(MediaInfoWrapper info, string filterName, string name, int lcid, int id, AMMediaType sType)
    {
      _logger.Debug("DirectShowHelper: match video stream Name={0}, Filter={1}, Id={2}, LCID={3}", name, filterName, id, lcid);

      var m = LavSplitterVideo.Match(name);
      if (m.Success)
      {
        return LavSplitterVideoMatch(info, m, id);
      }

      m = HaaliSplitterVideo.Match(name);
      if (m.Success)
      {
        return HaaliSplitterVideoMatch(info, m, id);
      }

      return new VideoStream(id, id)
      {
          Name = string.Empty, 
          Codec = VideoCodec.V_UNDEFINED
      };
    }

    private static VideoStream LavSplitterVideoMatch(MediaInfoWrapper info, Match m, int id)
    {
      string language;
      string name;
      GetLavMainParameters(m, out language, out name);

      var codec = GetVideoCodec(m.Groups["codec"].Value);
      int width;
      if (!int.TryParse(m.Groups["width"].Value, out width))
      {
        width = 0;
      }

      int height;
      if (!int.TryParse(m.Groups["height"].Value, out height))
      {
        height = 0;
      }

      var result = info.VideoStreams.FirstOrDefault(x => x.StreamNumber == id);
      if (result != null && (result.Width == width || width == 0) && (result.Height == height || height == 0)
          && (codec == null || codec.Contains(result.Codec))
          && (string.IsNullOrEmpty(language) || result.Language.Equals(LanguageHelper.UnknownLanguage, StringComparison.OrdinalIgnoreCase) || result.Language.Equals(language, StringComparison.OrdinalIgnoreCase)))
      {
        return result;
      }

      return info.VideoStreams.FirstOrDefault(x =>
                                              (x.Width == width || width == 0) && (x.Height == height || height == 0)
                                              && (codec == null || codec.Contains(x.Codec))
                                              && (string.IsNullOrEmpty(language) || x.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
                                              && ((string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(language)
                                              && language.Equals(name, StringComparison.OrdinalIgnoreCase)) || x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
          ?? new VideoStream(id, id)
          {
            Name = name, 
            Language = language,
            Codec = codec != null ? codec.FirstOrDefault() : VideoCodec.V_UNDEFINED, 
            Height = height, 
            Width = width
          };
    }

    private static VideoStream HaaliSplitterVideoMatch(MediaInfoWrapper info, Match m, int id)
    {
      string language;
      string name;
      GetLavMainParameters(m, out language, out name);

      var result = info.VideoStreams.FirstOrDefault(x => x.StreamNumber == id);
      if (result != null && 
          (string.IsNullOrEmpty(language) || result.Language.Equals(LanguageHelper.UnknownLanguage, StringComparison.OrdinalIgnoreCase) || result.Language.Equals(language, StringComparison.OrdinalIgnoreCase)))
      {
        return result;
      }

      return info.VideoStreams.FirstOrDefault(x =>
                                              (string.IsNullOrEmpty(language) || x.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
                                              && ((string.IsNullOrEmpty(x.Name) && !string.IsNullOrEmpty(language)
                                              && language.Equals(name, StringComparison.OrdinalIgnoreCase)) || x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
          ?? new VideoStream(id, id)
          {
            Name = name, 
            Codec = VideoCodec.V_UNDEFINED,
            Language = language
          };
    }
  }
}
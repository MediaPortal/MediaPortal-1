/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#pragma once

#ifndef __DEMUXER_AUDIO_HELPER_DEFINED
#define __DEMUXER_AUDIO_HELPER_DEFINED

#include "moreuuids.h"

#include <libavformat/isom.h>

#include <dvdmedia.h>
#include <MMReg.h>

const AVCodecTag mp_wav_tags[] = {
  { CODEC_ID_WAVPACK,           0x5756 },
  { CODEC_ID_TTA,               0x77A1 },
  { CODEC_ID_ADPCM_4XM,         MKTAG('4', 'X', 'M', 'A')},
  { CODEC_ID_ADPCM_ADX,         MKTAG('S', 'a', 'd', 'x')},
  { CODEC_ID_ADPCM_EA,          MKTAG('A', 'D', 'E', 'A')},
  { CODEC_ID_ADPCM_EA_MAXIS_XA, MKTAG('A', 'D', 'X', 'A')},
  { CODEC_ID_ADPCM_IMA_WS,      MKTAG('A', 'I', 'W', 'S')},
  { CODEC_ID_ADPCM_THP,         MKTAG('T', 'H', 'P', 'A')},
  { CODEC_ID_ADPCM_XA,          MKTAG('P', 'S', 'X', 'A')},
  { CODEC_ID_AMR_NB,            MKTAG('n', 'b',   0,   0)},
  { CODEC_ID_BINKAUDIO_DCT,     MKTAG('B', 'A', 'U', '1')},
  { CODEC_ID_BINKAUDIO_RDFT,    MKTAG('B', 'A', 'U', '2')},
  { CODEC_ID_COOK,              MKTAG('c', 'o', 'o', 'k')},
  { CODEC_ID_DSICINAUDIO,       MKTAG('D', 'C', 'I', 'A')},
  { CODEC_ID_EAC3,              MKTAG('E', 'A', 'C', '3')},
  { CODEC_ID_INTERPLAY_DPCM,    MKTAG('I', 'N', 'P', 'A')},
  { CODEC_ID_MLP,               MKTAG('M', 'L', 'P', ' ')},
  { CODEC_ID_MP1,               0x50},
  { CODEC_ID_MP4ALS,            MKTAG('A', 'L', 'S', ' ')},
  { CODEC_ID_MUSEPACK7,         MKTAG('M', 'P', 'C', ' ')},
  { CODEC_ID_MUSEPACK8,         MKTAG('M', 'P', 'C', '8')},
  { CODEC_ID_NELLYMOSER,        MKTAG('N', 'E', 'L', 'L')},
  { CODEC_ID_QCELP,             MKTAG('Q', 'c', 'l', 'p')},
  { CODEC_ID_QDM2,              MKTAG('Q', 'D', 'M', '2')},
  { CODEC_ID_RA_144,            MKTAG('1', '4', '_', '4')},
  { CODEC_ID_RA_288,            MKTAG('2', '8', '_', '8')},
  { CODEC_ID_ROQ_DPCM,          MKTAG('R', 'o', 'Q', 'A')},
  { CODEC_ID_SHORTEN,           MKTAG('s', 'h', 'r', 'n')},
  { CODEC_ID_SPEEX,             MKTAG('s', 'p', 'x', ' ')},
  { CODEC_ID_TWINVQ,            MKTAG('T', 'W', 'I', '2')},
  { CODEC_ID_WESTWOOD_SND1,     MKTAG('S', 'N', 'D', '1')},
  { CODEC_ID_XAN_DPCM,          MKTAG('A', 'x', 'a', 'n')},
  { CODEC_ID_MP4ALS,            MKTAG('A', 'L', 'S', ' ')},
  { CODEC_ID_NONE,              0}
};

const struct AVCodecTag * const mp_wav_taglists[] = { avformat_get_riff_audio_tags(), mp_wav_tags, 0};

class CDemuxerAudioHelper
{
public:
  static CMediaType InitAudioType(CodecID codecId, unsigned int &codecTag, const wchar_t *container);
  static WAVEFORMATEX *CreateWVFMTEX(const AVStream *stream, ULONG *size);
  static WAVEFORMATEXFFMPEG *CreateWVFMTEX_FF(const AVStream *stream, ULONG *size);
  static WAVEFORMATEX_HDMV_LPCM *CreateWVFMTEX_LPCM(const AVStream *stream, ULONG *size);
  static WAVEFORMATEXTENSIBLE *CreateWFMTEX_RAW_PCM(const AVStream *stream, ULONG *size, const GUID subtype, ULONG *sampleSize);
  static MPEG1WAVEFORMAT *CreateMP1WVFMT(const AVStream *stream, ULONG *size);
  static VORBISFORMAT *CreateVorbis(const AVStream *stream, ULONG *size);
  static VORBISFORMAT2 *CreateVorbis2(const AVStream *stream, ULONG *size);
};

#endif
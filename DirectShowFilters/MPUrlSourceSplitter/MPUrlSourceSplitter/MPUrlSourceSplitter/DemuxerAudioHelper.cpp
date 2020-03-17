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

#include "StdAfx.h"

#include "DemuxerAudioHelper.h"
#include "DemuxerUtils.h"

#define countof(array) (sizeof(array) / sizeof(array[0]))

// map codec ids to media subtypes
static struct
{
  CodecID codec;
  const GUID *subtype;
  unsigned codecTag;
  const GUID *format;
} audio_map [] =
{
  { CODEC_ID_AC3,        &MEDIASUBTYPE_DOLBY_AC3,         WAVE_FORMAT_DOLBY_AC3,  NULL },
  { CODEC_ID_AAC,        &MEDIASUBTYPE_AAC,               WAVE_FORMAT_AAC,        NULL },
  { CODEC_ID_AAC_LATM,   &MEDIASUBTYPE_LATM_AAC,          WAVE_FORMAT_LATM_AAC,   NULL },
  { CODEC_ID_DTS,        &MEDIASUBTYPE_WAVE_DTS,          NULL,                   NULL },
  { CODEC_ID_EAC3,       &MEDIASUBTYPE_DOLBY_DDPLUS,      NULL,                   NULL },
  { CODEC_ID_TRUEHD,     &MEDIASUBTYPE_DOLBY_TRUEHD,      NULL,                   NULL },
  { CODEC_ID_MLP,        &MEDIASUBTYPE_MLP,               WAVE_FORMAT_MLP,        NULL },
  { CODEC_ID_VORBIS,     &MEDIASUBTYPE_Vorbis2,           NULL,                   &FORMAT_VorbisFormat2 },
  { CODEC_ID_MP1,        &MEDIASUBTYPE_MPEG1AudioPayload, WAVE_FORMAT_MPEG,       NULL },
  { CODEC_ID_MP2,        &MEDIASUBTYPE_MPEG2_AUDIO,       WAVE_FORMAT_MPEG,       NULL },
  { CODEC_ID_MP3,        &MEDIASUBTYPE_MP3,               WAVE_FORMAT_MPEGLAYER3, NULL },
  { CODEC_ID_PCM_BLURAY, &MEDIASUBTYPE_BD_LPCM_AUDIO,     NULL,                   NULL },
  { CODEC_ID_PCM_DVD,    &MEDIASUBTYPE_DVD_LPCM_AUDIO,    NULL,                   NULL },
  { CODEC_ID_PCM_S16LE,  &MEDIASUBTYPE_PCM,               WAVE_FORMAT_PCM,        NULL },
  { CODEC_ID_PCM_S24LE,  &MEDIASUBTYPE_PCM,               WAVE_FORMAT_PCM,        NULL },
  { CODEC_ID_PCM_S32LE,  &MEDIASUBTYPE_PCM,               WAVE_FORMAT_PCM,        NULL },
  { CODEC_ID_PCM_F32LE,  &MEDIASUBTYPE_IEEE_FLOAT,        WAVE_FORMAT_IEEE_FLOAT, NULL },
  { CODEC_ID_WMAV1,      &MEDIASUBTYPE_WMAUDIO1,          WAVE_FORMAT_MSAUDIO1,   NULL },
  { CODEC_ID_WMAV2,      &MEDIASUBTYPE_WMAUDIO2,          WAVE_FORMAT_WMAUDIO2,   NULL },
  { CODEC_ID_WMAPRO,     &MEDIASUBTYPE_WMAUDIO3,          WAVE_FORMAT_WMAUDIO3,   NULL },
  { CODEC_ID_ADPCM_IMA_AMV, &MEDIASUBTYPE_IMA_AMV,        NULL,                   NULL },
  { CODEC_ID_FLAC,       &MEDIASUBTYPE_FLAC_FRAMED,       WAVE_FORMAT_FLAC,       NULL },
  { CODEC_ID_COOK,       &MEDIASUBTYPE_COOK,              WAVE_FORMAT_COOK,       NULL },
  { CODEC_ID_ATRAC1,     &MEDIASUBTYPE_ATRC,              WAVE_FORMAT_ATRC,       NULL },
  { CODEC_ID_ATRAC3,     &MEDIASUBTYPE_ATRC,              WAVE_FORMAT_ATRC,       NULL },
  { CODEC_ID_SIPR,       &MEDIASUBTYPE_SIPR,              WAVE_FORMAT_SIPR,       NULL },
  { CODEC_ID_RA_288,     &MEDIASUBTYPE_28_8,              WAVE_FORMAT_28_8,       NULL },
  { CODEC_ID_RA_144,     &MEDIASUBTYPE_14_4,              WAVE_FORMAT_14_4,       NULL },
  { CODEC_ID_RALF,       &MEDIASUBTYPE_RALF,              WAVE_FORMAT_RALF,       NULL },
  { CODEC_ID_ALAC,       &MEDIASUBTYPE_ALAC,              NULL,                   NULL },
  { CODEC_ID_MP4ALS,     &MEDIASUBTYPE_ALS,               NULL,                   NULL }
};

CMediaType CDemuxerAudioHelper::InitAudioType(CodecID codecId, unsigned int &codecTag, const wchar_t *container)
{
  CMediaType mediaType;

  mediaType.InitMediaType();
  mediaType.majortype = MEDIATYPE_Audio;
  mediaType.subtype = FOURCCMap(codecTag);
  mediaType.formattype = FORMAT_WaveFormatEx; //default value
  mediaType.SetSampleSize(256000);

  // check against values from the map above
  for(unsigned i = 0; i < countof(audio_map); ++i)
  {
    if (audio_map[i].codec == codecId)
    {
      if (audio_map[i].subtype)
      {
        mediaType.subtype = *audio_map[i].subtype;
      }

      if (audio_map[i].codecTag)
      {
        codecTag = audio_map[i].codecTag;
      }

      if (audio_map[i].format)
      {
         mediaType.formattype = *audio_map[i].format;
      }
      break;
    }
  }

  // special cases
  switch(codecId)
  {
  case CODEC_ID_PCM_F64LE:
    // Qt PCM
    if (codecTag == MKTAG('f', 'l', '6', '4'))
    {
      mediaType.subtype = MEDIASUBTYPE_PCM_FL64_le;
    }
    break;
  case CODEC_ID_PCM_S16BE:
    if (wcscmp(container, L"mpeg") == 0)
    {
       mediaType.subtype = MEDIASUBTYPE_DVD_LPCM_AUDIO;
    }
    break;
  }

  return mediaType;
}

WAVEFORMATEX *CDemuxerAudioHelper::CreateWVFMTEX(const AVStream *stream, ULONG *size)
{
  HRESULT result = S_OK;
  WAVEFORMATEX *wvfmt = NULL;
  CHECK_POINTER_DEFAULT_HRESULT(result, stream);
  CHECK_POINTER_DEFAULT_HRESULT(result, size);

  if (SUCCEEDED(result))
  {
    wvfmt = (WAVEFORMATEX *)CoTaskMemAlloc(sizeof(WAVEFORMATEX) + stream->codec->extradata_size);
    CHECK_POINTER_HRESULT(result, wvfmt, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      memset(wvfmt, 0, sizeof(WAVEFORMATEX));

      wvfmt->wFormatTag = stream->codec->codec_tag;
      wvfmt->nChannels = stream->codec->channels ? stream->codec->channels : 2;
      wvfmt->nSamplesPerSec = stream->codec->sample_rate ? stream->codec->sample_rate : 48000;
      wvfmt->nAvgBytesPerSec = stream->codec->bit_rate / 8;

      if ((stream->codec->codec_id == CODEC_ID_AAC) || (stream->codec->codec_id == CODEC_ID_AAC_LATM))
      {
        wvfmt->wBitsPerSample = 0;
        wvfmt->nBlockAlign = 1;
      }
      else
      {
        wvfmt->wBitsPerSample = CDemuxerUtils::GetBitsPerSample(stream->codec, false);

        if (stream->codec->block_align > 0 )
        {
          wvfmt->nBlockAlign = stream->codec->block_align;
        }
        else
        {
          /*if (wvfmt->wBitsPerSample == 0)
          {
            DbgOutString(L"BitsPerSample is 0, no good!");
          }*/

          wvfmt->nBlockAlign = (WORD)((wvfmt->nChannels * av_get_bits_per_sample_fmt(stream->codec->sample_fmt)) / 8);
        }
      }
      if (wvfmt->nAvgBytesPerSec == 0)
      {
        wvfmt->nAvgBytesPerSec = (wvfmt->nSamplesPerSec * wvfmt->nChannels * wvfmt->wBitsPerSample) >> 3;
      }

      if (stream->codec->extradata_size > 0)
      {
        wvfmt->cbSize = stream->codec->extradata_size;
        memcpy((BYTE *)wvfmt + sizeof(WAVEFORMATEX), stream->codec->extradata, stream->codec->extradata_size);
      }
    }

    *size = SUCCEEDED(result) ? (sizeof(WAVEFORMATEX) + stream->codec->extradata_size) : 0;
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM(wvfmt));
  return wvfmt;
}

WAVEFORMATEXFFMPEG *CDemuxerAudioHelper::CreateWVFMTEX_FF(const AVStream *stream, ULONG *size)
{
  HRESULT result = S_OK;
  WAVEFORMATEXFFMPEG *wfex_ff = NULL;
  CHECK_POINTER_DEFAULT_HRESULT(result, stream);
  CHECK_POINTER_DEFAULT_HRESULT(result, size);

  if (SUCCEEDED(result))
  {
    WAVEFORMATEX *wvfmt = CDemuxerAudioHelper::CreateWVFMTEX(stream, size);
    CHECK_POINTER_HRESULT(result, wvfmt, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      const size_t diff_size = sizeof(WAVEFORMATEXFFMPEG) - sizeof(WAVEFORMATEX);
      wfex_ff = (WAVEFORMATEXFFMPEG *)CoTaskMemAlloc(diff_size + *size);
      CHECK_POINTER_HRESULT(result, wfex_ff, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        memset(wfex_ff, 0, sizeof(WAVEFORMATEXFFMPEG));
        memcpy(&wfex_ff->wfex, wvfmt, *size);

        wfex_ff->nCodecId = stream->codec->codec_id;
      }

      FREE_MEM(wvfmt);
    }

    *size = SUCCEEDED(result) ? (sizeof(WAVEFORMATEXFFMPEG) + wfex_ff->wfex.cbSize) : 0;
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM(wfex_ff));
  return wfex_ff;
}

WAVEFORMATEX_HDMV_LPCM *CDemuxerAudioHelper::CreateWVFMTEX_LPCM(const AVStream *stream, ULONG *size)
{
  HRESULT result = S_OK;
  WAVEFORMATEX_HDMV_LPCM *lpcm = NULL;
  CHECK_POINTER_DEFAULT_HRESULT(result, stream);
  CHECK_POINTER_DEFAULT_HRESULT(result, size);

  if (SUCCEEDED(result))
  {
    WAVEFORMATEX *wvfmt = CDemuxerAudioHelper::CreateWVFMTEX(stream, size);
    CHECK_POINTER_HRESULT(result, wvfmt, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      lpcm = (WAVEFORMATEX_HDMV_LPCM *)CoTaskMemAlloc(sizeof(WAVEFORMATEX_HDMV_LPCM));
      CHECK_POINTER_HRESULT(result, lpcm, result, E_OUTOFMEMORY);
    }

    if (SUCCEEDED(result))
    {
      memset(lpcm, 0, sizeof(WAVEFORMATEX_HDMV_LPCM));
      memcpy(lpcm, wvfmt, sizeof(WAVEFORMATEX));

      lpcm->cbSize = sizeof(WAVEFORMATEX_HDMV_LPCM) - sizeof(WAVEFORMATEX);
      BYTE channel_conf = 0;

      switch (stream->codec->channel_layout)
      {
      case AV_CH_LAYOUT_MONO:
        channel_conf = 1;
        break;
      case AV_CH_LAYOUT_STEREO:
        channel_conf = 3;
        break;
      case AV_CH_LAYOUT_SURROUND:
        channel_conf = 4;
        break;
      case AV_CH_LAYOUT_2_1:
        channel_conf = 5;
        break;
      case AV_CH_LAYOUT_4POINT0:
        channel_conf = 6;
        break;
      case AV_CH_LAYOUT_2_2:
        channel_conf = 7;
        break;
      case AV_CH_LAYOUT_5POINT0:
        channel_conf = 8;
        break;
      case AV_CH_LAYOUT_5POINT1:
        channel_conf = 9;
        break;
      case AV_CH_LAYOUT_7POINT0:
        channel_conf = 10;
        break;
      case AV_CH_LAYOUT_7POINT1:
        channel_conf = 11;
        break;
      default:
        channel_conf = 0;
      }

      lpcm->channel_conf = channel_conf;
    }

    FREE_MEM(wvfmt);
    *size = SUCCEEDED(result) ? sizeof(WAVEFORMATEX_HDMV_LPCM) : 0;
  }

  return lpcm;
}

WAVEFORMATEXTENSIBLE *CDemuxerAudioHelper::CreateWFMTEX_RAW_PCM(const AVStream *stream, ULONG *size, const GUID subtype, ULONG *sampleSize)
{
  HRESULT result = S_OK;
  WAVEFORMATEXTENSIBLE *wfex = NULL;
  CHECK_POINTER_DEFAULT_HRESULT(result, stream);
  CHECK_POINTER_DEFAULT_HRESULT(result, size);

  if (SUCCEEDED(result))
  {
    wfex = (WAVEFORMATEXTENSIBLE *)CoTaskMemAlloc(sizeof(WAVEFORMATEXTENSIBLE));
    CHECK_POINTER_HRESULT(result, wfex, result, E_OUTOFMEMORY);
  }

  if (SUCCEEDED(result))
  {
    memset(wfex, 0, sizeof(*wfex));

    WAVEFORMATEX *wfe = &wfex->Format;
    wfe->wFormatTag = (WORD)subtype.Data1;
    wfe->nChannels = stream->codec->channels;
    wfe->nSamplesPerSec = stream->codec->sample_rate;

    if ((stream->codec->sample_fmt == AV_SAMPLE_FMT_S32) && (stream->codec->bits_per_raw_sample > 0))
    {
      wfe->wBitsPerSample = stream->codec->bits_per_raw_sample > 24 ? 32 : (stream->codec->bits_per_raw_sample > 16 ? 24 : 16);
    }
    else
    {
      wfe->wBitsPerSample = av_get_bits_per_sample_fmt(stream->codec->sample_fmt);
    }

    wfe->nBlockAlign = wfe->nChannels * wfe->wBitsPerSample / 8;
    wfe->nAvgBytesPerSec = wfe->nSamplesPerSec * wfe->nBlockAlign;

    DWORD dwChannelMask = 0;
    if (((wfe->wBitsPerSample > 16) || (wfe->nSamplesPerSec > 48000)) && (wfe->nChannels <= 2))
    {
      dwChannelMask = wfe->nChannels == 2 ? (SPEAKER_FRONT_LEFT | SPEAKER_FRONT_RIGHT) : SPEAKER_FRONT_CENTER;
    }
    else if (wfe->nChannels > 2)
    {
      dwChannelMask = (DWORD)stream->codec->channel_layout;

      if (dwChannelMask == 0)
      {
        dwChannelMask = (DWORD)av_get_default_channel_layout(wfe->nChannels);
      }
    }

    if (dwChannelMask != 0)
    {
      wfex->Format.wFormatTag = WAVE_FORMAT_EXTENSIBLE;
      wfex->Format.cbSize = sizeof(*wfex) - sizeof(wfex->Format);
      wfex->dwChannelMask = dwChannelMask;

      if ((stream->codec->sample_fmt == AV_SAMPLE_FMT_S32) && (stream->codec->bits_per_raw_sample > 0))
      {
        wfex->Samples.wValidBitsPerSample = stream->codec->bits_per_raw_sample;
      }
      else
      {
        wfex->Samples.wValidBitsPerSample = wfex->Format.wBitsPerSample;
      }

      wfex->SubFormat = subtype;
      *size = sizeof(WAVEFORMATEXTENSIBLE);
    }
    else
    {
      *size = sizeof(WAVEFORMATEX);
    }

    *sampleSize = wfe->wBitsPerSample * wfe->nChannels / 8;
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM(wfex));
  return wfex;
}

MPEG1WAVEFORMAT *CDemuxerAudioHelper::CreateMP1WVFMT(const AVStream *stream, ULONG *size)
{
  HRESULT result = S_OK;
  MPEG1WAVEFORMAT *mpwvfmt = NULL;
  CHECK_POINTER_DEFAULT_HRESULT(result, stream);
  CHECK_POINTER_DEFAULT_HRESULT(result, size);

  if (SUCCEEDED(result))
  {
    WAVEFORMATEX *wvfmt = CreateWVFMTEX(stream, size);
    CHECK_POINTER_HRESULT(result, wvfmt, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      mpwvfmt = (MPEG1WAVEFORMAT *)CoTaskMemAlloc(sizeof(MPEG1WAVEFORMAT));
      CHECK_POINTER_HRESULT(result, mpwvfmt, result, E_OUTOFMEMORY);
    }

    if (SUCCEEDED(result))
    {
      memset(mpwvfmt, 0, sizeof(MPEG1WAVEFORMAT));
      memcpy(&mpwvfmt->wfx, wvfmt, sizeof(WAVEFORMATEX));

      mpwvfmt->dwHeadBitrate = stream->codec->bit_rate;
      mpwvfmt->fwHeadMode = stream->codec->channels == 1 ? ACM_MPEG_SINGLECHANNEL : ACM_MPEG_DUALCHANNEL;
      mpwvfmt->fwHeadLayer = (stream->codec->codec_id == CODEC_ID_MP1) ? ACM_MPEG_LAYER1 : ACM_MPEG_LAYER2;

      if (stream->codec->sample_rate == 0)
      {
        stream->codec->sample_rate = 48000;
      }

      mpwvfmt->wfx.wFormatTag = WAVE_FORMAT_MPEG;
      mpwvfmt->wfx.nBlockAlign = (stream->codec->codec_id == CODEC_ID_MP1)
        ? (12 * stream->codec->bit_rate / stream->codec->sample_rate) * 4
        : 144 * stream->codec->bit_rate / stream->codec->sample_rate;

      mpwvfmt->wfx.cbSize = sizeof(MPEG1WAVEFORMAT) - sizeof(WAVEFORMATEX);

      *size = sizeof(MPEG1WAVEFORMAT);
    }

    FREE_MEM(wvfmt);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM(mpwvfmt));
  return mpwvfmt;
}

VORBISFORMAT *CDemuxerAudioHelper::CreateVorbis(const AVStream *stream, ULONG *size)
{
  HRESULT result = S_OK;
  VORBISFORMAT *vfmt = NULL;
  CHECK_POINTER_DEFAULT_HRESULT(result, stream);
  CHECK_POINTER_DEFAULT_HRESULT(result, size);

  if (SUCCEEDED(result))
  {
    vfmt = (VORBISFORMAT *)CoTaskMemAlloc(sizeof(VORBISFORMAT));
    CHECK_POINTER_HRESULT(result, vfmt, result, E_OUTOFMEMORY);
  }

  if (SUCCEEDED(result))
  {
    memset(vfmt, 0, sizeof(VORBISFORMAT));

    vfmt->nChannels = stream->codec->channels;
    vfmt->nSamplesPerSec = stream->codec->sample_rate;
    vfmt->nAvgBitsPerSec = stream->codec->bit_rate;
    vfmt->nMinBitsPerSec = vfmt->nMaxBitsPerSec = (DWORD)-1;

    *size = sizeof(VORBISFORMAT);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM(vfmt));
  return vfmt;
}

VORBISFORMAT2 *CDemuxerAudioHelper::CreateVorbis2(const AVStream *stream, ULONG *size)
{
  HRESULT result = S_OK;
  VORBISFORMAT2* pvf2 = NULL;
  CHECK_POINTER_DEFAULT_HRESULT(result, stream);
  CHECK_POINTER_DEFAULT_HRESULT(result, size);

  if (SUCCEEDED(result))
  {
    BYTE *extraData = stream->codec->extradata;
    int totalSize = 0;

    // for valid Vorbis format there are 3 blocks
    int sizes[3] = {0, 0, 0};

    // read the number of blocks, and then the sizes of the individual blocks
    int i = 0;

    for (BYTE n = *extraData++; (SUCCEEDED(result) && (n > 0)); n--)
    {
      // into 3rd item goes somethnig special, so sizes[] can be filled up to 2nd item
      CHECK_CONDITION_EXECUTE(i < 2, result = E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        int size = 0;

        // Xiph Lacing
        do
        {
          size += *extraData++;
        }
        while (*extraData++ == 0xFF);

        sizes[i++] = size;
      }
    }

    // sizes[] must be filled up to 2nd item
    CHECK_CONDITION_EXECUTE(i != 1, result = E_OUTOFMEMORY);

    totalSize = sizes[0] + sizes[1] + sizes[2];

    // get the size of the last block
    sizes[2] = stream->codec->extradata_size - (int)(extraData - stream->codec->extradata) - totalSize;
    totalSize += sizes[2];

    // 3 blocks is the currently valid Vorbis format
    if  (SUCCEEDED(result))
    {
      VORBISFORMAT2* pvf2 = (VORBISFORMAT2*)CoTaskMemAlloc(sizeof(VORBISFORMAT2) + totalSize);
      CHECK_POINTER_HRESULT(result, pvf2, result, E_OUTOFMEMORY);
    }

    if (SUCCEEDED(result))
    {
      memset(pvf2, 0, sizeof(VORBISFORMAT2));

      pvf2->Channels = stream->codec->channels;
      pvf2->SamplesPerSec = stream->codec->sample_rate;
      pvf2->BitsPerSample = CDemuxerUtils::GetBitsPerSample(stream->codec, false);

      BYTE *p2 = (BYTE *)pvf2 + sizeof(VORBISFORMAT2);
      for (unsigned int i = 0; i < 3; extraData += sizes[i], p2 += sizes[i], i++)
      {
        memcpy(p2, extraData, pvf2->HeaderSize[i] = sizes[i]);
      }
    }

    *size = (SUCCEEDED(result)) ? (sizeof(VORBISFORMAT2) + totalSize) : 0;
  }

  
  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM(pvf2));
  return NULL;

}
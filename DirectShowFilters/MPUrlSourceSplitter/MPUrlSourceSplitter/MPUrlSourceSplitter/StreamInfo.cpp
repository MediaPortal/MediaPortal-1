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

#include "StreamInfo.h"
#include "DemuxerUtils.h"
#include "DemuxerAudioHelper.h"
#include "DemuxerVideoHelper.h"

#include <libavformat/isom.h>

static wchar_t *CreateVOBSubHeaderFromMP4(int width, int height, MOVStreamContext *context, const BYTE *buffer, int bufferSize)
{
  wchar_t *result = NULL;

  if (bufferSize >= (16 * 4))
  {
    int w = ((context != NULL) && (context->width != 0)) ? context->width : width;
    int h = ((context != NULL) && (context->height != 0)) ? context->height : height;

    result = Duplicate(L"");
    result = AppendString(result, L"# VobSub index file, v7 (do not modify this line!)\n");
    
    wchar_t *temp = FormatString(L"size: %dx%d\n", w, h);
    result = AppendString(result, temp);
    FREE_MEM(temp);

    result = AppendString(result, L"palette: ");

    for (int i = 0; i < (16 * 4); i += 4)
    {
      BYTE y = (buffer[i + 1] - 16) * 255 / 219;
      BYTE u = buffer[i + 2];
      BYTE v = buffer[i + 3];

      BYTE r = (BYTE)min(max(1.0 * y + 1.4022 * (v - 128), 0), 255);
      BYTE g = (BYTE)min(max(1.0 * y - 0.3456 * (u - 128) - 0.7145 * (v - 128), 0), 255);
      BYTE b = (BYTE)min(max(1.0 * y + 1.7710 * (u - 128), 0) , 255);

      wchar_t *temp = FormatString(L"%02x%02x%02x", r, g, b);
      if (i != 0)
      {
        result = AppendString(result, L",");
      }
      result = AppendString(result, temp);
      FREE_MEM(temp);
    }

    result = AppendString(result, L"\n");
  }

  return result;
}

static wchar_t *GetDefaultVOBSubHeader(int width, int height)
{
  wchar_t *result = Duplicate(L"# VobSub index file, v7 (do not modify this line!)\n");

  wchar_t *temp = FormatString(L"size: %dx%d\n", width, height);
  result = AppendString(result, temp);
  FREE_MEM(temp);

  result = AppendString(result, L"palette: 000000,f0f0f0,dddddd,222222,3333fa,1111bb,fa3333,bb1111,33fa33,11bb11,fafa33,bbbb11,fa33fa,bb11bb,33fafa,11bbbb\n");

  return result;
}

CStreamInfo::CStreamInfo(HRESULT *result)
{
  this->mediaTypes = NULL;
  this->streamDescription = NULL;
  this->containerFormat = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->mediaTypes = new CMediaTypeCollection(result);
    CHECK_POINTER_HRESULT(*result, this->mediaTypes, *result, E_OUTOFMEMORY);
  }
}

CStreamInfo::CStreamInfo(HRESULT *result, AVFormatContext *formatContext, AVStream *stream, const wchar_t *containerFormat)
{
  this->mediaTypes = NULL;
  this->streamDescription = NULL;
  this->containerFormat = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->mediaTypes = new CMediaTypeCollection(result);
    CHECK_POINTER_HRESULT(*result, this->mediaTypes, *result, E_OUTOFMEMORY);

    SET_STRING_HRESULT_WITH_NULL(this->containerFormat, containerFormat, *result);

    if (SUCCEEDED(*result))
    {
      switch(stream->codec->codec_type)
      {
      case AVMEDIA_TYPE_AUDIO:
        *result = CreateAudioMediaType(formatContext, stream);
        break;
      case AVMEDIA_TYPE_VIDEO:
        *result = CreateVideoMediaType(formatContext, stream);
        break;
      case AVMEDIA_TYPE_SUBTITLE:
        *result = CreateSubtitleMediaType(formatContext, stream);
        break;
      default:
        *result = E_FAIL;
        break;
      }

      this->streamDescription = CDemuxerUtils::GetStreamDescription(stream);
    }
  }
}

CStreamInfo::~CStreamInfo(void)
{
  FREE_MEM(this->streamDescription);
  FREE_MEM(this->containerFormat);
  FREE_MEM_CLASS(this->mediaTypes);
}

/* get methods */

CMediaTypeCollection *CStreamInfo::GetMediaTypes(void)
{
  return this->mediaTypes;
}

const wchar_t *CStreamInfo::GetStreamDescription(void)
{
  return this->streamDescription;
}

/* set method */

bool CStreamInfo::SetStreamDescription(const wchar_t *streamDescription)
{
  SET_STRING_RETURN_WITH_NULL(this->streamDescription, streamDescription);
}

/* other methods */

HRESULT CStreamInfo::CreateAudioMediaType(AVFormatContext *formatContext, AVStream *stream)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, formatContext);
  CHECK_POINTER_DEFAULT_HRESULT(result, stream);

  if (SUCCEEDED(result))
  {
    // make sure DTS Express has valid settings
    if ((stream->codec->codec_id == CODEC_ID_DTS) && (stream->codec->codec_tag == 0xA2))
    {
      stream->codec->channels = stream->codec->channels ? stream->codec->channels : 2;
      stream->codec->sample_rate = stream->codec->sample_rate ? stream->codec->sample_rate : 48000;
    }

    if (stream->codec->codec_tag == 0)
    {
      stream->codec->codec_tag = av_codec_get_tag(mp_wav_taglists, stream->codec->codec_id);
    }

    CHECK_CONDITION_HRESULT(result, (stream->codec->channels == 0) || (stream->codec->sample_rate == 0), E_FAIL, result);

    if (SUCCEEDED(result))
    {
      CMediaType mediaType = CDemuxerAudioHelper::InitAudioType(stream->codec->codec_id, stream->codec->codec_tag, this->containerFormat);

      if (mediaType.formattype == FORMAT_WaveFormatEx)
      {
        // special logic for the MPEG1 Audio Formats (MP1, MP2)
        if (mediaType.subtype == MEDIASUBTYPE_MPEG1AudioPayload)
        {
          mediaType.pbFormat = (BYTE *)CDemuxerAudioHelper::CreateMP1WVFMT(stream, &mediaType.cbFormat);
        }
        else if (mediaType.subtype == MEDIASUBTYPE_BD_LPCM_AUDIO)
        {
          mediaType.pbFormat = (BYTE *)CDemuxerAudioHelper::CreateWVFMTEX_LPCM(stream, &mediaType.cbFormat);

          CMediaType *addMediaType = new CMediaType(mediaType, &result);
          CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));

          mediaType.subtype = MEDIASUBTYPE_HDMV_LPCM_AUDIO;
        }
        else if (((mediaType.subtype == MEDIASUBTYPE_PCM) || (mediaType.subtype == MEDIASUBTYPE_IEEE_FLOAT)) && (stream->codec->codec_tag != WAVE_FORMAT_EXTENSIBLE))
        {
          // create raw PCM media type
          mediaType.pbFormat = (BYTE *)CDemuxerAudioHelper::CreateWFMTEX_RAW_PCM(stream, &mediaType.cbFormat, mediaType.subtype, &mediaType.lSampleSize);
        }
        else
        {
          WAVEFORMATEX *wvfmt = CDemuxerAudioHelper::CreateWVFMTEX(stream, &mediaType.cbFormat);

          if ((stream->codec->codec_tag == WAVE_FORMAT_EXTENSIBLE) && (stream->codec->extradata_size >= 22))
          {
            // the WAVEFORMATEXTENSIBLE GUID is not recognized by the audio renderers
            // set the actual subtype as GUID

            WAVEFORMATEXTENSIBLE *wvfmtex = (WAVEFORMATEXTENSIBLE *)wvfmt;
            mediaType.subtype = wvfmtex->SubFormat;
          }
          mediaType.pbFormat = (BYTE *)wvfmt;

          if (stream->codec->codec_id == CODEC_ID_FLAC)
          {
            // these are required to block accidental connection to ReClock

            wvfmt->nAvgBytesPerSec = (wvfmt->nSamplesPerSec * wvfmt->nChannels * wvfmt->wBitsPerSample) >> 3;
            wvfmt->nBlockAlign = 1;
            mediaType.subtype = MEDIASUBTYPE_FLAC_FRAMED;

            CMediaType *addMediaType = new CMediaType(mediaType, &result);
            CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));

            mediaType.subtype = MEDIASUBTYPE_FLAC;
          }
          else if (stream->codec->codec_id == CODEC_ID_EAC3)
          {
            CMediaType *addMediaType = new CMediaType(mediaType, &result);
            CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));

            mediaType.subtype = MEDIASUBTYPE_DOLBY_DDPLUS_ARCSOFT;
          }
          else if (stream->codec->codec_id == CODEC_ID_DTS)
          {
            wvfmt->wFormatTag = WAVE_FORMAT_DTS2;
          }
          else if (stream->codec->codec_id == CODEC_ID_TRUEHD)
          {
            CMediaType *addMediaType = new CMediaType(mediaType, &result);
            CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));

            mediaType.subtype = MEDIASUBTYPE_DOLBY_TRUEHD_ARCSOFT;
          }
          else if (stream->codec->codec_id == CODEC_ID_AAC)
          {
            mediaType.subtype = MEDIASUBTYPE_AAC_ADTS;
            wvfmt->wFormatTag = (WORD)mediaType.subtype.Data1;

            CMediaType *addMediaType = new CMediaType(mediaType, &result);
            CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));

            mediaType.subtype = MEDIASUBTYPE_AAC;
            wvfmt->wFormatTag = (WORD)mediaType.subtype.Data1;
          }
          else if (stream->codec->codec_id == CODEC_ID_AAC_LATM)
          {
            CMediaType *addMediaType = new CMediaType(mediaType, &result);
            CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));

            mediaType.subtype = MEDIASUBTYPE_MPEG_LOAS;
            wvfmt->wFormatTag = (WORD)mediaType.subtype.Data1;
          }
        }
      }
      else if ((mediaType.formattype == FORMAT_VorbisFormat2) && (mediaType.subtype == MEDIASUBTYPE_Vorbis2))
      {
        // with Matroska and Ogg we know how to split up the extradata and put it into a VorbisFormat2
        if ((_wcsicmp(this->containerFormat, L"matroska") == 0) || (_wcsicmp(this->containerFormat, L"ogg")))
        {
          BYTE *vorbis2 = (BYTE *)CDemuxerAudioHelper::CreateVorbis2(stream, &mediaType.cbFormat);

          if (vorbis2)
          {
            mediaType.pbFormat = vorbis2;

            CMediaType *addMediaType = new CMediaType(mediaType, &result);
            CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));
          }
        }

        // old vorbis header without extradata
        mediaType.subtype = MEDIASUBTYPE_Vorbis;
        mediaType.formattype = FORMAT_VorbisFormat;
        mediaType.pbFormat = (BYTE *)CDemuxerAudioHelper::CreateVorbis(stream, &mediaType.cbFormat);
      }

      CMediaType *addMediaType = new CMediaType(mediaType, &result);
      CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));

      if (SUCCEEDED(result))
      {
        // create our special media type
        CMediaType *specialMediaType = new CMediaType();
        CHECK_POINTER_HRESULT(result, specialMediaType, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          specialMediaType->InitMediaType();
          specialMediaType->SetSampleSize(256000);
          specialMediaType->majortype = MEDIATYPE_Audio;
          specialMediaType->subtype = MEDIASUBTYPE_FFMPEG_AUDIO;
          specialMediaType->formattype = FORMAT_WaveFormatExFFMPEG;
          specialMediaType->pbFormat = (BYTE *)CDemuxerAudioHelper::CreateWVFMTEX_FF(stream, &specialMediaType->cbFormat);
        }

        CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(specialMediaType), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(specialMediaType));
      }
    }
  }

  return result;
}

HRESULT CStreamInfo::CreateVideoMediaType(AVFormatContext *formatContext, AVStream *stream)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, formatContext);
  CHECK_POINTER_DEFAULT_HRESULT(result, stream);

  if (SUCCEEDED(result))
  {
    if ((stream->codec->codec_tag == 0) && (stream->codec->codec_id != CODEC_ID_DVVIDEO))
    {
      stream->codec->codec_tag = av_codec_get_tag(mp_bmp_taglists, stream->codec->codec_id);
    }

    if ((stream->codec->width == 0) ||
        (stream->codec->height == 0) ||
        (
          ((stream->codec->codec_id == CODEC_ID_MPEG1VIDEO) || (stream->codec->codec_id == CODEC_ID_MPEG2VIDEO)) &&
          ((stream->codec->time_base.den == 0) || (stream->codec->time_base.num == 0))
        ))
    {
      result = E_FAIL;
    }
  }

  if (SUCCEEDED(result))
  {
    CMediaType mediaType = CDemuxerVideoHelper::InitVideoType(stream->codec->codec_id, stream->codec->codec_tag, this->containerFormat);

    mediaType.SetTemporalCompression(TRUE);
    mediaType.SetVariableSize();

    // somewhat hackish to force VIH for AVI content.
    // TODO: figure out why exactly this is required
    if ((wcscmp(this->containerFormat, L"avi") == 0) && (stream->codec->codec_id != CODEC_ID_H264))
    {
      mediaType.formattype = FORMAT_VideoInfo;
    }

    // if we need aspect info, we switch to VIH2
    AVRational r = stream->sample_aspect_ratio;
    AVRational rc = stream->codec->sample_aspect_ratio;

    if ((mediaType.formattype == FORMAT_VideoInfo) && 
        (
          ((r.den > 0) && (r.num > 0) && ((r.den > 1) || (r.num > 1))) ||
          ((rc.den > 0) && (rc.num > 0) && ((rc.den > 1) || (rc.num > 1)))
        ))
    {
      mediaType.formattype = FORMAT_VideoInfo2;
    }

    if (mediaType.formattype == FORMAT_VideoInfo)
    {
      mediaType.pbFormat = (BYTE *)CDemuxerVideoHelper::CreateVIH(stream, &mediaType.cbFormat);
    }
    else if (mediaType.formattype == FORMAT_VideoInfo2)
    {
      mediaType.pbFormat = (BYTE *)CDemuxerVideoHelper::CreateVIH2(stream, &mediaType.cbFormat, this->containerFormat);

      if (mediaType.subtype == MEDIASUBTYPE_WVC1)
      {
        // if we send the cyberlink subtype first, it'll work with it, and with ffdshow, dmo and mpc-hc internal

        VIDEOINFOHEADER2 *vih2 = (VIDEOINFOHEADER2 *)mediaType.pbFormat;

        if (*((BYTE*)vih2 + sizeof(VIDEOINFOHEADER2)) == 0)
        {
          mediaType.subtype = MEDIASUBTYPE_WVC1_CYBERLINK;

          CMediaType *addMediaType = new CMediaType(mediaType, &result);
          CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));

          mediaType.subtype = MEDIASUBTYPE_WVC1_ARCSOFT;

          addMediaType = new CMediaType(mediaType, &result);
          CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));
        }

        mediaType.subtype = MEDIASUBTYPE_WVC1;
      }
      else if (mediaType.subtype == MEDIASUBTYPE_WMVA)
      {
        CMediaType *addMediaType = new CMediaType(mediaType, &result);
        CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));

        mediaType.subtype = MEDIASUBTYPE_WVC1;
        VIDEOINFOHEADER2 *vih2 = (VIDEOINFOHEADER2 *)mediaType.pbFormat;
        vih2->bmiHeader.biCompression = mediaType.subtype.Data1;
      }
    }
    else if (mediaType.formattype == FORMAT_MPEGVideo)
    {
      mediaType.pbFormat = (BYTE *)CDemuxerVideoHelper::CreateMPEG1VI(stream, &mediaType.cbFormat);
    }
    else if (mediaType.formattype == FORMAT_MPEG2Video)
    {
      bool annexB = ((wcscmp(this->containerFormat, L"rawvideo") == 0) || (wcscmp(this->containerFormat, L"rtp") == 0) || (wcscmp(this->containerFormat, L"rtsp") == 0) || (wcscmp(this->containerFormat, L"avi") == 0));
      mediaType.pbFormat = (BYTE *)CDemuxerVideoHelper::CreateMPEG2VI(stream, &mediaType.cbFormat, this->containerFormat, annexB);

      if ((stream->codec->codec_id == CODEC_ID_H264) && annexB)
      {
        mediaType.subtype = MEDIASUBTYPE_H264;
        MPEG2VIDEOINFO *mp2vi = (MPEG2VIDEOINFO *)mediaType.pbFormat;
        mp2vi->dwFlags = 0;
        mp2vi->hdr.bmiHeader.biCompression = mediaType.subtype.Data1;
      }
    }

    if ((stream->codec->codec_id == CODEC_ID_RAWVIDEO) && (stream->codec->codec_tag == 0))
    {
      switch (stream->codec->pix_fmt)
      {
      case PIX_FMT_BGRA:
        {
          mediaType.subtype = MEDIASUBTYPE_ARGB32;
          
          CMediaType *addMediaType = new CMediaType(mediaType, &result);
          CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));

          mediaType.subtype = MEDIASUBTYPE_RGB32;
        }
        break;
      case PIX_FMT_BGR24:
        mediaType.subtype = MEDIASUBTYPE_RGB24;
        break;
      default:
        //DbgLog((LOG_TRACE, 10, L"::CreateVideoMediaType(): Unsupported raw video pixel format"));
        break;
      }
    }

    if (stream->codec->codec_id == CODEC_ID_MJPEG)
    {
      BITMAPINFOHEADER *pBMI = NULL;

      CDemuxerVideoHelper::VideoFormatTypeHandler(mediaType.pbFormat, &mediaType.formattype, &pBMI, NULL, NULL, NULL);

      DWORD fourCC = MKTAG('M','J','P','G');

      // if the original fourcc is different to MJPG, add this one
      if (fourCC != pBMI->biCompression)
      {
        CMediaType *addMediaType = new CMediaType(mediaType, &result);
        CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));

        mediaType.subtype = FOURCCMap(fourCC);
        pBMI->biCompression = fourCC;
      }
    }

    CMediaType *addMediaType = new CMediaType(mediaType, &result);
    CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));
  }

  return result;
}

HRESULT CStreamInfo::CreateSubtitleMediaType(AVFormatContext *formatContext, AVStream *stream)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, formatContext);
  CHECK_POINTER_DEFAULT_HRESULT(result, stream);

  if (SUCCEEDED(result))
  {
    // skip teletext
    CHECK_CONDITION_EXECUTE(stream->codec->codec_id == CODEC_ID_DVB_TELETEXT, result = E_FAIL);
  }

  if (SUCCEEDED(result))
  {
    CMediaType mediaType;
    mediaType.majortype = MEDIATYPE_Subtitle;
    mediaType.formattype = FORMAT_SubtitleInfo;

    int extra = stream->codec->extradata_size;
    if (stream->codec->codec_id == CODEC_ID_MOV_TEXT)
    {
      extra = 0;
    }

    // create format info
    SUBTITLEINFO *subInfo = (SUBTITLEINFO *)mediaType.AllocFormatBuffer(sizeof(SUBTITLEINFO) + extra);
    CHECK_POINTER_HRESULT(result, subInfo, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      memset(subInfo, 0, mediaType.FormatLength());

      if (av_dict_get(stream->metadata, "language", NULL, 0) != NULL)
      {
        char *language = av_dict_get(stream->metadata, "language", NULL, 0)->value;
        strncpy_s(subInfo->IsoLang, 4, language, _TRUNCATE);
      }
      else
      {
        strncpy_s(subInfo->IsoLang, 4, "und", _TRUNCATE);
      }

      if (av_dict_get(stream->metadata, "title", NULL, 0) != NULL)
      {
        // read metadata
        char *title = av_dict_get(stream->metadata, "title", NULL, 0)->value;

        // convert to wchar
        MultiByteToWideChar(CP_UTF8, 0, title, -1, subInfo->TrackName, 256);
      }

      subInfo->dwOffset = sizeof(SUBTITLEINFO);

      // find first video stream
      AVStream *vidStream = NULL;
      for (unsigned i = 0; i < formatContext->nb_streams; i++)
      {
        if (formatContext->streams[i]->codec->codec_type == AVMEDIA_TYPE_VIDEO)
        {
          vidStream = formatContext->streams[i];
          break;
        }
      }

      // extra data
      if ((wcscmp(this->containerFormat, L"mp4") == 0) && (stream->codec->codec_id == CODEC_ID_DVD_SUBTITLE))
      {
        wchar_t *strVobSubHeader = CreateVOBSubHeaderFromMP4(vidStream ? vidStream->codec->width : 720, vidStream ? vidStream->codec->height : 576, (MOVStreamContext *)stream->priv_data, stream->codec->extradata, extra);
        char *strVobSubHeaderA = ConvertToMultiByteW(strVobSubHeader);
        CHECK_POINTER_HRESULT(result, strVobSubHeaderA, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          unsigned int length = strlen(strVobSubHeaderA);
          mediaType.ReallocFormatBuffer((ULONG)(sizeof(SUBTITLEINFO) + length));
          memcpy(mediaType.pbFormat + sizeof(SUBTITLEINFO), strVobSubHeaderA, length);
        }

        FREE_MEM(strVobSubHeaderA);
        FREE_MEM(strVobSubHeader);
      }
      else if ((wcscmp(this->containerFormat, L"mpeg") == 0)  && (stream->codec->codec_id == CODEC_ID_DVD_SUBTITLE))
      {
        wchar_t *strVobSubHeader = GetDefaultVOBSubHeader(vidStream ? vidStream->codec->width : 720, vidStream ? vidStream->codec->height : 576);
        char *strVobSubHeaderA = ConvertToMultiByteW(strVobSubHeader);
        CHECK_POINTER_HRESULT(result, strVobSubHeaderA, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          unsigned int length = strlen(strVobSubHeaderA);
          mediaType.ReallocFormatBuffer((ULONG)(sizeof(SUBTITLEINFO) + length));
          memcpy(mediaType.pbFormat + sizeof(SUBTITLEINFO), strVobSubHeaderA, length);

          mediaType.subtype = MEDIASUBTYPE_VOBSUB;

          CMediaType *addMediaType = new CMediaType(mediaType, &result);
          CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));

          // offer the DVD subtype
          addMediaType = new CMediaType();
          CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            addMediaType->majortype = MEDIATYPE_Video;
            addMediaType->subtype = MEDIASUBTYPE_DVD_SUBPICTURE;
            addMediaType->formattype = FORMAT_None;
          }

          CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));
        }

        FREE_MEM(strVobSubHeaderA);
        FREE_MEM(strVobSubHeader);

        return result;
      }
      else
      {
        memcpy(mediaType.pbFormat + sizeof(SUBTITLEINFO), stream->codec->extradata, extra);
      }

      mediaType.subtype = stream->codec->codec_id == CODEC_ID_TEXT ? MEDIASUBTYPE_UTF8 :
        stream->codec->codec_id == CODEC_ID_MOV_TEXT ? MEDIASUBTYPE_UTF8 :
        stream->codec->codec_id == CODEC_ID_SSA ? MEDIASUBTYPE_ASS :
        stream->codec->codec_id == CODEC_ID_HDMV_PGS_SUBTITLE ? MEDIASUBTYPE_HDMVSUB :
        stream->codec->codec_id == CODEC_ID_DVD_SUBTITLE ? MEDIASUBTYPE_VOBSUB :
        stream->codec->codec_id == CODEC_ID_DVB_SUBTITLE ? MEDIASUBTYPE_DVB_SUBTITLES :
        MEDIASUBTYPE_NULL;

      CMediaType *addMediaType = new CMediaType(mediaType, &result);
      CHECK_POINTER_HRESULT(result, addMediaType, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, this->mediaTypes->Add(addMediaType), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(addMediaType));
    }
  }

  return result;
}
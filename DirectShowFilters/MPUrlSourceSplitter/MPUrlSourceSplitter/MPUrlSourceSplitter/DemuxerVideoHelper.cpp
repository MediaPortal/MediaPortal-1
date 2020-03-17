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

#include "DemuxerVideoHelper.h"
#include "ExtraDataParser.h"
#include "H264Nalu.h"

#pragma warning(push)
#pragma warning(disable:4244)
extern "C" {
#define __STDC_CONSTANT_MACROS
#include "libavformat/avformat.h"
#include "libavutil/intreadwrite.h"
#include "libavutil/pixdesc.h"
}
#pragma warning(pop)

// 125fps is the highest we accept as "sane"
#define MIN_TIME_PER_FRAME                                            80000

#define countof(array) (sizeof(array) / sizeof(array[0]))

// map codec ids to media subtypes
static struct
{
  CodecID codec;
  const GUID *subtype;
  unsigned codecTag;
  const GUID *format;
} video_map[] =
{
  { CODEC_ID_H263,       &MEDIASUBTYPE_H263,         NULL,                   NULL },
  { CODEC_ID_H263I,      &MEDIASUBTYPE_H263,         NULL,                   NULL },
  { CODEC_ID_H264,       &MEDIASUBTYPE_AVC1,         MKTAG('A','V','C','1'), &FORMAT_MPEG2Video },
  { CODEC_ID_MPEG1VIDEO, &MEDIASUBTYPE_MPEG1Payload, NULL,                   &FORMAT_MPEGVideo  },
  { CODEC_ID_MPEG2VIDEO, &MEDIASUBTYPE_MPEG2_VIDEO,  NULL,                   &FORMAT_MPEG2Video },
  { CODEC_ID_RV10,       &MEDIASUBTYPE_RV10,         MKTAG('R','V','1','0'), &FORMAT_VideoInfo2 },
  { CODEC_ID_RV20,       &MEDIASUBTYPE_RV20,         MKTAG('R','V','2','0'), &FORMAT_VideoInfo2 },
  { CODEC_ID_RV30,       &MEDIASUBTYPE_RV30,         MKTAG('R','V','3','0'), &FORMAT_VideoInfo2 },
  { CODEC_ID_RV40,       &MEDIASUBTYPE_RV40,         MKTAG('R','V','4','0'), &FORMAT_VideoInfo2 },
  { CODEC_ID_AMV,        &MEDIASUBTYPE_AMVV,         MKTAG('A','M','V','V'), NULL },
};

CMediaType CDemuxerVideoHelper::InitVideoType(CodecID codecId, unsigned int &codecTag, const wchar_t *container)
{
  CMediaType mediaType;
  mediaType.InitMediaType();
  mediaType.majortype = MEDIATYPE_Video;
  mediaType.subtype = FOURCCMap(codecTag);
  mediaType.formattype = FORMAT_VideoInfo;    //default value

  // check against values from the map above
  for(unsigned i = 0; i < countof(video_map); ++i)
  {
    if (video_map[i].codec == codecId)
    {
      if (video_map[i].subtype)
      {
        mediaType.subtype = *video_map[i].subtype;
      }

      if (video_map[i].codecTag)
      {
        codecTag = video_map[i].codecTag;
      }

      if (video_map[i].format)
      {
         mediaType.formattype = *video_map[i].format;
      }
      break;
    }
  }

  switch(codecId)
  {
  // all these codecs should use VideoInfo2
  case CODEC_ID_ASV1:
  case CODEC_ID_ASV2:
  case CODEC_ID_FLV1:
  case CODEC_ID_HUFFYUV:
  case CODEC_ID_WMV3:
    mediaType.formattype = FORMAT_VideoInfo2;
    break;
  case CODEC_ID_MPEG4:
    mediaType.formattype = (wcscmp(container, L"mp4") == 0) ? FORMAT_MPEG2Video : FORMAT_VideoInfo2;
    break;
  case CODEC_ID_VC1:
    CHECK_CONDITION_EXECUTE(codecTag != MKTAG('W','M','V','A'), codecTag = MKTAG('W','V','C','1'));
    mediaType.formattype = FORMAT_VideoInfo2;
    mediaType.subtype = FOURCCMap(codecTag);
    break;
  case CODEC_ID_DVVIDEO:
    CHECK_CONDITION_EXECUTE(codecTag == 0, mediaType.subtype = MEDIASUBTYPE_DVCP);
    break;
  }

  return mediaType;
}

VIDEOINFOHEADER *CDemuxerVideoHelper::CreateVIH(const AVStream *stream, ULONG *size)
{
  VIDEOINFOHEADER *pvi = NULL;
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, stream);
  CHECK_POINTER_DEFAULT_HRESULT(result, size);

  if (SUCCEEDED(result))
  {
    pvi = (VIDEOINFOHEADER *)CoTaskMemAlloc(ULONG(sizeof(VIDEOINFOHEADER) + stream->codec->extradata_size));
    CHECK_POINTER_HRESULT(result, pvi, result, E_OUTOFMEMORY);
  }

  if (SUCCEEDED(result))
  {
    memset(pvi, 0, sizeof(VIDEOINFOHEADER));

    // get the frame rate
    REFERENCE_TIME r_avg, avg_avg, tb_avg;
    if ((stream->r_frame_rate.den > 0) && (stream->r_frame_rate.num > 0) && ((r_avg = av_rescale(DSHOW_TIME_BASE, stream->r_frame_rate.den, stream->r_frame_rate.num)) > MIN_TIME_PER_FRAME))
    {
      pvi->AvgTimePerFrame = r_avg;
    }
    else if ((stream->avg_frame_rate.den > 0) && (stream->avg_frame_rate.num > 0) && ((avg_avg = av_rescale(DSHOW_TIME_BASE, stream->avg_frame_rate.den, stream->avg_frame_rate.num)) > MIN_TIME_PER_FRAME))
    {
      pvi->AvgTimePerFrame = avg_avg;
    }
    else if ((stream->codec->time_base.den > 0) && (stream->codec->time_base.num > 0) && (stream->codec->ticks_per_frame > 0) && ((tb_avg = av_rescale(DSHOW_TIME_BASE, stream->codec->time_base.num * stream->codec->ticks_per_frame, stream->codec->time_base.den)) > MIN_TIME_PER_FRAME))
    {
      pvi->AvgTimePerFrame = tb_avg;
    }

    pvi->dwBitErrorRate = 0;
    pvi->dwBitRate = stream->codec->bit_rate;
    RECT empty_tagrect = {0,0,0,0};
    pvi->rcSource = empty_tagrect;    // some codecs like wmv are setting that value to the video current value
    pvi->rcTarget = empty_tagrect;
    pvi->rcTarget.right = pvi->rcSource.right = stream->codec->width;
    pvi->rcTarget.bottom = pvi->rcSource.bottom = stream->codec->height;

    memcpy((BYTE*)&pvi->bmiHeader + sizeof(BITMAPINFOHEADER), stream->codec->extradata, stream->codec->extradata_size);
    pvi->bmiHeader.biSize = ULONG(sizeof(BITMAPINFOHEADER) + stream->codec->extradata_size);

    pvi->bmiHeader.biWidth = stream->codec->width;
    pvi->bmiHeader.biHeight = stream->codec->height;
    pvi->bmiHeader.biBitCount = stream->codec->bits_per_coded_sample;

    // validate biBitCount is set to something useful
    if ((pvi->bmiHeader.biBitCount == 0) || (stream->codec->codec_id == CODEC_ID_RAWVIDEO))
    {
      pvi->bmiHeader.biBitCount = av_get_bits_per_pixel2(stream->codec->pix_fmt);
    }
    pvi->bmiHeader.biSizeImage = DIBSIZE(pvi->bmiHeader);   // calculating this value doesn't really make a lot of sense, but apparently some decoders freak out if its 0

    pvi->bmiHeader.biCompression = stream->codec->codec_tag;
    // TOFIX The bitplanes is depending on the subtype
    pvi->bmiHeader.biPlanes = 1;
    pvi->bmiHeader.biClrUsed = 0;
    pvi->bmiHeader.biClrImportant = 0;
    pvi->bmiHeader.biYPelsPerMeter = 0;
    pvi->bmiHeader.biXPelsPerMeter = 0;

    *size = sizeof(VIDEOINFOHEADER) + stream->codec->extradata_size;
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM(pvi));
  return pvi;
}

VIDEOINFOHEADER2 *CDemuxerVideoHelper::CreateVIH2(const AVStream *stream, ULONG *size)
{
  return CDemuxerVideoHelper::CreateVIH2(stream, size, L"");
}

#define VC1_CODE_RES0                                                 0x00000100
#define IS_VC1_MARKER(x)                                              (((x) & ~0xFF) == VC1_CODE_RES0)

VIDEOINFOHEADER2 *CDemuxerVideoHelper::CreateVIH2(const AVStream *stream, ULONG *size, const wchar_t *container)
{
  VIDEOINFOHEADER2 *vih2 = NULL;
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, stream);
  CHECK_POINTER_DEFAULT_HRESULT(result, size);
  CHECK_POINTER_DEFAULT_HRESULT(result, container);

  if (SUCCEEDED(result))
  {
    int extra = 0;
    BYTE *extraData = NULL;
    BOOL zeroPad = FALSE;

    if ((stream->codec->codec_id == CODEC_ID_VC1) && (stream->codec->extradata_size))
    {
      int i = 0;
      for (i = 0; i < (stream->codec->extradata_size - 4); i++)
      {
        uint32_t code = AV_RB32(stream->codec->extradata + i);
        if (IS_VC1_MARKER(code))
        {
          break;
        }
      }

      if (i == 0)
      {
        zeroPad = true;
      }
      /*else if (i > 1)
      {
        DbgLog((LOG_TRACE, 10, L"CLAVFVideoHelper::CreateVIH2(): VC-1 extradata does not start at position 0/1, but %d", i));
      }*/
    }

    // create a VIH that we'll convert
    VIDEOINFOHEADER *vih = CreateVIH(stream, size);
    CHECK_POINTER_HRESULT(result, vih, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      if (stream->codec->extradata_size > 0)
      {
        extra = stream->codec->extradata_size;

        // increase extra size by one, because VIH2 requires one 0 byte between header and extra data
        if (zeroPad)
        {
          extra++;
        }

        extraData = stream->codec->extradata;
      }

      vih2 = (VIDEOINFOHEADER2 *)CoTaskMemAlloc(sizeof(VIDEOINFOHEADER2) + extra);
      CHECK_POINTER_HRESULT(result, vih2, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        memset(vih2, 0, sizeof(VIDEOINFOHEADER2));

        vih2->rcSource = vih->rcSource;
        vih2->rcTarget = vih->rcTarget;
        vih2->dwBitRate = vih->dwBitRate;
        vih2->dwBitErrorRate = vih->dwBitErrorRate;
        vih2->AvgTimePerFrame = vih->AvgTimePerFrame;

        // calculate aspect ratio
        AVRational r = stream->sample_aspect_ratio;
        AVRational rc = stream->codec->sample_aspect_ratio;

        int num = vih->bmiHeader.biWidth, den = vih->bmiHeader.biHeight;

        if ((r.den > 0) && (r.num > 0) && ((r.den > 1) || (r.num > 1)))
        {
          av_reduce(&num, &den, (int64_t)r.num * num, (int64_t)r.den * den, 255);
        }
        else if ((rc.den > 0) && (rc.num > 0) && ((rc.den > 1) || (rc.num > 1)))
        {
          av_reduce(&num, &den, (int64_t)rc.num * num, (int64_t)rc.den * den, 255);
        }
        else
        {
          if (stream->codec->codec_id == CODEC_ID_RV40)
          {
            AVDictionaryEntry *w = av_dict_get(stream->metadata, "rm_width", NULL, 0);
            AVDictionaryEntry *h = av_dict_get(stream->metadata, "rm_height", NULL, 0);

            if ((w != NULL) && (h != NULL))
            {
              num = atoi(w->value);
              den = atoi(h->value);
            }
          }
          av_reduce(&num, &den, num, den, num);
        }
        vih2->dwPictAspectRatioX = num;
        vih2->dwPictAspectRatioY = den;

        memcpy(&vih2->bmiHeader, &vih->bmiHeader, sizeof(BITMAPINFOHEADER));
        vih2->bmiHeader.biSize = sizeof(BITMAPINFOHEADER) + extra;

        vih2->dwInterlaceFlags = 0;
        vih2->dwCopyProtectFlags = 0;
        vih2->dwControlFlags = 0;
        vih2->dwReserved2 = 0;

        if (extra != 0)
        {
          // the first byte after the infoheader has to be 0 in mpeg-ts
          if (zeroPad)
          {
            *((BYTE*)vih2 + sizeof(VIDEOINFOHEADER2)) = 0;

            // after that, the extradata .. size reduced by one again
            memcpy((BYTE*)vih2 + sizeof(VIDEOINFOHEADER2) + 1, extraData, extra - 1);

          }
          else
          {
            memcpy((BYTE*)vih2 + sizeof(VIDEOINFOHEADER2), extraData, extra);
          }
        }

        *size = sizeof(VIDEOINFOHEADER2) + extra;
      }
    }

    FREE_MEM(vih);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM(vih2));
  return vih2;
}

MPEG1VIDEOINFO *CDemuxerVideoHelper::CreateMPEG1VI(const AVStream *stream, ULONG *size)
{
  MPEG1VIDEOINFO *mp1vi = NULL;
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, stream);
  CHECK_POINTER_DEFAULT_HRESULT(result, size);

  if (SUCCEEDED(result))
  {
    int extra = 0;
    BYTE *extraData = NULL;

    // create a VIH that we'll convert
    VIDEOINFOHEADER *vih = CreateVIH(stream, size);
    CHECK_POINTER_HRESULT(result, vih, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      if (stream->codec->extradata_size > 0)
      {
        extra = stream->codec->extradata_size;
        extraData = stream->codec->extradata;
      }

      mp1vi = (MPEG1VIDEOINFO *)CoTaskMemAlloc(sizeof(MPEG1VIDEOINFO) + extra);
      CHECK_POINTER_HRESULT(result, mp1vi, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        memset(mp1vi, 0, sizeof(MPEG1VIDEOINFO));

        // the MPEG1VI is a thin wrapper around a VIH, so its easy!
        memcpy(&mp1vi->hdr, vih, sizeof(VIDEOINFOHEADER));
        mp1vi->hdr.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);

        mp1vi->dwStartTimeCode = 0; // is this not 0 anywhere..?
        mp1vi->hdr.bmiHeader.biPlanes = 0;
        mp1vi->hdr.bmiHeader.biCompression = 0;

        // copy extradata over
        if (extra != 0)
        {
          CExtraDataParser parser = CExtraDataParser(extraData, extra);
          mp1vi->cbSequenceHeader = (DWORD)parser.ParseMPEGSequenceHeader(mp1vi->bSequenceHeader);
        }

        *size = SIZE_MPEG1VIDEOINFO(mp1vi);
      }
    }
    
    FREE_MEM(vih);
  }

  return mp1vi;
}

MPEG2VIDEOINFO *CDemuxerVideoHelper::CreateMPEG2VI(const AVStream *stream, ULONG *size)
{
  return CDemuxerVideoHelper::CreateMPEG2VI(stream, size, L"");
}

MPEG2VIDEOINFO *CDemuxerVideoHelper::CreateMPEG2VI(const AVStream *stream, ULONG *size, const wchar_t *container)
{
  return CDemuxerVideoHelper::CreateMPEG2VI(stream, size, container, false);
}

DWORD avc_quant(BYTE *src, BYTE *dst, int extralen)
{
  DWORD cb = 0;
  BYTE* src_end = (BYTE *) src + extralen;
  BYTE* dst_end = (BYTE *) dst + extralen;
  src += 5;
  
  // two runs, for sps and pps
  for (int i = 0; i < 2; i++)
  {
    for (int n = *(src++) & 0x1f; n > 0; n--)
    {
      unsigned len = (((unsigned)src[0] << 8) | src[1]) + 2;

      if ((src + len > src_end) || (dst + len > dst_end))
      {
        ASSERT(0);
        break;
      }

      memcpy(dst, src, len);
      src += len;
      dst += len;
      cb += len;
    }
  }
  return cb;
}

size_t avc_parse_annexb(BYTE *extra, int extrasize, BYTE *dst)
{
  size_t dstSize = 0;

  CH264Nalu Nalu;
  Nalu.SetBuffer(extra, extrasize, 0);
  while (Nalu.ReadNext())
  {
    const BYTE *data = Nalu.GetDataBuffer();

    if ((Nalu.GetType() == NALU_TYPE_SPS) || (Nalu.GetType() == NALU_TYPE_PPS))
    {
      size_t len = Nalu.GetDataLength();
      AV_WB16(dst+dstSize, (uint16_t)len);
      dstSize += 2;
      memcpy(dst+dstSize, Nalu.GetDataBuffer(), Nalu.GetDataLength());
      dstSize += Nalu.GetDataLength();
    }
  }
  return dstSize;
}

MPEG2VIDEOINFO *CDemuxerVideoHelper::CreateMPEG2VI(const AVStream *stream, ULONG *size, const wchar_t *container, bool annexB)
{
  MPEG2VIDEOINFO *mp2vi = NULL;
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, stream);
  CHECK_POINTER_DEFAULT_HRESULT(result, size);
  CHECK_POINTER_DEFAULT_HRESULT(result, container);

  if (SUCCEEDED(result))
  {
    int extra = 0;
    BYTE *extraData = NULL;

    // create a VIH that we'll convert
    VIDEOINFOHEADER2 *vih2 = CreateVIH2(stream, size);
    CHECK_POINTER_HRESULT(result, vih2, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      if (stream->codec->extradata_size > 0)
      {
        extra = stream->codec->extradata_size;
        extraData = stream->codec->extradata;
      }

      mp2vi = (MPEG2VIDEOINFO *)CoTaskMemAlloc(sizeof(MPEG2VIDEOINFO) + max(extra - 4, 0));
      CHECK_POINTER_HRESULT(result, mp2vi, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        memset(mp2vi, 0, sizeof(MPEG2VIDEOINFO));
        memcpy(&mp2vi->hdr, vih2, sizeof(VIDEOINFOHEADER2));
        mp2vi->hdr.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);

        // set profile/level if we know them
        mp2vi->dwProfile = (stream->codec->profile != FF_PROFILE_UNKNOWN) ? stream->codec->profile : 0;
        mp2vi->dwLevel = (stream->codec->level != FF_LEVEL_UNKNOWN) ? stream->codec->level : 0;
        //mp2vi->dwFlags = 4; // where do we get flags otherwise..?

        if (extra > 0)
        {
          bool copyUntouched = false;
          
          // don't even go there for mpeg-ts for now, we supply annex-b
          if ((stream->codec->codec_id == CODEC_ID_H264) && (!annexB))
          {
            if (*(char *)extraData == 1)
            {
              if (extraData[1])
              {
                mp2vi->dwProfile = extraData[1];
              }

              if (extraData[3])
              {
                mp2vi->dwLevel = extraData[3];
              }

              mp2vi->dwFlags = (extraData[4] & 3) + 1;
              mp2vi->cbSequenceHeader = avc_quant(extraData, (BYTE *)(&mp2vi->dwSequenceHeader[0]), extra);
            }
            else
            {
              // MPEG-TS AnnexB
              mp2vi->dwFlags = 4;
              mp2vi->cbSequenceHeader = (DWORD)avc_parse_annexb(extraData, extra, (BYTE *)(&mp2vi->dwSequenceHeader[0]));
            }
          }
          else if (stream->codec->codec_id == CODEC_ID_MPEG2VIDEO)
          {
            CExtraDataParser parser = CExtraDataParser(extraData, extra);
            mp2vi->cbSequenceHeader = (DWORD)parser.ParseMPEGSequenceHeader((BYTE *)&mp2vi->dwSequenceHeader[0]);
            mp2vi->hdr.bmiHeader.biPlanes = 0;
            mp2vi->hdr.bmiHeader.biCompression = 0;
          }
          else
          {
            copyUntouched = true;
          }

          if (copyUntouched)
          {
            mp2vi->cbSequenceHeader = extra;
            memcpy(&mp2vi->dwSequenceHeader[0], extraData, extra);
          }
        }

        *size = SIZE_MPEG2VIDEOINFO(mp2vi);
      }
    }

    FREE_MEM(vih2);
  }

  return mp2vi;
}

void CDemuxerVideoHelper::VideoFormatTypeHandler(const AM_MEDIA_TYPE &mt, BITMAPINFOHEADER **pBMI, REFERENCE_TIME *prtAvgTime, DWORD *pDwAspectX, DWORD *pDwAspectY)
{
  CDemuxerVideoHelper::VideoFormatTypeHandler(mt.pbFormat, &mt.formattype, pBMI, prtAvgTime, pDwAspectX, pDwAspectY);
}

void CDemuxerVideoHelper::VideoFormatTypeHandler(const BYTE *format, const GUID *formattype, BITMAPINFOHEADER **pBMI, REFERENCE_TIME *prtAvgTime, DWORD *pDwAspectX, DWORD *pDwAspectY)
{
  REFERENCE_TIME rtAvg = 0;
  BITMAPINFOHEADER *bmi = NULL;
  DWORD dwAspectX = 0, dwAspectY = 0;

  if (*formattype == FORMAT_VideoInfo)
  {
    VIDEOINFOHEADER *vih = (VIDEOINFOHEADER *)format;
    rtAvg = vih->AvgTimePerFrame;
    bmi = &vih->bmiHeader;
  }
  else if (*formattype == FORMAT_VideoInfo2)
  {
    VIDEOINFOHEADER2 *vih2 = (VIDEOINFOHEADER2 *)format;
    rtAvg = vih2->AvgTimePerFrame;
    bmi = &vih2->bmiHeader;
    dwAspectX = vih2->dwPictAspectRatioX;
    dwAspectY = vih2->dwPictAspectRatioY;
  }
  else if (*formattype == FORMAT_MPEGVideo)
  {
    MPEG1VIDEOINFO *mp1vi = (MPEG1VIDEOINFO *)format;
    rtAvg = mp1vi->hdr.AvgTimePerFrame;
    bmi = &mp1vi->hdr.bmiHeader;
  }
  else if (*formattype == FORMAT_MPEG2Video)
  {
    MPEG2VIDEOINFO *mp2vi = (MPEG2VIDEOINFO *)format;
    rtAvg = mp2vi->hdr.AvgTimePerFrame;
    bmi = &mp2vi->hdr.bmiHeader;
    dwAspectX = mp2vi->hdr.dwPictAspectRatioX;
    dwAspectY = mp2vi->hdr.dwPictAspectRatioY;
  }
  else
  {
    ASSERT(FALSE);
  }

  if (pBMI)
  {
    *pBMI = bmi;
  }

  if (prtAvgTime)
  {
    *prtAvgTime = rtAvg;
  }

  if (pDwAspectX && pDwAspectY)
  {
    *pDwAspectX = dwAspectX;
    *pDwAspectY = dwAspectY;
  }
}
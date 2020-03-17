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

#ifndef __DEMUXER_VIDEO_HELPER_DEFINED
#define __DEMUXER_VIDEO_HELPER_DEFINED

#include "moreuuids.h"

#include <libavformat/isom.h>

#include "dvdmedia.h"

const AVCodecTag mp_bmp_tags[] = {
  { CODEC_ID_AMV,               MKTAG('A', 'M', 'V', 'V')},
  { CODEC_ID_BETHSOFTVID,       MKTAG('B', 'E', 'T', 'H')},
  { CODEC_ID_BFI,               MKTAG('B', 'F', 'I', 'V')},
  { CODEC_ID_C93,               MKTAG('C', '9', '3', 'V')},
  { CODEC_ID_CDGRAPHICS,        MKTAG('C', 'D', 'G', 'R')},
  { CODEC_ID_DNXHD,             MKTAG('A', 'V', 'd', 'n')},
  { CODEC_ID_DSICINVIDEO,       MKTAG('D', 'C', 'I', 'V')},
  { CODEC_ID_DXA,               MKTAG('D', 'X', 'A', '1')},
  { CODEC_ID_FLIC,              MKTAG('F', 'L', 'I', 'C')},
  { CODEC_ID_IDCIN,             MKTAG('I', 'D', 'C', 'I')},
  { CODEC_ID_INTERPLAY_VIDEO,   MKTAG('I', 'N', 'P', 'V')},
  { CODEC_ID_MDEC,              MKTAG('M', 'D', 'E', 'C')},
  { CODEC_ID_MOTIONPIXELS,      MKTAG('M', 'V', 'I', '1')},
  { CODEC_ID_NUV,               MKTAG('N', 'U', 'V', '1')},
  { CODEC_ID_RL2,               MKTAG('R', 'L', '2', 'V')},
  { CODEC_ID_ROQ,               MKTAG('R', 'o', 'Q', 'V')},
  { CODEC_ID_RV10,              MKTAG('R', 'V', '1', '0')},
  { CODEC_ID_RV20,              MKTAG('R', 'V', '2', '0')},
  { CODEC_ID_RV30,              MKTAG('R', 'V', '3', '0')},
  { CODEC_ID_RV40,              MKTAG('R', 'V', '4', '0')},
  { CODEC_ID_TGV,               MKTAG('f', 'V', 'G', 'T')},
  { CODEC_ID_THP,               MKTAG('T', 'H', 'P', 'V')},
  { CODEC_ID_TIERTEXSEQVIDEO,   MKTAG('T', 'S', 'E', 'Q')},
  { CODEC_ID_TXD,               MKTAG('T', 'X', 'D', 'V')},
  { CODEC_ID_VP6A,              MKTAG('V', 'P', '6', 'A')},
  { CODEC_ID_VMDVIDEO,          MKTAG('V', 'M', 'D', 'V')},
  { CODEC_ID_WS_VQA,            MKTAG('V', 'Q', 'A', 'V')},
  { CODEC_ID_XAN_WC3,           MKTAG('W', 'C', '3', 'V')},
  { CODEC_ID_NONE,              0}
};
const struct AVCodecTag * const mp_bmp_taglists[] = { avformat_get_riff_video_tags(), mp_bmp_tags, 0};

class CDemuxerVideoHelper
{
public:
  
  static CMediaType InitVideoType(CodecID codecId, unsigned int &codecTag, const wchar_t *container);
  static VIDEOINFOHEADER *CreateVIH(const AVStream *stream, ULONG *size);
  static VIDEOINFOHEADER2 *CreateVIH2(const AVStream *stream, ULONG *size);
  static VIDEOINFOHEADER2 *CreateVIH2(const AVStream *stream, ULONG *size, const wchar_t *container);
  static MPEG1VIDEOINFO *CreateMPEG1VI(const AVStream *stream, ULONG *size);
  static MPEG2VIDEOINFO *CreateMPEG2VI(const AVStream *stream, ULONG *size);
  static MPEG2VIDEOINFO *CreateMPEG2VI(const AVStream *stream, ULONG *size, const wchar_t *container);
  static MPEG2VIDEOINFO *CreateMPEG2VI(const AVStream *stream, ULONG *size, const wchar_t *container, bool annexB);

  static void VideoFormatTypeHandler(const AM_MEDIA_TYPE &mt, BITMAPINFOHEADER **pBMI, REFERENCE_TIME *prtAvgTime, DWORD *pDwAspectX, DWORD *pDwAspectY);
  static void VideoFormatTypeHandler(const BYTE *format, const GUID *formattype, BITMAPINFOHEADER **pBMI, REFERENCE_TIME *prtAvgTime, DWORD *pDwAspectX, DWORD *pDwAspectY);
};

#endif
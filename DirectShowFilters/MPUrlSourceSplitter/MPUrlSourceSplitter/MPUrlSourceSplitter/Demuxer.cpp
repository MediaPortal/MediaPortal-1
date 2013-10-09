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

#include "Demuxer.h"
#include "DemuxerUtils.h"
#include "DemuxerVideoHelper.h"
#include "FlvPacket.h"

#include "moreuuids.h"

#include <assert.h>
#include <Shlwapi.h>

#ifdef _DEBUG
#define MODULE_NAME                                                         L"Demuxerd"
#else
#define MODULE_NAME                                                         L"Demuxer"
#endif

#define METHOD_SEEK_NAME                                                    L"Seek()"
#define METHOD_SEEK_BY_TIME_NAME                                            L"SeekByTime()"
#define METHOD_SEEK_BY_POSITION_NAME                                        L"SeekByPosition()"
#define METHOD_SEEK_BY_SEQUENCE_READING_NAME                                L"SeekBySequenceReading()"
#define METHOD_GET_NEXT_PACKET_NAME                                         L"GetNextPacket()"

#define FLV_SEEKING_BUFFER_SIZE                                             32 * 1024   // size of buffer to read from stream
#define FLV_PACKET_MINIMUM_CHECKED                                          5           // minimum FLV packets to check in buffer
#define FLV_DO_NOT_SEEK_DIFFERENCE                                          10000       // time in ms when FLV packet dts is closer to seek time
#define FLV_SEEKING_POSITIONS                                               1024        // maximum FLV seeking positions

extern "C" void asf_reset_header2(AVFormatContext *s);

struct FlvSeekPosition
{
  int64_t time;
  int64_t position;
};

#define countof(array) (sizeof(array) / sizeof(array[0]))

#define AVFORMAT_GENPTS                                                     0

static const char *RAW_VIDEO = "rawvideo";
static const char *RAW_VIDEO_DESC = "raw video files";

static const char *RAW_AUDIO = "rawaudio";
static const char *RAW_AUDIO_DESC = "raw audio files";

struct input_format_map
{
  const char *orig_format;
  const char *new_format;
  const char *new_description;
} input_formats [] =
{
  // shorten these formats
  { "matroska,webm",           "matroska", NULL },
  { "mov,mp4,m4a,3gp,3g2,mj2", "mp4",      "MPEG-4/QuickTime format" },

  // raw Video formats (grouped into "rawvideo")
  { "dnxhd", RAW_VIDEO, RAW_VIDEO_DESC },
  { "h261",  RAW_VIDEO, RAW_VIDEO_DESC },
  { "h263",  RAW_VIDEO, RAW_VIDEO_DESC },
  { "h264",  RAW_VIDEO, RAW_VIDEO_DESC },
  { "ingenient", RAW_VIDEO, RAW_VIDEO_DESC },
  { "mjpeg", RAW_VIDEO, RAW_VIDEO_DESC },
  { "vc1",   RAW_VIDEO, RAW_VIDEO_DESC },

  // raw Audio Formats (grouped into "rawaudio")
  { "dirac", RAW_AUDIO, RAW_AUDIO_DESC },
  { "f32be", RAW_AUDIO, RAW_AUDIO_DESC },
  { "f32le", RAW_AUDIO, RAW_AUDIO_DESC },
  { "f64be", RAW_AUDIO, RAW_AUDIO_DESC },
  { "f64le", RAW_AUDIO, RAW_AUDIO_DESC },
  { "g722",  RAW_AUDIO, RAW_AUDIO_DESC },
  { "gsm",   RAW_AUDIO, RAW_AUDIO_DESC },
  { "s16be", RAW_AUDIO, RAW_AUDIO_DESC },
  { "s16le", RAW_AUDIO, RAW_AUDIO_DESC },
  { "s24be", RAW_AUDIO, RAW_AUDIO_DESC },
  { "s24le", RAW_AUDIO, RAW_AUDIO_DESC },
  { "s32be", RAW_AUDIO, RAW_AUDIO_DESC },
  { "s32le", RAW_AUDIO, RAW_AUDIO_DESC },
  { "s8",    RAW_AUDIO, RAW_AUDIO_DESC },
  { "u16be", RAW_AUDIO, RAW_AUDIO_DESC },
  { "u16le", RAW_AUDIO, RAW_AUDIO_DESC },
  { "u24be", RAW_AUDIO, RAW_AUDIO_DESC },
  { "u24le", RAW_AUDIO, RAW_AUDIO_DESC },
  { "u32be", RAW_AUDIO, RAW_AUDIO_DESC },
  { "u32le", RAW_AUDIO, RAW_AUDIO_DESC },
  { "u8",    RAW_AUDIO, RAW_AUDIO_DESC },

  // disabled Formats
  { "applehttp", NULL, NULL },
  { "ass", NULL, NULL },
  { "ffm", NULL, NULL },
  { "ffmetadata", NULL, NULL },
  { "microdvd", NULL, NULL },
  { "mpegtsraw", NULL, NULL },
  { "spdif", NULL, NULL },
  { "srt", NULL, NULL },
  { "tty", NULL, NULL },
  { "vc1test", NULL, NULL },
  { "yuv4mpegpipe", NULL, NULL },
};

void GetInputFormatInfo(AVInputFormat *inputFormat, const char **resName, const char **resDescription)
{
  const char *name = inputFormat->name;
  const char *desc = inputFormat->long_name;

  for (int i=0; i < countof(input_formats); ++i)
  {
    if (strcmp(input_formats[i].orig_format, name) == 0)
    {
      name = input_formats[i].new_format;

      if (input_formats[i].new_description != NULL)
      {
        desc = input_formats[i].new_description;
      }
      break;
    }
  }

  if (resName != NULL)
  {
    *resName = name;
  }
  if (resDescription)
  {
    *resDescription = desc;
  }
}

static int GetAudioCodecPriority(AVCodecContext *codec)
{
  int priority = 0;

  switch(codec->codec_id)
  {
  case CODEC_ID_FLAC:
  case CODEC_ID_TRUEHD:
  case CODEC_ID_MLP:
  case CODEC_ID_TTA:
  case CODEC_ID_MP4ALS:
  // all the PCM codecs
  case CODEC_ID_PCM_S16LE:
  case CODEC_ID_PCM_S16BE:
  case CODEC_ID_PCM_U16LE:
  case CODEC_ID_PCM_U16BE:
  case CODEC_ID_PCM_S32LE:
  case CODEC_ID_PCM_S32BE:
  case CODEC_ID_PCM_U32LE:
  case CODEC_ID_PCM_U32BE:
  case CODEC_ID_PCM_S24LE:
  case CODEC_ID_PCM_S24BE:
  case CODEC_ID_PCM_U24LE:
  case CODEC_ID_PCM_U24BE:
  case CODEC_ID_PCM_F32BE:
  case CODEC_ID_PCM_F32LE:
  case CODEC_ID_PCM_F64BE:
  case CODEC_ID_PCM_F64LE:
  case CODEC_ID_PCM_DVD:
  case CODEC_ID_PCM_BLURAY:
    priority = 10;
    break;
  case CODEC_ID_WAVPACK:
  case CODEC_ID_EAC3:
    priority = 8;
    break;
  case CODEC_ID_DTS:
    priority = 7;
    if (codec->profile >= FF_PROFILE_DTS_HD_HRA)
    {
      priority += 2;
    }
    else if (codec->profile >= FF_PROFILE_DTS_ES)
    {
      priority += 1;
    }
    break;
  case CODEC_ID_AC3:
  case CODEC_ID_AAC:
  case CODEC_ID_AAC_LATM:
    priority = 5;
    break;
  }

  // WAVE_FORMAT_EXTENSIBLE is multi-channel PCM, which doesn't have a proper tag otherwise
  if (codec->codec_tag == WAVE_FORMAT_EXTENSIBLE)
  {
    priority = 10;
  }

  return priority;
}

CDemuxer::CDemuxer(CLogger *logger, IFilter *filter, HRESULT *result)
{
  this->logger = NULL;
  this->inputFormat = NULL;
  this->formatContext = NULL;
  this->flags = FLAG_DEMUXER_NONE;
  this->streamParseType = NULL;
  this->filter = NULL;
  //this->dontChangeTimestamps = false;
  //this->flvTimestamps = NULL;

  for (unsigned int i = 0; i < CDemuxer::Unknown; i++)
  {
    this->streams[i] = NULL;
  }

  if (result != NULL)
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, logger);
    CHECK_POINTER_DEFAULT_HRESULT(*result, filter);

    if (SUCCEEDED(result))
    {
      this->logger = logger;
      this->filter = filter;

      //this->flvTimestamps = ALLOC_MEM_SET(this->flvTimestamps, FlvTimestamp, FLV_TIMESTAMP_MAX, 0);
      //CHECK_POINTER_HRESULT(*result, this->flvTimestamps, *result, E_OUTOFMEMORY);
    }

    if (SUCCEEDED(result))
    {
      for (unsigned int i = 0; i < CDemuxer::Unknown; i++)
      {
        this->streams[i] = new CStreamCollection();
        CHECK_POINTER_HRESULT(*result, this->streams[i], *result, E_OUTOFMEMORY);

        this->activeStream[i] = ACTIVE_STREAM_NOT_SPECIFIED;
      }
    }
  }
}

CDemuxer::~CDemuxer(void)
{
  this->CleanupFormatContext();

  for (unsigned int i = 0; i < CDemuxer::Unknown; i++)
  {
    FREE_MEM_CLASS(this->streams[i]);
  }

  FREE_MEM(this->inputFormat);
  FREE_MEM(this->streamParseType);
  //FREE_MEM(this->flvTimestamps);
}

/* get methods */

CStreamCollection *CDemuxer::GetStreams(StreamType type)
{
  return this->streams[type];
}

int64_t CDemuxer::GetDuration(void)
{
  int64_t duration = this->filter->GetDuration();

  if (duration == DURATION_UNSPECIFIED)
  {
    if ((this->formatContext->duration == (int64_t)AV_NOPTS_VALUE) || (this->formatContext->duration < 0LL))
    {
      // no duration is available for us
      duration = -1;
    }
    else
    {
      duration = this->formatContext->duration;
    }

    duration = ConvertTimestampToRT(duration, 1, AV_TIME_BASE, 0);
  }
  else if (duration != DURATION_LIVE_STREAM)
  {
    duration *= 10000; // DSHOW_TIME_BASE / 1000
  }

  return duration;
}

const wchar_t *CDemuxer::GetContainerFormat(void)
{
  return this->inputFormat;
}

HRESULT CDemuxer::GetNextPacket(COutputPinPacket *packet)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, packet);

  if (SUCCEEDED(result))
  {
    // S_FALSE means no packet
    result = S_FALSE;

    // read FFmpeg packet
    AVPacket ffmpegPacket;

    // assume we are not eof
    CHECK_CONDITION_EXECUTE(this->formatContext->pb != NULL, this->formatContext->pb->eof_reached = 0);

    int ffmpegResult = 0;
    ffmpegResult = av_read_frame(this->formatContext, &ffmpegPacket);

    if ((ffmpegResult == AVERROR(EINTR)) || (ffmpegResult == AVERROR(EAGAIN)))
    {
      // timeout, probably no real error, return empty packet
    }
    else if (ffmpegResult == AVERROR_EOF)
    {
      // end of file reached
      result = HRESULT_FROM_WIN32(ERROR_END_OF_MEDIA);
    }
    else if (ffmpegResult < 0)
    {
      // meh, fail
      result = HRESULT_FROM_WIN32(ERROR_END_OF_MEDIA);
    }
    else if ((ffmpegPacket.size <= 0) || (ffmpegPacket.stream_index < 0) || ((unsigned)ffmpegPacket.stream_index >= this->formatContext->nb_streams))
    {
      // in some cases ffmpeg returns a zero or negative packet size
      av_free_packet(&ffmpegPacket);
    }
    else
    {
      // set result to S_OK, S_FALSE means that no packet is available
      result = S_OK;

      AVStream *stream = this->formatContext->streams[ffmpegPacket.stream_index];

      if ((this->IsMatroska() || this->IsOgg()) && (stream->codec->codec_id == CODEC_ID_H264))
      {
        if ((stream->codec->extradata_size == 0) || (stream->codec->extradata[0] != 1) || (AV_RB32(ffmpegPacket.data) == 0x00000001))
        {
          packet->SetFlags(packet->GetFlags() | OUTPUT_PIN_PACKET_FLAG_PACKET_H264_ANNEXB);
        }
        else
        {
          // no DTS for H264 in native format
          ffmpegPacket.dts = AV_NOPTS_VALUE;
        }
      }

      if (this->IsAvi() && (stream->codec != NULL) && (stream->codec->codec_type == AVMEDIA_TYPE_VIDEO))
      {
        // AVI's always have borked pts, specially if this->formatContext->flags includes
        // AVFMT_FLAG_GENPTS so always use dts
        ffmpegPacket.pts = AV_NOPTS_VALUE;
      }

      if ((stream->codec->codec_id == CODEC_ID_RV10) || (stream->codec->codec_id == CODEC_ID_RV20) || (stream->codec->codec_id == CODEC_ID_RV30) || (stream->codec->codec_id == CODEC_ID_RV40))
      {
        ffmpegPacket.pts = AV_NOPTS_VALUE;
      }

      // never use DTS for these formats
      if (!this->IsAvi() && ((stream->codec->codec_id == CODEC_ID_MPEG2VIDEO) || (stream->codec->codec_id == CODEC_ID_MPEG1VIDEO) || ((stream->codec->codec_id == CODEC_ID_H264) && !this->IsMatroska())))
      {
        ffmpegPacket.dts = AV_NOPTS_VALUE;
      }

      result = packet->CreateBuffer((ffmpegPacket.data != NULL) ? ffmpegPacket.size : 1) ? result : E_OUTOFMEMORY;
      if (SUCCEEDED(result) && (ffmpegPacket.data != NULL))
      {
        packet->GetBuffer()->AddToBuffer(ffmpegPacket.data, ffmpegPacket.size);
      }

      if (SUCCEEDED(result))
      {
        packet->SetStreamPid((unsigned int)ffmpegPacket.stream_index);

        if (this->IsMpegTs() || this->IsMpegPs())
        {
          int64_t start_time = av_rescale_q(this->formatContext->start_time, AV_RATIONAL_TIMEBASE, stream->time_base);
          const int64_t pts_diff = ffmpegPacket.pts - start_time;
          const int64_t dts_diff = ffmpegPacket.dts - start_time;

          if (((ffmpegPacket.pts == AV_NOPTS_VALUE) || (pts_diff < -stream->time_base.den)) && ((ffmpegPacket.dts == AV_NOPTS_VALUE) || (dts_diff < -stream->time_base.den)) && (stream->pts_wrap_bits < 63))
          {
            if (ffmpegPacket.pts != AV_NOPTS_VALUE)
            {
              ffmpegPacket.pts += 1LL << stream->pts_wrap_bits;
            }

            if (ffmpegPacket.dts != AV_NOPTS_VALUE)
            {
              ffmpegPacket.dts += 1LL << stream->pts_wrap_bits;
            }
          }
        }

        REFERENCE_TIME pts = (REFERENCE_TIME)ConvertTimestampToRT(ffmpegPacket.pts, stream->time_base.num, stream->time_base.den, (int64_t)AV_NOPTS_VALUE);
        REFERENCE_TIME dts = (REFERENCE_TIME)ConvertTimestampToRT(ffmpegPacket.dts, stream->time_base.num, stream->time_base.den, (int64_t)AV_NOPTS_VALUE);

        REFERENCE_TIME duration = (REFERENCE_TIME)ConvertTimestampToRT((this->IsMatroska() && (stream->codec->codec_type == AVMEDIA_TYPE_SUBTITLE)) ? ffmpegPacket.convergence_duration : ffmpegPacket.duration, stream->time_base.num, stream->time_base.den, 0);

        REFERENCE_TIME rt = COutputPinPacket::INVALID_TIME;

        // try the different times set, pts first, dts when pts is not valid
        if (pts != COutputPinPacket::INVALID_TIME)
        {
          rt = pts;
        }
        else if (dts != COutputPinPacket::INVALID_TIME)
        {
          rt = dts;
        }

        if (stream->codec->codec_id == CODEC_ID_VC1)
        {
          if (this->IsMatroska() && this->IsVc1Correction())
          {
            rt = pts;

            if (!this->IsVc1SeenTimestamp())
            {
              if ((rt == COutputPinPacket::INVALID_TIME) && (dts != COutputPinPacket::INVALID_TIME))
              {
                rt = dts;
              }

              this->flags &= ~FLAG_DEMUXER_VC1_SEEN_TIMESTAMP;
              this->flags |= (pts != COutputPinPacket::INVALID_TIME) ? FLAG_DEMUXER_VC1_SEEN_TIMESTAMP : FLAG_DEMUXER_NONE;
            }
          }
          else if (this->IsVc1Correction())
          {
            rt = dts;
            packet->SetFlags(packet->GetFlags() | OUTPUT_PIN_PACKET_FLAG_PACKET_PARSED);
          }
        }
        else if (stream->codec->codec_id == CODEC_ID_MOV_TEXT)
        {
          packet->SetFlags(packet->GetFlags() | OUTPUT_PIN_PACKET_FLAG_PACKET_MOV_TEXT);
        }

        // mark the packet as parsed, so the forced subtitle parser doesn't hit it
        if (stream->codec->codec_id == CODEC_ID_HDMV_PGS_SUBTITLE)
        {
          packet->SetFlags(packet->GetFlags() | OUTPUT_PIN_PACKET_FLAG_PACKET_PARSED);
        }

        packet->SetStartTime(rt);
        packet->SetEndTime(rt);

        if (rt != COutputPinPacket::INVALID_TIME)
        {
          REFERENCE_TIME newEndTime = packet->GetEndTime() + (((duration > 0) || (stream->codec->codec_id == CODEC_ID_TRUEHD)) ? duration : 1);
          packet->SetEndTime(newEndTime);
        }

        if (stream->codec->codec_type == AVMEDIA_TYPE_SUBTITLE)
        {
          packet->SetDiscontinuity(true);

          /*if (forcedSubStream)
          {
          packet->SetFlags(packet->GetFlags() | FLAG_PACKET_FORCED_SUBTITLE);
          packet->SetFlags(packet->GetFlags() & (~FLAG_PACKET_PARSED));
          }*/
        }

        packet->SetSyncPoint((ffmpegPacket.flags & AV_PKT_FLAG_KEY) != 0);
        //pPacket->bAppendable = 0; //!pPacket->bSyncPoint;
        packet->SetDiscontinuity((ffmpegPacket.flags & AV_PKT_FLAG_CORRUPT) != 0);
        //#ifdef DEBUG
        //        if (pkt.flags & AV_PKT_FLAG_CORRUPT)
        //          DbgLog((LOG_TRACE, 10, L"::GetNextPacket() - Signaling Discontinuinty because of corrupt package"));
        //#endif

        if (packet->GetStartTime() != AV_NOPTS_VALUE)
        {
          //m_rtCurrent = packet->GetStartTime();
        }
      }

      av_free_packet(&ffmpegPacket);

      //if ((this->m_bFlv) && (this->flvTimestamps != NULL) && (pPacket->StreamId < FLV_TIMESTAMP_MAX))
      //{
      //  // in case of FLV video check timestamps, can be wrong in case of live streams
      //  FlvTimestamp *timestamp = &this->flvTimestamps[pPacket->StreamId];

      //  int64_t lastStart = pPacket->rtStart;
      //  int64_t lastStop = pPacket->rtStop;

      //  if ((!this->dontChangeTimestamps) && ((pPacket->rtStart - timestamp->decreaseTimestamp - timestamp->lastPacketStart) > DSHOW_TIME_BASE))
      //  {
      //    // if difference between two packets is greater than one second
      //    // it should happen only on start of stream

      //    this->m_pFilter->GetLogger()->Log(LOGGER_VERBOSE, L"%s: %s: id: %d, packet start: %lld, packet stop: %lld, last start: %lld, last stop: %lld, decrease: %lld", MODULE_NAME, METHOD_GET_NEXT_PACKET_NAME, pPacket->StreamId, pPacket->rtStart, pPacket->rtStop, timestamp->lastPacketStart, timestamp->lastPacketStop, timestamp->decreaseTimestamp);

      //    pPacket->rtStop = (timestamp->lastPacketStop / 10000 + 1) * 10000 + pPacket->rtStop - pPacket->rtStart;
      //    pPacket->rtStart = (timestamp->lastPacketStop / 10000 + 1) * 10000;

      //    timestamp->decreaseTimestamp = lastStart - pPacket->rtStart;
      //    for (int i = 0; i < FLV_TIMESTAMP_MAX; i++)
      //    {
      //      FlvTimestamp *tms = &this->flvTimestamps[i];
      //      tms->decreaseTimestamp = timestamp->decreaseTimestamp;
      //      tms->needRecalculate = true;
      //    }
      //    timestamp->needRecalculate = false;
      //    this->dontChangeTimestamps = true;

      //    this->m_pFilter->GetLogger()->Log(LOGGER_VERBOSE, L"%s: %s: id: %d, packet start: %lld, packet stop: %lld, last start: %lld, last stop: %lld, decrease: %lld", MODULE_NAME, METHOD_GET_NEXT_PACKET_NAME, pPacket->StreamId, pPacket->rtStart, pPacket->rtStop, timestamp->lastPacketStart, timestamp->lastPacketStop, timestamp->decreaseTimestamp);
      //  }
      //  else
      //  {
      //    if (timestamp->needRecalculate)
      //    {
      //      timestamp->decreaseTimestamp -= timestamp->lastPacketStart;
      //      timestamp->needRecalculate = false;
      //    }

      //    pPacket->rtStop -= timestamp->decreaseTimestamp;
      //    pPacket->rtStart -= timestamp->decreaseTimestamp;
      //  }

      //  timestamp->lastPacketStart = lastStart;
      //  timestamp->lastPacketStop = lastStop;
      //}
    }
  }

  return result;
}

/* set methods */

void CDemuxer::SetActiveStream(StreamType streamType, int activeStreamId)
{
  CStreamCollection *streams = this->streams[(CDemuxer::StreamType)streamType];

  for (unsigned int i = 0; i < streams->Count(); i++)
  {
    CStream *stream = streams->GetItem(i);

    if (stream->GetPid() == activeStreamId)
    {
      this->activeStream[streamType] = i;
      break;
    }
  }
}

/* other methods */

HRESULT CDemuxer::OpenStream(AVIOContext *demuxerContext, const wchar_t *streamUrl)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, demuxerContext);
  CHECK_POINTER_DEFAULT_HRESULT(result, streamUrl);

  if (SUCCEEDED(result))
  {
    int ret; // return code from avformat functions

    // format context needs streamUrl in char *
    char *streamUrlA = ConvertToMultiByteW(streamUrl);
    CHECK_POINTER_HRESULT(result, streamUrlA, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      // create the format context
      this->formatContext = avformat_alloc_context();
      this->formatContext->pb = demuxerContext;

      ret = avformat_open_input(&this->formatContext, streamUrlA, NULL, NULL);
      CHECK_CONDITION_EXECUTE(ret < 0, result = ret);

      if (SUCCEEDED(result))
      {
        ret = this->InitFormatContext(streamUrl);
        CHECK_CONDITION_EXECUTE(ret < 0, result = ret);
      }
    }

    FREE_MEM(streamUrlA);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), CleanupFormatContext());

  return result;
}

bool CDemuxer::IsSetFlags(unsigned int flags)
{
  return ((this->flags & flags) == flags);
}

bool CDemuxer::IsFlv(void)
{
  return this->IsSetFlags(FLAG_DEMUXER_FLV);
}

bool CDemuxer::IsAsf(void)
{
  return this->IsSetFlags(FLAG_DEMUXER_ASF);
}

bool CDemuxer::IsMp4(void)
{
  return this->IsSetFlags(FLAG_DEMUXER_MP4);
}

bool CDemuxer::IsMatroska(void)
{
  return this->IsSetFlags(FLAG_DEMUXER_MATROSKA);
}

bool CDemuxer::IsOgg(void)
{
  return this->IsSetFlags(FLAG_DEMUXER_OGG);
}

bool CDemuxer::IsAvi(void)
{
  return this->IsSetFlags(FLAG_DEMUXER_AVI);
}

bool CDemuxer::IsMpegTs(void)
{
  return this->IsSetFlags(FLAG_DEMUXER_MPEG_TS);
}

bool CDemuxer::IsMpegPs(void)
{
  return this->IsSetFlags(FLAG_DEMUXER_MPEG_PS);
}

bool CDemuxer::IsEvo(void)
{
  return this->IsSetFlags(FLAG_DEMUXER_EVO);
}

bool CDemuxer::IsRm(void)
{
  return this->IsSetFlags(FLAG_DEMUXER_RM);
}

bool CDemuxer::IsVc1SeenTimestamp(void)
{
  return this->IsSetFlags(FLAG_DEMUXER_VC1_SEEN_TIMESTAMP);
}

bool CDemuxer::IsVc1Correction(void)
{
  return this->IsSetFlags(FLAG_DEMUXER_VC1_CORRECTION);
}

CStream *CDemuxer::SelectVideoStream(void)
{
  CStream *result = NULL;

  CStreamCollection *videoStreams = this->GetStreams(CDemuxer::Video);

  for (unsigned int i = 0; i < videoStreams->Count(); i++)
  {
    CStream *stream = videoStreams->GetItem(i);

    if (result == NULL)
    {
      result = stream;
      continue;
    }

    uint64_t bestPixels = this->formatContext->streams[result->GetPid()]->codec->width * this->formatContext->streams[result->GetPid()]->codec->height;
    uint64_t checkPixels = this->formatContext->streams[stream->GetPid()]->codec->width * this->formatContext->streams[stream->GetPid()]->codec->height;

    if ((this->formatContext->streams[result->GetPid()]->codec->codec_id == CODEC_ID_NONE) && (this->formatContext->streams[stream->GetPid()]->codec->codec_id != CODEC_ID_NONE))
    {
      result = stream;
      continue;
    }

    int check_nb_f = this->formatContext->streams[stream->GetPid()]->codec_info_nb_frames;
    int best_nb_f  = this->formatContext->streams[result->GetPid()]->codec_info_nb_frames;

    if (this->IsRm() && (check_nb_f > 0) && (best_nb_f <= 0))
    {
      result = stream;
    }
    else if ((!this->IsRm()) || (check_nb_f > 0))
    {
      if (checkPixels > bestPixels)
      {
        result = stream;
      }
      else if (checkPixels == bestPixels)
      {
        int bestRate = this->formatContext->streams[result->GetPid()]->codec->bit_rate;
        int checkRate = this->formatContext->streams[stream->GetPid()]->codec->bit_rate;

        if ((bestRate != 0) && (checkRate != 0) && (checkRate > bestRate))
        {
          result = stream;
        }
      }
    }
  }

  return result;
}

CStream *CDemuxer::SelectAudioStream(void)
{
  CStream *result = NULL;
  CStreamCollection *audioStreams = this->GetStreams(CDemuxer::Audio);

  // check for a stream with a default flag
  // if in our current set is one, that one prevails
  for (unsigned int i = 0; i < audioStreams->Count(); i++)
  {
    CStream *stream = audioStreams->GetItem(i);

    if (this->formatContext->streams[stream->GetPid()]->disposition & AV_DISPOSITION_DEFAULT)
    {
      result = stream;
      break;
    }
  }

#define DISPO_IMPAIRED (AV_DISPOSITION_HEARING_IMPAIRED | AV_DISPOSITION_VISUAL_IMPAIRED)

  if ((result == NULL) && (audioStreams->Count() != 0))
  {
    // if only one stream is left, just use that one
    if (audioStreams->Count() == 1)
    {
      result = audioStreams->GetItem(0);
    }
    else
    {
      // check for quality
      for (unsigned int i = 0; i < audioStreams->Count(); i++)
      {
        CStream *stream = audioStreams->GetItem(i);

        if (result == NULL)
        { 
          result = stream;
          continue;
        }

        AVStream *oldStream = this->formatContext->streams[result->GetPid()];
        AVStream *newStream = this->formatContext->streams[stream->GetPid()];

        int check_nb_f = newStream->codec_info_nb_frames;
        int best_nb_f  = oldStream->codec_info_nb_frames;

        if (this->IsRm() && ((check_nb_f > 0) && (best_nb_f <= 0)))
        {
          result = stream;
        }
        else if ((!this->IsRm()) || (check_nb_f > 0))
        {
           if (((oldStream->disposition & DISPO_IMPAIRED) == 0) != ((newStream->disposition & DISPO_IMPAIRED) == 0))
           {
            if ((newStream->disposition & DISPO_IMPAIRED) == 0)
            {
              result = stream;
            }
            continue;
          }

          // first, check number of channels
          int oldChannelCount = oldStream->codec->channels;
          int newChannelCount = newStream->codec->channels;

          if (newChannelCount > oldChannelCount)
          {
            result = stream;
          }
          else if (newChannelCount == oldChannelCount)
          {
            // same number of channels, check codec
            int oldPriority = GetAudioCodecPriority(oldStream->codec);
            int newPriority = GetAudioCodecPriority(newStream->codec);

            if (newPriority > oldPriority)
            {
              result = stream;
            }
            else if (newPriority == oldPriority)
            {
              int bestRate = oldStream->codec->bit_rate;
              int checkRate = newStream->codec->bit_rate;

              if ((bestRate != 0) && (checkRate != 0) && (checkRate > bestRate))
              {
                result = stream;
              }
            }
          }
        }
      }
    }
  }

  return result;
}

HRESULT CDemuxer::Seek(REFERENCE_TIME time)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_SEEK_NAME);
  this->logger->Log(LOGGER_INFO, L"%s: %s: seeking to time: %lld", MODULE_NAME, METHOD_SEEK_NAME, time);

  // get seeking capabilities from filter
  unsigned int seekingCapabilities = this->filter->GetSeekingCapabilities();
  bool seeked = false;
  // we prefer seeking by position, it's simplier and buffer is also based on position

  if (seekingCapabilities & SEEKING_METHOD_POSITION)
  {
    int flags = AVSEEK_FLAG_BACKWARD;
    HRESULT result = this->SeekByPosition(time, flags);

    if (FAILED(result))
    {
      this->logger->Log(LOGGER_WARNING, L"%s: %s: first seek by position failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, result);

      result = this->SeekByPosition(time, AVSEEK_FLAG_ANY);
      if (FAILED(result))
      {
        this->logger->Log(LOGGER_WARNING, L"%s: %s: second seek by position failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, result);
      }
    }

    if (SUCCEEDED(result))
    {
      seeked = true;
    }
  }

  if ((!seeked) && (seekingCapabilities & SEEKING_METHOD_TIME))
  {
    int flags = AVSEEK_FLAG_BACKWARD;
    HRESULT result = this->SeekByTime(time, flags);

    if (FAILED(result))
    {
      this->logger->Log(LOGGER_WARNING, L"%s: %s: first seek by time failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, result);

      flags = 0;
      result = this->SeekByTime(time, flags);    // seek forward
      if (FAILED(result))
      {
        this->logger->Log(LOGGER_WARNING, L"%s: %s: second seek by time failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, result);
      }
    }

    if (SUCCEEDED(result))
    {
      seeked = true;
    }
  }

  if ((!seeked) && (seekingCapabilities == SEEKING_METHOD_NONE))
  {
    // it should not happen
    // seeking backward is simple => just moving backward in buffer
    // seeking forward is waiting for right timestamp by sequence reading
    int flags = AVSEEK_FLAG_BACKWARD;
    HRESULT result = this->SeekBySequenceReading(time, flags);

    if (FAILED(result))
    {
      this->logger->Log(LOGGER_WARNING, L"%s: %s: first seek by sequence reading failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, result);

      result = this->SeekBySequenceReading(time, flags | AVSEEK_FLAG_ANY);
      if (FAILED(result))
      {
        this->logger->Log(LOGGER_WARNING, L"%s: %s: second seek by sequence reading failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, result);
      }
    }

    if (SUCCEEDED(result))
    {
      seeked = true;
    }
  }

  if (!seeked)
  {
    // we didn't seek by position or time
    this->logger->Log(LOGGER_WARNING, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_NAME, L"didn't seek by position or time");
  }

  //if (seeked)
  //{
  //  // set that we don't change timestamps
  //  // in another case if we recalculate timestamps the video freeze
  //  this->dontChangeTimestamps = true;
  //}

  for (unsigned i = 0; i < this->formatContext->nb_streams; i++)
  {
    CDemuxer::InitParser(this->formatContext, this->formatContext->streams[i]);
    this->UpdateParserFlags(this->formatContext->streams[i]);
  }

  this->flags &= ~FLAG_DEMUXER_VC1_SEEN_TIMESTAMP;
  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_SEEK_NAME);

  return (seeked) ? S_OK : E_FAIL;
}

/* protected methods */

HRESULT CDemuxer::InitFormatContext(const wchar_t *streamUrl)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamUrl);

  if (SUCCEEDED(result))
  {
    const char *format = NULL;
    GetInputFormatInfo(this->formatContext->iformat, &format, NULL);

    result = ((format == NULL) && (this->formatContext->iformat == NULL)) ? E_FAIL : result;

    if (SUCCEEDED(result))
    {
      this->inputFormat = ConvertToUnicodeA((format != NULL) ? format : this->formatContext->iformat->name);
      CHECK_POINTER_HRESULT(result, this->inputFormat, result, E_OUTOFMEMORY);
    }

    if (SUCCEEDED(result))
    {
      this->flags &= ~FLAG_DEMUXER_VC1_SEEN_TIMESTAMP;

      wchar_t *extension = (streamUrl != NULL) ? PathFindExtensionW(streamUrl) : NULL;

      this->flags |= (_wcsnicmp(this->inputFormat, L"flv", 3) == 0) ? FLAG_DEMUXER_FLV : FLAG_DEMUXER_NONE;
      this->flags |= (_wcsnicmp(this->inputFormat, L"asf", 3) == 0) ? FLAG_DEMUXER_ASF : FLAG_DEMUXER_NONE;
      this->flags |= (_wcsnicmp(this->inputFormat, L"mp4", 3) == 0) ? FLAG_DEMUXER_MP4 : FLAG_DEMUXER_NONE;
      this->flags |= (_wcsnicmp(this->inputFormat, L"matroska", 8) == 0) ? FLAG_DEMUXER_MATROSKA : FLAG_DEMUXER_NONE;
      this->flags |= (_wcsnicmp(this->inputFormat, L"ogg", 3) == 0) ? FLAG_DEMUXER_OGG : FLAG_DEMUXER_NONE;
      this->flags |= (_wcsnicmp(this->inputFormat, L"avi", 3) == 0) ? FLAG_DEMUXER_AVI : FLAG_DEMUXER_NONE;
      this->flags |= (_wcsnicmp(this->inputFormat, L"mpegts", 6) == 0) ? FLAG_DEMUXER_MPEG_TS : FLAG_DEMUXER_NONE;
      this->flags |= (_wcsicmp(this->inputFormat, L"mpeg") == 0) ? FLAG_DEMUXER_MPEG_PS : FLAG_DEMUXER_NONE;
      this->flags |= (((extension != NULL) ? (_wcsicmp(extension, L".evo") == 0) : true) && (_wcsicmp(this->inputFormat, L"mpeg") == 0)) ? FLAG_DEMUXER_EVO : FLAG_DEMUXER_NONE;
      this->flags |= (_wcsicmp(this->inputFormat, L"rm") == 0) ? FLAG_DEMUXER_RM : FLAG_DEMUXER_NONE;

      if (AVFORMAT_GENPTS)
      {
        this->formatContext->flags |= AVFMT_FLAG_GENPTS;
      }

      this->formatContext->flags |= AVFMT_FLAG_IGNPARSERSYNC;

      /*if (this->IsMpegTs())
      {
        this->formatContext->max_analyze_duration = (this->formatContext->max_analyze_duration * 3) / 2;
      }*/

      int ret = avformat_find_stream_info(this->formatContext, NULL);
      CHECK_CONDITION_EXECUTE(ret < 0, result = ret);
      
      FREE_MEM(this->streamParseType);
      this->streamParseType = ALLOC_MEM_SET(this->streamParseType, enum AVStreamParseType, this->formatContext->nb_streams, 0);
      CHECK_POINTER_HRESULT(result, this->streamParseType, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        for (unsigned int i = 0; i < this->formatContext->nb_streams; i++)
        {
          AVStream *st = this->formatContext->streams[i];

          // disable full stream parsing for these formats
          if (st->need_parsing == AVSTREAM_PARSE_FULL)
          {
            if (st->codec->codec_id == CODEC_ID_DVB_SUBTITLE)
            {
              st->need_parsing = AVSTREAM_PARSE_NONE;
            }
          }

          if (this->IsOgg() && (st->codec->codec_id == CODEC_ID_H264))
          {
            st->need_parsing = AVSTREAM_PARSE_FULL;
          }

          // create the parsers with the appropriate flags
          CDemuxer::InitParser(this->formatContext, st);
          this->UpdateParserFlags(st);

          this->streamParseType[i] = st->need_parsing;

          if (((st->codec->codec_id == CODEC_ID_DTS) && (st->codec->codec_tag == 0xA2)) ||
              ((st->codec->codec_id == CODEC_ID_EAC3) && (st->codec->codec_tag == 0xA1)))
          {
            st->disposition |= AV_DISPOSITION_SECONDARY_AUDIO;
          }

          // update substreams
          for (unsigned int j = 0; j < this->formatContext->nb_streams; j++)
          {
            AVStream *tempStream = this->formatContext->streams[j];

            // find and flag the AC-3 substream
            if (this->IsMpegTs() && (tempStream->codec->codec_id == CODEC_ID_TRUEHD))
            {
              int id = tempStream->id;
              AVStream *subStream = NULL;

              for (unsigned int k = 0; k < this->formatContext->nb_streams; k++)
              {
                AVStream *sst = this->formatContext->streams[k];

                if ((j != k) && (sst->id == id))
                {
                  subStream = sst;
                  break;
                }
              }

              if (subStream != NULL)
              {
                subStream->disposition = tempStream->disposition | AV_DISPOSITION_SUB_STREAM;
                av_dict_copy(&subStream->metadata, tempStream->metadata, 0);
              }
            }
          }

          /*if ((st->codec->codec_type == AVMEDIA_TYPE_ATTACHMENT) && (st->codec->codec_id == CODEC_ID_TTF))
          {
            if (!m_pFontInstaller)
            {
              m_pFontInstaller = new CFontInstaller();
            }

            m_pFontInstaller->InstallFont(st->codec->extradata, st->codec->extradata_size);
          }*/
        }
      }

      if (SUCCEEDED(result))
      {
        // create streams

        // try to use non-blocking methods
        this->formatContext->flags |= AVFMT_FLAG_NONBLOCK;

        for (int i = 0; i < countof(this->streams); i++)
        {
          this->streams[i]->Clear();
        }

        unsigned int program = UINT_MAX;

        if (this->formatContext->nb_programs != 0)
        {
          // use a scoring system to select the best available program
          // a "good" program at least has a valid video and audio stream
          // we'll try here to detect these streams and decide on the best program
          // every present stream gets one point, if it appears to be valid, it gets 4
          // valid video streams have a width and height, valid audio streams have a channel count
          // if one program was found with both streams valid, we'll stop looking

          DWORD score = 0; // stream found: 1, stream valid: 4

          for (unsigned int i = 0; i < this->formatContext->nb_programs; i++)
          {
            if (this->formatContext->programs[i]->nb_stream_indexes > 0)
            {
              DWORD videoScore = 0;
              DWORD audioScore = 0;

              for (unsigned k = 0; k < this->formatContext->programs[i]->nb_stream_indexes; k++)
              {
                unsigned streamIndex = this->formatContext->programs[i]->stream_index[k];
                AVStream *stream = this->formatContext->streams[streamIndex];

                if ((stream->codec->codec_type == AVMEDIA_TYPE_VIDEO) && (videoScore < 4))
                {
                  videoScore = ((stream->codec->width != 0) && (stream->codec->height != 0)) ? 4 : 1;
                }
                else if ((stream->codec->codec_type == AVMEDIA_TYPE_AUDIO) && (audioScore < 4))
                {
                  audioScore = (stream->codec->channels != 0) ? 4 : 1;
                }
              }

              // check the score of the previously found stream
              // in addition, we always require a valid video stream (or none), a invalid one is not allowed

              if (videoScore != 1 && (videoScore + audioScore) > score)
              {
                score = videoScore + audioScore;
                program = i;

                if (score == 8)
                {
                  break;
                }
              }
            }
          }
        }

        // stream has programs
        bool isProgram = (program < this->formatContext->nb_programs);

        // discard unwanted programs
        if (isProgram)
        {
          for (unsigned int i = 0; i < this->formatContext->nb_programs; i++)
          {
            CHECK_CONDITION_EXECUTE(i != program, this->formatContext->programs[i]->discard = AVDISCARD_ALL);
          }
        }

        // re-compute the overall stream duration based on video and audio durations
        int64_t duration = INT64_MIN;
        int64_t streamDuration = 0;
        int64_t startTime = INT64_MAX;
        int64_t streamStartTime = 0;

        // number of streams (either in stream or in program)
        unsigned int streamCount = isProgram ? this->formatContext->programs[program]->nb_stream_indexes : this->formatContext->nb_streams;

        // stream has PGS streams
        bool hasPGS = false;

        // add streams from selected program, or all streams if no program was selected
        for (unsigned int i = 0; i < streamCount; i++)
        {
          int streamIndex = isProgram ? this->formatContext->programs[program]->stream_index[i] : i;

          AVStream *stream = this->formatContext->streams[streamIndex];

          // if known stream type, add stream

          if ((stream->codec->codec_type == AVMEDIA_TYPE_VIDEO) ||
              (stream->codec->codec_type == AVMEDIA_TYPE_AUDIO) ||
              (stream->codec->codec_type == AVMEDIA_TYPE_SUBTITLE))
          {
            if ((stream->codec->codec_type == AVMEDIA_TYPE_UNKNOWN) ||
              (stream->discard == AVDISCARD_ALL) ||
              ((stream->codec->codec_id == CODEC_ID_NONE) && (stream->codec->codec_tag == 0)) ||
              (stream->disposition & AV_DISPOSITION_ATTACHED_PIC))
            {
              stream->discard = AVDISCARD_ALL;
              result = S_FALSE;
            }

            if (result == S_OK)
            {
              CStream *newStream = new CStream();
              CHECK_POINTER_HRESULT(result, newStream, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                newStream->SetPid(streamIndex);

                // Extract language
                const char *lang = NULL;
                if (av_dict_get(stream->metadata, "language", NULL, 0) != NULL)
                {
                  lang = av_dict_get(stream->metadata, "language", NULL, 0)->value;
                }

                wchar_t *language = ConvertToUnicodeA(lang);
                result = (newStream->SetLanguage((language != NULL) ? CDemuxerUtils::ProbeForISO6392(language) : L"und")) ? result : E_OUTOFMEMORY;
                FREE_MEM(language);

                CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = newStream->CreateStreamInfo(this->formatContext, stream, this->inputFormat));
                CHECK_CONDITION_EXECUTE(FAILED(result), stream->discard = AVDISCARD_ALL);
              }

              if (SUCCEEDED(result))
              {
                switch(stream->codec->codec_type)
                {
                case AVMEDIA_TYPE_VIDEO:
                  result = (this->streams[CDemuxer::Video]->Add(newStream)) ? result : E_OUTOFMEMORY;
                  break;
                case AVMEDIA_TYPE_AUDIO:
                  result = (this->streams[CDemuxer::Audio]->Add(newStream)) ? result : E_OUTOFMEMORY;
                  break;
                case AVMEDIA_TYPE_SUBTITLE:
                  result = (this->streams[CDemuxer::Subpic]->Add(newStream)) ? result : E_OUTOFMEMORY;
                  break;
                default:
                  // unsupported stream
                  // normally this should be caught while creating the stream info already.
                  result = E_FAIL;
                  break;
                }
              }

              CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(newStream));
            }

            if (result != S_OK)
            {
              result = S_OK;
              continue;
            }

            if ((stream->codec->codec_type == AVMEDIA_TYPE_VIDEO) || (stream->codec->codec_type == AVMEDIA_TYPE_AUDIO))
            {
              if (stream->duration != AV_NOPTS_VALUE)
              {
                streamDuration = av_rescale_q(stream->duration, stream->time_base, AV_RATIONAL_TIMEBASE);

                CHECK_CONDITION_EXECUTE(streamDuration > duration, duration = streamDuration);
              }

              if (stream->start_time != AV_NOPTS_VALUE)
              {
                streamStartTime = av_rescale_q(stream->start_time, stream->time_base, AV_RATIONAL_TIMEBASE);

                if ((startTime != INT64_MAX) && (this->IsMpegTs() || this->IsMpegPs()) && (stream->pts_wrap_bits < 60))
                {
                  int64_t start = av_rescale_q(startTime, AV_RATIONAL_TIMEBASE, stream->time_base);

                  if ((start < (3LL << (stream->pts_wrap_bits - 3))) && (stream->start_time > (3LL << (stream->pts_wrap_bits - 2))))
                  {
                    startTime = av_rescale_q(start + (1LL << stream->pts_wrap_bits), stream->time_base, AV_RATIONAL_TIMEBASE);

                  }
                  else if ((stream->start_time < (3LL << (stream->pts_wrap_bits - 3))) && (start > (3LL << (stream->pts_wrap_bits - 2))))
                  {
                    streamStartTime = av_rescale_q(stream->start_time + (1LL << stream->pts_wrap_bits), stream->time_base, AV_RATIONAL_TIMEBASE);
                  }
                }

                CHECK_CONDITION_EXECUTE(streamStartTime < startTime, startTime = streamStartTime);
              }
            }

            hasPGS = (stream->codec->codec_id == CODEC_ID_HDMV_PGS_SUBTITLE) ? true : hasPGS;
          }
        }

        if (duration != INT64_MIN)
        {
          this->formatContext->duration = duration;
        }

        if (startTime != INT64_MAX)
        {
          this->formatContext->start_time = startTime;
        }

        if (hasPGS)
        {
          CStream *stream = new CStream();
          CHECK_POINTER_HRESULT(result, stream, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            stream->SetPid(FORCED_SUBTITLE_PID);
            result = stream->CreateStreamInfo();
          }

          CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (stream->SetLanguage(L"und")) ? result : E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            // create media type
            CMediaType *mediaType = new CMediaType();
            CHECK_POINTER_HRESULT(result, mediaType, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              mediaType->majortype = MEDIATYPE_Subtitle;
              mediaType->subtype = MEDIASUBTYPE_HDMVSUB;
              mediaType->formattype = FORMAT_SubtitleInfo;

              SUBTITLEINFO *subInfo = (SUBTITLEINFO *)mediaType->AllocFormatBuffer(sizeof(SUBTITLEINFO));
              CHECK_POINTER_HRESULT(result, subInfo, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                memset(subInfo, 0, mediaType->FormatLength());
                wcscpy_s(subInfo->TrackName, FORCED_SUB_STRING);
                subInfo->dwOffset = sizeof(SUBTITLEINFO);

                result = (stream->GetStreamInfo()->GetMediaTypes()->Add(mediaType)) ? result : E_OUTOFMEMORY;
              }

              // there is no need to free subInfo
              // in case of error it is freed with ~CMediaType()
            }

            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(mediaType));
          }

          CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (this->streams[CDemuxer::Subpic]->Add(stream)) ? result : E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(stream));
        }
      }
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), this->CleanupFormatContext());
  return result;
}

void CDemuxer::CleanupFormatContext(void)
{
  if (this->formatContext)
  {
    av_close_input_file(this->formatContext);
    this->formatContext = NULL;
  }
  FREE_MEM(this->streamParseType);
}

int CDemuxer::InitParser(AVFormatContext *formatContext, AVStream *stream)
{
  if ((stream->parser == NULL) && (stream->need_parsing) && ((formatContext->flags & AVFMT_FLAG_NOPARSE) == 0))
  {
    stream->parser = av_parser_init(stream->codec->codec_id);

    if (stream->parser != NULL)
    {
      if (stream->need_parsing == AVSTREAM_PARSE_HEADERS)
      {
        stream->parser->flags |= PARSER_FLAG_COMPLETE_FRAMES;
      }
      else if (stream->need_parsing == AVSTREAM_PARSE_FULL_ONCE)
      {
        stream->parser->flags |= PARSER_FLAG_ONCE;
      }
    }
    else
    {
      return -1;
    }
  }

  return 0;
}

void CDemuxer::UpdateParserFlags(AVStream *stream)
{
  if (stream->parser != NULL)
  {
    if (((stream->codec->codec_id == CODEC_ID_MPEG2VIDEO) || (stream->codec->codec_id == CODEC_ID_MPEG1VIDEO)) &&
        (_wcsicmp(this->inputFormat, L"mpegvideo") != 0))
    {
      stream->parser->flags |= PARSER_FLAG_NO_TIMESTAMP_MANGLING;
    }
    else if (stream->codec->codec_id == CODEC_ID_VC1)
    {
      if (this->IsVc1Correction())
      {
        stream->parser->flags &= ~PARSER_FLAG_NO_TIMESTAMP_MANGLING;
      }
      else
      {
        stream->parser->flags |= PARSER_FLAG_NO_TIMESTAMP_MANGLING;
      }
    }
  }
}

REFERENCE_TIME CDemuxer::ConvertTimestampToRT(int64_t pts, int num, int den, int64_t starttime)
{
  if (pts == (int64_t)AV_NOPTS_VALUE)
  {
    return COutputPinPacket::INVALID_TIME;
  }

  if (starttime == AV_NOPTS_VALUE)
  {
    starttime = (this->formatContext->start_time != AV_NOPTS_VALUE) ? av_rescale(this->formatContext->start_time, den, (int64_t)AV_TIME_BASE * num) : 0;
  }

  if (starttime != 0)
  {
    pts -= starttime;
  }

  // let av_rescale do the work, it's smart enough to not overflow
  return av_rescale(pts, (int64_t)num * DSHOW_TIME_BASE, den);
}

int64_t CDemuxer::ConvertRTToTimestamp(REFERENCE_TIME timestamp, int num, int den, int64_t starttime)
{
  if (timestamp == COutputPinPacket::INVALID_TIME)
  {
    return (int64_t)AV_NOPTS_VALUE;
  }

  if(starttime == AV_NOPTS_VALUE)
  {
    starttime = (this->formatContext->start_time != AV_NOPTS_VALUE) ? av_rescale(this->formatContext->start_time, den, (int64_t)AV_TIME_BASE * num) : 0;
  }

  int64_t pts = av_rescale(timestamp, den, (int64_t)num * DSHOW_TIME_BASE);
  if(starttime != 0)
  {
    pts += starttime;
  }

  return pts;
}

HRESULT CDemuxer::SeekByTime(REFERENCE_TIME time, int flags)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_SEEK_BY_TIME_NAME);

  HRESULT result = S_OK;

  AVStream *st = NULL;
  AVIndexEntry *ie = NULL;  
  int64_t seek_pts = time;
  int videoStreamId = (this->activeStream[CDemuxer::Video] == ACTIVE_STREAM_NOT_SPECIFIED) ? -1 : this->streams[CDemuxer::Video]->GetItem(this->activeStream[CDemuxer::Video])->GetPid();

  // if we have a video stream, seek on that one
  // if we don't, well, then don't
  if (time >= 0)
  {
    if (videoStreamId != -1)
    {
      AVStream *stream = this->formatContext->streams[videoStreamId];
      seek_pts = ConvertRTToTimestamp(time, stream->time_base.num, stream->time_base.den, (int64_t)AV_NOPTS_VALUE);
    }
    else
    {
      seek_pts = ConvertRTToTimestamp(time, 1, AV_TIME_BASE, (int64_t)AV_NOPTS_VALUE);
    }
  }

  if (videoStreamId < 0)
  {
    this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, L"videoStreamId < 0");
    videoStreamId = av_find_default_stream_index(this->formatContext);
    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: videoStreamId: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, videoStreamId);
    if (videoStreamId < 0)
    {
      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, L"not found video stream ID");
      result = -1;
    }

    if (result == S_OK)
    {
      st = this->formatContext->streams[videoStreamId];
      /* timestamp for default must be expressed in AV_TIME_BASE units */
      seek_pts = av_rescale(time, st->time_base.den, AV_TIME_BASE * (int64_t)st->time_base.num);
    }
  }

  if (SUCCEEDED(result))
  {
    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: time: %lld, seek_pts: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, time, seek_pts);

    st = this->formatContext->streams[videoStreamId];

    // check is requested time is not in buffer

    bool found = false;
    int index = -1;

    ff_read_frame_flush(this->formatContext);
    index = av_index_search_timestamp(st, seek_pts, flags);

    if (!found) 
    {
      st->nb_index_entries = 0;
      st->nb_frames = 0;

      // seek to time
      int64_t seekedTime = this->filter->SeekToTime(time / 10000); // (1000 / DSHOW_TIME_BASE)
      if ((seekedTime < 0) || (seekedTime > (time / 10000)))
      {
        this->logger->Log(LOGGER_ERROR, L"%s: %s: invalid seek time returned: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, seekedTime);
        result = -2;
      }

      if (SUCCEEDED(result) && (this->IsAsf()))
      {
        found = true;
        asf_reset_header2(this->formatContext);
      }

      if (SUCCEEDED(result) && (this->IsMp4()))
      {
        if (this->formatContext->iformat->read_seek)
        {
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seeking by internal MP4 format time seeking method", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME);
          ff_read_frame_flush(this->formatContext);
          int ret = this->formatContext->iformat->read_seek(this->formatContext, videoStreamId, seek_pts, flags);
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seeking by internal format time seeking method result: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, ret);

          // if ret is not 0, then error and seek failed
          found = (ret == 0);
          result = (ret == 0) ? 0 : (-8);
        }
        else
        {
          // error, MP4 must have its internal time seeking method
          result = -9;
        }
      }

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seeked to time: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, seekedTime);
    }

    ff_read_frame_flush(this->formatContext);

    if (SUCCEEDED(result) && (!found))
    {
      st = this->formatContext->streams[videoStreamId];
      ff_read_frame_flush(this->formatContext);
      index = av_index_search_timestamp(st, seek_pts, flags);

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: index: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, index);

      if ((index < 0) && (st->nb_index_entries > 0) && (seek_pts < st->index_entries[0].timestamp))
      {
        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, L"failing");
        result = -3;
      }

      if (SUCCEEDED(result) && (index >= 0) && (st->nb_index_entries > 0))
      {
        ie = &st->index_entries[index];
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: timestamp: %lld, seek_pts: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, ie->timestamp, seek_pts);
        if (ie->timestamp >= seek_pts)
        {
          // we found index entry with higher timestamp than requested
          found = true;
        }
      }

      if (SUCCEEDED(result) && (!found))
      {
        // we have to seek in stream

        // if index is on the end of index entries than probably we have to seek to unbuffered part
        // (and we don't know right position)
        // in another case we seek in bufferred part or at least we have right position where to seek
        if ((index < 0) || (index == st->nb_index_entries - 1))
        {
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: index entries: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, st->nb_index_entries);
          AVPacket avPacket;

          if ((st->nb_index_entries) && (index >= 0))
          {
            ie = &st->index_entries[index];

            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seeking to position: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, ie->pos);
            if (avio_seek(this->formatContext->pb, ie->pos, SEEK_SET) < 0)
            {
              result = -4;
            }

            if (SUCCEEDED(result))
            {
              ff_update_cur_dts(this->formatContext, st, ie->timestamp);
            }
          }
          else
          {
            // seek to zero (after seeking byte position is set to zero)
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: flushing, seeking to position: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, 0);
            avio_flush(this->formatContext->pb);
            if (avio_seek(this->formatContext->pb, 0, SEEK_SET) < 0)
            {
              result = -5;
            }
          }

          if (SUCCEEDED(result))
          {
            if (ie != NULL)
            {
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: index timestamp: %lld, index position: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, ie->timestamp, ie->pos);
            }
            int nonkey = 0;

            // read stream until we find requested time
            while ((!found) && SUCCEEDED(result))
            {
              int read_status = 0;
              do
              {
                read_status = av_read_frame(this->formatContext, &avPacket);
              } while (read_status == AVERROR(EAGAIN));

              if (read_status < 0)
              {
                // error occured
                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: av_read_frame() returned error: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, read_status);
                break;
              }

              av_free_packet(&avPacket);

              if (videoStreamId == avPacket.stream_index)
              {
                if (avPacket.flags & AV_PKT_FLAG_KEY)
                {
                  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: found keyframe with timestamp: %lld, position: %lld, stream index: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, avPacket.dts, avPacket.pos, videoStreamId);
                  found = true;
                  break;
                }

                if((nonkey++ > 1000) && (st->codec->codec_id != CODEC_ID_CDGRAPHICS))
                {
                  this->logger->Log(LOGGER_ERROR, L"%s: %s: failed as this stream seems to contain no keyframes after the target timestamp, %d non keyframes found", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, nonkey);
                  break;
                }
              }
            }
          }
        }
        else
        {
          found = true;
        }
      }
    }

    if (SUCCEEDED(result) && found && (st->nb_index_entries))
    {
      ff_read_frame_flush(this->formatContext);
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: searching keyframe with timestamp: %lld, stream index: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, seek_pts, videoStreamId);
      index = av_index_search_timestamp(st, seek_pts, flags);

      if (index < 0)
      {
        this->logger->Log(LOGGER_WARNING, L"%s: %s: index lower than zero: %d, setting to zero", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, index);
        index = 0;
      }

      if (SUCCEEDED(result))
      {
        ie = &st->index_entries[index];

        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seek to position: %lld, time: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, ie->pos, ie->timestamp);

        int64_t ret = avio_seek(this->formatContext->pb, ie->pos, SEEK_SET);
        if (ret < 0)
        {
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seek to requested position %lld failed: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, ie->pos, ret);
          result = -7;
        }

        if (SUCCEEDED(result))
        {
          ff_update_cur_dts(this->formatContext, st, ie->timestamp);
        }
      }
    }
  }

  this->logger->Log(LOGGER_INFO, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, result);
  return result;
}

HRESULT CDemuxer::SeekByPosition(REFERENCE_TIME time, int flags)
{
  // gets this->logger instance from filter
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME);

  HRESULT result = S_OK;

  AVStream *st = NULL;
  AVIndexEntry *ie = NULL;
  int64_t seek_pts = time;
  int videoStreamId = (this->activeStream[CDemuxer::Video] == ACTIVE_STREAM_NOT_SPECIFIED) ? -1 : this->streams[CDemuxer::Video]->GetItem(this->activeStream[CDemuxer::Video])->GetPid();

  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream count: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->formatContext->nb_streams);

  // if we have a video stream, seek on that one
  // if we don't, well, then don't
  if (time >= 0)
  {
    if (videoStreamId != -1)
    {
      AVStream *stream = this->formatContext->streams[videoStreamId];
      int64_t start_time = AV_NOPTS_VALUE;

      // MPEG-TS needs a protection against a wrapped around start time
      // it is possible for the start_time in this->formatContext to be wrapped around, but the start_time in the current stream not to be
      // in this case, ConvertRTToTimestamp would produce timestamps not valid for seeking
      // compensate for this by creating a negative start_time, resembling the actual value in m_avFormat->start_time without wrapping

      if (this->IsMpegTs() && (stream->start_time != AV_NOPTS_VALUE))
      {
        int64_t start = av_rescale_q(stream->start_time, stream->time_base, AV_RATIONAL_TIMEBASE);

        if (start < this->formatContext->start_time
          && this->formatContext->start_time > av_rescale_q(3LL << (stream->pts_wrap_bits - 2), stream->time_base, AV_RATIONAL_TIMEBASE)
          && start < av_rescale_q(3LL << (stream->pts_wrap_bits - 3), stream->time_base, AV_RATIONAL_TIMEBASE))
        {
          start_time = this->formatContext->start_time - av_rescale_q(1LL << stream->pts_wrap_bits, stream->time_base, AV_RATIONAL_TIMEBASE);
        }
      }

      seek_pts = ConvertRTToTimestamp(time, stream->time_base.num, stream->time_base.den, (int64_t)AV_NOPTS_VALUE);
    }
    else
    {
      seek_pts = ConvertRTToTimestamp(time, 1, AV_TIME_BASE, (int64_t)AV_NOPTS_VALUE);
    }
  }

  if (seek_pts < 0)
  {
    seek_pts = 0;
  }

  if ((wcscmp(this->inputFormat, L"rawvideo") == 0) && (seek_pts == 0))
  {
   // return this->SeekByte(0, AVSEEK_FLAG_BACKWARD);
  }

  if (videoStreamId < 0)
  {
    this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, L"videoStreamId < 0");
    videoStreamId = av_find_default_stream_index(this->formatContext);
    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: videoStreamId: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, videoStreamId);
    if (videoStreamId < 0)
    {
      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, L"not found video stream ID");
      result = -1;
    }

    if (result == S_OK)
    {
      st = this->formatContext->streams[videoStreamId];
      // timestamp for default must be expressed in AV_TIME_BASE units
      seek_pts = av_rescale(time, st->time_base.den, AV_TIME_BASE * (int64_t)st->time_base.num);
    }
  }

  bool found = false;
  // if it isn't FLV video, try to seek by internal FFmpeg time seeking method
  if (SUCCEEDED(result) && (!this->IsFlv()))
  {
    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: time: %lld, seek_pts: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, time, seek_pts);

    int ret = 0;
    if (this->formatContext->iformat->read_seek)
    {
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seeking by internal format time seeking method", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME);
      ff_read_frame_flush(this->formatContext);
      ret = this->formatContext->iformat->read_seek(this->formatContext, videoStreamId, seek_pts, flags);
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seeking by internal format time seeking method result: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, ret);
    } 
    else
    {
      ret = -1;
    }

    if (ret >= 0)
    {
      found = true;
    }
  }

  if (SUCCEEDED(result) && (!found))
  {
    st = this->formatContext->streams[videoStreamId];

    int index = -1;

    ff_read_frame_flush(this->formatContext);
    index = av_index_search_timestamp(st, seek_pts, flags);

    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: index: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, index);

    if ((index < 0) && (st->nb_index_entries) && (seek_pts < st->index_entries[0].timestamp))
    {
      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, L"failing");
      result = -2;
    }

    if (SUCCEEDED(result) && (index >= 0))
    {
      ie = &st->index_entries[index];
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: timestamp: %lld, seek_pts: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, ie->timestamp, seek_pts);
      if (ie->timestamp >= seek_pts)
      {
        // we found index entry with higher timestamp than requested
        if (!this->IsFlv())
        {
          // only when not FLV video
          found = true;
        }
      }
    }

    if (SUCCEEDED(result) && (!found))
    {
      // we have to seek in stream

      // if index is on the end of index entries than probably we have to seek to unbuffered part
      // (and we don't know right position)
      // in another case we seek in bufferred part or at least we have right position where to seek
      if ((index < 0) || (index == st->nb_index_entries - 1) || (this->IsFlv()))
      {
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: index entries: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, st->nb_index_entries);
        AVPacket avPacket;

        if ((st->nb_index_entries) && (index >= 0))
        {
          ie = &st->index_entries[index];

          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seeking to position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, ie->pos);
          if (avio_seek(this->formatContext->pb, ie->pos, SEEK_SET) < 0)
          {
            result = -3;
          }

          if (SUCCEEDED(result))
          {
            ff_update_cur_dts(this->formatContext, st, ie->timestamp);
          }
        }
        else
        {
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seeking to position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->formatContext->data_offset);
          if (avio_seek(this->formatContext->pb, this->formatContext->data_offset, SEEK_SET) < 0)
          {
            result = -4;
          }
        }

        if (SUCCEEDED(result))
        {
          if (ie != NULL)
          {
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: index timestamp: %lld, index position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, ie->timestamp, ie->pos);
          }
          int nonkey = 0;
          int64_t totalLength = 0;
          if (this->filter->GetTotalLength(&totalLength) != S_OK)
          {
            totalLength = 0;
          }

          // allocate memory for seeking positions
          ALLOC_MEM_DEFINE_SET(flvSeekingPositions, FlvSeekPosition, FLV_SEEKING_POSITIONS, 0);
          for (int i = 0; i < FLV_SEEKING_POSITIONS; i++)
          {
            flvSeekingPositions[i].position = 0;
            flvSeekingPositions[i].time = -1;
          }

          int activeFlvSeekingPosition = -1;    // any position is active
          bool backwardSeeking = false;         // specify if we seek back in flvSeekingPositions
          bool enabledSeeking = true;           // specify if seeking is enabled

          // read stream until we find requested time
          while ((!found) && SUCCEEDED(result))
          {
            int read_status = 0;
            int64_t currentPosition = -1;
            do
            {
              if (this->IsFlv())
              {
                currentPosition = avio_seek(this->formatContext->pb, 0, SEEK_CUR);
              }
              read_status = av_read_frame(this->formatContext, &avPacket);
            } while (read_status == AVERROR(EAGAIN));

                        
            if (read_status < 0)
            {
              // error occured
              // exit only when it's not FLV video or FLV video has no active seeking position
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: av_read_frame() returned error: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, read_status);
              if (((this->IsFlv()) && (activeFlvSeekingPosition == (-1))) ||
                  (!this->IsFlv()))
              {
                break;
              }
              
              if (this->IsFlv())
              {
                // while seeking forward we didn't find keyframe
                // we must seek backward
                backwardSeeking = true;
              }
            }

            av_free_packet(&avPacket);

            if ((videoStreamId == avPacket.stream_index) && (avPacket.dts > seek_pts))
            {
              if (avPacket.flags & AV_PKT_FLAG_KEY)
              {
                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: found keyframe with timestamp: %lld, position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, avPacket.dts, avPacket.pos);
                found = true;
                break;
              }

              if((nonkey++ > 1000) && (st->codec->codec_id != CODEC_ID_CDGRAPHICS))
              {
                this->logger->Log(LOGGER_ERROR, L"%s: %s: failed as this stream seems to contain no keyframes after the target timestamp, %d non keyframes found", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, nonkey);
                break;
              }
            }

            if ((videoStreamId == avPacket.stream_index) && 
                (this->IsFlv()) && 
                (flvSeekingPositions != NULL) &&
                (backwardSeeking) &&
                (activeFlvSeekingPosition >= 0) &&
                (read_status < 0))
            {
              // if we are seeking backward it means that on flvSeekingPositions[activeFlvSeekingPosition]
              // was wrong seeking value (it means that we didn't find key frame after seeking)
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: trying to seek backward: %lld, active seeking position: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, flvSeekingPositions[activeFlvSeekingPosition], activeFlvSeekingPosition);

              avio_seek(this->formatContext->pb, flvSeekingPositions[activeFlvSeekingPosition--].position, SEEK_SET);
            }

            if ((videoStreamId == avPacket.stream_index) && 
                ((avPacket.dts + FLV_DO_NOT_SEEK_DIFFERENCE) < seek_pts) &&
                (this->IsFlv()) && 
                (totalLength > 0) && 
                (flvSeekingPositions != NULL) &&
                (!backwardSeeking) &&
                (enabledSeeking))
            {
              // in case of FLV video try to guess right position value
              // do not try to guess when we are closer than FLV_DO_NOT_SEEK_DIFFERENCE ms (avPacket.dts + FLV_DO_NOT_SEEK_DIFFERENCE)

              int64_t duration = this->GetDuration() / 10000;
              // make guess of position by current packet position, time and seek time
              // because guessPosition1 is calculated as linear extrapolation it must be checked against total length
              int64_t guessPosition1 = min(totalLength, (avPacket.dts > 0) ? (seek_pts * avPacket.pos / avPacket.dts) : 0);
              int64_t guessPosition2 = seek_pts * totalLength / duration;

              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: dts: %lld, position: %lld, total length: %lld, duration: %lld, seek: %lld, remembered packet position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, avPacket.dts, avPacket.pos, totalLength, duration, seek_pts, currentPosition);

              // check for packet which is nearest lower than seek_pts and is closer than current packet
              int64_t checkNearestPosition = -1;
              int64_t checkNearestTime = -1;
              for (int i = 0; i < FLV_SEEKING_POSITIONS; i++)
              {
                if ((flvSeekingPositions[i].time != (-1)) && (flvSeekingPositions[i].time < seek_pts) && (flvSeekingPositions[i].time > checkNearestTime))
                {
                  checkNearestTime = flvSeekingPositions[i].time;
                  checkNearestPosition = flvSeekingPositions[i].position;
                }
              }

              if ((checkNearestTime != (-1)) && (checkNearestTime > avPacket.dts))
              {
                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: current packet dts is lower than some of previous seeks, disabling seeks, packet dts: %lld, stored dts: %lld, stored position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, avPacket.dts, checkNearestTime, checkNearestPosition);

                avio_seek(this->formatContext->pb, checkNearestPosition, SEEK_SET);
                enabledSeeking = false;
              }

              flvSeekingPositions[++activeFlvSeekingPosition].position = currentPosition;
              flvSeekingPositions[activeFlvSeekingPosition].time = avPacket.dts;

              int64_t guessPosition = (guessPosition1 + guessPosition2) / 2;

              if ((enabledSeeking) && (checkNearestPosition != (-1)) && (checkNearestPosition > guessPosition))
              {
                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: guessed position is lower than some of previous seeks, disabling seeks, guess position: %lld, stored dts: %lld, stored position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, guessPosition, checkNearestTime, checkNearestPosition);

                avio_seek(this->formatContext->pb, checkNearestPosition, SEEK_SET);
                enabledSeeking = false;
              }

              // guess position have to be lower than total length
              if ((guessPosition < totalLength) && (enabledSeeking))
              {
                // we can't do avio_seek because we need data from exact position
                // we need to call our seek
                
                if (this->formatContext->pb->seek(this->formatContext->pb->opaque, guessPosition, SEEK_SET) >= 0)
                {
                  int firstFlvPacketPosition = -1;    // position of first FLV packet
                  int packetsChecked  = 0;            // checked FLV packets count
                  int processedBytes = 0;             // processed bytes for correct seek position value

                  ALLOC_MEM_DEFINE_SET(buffer, unsigned char, FLV_SEEKING_BUFFER_SIZE, 0);

                  while ((firstFlvPacketPosition < 0) || (packetsChecked <= FLV_PACKET_MINIMUM_CHECKED))
                  {
                    // repeat until first FLV packet is found and verified by at least (FLV_PACKET_MINIMUM_CHECKED + 1) another FLV packet

                    // we can't read data with avio_read, because it uses internal FLV format methods to parse data
                    // because in most cases we seek to non-FLV packet (but somewhere else) we need to use our method to read data
                    int readBytes = this->formatContext->pb->read_packet(this->formatContext->pb->opaque, buffer, FLV_SEEKING_BUFFER_SIZE);
                    if (readBytes > 0)
                    {
                      // try to find flv packets in buffer

                      int i = 0;
                      int length = 0;
                      while (i < readBytes)
                      {
                        // we have to check bytes in whole buffer

                        if (((buffer[i] == FLV_PACKET_AUDIO) || (buffer[i] == FLV_PACKET_VIDEO) || (buffer[i] == FLV_PACKET_META)) && (firstFlvPacketPosition == (-1)))
                        {
                          length = 0;
                          // possible audio, video or meta tag
                          if ((i + 3) < readBytes)
                          {
                            // in buffer have to be at least 3 next bytes for length
                            // remember length and possible first FLV packet postion
                            length = (buffer[i + 1] << 8 | buffer[i + 2]) << 8 | buffer[i + 3];
                            if (length > (readBytes - i))
                            {
                              // length has wrong value, it's after valid data
                              firstFlvPacketPosition = -1;
                              packetsChecked = 0;
                              i++;
                              continue;
                            }
                            // the length is in valid range
                            // remeber first FLV packet position and skip to possible next packet
                            firstFlvPacketPosition = i;
                            i += length + 15;
                            continue;
                          }
                          else
                          {
                            // clear first FLV packet position and go to next byte in buffer
                            firstFlvPacketPosition = -1;
                            packetsChecked = 0;
                            i++;
                            continue;
                          }
                        }
                        else if (((buffer[i] == FLV_PACKET_AUDIO) || (buffer[i] == FLV_PACKET_VIDEO) || (buffer[i] == FLV_PACKET_META)) && (firstFlvPacketPosition != (-1)))
                        {
                          // possible next packet, verify
                          int previousLength = -1;
                          int nextLength = -1;

                          if ((i - 3) >= 0)
                          {
                            // valid range for previous length
                            previousLength = (buffer[i - 3] << 8 | buffer[i - 2]) << 8 | buffer[i - 1];
                          }

                          if ((i + 3) < readBytes)
                          {
                            // valid range for previous length
                            nextLength = (buffer[i + 1] << 8 | buffer[i + 2]) << 8 | buffer[i + 3];
                          }

                          if ((previousLength != (-1)) && (nextLength != (-1)))
                          {
                            if (previousLength == (length + 11))
                            {
                              // correct value of previous length
                              // skip to next possible FLV packet
                              packetsChecked++;
                              i += nextLength + 15;
                              length = nextLength;
                              continue;
                            }
                          }

                          // bad FLV packet
                          i = firstFlvPacketPosition + 1;
                          firstFlvPacketPosition = -1;
                          packetsChecked = 0;
                          continue;
                        }
                        else if (firstFlvPacketPosition != (-1))
                        {
                          // FLV packet after first FLV packet not found
                          // first FLV packet is not FLV packet
                          i = firstFlvPacketPosition + 1;
                          firstFlvPacketPosition = -1;
                          packetsChecked = 0;
                          continue;
                        }

                        // go to next byte in buffer
                        i++;
                      }
                    }
                    else
                    {
                      if (readBytes < 0)
                      {
                        this->logger->Log(LOGGER_WARNING, L"%s: %s: avio_read() returned error: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, readBytes);
                      }
                      break;
                    }

                    if ((firstFlvPacketPosition < 0) || (packetsChecked <= FLV_PACKET_MINIMUM_CHECKED))
                    {
                      processedBytes += readBytes;
                      this->logger->Log(LOGGER_WARNING, L"%s: %s: not found relevant FLV packet, processed bytes: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, processedBytes);
                    }
                  }

                  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: first FLV position: 0x%08X, packets checked: %d, processed bytes: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, firstFlvPacketPosition, packetsChecked, processedBytes);

                  FREE_MEM(buffer);

                  if ((firstFlvPacketPosition >= 0) && (packetsChecked > FLV_PACKET_MINIMUM_CHECKED))
                  {
                    // first FLV packet position is set
                    // at least (FLV_PACKET_MINIMUM_CHECKED + 1) another packet checked
                    // seek to position
                    int64_t valueToSeek = guessPosition + processedBytes + firstFlvPacketPosition;

                    bool canSeek = true;
                    for (int i = 0; i <= activeFlvSeekingPosition; i++)
                    {
                      if (valueToSeek == flvSeekingPositions[i].position)
                      {
                        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: trying to seek to same position: %lld, disabled", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, valueToSeek);
                        canSeek = false;
                        break;
                      }
                    }

                    if (canSeek)
                    {
                      flvSeekingPositions[++activeFlvSeekingPosition].position = valueToSeek;

                      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: trying to seek: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, valueToSeek);
                      avio_seek(this->formatContext->pb, valueToSeek, SEEK_SET);
                    }
                    else
                    {
                      // we must seek to relevant FLV packet
                      // just use nearest lower packet for specified seek_pts
                      int64_t nearestPosition = -1;
                      int64_t nearestTime = -1;
                      for (int i = 0; i < FLV_SEEKING_POSITIONS; i++)
                      {
                        if ((flvSeekingPositions[i].time != (-1)) && (flvSeekingPositions[i].time < seek_pts) && (flvSeekingPositions[i].time > nearestTime))
                        {
                          nearestTime = flvSeekingPositions[i].time;
                          nearestPosition = flvSeekingPositions[i].position;
                        }
                      }

                      if (nearestPosition != (-1))
                      {
                        // disable another seeking, just wait for data by sequence reading
                        enabledSeeking = false;

                        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: trying to seek: %lld, disabling further seek", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, nearestPosition);
                        avio_seek(this->formatContext->pb, nearestPosition, SEEK_SET);
                      }
                      else
                      {
                        // very bad, but this should not happen, because at least one FLV packet is added to flvSeekingPositions
                        // when starting this special seeking procedure

                        this->logger->Log(LOGGER_WARNING, L"%s: %s: cannot find any seeking position, computed seeking position was disabled", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME);
                      }
                    }
                  }
                  else
                  {
                    // it's bad, we not found FLV packet position or it cannot be checked
                    // seek to last usable index

                    this->logger->Log(LOGGER_WARNING, L"%s: %s: not found relevant FLV packet, seeking to last usable index position", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME);

                    ff_read_frame_flush(this->formatContext);
                    index = av_index_search_timestamp(st, seek_pts, flags);

                    if (index < 0)
                    {
                      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seeking to position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->formatContext->data_offset);
                      if (avio_seek(this->formatContext->pb, this->formatContext->data_offset, SEEK_SET) < 0)
                      {
                        result = -8;
                      }
                    }
                    else
                    {
                      ie = &st->index_entries[index];

                      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seek to position: %lld, time: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, ie->pos, ie->timestamp);

                      int64_t ret = avio_seek(this->formatContext->pb, ie->pos, SEEK_SET);
                      if (ret < 0)
                      {
                        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seek to requested position %lld failed: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, ie->pos, ret);
                        result = -9;
                      }

                      if (SUCCEEDED(result))
                      {
                        ff_update_cur_dts(this->formatContext, st, ie->timestamp);
                      }
                    }
                  }
                }
              }
            }
          }

          FREE_MEM(flvSeekingPositions);
        }
      }
      else
      {
        found = true;
      }
    }
  }

  if (SUCCEEDED(result) && found)
  {
    int index = -1;
    st = this->formatContext->streams[videoStreamId];

    ff_read_frame_flush(this->formatContext);
    index = av_index_search_timestamp(st, seek_pts, flags);

    if (index < 0)
    {
      // it's only warning, because we seek to start of stream
      // it means that we have to read stream from start to find requested timestamp
      // or change ffmpeg binaries to hold index entries for current stream format
      this->logger->Log(LOGGER_WARNING, L"%s: %s: index lower than zero: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, index);
      //result = -6;
    }

    if (this->IsFlv())
    {
      // in case of FLV video we can be after seek time, but searching index can find index too back
      ff_read_frame_flush(this->formatContext);
      int index2 = av_index_search_timestamp(st, seek_pts, flags & (~AVSEEK_FLAG_BACKWARD));

      if (index2 < 0)
      {
        this->logger->Log(LOGGER_WARNING, L"%s: %s: second index lower than zero: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, index2);
      }
      else
      {
        // we choose index which is closer to seek time
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: first index: %d, second index: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, index, index2);

        AVIndexEntry *ie1 = &st->index_entries[index];
        AVIndexEntry *ie2 = &st->index_entries[index2];

        int64_t diff1 = abs(seek_pts - ie1->timestamp);
        int64_t diff2 = abs(seek_pts - ie2->timestamp);

        if (diff2 < diff1)
        {
          // choose second index, it's closer to requested seek time
          index = index2;
        }
      }
    }

    if (SUCCEEDED(result) && (st->index_entries != NULL))
    {
      ie = &st->index_entries[index];

      if (SUCCEEDED(result) && (!this->IsMatroska()))
      {
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seek to position: %lld, time: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, ie->pos, ie->timestamp);
        int64_t ret = avio_seek(this->formatContext->pb, ie->pos, SEEK_SET);
        if (ret < 0)
        {
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seek to requested position %lld failed: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, ie->pos, ret);
          result = -8;
        }
      }

      if (SUCCEEDED(result))
      {
        ff_update_cur_dts(this->formatContext, st, ie->timestamp);
      }
    }
  }

  this->logger->Log(LOGGER_INFO, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, result);
  return result;
}

HRESULT CDemuxer::SeekBySequenceReading(REFERENCE_TIME time, int flags)
{
  // gets logger instance from filter
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME);

  HRESULT result = S_OK;

  AVStream *st = NULL;
  AVIndexEntry *ie = NULL;
  int64_t seek_pts = time;
  int videoStreamId = (this->activeStream[CDemuxer::Video] == ACTIVE_STREAM_NOT_SPECIFIED) ? -1 : this->streams[CDemuxer::Video]->GetItem(this->activeStream[CDemuxer::Video])->GetPid();

  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream count: %d", MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, this->formatContext->nb_streams);

  // if we have a video stream, seek on that one
  // if we don't, well, then don't
  if (time >= 0)
  {
    if (videoStreamId != -1)
    {
      AVStream *stream = this->formatContext->streams[videoStreamId];
      seek_pts = this->ConvertRTToTimestamp(time, stream->time_base.num, stream->time_base.den, (int64_t)AV_NOPTS_VALUE);
    }
    else
    {
      seek_pts = this->ConvertRTToTimestamp(time, 1, AV_TIME_BASE, (int64_t)AV_NOPTS_VALUE);
    }
  }

  if (videoStreamId < 0)
  {
    this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, L"videoStreamId < 0");
    videoStreamId = av_find_default_stream_index(this->formatContext);
    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: videoStreamId: %d", MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, videoStreamId);
    if (videoStreamId < 0)
    {
      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, L"not found video stream ID");
      result = -1;
    }

    if (result == S_OK)
    {
      st = this->formatContext->streams[videoStreamId];
      // timestamp for default must be expressed in AV_TIME_BASE units
      seek_pts = av_rescale(time, st->time_base.den, AV_TIME_BASE * (int64_t)st->time_base.num);
    }
  }

  bool found = false;

  if (SUCCEEDED(result))
  {
    st = this->formatContext->streams[videoStreamId];
    if (st->nb_index_entries == 0)
    {
      // bad, we don't have index and we can't seek
      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, L"no index entries");
      result = -2;
    }
  }

  if (SUCCEEDED(result))
  {
    // get stream available length
    int64_t availableLength = 0;
    result = this->filter->GetAvailableLength(&availableLength);

    // get index for seeking
    int index = -1;
    if (SUCCEEDED(result))
    {
      index = av_index_search_timestamp(st, seek_pts, flags);
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: index: %d", MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, index);

      if (index < 0)
      {
        // we have index entries and seek_pts (seek time) is lower than first timestamp in index entries
        // (first timestamp should be start of video)
        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, L"failing, not found index");
        result = -4;
      }
    }

    if (SUCCEEDED(result))
    {
      ie = &st->index_entries[index];
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: timestamp: %lld, seek_pts: %lld", MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, ie->timestamp, seek_pts);

      if (ie->timestamp >= seek_pts)
      {
        // we found index entry with higher timestamp than requested
        if (!this->IsFlv())
        {
          // only when not FLV video
          found = true;
        }
      }
    }

    if (SUCCEEDED(result) && (!found))
    {
      if (ie != NULL)
      {
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: index timestamp: %lld, index position: %lld", MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, ie->timestamp, ie->pos);
      }

      // if seek time is before end of indexes (end of timestamps)
      if (!((index == (st->nb_index_entries - 1)) && (seek_pts > st->index_entries[index].timestamp)))
      {
        // if index position is before available length, just seek to requested position
        if (ie->pos < availableLength)
        {
          ff_read_frame_flush(this->formatContext);
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seeking to position: %lld", MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, ie->pos);
          if (avio_seek(this->formatContext->pb, ie->pos, SEEK_SET) < 0)
          {
            result = -6;
          }

          if (SUCCEEDED(result))
          {
            ff_update_cur_dts(this->formatContext, st, ie->timestamp);
          }

          found = true;
        }
      }

      if (SUCCEEDED(result) && (!found))
      {
        AVPacket avPacket;
        int nonkey = 0;

        // read stream until we find requested time
        while ((!found) && SUCCEEDED(result))
        {
          int read_status = 0;
          do
          {
            read_status = av_read_frame(this->formatContext, &avPacket);
          } while (read_status == AVERROR(EAGAIN));


          if (read_status < 0)
          {
            // error occured
            break;
          }

          av_free_packet(&avPacket);

          if ((videoStreamId == avPacket.stream_index) && (avPacket.dts > seek_pts))
          {
            if (avPacket.flags & AV_PKT_FLAG_KEY)
            {
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: found keyframe with timestamp: %lld, position: %lld", MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, avPacket.dts, avPacket.pos);
              found = true;
              break;
            }

            if((nonkey++ > 1000) && (st->codec->codec_id != CODEC_ID_CDGRAPHICS))
            {
              this->logger->Log(LOGGER_ERROR, L"%s: %s: failed as this stream seems to contain no keyframes after the target timestamp, %d non keyframes found", MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, nonkey);
              break;
            }
          }
        }
      }
    }
  }

  if (SUCCEEDED(result) && (found))
  {
    // if there is internal ffmpeg time seeking method, call it
    int ret = 0;
    if (this->formatContext->iformat->read_seek)
    {
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seeking by internal format time seeking method", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME);
      ff_read_frame_flush(this->formatContext);
      ret = this->formatContext->iformat->read_seek(this->formatContext, videoStreamId, seek_pts, flags);
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seeking by internal format time seeking method result: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, ret);
    } 
  }

  if (SUCCEEDED(result) && found)
  {
    int index = -1;
    st = this->formatContext->streams[videoStreamId];

    ff_read_frame_flush(this->formatContext);
    index = av_index_search_timestamp(st, seek_pts, flags);

    if (index < 0)
    {
      this->logger->Log(LOGGER_ERROR, L"%s: %s: index lower than zero: %d", MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, index);
      result = -7;
    }

    if (this->IsFlv())
    {
      // in case of FLV video we can be after seek time, but searching index can find index too back
      ff_read_frame_flush(this->formatContext);
      int index2 = av_index_search_timestamp(st, seek_pts, flags & (~AVSEEK_FLAG_BACKWARD));

      if (index2 < 0)
      {
        this->logger->Log(LOGGER_WARNING, L"%s: %s: second index lower than zero: %d", MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, index2);
      }
      else
      {
        // we choose index which is closer to seek time
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: first index: %d, second index: %d", MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, index, index2);

        AVIndexEntry *ie1 = &st->index_entries[index];
        AVIndexEntry *ie2 = &st->index_entries[index2];

        int64_t diff1 = abs(seek_pts - ie1->timestamp);
        int64_t diff2 = abs(seek_pts - ie2->timestamp);

        if (diff2 < diff1)
        {
          // choose second index, it's closer to requested seek time
          index = index2;
        }
      }
    }

    if (SUCCEEDED(result))
    {
      ie = &st->index_entries[index];

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seek to position: %lld, time: %lld", MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, ie->pos, ie->timestamp);

      if (SUCCEEDED(result))
      {
        int64_t ret = avio_seek(this->formatContext->pb, ie->pos, SEEK_SET);
        if (ret < 0)
        {
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: seek to requested position %lld failed: %d", MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, ie->pos, ret);
          result = -8;
        }
      }

      if (SUCCEEDED(result))
      {
        ff_update_cur_dts(this->formatContext, st, ie->timestamp);
      }
    }
  }

  this->logger->Log(LOGGER_INFO, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, result);
  return result;
}
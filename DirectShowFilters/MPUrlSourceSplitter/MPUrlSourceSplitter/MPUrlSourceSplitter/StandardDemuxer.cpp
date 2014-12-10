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

#include "StandardDemuxer.h"
#include "DemuxerUtils.h"
#include "DemuxerVideoHelper.h"
#include "FlvPacket.h"
#include "LockMutex.h"
#include "ErrorCodes.h"
//#include "Parameters.h"
//#include "StreamPackage.h"
//#include "StreamPackageDataRequest.h"
//#include "StreamPackageDataResponse.h"
//#include "StreamPackagePacketRequest.h"
//#include "StreamPackagePacketResponse.h"
#include "FFmpegLogger.h"

extern "C++" CFFmpegLogger *ffmpegLogger;

extern "C" void asf_reset_header2(AVFormatContext *s);

#include "moreuuids.h"

#include <assert.h>
#include <Shlwapi.h>
#include <process.h>

#ifdef _DEBUG
#define MODULE_NAME                                                   L"StandardDemuxerd"
#else
#define MODULE_NAME                                                   L"StandardDemuxer"
#endif

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

CStandardDemuxer::CStandardDemuxer(HRESULT *result, CLogger *logger, IDemuxerOwner *filter, CParameterCollection *configuration)
  : CDemuxer(result, logger, filter, configuration)
{
  this->containerFormat = NULL;
  this->formatContext = NULL;
  this->demuxerContext = NULL;
  this->ffmpegContext = NULL;

  for (unsigned int i = 0; i < CStream::Unknown; i++)
  {
    this->streams[i] = NULL;
  }

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    for (unsigned int i = 0; (SUCCEEDED(*result) && (i < CStream::Unknown)); i++)
    {
      this->streams[i] = new CStreamCollection(result);
      CHECK_POINTER_HRESULT(*result, this->streams[i], *result, E_OUTOFMEMORY);

      this->activeStream[i] = ACTIVE_STREAM_NOT_SPECIFIED;
    }

    if (SUCCEEDED(*result))
    {
      // FFMpeg context after registering (RegisterFFmpegContext() method) will be released from memory by calling UnregisterFFmpegContext() method
      // explicit release is allowed only in case of failed registering
      this->ffmpegContext = new CFFmpegContext(result, this);
      CHECK_POINTER_HRESULT(*result, this->ffmpegContext, *result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(*result, ffmpegLogger->RegisterFFmpegContext(this->ffmpegContext), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(this->ffmpegContext));
    }
  }
}

CStandardDemuxer::~CStandardDemuxer(void)
{
  // destroy create demuxer worker (if not finished earlier)
  this->DestroyCreateDemuxerWorker();

  // destroy demuxing worker (if not finished earlier)
  this->DestroyDemuxingWorker();

  this->CleanupFormatContext();

  // release AVIOContext for demuxer
  if (this->demuxerContext != NULL)
  {
    av_free(this->demuxerContext->buffer);
    av_free(this->demuxerContext);
    this->demuxerContext = NULL;
  }

  for (unsigned int i = 0; i < CStream::Unknown; i++)
  {
    FREE_MEM_CLASS(this->streams[i]);
  }

  FREE_MEM(this->containerFormat);

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->ffmpegContext, ffmpegLogger->UnregisterFFmpegContext(this->ffmpegContext));
}

/* CDemuxer methods */

int64_t CStandardDemuxer::GetDuration(void)
{
  int64_t duration = this->filter->GetDuration();

  if (duration == DURATION_UNSPECIFIED)
  {
    if ((this->formatContext == NULL) || (this->formatContext->duration == (int64_t)AV_NOPTS_VALUE) || (this->formatContext->duration < 0LL))
    {
      // no duration is available for us
      duration = -1;
    }
    else
    {
      duration = ConvertTimestampToRT(this->formatContext->duration, 1, AV_TIME_BASE, 0);
    }

  }
  else if (duration != DURATION_LIVE_STREAM)
  {
    duration *= (DSHOW_TIME_BASE / 1000);
  }

  return duration;
}

uint64_t CStandardDemuxer::GetPositionForStreamTime(uint64_t streamTime)
{
  uint64_t result = 0;

  int streamId = -1;
  CStream *activeStream = NULL;

  for (unsigned int i = 0; ((streamId == (-1)) && (i < CStream::Unknown)); i++)
  {
    // stream groups are in order: video, audio, subtitle = in our preference
    if (this->GetStreams((CStream::StreamType)i)->Count() > 0)
    {
      activeStream = this->GetStreams((CStream::StreamType)i)->GetItem((this->activeStream[(CStream::StreamType)i] == ACTIVE_STREAM_NOT_SPECIFIED) ? 0 : this->activeStream[(CStream::StreamType)i]);
      streamId = activeStream->GetPid();
    }
  }

  if (streamId != (-1))
  {
    int64_t streamTimestamp = 0;
    if ((time >= 0) && (streamId != (-1)))
    {
      AVStream *stream = this->formatContext->streams[streamId];
      streamTimestamp = ConvertRTToTimestamp(streamTime * (DSHOW_TIME_BASE / 1000), stream->time_base.num, stream->time_base.den, (int64_t)AV_NOPTS_VALUE);
    }

    AVStream *st = this->formatContext->streams[streamId];

    if (st->nb_index_entries != 0)
    {
      // check FFmpeg seek index entries
      int index = av_index_search_timestamp(st, streamTimestamp, AVSEEK_FLAG_BACKWARD);

      if (index != (-1))
      {
        AVIndexEntry *ie = &st->index_entries[index];

        result = (ie->pos > 0) ? (uint64_t)ie->pos : 0;
      }
    }
    else
    {
      // check our seek index entry collection
      HRESULT index = activeStream->GetSeekIndexEntries()->FindSeekIndexEntry(streamTimestamp, true);

      if (SUCCEEDED(index))
      {
        CSeekIndexEntry *ie = activeStream->GetSeekIndexEntries()->GetItem((unsigned int)index);

        result = (ie->GetPosition() > 0) ? (uint64_t)ie->GetPosition() : 0;
      }
    }
  }

  return result;
}

// IFFmpegLog interface

bool CStandardDemuxer::FFmpegLog(CFFmpegLogger *ffmpegLogger, CFFmpegContext *context, void *ffmpegPtr, int ffmpegLogLevel, const char *ffmpegFormat, va_list ffmpegList)
{
  bool result = false;
  AVFormatContext *formatContext = (AVFormatContext *)ffmpegPtr;
  CStandardDemuxer *demuxer = NULL;

  if ((formatContext != NULL) && (formatContext->pb != NULL) && (formatContext->pb->opaque != NULL))
  {
    demuxer = (CStandardDemuxer *)(formatContext->pb->opaque);

    if (demuxer != NULL)
    {
      result = true;

      if ((!demuxer->IsAvi()) && ((!demuxer->IsMpegTs()) || (demuxer->IsMpegTs() && (ffmpegLogLevel < AV_LOG_WARNING))))
      {
        wchar_t *message = ffmpegLogger->GetFFmpegMessage(ffmpegFormat, ffmpegList);

        if (message != NULL)
        {
          demuxer->logger->Log(LOGGER_VERBOSE, L"%s: %s: demuxer stream: %u, log level: %d, message: '%s'", MODULE_NAME, METHOD_FFMPEG_LOG_NAME, demuxer->GetDemuxerId(), ffmpegLogLevel, message);
        }

        FREE_MEM(message);
      }
    }
  }

  return result;
}

/* get methods */

CStreamCollection *CStandardDemuxer::GetStreams(CStream::StreamType type)
{
  return this->streams[type];
}

const wchar_t *CStandardDemuxer::GetContainerFormat(void)
{
  return this->containerFormat;
}

CStream *CStandardDemuxer::GetActiveStream(CStream::StreamType streamType)
{
  CStream *result = NULL;
  int activeStreamId = this->activeStream[streamType];

  if (activeStreamId != ACTIVE_STREAM_NOT_SPECIFIED)
  {
    CStreamCollection *streams = this->streams[(CStream::StreamType)streamType];

    for (unsigned int i = 0; i < streams->Count(); i++)
    {
      CStream *stream = streams->GetItem(i);

      if (stream->GetPid() == activeStreamId)
      {
        result = stream;
        break;
      }
    }
  }

  return result;
}

/* set methods */

void CStandardDemuxer::SetActiveStream(CStream::StreamType streamType, int activeStreamId)
{
  CStreamCollection *streams = this->streams[(CStream::StreamType)streamType];

  if (activeStreamId == ACTIVE_STREAM_NOT_SPECIFIED)
  {
    this->activeStream[streamType] = ACTIVE_STREAM_NOT_SPECIFIED;
  }
  else
  {
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
}

/* other methods */

CStream *CStandardDemuxer::SelectVideoStream(void)
{
  CStream *result = NULL;

  CStreamCollection *videoStreams = this->GetStreams(CStream::Video);

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

CStream *CStandardDemuxer::SelectAudioStream(void)
{
  CStream *result = NULL;
  CStreamCollection *audioStreams = this->GetStreams(CStream::Audio);

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

HRESULT CStandardDemuxer::Seek(REFERENCE_TIME time)
{
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_SEEK_NAME, this->demuxerId);
  this->logger->Log(LOGGER_INFO, L"%s: %s: stream %u, seeking to time: %lld", MODULE_NAME, METHOD_SEEK_NAME, this->demuxerId, time);

  HRESULT result = S_OK;

  // get seeking capabilities from filter
  unsigned int seekingCapabilities = this->filter->GetSeekingCapabilities();
  // we prefer seeking by position, it's simplier and buffer is also based on position

  if (seekingCapabilities & SEEKING_METHOD_POSITION)
  {
    result = this->SeekByPosition(time, AVSEEK_FLAG_BACKWARD);

    if (FAILED(result))
    {
      this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, first seek by position failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, this->demuxerId, result);

      result = this->SeekByPosition(time, AVSEEK_FLAG_ANY);
      if (FAILED(result))
      {
        this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, second seek by position failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, this->demuxerId, result);
      }
    }
  }

  if (SUCCEEDED(result) && (seekingCapabilities & SEEKING_METHOD_TIME))
  {
    result = this->SeekByTime(time, AVSEEK_FLAG_BACKWARD);

    if (FAILED(result))
    {
      this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, first seek by time failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, this->demuxerId, result);

      result = this->SeekByTime(time, AVSEEK_FLAG_ANY);    // seek forward
      if (FAILED(result))
      {
        this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, second seek by time failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, this->demuxerId, result);
      }
    }
  }

  if (SUCCEEDED(result) && (seekingCapabilities == SEEKING_METHOD_NONE))
  {
    // it should not happen
    // seeking backward is simple => just moving backward in buffer
    // seeking forward is waiting for right timestamp by sequence reading
    result = this->SeekBySequenceReading(time, AVSEEK_FLAG_BACKWARD);

    if (FAILED(result))
    {
      this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, first seek by sequence reading failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, this->demuxerId, result);

      result = this->SeekBySequenceReading(time, AVSEEK_FLAG_ANY);
      if (FAILED(result))
      {
        this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, second seek by sequence reading failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, this->demuxerId, result);
      }
    }
  }

  for (unsigned i = 0; i < this->formatContext->nb_streams; i++)
  {
    this->InitParser(this->formatContext, this->formatContext->streams[i]);
    this->UpdateParserFlags(this->formatContext->streams[i]);
  }

  this->flags &= ~STANDARD_DEMUXER_FLAG_VC1_SEEN_TIMESTAMP;
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_END_FORMAT, MODULE_NAME, METHOD_SEEK_NAME, this->demuxerId);

  return result;
}

/* protected methods */

void CStandardDemuxer::CleanupDemuxerInternal(void)
{
  if (this->demuxerContext != NULL)
  {
    av_free(this->demuxerContext->buffer);
    av_free(this->demuxerContext);
    this->demuxerContext = NULL;
    this->demuxerContextBufferPosition = 0;
  }
}

void CStandardDemuxer::DemuxingWorkerInternal(void)
{
  if (this->IsSetFlags(DEMUXER_FLAG_DISABLE_DEMUXING_WITH_RETURN_TO_DEMUXING_WORKER) || this->IsSetFlags(DEMUXER_FLAG_DISABLE_DEMUXING_WITH_SAFE_RETURN_TO_DEMUXING_WORKER))
  {
    this->flags |= DEMUXER_FLAG_DISABLE_DEMUXING;
    this->flags &= ~(DEMUXER_FLAG_DISABLE_DEMUXING_WITH_RETURN_TO_DEMUXING_WORKER | DEMUXER_FLAG_DISABLE_DEMUXING_WITH_SAFE_RETURN_TO_DEMUXING_WORKER);

    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, disabled demuxing", MODULE_NAME, METHOD_DEMUXING_WORKER_NAME, this->demuxerId);
  }

  if ((!this->IsSetFlags(DEMUXER_FLAG_DISABLE_DEMUXING)) && 
    (!this->IsSetFlags(DEMUXER_FLAG_DISABLE_READING)) && 
    (!this->IsEndOfStreamOutputPacketQueued()))
  {
    // S_FALSE means no packet
    HRESULT result = S_FALSE;
    COutputPinPacket *packet = new COutputPinPacket(&result);
    CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      result = this->GetNextPacketInternal(packet);

      if (FAILED(result) && (result != E_PAUSE_SEEK_STOP_MODE_DISABLE_READING))
      {
        // any error code (except disabled reading) for end of stream
        this->logger->Log(LOGGER_INFO, L"%s: %s: stream %u, end of stream, error: 0x%08X", MODULE_NAME, METHOD_DEMUXING_WORKER_NAME, this->demuxerId, result);

        packet->SetDemuxerId(this->demuxerId);
        packet->SetEndOfStream(true, (result == E_NO_MORE_DATA_AVAILABLE) ? S_OK : result);
        result = S_OK;
      }
    }

    // S_FALSE means no packet
    if (result == S_OK)
    {
      CLockMutex lock(this->outputPacketMutex, INFINITE);

      if (packet->IsEndOfStream())
      {
        bool queuedEndOfStream = false;
        HRESULT endOfStreamResult = packet->GetEndOfStreamResult();

        for (unsigned int i = 0; (SUCCEEDED(result) && (i < CStream::Unknown)); i++)
        {
          CStreamCollection *streams = this->streams[i];

          for (unsigned int j = 0; (SUCCEEDED(result) && (j < streams->Count())); j++)
          {
            CStream *stream = streams->GetItem(j);

            COutputPinPacket *endOfStreamPacket = new COutputPinPacket(&result);
            CHECK_POINTER_HRESULT(result, endOfStreamPacket, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              endOfStreamPacket->SetDemuxerId(this->demuxerId);
              endOfStreamPacket->SetEndOfStream(true, packet->GetEndOfStreamResult());
              endOfStreamPacket->SetStreamPid(stream->GetPid());

              CHECK_CONDITION_HRESULT(result, this->outputPacketCollection->Add(endOfStreamPacket), result, E_OUTOFMEMORY);
              queuedEndOfStream = true;
            }

            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(endOfStreamPacket));
          }
        }

        if (SUCCEEDED(result) && (!queuedEndOfStream))
        {
          CHECK_CONDITION_HRESULT(result, this->outputPacketCollection->Add(packet), result, E_OUTOFMEMORY);

          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packet));
        }

        CHECK_CONDITION_EXECUTE(queuedEndOfStream, FREE_MEM_CLASS(packet));

        if (SUCCEEDED(result))
        {
          this->flags |= DEMUXER_FLAG_END_OF_STREAM_OUTPUT_PACKET_QUEUED;
          this->logger->Log(LOGGER_INFO, L"%s: %s: stream %u, queued end of stream output packet, result: 0x%08X", MODULE_NAME, METHOD_DEMUXING_WORKER_NAME, this->demuxerId, endOfStreamResult);
        }
      }
      else
      {
        CHECK_CONDITION_HRESULT(result, this->outputPacketCollection->Add(packet), result, E_OUTOFMEMORY);
      }
    }

    CHECK_CONDITION_EXECUTE(result != S_OK, FREE_MEM_CLASS(packet));
  }
}

HRESULT CStandardDemuxer::GetNextPacketInternal(COutputPinPacket *packet)
{
  // S_FALSE means no packet
  HRESULT result = S_FALSE;
  CHECK_POINTER_DEFAULT_HRESULT(result, packet);

  if (SUCCEEDED(result))
  {
    // read FFmpeg packet
    AVPacket ffmpegPacket;

    // assume we are not eof
    CHECK_CONDITION_EXECUTE(this->formatContext->pb != NULL, this->formatContext->pb->eof_reached = 0);

    int ffmpegResult = av_read_frame(this->formatContext, &ffmpegPacket);

    if ((ffmpegResult == AVERROR(EINTR)) || (ffmpegResult == AVERROR(EAGAIN)))
    {
      // timeout, probably no real error, return empty packet
    }
    else if (ffmpegResult == AVERROR_EOF)
    {
      // end of file reached
      result = E_NO_MORE_DATA_AVAILABLE;
    }
    else if ((ffmpegResult == E_CONNECTION_LOST_TRYING_REOPEN) || (this->IsSetFlags(DEMUXER_FLAG_PENDING_DISCONTINUITY)))
    {
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, discontinuity received or connection lost", MODULE_NAME, METHOD_GET_NEXT_PACKET_INTERNAL_NAME, this->demuxerId);

      // connection lost or pending discontinuity
      // FFmpeg sometimes doesn't return error code, but send end of stream
      ff_read_frame_flush(this->formatContext);

      result = S_FALSE;
      this->flags &= ~(DEMUXER_FLAG_PENDING_DISCONTINUITY | DEMUXER_FLAG_PENDING_DISCONTINUITY_WITH_REPORT);

      // set discontinuity for all streams, we have lost some data

      for (unsigned int i = 0; i < CStream::Unknown; i++)
      {
        CStreamCollection *streams = this->GetStreams((CStream::StreamType)i);

        for (unsigned int j = 0; j < streams->Count(); j++)
        {
          CStream *stream = streams->GetItem(j);

          stream->SetDiscontinuity(true);
        }
      }
    }
    else if (ffmpegResult < 0)
    {
      // meh, fail
      result = ffmpegResult;
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

      CStream *activeStream = NULL;
      for (unsigned int i = 0; i < CStream::Unknown; i++)
      {
        CStreamCollection *streams = this->GetStreams((CStream::StreamType)i);

        for (unsigned int j = 0; j < streams->Count(); j++)
        {
          CStream *stream = streams->GetItem(j);

          if (ffmpegPacket.stream_index ==  stream->GetPid())
          {
            // we found active stream to FFmpeg packet stream index
            activeStream = stream;
            break;
          }
        }
      }

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

      CHECK_CONDITION_HRESULT(result, packet->GetBuffer()->InitializeBuffer((ffmpegPacket.data != NULL) ? ffmpegPacket.size : 1), result, E_OUTOFMEMORY);
      if (SUCCEEDED(result) && (ffmpegPacket.data != NULL))
      {
        packet->GetBuffer()->AddToBuffer(ffmpegPacket.data, ffmpegPacket.size);
      }

      if (SUCCEEDED(result))
      {
        packet->SetStreamPid((unsigned int)ffmpegPacket.stream_index);
        packet->SetDemuxerId(this->demuxerId);

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

        REFERENCE_TIME pts = this->GetPacketPts(&ffmpegPacket);
        REFERENCE_TIME dts = this->GetPacketDts(&ffmpegPacket);

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

              this->flags &= ~STANDARD_DEMUXER_FLAG_VC1_SEEN_TIMESTAMP;
              this->flags |= (pts != COutputPinPacket::INVALID_TIME) ? STANDARD_DEMUXER_FLAG_VC1_SEEN_TIMESTAMP : DEMUXER_FLAG_NONE;
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

        if (activeStream != NULL)
        {
          if (packet->IsDiscontinuity() || ((ffmpegPacket.flags & AV_PKT_FLAG_CORRUPT) != 0) || activeStream->IsDiscontinuity())
          {
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: demuxer: %u, stream: %d, discontinuity, packet discontinuity: %u, FFmpeg packet corrupt: %u, stream discontinuity: %u", MODULE_NAME, METHOD_GET_NEXT_PACKET_INTERNAL_NAME, this->demuxerId, packet->GetStreamPid(), packet->IsDiscontinuity() ? 1 : 0, ((ffmpegPacket.flags & AV_PKT_FLAG_CORRUPT) != 0) ? 1 : 0, activeStream->IsDiscontinuity() ? 1 : 0);
          }

          packet->SetDiscontinuity(packet->IsDiscontinuity() || ((ffmpegPacket.flags & AV_PKT_FLAG_CORRUPT) != 0) || activeStream->IsDiscontinuity());
          activeStream->SetDiscontinuity(false);
        }
        else
        {
          if (packet->IsDiscontinuity() || ((ffmpegPacket.flags & AV_PKT_FLAG_CORRUPT) != 0))
          {
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: demuxer: %u, stream: %d, discontinuity, packet discontinuity: %u, FFmpeg packet corrupt: %u", MODULE_NAME, METHOD_GET_NEXT_PACKET_INTERNAL_NAME, this->demuxerId, packet->GetStreamPid(), packet->IsDiscontinuity() ? 1 : 0, ((ffmpegPacket.flags & AV_PKT_FLAG_CORRUPT) != 0) ? 1 : 0);
          }

          packet->SetDiscontinuity(packet->IsDiscontinuity() || ((ffmpegPacket.flags & AV_PKT_FLAG_CORRUPT) != 0));
        }

        //#ifdef DEBUG
        //        if (pkt.flags & AV_PKT_FLAG_CORRUPT)
        //          DbgLog((LOG_TRACE, 10, L"::GetNextPacket() - Signaling Discontinuinty because of corrupt package"));
        //#endif

        //if (packet->GetStartTime() != AV_NOPTS_VALUE)
        //{
        //  //m_rtCurrent = packet->GetStartTime();
        //}
      }

      // check if stream is building seeking index
      if (SUCCEEDED(result) && (activeStream != NULL) && (ffmpegPacket.pts != AV_NOPTS_VALUE) && (ffmpegPacket.pos >= 0) && (stream->nb_index_entries == 0))
      {
        // stream doesn't create seek index
        // create our own seek index

        // 1 second minimum differrence between seek index entries
        int64_t minDiff = ConvertRTToTimestamp(DSHOW_TIME_BASE, stream->time_base.num, stream->time_base.den, 0);

        if ((activeStream->GetSeekIndexEntries()->Count() == 0) || 
          (ffmpegPacket.pts > (minDiff + activeStream->GetSeekIndexEntries()->GetItem(activeStream->GetSeekIndexEntries()->Count() - 1)->GetTimestamp())))
        {
          // update seeking index

          result = activeStream->GetSeekIndexEntries()->AddSeekIndexEntry(ffmpegPacket.pos, ffmpegPacket.pts);
          if (result == E_SEEK_INDEX_ENTRY_EXISTS)
          {
            result = S_OK;
          }
        }
      }

      av_free_packet(&ffmpegPacket);
    }
  }

  return result;
}

REFERENCE_TIME CStandardDemuxer::ConvertTimestampToRT(int64_t pts, int num, int den, int64_t starttime)
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

int64_t CStandardDemuxer::ConvertRTToTimestamp(REFERENCE_TIME timestamp, int num, int den, int64_t starttime)
{
  if (timestamp == COutputPinPacket::INVALID_TIME)
  {
    return (int64_t)AV_NOPTS_VALUE;
  }

  if (starttime == AV_NOPTS_VALUE)
  {
    starttime = (this->formatContext->start_time != AV_NOPTS_VALUE) ? av_rescale(this->formatContext->start_time, den, (int64_t)AV_TIME_BASE * num) : 0;
  }

  int64_t pts = av_rescale(timestamp, den, (int64_t)num * DSHOW_TIME_BASE);
  if (starttime != 0)
  {
    pts += starttime;
  }

  return pts;
}

void CStandardDemuxer::CleanupFormatContext(void)
{
  if (this->formatContext)
  {
    avformat_close_input(&this->formatContext);
    this->formatContext = NULL;
  }
}

bool CStandardDemuxer::IsFlv(void)
{
  return this->IsSetFlags(STANDARD_DEMUXER_FLAG_FLV);
}

bool CStandardDemuxer::IsAsf(void)
{
  return this->IsSetFlags(STANDARD_DEMUXER_FLAG_ASF);
}

bool CStandardDemuxer::IsMp4(void)
{
  return this->IsSetFlags(STANDARD_DEMUXER_FLAG_MP4);
}

bool CStandardDemuxer::IsMatroska(void)
{
  return this->IsSetFlags(STANDARD_DEMUXER_FLAG_MATROSKA);
}

bool CStandardDemuxer::IsOgg(void)
{
  return this->IsSetFlags(STANDARD_DEMUXER_FLAG_OGG);
}

bool CStandardDemuxer::IsAvi(void)
{
  return this->IsSetFlags(STANDARD_DEMUXER_FLAG_AVI);
}

bool CStandardDemuxer::IsMpegTs(void)
{
  return this->IsSetFlags(STANDARD_DEMUXER_FLAG_MPEG_TS);
}

bool CStandardDemuxer::IsMpegPs(void)
{
  return this->IsSetFlags(STANDARD_DEMUXER_FLAG_MPEG_PS);
}

bool CStandardDemuxer::IsRm(void)
{
  return this->IsSetFlags(STANDARD_DEMUXER_FLAG_RM);
}

bool CStandardDemuxer::IsVc1SeenTimestamp(void)
{
  return this->IsSetFlags(STANDARD_DEMUXER_FLAG_VC1_SEEN_TIMESTAMP);
}

bool CStandardDemuxer::IsVc1Correction(void)
{
  return this->IsSetFlags(STANDARD_DEMUXER_FLAG_VC1_CORRECTION);
}

// rewritten ff_gen_search from FFmpeg
int64_t BinaryTimestampSearch(AVFormatContext *formatContext, int streamIndex, int64_t targetTimestamp, int64_t *foundTimestamp, int flags)
{
  int64_t wrap = formatContext->streams[streamIndex]->pts_wrap_bits > 0 && formatContext->streams[streamIndex]->pts_wrap_bits < 63 ? 1LL << formatContext->streams[streamIndex]->pts_wrap_bits : 0;

  int64_t minPosition = formatContext->data_offset;
  int64_t minTimestamp = formatContext->iformat->read_timestamp(formatContext, streamIndex, &minPosition, INT64_MAX);

  if (minTimestamp == AV_NOPTS_VALUE)
  {
    return E_NOT_FOUND_MINIMUM_TIMESTAMP;
  }

  if (minTimestamp >= targetTimestamp)
  {
    *foundTimestamp = minTimestamp;
    return minPosition;
  }

  int64_t step = 1024;
  int64_t streamSize = avio_size(formatContext->pb);
  int64_t maxPosition = streamSize - 1;
  int64_t maxTimestamp = AV_NOPTS_VALUE;

  do
  {
    maxPosition -= step;
    maxTimestamp = formatContext->iformat->read_timestamp(formatContext, streamIndex, &maxPosition, maxPosition + step);
    step += step;
  }
  while ((maxTimestamp == AV_NOPTS_VALUE) && (maxPosition >= step));

  if (maxTimestamp == AV_NOPTS_VALUE)
  {
    return E_NOT_FOUND_MAXIMUM_TIMESTAMP;
  }

  while (true)
  {
    int64_t tempPosition = maxPosition + 1;
    int64_t tempTimestamp = formatContext->iformat->read_timestamp(formatContext, streamIndex, &tempPosition, INT64_MAX);

    if (tempTimestamp == AV_NOPTS_VALUE)
    {
      break;
    }

    maxTimestamp = tempTimestamp;
    maxPosition = tempPosition;

    if (tempPosition >= streamSize)
    {
      break;
    }
  }

  int64_t positionLimit = maxPosition;

  if (((maxTimestamp > minTimestamp) && (maxTimestamp <= targetTimestamp)) || ((maxTimestamp < minTimestamp) && ((maxTimestamp + wrap) <= targetTimestamp)))
  {
    *foundTimestamp = maxTimestamp;
    return maxPosition;
  }

  if (minTimestamp > maxTimestamp)
  {
    maxTimestamp += wrap;

    if (minTimestamp > maxTimestamp)
    {
      return E_MINIMUM_TIMESTAMP_GREATER_THAN_MAXIMUM_TIMESTAMP;
    }
  }
  else if (minTimestamp == maxTimestamp)
  {
    positionLimit = minPosition;
  }

  unsigned int noChange = 0;
  int64_t position = 0;

  while (minPosition < positionLimit)
  {
    if (positionLimit > maxPosition)
    {
      return E_POSITION_LIMIT_OVER_MAXIMUM_POSITION;
    }

    if (noChange == 0)
    {
      int64_t approximateKeyframeDistance = maxPosition - positionLimit;
      // interpolate position (better than dichotomy)
      position = av_rescale(targetTimestamp - minTimestamp, maxPosition - minPosition, maxTimestamp - minTimestamp)
        + minPosition - approximateKeyframeDistance;
    }
    else if (noChange == 1)
    {
      // bisection, if interpolation failed to change min or max position last time
      position = (minPosition + positionLimit) >> 1;
    }
    else
    {
      // linear search if bisection failed, can only happen if there are very few or no keyframes between min/max position
      position = minPosition;
    }

    if (position <= minPosition)
    {
      position = minPosition + 1;
    }
    else if (position > positionLimit)
    {
      position = positionLimit;
    }

    int64_t startPosition = position;
    int64_t timestamp = formatContext->iformat->read_timestamp(formatContext, streamIndex, &position, INT64_MAX); //may pass pos_limit instead of -1

    if ((timestamp != AV_NOPTS_VALUE) && (timestamp < minTimestamp))
    {
      timestamp += wrap;
    }

    if (position == maxPosition)
    {
      noChange++;
    }
    else
    {
      noChange = 0;
    }

    if (timestamp == AV_NOPTS_VALUE)
    {
      return E_NOT_FOUND_TIMESTAMP;
    }

    if (targetTimestamp <= timestamp)
    {
      positionLimit = startPosition - 1;
      maxPosition = position;
      maxTimestamp = timestamp;
    }

    if (targetTimestamp >= timestamp)
    {
      minPosition = position;
      minTimestamp = timestamp;
    }
  }

  *foundTimestamp = (flags & AVSEEK_FLAG_BACKWARD) ? minTimestamp : maxTimestamp;
  return (flags & AVSEEK_FLAG_BACKWARD) ? minPosition : maxPosition;
}

HRESULT CStandardDemuxer::SeekByPosition(REFERENCE_TIME time, int flags)
{
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId);

  HRESULT result = S_OK;
  int64_t seek_pts = time;

  // seek preference :
  // 1. in case of container
  //    a. active video stream
  //    b. any video stream
  //    c. active audio stream
  //    d. any audio stream
  //    e. active subtitle stream
  //    f. any subtitle stream
  // 2. in case of packet stream !!! this case is not tested !!!
  //    in that case is assumed that there is only one stream in all groups (video, audio, subtitle)

  int streamId = -1;
  for (unsigned int i = 0; ((streamId == (-1)) && (i < CStream::Unknown)); i++)
  {
    // stream groups are in order: video, audio, subtitle = in our preference
    if (this->GetStreams((CStream::StreamType)i)->Count() > 0)
    {
      streamId = this->GetStreams((CStream::StreamType)i)->GetItem((this->activeStream[(CStream::StreamType)i] == ACTIVE_STREAM_NOT_SPECIFIED) ? 0 : this->activeStream[(CStream::StreamType)i])->GetPid();
    }
  }

  if (streamId == (-1))
  {
    this->logger->Log(LOGGER_ERROR, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, L"no stream to seek");
    result = E_NO_STREAM_TO_SEEK;
  }

  if ((time >= 0) && (streamId != (-1)))
  {
    AVStream *stream = this->formatContext->streams[streamId];
    seek_pts = ConvertRTToTimestamp(time, stream->time_base.num, stream->time_base.den, (int64_t)AV_NOPTS_VALUE);
  }

  bool found = false;

  // enable reading from seek method, do not allow (yet) to read from demuxing worker
  this->filter->SetPauseSeekStopMode(PAUSE_SEEK_STOP_MODE_DISABLE_DEMUXING);
  this->flags |= DEMUXER_FLAG_DISABLE_DEMUXING_WITH_RETURN_TO_DEMUXING_WORKER;
  this->flags &= ~DEMUXER_FLAG_DISABLE_READING;
  // wait until DemuxingWorker() confirm
  while (!this->IsSetFlags(DEMUXER_FLAG_DISABLE_DEMUXING))
  {
    Sleep(1);
  }

  // if it isn't FLV video, try to seek by internal FFmpeg time seeking method
  if (SUCCEEDED(result) && (!this->IsFlv()) && (!this->IsMpegTs()))
  {
    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, time: %lld, seek_pts: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, time, seek_pts);

    if (this->formatContext->iformat->read_seek)
    {
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking by internal format time seeking method", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId);
      ff_read_frame_flush(this->formatContext);
      result = (HRESULT)this->formatContext->iformat->read_seek(this->formatContext, streamId, seek_pts, flags);
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking by internal format time seeking method result: 0x%08X", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, result);

      found = true;
    } 
  }

  if (SUCCEEDED(result) && (!found))
  {
    AVStream *st = this->formatContext->streams[streamId];

    ff_read_frame_flush(this->formatContext);
    int index = av_index_search_timestamp(st, seek_pts, flags);

    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, index: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, index);

    if ((index < 0) && (st->nb_index_entries) && (seek_pts < st->index_entries[0].timestamp))
    {
      this->logger->Log(LOGGER_ERROR, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, L"failing");
      result = E_NOT_FOUND_SEEK_INDEX_ENTRY;
    }

    if (SUCCEEDED(result) && (index >= 0))
    {
      AVIndexEntry *ie = &st->index_entries[index];
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, timestamp: %lld, seek_pts: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, ie->timestamp, seek_pts);
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

      // check specific container formats
      if (this->IsFlv())
      {
        // FLV container format

        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, index entries: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, st->nb_index_entries);
        AVPacket avPacket;
        AVIndexEntry *ie = NULL;

        if ((st->nb_index_entries) && (index >= 0))
        {
          ie = &st->index_entries[index];

          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking to position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, ie->pos);
          int64_t res = avio_seek(this->formatContext->pb, ie->pos, SEEK_SET);
          if (res < 0)
          {
            result = (HRESULT)res;
            this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, avio_seek error: 0x%08X", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, result);
          }

          if (SUCCEEDED(result))
          {
            ff_update_cur_dts(this->formatContext, st, ie->timestamp);
          }
        }
        else
        {
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking to position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, this->formatContext->data_offset);
          int64_t res = avio_seek(this->formatContext->pb, this->formatContext->data_offset, SEEK_SET);
          if (res < 0)
          {
            result = (HRESULT)res;
            this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, avio_seek error: 0x%08X", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, result);
          }
        }

        if (SUCCEEDED(result))
        {
          if (ie != NULL)
          {
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, index timestamp: %lld, index position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, ie->timestamp, ie->pos);
          }

          CFlvPacket *flvPacket = new CFlvPacket(&result);
          ALLOC_MEM_DEFINE_SET(buffer, unsigned char, FLV_SEEKING_TOTAL_BUFFER_SIZE, 0);
          ALLOC_MEM_DEFINE_SET(flvSeekBoundaries, FlvSeekPosition, FLV_BOUNDARIES_COUNT, 0);

          CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);
          CHECK_POINTER_HRESULT(result, flvSeekBoundaries, result, E_OUTOFMEMORY);
          CHECK_POINTER_HRESULT(result, flvPacket, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            flvSeekBoundaries[FLV_SEEK_LOWER_BOUNDARY].time = 0;
            flvSeekBoundaries[FLV_SEEK_LOWER_BOUNDARY].position = 0;

            flvSeekBoundaries[FLV_SEEK_UPPER_BOUNDARY].time = ConvertRTToTimestamp(this->GetDuration(), st->time_base.num, st->time_base.den, (int64_t)AV_NOPTS_VALUE);
            flvSeekBoundaries[FLV_SEEK_UPPER_BOUNDARY].position = this->DemuxerSeek(this, 0, AVSEEK_SIZE);

            CHECK_CONDITION_HRESULT(result, flvSeekBoundaries[FLV_SEEK_UPPER_BOUNDARY].position >= 0, result, (HRESULT)flvSeekBoundaries[FLV_SEEK_UPPER_BOUNDARY].position);
          }

          if (SUCCEEDED(result))
          {
            int64_t seekPosition = avio_seek(this->formatContext->pb, 0, SEEK_CUR);

            // read stream until we find requested time
            while ((!found) && SUCCEEDED(result))
            {
              unsigned int bufferPosition = 0;
              int64_t flvPacketOffset = -1;

              // synchronize within stream to first FLV packet after seekPosition
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking to position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, seekPosition);
              int64_t res = avio_seek(this->formatContext->pb, seekPosition, SEEK_SET);

              // we can't do avio_seek because we need data from exact position
              // we need to call our seek
              this->formatContext->pb->seek(this->formatContext->pb->opaque, seekPosition, SEEK_SET);

              while (SUCCEEDED(result) && (bufferPosition < FLV_SEEKING_TOTAL_BUFFER_SIZE))
              {
                flvPacket->Clear();

                // we can't read data with avio_read, because it uses internal FLV format methods to parse data
                // because in most cases we seek to non-FLV packet (but somewhere else) we need to use our method to read data
                int readBytes = this->formatContext->pb->read_packet(this->formatContext->pb->opaque, buffer + bufferPosition, min(FLV_SEEKING_TOTAL_BUFFER_SIZE - bufferPosition, FLV_SEEKING_BUFFER_SIZE));
                if (readBytes <= 0)
                {
                  result = (HRESULT)readBytes;
                  this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, avio_read() returned error: 0x%08X", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, result);
                  break;
                }
                else
                {
                  bufferPosition += (unsigned int)readBytes;
                  int flvFindResult = flvPacket->FindPacket(buffer, bufferPosition, FLV_PACKET_MINIMUM_CHECKED);

                  if (flvFindResult < 0)
                  {
                    // some kind of error occurred, in all cases we try to read more data
                    switch (flvFindResult)
                    {
                    case FLV_FIND_RESULT_NOT_FOUND:
                      // no FLV packet or candidate found
                      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, no FLV packet or candidate found in data, length: %u", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, bufferPosition);
                      break;
                    case FLV_FIND_RESULT_NOT_ENOUGH_DATA_FOR_HEADER:
                      // not enough data for FLV header in buffer
                      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, not enough data for FLV header, length: %u", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, bufferPosition);
                      break;
                    case FLV_FIND_RESULT_NOT_ENOUGH_MEMORY:
                      // this should not happen
                      result = E_OUTOFMEMORY;
                      break;
                    case FLV_FIND_RESULT_NOT_FOUND_MINIMUM_PACKETS:
                      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, not found minimum FLV packets, length: %u", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, bufferPosition);
                      // found several FLV packets, but lower than requested
                      break;
                    }
                  }
                  else
                  {
                    // found at least requested count of FLV packets, the position of first is in flvFindResult
                    // seek to specified position, read avPacket and check dts
                    flvPacketOffset = (unsigned int)flvFindResult;
                    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, found FLV packet at position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, seekPosition + flvPacketOffset);
                    break;
                  }
                }
              }
              CHECK_CONDITION_EXECUTE(flvPacketOffset == (-1), result = E_NOT_FOUND_ANY_FLV_PACKET);

              // seek back to position where we started
              this->formatContext->pb->seek(this->formatContext->pb->opaque, seekPosition, SEEK_SET);
              seekPosition += flvPacketOffset;

              if (SUCCEEDED(result))
              {
                // seek to specified position, read avPacket and check dts
                res = avio_seek(this->formatContext->pb, seekPosition, SEEK_SET);

                while (SUCCEEDED(result))
                {
                  int avResult = 0;
                  do
                  {
                    if ((avResult == E_CONNECTION_LOST_TRYING_REOPEN) || this->IsSetFlags(DEMUXER_FLAG_PENDING_DISCONTINUITY))
                    {
                      this->flags &= ~DEMUXER_FLAG_PENDING_DISCONTINUITY;
                    }

                    avResult = av_read_frame(this->formatContext, &avPacket);

                    // assume we are not eof
                    CHECK_CONDITION_EXECUTE(this->formatContext->pb != NULL, this->formatContext->pb->eof_reached = 0);

                    if (avResult == E_CONNECTION_LOST_TRYING_REOPEN)
                    {
                      ff_read_frame_flush(this->formatContext);
                    }

                    if ((avResult == AVERROR(EAGAIN)) || (avResult == E_CONNECTION_LOST_TRYING_REOPEN))
                    {
                      Sleep(1);
                    }
                  } while ((avResult == AVERROR(EAGAIN)) || (avResult == E_CONNECTION_LOST_TRYING_REOPEN));

                  CHECK_CONDITION_EXECUTE(avResult < 0, result = (HRESULT)avResult);
                  av_free_packet(&avPacket);

                  if ((streamId != avPacket.stream_index) || (avPacket.dts < 0))
                  {
                    // continue reading next avPacket, because we don't have avPacket with right stream index or avPacket doesn't have dts
                    continue;
                  }

                  // check avPacket dts and compare it with seek_pts
                  // if necessary, adjust seeking boundaries
                  if (avPacket.dts < seek_pts)
                  {
                    if (avPacket.dts >= flvSeekBoundaries[FLV_SEEK_LOWER_BOUNDARY].time)
                    {
                      flvSeekBoundaries[FLV_SEEK_LOWER_BOUNDARY].time = avPacket.dts;
                      flvSeekBoundaries[FLV_SEEK_LOWER_BOUNDARY].position = seekPosition;
                    }
                  }
                  else if (avPacket.dts == seek_pts)
                  {
                    flvSeekBoundaries[FLV_SEEK_LOWER_BOUNDARY].time = avPacket.dts;
                    flvSeekBoundaries[FLV_SEEK_LOWER_BOUNDARY].position = seekPosition;

                    flvSeekBoundaries[FLV_SEEK_UPPER_BOUNDARY].time = avPacket.dts;
                    flvSeekBoundaries[FLV_SEEK_UPPER_BOUNDARY].position = seekPosition;

                    found = true;
                  }
                  else
                  {
                    if (avPacket.dts <= flvSeekBoundaries[FLV_SEEK_UPPER_BOUNDARY].time)
                    {
                      flvSeekBoundaries[FLV_SEEK_UPPER_BOUNDARY].time = avPacket.dts;
                      flvSeekBoundaries[FLV_SEEK_UPPER_BOUNDARY].position = seekPosition;
                    }
                  }

                  break;
                }
              }

              if (SUCCEEDED(result) && (!found))
              {
                int64_t diff = ConvertRTToTimestamp(FLV_NO_SEEK_DIFFERENCE_TIME, st->time_base.num, st->time_base.den, 0);

                if ((flvSeekBoundaries[FLV_SEEK_LOWER_BOUNDARY].time + diff) > seek_pts)
                {
                  found = true;
                }
              }

              if (SUCCEEDED(result) && (!found))
              {
                // we still don't have appropriate FLV packet
                // guess new seek position

                seekPosition = (flvSeekBoundaries[FLV_SEEK_UPPER_BOUNDARY].position - flvSeekBoundaries[FLV_SEEK_LOWER_BOUNDARY].position) / 2 + flvSeekBoundaries[FLV_SEEK_LOWER_BOUNDARY].position;
              }

              if (SUCCEEDED(result) && (found))
              {
                // we got appropriate FLV packet
                // seek to that position

                avio_seek(this->formatContext->pb, flvSeekBoundaries[FLV_SEEK_LOWER_BOUNDARY].position, SEEK_SET);
              }
            }
          }

          FREE_MEM(buffer);
          FREE_MEM(flvSeekBoundaries);
          FREE_MEM_CLASS(flvPacket);
        }
      }
      else if (this->IsMpegTs())
      {
        // MPEG2 TS container format
        ff_read_frame_flush(this->formatContext);

        int64_t foundTimestamp = AV_NOPTS_VALUE;
        int64_t seekPosition = BinaryTimestampSearch(this->formatContext, streamId, seek_pts, &foundTimestamp, flags);

        if (seekPosition < 0)
        {
          result = HRESULT(seekPosition);
        }

        if (SUCCEEDED(result))
        {
          found = true;

          // do the seek

          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking to position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, seekPosition);
          int64_t ret = avio_seek(this->formatContext->pb, seekPosition, SEEK_SET);
          if (ret < 0)
          {
            result = (HRESULT)ret;
            this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, avio_seek error: 0x%08X", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, result);
          }

          if (SUCCEEDED(result))
          {
            ff_read_frame_flush(this->formatContext);
            ff_update_cur_dts(this->formatContext, st, foundTimestamp);
          }
        }
      }
      else
      {
        // not specified container format
        // use generic seeking method

        if ((index < 0) || (index == (st->nb_index_entries - 1)))
        {
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, index entries: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, st->nb_index_entries);
          AVIndexEntry *ie = NULL;

          if ((st->nb_index_entries) && (index >= 0))
          {
            ie = &st->index_entries[index];

            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking to position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, ie->pos);
            int64_t res = avio_seek(this->formatContext->pb, ie->pos, SEEK_SET);
            if (res < 0)
            {
              result = (HRESULT)res;
              this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, avio_seek error: 0x%08X", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, result);
            }

            if (SUCCEEDED(result))
            {
              ff_update_cur_dts(this->formatContext, st, ie->timestamp);
            }
          }
          else
          {
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking to position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, this->formatContext->data_offset);
            int64_t res = avio_seek(this->formatContext->pb, this->formatContext->data_offset, SEEK_SET);
            if (res < 0)
            {
              result = (HRESULT)res;
              this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, avio_seek error: 0x%08X", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, result);
            }
          }

          found = true;
        }
        else
        {
          found = true;
        }
      }
    }
  }

  if (SUCCEEDED(result) && found)
  {
    // we must clean end of stream flag, to restart demuxing
    // in another case, demuxing will not work and we don't have any video or audio
    this->flags &= ~DEMUXER_FLAG_END_OF_STREAM_OUTPUT_PACKET_QUEUED;

    {
      // lock access to media packets and output packets
      CLockMutex outputPacketLock(this->outputPacketMutex, INFINITE);

      // clear output packets
      this->outputPacketCollection->Clear();
    }
  }

  this->logger->Log(LOGGER_INFO, SUCCEEDED(result) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, result);
  return result;
}

HRESULT CStandardDemuxer::SeekByTime(REFERENCE_TIME time, int flags)
{
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId);

  HRESULT result = S_OK;

  AVStream *st = NULL;
  AVIndexEntry *ie = NULL;  
  int64_t seek_pts = time;

  // seek preference :
  // 1. in case of container
  //    a. active video stream
  //    b. any video stream
  //    c. active audio stream
  //    d. any audio stream
  //    e. active subtitle stream
  //    f. any subtitle stream
  // 2. in case of packet stream
  //    in that case is assumed that there is only one stream in all groups (video, audio, subtitle)

  int streamId = -1;

  for (unsigned int i = 0; ((streamId == (-1)) && (i < CStream::Unknown)); i++)
  {
    // stream groups are in order: video, audio, subtitle = in our preference
    if (this->GetStreams((CStream::StreamType)i)->Count() > 0)
    {
      CStream *activeStream = this->GetStreams((CStream::StreamType)i)->GetItem((this->activeStream[(CStream::StreamType)i] == ACTIVE_STREAM_NOT_SPECIFIED) ? 0 : this->activeStream[(CStream::StreamType)i]);
      streamId = activeStream->GetPid();
    }
  }

  if (streamId == (-1))
  {
    this->logger->Log(LOGGER_ERROR, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, L"no stream to seek");
    result = E_NO_STREAM_TO_SEEK;
  }

  if ((time >= 0) && (streamId != (-1)))
  {
    AVStream *stream = this->formatContext->streams[streamId];
    seek_pts = ConvertRTToTimestamp(time, stream->time_base.num, stream->time_base.den, (int64_t)AV_NOPTS_VALUE);
  }

  if (SUCCEEDED(result))
  {
    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, time: %lld, seek_pts: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, time, seek_pts);

    this->flags |= DEMUXER_FLAG_DISABLE_DEMUXING_WITH_RETURN_TO_DEMUXING_WORKER;
    this->flags &= ~DEMUXER_FLAG_DISABLE_READING;
    // wait until DemuxingWorker() confirm
    while (!this->IsSetFlags(DEMUXER_FLAG_DISABLE_DEMUXING))
    {
      Sleep(1);
    }

    st = this->formatContext->streams[streamId];

    bool found = false;
    ff_read_frame_flush(this->formatContext);

    st->nb_index_entries = 0;
    st->nb_frames = 0;

    // seek to time
    int64_t seekedTime = this->filter->SeekToTime(this->demuxerId, time / (DSHOW_TIME_BASE / 1000)); // (1000 / DSHOW_TIME_BASE)

    // enable reading from seek method, do not allow (yet) to read from demuxing worker
    this->filter->SetPauseSeekStopMode(PAUSE_SEEK_STOP_MODE_DISABLE_DEMUXING);

    if (seekedTime >= 0)
    {
      // lock access to output packets
      CLockMutex outputPacketLock(this->outputPacketMutex, INFINITE);

      // clear output packets
      this->outputPacketCollection->Clear();

      // set buffer position to zero
      this->demuxerContextBufferPosition = 0;
      this->flags &= ~DEMUXER_FLAG_END_OF_STREAM_OUTPUT_PACKET_QUEUED;

      // clear our seeking entries
      for (unsigned int i = 0; (i < CStream::Unknown); i++)
      {
        for (unsigned int j = 0; j < this->GetStreams((CStream::StreamType)i)->Count(); j++)
        {
          CStream *stream = this->GetStreams((CStream::StreamType)i)->GetItem(j);

          stream->GetSeekIndexEntries()->Clear();
        }
      }
    }
    else
    {
      result = (HRESULT)seekedTime;
    }

    // now we are ready to receive data

    if (SUCCEEDED(result) && (this->IsAsf()))
    {
      found = true;
      asf_reset_header2(this->formatContext);
    }

    if (SUCCEEDED(result) && (this->IsMp4()))
    {
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking by internal MP4 format time seeking method", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId);
      ff_read_frame_flush(this->formatContext);

      result = (HRESULT)this->formatContext->iformat->read_seek(this->formatContext, streamId, seek_pts, flags);
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking by internal format time seeking method result: 0x%08X", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, result);

      // if ret is not 0, then error and seek failed
      found = (result == S_OK);
    }

    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeked to time: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, seekedTime));

    ff_read_frame_flush(this->formatContext);

    if (SUCCEEDED(result) && (!found))
    {
      st = this->formatContext->streams[streamId];
      ff_read_frame_flush(this->formatContext);
      int index = av_index_search_timestamp(st, seek_pts, flags);

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, index: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, index);

      if ((index < 0) && (st->nb_index_entries > 0) && (seek_pts < st->index_entries[0].timestamp))
      {
        this->logger->Log(LOGGER_ERROR, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, L"failing");
        result = E_NOT_FOUND_SEEK_INDEX_ENTRY;
      }

      if (SUCCEEDED(result) && (index >= 0) && (st->nb_index_entries > 0))
      {
        ie = &st->index_entries[index];
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, timestamp: %lld, seek_pts: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, ie->timestamp, seek_pts);
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
        if ((index < 0) || (index == (st->nb_index_entries - 1)))
        {
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, index entries: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, st->nb_index_entries);
          AVPacket avPacket;

          if ((st->nb_index_entries) && (index >= 0))
          {
            ie = &st->index_entries[index];

            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking to position: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, ie->pos);
            int64_t res = avio_seek(this->formatContext->pb, ie->pos, SEEK_SET);
            if (res < 0)
            {
              result = (HRESULT)res;
              this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, avio_seek error: 0x%08X", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, result);
            }

            if (SUCCEEDED(result))
            {
              ff_update_cur_dts(this->formatContext, st, ie->timestamp);
            }
          }
          else
          {
            // seek to zero (after seeking byte position is set to zero)
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, flushing, seeking to position: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, 0);
            avio_flush(this->formatContext->pb);
            int64_t res = avio_seek(this->formatContext->pb, 0, SEEK_SET);
            if (res < 0)
            {
              result = (HRESULT)res;
              this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, avio_seek error: 0x%08X", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, result);
            }
          }

          if (SUCCEEDED(result))
          {
            if (ie != NULL)
            {
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, index timestamp: %lld, index position: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, ie->timestamp, ie->pos);
            }
            int nonkey = 0;

            // read stream until we find requested time
            while ((!found) && SUCCEEDED(result))
            {
              int avResult = 0;
              do
              {
                if ((avResult == E_CONNECTION_LOST_TRYING_REOPEN) || this->IsSetFlags(DEMUXER_FLAG_PENDING_DISCONTINUITY))
                {
                  this->flags &= ~DEMUXER_FLAG_PENDING_DISCONTINUITY;
                }

                avResult = av_read_frame(this->formatContext, &avPacket);

                // assume we are not eof
                CHECK_CONDITION_EXECUTE(this->formatContext->pb != NULL, this->formatContext->pb->eof_reached = 0);

                if (avResult == E_CONNECTION_LOST_TRYING_REOPEN)
                {
                  ff_read_frame_flush(this->formatContext);
                }

                if ((avResult == AVERROR(EAGAIN)) || (avResult == E_CONNECTION_LOST_TRYING_REOPEN))
                {
                  Sleep(1);
                }
              } while ((avResult == AVERROR(EAGAIN)) || (avResult == E_CONNECTION_LOST_TRYING_REOPEN));

              if (avResult < 0)
              {
                // error occured
                result = (HRESULT)avResult;
                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, av_read_frame() returned error: 0x%08X", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, result);
                break;
              }

              av_free_packet(&avPacket);

              if (streamId == avPacket.stream_index)
              {
                found = true;
                break;
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
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, searching keyframe with timestamp: %lld, stream index: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, seek_pts, streamId);
      int index = av_index_search_timestamp(st, seek_pts, flags);

      if (index < 0)
      {
        this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, index lower than zero: %d, setting to zero", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, index);
        index = 0;
      }

      if (SUCCEEDED(result))
      {
        ie = &st->index_entries[index];

        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seek to position: %lld, time: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, ie->pos, ie->timestamp);

        int64_t ret = avio_seek(this->formatContext->pb, ie->pos, SEEK_SET);
        if (ret < 0)
        {
          result = (HRESULT)ret;
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seek to requested position %lld failed: 0x%08X", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, ie->pos, result);
        }

        if (SUCCEEDED(result))
        {
          ff_update_cur_dts(this->formatContext, st, ie->timestamp);
        }
      }
    }
  }

  this->logger->Log(LOGGER_INFO, SUCCEEDED(result) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId, result);
  return result;
}

HRESULT CStandardDemuxer::SeekBySequenceReading(REFERENCE_TIME time, int flags)
{
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, this->demuxerId);

  HRESULT result = S_OK;
  int64_t seek_pts = time;

  // seek preference :
  // 1. in case of container (DEMUXER_FLAG_STREAM_IN_CONTAINER)
  //    a. active video stream
  //    b. any video stream
  //    c. active audio stream
  //    d. any audio stream
  //    e. active subtitle stream
  //    f. any subtitle stream
  // 2. in case of packet stream (DEMUXER_FLAG_STREAM_IN_PACKETS) - !!! this case is not tested !!!
  //    in that case is assumed that there is only one stream in all groups (video, audio, subtitle)

  int streamId = -1;
  CStream *activeStream = NULL;

  for (unsigned int i = 0; ((streamId == (-1)) && (i < CStream::Unknown)); i++)
  {
    // stream groups are in order: video, audio, subtitle = in our preference
    if (this->GetStreams((CStream::StreamType)i)->Count() > 0)
    {
      activeStream = this->GetStreams((CStream::StreamType)i)->GetItem((this->activeStream[(CStream::StreamType)i] == ACTIVE_STREAM_NOT_SPECIFIED) ? 0 : this->activeStream[(CStream::StreamType)i]);
      streamId = activeStream->GetPid();
    }
  }

  if (streamId == (-1))
  {
    this->logger->Log(LOGGER_ERROR, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, L"no stream to seek");
    result = E_NO_STREAM_TO_SEEK;
  }

  if ((time >= 0) && (streamId != (-1)))
  {
    AVStream *stream = this->formatContext->streams[streamId];
    seek_pts = ConvertRTToTimestamp(time, stream->time_base.num, stream->time_base.den, (int64_t)AV_NOPTS_VALUE);
  }

  bool found = false;

  if (SUCCEEDED(result))
  {
    AVPacket avPacket;

    // enable reading from seek method, do not allow (yet) to read from demuxing worker
    // we must read after last demuxed packet
    this->filter->SetPauseSeekStopMode(PAUSE_SEEK_STOP_MODE_DISABLE_DEMUXING);
    this->flags |= DEMUXER_FLAG_DISABLE_DEMUXING_WITH_SAFE_RETURN_TO_DEMUXING_WORKER;
    this->flags &= ~DEMUXER_FLAG_DISABLE_READING;
    // wait until DemuxingWorker() confirm
    while (!this->IsSetFlags(DEMUXER_FLAG_DISABLE_DEMUXING))
    {
      Sleep(1);
    }

    while (SUCCEEDED(result))
    {
      int avResult = 0;
      do
      {
        if ((avResult == E_CONNECTION_LOST_TRYING_REOPEN) || this->IsSetFlags(DEMUXER_FLAG_PENDING_DISCONTINUITY))
        {
          this->flags &= ~DEMUXER_FLAG_PENDING_DISCONTINUITY;
        }

        avResult = av_read_frame(this->formatContext, &avPacket);

        // assume we are not eof
        CHECK_CONDITION_EXECUTE(this->formatContext->pb != NULL, this->formatContext->pb->eof_reached = 0);

        if (avResult == E_CONNECTION_LOST_TRYING_REOPEN)
        {
          ff_read_frame_flush(this->formatContext);
        }

        if ((avResult == AVERROR(EAGAIN)) || (avResult == E_CONNECTION_LOST_TRYING_REOPEN))
        {
          Sleep(1);
        }
      } while ((avResult == AVERROR(EAGAIN)) || (avResult == E_CONNECTION_LOST_TRYING_REOPEN));

      CHECK_CONDITION_EXECUTE(avResult < 0, result = (HRESULT)avResult);
      av_free_packet(&avPacket);

      if ((streamId != avPacket.stream_index) || (avPacket.dts < 0))
      {
        // continue reading next avPacket, because we don't have avPacket with right stream index or avPacket doesn't have dts
        continue;
      }

      break;
    }

    // in avPacket we have current dts
    // compare it with seek_pts 
    // then decide to seek to start of stream (or index if better found) or read continuosly until seek_pts

    if (avPacket.dts > seek_pts)
    {
      // seek to start of stream or to index (if better found)

      AVStream *st = this->formatContext->streams[streamId];
      int64_t seekTimestamp = st->first_dts;
      int64_t seekPosition = this->formatContext->data_offset;

      if (st->nb_index_entries != 0)
      {
        // check FFmpeg seek index entries
        int index = av_index_search_timestamp(st, seek_pts, flags);

        if (index != (-1))
        {
          AVIndexEntry *ie = &st->index_entries[index];

          seekPosition = ie->pos;
          seekTimestamp = ie->timestamp;

          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, found FFmpeg seek index entry, pos: %lld, timestamp: %lld, seek pts: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, seekPosition, seekTimestamp, seek_pts);
        }
      }
      else
      {
        // check our seek index entry collection
        HRESULT index = activeStream->GetSeekIndexEntries()->FindSeekIndexEntry(seek_pts, ((flags & AVSEEK_FLAG_BACKWARD) == AVSEEK_FLAG_BACKWARD));

        if (SUCCEEDED(index))
        {
          CSeekIndexEntry *ie = activeStream->GetSeekIndexEntries()->GetItem((unsigned int)index);

          seekPosition = ie->GetPosition();
          seekTimestamp = ie->GetTimestamp();

          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, found our seek index entry, pos: %lld, timestamp: %lld, seek pts: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, seekPosition, seekTimestamp, seek_pts);
        }
      }

      int64_t ret = avio_seek(this->formatContext->pb, seekPosition, SEEK_SET);
      if (ret < 0)
      {
        result = (HRESULT)ret;
        this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, avio_seek error: 0x%08X", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId, result);
      }

      if (SUCCEEDED(result))
      {
        ff_read_frame_flush(this->formatContext);
        ff_update_cur_dts(this->formatContext, st, seekTimestamp);
      }
    }

    // continuously read until we found avPacket with dts higher than seek_pts (or error)

    while (SUCCEEDED(result))
    {
      int avResult = 0;
      do
      {
        if ((avResult == E_CONNECTION_LOST_TRYING_REOPEN) || this->IsSetFlags(DEMUXER_FLAG_PENDING_DISCONTINUITY))
        {
          this->flags &= ~DEMUXER_FLAG_PENDING_DISCONTINUITY;
        }

        avResult = av_read_frame(this->formatContext, &avPacket);

        // assume we are not eof
        CHECK_CONDITION_EXECUTE(this->formatContext->pb != NULL, this->formatContext->pb->eof_reached = 0);

        if (avResult == E_CONNECTION_LOST_TRYING_REOPEN)
        {
          ff_read_frame_flush(this->formatContext);
        }

        if ((avResult == AVERROR(EAGAIN)) || (avResult == E_CONNECTION_LOST_TRYING_REOPEN))
        {
          Sleep(1);
        }
      } while ((avResult == AVERROR(EAGAIN)) || (avResult == E_CONNECTION_LOST_TRYING_REOPEN));

      CHECK_CONDITION_EXECUTE(avResult < 0, result = (HRESULT)avResult);
      av_free_packet(&avPacket);

      if ((streamId != avPacket.stream_index) || (avPacket.pts < 0))
      {
        // continue reading next avPacket, because we don't have avPacket with right stream index or avPacket doesn't have dts
        continue;
      }

      if (avPacket.pts > seek_pts)
      {
        found = true;
        break;
      }
    }
  }

  if (SUCCEEDED(result) && found)
  {
    // we must clean end of stream flag, to restart demuxing
    // in another case, demuxing will not work and we don't have any video or audio
    this->flags &= ~DEMUXER_FLAG_END_OF_STREAM_OUTPUT_PACKET_QUEUED;

    {
      // lock access to media packets and output packets
      CLockMutex outputPacketLock(this->outputPacketMutex, INFINITE);

      // clear output packets
      this->outputPacketCollection->Clear();
    }
  }

  this->logger->Log(LOGGER_INFO, SUCCEEDED(result) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, this->demuxerId, result);
  return result;
}

int CStandardDemuxer::InitParser(AVFormatContext *formatContext, AVStream *stream)
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

void CStandardDemuxer::UpdateParserFlags(AVStream *stream)
{
  if (stream->parser != NULL)
  {
    if (((stream->codec->codec_id == CODEC_ID_MPEG2VIDEO) || (stream->codec->codec_id == CODEC_ID_MPEG1VIDEO)) &&
      (_wcsicmp(this->containerFormat, L"mpegvideo") != 0))
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

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
#include "LockMutex.h"
#include "ErrorCodes.h"
#include "Parameters.h"
#include "StreamPackage.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackageDataResponse.h"
#include "StreamPackagePacketRequest.h"
#include "StreamPackagePacketResponse.h"

#include "moreuuids.h"

#include <assert.h>
#include <Shlwapi.h>
#include <process.h>

#ifdef _DEBUG
#define MODULE_NAME                                                         L"Demuxerd"
#else
#define MODULE_NAME                                                         L"Demuxer"
#endif

#define METHOD_DEMUXER_MESSAGE_FORMAT                                       L"%s: %s: stream %u, %s"
#define METHOD_DEMUXER_START_FORMAT                                         L"%s: %s: stream %u, Start"
#define METHOD_DEMUXER_END_FORMAT                                           L"%s: %s: stream %u, End"
#define METHOD_DEMUXER_END_FAIL_FORMAT                                      L"%s: %s: stream %u, End, Fail"
#define METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT                              L"%s: %s: stream %u, End, Fail, result: 0x%08X"

#define METHOD_SEEK_NAME                                                    L"Seek()"
#define METHOD_SEEK_BY_TIME_NAME                                            L"SeekByTime()"
#define METHOD_SEEK_BY_POSITION_NAME                                        L"SeekByPosition()"
#define METHOD_SEEK_BY_SEQUENCE_READING_NAME                                L"SeekBySequenceReading()"
#define METHOD_GET_NEXT_PACKET_NAME                                         L"GetNextPacket()"

#define METHOD_CREATE_CREATE_DEMUXER_WORKER_NAME                            L"CreateCreateDemuxerWorker()"
#define METHOD_DESTROY_CREATE_DEMUXER_WORKER_NAME                           L"DestroyCreateDemuxerWorker()"
#define METHOD_CREATE_DEMUXER_WORKER_NAME                                   L"CreateDemuxerWorker()"

#define METHOD_DEMUXER_SEEK_NAME                                            L"DemuxerSeek()"
#define METHOD_DEMUXER_READ_NAME                                            L"DemuxerRead()"

#define METHOD_CREATE_DEMUXING_WORKER_NAME                                  L"CreateDemuxingWorker()"
#define METHOD_DESTROY_DEMUXING_WORKER_NAME                                 L"DestroyDemuxingWorker()"
#define METHOD_DEMUXING_WORKER_NAME                                         L"DemuxingWorker()"

#define METHOD_LOAD_MEDIA_PACKET_FOR_PROCESSING_NAME                        L"LoadMediaPacketForProcessing()"

#define DEMUXER_READ_BUFFER_SIZE								                            32768

//#define FLV_PACKET_MINIMUM_CHECKED                                          5           // minimum FLV packets to check in buffer
//#define FLV_DO_NOT_SEEK_DIFFERENCE                                          10000       // time in ms when FLV packet dts is closer to seek time
//#define FLV_SEEKING_POSITIONS                                               1024        // maximum FLV seeking positions

#define MAXIMUM_MPEG2_TS_DATA_PACKET                                        (188 * 5577)   // 5577 * 188 = 1048476 < 1 MB

extern "C" void asf_reset_header2(AVFormatContext *s);

struct FlvSeekPosition
{
  int64_t time;
  int64_t position;
};

#define FLV_SEEK_UPPER_BOUNDARY                                             1
#define FLV_SEEK_LOWER_BOUNDARY                                             0
#define FLV_BOUNDARIES_COUNT                                                2
#define FLV_SEEKING_BUFFER_SIZE                                             32 * 1024   // size of buffer to read from stream
#define FLV_SEEKING_TOTAL_BUFFER_SIZE                                       32 * FLV_SEEKING_BUFFER_SIZE // total buffer size for reading from stream (1 MB)
#define FLV_NO_SEEK_DIFFERENCE_TIME                                         10000000    // time in DSHOW_TIME_BASE units between lower FLV seeking boundary time and seek pts to ignore seeking

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

CDemuxer::CDemuxer(HRESULT *result, CLogger *logger, IDemuxerOwner *filter, CParameterCollection *configuration)
  : CFlags()
{
  this->logger = NULL;
  this->containerFormat = NULL;
  this->formatContext = NULL;
  this->filter = NULL;
  //this->dontChangeTimestamps = false;
  //this->flvTimestamps = NULL;
  this->demuxerContextBufferPosition = 0;
  this->demuxerContext = NULL;
  this->createDemuxerWorkerShouldExit = false;
  this->createDemuxerWorkerThread = NULL;
  this->pauseSeekStopRequest = PAUSE_SEEK_STOP_MODE_NONE;
  this->demuxerId = 0;
  this->configuration = NULL;
  this->outputPacketCollection = NULL;
  this->outputPacketMutex = NULL;
  this->streamInputFormat = NULL;
  this->demuxingWorkerThread = NULL;
  this->demuxingWorkerShouldExit = false;
  this->packetInputFormat = NULL;
  this->createDemuxerError = S_OK;
  this->demuxerContextRequestId = 0;

  for (unsigned int i = 0; i < CStream::Unknown; i++)
  {
    this->streams[i] = NULL;
  }

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, logger);
    CHECK_POINTER_DEFAULT_HRESULT(*result, filter);
    CHECK_POINTER_DEFAULT_HRESULT(*result, configuration);

    if (SUCCEEDED(*result))
    {
      this->logger = logger;
      this->filter = filter;

      //this->flvTimestamps = ALLOC_MEM_SET(this->flvTimestamps, FlvTimestamp, FLV_TIMESTAMP_MAX, 0);
      //CHECK_POINTER_HRESULT(*result, this->flvTimestamps, *result, E_OUTOFMEMORY);

      this->outputPacketCollection = new COutputPinPacketCollection(result);
      CHECK_POINTER_HRESULT(*result, this->outputPacketCollection, *result, E_OUTOFMEMORY);

      this->configuration = new CParameterCollection(result);
      CHECK_POINTER_HRESULT(*result, this->configuration, *result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(*result, this->configuration->Append(configuration), *result, E_OUTOFMEMORY);

      this->outputPacketMutex = CreateMutex(NULL, FALSE, NULL);

      CHECK_POINTER_HRESULT(*result, this->outputPacketMutex, *result, E_OUTOFMEMORY);

      for (unsigned int i = 0; i < CStream::Unknown; i++)
      {
        this->streams[i] = new CStreamCollection(result);
        CHECK_POINTER_HRESULT(*result, this->streams[i], *result, E_OUTOFMEMORY);

        this->activeStream[i] = ACTIVE_STREAM_NOT_SPECIFIED;
      }
    }
  }
}

CDemuxer::~CDemuxer(void)
{
  // destroy create demuxer worker (if not finished earlier)
  this->DestroyCreateDemuxerWorker();

  // destroy demuxing worker (if not finished earlier)
  this->DestroyDemuxingWorker();

  FREE_MEM_CLASS(this->outputPacketCollection);

  if (this->outputPacketMutex != NULL)
  {
    CloseHandle(this->outputPacketMutex);
    this->outputPacketMutex = NULL;
  }

  this->CleanupFormatContext();

  // release AVIOContext for demuxer
  if (this->demuxerContext != NULL)
  {
    av_free(this->demuxerContext->buffer);
    av_free(this->demuxerContext);
    this->demuxerContext = NULL;
  }

  this->demuxerContextBufferPosition = 0;

  for (unsigned int i = 0; i < CStream::Unknown; i++)
  {
    FREE_MEM_CLASS(this->streams[i]);
  }

  FREE_MEM(this->containerFormat);
  FREE_MEM(this->streamInputFormat);
  //FREE_MEM(this->flvTimestamps);
  FREE_MEM_CLASS(this->configuration);
  FREE_MEM_CLASS(this->packetInputFormat);
}

/* get methods */

CStreamCollection *CDemuxer::GetStreams(CStream::StreamType type)
{
  return this->streams[type];
}

int64_t CDemuxer::GetDuration(void)
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

const wchar_t *CDemuxer::GetContainerFormat(void)
{
  return this->containerFormat;
}

HRESULT CDemuxer::GetOutputPinPacket(COutputPinPacket *packet)
{
  // S_FALSE means no packet
  HRESULT result = S_FALSE;
  CHECK_POINTER_DEFAULT_HRESULT(result, packet);

  if (SUCCEEDED(result))
  {
    CLockMutex lock(this->outputPacketMutex, INFINITE);

    COutputPinPacket *outputPacket = this->outputPacketCollection->GetItem(0);
    if (outputPacket != NULL)
    {
      if (!outputPacket->IsEndOfStream())
      {
        CHECK_CONDITION_HRESULT(result, packet->GetBuffer()->InitializeBuffer(outputPacket->GetBuffer()->GetBufferOccupiedSpace()), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), packet->GetBuffer()->AddToBufferWithResize(outputPacket->GetBuffer()));
      }

      packet->SetStreamPid(outputPacket->GetStreamPid());
      packet->SetDemuxerId(outputPacket->GetDemuxerId());
      packet->SetStartTime(outputPacket->GetStartTime());
      packet->SetEndTime(outputPacket->GetEndTime());
      packet->SetFlags(outputPacket->GetFlags());
      packet->SetMediaType(outputPacket->GetMediaType());
      packet->SetEndOfStream(outputPacket->IsEndOfStream(), outputPacket->GetEndOfStreamResult());
      outputPacket->SetMediaType(NULL);

      if (SUCCEEDED(result))
      {
        this->outputPacketCollection->Remove(0);
        result = S_OK;
      }
    }
  }

  return result;
}

unsigned int CDemuxer::GetDemuxerId(void)
{
  return this->demuxerId;
}

IDemuxerOwner *CDemuxer::GetDemuxerOwner(void)
{
  return this->filter;
}

HRESULT CDemuxer::GetCreateDemuxerError(void)
{
  return this->createDemuxerError;
}

/* set methods */

void CDemuxer::SetActiveStream(CStream::StreamType streamType, int activeStreamId)
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

void CDemuxer::SetDemuxerId(unsigned int demuxerId)
{
  this->demuxerId = demuxerId;
}

void CDemuxer::SetPauseSeekStopRequest(bool pauseSeekStopRequest)
{
  this->pauseSeekStopRequest = pauseSeekStopRequest ? PAUSE_SEEK_STOP_MODE_DISABLE_READING : PAUSE_SEEK_STOP_MODE_NONE;
  this->filter->SetPauseSeekStopMode(this->pauseSeekStopRequest);
}

void CDemuxer::SetRealDemuxingNeeded(bool realDemuxingNeeded)
{
  this->flags &= ~DEMUXER_FLAG_REAL_DEMUXING_NEEDED;
  this->flags |= (realDemuxingNeeded) ? DEMUXER_FLAG_REAL_DEMUXING_NEEDED : DEMUXER_FLAG_NONE;
}

HRESULT CDemuxer::SetStreamInformation(CStreamInformation *streamInformation)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamInformation);

  if (SUCCEEDED(result))
  {
    this->flags &= ~(DEMUXER_FLAG_STREAM_IN_CONTAINER | DEMUXER_FLAG_STREAM_IN_PACKETS);

    this->flags |= (streamInformation->IsContainer()) ? DEMUXER_FLAG_STREAM_IN_CONTAINER : DEMUXER_FLAG_NONE;
    this->flags |= (streamInformation->IsPackets()) ? DEMUXER_FLAG_STREAM_IN_PACKETS : DEMUXER_FLAG_NONE;

    SET_STRING_HRESULT_WITH_NULL(this->streamInputFormat, streamInformation->GetStreamInputFormat(), result);
  }

  return result;
}

/* other methods */

bool CDemuxer::IsFlv(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_FLV);
}

bool CDemuxer::IsAsf(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_ASF);
}

bool CDemuxer::IsMp4(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_MP4);
}

bool CDemuxer::IsMatroska(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_MATROSKA);
}

bool CDemuxer::IsOgg(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_OGG);
}

bool CDemuxer::IsAvi(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_AVI);
}

bool CDemuxer::IsMpegTs(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_MPEG_TS);
}

bool CDemuxer::IsMpegPs(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_MPEG_PS);
}

bool CDemuxer::IsRm(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_RM);
}

bool CDemuxer::IsVc1SeenTimestamp(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_VC1_SEEN_TIMESTAMP);
}

bool CDemuxer::IsVc1Correction(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_VC1_CORRECTION);
}

bool CDemuxer::IsCreatedDemuxer(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_CREATED_DEMUXER);
}

bool CDemuxer::IsCreateDemuxerWorkerFinished(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED);
}

bool CDemuxer::IsRealDemuxingNeeded(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_REAL_DEMUXING_NEEDED);
}

bool CDemuxer::HasStartedCreatingDemuxer(void)
{
  return (this->createDemuxerWorkerThread != NULL);
}

bool CDemuxer::IsEndOfStreamOutputPacketQueued(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_END_OF_STREAM_OUTPUT_PACKET_QUEUED);
}

HRESULT CDemuxer::StartCreatingDemuxer(void)
{
  HRESULT result = S_OK;

  if (SUCCEEDED(result) && (!this->IsCreateDemuxerWorkerFinished()) && (this->createDemuxerWorkerThread == NULL))
  {
    result = this->CreateCreateDemuxerWorker();
  }

  return result;
}

HRESULT CDemuxer::StartDemuxing(void)
{
  HRESULT result = this->IsCreatedDemuxer() ? S_OK : E_NOT_VALID_STATE;

  if (SUCCEEDED(result))
  {
    result = this->CreateDemuxingWorker();
  }

  return result;
}

CStream *CDemuxer::SelectVideoStream(void)
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

CStream *CDemuxer::SelectAudioStream(void)
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

HRESULT CDemuxer::Seek(REFERENCE_TIME time)
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

  this->flags &= ~DEMUXER_FLAG_VC1_SEEN_TIMESTAMP;
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_END_FORMAT, MODULE_NAME, METHOD_SEEK_NAME, this->demuxerId);

  return result;
}

uint64_t CDemuxer::GetPositionForStreamTime(uint64_t streamTime)
{
  uint64_t result = 0;

  if (this->IsRealDemuxingNeeded())
  {
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
  }
  else
  {
    // IPTV demuxing case (no demuxing, just passing data out)
    result = this->demuxerContextBufferPosition;
  }

  return result;
}

/* protected methods */

void CDemuxer::CleanupFormatContext(void)
{
  if (this->formatContext)
  {
    avformat_close_input(&this->formatContext);
    this->formatContext = NULL;
  }
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

HRESULT CDemuxer::SeekByTime(REFERENCE_TIME time, int flags)
{
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->demuxerId);

  HRESULT result = S_OK;

  AVStream *st = NULL;
  AVIndexEntry *ie = NULL;  
  int64_t seek_pts = time;

  // seek preference :
  // 1. in case of container (DEMUXER_FLAG_STREAM_IN_CONTAINER)
  //    a. active video stream
  //    b. any video stream
  //    c. active audio stream
  //    d. any audio stream
  //    e. active subtitle stream
  //    f. any subtitle stream
  // 2. in case of packet stream (DEMUXER_FLAG_STREAM_IN_PACKETS)
  //    in that case is assumed that there is only one stream in all groups (video, audio, subtitle)

  int streamId = -1;
  for (unsigned int i = 0; i < CStream::Unknown; i++)
  {
    // stream groups are in order: video, audio, subtitle = in our preference
    if (this->GetStreams((CStream::StreamType)i)->Count() > 0)
    {
      streamId = this->GetStreams((CStream::StreamType)i)->GetItem((this->activeStream[(CStream::StreamType)i] == ACTIVE_STREAM_NOT_SPECIFIED) ? 0 : this->activeStream[(CStream::StreamType)i])->GetPid();
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

    st = this->formatContext->streams[streamId];

    bool found = false;
    int index = -1;

    ff_read_frame_flush(this->formatContext);
    index = av_index_search_timestamp(st, seek_pts, flags);

    if (!found) 
    {
      st->nb_index_entries = 0;
      st->nb_frames = 0;
      
      // seek to time
      int64_t seekedTime = this->filter->SeekToTime(this->demuxerId, time / (DSHOW_TIME_BASE / 1000)); // (1000 / DSHOW_TIME_BASE)

      if (seekedTime >= 0)
      {
        // lock access to output packets
        CLockMutex outputPacketLock(this->outputPacketMutex, INFINITE);

        // clear output packets
        this->outputPacketCollection->Clear();

        // set buffer position to zero
        this->demuxerContextBufferPosition = 0;
        this->flags &= ~DEMUXER_FLAG_END_OF_STREAM_OUTPUT_PACKET_QUEUED;

        // enable reading from seek method, do not allow (yet) to read from demuxing worker
        this->pauseSeekStopRequest = PAUSE_SEEK_STOP_MODE_DISABLE_DEMUXING;
        this->filter->SetPauseSeekStopMode(this->pauseSeekStopRequest);
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
    }

    ff_read_frame_flush(this->formatContext);

    if (SUCCEEDED(result) && (!found))
    {
      st = this->formatContext->streams[streamId];
      ff_read_frame_flush(this->formatContext);
      index = av_index_search_timestamp(st, seek_pts, flags);

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
              int read_status = 0;
              do
              {
                read_status = av_read_frame(this->formatContext, &avPacket);
              } while (read_status == AVERROR(EAGAIN));

              if (read_status < 0)
              {
                // error occured
                result = (HRESULT)read_status;
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
      index = av_index_search_timestamp(st, seek_pts, flags);

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

HRESULT CDemuxer::SeekByPosition(REFERENCE_TIME time, int flags)
{
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->demuxerId);

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
  this->pauseSeekStopRequest = PAUSE_SEEK_STOP_MODE_DISABLE_DEMUXING;
  this->filter->SetPauseSeekStopMode(this->pauseSeekStopRequest);

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

    int index = -1;

    ff_read_frame_flush(this->formatContext);
    index = av_index_search_timestamp(st, seek_pts, flags);

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
                  int read_status = 0;
                  do
                  {
                    read_status = av_read_frame(this->formatContext, &avPacket);
                  } while (read_status == AVERROR(EAGAIN));

                  CHECK_CONDITION_EXECUTE(read_status < 0, result = (HRESULT)read_status);
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

HRESULT CDemuxer::SeekBySequenceReading(REFERENCE_TIME time, int flags)
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
    int read_status = 0;

    // enable reading from seek method, do not allow (yet) to read from demuxing worker
    this->pauseSeekStopRequest = PAUSE_SEEK_STOP_MODE_DISABLE_DEMUXING;
    this->filter->SetPauseSeekStopMode(this->pauseSeekStopRequest);

    while (SUCCEEDED(result))
    {
      do
      {
        read_status = av_read_frame(this->formatContext, &avPacket);
      } while (read_status == AVERROR(EAGAIN));

      CHECK_CONDITION_EXECUTE(read_status < 0, result = (HRESULT)read_status);
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
      do
      {
        read_status = av_read_frame(this->formatContext, &avPacket);
      } while (read_status == AVERROR(EAGAIN));

      CHECK_CONDITION_EXECUTE(read_status < 0, result = (HRESULT)read_status);
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

unsigned int WINAPI CDemuxer::CreateDemuxerWorker(LPVOID lpParam)
{
  CDemuxer *caller = (CDemuxer *)lpParam;

  caller->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXER_WORKER_NAME, caller->demuxerId);

  HRESULT result = S_OK;
  while ((!caller->createDemuxerWorkerShouldExit) && (!caller->IsCreatedDemuxer()) && (!IS_OUR_ERROR(result)))
  {
    if (!caller->IsCreatedDemuxer())
    {
      caller->demuxerContextBufferPosition = 0;

      if (SUCCEEDED(result) && (caller->IsRealDemuxingNeeded()))
      {
        if (caller->demuxerContext == NULL)
        {
          uint8_t *buffer = (uint8_t *)av_mallocz(DEMUXER_READ_BUFFER_SIZE + FF_INPUT_BUFFER_PADDING_SIZE);
          caller->demuxerContext = avio_alloc_context(buffer, DEMUXER_READ_BUFFER_SIZE, 0, caller, DemuxerRead, NULL, DemuxerSeek);
        }

        CHECK_POINTER_HRESULT(result, caller->demuxerContext, result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), caller->logger->Log(LOGGER_ERROR, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXER_WORKER_NAME, caller->demuxerId, L"not enough memory to allocate AVIOContext"));

        if (SUCCEEDED(result))
        {
          result = caller->OpenStream(caller->demuxerContext);

          if (FAILED(result))
          {
            caller->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, OpenStream() error: 0x%08X", MODULE_NAME, METHOD_CREATE_DEMUXER_WORKER_NAME, caller->demuxerId, result);

            // clean up
            av_free(caller->demuxerContext->buffer);
            av_free(caller->demuxerContext);
            caller->demuxerContext = NULL;
            caller->demuxerContextBufferPosition = 0;
          }
        }
      }

      caller->createDemuxerError = result;

      if (SUCCEEDED(result))
      {
        caller->flags |= DEMUXER_FLAG_CREATED_DEMUXER;
        break;
      }
      else
      {
        if (caller->demuxerContext != NULL)
        {
          av_free(caller->demuxerContext->buffer);
          av_free(caller->demuxerContext);
          caller->demuxerContext = NULL;
          caller->demuxerContextBufferPosition = 0;
        }
      }
    }

    if (!caller->IsCreatedDemuxer())
    {
      Sleep(100);
    }
  }

  caller->logger->Log(LOGGER_INFO, METHOD_DEMUXER_END_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXER_WORKER_NAME, caller->demuxerId);
  caller->flags |= DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED;

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}

HRESULT CDemuxer::CreateCreateDemuxerWorker(void)
{
  HRESULT result = S_OK;

  this->flags &= ~DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED;

  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_CREATE_CREATE_DEMUXER_WORKER_NAME, this->demuxerId);

  this->createDemuxerWorkerShouldExit = false;

  this->createDemuxerWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CDemuxer::CreateDemuxerWorker, this, 0, NULL);

  if (this->createDemuxerWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, _beginthreadex() error: 0x%08X", MODULE_NAME, METHOD_CREATE_CREATE_DEMUXER_WORKER_NAME, this->demuxerId, result);
    this->flags |= DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED;
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_CREATE_CREATE_DEMUXER_WORKER_NAME, this->demuxerId, result);
  return result;
}

HRESULT CDemuxer::DestroyCreateDemuxerWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_DESTROY_CREATE_DEMUXER_WORKER_NAME, this->demuxerId);

  this->createDemuxerWorkerShouldExit = true;
  this->filter->SetPauseSeekStopMode(PAUSE_SEEK_STOP_MODE_DISABLE_READING);

  // wait for the create demuxer worker thread to exit
  if (this->createDemuxerWorkerThread != NULL)
  {
    if (WaitForSingleObject(this->createDemuxerWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_DESTROY_CREATE_DEMUXER_WORKER_NAME, this->demuxerId, L"thread didn't exit, terminating thread");
      TerminateThread(this->createDemuxerWorkerThread, 0);
    }
    CloseHandle(this->createDemuxerWorkerThread);
  }

  this->createDemuxerWorkerThread = NULL;
  this->createDemuxerWorkerShouldExit = false;
  this->flags |= DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED;

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DESTROY_CREATE_DEMUXER_WORKER_NAME, this->demuxerId, result);
  return result;
}

HRESULT CDemuxer::DemuxerReadPosition(int64_t position, uint8_t *buffer, int length, uint64_t flags)
{
  HRESULT result = S_OK;
  CHECK_CONDITION(result, length >= 0, S_OK, E_INVALIDARG);
  CHECK_POINTER_DEFAULT_HRESULT(result, buffer);

  if (SUCCEEDED(result) && (length > 0))
  {
    CStreamPackage *package = new CStreamPackage(&result);
    CHECK_POINTER_HRESULT(result, package, result, E_OUTOFMEMORY);
    
    unsigned int requestId = this->demuxerContextRequestId++;
    if (SUCCEEDED(result))
    {
      CStreamPackageDataRequest *request = new CStreamPackageDataRequest(&result);
      CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        request->SetAnyDataLength((flags & STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_DATA_LENGTH) == STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_DATA_LENGTH);
        request->SetAnyNonZeroDataLength((flags & STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_NONZERO_DATA_LENGTH) == STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_NONZERO_DATA_LENGTH);
        request->SetId(requestId);
        request->SetStreamId(this->demuxerId);
        request->SetStart(position);
        request->SetLength((unsigned int)length);
        
        package->SetRequest(request);
      }
      
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(request));
    }

    if (this->IsSetFlags(DEMUXER_FLAG_PENDING_DISCONTINUITY))
    {
      if (this->IsSetFlags(DEMUXER_FLAG_PENDING_DISCONTINUITY_WITH_REPORT))
      {
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, request %u, start: %lld, length: %d, discontinuity reported", MODULE_NAME, METHOD_DEMUXER_READ_NAME, this->demuxerId, requestId, position, length);
      }

      this->flags &= ~(DEMUXER_FLAG_PENDING_DISCONTINUITY | DEMUXER_FLAG_PENDING_DISCONTINUITY_WITH_REPORT);
      result = E_CONNECTION_LOST_TRYING_REOPEN;
    }

    CHECK_HRESULT_EXECUTE(result, this->filter->ProcessStreamPackage(package));
    CHECK_HRESULT_EXECUTE(result, package->GetError());

    if (SUCCEEDED(result))
    {
      // successfully processed stream package request
      CStreamPackageDataResponse *response = dynamic_cast<CStreamPackageDataResponse *>(package->GetResponse());

      response->GetBuffer()->CopyFromBuffer(buffer, response->GetBuffer()->GetBufferOccupiedSpace());
      result = response->GetBuffer()->GetBufferOccupiedSpace();

      if (response->IsDiscontinuity())
      {
        this->flags |= DEMUXER_FLAG_PENDING_DISCONTINUITY;

        if (result != 0)
        {
          this->flags |= DEMUXER_FLAG_PENDING_DISCONTINUITY_WITH_REPORT;
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, request %u, start: %lld, length: %d, pending discontinuity", MODULE_NAME, METHOD_DEMUXER_READ_NAME, this->demuxerId, requestId, position, length);
        }
      }
    }

    CHECK_CONDITION_EXECUTE(FAILED(result) && (result != E_CONNECTION_LOST_TRYING_REOPEN), this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, request %u, start: %lld, length: %d, error: 0x%08X", MODULE_NAME, METHOD_DEMUXER_READ_NAME, this->demuxerId, requestId, position, length, result));

    FREE_MEM_CLASS(package);
  }
  return result;
}

int CDemuxer::DemuxerRead(void *opaque, uint8_t *buf, int buf_size)
{
  CDemuxer *demuxer = static_cast<CDemuxer *>(opaque);
  HRESULT result = demuxer->DemuxerReadPosition(demuxer->demuxerContextBufferPosition, buf, buf_size, STREAM_PACKAGE_DATA_REQUEST_FLAG_NONE);

  if (SUCCEEDED(result))
  {
    // in case of success is in result is length of returned data
    demuxer->demuxerContextBufferPosition += (unsigned int)result;
  }

  return (int)result;
}

int64_t CDemuxer::DemuxerSeek(void *opaque,  int64_t offset, int whence)
{
  CDemuxer *demuxer = static_cast<CDemuxer *>(opaque);

  int64_t result = -1;

  if (whence == SEEK_SET)
  {
	  demuxer->demuxerContextBufferPosition = offset;
    result = demuxer->demuxerContextBufferPosition;
  }
  else if (whence == SEEK_CUR)
  {
    demuxer->demuxerContextBufferPosition += offset;
    result = demuxer->demuxerContextBufferPosition;

  }
  else if ((whence == SEEK_END) || (whence == AVSEEK_SIZE))
  {
    HRESULT res = S_OK;
    CStreamProgress *progress = new CStreamProgress();
    CHECK_POINTER_HRESULT(result, progress, result, E_OUTOFMEMORY);

    if (SUCCEEDED(res))
    {
      progress->SetStreamId(demuxer->GetDemuxerId());
      res = demuxer->filter->QueryStreamProgress(progress);
    }

    if (SUCCEEDED(res))
    {
      if (whence == SEEK_END)
      {
        demuxer->demuxerContextBufferPosition = progress->GetTotalLength() - offset;
        result = demuxer->demuxerContextBufferPosition;
      }
      else
      {
        result = progress->GetTotalLength();
      }
    }

    FREE_MEM_CLASS(progress);
  }

  return result;
}

unsigned int WINAPI CDemuxer::DemuxingWorker(LPVOID lpParam)
{
  CDemuxer *caller = (CDemuxer *)lpParam;
  caller->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_DEMUXING_WORKER_NAME, caller->demuxerId);

  while (!caller->demuxingWorkerShouldExit)
  {
    if ((caller->pauseSeekStopRequest == PAUSE_SEEK_STOP_MODE_NONE) && (!caller->IsEndOfStreamOutputPacketQueued()))
    {
      // S_FALSE means no packet
      HRESULT result = S_FALSE;
      COutputPinPacket *packet = new COutputPinPacket(&result);
      CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        result = caller->GetNextPacketInternal(packet);

        if (FAILED(result) && (result != E_PAUSE_SEEK_STOP_MODE_DISABLE_READING))
        {
          // any error code (except disabled reading) for end of stream

          packet->SetDemuxerId(caller->demuxerId);
          packet->SetEndOfStream(true, (result == E_NO_MORE_DATA_AVAILABLE) ? S_OK : result);
          result = S_OK;
        }
      }

      // S_FALSE means no packet
      if (result == S_OK)
      {
        CLockMutex lock(caller->outputPacketMutex, INFINITE);
        
        if (packet->IsEndOfStream())
        {
          bool queuedEndOfStream = false;
          HRESULT endOfStreamResult = packet->GetEndOfStreamResult();

          for (unsigned int i = 0; (SUCCEEDED(result) && (i < CStream::Unknown)); i++)
          {
            CStreamCollection *streams = caller->streams[i];

            for (unsigned int j = 0; (SUCCEEDED(result) && (j < streams->Count())); j++)
            {
              CStream *stream = streams->GetItem(j);

              COutputPinPacket *endOfStreamPacket = new COutputPinPacket(&result);
              CHECK_POINTER_HRESULT(result, endOfStreamPacket, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                endOfStreamPacket->SetDemuxerId(caller->demuxerId);
                endOfStreamPacket->SetEndOfStream(true, packet->GetEndOfStreamResult());
                endOfStreamPacket->SetStreamPid(stream->GetPid());

                CHECK_CONDITION_HRESULT(result, caller->outputPacketCollection->Add(endOfStreamPacket), result, E_OUTOFMEMORY);
                queuedEndOfStream = true;
              }

              CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(endOfStreamPacket));
            }
          }

          if (SUCCEEDED(result) && (!queuedEndOfStream))
          {
            CHECK_CONDITION_HRESULT(result, caller->outputPacketCollection->Add(packet), result, E_OUTOFMEMORY);

            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packet));
          }

          CHECK_CONDITION_EXECUTE(queuedEndOfStream, FREE_MEM_CLASS(packet));

          if (SUCCEEDED(result))
          {
            caller->flags |= DEMUXER_FLAG_END_OF_STREAM_OUTPUT_PACKET_QUEUED;
            caller->logger->Log(LOGGER_INFO, L"%s: %s: stream %u, queued end of stream output packet, result: 0x%08X", MODULE_NAME, METHOD_DEMUXING_WORKER_NAME, caller->demuxerId, endOfStreamResult);
          }
        }
        else
        {
          CHECK_CONDITION_HRESULT(result, caller->outputPacketCollection->Add(packet), result, E_OUTOFMEMORY);
        }
      }

      CHECK_CONDITION_EXECUTE(result != S_OK, FREE_MEM_CLASS(packet));
    }

    Sleep(1);
  }

  caller->logger->Log(LOGGER_INFO, METHOD_DEMUXER_END_FORMAT, MODULE_NAME, METHOD_DEMUXING_WORKER_NAME, caller->demuxerId);

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}

HRESULT CDemuxer::CreateDemuxingWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXING_WORKER_NAME, this->demuxerId);

  this->demuxingWorkerShouldExit = false;

  this->demuxingWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CDemuxer::DemuxingWorker, this, 0, NULL);

  if (this->demuxingWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, _beginthreadex() error: 0x%08X", MODULE_NAME, METHOD_CREATE_DEMUXING_WORKER_NAME, this->demuxerId, result);
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXING_WORKER_NAME, this->demuxerId, result);
  return result;
}

HRESULT CDemuxer::DestroyDemuxingWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_DESTROY_DEMUXING_WORKER_NAME, this->demuxerId);

  this->demuxingWorkerShouldExit = true;

  // wait for the receive data worker thread to exit      
  if (this->demuxingWorkerThread != NULL)
  {
    if (WaitForSingleObject(this->demuxingWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_DESTROY_DEMUXING_WORKER_NAME, this->demuxerId, L"thread didn't exit, terminating thread");
      TerminateThread(this->demuxingWorkerThread, 0);
    }
    CloseHandle(this->demuxingWorkerThread);
  }

  this->demuxingWorkerThread = NULL;
  this->demuxingWorkerShouldExit = false;

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DESTROY_DEMUXING_WORKER_NAME, this->demuxerId, result);
  return result;
}

HRESULT CDemuxer::GetNextPacketInternal(COutputPinPacket *packet)
{
  // S_FALSE means no packet
  HRESULT result = S_FALSE;
  CHECK_POINTER_DEFAULT_HRESULT(result, packet);

  if (SUCCEEDED(result) && (this->IsRealDemuxingNeeded()))
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
    else if (ffmpegResult == E_CONNECTION_LOST_TRYING_REOPEN)
    {
      ff_read_frame_flush(this->formatContext);

      this->flags |= DEMUXER_FLAG_CONNECTION_LOST_TRYING_REOPEN;
      result = S_FALSE;
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

        REFERENCE_TIME pts = this->IsSetFlags(DEMUXER_FLAG_STREAM_IN_PACKETS) ? ffmpegPacket.pts : (REFERENCE_TIME)ConvertTimestampToRT(ffmpegPacket.pts, stream->time_base.num, stream->time_base.den, (int64_t)AV_NOPTS_VALUE);
        REFERENCE_TIME dts = this->IsSetFlags(DEMUXER_FLAG_STREAM_IN_PACKETS) ? ffmpegPacket.dts : (REFERENCE_TIME)ConvertTimestampToRT(ffmpegPacket.dts, stream->time_base.num, stream->time_base.den, (int64_t)AV_NOPTS_VALUE);

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

              this->flags &= ~DEMUXER_FLAG_VC1_SEEN_TIMESTAMP;
              this->flags |= (pts != COutputPinPacket::INVALID_TIME) ? DEMUXER_FLAG_VC1_SEEN_TIMESTAMP : DEMUXER_FLAG_NONE;
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

        packet->SetDiscontinuity(((ffmpegPacket.flags & AV_PKT_FLAG_CORRUPT) != 0) || this->IsSetFlags(DEMUXER_FLAG_CONNECTION_LOST_TRYING_REOPEN));
        this->flags &= ~DEMUXER_FLAG_CONNECTION_LOST_TRYING_REOPEN;

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
      if (SUCCEEDED(result) && (ffmpegPacket.pts != AV_NOPTS_VALUE) && (ffmpegPacket.pos >= 0) && (stream->nb_index_entries == 0))
      {
        // stream doesn't create seek index
        // create our own seek index

        for (unsigned int i = 0; i < CStream::Unknown; i++)
        {
          // stream groups are in order: video, audio, subtitle = in our preference
          if (this->GetStreams((CStream::StreamType)i)->Count() > 0)
          {
            CStream *activeStream = this->GetStreams((CStream::StreamType)i)->GetItem((this->activeStream[(CStream::StreamType)i] == ACTIVE_STREAM_NOT_SPECIFIED) ? 0 : this->activeStream[(CStream::StreamType)i]);

            if (ffmpegPacket.stream_index == activeStream->GetPid())
            {
              // we found active stream to FFmpeg packet stream index
              
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
              break;
            }
          }
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

      //    this->m_pFilter->GetLogger()->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, id: %d, packet start: %lld, packet stop: %lld, last start: %lld, last stop: %lld, decrease: %lld", MODULE_NAME, METHOD_GET_NEXT_PACKET_NAME, this->parserStreamId, pPacket->StreamId, pPacket->rtStart, pPacket->rtStop, timestamp->lastPacketStart, timestamp->lastPacketStop, timestamp->decreaseTimestamp);

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

      //    this->m_pFilter->GetLogger()->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, id: %d, packet start: %lld, packet stop: %lld, last start: %lld, last stop: %lld, decrease: %lld", MODULE_NAME, METHOD_GET_NEXT_PACKET_NAME, this->parserStreamId, pPacket->StreamId, pPacket->rtStart, pPacket->rtStop, timestamp->lastPacketStart, timestamp->lastPacketStop, timestamp->decreaseTimestamp);
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
  else if (SUCCEEDED(result) && (!this->IsRealDemuxingNeeded()))
  {
    ALLOC_MEM_DEFINE_SET(temp, unsigned char, MAXIMUM_MPEG2_TS_DATA_PACKET, 0);

    if (temp != NULL)
    {
      HRESULT res = this->DemuxerReadPosition(this->demuxerContextBufferPosition, temp, MAXIMUM_MPEG2_TS_DATA_PACKET, STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_NONZERO_DATA_LENGTH);

      if (res > 0)
      {
        CHECK_CONDITION_HRESULT(result, packet->GetBuffer()->InitializeBuffer((unsigned int)res), result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          packet->GetBuffer()->AddToBuffer(temp, (unsigned int)res);

          packet->SetStreamPid(0);
          packet->SetDemuxerId(this->demuxerId);

          this->demuxerContextBufferPosition += (unsigned int)res;
          result = S_OK;
        }
      }
      else if ((res < 0) && (res != E_CONNECTION_LOST_TRYING_REOPEN))
      {
        packet->SetStreamPid(0);
        packet->SetDemuxerId(this->demuxerId);

        result = res;
      }
    }

    FREE_MEM(temp);
  }

  return result;
}

HRESULT CDemuxer::OpenStream(AVIOContext *demuxerContext)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, demuxerContext);

  if (SUCCEEDED(result))
  {
    int ret; // return code from avformat functions

    if (SUCCEEDED(result))
    {
      // create the format context
      this->formatContext = avformat_alloc_context();
      this->formatContext->pb = demuxerContext;

      if (this->IsSetFlags(DEMUXER_FLAG_STREAM_IN_PACKETS))
      {
        FREE_MEM_CLASS(this->packetInputFormat);
        this->packetInputFormat = new CPacketInputFormat(&result, this, this->streamInputFormat);
      }

      if (SUCCEEDED(result))
      {
        ret = avformat_open_input(&this->formatContext, "", this->packetInputFormat, NULL);

        CHECK_CONDITION_EXECUTE(ret < 0, result = ret);

        if (SUCCEEDED(result))
        {
          ret = this->InitFormatContext();
          CHECK_CONDITION_EXECUTE(ret < 0, result = ret);
        }
      }

      CHECK_CONDITION_EXECUTE(FAILED(result), this->flags &= ~DEMUXER_FLAG_ALL_CONTAINERS);
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), CleanupFormatContext());

  return result;
}

HRESULT CDemuxer::InitFormatContext(void)
{
  HRESULT result = S_OK;

  if (SUCCEEDED(result))
  {
    const char *format = NULL;
    GetInputFormatInfo(this->formatContext->iformat, &format, NULL);

    result = ((format == NULL) && (this->formatContext->iformat == NULL)) ? E_FAIL : result;

    if (SUCCEEDED(result))
    {
      this->containerFormat = ConvertToUnicodeA((format != NULL) ? format : this->formatContext->iformat->name);
      CHECK_POINTER_HRESULT(result, this->containerFormat, result, E_OUTOFMEMORY);
    }

    if (SUCCEEDED(result))
    {
      this->flags &= ~DEMUXER_FLAG_VC1_SEEN_TIMESTAMP;

      this->flags |= (_wcsnicmp(this->containerFormat, L"flv", 3) == 0) ? DEMUXER_FLAG_FLV : DEMUXER_FLAG_NONE;
      this->flags |= (_wcsnicmp(this->containerFormat, L"asf", 3) == 0) ? DEMUXER_FLAG_ASF : DEMUXER_FLAG_NONE;
      this->flags |= (_wcsnicmp(this->containerFormat, L"mp4", 3) == 0) ? DEMUXER_FLAG_MP4 : DEMUXER_FLAG_NONE;
      this->flags |= (_wcsnicmp(this->containerFormat, L"matroska", 8) == 0) ? DEMUXER_FLAG_MATROSKA : DEMUXER_FLAG_NONE;
      this->flags |= (_wcsnicmp(this->containerFormat, L"ogg", 3) == 0) ? DEMUXER_FLAG_OGG : DEMUXER_FLAG_NONE;
      this->flags |= (_wcsnicmp(this->containerFormat, L"avi", 3) == 0) ? DEMUXER_FLAG_AVI : DEMUXER_FLAG_NONE;
      this->flags |= (_wcsnicmp(this->containerFormat, L"mpegts", 6) == 0) ? DEMUXER_FLAG_MPEG_TS : DEMUXER_FLAG_NONE;
      this->flags |= (_wcsicmp(this->containerFormat, L"mpeg") == 0) ? DEMUXER_FLAG_MPEG_PS : DEMUXER_FLAG_NONE;
      this->flags |= (_wcsicmp(this->containerFormat, L"rm") == 0) ? DEMUXER_FLAG_RM : DEMUXER_FLAG_NONE;

      if (AVFORMAT_GENPTS)
      {
        this->formatContext->flags |= AVFMT_FLAG_GENPTS;
      }

      this->formatContext->flags |= AVFMT_FLAG_IGNPARSERSYNC;

      // set minimum time for stream analysis in FFmpeg (1000 ms)
      this->formatContext->max_analyze_duration = 1000000;

      unsigned int startTime = GetTickCount();

      int ret = avformat_find_stream_info(this->formatContext, NULL);
      CHECK_CONDITION_EXECUTE(ret < 0, result = ret);
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, finished finding stream information (%s), duration: %u ms", MODULE_NAME, L"InitFormatContext()", this->demuxerId, ret < 0 ? L"failed" : L"succeeded", GetTickCount() - startTime);

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
        //bool hasPGS = false;

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
              CStream *newStream = new CStream(&result);
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

                CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = newStream->CreateStreamInfo(this->formatContext, stream, this->containerFormat));
                CHECK_CONDITION_EXECUTE(FAILED(result), stream->discard = AVDISCARD_ALL);
              }

              if (SUCCEEDED(result))
              {
                switch(stream->codec->codec_type)
                {
                case AVMEDIA_TYPE_VIDEO:
                  newStream->SetStreamType(CStream::Video);
                  result = (this->streams[CStream::Video]->Add(newStream)) ? result : E_OUTOFMEMORY;
                  break;
                case AVMEDIA_TYPE_AUDIO:
                  newStream->SetStreamType(CStream::Audio);
                  result = (this->streams[CStream::Audio]->Add(newStream)) ? result : E_OUTOFMEMORY;
                  break;
                case AVMEDIA_TYPE_SUBTITLE:
                  newStream->SetStreamType(CStream::Subpic);
                  result = (this->streams[CStream::Subpic]->Add(newStream)) ? result : E_OUTOFMEMORY;
                  break;
                default:
                  // unsupported stream
                  // normally this should be caught while creating the stream info already
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

            //hasPGS = (stream->codec->codec_id == CODEC_ID_HDMV_PGS_SUBTITLE) ? true : hasPGS;
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

        //if (hasPGS)
        //{
        //  CStream *stream = new CStream();
        //  CHECK_POINTER_HRESULT(result, stream, result, E_OUTOFMEMORY);

        //  if (SUCCEEDED(result))
        //  {
        //    stream->SetPid(FORCED_SUBTITLE_PID);
        //    result = stream->CreateStreamInfo();
        //  }

        //  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (stream->SetLanguage(L"und")) ? result : E_OUTOFMEMORY);

        //  if (SUCCEEDED(result))
        //  {
        //    // create media type
        //    CMediaType *mediaType = new CMediaType();
        //    CHECK_POINTER_HRESULT(result, mediaType, result, E_OUTOFMEMORY);

        //    if (SUCCEEDED(result))
        //    {
        //      mediaType->majortype = MEDIATYPE_Subtitle;
        //      mediaType->subtype = MEDIASUBTYPE_HDMVSUB;
        //      mediaType->formattype = FORMAT_SubtitleInfo;

        //      SUBTITLEINFO *subInfo = (SUBTITLEINFO *)mediaType->AllocFormatBuffer(sizeof(SUBTITLEINFO));
        //      CHECK_POINTER_HRESULT(result, subInfo, result, E_OUTOFMEMORY);

        //      if (SUCCEEDED(result))
        //      {
        //        memset(subInfo, 0, mediaType->FormatLength());
        //        wcscpy_s(subInfo->TrackName, FORCED_SUB_STRING);
        //        subInfo->dwOffset = sizeof(SUBTITLEINFO);

        //        result = (stream->GetStreamInfo()->GetMediaTypes()->Add(mediaType)) ? result : E_OUTOFMEMORY;
        //      }

        //      // there is no need to free subInfo
        //      // in case of error it is freed with ~CMediaType()
        //    }

        //    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(mediaType));
        //  }

        //  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (this->streams[CDemuxer::Subpic]->Add(stream)) ? result : E_OUTOFMEMORY);
        //  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(stream));
        //}
      }
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), this->CleanupFormatContext());
  return result;
}

// IPacketDemuxer interface

HRESULT CDemuxer::GetNextMediaPacket(CMediaPacket **mediaPacket, uint64_t flags)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, mediaPacket);

  if (SUCCEEDED(result))
  {
    CStreamPackage *package = new CStreamPackage(&result);
    CHECK_POINTER_HRESULT(result, package, result, E_OUTOFMEMORY);

    unsigned int requestId = this->demuxerContextRequestId++;
    if (SUCCEEDED(result))
    {
      CStreamPackagePacketRequest *request = new CStreamPackagePacketRequest(&result);
      CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        request->SetResetPacketCounter((flags & STREAM_PACKAGE_PACKET_REQUEST_FLAG_RESET_PACKET_COUNTER) == STREAM_PACKAGE_PACKET_REQUEST_FLAG_RESET_PACKET_COUNTER);
        request->SetId(requestId);
        request->SetStreamId(this->demuxerId);

        package->SetRequest(request);
      }

      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(request));
    }

    CHECK_HRESULT_EXECUTE(result, this->filter->ProcessStreamPackage(package));
    CHECK_HRESULT_EXECUTE(result, package->GetError());

    if (SUCCEEDED(result))
    {
      // successfully processed stream package request
      CStreamPackagePacketResponse *response = dynamic_cast<CStreamPackagePacketResponse *>(package->GetResponse());

      *mediaPacket = (CMediaPacket *)response->GetMediaPacket()->Clone();
      CHECK_POINTER_HRESULT(result, (*mediaPacket), result, E_OUTOFMEMORY);
    }

    CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, request %u, error: 0x%08X", MODULE_NAME, METHOD_DEMUXER_READ_NAME, this->demuxerId, requestId, result));

    FREE_MEM_CLASS(package);
  }

  return result;
}

int CDemuxer::StreamReadPosition(int64_t position, uint8_t *buffer, int length, uint64_t flags)
{
  return this->DemuxerReadPosition(position, buffer, length, flags);
}
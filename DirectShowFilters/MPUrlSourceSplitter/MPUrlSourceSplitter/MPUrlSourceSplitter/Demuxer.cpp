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

#define METHOD_CREATE_DEMUXER_READ_REQUEST_WORKER_NAME                      L"CreateDemuxerReadRequestWorker()"
#define METHOD_DESTROY_DEMUXER_READ_REQUEST_WORKER_NAME                     L"DestroyDemuxerReadRequestWorker()"
#define METHOD_DEMUXER_READ_REQUEST_WORKER_NAME                             L"DemuxerReadRequestWorker()"

#define METHOD_CREATE_DEMUXING_WORKER_NAME                                  L"CreateDemuxingWorker()"
#define METHOD_DESTROY_DEMUXING_WORKER_NAME                                 L"DestroyDemuxingWorker()"
#define METHOD_DEMUXING_WORKER_NAME                                         L"DemuxingWorker()"

#define METHOD_LOAD_MEDIA_PACKET_FOR_PROCESSING_NAME                        L"LoadMediaPacketForProcessing()"

#define DEMUXER_READ_BUFFER_SIZE								                            32768


//#define FLV_PACKET_MINIMUM_CHECKED                                          5           // minimum FLV packets to check in buffer
//#define FLV_DO_NOT_SEEK_DIFFERENCE                                          10000       // time in ms when FLV packet dts is closer to seek time
//#define FLV_SEEKING_POSITIONS                                               1024        // maximum FLV seeking positions

#define MAXIMUM_MPEG2_TS_DATA_PACKET                                        (188 * 55775)   // 55775 * 188 = 10485700 < 10 MB

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

CDemuxer::CDemuxer(CLogger *logger, IFilter *filter, CParameterCollection *configuration, HRESULT *result)
{
  this->logger = NULL;
  this->containerFormat = NULL;
  this->formatContext = NULL;
  this->flags = DEMUXER_FLAG_NONE;
  this->streamParseType = NULL;
  this->filter = NULL;
  //this->dontChangeTimestamps = false;
  //this->flvTimestamps = NULL;
  this->demuxerContextBufferPosition = 0;
  this->demuxerContext = NULL;
  this->createDemuxerWorkerShouldExit = false;
  this->createDemuxerWorkerThread = NULL;
  this->demuxerReadRequest = NULL;
  this->demuxerReadRequestMutex = NULL;
  this->demuxerReadRequestWorkerShouldExit = false;
  this->demuxerReadRequestId = 0;
  this->demuxerReadRequestWorkerThread = NULL;
  this->mediaPacketCollection = NULL;
  this->mediaPacketMutex = NULL;
  this->pauseSeekStopRequest = PAUSE_SEEK_STOP_REQUEST_NONE;
  this->parserStreamId = 0;
  this->configuration = NULL;
  this->outputPacketCollection = NULL;
  this->outputPacketMutex = NULL;
  this->mediaPacketCollectionCacheFile = NULL;
  this->streamInputFormat = NULL;
  this->demuxingWorkerThread = NULL;
  this->demuxingWorkerShouldExit = false;
  this->lastMediaPacket = 0;
  this->streamTime = 0;
  this->packetInputFormat = NULL;

  for (unsigned int i = 0; i < CStream::Unknown; i++)
  {
    this->streams[i] = NULL;
  }

  if (result != NULL)
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, logger);
    CHECK_POINTER_DEFAULT_HRESULT(*result, filter);
    CHECK_POINTER_DEFAULT_HRESULT(*result, configuration);

    if (SUCCEEDED(result))
    {
      this->logger = logger;
      this->filter = filter;

      //this->flvTimestamps = ALLOC_MEM_SET(this->flvTimestamps, FlvTimestamp, FLV_TIMESTAMP_MAX, 0);
      //CHECK_POINTER_HRESULT(*result, this->flvTimestamps, *result, E_OUTOFMEMORY);

      this->mediaPacketCollection = new CMediaPacketCollection();
      CHECK_POINTER_HRESULT(*result, this->mediaPacketCollection, *result, E_OUTOFMEMORY);

      this->outputPacketCollection = new COutputPinPacketCollection();
      CHECK_POINTER_HRESULT(*result, this->outputPacketCollection, *result, E_OUTOFMEMORY);

      this->configuration = new CParameterCollection();
      CHECK_POINTER_HRESULT(*result, this->configuration, *result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE(SUCCEEDED(*result), *result = (this->configuration->Append(configuration)) ? (*result) : E_OUTOFMEMORY);

      this->totalLength = 0;

      this->flags |= DEMUXER_FLAG_ESTIMATE_TOTAL_LENGTH;

      this->demuxerReadRequestMutex = CreateMutex(NULL, FALSE, NULL);
      this->mediaPacketMutex = CreateMutex(NULL, FALSE, NULL);
      this->outputPacketMutex = CreateMutex(NULL, FALSE, NULL);
      this->lastReceivedMediaPacketTime = GetTickCount();
      this->mediaPacketCollectionCacheFile = new CCacheFile();

      CHECK_POINTER_HRESULT(*result, this->demuxerReadRequestMutex, *result, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*result, this->mediaPacketMutex, *result, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*result, this->outputPacketMutex, *result, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*result, this->mediaPacketCollectionCacheFile, *result, E_OUTOFMEMORY);
    }

    if (SUCCEEDED(result))
    {
      for (unsigned int i = 0; i < CStream::Unknown; i++)
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
  // destroy create demuxer worker (if not finished earlier)
  this->DestroyCreateDemuxerWorker();

  // destroy demuxing worker (if not finished earlier)
  this->DestroyDemuxingWorker();

  // destroy demuxer read request worker
  this->DestroyDemuxerReadRequestWorker();

  FREE_MEM_CLASS(this->demuxerReadRequest);
  FREE_MEM_CLASS(this->mediaPacketCollection);
  FREE_MEM_CLASS(this->outputPacketCollection);

  if (this->demuxerReadRequestMutex != NULL)
  {
    CloseHandle(this->demuxerReadRequestMutex);
    this->demuxerReadRequestMutex = NULL;
  }
  
  if (this->mediaPacketMutex != NULL)
  {
    CloseHandle(this->mediaPacketMutex);
    this->mediaPacketMutex = NULL;
  }

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
  FREE_MEM(this->streamParseType);
  //FREE_MEM(this->flvTimestamps);
  FREE_MEM_CLASS(this->mediaPacketCollectionCacheFile);
  FREE_MEM_CLASS(this->configuration);
  FREE_MEM_CLASS(this->packetInputFormat);
}

HRESULT CDemuxer::SetStreamCount(unsigned int streamCount, bool liveStream)
{
  // this method should not be called
  return E_FAIL;
}

HRESULT CDemuxer::PushStreamReceiveData(unsigned int streamId, CStreamReceiveData *streamReceiveData)
{
  HRESULT result = S_OK;

  {
    CLockMutex lock(this->mediaPacketMutex, INFINITE);

    this->flags |= (streamReceiveData->IsContainer()) ? DEMUXER_FLAG_STREAM_IN_CONTAINER : DEMUXER_FLAG_NONE;
    this->flags |= (streamReceiveData->IsPackets()) ? DEMUXER_FLAG_STREAM_IN_PACKETS : DEMUXER_FLAG_NONE;

    if ((this->streamInputFormat == NULL) && (streamReceiveData->GetStreamInputFormat() != NULL))
    {
      this->streamInputFormat = Duplicate(streamReceiveData->GetStreamInputFormat());
      CHECK_POINTER_HRESULT(result, this->streamInputFormat, result, E_OUTOFMEMORY);
    }

    if (streamReceiveData->GetTotalLength()->IsSet())
    {
      this->totalLength = streamReceiveData->GetTotalLength()->GetTotalLength();
      
      this->flags &= ~DEMUXER_FLAG_ESTIMATE_TOTAL_LENGTH;
      this->flags |= (streamReceiveData->GetTotalLength()->IsEstimate()) ? DEMUXER_FLAG_ESTIMATE_TOTAL_LENGTH : DEMUXER_FLAG_NONE;
    }

    if (streamReceiveData->GetMediaPacketCollection()->Count() != 0)
    {
      // in case of splitter we process all media packets
      // in case of IPTV we assume that CMD_PLAY request come ASAP after Load() method is finished

      // remember last received media packet time
      this->lastReceivedMediaPacketTime = GetTickCount();

      CHECK_POINTER_DEFAULT_HRESULT(result, streamReceiveData->GetMediaPacketCollection());

      for (unsigned int i = 0; (SUCCEEDED(result)) && (i < streamReceiveData->GetMediaPacketCollection()->Count()); i++)
      {
        CMediaPacket *mediaPacket = streamReceiveData->GetMediaPacketCollection()->GetItem(i);

        CMediaPacketCollection *unprocessedMediaPackets = new CMediaPacketCollection();
        if (unprocessedMediaPackets->Add(static_cast<CMediaPacket *>(mediaPacket->Clone())))
        {
          int64_t start = mediaPacket->GetStart();
          int64_t stop = mediaPacket->GetEnd();

          result = S_OK;
          while ((unprocessedMediaPackets->Count() != 0) && (result == S_OK))
          {
            // there is still some unprocessed media packets
            // get first media packet
            CMediaPacket *unprocessedMediaPacket = static_cast<CMediaPacket *>(unprocessedMediaPackets->GetItem(0)->Clone());

            // remove first unprocessed media packet
            // its clone is going to be processed
            unprocessedMediaPackets->Remove(0);

            // set loaded to memory time
            unprocessedMediaPacket->SetLoadedToMemoryTime(this->lastReceivedMediaPacketTime);

            int64_t unprocessedMediaPacketStart = unprocessedMediaPacket->GetStart();
            int64_t unprocessedMediaPacketEnd = unprocessedMediaPacket->GetEnd();

            // try to find overlapping region
            CMediaPacket *region = this->mediaPacketCollection->GetOverlappedRegion(unprocessedMediaPacket);
            if (region != NULL)
            {
              if ((region->GetStart() == 0) && (region->GetEnd() == 0))
              {
                // there isn't overlapping media packet
                // whole packet can be added to collection
                result = (this->mediaPacketCollection->Add(static_cast<CMediaPacket *>(unprocessedMediaPacket->Clone()))) ? S_OK : E_FAIL;
              }
              else
              {
                // current unprocessed media packet is overlapping some media packet in media packet collection
                // it means that this packet has same data (in overlapping range)
                // there is no need to duplicate data in collection

                int64_t overlappingRegionStart = region->GetStart();
                int64_t overlappingRegionEnd = region->GetEnd();

                if (SUCCEEDED(result) && (unprocessedMediaPacketStart < overlappingRegionStart))
                {
                  // initialize part
                  int64_t start = unprocessedMediaPacketStart;
                  int64_t end = overlappingRegionStart - 1;
                  CMediaPacket *part = unprocessedMediaPacket->CreateMediaPacketBasedOnPacket(start, end);

                  result = (part != NULL) ? S_OK : E_POINTER;
                  if (SUCCEEDED(result))
                  {
                    result = (unprocessedMediaPackets->Add(part)) ? S_OK : E_FAIL;
                  }
                }

                if (SUCCEEDED(result) && (unprocessedMediaPacketEnd > overlappingRegionEnd))
                {
                  // initialize part
                  int64_t start = overlappingRegionEnd + 1;
                  int64_t end = unprocessedMediaPacketEnd;
                  CMediaPacket *part = unprocessedMediaPacket->CreateMediaPacketBasedOnPacket(start, end);

                  result = (part != NULL) ? S_OK : E_POINTER;
                  if (SUCCEEDED(result))
                  {
                    result = (unprocessedMediaPackets->Add(part)) ? S_OK : E_FAIL;
                  }
                }
              }
            }
            else
            {
              // there is serious error
              result = E_FAIL;
            }
            FREE_MEM_CLASS(region);

            // delete processed media packet
            FREE_MEM_CLASS(unprocessedMediaPacket);
          }
        }

        // media packets collection is not longer needed
        FREE_MEM_CLASS(unprocessedMediaPackets);
      }
    }

    if (streamReceiveData->GetEndOfStreamReached()->IsSet())
    {
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, media packet count: %u, stream position: %llu", MODULE_NAME, METHOD_PUSH_STREAM_RECEIVE_DATA_NAME, this->parserStreamId, this->mediaPacketCollection->Count(), streamReceiveData->GetEndOfStreamReached()->GetStreamPosition());

      if (!this->IsLiveStream())
      {
        // check media packets from supplied last valid stream position
        int64_t startPosition = 0;
        int64_t endPosition = 0;
        unsigned int mediaPacketIndex = this->mediaPacketCollection->GetMediaPacketIndexBetweenPositions(streamReceiveData->GetEndOfStreamReached()->GetStreamPosition());

        if (mediaPacketIndex != UINT_MAX)
        {
          CMediaPacket *mediaPacket = this->mediaPacketCollection->GetItem(mediaPacketIndex);
          startPosition = mediaPacket->GetStart();
          endPosition = mediaPacket->GetEnd();
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, for stream position '%llu' found media packet, start: %llu, end: %llu", MODULE_NAME, METHOD_PUSH_STREAM_RECEIVE_DATA_NAME, this->parserStreamId, streamReceiveData->GetEndOfStreamReached()->GetStreamPosition(), startPosition, endPosition);
        }

        for (int i = 0; i < 2; i++)
        {
          // because collection is sorted
          // then simple going through all media packets will reveal if there is some empty place
          while (mediaPacketIndex != UINT_MAX)
          {
            CMediaPacket *mediaPacket = this->mediaPacketCollection->GetItem(mediaPacketIndex);
            int64_t mediaPacketStart = mediaPacket->GetStart();
            int64_t mediaPacketEnd = mediaPacket->GetEnd();

            if (startPosition == mediaPacketStart)
            {
              // next start time is next to end of current media packet
              startPosition = mediaPacketEnd + 1;
              mediaPacketIndex++;

              if (mediaPacketIndex >= this->mediaPacketCollection->Count())
              {
                // stop checking, all media packets checked
                endPosition = startPosition;
                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, all media packets checked, start: %llu, end: %llu", MODULE_NAME, METHOD_PUSH_STREAM_RECEIVE_DATA_NAME, this->parserStreamId, startPosition, endPosition);
                mediaPacketIndex = UINT_MAX;
              }
            }
            else
            {
              // we found gap between media packets
              // set end time and stop checking media packets
              endPosition = mediaPacketStart - 1;
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, found gap between media packets, start: %llu, end: %llu", MODULE_NAME, METHOD_PUSH_STREAM_RECEIVE_DATA_NAME, this->parserStreamId, startPosition, endPosition);
              mediaPacketIndex = UINT_MAX;
            }
          }

          if ((!this->IsEstimateTotalLength()) && (startPosition >= this->totalLength) && (i == 0))
          {
            // we are after end of stream
            // check media packets from start if we don't have gap
            startPosition = 0;
            endPosition = 0;
            mediaPacketIndex = this->mediaPacketCollection->GetMediaPacketIndexBetweenPositions(startPosition);
            this->flags |= DEMUXER_FLAG_TOTAL_LENGTH_RECEIVED;
            this->logger->Log(LOGGER_VERBOSE, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_PUSH_STREAM_RECEIVE_DATA_NAME, this->parserStreamId, L"searching for gap in media packets from beginning");
          }
          else
          {
            // we found some gap
            break;
          }
        }

        if (((!this->IsEstimateTotalLength()) && (startPosition < this->totalLength)) || (this->IsEstimateTotalLength()))
        {
          // found part which is not downloaded
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, requesting stream part from: %llu, to: %llu", MODULE_NAME, METHOD_PUSH_STREAM_RECEIVE_DATA_NAME, this->parserStreamId, startPosition, endPosition);

          this->filter->SeekToPosition(startPosition, endPosition);
        }
        else
        {
          // all data received
          this->flags |= DEMUXER_FLAG_ALL_DATA_RECEIVED;
          this->logger->Log(LOGGER_VERBOSE, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_PUSH_STREAM_RECEIVE_DATA_NAME, this->parserStreamId, L"all data received");

          // if downloading file, download callback can be called after storing all data to download file
        }
      }
      else
      {
        // live stream, all data received
        this->flags |= DEMUXER_FLAG_ALL_DATA_RECEIVED;
        this->logger->Log(LOGGER_VERBOSE, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_PUSH_STREAM_RECEIVE_DATA_NAME, this->parserStreamId, L"all data received");

        // if downloading file, download callback can be called after storing all data to download file
      }
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_PUSH_STREAM_RECEIVE_DATA_NAME, this->parserStreamId, result));
  return result;
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
        result = packet->CreateBuffer(outputPacket->GetBuffer()->GetBufferOccupiedSpace()) ? result : E_OUTOFMEMORY;
        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), packet->GetBuffer()->AddToBufferWithResize(outputPacket->GetBuffer()));
      }

      packet->SetStreamPid(outputPacket->GetStreamPid());
      packet->SetDemuxerId(outputPacket->GetDemuxerId());
      packet->SetStartTime(outputPacket->GetStartTime());
      packet->SetEndTime(outputPacket->GetEndTime());
      packet->SetFlags(outputPacket->GetFlags());
      packet->SetMediaType(outputPacket->GetMediaType());
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

unsigned int CDemuxer::GetParserStreamId(void)
{
  return this->parserStreamId;
}

wchar_t *CDemuxer::GetMediaPacketCollectionCacheFilePath(void)
{
  wchar_t *result = NULL;
  const wchar_t *folder = this->configuration->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

  if (folder != NULL)
  {
    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());

    if (guid != NULL)
    {
      result = FormatString(L"%smpurlsourcesplitter_%s_demuxer_%08u.temp", folder, guid, this->parserStreamId);
    }

    FREE_MEM(guid);
  }

  return result;
}

IFilter *CDemuxer::GetFilter(void)
{
  return this->filter;
}

const wchar_t *CDemuxer::GetCacheFilePath(void)
{
  return (this->mediaPacketCollectionCacheFile != NULL) ? this->mediaPacketCollectionCacheFile->GetCacheFile() : NULL;
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

void CDemuxer::SetParserStreamId(unsigned int parserStreamId)
{
  this->parserStreamId = parserStreamId;
}

void CDemuxer::SetPauseSeekStopRequest(bool pauseSeekStopRequest)
{
  this->pauseSeekStopRequest = pauseSeekStopRequest ? PAUSE_SEEK_STOP_REQUEST_DISABLE_ALL : PAUSE_SEEK_STOP_REQUEST_NONE;
}

void CDemuxer::SetLiveStream(bool liveStream)
{
  this->flags &= ~DEMUXER_FLAG_LIVE_STREAM;
  this->flags |= (liveStream) ? DEMUXER_FLAG_LIVE_STREAM : DEMUXER_FLAG_NONE;
}

void CDemuxer::SetRealDemuxingNeeded(bool realDemuxingNeeded)
{
  this->flags &= ~DEMUXER_FLAG_REAL_DEMUXING_NEEDED;
  this->flags |= (realDemuxingNeeded) ? DEMUXER_FLAG_REAL_DEMUXING_NEEDED : DEMUXER_FLAG_NONE;
}

/* other methods */

bool CDemuxer::IsSetFlags(unsigned int flags)
{
  return ((this->flags & flags) == flags);
}

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

bool CDemuxer::IsAllDataReceived(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_ALL_DATA_RECEIVED);
}

bool CDemuxer::IsTotalLengthReceived(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_TOTAL_LENGTH_RECEIVED);
}

bool CDemuxer::IsCreatedDemuxer(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_CREATED_DEMUXER);
}

bool CDemuxer::IsLiveStream(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_LIVE_STREAM);
}

bool CDemuxer::IsEstimateTotalLength(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_ESTIMATE_TOTAL_LENGTH);
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
  return ((this->demuxerReadRequestWorkerThread != NULL) && (this->createDemuxerWorkerThread != NULL));
}

bool CDemuxer::IsEndOfStreamOutputPacketQueued(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_END_OF_STREAM_OUTPUT_PACKET_QUEUED);
}
  
HRESULT CDemuxer::StartCreatingDemuxer(void)
{
  HRESULT result = S_OK;

  if (this->demuxerReadRequestWorkerThread == NULL)
  {
    result = this->CreateDemuxerReadRequestWorker();
  }

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
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_SEEK_NAME, this->parserStreamId);
  this->logger->Log(LOGGER_INFO, L"%s: %s: stream %u, seeking to time: %lld", MODULE_NAME, METHOD_SEEK_NAME, this->parserStreamId, time);

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
      this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, first seek by position failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, this->parserStreamId, result);

      result = this->SeekByPosition(time, AVSEEK_FLAG_ANY);
      if (FAILED(result))
      {
        this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, second seek by position failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, this->parserStreamId, result);
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
      this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, first seek by time failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, this->parserStreamId, result);

      flags = 0;
      result = this->SeekByTime(time, flags);    // seek forward
      if (FAILED(result))
      {
        this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, second seek by time failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, this->parserStreamId, result);
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
      this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, first seek by sequence reading failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, this->parserStreamId, result);

      result = this->SeekBySequenceReading(time, flags | AVSEEK_FLAG_ANY);
      if (FAILED(result))
      {
        this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, second seek by sequence reading failed: 0x%08X", MODULE_NAME, METHOD_SEEK_NAME, this->parserStreamId, result);
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
    this->logger->Log(LOGGER_WARNING, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_NAME, this->parserStreamId, L"didn't seek by position or time");
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
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_END_FORMAT, MODULE_NAME, METHOD_SEEK_NAME, this->parserStreamId);

  return (seeked) ? S_OK : E_FAIL;
}

void CDemuxer::ReportStreamTime(uint64_t streamTime)
{
  this->streamTime = streamTime;
}

/* protected methods */

void CDemuxer::CleanupFormatContext(void)
{
  if (this->formatContext)
  {
    avformat_close_input(&this->formatContext);
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
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId);

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
    this->logger->Log(LOGGER_ERROR, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, L"no stream to seek");
    result = -1;
  }

  if ((time >= 0) && (streamId != (-1)))
  {
      AVStream *stream = this->formatContext->streams[streamId];
      seek_pts = ConvertRTToTimestamp(time, stream->time_base.num, stream->time_base.den, (int64_t)AV_NOPTS_VALUE);
  }

  if (SUCCEEDED(result))
  {
    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, time: %lld, seek_pts: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, time, seek_pts);

    st = this->formatContext->streams[streamId];

    // check is requested time is not in buffer

    bool found = false;
    int index = -1;

    ff_read_frame_flush(this->formatContext);
    index = av_index_search_timestamp(st, seek_pts, flags);

    if (!found) 
    {
      st->nb_index_entries = 0;
      st->nb_frames = 0;
      
      // notify protocol that we can't receive any data
      // protocol have to supress sending data and will wait until we are ready
      this->filter->SetSupressData(this->parserStreamId, true);

      // seek to time
      int64_t seekedTime = this->filter->SeekToTime(this->parserStreamId, time / (DSHOW_TIME_BASE / 1000)); // (1000 / DSHOW_TIME_BASE)

      {
        // lock access to media packets and output packets
        CLockMutex mediaPacketLock(this->mediaPacketMutex, INFINITE);
        CLockMutex outputPacketLock(this->outputPacketMutex, INFINITE);

        // clear output packets
        this->outputPacketCollection->Clear();

        // clear media packets, we are starting from beginning
        // delete buffer file and set buffer position to zero
        this->mediaPacketCollection->Clear();
        this->mediaPacketCollectionCacheFile->Clear();
        this->demuxerContextBufferPosition = 0;
        this->lastMediaPacket = 0;
        this->lastReceivedMediaPacketTime = GetTickCount();

        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, setting total length to zero, estimate: %d", MODULE_NAME, METHOD_SEEK_TO_TIME_NAME, this->parserStreamId, SUCCEEDED(result) ? 1 : 0);
        this->totalLength = 0;
        this->flags &= ~(DEMUXER_FLAG_ESTIMATE_TOTAL_LENGTH | DEMUXER_FLAG_END_OF_STREAM_OUTPUT_PACKET_QUEUED);
        this->flags |= (SUCCEEDED(result)) ? DEMUXER_FLAG_ESTIMATE_TOTAL_LENGTH : DEMUXER_FLAG_NONE;
      }

      // if correctly seeked than reset flag that all data are received
      // in another case we don't received any other data
      this->flags &= ~DEMUXER_FLAG_ALL_DATA_RECEIVED;
      this->flags |= (seekedTime < 0) ? DEMUXER_FLAG_ALL_DATA_RECEIVED : DEMUXER_FLAG_NONE;

      // now we are ready to receive data
      // notify protocol that we can receive data
      this->filter->SetSupressData(this->parserStreamId, false);

      if ((seekedTime < 0) || (seekedTime > (time / (DSHOW_TIME_BASE / 1000))))
      {
        this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, invalid seek time returned: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, seekedTime);
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
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking by internal MP4 format time seeking method", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId);
          ff_read_frame_flush(this->formatContext);

          int ret = this->formatContext->iformat->read_seek(this->formatContext, streamId, seek_pts, flags);
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking by internal format time seeking method result: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, ret);

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

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeked to time: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, seekedTime);
    }

    ff_read_frame_flush(this->formatContext);

    if (SUCCEEDED(result) && (!found))
    {
      st = this->formatContext->streams[streamId];
      ff_read_frame_flush(this->formatContext);
      index = av_index_search_timestamp(st, seek_pts, flags);

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, index: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, index);

      if ((index < 0) && (st->nb_index_entries > 0) && (seek_pts < st->index_entries[0].timestamp))
      {
        this->logger->Log(LOGGER_ERROR, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, L"failing");
        result = -3;
      }

      if (SUCCEEDED(result) && (index >= 0) && (st->nb_index_entries > 0))
      {
        ie = &st->index_entries[index];
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, timestamp: %lld, seek_pts: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, ie->timestamp, seek_pts);
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
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, index entries: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, st->nb_index_entries);
          AVPacket avPacket;

          if ((st->nb_index_entries) && (index >= 0))
          {
            ie = &st->index_entries[index];

            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking to position: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, ie->pos);
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
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, flushing, seeking to position: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, 0);
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
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, index timestamp: %lld, index position: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, ie->timestamp, ie->pos);
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
                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, av_read_frame() returned error: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, read_status);
                break;
              }

              av_free_packet(&avPacket);

              if (streamId == avPacket.stream_index)
              {
                found = true;
                break;

                //if (avPacket.flags & AV_PKT_FLAG_KEY)
                //{
                //  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, found keyframe with timestamp: %lld, position: %lld, stream index: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, avPacket.dts, avPacket.pos, streamId);
                //  found = true;
                //  break;
                //}

                //if((nonkey++ > 1000) && (st->codec->codec_id != CODEC_ID_CDGRAPHICS))
                //{
                //  this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, failed as this stream seems to contain no keyframes after the target timestamp, %d non keyframes found", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, nonkey);
                //  //found = true;
                //  break;
                //}
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
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, searching keyframe with timestamp: %lld, stream index: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, seek_pts, streamId);
      index = av_index_search_timestamp(st, seek_pts, flags);

      if (index < 0)
      {
        this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, index lower than zero: %d, setting to zero", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, index);
        index = 0;
      }

      if (SUCCEEDED(result))
      {
        ie = &st->index_entries[index];

        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seek to position: %lld, time: %lld", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, ie->pos, ie->timestamp);

        int64_t ret = avio_seek(this->formatContext->pb, ie->pos, SEEK_SET);
        if (ret < 0)
        {
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seek to requested position %lld failed: %d", MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, ie->pos, ret);
          result = -7;
        }

        if (SUCCEEDED(result))
        {
          ff_update_cur_dts(this->formatContext, st, ie->timestamp);
        }
      }
    }
  }

  this->logger->Log(LOGGER_INFO, SUCCEEDED(result) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_SEEK_BY_TIME_NAME, this->parserStreamId, result);
  return result;
}

HRESULT CDemuxer::SeekByPosition(REFERENCE_TIME time, int flags)
{
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId);

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
    this->logger->Log(LOGGER_ERROR, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, L"no stream to seek");
    result = -1;
  }

  if ((time >= 0) && (streamId != (-1)))
  {
      AVStream *stream = this->formatContext->streams[streamId];
      seek_pts = ConvertRTToTimestamp(time, stream->time_base.num, stream->time_base.den, (int64_t)AV_NOPTS_VALUE);
  }

  bool found = false;

  // if it isn't FLV video, try to seek by internal FFmpeg time seeking method
  if (SUCCEEDED(result) && (!this->IsFlv()))
  {
    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, time: %lld, seek_pts: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, time, seek_pts);

    int ret = 0;
    if (this->formatContext->iformat->read_seek)
    {
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking by internal format time seeking method", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId);
      ff_read_frame_flush(this->formatContext);
      ret = this->formatContext->iformat->read_seek(this->formatContext, streamId, seek_pts, flags);
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking by internal format time seeking method result: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, ret);
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
    AVStream *st = this->formatContext->streams[streamId];

    int index = -1;

    ff_read_frame_flush(this->formatContext);
    index = av_index_search_timestamp(st, seek_pts, flags);

    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, index: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, index);

    if ((index < 0) && (st->nb_index_entries) && (seek_pts < st->index_entries[0].timestamp))
    {
      this->logger->Log(LOGGER_ERROR, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, L"failing");
      result = -2;
    }

    if (SUCCEEDED(result) && (index >= 0))
    {
      AVIndexEntry *ie = &st->index_entries[index];
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, timestamp: %lld, seek_pts: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, ie->timestamp, seek_pts);
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
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, index entries: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, st->nb_index_entries);
        AVPacket avPacket;
        AVIndexEntry *ie = NULL;

        if ((st->nb_index_entries) && (index >= 0))
        {
          ie = &st->index_entries[index];

          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking to position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, ie->pos);
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
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking to position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, this->formatContext->data_offset);
          if (avio_seek(this->formatContext->pb, this->formatContext->data_offset, SEEK_SET) < 0)
          {
            result = -4;
          }
        }

        if (SUCCEEDED(result) && this->IsFlv())
        {
          if (ie != NULL)
          {
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, index timestamp: %lld, index position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, ie->timestamp, ie->pos);
          }

          // enable reading from seek method, do not allow (yet) to read from demuxing worker
          this->pauseSeekStopRequest = PAUSE_SEEK_STOP_REQUEST_DISABLE_DEMUXING;

          CFlvPacket *flvPacket = new CFlvPacket();
          ALLOC_MEM_DEFINE_SET(buffer, unsigned char, FLV_SEEKING_TOTAL_BUFFER_SIZE, 0);
          ALLOC_MEM_DEFINE_SET(flvSeekBoundaries, FlvSeekPosition, FLV_BOUNDARIES_COUNT, 0);

          CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);
          CHECK_POINTER_HRESULT(result, flvSeekBoundaries, result, E_OUTOFMEMORY);
          CHECK_POINTER_HRESULT(result, flvPacket, result, E_OUTOFMEMORY);

          flvSeekBoundaries[FLV_SEEK_LOWER_BOUNDARY].time = 0;
          flvSeekBoundaries[FLV_SEEK_LOWER_BOUNDARY].position = 0;

          flvSeekBoundaries[FLV_SEEK_UPPER_BOUNDARY].time = this->GetDuration() / (DSHOW_TIME_BASE / 1000);
          flvSeekBoundaries[FLV_SEEK_UPPER_BOUNDARY].position = this->totalLength;

          int64_t seekPosition = avio_seek(this->formatContext->pb, 0, SEEK_CUR);

          // read stream until we find requested time
          while ((!found) && SUCCEEDED(result))
          {
            unsigned int bufferPosition = 0;
            int64_t flvPacketOffset = -1;

            // synchronize within stream to first FLV packet after seekPosition
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, seeking to position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, seekPosition);
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
                this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, avio_read() returned error: %d", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, readBytes);
                result = -6;
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
                    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, no FLV packet or candidate found in data, length: %u", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, bufferPosition);
                    break;
                  case FLV_FIND_RESULT_NOT_ENOUGH_DATA_FOR_HEADER:
                    // not enough data for FLV header in buffer
                    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, not enough data for FLV header, length: %u", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, bufferPosition);
                    break;
                  case FLV_FIND_RESULT_NOT_ENOUGH_MEMORY:
                    // this should not happen
                    result = E_OUTOFMEMORY;
                    break;
                  case FLV_FIND_RESULT_NOT_FOUND_MINIMUM_PACKETS:
                    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, not found minimum FLV packets, length: %u", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, bufferPosition);
                    // found several FLV packets, but lower than requested
                    break;
                  }
                }
                else
                {
                  // found at least requested count of FLV packets, the position of first is in flvFindResult
                  // seek to specified position, read avPacket and check dts
                  flvPacketOffset = (unsigned int)flvFindResult;
                  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, found FLV packet at position: %lld", MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, seekPosition + flvPacketOffset);
                  break;
                }
              }
            }
            CHECK_CONDITION_EXECUTE(flvPacketOffset == (-1), result = -7);

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

                CHECK_CONDITION_EXECUTE(read_status < 0, result = -8);
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

          FREE_MEM(buffer);
          FREE_MEM(flvSeekBoundaries);
          FREE_MEM_CLASS(flvPacket);
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

  this->logger->Log(LOGGER_INFO, SUCCEEDED(result) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, result);
  return result;
}

HRESULT CDemuxer::SeekBySequenceReading(REFERENCE_TIME time, int flags)
{
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, this->parserStreamId);

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
    this->logger->Log(LOGGER_ERROR, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_SEEK_BY_POSITION_NAME, this->parserStreamId, L"no stream to seek");
    result = -1;
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
    this->pauseSeekStopRequest = PAUSE_SEEK_STOP_REQUEST_DISABLE_DEMUXING;

    while (SUCCEEDED(result))
    {
      do
      {
        read_status = av_read_frame(this->formatContext, &avPacket);
      } while (read_status == AVERROR(EAGAIN));

      CHECK_CONDITION_EXECUTE(read_status < 0, result = -2);
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
      int index = -1;

      if (st->nb_index_entries != 0)
      {
        index = av_index_search_timestamp(st, seek_pts, flags);
      }

      AVIndexEntry *ie = (index != -1) ? &st->index_entries[index] : NULL;
      int64_t seekPosition = (ie != NULL) ? ie->pos : this->formatContext->data_offset;

      avio_seek(this->formatContext->pb, seekPosition, SEEK_SET);
    }

    // continuosly read until we found avPacket with dts higher than seek_pts (or error)

    while (SUCCEEDED(result))
    {
      do
      {
        read_status = av_read_frame(this->formatContext, &avPacket);
      } while (read_status == AVERROR(EAGAIN));

      CHECK_CONDITION_EXECUTE(read_status < 0, result = -3);
      av_free_packet(&avPacket);

      if ((streamId != avPacket.stream_index) || (avPacket.dts < 0))
      {
        // continue reading next avPacket, because we don't have avPacket with right stream index or avPacket doesn't have dts
        continue;
      }

      if (avPacket.dts > seek_pts)
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

  this->logger->Log(LOGGER_INFO, SUCCEEDED(result) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_SEEK_BY_SEQUENCE_READING_NAME, this->parserStreamId, result);
  return result;
}

unsigned int WINAPI CDemuxer::CreateDemuxerWorker(LPVOID lpParam)
{
  CDemuxer *caller = (CDemuxer *)lpParam;

  caller->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXER_WORKER_NAME, caller->parserStreamId);

  //while ((!caller->createDemuxerWorkerShouldExit) && (!caller->IsCreatedDemuxer()) && (!caller->IsAllDataReceived()) && (caller->GetParserHosterStatus() >= STATUS_NONE))
  while ((!caller->createDemuxerWorkerShouldExit) && (!caller->IsCreatedDemuxer()) && (!caller->IsAllDataReceived()))
  {
    if (!caller->IsCreatedDemuxer())
    {
      caller->demuxerContextBufferPosition = 0;

      HRESULT result = S_OK;

      if (SUCCEEDED(result) && (caller->IsRealDemuxingNeeded()))
      {
        if (caller->demuxerContext == NULL)
        {
          uint8_t *buffer = (uint8_t *)av_mallocz(DEMUXER_READ_BUFFER_SIZE + FF_INPUT_BUFFER_PADDING_SIZE);
          caller->demuxerContext = avio_alloc_context(buffer, DEMUXER_READ_BUFFER_SIZE, 0, caller, DemuxerRead, NULL, DemuxerSeek);
        }

        CHECK_POINTER_HRESULT(result, caller->demuxerContext, result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), caller->logger->Log(LOGGER_ERROR, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXER_WORKER_NAME, caller->parserStreamId, L"not enough memory to allocate AVIOContext"));

        if (SUCCEEDED(result))
        {
          result = caller->OpenStream(caller->demuxerContext);

          if (FAILED(result))
          {
            // clean up
            av_free(caller->demuxerContext->buffer);
            av_free(caller->demuxerContext);
            caller->demuxerContext = NULL;
            caller->demuxerContextBufferPosition = 0;
          }
        }
      }

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

    Sleep(100);
  }

  caller->logger->Log(LOGGER_INFO, METHOD_DEMUXER_END_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXER_WORKER_NAME, caller->parserStreamId);
  caller->flags |= DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED;

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}

HRESULT CDemuxer::CreateCreateDemuxerWorker(void)
{
  HRESULT result = S_OK;

  this->flags &= ~DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED;

  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_CREATE_CREATE_DEMUXER_WORKER_NAME, this->parserStreamId);

  this->createDemuxerWorkerShouldExit = false;

  this->createDemuxerWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CDemuxer::CreateDemuxerWorker, this, 0, NULL);

  if (this->createDemuxerWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, _beginthreadex() error: 0x%08X", MODULE_NAME, METHOD_CREATE_CREATE_DEMUXER_WORKER_NAME, this->parserStreamId, result);
    this->flags |= DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED;
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_CREATE_CREATE_DEMUXER_WORKER_NAME, this->parserStreamId, result);
  return result;
}

HRESULT CDemuxer::DestroyCreateDemuxerWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_DESTROY_CREATE_DEMUXER_WORKER_NAME, this->parserStreamId);

  this->createDemuxerWorkerShouldExit = true;

  // wait for the create demuxer worker thread to exit
  if (this->createDemuxerWorkerThread != NULL)
  {
    if (WaitForSingleObject(this->createDemuxerWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_DESTROY_CREATE_DEMUXER_WORKER_NAME, this->parserStreamId, L"thread didn't exit, terminating thread");
      TerminateThread(this->createDemuxerWorkerThread, 0);
    }
    CloseHandle(this->createDemuxerWorkerThread);
  }

  this->createDemuxerWorkerThread = NULL;
  this->createDemuxerWorkerShouldExit = false;
  this->flags |= DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED;

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DESTROY_CREATE_DEMUXER_WORKER_NAME, this->parserStreamId, result);
  return result;
}

int CDemuxer::DemuxerReadPosition(int64_t position, uint8_t *buffer, int length)
{
  HRESULT result = S_OK;
  CHECK_CONDITION(result, length >= 0, S_OK, E_INVALIDARG);
  CHECK_POINTER_DEFAULT_HRESULT(result, buffer);

  if ((SUCCEEDED(result)) && (length > 0) && (this->demuxerReadRequest == NULL))
  {
    {
      // lock access to demuxer read request
      CLockMutex lock(this->demuxerReadRequestMutex, INFINITE);

      this->demuxerReadRequest = new CAsyncRequest();
      CHECK_POINTER_HRESULT(result, this->demuxerReadRequest, result, E_OUTOFMEMORY);

      result = this->demuxerReadRequest->Request(this->demuxerReadRequestId++, position, length, buffer, NULL);

      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(this->demuxerReadRequest));
    }

    if (SUCCEEDED(result))
    {
      DWORD ticks = GetTickCount();
      DWORD timeout = this->filter->GetReceiveDataTimeout();

      result = (timeout != UINT_MAX) ? S_OK : E_UNEXPECTED;

      if (SUCCEEDED(result))
      {
        // if ranges are not supported than we must wait for data

        result = VFW_E_TIMEOUT;

        // wait until request is completed or cancelled
        while (!this->demuxerReadRequestWorkerShouldExit)
        {
          unsigned int seekingCapabilities = this->filter->GetSeekingCapabilities();

          {
            // lock access to demuxer read request
            CLockMutex lock(this->demuxerReadRequestMutex, INFINITE);

            if ((!this->IsEstimateTotalLength()) && (this->demuxerReadRequest->GetStart() >= this->totalLength))
            {
              // something bad occured
              // graph requests data that are beyond stream (data doesn't exists)
              this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, graph requests data beyond stream, stream total length: %llu, request start: %llu", MODULE_NAME, METHOD_DEMUXER_READ_NAME, this->parserStreamId, this->totalLength, this->demuxerReadRequest->GetStart());
              // complete result with error code
              this->demuxerReadRequest->Complete(E_REQUESTED_DATA_AFTER_TOTAL_LENGTH);
            }

            if (this->demuxerReadRequest->GetState() == CAsyncRequest::Completed)
            {
              // request is completed, return error or readed data length
              result = SUCCEEDED(this->demuxerReadRequest->GetErrorCode()) ? this->demuxerReadRequest->GetBufferLength() : this->demuxerReadRequest->GetErrorCode();
              break;
            }
            else if (this->demuxerReadRequest->GetState() == CAsyncRequest::WaitingIgnoreTimeout)
            {
              // we are waiting for data and we have to ignore timeout
            }
            else
            {
              // common case, not for live stream
              if ((!this->IsLiveStream()) && (seekingCapabilities != SEEKING_METHOD_NONE) && ((GetTickCount() - ticks) > timeout))
              {
                // if seeking is supported and timeout occured then stop waiting for data and exit with VFW_E_TIMEOUT error
                result = VFW_E_TIMEOUT;
                break;
              }
            }
          }

          // sleep some time
          Sleep(10);
        }
      }

      {
        // lock access to demuxer read request
        CLockMutex lock(this->demuxerReadRequestMutex, INFINITE);

        FREE_MEM_CLASS(this->demuxerReadRequest);
      }

      if (FAILED(result))
      {
        this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, requesting data from position: %llu, length: %lu, request id: %u, result: 0x%08X", MODULE_NAME, METHOD_DEMUXER_READ_NAME, this->parserStreamId, this->demuxerContextBufferPosition, length, this->demuxerReadRequestId, result);
      }
    }
  }
  else if ((SUCCEEDED(result)) && (length > 0) && (this->demuxerReadRequest != NULL))
  {
    this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, current read request is not finished, current read request: position: %llu, length: %lu, new request: position: %llu, length: %lu", MODULE_NAME, METHOD_DEMUXER_READ_NAME, this->parserStreamId, this->demuxerReadRequest->GetStart(), this->demuxerReadRequest->GetBufferLength(), this->demuxerContextBufferPosition, length);

    result = -1;
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DEMUXER_READ_NAME, this->parserStreamId, result));

  return SUCCEEDED(result) ? result : (-1);
}

int CDemuxer::DemuxerRead(void *opaque, uint8_t *buf, int buf_size)
{
  CDemuxer *demuxer = static_cast<CDemuxer *>(opaque);
  int result = 0;

  result = demuxer->DemuxerReadPosition(demuxer->demuxerContextBufferPosition, buf, buf_size);

  if (result > 0)
  {
    // in case of success is in result is length of returned data
    demuxer->demuxerContextBufferPosition += result;
  }

  return result;
}

int64_t CDemuxer::DemuxerSeek(void *opaque,  int64_t offset, int whence)
{
  CDemuxer *demuxer = static_cast<CDemuxer *>(opaque);

  //CHECK_CONDITION_EXECUTE((!filter->IsAvi()) && (filter->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), filter->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DEMUXER_SEEK_NAME));

  LONGLONG total = 0;
  LONGLONG available = 0;

  demuxer->Length(&total, &available);

  int64_t result = 0;
  bool resultSet = false;

  if (whence == SEEK_SET)
  {
	  demuxer->demuxerContextBufferPosition = offset;
    //CHECK_CONDITION_EXECUTE((!filter->IsAvi()) && (filter->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), filter->logger->Log(LOGGER_INFO, L"%s: %s: offset: %lld, SEEK_SET", MODULE_NAME, METHOD_DEMUXER_SEEK_NAME, offset));
  }
  else if (whence == SEEK_CUR)
  {
    demuxer->demuxerContextBufferPosition += offset;
    //CHECK_CONDITION_EXECUTE((!filter->IsAvi()) && (filter->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), filter->logger->Log(LOGGER_INFO, L"%s: %s: offset: %lld, SEEK_CUR", MODULE_NAME, METHOD_DEMUXER_SEEK_NAME, offset));
  }
  else if (whence == SEEK_END)
  {
    demuxer->demuxerContextBufferPosition = total - offset;
    //CHECK_CONDITION_EXECUTE((!filter->IsAvi()) && (filter->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), filter->logger->Log(LOGGER_INFO, L"%s: %s: offset: %lld, SEEK_END", MODULE_NAME, METHOD_DEMUXER_SEEK_NAME, offset));
  }
  else if (whence == AVSEEK_SIZE)
  {
    result = total;
    resultSet = true;
    //CHECK_CONDITION_EXECUTE((!filter->IsAvi()) && (filter->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), filter->logger->Log(LOGGER_INFO, L"%s: %s: offset: %lld, AVSEEK_SIZE", MODULE_NAME, METHOD_DEMUXER_SEEK_NAME, offset));
  }
  else
  {
    result = E_INVALIDARG;
    resultSet = true;
    //filter->logger->Log(LOGGER_ERROR, L"%s: %s: offset: %lld, unknown seek value", MODULE_NAME, METHOD_DEMUXER_SEEK_NAME, offset);
  }

  if (!resultSet)
  {
    result = demuxer->demuxerContextBufferPosition;
    resultSet = true;
  }

  //CHECK_CONDITION_EXECUTE((!filter->IsAvi()) && (filter->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), filter->logger->Log(LOGGER_INFO, L"%s: %s: End, result: %lld", MODULE_NAME, METHOD_DEMUXER_SEEK_NAME, result));
  return result;
}

unsigned int WINAPI CDemuxer::DemuxerReadRequestWorker(LPVOID lpParam)
{
  CDemuxer *caller = (CDemuxer *)lpParam;
  caller->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, caller->parserStreamId);

  DWORD lastCheckTime = GetTickCount();
  // holds last waiting request id to avoid multiple message logging
  unsigned int lastWaitingRequestId = 0;

  while (!caller->demuxerReadRequestWorkerShouldExit)
  {
    {
      // lock access to demuxer read requests
      CLockMutex requestLock(caller->demuxerReadRequestMutex, INFINITE);

      if (caller->demuxerReadRequest != NULL)
      {
        CAsyncRequest *request = caller->demuxerReadRequest;

        // check if demuxer worker should be finished
        if (caller->createDemuxerWorkerShouldExit)
        {
          // deny request and report as failed
          request->Complete(E_DEMUXER_WORKER_STOP_REQUEST);
        }

        //if (FAILED(caller->GetParserHosterStatus()))
        //{
        //  // there is unrecoverable error while receiving data
        //  // signalize, that we received all data and no other data come
        //  request->Complete(caller->GetParserHosterStatus());
        //}

        if ((request->GetState() == CAsyncRequest::Waiting) || (request->GetState() == CAsyncRequest::WaitingIgnoreTimeout))
        {
          // process only waiting requests
          // variable to store found data length
          unsigned int foundDataLength = 0;
          HRESULT result = S_OK;
          // current stream position is get only when media packet for request is not found
          int64_t currentStreamPosition = -1;

          // first try to find starting media packet (packet which have first data)
          unsigned int packetIndex = UINT_MAX;
          {
            // lock access to media packets
            CLockMutex mediaPacketLock(caller->mediaPacketMutex, INFINITE);

            int64_t startPosition = request->GetStart();
            packetIndex = caller->mediaPacketCollection->GetMediaPacketIndexBetweenPositions(startPosition);

            while (packetIndex != UINT_MAX)
            {
              unsigned int mediaPacketDataStart = 0;
              unsigned int mediaPacketDataLength = 0;

              // get media packet
              CMediaPacket *mediaPacket = caller->mediaPacketCollection->GetItem(packetIndex);
              // check packet values against async request values
              result = caller->CheckValues(request, mediaPacket, &mediaPacketDataStart, &mediaPacketDataLength, startPosition);

              if (SUCCEEDED(result))
              {
                // successfully checked values
                int64_t positionStart = mediaPacket->GetStart();
                int64_t positionEnd = mediaPacket->GetEnd();

                // copy data from media packet to request buffer
                unsigned char *requestBuffer = request->GetBuffer() + foundDataLength;

                if ((request->GetBuffer() != NULL) && (caller->mediaPacketCollectionCacheFile->LoadItems(caller->mediaPacketCollection, packetIndex, true)))
                {
                  mediaPacket->GetBuffer()->CopyFromBuffer(requestBuffer, mediaPacketDataLength, mediaPacketDataStart);
                }

                // update length of data
                foundDataLength += mediaPacketDataLength;

                if (foundDataLength < (unsigned int)request->GetBufferLength())
                {
                  // find another media packet after end of this media packet
                  startPosition = positionEnd + 1;
                  packetIndex = caller->mediaPacketCollection->GetMediaPacketIndexBetweenPositions(startPosition);
                }
                else
                {
                  // do not find any more media packets for this request because we have enough data
                  break;
                }
              }
              else
              {
                // some error occured
                // do not find any more media packets for this request because request failed
                break;
              }
            }

            if (SUCCEEDED(result))
            {
              if (foundDataLength < (unsigned int)request->GetBufferLength())
              {
                // found data length is lower than requested
                DWORD currentTime = GetTickCount();
                if ((!caller->IsLiveStream()) && (!caller->IsAllDataReceived()) && ((currentTime - caller->lastReceivedMediaPacketTime) > caller->filter->GetReceiveDataTimeout()))
                {
                  // we don't receive data from protocol at least for specified timeout
                  // finish request with error to avoid freeze
                  caller->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, request '%u' doesn't receive data for specified time, current time: %d, last received data time: %d, specified timeout: %d", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, caller->parserStreamId, request->GetRequestId(), currentTime, caller->lastReceivedMediaPacketTime, caller->filter->GetReceiveDataTimeout());
                  request->Complete(VFW_E_TIMEOUT);
                }
                else if ((!caller->IsAllDataReceived()) && (!caller->IsEstimateTotalLength()) && (caller->totalLength > (request->GetStart() + request->GetBufferLength())))
                {
                  // we are receiving data, wait for all requested data
                }
                else if ((caller->pauseSeekStopRequest != PAUSE_SEEK_STOP_REQUEST_NONE) || (caller->IsAllDataReceived()) || ((caller->IsTotalLengthReceived()) && (!caller->IsEstimateTotalLength()) && (caller->totalLength <= (request->GetStart() + request->GetBufferLength()))))
                {
                  // we are not receiving more data
                  // finish request
                  caller->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, no more data available, request '%u', start '%lld', size '%d'", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, caller->parserStreamId, request->GetRequestId(), request->GetStart(), request->GetBufferLength());
                  request->SetBufferLength(foundDataLength);
                  request->Complete(S_OK);
                }
              }
              else if (foundDataLength == request->GetBufferLength())
              {
                // found data length is equal than requested, return S_OK
                request->SetBufferLength(foundDataLength);
                request->Complete(S_OK);
              }
              else
              {
                caller->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, request '%u' found data length '%u' bigger than requested '%lu'", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, caller->parserStreamId, request->GetRequestId(), foundDataLength, request->GetBufferLength());
                request->Complete(E_RESULT_DATA_LENGTH_BIGGER_THAN_REQUESTED);
              }
            }
            else
            {
              // some error occured
              // complete async request with error
              // set request is completed with result
              caller->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, request '%u' complete status: 0x%08X", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, caller->parserStreamId, request->GetRequestId(), result);
              request->SetBufferLength(foundDataLength);
              request->Complete(result);
            }

            if ((packetIndex == UINT_MAX) && (caller->IsAllDataReceived()) && ((request->GetState() == CAsyncRequest::Waiting) || (request->GetState() == CAsyncRequest::WaitingIgnoreTimeout)))
            {
              // if all data received then no more will come and we can fail
              caller->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, request '%u' no more data available", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, caller->parserStreamId, request->GetRequestId());
              request->Complete(E_NO_MORE_DATA_AVAILABLE);
            }
          }

          if ((packetIndex == UINT_MAX) && (request->GetState() == CAsyncRequest::Waiting))
          {
            // get current stream position
            LONGLONG total = 0;
            
            CStreamProgress *streamProgress = new CStreamProgress();
            HRESULT queryStreamProgressResult = (streamProgress != NULL) ? S_OK : E_OUTOFMEMORY;

            if (SUCCEEDED(queryStreamProgressResult))
            {
              streamProgress->SetStreamId(caller->parserStreamId);
              queryStreamProgressResult = caller->filter->QueryStreamProgress(streamProgress);
            }

            if (SUCCEEDED(queryStreamProgressResult))
            {
              total = streamProgress->GetTotalLength();
              currentStreamPosition = streamProgress->GetCurrentLength();
            }

            if (FAILED(queryStreamProgressResult))
            {
              caller->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, failed to get current stream position: 0x%08X", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, caller->parserStreamId, queryStreamProgressResult);
              currentStreamPosition = -1;
            }

            FREE_MEM_CLASS(streamProgress);
          }

          if ((packetIndex == UINT_MAX) && ((request->GetState() == CAsyncRequest::Waiting) || (request->GetState() == CAsyncRequest::WaitingIgnoreTimeout)))
          {
            if (caller->IsAllDataReceived())
            {
              // if all data received then no more will come and we can fail
              caller->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, request '%u' no more data available", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, caller->parserStreamId, request->GetRequestId());
              request->Complete(E_NO_MORE_DATA_AVAILABLE);
            }
          }

          if ((packetIndex == UINT_MAX) && (request->GetState() == CAsyncRequest::Waiting))
          {
            // first check current stream position and request start
            // if request start is just next to current stream position then only wait for data and do not issue seek request
            if (currentStreamPosition != (-1))
            {
              // current stream position has valid value
              if (request->GetStart() > currentStreamPosition)
              {
                // if request start is after current stream position than we have to issue seek request (if supported)
                if (request->GetRequestId() != lastWaitingRequestId)
                {
                  caller->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, request '%u', start '%llu' (size '%lu') after current stream position '%llu'", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, caller->parserStreamId, request->GetRequestId(), request->GetStart(), request->GetBufferLength(), currentStreamPosition);
                }
              }
              else if ((request->GetStart() <= currentStreamPosition) && ((request->GetStart() + request->GetBufferLength()) > currentStreamPosition))
              {
                // current stream position is within current request
                // we are receiving data, do nothing, just wait for all data
                request->WaitAndIgnoreTimeout();
              }
              else
              {
                // if request start is before current stream position than we have to issue seek request
                if (request->GetRequestId() != lastWaitingRequestId)
                {
                  CHECK_CONDITION_EXECUTE(!caller->IsAvi(), caller->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, request '%u', start '%llu' (size '%lu') before current stream position '%llu'", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, caller->parserStreamId, request->GetRequestId(), request->GetStart(), request->GetBufferLength(), currentStreamPosition));
                }
              }
            }

            if ((request->GetState() == CAsyncRequest::Waiting) && (request->GetRequestId() != lastWaitingRequestId))
            {
              // there isn't any packet containg some data for request
              // check if seeking by position is supported

              lastWaitingRequestId = request->GetRequestId();

              unsigned int seekingCapabilities = caller->filter->GetSeekingCapabilities();
              if (seekingCapabilities & SEEKING_METHOD_POSITION)
              {
                if (SUCCEEDED(result))
                {
                  // not found start packet and request wasn't requested from filter yet
                  // first found start and end of request

                  int64_t requestStart = request->GetStart();
                  int64_t requestEnd = requestStart;

                  unsigned int startIndex = 0;
                  unsigned int endIndex = 0;
                  {
                    // lock access to media packets
                    CLockMutex mediaPacketLock(caller->mediaPacketMutex, INFINITE);

                    if (caller->mediaPacketCollection->GetItemInsertPosition(request->GetStart(), &startIndex, &endIndex))
                    {
                      // start and end index found successfully
                      if (startIndex == endIndex)
                      {
                        int64_t endPacketStartPosition = 0;
                        int64_t endPacketStopPosition = 0;
                        unsigned int mediaPacketIndex = caller->mediaPacketCollection->GetMediaPacketIndexBetweenPositions(endPacketStartPosition);

                        // media packet exists in collection
                        while (mediaPacketIndex != UINT_MAX)
                        {
                          CMediaPacket *mediaPacket = caller->mediaPacketCollection->GetItem(mediaPacketIndex);
                          int64_t mediaPacketStart = mediaPacket->GetStart();
                          int64_t mediaPacketEnd = mediaPacket->GetEnd();
                          if (endPacketStartPosition == mediaPacketStart)
                          {
                            // next start time is next to end of current media packet
                            endPacketStartPosition = mediaPacketEnd + 1;
                            mediaPacketIndex++;

                            if (mediaPacketIndex >= caller->mediaPacketCollection->Count())
                            {
                              // stop checking, all media packets checked
                              mediaPacketIndex = UINT_MAX;
                            }
                          }
                          else
                          {
                            endPacketStopPosition = mediaPacketStart - 1;
                            mediaPacketIndex = UINT_MAX;
                          }
                        }

                        requestEnd = endPacketStopPosition;
                      }
                      else if ((startIndex == (caller->mediaPacketCollection->Count() - 1)) && (endIndex == UINT_MAX))
                      {
                        // media packet belongs to end
                        // do nothing, default request is from specific point until end of stream
                      }
                      else if ((startIndex == UINT_MAX) && (endIndex == 0))
                      {
                        // media packet belongs to start
                        CMediaPacket *endMediaPacket = caller->mediaPacketCollection->GetItem(endIndex);
                        if (endMediaPacket != NULL)
                        {
                          // requests data from requestStart until end packet start position
                          requestEnd = endMediaPacket->GetStart() - 1;
                        }
                      }
                      else
                      {
                        // media packet belongs between packets startIndex and endIndex
                        CMediaPacket *endMediaPacket = caller->mediaPacketCollection->GetItem(endIndex);
                        if (endMediaPacket != NULL)
                        {
                          // requests data from requestStart until end packet start position
                          requestEnd = endMediaPacket->GetStart() - 1;
                        }
                      }
                    }
                  }

                  if (requestEnd < requestStart)
                  {
                    CHECK_CONDITION_EXECUTE(!caller->IsAvi(), caller->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, request '%u' has start '%llu' after end '%llu', modifying to equal", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, caller->parserStreamId, request->GetRequestId(), requestStart, requestEnd));
                    requestEnd = requestStart;
                  }

                  // request filter to receive data from request start to end
                  result = (caller->filter->SeekToPosition(requestStart, requestEnd) >= 0) ? S_OK : E_FAIL;
                }

                if (FAILED(result))
                {
                  // if error occured while requesting filter for data
                  caller->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, request '%u' error while requesting data, complete status: 0x%08X", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, caller->parserStreamId, request->GetRequestId(), result);
                  request->Complete(result);
                }
              }
            }
          }
        }
      }
    }

    {
      if ((GetTickCount() - lastCheckTime) > CACHE_FILE_LOAD_TO_MEMORY_TIME_SPAN_DEFAULT)
      {
        lastCheckTime = GetTickCount();

        // lock access to media packets
        CLockMutex mediaPacketLock(caller->mediaPacketMutex, INFINITE);

        if (caller->IsLiveStream())
        {
          // remove used media packets
          // in case of live stream they will not be needed (after created demuxer and started playing)
          // in case of seeking based on position there can be serious problem, because position in data is not related to play time - switching audio stream will not work
          // this case will be very rare, because only HTTP protocol is based on position
          // UDP and RTP protocols can be problematic (MPEG2 TS without timestamps)

          if (caller->mediaPacketCollection->Count() > 0)
          {
            unsigned int packetsToRemove = 0;
            while (packetsToRemove < caller->mediaPacketCollection->Count())
            {
              CMediaPacket *mediaPacket = caller->mediaPacketCollection->GetItem(packetsToRemove);

              if ((mediaPacket->GetPresentationTimestampInDirectShowTimeUnits() / (DSHOW_TIME_BASE / 1000)) < (int64_t)caller->streamTime)
              {
                if (caller->demuxerContextBufferPosition <= mediaPacket->GetStart())
                {
                  caller->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, trying to remove not read data, position: %lld, media packet start: %lld", MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, caller->parserStreamId, caller->demuxerContextBufferPosition, mediaPacket->GetStart());
                  break;
                }

                packetsToRemove++;
              }
              else
              {
                break;
              }
            }

            if ((packetsToRemove > 0) && (caller->mediaPacketCollectionCacheFile->RemoveItems(caller->mediaPacketCollection, 0, packetsToRemove)))
            {
              caller->mediaPacketCollection->Remove(0, packetsToRemove);
              caller->lastMediaPacket -= packetsToRemove;
            }
          }
        }

        if (caller->mediaPacketCollectionCacheFile->GetCacheFile() == NULL)
        {
          wchar_t *cacheFilePath = caller->GetMediaPacketCollectionCacheFilePath();
          caller->mediaPacketCollectionCacheFile->SetCacheFile(cacheFilePath);
          FREE_MEM(cacheFilePath);
        }

        // store all media packets (which are not stored) to file
        if ((caller->mediaPacketCollectionCacheFile->GetCacheFile() != NULL) && (caller->mediaPacketCollection->Count() != 0))
        {
          caller->mediaPacketCollectionCacheFile->StoreItems(caller->mediaPacketCollection, lastCheckTime, caller->IsAllDataReceived());
        }
      }
    }

    Sleep(1);
  }

  caller->logger->Log(LOGGER_INFO, METHOD_DEMUXER_END_FORMAT, MODULE_NAME, METHOD_DEMUXER_READ_REQUEST_WORKER_NAME, caller->parserStreamId);

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}

HRESULT CDemuxer::CreateDemuxerReadRequestWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXER_READ_REQUEST_WORKER_NAME, this->parserStreamId);

  this->demuxerReadRequestWorkerShouldExit = false;

  this->demuxerReadRequestWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CDemuxer::DemuxerReadRequestWorker, this, 0, NULL);

  if (this->demuxerReadRequestWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, _beginthreadex() error: 0x%08X", MODULE_NAME, METHOD_CREATE_DEMUXER_READ_REQUEST_WORKER_NAME, this->parserStreamId, result);
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXER_READ_REQUEST_WORKER_NAME, this->parserStreamId, result);
  return result;
}

HRESULT CDemuxer::DestroyDemuxerReadRequestWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_DESTROY_DEMUXER_READ_REQUEST_WORKER_NAME, this->parserStreamId);

  this->demuxerReadRequestWorkerShouldExit = true;

  // wait for the receive data worker thread to exit      
  if (this->demuxerReadRequestWorkerThread != NULL)
  {
    if (WaitForSingleObject(this->demuxerReadRequestWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_DESTROY_DEMUXER_READ_REQUEST_WORKER_NAME, this->parserStreamId, L"thread didn't exit, terminating thread");
      TerminateThread(this->demuxerReadRequestWorkerThread, 0);
    }
    CloseHandle(this->demuxerReadRequestWorkerThread);
  }

  this->demuxerReadRequestWorkerThread = NULL;
  this->demuxerReadRequestWorkerShouldExit = false;

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DESTROY_DEMUXER_READ_REQUEST_WORKER_NAME, this->parserStreamId, result);
  return result;
}

unsigned int WINAPI CDemuxer::DemuxingWorker(LPVOID lpParam)
{
  CDemuxer *caller = (CDemuxer *)lpParam;
  caller->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_DEMUXING_WORKER_NAME, caller->parserStreamId);

  while (!caller->demuxingWorkerShouldExit)
  {
    if ((caller->pauseSeekStopRequest == PAUSE_SEEK_STOP_REQUEST_NONE) && (!caller->IsEndOfStreamOutputPacketQueued()))
    {
      // S_FALSE means no packet
      HRESULT result = S_FALSE;
      COutputPinPacket *packet = new COutputPinPacket();
      CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        result = caller->GetNextPacketInternal(packet);

        if ((result == E_REQUESTED_DATA_AFTER_TOTAL_LENGTH) ||
            (result == E_NO_MORE_DATA_AVAILABLE))
        {
          // special error code for end of stream

          packet->SetDemuxerId(caller->parserStreamId);
          packet->SetEndOfStream(true);
          result = S_OK;
          caller->flags |= DEMUXER_FLAG_END_OF_STREAM_OUTPUT_PACKET_QUEUED;
          caller->logger->Log(LOGGER_INFO, L"%s: %s: stream %u, queued end of stream output packet", MODULE_NAME, METHOD_DEMUXING_WORKER_NAME, caller->parserStreamId);
        }
      }

      // S_FALSE means no packet
      if (result == S_OK)
      {
        CLockMutex lock(caller->outputPacketMutex, INFINITE);

        result = caller->outputPacketCollection->Add(packet) ? result : E_OUTOFMEMORY;
      }

      CHECK_CONDITION_EXECUTE(result != S_OK, FREE_MEM_CLASS(packet));
    }

    Sleep(1);
  }

  caller->logger->Log(LOGGER_INFO, METHOD_DEMUXER_END_FORMAT, MODULE_NAME, METHOD_DEMUXING_WORKER_NAME, caller->parserStreamId);

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}

HRESULT CDemuxer::CreateDemuxingWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXING_WORKER_NAME, this->parserStreamId);

  this->demuxingWorkerShouldExit = false;

  this->demuxingWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CDemuxer::DemuxingWorker, this, 0, NULL);

  if (this->demuxingWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, _beginthreadex() error: 0x%08X", MODULE_NAME, METHOD_CREATE_DEMUXING_WORKER_NAME, this->parserStreamId, result);
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXING_WORKER_NAME, this->parserStreamId, result);
  return result;
}

HRESULT CDemuxer::DestroyDemuxingWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_DESTROY_DEMUXING_WORKER_NAME, this->parserStreamId);

  this->demuxingWorkerShouldExit = true;

  // wait for the receive data worker thread to exit      
  if (this->demuxingWorkerThread != NULL)
  {
    if (WaitForSingleObject(this->demuxingWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_DESTROY_DEMUXING_WORKER_NAME, this->parserStreamId, L"thread didn't exit, terminating thread");
      TerminateThread(this->demuxingWorkerThread, 0);
    }
    CloseHandle(this->demuxingWorkerThread);
  }

  this->demuxingWorkerThread = NULL;
  this->demuxingWorkerShouldExit = false;

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DESTROY_DEMUXING_WORKER_NAME, this->parserStreamId, result);
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
    else if (ffmpegResult < 0)
    {
      // meh, fail
      result = E_NO_MORE_DATA_AVAILABLE;
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
        packet->SetDemuxerId(this->parserStreamId);

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
    unsigned int dataSize = 0;
    {
      CLockMutex lock(this->mediaPacketMutex, 100);

      if ((lock.IsLocked()) && (this->mediaPacketCollection->Count() > 0))
      {
        CMediaPacket *mediaPacket = this->mediaPacketCollection->GetItem(this->mediaPacketCollection->Count() - 1);

        // we must have more data (media packet start + length) then current position within data (this->demuxerContextBufferPosition)
        uint64_t end = (uint64_t)mediaPacket->GetStart() + (uint64_t)mediaPacket->GetLength();
        if (end > (uint64_t)this->demuxerContextBufferPosition)
        {
          dataSize = (unsigned int)(end - (uint64_t)this->demuxerContextBufferPosition);
          // do not pass very big blocks of data, limit data size to MAXIMUM_MPEG2_TS_DATA_PACKET
          dataSize = (dataSize > MAXIMUM_MPEG2_TS_DATA_PACKET) ? MAXIMUM_MPEG2_TS_DATA_PACKET : dataSize;
        }
      }
    }

    if (dataSize > 0)
    {
      ALLOC_MEM_DEFINE_SET(temp, unsigned char, dataSize, 0);

      if (temp != NULL)
      {
        int res = this->DemuxerRead(this, temp, dataSize);

        if (res > 0)
        {
          result = packet->CreateBuffer((unsigned int)res) ? result : E_OUTOFMEMORY;

          if (SUCCEEDED(result))
          {
            packet->GetBuffer()->AddToBuffer(temp, (unsigned int)res);
            result = S_OK;
          }
        }
        else if (res <= 0)
        {
          result = E_NO_MORE_DATA_AVAILABLE;
        }
      }

      FREE_MEM(temp);
    }
    else if (this->IsAllDataReceived())
    {
      result = E_NO_MORE_DATA_AVAILABLE;
    }
  }

  return result;
}

HRESULT CDemuxer::CheckValues(CAsyncRequest *request, CMediaPacket *mediaPacket, unsigned int *mediaPacketDataStart, unsigned int *mediaPacketDataLength, int64_t startPosition)
{
  HRESULT result = S_OK;

  CHECK_POINTER_DEFAULT_HRESULT(result, request);
  CHECK_POINTER_DEFAULT_HRESULT(result, mediaPacket);
  CHECK_POINTER_DEFAULT_HRESULT(result, mediaPacketDataStart);
  CHECK_POINTER_DEFAULT_HRESULT(result, mediaPacketDataLength);

  if (SUCCEEDED(result))
  {
    LONGLONG requestStart = request->GetStart();
    LONGLONG requestEnd = request->GetStart() + request->GetBufferLength();

    CHECK_CONDITION_HRESULT(result, ((startPosition >= requestStart) && (startPosition <= requestEnd)), result, E_INVALIDARG);

    if (SUCCEEDED(result))
    {
      int64_t mediaPacketStart = mediaPacket->GetStart();
      int64_t mediaPacketEnd = mediaPacket->GetEnd();

      if (SUCCEEDED(result))
      {
        // check if start position is in media packet
        CHECK_CONDITION_HRESULT(result, ((startPosition >= mediaPacketStart) && (startPosition <= mediaPacketEnd)), result, E_INVALIDARG);

        if (SUCCEEDED(result))
        {
          // increase position end because position end is stamp of last byte in buffer
          mediaPacketEnd++;

          // check if async request and media packet are overlapping
          CHECK_CONDITION_HRESULT(result, ((requestStart <= mediaPacketEnd) && (requestEnd >= mediaPacketStart)), result, E_INVALIDARG);
        }
      }

      if (SUCCEEDED(result))
      {
        // check problematic values
        // maximum length of data in media packet can be UINT_MAX - 1
        // async request cannot start after UINT_MAX - 1 because then async request and media packet are not overlapping

        int64_t tempMediaPacketDataStart = ((startPosition - mediaPacketStart) > 0) ? startPosition : mediaPacketStart;
        if ((min(requestEnd, mediaPacketEnd) - tempMediaPacketDataStart) >= UINT_MAX)
        {
          // it's there just for sure
          // problem: length of data is bigger than possible values for copying data
          result = E_OUTOFMEMORY;
        }

        if (SUCCEEDED(result))
        {
          // all values are correct
          *mediaPacketDataStart = (unsigned int)(tempMediaPacketDataStart - mediaPacketStart);
          *mediaPacketDataLength = (unsigned int)(min(requestEnd, mediaPacketEnd) - tempMediaPacketDataStart);
        }
      }
    }
  }

  return result;
}

HRESULT CDemuxer::GetTotalLength(int64_t *totalLength)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, totalLength);

  if (SUCCEEDED(result))
  {
    int64_t availableLength = 0;
    result = this->Length(totalLength, &availableLength);
  }

  return result;
}

HRESULT CDemuxer::GetAvailableLength(int64_t *availableLength)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, availableLength);

  if (SUCCEEDED(result))
  {
    int64_t totalLength = 0;
    result = this->Length(&totalLength, availableLength);
  }

  return result;
}

HRESULT CDemuxer::Length(LONGLONG *total, LONGLONG *available)
{
  //CHECK_CONDITION_EXECUTE((!this->IsAvi()) && (this->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, MODULE_NAME, METHOD_LENGTH_NAME));

  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, total);
  CHECK_POINTER_DEFAULT_HRESULT(result, available);

  unsigned int mediaPacketCount = 0;
  {
    CLockMutex lock(this->mediaPacketMutex, INFINITE);
    mediaPacketCount = this->mediaPacketCollection->Count();
  }

  if (SUCCEEDED(result))
  {
    *total = this->totalLength;
    *available = this->totalLength;
    
    CStreamAvailableLength *availableLength = new CStreamAvailableLength();
    CHECK_POINTER_HRESULT(result, availableLength, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      availableLength->SetStreamId(this->parserStreamId);
      result = this->filter->QueryStreamAvailableLength(availableLength);
    }

    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), *available = availableLength->GetAvailableLength());
    
    if (FAILED(result))
    {
      // error occured while requesting stream available length
      CLockMutex lock(this->mediaPacketMutex, INFINITE);
      mediaPacketCount = this->mediaPacketCollection->Count();

      // return default value = last media packet end
      *available = 0;
      for (unsigned int i = 0; i < mediaPacketCount; i++)
      {
        CMediaPacket *mediaPacket = this->mediaPacketCollection->GetItem(i);
        int64_t mediaPacketStart = mediaPacket->GetStart();
        int64_t mediaPacketEnd = mediaPacket->GetEnd();

        if ((mediaPacketEnd + 1) > (*available))
        {
          *available = mediaPacketEnd + 1;
        }
      }

      result = S_OK;
    }
    FREE_MEM_CLASS(availableLength);

    result = (this->IsEstimateTotalLength()) ? VFW_S_ESTIMATED : S_OK;
    //CHECK_CONDITION_EXECUTE((!this->IsAvi()) && (this->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), this->logger->Log(LOGGER_VERBOSE, L"%s: %s: total length: %llu, available length: %llu, estimate: %u, media packets: %u", MODULE_NAME, METHOD_LENGTH_NAME, this->totalLength, *available, (this->IsEstimateTotalLength()) ? 1 : 0, mediaPacketCount));
  }

  //CHECK_CONDITION_EXECUTE((!this->IsAvi()) && (this->lastCommand != CMPUrlSourceSplitter::CMD_PLAY), this->logger->Log(LOGGER_VERBOSE, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_LENGTH_NAME, result));
  return result;
}

HRESULT CDemuxer::OpenStream(AVIOContext *demuxerContext)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, demuxerContext);

  if (SUCCEEDED(result))
  {
    int ret; // return code from avformat functions
    unsigned int mediaPacketCount = 0;

    {
      CLockMutex lock(this->mediaPacketMutex, INFINITE);

      mediaPacketCount = this->mediaPacketCollection->Count();
    }

    CHECK_CONDITION_HRESULT(result, mediaPacketCount != 0, result, E_FAIL);

    if (SUCCEEDED(result))
    {
      // create the format context
      this->formatContext = avformat_alloc_context();
      this->formatContext->pb = demuxerContext;

      if (this->IsSetFlags(DEMUXER_FLAG_STREAM_IN_PACKETS))
      {
        FREE_MEM_CLASS(this->packetInputFormat);
        this->packetInputFormat = new CPacketInputFormat(this, this->streamInputFormat);
      }

      ret = avformat_open_input(&this->formatContext, "", this->packetInputFormat, NULL);

      CHECK_CONDITION_EXECUTE(ret < 0, result = ret);

      if (SUCCEEDED(result))
      {
        ret = this->InitFormatContext();
        CHECK_CONDITION_EXECUTE(ret < 0, result = ret);
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

HRESULT CDemuxer::GetNextMediaPacket(CMediaPacket **mediaPacket)
{
  HRESULT result = S_FALSE;
  CHECK_POINTER_DEFAULT_HRESULT(result, mediaPacket);

  if (SUCCEEDED(result))
  {
    CLockMutex lock(this->mediaPacketMutex, 100);

    if (lock.IsLocked())
    {
      CMediaPacket *temp = this->mediaPacketCollection->GetItem(this->lastMediaPacket);
      if (temp != NULL)
      {
        if (this->mediaPacketCollectionCacheFile->LoadItems(this->mediaPacketCollection, this->lastMediaPacket, true))
        {
          this->lastMediaPacket++;
          result = S_OK;
          (*mediaPacket) = temp;
        }
      }
      else if ((temp == NULL) && (this->IsSetFlags(DEMUXER_FLAG_ALL_DATA_RECEIVED)))
      {
        // all data received, we don't have any other packet
        result = AVERROR_EOF;
      }
    }
  }

  return result;
}

int CDemuxer::StreamRead(int64_t position, uint8_t *buffer, int length)
{
  return this->DemuxerReadPosition(position, buffer, length);
}
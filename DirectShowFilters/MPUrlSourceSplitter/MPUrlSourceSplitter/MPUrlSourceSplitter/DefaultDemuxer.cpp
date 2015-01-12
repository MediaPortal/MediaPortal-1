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

#include "DefaultDemuxer.h"
#include "LockMutex.h"
#include "ErrorCodes.h"
#include "StreamPackageDataRequest.h"

#ifdef _DEBUG
#define MODULE_NAME                                                         L"DefaultDemuxerd"
#else
#define MODULE_NAME                                                         L"DefaultDemuxer"
#endif

#define MINIMUM_IPTV_BUFFER_SIZE                                            (188 * 174)       // 174 * 188 = 32712 < 32 kB
#define MAXIMUM_IPTV_BUFFER_SZE                                             (32 * MINIMUM_IPTV_BUFFER_SIZE)   // 32 * 32712 = 1046784 < 1 MB

CDefaultDemuxer::CDefaultDemuxer(HRESULT *result, CLogger *logger, IDemuxerOwner *filter, CParameterCollection *configuration)
  : CDemuxer(result, logger, filter, configuration)
{
  this->iptvBuffer = NULL;
  this->iptvBufferSize = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
  }
}

CDefaultDemuxer::~CDefaultDemuxer(void)
{
  // destroy create demuxer worker (if not finished earlier)
  this->DestroyCreateDemuxerWorker();

  // destroy demuxing worker (if not finished earlier)
  this->DestroyDemuxingWorker();

  FREE_MEM(this->iptvBuffer);
}

/* CDemuxer methods */

int64_t CDefaultDemuxer::GetDuration(void)
{
  int64_t duration = this->filter->GetDuration();

  if (duration == DURATION_UNSPECIFIED)
  {
      // no duration is available for us
      duration = -1;
  }
  else if (duration != DURATION_LIVE_STREAM)
  {
    duration *= (DSHOW_TIME_BASE / 1000);
  }

  return duration;
}

uint64_t CDefaultDemuxer::GetPositionForStreamTime(uint64_t streamTime)
{
  uint64_t result = 0;

  // IPTV demuxing case (no demuxing, just passing data out)
  result = this->demuxerContextBufferPosition;

  return result;
}

/* get methods */

/* set methods */

/* other methods */

/* protected methods */

HRESULT CDefaultDemuxer::CreateDemuxerInternal(void)
{
  return S_OK;
}

void CDefaultDemuxer::CleanupDemuxerInternal(void)
{
}

HRESULT CDefaultDemuxer::DemuxingWorkerInternal(void)
{
  // S_FALSE means no packet
  HRESULT result = S_FALSE;

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
      LOCK_MUTEX(this->outputPacketMutex, INFINITE)

      if (packet->IsEndOfStream())
      {
        HRESULT endOfStreamResult = packet->GetEndOfStreamResult();

        if (SUCCEEDED(result))
        {
          CHECK_CONDITION_HRESULT(result, this->outputPacketCollection->Add(packet), result, E_OUTOFMEMORY);

          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packet));
        }

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

      UNLOCK_MUTEX(this->outputPacketMutex)
    }

    CHECK_CONDITION_EXECUTE(result != S_OK, FREE_MEM_CLASS(packet));
  }

  return result;
}

HRESULT CDefaultDemuxer::GetNextPacketInternal(COutputPinPacket *packet)
{
  // S_FALSE means no packet
  HRESULT result = S_FALSE;
  CHECK_POINTER_DEFAULT_HRESULT(result, packet);

  if (SUCCEEDED(result))
  {
    if (this->iptvBufferSize == 0)
    {
      this->iptvBufferSize = MINIMUM_IPTV_BUFFER_SIZE;
      this->iptvBuffer = ALLOC_MEM_SET(this->iptvBuffer, unsigned char, this->iptvBufferSize, 0);

      CHECK_POINTER_HRESULT(result, this->iptvBuffer, result, E_OUTOFMEMORY);
    }

    if (this->iptvBuffer != NULL)
    {
      HRESULT res = this->DemuxerReadPosition(this->demuxerContextBufferPosition, this->iptvBuffer, this->iptvBufferSize, STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_NONZERO_DATA_LENGTH);

      if (res > 0)
      {
        unsigned int receivedSize = (unsigned int)res;
        CHECK_CONDITION_HRESULT(result, packet->GetBuffer()->InitializeBuffer(receivedSize), result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          packet->GetBuffer()->AddToBuffer(this->iptvBuffer, receivedSize);

          packet->SetStreamPid(0);
          packet->SetDemuxerId(this->demuxerId);

          this->demuxerContextBufferPosition += receivedSize;
          result = S_OK;
        }

        if ((receivedSize == this->iptvBufferSize) && (this->iptvBufferSize < MAXIMUM_IPTV_BUFFER_SZE))
        {
          // need to increase buffer size, maybe we are slow in reading data
          unsigned int previousSize = this->iptvBufferSize;

          this->iptvBufferSize *= 2;
          FREE_MEM(this->iptvBuffer);
          this->iptvBuffer = ALLOC_MEM_SET(this->iptvBuffer, unsigned char, this->iptvBufferSize, 0);

          if (this->iptvBuffer == NULL)
          {
            // try to create IPTV buffer with MINIMUM_IPTV_BUFFER_SIZE size
            this->iptvBufferSize = 0;
          }

          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: demuxer: %u, stream: %d, resized IPTV buffer, from: %u, to: %u", MODULE_NAME, METHOD_GET_NEXT_PACKET_INTERNAL_NAME, this->demuxerId, packet->GetStreamPid(), previousSize, this->iptvBufferSize);
        }
      }
      else if ((res < 0) && (res != E_CONNECTION_LOST_TRYING_REOPEN))
      {
        packet->SetStreamPid(0);
        packet->SetDemuxerId(this->demuxerId);

        result = res;
      }
      else if ((res == E_CONNECTION_LOST_TRYING_REOPEN) || (this->IsSetFlags(DEMUXER_FLAG_PENDING_DISCONTINUITY)))
      {
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, discontinuity received or connection lost", MODULE_NAME, METHOD_GET_NEXT_PACKET_INTERNAL_NAME, this->demuxerId);

        result = S_FALSE;
        this->flags &= ~(DEMUXER_FLAG_PENDING_DISCONTINUITY | DEMUXER_FLAG_PENDING_DISCONTINUITY_WITH_REPORT);
      }
    }
  }

  return result;
}

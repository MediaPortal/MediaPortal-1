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

#include "PacketDemuxer.h"
#include "ErrorCodes.h"
#include "StreamPackage.h"
#include "StreamPackagePacketRequest.h"
#include "StreamPackagePacketResponse.h"

#ifdef _DEBUG
#define MODULE_NAME                                                   L"PacketDemuxerd"
#else
#define MODULE_NAME                                                   L"PacketDemuxer"
#endif

CPacketDemuxer::CPacketDemuxer(HRESULT *result, CLogger *logger, IDemuxerOwner *filter, CParameterCollection *configuration)
  : CStandardDemuxer(result, logger, filter, configuration)
{
  this->streamInputFormat = NULL;
  this->packetInputFormat = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
  }
}

CPacketDemuxer::~CPacketDemuxer(void)
{
  // destroy create demuxer worker (if not finished earlier)
  this->DestroyCreateDemuxerWorker();

  // destroy demuxing worker (if not finished earlier)
  this->DestroyDemuxingWorker();

  FREE_MEM(this->streamInputFormat);
  FREE_MEM_CLASS(this->packetInputFormat);
}

// IPacketDemuxer interface

HRESULT CPacketDemuxer::GetNextMediaPacket(CMediaPacket **mediaPacket, uint64_t flags)
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

    if (this->IsSetFlags(DEMUXER_FLAG_PENDING_DISCONTINUITY))
    {
      if (this->IsSetFlags(DEMUXER_FLAG_PENDING_DISCONTINUITY_WITH_REPORT))
      {
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, request %u, discontinuity reported", MODULE_NAME, METHOD_GET_NEXT_MEDIA_PACKET_NAME, this->demuxerId, requestId);
      }

      this->flags &= ~(DEMUXER_FLAG_PENDING_DISCONTINUITY | DEMUXER_FLAG_PENDING_DISCONTINUITY_WITH_REPORT);
      result = E_CONNECTION_LOST_TRYING_REOPEN;
    }

    CHECK_HRESULT_EXECUTE(result, this->filter->ProcessStreamPackage(package));
    CHECK_HRESULT_EXECUTE(result, package->GetError());

    if (SUCCEEDED(result))
    {
      // successfully processed stream package request
      CStreamPackagePacketResponse *response = dynamic_cast<CStreamPackagePacketResponse *>(package->GetResponse());

      *mediaPacket = (CMediaPacket *)response->GetMediaPacket()->Clone();
      CHECK_POINTER_HRESULT(result, (*mediaPacket), result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), (*mediaPacket)->SetDiscontinuity(response->IsDiscontinuity(), UINT_MAX));

      if (response->IsDiscontinuity())
      {
        this->flags |= DEMUXER_FLAG_PENDING_DISCONTINUITY;

        if (result != 0)
        {
          this->flags |= DEMUXER_FLAG_PENDING_DISCONTINUITY_WITH_REPORT;
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, request %u, pending discontinuity", MODULE_NAME, METHOD_GET_NEXT_MEDIA_PACKET_NAME, this->demuxerId, requestId);
        }
      }
    }

    CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, request %u, error: 0x%08X", MODULE_NAME, METHOD_GET_NEXT_MEDIA_PACKET_NAME, this->demuxerId, requestId, result));

    FREE_MEM_CLASS(package);
  }

  return result;
}

int CPacketDemuxer::StreamReadPosition(int64_t position, uint8_t *buffer, int length, uint64_t flags)
{
  return this->DemuxerReadPosition(position, buffer, length, flags);
}

/* get methods */

/* set methods */

HRESULT CPacketDemuxer::SetStreamInformation(CStreamInformation *streamInformation)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamInformation);
  CHECK_CONDITION_HRESULT(result, streamInformation->IsPackets(), result, E_NOT_VALID_STATE);

  if (SUCCEEDED(result))
  {
    SET_STRING_HRESULT_WITH_NULL(this->streamInputFormat, streamInformation->GetStreamInputFormat(), result);
  }

  return result;
}

/* other methods */

/* protected methods */

int64_t CPacketDemuxer::GetPacketPts(AVStream *stream, AVPacket *packet)
{
  return packet->pts;
}

int64_t CPacketDemuxer::GetPacketDts(AVStream *stream, AVPacket *packet)
{
  return packet->dts;
}

HRESULT CPacketDemuxer::OpenStream(AVIOContext *demuxerContext)
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

      FREE_MEM_CLASS(this->packetInputFormat);
      this->packetInputFormat = new CPacketInputFormat(&result, this, this->streamInputFormat);

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

      CHECK_CONDITION_EXECUTE(FAILED(result), this->flags &= ~STANDARD_DEMUXER_FLAG_ALL_CONTAINERS);
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), this->CleanupFormatContext());

  return result;
}
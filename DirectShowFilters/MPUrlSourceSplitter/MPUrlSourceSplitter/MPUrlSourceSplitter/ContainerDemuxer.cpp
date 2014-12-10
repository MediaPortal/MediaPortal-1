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

#include "ContainerDemuxer.h"

CContainerDemuxer::CContainerDemuxer(HRESULT *result, CLogger *logger, IDemuxerOwner *filter, CParameterCollection *configuration)
  : CStandardDemuxer(result, logger, filter, configuration)
{
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
  }
}

CContainerDemuxer::~CContainerDemuxer(void)
{
  // destroy create demuxer worker (if not finished earlier)
  this->DestroyCreateDemuxerWorker();

  // destroy demuxing worker (if not finished earlier)
  this->DestroyDemuxingWorker();
}

/* get methods */

/* set methods */

HRESULT CContainerDemuxer::SetStreamInformation(CStreamInformation *streamInformation)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamInformation);
  CHECK_CONDITION_HRESULT(result, streamInformation->IsContainer(), result, E_NOT_VALID_STATE);

  return result;
}

/* other methods */

/* protected methods */

int64_t CContainerDemuxer::GetPacketPts(AVStream *stream, AVPacket *packet)
{
  return (REFERENCE_TIME)ConvertTimestampToRT(packet->pts, stream->time_base.num, stream->time_base.den, (int64_t)AV_NOPTS_VALUE);
}

int64_t CContainerDemuxer::GetPacketDts(AVStream *stream, AVPacket *packet)
{
  return (REFERENCE_TIME)ConvertTimestampToRT(packet->dts, stream->time_base.num, stream->time_base.den, (int64_t)AV_NOPTS_VALUE);
}

HRESULT CContainerDemuxer::OpenStream(AVIOContext *demuxerContext)
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

      if (SUCCEEDED(result))
      {
        ret = avformat_open_input(&this->formatContext, "", NULL, NULL);

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
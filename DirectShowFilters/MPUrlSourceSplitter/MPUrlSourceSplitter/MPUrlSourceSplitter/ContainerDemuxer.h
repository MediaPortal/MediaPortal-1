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

#ifndef __CONTAINER_DEMUXER_DEFINED
#define __CONTAINER_DEMUXER_DEFINED

#include "StandardDemuxer.h"

#define CONTAINER_DEMUXER_FLAG_NONE                                   STANDARD_DEMUXER_FLAG_NONE

#define CONTAINER_DEMUXER_FLAG_LAST                                   (STANDARD_DEMUXER_FLAG_LAST + 0)

class CContainerDemuxer : public CStandardDemuxer
{
public:
  CContainerDemuxer(HRESULT *result, CLogger *logger, IDemuxerOwner *filter, CParameterCollection *configuration);
  virtual ~CContainerDemuxer(void);

  /* get methods */

  /* set methods */

  // sets stream information to demuxer
  // @param streamInformation : the stream information reported by parser or protocol
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT SetStreamInformation(CStreamInformation *streamInformation);

  /* other methods */

protected:

  /* methods */

  // gets AV packet PTS
  // @param stream : the AV stream
  // @param packet : the AV packet to get PTS
  // @return : the PTS of AV packet
  virtual int64_t GetPacketPts(AVStream *stream, AVPacket *packet);

  // gets AV packet DTS
  // @param stream : the AV stream
  // @param packet : the AV packet to get DTS
  // @return : the DTS of AV packet
  virtual int64_t GetPacketDts(AVStream *stream, AVPacket *packet);

  // opens stream
  // @param demuxerContext : demuxer context
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT OpenStream(AVIOContext *demuxerContext);
};

#endif
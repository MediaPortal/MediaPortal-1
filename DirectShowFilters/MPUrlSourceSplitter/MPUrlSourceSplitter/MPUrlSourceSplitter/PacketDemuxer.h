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

#ifndef __PACKET_DEMUXER_DEFINED
#define __PACKET_DEMUXER_DEFINED

#include "StandardDemuxer.h"
#include "MediaPacketCollection.h"
#include "IPacketDemuxer.h"
#include "PacketInputFormat.h"

#define PACKET_DEMUXER_FLAG_NONE                                      STANDARD_DEMUXER_FLAG_NONE

#define PACKET_DEMUXER_FLAG_LAST                                      (STANDARD_DEMUXER_FLAG_LAST + 0)

#define METHOD_GET_NEXT_MEDIA_PACKET_NAME                             L"GetNextMediaPacket()"

class CPacketDemuxer : public CStandardDemuxer, public IPacketDemuxer
{
public:
  CPacketDemuxer(HRESULT *result, CLogger *logger, IDemuxerOwner *filter, CParameterCollection *configuration);
  virtual ~CPacketDemuxer(void);

  // IPacketDemuxer interface

  // gets next available media packet
  // @param mediaPacket : reference to variable to store to reference to media packet
  // @param flags : the flags
  // @return : 
  // S_OK     = media packet returned
  // S_FALSE  = no media packet available
  // negative values are error
  HRESULT GetNextMediaPacket(CMediaPacket **mediaPacket, uint64_t flags);

  // reads data from stream from specified position into buffer
  // @param position : the position in stream to start reading data
  // @param buffer : the buffer to store data
  // @param length : the size of requested data
  // @param flags : the flags
  // @return : the length of read data, negative values are errors
  int StreamReadPosition(int64_t position, uint8_t *buffer, int length, uint64_t flags);

  /* get methods */

  /* set methods */

  // sets stream information to demuxer
  // @param streamInformation : the stream information reported by parser or protocol
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT SetStreamInformation(CStreamInformation *streamInformation);

  /* other methods */

protected:

  // holds stream input format (if specified)
  wchar_t *streamInputFormat;
  // holds special packet input format (only in case of DEMUXER_FLAG_STREAM_IN_PACKETS set flag)
  CPacketInputFormat *packetInputFormat;

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
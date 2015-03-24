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

#ifndef __PACKET_INPUT_FORMAT_DEFINED
#define __PACKET_INPUT_FORMAT_DEFINED

#include "IPacketDemuxer.h"
#include "Flags.h"

#define PACKET_INPUT_FORMAT_IDENTIFIER                                "packet"
#define PACKET_INPUT_FORMAT_LONG_NAME                                 "Packet input"

#define PACKET_INPUT_FORMAT_FLAG_NONE                                 FLAGS_NONE

#define PACKET_INPUT_FORMAT_FLAG_RESET_PACKET_COUNTER                 (1 << (FLAGS_LAST + 0))
#define PACKET_INPUT_FORMAT_FLAG_DISCONTINUITY                        (1 << (FLAGS_LAST + 1))

// we can't use CFlags class, because AVInputFormat has already flags member
class CPacketInputFormat : public AVInputFormat
{
public:
  CPacketInputFormat(HRESULT *result, IPacketDemuxer *demuxer, const wchar_t *streamFormat);
  ~CPacketInputFormat(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // tests if specific combination of flags is set
  // @param flags : the set of flags to test
  // @return : true if set of flags is set, false otherwise
  bool IsSetFlags(uint32_t flags);

protected:

  IPacketDemuxer *demuxer;
  wchar_t *streamFormat;

  AVFormatContext *streamFormatContext;
  AVIOContext *streamIoContext;

  int64_t streamIoContextBufferPosition;

  uint32_t internalFlags;

  /* static methods */

  static int ReadHeader(AVFormatContext *formatContext);
  static int ReadPacket(AVFormatContext *formatContext, AVPacket *packet);
  static int Seek(AVFormatContext *formatContext, int stream_index, int64_t timestamp, int flags);

  static int StreamRead(void *opaque, uint8_t *buf, int buf_size);
  static int64_t StreamSeek(void *opaque,  int64_t offset, int whence);
};

#endif
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

#ifndef __IPACKET_DEMUXER_DEFINED
#define __IPACKET_DEMUXER_DEFINED

#include "MediaPacket.h"

// defines interface for packet demuxer
// it is specific interface between packet input format (CPacketInputFormat) and demuxer (CDemuxer)
struct IPacketDemuxer
{
  // gets next available media packet
  // @param mediaPacket : reference to variable to store to reference to media packet
  // @param flags : the flags
  // @return : 
  // S_OK     = media packet returned
  // S_FALSE  = no media packet available
  // negative values are error
  virtual HRESULT GetNextMediaPacket(CMediaPacket **mediaPacket, uint64_t flags) = 0;

  // reads data from stream from specified position into buffer
  // @param position : the position in stream to start reading data
  // @param buffer : the buffer to store data
  // @param length : the size of requested data
  // @param flags : the flags
  // @return : the length of read data, negative values are errors
  virtual int StreamReadPosition(int64_t position, uint8_t *buffer, int length, uint64_t flags) = 0;
};

#endif
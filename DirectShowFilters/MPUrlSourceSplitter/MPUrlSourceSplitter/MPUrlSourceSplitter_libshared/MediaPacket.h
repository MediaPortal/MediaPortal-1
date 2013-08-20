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

#ifndef __MEDIA_PACKET_DEFINED
#define __MEDIA_PACKET_DEFINED

#include "LinearBuffer.h"

#include <stdint.h>

// CMediaPacket class is wrapper for IMediaSample interface
// this class doesn't implement all methods of IMediaSample interface
class CMediaPacket
{
public:
  CMediaPacket(void);
  virtual ~CMediaPacket();

  // gets linear buffer
  // @return : linear buffer or NULL if error or media packet is stored to file
  CLinearBuffer *GetBuffer();

  // gets the stream position where this packet starts
  // @return : the stream position where this packet starts
  int64_t GetStart(void);

  // gets the stream position where this packet ends
  // @return : the stream position where this packet ends
  int64_t GetEnd(void);

  // sets the stream position where this packet starts
  // @param position : the stream position where this packet starts
  void SetStart(int64_t position);

  // sets the stream position where this packet ends
  // @param position : the stream position where this packet ends
  void SetEnd(int64_t position);

  // deeply clones current instance of media packet
  // @return : deep clone of current instance or NULL if error
  CMediaPacket *Clone(void);

  // deeply clone current instance of media packet with specified position range to new media packet
  // @param start : start position of new media packet
  // @param end : end position of new media packet
  // @return : new media packet or NULL if error or media packet is stored to file
  CMediaPacket *CreateMediaPacketBasedOnPacket(int64_t start, int64_t end);

  // tests if media packet is stored to file
  // @return : true if media packet is stored to file, false otherwise
  bool IsStoredToFile(void);

  // sets position within store file
  // if media packet is stored than linear buffer is deleted
  // if store file path is cleared (NULL) than linear buffer is created
  // @param position : the position of start of media packet within store file or (-1) if media packet is in memory
  void SetStoredToFile(int64_t position);

  // gets position of start of media packet within store file
  // @return : file position or -1 if error
  int64_t GetStoreFilePosition(void);

protected:
  // internal linear buffer for media data
  CLinearBuffer *buffer;

  // start sample - byte position
  int64_t start;
  // end sample - byte position
  int64_t end;

  // posittion in store file
  int64_t storeFilePosition;
};

#endif


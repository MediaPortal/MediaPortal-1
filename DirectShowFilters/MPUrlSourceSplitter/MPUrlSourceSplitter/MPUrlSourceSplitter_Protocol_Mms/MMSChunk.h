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

#ifndef __MMSCHUNK_DEFINED
#define __MMSCHUNK_DEFINED

// chunk type contains 2 fields: Frame and PacketID.
// Frame is 0x24 or 0xA4(rarely), different PacketID indicates different packet type.

#define CHUNK_TYPE_DATA                                           0x4424
#define CHUNK_TYPE_ASF_HEADER                                     0x4824
#define CHUNK_TYPE_END                                            0x4524
#define CHUNK_TYPE_STREAM_CHANGE                                  0x4324

class MMSChunk
{
public:
  // constructor
  // create instance of MMSChunk class
  MMSChunk(void);

  // create instance of MMSChunk class as deep clone
  // @param chunk : reference to MMS chunk (can be NULL)
  MMSChunk(MMSChunk *chunk);

  // destructor
  ~MMSChunk(void);

  // cleares current instance
  void Clear(void);

  // tests if current instance of MMS chunk is cleared
  // @return : true if instance is clear, false otherwise
  bool IsCleared(void);

  unsigned int GetChunkDataLength(void);

  unsigned int GetExtraHeaderDataLength(void);

  void SetChunkType(unsigned int headerType);

  unsigned int GetChunkType(void);

  const unsigned char *GetChunkData(void);

  const unsigned char *GetExtraHeaderData(void);

  bool SetChunkData(const unsigned char *chunkData, unsigned int length);

  bool SetExtraHeaderData(const unsigned char *extraHeaderData, unsigned int length);

protected:

  unsigned int chunkType;

  unsigned int chunkDataLength;

  unsigned int extraHeaderLength;

  unsigned char *chunkData;

  unsigned char *extraHeaderData;

  bool SetChunkDataLength(unsigned int chunkDataLength);

  bool SetExtraHeaderDataLength(unsigned int extraHeaderDataLength);
};

#endif
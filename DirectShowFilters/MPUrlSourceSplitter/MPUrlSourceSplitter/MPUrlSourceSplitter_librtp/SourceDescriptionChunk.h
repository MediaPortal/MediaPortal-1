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

#ifndef __SOURCE_DESCRIPTION_CHUNK_DEFINED
#define __SOURCE_DESCRIPTION_CHUNK_DEFINED

#include "SourceDescriptionItemCollection.h"

#define SOURCE_DESCRIPTION_CHUNK_MIN_SIZE                             8       // minimum size for source description (SSRC/CSRC + NULL SDES item + padding)

class CSourceDescriptionChunk
{
public:
  // initializes a new instance of CSourceDescriptionChunk class
  CSourceDescriptionChunk(HRESULT *result);
  virtual ~CSourceDescriptionChunk(void);

  /* get methods */

  // gets SSRC or CSRC of source description chunk
  // @return : SSRC or CSRC
  virtual unsigned int GetIdentifier(void);

  // gets chunk size
  // @return : chunk size
  virtual unsigned int GetSize(void);

  // gets items in source description chunk
  // @return : source description items
  virtual CSourceDescriptionItemCollection *GetItems(void);

  // get whole source description chunk into buffer
  // @param buffer : the buffer to store source description chunk
  // @param length : the length of buffer
  // @return : true if successful, false otherwise
  virtual bool GetChunk(unsigned char *buffer, unsigned int length);

  /* set methods */

  // sets SSRC or CSRC of source description chunk
  // @param identifier : SSRC or CSRC of source description chunk
  virtual void SetIdentifier(unsigned int identifier);

  /* other methods */

  // sets current instance to default state
  virtual void Clear(void);

  // parses data in buffer
  // @param buffer : buffer with source description chunk data for parsing
  // @param length : the length of data in buffer
  // @return : true if successfully parsed, false otherwise
  virtual bool Parse(const unsigned char *buffer, unsigned int length);

protected:

  // holds SSRC/CSRC
  unsigned int identifier;

  // holds source description items
  CSourceDescriptionItemCollection *items;
};

#endif
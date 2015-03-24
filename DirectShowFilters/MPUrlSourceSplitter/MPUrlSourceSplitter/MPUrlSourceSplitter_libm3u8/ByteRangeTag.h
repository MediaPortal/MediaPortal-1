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

#ifndef __BYTE_RANGE_TAG_DEFINED
#define __BYTE_RANGE_TAG_DEFINED

#include "Tag.h"

#define BYTE_RANGE_TAG_FLAG_NONE                                      TAG_FLAG_NONE

#define BYTE_RANGE_TAG_FLAG_LAST                                      (TAG_FLAG_LAST + 0)

#define TAG_BYTE_RANGE                                                L"EXT-X-BYTERANGE"

#define BYTE_RANGE_OFFSET_SEPARATOR                                   L"@"
#define BYTE_RANGE_OFFSET_SEPARATOR_LENGTH                            1

#define BYTE_RANGE_LENGTH_NOT_SPECIFIED                               DECIMAL_INTEGER_NOT_SPECIFIED
#define BYTE_RANGE_OFFSET_NOT_SPECIFIED                               DECIMAL_INTEGER_NOT_SPECIFIED

class CByteRangeTag : public CTag
{
public:
  CByteRangeTag(HRESULT *result);
  virtual ~CByteRangeTag(void);

  /* get methods */

  // gets offset of range request
  // @return : offset or BYTE_RANGE_OFFSET_NOT_SPECIFIED if not specified
  unsigned int GetOffset(void);

  // gets length of range request
  // @return : length or BYTE_RANGE_LENGTH_NOT_SPECIFIED if not specified
  unsigned int GetLength(void);

  /* set methods */

  /* other methods */

  // tests if item can be part of media playlist
  // @param : the playlist version
  // @return : true if item can be part of media playlist, false otherwise
  virtual bool IsMediaPlaylistItem(unsigned int version);

  // tests if item can be part of master playlist
  // @param : the playlist version
  // @return : true if item can be part of master playlist, false otherwise
  virtual bool IsMasterPlaylistItem(unsigned int version);

  // tests if tag is applicable to playlist item
  // @return : true if tag is applicable to playlist item, false otherwise
  virtual bool IsPlaylistItemTag(void);

  // applies tag to one or more playlist items
  // @param version : the playlist version
  // @param notProcessedItems : the not processed playlist items including current tag
  // @param processedPlaylistItems : the processed playlist items (they not exist in notProcessedItems collection)
  // @return : true if tag is applied to playlist items, false otherwise
  virtual bool ApplyTagToPlaylistItems(unsigned int version, CItemCollection *notProcessedItems, CPlaylistItemCollection *processedPlaylistItems);

  // clears current instance
  virtual void Clear(void);

protected:

  // holds length of the sub-range in bytes
  unsigned int length;
  // holds byte offset from the beginning of the resource
  unsigned int offset;

  /* methods */

  // parses current tag
  // @param : the playlist version
  // @return : true if successful, false otherwise
  virtual bool ParseTag(unsigned int version);

  // creates item
  // @return : item or NULL if error
  virtual CItem *CreateItem(void);

  // deeply clones current instance to specified item
  // @param item : the item to clone current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CItem *item);
};

#endif
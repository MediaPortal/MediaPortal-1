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

#ifndef __PLAYLIST_ITEM_DEFINED
#define __PLAYLIST_ITEM_DEFINED

#include "Item.h"
#include "TagCollection.h"

#define PLAYLIST_ITEM_FLAG_NONE                                                ITEM_FLAG_NONE

#define PLAYLIST_ITEM_FLAG_LAST                                                (ITEM_FLAG_LAST + 0)

class CPlaylistItem : public CItem
{
public:
  CPlaylistItem(HRESULT *result);
  virtual ~CPlaylistItem(void);

  /* get methods */

  // gets tags associated to playlist item
  // @return : tag collection
  CTagCollection *GetTags(void);

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

  // clears current instance
  virtual void Clear(void);

  // parses data in buffer
  // @param buffer : buffer with item data for parsing
  // @param length : the length of data in buffer
  // @param : the playlist version
  // @return : return position in buffer after processing or 0 if not processed
  virtual unsigned int Parse(const wchar_t *buffer, unsigned int length, unsigned int version);

  // parses item data
  // @param item : the item to parse
  // @return : true if successful, false otherwise
  virtual bool ParsePlaylistItem(CItem *item);

protected:

  // holds tags associated to playlist item
  CTagCollection *tags;

  /* methods */

  // creates item
  // @return : item or NULL if error
  virtual CItem *CreateItem(void);

  // deeply clones current instance to specified item
  // @param item : the item to clone current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CItem *payloadType);
};

#endif
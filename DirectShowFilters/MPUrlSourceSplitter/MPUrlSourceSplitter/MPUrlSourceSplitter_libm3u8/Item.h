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

#ifndef __ITEM_DEFINED
#define __ITEM_DEFINED

#include "Flags.h"

#define ITEM_FLAG_NONE                                                FLAGS_NONE

#define ITEM_FLAG_PLAYLIST_ITEM                                       (1 << (FLAGS_LAST + 0))
#define ITEM_FLAG_TAG                                                 (1 << (FLAGS_LAST + 1))
#define ITEM_FLAG_COMMENT                                             (1 << (FLAGS_LAST + 2))

#define ITEM_FLAG_LAST                                                (FLAGS_LAST + 3)

class CItem : public CFlags
{
public:
  CItem(HRESULT *result);
  virtual ~CItem(void);

  /* get methods */

  // gets item content
  // @return : item content or NULL if not specified
  virtual const wchar_t *GetItemContent(void);

  /* set methods */

  /* other methods */

  // tests if item is playlist item
  // @return : true if item is playlist item, false otherwise
  bool IsPlaylistItem(void);

  // tests if item is tag
  // @return : true if item is tag, false otherwise
  bool IsTag(void);

  // tests if item is comment
  // @return : true if item is comment, false otherwise
  bool IsComment(void);

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
  virtual bool ParseItem(CItem *item);

  // deep clone of current instance
  // @return : reference to clone of item or NULL if error
  virtual CItem *Clone(void);

protected:

  // holds item content
  wchar_t *itemContent;

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
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

#ifndef __DURATION_TITLE_FLOATING_TAG_DEFINED
#define __DURATION_TITLE_FLOATING_TAG_DEFINED

#include "Tag.h"

#define DURATION_TITLE_FLOATING_TAG_FLAG_NONE                         TAG_FLAG_NONE

#define DURATION_TITLE_FLOATING_TAG_FLAG_LAST                         (TAG_FLAG_LAST + 0)

#define TAG_DURATION_TITLE_FLOATING                                   L"EXTINF"

#define DURATION_TITLE_FLOATING_SEPARATOR                             L","
#define DURATION_TITLE_FLOATING_SEPARATOR_LENGTH                      1

class CDurationTitleFloatingTag : public CTag
{
public:
  CDurationTitleFloatingTag(HRESULT *result);
  virtual ~CDurationTitleFloatingTag(void);

  /* get methods */

  // gets duration in ms
  // @return : duration in ms
  unsigned int GetDuration(void);

  // gets title
  // @return : title or NULL if not specified
  const wchar_t *GetTitle(void);

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

  // holds duration in ms
  unsigned int duration;
  // holds title
  wchar_t *title;

  /* methods */

  // parses current tag
  // @return : true if successful, false otherwise
  virtual bool ParseTag(void);

  // creates item
  // @return : item or NULL if error
  virtual CItem *CreateItem(void);

  // deeply clones current instance to specified item
  // @param item : the item to clone current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CItem *item);
};

#endif
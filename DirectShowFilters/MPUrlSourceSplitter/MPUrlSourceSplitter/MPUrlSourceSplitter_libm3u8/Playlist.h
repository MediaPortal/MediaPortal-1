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

#ifndef __PLAYLIST_DEFINED
#define __PLAYLIST_DEFINED

#include "Flags.h"
#include "ItemCollection.h"
#include "TagCollection.h"
#include "PlaylistItemCollection.h"

#define PLAYLIST_FLAG_NONE                                            FLAGS_NONE

#define PLAYLIST_FLAG_DETECTED_HEADER                                 (1 << (FLAGS_LAST + 0))
#define PLAYLIST_FLAG_DETECTED_VERSION_01                             (1 << (FLAGS_LAST + 1))

#define PLAYLIST_FLAG_LAST                                            (FLAGS_LAST + 2)

class CPlaylist : public CFlags
{
public:
  CPlaylist(HRESULT *result);
  virtual ~CPlaylist(void);

  /* get methods */

  // gets playlist detected version
  // @return : playlist detected version
  virtual unsigned int GetDetectedVersion(void);

  // gets item collection
  // @return : item collection
  virtual CItemCollection *GetItems(void);

  // gets tag collection
  // @return : tag collection
  virtual CTagCollection *GetTags(void);

  // gets playlist items collection
  // @return : playlist items collection
  virtual CPlaylistItemCollection *GetPlaylistItems(void);

  /* set methods */

  /* other methods */

  // clears current instance to default state
  virtual void Clear(void);

  // parses data in buffer
  // @param buffer : buffer with session description data for parsing
  // @param length : the length of data in buffer
  // @return : true if successfully parsed, false otherwise
  virtual HRESULT Parse(const wchar_t *buffer, unsigned int length);

  // parses tags and playlist items into playlist structures
  // @param tag : the tag collection to parse
  // @param playlistItems : the playlist items to parse
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT ParseTagsAndPlaylistItems(CTagCollection *tags, CPlaylistItemCollection *playlistItems);

protected:

  // holds detected version
  unsigned int detectedVersion;

  // holds playlist items, tags and comments
  CItemCollection *items;

  // holds playlist tag (not associated to any playlist item)
  CTagCollection *tags;

  // holds playlist items (with associated tags)
  CPlaylistItemCollection *playlistItems;

  /* methods */

  // parses items into playlist
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT ParseItemsInternal(void);

  // checks detected playlist version
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT CheckPlaylistVersion(void);

  // parses tags and playlist items
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT ParseTagsAndPlaylistItemsInternal(void);
};

#endif
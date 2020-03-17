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

#ifndef __MEDIA_PLAYLIST_DEFINED
#define __MEDIA_PLAYLIST_DEFINED

#include "Playlist.h"
#include "M3u8FragmentCollection.h"

#define MEDIA_PLAYLIST_FLAG_NONE                                      PLAYLIST_FLAG_NONE

#define MEDIA_PLAYLIST_FLAG_LAST                                      (PLAYLIST_FLAG_LAST + 0)

class CMediaPlaylist : public CPlaylist
{
public:
  CMediaPlaylist(HRESULT *result);
  virtual ~CMediaPlaylist(void);

  /* get methods */

  // gets media playlist version
  // @return : media playlist version
  virtual unsigned int GetVersion(void);

  // gets media playlist fragments
  // @return : media playlist fragments
  virtual CM3u8FragmentCollection *GetFragments(void);

  /* set methods */

  /* other methods */

protected:

  // holds m3u8 fragments
  CM3u8FragmentCollection *fragments;

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
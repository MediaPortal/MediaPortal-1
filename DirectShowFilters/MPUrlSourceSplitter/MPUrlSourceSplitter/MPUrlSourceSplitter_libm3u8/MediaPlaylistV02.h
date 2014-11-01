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

#ifndef __MEDIA_PLAYLIST_V02_DEFINED
#define __MEDIA_PLAYLIST_V02_DEFINED

#include "MediaPlaylist.h"

#define MEDIA_PLAYLIST_V02_FLAG_NONE                                  MEDIA_PLAYLIST_FLAG_NONE

#define MEDIA_PLAYLIST_V02_FLAG_LAST                                  (MEDIA_PLAYLIST_FLAG_LAST + 0)

#define MEDIA_PLAYLIST_VERSION_02                                     PLAYLIST_VERSION_02

class CMediaPlaylistV02 : public CMediaPlaylist
{
public:
  CMediaPlaylistV02(HRESULT *result);
  virtual ~CMediaPlaylistV02(void);

  /* get methods */

  // gets media playlist version
  // @return : media playlist version
  virtual unsigned int GetVersion(void);

  /* set methods */

  /* other methods */

protected:

  /* methods */

  // checks detected playlist version
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT CheckPlaylistVersion(void);

  // parses tags and playlist items
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT ParseTagsAndPlaylistItemsInternal(void);
};

#endif
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

#ifndef __MASTER_PLAYLIST_V01_DEFINED
#define __MASTER_PLAYLIST_V01_DEFINED

#include "MasterPlaylist.h"

#define MASTER_PLAYLIST_V01_FLAG_NONE                                 MASTER_PLAYLIST_FLAG_NONE

#define MASTER_PLAYLIST_V01_FLAG_LAST                                 (MASTER_PLAYLIST_FLAG_LAST + 0)

#define MASTER_PLAYLIST_VERSION_01                                    PLAYLIST_VERSION_01

class CMasterPlaylistV01 : public CMasterPlaylist
{
public:
  CMasterPlaylistV01(HRESULT *result);
  virtual ~CMasterPlaylistV01(void);

  /* get methods */

  // gets master playlist version
  // @return : master playlist version
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
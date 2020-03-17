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

#ifndef __MASTER_PLAYLIST_FACTORY_DEFINED
#define __MASTER_PLAYLIST_FACTORY_DEFINED

#include "MasterPlaylist.h"

#define CREATE_SPECIFIC_PLAYLIST(tempPlaylist, playlistType, version, continueParsing, playlist)                      \
                                                                                                                      \
if (SUCCEEDED(continueParsing) && (playlist == NULL) && (tempPlaylist->GetDetectedVersion() == version))              \
{                                                                                                                     \
  playlistType *specificPlaylist = new playlistType(&continueParsing);                                                \
  CHECK_POINTER_HRESULT(continueParsing, specificPlaylist, continueParsing, E_OUTOFMEMORY);                           \
                                                                                                                      \
  if (SUCCEEDED(continueParsing))                                                                                     \
  {                                                                                                                   \
    continueParsing = specificPlaylist->ParseTagsAndPlaylistItems(tempPlaylist->GetTags(), tempPlaylist->GetPlaylistItems()); \
    if (SUCCEEDED(continueParsing))                                                                                   \
    {                                                                                                                 \
      playlist = specificPlaylist;                                                                                    \
    }                                                                                                                 \
  }                                                                                                                   \
                                                                                                                      \
  if (playlist == NULL)                                                                                               \
  {                                                                                                                   \
    FREE_MEM_CLASS(specificPlaylist);                                                                                 \
  }                                                                                                                   \
}


class CMasterPlaylistFactory
{
public:
  CMasterPlaylistFactory(HRESULT *result);
  ~CMasterPlaylistFactory(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // creates master playlist from buffer
  // @param result : reference to HRESULT variable holding error code if some error
  // @param buffer : buffer with master playlist data for parsing
  // @param length : the length of data in buffer
  // @return : master playlist or NULL
  CMasterPlaylist *CreateMasterPlaylist(HRESULT *result, const wchar_t *buffer, unsigned int length);
};

#endif
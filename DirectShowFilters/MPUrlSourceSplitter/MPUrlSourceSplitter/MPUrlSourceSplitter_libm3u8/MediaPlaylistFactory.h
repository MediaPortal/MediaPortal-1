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

#ifndef __MEDIA_PLAYLIST_FACTORY_DEFINED
#define __MEDIA_PLAYLIST_FACTORY_DEFINED

#include "MediaPlaylist.h"

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


class CMediaPlaylistFactory
{
public:
  CMediaPlaylistFactory(HRESULT *result);
  ~CMediaPlaylistFactory(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // creates media playlist from buffer
  // @param result : reference to HRESULT variable holding error code if some error
  // @param buffer : buffer with media playlist data for parsing
  // @param length : the length of data in buffer
  // @return : media playlist or NULL
  CMediaPlaylist *CreateMediaPlaylist(HRESULT *result, const wchar_t *buffer, unsigned int length);
};

#endif
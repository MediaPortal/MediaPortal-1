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

#include "StdAfx.h"

#include "MediaPlaylistFactory.h"
#include "MediaPlaylistV01.h"
#include "MediaPlaylistV02.h"
#include "MediaPlaylistV03.h"
#include "MediaPlaylistV04.h"
#include "MediaPlaylistV05.h"
#include "MediaPlaylistV06.h"
#include "MediaPlaylistV07.h"

CMediaPlaylistFactory::CMediaPlaylistFactory(HRESULT *result)
{
}

CMediaPlaylistFactory::~CMediaPlaylistFactory(void)
{
}

/* get methods */

/* set methods */

/* other methods */

CMediaPlaylist *CMediaPlaylistFactory::CreateMediaPlaylist(HRESULT *result, const wchar_t *buffer, unsigned int length)
{
  CMediaPlaylist *mediaPlaylist = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, buffer);
    CHECK_CONDITION_HRESULT(*result, length > 0, *result, E_INVALIDARG);

    if (SUCCEEDED(*result))
    {
      CMediaPlaylist *temp = new CMediaPlaylist(result);
      CHECK_POINTER_HRESULT(*result, temp, *result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE(SUCCEEDED(*result), *result = temp->Parse(buffer, length));

      if (SUCCEEDED(*result))
      {
        // check most specific media playlists first
        CREATE_SPECIFIC_PLAYLIST(temp, CMediaPlaylistV07, MEDIA_PLAYLIST_VERSION_07, (*result), mediaPlaylist);
        CREATE_SPECIFIC_PLAYLIST(temp, CMediaPlaylistV06, MEDIA_PLAYLIST_VERSION_06, (*result), mediaPlaylist);
        CREATE_SPECIFIC_PLAYLIST(temp, CMediaPlaylistV05, MEDIA_PLAYLIST_VERSION_05, (*result), mediaPlaylist);
        CREATE_SPECIFIC_PLAYLIST(temp, CMediaPlaylistV04, MEDIA_PLAYLIST_VERSION_04, (*result), mediaPlaylist);
        CREATE_SPECIFIC_PLAYLIST(temp, CMediaPlaylistV03, MEDIA_PLAYLIST_VERSION_03, (*result), mediaPlaylist);
        CREATE_SPECIFIC_PLAYLIST(temp, CMediaPlaylistV02, MEDIA_PLAYLIST_VERSION_02, (*result), mediaPlaylist);
        CREATE_SPECIFIC_PLAYLIST(temp, CMediaPlaylistV01, MEDIA_PLAYLIST_VERSION_01, (*result), mediaPlaylist);
      }

      CHECK_CONDITION_NOT_NULL_EXECUTE(mediaPlaylist, FREE_MEM_CLASS(temp));

      if (SUCCEEDED(*result) && (mediaPlaylist == NULL))
      {
        mediaPlaylist = temp;
      }

      CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(temp));
    }

    CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(mediaPlaylist));
  }

  return mediaPlaylist;
}

/* protected methods */

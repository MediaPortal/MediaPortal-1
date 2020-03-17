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

#include "MasterPlaylistFactory.h"
#include "MasterPlaylistV01.h"

CMasterPlaylistFactory::CMasterPlaylistFactory(HRESULT *result)
{
}

CMasterPlaylistFactory::~CMasterPlaylistFactory(void)
{
}

/* get methods */

/* set methods */

/* other methods */

CMasterPlaylist *CMasterPlaylistFactory::CreateMasterPlaylist(HRESULT *result, const wchar_t *buffer, unsigned int length)
{
  CMasterPlaylist *masterPlaylist = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, buffer);
    CHECK_CONDITION_HRESULT(*result, length > 0, *result, E_INVALIDARG);

    if (SUCCEEDED(*result))
    {
      CMasterPlaylist *temp = new CMasterPlaylist(result);
      CHECK_POINTER_HRESULT(*result, temp, *result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE(SUCCEEDED(*result), *result = temp->Parse(buffer, length));

      if (SUCCEEDED(*result))
      {
        // check most specific master playlists first
        CREATE_SPECIFIC_PLAYLIST(temp, CMasterPlaylistV01, MASTER_PLAYLIST_VERSION_01, (*result), masterPlaylist);
      }

      CHECK_CONDITION_NOT_NULL_EXECUTE(masterPlaylist, FREE_MEM_CLASS(temp));

      if (SUCCEEDED(*result) && (masterPlaylist == NULL))
      {
        masterPlaylist = temp;
      }

      CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(temp));
    }

    CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(masterPlaylist));
  }

  return masterPlaylist;
}

/* protected methods */

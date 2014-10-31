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

#include "MasterPlaylist.h"
#include "ErrorCodes.h"

CMasterPlaylist::CMasterPlaylist(HRESULT *result)
  : CPlaylist(result)
{
}

CMasterPlaylist::~CMasterPlaylist(void)
{
}

/* get methods */

unsigned int CMasterPlaylist::GetVersion(void)
{
  return PLAYLIST_VERSION_NOT_DEFINED;
}

/* set methods */

/* other methods */

/* protected methods */

HRESULT CMasterPlaylist::ParseItemsInternal(void)
{
  HRESULT result = __super::ParseItemsInternal();

  if (SUCCEEDED(result) && (this->GetVersion() != PLAYLIST_VERSION_NOT_DEFINED))
  {
    // check validity of master playlist

    for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->playlistItems->Count())); i++)
    {
      CPlaylistItem *playlistItem = this->playlistItems->GetItem(i);

      CHECK_CONDITION_HRESULT(result, playlistItem->IsMasterPlaylistItem(this->GetVersion()), result, E_M3U8_NOT_SUPPORTED_PLAYLIST_ITEM);

      for (unsigned int j = 0; (SUCCEEDED(result) && (j < playlistItem->GetTags()->Count())); j++)
      {
        CTag *tag = playlistItem->GetTags()->GetItem(j);

        CHECK_CONDITION_HRESULT(result, tag->IsMasterPlaylistItem(this->GetVersion()), result, E_M3U8_NOT_SUPPORTED_TAG);
      }
    }
  }

  return result;
}

HRESULT CMasterPlaylist::CheckPlaylistVersion(void)
{
  HRESULT result = S_OK;

  // check playlist version against our playlist version (if specified)
  if (this->GetVersion() != PLAYLIST_VERSION_NOT_DEFINED)
  {
    result = (this->GetVersion() == this->detectedVersion) ? S_OK : E_M3U8_NOT_SUPPORTED_PLAYLIST_VERSION;
  }

  return result;
}

HRESULT CMasterPlaylist::ParseTagsAndPlaylistItemsInternal(void)
{
  HRESULT result = __super::ParseTagsAndPlaylistItemsInternal();

  if (SUCCEEDED(result) && (this->GetVersion() != PLAYLIST_VERSION_NOT_DEFINED))
  {
    // check validity of master playlist

    for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->playlistItems->Count())); i++)
    {
      CPlaylistItem *playlistItem = this->playlistItems->GetItem(i);

      CHECK_CONDITION_HRESULT(result, playlistItem->IsMasterPlaylistItem(this->GetVersion()), result, E_M3U8_NOT_SUPPORTED_PLAYLIST_ITEM);

      for (unsigned int j = 0; (SUCCEEDED(result) && (j < playlistItem->GetTags()->Count())); j++)
      {
        CTag *tag = playlistItem->GetTags()->GetItem(j);

        CHECK_CONDITION_HRESULT(result, tag->IsMasterPlaylistItem(this->GetVersion()), result, E_M3U8_NOT_SUPPORTED_TAG);
      }
    }
  }

  return result;
}
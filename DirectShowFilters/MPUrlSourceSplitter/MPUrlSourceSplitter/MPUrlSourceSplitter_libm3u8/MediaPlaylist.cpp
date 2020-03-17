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

#include "MediaPlaylist.h"
#include "ErrorCodes.h"

CMediaPlaylist::CMediaPlaylist(HRESULT *result)
  : CPlaylist(result)
{
  this->fragments = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->fragments = new CM3u8FragmentCollection(result);

    CHECK_POINTER_HRESULT(*result, this->fragments, *result, E_OUTOFMEMORY);
  }
}

CMediaPlaylist::~CMediaPlaylist(void)
{
  FREE_MEM_CLASS(this->fragments);
}

/* get methods */

unsigned int CMediaPlaylist::GetVersion(void)
{
  return PLAYLIST_VERSION_NOT_DEFINED;
}

CM3u8FragmentCollection *CMediaPlaylist::GetFragments(void)
{
  return this->fragments;
}

/* set methods */

/* other methods */

/* protected methods */

HRESULT CMediaPlaylist::ParseItemsInternal(void)
{
  HRESULT result = __super::ParseItemsInternal();

  if (SUCCEEDED(result) && (this->GetVersion() != PLAYLIST_VERSION_NOT_DEFINED))
  {
    // check validity of media playlist

    for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->playlistItems->Count())); i++)
    {
      CPlaylistItem *playlistItem = this->playlistItems->GetItem(i);

      CHECK_CONDITION_HRESULT(result, playlistItem->IsMediaPlaylistItem(this->GetVersion()), result, E_M3U8_NOT_SUPPORTED_PLAYLIST_ITEM);

      for (unsigned int j = 0; (SUCCEEDED(result) && (j < playlistItem->GetTags()->Count())); j++)
      {
        CTag *tag = playlistItem->GetTags()->GetItem(j);

        CHECK_CONDITION_HRESULT(result, tag->IsMediaPlaylistItem(this->GetVersion()), result, E_M3U8_NOT_SUPPORTED_TAG);
      }
    }
  }

  return result;
}

HRESULT CMediaPlaylist::CheckPlaylistVersion(void)
{
  HRESULT result = S_OK;

  // check playlist version against our playlist version (if specified)
  if (this->GetVersion() != PLAYLIST_VERSION_NOT_DEFINED)
  {
    result = (this->GetVersion() == this->detectedVersion) ? S_OK : E_M3U8_NOT_SUPPORTED_PLAYLIST_VERSION;
  }

  return result;
}

HRESULT CMediaPlaylist::ParseTagsAndPlaylistItemsInternal(void)
{
  this->fragments->Clear();
  HRESULT result = __super::ParseTagsAndPlaylistItemsInternal();

  if (SUCCEEDED(result) && (this->GetVersion() != PLAYLIST_VERSION_NOT_DEFINED))
  {
    // check validity of media playlist

    for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->playlistItems->Count())); i++)
    {
      CPlaylistItem *playlistItem = this->playlistItems->GetItem(i);

      CHECK_CONDITION_HRESULT(result, playlistItem->IsMediaPlaylistItem(this->GetVersion()), result, E_M3U8_NOT_SUPPORTED_PLAYLIST_ITEM);

      for (unsigned int j = 0; (SUCCEEDED(result) && (j < playlistItem->GetTags()->Count())); j++)
      {
        CTag *tag = playlistItem->GetTags()->GetItem(j);

        CHECK_CONDITION_HRESULT(result, tag->IsMediaPlaylistItem(this->GetVersion()), result, E_M3U8_NOT_SUPPORTED_TAG);
      }
    }
  }

  return result;
}
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

#include "Playlist.h"
#include "ItemFactory.h"
#include "GeneralTag.h"
#include "PlaylistItem.h"
#include "ErrorCodes.h"
#include "HeaderTag.h"
#include "VersionTag.h"

CPlaylist::CPlaylist(HRESULT *result)
  : CFlags()
{
  this->items = NULL;
  this->tags = NULL;
  this->playlistItems = NULL;
  this->detectedVersion = PLAYLIST_VERSION_NOT_DEFINED;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->items = new CItemCollection(result);
    this->tags = new CTagCollection(result);
    this->playlistItems = new CPlaylistItemCollection(result);

    CHECK_POINTER_HRESULT(*result, this->items, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->tags, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->playlistItems, *result, E_OUTOFMEMORY);
  }
}

CPlaylist::~CPlaylist(void)
{
  FREE_MEM_CLASS(this->items);
  FREE_MEM_CLASS(this->tags);
  FREE_MEM_CLASS(this->playlistItems);
}

/* get methods */

unsigned int CPlaylist::GetDetectedVersion(void)
{
  return this->detectedVersion;
}

CItemCollection *CPlaylist::GetItems(void)
{
  return this->items;
}

CTagCollection *CPlaylist::GetTags(void)
{
  return this->tags;
}

CPlaylistItemCollection *CPlaylist::GetPlaylistItems(void)
{
  return this->playlistItems;
}

/* set methods */

/* other methods */

void CPlaylist::Clear(void)
{
  this->flags = PLAYLIST_FLAG_NONE;
  this->items->Clear();
  this->tags->Clear();
  this->playlistItems->Clear();
  this->detectedVersion = PLAYLIST_VERSION_NOT_DEFINED;
}

HRESULT CPlaylist::Parse(const wchar_t *buffer, unsigned int length)
{
  this->Clear();

  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, buffer);
  CHECK_CONDITION_HRESULT(result, length > 0, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    // split playlist into lines
    // skip all empty lines

    unsigned int processed = 0;
    CItemFactory *factory = new CItemFactory(&result);
    CHECK_POINTER_HRESULT(result, factory, result, E_OUTOFMEMORY);

    while (SUCCEEDED(result) && (processed < length))
    {
      unsigned int position = 0;
      CItem *item = factory->CreateItem(&result, buffer + processed, length - processed, &position);
      CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

      // in playlist can be only tag or playlist item
      // skip everything else (comments, empty lines, ...)

      if ((!this->IsSetFlags(PLAYLIST_FLAG_DETECTED_HEADER)) && item->IsTag())
      {
        CHeaderTag *headerTag = dynamic_cast<CHeaderTag *>(item);
        this->flags |= (headerTag != NULL) ? PLAYLIST_FLAG_DETECTED_HEADER : PLAYLIST_FLAG_NONE;
      }

      if (SUCCEEDED(result) && (item->IsTag() || item->IsPlaylistItem()))
      {
        CHECK_CONDITION_HRESULT(result, this->items->Add(item), result, E_OUTOFMEMORY);
      }
      else
      {
        FREE_MEM_CLASS(item);
      }
      
      processed += position;
    }

    FREE_MEM_CLASS(factory);
  }

  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->ParseItemsInternal());

  return result;
}

HRESULT CPlaylist::ParseTagsAndPlaylistItems(CTagCollection *tags, CPlaylistItemCollection *playlistItems)
{
  this->Clear();

  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, tags);
  CHECK_POINTER_DEFAULT_HRESULT(result, playlistItems);

  if (SUCCEEDED(result))
  {
    CHECK_CONDITION_HRESULT(result, this->tags->Append(tags), result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(result, this->playlistItems->Append(playlistItems), result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      // each playlist must contain at least one item and must contain header tag
      // the header tag must be first tag in playlist

      CHeaderTag *headerTag = dynamic_cast<CHeaderTag *>(this->tags->GetItem(0));
      CHECK_POINTER_HRESULT(result, headerTag, result, E_M3U8_NOT_PLAYLIST);

      this->flags |= (headerTag != NULL) ? PLAYLIST_FLAG_DETECTED_HEADER : PLAYLIST_FLAG_NONE;
    }

    this->detectedVersion = PLAYLIST_VERSION_01;
    if (SUCCEEDED(result))
    {
      // each playlist must specify version
      // if not, it is PLAYLIST_VERSION_01

      CVersionTag *version = this->items->GetVersion();

      this->detectedVersion = (version != NULL) ? version->GetVersion() : PLAYLIST_VERSION_01;
      result = this->CheckPlaylistVersion();
    }

    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->ParseTagsAndPlaylistItemsInternal());
  }

  return result;
}

/* protected methods */

HRESULT CPlaylist::ParseItemsInternal(void)
{
  HRESULT result = S_OK;

  if (SUCCEEDED(result))
  {
    // each playlist must contain at least one item and must contain header tag
    // the header tag must be first tag in playlist

    CHeaderTag *headerTag = dynamic_cast<CHeaderTag *>(this->items->GetItem(0));
    CHECK_POINTER_HRESULT(result, headerTag, result, E_M3U8_NOT_PLAYLIST);

    this->flags |= (headerTag != NULL) ? PLAYLIST_FLAG_DETECTED_HEADER : PLAYLIST_FLAG_NONE;
  }

  this->detectedVersion = PLAYLIST_VERSION_01;
  if (SUCCEEDED(result))
  {
    // each playlist must specify version
    // if not, it is PLAYLIST_VERSION_01

    CVersionTag *version = this->items->GetVersion();

    this->detectedVersion = (version != NULL) ? version->GetVersion() : PLAYLIST_VERSION_01;
    result = this->CheckPlaylistVersion();
  }

  if (SUCCEEDED(result))
  {
    while(SUCCEEDED(result) && (this->items->Count() != 0))
    {
      // check if item is tag or playlist item

      CItem *item = this->items->GetItem(0);

      if (item->IsTag())
      {
        // check if tag is media or master playlist item for specified version

        CTag *tag = dynamic_cast<CTag *>(item);

        if (tag->IsMediaPlaylistItem(this->detectedVersion) || tag->IsMasterPlaylistItem(this->detectedVersion))
        {
          if (tag->IsPlaylistItemTag())
          {
            CHECK_CONDITION_HRESULT(result, tag->ApplyTagToPlaylistItems(this->detectedVersion, this->items, this->playlistItems), result, E_M3U8_NO_PLAYLIST_ITEM_FOR_TAG);
          }
          else
          {
            // tag is playlist tag
            CTag *clone = (CTag *)tag->Clone();
            CHECK_POINTER_HRESULT(result, clone, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, this->tags->Add(clone), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(clone));
          }
        }
      }
      else if (item->IsPlaylistItem())
      {
        CPlaylistItem *playlistItem = (CPlaylistItem *)item->Clone();
        CHECK_POINTER_HRESULT(result, playlistItem, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, this->playlistItems->Add(playlistItem), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(playlistItem));
      }

      // tag or playlist item is processed, we can remove it
      this->items->Remove(0);
    }
  }

  return result;
}

HRESULT CPlaylist::CheckPlaylistVersion(void)
{
  // accept all playlist versions
  return S_OK;
}

HRESULT CPlaylist::ParseTagsAndPlaylistItemsInternal(void)
{
  return S_OK;
}
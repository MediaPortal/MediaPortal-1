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

#include "MasterPlaylistV01.h"
#include "ErrorCodes.h"

CMasterPlaylistV01::CMasterPlaylistV01(HRESULT *result)
  : CMasterPlaylist(result)
{
}

CMasterPlaylistV01::~CMasterPlaylistV01(void)
{
}

/* get methods */

unsigned int CMasterPlaylistV01::GetVersion(void)
{
  return PLAYLIST_VERSION_01;
}

/* set methods */

/* other methods */

/* protected methods */

HRESULT CMasterPlaylistV01::CheckPlaylistVersion(void)
{
  HRESULT result = (PLAYLIST_VERSION_01 == this->detectedVersion) ? S_OK : E_M3U8_NOT_SUPPORTED_PLAYLIST_VERSION;

  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->flags |= PLAYLIST_FLAG_DETECTED_VERSION_01);

  return result;
}

HRESULT CMasterPlaylistV01::ParseTagsAndPlaylistItemsInternal(void)
{
  HRESULT result = __super::ParseTagsAndPlaylistItemsInternal();

  if (SUCCEEDED(result))
  {
    // master playlist version 01 has these tags:
    // EXTM3U - header tag, it is checked in CPlaylist
    // EXTINF - playlist item tag, MUST NOT be in master playlist
    // EXT-X-TARGETDURATION - playlist tag, approximate duration of the next media file that will be added to the main presentation - ignored
    // EXT-X-MEDIA-SEQUENCE - playlist tag, indicates the sequence number of the first URI that appears in a playlist file - ignored ???
    // EXT-X-KEY - multiple playlist item tag, provides information necessary to decrypt media files that follow it - E_DRM_PROTECTED
    // EXT-X-PROGRAM-DATE-TIME - playlist item tag, associates the beginning of the next media file with an absolute date and/or time - ignored
    // EXT-X-ALLOW-CACHE - playlist tag, indicates whether the client MAY cache downloaded media files for later replay - ignored
    // EXT-X-ENDLIST - playlist tag, indicates that no more media files will be added to the Playlist file
    // EXT-X-STREAM-INF - playlist item tag, indicates that the next URI in the playlist file identifies another playlist file
    // EXT-X-DISCONTINUITY - playlist item tag, indicates that the media file following it has different characteristics than the one that preceded it

    // for master playlist we need to check EXT-X-STREAM-INF tags in playlist items and create groups of same streams



    result = E_NOTIMPL;
  }

  return result;
}
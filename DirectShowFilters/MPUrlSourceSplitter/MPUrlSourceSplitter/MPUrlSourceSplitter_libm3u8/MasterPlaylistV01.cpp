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
  return (PLAYLIST_VERSION_01 == this->detectedVersion) ? S_OK : E_M3U8_NOT_SUPPORTED_PLAYLIST_VERSION;
}

HRESULT CMasterPlaylistV01::ParseTagsAndPlaylistItemsInternal(void)
{
  HRESULT result = __super::ParseTagsAndPlaylistItemsInternal();

  if (SUCCEEDED(result))
  {
    result = E_NOTIMPL;
  }

  return result;
}
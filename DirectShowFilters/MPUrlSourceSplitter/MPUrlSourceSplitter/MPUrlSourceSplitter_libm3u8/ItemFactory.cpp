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

#include "ItemFactory.h"
#include "PlaylistItem.h"
#include "GeneralTagFactory.h"
#include "ErrorCodes.h"

CItemFactory::CItemFactory(HRESULT *result)
{
}

CItemFactory::~CItemFactory(void)
{
}

/* get methods */

/* set methods */

/* other methods */

CItem *CItemFactory::CreateItem(HRESULT *result, unsigned int version, const wchar_t *buffer, unsigned int length, unsigned int *position)
{
  CItem *item = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, buffer);
    CHECK_POINTER_DEFAULT_HRESULT(*result, position);
    CHECK_CONDITION_HRESULT(*result, length > 0, *result, E_INVALIDARG);

    if (SUCCEEDED(*result))
    {
      *position = 0;
      CItem *temp = new CItem(result);
      CHECK_POINTER_HRESULT(*result, temp, *result, E_OUTOFMEMORY);

      if (SUCCEEDED(*result))
      {
        *position = temp->Parse(buffer, length, version);
        CHECK_CONDITION_HRESULT(*result, *position != 0, *result, E_M3U8_NO_ITEM_FOUND);
      }

      if (SUCCEEDED(*result))
      {
        // it is M3U8 item, it can be playlist item, tag or comment

        if (SUCCEEDED(*result))
        {
          CGeneralTagFactory *factory = new CGeneralTagFactory(result);
          CHECK_POINTER_HRESULT(*result, factory, *result, E_OUTOFMEMORY);

          CHECK_CONDITION_EXECUTE(SUCCEEDED(*result), item = factory->CreateTag(result, version, temp));

          FREE_MEM_CLASS(factory);

          switch (*result)
          {
          case E_M3U8_NO_GENERAL_TAG_FOUND:
            *result = S_OK;
            break;
          default:
            break;
          }
        }

        if (SUCCEEDED(*result) && (item == NULL))
        {
          // item is not tag, it can be playlist item
          CPlaylistItem *playlistItem = new CPlaylistItem(result);
          CHECK_POINTER_HRESULT(*result, playlistItem, *result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(*result, playlistItem->ParsePlaylistItem(temp), *result, E_M3U8_NO_PLAYLIST_ITEM_FOUND);

          CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(playlistItem));

          switch (*result)
          {
          case E_M3U8_NO_PLAYLIST_ITEM_FOUND:
            *result = S_OK;
            break;
          default:
            break;
          }

          CHECK_CONDITION_EXECUTE(SUCCEEDED(*result), item = playlistItem);
        }
      }

      CHECK_CONDITION_NOT_NULL_EXECUTE(item, FREE_MEM_CLASS(temp));

      if (SUCCEEDED(*result) && (item == NULL))
      {
        item = temp;
      }

      CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(temp));
    }

    if (FAILED(*result))
    {
      FREE_MEM_CLASS(item);
      *position = 0;
    }
  }

  return item;
}
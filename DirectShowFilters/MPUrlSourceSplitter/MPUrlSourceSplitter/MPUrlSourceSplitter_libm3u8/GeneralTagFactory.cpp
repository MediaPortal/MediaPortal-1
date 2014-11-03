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

#include "GeneralTagFactory.h"
#include "TagFactory.h"
#include "CommentTag.h"
#include "ErrorCodes.h"

CGeneralTagFactory::CGeneralTagFactory(HRESULT *result)
{
}

CGeneralTagFactory::~CGeneralTagFactory(void)
{
}

/* get methods */

/* set methods */

/* other methods */

CGeneralTag *CGeneralTagFactory::CreateTag(HRESULT *result, unsigned int version, CItem *item)
{
  CGeneralTag *tag = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, item);

    if (SUCCEEDED(*result))
    {
      CGeneralTag *temp = new CGeneralTag(result);
      CHECK_POINTER_HRESULT(*result, temp, *result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(*result, temp->ParseItem(item), *result, E_M3U8_NOT_VALID_GENERAL_TAG_FOUND);

      if (SUCCEEDED(*result))
      {
        // it is M3U8 general tag, it can be tag or comment

        if (SUCCEEDED(*result))
        {
          CTagFactory *factory = new CTagFactory(result);
          CHECK_POINTER_HRESULT(*result, factory, *result, E_OUTOFMEMORY);

          CHECK_CONDITION_EXECUTE(SUCCEEDED(*result), tag = factory->CreateTag(result, version, temp));

          FREE_MEM_CLASS(factory);

          switch (*result)
          {
          case E_M3U8_NOT_VALID_TAG_FOUND:
            *result = S_OK;
            break;
          default:
            break;
          }
        }

        if (SUCCEEDED(*result) && (tag == NULL))
        {
          CCommentTag *comment = new CCommentTag(result);
          CHECK_POINTER_HRESULT(*result, comment, *result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(*result, comment->ParseGeneralTag(temp, version), *result, E_M3U8_NOT_VALID_COMMENT_TAG_FOUND);

          CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(comment));

          switch (*result)
          {
          case E_M3U8_NOT_VALID_COMMENT_TAG_FOUND:
            *result = S_OK;
            break;
          default:
            break;
          }

          CHECK_CONDITION_EXECUTE(SUCCEEDED(*result), tag = comment);
        }
      }

      CHECK_CONDITION_NOT_NULL_EXECUTE(tag, FREE_MEM_CLASS(temp));

      if (SUCCEEDED(*result) && (tag == NULL))
      {
        tag = temp;
      }

      CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(temp));
    }

    CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(tag));
  }

  return tag;
}
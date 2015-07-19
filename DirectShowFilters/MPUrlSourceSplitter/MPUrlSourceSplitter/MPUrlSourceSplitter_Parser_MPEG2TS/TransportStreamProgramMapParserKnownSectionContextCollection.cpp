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

#include "stdafx.h"
#include "TransportStreamProgramMapParserKnownSectionContextCollection.h"

CTransportStreamProgramMapParserKnownSectionContextCollection::CTransportStreamProgramMapParserKnownSectionContextCollection(HRESULT *result)
  : CCollection(result)
{
}

CTransportStreamProgramMapParserKnownSectionContextCollection::~CTransportStreamProgramMapParserKnownSectionContextCollection()
{
}

/* get methods */

/* set methods */

/* other methods */

bool CTransportStreamProgramMapParserKnownSectionContextCollection::Add(uint16_t programNumber, unsigned int crc32)
{
  // ensure that enough space is in collection
  HRESULT result = S_OK;
  CHECK_CONDITION_HRESULT(result, this->EnsureEnoughSpace(this->itemCount + 1), result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    unsigned int startIndex = 0;
    unsigned int endIndex = 0;

    CHECK_CONDITION_HRESULT(result, this->GetItemInsertPosition(programNumber, &startIndex, &endIndex), result, E_FAIL);

    if (SUCCEEDED(result))
    {
      if (startIndex == endIndex)
      {
        // program number already in collection, just update CRC32

        CTransportStreamProgramMapParserKnownSectionContext *context = this->GetItem(endIndex);

        context->SetCrc32(crc32);
      }
      else
      {
        endIndex = min(endIndex, this->itemCount);

        CTransportStreamProgramMapParserKnownSectionContext *context = new CTransportStreamProgramMapParserKnownSectionContext(&result, programNumber, crc32);
        CHECK_POINTER_HRESULT(result, context, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, __super::Insert(endIndex, context), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(context));
      }
    }
  }

  return SUCCEEDED(result);
}

bool CTransportStreamProgramMapParserKnownSectionContextCollection::Contains(uint16_t programNumber, unsigned int crc32)
{
  unsigned int startIndex = 0;
  unsigned int endIndex = 0;

  bool result = this->GetItemInsertPosition(programNumber, &startIndex, &endIndex);
  result &= (startIndex == endIndex);

  if (result)
  {
    // program number already in collection, check CRC32

    CTransportStreamProgramMapParserKnownSectionContext *context = this->GetItem(endIndex);

    result &= (crc32 == context->GetCrc32());
  }

  return result;
}

/* protected methods */

bool CTransportStreamProgramMapParserKnownSectionContextCollection::GetItemInsertPosition(uint16_t value, unsigned int *startIndex, unsigned int *endIndex)
{
  bool result = ((startIndex != NULL) && (endIndex != NULL));

  if (result)
  {
    result = (this->Count() > 0);

    if (result)
    {
      unsigned int first = 0;
      unsigned int last = this->Count() - 1;
      result = false;

      while ((first <= last) && (first != UINT_MAX) && (last != UINT_MAX))
      {
        // compute middle index
        unsigned int middle = (first + last) / 2;

        // get item at middle index
        unsigned int item = this->GetItem(middle)->GetProgramNumber();

        // compare item value
        if (value > item)
        {
          // value is bigger than item
          // search in top half
          first = middle + 1;
        }
        else if (value < item)
        {
          // value is lower than item
          // search in bottom half
          last = middle - 1;
        }
        else
        {
          // we found item with same value
          *startIndex = middle;
          *endIndex = middle;
          result = true;
          break;
        }
      }

      if (!result)
      {
        // we don't found value
        // it means that item with 'value' belongs between first and last
        *startIndex = last;
        *endIndex = (first >= this->Count()) ? UINT_MAX : first;
        result = true;
      }
    }
    else
    {
      *startIndex = UINT_MAX;
      *endIndex = 0;
      result = true;
    }
  }

  return result;
}

CTransportStreamProgramMapParserKnownSectionContext *CTransportStreamProgramMapParserKnownSectionContextCollection::Clone(CTransportStreamProgramMapParserKnownSectionContext *item)
{
  return NULL;
}
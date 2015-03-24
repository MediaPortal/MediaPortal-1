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

#include "MovieExtendsHeaderBox.h"
#include "BoxCollection.h"

CMovieExtendsHeaderBox::CMovieExtendsHeaderBox(HRESULT *result)
  : CFullBox(result)
{
  this->type = NULL;
  this->fragmentDuration = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(MOVIE_EXTENDS_HEADER_BOX_TYPE);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
  }
}

CMovieExtendsHeaderBox::~CMovieExtendsHeaderBox(void)
{
}

/* get methods */

uint64_t CMovieExtendsHeaderBox::GetFragmentDuration(void)
{
  return this->fragmentDuration;
}

/* set methods */

void CMovieExtendsHeaderBox::SetFragmentDuration(uint64_t fragmentDuration)
{
  this->fragmentDuration = fragmentDuration;
}

/* other methods */

wchar_t *CMovieExtendsHeaderBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sFragment duration: %u"
      ,
      
      previousResult,
      indent, this->GetFragmentDuration()

      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CMovieExtendsHeaderBox::GetBoxSize(void)
{
  uint64_t result = 0;

  switch(this->GetVersion())
  {
  case 0:
    result = 4;
    break;
  case 1:
    result = 8;
    break;
  default:
    break;
  }

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CMovieExtendsHeaderBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, MOVIE_EXTENDS_HEADER_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is movie extends header box, parse all values
      uint32_t position = this->HasExtendedHeader() ? FULL_BOX_HEADER_LENGTH_SIZE64 : FULL_BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        switch (this->GetVersion())
        {
        case 0:
          RBE32INC(buffer, position, this->fragmentDuration);
          break;
        case 1:
          RBE64INC(buffer, position, this->fragmentDuration);
          break;
        default:
          continueParsing = E_FAIL;
          break;
        }
      }

      if (SUCCEEDED(continueParsing) && processAdditionalBoxes)
      {
        this->ProcessAdditionalBoxes(buffer, length, position);
      }

      this->flags &= ~BOX_FLAG_PARSED;
      this->flags |= SUCCEEDED(continueParsing) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
    }
  }

  return this->IsSetFlags(BOX_FLAG_PARSED);
}

uint32_t CMovieExtendsHeaderBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    switch(this->GetVersion())
    {
    case 0:
      WBE32INC(buffer, result, this->GetFragmentDuration());
      break;
    case 1:
      WBE64INC(buffer, result, this->GetFragmentDuration());
      break;
    default:
      result = 0;
      break;
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}
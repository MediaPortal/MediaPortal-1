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

#include "MediaHeaderBox.h"
#include "BoxCollection.h"

CMediaHeaderBox::CMediaHeaderBox(HRESULT *result)
  : CFullBox(result)
{
  this->creationTime = 0;
  this->modificationTime = 0;
  this->timeScale = 0;
  this->duration = 0;
  this->type = NULL;
  this->language = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(MEDIA_HEADER_BOX_TYPE);
    this->language = Duplicate(MEDIA_HEADER_LANGUAGE_UNDEFINED);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->language, *result, E_OUTOFMEMORY);
  }
}

CMediaHeaderBox::~CMediaHeaderBox(void)
{
  FREE_MEM(this->language);
}

/* get methods */

bool CMediaHeaderBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

uint64_t CMediaHeaderBox::GetCreationTime(void)
{
  return this->creationTime;
}

uint64_t CMediaHeaderBox::GetModificationTime(void)
{
  return this->modificationTime;
}

uint32_t CMediaHeaderBox::GetTimeScale(void)
{
  return this->timeScale;
}

uint64_t CMediaHeaderBox::GetDuration(void)
{
  return this->duration;
}

const wchar_t *CMediaHeaderBox::GetLanguage(void)
{
  return this->language;
}

/* set methods */

void CMediaHeaderBox::SetCreationTime(uint64_t creationTime)
{
  this->creationTime = creationTime;
}

void CMediaHeaderBox::SetModificationTime(uint64_t modificationTime)
{
  this->modificationTime = modificationTime;
}

void CMediaHeaderBox::SetTimeScale(uint32_t timeScale)
{
  this->timeScale = timeScale;
}

void CMediaHeaderBox::SetDuration(uint64_t duration)
{
  this->duration = duration;
}

bool CMediaHeaderBox::SetLanguage(const wchar_t *language)
{
  SET_STRING_RETURN(this->language, language);
}

/* other methods */

bool CMediaHeaderBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CMediaHeaderBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sCreation time: %I64u\n" \
      L"%sModification time: %I64u\n" \
      L"%sTime scale: %u\n" \
      L"%sDuration: %I64u\n" \
      L"%sLanguage: %s"
      ,
      
      previousResult,
      indent, this->GetCreationTime(),
      indent, this->GetModificationTime(),
      indent, this->GetTimeScale(),
      indent, this->GetDuration(),
      indent, this->GetLanguage()
      );

  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CMediaHeaderBox::GetBoxSize(void)
{
  uint64_t result = 0;

  switch(this->GetVersion())
  {
  case 0:
    result = 20;
    break;
  case 1:
    result = 32;
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

bool CMediaHeaderBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  FREE_MEM(this->language);

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, MEDIA_HEADER_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is media data box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        switch (this->GetVersion())
        {
        case 0:
          RBE32INC(buffer, position, this->creationTime);
          RBE32INC(buffer, position, this->modificationTime);
          RBE32INC(buffer, position, this->timeScale);
          RBE32INC(buffer, position, this->duration);
          break;
        case 1:
          RBE64INC(buffer, position, this->creationTime);
          RBE64INC(buffer, position, this->modificationTime);
          RBE32INC(buffer, position, this->timeScale);
          RBE64INC(buffer, position, this->duration);
          break;
        default:
          continueParsing = E_FAIL;
          break;
        }
      }

      if (SUCCEEDED(continueParsing))
      {
        // each character is packed as the difference between its ASCII value and 0x60
        // since the code is confined to being three lower-case letters, these values are strictly positive

        RBE16INC_DEFINE(buffer, position, languageCode, uint16_t);

        // bit(1) pad = 0;
        // unsigned int(5)[3] language; // ISO-639-2/T language code

        ALLOC_MEM_DEFINE_SET(languageCodeAscii, char, 4, 0);
        CHECK_POINTER_HRESULT(continueParsing, languageCodeAscii, continueParsing, E_OUTOFMEMORY);

        if (SUCCEEDED(continueParsing))
        {
          languageCodeAscii[0] = ((languageCode & 0x7C00) >> 10) + 0x60;
          languageCodeAscii[1] = ((languageCode & 0x03E0) >> 5) + 0x60;
          languageCodeAscii[2] = (languageCode & 0x001F) + 0x60;

          this->language = ConvertToUnicodeA(languageCodeAscii);
          CHECK_POINTER_HRESULT(continueParsing, this->language, continueParsing, E_OUTOFMEMORY);
        }

        FREE_MEM(languageCodeAscii);

        // skip 2 bytes pre-defined
        position += 2;
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

uint32_t CMediaHeaderBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    switch (this->GetVersion())
    {
    case 0:
      WBE32INC(buffer, result, this->GetCreationTime());
      WBE32INC(buffer, result, this->GetModificationTime());
      WBE32INC(buffer, result, this->GetTimeScale());
      WBE32INC(buffer, result, this->GetDuration());
      break;
    case 1:
      WBE64INC(buffer, result, this->GetCreationTime());
      WBE64INC(buffer, result, this->GetModificationTime());
      WBE32INC(buffer, result, this->GetTimeScale());
      WBE64INC(buffer, result, this->GetDuration());
      break;
    default:
      result = 0;
      break;
    }

    if (result != 0)
    {
      // each character is packed as the difference between its ASCII value and 0x60
      // since the code is confined to being three lower-case letters, these values are strictly positive

      uint16_t languageCode = 0;
      char *languageCodeAscii = ConvertToMultiByteW(this->GetLanguage());
      result = (languageCodeAscii != NULL) ? result : 0;

      if (result != 0)
      {
        result = (strlen(languageCodeAscii) == 3) ? result : 0;

        if (result != 0)
        {
          // bit(1) pad = 0;
          // unsigned int(5)[3] language; // ISO-639-2/T language code

          languageCode |= (((languageCodeAscii[0] - 0x60) << 10) & 0x7C00);
          languageCode |= (((languageCodeAscii[1] - 0x60) << 5) & 0x03E0);
          languageCode |= ((languageCodeAscii[2] - 0x60) & 0x001F);

          WBE16INC(buffer, result, languageCode);

          // skip 2 bytes pre-defined
          result += 2;
        }
      }
      FREE_MEM(languageCodeAscii);
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}
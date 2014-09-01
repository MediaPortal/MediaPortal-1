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

#include "Box.h"
#include "BoxCollection.h"
#include "BoxFactory.h"

CBox::CBox(HRESULT *result)
  : CFlags()
{
  this->length = 0;
  this->type = NULL;
  this->boxes = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->boxes = new CBoxCollection(result);
    CHECK_POINTER_HRESULT(*result, this->boxes, *result, E_OUTOFMEMORY);
  }
}

CBox::~CBox(void)
{
  FREE_MEM(this->type);
  FREE_MEM_CLASS(this->boxes);
}

/* get methods */

uint64_t CBox::GetSize(void)
{
  return (this->length == 0) ? this->GetBoxSize() : this->length;
}

const wchar_t *CBox::GetType(void)
{
  return this->type;
}

bool CBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

CBoxCollection *CBox::GetBoxes(void)
{
  return this->boxes;
}

/* set methods */

/* other methods */

bool CBox::IsBox(void)
{
  return ((this->length >= BOX_HEADER_LENGTH) && (this->type != NULL));
}

bool CBox::IsParsed(void)
{
  return this->IsSetFlags(BOX_FLAG_PARSED);
}

bool CBox::IsBigSize(void)
{
  return (this->GetSize() > ((uint64_t)UINT_MAX));
}

bool CBox::IsSizeUnspecifed(void)
{
  return this->IsSetFlags(BOX_FLAG_UNSPECIFIED_SIZE);
}

bool CBox::HasExtendedHeader(void)
{
  return this->IsSetFlags(BOX_FLAG_HAS_EXTENDED_HEADER);
}

bool CBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

uint64_t CBox::GetBoxSize(void)
{
  uint64_t result = BOX_HEADER_LENGTH;

  for (unsigned int i = 0; i < this->GetBoxes()->Count(); i++)
  {
    CBox *box = this->GetBoxes()->GetItem(i);
    result += box->GetBoxSize();
  }

  if (result > (uint64_t)UINT_MAX)
  {
    // size of box doesn't fit into box header
    result = result - BOX_HEADER_LENGTH + BOX_HEADER_LENGTH_SIZE64;
  }

  return result;
}

HRESULT CBox::GetString(const uint8_t *buffer, uint32_t length, uint32_t startPosition, wchar_t **output, uint32_t *positionAfterString)
{
  return this->GetString(buffer, length, startPosition, output, positionAfterString, UINT32_MAX);
}

HRESULT CBox::GetString(const uint8_t *buffer, uint32_t length, uint32_t startPosition, wchar_t **output, uint32_t *positionAfterString, uint32_t maxLength)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, buffer);
  CHECK_POINTER_DEFAULT_HRESULT(result, output);
  CHECK_POINTER_DEFAULT_HRESULT(result, positionAfterString);

  if (SUCCEEDED(result))
  {
    bool foundEnd = false;
    bool maxLengthReached = false;
    uint32_t tempPosition = startPosition;

    while (tempPosition < length)
    {
      if ((tempPosition - startPosition) < maxLength)
      {
        if (RBE8(buffer, tempPosition) == 0)
        {
          // null terminating character
          foundEnd = true;
          break;
        }
        else
        {
          tempPosition++;
        }
      }
      else
      {
        maxLengthReached = true;
        foundEnd = true;
        break;
      }
    }

    result = (foundEnd) ? S_OK : HRESULT_FROM_WIN32(ERROR_INVALID_DATA);

    if (SUCCEEDED(result))
    {
      // if foundEnd is true then in tempPosition is positon of null terminating character
      uint32_t copyLength = tempPosition - startPosition;
      uint8_t *utf8string = ALLOC_MEM_SET(utf8string, uint8_t, copyLength + 1, 0);
      CHECK_POINTER_HRESULT(result, utf8string, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        // copy value from buffer and convert it into wchar_t (Unicode)
        memcpy(utf8string, buffer + startPosition, copyLength);

        *output = ConvertUtf8ToUnicode((char *)utf8string);
        *positionAfterString = tempPosition + ((maxLengthReached) ? 0 : 1);
        CHECK_POINTER_HRESULT(result, *output, result, E_OUTOFMEMORY);
      }
    }
  }

  return result;
}

uint32_t CBox::SetString(uint8_t *buffer, uint32_t length, const wchar_t *input)
{
  uint32_t result = 0;

  if (buffer != NULL)
  {
    if (input != NULL)
    {
      char *converted = ConvertUnicodeToUtf8(input);
      if (converted != NULL)
      {
        unsigned int length = strlen(converted);
        memcpy(buffer, converted, length);
        result += length;

        // write NULL terminating character
        WBE8INC(buffer, result, 0);
      }
      FREE_MEM(converted);
    }
  }

  return result;
}

uint32_t CBox::GetStringSize(const wchar_t *input)
{
  uint32_t result = 0;

  if (input != NULL)
  {
    char *converted = ConvertUnicodeToUtf8(input);
    if (converted != NULL)
    {
      result = strlen(converted) + 1;
    }
    FREE_MEM(converted);
  }

  return result;
}

wchar_t *CBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;

  if (this->IsBox())
  {
    // prepare boxes collection
    wchar_t *boxes = NULL;
    wchar_t *tempIndent = FormatString(L"%s\t", indent);
    for (unsigned int i = 0; i < this->GetBoxes()->Count(); i++)
    {
      CBox *box = this->GetBoxes()->GetItem(i);
      wchar_t *tempBoxes = FormatString(
        L"%s%s%s--- box %d start ---\n%s\n%s--- box %d end ---",
        (i == 0) ? L"" : boxes,
        (i == 0) ? L"" : L"\n",
        tempIndent, i + 1,
        box->GetParsedHumanReadable(tempIndent),
        tempIndent, i + 1);
      FREE_MEM(boxes);

      boxes = tempBoxes;
    }

    result = FormatString(
      L"%sFlags: 0x%16u\n" \
      L"%sType: '%s'\n" \
      L"%sSize: %llu\n" \
      L"%sExtended header: %s\n" \
      L"%sUnspecified size: %s\n" \
      L"%sBoxes:%s" \
      L"%s"
      , 
      
      indent, this->GetFlags(),
      indent, this->type,
      indent, this->GetSize(),
      indent, this->HasExtendedHeader() ? L"true" : L"false",
      indent, this->IsSizeUnspecifed() ? L"true" : L"false",
      indent, (this->GetBoxes()->Count() == 0) ? L"" : L"\n",
      (this->GetBoxes()->Count() == 0) ? L"" : boxes
      
      );

    FREE_MEM(boxes);
    FREE_MEM(tempIndent);
  }

  return result;
}

bool CBox::IsType(const wchar_t *type)
{
  return ((this->GetType() != NULL) && (type != NULL) && (wcsncmp(this->GetType(), type, BOX_TYPE_LENGTH) == 0));
}

bool CBox::ProcessAdditionalBoxes(const uint8_t *buffer, uint32_t length, uint32_t position)
{
  if (this->boxes != NULL)
  {
    this->boxes->Clear();
  }

  HRESULT continueParsing = ((this->boxes != NULL) && (this->GetSize() <= (uint64_t)length)) ? S_OK : E_INVALIDARG;

  if (SUCCEEDED(continueParsing))
  {
    uint32_t processed = 0;
    uint32_t sizeToProcess = (uint32_t)this->GetSize() -  position;
    CBoxFactory *factory = this->GetBoxFactory();
    CHECK_POINTER_HRESULT(continueParsing, factory, continueParsing, E_OUTOFMEMORY);

    while (SUCCEEDED(continueParsing) && (processed < sizeToProcess))
    {
      CBox *box = factory->CreateBox(buffer + position + processed, (uint32_t)(sizeToProcess - processed));
      CHECK_POINTER_HRESULT(continueParsing, box, continueParsing, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(continueParsing, this->boxes->Add(box), continueParsing, E_OUTOFMEMORY);
      processed += (uint32_t)box->GetSize();

      CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(box));
    }

    FREE_MEM_CLASS(factory);
  }

  if (FAILED(continueParsing) && (this->boxes != NULL))
  {
    this->boxes->Clear();
  }

  return SUCCEEDED(continueParsing);
}

bool CBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  this->length = 0;
  this->flags &= ~(BOX_FLAG_HAS_EXTENDED_HEADER | BOX_FLAG_PARSED | BOX_FLAG_UNSPECIFIED_SIZE);
  FREE_MEM(this->type);

  if (this->boxes != NULL)
  {
    this->boxes->Clear();
  }

  if ((buffer != NULL) && (length >= BOX_HEADER_LENGTH) && (this->boxes != NULL))
  {
    uint64_t size = RBE32(buffer, 0);

    if (size == 1)
    {
      // the actual size is in the field largesize (after BOX_HEADER_LENGTH int(64))
      if (length >= BOX_HEADER_LENGTH_SIZE64)
      {
        // enough data for reading uint64_t size
        size = RBE64(buffer, BOX_HEADER_LENGTH);
        this->flags |= BOX_FLAG_HAS_EXTENDED_HEADER;
      }
    }

    // set length of box
    // if size == 0 then box is the last one in buffer and its content extends to the end of the file (buffer)
    this->length = (size == 0) ? length : size;
    this->flags |= (size == 0) ? BOX_FLAG_UNSPECIFIED_SIZE : BOX_FLAG_NONE;

    // read box type
    uint8_t *type = ALLOC_MEM_SET(type, uint8_t, 5, 0);
    if (type != NULL)
    {
      // copy 4 chars after size field
      memcpy(type, buffer + 4, BOX_TYPE_LENGTH);
    }

    this->type = ConvertToUnicodeA((char *)type);
    this->flags |= (this->type != NULL) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
  }

  return this->IsSetFlags(BOX_FLAG_PARSED);
}

uint32_t CBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t position = 0;

  if (((buffer != NULL) && (!this->IsBigSize())))
  {
    // check box size against buffer length
    unsigned int size = (uint32_t)this->GetSize();

    if (size <= length)
    {
      WBE32INC(buffer, position, (uint32_t)this->GetSize());

      char *type = ConvertToMultiByteW(this->GetType());
      position = (type != NULL) ? position : 0;

      if (position != 0)
      {
        memcpy(buffer + position, type, 4);
        position += 4;
      }

      FREE_MEM(type);

      if ((position != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
      {
        uint32_t boxSizes = this->GetAdditionalBoxes(buffer + position, length - position);
        position = (boxSizes != 0) ? (position + boxSizes) : 0;
      }
    }
  }

  return position;
}

uint32_t CBox::GetAdditionalBoxes(uint8_t *buffer, uint32_t length)
{
  uint32_t processed = 0;

  for (unsigned int i = 0; i < this->GetBoxes()->Count(); i++)
  {
    CBox *box = this->GetBoxes()->GetItem(i);

    processed = (box->GetBox(buffer + processed, length - processed)) ? (processed + (uint32_t)box->GetBoxSize()) : 0;

    if (processed == 0)
    {
      // error occured
      break;
    }
  }

  return processed;
}

void CBox::ResetSize(void)
{
  this->length = 0;
  this->flags &= ~(BOX_FLAG_HAS_EXTENDED_HEADER | BOX_FLAG_UNSPECIFIED_SIZE);
}

CBoxFactory *CBox::GetBoxFactory(void)
{
  HRESULT result = S_OK;
  CBoxFactory *factory = new CBoxFactory(&result);
  CHECK_POINTER_HRESULT(result, factory, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(factory));
  return factory;
}
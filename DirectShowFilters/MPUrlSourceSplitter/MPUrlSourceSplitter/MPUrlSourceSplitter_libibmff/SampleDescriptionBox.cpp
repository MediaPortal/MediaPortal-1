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

#include "SampleDescriptionBox.h"
#include "SampleEntryBoxFactory.h"
#include "BoxCollection.h"

CSampleDescriptionBox::CSampleDescriptionBox(HRESULT *result, uint32_t handlerType)
  : CFullBox(result)
{
  this->type = NULL;
  this->sampleEntries = NULL;
  this->handlerType = handlerType;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(SAMPLE_DESCRIPTION_BOX_TYPE);
    this->sampleEntries = new CSampleEntryBoxCollection(result);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->sampleEntries, *result, E_OUTOFMEMORY);
  }
}

CSampleDescriptionBox::~CSampleDescriptionBox(void)
{
  FREE_MEM_CLASS(this->sampleEntries);
}

/* get methods */

bool CSampleDescriptionBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

CSampleEntryBoxCollection *CSampleDescriptionBox::GetSampleEntries(void)
{
  return this->sampleEntries;
}

uint32_t CSampleDescriptionBox::GetHandlerType(void)
{
  return this->handlerType;
}

/* set methods */

/* other methods */

wchar_t *CSampleDescriptionBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare sample entries collection
    wchar_t *boxes = NULL;
    wchar_t *tempIndent = FormatString(L"%s\t", indent);
    for (unsigned int i = 0; i < this->GetSampleEntries()->Count(); i++)
    {
      CBox *box = this->GetSampleEntries()->GetItem(i);
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
    
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sHandler type: 0x%08X\n" \
      L"%sSample entries:%s" \
      L"%s"
      ,
      
      previousResult,
      indent, this->GetHandlerType(),
      indent, (this->GetSampleEntries()->Count() == 0) ? L"" : L"\n",
      (this->GetSampleEntries()->Count() == 0) ? L"" : boxes
      );

    FREE_MEM(boxes);
    FREE_MEM(tempIndent);
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CSampleDescriptionBox::GetBoxSize(void)
{
  uint64_t result = 4;

  for (unsigned int i = 0; i < this->GetSampleEntries()->Count(); i++)
  {
    uint64_t boxSize = this->GetSampleEntries()->GetItem(i)->GetSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CSampleDescriptionBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  this->sampleEntries->Clear();

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, SAMPLE_DESCRIPTION_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is sample description box, parse all values
      uint32_t position = this->HasExtendedHeader() ? FULL_BOX_HEADER_LENGTH_SIZE64 : FULL_BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        RBE32INC_DEFINE(buffer, position, sampleEntriesCount, uint32_t);

        CSampleEntryBoxFactory *factory = new CSampleEntryBoxFactory(&continueParsing);
        CHECK_POINTER_HRESULT(continueParsing, factory, continueParsing, E_OUTOFMEMORY);

        for (uint32_t i = 0; (SUCCEEDED(continueParsing) && (i < sampleEntriesCount)); i++)
        {
          CBox *box = factory->CreateBox(buffer + position, length - position, this->GetHandlerType());
          CHECK_POINTER_HRESULT(continueParsing, box, continueParsing, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(continueParsing, this->sampleEntries->Add((CSampleEntryBox *)box), continueParsing, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), position += (uint32_t)box->GetSize());

          CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(box));
        }

        FREE_MEM_CLASS(factory);
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

uint32_t CSampleDescriptionBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE32INC(buffer, result, this->GetSampleEntries()->Count());

    for (unsigned int i = 0; ((result != 0) && (i < this->GetSampleEntries()->Count())); i++)
    {
      CSampleEntryBox *box = this->GetSampleEntries()->GetItem(i);
      result = box->GetBox(buffer + result, length - result) ? (result + (uint32_t)box->GetSize()) : 0;
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}
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

CSampleDescriptionBox::CSampleDescriptionBox(uint32_t handlerType)
  : CFullBox()
{
  this->type = Duplicate(SAMPLE_DESCRIPTION_BOX_TYPE);
  this->sampleEntries = new CSampleEntryBoxCollection();
  this->handlerType = handlerType;
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

bool CSampleDescriptionBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

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
  if (this->sampleEntries != NULL)
  {
    this->sampleEntries->Clear();
  }

  bool result = (this->sampleEntries != NULL);
  // in bad case we don't have objects, but still it can be valid box
  result &= __super::ParseInternal(buffer, length, false);

  if (result)
  {
    if (wcscmp(this->type, SAMPLE_DESCRIPTION_BOX_TYPE) != 0)
    {
      // incorect box type
      this->parsed = false;
    }
    else
    {
      // box is file type box, parse all values
      uint32_t position = this->HasExtendedHeader() ? FULL_BOX_HEADER_LENGTH_SIZE64 : FULL_BOX_HEADER_LENGTH;
      bool continueParsing = (this->GetSize() <= (uint64_t)length);

      if (continueParsing)
      {
        RBE32INC_DEFINE(buffer, position, sampleEntriesCount, uint32_t);

        CSampleEntryBoxFactory *factory = new CSampleEntryBoxFactory();
        continueParsing &= (factory != NULL);

        for (uint32_t i = 0; (continueParsing && (i < sampleEntriesCount)); i++)
        {
          CBox *box = factory->CreateBox(buffer + position, length - position, this->GetHandlerType());
          continueParsing &= (box != NULL);

          if (continueParsing)
          {
            continueParsing &= this->sampleEntries->Add((CSampleEntryBox *)box);
            position += (uint32_t)box->GetSize();
          }

          if (!continueParsing)
          {
            FREE_MEM_CLASS(box);
          }
        }
      }

      if (continueParsing && processAdditionalBoxes)
      {
        this->ProcessAdditionalBoxes(buffer, length, position);
      }

      this->parsed = continueParsing;
    }
  }

  result = this->parsed;

  return result;
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
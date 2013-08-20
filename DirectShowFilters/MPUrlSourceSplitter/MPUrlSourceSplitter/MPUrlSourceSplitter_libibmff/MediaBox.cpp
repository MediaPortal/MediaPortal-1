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

#include "MediaBox.h"
#include "MediaInformationBoxFactory.h"
#include "HandlerBox.h"
#include "BoxCollection.h"

CMediaBox::CMediaBox(void)
  : CBox()
{
  this->type = Duplicate(MEDIA_BOX_TYPE);
}

CMediaBox::~CMediaBox(void)
{
}

/* get methods */

bool CMediaBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

/* set methods */

/* other methods */

bool CMediaBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CMediaBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s"
      ,
      
      previousResult
      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CMediaBox::GetBoxSize(void)
{
  return __super::GetBoxSize();
}

bool CMediaBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  bool result = __super::ParseInternal(buffer, length, false);

  if (result)
  {
    if (wcscmp(this->type, MEDIA_BOX_TYPE) != 0)
    {
      // incorect box type
      this->parsed = false;
    }
    else
    {
      // box is file type box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      bool continueParsing = (this->GetSize() <= (uint64_t)length);

      if (continueParsing)
      {
        uint64_t processedSize = 0;
        uint64_t sizeToProcess = this->GetSize() - (this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH);
        CMediaInformationBoxFactory *factory = new CMediaInformationBoxFactory();
        continueParsing &= (factory != NULL);

        uint32_t handlerType = 0;

        while (continueParsing && (processedSize < sizeToProcess))
        {
          CBox *box = factory->CreateBox(buffer + position + processedSize, (uint32_t)(sizeToProcess - processedSize), handlerType);
          continueParsing &= (box != NULL);

          if (continueParsing)
          {
            continueParsing &= this->GetBoxes()->Add(box);
            processedSize += (uint32_t)box->GetSize();

            if (box->IsType(HANDLER_BOX_TYPE))
            {
              handlerType = ((CHandlerBox *)box)->GetHandlerType();
            }
          }

          if (!continueParsing)
          {
            FREE_MEM_CLASS(box);
          }
        }

        FREE_MEM_CLASS(factory);
      }

      this->parsed = continueParsing;
    }
  }

  result = this->parsed;

  return result;
}

uint32_t CMediaBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}
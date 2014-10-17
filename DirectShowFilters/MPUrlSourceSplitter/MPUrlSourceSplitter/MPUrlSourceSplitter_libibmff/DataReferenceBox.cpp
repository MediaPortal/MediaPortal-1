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

#include "DataReferenceBox.h"
#include "BoxFactory.h"
#include "BoxCollection.h"

CDataReferenceBox::CDataReferenceBox(HRESULT *result)
  : CFullBox(result)
{
  this->type = NULL;
  this->dataEntryBoxCollection = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(DATA_REFERENCE_BOX_TYPE);
    this->dataEntryBoxCollection = new CDataEntryBoxCollection(result);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->dataEntryBoxCollection, *result, E_OUTOFMEMORY);
  }
}

CDataReferenceBox::~CDataReferenceBox(void)
{
  FREE_MEM_CLASS(this->dataEntryBoxCollection);
}

/* get methods */

bool CDataReferenceBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

CDataEntryBoxCollection *CDataReferenceBox::GetDataEntryBoxCollection(void)
{
  return this->dataEntryBoxCollection;
}

/* set methods */

/* other methods */

wchar_t *CDataReferenceBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare server entry table
    wchar_t *dataEntryBoxes = NULL;
    wchar_t *tempIndent = FormatString(L"%s\t", indent);
    for (unsigned int i = 0; i < this->GetDataEntryBoxCollection()->Count(); i++)
    {
      CDataEntryBox *dataEntryBox = this->GetDataEntryBoxCollection()->GetItem(i);
      wchar_t *tempDataEntryBoxes = FormatString(
        L"%s%s%s--- data entry box %d start ---\n%s\n%s--- data entry box %d end ---",
        (i == 0) ? L"" : dataEntryBoxes,
        (i == 0) ? L"" : L"\n",
        tempIndent, i + 1,
        dataEntryBox->GetParsedHumanReadable(tempIndent),
        tempIndent, i + 1);
      FREE_MEM(dataEntryBoxes);
      dataEntryBoxes = tempDataEntryBoxes;
    }

    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sData reference box count: %d" \
      L"%s%s"
      ,
      
      previousResult,
      indent, this->GetDataEntryBoxCollection()->Count(),
      (dataEntryBoxes == NULL) ? L"" : L"\n", (dataEntryBoxes == NULL) ? L"" : dataEntryBoxes

      );

    FREE_MEM(dataEntryBoxes);
    FREE_MEM(tempIndent);
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CDataReferenceBox::GetBoxSize(void)
{
  uint64_t result = 4;

  for (unsigned int i = 0; i < this->GetDataEntryBoxCollection()->Count(); i++)
  {
    uint64_t boxSize = this->GetDataEntryBoxCollection()->GetItem(i)->GetSize();
    result = (boxSize != 0) ? (result + boxSize) : 0;

    if (result == 0)
    {
      // error occured
      break;
    }
  }

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CDataReferenceBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  this->dataEntryBoxCollection->Clear();

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, DATA_REFERENCE_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is data reference box, parse all values
      uint32_t position = this->HasExtendedHeader() ? FULL_BOX_HEADER_LENGTH_SIZE64 : FULL_BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        CBoxFactory *factory = new CBoxFactory(&continueParsing);
        CHECK_POINTER_HRESULT(continueParsing, factory, continueParsing, E_OUTOFMEMORY);

        if (SUCCEEDED(continueParsing))
        {
          RBE32INC_DEFINE(buffer, position, dataEntryBoxCount, uint32_t);

          CHECK_CONDITION_HRESULT(continueParsing, this->dataEntryBoxCollection->EnsureEnoughSpace(dataEntryBoxCount), continueParsing, E_OUTOFMEMORY);

          for (uint32_t i = 0; (SUCCEEDED(continueParsing) && (i < dataEntryBoxCount)); i++)
          {
            CBox *box = factory->CreateBox(buffer + position, length - position);
            CHECK_POINTER_HRESULT(continueParsing, box, continueParsing, E_OUTOFMEMORY);

            if (SUCCEEDED(continueParsing))
            {
              CDataEntryBox *dataEntryBox = dynamic_cast<CDataEntryBox *>(box);
              CHECK_POINTER_HRESULT(continueParsing, dataEntryBox, continueParsing, E_FAIL);

              CHECK_CONDITION_HRESULT(continueParsing, this->dataEntryBoxCollection->Add(dataEntryBox), continueParsing, E_OUTOFMEMORY);
              CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), position += (uint32_t)box->GetSize());
            }

            CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(box));
          }
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

uint32_t CDataReferenceBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE32INC(buffer, result, this->GetDataEntryBoxCollection()->Count());

    for (unsigned int i = 0; (i < this->GetDataEntryBoxCollection()->Count()); i++)
    {
      CDataEntryBox *box = this->GetDataEntryBoxCollection()->GetItem(i);

      result = box->GetBox(buffer + result, length - result) ? (result + (uint32_t)box->GetSize()) : 0;

      if (result == 0)
      {
        // error occured
        break;
      }
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}
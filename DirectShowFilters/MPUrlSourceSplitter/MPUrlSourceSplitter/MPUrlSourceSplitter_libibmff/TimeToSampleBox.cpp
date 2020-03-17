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

#include "TimeToSampleBox.h"
#include "BoxCollection.h"

CTimeToSampleBox::CTimeToSampleBox(HRESULT *result)
  : CFullBox(result)
{
  this->type = NULL;
  this->timesToSamples = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(TIME_TO_SAMPLE_BOX_TYPE);
    this->timesToSamples = new CTimeToSampleCollection(result);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->timesToSamples, *result, E_OUTOFMEMORY);
  }
}

CTimeToSampleBox::~CTimeToSampleBox(void)
{
  FREE_MEM_CLASS(this->timesToSamples);
}

/* get methods */

CTimeToSampleCollection *CTimeToSampleBox::GetTimesToSamples(void)
{
  return this->timesToSamples;
}

/* set methods */

/* other methods */

wchar_t *CTimeToSampleBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare sample entries collection
    wchar_t *timesToSamples = NULL;
    wchar_t *tempIndent = FormatString(L"%s\t", indent);
    for (unsigned int i = 0; i < this->GetTimesToSamples()->Count(); i++)
    {
      CTimeToSample *timeToSample = this->GetTimesToSamples()->GetItem(i);
      wchar_t *tempTimesToSamples = FormatString(
        L"%s%s%sSample count: %5u Sample delta: %5u",
        (i == 0) ? L"" : timesToSamples,
        (i == 0) ? L"" : L"\n",
        tempIndent,
        timeToSample->GetSampleCount(),
        timeToSample->GetSampleDelta());
      FREE_MEM(timesToSamples);

      timesToSamples = tempTimesToSamples;
    }

    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sTimes to samples:%s" \
      L"%s"
      ,
      
      previousResult,
      indent, (this->GetTimesToSamples()->Count() == 0) ? L"" : L"\n",
      (this->GetTimesToSamples()->Count() == 0) ? L"" : timesToSamples

      );

    FREE_MEM(timesToSamples);
    FREE_MEM(tempIndent);
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CTimeToSampleBox::GetBoxSize(void)
{
  uint64_t result = 4 + this->GetTimesToSamples()->Count() * 8;

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CTimeToSampleBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  this->timesToSamples->Clear();

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, TIME_TO_SAMPLE_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is time to sample box, parse all values
      uint32_t position = this->HasExtendedHeader() ? FULL_BOX_HEADER_LENGTH_SIZE64 : FULL_BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        RBE32INC_DEFINE(buffer, position, entryCount, uint32_t);

        CHECK_CONDITION_HRESULT(continueParsing, this->timesToSamples->EnsureEnoughSpace(entryCount), continueParsing, E_OUTOFMEMORY);

        for (uint32_t i = 0; (SUCCEEDED(continueParsing) && (i < entryCount)); i++)
        {
          CTimeToSample *timeToSample = new CTimeToSample(&continueParsing);
          CHECK_POINTER_HRESULT(continueParsing, timeToSample, continueParsing, E_OUTOFMEMORY);

          if (SUCCEEDED(continueParsing))
          {
            timeToSample->SetSampleCount(RBE32(buffer, position));
            position += 4;

            timeToSample->SetSampleDelta(RBE32(buffer, position));
            position += 4;
          }

          CHECK_CONDITION_HRESULT(continueParsing, this->timesToSamples->Add(timeToSample), continueParsing, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(timeToSample));
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

uint32_t CTimeToSampleBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE32INC(buffer, result, this->GetTimesToSamples()->Count());

    for (uint32_t i = 0; (i < this->GetTimesToSamples()->Count()); i++)
    {
      CTimeToSample *timeToSample = this->GetTimesToSamples()->GetItem(i);
      WBE32INC(buffer, result, timeToSample->GetSampleCount());
      WBE32INC(buffer, result, timeToSample->GetSampleDelta());
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}
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

#include "TrackRunBox.h"
#include "BoxCollection.h"

CTrackRunBox::CTrackRunBox(void)
  : CFullBox()
{
  this->type = Duplicate(TRACK_RUN_BOX_TYPE);
  this->samples = new CSampleCollection();
  this->dataOffset = 0;
  this->firstSampleFlags = 0;
}

CTrackRunBox::~CTrackRunBox(void)
{
  FREE_MEM_CLASS(this->samples);
}

/* get methods */

bool CTrackRunBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

int32_t CTrackRunBox::GetDataOffset(void)
{
  return this->dataOffset;
}

uint32_t CTrackRunBox::GetFirstSampleFlags(void)
{
  return this->firstSampleFlags;
}

CSampleCollection *CTrackRunBox::GetSamples(void)
{
  return this->samples;
}

/* set methods */

void CTrackRunBox::SetDataOffset(int32_t dataOffset)
{
  this->dataOffset = dataOffset;
}

void CTrackRunBox::SetFirstSampleFlags(uint32_t firstSampleFlags)
{
  this->firstSampleFlags = firstSampleFlags;
}

/* other methods */

bool CTrackRunBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CTrackRunBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare samples
    wchar_t *samples = NULL;
    wchar_t *tempIndent = FormatString(L"%s\t", indent);
    for (unsigned int i = 0; i < this->GetSamples()->Count(); i++)
    {
      CSample *sample = this->GetSamples()->GetItem(i);

      wchar_t *tempSample = FormatString(
        L"%s%s%sSample duration: present: %s value: %u, sample size: present: %s value: %u, sample flags: present: %s value: 0x%08X, sample composition time offset: present: %s value: %u",
        (i == 0) ? L"" : samples,
        (i == 0) ? L"" : L"\n",
        tempIndent,
        this->IsSampleDurationPresent() ? L"true" : L"false", sample->GetSampleDuration(),
        this->IsSampleSizePresent() ? L"true" : L"false", sample->GetSampleSize(),
        this->IsSampleFlagsPresent() ? L"true" : L"false", sample->GetSampleFlags(),
        this->IsSampleCompositionTimeOffsetsPresent() ? L"true" : L"false", sample->GetSampleCompositionTimeOffset()
        );
      FREE_MEM(samples);

      samples = tempSample;
    }

    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sData offset: present: %s value: %d\n" \
      L"%sFirst sample flags: present: %s value: 0x%08X\n" \
      L"%sSamples:%s" \
      L"%s"
      ,
      
      previousResult,
      indent, this->IsDataOffsetPresent() ? L"true" : L"false", this->GetDataOffset(),
      indent, this->IsFirstDataSampleFlagsPresent() ? L"true" : L"false", this->GetFirstSampleFlags(),
      indent, (this->GetSamples()->Count() == 0) ? L"" : L"\n",
      (this->GetSamples()->Count() == 0) ? L"" : samples
      );

    FREE_MEM(samples);
    FREE_MEM(tempIndent);
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CTrackRunBox::GetBoxSize(void)
{
  uint64_t result = 0;

  if (this->IsSampleDurationPresent())
  {
    result += 4;
  }
  if (this->IsSampleSizePresent())
  {
    result += 4;
  }
  if (this->IsSampleFlagsPresent())
  {
    result += 4;
  }
  if (this->IsSampleCompositionTimeOffsetsPresent())
  {
    result += 4;
  }

  result = result * this->GetSamples()->Count();
  result += 4;
  if (this->IsDataOffsetPresent())
  {
    result += 4;
  }
  if (this->IsFirstDataSampleFlagsPresent())
  {
    result += 4;
  }

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CTrackRunBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  if (this->samples != NULL)
  {
    this->samples->Clear();
  }

  bool result = (this->samples != NULL);
  result &= __super::ParseInternal(buffer, length, false);

  if (result)
  {
    if (wcscmp(this->type, TRACK_RUN_BOX_TYPE) != 0)
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
        RBE32INC_DEFINE(buffer, position, sampleCount, uint32_t);
        if (this->IsDataOffsetPresent())
        {
          RBE32INC(buffer, position, this->dataOffset);
        }
        if (this->IsFirstDataSampleFlagsPresent())
        {
          RBE32INC(buffer, position, this->firstSampleFlags);
        }

        for (uint32_t i = 0; (continueParsing && (i < sampleCount)); i++)
        {
          CSample *sample = new CSample();
          continueParsing &= (sample != NULL);

          if (continueParsing)
          {
            // fill sample data

            if (this->IsSampleDurationPresent())
            {
              sample->SetSampleDuration(RBE32(buffer, position));
              position += 4;
            }

            if (this->IsSampleSizePresent())
            {
              sample->SetSampleSize(RBE32(buffer, position));
              position += 4;
            }

            if (this->IsSampleFlagsPresent())
            {
              sample->SetSampleFlags(RBE32(buffer, position));
              position += 4;
            }

            if (this->IsSampleCompositionTimeOffsetsPresent())
            {
              sample->SetSampleCompositionTimeOffset(RBE32(buffer, position));
              position += 4;
            }

            continueParsing &= this->samples->Add(sample);
          }

          if (!continueParsing)
          {
            FREE_MEM_CLASS(sample);
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

bool CTrackRunBox::IsDataOffsetPresent(void)
{
  return ((this->GetFlags() & FLAGS_DATA_OFFSET_PRESENT) != 0);
}

bool CTrackRunBox::IsFirstDataSampleFlagsPresent(void)
{
  return ((this->GetFlags() & FLAGS_FIRST_SAMPLE_FLAGS_PRESENT) != 0);
}

bool CTrackRunBox::IsSampleDurationPresent(void)
{
  return ((this->GetFlags() & FLAGS_SAMPLE_DURATION_PRESENT) != 0);
}

bool CTrackRunBox::IsSampleSizePresent(void)
{
  return ((this->GetFlags() & FLAGS_SAMPLE_SIZE_PRESENT) != 0);
}

bool CTrackRunBox::IsSampleFlagsPresent(void)
{
  return ((this->GetFlags() & FLAGS_SAMPLE_FLAGS_PRESENT) != 0);
}

bool CTrackRunBox::IsSampleCompositionTimeOffsetsPresent(void)
{
  return ((this->GetFlags() & FLAGS_SAMPLE_COMPOSITION_TIME_OFFSETS_PRESENT) != 0);
}

uint32_t CTrackRunBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE32INC(buffer, result, this->GetSamples()->Count());

    if (this->IsDataOffsetPresent())
    {
      WBE32INC(buffer, result, this->GetDataOffset());
    }
    if (this->IsFirstDataSampleFlagsPresent())
    {
      WBE32INC(buffer, result, this->GetFirstSampleFlags());
    }

    for (uint32_t i = 0; (i < this->GetSamples()->Count()); i++)
    {
      CSample *sample = this->GetSamples()->GetItem(i);

      if (this->IsSampleDurationPresent())
      {
        WBE32INC(buffer, result, sample->GetSampleDuration());
      }

      if (this->IsSampleSizePresent())
      {
        WBE32INC(buffer, result, sample->GetSampleSize());
      }

      if (this->IsSampleFlagsPresent())
      {
        WBE32INC(buffer, result, sample->GetSampleFlags());
      }

      if (this->IsSampleCompositionTimeOffsetsPresent())
      {
        WBE32INC(buffer, result, sample->GetSampleCompositionTimeOffset());
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
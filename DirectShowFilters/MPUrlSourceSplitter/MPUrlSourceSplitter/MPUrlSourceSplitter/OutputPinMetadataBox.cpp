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

#include "OutputPinMetadataBox.h"
#include "BufferHelper.h"
#include "BoxConstants.h"
#include "BoxCollection.h"

COutputPinMetadataBox::COutputPinMetadataBox(HRESULT *result)
  : CBox(result)
{
  this->mediaSampleFlags = OUTPUT_PIN_METADATA_BOX_FLAG_NONE;
  this->mediaSampleActual = 0;
  this->mediaSampleBufferSize = 0;
  this->mediaSampleStart = 0;
  this->mediaSampleEnd = 0;
  this->mediaSampleMediaStart = 0;
  this->mediaSampleMediaEnd = 0;
  this->mediaSampleStreamId = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(OUTPUT_PIN_METADATA_BOX_TYPE);
    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
  }
}

COutputPinMetadataBox::~COutputPinMetadataBox(void)
{
}

/* get methods */

/* set methods */

bool COutputPinMetadataBox::SetMediaSample(IMediaSample *mediaSample)
{
  bool result = (mediaSample != NULL);

  if (result)
  {
    this->mediaSampleFlags = OUTPUT_PIN_METADATA_BOX_FLAG_NONE;

    this->mediaSampleFlags |= (mediaSample->IsDiscontinuity() == S_OK) ? OUTPUT_PIN_METADATA_BOX_FLAG_DISCONTINUITY : OUTPUT_PIN_METADATA_BOX_FLAG_NONE;
    this->mediaSampleFlags |= (mediaSample->IsPreroll() == S_OK) ? OUTPUT_PIN_METADATA_BOX_FLAG_PREROLL : OUTPUT_PIN_METADATA_BOX_FLAG_NONE;
    this->mediaSampleFlags |= (mediaSample->IsSyncPoint() == S_OK) ? OUTPUT_PIN_METADATA_BOX_FLAG_SYNC_POINT : OUTPUT_PIN_METADATA_BOX_FLAG_NONE;

    this->mediaSampleActual = mediaSample->GetActualDataLength();
    this->mediaSampleBufferSize = mediaSample->GetSize();

    int64_t timeStart = 0;
    int64_t timeEnd = 0;

    HRESULT res = mediaSample->GetTime(&timeStart, &timeEnd);
    switch (res)
    {
    case S_OK:
      {
        this->mediaSampleStart = timeStart;
        this->mediaSampleEnd = timeEnd;
        this->mediaSampleFlags |= OUTPUT_PIN_METADATA_BOX_FLAG_TIME_VALID | OUTPUT_PIN_METADATA_BOX_FLAG_STOP_VALID;
      }
      break;
    case VFW_E_SAMPLE_TIME_NOT_SET:
      break;
    case VFW_S_NO_STOP_TIME:
      {
        this->mediaSampleStart = timeStart;
        this->mediaSampleFlags |= OUTPUT_PIN_METADATA_BOX_FLAG_TIME_VALID;
      }
      break;
    default:
      result = false;
      break;
    }

    if (result)
    {
      res = mediaSample->GetMediaTime(&timeStart, &timeEnd);
      switch (res)
      {
      case S_OK:
        {
          this->mediaSampleMediaStart = timeStart;
          this->mediaSampleMediaEnd = (int32_t)(timeEnd - timeStart);
          this->mediaSampleFlags |= OUTPUT_PIN_METADATA_BOX_FLAG_MEDIA_TIME_VALID;
        }
        break;
      case VFW_E_MEDIA_TIME_NOT_SET:
        break;
      default:
        result = false;
        break;
      }
    }

    //
    //
    //mediaSample->GetMediaType()
    //
    //
    
  }

  return result;
}

/* other methods */

wchar_t *COutputPinMetadataBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s", 
      
      previousResult
      );
  }

  FREE_MEM(previousResult);

  return result;
}

/* protected methods */

uint64_t COutputPinMetadataBox::GetBoxSize(void)
{
  uint64_t result = 44;

  uint64_t boxSize = __super::GetBoxSize();
  result = (boxSize != 0) ? (result + boxSize) : 0; 

  return result;
}

bool COutputPinMetadataBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, OUTPUT_PIN_METADATA_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
    
    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is RTSP main box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      bool continueParsing = (this->GetSize() <= (uint64_t)length);

      if (continueParsing)
      {
        RBE32INC(buffer, position, this->mediaSampleFlags);
        RBE32INC(buffer, position, this->mediaSampleActual);
        RBE32INC(buffer, position, this->mediaSampleBufferSize);

        RBE64INC(buffer, position, this->mediaSampleStart);
        RBE64INC(buffer, position, this->mediaSampleEnd);
        RBE64INC(buffer, position, this->mediaSampleMediaStart);

        RBE32INC(buffer, position, this->mediaSampleMediaEnd);
        RBE32INC(buffer, position, this->mediaSampleStreamId);
      }

      if (continueParsing && processAdditionalBoxes)
      {
        this->ProcessAdditionalBoxes(buffer, length, position);
      }
      
      this->flags &= ~BOX_FLAG_PARSED;
      this->flags |= continueParsing ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
    }
  }

  return this->IsSetFlags(BOX_FLAG_PARSED);
}

uint32_t COutputPinMetadataBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE32INC(buffer, result, this->mediaSampleFlags);
    WBE32INC(buffer, result, this->mediaSampleActual);
    WBE32INC(buffer, result, this->mediaSampleBufferSize);

    WBE64INC(buffer, result, this->mediaSampleStart);
    WBE64INC(buffer, result, this->mediaSampleEnd);
    WBE64INC(buffer, result, this->mediaSampleMediaStart);

    WBE32INC(buffer, result, this->mediaSampleMediaEnd);
    WBE32INC(buffer, result, this->mediaSampleStreamId);

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}
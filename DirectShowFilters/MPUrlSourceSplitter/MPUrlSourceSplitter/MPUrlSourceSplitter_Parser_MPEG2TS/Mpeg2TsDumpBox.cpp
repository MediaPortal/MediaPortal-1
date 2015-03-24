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

#include "Mpeg2TsDumpBox.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackageDataResponse.h"
#include "BoxCollection.h"
#include "BufferHelper.h"

CMpeg2TsDumpBox::CMpeg2TsDumpBox(HRESULT *result)
  : CDumpBox(result)
{
  this->packageState = 0;
  this->packageErrorCode = 0;

  this->requestFlags = 0;
  this->requestId = 0;
  this->requestStart = 0;
  this->requestLength = 0;
  this->requestStreamId = 0;
  this->requestStartTime = 0;

  this->responseFlags = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(MPEG2TS_DUMP_BOX_TYPE);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
  }
}

CMpeg2TsDumpBox::~CMpeg2TsDumpBox(void)
{
}

/* get methods */

/* set methods */

bool CMpeg2TsDumpBox::SetStreamPackage(CStreamPackage *streamPackage)
{
  CStreamPackageDataRequest *request = dynamic_cast<CStreamPackageDataRequest *>(streamPackage->GetRequest());
  CStreamPackageDataResponse *response = dynamic_cast<CStreamPackageDataResponse *>(streamPackage->GetResponse());

  bool result = (request != NULL) && (response != NULL);

  if (result)
  {
    if (response->GetBuffer() != NULL)
    {
      unsigned int bufferSize = response->GetBuffer()->GetBufferOccupiedSpace();
      ALLOC_MEM_DEFINE_SET(buffer, uint8_t, bufferSize, 0);
      result &= (buffer != NULL);

      if (result)
      {
        response->GetBuffer()->CopyFromBuffer(buffer, bufferSize);
        result &= this->SetPayload(buffer, bufferSize);
      }

      FREE_MEM(buffer);
    }
  }

  if (result)
  {
    this->packageState = streamPackage->GetState();
    this->packageErrorCode = streamPackage->GetError();

    this->requestFlags = request->GetFlags();
    this->requestId = request->GetId();
    this->requestStart = request->GetStart();
    this->requestLength = request->GetLength();
    this->requestStreamId = request->GetStreamId();
    this->requestStartTime = request->GetStartTime();

    this->responseFlags = response->GetFlags();
  }

  return result;
}

void CMpeg2TsDumpBox::SetInputData(bool inputData)
{
  this->flags &= ~MPEG2TS_DUMP_BOX_FLAG_INPUT_DATA;
  this->flags |= (inputData) ? MPEG2TS_DUMP_BOX_FLAG_INPUT_DATA : MPEG2TS_DUMP_BOX_FLAG_NONE;
}

void CMpeg2TsDumpBox::SetOutputData(bool outputData)
{
  this->flags &= ~MPEG2TS_DUMP_BOX_FLAG_OUTPUT_DATA;
  this->flags |= (outputData) ? MPEG2TS_DUMP_BOX_FLAG_OUTPUT_DATA : MPEG2TS_DUMP_BOX_FLAG_NONE;
}

/* other methods */

bool CMpeg2TsDumpBox::IsInputData(void)
{
  return this->IsSetFlags(MPEG2TS_DUMP_BOX_FLAG_INPUT_DATA);
}

bool CMpeg2TsDumpBox::IsOutputData(void)
{
  return this->IsSetFlags(MPEG2TS_DUMP_BOX_FLAG_OUTPUT_DATA);
}

/* protected methods */

uint64_t CMpeg2TsDumpBox::GetBoxSize(void)
{
  uint64_t result = 0;

  if (this->IsSetAnyOfFlags(MPEG2TS_DUMP_BOX_FLAG_INPUT_DATA | MPEG2TS_DUMP_BOX_FLAG_OUTPUT_DATA))
  {
    result = 37;
  }
  
  uint64_t boxSize = __super::GetBoxSize();
  result = (boxSize != 0) ? (result + boxSize) : 0; 

  return result;
}

uint32_t CMpeg2TsDumpBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    if (this->IsSetAnyOfFlags(MPEG2TS_DUMP_BOX_FLAG_INPUT_DATA | MPEG2TS_DUMP_BOX_FLAG_OUTPUT_DATA))
    {
      WBE8INC(buffer, result, this->packageState);
      WBE32INC(buffer, result, this->packageErrorCode);

      WBE32INC(buffer, result, this->requestFlags);
      WBE32INC(buffer, result, this->requestId);
      WBE64INC(buffer, result, this->requestStart);
      WBE32INC(buffer, result, this->requestLength);
      WBE32INC(buffer, result, this->requestStreamId);
      WBE32INC(buffer, result, this->requestStartTime);

      WBE32INC(buffer, result, this->responseFlags);
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}

unsigned int CMpeg2TsDumpBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes, bool checkType)
{
  uint32_t position = __super::ParseInternal(buffer, length, false, false);

  if (position != 0)
  {
    HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

    if (checkType)
    {
      this->flags &= BOX_FLAG_PARSED;
      this->flags |= (wcscmp(this->type, MPEG2TS_DUMP_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
    }

    // TO DO: parse box

    if (SUCCEEDED(continueParsing) && processAdditionalBoxes)
    {
      this->ProcessAdditionalBoxes(buffer, length, position);
    }

    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= SUCCEEDED(continueParsing) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
  }

  return this->IsSetFlags(BOX_FLAG_PARSED) ? position : 0;
}
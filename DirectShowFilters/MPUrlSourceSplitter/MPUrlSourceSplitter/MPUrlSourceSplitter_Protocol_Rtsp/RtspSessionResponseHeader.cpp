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

#include "RtspSessionResponseHeader.h"
#include "conversions.h"

CRtspSessionResponseHeader::CRtspSessionResponseHeader(HRESULT *result)
  : CRtspResponseHeader(result)
{
  this->sessionId = NULL;
  this->timeout = RTSP_SESSION_RESPONSE_TIMEOUT_DEFAULT;
}

CRtspSessionResponseHeader::~CRtspSessionResponseHeader(void)
{
  FREE_MEM(this->sessionId);
}

/* get methods */

const wchar_t *CRtspSessionResponseHeader::GetSessionId(void)
{
  return this->sessionId;
}

unsigned int CRtspSessionResponseHeader::GetTimeout(void)
{
  return this->timeout;
}

/* set methods */

/* other methods */

bool CRtspSessionResponseHeader::Parse(const wchar_t *header, unsigned int length)
{
  bool result = __super::Parse(header, length);

  if (result)
  {
    result &= (_wcsicmp(this->name, RTSP_SESSION_RESPONSE_HEADER_TYPE) == 0);

    if (result)
    {
      unsigned int position = 0;
      unsigned int valueLength = this->GetValueLength();
      int index = IndexOf(this->value, valueLength, RTSP_SESSION_RESPONSE_HEADER_SEPARATOR, RTSP_SESSION_RESPONSE_HEADER_SEPARATOR_LENGTH);

      if (index > 0)
      {
        // first is session ID
        // second is timeout (or unknown option)

        this->sessionId = Substring(this->value, 0, index);
        result &= (this->sessionId != NULL);
        position = index + 1;

        while (result && (position < valueLength))
        {
          // try to find separator
          index = IndexOf(this->value + position, valueLength - position, RTSP_SESSION_RESPONSE_HEADER_SEPARATOR, RTSP_SESSION_RESPONSE_HEADER_SEPARATOR_LENGTH);
          unsigned int tempLength = (index == (-1)) ? (valueLength - position) : (index);

          if (wcsncmp(this->value + position, RTSP_SESSION_RESPONSE_HEADER_PARAMETER_TIMEOUT, RTSP_SESSION_RESPONSE_HEADER_PARAMETER_TIMEOUT_LENGTH) == 0)
          {
            int index2 = IndexOf(this->value + position, tempLength, RTSP_SESSION_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR, RTSP_SESSION_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH);
            result &= (index2 > 0);

            if (result)
            {
              this->timeout = GetValueUnsignedInt(this->value + position + index2 + RTSP_SESSION_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH, 0);
            }
          }
          else
          {
            // unknown parameter, ignore
          }

          position += tempLength + 1;
        }
      }
      else
      {
        // whole value is session ID
        this->sessionId = Duplicate(this->value);
        result &= (this->sessionId != NULL);
      }
    }
  }

  if (result)
  {
    this->responseHeaderType = Duplicate(RTSP_SESSION_RESPONSE_HEADER_TYPE);
    result &= (this->responseHeaderType != NULL);
  }

  return result;
}

/* protected methods */

bool CRtspSessionResponseHeader::CloneInternal(CHttpHeader *clone)
{
  bool result = __super::CloneInternal(clone);
  CRtspSessionResponseHeader *header = dynamic_cast<CRtspSessionResponseHeader *>(clone);
  result &= (header != NULL);

  if (result)
  {
    header->timeout = this->timeout;
    SET_STRING_AND_RESULT_WITH_NULL(header->sessionId, this->sessionId, result);
  }

  return result;
}

CHttpHeader *CRtspSessionResponseHeader::CreateHeader(void)
{
  HRESULT result = S_OK;
  CRtspSessionResponseHeader *header = new CRtspSessionResponseHeader(&result);
  CHECK_POINTER_HRESULT(result, header, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(header));
  return header;
}




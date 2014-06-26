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

#include "RtpMapAttribute.h"
#include "conversions.h"

CRtpMapAttribute::CRtpMapAttribute(HRESULT *result)
  : CAttribute(result)
{
  this->payloadType = 0;
  this->clockRate = 0;
  this->encodingName = NULL;
  this->encodingParameters = NULL;
}

CRtpMapAttribute::~CRtpMapAttribute(void)
{
  FREE_MEM(this->encodingName);
  FREE_MEM(this->encodingParameters);
}

/* get methods */

unsigned int CRtpMapAttribute::GetPayloadType(void)
{
  return this->payloadType;
}

const wchar_t *CRtpMapAttribute::GetEncodingName(void)
{
  return this->encodingName;
}

unsigned int CRtpMapAttribute::GetClockRate(void)
{
  return this->clockRate;
}

const wchar_t *CRtpMapAttribute::GetEncodingParameters(void)
{
  return this->encodingParameters;
}

/* set methods */

/* other methods */

void CRtpMapAttribute::Clear(void)
{
  __super::Clear();

  this->payloadType = 0;
  this->clockRate = 0;
  FREE_MEM(this->encodingName);
  FREE_MEM(this->encodingParameters);
}

unsigned int CRtpMapAttribute::Parse(const wchar_t *buffer, unsigned int length)
{
  unsigned int tempResult = __super::Parse(buffer, length);
  unsigned int result = (tempResult > SESSION_TAG_SIZE) ? tempResult : 0;

  if (result != 0)
  {
    // successful parsing of session tag
    // compare it to our session tag
    result = (wcscmp(this->originalTag, TAG_ATTRIBUTE) == 0) ? result : 0;
    result = (this->tagContent != NULL) ? result : 0;

    result = (wcscmp(this->attribute, TAG_ATTRIBUTE_RTP_MAP) == 0) ? result : 0;
    result = (this->value != NULL) ? result : 0;
  }

  if (result != 0)
  {
    FREE_MEM(this->instanceTag);
    this->instanceTag = Duplicate(TAG_ATTRIBUTE_INSTANCE_RTP_MAP);
    result = (this->instanceTag != NULL) ? result : 0;
    result = (this->value != NULL) ? result : 0;

    if (result != 0)
    {
      unsigned int valueLength = wcslen(this->value);
      result = (valueLength != 0) ? result : 0;

      if (result != 0)
      {
        unsigned int position = 0;
        int index = IndexOf(this->value, valueLength, L" ", 1);
        result = (index > 0) ? result : 0;

        if (result != 0)
        {
          this->payloadType = GetValueUnsignedInt(this->value, UINT_MAX);
          result = (this->payloadType != UINT_MAX) ? result : 0;

          position += index + 1;
        }
        result = (position < valueLength) ? result : 0;

        if (result != 0)
        {
          index = IndexOf(this->value + position, valueLength - position, L"/", 1);
          result = (index > 0) ? result : 0;

          if (result != 0)
          {
            this->encodingName = Substring(this->value, position, index);
            result = (this->encodingName != NULL) ? result : 0;

            position += index + 1;
          }
        }
        result = (position < valueLength) ? result : 0;

        if (result != 0)
        {
          index = IndexOf(this->value + position, valueLength - position, L"/", 1);

          this->clockRate = GetValueUnsignedInt(this->value + position, UINT_MAX);
          result = (this->clockRate != UINT_MAX) ? result : 0;

          if ((result != 0) && (index != (-1)))
          {
            // additional encoding parameters exists
            this->encodingParameters = Substring(this->value, position + index + 1, valueLength - position - index - 1);
            result = (this->encodingParameters != NULL) ? result : 0;
          }
        }
      }
    }
  }

  return result;
}
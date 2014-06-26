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

#include "NormalPlayTimeRangeAttribute.h"

#include "conversions.h"

CNormalPlayTimeRangeAttribute::CNormalPlayTimeRangeAttribute(HRESULT *result)
  : CRangeAttribute(result)
{
  this->startTime = 0;
  this->endTime = 0;
}

CNormalPlayTimeRangeAttribute::~CNormalPlayTimeRangeAttribute(void)
{
}

/* get methods */

uint64_t CNormalPlayTimeRangeAttribute::GetStartTime(void)
{
  return this->startTime;
}

uint64_t CNormalPlayTimeRangeAttribute::GetEndTime(void)
{
  return this->endTime;
}

/* set methods */

/* other methods */

bool CNormalPlayTimeRangeAttribute::IsSetStartTime(void)
{
  return this->IsSetFlags(NORMAL_PLAY_TIME_RANGE_ATTRIBUTE_FLAG_START_TIME);
}

bool CNormalPlayTimeRangeAttribute::IsSetEndTime(void)
{
  return this->IsSetFlags(NORMAL_PLAY_TIME_RANGE_ATTRIBUTE_FLAG_END_TIME);
}

unsigned int CNormalPlayTimeRangeAttribute::Parse(const wchar_t *buffer, unsigned int length)
{
  unsigned int tempResult = __super::Parse(buffer, length);
  unsigned int result = (tempResult > SESSION_TAG_SIZE) ? tempResult : 0;

  if (result != 0)
  {
    // successful parsing of session tag
    // compare it to our session tag

    FREE_MEM(this->instanceTag);
    this->instanceTag = Duplicate(TAG_ATTRIBUTE_INSTANCE_NORMAL_PLAY_TIME_RANGE);
    result = (this->instanceTag != NULL) ? result : 0;
    result = (this->rangeSpecification != NULL) ? result : 0;
    unsigned int rangeSpecificationLength = (result != 0) ? wcslen(this->rangeSpecification) : 0;

    if (result != 0)
    {
      int index = IndexOf(this->rangeSpecification, rangeSpecificationLength, NORMAL_PLAY_TIME_IDENTIFIER, NORMAL_PLAY_TIME_IDENTIFIER_LENGTH);
      result = (index >= 0) ? result : 0;
      result = (rangeSpecificationLength > ((unsigned int)index + NORMAL_PLAY_TIME_IDENTIFIER_LENGTH)) ? result : 0;

      if (result != 0)
      {
        int separatorIndex = IndexOf(this->rangeSpecification + index + NORMAL_PLAY_TIME_IDENTIFIER_LENGTH, rangeSpecificationLength - index - NORMAL_PLAY_TIME_IDENTIFIER_LENGTH, NORMAL_PLAY_TIME_SEPARATOR, NORMAL_PLAY_TIME_SEPARATOR_LENGTH);
        result = (separatorIndex > 0) ? result : 0;
        result = (rangeSpecificationLength >= ((unsigned int)index + NORMAL_PLAY_TIME_IDENTIFIER_LENGTH + (unsigned int)separatorIndex + NORMAL_PLAY_TIME_SEPARATOR_LENGTH)) ? result : 0;

        if (result > 0)
        {
          wchar_t *startTimeString = Substring(this->rangeSpecification, index + NORMAL_PLAY_TIME_IDENTIFIER_LENGTH, separatorIndex);
          wchar_t *endTimeString = Substring(this->rangeSpecification, index + NORMAL_PLAY_TIME_IDENTIFIER_LENGTH + separatorIndex + NORMAL_PLAY_TIME_SEPARATOR_LENGTH, rangeSpecificationLength - index - NORMAL_PLAY_TIME_IDENTIFIER_LENGTH - separatorIndex - NORMAL_PLAY_TIME_SEPARATOR_LENGTH);

          unsigned int startTimeStringLength = (startTimeString != NULL) ? wcslen(startTimeString) : 0;
          unsigned int endTimeStringLength = (endTimeString != NULL) ? wcslen(endTimeString) : 0;

          result = (startTimeStringLength > 0) ? result : 0;
        
          if (result != 0)
          {
            int decimalIndex = IndexOf(startTimeString, startTimeStringLength, NORMAL_PLAY_TIME_DECIMAL_SEPARATOR, NORMAL_PLAY_TIME_DECIMAL_SEPARATOR_LENGTH);

            if (decimalIndex == (-1))
            {
              // number without decimal part
              this->startTime = GetValueUnsignedInt64(startTimeString, 0) * 1000;
              this->flags |= NORMAL_PLAY_TIME_RANGE_ATTRIBUTE_FLAG_START_TIME;
            }
            else
            {
              // number with decimal part
              this->endTime = GetValueUnsignedInt64(startTimeString + decimalIndex + NORMAL_PLAY_TIME_DECIMAL_SEPARATOR_LENGTH, 0);
              for (unsigned int i = 0; i < (max(startTimeStringLength - decimalIndex - NORMAL_PLAY_TIME_DECIMAL_SEPARATOR_LENGTH, 3) - 3); i++)
              {
                this->endTime /= 10;
              }
              this->endTime += GetValueUnsignedInt64(startTimeString, 0) * 1000;
              this->flags |= NORMAL_PLAY_TIME_RANGE_ATTRIBUTE_FLAG_START_TIME;
            }

            if (endTimeStringLength != 0)
            {
              decimalIndex = IndexOf(endTimeString, endTimeStringLength, NORMAL_PLAY_TIME_DECIMAL_SEPARATOR, NORMAL_PLAY_TIME_DECIMAL_SEPARATOR_LENGTH);

              if (decimalIndex == (-1))
              {
                // number without decimal part
                this->endTime = GetValueUnsignedInt64(endTimeString, 0) * 1000;
                this->flags |= NORMAL_PLAY_TIME_RANGE_ATTRIBUTE_FLAG_END_TIME;
              }
              else
              {
                // number with decimal part
                this->endTime = GetValueUnsignedInt64(endTimeString + decimalIndex + NORMAL_PLAY_TIME_DECIMAL_SEPARATOR_LENGTH, 0);
                for (unsigned int i = 0; i < (max(endTimeStringLength - decimalIndex - NORMAL_PLAY_TIME_DECIMAL_SEPARATOR_LENGTH, 3) - 3); i++)
                {
                  this->endTime /= 10;
                }
                this->endTime += GetValueUnsignedInt64(endTimeString, 0) * 1000;
                this->flags |= NORMAL_PLAY_TIME_RANGE_ATTRIBUTE_FLAG_END_TIME;
              }
            }
          }

          FREE_MEM(startTimeString);
          FREE_MEM(endTimeString);
        }
      }
    }
  }

  return result;
}

void CNormalPlayTimeRangeAttribute::Clear(void)
{
  __super::Clear();

  this->startTime = 0;
  this->endTime = 0;
}
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

#include "ByteRangeAttribute.h"

CByteRangeAttribute::CByteRangeAttribute(HRESULT *result)
  : CAttribute(result)
{
  this->length = BYTE_RANGE_LENGTH_NOT_SPECIFIED;
  this->offset = BYTE_RANGE_OFFSET_NOT_SPECIFIED;
}

CByteRangeAttribute::~CByteRangeAttribute(void)
{
}

/* get methods */

/* set methods */

/* other methods */

void CByteRangeAttribute::Clear(void)
{
  __super::Clear();

  this->length = BYTE_RANGE_LENGTH_NOT_SPECIFIED;
  this->offset = BYTE_RANGE_OFFSET_NOT_SPECIFIED;
}

bool CByteRangeAttribute::Parse(const wchar_t *name, const wchar_t *value)
{
  bool result = __super::Parse(name, value);

  if (result)
  {
    int index = IndexOf(value, BYTE_RANGE_OFFSET_SEPARATOR);

    if (index != (-1))
    {
      // byte range length and offset specified

      wchar_t *lengthValue = Substring(value, 0, index);
      wchar_t *offsetValue = Substring(value, index + BYTE_RANGE_OFFSET_SEPARATOR_LENGTH);

      this->length = CAttribute::GetDecimalInteger(lengthValue);
      this->offset = CAttribute::GetDecimalInteger(offsetValue);

      FREE_MEM(lengthValue);
      FREE_MEM(offsetValue);

      result &= (this->length != BYTE_RANGE_LENGTH_NOT_SPECIFIED);
      result &= (this->offset != BYTE_RANGE_OFFSET_NOT_SPECIFIED);
    }
    else
    {
      // only byte range length specified
      this->length = CAttribute::GetDecimalInteger(value);

      result &= (this->length != BYTE_RANGE_LENGTH_NOT_SPECIFIED);
    }
  }

  return result;
}

/* protected methods */

CAttribute *CByteRangeAttribute::CreateAttribute(void)
{
  HRESULT result = S_OK;
  CByteRangeAttribute *attribute = new CByteRangeAttribute(&result);
  CHECK_POINTER_HRESULT(result, attribute, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(attribute));
  return attribute;
}

bool CByteRangeAttribute::CloneInternal(CAttribute *attribute)
{
  bool result = __super::CloneInternal(attribute);
  CByteRangeAttribute *byteRange = dynamic_cast<CByteRangeAttribute *>(attribute);
  result &= (byteRange != NULL);

  if (result)
  {
    byteRange->length = this->length;
    byteRange->offset = this->offset;
  }

  return result;
}
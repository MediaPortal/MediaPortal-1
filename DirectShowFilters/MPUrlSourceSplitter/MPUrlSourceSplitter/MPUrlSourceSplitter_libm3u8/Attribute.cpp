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

#include "Attribute.h"
#include "conversions.h"

CAttribute::CAttribute(HRESULT *result)
  : CFlags()
{
  this->name = NULL;
  this->value = NULL;
}

CAttribute::~CAttribute(void)
{
  FREE_MEM(this->name);
  FREE_MEM(this->value);
}

/* get methods */

const wchar_t *CAttribute::GetName(void)
{
  return this->name;
}

const wchar_t *CAttribute::GetValue(void)
{
  return this->value;
}


/* set methods */

/* other methods */

void CAttribute::Clear(void)
{
  FREE_MEM(this->name);
  FREE_MEM(this->value);
}

bool CAttribute::Parse(unsigned int version, const wchar_t *buffer, unsigned int length)
{
  this->Clear();

  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, buffer);
  CHECK_CONDITION_HRESULT(result, length > 0, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    int index = IndexOf(buffer, length, ATTRIBUTE_NAME_VALUE_SEPARATOR, ATTRIBUTE_NAME_VALUE_SEPARATOR_LENGTH);
    CHECK_CONDITION_HRESULT(result, index > 0, result, E_FAIL);

    if (SUCCEEDED(result))
    {
      wchar_t *name = Substring(buffer, 0, (unsigned int)index);
      wchar_t *value = Substring(buffer, index + ATTRIBUTE_NAME_VALUE_SEPARATOR_LENGTH, length - index - ATTRIBUTE_NAME_VALUE_SEPARATOR_LENGTH);

      CHECK_CONDITION_HRESULT(result, this->Parse(version, name, value), result, E_FAIL);

      FREE_MEM(name);
      FREE_MEM(value);
    }
  }

  return SUCCEEDED(result);
}

bool CAttribute::Parse(unsigned int version, const wchar_t *name, const wchar_t *value)
{
  this->Clear();

  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, name);
  CHECK_POINTER_DEFAULT_HRESULT(result, value);

  if (SUCCEEDED(result))
  {
    this->name = Duplicate(name);
    this->value = Duplicate(value);

    CHECK_POINTER_HRESULT(result, this->name, result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(result, this->value, result, E_OUTOFMEMORY);
  }
  
  return SUCCEEDED(result);
}

CAttribute *CAttribute::Clone(void)
{
  HRESULT result = S_OK;
  CAttribute *clone = this->CreateAttribute();
  CHECK_POINTER_HRESULT(result, clone, result, E_OUTOFMEMORY);

  CHECK_CONDITION_HRESULT(result, this->CloneInternal(clone), result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(clone));
  return clone;
}


/* static methods */

unsigned int CAttribute::GetDecimalInteger(const wchar_t *value)
{
  return GetValueUint(value, DECIMAL_INTEGER_NOT_SPECIFIED);
}

unsigned int CAttribute::GetHexadecimalInteger(const wchar_t *value)
{
  return GetHexValueUint(value, DECIMAL_INTEGER_NOT_SPECIFIED);
}

double CAttribute::GetDecimalFloatingPoint(const wchar_t *value)
{
  return GetValueDouble(value, DECIMAL_FLOATING_NOT_SPECIFIED);
}

wchar_t *CAttribute::GetQuotedString(const wchar_t *value)
{
  wchar_t *result = NULL;
  unsigned int valueLength = wcslen(value);

  // find first and last quote
  int firstQuoteIndex = -1;
  int lastQuoteIndex = -1;
  
  for (unsigned int i = 0; i < valueLength; i++)
  {
    if ((firstQuoteIndex == (-1)) && (value[i] == L'"'))
    {
      firstQuoteIndex = i;
    }

    if ((lastQuoteIndex == (-1)) && (value[valueLength - i - 1] == L'"'))
    {
      lastQuoteIndex = valueLength - i - 1;
    }

    if ((firstQuoteIndex != (-1)) && (lastQuoteIndex != (-1)))
    {
      break;
    }
  }

  if ((firstQuoteIndex != (-1)) && (lastQuoteIndex != (-1)) && (lastQuoteIndex != firstQuoteIndex))
  {
    // found both quotes and not on same position
    result = ALLOC_MEM_SET(result, wchar_t, (lastQuoteIndex - firstQuoteIndex), 0);

    if (result != NULL)
    {
      wcsncpy_s(result, lastQuoteIndex - firstQuoteIndex, value + firstQuoteIndex + 1, lastQuoteIndex - firstQuoteIndex - 1);
    }
  }

  return result;
}

wchar_t *CAttribute::GetEnumeratedString(const wchar_t *value)
{
  return Duplicate(value);
}

unsigned int CAttribute::GetDecimalResolutionWidth(const wchar_t *value)
{
  unsigned int result = RESOLUTION_NOT_SPECIFIED;
  int index = IndexOf(value, L"x");

  if (index != (-1))
  {
    wchar_t *widthValue = Substring(value, 0, index);

    result = GetValueUint(widthValue, RESOLUTION_NOT_SPECIFIED);

    FREE_MEM(widthValue);
  }

  return result;
}

unsigned int CAttribute::GetDecimalResolutionHeight(const wchar_t *value)
{
  unsigned int result = RESOLUTION_NOT_SPECIFIED;
  int index = IndexOf(value, L"x");

  if (index != (-1))
  {
    wchar_t *heightValue = Substring(value, 0, index);

    result = GetValueUint(heightValue, RESOLUTION_NOT_SPECIFIED);

    FREE_MEM(heightValue);
  }

  return result;
}

/* protected methods */

CAttribute *CAttribute::CreateAttribute(void)
{
  HRESULT result = S_OK;
  CAttribute *attribute = new CAttribute(&result);
  CHECK_POINTER_HRESULT(result, attribute, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(attribute));
  return attribute;
}

bool CAttribute::CloneInternal(CAttribute *attribute)
{
  bool result = (attribute != NULL);

  if (result)
  {
    attribute->flags = this->flags;

    SET_STRING_AND_RESULT_WITH_NULL(attribute->name, this->name, result);
    SET_STRING_AND_RESULT_WITH_NULL(attribute->value, this->value, result);
  }

  return result;
}
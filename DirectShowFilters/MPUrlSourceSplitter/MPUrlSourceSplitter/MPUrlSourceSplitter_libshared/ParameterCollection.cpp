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

#include "ParameterCollection.h"

CParameterCollection::CParameterCollection(HRESULT *result)
  : CKeyedCollection(result)
{
}

CParameterCollection::~CParameterCollection(void)
{
}

int CParameterCollection::CompareItemKeys(const wchar_t *firstKey, const wchar_t *secondKey, void *context)
{
  bool invariant = (*(bool *)context);

  if (invariant)
  {
    return _wcsicmp(firstKey, secondKey);
  }
  else
  {
    return wcscmp(firstKey, secondKey);
  }
}

bool CParameterCollection::Add(const wchar_t *name, const wchar_t *value)
{
  HRESULT result = S_OK;
  CParameter *parameter = new CParameter(&result, name, value);
  CHECK_POINTER_HRESULT(result, parameter, result, E_OUTOFMEMORY);

  CHECK_CONDITION_HRESULT(result, this->CCollection::Add(parameter), result, E_OUTOFMEMORY);
  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(parameter));

  return SUCCEEDED(result);
}

bool CParameterCollection::Contains(const wchar_t *name, bool invariant)
{
  return __super::Contains(name, (void *)&invariant);
}

bool CParameterCollection::Update(const wchar_t *name, bool invariant, CParameter *parameter)
{
  return __super::Update(name, (void *)&invariant, parameter);
}

bool CParameterCollection::Update(const wchar_t *name, bool invariant, const wchar_t *value)
{
  unsigned int index = this->GetItemIndex(name, (void *)&invariant);
  bool result = true;

  if (index != UINT_MAX)
  {
    result = __super::Remove(index);
  }
  
  if (result)
  {
    result = this->Add(name, value);
  }

  return result;
}

const wchar_t *CParameterCollection::GetKey(CParameter *item)
{
  return item->GetName();
}

CParameter *CParameterCollection::GetParameter(unsigned int index)
{
  return __super::GetItem(index);
}

CParameter *CParameterCollection::GetParameter(const wchar_t *name, bool invariant)
{
  return this->GetItem(name, (void *)&invariant);
}

void CParameterCollection::LogCollection(CLogger *logger, unsigned int loggerLevel, const wchar_t *protocolName, const wchar_t *functionName)
{
  unsigned int count = this->Count();
  if (protocolName == NULL)
  {
    logger->Log(loggerLevel, L"%s: configuration parameters: %u", functionName, count);
  }
  else
  {
    logger->Log(loggerLevel, L"%s: %s: configuration parameters: %u", protocolName, functionName, count);
  }
  for (unsigned int i = 0; i < count; i++)
  {
    CParameter *parameter = this->GetParameter(i);
    if (protocolName == NULL)
    {
      logger->Log(loggerLevel, L"%s: parameter %u, name: '%s', value: '%s'", functionName, i + 1, parameter->GetName(), parameter->GetValue());
    }
    else
    {
      logger->Log(loggerLevel, L"%s: %s: parameter %u, name: '%s', value: '%s'", protocolName, functionName, i + 1, parameter->GetName(), parameter->GetValue());
    }
  }
}

const wchar_t *CParameterCollection::GetValue(const wchar_t *name, bool invariant, const wchar_t *defaultValue)
{
  CParameter *parameter = this->GetParameter(name, invariant);
  if (parameter != NULL)
  {
    return parameter->GetValue();
  }
  else
  {
    return defaultValue;
  }
}

long CParameterCollection::GetValueLong(const wchar_t *name, bool invariant, long defaultValue)
{
  const wchar_t *value = this->GetValue(name, invariant, L"");
  wchar_t *end = NULL;
  long valueLong = wcstol(value, &end, 10);
  if ((valueLong == 0) && (value == end))
  {
    // error while converting
    valueLong = defaultValue;
  }

  return valueLong;
}

long CParameterCollection::GetValueUnsignedInt(const wchar_t *name, bool invariant, unsigned int defaultValue)
{
  const wchar_t *value = this->GetValue(name, invariant, L"");
  wchar_t *end = NULL;
  unsigned long valueLong = wcstoul(value, &end, 10);
  if ((valueLong == 0) && (value == end))
  {
    // error while converting
    valueLong = defaultValue;
  }

  return (unsigned int)valueLong;
}

int64_t CParameterCollection::GetValueInt64(const wchar_t *name, bool invariant, int64_t defaultValue)
{
  const wchar_t *value = this->GetValue(name, invariant, L"");
  wchar_t *end = NULL;
  int64_t valueLong = _wcstoi64(value, &end, 10);
  if ((valueLong == 0) && (value == end))
  {
    // error while converting
    valueLong = defaultValue;
  }

  return (unsigned int)valueLong;
}

bool CParameterCollection::GetValueBool(const wchar_t *name, bool invariant, bool defaultValue)
{
  switch (this->GetValueLong(name, invariant, -1))
  {
  case 0:
    return false;
  case 1:
    return true;
  default:
    return defaultValue;
  }
}

CParameter *CParameterCollection::Clone(CParameter *item)
{
  return item->Clone();
}

bool CParameterCollection::CopyParameter(const wchar_t *parameterName, bool invariant, const wchar_t *newParameterName)
{
  HRESULT result = ((parameterName == NULL) || (newParameterName == NULL)) ? E_POINTER : S_OK;

  if (SUCCEEDED(result))
  {
    if (this->Contains(parameterName, invariant))
    {
      CParameter *newParameter = new CParameter(&result, newParameterName, this->GetValue(parameterName, invariant, L""));
      CHECK_POINTER_HRESULT(result, newParameter, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, this->CCollection::Add(newParameter), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(newParameter));
    }
  }

  return SUCCEEDED(result);
}

bool CParameterCollection::Remove(const wchar_t *name, bool invariant)
{
  return __super::Remove(name, (void *)&invariant);
}
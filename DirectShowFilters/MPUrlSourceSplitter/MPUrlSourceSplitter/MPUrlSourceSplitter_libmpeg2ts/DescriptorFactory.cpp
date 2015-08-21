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

#include "DescriptorFactory.h"

#include "ConditionalAccessDescriptor.h"

CDescriptorFactory::CDescriptorFactory(HRESULT *result)
{
}

CDescriptorFactory::~CDescriptorFactory(void)
{
}

/* get methods */

/* set methods */

/* other methods */


CDescriptor *CDescriptorFactory::CreateDescriptor(const uint8_t *buffer, uint32_t length)
{
  CDescriptor *result = NULL;
  HRESULT continueParsing = ((buffer != NULL) && (length > 0)) ? S_OK : E_INVALIDARG;

  if (SUCCEEDED(continueParsing))
  {
    CDescriptor *descriptor = new CDescriptor(&continueParsing);
    CHECK_POINTER_HRESULT(continueParsing, descriptor, continueParsing, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(continueParsing, descriptor->Parse(buffer, length), continueParsing, E_FAIL);
    if (SUCCEEDED(continueParsing))
    {
      CREATE_SPECIFIC_DESCRIPTOR(descriptor, TAG_CONDITIONAL_ACCESS_DESCRIPTOR, CConditionalAccessDescriptor, buffer, length, continueParsing, result);
    }

    if (SUCCEEDED(continueParsing) && (result == NULL))
    {
      result = descriptor;
    }

    CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(descriptor));
  }

  return result;
}

/* protected methods */

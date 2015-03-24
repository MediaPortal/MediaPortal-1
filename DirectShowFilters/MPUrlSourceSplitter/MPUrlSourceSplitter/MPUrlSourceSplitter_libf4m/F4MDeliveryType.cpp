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

#include "F4MDeliveryType.h"
#include "F4M_Elements.h"

CF4MDeliveryType::CF4MDeliveryType(void)
{
  this->deliveryType = NULL;
}

CF4MDeliveryType::~CF4MDeliveryType(void)
{
  FREE_MEM(this->deliveryType);
}

/* get methods */

const wchar_t *CF4MDeliveryType::GetDeliveryType(void)
{
  return this->deliveryType;
}

/* set methods */

bool CF4MDeliveryType::SetDeliveryType(const wchar_t *deliveryType)
{
  SET_STRING_RETURN_WITH_NULL(this->deliveryType, deliveryType);
}

/* other methods */

bool CF4MDeliveryType::IsStreaming(void)
{
  return (this->GetDeliveryType() == NULL) ? false : (wcscmp(this->GetDeliveryType(), F4M_ELEMENT_DELIVERYTYPE_VALUE_STREAMINGW) == 0);
}

bool CF4MDeliveryType::IsProgressive(void)
{
  return (this->GetDeliveryType() == NULL) ? false : (wcscmp(this->GetDeliveryType(), F4M_ELEMENT_DELIVERYTYPE_VALUE_PROGRESSIVEW) == 0);
}

void CF4MDeliveryType::Clear(void)
{
  FREE_MEM(this->deliveryType);
}
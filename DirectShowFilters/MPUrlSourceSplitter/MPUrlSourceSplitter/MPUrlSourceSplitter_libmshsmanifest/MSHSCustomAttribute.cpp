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

#include "MSHSCustomAttribute.h"

CMSHSCustomAttribute::CMSHSCustomAttribute(void)
  : CSerializable()
{
  this->name = NULL;
  this->value = NULL;
}

CMSHSCustomAttribute::~CMSHSCustomAttribute(void)
{
}

/* get methods */

const wchar_t *CMSHSCustomAttribute::GetName(void)
{
  return this->name;
}

const wchar_t *CMSHSCustomAttribute::GetValue(void)
{
  return this->value;
}

/* set methods */

bool CMSHSCustomAttribute::SetName(const wchar_t *name)
{
  SET_STRING_RETURN_WITH_NULL(this->name, name);
}

bool CMSHSCustomAttribute::SetValue(const wchar_t *value)
{
  SET_STRING_RETURN_WITH_NULL(this->value, value);
}

/* other methods */

uint32_t CMSHSCustomAttribute::GetSerializeSize(void)
{
  uint32_t required = 0;
  required += this->GetSerializeStringSize(this->name);
  required += this->GetSerializeStringSize(this->value);

  return required;
}

bool CMSHSCustomAttribute::Serialize(uint8_t *buffer)
{
  bool result = __super::Serialize(buffer);
  uint32_t position = __super::GetSerializeSize();

  if (result)
  {
    result &= this->SerializeString(buffer + position, this->name);
    position += this->GetSerializeStringSize(this->name);

    result &= this->SerializeString(buffer + position, this->value);
    position += this->GetSerializeStringSize(this->value);
  }

  return result;
}

bool CMSHSCustomAttribute::Deserialize(const uint8_t *buffer)
{
  FREE_MEM(this->name);
  FREE_MEM(this->value);

  bool result = __super::Deserialize(buffer);
  uint32_t position = __super::GetSerializeSize();

  if (result)
  {
    result &= this->DeserializeString(buffer + position, &this->name);
    position += this->GetSerializeStringSize(this->name);
  }

  if (result)
  {
    result &= this->DeserializeString(buffer + position, &this->value);
    position += this->GetSerializeStringSize(this->value);
  }

  return result;
}
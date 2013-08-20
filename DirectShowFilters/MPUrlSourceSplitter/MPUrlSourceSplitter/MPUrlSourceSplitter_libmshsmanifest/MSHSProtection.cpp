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

#include "MSHSProtection.h"
#include "BufferHelper.h"

CMSHSProtection::CMSHSProtection(void) 
  : CSerializable()
{
  this->systemId = GUID_NULL;
  this->content = NULL;
}

CMSHSProtection::~CMSHSProtection(void)
{
  FREE_MEM(this->content);
}

/* get methods */

GUID CMSHSProtection::GetSystemId(void)
{
  return this->systemId;
}

const wchar_t *CMSHSProtection::GetContent(void)
{
  return this->content;
}

/* set methods */

void CMSHSProtection::SetSystemId(GUID systemId)
{
  this->systemId = systemId;
}

bool CMSHSProtection::SetContent(const wchar_t *content)
{
  SET_STRING_RETURN_WITH_NULL(this->content, content);
}

/* other methods */

uint32_t CMSHSProtection::GetSerializeSize(void)
{
  uint32_t required = sizeof(GUID);
  required += this->GetSerializeStringSize(this->content);

  return required;
}


bool CMSHSProtection::Serialize(uint8_t *buffer)
{
  bool result = __super::Serialize(buffer);
  uint32_t position = __super::GetSerializeSize();

  if (result)
  {
    uint8_t *systemId = (uint8_t *)&this->systemId;
    for (uint8_t i = 0; i < sizeof(GUID); i++)
    {
      WBE8INC(buffer, position, systemId[i]);
    }

    // store content
    result &= this->SerializeString(buffer + position, this->content);
    position += this->GetSerializeStringSize(this->content);
  }

  return result;
}

bool CMSHSProtection::Deserialize(const uint8_t *buffer)
{
  this->systemId = GUID_NULL;
  FREE_MEM(this->content);

  bool result = __super::Deserialize(buffer);
  uint32_t position = __super::GetSerializeSize();

  if (result)
  {
    uint8_t *systemId = (uint8_t *)&this->systemId;
    for (uint8_t i = 0; i < sizeof(GUID); i++)
    {
      RBE8INC(buffer, position, systemId[i]);
    }

    result &= this->DeserializeString(buffer + position, &this->content);
    position += this->GetSerializeStringSize(this->content);
  }

  return result;
}
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

#include "SourceDescriptionItemFactory.h"

#include "SourceDescriptionItem.h"
#include "NullSourceDescriptionItem.h"
#include "CanonicalEndPointSourceDescriptionItem.h"
#include "UserNameSourceDescriptionItem.h"
#include "EmailSourceDescriptionItem.h"
#include "PhoneNumberSourceDescriptionItem.h"
#include "GeographicUserLocationSourceDescriptionItem.h"
#include "ToolSourceDescriptionItem.h"
#include "NoteSourceDescriptionItem.h"
#include "PrivateSourceDescriptionItem.h"

CSourceDescriptionItemFactory::CSourceDescriptionItemFactory(void)
{
}

CSourceDescriptionItemFactory::~CSourceDescriptionItemFactory(void)
{
}

CSourceDescriptionItem *CSourceDescriptionItemFactory::CreateSourceDescriptionItem(const unsigned char *buffer, unsigned int length, unsigned int *position)
{
  CSourceDescriptionItem *result = NULL;
  bool continueParsing = ((buffer != NULL) && (length > 0) && (position != NULL));

  if (continueParsing)
  {
    *position = 0;

    if (continueParsing)
    {
      CREATE_SPECIFIC_SOURCE_DESCRIPTION_ITEM(CCanonicalEndPointSourceDescriptionItem, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SOURCE_DESCRIPTION_ITEM(CUserNameSourceDescriptionItem, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SOURCE_DESCRIPTION_ITEM(CEmailSourceDescriptionItem, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SOURCE_DESCRIPTION_ITEM(CPhoneNumberSourceDescriptionItem, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SOURCE_DESCRIPTION_ITEM(CGeographicUserLocationSourceDescriptionItem, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SOURCE_DESCRIPTION_ITEM(CToolSourceDescriptionItem, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SOURCE_DESCRIPTION_ITEM(CNoteSourceDescriptionItem, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SOURCE_DESCRIPTION_ITEM(CPrivateSourceDescriptionItem, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SOURCE_DESCRIPTION_ITEM(CNullSourceDescriptionItem, buffer, length, continueParsing, result, (*position));
    }
  }

  if (!continueParsing)
  {
    FREE_MEM_CLASS(result);
    *position = 0;
  }

  return result;
}
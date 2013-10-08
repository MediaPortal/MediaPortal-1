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

#include "SessionTagFactory.h"
#include "ProtocolVersion.h"
#include "Origin.h"
#include "SessionName.h"
#include "SessionInformation.h"
#include "ConnectionData.h"
#include "MediaDescription.h"

#include "NormalPlayTimeRangeAttribute.h"
#include "RangeAttribute.h"
#include "ControlAttribute.h"
#include "RtpMapAttribute.h"
#include "BinaryAttribute.h"
#include "Attribute.h"

CSessionTagFactory::CSessionTagFactory(void)
{
}

CSessionTagFactory::~CSessionTagFactory(void)
{
}

CSessionTag *CSessionTagFactory::CreateSessionTag(const wchar_t *buffer, unsigned int length, unsigned int *position)
{
  CSessionTag *result = NULL;
  bool continueParsing = ((buffer != NULL) && (length > 0) && (position != NULL));

  if (continueParsing)
  {
    *position = 0;
    CSessionTag *sessionTag = new CSessionTag();
    continueParsing &= (sessionTag != NULL);

    if (continueParsing)
    {
      *position = sessionTag->Parse(buffer, length);
      continueParsing &= (*position != 0);
    }

    if (continueParsing)
    {
      // insert most specific session tags on top

      CREATE_SPECIFIC_SESSION_TAG(sessionTag, TAG_PROTOCOL_VERSION, CProtocolVersion, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SESSION_TAG(sessionTag, TAG_ORIGIN, COrigin, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SESSION_TAG(sessionTag, TAG_SESSION_NAME, CSessionName, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SESSION_TAG(sessionTag, TAG_SESSION_INFORMATION, CSessionInformation, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SESSION_TAG(sessionTag, TAG_CONNECTION_DATA, CConnectionData, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SESSION_TAG(sessionTag, TAG_MEDIA_DESCRIPTION, CMediaDescription, buffer, length, continueParsing, result, (*position));

      CREATE_SPECIFIC_SESSION_TAG(sessionTag, TAG_ATTRIBUTE, CNormalPlayTimeRangeAttribute, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SESSION_TAG(sessionTag, TAG_ATTRIBUTE, CRangeAttribute, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SESSION_TAG(sessionTag, TAG_ATTRIBUTE, CControlAttribute, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SESSION_TAG(sessionTag, TAG_ATTRIBUTE, CRtpMapAttribute, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SESSION_TAG(sessionTag, TAG_ATTRIBUTE, CBinaryAttribute, buffer, length, continueParsing, result, (*position));
      CREATE_SPECIFIC_SESSION_TAG(sessionTag, TAG_ATTRIBUTE, CAttribute, buffer, length, continueParsing, result, (*position));
    }

    CHECK_CONDITION_NOT_NULL_EXECUTE(result, FREE_MEM_CLASS(sessionTag));

    if (continueParsing && (result == NULL))
    {
      result = sessionTag;
    }
  }

  if (!continueParsing)
  {
    FREE_MEM_CLASS(result);
    *position = 0;
  }

  return result;
}
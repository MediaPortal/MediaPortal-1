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

#include "SessionDescription.h"
#include "SessionTagFactory.h"

CSessionDescription::CSessionDescription(HRESULT *result)
{
  //this->bandwidth = NULL;
  this->connectionData = NULL;
  //this->emailAddress = NULL;
  this->origin = NULL;
  //this->phoneNumber = NULL;
  this->protocolVersion = NULL;
  this->sessionInformation = NULL;
  this->sessionName = NULL;
  //this->sessionTimes = NULL;
  //this->sessionUri = NULL;

  this->mediaDescriptions = NULL;
  this->attributes = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->mediaDescriptions = new CMediaDescriptionCollection(result);
    this->attributes = new CAttributeCollection(result);

    CHECK_POINTER_HRESULT(*result, this->mediaDescriptions, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->attributes, *result, E_OUTOFMEMORY);
  }
}

CSessionDescription::~CSessionDescription(void)
{
  //FREE_MEM_CLASS(this->bandwidth);
  FREE_MEM_CLASS(this->connectionData);
  //FREE_MEM_CLASS(this->emailAddress);
  FREE_MEM_CLASS(this->origin);
  //FREE_MEM_CLASS(this->phoneNumber);
  FREE_MEM_CLASS(this->protocolVersion);
  FREE_MEM_CLASS(this->sessionInformation);
  FREE_MEM_CLASS(this->sessionName);
  //FREE_MEM_CLASS(this->sessionTimes);
  //FREE_MEM_CLASS(this->sessionUri);

  FREE_MEM_CLASS(this->mediaDescriptions);
  FREE_MEM_CLASS(this->attributes);
}

/* get methods */

CProtocolVersion *CSessionDescription::GetProtocolVersion(void)
{
  return this->protocolVersion;
}

COrigin *CSessionDescription::GetOrigin(void)
{
  return this->origin;
}

CSessionName *CSessionDescription::GetSessionName(void)
{
  return this->sessionName;
}

CSessionInformation *CSessionDescription::GetSessionInformation(void)
{
  return this->sessionInformation;
}

CConnectionData *CSessionDescription::GetConnectionData(void)
{
  return this->connectionData;
}

CMediaDescriptionCollection *CSessionDescription::GetMediaDescriptions(void)
{
  return this->mediaDescriptions;
}

CAttributeCollection *CSessionDescription::GetAttributes(void)
{
  return this->attributes;
}

/* set methods */

/* other methods */

bool CSessionDescription::Parse(const wchar_t *buffer, unsigned int length)
{
  //FREE_MEM_CLASS(this->bandwidth);
  FREE_MEM_CLASS(this->connectionData);
  //FREE_MEM_CLASS(this->emailAddress);
  FREE_MEM_CLASS(this->origin);
  //FREE_MEM_CLASS(this->phoneNumber);
  FREE_MEM_CLASS(this->protocolVersion);
  FREE_MEM_CLASS(this->sessionInformation);
  FREE_MEM_CLASS(this->sessionName);
  //FREE_MEM_CLASS(this->sessionTimes);
  //FREE_MEM_CLASS(this->sessionUri);

  bool result = ((buffer != NULL) && (length > SESSION_TAG_SIZE) && (this->mediaDescriptions != NULL) && (this->attributes != NULL));

  if (result)
  {
    this->mediaDescriptions->Clear();
    this->attributes->Clear();

    wchar_t *tempReplacedBuffer = ReplaceString(buffer, L"\r", L"\n");
    wchar_t *replacedBuffer = ReplaceString(tempReplacedBuffer, L"\n\n", L"\n");
    CSessionTagFactory *factory = new CSessionTagFactory();
    result &= (factory != NULL) && (replacedBuffer != NULL);

    if (result)
    {
      unsigned int replacedLength = wcslen(replacedBuffer);
      unsigned int parsedPosition = 0;

      while (result && (parsedPosition < replacedLength))
      {
        unsigned int position = 0;
        CSessionTag *sessionTag =  factory->CreateSessionTag(replacedBuffer + parsedPosition, replacedLength - parsedPosition, &position);
        result &= (position != 0);
        bool assigned = false;

        if (result)
        {
          ASSIGN_SESSION_TAG_BY_ORIGINAL_TAG(sessionTag, TAG_PROTOCOL_VERSION, this->protocolVersion, CProtocolVersion, assigned, result);
          ASSIGN_SESSION_TAG_BY_ORIGINAL_TAG(sessionTag, TAG_ORIGIN, this->origin, COrigin, assigned, result);
          ASSIGN_SESSION_TAG_BY_ORIGINAL_TAG(sessionTag, TAG_SESSION_NAME, this->sessionName, CSessionName, assigned, result);
          ASSIGN_SESSION_TAG_BY_ORIGINAL_TAG(sessionTag, TAG_SESSION_INFORMATION, this->sessionInformation, CSessionInformation, assigned, result);
          ASSIGN_SESSION_TAG_BY_ORIGINAL_TAG(sessionTag, TAG_CONNECTION_DATA, this->connectionData, CConnectionData, assigned, result);

          ADD_SESSION_TAG_TO_COLLECTION_BY_ORIGINAL_TAG(sessionTag, TAG_MEDIA_DESCRIPTION, this->mediaDescriptions, CMediaDescription, assigned, result);
          ADD_SESSION_TAG_TO_COLLECTION_BY_ORIGINAL_TAG(sessionTag, TAG_ATTRIBUTE, this->attributes, CAttribute, assigned, result);
        }

        if ((!result) || (!assigned))
        {
          FREE_MEM_CLASS(sessionTag);
        }

        parsedPosition += position;
      }
    }

    FREE_MEM_CLASS(factory);
    FREE_MEM(replacedBuffer);
    FREE_MEM(tempReplacedBuffer);
  }

  if (!result)
  {
    // each session description must have protocol version, origin and session name
    result &= ((this->protocolVersion != NULL) && (this->origin != NULL) && (this->sessionName != NULL));
  }

  return result;
}
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

#pragma once

#ifndef __SESSION_DESCRIPTION_DEFINED
#define __SESSION_DESCRIPTION_DEFINED

#include "ProtocolVersion.h"
#include "Origin.h"
#include "SessionName.h"
#include "SessionInformation.h"
//#include "SessionUri.h"
//#include "EmailAddress.h"
//#include "PhoneNumber.h"
#include "ConnectionData.h"
//#include "Bandwidth.h"
//#include "SessionTimeCollection.h"
#include "MediaDescriptionCollection.h"
#include "AttributeCollection.h"

class CSessionDescription
{
public:
  // initializes a new instance of CSessionDescription class
  CSessionDescription(void);
  ~CSessionDescription(void);

  /* get methods */

  // gets protocol version
  // @return : protocol version
  CProtocolVersion *GetProtocolVersion(void);

  // gets origin
  // @return : origin
  COrigin *GetOrigin(void);

  // gets session name
  // @return : session name
  CSessionName *GetSessionName(void);

  // gets session information
  // @return : session infromation or NULL if not specified
  CSessionInformation *GetSessionInformation(void);

  // gets connection data
  // @return : connection data or NULL if not specified
  CConnectionData *GetConnectionData(void);

  // gets media descriptions
  // @return : media descriptions
  CMediaDescriptionCollection *GetMediaDescriptions(void);

  // gets attributes
  // @return : attributes
  CAttributeCollection *GetAttributes(void);

  /* set methods */

  /* other methods */

  // parses data in buffer
  // @param buffer : buffer with session description data for parsing
  // @param length : the length of data in buffer
  // @return : true if successfully parsed, false otherwise
  bool Parse(const wchar_t *buffer, unsigned int length);

protected:

  // holds protocol version
  CProtocolVersion *protocolVersion;

  // holds origin
  COrigin *origin;

  // holds session name
  CSessionName *sessionName;

  // holds session information
  CSessionInformation *sessionInformation;

  // holds session URI
  //CSessionUri *sessionUri;

  // holds email address
  //CEmailAddress *emailAddress;

  // holds phone number
  //CPhoneNumber *phoneNumber;

  CConnectionData *connectionData;

  // holds bandwidth
  //CBandwidth *bandwidth;

  // holds session times and repeat times
  //CSessionTimeCollection *sessionTimes;

  // holds media description collection
  CMediaDescriptionCollection *mediaDescriptions;

  // holds attributes
  CAttributeCollection *attributes;
};

#endif
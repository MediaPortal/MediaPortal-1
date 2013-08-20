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

#ifndef __PARSED_MEDIA_DATA_BOX_DEFINED
#define __PARSED_MEDIA_DATA_BOX_DEFINED

#include "AkamaiFlvPacketCollection.h"

class CParsedMediaDataBox
{
public:
  CParsedMediaDataBox(void);
  ~CParsedMediaDataBox(void);

  /* get methods */

  // gets akamai GUID
  // @return : akamai GUID or NULL if error
  const wchar_t *GetAkamaiGuid(void);

  // gets akamai FLV packet collection
  // @return : akamai FLV packet collection
  CAkamaiFlvPacketCollection *GetAkamaiFlvPackets(void);

  /* set methods */

  // sets akamai GUID
  // @param akamaiGuid : akamai GUID to set
  // @return : true if successful, false otherwise
  bool SetAkamaiGuid(const wchar_t *akamaiGuid);

  // sets if media data box is in data
  // @param isMediaDataBox : specifies if media data box is in data
  void SetMediaDataBox(bool isMediaDataBox);

  /* other methods */

  // tests if media data box is in data
  // @return : true if media data box is in data, false otherwise
  bool IsMediaDataBox(void);

protected:

  // specifies if media data box is in data
  bool isMediaDataBox;
  // holds akamai FLV packet collection from media data box
  CAkamaiFlvPacketCollection *akamaiFlvPacketCollection;
  // holds akamai GUID
  wchar_t *akamaiGuid;
};

#endif
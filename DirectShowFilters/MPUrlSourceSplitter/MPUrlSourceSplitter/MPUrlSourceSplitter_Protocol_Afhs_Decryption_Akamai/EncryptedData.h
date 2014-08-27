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

#ifndef __ENCRYPTED_DATA_DEFINED
#define __ENCRYPTED_DATA_DEFINED

#include "AkamaiFlvPacket.h"

class CEncryptedData
{
public:
  CEncryptedData(void);
  ~CEncryptedData(void);

  /* get methods */

  // gets encrypted data
  // @return : encrypted data
  const uint8_t *GetEncryptedData(void);

  // gets encrypted data length
  // @return : encrypted data length
  unsigned int GetEncryptedLength(void);

  // gets akamai FLV packet
  // @return : akamai FLV packet
  CAkamaiFlvPacket *GetAkamaiFlvPacket(void);

  /* set methods */

  // sets encrypted data
  // @param encryptedData : encrypted data to set
  void SetEncryptedData(uint8_t *encryptedData);

  // sets encrypted data length
  // @param length : encrypted data length to set
  void SetEncryptedLength(unsigned int encryptedLength);

  // sets akamai FLV packet (only reference, packet is freed in desctructor)
  // @param flvPacket : akamai FLV packet to set
  void SetAkamaiFlvPacket(CAkamaiFlvPacket *flvPacket);

  /* other methods */

protected:
  // holds encrypted data
  uint8_t *encryptedData;
  // holds encrypted data length
  unsigned int encryptedLength;
  // holds akamai FLV packet
  CAkamaiFlvPacket *flvPacket;

  /* methods */
};

#endif
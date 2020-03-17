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

#ifndef __AKAMAI_FLV_PACKET_DEFINED
#define __AKAMAI_FLV_PACKET_DEFINED

#include "FlvPacket.h"

#define AKAMAI_VERSION_UNSPECIFIED                                            0
#define AKAMAI_VERSION_11                                                     11
#define AKAMAI_VERSION_12                                                     12

#define AKAMAI_PACKET_DATA_START                                              11
#define AKAMAI_PACKET_HEADER_MINIMUM_SIZE                                     13

#define AKAMAI_IV_SIZE                                                        16

#define FLV_PARSE_RESULT_ERROR_COUNT_AKAMAI                                   (FLV_PARSE_RESULT_ERROR_COUNT + 6)

#define FLV_PARSE_RESULT_NOT_AKAMAI_PACKET                                    (-FLV_PARSE_RESULT_ERROR_COUNT - 1)
#define FLV_PARSE_RESULT_NOT_ENOUGH_DATA_FOR_AKAMAI_HEADER                    (-FLV_PARSE_RESULT_ERROR_COUNT - 2)
#define FLV_PARSE_RESULT_NOT_ENOUGH_DATA_FOR_AKAMAI_PACKET                    (-FLV_PARSE_RESULT_ERROR_COUNT - 3)
#define FLV_PARSE_RESULT_NOT_FOUND_KEY_END                                    (-FLV_PARSE_RESULT_ERROR_COUNT - 4)
#define FLV_PARSE_RESULT_CANNOT_GET_KEY_URL                                   (-FLV_PARSE_RESULT_ERROR_COUNT - 5)
#define FLV_PARSE_RESULT_CANNOT_GET_SESSION_ID                                (-FLV_PARSE_RESULT_ERROR_COUNT - 6)

class CAkamaiFlvPacket : public CFlvPacket
{
public:
  CAkamaiFlvPacket(HRESULT *result);
  virtual ~CAkamaiFlvPacket(void);

  /* get methods */

  // gets FLV packet type
  // @return : FLV packet type
  virtual unsigned int GetType(void);

  // gets akamai packet version
  // @return : akamai packet version or AKAMAI_VERSION_UNSPECIFIED if unknown
  virtual uint8_t GetVersion(void);

  // gets ecm id
  // @return : ecm id
  virtual int32_t GetEcmId(void);

  // gets ecm timestamp
  // @return : ecm timestamp
  virtual int32_t GetEcmTimestamp(void);

  // gets key url
  // @return : key url or NULL if error
  virtual const wchar_t *GetKeyUrl(void);

  // gets session ID
  // @return : session ID or NULL if error
  virtual const wchar_t *GetSessionId(void);

  // gets initialization vector
  // @return : initialization vector or NULL if error
  virtual const uint8_t *GetIV(void);

  // gets IV size
  // @return : IV size
  virtual unsigned int GetIvSize(void);

  /* set methods */

  /* other methods */

  // tests if current instance is valid
  // @return : true if valid, false otherwise
  virtual bool IsValid();

  // tests if packet is akamai FLV packet
  // @return : true if packet is akamai FLV packet, false otherwise
  virtual bool IsAkamaiFlvPacket(void);

  // parses buffer for FLV packet
  // @param buffer : buffer to parse
  // @param length : length of buffer
  // @return : 0 if FLV packet found, FLV_PARSE_RESULT value otherwise
  virtual int ParsePacket(const unsigned char *buffer, unsigned int length);

  // parses buffer for FLV packet
  // @param buffer : linear buffer to parse
  // @return : 0 if FLV packet found, FLV_PARSE_RESULT value otherwise
  virtual int ParsePacket(CLinearBuffer *buffer);

  // tests if current instance has initialization vector
  // @return : true if current instance has IV, false otherwise
  virtual bool HasIV(void);

  // tests if current instance has key url
  // @return : true if current instance has key url, false otherwise
  virtual bool HasKey(void);

  // clears current instance
  virtual void Clear(void);

protected:
  // akamai version
  uint8_t version;
  // specifies if akamai packet has initialization vector
  bool hasIV;
  // specifies if akamai packet has key url
  bool hasKey;
  // holds ecm Id
  int32_t ecmId;
  // holds ecm timestamp
  int32_t ecmTimestamp;
  // holds initialization vector (if packet has IV)
  uint8_t *iv;
  // holds IV size
  unsigned int ivSize;
  // holds key url (if packet has key url)
  wchar_t *keyUrl;
  // holds session ID (if packet has key url)
  wchar_t *sessionId;
  // holds kdf version
  int16_t kdfVersion;

  /* methods */
};

#endif
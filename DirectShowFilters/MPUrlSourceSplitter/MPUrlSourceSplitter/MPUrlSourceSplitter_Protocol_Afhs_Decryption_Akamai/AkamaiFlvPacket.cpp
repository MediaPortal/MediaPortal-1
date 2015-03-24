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

#include "AkamaiFlvPacket.h"
#include "BufferHelper.h"

CAkamaiFlvPacket::CAkamaiFlvPacket(HRESULT *result)
  : CFlvPacket(result)
{
  this->version = AKAMAI_VERSION_UNSPECIFIED;
  this->ecmId = 0;
  this->ecmTimestamp = 0;
  this->hasIV = false;
  this->hasKey = false;
  this->iv = NULL;
  this->keyUrl = NULL;
  this->sessionId = NULL;
  this->ivSize = 0;
  this->kdfVersion = 0;
}

CAkamaiFlvPacket::~CAkamaiFlvPacket(void)
{
  FREE_MEM(this->iv);
  FREE_MEM(this->keyUrl);
  FREE_MEM(this->sessionId);
}

/* get methods */

unsigned int CAkamaiFlvPacket::GetType(void)
{
  return (__super::GetType() & 0x0000000D);
}

uint8_t CAkamaiFlvPacket::GetVersion(void)
{
  return this->version;
}

int32_t CAkamaiFlvPacket::GetEcmId(void)
{
  return this->ecmId;
}

int32_t CAkamaiFlvPacket::GetEcmTimestamp(void)
{
  return this->ecmTimestamp;
}

const wchar_t *CAkamaiFlvPacket::GetKeyUrl(void)
{
  return this->keyUrl;
}

const wchar_t *CAkamaiFlvPacket::GetSessionId(void)
{
  return this->sessionId;
}

const uint8_t *CAkamaiFlvPacket::GetIV(void)
{
  return this->iv;
}

unsigned int CAkamaiFlvPacket::GetIvSize(void)
{
  return this->ivSize;
}

/* set methods */

/* other methods */

bool CAkamaiFlvPacket::IsValid()
{
  return (__super::IsValid() && (this->GetVersion() == AKAMAI_VERSION_12));
}

bool CAkamaiFlvPacket::IsAkamaiFlvPacket(void)
{
  return ((this->type & 0x00000002) != 0);
}

int CAkamaiFlvPacket::ParsePacket(const unsigned char *buffer, unsigned int length)
{
  int result = __super::ParsePacket(buffer, length);

  if (result == FLV_PARSE_RESULT_OK)
  {
    // correctly parsed FLV packet
    // it doesn't mean that packet is also akamai FLV packet

    result = FLV_PARSE_RESULT_NOT_ENOUGH_DATA_FOR_AKAMAI_HEADER;

    if (this->size >= (AKAMAI_PACKET_DATA_START + AKAMAI_PACKET_HEADER_MINIMUM_SIZE))
    {
      // valid akamai FLV packet size
      unsigned int position = AKAMAI_PACKET_DATA_START;
      uint8_t *buf = this->packet;

      RBE8INC_DEFINE(buf, position, firstByte, uint8_t);
      this->version = firstByte >> 4;
      if (this->version != AKAMAI_VERSION_11)
      {
        this->version = firstByte;
      }

      switch (this->version)
      {
      case AKAMAI_VERSION_12:
        {
          result = FLV_PARSE_RESULT_OK;

          RBE32INC(buf, position, this->ecmId);
          RBE32INC(buf, position, this->ecmTimestamp);
          RBE16INC(buf, position, this->kdfVersion);

          // skip reserved byte
          position++;

          RBE8INC_DEFINE(buf, position, messageByte, uint8_t);

          this->hasIV = ((messageByte & 0x02) != 0);
          this->hasKey = ((messageByte & 0x04) != 0);

          if (this->hasIV)
          {
            // next 16 bytes is IV
            this->ivSize = AKAMAI_IV_SIZE;
            this->iv = ALLOC_MEM_SET(this->iv, uint8_t, AKAMAI_IV_SIZE, 0);
            result = (this->iv != NULL) ? result : FLV_PARSE_RESULT_NOT_ENOUGH_MEMORY;

            if (result == FLV_PARSE_RESULT_OK)
            {
              for (unsigned int i = 0; i < AKAMAI_IV_SIZE; i++)
              {
                RBE8INC(buf, position, *(this->iv + i));
              }
            }
          }

          if ((result == FLV_PARSE_RESULT_OK) && (this->hasKey))
          {
            // everything until zero byte is key url
            // get key url size, than read it
            unsigned int tempPosition = position;
            unsigned int size = 0;
            uint8_t tempValue = 0;
            bool foundEnd = false;

            while ((position + size) < length)
            {
              RBE8INC(buf, tempPosition, tempValue);
              if (tempValue != 0)
              {
                size++;
              }
              else
              {
                foundEnd = true;
                break;
              }
            }
            result = (foundEnd) ? result : FLV_PARSE_RESULT_NOT_FOUND_KEY_END;

            if ((result == FLV_PARSE_RESULT_OK) && (size > 0))
            {
              ALLOC_MEM_DEFINE_SET(tempKeyUrl, char, (size + 1), 0);
              result = (tempKeyUrl != NULL) ? result : FLV_PARSE_RESULT_NOT_ENOUGH_MEMORY;

              if (result == FLV_PARSE_RESULT_OK)
              {
                for (unsigned int i = 0; i < size; i++)
                {
                  RBE8INC(buf, position, *(tempKeyUrl + i));
                }

                this->keyUrl = ConvertToUnicodeA(tempKeyUrl);
                result = (this->keyUrl != NULL) ? result : FLV_PARSE_RESULT_CANNOT_GET_KEY_URL;

                if (result == FLV_PARSE_RESULT_OK)
                {
                  // set session ID
                  int sessionIdIndex = IndexOf(this->keyUrl, L"/key_");
                  if (sessionIdIndex != (-1))
                  {
                    this->sessionId = Substring(this->keyUrl, sessionIdIndex + 5);
                    result = (this->sessionId != NULL) ? result : FLV_PARSE_RESULT_CANNOT_GET_SESSION_ID;
                  }
                }
              }
              FREE_MEM(tempKeyUrl);
            }
          }

          if (result != FLV_PARSE_RESULT_OK)
          {
            this->Clear();
          }
        }
        break;
      default:
        // bad packet version
        this->Clear();
        result = FLV_PARSE_RESULT_NOT_AKAMAI_PACKET;
        break;
      }
    }
  }

  return result;
}

int CAkamaiFlvPacket::ParsePacket(CLinearBuffer *buffer)
{
  return __super::ParsePacket(buffer);
}

bool CAkamaiFlvPacket::HasIV(void)
{
  return this->hasIV;
}

bool CAkamaiFlvPacket::HasKey(void)
{
  return this->hasKey;
}

void CAkamaiFlvPacket::Clear(void)
{
  this->version = AKAMAI_VERSION_UNSPECIFIED;
  this->ecmId = 0;
  this->ecmTimestamp = 0;
  this->hasIV = false;
  this->hasKey = false;
  FREE_MEM(this->iv);
  FREE_MEM(this->keyUrl);
  this->ivSize = 0;
  this->kdfVersion = 0;

  __super::Clear();
}
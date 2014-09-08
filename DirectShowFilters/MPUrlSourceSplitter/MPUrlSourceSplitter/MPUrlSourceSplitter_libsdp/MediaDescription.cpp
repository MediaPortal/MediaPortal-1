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

#include "MediaDescription.h"
#include "SessionTagFactory.h"
#include "conversions.h"
#include "BinaryAttribute.h"
#include "RtpMapAttribute.h"
#include "KnownPayloadTypeCollection.h"

CMediaDescription::CMediaDescription(HRESULT *result)
  : CSessionTag(result)
{
  this->mediaType = NULL;
  this->numberOfPorts = MEDIA_DESCRIPTION_NUMBER_OF_PORTS_DEFAULT;
  this->port = MEDIA_DESCRIPTION_PORT_DEFAULT;
  this->transportProtocol = NULL;
  this->attributes = NULL;
  this->mediaFormats = NULL;
  this->connectionData = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->attributes = new CAttributeCollection(result);
    this->mediaFormats = new CMediaFormatCollection(result);

    CHECK_POINTER_HRESULT(*result, this->attributes, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->mediaFormats, *result, E_OUTOFMEMORY);
  }
}

CMediaDescription::~CMediaDescription(void)
{
  FREE_MEM(this->mediaType);
  FREE_MEM(this->transportProtocol);
  FREE_MEM_CLASS(this->attributes);
  FREE_MEM_CLASS(this->mediaFormats);
  FREE_MEM_CLASS(this->connectionData);
}

/* get methods */

const wchar_t *CMediaDescription::GetMediaType(void)
{
  return this->mediaType;
}

unsigned int CMediaDescription::GetPort(void)
{
  return this->port;
}

unsigned int CMediaDescription::GetNumberOfPorts(void)
{
  return this->numberOfPorts;
}

const wchar_t *CMediaDescription::GetTransportProtocol(void)
{
  return this->transportProtocol;
}

CAttributeCollection *CMediaDescription::GetAttributes(void)
{
  return this->attributes;
}

CMediaFormatCollection *CMediaDescription::GetMediaFormats(void)
{
  return this->mediaFormats;
}

CConnectionData *CMediaDescription::GetConnectionData(void)
{
  return this->connectionData;
}

/* set methods */

/* other methods */

bool CMediaDescription::IsAudio(void)
{
  return this->IsSetFlags(MEDIA_DESCRIPTION_FLAG_MEDIA_TYPE_AUDIO);
}

bool CMediaDescription::IsVideo(void)
{
  return this->IsSetFlags(MEDIA_DESCRIPTION_FLAG_MEDIA_TYPE_VIDEO);
}

bool CMediaDescription::IsApplication(void)
{
  return this->IsSetFlags(MEDIA_DESCRIPTION_FLAG_MEDIA_TYPE_APPLICATION);
}

bool CMediaDescription::IsData(void)
{
  return this->IsSetFlags(MEDIA_DESCRIPTION_FLAG_MEDIA_TYPE_DATA);
}

bool CMediaDescription::IsControl(void)
{
  return this->IsSetFlags(MEDIA_DESCRIPTION_FLAG_MEDIA_TYPE_CONTROL);
}

void CMediaDescription::Clear(void)
{
  __super::Clear();

  FREE_MEM(this->mediaType);
  FREE_MEM(this->transportProtocol);
  this->numberOfPorts = MEDIA_DESCRIPTION_NUMBER_OF_PORTS_DEFAULT;
  this->port = MEDIA_DESCRIPTION_PORT_DEFAULT;

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->attributes, this->attributes->Clear());
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->mediaFormats, this->mediaFormats->Clear());
  FREE_MEM_CLASS(this->connectionData);
}

unsigned int CMediaDescription::Parse(const wchar_t *buffer, unsigned int length)
{
  unsigned int tempResult = __super::Parse(buffer, length);
  unsigned int result = (tempResult > SESSION_TAG_SIZE) ? tempResult : 0;

  if (result != 0)
  {
    // successful parsing of session tag
    // compare it to our session tag
    result = (wcscmp(this->originalTag, TAG_MEDIA_DESCRIPTION) == 0) ? result : 0;
    result = (this->tagContent != NULL) ? result : 0;
  }

  if (result != 0)
  {
    this->instanceTag = Duplicate(TAG_MEDIA_DESCRIPTION);
    unsigned int tagContentLength = result - SESSION_TAG_SIZE - 1;
    result = (this->instanceTag != NULL) ? result : 0;
    
    // media type
    unsigned int position = 0;
    int index = IndexOf(this->tagContent, tagContentLength, L" ", 1);
    result = (index > 0) ? result : 0;

    if (result != 0)
    {
      this->mediaType = Substring(this->tagContent, position, index);
      result = (this->mediaType != NULL) ? result : 0;
    }

    if (result != 0)
    {
      position += index + 1;
    }

    // port and number of ports (optional)
    index = IndexOf(this->tagContent + position, tagContentLength - position, L" ", 1);
    result = (index > 0) ? result : 0;

    if (result != 0)
    {
      this->port = GetValueUint(this->tagContent + position, MEDIA_DESCRIPTION_PORT_DEFAULT);
    }

    if (result != 0)
    {
      position += index + 1;
    }

    // transport protocol
    index = IndexOf(this->tagContent + position, tagContentLength, L" ", 1);
    result = (index > 0) ? result : 0;

    if (result != 0)
    {
      this->transportProtocol = Substring(this->tagContent, position, index);
      result = (this->transportProtocol != NULL) ? result : 0;
    }

    if (result != 0)
    {
      position += index + 1;
    }

    // media formats
    while ((result != 0) && (position < tagContentLength))
    {
      index = IndexOf(this->tagContent + position, tagContentLength, L" ", 1);
      unsigned int payloadType = GetValueUint(this->tagContent + position, MEDIA_FORMAT_PAYLOAD_TYPE_UNSPECIFIED);

      if (index == (-1))
      {
        // no space, last media format
        position = tagContentLength;
      }
      else
      {
        position += index + 1;
      }

      HRESULT res = S_OK;
      CMediaFormat *mediaFormat = new CMediaFormat(&res);
      result = (SUCCEEDED(res) && (mediaFormat != NULL)) ? result : 0;

      if (result != 0)
      {
        mediaFormat->SetPayloadType(payloadType);

        result = this->mediaFormats->Add(mediaFormat) ? result : 0;
      }

      if (result == 0)
      {
        FREE_MEM_CLASS(mediaFormat);
      }
    }

    // parse everything to next media description or end of session description

    CSessionTagFactory *factory = new CSessionTagFactory();
    result = (factory != NULL) ? result : 0;

    unsigned int parsedPosition = result;
    bool foundMediaDescription = false;

    while ((!foundMediaDescription) && (result != 0) && (parsedPosition < length))
    {
      unsigned int bufferPosition = 0;
      CSessionTag *sessionTag =  factory->CreateSessionTag(buffer + parsedPosition, length - parsedPosition, &bufferPosition);
      result = (bufferPosition != 0) ? result : 0;
      bool assigned = false;

      if (result != 0)
      {
        foundMediaDescription |= sessionTag->IsOriginalTag(TAG_MEDIA_DESCRIPTION);
        if (!foundMediaDescription)
        {
          ASSIGN_SESSION_TAG_BY_ORIGINAL_TAG(sessionTag, TAG_CONNECTION_DATA, this->connectionData, CConnectionData, assigned, result);

          ADD_SESSION_TAG_TO_COLLECTION_BY_ORIGINAL_TAG(sessionTag, TAG_ATTRIBUTE, this->attributes, CBinaryAttribute, assigned, result);
          ADD_SESSION_TAG_TO_COLLECTION_BY_ORIGINAL_TAG(sessionTag, TAG_ATTRIBUTE, this->attributes, CRtpMapAttribute, assigned, result);
          ADD_SESSION_TAG_TO_COLLECTION_BY_ORIGINAL_TAG(sessionTag, TAG_ATTRIBUTE, this->attributes, CAttribute, assigned, result);
        }
      }

      if ((!result) || (!assigned) || (foundMediaDescription))
      {
        FREE_MEM_CLASS(sessionTag);
      }

      if (!foundMediaDescription)
      {
        parsedPosition += bufferPosition;
      }
    }

    result = parsedPosition;

    FREE_MEM_CLASS(factory);

    // set flags
    if (result != 0)
    {
      if (wcscmp(this->mediaType, MEDIA_DESCRIPTION_MEDIA_TYPE_AUDIO) == 0)
      {
        this->flags |= MEDIA_DESCRIPTION_FLAG_MEDIA_TYPE_AUDIO;
      }
      if (wcscmp(this->mediaType, MEDIA_DESCRIPTION_MEDIA_TYPE_VIDEO) == 0)
      {
        this->flags |= MEDIA_DESCRIPTION_FLAG_MEDIA_TYPE_VIDEO;
      }
      if (wcscmp(this->mediaType, MEDIA_DESCRIPTION_MEDIA_TYPE_APPLICATION) == 0)
      {
        this->flags |= MEDIA_DESCRIPTION_FLAG_MEDIA_TYPE_APPLICATION;
      }
      if (wcscmp(this->mediaType, MEDIA_DESCRIPTION_MEDIA_TYPE_DATA) == 0)
      {
        this->flags |= MEDIA_DESCRIPTION_FLAG_MEDIA_TYPE_DATA;
      }
      if (wcscmp(this->mediaType, MEDIA_DESCRIPTION_MEDIA_TYPE_CONTROL) == 0)
      {
        this->flags |= MEDIA_DESCRIPTION_FLAG_MEDIA_TYPE_CONTROL;
      }

      if (wcscmp(this->transportProtocol, MEDIA_DESCRIPTION_TRANSPORT_PROTOCOL_RTP_AVP) == 0)
      {
        this->flags |= MEDIA_DESCRIPTION_FLAG_TRANSPORT_PROTOCOL_RTP_AVP;
      }
      if (wcscmp(this->transportProtocol, MEDIA_DESCRIPTION_TRANSPORT_PROTOCOL_UDP) == 0)
      {
        this->flags |= MEDIA_DESCRIPTION_FLAG_TRANSPORT_PROTOCOL_UDP;
      }

      HRESULT res = S_OK;
      CKnownPayloadTypeCollection *knownPayloadTypes = new CKnownPayloadTypeCollection(&res);
      result = (SUCCEEDED(res) && (knownPayloadTypes != NULL)) ? result : 0;

      CHECK_CONDITION_EXECUTE(result != 0, result = (knownPayloadTypes->Count() != 0) ? result : 0);

      // update media formats
      for (unsigned int i = 0; ((result != 0) && (i < this->mediaFormats->Count())); i++)
      {
        CMediaFormat *mediaFormat = this->mediaFormats->GetItem(i);
        result = mediaFormat->SetType(this->mediaType) ? result : 0;

        if (result != 0)
        {
          // try to find known media type for media format

          if (mediaFormat->GetPayloadType() != MEDIA_FORMAT_PAYLOAD_TYPE_UNSPECIFIED)
          {
            for (unsigned int j = 0; ((result != 0) && (j < knownPayloadTypes->Count())); j++)
            {
              CPayloadType *knownPayloadType = knownPayloadTypes->GetItem(j);

              if (mediaFormat->GetPayloadType() == knownPayloadType->GetId())
              {
                result = mediaFormat->SetName(knownPayloadType->GetEncodingName()) ? result : 0;
                mediaFormat->SetChannels((knownPayloadType->GetChannels() == PAYLOAD_TYPE_CHANNELS_VARIABLE) ? MEDIA_FORMAT_CHANNELS_UNSPECIFIED : knownPayloadType->GetChannels());
                mediaFormat->SetClockRate((knownPayloadType->GetClockRate() == PAYLOAD_TYPE_CLOCK_RATE_VARIABLE) ? MEDIA_FORMAT_CLOCK_RATE_UNSPECIFIED : knownPayloadType->GetClockRate());
              }
            }
          }
        }

        if (result != 0)
        {
          // try to find rtpmap attribute
          for (unsigned int j = 0; ((result != 0) && (j < this->attributes->Count())); j++)
          {
            CAttribute *attribute = this->attributes->GetItem(j);

            if (attribute->IsInstanceTag(TAG_ATTRIBUTE_INSTANCE_RTP_MAP))
            {
              CRtpMapAttribute *rtpMap = dynamic_cast<CRtpMapAttribute *>(attribute);
              result = (rtpMap != NULL) ? result : 0;

              if (result != 0)
              {
                if (rtpMap->GetPayloadType() == mediaFormat->GetPayloadType())
                {
                  // same payload type
                  result = mediaFormat->SetName(rtpMap->GetEncodingName()) ? result : 0;
                  mediaFormat->SetClockRate(rtpMap->GetClockRate());

                  if (this->IsAudio())
                  {
                    mediaFormat->SetChannels(GetValueUint(rtpMap->GetEncodingParameters(), MEDIA_FORMAT_CHANNELS_UNSPECIFIED));
                  }

                  // go to next media format
                  break;
                }
              }
            }
          }
        }
      }

      FREE_MEM_CLASS(knownPayloadTypes);
    }
  }

  return result;
}
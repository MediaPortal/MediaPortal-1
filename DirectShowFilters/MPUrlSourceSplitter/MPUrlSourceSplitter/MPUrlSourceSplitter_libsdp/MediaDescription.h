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

#ifndef __MEDIA_DESCRPTION_DEFINED
#define __MEDIA_DESCRPTION_DEFINED

#include "SessionTag.h"
#include "AttributeCollection.h"
#include "MediaFormatCollection.h"
#include "ConnectionData.h"

#define TAG_MEDIA_DESCRIPTION                                         L"m"

#define MEDIA_DESCRIPTION_MEDIA_TYPE_AUDIO                            L"audio"
#define MEDIA_DESCRIPTION_MEDIA_TYPE_VIDEO                            L"video"
#define MEDIA_DESCRIPTION_MEDIA_TYPE_APPLICATION                      L"application"
#define MEDIA_DESCRIPTION_MEDIA_TYPE_DATA                             L"data"
#define MEDIA_DESCRIPTION_MEDIA_TYPE_CONTROL                          L"control"

#define MEDIA_DESCRIPTION_PORT_DEFAULT                                0
#define MEDIA_DESCRIPTION_NUMBER_OF_PORTS_DEFAULT                     1

#define MEDIA_DESCRIPTION_TRANSPORT_PROTOCOL_RTP_AVP                  L"RTP/AVP"
#define MEDIA_DESCRIPTION_TRANSPORT_PROTOCOL_UDP                      L"udp"

#define MEDIA_DESCRIPTION_FLAG_NONE                                   SESSION_TAG_FLAG_NONE

#define MEDIA_DESCRIPTION_FLAG_MEDIA_TYPE_AUDIO                       (1 << (SESSION_TAG_FLAG_LAST + 0))
#define MEDIA_DESCRIPTION_FLAG_MEDIA_TYPE_VIDEO                       (1 << (SESSION_TAG_FLAG_LAST + 1))
#define MEDIA_DESCRIPTION_FLAG_MEDIA_TYPE_APPLICATION                 (1 << (SESSION_TAG_FLAG_LAST + 2))
#define MEDIA_DESCRIPTION_FLAG_MEDIA_TYPE_DATA                        (1 << (SESSION_TAG_FLAG_LAST + 3))
#define MEDIA_DESCRIPTION_FLAG_MEDIA_TYPE_CONTROL                     (1 << (SESSION_TAG_FLAG_LAST + 4))

#define MEDIA_DESCRIPTION_FLAG_TRANSPORT_PROTOCOL_RTP_AVP             (1 << (SESSION_TAG_FLAG_LAST + 5))
#define MEDIA_DESCRIPTION_FLAG_TRANSPORT_PROTOCOL_UDP                 (1 << (SESSION_TAG_FLAG_LAST + 6))

#define MEDIA_DESCRIPTION_FLAG_LAST                                   (SESSION_TAG_FLAG_LAST + 7)

class CMediaDescription : public CSessionTag
{
public:
  // initializes a new instance of CMediaDescription class
  CMediaDescription(HRESULT *result);
  virtual ~CMediaDescription(void);

  /* get methods */

  // gets media type
  // @return : media type or NULL if error
  virtual const wchar_t *GetMediaType(void);

  // gets port to which the media stream will be sent
  // @return : port to which the media stream will be sent
  virtual unsigned int GetPort(void);

  // gets number of ports used to transport (typically MEDIA_DESCRIPTION_NUMBER_OF_PORTS_DEFAULT)
  // @return : number of ports used to transport
  virtual unsigned int GetNumberOfPorts(void);

  // gets transport protocol
  // @return : transport protocol or NULL if error
  virtual const wchar_t *GetTransportProtocol(void);

  // gets attributes
  // @return : attributes collection
  virtual CAttributeCollection *GetAttributes(void);

  // gets media formats
  // @return : media formats collection
  virtual CMediaFormatCollection *GetMediaFormats(void);

  // gets connection data
  // @return : connection data or NULL if not specified or error
  virtual CConnectionData *GetConnectionData(void);

  /* set methods */

  /* other methods */

  // tests if media type is audio
  // @return : true if media type is audio, false otherwise
  virtual bool IsAudio(void);

  // tests if media type is video
  // @return : true if media type is video, false otherwise
  virtual bool IsVideo(void);

  // tests if media type is application
  // @return : true if media type is application, false otherwise
  virtual bool IsApplication(void);

  // tests if media type is data
  // @return : true if media type is data, false otherwise
  virtual bool IsData(void);

  // tests if media type is control
  // @return : true if media type is control, false otherwise
  virtual bool IsControl(void);

  // parses data in buffer
  // @param buffer : buffer with session tag data for parsing
  // @param length : the length of data in buffer
  // @return : return position in buffer after processing or 0 if not processed
  virtual unsigned int Parse(const wchar_t *buffer, unsigned int length);

  // clears current instance
  virtual void Clear(void);

protected:
  // holds media type
  wchar_t *mediaType;

  // holds transport port to which the media stream will be sent
  unsigned int port;

  // holds number of ports used to transport (typically MEDIA_DESCRIPTION_NUMBER_OF_PORTS_DEFAULT)
  unsigned int numberOfPorts;

  // holds transport protocol
  wchar_t *transportProtocol;

  // holds attributes
  CAttributeCollection *attributes;

  // holds media formats
  CMediaFormatCollection *mediaFormats;

  // holds connection data
  CConnectionData *connectionData;
};

#endif
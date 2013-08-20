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

#ifndef __MEDIA_DEFINED
#define __MEDIA_DEFINED

class CMedia
{
public:
  // constructor
  // create instance of CBootstrapInfo class
  CMedia(const wchar_t *url, unsigned int bitrate, unsigned int width, unsigned int height,
    const wchar_t *drmAdditionalHeaderId, const wchar_t *bootstrapInfoId, const wchar_t *dvrInfoId,
    const wchar_t *groupspec, const wchar_t *multicastStreamName, const wchar_t *metadata);

  // destructor
  ~CMedia(void);

  // gets url associated with piece of media
  // @return : the url or NULL if error
  const wchar_t *GetUrl(void);

  // gets media bitrate
  // @return : media bitrate or UINT_MAX if not specified
  unsigned int GetBitrate(void);

  // gets media width
  // @return : media width or UINT_MAX if not specified
  unsigned int GetWidth(void);

  // gets media height
  // @return : media height or UINT_MAX if not specified
  unsigned int GetHeight(void);

  // gets DRM additional header ID
  // @return : the DRM additional header ID or NULL if not specified
  const wchar_t *GetDrmAdditionalHeaderId(void);

  // gets bootstrap info ID
  // @return : the bootstrap info ID or NULL if not specified
  const wchar_t *GetBootstrapInfoId(void);

  // gets media dvr info ID
  // @return : the dvr info ID or NULL if not specified
  const wchar_t *GetDvrInfoId(void);

  // gets media group specifier for multicast media
  // @return : the media group specifier for multicast media or NULL if not specified
  const wchar_t *GetGroupspec(void);

  // gets stream name for multicast media
  // @return : the stream name for multicast media or NULL if not specified
  const wchar_t *GetMulticastStreamName(void);

  // gets media BASE64 encoded metadata
  // @return : media BASE64 encoded metadata or NULL if not specified
  const wchar_t *GetMetadata(void);

  // gets decoding result of BASE64 encoded metadata
  // @return : E_NOT_VALID_STATE if metadata is NULL or result from base64_decode() method
  HRESULT GetDecodeResult(void);

  // gets decoded metadata
  // @return : decoded metadata or NULL if error
  const unsigned char *GetDecodedMetadata(void);

  // gets decoded metadata length
  // @return : decoded metadata length, UINT_MAX if error
  unsigned int GetDecodedMetadataLength(void);

private:
  // stores media url
  wchar_t *url;
  // stores media bitrate
  unsigned int bitrate;
  // stores media width
  unsigned int width;
  // stores media height
  unsigned int height;
  // stores DRM additional header
  wchar_t *drmAdditionalHeaderId;
  // stores the ID of <bootstrapInfo> element
  wchar_t *bootstrapInfoId;
  // stores the ID of <dvrInfo> element
  wchar_t *dvrInfoId;
  // stores group specifier for multicast media
  wchar_t *groupspec;
  // stores stream name for multicast media
  wchar_t *multicastStreamName;
  // stores BASE64 encoded metadata
  wchar_t *metadata;

  // stores result of BASE64 decoding
  HRESULT decodeResult;
  // stores decoded metadata
  unsigned char *decodedMetadata;
  // stores length of decoded metadata
  unsigned int decodedLength;
};

#endif
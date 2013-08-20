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

#ifndef __F4M_MEDIA_DEFINED
#define __F4M_MEDIA_DEFINED

class CF4MMedia
{
public:
  // initializes a new instance of CF4MMedia class
  CF4MMedia(void);

  // destructor
  ~CF4MMedia(void);

  /* get methods */

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
  // @return : DRM additional header ID or NULL if not specified
  const wchar_t *GetDrmAdditionalHeaderId(void);

  // gets bootstrap info ID
  // @return : bootstrap info ID or NULL if not specified
  const wchar_t *GetBootstrapInfoId(void);

  // gets DVR info ID
  // @retunr : DVR info ID or NULL if not specified
  const wchar_t *GetDvrInfoId(void);

  // gets group specifier for multicast media
  // @return : group specifier for multicast media or NULL if not specified
  const wchar_t *GetGroupSpecifier(void);

  // gets stream name for multicast media
  // @return : stream name for multicast media or NULL if not specified
  const wchar_t *GetMulticastStreamName(void);

  // gets BASE64 encoded stream metadata
  // @return : BASE64 encoded stream metadata or NULL if not specified
  const wchar_t *GetMetadata(void);

  // gets media URL
  // @return : media URL or NULL if not specified
  const wchar_t *GetUrl(void);

  /* set methods */

  // sets media bitrate
  // @param bitrate : media bitrate or UINT_MAX if not specified
  // @return : true if successful, false otherwise
  bool SetBitrate(unsigned int bitrate);

  // sets media width
  // @param width : media width or UINT_MAX if not specified
  // @return : true if successful, false otherwise
  bool SetWidth(unsigned int width);

  // sets media height
  // @param height : media height or UINT_MAX if not specified
  // @return : true if successful, false otherwise
  bool SetHeight(unsigned int height);

  // sets DRM additional header ID
  // @param drmAdditionalHeaderId : DRM additional header ID or NULL if not specified
  // @return : true if successful, false otherwise
  bool SetDrmAdditionalHeaderId(const wchar_t *drmAdditionalHeaderId);

  // sets bootstrap info ID
  // @param bootstrapInfoId : bootstrap info ID or NULL if not specified
  // @return : true if successful, false otherwise
  bool SetBootstrapInfoId(const wchar_t *bootstrapInfoId);

  // sets DVR info ID
  // @param dvrInfoId : DVR info ID or NULL if not specified
  // @return : true if successful, false otherwise
  bool SetDvrInfoId(const wchar_t *dvrInfoId);

  // sets group specifier for multicast media or NULL if not specified
  // @param groupSpecifier : group specifier for multicast media
  // @return : true if successful, false otherwise
  bool SetGroupSpecifier(const wchar_t *groupSpecifier);

  // sets stream name for multicast media or NULL if not specified
  // @param multicastStreamName : stream name for multicast media
  // @return : true if successful, false otherwise
  bool SetMulticastStreamName(const wchar_t *multicastStreamName);

  // sets BASE64 encoded stream metadata
  // @param metadata : BASE64 encoded stream metadata or NULL if not specified
  // @return : true if successful, false otherwise
  bool SetMetadata(const wchar_t *metadata);

  // sets media URL
  // @param url : media URL to set
  // @return : true if successful, false otherwise
  bool SetUrl(const wchar_t *url);

private:
  // store media bitrate
  unsigned int bitrate;
  // stores media width
  unsigned int width;
  // stores media height
  unsigned int height;
  // stores DRM additional header ID
  wchar_t *drmAdditionalHeaderId;
  // stores bootstrap info ID
  wchar_t *bootstrapInfoId;
  // stores DVR info ID
  wchar_t *dvrInfoId;
  // stores group specifier for multicast media
  wchar_t *groupSpecifier;
  // stores multicast stream name
  wchar_t *multicastStreamName;
  // stores BASE64 encoded metadata
  wchar_t *metadata;
  // stores media url
  wchar_t *url;
};

#endif
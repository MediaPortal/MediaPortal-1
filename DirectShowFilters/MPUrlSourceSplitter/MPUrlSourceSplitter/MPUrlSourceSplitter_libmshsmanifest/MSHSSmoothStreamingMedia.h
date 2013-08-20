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

#ifndef __MSHS_SMOOTH_STREAMIN_MEDIA_DEFINED
#define __MSHS_SMOOTH_STREAMIN_MEDIA_DEFINED

#include "MSHSProtectionCollection.h"
#include "MSHSStreamCollection.h"
#include "Serializable.h"

#define MANIFEST_MAJOR_VERSION                                                2
#define MANIFEST_MINOR_VERSION                                                0
#define MANIFEST_TIMESCALE_DEFAULT                                            10000000

class CMSHSSmoothStreamingMedia :
  public CSerializable
{
public:
  // creates new instance of CMSHSSmoothStreamingMedia class
  CMSHSSmoothStreamingMedia(void);

  // destructor
  ~CMSHSSmoothStreamingMedia(void);

  /* get methods */

  // gets manifest major version (must be 2)
  // @return : major version
  uint32_t GetMajorVersion(void);

  // gets manifest minor version (must be 0)
  // @return : minor version
  uint32_t GetMinorVersion(void);

  // gets time scale of the duration, specified as the number of increments in one second
  // the default value is MANIFEST_TIMESCALE_DEFAULT
  // @return : time scale of duration
  uint64_t GetTimeScale(void);

  // gets duration of the presentation, specified as the number of time increments indicated by the value of the time scale
  // @return : duration of presentation
  uint64_t GetDuration(void);

  // gets protections applicable to streams
  // @return : protections
  CMSHSProtectionCollection *GetProtections(void);

  // gets streams
  // @return : streams
  CMSHSStreamCollection *GetStreams(void);

  /* set methods */

  // sets major version
  // @param majorVersion : major version to set
  void SetMajorVersion(uint32_t majorVersion);

  // sets minor version
  // @param minorVersion : minor version to set
  void SetMinorVersion(uint32_t minorVersion);

  // sets time scale
  // @param timeScale : time scale to set
  void SetTimeScale(uint64_t timeScale);

  // sets duration
  // @param duration : duration to set
  void SetDuration(uint64_t duration);

  /* other methods*/

  // tests if media is protected
  // @return : true if protected, false otherwise
  bool IsProtected(void);

  // gets necessary buffer length for serializing instance
  // @return : necessary size for buffer
  virtual uint32_t GetSerializeSize(void);

  // serialize instance into buffer, buffer must be allocated before and must have necessary size
  // @param buffer : buffer which stores serialized instance
  // @return : true if successful, false otherwise
  virtual bool Serialize(uint8_t *buffer);

  // deserializes instance
  // @param : buffer which stores serialized instance
  // @return : true if successful, false otherwise
  virtual bool Deserialize(const uint8_t *buffer);

private:
  uint32_t majorVersion;

  uint32_t minorVersion;

  uint64_t timeScale;

  uint64_t duration;

  CMSHSProtectionCollection *protections;

  CMSHSStreamCollection *streams;
};

#endif
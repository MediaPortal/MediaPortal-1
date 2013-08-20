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

#ifndef __TIME_TO_SAMPLE_DEFINED
#define __TIME_TO_SAMPLE_DEFINED

#include <stdint.h>

class CTimeToSample
{
public:
  // initializes a new instance of CTimeToSample class
  CTimeToSample(void);

  // destructor
  ~CTimeToSample(void);

  /* get methods */

  // gets counts the number of consecutive samples that have the given duration
  // @return : counts the number of consecutive samples that have the given duration
  virtual uint32_t GetSampleCount(void);

  // gets the delta of these samples in the time-scale of the media
  // @return : the delta of these samples in the time-scale of the media
  virtual uint32_t GetSampleDelta(void);

  /* set methods */

  // sets counts the number of consecutive samples that have the given duration
  // @param sampleCount : counts the number of consecutive samples to set that have the given duration
  void SetSampleCount(uint32_t sampleCount);

  // the delta of these samples in the time-scale of the media
  // @param sampleDelta : the delta of these samples in the time-scale of the media to set
  void SetSampleDelta(uint32_t sampleDelta);

  /* other methods */

protected:

  // counts the number of consecutive samples that have the given duration
  uint32_t sampleCount;

  // the delta of these samples in the time-scale of the media
  uint32_t sampleDelta;
};

#endif
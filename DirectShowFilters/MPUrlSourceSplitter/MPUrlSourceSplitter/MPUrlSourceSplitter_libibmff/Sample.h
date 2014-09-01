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

#ifndef __SAMPLE_DEFINED
#define __SAMPLE_DEFINED

#include <stdint.h>

class CSample
{
public:
  // initializes a new instance of CSample class
  CSample(HRESULT *result);

  // destructor
  ~CSample(void);

  /* get methods */

  // gets sample duration
  // @return : sample duration
  virtual uint32_t GetSampleDuration(void);

  // gets sample size
  // @return : sample size
  virtual uint32_t GetSampleSize(void);

  // gets sample flags
  // @return : sample flags
  virtual uint32_t GetSampleFlags(void);

  // gets sample composition time offset
  // @return : sample composition time offset
  virtual uint32_t GetSampleCompositionTimeOffset(void);

  /* set methods */

  // sets sample duration
  // @param sampleDuration : the sample duration to set
  virtual void SetSampleDuration(uint32_t sampleDuration);

  // sets sample size
  // @param sampleSize : the sample size to set
  virtual void SetSampleSize(uint32_t sampleSize);

  // sets sample flags
  // @param sampleFlags : the sample flags to set
  virtual void SetSampleFlags(uint32_t sampleFlags);

  // sets sample composition time offset
  // @param sampleCompositionTimeOffset : the sample composition time offset to set
  virtual void SetSampleCompositionTimeOffset(uint32_t sampleCompositionTimeOffset);

  /* other methods */

protected:

  uint32_t sampleDuration;

  uint32_t sampleSize;

  uint32_t sampleFlags;

  uint32_t sampleCompositionTimeOffset;  
};

#endif
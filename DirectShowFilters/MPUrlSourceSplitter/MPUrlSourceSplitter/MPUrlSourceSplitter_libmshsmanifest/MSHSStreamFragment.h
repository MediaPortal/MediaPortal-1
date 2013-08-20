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

#ifndef __MSHS_STREAM_FRAGMENT_DEFINED
#define __MSHS_STREAM_FRAGMENT_DEFINED

#include "Serializable.h"

#include <stdint.h>

class CMSHSStreamFragment : public CSerializable
{
public:
  // creats new instance of CMSHSStreamFragment class
  CMSHSStreamFragment(void);

  // desctructor
  ~CMSHSStreamFragment(void);

  /* get methods */

  // gets fragment number
  // @return : fragment number
  uint32_t GetFragmentNumber(void);

  // gets fragment duration
  // @return : fragment duration
  uint64_t GetFragmentDuration(void);

  // gets fragment time
  // @return : fragment time
  uint64_t GetFragmentTime(void);

  /* set methods */

  // sets fragment number
  // @param fragmentNumber : fragment number to set
  void SetFragmentNumber(uint32_t fragmentNumber);

  // gets fragment duration
  // @param fragmentDuration : fragment duration to set
  void SetFragmentDuration(uint64_t fragmentDuration);

  // sets fragment time
  // @param fragmentTime : fragment time to set
  void SetFragmentTime(uint64_t fragmentTime);

  /* other methods */

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

  // ordinal of the stream fragment element in the Stream
  // if FragmentNumber is specified, its value MUST monotonically increase with the value of FragmentTime
  uint32_t fragmentNumber;

  // duration of the Fragment, specified as a number of increments defined by the implicit or explicit value
  // of the containing StreamElement's StreamTimeScale field
  // if the FragmentDuration field is omitted, its implicit value MUST be computed by the Client by subtracting
  // the value of the preceding StreamFragmentElement's FragmentTime field from the value of this StreamFragmentElement's
  // FragmentTime field
  // if no subsequent StreamFragmentElement exists, the implicit value of FragmentTime is 0
  uint64_t fragmentDuration;

  // time of the Fragment, specified as a number of increments defined by the implicit or explicit value
  // of the containing StreamElement's StreamTimeScale field
  // if the FragmentDuration field is omitted, its implicit value MUST be computed by the Client by adding the value
  // of the preceding StreamFragmentElement's FragmentTime field to the value of this StreamFragmentElement's
  // FragmentDuration field
  // if no preceding StreamFragmentElement exists, the implicit value of FragmentTime is 0
  uint64_t fragmentTime;
  
};

#endif
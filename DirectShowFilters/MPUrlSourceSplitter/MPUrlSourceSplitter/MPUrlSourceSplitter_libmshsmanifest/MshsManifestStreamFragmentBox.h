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

#ifndef __MSHS_MANIFEST_STREAM_FRAGMENT_BOX_DEFINED
#define __MSHS_MANIFEST_STREAM_FRAGMENT_BOX_DEFINED

#include "Box.h"

#include <stdint.h>

#define MSHS_MANIFEST_STREAM_FRAGMENT_BOX_TYPE                        L"mssf"

#define MSHS_MANIFEST_STREAM_FRAGMENT_BOX_FLAG_NONE                   BOX_FLAG_NONE

#define MSHS_MANIFEST_STREAM_FRAGMENT_BOX_FLAG_LAST                   (BOX_FLAG_LAST + 0)

class CMshsManifestStreamFragmentBox : public CBox
{
public:
  // creats new instance of CMshsManifestStreamFragmentBox class
  CMshsManifestStreamFragmentBox(HRESULT *result);
  // desctructor
  virtual ~CMshsManifestStreamFragmentBox(void);

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

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @return : true if all data were successfully stored into buffer, false otherwise
  virtual bool GetBox(uint8_t *buffer, uint32_t length);

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

  // gets box data in human readable format
  // @param indent : string to insert before each line
  // @return : box data in human readable format or NULL if error
  virtual wchar_t *GetParsedHumanReadable(const wchar_t *indent);

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
  
  /* methods */

  // gets whole box size
  // method is called to determine whole box size for storing box into buffer
  // @return : size of box 
  virtual uint64_t GetBoxSize(void);

  // parses data in buffer
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @param processAdditionalBoxes : specifies if additional boxes have to be processed
  // @return : true if parsed successfully, false otherwise
  virtual bool ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes);

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @param processAdditionalBoxes : specifies if additional boxes have to be processed (added to buffer)
  // @return : number of bytes stored into buffer, 0 if error
  virtual uint32_t GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes);
};

#endif
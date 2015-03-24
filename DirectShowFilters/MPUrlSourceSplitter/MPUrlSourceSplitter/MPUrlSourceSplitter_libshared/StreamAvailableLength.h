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

#ifndef __STREAM_AVAILABLE_LENGTH_DEFINED
#define __STREAM_AVAILABLE_LENGTH_DEFINED

class CStreamAvailableLength
{
public:
  CStreamAvailableLength(void);
  ~CStreamAvailableLength(void);

  /* get methods */

  // gets available length
  // @return : the available length value
  LONGLONG GetAvailableLength(void);

  // gets stream ID to get stream available length
  // @return : stream ID to get stream available length
  unsigned int GetStreamId(void);

  /* set methods */

  // sets available length
  // @param availableLength : the available length value
  void SetAvailableLength(LONGLONG availableLength);

  // sets stream ID to get stream available length
  // @param streamId : the stream ID to set
  void SetStreamId(unsigned int streamId);

  /* other methods */

private:
  unsigned int streamId;
  LONGLONG availableLength;
};

#endif
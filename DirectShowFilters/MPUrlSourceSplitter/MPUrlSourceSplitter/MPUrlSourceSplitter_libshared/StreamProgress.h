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

#ifndef __STREAM_PROGRESS_DEFINED
#define __STREAM_PROGRESS_DEFINED

class CStreamProgress
{
public:
  CStreamProgress(void);
  ~CStreamProgress(void);

  /* get methods */

  // gets total length
  // @return : the total length value
  LONGLONG GetTotalLength(void);

  // gets current length
  // @return : the current length value
  LONGLONG GetCurrentLength(void);

  // gets stream ID to get stream progress
  // @return : stream ID to get stream progress
  unsigned int GetStreamId(void);

  /* set methods */

  // sets total length
  // @param totalLength : the total length to set
  void SetTotalLength(LONGLONG totalLength);

  // sets current length
  // @param currentLength : the current length to set
  void SetCurrentLength(LONGLONG currentLength);

  // sets stream ID to get stream progress
  // @param streamId : the stream ID to set
  void SetStreamId(unsigned int streamId);

  /* other methods */

private:
  unsigned int streamId;
  LONGLONG total;
  LONGLONG current;
};

#endif
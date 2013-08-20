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

#ifndef __STREAM_DEFINED
#define __STREAM_DEFINED

#include "StreamInfo.h"

class CStream
{
public:
  CStream(void);
  ~CStream(void);

  /* get methods */

  // gets stream info associated with stream
  // @return : stream info or NULL if error
  CStreamInfo *GetStreamInfo(void);

  // gets stream PID
  // @return : stream PID
  unsigned int GetPid(void);

  // gets stream language
  // @return : stream language or NULL if error
  const wchar_t *GetLanguage(void);

  /* set methods */

  // sets stream PID
  // @param pid : the stream PID to set
  void SetPid(unsigned int pid);

  // sets stream language
  // @param language : the language to set
  // @return : true if successful, false otherwise
  bool SetLanguage(const wchar_t *language);

  /* other methods */

  // creates stream info
  // @return : S_OK if successful, error code otherwise
  HRESULT CreateStreamInfo(void);

  // creates stream info
  // @return : S_OK if successful, error code otherwise
  HRESULT CreateStreamInfo(AVFormatContext *formatContext, AVStream *stream, const wchar_t *containerFormat);

protected:

  // holds stream info
  CStreamInfo *streamInfo;

  unsigned int pid;

  wchar_t *language;
};



#endif
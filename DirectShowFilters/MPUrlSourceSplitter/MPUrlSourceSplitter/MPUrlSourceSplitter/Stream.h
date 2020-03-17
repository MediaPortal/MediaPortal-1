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

#include "Flags.h"
#include "StreamInfo.h"
#include "SeekIndexEntryCollection.h"

#define STREAM_FLAG_NONE                                              FLAGS_NONE

#define STREAM_FLAG_DISCONTINUITY                                     (1 << (FLAGS_LAST + 0))

#define STREAM_FLAG_LAST                                              (FLAGS_LAST + 1)

class CStream : public CFlags
{
public:
  enum StreamType { Video, Audio, Subpic, Unknown };

  CStream(HRESULT *result);
  ~CStream(void);

  /* get methods */

  // gets stream info associated with stream
  // @return : stream info or NULL if error
  CStreamInfo *GetStreamInfo(void);

  // gets stream PID in demuxer
  // @return : stream PID in demuxer
  unsigned int GetPid(void);

  // gets stream language
  // @return : stream language or NULL if error
  const wchar_t *GetLanguage(void);

  // gets stream type
  // @return : stream type
  StreamType GetStreamType(void);

  // gets seek index entry collection
  // @return : seek index entry collection
  CSeekIndexEntryCollection *GetSeekIndexEntries(void);

  /* set methods */

  // sets stream PID in demuxer
  // @param pid : the stream PID in demuxer to set
  void SetPid(unsigned int pid);

  // sets stream language
  // @param language : the language to set
  // @return : true if successful, false otherwise
  bool SetLanguage(const wchar_t *language);

  // sets stream type
  // @param streamType : stream type to set
  void SetStreamType(StreamType streamType);

  // sets discontinuity flag
  // @param discontinuity : true if discontinuity, false otherwise
  void SetDiscontinuity(bool discontinuity);

  /* other methods */

  // tests if discontinuity flag is set
  // @return : true if discontinuity flag is set, false otherwise
  bool IsDiscontinuity(void);

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

  StreamType streamType;

  // holds seek index entries
  CSeekIndexEntryCollection *seekIndexEntries;
};



#endif
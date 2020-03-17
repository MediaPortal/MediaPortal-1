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

#ifndef __FFMPEG_CONTEXT_DEFINED
#define __FFMPEG_CONTEXT_DEFINED

#include "Flags.h"

#define FFMPEG_CONTEXT_FLAG_NONE                                      FLAGS_NONE

#define FFMPEG_CONTEXT_FLAG_LAST                                      (FLAGS_LAST + 0)

class CFFmpegLogger;
class CFFmpegContext;

#define METHOD_FFMPEG_LOG_NAME                                        L"FFmpegLog()"

struct IFFmpegLog
{
public:
  virtual bool FFmpegLog(CFFmpegLogger *ffmpegLogger, CFFmpegContext *context, void *ffmpegPtr, int ffmpegLogLevel, const char *ffmpegFormat, va_list ffmpegList) = 0;
};

class CFFmpegContext : public CFlags
{
public:
  CFFmpegContext(HRESULT *result, IFFmpegLog *ffmpegLog);
  virtual ~CFFmpegContext(void);

  /* get methods */

  // gets FFmpeg log callback
  // @return : FFmpeg log callback
  virtual IFFmpegLog *GetFFmpegLog(void);

  /* set methods */

  /* other methods */

protected:
  // holds log callback
  IFFmpegLog *ffmpegLog;

  /* methods */
};

#endif
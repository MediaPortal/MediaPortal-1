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

#ifndef __FFMPEG_LOGGER_DEFINED
#define __FFMPEG_LOGGER_DEFINED

#include "Logger.h"
#include "FFmpegContextCollection.h"

class CFFmpegLogger
{
public:
  CFFmpegLogger(HRESULT *result, CStaticLogger *staticLogger);
  virtual ~CFFmpegLogger(void);

  /* get methods */

  // gets formatted FFmpeg message
  // caller is responsible for freeinf memory
  // @param ffmpegFormat : string format supplied by FFmpeg
  // @param ffmpegList : list of parameters
  // @return : FFmpeg formatted message or NULL if error
  wchar_t *GetFFmpegMessage(const char *ffmpegFormat, va_list ffmpegList);

  /* set methods */

  /* other methods */

  // log message to log file
  // @param logLevel : the log level of message
  // @param format : the formating string
  void Log(unsigned int logLevel, const wchar_t *format, ...);

  // registers FFmpeg context
  // @param ffmpegContext : the FFmpeg context to register
  // @return : true if successful, false otherwise
  bool RegisterFFmpegContext(CFFmpegContext *ffmpegContext);

  // unregisters FFmpeg context
  // @param ffmpegContext : the FFmpeg context to unregister
  void UnregisterFFmpegContext(CFFmpegContext *ffmpegContext);

protected:
  // holds instance of static logger
  CStaticLogger *staticLogger;
  // holds registered FFmpeg contexts
  CFFmpegContextCollection *contexts;
  // holds locking mutex
  HANDLE mutex;

  /* methods */

  // get human-readable value of log level
  // @param level : the level of message
  // @return : the human-readable log level
  static const wchar_t *GetLogLevel(unsigned int level);

  wchar_t *GetLogMessage(unsigned int logLevel, const wchar_t *format, va_list vl);

  void LogMessage(unsigned int logLevel, const wchar_t *message);
  void Log(unsigned int logLevel, const wchar_t *format, va_list vl);

  // FFmpeg log callback
  static void LogCallback(void *ptr, int log_level, const char *format, va_list vl);
};

#endif
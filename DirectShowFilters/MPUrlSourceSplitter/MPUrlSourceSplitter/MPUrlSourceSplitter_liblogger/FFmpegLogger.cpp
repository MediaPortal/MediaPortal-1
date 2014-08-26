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

#include "StdAfx.h"

#include "FFmpegLogger.h"
#include "LockMutex.h"
#include "StaticLogger.h"

#include <crtdbg.h>

#pragma warning(push)
#pragma warning(disable:4244)
extern "C" {
#define __STDC_CONSTANT_MACROS
#include "libavformat/avformat.h"
#include "libavutil/intreadwrite.h"
#include "libavutil/pixdesc.h"
}
#pragma warning(pop)

extern "C++" CFFmpegLogger *ffmpegLogger;

CFFmpegLogger::CFFmpegLogger(HRESULT *result, CStaticLogger *staticLogger)
{
  this->staticLogger = NULL;
  this->mutex = NULL;
  this->contexts = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, staticLogger);

    if (SUCCEEDED(*result))
    {
      this->staticLogger = staticLogger;
      this->mutex = CreateMutex(NULL, FALSE, NULL);
      this->contexts = new CFFmpegContextCollection(result);

      CHECK_POINTER_HRESULT(*result, this->mutex, *result, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*result, this->contexts, *result, E_OUTOFMEMORY);

      if (SUCCEEDED(*result))
      {
        // initialize FFmpeg
        av_register_all();

        // callback for FFmpeg log is not set
        av_log_set_callback(LogCallback);
        av_log_set_level(AV_LOG_DEBUG);
      }
    }
  }
}

CFFmpegLogger::~CFFmpegLogger(void)
{
  FREE_MEM_CLASS(this->contexts);

  if (this->mutex != NULL)
  {
    CloseHandle(this->mutex);
  }
  this->mutex = NULL;
}

/* get methods */

void InvalidParameterHandler(const wchar_t *expression, const wchar_t *function, const wchar_t *file, unsigned int line, uintptr_t pReserved)
{
  // in release it doesn't output any valuable information
#ifdef _DEBUG
  if (ffmpegLogger != NULL)
  {
    ffmpegLogger->Log(LOGGER_VERBOSE, L"%s: %s: invalid parameter detected in function '%s', file '%s', line %d.\nExpression: %s", "FFmpegLogger", L"InvalidParameterHandler()", function, file, line, expression);
  }
#endif
}

wchar_t *CFFmpegLogger::GetFFmpegMessage(const char *ffmpegFormat, va_list ffmpegList)
{
  wchar_t *result = NULL;

  int warnReportMode = _CrtSetReportMode(_CRT_WARN, 0);
  int errorReportMode = _CrtSetReportMode(_CRT_ERROR, 0);
  int assertReportMode = _CrtSetReportMode(_CRT_ASSERT, 0);

  _invalid_parameter_handler previousHandler = _set_invalid_parameter_handler(InvalidParameterHandler);

  int length = _vscprintf(ffmpegFormat, ffmpegList) + 1;
  ALLOC_MEM_DEFINE_SET(buffer, char, length, 0);
  if (buffer != NULL)
  {
    if (vsprintf_s(buffer, length, ffmpegFormat, ffmpegList) != (-1))
    {
      char *trimmed = TrimA(buffer);
      if (trimmed != NULL)
      {
        result = ConvertToUnicodeA(trimmed);
      }
      FREE_MEM(trimmed);
    }
  }

  FREE_MEM(buffer);

  // set original values for error messages back
  _set_invalid_parameter_handler(previousHandler);

  _CrtSetReportMode(_CRT_WARN, warnReportMode);
  _CrtSetReportMode(_CRT_ERROR, errorReportMode);
  _CrtSetReportMode(_CRT_ASSERT, assertReportMode);

  return result;
}

/* set methods */

/* other methods */

void CFFmpegLogger::Log(unsigned int logLevel, const wchar_t *format, ...)
{
  va_list vl;
  va_start(vl, format);

  this->Log(logLevel, format, vl);

  va_end(vl);
}

bool CFFmpegLogger::RegisterFFmpegContext(CFFmpegContext *ffmpegContext)
{
  bool result = false;

  {
    CLockMutex lock(this->mutex, INFINITE);

    result |= this->contexts->Add(ffmpegContext);
  }

  return result;
}

void CFFmpegLogger::UnregisterFFmpegContext(CFFmpegContext *ffmpegContext)
{
  CLockMutex lock(this->mutex, INFINITE);

  for (unsigned int i = 0; i < this->contexts->Count(); i++)
  {
    CFFmpegContext *context = this->contexts->GetItem(i);

    if (context == ffmpegContext)
    {
      this->contexts->Remove(i);
      break;
    }
  }
}

/* protected methods */

const wchar_t *CFFmpegLogger::GetLogLevel(unsigned int level)
{
  switch(level)
  {
  case LOGGER_NONE:
    return L"         ";
  case LOGGER_ERROR:
    return L"[Error]  ";
  case LOGGER_WARNING:
    return L"[Warning]";
  case LOGGER_INFO:
    return L"[Info]   ";
  case LOGGER_VERBOSE:
    return L"[Verbose]";
  default:
    return L"         ";
  }
}

wchar_t *CFFmpegLogger::GetLogMessage(unsigned int logLevel, const wchar_t *format, va_list vl)
{
  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);

  int length = _vscwprintf(format, vl) + 1;
  ALLOC_MEM_DEFINE_SET(buffer, wchar_t, length, 0);
  if (buffer != NULL)
  {
    vswprintf_s(buffer, length, format, vl);
  }

  wchar_t *guid = ConvertGuidToString(GUID_NULL);

  wchar_t *logRow = FormatString(L"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%4x] [%s] %s %s\r\n",
    systemTime.wDay, systemTime.wMonth, systemTime.wYear,
    systemTime.wHour, systemTime.wMinute, systemTime.wSecond,
    systemTime.wMilliseconds,
    GetCurrentThreadId(),
    guid,
    CFFmpegLogger::GetLogLevel(logLevel),
    buffer);

  FREE_MEM(guid);
  FREE_MEM(buffer);

  return logRow;

  return NULL;
}

void CFFmpegLogger::LogMessage(unsigned int logLevel, const wchar_t *message)
{
  this->staticLogger->LogMessage(LOGGER_CONTEXT_INVALID_HANDLE, logLevel, message);
}

void CFFmpegLogger::Log(unsigned int logLevel, const wchar_t *format, va_list vl)
{
  wchar_t *logRow = this->GetLogMessage(logLevel, format, vl);

  if (logRow != NULL)
  {
    this->LogMessage(logLevel, logRow);
    FREE_MEM(logRow);
  }
}

void CFFmpegLogger::LogCallback(void *ptr, int log_level, const char *format, va_list vl)
{
  CLockMutex(ffmpegLogger->mutex, INFINITE);

  bool logged = false;

  for (unsigned int i = 0; ((!logged) && (i < ffmpegLogger->contexts->Count())); i++)
  {
    CFFmpegContext *context = ffmpegLogger->contexts->GetItem(i);

    logged |= context->GetFFmpegLog()->FFmpegLog(ffmpegLogger, context, ptr, log_level, format, vl);
  }

  if (!logged)
  {
    // log through default FFmpeg logger

    wchar_t *message = ffmpegLogger->GetFFmpegMessage(format, vl);

    if (message != NULL)
    {
      ffmpegLogger->Log(LOGGER_VERBOSE, L"%s: %s: FFmpeg unknown source, log level: %d, message: '%s'", L"FFmpegLogger", L"LogCallback()", log_level, message);
    }

    FREE_MEM(message);
  }
}
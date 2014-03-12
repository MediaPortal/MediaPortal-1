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

// dllmain.cpp : Defines the entry point for the DLL application.

#pragma warning(push)
// disable warning: 'INT8_MIN' : macro redefinition
// warning is caused by stdint.h and intsafe.h, which both define same macro
#pragma warning(disable:4005)

#include "stdafx.h"

#include <InitGuid.h>
#include "moreuuids.h"

#define wszIptvSource                                           L"MediaPortal IPTV Source Filter"
#define wszUrlSourceSplitter                                    L"MediaPortal Url Source Splitter"

#include "MPUrlSourceSplitter.h"
#include "StaticLogger.h"
#include <curl/curl.h>

#include <dbghelp.h>
#include <Shlwapi.h>

#pragma warning(pop)

extern "C++" CLogger *ffmpegLoggerInstance;
extern "C++" CStaticLogger *staticLogger = NULL;

// holds reference to exception handler returned in registration
PVOID exceptionHandler = NULL;
// exception handler for any unhandled exception in process
static LONG WINAPI ExceptionHandler(struct _EXCEPTION_POINTERS *exceptionInfo);

// Filter setup data
const AMOVIESETUP_MEDIATYPE sudIptvMediaTypes[] =
{
  { &MEDIATYPE_Stream, &MEDIASUBTYPE_MPEG2_TRANSPORT }        // for MP IPTV source
};

const AMOVIESETUP_MEDIATYPE sudUrlSourceSplitterMediaTypes[] =
{
  { &MEDIATYPE_Stream, &MEDIASUBTYPE_NULL }                   // for MP Url Source Splitter
};

const AMOVIESETUP_PIN sudIptvOutputPins[] = 
{
  {
    L"Output",            // pin name
      FALSE,              // is rendered?    
      TRUE,               // is output?
      FALSE,              // zero instances allowed?
      FALSE,              // many instances allowed?
      &CLSID_NULL,        // connects to filter (for bridge pins)
      NULL,               // connects to pin (for bridge pins)
      0,                  // count of registered media types
      NULL                // list of registered media types
  } // for MP IPTV source
};

const AMOVIESETUP_PIN sudUrlSourceSplitterOutputPins[] = 
{
  {
    L"Output",            // pin name
      FALSE,              // is rendered?    
      TRUE,               // is output?
      FALSE,              // zero instances allowed?
      TRUE,               // many instances allowed?
      &CLSID_NULL,        // connects to filter (for bridge pins)
      NULL,               // connects to pin (for bridge pins)
      0,                  // count of registered media types
      NULL                // list of registered media types
  } // for MP Url Source Splitter
};

// for MP IPTV source
const AMOVIESETUP_FILTER sudIptvFilterReg =
{
  &GUID_MP_IPTV_SOURCE,             // filter clsid
  wszIptvSource,                    // filter name
  MERIT_NORMAL,                     // merit
  1,                                // count of registered pins
  sudIptvOutputPins,                // list of pins to register
  CLSID_LegacyAmFilterCategory      // this parameter is not included in Windows 7.1 SDK
};

// for MP Url Source Splitter
const AMOVIESETUP_FILTER sudUrlSourceSplitterFilterReg =
{
  &GUID_MP_URL_SOURCE_SPLITTER,     // filter clsid
  wszUrlSourceSplitter,             // filter name
  MERIT_NORMAL,                     // merit
  1,                                // count of registered pins
  sudUrlSourceSplitterOutputPins,   // list of pins to register
  CLSID_LegacyAmFilterCategory      // this parameter is not included in Windows 7.1 SDK
};

// List of class IDs and creator functions for the class factory. This
// provides the link between the OLE entry point in the DLL and an object
// being created. The class factory will call the static CreateInstance.
// We provide a set of filters in this one DLL.

CFactoryTemplate g_Templates[] = 
{
  { 
    wszIptvSource,                                      // Name
    sudIptvFilterReg.clsID,                             // CLSID
    CMPUrlSourceSplitter::CreateInstanceIptvSource,     // Method to create an instance
    NULL,                                               // Initialization function
    &sudIptvFilterReg                                   // Set-up information (for filters)
  }, // for MP IPTV source

  { 
    wszUrlSourceSplitter,                                     // Name
    sudUrlSourceSplitterFilterReg.clsID,                      // CLSID
    CMPUrlSourceSplitter::CreateInstanceUrlSourceSplitter,    // Method to create an instance
    NULL,                                                     // Initialization function
    &sudUrlSourceSplitterFilterReg                            // Set-up information (for filters)
  } // for MP Url Source Splitter
};

int g_cTemplates = sizeof(g_Templates) / sizeof(g_Templates[0]);

extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);

STDAPI DllRegisterServer()
{
  return AMovieDllRegisterServer2(TRUE);
}

STDAPI DllUnregisterServer()
{
  return AMovieDllRegisterServer2(FALSE);
}

BOOL APIENTRY DllMain(HMODULE hModule,
  DWORD  ul_reason_for_call,
  LPVOID lpReserved
  )
{
  switch (ul_reason_for_call)
  {
  case DLL_PROCESS_ATTACH:
    {
      if (exceptionHandler == NULL)
      {
        // register exception handler
        exceptionHandler = AddVectoredExceptionHandler(1, ExceptionHandler);
      }

      staticLogger = new CStaticLogger();
      curl_global_init(CURL_GLOBAL_ALL);
    }
    break;
  case DLL_THREAD_ATTACH:
    break;
  case DLL_THREAD_DETACH:
    break;
  case DLL_PROCESS_DETACH:
    {
      if (exceptionHandler != NULL)
      {
        RemoveVectoredExceptionHandler(exceptionHandler);
        exceptionHandler = NULL;
      }

      // free FFmpeg logger instance
      FREE_MEM_CLASS(ffmpegLoggerInstance);
      FREE_MEM_CLASS(staticLogger);
      curl_global_cleanup();
    }
    break;
  }

  return DllEntryPoint((HINSTANCE)(hModule), ul_reason_for_call, lpReserved);
}

LONG WINAPI ExceptionHandler(struct _EXCEPTION_POINTERS *exceptionInfo)
{
  // we received some unhandled exception
  // flush logs and continue with processing exception

  // by ntstatus.h:

  //
  //  Values are 32 bit values laid out as follows:
  //
  //   3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1
  //   1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
  //  +---+-+-+-----------------------+-------------------------------+
  //  |Sev|C|R|     Facility          |               Code            |
  //  +---+-+-+-----------------------+-------------------------------+
  //
  //  where
  //
  //      Sev - is the severity code
  //
  //          00 - Success
  //          01 - Informational
  //          10 - Warning
  //          11 - Error
  //
  //      C - is the Customer code flag (0 for Microsoft errors, 1 for custom errors)
  //
  //      R - is a reserved bit
  //
  //      Facility - is the facility code
  //
  //      Code - is the facility's status code
  //
  // we care only about errors
  if (((exceptionInfo->ExceptionRecord->ExceptionCode & 0xF0000000) == 0xC0000000) &&
       (exceptionHandler != NULL))
  {
    // remove exception handler
    RemoveVectoredExceptionHandler(exceptionHandler);
    exceptionHandler = NULL;

    if (staticLogger != NULL)
    {
      SYSTEMTIME currentLocalTime;
      MINIDUMP_EXCEPTION_INFORMATION minidumpException;
      GetLocalTime(&currentLocalTime);

      for (unsigned int i = 0; i < staticLogger->GetLoggerContexts()->Count(); i++)
      {
        CStaticLoggerContext *context = staticLogger->GetLoggerContexts()->GetItem(i);

        wchar_t *contextLogFile = Duplicate(context->GetLogFile());
        PathRemoveFileSpec(contextLogFile);

        // files with 'dmp' extension are known for Visual Studio

        wchar_t *dumpFileName = FormatString(L"%s\\MPUrlSourceSplitter-%04.4d-%02.2d-%02.2d-%02.2d-%02.2d-%02.2d-%03.3d.dmp", contextLogFile,
          currentLocalTime.wYear, currentLocalTime.wMonth, currentLocalTime.wDay,
          currentLocalTime.wHour, currentLocalTime.wMinute, currentLocalTime.wSecond, currentLocalTime.wMilliseconds);

        HANDLE dumpFile = CreateFile(dumpFileName, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_WRITE | FILE_SHARE_READ, 0, CREATE_ALWAYS, 0, 0);

        if (dumpFile != INVALID_HANDLE_VALUE)
        {
          minidumpException.ThreadId = GetCurrentThreadId();
          minidumpException.ExceptionPointers = exceptionInfo;
          minidumpException.ClientPointers = TRUE;

          MINIDUMP_TYPE miniDumpType = (MINIDUMP_TYPE)
            (MiniDumpWithFullMemory | MiniDumpWithDataSegs | MiniDumpIgnoreInaccessibleMemory); 

          if (MiniDumpWriteDump(GetCurrentProcess(), GetCurrentProcessId(), dumpFile, miniDumpType, &minidumpException, NULL, NULL) == TRUE)
          {
            wchar_t *guid = ConvertGuidToString(GUID_NULL);

            wchar_t *message = FormatString(L"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%4x] [%s] %s %s\r\n",
              currentLocalTime.wDay, currentLocalTime.wMonth, currentLocalTime.wYear,
              currentLocalTime.wHour, currentLocalTime.wMinute, currentLocalTime.wSecond,
              currentLocalTime.wMilliseconds,
              GetCurrentThreadId(),
              guid,
              L"[Error]  ",
              dumpFileName);

            staticLogger->LogMessage(context->GetMutex(), LOGGER_ERROR, message);

            FREE_MEM(guid);
            FREE_MEM(message);
          }

          CloseHandle(dumpFile);
        }

        FREE_MEM(dumpFileName);
        FREE_MEM(contextLogFile);
      }
    }

    if (staticLogger != NULL)
    {
      staticLogger->Flush();
    }
  }

  return EXCEPTION_CONTINUE_SEARCH;
}
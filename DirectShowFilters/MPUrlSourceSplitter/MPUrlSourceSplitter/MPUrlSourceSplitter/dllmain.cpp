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

#define wszIptvSource                                                 L"MediaPortal IPTV Source Filter"
#define wszUrlSourceSplitter                                          L"MediaPortal Url Source Splitter"

#include "MPUrlSourceSplitter.h"
#include "StaticLogger.h"
#include "FFmpegLogger.h"
#include "CrashReport.h"

#include <curl/curl.h>

#include "ErrorCodes.h"

#pragma warning(pop)

#define AVCODED_MODULE_FILE_NAME                                      L"avcodec-mpurlsourcesplitter-54.dll"
#define AVFORMAT_MODULE_FILE_NAME                                     L"avformat-mpurlsourcesplitter-54.dll"
#define AVUTIL_MODULE_FILE_NAME                                       L"avutil-mpurlsourcesplitter-51.dll"
#define LIBCULR_MODULE_FILE_NAME                                      L"MPUrlSourceSplitter_libcurl.dll"

extern "C++" CFFmpegLogger *ffmpegLogger = NULL;
extern "C++" CStaticLogger *staticLogger = NULL;
CCrashReport *crashReport = NULL;

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
  HRESULT result = S_OK;

  switch (ul_reason_for_call)
  {
  case DLL_PROCESS_ATTACH:
    {
      // initialize crash report with default settings
      // with each passed URL can be crash report turned off or can be changed settings

      if (SUCCEEDED(result))
      {
#pragma warning(push)
        // disable warning: 'ApplicationInfo': was declared deprecated
        // disable warning: 'HandlerSettings': was declared deprecated
#pragma warning(disable:4996)

        crashReport = new CCrashReport(&result);
        CHECK_CONDITION_HRESULT(result, crashReport, result, E_OUTOFMEMORY);

        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(crashReport));

#pragma warning(pop)

        if (SUCCEEDED(result))
        {
          staticLogger = new CStaticLogger(&result);
          ffmpegLogger = new CFFmpegLogger(&result, staticLogger);
          curl_global_init(CURL_GLOBAL_ALL);

          CHECK_CONDITION_HRESULT(result, staticLogger, result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(staticLogger));

          if (SUCCEEDED(result))
          {
            ALLOC_MEM_DEFINE_SET(moduleFileName, wchar_t, MAX_PATH, 0);
            CHECK_POINTER_HRESULT(result, moduleFileName, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, GetModuleFileName(GetModuleHandle(MODULE_FILE_NAME), moduleFileName, MAX_PATH) != 0, result, E_CANNOT_GET_MODULE_FILE_NAME);
            CHECK_CONDITION_HRESULT(result, staticLogger->RegisterModule(moduleFileName), result, E_OUTOFMEMORY);
            memset(moduleFileName, 0, MAX_PATH * sizeof(wchar_t));

            CHECK_CONDITION_HRESULT(result, GetModuleFileName(GetModuleHandle(AVCODED_MODULE_FILE_NAME), moduleFileName, MAX_PATH) != 0, result, E_CANNOT_GET_MODULE_FILE_NAME);
            CHECK_CONDITION_HRESULT(result, staticLogger->RegisterModule(moduleFileName), result, E_OUTOFMEMORY);
            memset(moduleFileName, 0, MAX_PATH * sizeof(wchar_t));

            CHECK_CONDITION_HRESULT(result, GetModuleFileName(GetModuleHandle(AVFORMAT_MODULE_FILE_NAME), moduleFileName, MAX_PATH) != 0, result, E_CANNOT_GET_MODULE_FILE_NAME);
            CHECK_CONDITION_HRESULT(result, staticLogger->RegisterModule(moduleFileName), result, E_OUTOFMEMORY);
            memset(moduleFileName, 0, MAX_PATH * sizeof(wchar_t));

            CHECK_CONDITION_HRESULT(result, GetModuleFileName(GetModuleHandle(AVUTIL_MODULE_FILE_NAME), moduleFileName, MAX_PATH) != 0, result, E_CANNOT_GET_MODULE_FILE_NAME);
            CHECK_CONDITION_HRESULT(result, staticLogger->RegisterModule(moduleFileName), result, E_OUTOFMEMORY);
            memset(moduleFileName, 0, MAX_PATH * sizeof(wchar_t));

            CHECK_CONDITION_HRESULT(result, GetModuleFileName(GetModuleHandle(LIBCULR_MODULE_FILE_NAME), moduleFileName, MAX_PATH) != 0, result, E_CANNOT_GET_MODULE_FILE_NAME);
            CHECK_CONDITION_HRESULT(result, staticLogger->RegisterModule(moduleFileName), result, E_OUTOFMEMORY);
            memset(moduleFileName, 0, MAX_PATH * sizeof(wchar_t));

            FREE_MEM(moduleFileName);
          }
        }
      }
    }
    break;
  case DLL_THREAD_ATTACH:
    break;
  case DLL_THREAD_DETACH:
    break;
  case DLL_PROCESS_DETACH:
    {
      // free FFmpeg logger instance and static logger instance
      curl_global_cleanup();
      FREE_MEM_CLASS(ffmpegLogger);
      FREE_MEM_CLASS(staticLogger);

      FREE_MEM_CLASS(crashReport);
    }
    break;
  }

  return FAILED(result) ? FALSE : DllEntryPoint((HINSTANCE)(hModule), ul_reason_for_call, lpReserved);
}
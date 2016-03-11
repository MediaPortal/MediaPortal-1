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
#include "stdafx.h"

#include "MPIPTVSource.h"
#include "Logger.h"

#define g_wszPushSource     L"MediaPortal IPTV Source Filter"

#define MODULE_NAME                                               _T("DLLMAIN")

#define METHOD_DLL_REGISTER_SERVER_NAME                           _T("DllRegisterServer()")
#define METHOD_DLL_UNREGISTER_SERVER_NAME                         _T("DllUnregisterServer()")
#define METHOD_DLL_MAIN_NAME                                      _T("DllMain()")

// Filter setup data
const AMOVIESETUP_MEDIATYPE iptvOpPinTypes =
{
  &MEDIATYPE_Stream,              // Major type
  &MEDIASUBTYPE_MPEG2_TRANSPORT   // Minor type
};

const AMOVIESETUP_PIN iptvOutputPin = 
{
  L"Output",      // Obsolete, not used.
  FALSE,          // Is this pin rendered?
  TRUE,           // Is it an output pin?
  FALSE,          // Can the filter create zero instances?
  FALSE,          // Does the filter create multiple instances?
  &CLSID_NULL,    // Obsolete.
  NULL,           // Obsolete.
  1,              // Number of media types.
  &iptvOpPinTypes // Pointer to media types.
};

const AMOVIESETUP_FILTER iptvPushSource =
{
  &CLSID_MPIPTVSource,          // Filter CLSID
  g_wszPushSource,              // String name
  MERIT_NORMAL,                 // Filter merit
  1,                            // Number pins
  &iptvOutputPin,               // Pin details
  CLSID_LegacyAmFilterCategory  // Filter category
};

// List of class IDs and creator functions for the class factory. This
// provides the link between the OLE entry point in the DLL and an object
// being created. The class factory will call the static CreateInstance.
// We provide a set of filters in this one DLL.

CFactoryTemplate g_Templates[1] = 
{
  { 
    g_wszPushSource,                    // Name
    &CLSID_MPIPTVSource,                // CLSID
    CMPIPTVSource::CreateInstance,      // Method to create an instance of MyComponent
    NULL,                               // Initialization function
    &iptvPushSource                     // Set-up information (for filters)
  }
};

int g_cTemplates = sizeof(g_Templates) / sizeof(g_Templates[0]);    

extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);

STDAPI DllRegisterServer()
{
  CLogger logger;
  logger.Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DLL_REGISTER_SERVER_NAME);

  return AMovieDllRegisterServer2(TRUE);
}

STDAPI DllUnregisterServer()
{
  CLogger logger;
  logger.Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DLL_UNREGISTER_SERVER_NAME);

  return AMovieDllRegisterServer2(FALSE);
}

BOOL APIENTRY DllMain( HMODULE hModule,
  DWORD  ul_reason_for_call,
  LPVOID lpReserved
  )
{
  CLogger logger;
  logger.Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DLL_MAIN_NAME);

  switch (ul_reason_for_call)
  {
  case DLL_PROCESS_ATTACH:
    logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_DLL_MAIN_NAME, _T("DLL_PROCESS_ATTACH"));
    break;
  case DLL_THREAD_ATTACH:
    logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_DLL_MAIN_NAME, _T("DLL_THREAD_ATTACH"));
    break;
  case DLL_THREAD_DETACH:
    logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_DLL_MAIN_NAME, _T("DLL_THREAD_DETACH"));
    break;
  case DLL_PROCESS_DETACH:
    logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_DLL_MAIN_NAME, _T("DLL_PROCESS_DETACH"));
    break;
  }

  BOOL result = DllEntryPoint((HINSTANCE)(hModule), ul_reason_for_call, lpReserved);

  logger.Log(LOGGER_INFO, _T("%s: %s: result: %d"), MODULE_NAME, METHOD_DLL_MAIN_NAME, result);
  logger.Log(LOGGER_INFO, (result) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, MODULE_NAME, METHOD_DLL_MAIN_NAME);

  return result;
}

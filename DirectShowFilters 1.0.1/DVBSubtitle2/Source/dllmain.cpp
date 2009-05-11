/* 
 *	Copyright (C) 2006-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#pragma warning( disable: 4995 4996 )

#include <string>
#include <shlobj.h>
#include "DVBSub.h"

static bool folderOk = false;

using std::string;

// Setup data
const AMOVIESETUP_MEDIATYPE sudPinTypesSubtitle =
{
  &MEDIATYPE_Stream, &MEDIASUBTYPE_MPEG2_TRANSPORT
};

const AMOVIESETUP_PIN sudPins[] =
{
  {
    L"SubtitleIn",           // Pin string name
    FALSE,                   // Is it rendered
    FALSE,                   // Is it an output
    FALSE,                   // Allowed none
    FALSE,                   // Likewise many
    &CLSID_NULL,             // Connects to filter
    L"SubtitleIn",           // Connects to pin
    1,                       // Number of types
    &sudPinTypesSubtitle     // Pin information
  }
};

const AMOVIESETUP_FILTER FilterInfo =
{
  &CLSID_DVBSub2,            // Filter CLSID
  L"MediaPortal DVBSub2",    // String name
  MERIT_DO_NOT_USE,          // Filter merit
  1,                         // Number pins
  sudPins                    // Pin details
};


CFactoryTemplate g_Templates[1] = 
{
  { 
    L"MediaPortal DVBSub2",     // Name
    &CLSID_DVBSub2,             // CLSID
    CDVBSub::CreateInstance,    // Method to create an instance of MyComponent
    NULL,                       // Initialization function
    &FilterInfo                 // Set-up information (for filters)
  }
};

int g_cTemplates = 1;

STDAPI DllRegisterServer()
{
  LogDebug( "DllRegisterServer - called" );
  int error = AMovieDllRegisterServer2( TRUE );
  LogDebug( "  AMovieDllRegisterServer2 returned %i", error );
  return error;
}

STDAPI DllUnregisterServer()
{
  LogDebug( "DllUnregisterServer - called" );
  int error = AMovieDllRegisterServer2( FALSE );
  LogDebug( "  AMovieDllRegisterServer2 returned %i", error );
  return error;
}

extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);

BOOL APIENTRY DllMain(HANDLE hModule, 
                      DWORD  dwReason, 
                      LPVOID lpReserved)
{
  LogDebug( "DllMain - called" );
  return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);
}

// Logging 
//#ifdef DEBUG
char *logbuffer=NULL; 
void GetLogFile(char *pLog)
{
  TCHAR folder[MAX_PATH];
  ::SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
  sprintf(pLog,"%s\\Team MediaPortal\\MediaPortal\\Log\\DVBSubs.log",folder);
}


void LogDebug(const char *fmt, ...) 
{
  va_list ap;
  va_start(ap,fmt);

  char buffer[1000]; 
  int tmp;
  va_start(ap,fmt);
  tmp=vsprintf(buffer, fmt, ap);
  va_end(ap); 
  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);

//#ifdef DONTLOG
  TCHAR filename[1024];
  GetLogFile(filename);
  FILE* fp = fopen(filename,"a+");

  if (fp!=NULL)
  {
    fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%x]%s\n",
      systemTime.wDay, systemTime.wMonth, systemTime.wYear,
      systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
      systemTime.wMilliseconds,
      GetCurrentThreadId(),
      buffer);
    fclose(fp);
  }
//#endif
  char buf[1000];
  sprintf(buf,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
    systemTime.wDay, systemTime.wMonth, systemTime.wYear,
    systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
    buffer);
  ::OutputDebugString(buf);
};

//#else
//void LogDebug(const char *fmt, ...) {}
//#endif

//#ifdef DEBUG
char pts_temp[500];
void LogDebugPTS( const char *fmt, uint64_t pts ) 
{
  int h,m,s,u;

  pts /= 90; // Convert to milliseconds
  h = int( ( pts / ( 1000*60*60 ) ) );
  m = int( ( pts / ( 1000*60 ) ) - ( h*60 ) );
  s = int( ( pts/1000 ) - ( h*3600 ) - ( m*60 ) );
  u = int( pts - ( h*1000*60*60 ) - ( m*1000*60 ) - ( s*1000 ) );

  sprintf( pts_temp,"%s %d:%02d:%02d%c%03d",fmt,h,m,s,'.',u );
  LogDebug( pts_temp );
}
//#else
//void LogDebugPTS( const char *fmt, uint64_t pts  ){}
//#endif 

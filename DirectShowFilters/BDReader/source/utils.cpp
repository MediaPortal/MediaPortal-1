#include "StdAfx.h"

#include <commdlg.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include <shlobj.h>

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"


static wchar_t logFile[MAX_PATH];
static WORD logFileParsed = -1;

void GetLogFile(wchar_t *pLog)
{
  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);
  if(logFileParsed != systemTime.wDay)
  {
    wchar_t folder[MAX_PATH];
    ::SHGetSpecialFolderPathW(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
    swprintf_s(logFile,L"%s\\Team MediaPortal\\MediaPortal\\Log\\BDReader-%04.4d-%02.2d-%02.2d.Log",folder, systemTime.wYear, systemTime.wMonth, systemTime.wDay);
    logFileParsed=systemTime.wDay; // rec
  }
  wcscpy(pLog, &logFile[0]);
}


static char logbuffer[2000]; 
static wchar_t logbufferw[2000];
void LogDebug(const wchar_t *fmt, ...)
{
	va_list ap;
	va_start(ap,fmt);

	va_start(ap,fmt);
	vswprintf_s(logbufferw, fmt, ap);
	va_end(ap); 

	wchar_t fileName[MAX_PATH];
	GetLogFile(fileName);
	FILE* fp = _wfopen(fileName,L"a+, ccs=UTF-8");
	if (fp!=NULL)
	{
	SYSTEMTIME systemTime;
	GetLocalTime(&systemTime);
		fwprintf(fp,L"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%5x]%s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,systemTime.wMilliseconds,
      GetCurrentThreadId(),
			logbufferw);
		fclose(fp);
		//::OutputDebugStringW(logbufferw);::OutputDebugStringW(L"\n");
	}
};

void LogDebug(const char *fmt, ...)
{
	va_list ap;
	va_start(ap,fmt);

	va_start(ap,fmt);
	vsprintf(logbuffer, fmt, ap);
	va_end(ap); 

	MultiByteToWideChar(CP_ACP, 0, logbuffer, -1,logbufferw, sizeof(logbuffer)/sizeof(wchar_t));
	LogDebug(L"%s", logbufferw);
};


// http://blogs.msdn.com/b/stevejs/archive/2005/12/19/505815.aspx

#include <windows.h>
#define MS_VC_EXCEPTION 0x406D1388
#pragma pack(push,8)
typedef struct tagTHREADNAME_INFO
{
   DWORD dwType; // Must be 0x1000.
   LPCSTR szName; // Pointer to name (in user addr space).
   DWORD dwThreadID; // Thread ID (-1=caller thread).
   DWORD dwFlags; // Reserved for future use, must be zero.
} THREADNAME_INFO;

#pragma pack(pop)

void SetThreadName(DWORD dwThreadID, char* threadName)
{
   Sleep(10);
   THREADNAME_INFO info;
   info.dwType = 0x1000;
   info.szName = threadName;
   info.dwThreadID = dwThreadID;
   info.dwFlags = 0;
   __try
   {
      RaiseException( MS_VC_EXCEPTION, 0, sizeof(info)/sizeof(ULONG_PTR), (ULONG_PTR*)&info );
   }
   __except(EXCEPTION_EXECUTE_HANDLER)
   {
   }
}
/**********
This library is free software; you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the
Free Software Foundation; either version 2.1 of the License, or (at your
option) any later version. (See <http://www.gnu.org/copyleft/lesser.html>.)

This library is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for
more details.

You should have received a copy of the GNU Lesser General Public License
along with this library; if not, write to the Free Software Foundation, Inc.,
59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
**********/
// Copyright (c) 1996-2000 Live Networks, Inc.  All rights reserved.
// Basic Usage Environment: for a simple, non-scripted, console application
// Implementation

#ifndef IMN_PIM
#include "BasicUsageEnvironment.hh"
#include <stdio.h>
#include <shlobj.h>

////////// BasicUsageEnvironment //////////

void Log(const char *fmt, ...) 
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

  TCHAR folder[MAX_PATH];
  TCHAR fileName[MAX_PATH];
  ::SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
  sprintf(fileName,"%s\\Team MediaPortal\\MediaPortal TV Server\\log\\streaming server.Log",folder);
  FILE* fp = fopen(fileName,"a+");
	if (fp!=NULL)
	{
		SYSTEMTIME systemTime;
		GetLocalTime(&systemTime);
		fprintf(fp,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
			buffer);
		fclose(fp);
	}
  char buf[1000];
	sprintf(buf,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d %s\n",
		systemTime.wDay, systemTime.wMonth, systemTime.wYear,
		systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
		buffer);
  ::OutputDebugString(buf);
}

#if defined(__WIN32__) || defined(_WIN32)
extern "C" int initializeWinsockIfNecessary();
#endif

BasicUsageEnvironment::BasicUsageEnvironment(TaskScheduler& taskScheduler)
: BasicUsageEnvironment0(taskScheduler) {
#if defined(__WIN32__) || defined(_WIN32)
  if (!initializeWinsockIfNecessary()) {
    setResultErrMsg("Failed to initialize 'winsock': ");
    reportBackgroundError();
    exit(1);
  }
#endif
}

BasicUsageEnvironment::~BasicUsageEnvironment() {
}

BasicUsageEnvironment*
BasicUsageEnvironment::createNew(TaskScheduler& taskScheduler) {
  return new BasicUsageEnvironment(taskScheduler);
}

int BasicUsageEnvironment::getErrno() const {
#if defined(__WIN32__) || defined(_WIN32)
#ifndef _WIN32_WCE
  if (errno == 0) {
    errno = WSAGetLastError();
  }
#endif
#endif
#if defined(_WIN32_WCE)
  return WSAGetLastError();
#else
  return errno;
#endif
}

UsageEnvironment& BasicUsageEnvironment::operator<<(char const* str) 
{
  char buffer[2048];
	sprintf(buffer, "%s", str);
  Log(buffer);
	return *this;
}

UsageEnvironment& BasicUsageEnvironment::operator<<(int i) {

  char buffer[2048];
	sprintf(buffer, "%d", i);
  Log(buffer);
	return *this;
}

UsageEnvironment& BasicUsageEnvironment::operator<<(unsigned u) {

  char buffer[2048];
	sprintf(buffer, "%u", u);
  Log(buffer);
	return *this;
}

UsageEnvironment& BasicUsageEnvironment::operator<<(double d) {

  char buffer[2048];
	sprintf(buffer, "%f", d);
  Log(buffer);
	return *this;
}

UsageEnvironment& BasicUsageEnvironment::operator<<(void* p) {

  char buffer[2048];
	sprintf(buffer, "%p", p);
  Log(buffer);
	return *this;
}
#endif


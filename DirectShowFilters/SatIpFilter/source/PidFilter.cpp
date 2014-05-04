#include "PidFilter.h"
#include <stdio.h>
#include <string.h>
// for Debug
#include <streams.h>
#include <shlobj.h>

static char logbuffer[2000];

void LogDebug2(const char *fmt, ...)
{
	va_list ap;
	va_start(ap, fmt);

	int tmp;
	va_start(ap, fmt);
	tmp = vsprintf(logbuffer, fmt, ap);
	va_end(ap);

	TCHAR folder[MAX_PATH];
	TCHAR fileName[MAX_PATH];
	::SHGetSpecialFolderPath(NULL, folder, CSIDL_COMMON_APPDATA, FALSE);
	sprintf(fileName, "%s\\Team MediaPortal\\MediaPortal TV Server\\log\\SatIP.Log", folder);

	FILE* fp = fopen(fileName, "a+");
	if (fp != NULL)
	{
		SYSTEMTIME systemTime;
		GetLocalTime(&systemTime);
		fprintf(fp, "%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%02.2d %s\n",
			systemTime.wDay, systemTime.wMonth, systemTime.wYear,
			systemTime.wHour, systemTime.wMinute, systemTime.wSecond, systemTime.wMilliseconds,
			logbuffer);
		fclose(fp);
	}
};

PidFilter::PidFilter()
{
	//_pids = new unsigned char[Number_of_Pids];
	_pids = new int[Number_of_Pids];
	pidCounter = 0;
}

PidFilter::~PidFilter()
{
	delete[] _pids;
	delete &pidCounter, &index;
}

void PidFilter::Add(int pid) {
	//memcpy(_pids+4, &pid, 4);
	_pids[pidCounter] = pid;
	pidCounter++;
}

void PidFilter::Del(int pid) {
	for (size_t i = 0; i < sizeof(_pids); i++)
	{
		if (_pids[i] == pid) {
			index = i;
			i = sizeof(_pids); // leafe the for
		}
	}

	for (int i = index; i < sizeof(_pids); i++) {
		_pids[index] = _pids[index + 1];
	}
	_pids[sizeof(_pids) - 1] = 0;

	pidCounter--;
}

bool PidFilter::PidRequested(unsigned short pid) {
	for (size_t i = 0; i < sizeof(_pids); i++)
	{
		if (_pids[i] == static_cast<int>(pid)) {
			return true;
		}
	}
	return false;
}
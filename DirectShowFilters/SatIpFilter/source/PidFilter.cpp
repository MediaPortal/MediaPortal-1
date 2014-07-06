#include "PidFilter.h"
#include <stdio.h>
#include <string.h>
#include <sstream>
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
	InitializeCriticalSection(&csPidsInAccess);
}

PidFilter::~PidFilter()
{
	DeleteCriticalSection(&csPidsInAccess);
}

void PidFilter::Add(uint16_t pid) {
	//memcpy(_pids+4, &pid, 4);
	EnterCriticalSection(&csPidsInAccess);
	pids.insert(pid);
	LeaveCriticalSection(&csPidsInAccess);
}

void PidFilter::Del(uint16_t pid) {
	EnterCriticalSection(&csPidsInAccess);
	pids.erase(pid);
	LeaveCriticalSection(&csPidsInAccess);
}

void PidFilter::Reset() {
	EnterCriticalSection(&csPidsInAccess);
	pids.clear();
	LeaveCriticalSection(&csPidsInAccess);
}

void PidFilter::SyncPids(std::string newPids) {
	EnterCriticalSection(&csPidsInAccess);
	
	// reset Array
	pids.clear();
	
	std::stringstream lines(newPids);
	std::string line2;

	while (std::getline(lines, line2, '#')) {
		uint16_t newPid = static_cast<uint16_t>(atoi(line2.c_str()));
		pids.insert(newPid);
		LogDebug2("SyncPid: %d", newPid);
	}
	LeaveCriticalSection(&csPidsInAccess);
}

void PidFilter::SyncPids(std::vector<uint16_t> newPids) {
	EnterCriticalSection(&csPidsInAccess);

	// reset Array
	pids.clear();

	for (size_t i = 0; i < newPids.size(); ++i) {
		pids.insert(newPids.at(i));
	}

	LeaveCriticalSection(&csPidsInAccess);
}

bool PidFilter::PidRequested(uint16_t pid) {
	// always allow PMT
	if (pid == 0) {
		return true;
	}
	
	EnterCriticalSection(&csPidsInAccess);
	bool result = pids.find(pid) != pids.end();
	LeaveCriticalSection(&csPidsInAccess);
	return result;
}

uint16_t PidFilter::getPidFromPackage(unsigned char* packageStart) {
	uint16_t result = 0xFFFF;
	// Check if the pointer points to a sync byte
	if (packageStart[0] == 0x47) {
		// Extract the least significant 13 bits
		result = (packageStart[1] & 0x1F);
		result <<= 8;
		result += packageStart[2];
	}
	return result;
}
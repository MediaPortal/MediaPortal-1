// Copyright (C) 2005-2015 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#pragma once

#include "stdafx.h"
#include <string>
#include <queue>

#include "alloctracing.h"

using namespace std;

const int MAX_LOG_LINE_LENGHT = 1000;
const int LOG_LINE_RESERVED = 32;

// These macros are used to keep the logging related changes minimal
#define Log m_pLogger->WriteLog
#define LogWaveFormat m_pLogger->WriteWaveFormat

class Logger
{
  public:
    Logger();
    ~Logger();

    void LogPath(TCHAR* dest, TCHAR* name);
    void LogRotate();

    void Start();
    void Stop();

    void WriteLog(const char *fmt, ...);
    void WriteWaveFormat(const WAVEFORMATEXTENSIBLE* pwfx, const char* text);

  private:
    static unsigned int WINAPI ThreadEntryPoint(void* pArguments);
    DWORD ThreadProc();

    const char* SubFormatToString(GUID subFormat);

    string GetLogLine();

    CCritSec m_qLock;
    CCritSec m_threadLock;

    queue<string> m_logQueue;
    CAMEvent m_eLog;
    CAMEvent m_eStop;

    HANDLE m_hLogger;

    char m_prevLogLine[MAX_LOG_LINE_LENGHT];
    int m_repeat;
};


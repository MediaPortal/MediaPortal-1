/**
 *	LogMessageWriter.cpp
 *	Copyright (C) 2004 Nate
 *
 *	This file is part of DigitalWatch, a free DTV watching and recording
 *	program for the VisionPlus DVB-T.
 *
 *	DigitalWatch is free software; you can redistribute it and/or modify
 *	it under the terms of the GNU General Public License as published by
 *	the Free Software Foundation; either version 2 of the License, or
 *	(at your option) any later version.
 *
 *	DigitalWatch is distributed in the hope that it will be useful,
 *	but WITHOUT ANY WARRANTY; without even the implied warranty of
 *	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *	GNU General Public License for more details.
 *
 *	You should have received a copy of the GNU General Public License
 *	along with DigitalWatch; if not, write to the Free Software
 *	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

#include "LogMessageWriter.h"
#include "GlobalFunctions.h"
#include "LogFileWriter.h"

//////////////////////////////////////////////////////////////////////
// LogMessageWriter
//////////////////////////////////////////////////////////////////////

LogMessageWriter::LogMessageWriter()
{
	m_logFilename = NULL;
	m_LogBufferLimit = 0;
	m_WriteThreadActive = FALSE;
}

LogMessageWriter::~LogMessageWriter()
{
	CAutoLock BufferLock(&m_BufferLock);
	FlushLogBuffer();
	if (m_WriteThreadActive)
		StopThread(250);

//	CAutoLock logFileLock(&m_logFileLock);
	if (m_logFilename)
	{
		delete[] m_logFilename;
		m_logFilename = NULL;
	}
}

void LogMessageWriter::ClearBuffer(void)
{
	CAutoLock BufferLock(&m_BufferLock);
	std::vector<CLogInfo*>::iterator it = m_Array.begin();
	for ( ; it != m_Array.end() ; it++ )
	{
		CLogInfo *logInfo = *it;
		delete logInfo;
	}
	m_Array.clear();
}

void LogMessageWriter::ThreadProc(void)
{
	m_WriteThreadActive = TRUE;

	BrakeThread Brake;
	
	while (!ThreadIsStopping(100))
	{
		FlushLogBuffer(m_LogBufferLimit);
		Sleep(100);
	}
	m_WriteThreadActive = FALSE;

	return;
}

void LogMessageWriter::FlushLogBuffer(int logSize)
{
//	CAutoLock logFileLock(&m_logFileLock);
	CAutoLock BufferLock(&m_BufferLock);
	if ((int)m_Array.size() < logSize)
		return;

//	::OutputDebugString(TEXT("LogMessageWriter::ThreadProc:Write."));
	USES_CONVERSION;
	if (m_logFilename)
	{
		LogFileWriter file;
		if SUCCEEDED(file.Open(m_logFilename, TRUE))
		{
			while(TRUE)
			{
				CLogInfo *logInfo = NULL;
				{
					if (m_Array.size() > 0)
					{
						std::vector<CLogInfo*>::iterator it = m_Array.begin();
						logInfo = *it;
						m_Array.erase(it);
					}
					else
						break;
				}

				if (logInfo)
				{
					for ( int i=0 ; i<logInfo->indent ; i++ )
					{
						file << "  ";
					}
					//Write one line at a time so we can do dos style EOL's
					LPWSTR pCurr = logInfo->pStr;
					while (pCurr[0] != '\0')
					{
						LPWSTR pEOL = wcschr(pCurr, '\n');
						if (pEOL)
						{
							pEOL[0] = '\0';
							file << pCurr << file.EOL;
							pEOL[0] = '\n';
							pCurr = pEOL + 1;
						}
						else
						{
							file << pCurr;
							break;
						}
					};

					if (logInfo)
						delete logInfo;
				}
			};
			file.Close();
		}
	}
//	::OutputDebugString(TEXT("LogMessageWriter::ThreadProc:Write Completed.\n"));
}

void LogMessageWriter::SetLogBufferLimit(int logBufferLimit)
{
//	CAutoLock logFileLock(&m_logFileLock);
	CAutoLock BufferLock(&m_BufferLock);
	m_LogBufferLimit = logBufferLimit;
	FlushLogBuffer(0);
}

int LogMessageWriter::GetLogBufferLimit(void)
{
//	CAutoLock logFileLock(&m_logFileLock);
	CAutoLock BufferLock(&m_BufferLock);
	return m_LogBufferLimit;
}

void LogMessageWriter::SetFilename(LPWSTR filename)
{
//	CAutoLock logFileLock(&m_logFileLock);
	CAutoLock BufferLock(&m_BufferLock);

	if ((wcslen(filename) > 2) &&
		((filename[1] == ':') ||
		 (filename[0] == '\\' && filename[1] == '\\')
		)
	   )
	{
		strCopy(m_logFilename, filename);
	}
	else
	{
		LPWSTR str = new wchar_t[MAX_PATH];
		GetCommandPath(str);
		swprintf(str, L"%s%s", str, filename);
		strCopy(m_logFilename, str);
		delete[] str;
	}
//	USES_CONVERSION;
//	::OutputDebugString(W2T(m_logFilename));
}

void LogMessageWriter::Write(LPWSTR pStr)
{
//	CAutoLock logFileLock(&m_logFileLock);

	USES_CONVERSION;
	if (m_logFilename)
	{
		if(!m_WriteThreadActive)
			StartThread();

		CLogInfo *item = new CLogInfo();
		item->pStr = NULL;
		strCopy(item->pStr, pStr);
		item->indent = m_indent;
		CAutoLock BufferLock(&m_BufferLock);
		m_Array.push_back(item);
	}
	return;
}

void LogMessageWriter::Clear()
{
//	CAutoLock logFileLock(&m_logFileLock);

	ClearBuffer();
	USES_CONVERSION;
	if (m_logFilename)
	{
		LogFileWriter file;
		if SUCCEEDED(file.Open(m_logFilename, FALSE))
		{
			file.Close();
		}
	}
}


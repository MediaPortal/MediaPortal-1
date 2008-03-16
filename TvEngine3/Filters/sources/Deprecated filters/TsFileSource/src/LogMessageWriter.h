/**
 *	LogMessageWriter.h
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

#ifndef LOGMESSAGEWRITER_H
#define LOGMESSAGEWRITER_H

#include "StdAfx.h"
#include "LogMessage.h"
#include <vector>
#include "TSThread.h"
#include "Global.h"

class CLogInfo
{
public:
	CLogInfo()
	{
		pStr = NULL;
		indent = 0;
	};
	virtual ~CLogInfo()
	{
		if (pStr)
			delete[] pStr;
	};

	LPWSTR pStr;
	int indent;

};

class LogMessageWriter : public LogMessageCallback, public TSThread
{
public:
	LogMessageWriter();
	virtual ~LogMessageWriter();
	void SetFilename(LPWSTR filename);
	virtual void Write(LPWSTR pStr);
	virtual void Clear();
	virtual	void SetLogBufferLimit(int logBufferLimit = 0);
	virtual int GetLogBufferLimit(void);
private:
	LPWSTR m_logFilename;
	CCritSec m_logFileLock;

	BOOL m_WriteThreadActive;
	int m_LogBufferLimit;
	std::vector<CLogInfo*> m_Array;
	CCritSec m_BufferLock;

	void ClearBuffer(void);
	void ThreadProc(void);
	void FlushLogBuffer(int logSize = 0);

};

#endif

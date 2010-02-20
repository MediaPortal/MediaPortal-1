/**
*	LogProfiler.h
*	Copyright (C) 2007 Nate
*
*  This file is part of TSFileSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSFileSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSFileSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSFileSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  bisswanger can be reached at WinSTB@hotmail.com
*    Homepage: http://www.winstb.de
*
*  bear and nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#ifndef LOGPROFILER_H
#define LOGPROFILER_H

#include "StdAfx.h"
#include "LogMessage.h"
#include "LogFileWriter.h"
#include <vector>

class ProfilerFunction;

///////////////////
// ProfilerEntry //
///////////////////
class ProfilerEntry
{
public:
	ProfilerEntry();
	virtual ~ProfilerEntry();

	__int64 timestamp;
	LPWSTR pName;
	ProfilerFunction *pSubFunction;
};

//////////////////////
// ProfilerFunction //
//////////////////////
class ProfilerFunction
{
public:
	ProfilerFunction(LPWSTR pFunctionName, ProfilerFunction *pParentFunction);
	virtual ~ProfilerFunction();

	void Write(LogFileWriter &writer, int indent);

	LPWSTR m_pFunctionName;
	ProfilerFunction *m_pParentFunction;

	__int64 m_startTime;
	__int64 m_finishTime;
	vector<ProfilerEntry *> m_entries;
};

////////////////////
// ProfilerThread //
////////////////////
/*class ProfilerThread
{
public:
	DWORD threadId;
	Profiler *pProfiler;
};*/

//////////////
// Profiler //
//////////////
class Profiler
{
public:
	Profiler(LPWSTR pFunctionName);
	virtual ~Profiler();
	virtual void AddTimeStamp(LPWSTR pName);
	virtual void AddComment(LPWSTR pComment);

	DWORD threadId;

private:
	BOOL Open(BOOL bClear);
	virtual void Close();
	virtual void Write();

private:

	LogFileWriter writer;

	Profiler *m_profiler;

	ProfilerFunction *m_pTopFunction;
	ProfilerFunction *m_pCurrentFunction;

	LPWSTR m_pFunctionName;
};

#endif

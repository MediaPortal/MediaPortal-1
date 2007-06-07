/**
*	LogProfiler.cpp
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

#include "LogProfiler.h"
#include "GlobalFunctions.h"

//////////////////////////////////////////////////////////////////////
// ProfilerEntry
//////////////////////////////////////////////////////////////////////

ProfilerEntry::ProfilerEntry()
{
	timestamp = 0;
	pName = NULL;
	pSubFunction = NULL;
}

ProfilerEntry::~ProfilerEntry()
{
	if (pName)
		delete[] pName;
	if (pSubFunction)
		delete pSubFunction;
}

//////////////////////////////////////////////////////////////////////
// ProfilerFunction
//////////////////////////////////////////////////////////////////////

ProfilerFunction::ProfilerFunction(LPWSTR pFunctionName, ProfilerFunction *pParentFunction)
{
	m_pFunctionName = NULL;
	strCopy(m_pFunctionName, pFunctionName);
	m_pParentFunction = pParentFunction;
	m_startTime = timeGetTime();
	m_finishTime = 0;
}

ProfilerFunction::~ProfilerFunction()
{
	if (m_pFunctionName)
		delete[] m_pFunctionName;

	vector<ProfilerEntry *>::iterator it = m_entries.begin();
	for ( ; it != m_entries.end() ; it++ )
	{
		ProfilerEntry *entry = *it;
		delete entry;
	}
	m_entries.clear();
}

void ProfilerFunction::Write(LogFileWriter &writer, int indent)
{
	int delta = (int)(m_finishTime - m_startTime);

	writer.indent(indent) << m_pFunctionName << " : " << delta << " ms\r\n";

	vector<ProfilerEntry *>::iterator it = m_entries.begin();
	__int64 lastTimestamp = m_startTime;
	int timestamp = 1;
	for ( ; it != m_entries.end() ; it++ )
	{
		ProfilerEntry *pEntry = *it;

		if (pEntry->timestamp != 0)
		{
			delta = (int)(pEntry->timestamp - lastTimestamp);
			lastTimestamp = pEntry->timestamp;
			
			writer.indent(indent) << " timestamp " << timestamp++ << ": " << pEntry->pName << " : " << delta << " ms\r\n";
		}
		else if (pEntry->pSubFunction)
		{
			pEntry->pSubFunction->Write(writer, indent+2);
		}
		else
		{
			writer.indent(indent) << " " << pEntry->pName << "\r\n";
		}
	}

	if (timestamp > 1)
	{
		delta = (int)(m_finishTime - lastTimestamp);
		writer.indent(indent) << " timestamp " << timestamp++ << ": Finish : " << delta << " ms\r\n";
	}

}


//////////////////////////////////////////////////////////////////////
// Profiler
//////////////////////////////////////////////////////////////////////

static vector<Profiler *> m_threads;
static BOOL m_bLogActive = TRUE;
static BOOL m_bLogChecked = FALSE;
static CCritSec m_writeLock;

Profiler::Profiler(LPWSTR pFunctionName)
{
	threadId = ::GetCurrentThreadId();

	m_pFunctionName = NULL;
	strCopy(m_pFunctionName, pFunctionName);

	m_profiler = NULL;
	m_pTopFunction = NULL;
	m_pCurrentFunction = NULL;

	if (m_bLogActive)
	{
		vector<Profiler *>::iterator it = m_threads.begin();
		for ( ; it != m_threads.end() ; it++ )
		{
			Profiler *thread = *it;
			if (thread->threadId == threadId)
			{
				m_profiler = thread;

				m_pCurrentFunction = new ProfilerFunction(pFunctionName, m_profiler->m_pCurrentFunction);

				ProfilerEntry *pEntry = new ProfilerEntry();
				pEntry->pSubFunction = m_pCurrentFunction;
				m_profiler->m_pCurrentFunction->m_entries.push_back(pEntry);

				m_profiler->m_pCurrentFunction = m_pCurrentFunction;
				break;
			}
		}
	}

	if (m_bLogChecked == FALSE)
	{
		CAutoLock writeLock(&m_writeLock);

		m_bLogActive = Open(TRUE);
		Close();

		m_bLogChecked = TRUE;
	}

	if ((m_profiler == NULL) && (m_bLogActive))
	{
		//ProfilerThread *thread = new ProfilerThread();
		//thread->threadId = threadId;
		//thread->pProfiler = this;
		m_threads.push_back(this);

		m_pTopFunction = new ProfilerFunction(pFunctionName, NULL);
		m_pCurrentFunction = m_pTopFunction;
	}
}

Profiler::~Profiler()
{
	if (m_bLogActive)
	{
		m_pCurrentFunction->m_finishTime = timeGetTime();
		if (m_profiler)
			m_profiler->m_pCurrentFunction = m_profiler->m_pCurrentFunction->m_pParentFunction;

		if ((m_profiler == NULL) || (m_pTopFunction == m_pCurrentFunction))
		{
			Write();

			//Remove this from the m_threads vector
			vector<Profiler *>::iterator it = m_threads.begin();
			while (it != m_threads.end())
			{
				if (*it == this)
				{
					m_threads.erase(it);
					continue;
				}
				it++;
			}

			m_pCurrentFunction = NULL;
			if (m_pTopFunction)
				delete m_pTopFunction;
		}
	}
}

void Profiler::AddTimeStamp(LPWSTR pName)
{
	if (m_bLogActive)
	{
		ProfilerEntry *pEntry = new ProfilerEntry();
		pEntry->timestamp = timeGetTime();
		strCopy(pEntry->pName, pName);

		m_pCurrentFunction->m_entries.push_back(pEntry);
	}
}

void Profiler::AddComment(LPWSTR pComment)
{
	if (m_bLogActive)
	{
		ProfilerEntry *pEntry = new ProfilerEntry();
		strCopy(pEntry->pName, pComment);

		m_pCurrentFunction->m_entries.push_back(pEntry);
	}
}

BOOL Profiler::Open(BOOL bClear)
{
	USES_CONVERSION;

	LPTSTR temp = new TCHAR[MAX_PATH];
	::GetTempPath(MAX_PATH, temp);

	LPWSTR filename = new wchar_t[MAX_PATH];
	swprintf(filename, L"%s%s", T2W(temp), L"TSFileSourceProfile.log");

	BOOL opened = writer.Open(filename, !bClear, FALSE);

	delete[] filename;
	delete[] temp;

	return opened;
}

void Profiler::Close()
{
	writer.Close();
}

void Profiler::Write()
{
	CAutoLock writeLock(&m_writeLock);

	Open(FALSE);

	writer << "------------- Thread Id:" << (int)threadId << " -------------\r\n";
	m_pTopFunction->Write(writer, 0);

	Close();
}


/**
 *	LogMessage.cpp
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

#include <stdio.h>
#include "LogMessage.h"
#include "GlobalFunctions.h"

//////////////////////////////////////////////////////////////////////
// LogMessageCallback
//////////////////////////////////////////////////////////////////////

LogMessageCallback::LogMessageCallback()
{
	static int handle = 1;
	m_handle = handle++;
	m_indent = 0;
}

LogMessageCallback::~LogMessageCallback()
{
}

int LogMessageCallback::GetHandle()
{
	return m_handle;
}

void LogMessageCallback::Indent()
{
	m_indent++;
}

void LogMessageCallback::Unindent()
{
	m_indent--;
}

void LogMessageCallback::Write(LPWSTR pStr)
{
}

void LogMessageCallback::Show(LPWSTR pStr)
{
}

void LogMessageCallback::Clear()
{
}

//////////////////////////////////////////////////////////////////////
// LogMessage
//////////////////////////////////////////////////////////////////////

LogMessage::LogMessage()
{
	m_lStrLength = 10;
	m_pStr = new wchar_t[m_lStrLength];
	m_pStr[0] = '\0';

	m_indent = 0;
}

LogMessage::~LogMessage()
{
	if (m_pStr)
	{
		if (m_pStr[0] != 0)
			WriteLogMessage();
		delete[] m_pStr;
		m_pStr = NULL;
	}

	CAutoLock callbacksLock(&m_callbacksLock);
	m_callbacks.clear();
}

int LogMessage::AddCallback(LogMessageCallback *callback)
{
	if (callback)
	{
		int handle = callback->GetHandle();

		CAutoLock callbacksLock(&m_callbacksLock);

		vector<LogMessageCallback *>::iterator it = m_callbacks.begin();
		while ( it != m_callbacks.end() )
		{
			if ((*it)->GetHandle() == handle)
			{
				::OutputDebugString("LogMessageCallback added to LogMessage a second time.\n");
				return handle;
			}
			it++;
		}

		m_callbacks.push_back(callback);
		return handle;
	}
	return 0;
}

void LogMessage::RemoveCallback(int handle)
{
	CAutoLock callbacksLock(&m_callbacksLock);

	vector<LogMessageCallback *>::iterator it = m_callbacks.begin();
	while ( it != m_callbacks.end() )
	{
		if ((*it)->GetHandle() == handle)
		{
			m_callbacks.erase(it);
			continue;
		}
		it++;
	}
}

void LogMessage::WriteLogMessage()
{
	if (m_pStr && (m_pStr[0] != 0))
	{
		CAutoLock callbacksLock(&m_callbacksLock);

		if (m_callbacks.size() == 0)
		{
			int i=0;	//break here to find messages that aren't getting written to a file
		}

		vector<LogMessageCallback *>::iterator it = m_callbacks.begin();
		for ( ; it != m_callbacks.end() ; it++ )
		{
			(*it)->Write(m_pStr);
		}
		m_pStr[0] = 0;
	}
}

int LogMessage::Write()
{
	WriteLogMessage();
	return FALSE;
}

int LogMessage::Write(int returnValue)
{
	WriteLogMessage();
	return returnValue;
}

int LogMessage::Show()
{
	return Show(FALSE);
}

int LogMessage::Show(int returnValue)
{
	if (m_pStr && (m_pStr[0] != 0))
	{
		CAutoLock callbacksLock(&m_callbacksLock);

		vector<LogMessageCallback *>::iterator it = m_callbacks.begin();
		for ( ; it != m_callbacks.end() ; it++ )
		{
			(*it)->Show(m_pStr);
			(*it)->Write(m_pStr);
		}
		//WriteLogMessage();
		m_pStr[0] = 0;
	}

	return returnValue;
}

void LogMessage::Indent()
{
	CAutoLock callbacksLock(&m_callbacksLock);

	vector<LogMessageCallback *>::iterator it = m_callbacks.begin();
	for ( ; it != m_callbacks.end() ; it++ )
	{
		(*it)->Indent();
	}
}

void LogMessage::Unindent()
{
	CAutoLock callbacksLock(&m_callbacksLock);

	vector<LogMessageCallback *>::iterator it = m_callbacks.begin();
	for ( ; it != m_callbacks.end() ; it++ )
	{
		(*it)->Unindent();
	}
}

void LogMessage::ClearFile()
{
	CAutoLock callbacksLock(&m_callbacksLock);

	vector<LogMessageCallback *>::iterator it = m_callbacks.begin();
	for ( ; it != m_callbacks.end() ; it++ )
	{
		(*it)->Clear();
	}
}

void LogMessage::LogVersionNumber()
{
	USES_CONVERSION;
	wchar_t filename[MAX_PATH];
	GetCommandExe((LPWSTR)&filename);

	DWORD zeroHandle, size = 0;
	while (TRUE)
	{
		size = GetFileVersionInfoSize(W2T((LPWSTR)&filename), &zeroHandle);
		if (size == 0)
			break;

		LPVOID pBlock = (LPVOID) new char[size];
		if (!GetFileVersionInfo(W2T((LPWSTR)&filename), zeroHandle, size, pBlock))
		{
			delete[] pBlock;
			break;
		}

		struct LANGANDCODEPAGE
		{
			WORD wLanguage;
			WORD wCodePage;
		} *lpTranslate;
		UINT uLen = 0;

		if (!VerQueryValue(pBlock, TEXT("\\VarFileInfo\\Translation"), (LPVOID*)&lpTranslate, &uLen) || (uLen == 0))
		{
			delete[] pBlock;
			break;
		}

		LPWSTR SubBlock = new wchar_t[256];
		swprintf(SubBlock, L"\\StringFileInfo\\%04x%04x\\FileVersion", lpTranslate[0].wLanguage, lpTranslate[0].wCodePage);

		LPSTR lpBuffer;
		UINT dwBytes = 0;
		if (!VerQueryValue(pBlock, W2T(SubBlock), (LPVOID*)&lpBuffer, &dwBytes))
		{
			delete[] pBlock;
			delete[] SubBlock;
			break;
		}
		((*this) << "Build - v" << lpBuffer << "\n").Write();

		delete[] SubBlock;
		delete[] pBlock;
		break;
	}
	if (size == 0)
		((*this) << "Error reading version number\n").Write();
}
/*
void LogMessage::writef(LPWSTR sz,...)
{

    va_list va;
    va_start(va, sz);

	int size = 10;//_vscwprintf(sz, va);
	LPWSTR buf;
	while (TRUE)
	{
		buf = new wchar_t[size+1];
		int result = _vsnwprintf(buf, size, sz, va);
		if (result >= 0)
			break;
		delete[] buf;
		size *= 2;
	}

    va_end(va);

	Require(size);

	_snwprintf(m_pStr, m_lStrLength, L"%s%s", m_pStr, buf);

	Write(FALSE);
}
*/
void LogMessage::showf(LPSTR sz,...)
{
    va_list va;
    va_start(va, sz);

	int size = 80;//_vscwprintf(sz, va);
	LPSTR buf;
	int length;
	while (TRUE)
	{
		buf = new char[size+1];
		length = _vsnprintf(buf, size, sz, va);
		if (length >= 0)
			break;
		delete[] buf;
		size *= 2;
	}

    va_end(va);

	if (length >= (int)m_lStrLength)
	{
		LPWSTR newStr = new wchar_t[length + 1];
		ZeroMemory(newStr, (length+1)*sizeof(wchar_t));
		memcpy(newStr, m_pStr, m_lStrLength);
		m_lStrLength = length + 1;
		delete[] m_pStr;
		m_pStr = newStr;
	}

	_snwprintf(m_pStr, m_lStrLength, L"%s%S", m_pStr, buf);

	delete[] buf;

	Show(FALSE);
}

//Numbers
LogMessage& LogMessage::operator<< (const int& val)
{
	_writef(L"%s%i", m_pStr, val);
	return *this;
}
LogMessage& LogMessage::operator<< (const double& val)
{
	_writef(L"%s%f", m_pStr, val);
	return *this;
}
LogMessage& LogMessage::operator<< (const __int64& val)
{
	_writef(L"%s%i", m_pStr, val);
	return *this;
}

//Characters
LogMessage& LogMessage::operator<< (const char& val)
{
	_writef(L"%s%C", m_pStr, val);
	return *this;
}
LogMessage& LogMessage::operator<< (const wchar_t& val)
{
	_writef(L"%s%c", m_pStr, val);
	return *this;
}

//Strings
LogMessage& LogMessage::operator<< (const LPSTR& val)
{
	_writef(L"%s%S", m_pStr, val);
	return *this;
}
LogMessage& LogMessage::operator<< (const LPWSTR& val)
{
	_writef(L"%s%s", m_pStr, val);
	return *this;
}
LogMessage& LogMessage::operator<< (const LPCSTR& val)
{
	_writef(L"%s%S", m_pStr, val);
	return *this;
}
LogMessage& LogMessage::operator<< (const LPCWSTR& val)
{
	_writef(L"%s%s", m_pStr, val);
	return *this;
}

LPWSTR LogMessage::GetBuffer()
{
	return m_pStr;
}

void LogMessage::_writef(LPWSTR sz,...)
{
	if (wcslen(sz) <= 0)
		return;
    va_list va;
    va_start(va, sz);

	unsigned long currLength = wcslen(m_pStr);

	int size;
	size = _vsnwprintf(m_pStr, m_lStrLength-1, sz, va);
	if (size < 0)
	{
		m_pStr[currLength] = '\0';

		LPWSTR newStr = NULL;
		while (size < 0)
		{
			m_lStrLength *= 2;

			if (newStr)
				delete[] newStr;
			newStr = new wchar_t[m_lStrLength];

			size = _vsnwprintf(newStr, m_lStrLength-1, sz, va);
		}
		delete[] m_pStr;
		m_pStr = newStr;
	}
	m_pStr[size] = '\0';

    va_end(va);
}

//////////////////////////////////////////////////////////////////////
// LogMessageCaller
//////////////////////////////////////////////////////////////////////

LogMessageCaller::LogMessageCaller()
{
	m_pLogCallback = NULL;
}

LogMessageCaller::~LogMessageCaller()
{
}

void LogMessageCaller::SetLogCallback(LogMessageCallback *callback)
{
	if (callback)
	{
		m_pLogCallback = callback;
		log.AddCallback(callback);
	}
}

//////////////////////////////////////////////////////////////////////
// LogMessageIndent
//////////////////////////////////////////////////////////////////////

LogMessageIndent::LogMessageIndent(LogMessage *log)
{
	m_log = log;
	m_log->Indent();
}

LogMessageIndent::~LogMessageIndent()
{
	Release();
}

void LogMessageIndent::Release()
{
	if (m_log)
		m_log->Unindent();
	m_log = NULL;
}


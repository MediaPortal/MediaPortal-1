/*
 * (C) 2024 see Authors.txt
 *
 * This file is part of MPC-HC.
 *
 * MPC-HC is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * MPC-HC is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

#pragma once

#include <windows.h>
#include <tchar.h>
#include <crtdbg.h>
#include <cstdarg>

// The debugging macros of the code: ASSERT and VERIFY as in the Visual C++ runtime, ATLTRACE writes to the debugger in Debug builds,
// TRACE writes nothing. The DirectShow base classes (streams.h) define ASSERT too, and its definition is kept.

#ifndef ASSERT
#define ASSERT(f) _ASSERTE(f)
#endif

#ifndef VERIFY
#ifdef _DEBUG
#define VERIFY(f) ASSERT(f)
#else
#define VERIFY(f) ((void)(f))
#endif
#endif

#ifndef TRACE
#define TRACE(...) ((void)0)
#endif

inline void AtlTrace(const char* fmt, ...)
{
#ifdef _DEBUG
    char buf[1024];
    va_list args;
    va_start(args, fmt);
    _vsnprintf_s(buf, _TRUNCATE, fmt, args);
    va_end(args);
    OutputDebugStringA(buf);
    OutputDebugStringA("\n");
#else
    (void)fmt;
#endif
}

inline void AtlTrace(const wchar_t* fmt, ...)
{
#ifdef _DEBUG
    wchar_t buf[1024];
    va_list args;
    va_start(args, fmt);
    _vsnwprintf_s(buf, _TRUNCATE, fmt, args);
    va_end(args);
    OutputDebugStringW(buf);
    OutputDebugStringW(L"\n");
#else
    (void)fmt;
#endif
}

#ifndef ATLTRACE
#define ATLTRACE(...) AtlTrace(__VA_ARGS__)
#endif

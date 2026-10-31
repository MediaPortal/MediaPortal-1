// stdafx.h: standard system include files

#pragma once

#ifndef VC_EXTRALEAN
#define VC_EXTRALEAN
#endif

#define WINVER 0x0502
#define _WIN32_WINNT 0x0502

#include <windows.h>
#include <tchar.h>
#include <stdio.h>
#include <crtdbg.h>

// MFC used to provide TRACE; the one use is a debug message only.
#ifndef TRACE
#define TRACE(...) ((void)0)
#endif

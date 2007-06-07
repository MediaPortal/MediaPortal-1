#pragma once

#define WIN32_LEAN_AND_MEAN		// Exclude rarely-used stuff from Windows headers
#define _WIN32_WINNT 0x400

#pragma warning (disable : 4995)
#pragma warning (disable : 4996)

//#include <commctrl.h>
#include <atlbase.h>
#include <atlconv.h>

// Windows Header Files:
#include <windows.h>
#include <windowsx.h>
#include <Commdlg.h>
// C RunTime Header Files
#include <stdlib.h>
#include <malloc.h>
#include <memory.h>
#include <tchar.h>
#include <stdio.h>
#include <streams.h>
#include <math.h>
#include <d3d9.h>
#include <vmr9.h>
#include <ddraw.h>


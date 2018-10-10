// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#ifndef WINVER    
#define WINVER 0x0600  
#endif

#ifndef _WIN32_WINNT                    
#define _WIN32_WINNT 0x0600 
#endif      

#ifndef _WIN32_WINDOWS  
#define _WIN32_WINDOWS 0x0600
#endif

#ifndef _WIN32_IE   
#define _WIN32_IE 0x0600 
#endif

#define WIN32_LEAN_AND_MEAN		// Exclude rarely-used stuff from Windows headers
#include <windows.h>
#include <stdio.h>
#include <tchar.h>

//#include <afx.h>
//#include <afxwin.h>         // MFC core and standard components


// TODO: reference additional headers your program requires here

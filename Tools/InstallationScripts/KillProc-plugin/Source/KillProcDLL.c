//
// KillProcDLL. ©2003 by DITMan, based upon the KILL_PROC_BY_NAME function
// programmed by Ravi, reach him at: http://www.physiology.wisc.edu/ravi/
//
// You may use this source code in any of your projects, while you keep this
// header intact, otherwise you CAN NOT use this code.
//
// My homepage:
//    http://petra.uniovi.es/~i6948857/index.php
//


#include <windows.h>
#include <tlhelp32.h>
#include <stdio.h>
//To make it a NSIS Plug-In
#include "exdll.h"

HINSTANCE g_hInstance;
HWND g_hwndParent;

BOOL WINAPI _DllMainCRTStartup(HINSTANCE hInst, ULONG ul_reason_for_call,
                               LPVOID lpReserved)
{
    g_hInstance = hInst;
    return TRUE;
}

int KILL_PROC_BY_NAME(const TCHAR *szToTerminate)
// Terminate all the process "szToTerminate" if it is currently running
// This works for Win/NT/2000/XP/Vista/7/10
// The process name is case-insensitive, i.e. "notepad.exe" and "NOTEPAD.EXE"
// will both work (for szToTerminate)
// Return codes are as follows:
//   0   = Process was successfully terminated
//   602 = Unable to terminate process for some other reason
//   603 = Process was not currently running
//   604 = No permission to terminate process
//   704 = CreateToolhelp32Snapshot failed
{
    HANDLE hProcessSnap;
    PROCESSENTRY32 pe32;
    HANDLE hProcess;
    DWORD iCode = 603;

    hProcessSnap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (hProcessSnap == INVALID_HANDLE_VALUE)
    {
        return 704;
    }
    pe32.dwSize = sizeof(PROCESSENTRY32);
    if (Process32First(hProcessSnap, &pe32)) // Gets first running process
    {
        do
        {
            if (_tcsicmp(szToTerminate, pe32.szExeFile) != 0)
            {
                continue;
            }
            hProcess = OpenProcess( PROCESS_TERMINATE, FALSE, pe32.th32ProcessID );
            if( hProcess == NULL )
            {
                iCode = 604;
                continue;
            }
            if (TerminateProcess(hProcess, 9009))
            {
                iCode = iCode == 603? 0: iCode;
            }
            else
            {
                iCode = 602;
            }
            CloseHandle( hProcess );
        }
        // loop through all running processes looking for process
        while (Process32Next(hProcessSnap, &pe32));
    }
    // clean the snapshot object
    CloseHandle(hProcessSnap);
    return iCode;
}


//
// This is the only exported function, KillProc. It receives the name
// of a process through the NSIS stack. The return-value from the
// KILL_PROC_BY_NAME function is stored in the $R0 variable, so push
// it before calling KillProc function if you don't want to lose the
// data that R0 could contain.
//
// You can call this function in NSIS like this:
//
// KillProcDLL::KillProc "process_name.exe"
//
// example:
//    KillProcDLL::KillProc "msnmsgr.exe"
//  would close MSN Messenger if running, and return 0 in R0
//  if it's not running, it would return 603.
//
//  ---------------------------      ----     ---    --   -

#ifdef __cplusplus
extern "C" {
#endif
void __declspec(dllexport) KillProc(HWND hwndParent, int string_size,
                                    TCHAR *variables, stack_t **stacktop,
                                    extra_parameters *extra)
{
    TCHAR parameter[200];
    TCHAR temp[13];
    int value;
    g_hwndParent = hwndParent;
    EXDLL_INIT();
    {
        popstring(parameter);
        value = KILL_PROC_BY_NAME(parameter);
        wsprintf(temp, _T("%d"), value);
        setuservariable(INST_R0, temp);
    }
}
#ifdef __cplusplus
}
#endif
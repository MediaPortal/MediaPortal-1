// main.cpp

#include <windows.h>
#include <stdio.h>
#include <mmsystem.h>
#include <mmdeviceapi.h>

#include "play.h"
#include "prefs.h"

int do_everything(int argc, LPCWSTR argv[]);

int _cdecl wmain(int argc, LPCWSTR argv[]) {
    HRESULT hr = S_OK;

    hr = CoInitialize(NULL);
    if (FAILED(hr)) {
        printf("CoInitialize failed: hr = 0x%08x", hr);
        return __LINE__;
    }

    int result = do_everything(argc, argv);
    
    CoUninitialize();
    return result;
}

int do_everything(int argc, LPCWSTR argv[]) {
    HRESULT hr = S_OK;

    // parse command line
    CPrefs prefs(argc, argv, hr);
    if (FAILED(hr)) {
        printf("CPrefs::CPrefs constructor failed: hr = 0x%08x\n", hr);
        return __LINE__;
    }
    if (S_FALSE == hr) {
        // nothing to do
        return 0;
    }

    PlayThreadArgs pta = {0};

    pta.hFile = prefs.m_hFile;
    pta.pWfx = prefs.m_pWfx;
    pta.nFrames = prefs.m_nFrames;
    pta.nBytes = prefs.m_nBytes;
    pta.pMMDevice = prefs.m_pMMDevice;
    pta.hr = E_UNEXPECTED;

    HANDLE hThread = CreateThread(
        NULL,
        0,
        PlayThreadFunction,
        &pta,
        0,
        NULL
    );

    if (NULL == hThread) {
        printf("CreateThread failed: GetLastError = %u\n", GetLastError());
        return __LINE__;
    }

    WaitForSingleObject(hThread, INFINITE);

    if (FAILED(pta.hr)) {
        printf("Thread returned failing HRESULT 0x%08x", pta.hr);
        CloseHandle(hThread);
    }
    
    CloseHandle(hThread);
 
    return 0;
}

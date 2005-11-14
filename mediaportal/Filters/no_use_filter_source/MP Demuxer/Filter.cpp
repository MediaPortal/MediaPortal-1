// SampleParser.cpp : Defines the entry point for the DLL application.
//
// Copyright (c) GDCL 2004. All Rights Reserved. 
// You are free to re-use this as the basis for your own filter development,
// provided you retain this copyright notice in the source.
// http://www.gdcl.co.uk

#include "stdafx.h"
#include "TSMuxSplitter.h"

CFactoryTemplate g_Templates[] = {
    {
        TSMuxSplitter::m_sudFilter.strName,
        TSMuxSplitter::m_sudFilter.clsID,
        TSMuxSplitter::CreateInstance,
        NULL,
        &TSMuxSplitter::m_sudFilter
    }
	,
	{ 
		L"Splitter PropPage",
		&__uuidof(MPDSTProperties), 
		MPDSTProperties::CreateInstance 
	}
};
int g_cTemplates = 2;

// self-registration entrypoint
STDAPI DllRegisterServer()
{
    // base classes will handle registration using the factory template table
    HRESULT hr = AMovieDllRegisterServer2(true);

    return hr;
}

STDAPI DllUnregisterServer()
{
    // base classes will handle de-registration using the factory template table
    HRESULT hr = AMovieDllRegisterServer2(false);

    return hr;
}

// if we declare the correct C runtime entrypoint and then forward it to the DShow base
// classes we will be sure that both the C/C++ runtimes and the base classes are initialized
// correctly
extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);
BOOL WINAPI DllMain(HANDLE hDllHandle, DWORD dwReason, LPVOID lpReserved)
{
    return DllEntryPoint(reinterpret_cast<HINSTANCE>(hDllHandle), dwReason, lpReserved);
}

// test.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "dxerr9.h"

BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
    return TRUE;
}




extern "C" DXERR9_API LPCWSTR GetDXErrorString(HRESULT DXError)
{
	return _bstr_t(::DXGetErrorString9(DXError));
}


extern "C" DXERR9_API LPCWSTR GetDXErrorDescription(HRESULT DXError)
{
	return _bstr_t(::DXGetErrorDescription9(DXError));
}

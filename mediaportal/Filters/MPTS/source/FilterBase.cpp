/*
	MediaPortal TS-SourceFilter by Agree

	
*/

#include <streams.h>
#include <initguid.h>

#include "MPTSFilter.h"

#define FilterName	L"MediaPortal TS-SourceFilter"

const AMOVIESETUP_MEDIATYPE acceptPinTypes =
{
	&MEDIATYPE_Stream,                  // major type
	&MEDIASUBTYPE_MPEG2_TRANSPORT      // minor type
};

const AMOVIESETUP_PIN outputPin =
{
	L"Output",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptPinTypes
};

const AMOVIESETUP_FILTER MPTSFilter =
{
	&CLSID_MPTSFilter,FilterName,MERIT_DO_NOT_USE,1,&outputPin
};

CFactoryTemplate g_Templates[] =
{
	{FilterName,&CLSID_MPTSFilter,CMPTSFilter::CreateInstance,NULL,&MPTSFilter},
};

int g_cTemplates = sizeof(g_Templates) / sizeof(g_Templates[0]);



STDAPI DllRegisterServer()
{
	return AMovieDllRegisterServer2( TRUE );
}

STDAPI DllUnregisterServer()
{
	return AMovieDllRegisterServer2( FALSE );
}

//
// DllEntryPoint
//
extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);

BOOL APIENTRY DllMain(HANDLE hModule,
					  DWORD  dwReason,
					  LPVOID lpReserved)
{
	return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);
}



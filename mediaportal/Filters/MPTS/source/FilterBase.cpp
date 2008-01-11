/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
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
	BOOL result=DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);
	return result;
}



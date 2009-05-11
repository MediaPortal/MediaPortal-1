/* 
 *      Copyright (C) 2006 Team MediaPortal
 *      http://www.team-mediaportal.com
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

#include "stdafx.h"


#include <d3d9.h>
#include <d3dx9.h>
#include <d3d9types.h>
#include <strsafe.h>

extern "C" __declspec(dllexport) HRESULT __stdcall VideoSurfaceToRGBSurface(IDirect3DSurface9* source, IDirect3DSurface9* dest)
{
	IDirect3DDevice9* device = NULL;
	HRESULT hr = source->GetDevice(&device);
	if(!FAILED(hr)){
		hr = device->StretchRect(source,NULL,dest,NULL,D3DTEXF_NONE);
	}
	//delete device;
	return hr;
}


#ifdef _MANAGED
#pragma managed(push, off)
#endif

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
    return TRUE;
}

#ifdef _MANAGED
#pragma managed(pop)
#endif


// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the DSHOWHELPER_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// DSHOWHELPER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.

#ifdef DSHOWHELPER_EXPORTS
#define DSHOWHELPER_API __declspec(dllexport)
#else
#define DSHOWHELPER_API __declspec(dllimport)
#endif

#include <dxva2api.h>
#include <dwmapi.h>

// This class is exported from the dshowhelper.dll
class DSHOWHELPER_API Cdshowhelper 
{
public:
	Cdshowhelper(void);
};

extern DSHOWHELPER_API int ndshowhelper;

DSHOWHELPER_API int fndshowhelper(void);

typedef HRESULT __stdcall TDXVA2CreateDirect3DDeviceManager9(__out UINT* pResetToken,__deref_out IDirect3DDeviceManager9** ppDeviceManager);
extern TDXVA2CreateDirect3DDeviceManager9* m_pDXVA2CreateDirect3DDeviceManager9;

typedef HRESULT __stdcall TMFCreateVideoSampleFromSurface(__in_opt IUnknown* pUnkSurface,__deref_out_opt IMFSample** ppSample);
extern TMFCreateVideoSampleFromSurface* m_pMFCreateVideoSampleFromSurface;

typedef HRESULT __stdcall TMFCreateVideoMediaType(__out_opt MFVIDEOFORMAT* videoformat, IMFVideoMediaType **videotype);
extern TMFCreateVideoMediaType* m_pMFCreateVideoMediaType;

typedef HRESULT __stdcall TMFCreateMediaType(IMFMediaType** ppIMediaType);
extern TMFCreateMediaType* m_pMFCreateMediaType;

typedef HRESULT __stdcall TDwmEnableMMCSS(BOOL fEnableMMCSS);
extern TDwmEnableMMCSS* m_pDwmEnableMMCSS;

typedef double __stdcall TW7GetRefreshRate();
extern TW7GetRefreshRate* m_pW7GetRefreshRate;

HRESULT MyGetService(IUnknown* punkObject, REFGUID guidService, REFIID riid, LPVOID* ppvObject);

void Log(const char *fmt, ...);
void LogRotate();
HRESULT __fastcall UnicodeToAnsi(LPCOLESTR pszW, LPSTR* ppszA);
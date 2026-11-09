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

#include "StdString.h"
#include <pdh.h>

class MPEVRCustomPresenter;

typedef HRESULT (WINAPI* D3DXCreateLinePtr) (LPDIRECT3DDEVICE9   pDevice, LPD3DXLINE* ppLine);

typedef HRESULT (WINAPI* D3DXCreateFontPtr)(
								LPDIRECT3DDEVICE9	pDevice,  
								int			    Height,
								UINT			  Width,
								UINT			  Weight,
								UINT			  MipLevels,
								bool			  Italic,
								DWORD			  CharSet,
								DWORD			  OutputPrecision,
								DWORD			  Quality,
								DWORD			  PitchAndFamily,
								LPCWSTR	    pFaceName,
								LPD3DXFONT* ppFont);

class StatsRenderer
{
public:
  StatsRenderer(MPEVRCustomPresenter* presenter, IDirect3DDevice9* device);
  ~StatsRenderer(void);

  void DrawText(const RECT &rc, const CStdString &strText);
  HRESULT DrawRect(DWORD color, DWORD alpha, const RECT &rect);
  
  void DrawTearingTest(Com::SmartPtr<IDirect3DSurface9> pSurface);
  void DrawStats();
  void VideSizeChanged();

private:

  HINSTANCE GetD3X9Dll();

  // D3D lib
  int m_nDXSdkRelease;
  CStdString m_strD3DX9Version;

  // for drawing stats
  Com::SmartPtr<ID3DXLine>    m_pLine;
  Com::SmartPtr<ID3DXFont>    m_pFont;
  Com::SmartPtr<ID3DXSprite>  m_pSprite;

  // function pointers to D3D methods
  D3DXCreateLinePtr		m_pD3DXCreateLine;
  D3DXCreateFontPtr		m_pD3DXCreateFont;
  HRESULT (__stdcall *m_pD3DXCreateSprite)(LPDIRECT3DDEVICE9 pDevice, LPD3DXSPRITE * ppSprite);

  double m_TextScale;
  int m_nTearingPos;
  HINSTANCE	m_hD3DX9Dll;

  // EVR presenter owns this 
  IDirect3DDevice9* m_pD3DDev;
  
  // Our friend class that contains the stats to be rendered
  MPEVRCustomPresenter* m_pPresenter;

  // Presenter notices us when video frame size changes
  bool m_bVideoSizeChanged;

	PDH_HQUERY m_cpuQuery;
	PDH_HCOUNTER m_cpuTotal;
	int m_counter;
	PDH_FMT_COUNTERVALUE counterVal;
	double  m_cpuTotalUsage;
};

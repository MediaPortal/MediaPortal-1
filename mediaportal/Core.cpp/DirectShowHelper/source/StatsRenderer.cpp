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

#include "stdafx.h"

#include <streams.h>
#include <d3dx9.h>

#include "statsrenderer.h"
#include "evrcustompresenter.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

#define countof(array) (sizeof(array)/sizeof(array[0]))

#pragma pack(push, 1)
template<int texcoords>
struct MYD3DVERTEX {float x, y, z, rhw; struct {float u, v;} t[texcoords];};
template<>
struct MYD3DVERTEX<0> 
{
  float x, y, z, rhw; 
  DWORD Diffuse;
};
#pragma pack(pop)

// For more details for memory leak detection see the alloctracing.h header
//#include "..\..\alloctracing.h"


StatsRenderer::StatsRenderer(MPEVRCustomPresenter* presenter, IDirect3DDevice9* device):
  m_pPresenter(presenter),
  m_pD3DDev(device),
  m_pD3DXCreateLine(NULL),
  m_pD3DXCreateFont(NULL),
  m_pD3DXCreateSprite(NULL),
  m_hD3DX9Dll(NULL),
  m_pFont(NULL),
  m_pSprite(NULL),
  m_pLine(NULL)
{
  HINSTANCE hDll;
  hDll = GetD3X9Dll();
  if(hDll)
  {
    (FARPROC&)m_pD3DXCreateLine    = GetProcAddress(hDll, "D3DXCreateLine");
    (FARPROC&)m_pD3DXCreateFont    = GetProcAddress(hDll, "D3DXCreateFontW");
    (FARPROC&)m_pD3DXCreateSprite = GetProcAddress(hDll, "D3DXCreateSprite");    
  }

  if (m_pD3DXCreateSprite)
  {
    m_pD3DXCreateSprite( m_pD3DDev, &m_pSprite);
  }

  if (m_pD3DXCreateLine)
  {
    m_pD3DXCreateLine(m_pD3DDev, &m_pLine);
  }
}


StatsRenderer::~StatsRenderer(void)
{
  m_pSprite = NULL;
  m_pLine = NULL;
}


static HRESULT DrawRect(CComPtr<IDirect3DDevice9> pD3DDev, MYD3DVERTEX<0> v[4])
{
	if (!pD3DDev)
  {
    return E_POINTER;
  }

  do
	{
    HRESULT hr;
    DWORD alphaBlend;

    hr = pD3DDev->GetRenderState(D3DRS_ALPHABLENDENABLE, &alphaBlend);
    hr = pD3DDev->SetRenderState(D3DRS_ALPHABLENDENABLE, TRUE);
    hr = pD3DDev->SetRenderState(D3DRS_SRCBLEND, D3DBLEND_SRCALPHA); 
    hr = pD3DDev->SetRenderState(D3DRS_DESTBLEND, D3DBLEND_INVSRCALPHA); 
    
    //D3DRS_COLORVERTEX 
    hr = pD3DDev->SetRenderState(D3DRS_ALPHATESTENABLE, FALSE); 
    hr = pD3DDev->SetRenderState(D3DRS_SCISSORTESTENABLE, FALSE); 
    hr = pD3DDev->SetRenderState(D3DRS_COLORWRITEENABLE, 
      D3DCOLORWRITEENABLE_ALPHA|D3DCOLORWRITEENABLE_BLUE
      |D3DCOLORWRITEENABLE_GREEN|D3DCOLORWRITEENABLE_RED); 

    hr = pD3DDev->SetFVF(D3DFVF_XYZRHW | D3DFVF_TEX0 | D3DFVF_DIFFUSE);

    MYD3DVERTEX<0> tmp = v[2]; v[2] = v[3]; v[3] = tmp;
    hr = pD3DDev->DrawPrimitiveUP(D3DPT_TRIANGLEFAN, 2, v, sizeof(v[0]));	

    hr = pD3DDev->SetRenderState(D3DRS_ALPHABLENDENABLE, alphaBlend);
    return S_OK;
    }
	while(0);

  return E_FAIL;
}


void StatsRenderer::DrawText(const RECT &rc, const CString &strText)
{
  D3DXCOLOR Color1( 1.0f, 0.2f, 0.2f, 1.0f );
  D3DXCOLOR Color0( 0.0f, 0.0f, 0.0f, 1.0f );
  RECT Rect1 = rc;
  RECT Rect2 = rc;

  OffsetRect(&Rect2 , 1, 1);
  m_pFont->DrawText( m_pSprite, strText, -1, &Rect2, DT_NOCLIP, Color0);
  m_pFont->DrawText( m_pSprite, strText, -1, &Rect1, DT_NOCLIP, Color1);
}


HRESULT StatsRenderer::DrawRect(DWORD color, DWORD alpha, const CRect &rect)
{
  DWORD Color = D3DCOLOR_ARGB(alpha, GetRValue(color), GetGValue(color), GetBValue(color));
  MYD3DVERTEX<0> v[] =
  {
    {float(rect.left),  float(rect.top),    0.5f, 2.0f, Color},
    {float(rect.right), float(rect.top),    0.5f, 2.0f, Color},
    {float(rect.left),  float(rect.bottom), 0.5f, 2.0f, Color},
    {float(rect.right), float(rect.bottom), 0.5f, 2.0f, Color},
  };

  for(int i = 0; i < countof(v); i++)
  {
    v[i].x -= 0.5;
    v[i].y -= 0.5;
  }

  return ::DrawRect(m_pD3DDev, v);
}


void StatsRenderer::DrawStats()
{
  if (m_bVideoSizeChanged)
  {
    m_pFont = NULL;
    m_bVideoSizeChanged = false;
  }
  
  // Font is created when it is needed for the first time so that video dimensions can be used to scale the text
  if (!m_pFont && m_pD3DXCreateFont)
  {
    int minSize = 1080;
    int CurrentSize = min(m_pPresenter->m_iVideoWidth, minSize);
    double Scale = double(CurrentSize) / double(minSize);
    m_TextScale = Scale;
    m_pD3DXCreateFont( m_pD3DDev,
                      UINT(-24.0*Scale),       // Height
                      UINT(-11.0*Scale),       // Width
                      CurrentSize < 800 ? FW_NORMAL : FW_BOLD,
                      0,                      // MipLevels, 0 = autogen mipmaps
                      FALSE,                  // Italic
                      DEFAULT_CHARSET,        
                      OUT_CHARACTER_PRECIS,   // OutputPrecision
                      ANTIALIASED_QUALITY,  
                      FIXED_PITCH | FF_DONTCARE,
                      L"Lucida Console",
                      &m_pFont);
  }

  LONGLONG llMaxJitter = m_pPresenter->m_MaxJitter;
  LONGLONG llMinJitter = m_pPresenter->m_MinJitter;
  LONGLONG llMaxSyncOffset = m_pPresenter->m_MaxSyncOffset;
  LONGLONG llMinSyncOffset = m_pPresenter->m_MinSyncOffset;

  RECT rc = {m_pPresenter->m_iVideoWidth/32, m_pPresenter->m_iVideoHeight/8, m_pPresenter->m_iVideoWidth, m_pPresenter->m_iVideoHeight};
  if (m_pFont && m_pSprite)
  {
    m_pSprite->Begin(D3DXSPRITE_ALPHABLEND);
    CString  strText;
    int TextHeight = int(25.0*m_TextScale + 0.5);
    int BlankHeight = int(10.0*m_TextScale + 0.5);
    
    strText.Format("Display: %d x %d @ %.6f Hz | Meas rfsh: %.6f Hz | MaxLine: %d | PCD: %.6f", 
      m_pPresenter->m_displayMode.Width, m_pPresenter->m_displayMode.Height,
      m_pPresenter->m_dD3DRefreshRate, 1000.0/m_pPresenter->m_displayParams.dEstRefreshCycle, m_pPresenter->m_displayParams.maxScanLine,
      m_pPresenter->m_fPCDMean);
    DrawText(rc, strText);
    OffsetRect(&rc, 0, TextHeight);

    strText.Format("Video: %d x %d %d:%d | Act FPS: %.4f (red)| Drawn: %d | Drop: %d", 
      m_pPresenter->m_iVideoWidth, m_pPresenter->m_iVideoHeight, 
      m_pPresenter->m_iARX, m_pPresenter->m_iARY, 
      10000000.0 / m_pPresenter->m_fJitterMean, m_pPresenter->m_iFramesDrawn, m_pPresenter->m_iFramesDropped);
    DrawText(rc, strText);
    OffsetRect(&rc, 0, TextHeight);

    OffsetRect(&rc, 0, BlankHeight); // Extra "line feed"

    strText.Format("Render time (grn): %+5.1f ms | NST: %+3.1f ms | AveRNST: %+3.1f ms | FrRat: %d | Stall: %+3.1f ms", 
      m_pPresenter->m_fSyncOffsetAvr/10000.0, m_pPresenter->m_llLastCFPts/10000.0, 
      m_pPresenter->m_fCFPMean/10000.0, m_pPresenter->m_frameRateRatio, m_pPresenter->m_stallTime/10000.0);
    DrawText(rc, strText);
    OffsetRect(&rc, 0, TextHeight);

    strText.Format("Raster offset (ylw): %5.2f ms | SOP: %4d | EOP: %4d | Locked: %d | Derr: %5.2f ms | Q: %d",
      m_pPresenter->m_rasterSyncOffset, m_pPresenter->m_LastStartOfPaintScanline, m_pPresenter->m_LastEndOfPaintScanline, 
      (int)m_pPresenter->m_DetectedLock, m_pPresenter->m_lastDelayErr/10000.0, (m_pPresenter->m_qScheduledSamples.Count()));
    DrawText(rc, strText);
    OffsetRect(&rc, 0, TextHeight);

    strText.Format("Rptd FPS: %.3f | Detd FPS: %.3f | DetFrT_SD: %+5.3f ms | DetSDur: %+5.3f ms",  
      ((m_pPresenter->m_rtTimePerFrame > 0) ? (10000000.0/m_pPresenter->m_rtTimePerFrame) : 0), 
      ((m_pPresenter->m_DetFrameTimeAve > 0) ? (1.0/(m_pPresenter->m_DetFrameTimeAve)) : 0),
      (m_pPresenter->m_DetectedFrameTimeStdDev/10000.0), (m_pPresenter->m_SampDuration/10000.0) );
    DrawText(rc, strText);
    OffsetRect(&rc, 0, TextHeight);

    if (m_pPresenter->m_pAVSyncClock) //Stats for MP Audio Renderer
    {
      OffsetRect(&rc, 0, BlankHeight); // Extra "line feed"
  
      strText.Format("Detd bias: %.7f | BiasAdj: %d | AudAdj: %.6f | AvePhDiff: %.6f | NumAdj: %d", 
        m_pPresenter->m_dBias, m_pPresenter->m_bBiasAdjustmentDone, m_pPresenter->m_dVariableFreq, 
        m_pPresenter->m_avPhaseDiff, m_pPresenter->m_iClockAdjustmentsDone);
      DrawText(rc, strText);
      OffsetRect(&rc, 0, TextHeight);

      CLOCKDATA clockData;
      m_pPresenter->m_pAVSyncClock->GetClockData(&clockData);

      strText.Format("HW vs system: %.7fx HW vs. sys: %+.3f ms resampling adj.: %.7f Drift: %+.3f ms", 
        clockData.driftMultiplier, clockData.driftHWvsSystem, clockData.resamplingAdjustment, clockData.currentDrift);
      DrawText(rc, strText);
      OffsetRect(&rc, 0, TextHeight);
    }

    if (m_pPresenter->m_numFilters > 0) 
    {
      OffsetRect(&rc, 0, BlankHeight); // Extra "line feed"
      
      strText.Format("");
      for (int i=0; i < 3 && i < m_pPresenter->m_numFilters; i++)
      {
        strText += m_pPresenter->m_filterNames[i];
        if (m_pPresenter->m_numFilters > (i+1)) strText += ", ";
      }
      DrawText(rc, strText);
      OffsetRect(&rc, 0, TextHeight);
    }

    if (m_pPresenter->m_numFilters > 3) 
    {
      strText.Format("");
      for (int i=3; i < 6 && i < m_pPresenter->m_numFilters; i++)
      {
        strText += m_pPresenter->m_filterNames[i];
        if (m_pPresenter->m_numFilters > (i+1)) strText += ", ";
      }
      DrawText(rc, strText);
      OffsetRect(&rc, 0, TextHeight);
    }

    if (m_pPresenter->m_numFilters > 6)
    {
      strText.Format("");
      for (int i=6; i < FILTER_LIST_SIZE && i < m_pPresenter->m_numFilters; i++)
      {
        strText += m_pPresenter->m_filterNames[i];
        if (m_pPresenter->m_numFilters > (i+1)) strText += ", ";
      }
      DrawText(rc, strText);
      OffsetRect(&rc, 0, TextHeight);
    }
    
//    strText.Format("QPut: %d | QNoPut: %d | QPop: %d | QPut-QPop: %d | BadSTimCnt: %d", 
//       m_pPresenter->m_qGoodPutCnt, m_pPresenter->m_qBadPutCnt, m_pPresenter->m_qGoodPopCnt, 
//       (m_pPresenter->m_qGoodPutCnt - m_pPresenter->m_qGoodPopCnt), m_pPresenter->m_qBadSampTimCnt );
//    DrawText(rc, strText);
//    OffsetRect(&rc, 0, TextHeight);

    OffsetRect(&rc, 0, BlankHeight); // Extra "line feed"
    m_pSprite->End();
  }

  if (m_pLine)
  {
    D3DXVECTOR2  Points[NB_JITTER];
    int nIndex;

    int DrawWidth = 625;
    int DrawHeight = 250;
    int Alpha = 80;
    int StartX = rc.left;
    int StartY = rc.top;

    DrawRect(RGB(0, 0, 0), Alpha, CRect(StartX, StartY, StartX + DrawWidth, StartY + DrawHeight));
    m_pLine->SetWidth(2.5); 
    m_pLine->SetAntialias(1);
    m_pLine->Begin();

    for (int i = 0; i <= DrawHeight; i += 5)
    {
      Points[0].x = (FLOAT)StartX;
      Points[0].y = (FLOAT)(StartY + i);
      Points[1].x = (FLOAT)(StartX + ((i + 25) % 25 ? 50 : 625));
      Points[1].y = (FLOAT)(StartY + i);
      m_pLine->Draw (Points, 2, D3DCOLOR_XRGB(100, 100, 255));
    }

    for (int i = 0; i < DrawWidth; i += 125) // Every 25:th sample
    {
      Points[0].x = (FLOAT)(StartX + i);
      Points[0].y = (FLOAT)(StartY + DrawHeight / 2);
      Points[1].x = (FLOAT)(StartX + i);
      Points[1].y = (FLOAT)(StartY + DrawHeight / 2 + 10);
      m_pLine->Draw (Points, 2, D3DCOLOR_XRGB(100, 100, 255));
    }

    // jitter curve
    for (int i = 0; i < NB_JITTER; i++)
    {
      nIndex = (m_pPresenter->m_nNextJitter + 1 + i) % NB_JITTER;
      if (nIndex < 0)
      {
        nIndex += NB_JITTER;
      }
      double Jitter = m_pPresenter->m_pllJitter[nIndex] - m_pPresenter->m_fJitterMean;
      Points[i].x  = (FLOAT)(StartX + (i * 5));
      double offsetY = StartY + Jitter / 3000 + 125;
      if (offsetY < StartY) offsetY = StartY;
      if (offsetY > StartY+DrawHeight) offsetY = StartY+DrawHeight;
      Points[i].y = (FLOAT)(offsetY);
    }    
    m_pLine->Draw(Points, NB_JITTER, D3DCOLOR_XRGB(255, 100, 100));

    // sync offset
    for (int i = 0; i < NB_JITTER; i++)
    {
      nIndex = (m_pPresenter->m_nNextSyncOffset + 1 + i) % NB_JITTER;
      if (nIndex < 0)
      {
        nIndex += NB_JITTER;
      }
      Points[i].x = (FLOAT)(StartX + (i * 5));
      double offsetY = StartY + (double)m_pPresenter->m_pllSyncOffset[nIndex] / 3000 + 125 ;
      if (offsetY < StartY) offsetY = StartY;
      if (offsetY > StartY+DrawHeight) offsetY = StartY + DrawHeight;
      Points[i].y = (FLOAT)(offsetY);
    }    
    m_pLine->Draw(Points, NB_JITTER, D3DCOLOR_XRGB(100, 200, 100));

    // raster sync offset (drawn at the bottom to keep the graph cleaner)
    for (int i = 0; i < NB_JITTER; i++)
    {
      // m_pllRasterSyncOffset is updated always at the same time as m_fJitterMean->m_nNextSyncOffset
      nIndex = (m_pPresenter->m_nNextSyncOffset + 1 + i) % NB_JITTER;
      if (nIndex < 0)
      {
        nIndex += NB_JITTER;
      }
      Points[i].x = (FLOAT)(StartX + (i * 5));
      double offsetY = StartY - m_pPresenter->m_pllRasterSyncOffset[nIndex] * 5 + DrawHeight;
      if (offsetY < StartY) offsetY = StartY;
      if (offsetY > StartY+DrawHeight) offsetY = StartY + DrawHeight;
      Points[i].y = (FLOAT)(offsetY);
     }    
    m_pLine->Draw(Points, NB_JITTER, D3DCOLOR_XRGB(220, 220, 100));
    m_pLine->End();
  }
  
}


void StatsRenderer::DrawTearingTest(CComPtr<IDirect3DSurface9> pSurface)
{
  RECT rcTearing;

  UINT height = m_pPresenter->m_iVideoHeight;
  if (height > m_pPresenter->m_displayMode.Height)
  {
    height = m_pPresenter->m_displayMode.Height;
  }

  UINT width = m_pPresenter->m_iVideoWidth;
  if (width > m_pPresenter->m_displayMode.Width)
  {
    width = m_pPresenter->m_displayMode.Width;
  }
  
  rcTearing.left = m_nTearingPos;
  rcTearing.top = 0;
  rcTearing.right = rcTearing.left + 4;
  rcTearing.bottom = height/8;
  m_pD3DDev->ColorFill(pSurface, &rcTearing, D3DCOLOR_ARGB(255,255,255,255));

  rcTearing.left = (rcTearing.right + 15) % width;
  rcTearing.right  = rcTearing.left + 4;
  m_pD3DDev->ColorFill(pSurface, &rcTearing, D3DCOLOR_ARGB(255,100,100,100));

  m_nTearingPos = (m_nTearingPos + 7) % width;
}


void StatsRenderer::VideSizeChanged()
{
  m_bVideoSizeChanged = true;
}


HINSTANCE StatsRenderer::GetD3X9Dll()
{
  if (m_hD3DX9Dll == NULL)
  {
    m_nDXSdkRelease = 0;
    // Try to load latest DX9 available
    for (int i = D3DX_SDK_VERSION; i > 23; i--)
    {
      // From MPC-HC Prevent using DXSDK April 2007 (crash sometimes during shader compilation)
      // We don't use shaders, but sounds like a buggy DLL version...
      if (i != 33)  
      {
        m_strD3DX9Version.Format(_T("d3dx9_%d.dll"), i);
        m_hD3DX9Dll = LoadLibrary (m_strD3DX9Version);
        if (m_hD3DX9Dll) 
        {
          m_nDXSdkRelease = i;
          break;
        }
      }
    }
  }
  return m_hD3DX9Dll;
}

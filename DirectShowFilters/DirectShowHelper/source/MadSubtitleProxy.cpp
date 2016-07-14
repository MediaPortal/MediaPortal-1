// Copyright (C) 2005-2015 Team MediaPortal
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

#include "StdAfx.h"

#include <initguid.h>
#include <streams.h>
#include <d3dx9.h>
#include "dshowhelper.h"

#include "MadSubtitleProxy.h"
#include "madpresenter.h"

MadSubtitleProxy::MadSubtitleProxy(IVMR9Callback* pCallback, IMediaControl* pMediaControl) :
  CUnknown(NAME("MadSubtitleProxy"), nullptr),
  m_pCallback(pCallback),
  m_pMediaControl(pMediaControl)
{
  Log("MadSubtitleProxy::Constructor() - instance 0x%x", this);
  CAutoLock cAutoLock(this);
}

MadSubtitleProxy::~MadSubtitleProxy()
{
  Log("MadSubtitleProxy::Destructor() - instance 0x%x", this);
  CAutoLock cAutoLock(this);
}

HRESULT MadSubtitleProxy::SetDevice(IDirect3DDevice9* device)
{
  CAutoLock cAutoLock(this);
  Log("MadSubtitleProxy::SetDevice() device : 0x:%x", device);

  m_deviceState.SetDevice(device);
  m_pMadD3DDev = device;

  if (!m_pMadD3DDev)
  {
    deviceNULL++;
    counterBeforeProcessOSD = 0;
  }

  // if we get many D3D device to null, seend a callback to stop the playback.
  if (!m_pMadD3DDev && deviceNULL > 3)
  {
    m_pCallback->ForceInitialize();
  }
  return S_OK;
}

HRESULT MadSubtitleProxy::Render(REFERENCE_TIME frameStart, int left, int top, int right, int bottom, int width, int height)
{
  CAutoLock cAutoLock(this);

  if (!m_pMadD3DDev)
  {
    Log("MadSubtitleProxy::SetDevice() Render : 0x:%x", m_pMadD3DDev);
    m_pMediaControl->Stop();
    Log("MadSubtitleProxy::SetDevice() Render ImediaControl Stop");
    return S_OK;
  }

  if (m_pCallback)
  {
    if (m_pMadD3DDev && counterBeforeProcessOSD < 10)
    {
      counterBeforeProcessOSD++;
      m_pMediaControl->Run();
      Log("MadSubtitleProxy::Render() counter before processing OSD callback : %u", counterBeforeProcessOSD);
    }
    
    // Let at least 7 render pass to permit to be on a correct D3D device
    if (m_pMadD3DDev && counterBeforeProcessOSD >= 10)
    {
      if (!GetNewDevice())
      {
        Log("MadSubtitleProxy::Render() SetNewDevice for D3D and subtitle : 0x:%x", m_pMadD3DDev);
        m_pMediaControl->Stop();
        m_pCallback->ForceOsdUpdate(true);
        m_pCallback->SetSubtitleDevice(reinterpret_cast<DWORD>(m_pMadD3DDev));
        SetNewDevice(true);
        m_pMediaControl->Run();
      }

      m_deviceState.Store();
      SetupMadDeviceState();

      m_pCallback->RenderSubtitle(frameStart, left, top, right, bottom, width, height);

      m_deviceState.Restore();
    }
  }
  return S_OK;
}

HRESULT MadSubtitleProxy::SetupMadDeviceState()
{
  HRESULT hr = E_UNEXPECTED;

  if (!m_pMadD3DDev)
    return S_OK;

  RECT newScissorRect;
  newScissorRect.bottom = 1080;
  newScissorRect.top = 0;
  newScissorRect.left = 0;
  newScissorRect.right = 1920;

  if (m_pMadD3DDev)
  {
    if (FAILED(hr = m_pMadD3DDev->SetScissorRect(&newScissorRect)))
      return hr;

    if (FAILED(hr = m_pMadD3DDev->SetVertexShader(NULL)))
      return hr;

    if (FAILED(hr = m_pMadD3DDev->SetPixelShader(NULL)))
      return hr;

    if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_ALPHABLENDENABLE, TRUE)))
      return hr;

    if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_CULLMODE, D3DCULL_NONE)))
      return hr;

    if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_LIGHTING, FALSE)))
      return hr;

    if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_ZENABLE, FALSE)))
      return hr;

    if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_SRCBLEND, D3DBLEND_ONE)))
      return hr;

    if (FAILED(hr = m_pMadD3DDev->SetRenderState(D3DRS_DESTBLEND, D3DBLEND_SRCALPHA)))
      return hr;
  }

  return hr;
}

void MadSubtitleProxy::SetNewDevice(bool pnewDevice)
{
  m_pNewDevice = pnewDevice;
}

bool MadSubtitleProxy::GetNewDevice()
{
  return (m_pNewDevice);
}

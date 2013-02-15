// Copyright (C) 2005-2012 Team MediaPortal
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
#include "EffectStateManager.h"

CPureDeviceStateManager::CPureDeviceStateManager(LPDIRECT3DDEVICE9 pDevice) :
  m_pDevice(pDevice),
  m_lRef(0),
  m_cacheRenderStates(D3DRS_BLENDOPALPHA),
  m_vecCacheSamplerStates(CACHED_STAGES, samplerStageCache(D3DSAMP_DMAPOFFSET)),
  m_vecCacheTextureStates(CACHED_STAGES, textureStateStageCache(D3DTSS_CONSTANT))
{
  m_pDevice->AddRef();
}

CPureDeviceStateManager::~CPureDeviceStateManager()
{
  m_pDevice->Release();
}

HRESULT CPureDeviceStateManager::QueryInterface(REFIID iid, LPVOID* ppv)
{
  if (iid == IID_IUnknown || iid == IID_ID3DXEffectStateManager)
    *ppv = static_cast<ID3DXEffectStateManager*>(this);
  else
  {
    *ppv = NULL;
    return E_NOINTERFACE;
  }

  reinterpret_cast<IUnknown*>(this)->AddRef();
  return S_OK;
}

ULONG CPureDeviceStateManager::AddRef()
{
  return (ULONG)InterlockedIncrement(&m_lRef);
}

ULONG CPureDeviceStateManager::Release()
{
  if (InterlockedDecrement(&m_lRef) == 0L)
  {
    delete this;
    return 0L;
  }

  return m_lRef;
}

void CPureDeviceStateManager::DirtyCachedValues()
{
  m_cacheRenderStates.dirtyall();

  vector <samplerStageCache>::iterator it_samplerStages;
  for(it_samplerStages = m_vecCacheSamplerStates.begin(); it_samplerStages != m_vecCacheSamplerStates.end(); it_samplerStages++)
  {
    (*it_samplerStages).dirtyall();
  }

  vector <textureStateStageCache>::iterator it_textureStages;
  for(it_textureStages = m_vecCacheTextureStates.begin(); it_textureStages != m_vecCacheTextureStates.end(); it_textureStages++)
  {
    (*it_textureStages).dirtyall();
  }
}

HRESULT CPureDeviceStateManager::SetRenderState(D3DRENDERSTATETYPE d3dRenderState, DWORD dwValue)
{
  if (m_cacheRenderStates.set_val(d3dRenderState, dwValue))
    return m_pDevice->SetRenderState(d3dRenderState, dwValue);

  return S_OK;
}

HRESULT CPureDeviceStateManager::SetSamplerState(DWORD dwStage, D3DSAMPLERSTATETYPE d3dSamplerState, DWORD dwValue)
{
  if (dwStage >= CACHED_STAGES || m_vecCacheSamplerStates[dwStage].set_val(d3dSamplerState, dwValue))
    return m_pDevice->SetSamplerState(dwStage, d3dSamplerState, dwValue);

  return S_OK;
}

HRESULT CPureDeviceStateManager::SetTextureStageState(DWORD dwStage, D3DTEXTURESTAGESTATETYPE d3dTextureStageState, DWORD dwValue)
{
  if (dwStage >= CACHED_STAGES || m_vecCacheTextureStates[dwStage].set_val(d3dTextureStageState, dwValue))
    return m_pDevice->SetTextureStageState(dwStage, d3dTextureStageState, dwValue);

  return S_OK;
}

HRESULT CPureDeviceStateManager::SetTexture(DWORD dwStage, LPDIRECT3DBASETEXTURE9 pTexture)
{
  return m_pDevice->SetTexture(dwStage, pTexture);
}

HRESULT CPureDeviceStateManager::SetVertexShader(LPDIRECT3DVERTEXSHADER9 pShader)
{
  return m_pDevice->SetVertexShader(pShader);
}

HRESULT CPureDeviceStateManager::SetPixelShader(LPDIRECT3DPIXELSHADER9 pShader)
{
  return m_pDevice->SetPixelShader(pShader);
}

HRESULT CPureDeviceStateManager::SetFVF(DWORD dwFVF)
{
  return m_pDevice->SetFVF(dwFVF);
}

HRESULT CPureDeviceStateManager::SetTransform(D3DTRANSFORMSTATETYPE State, CONST D3DMATRIX *pMatrix)
{
  return m_pDevice->SetTransform(State, pMatrix);
}

HRESULT CPureDeviceStateManager::SetMaterial(CONST D3DMATERIAL9 *pMaterial)
{
  return m_pDevice->SetMaterial(pMaterial);
}

HRESULT CPureDeviceStateManager::SetLight(DWORD Index, CONST D3DLIGHT9 *pLight)
{
  return m_pDevice->SetLight(Index, pLight);
}

HRESULT CPureDeviceStateManager::LightEnable(DWORD Index, BOOL Enable)
{
  return m_pDevice->LightEnable(Index, Enable);
}

HRESULT CPureDeviceStateManager::SetNPatchMode(FLOAT NumSegments)
{
  return m_pDevice->SetNPatchMode(NumSegments);
}

HRESULT CPureDeviceStateManager::SetVertexShaderConstantF(UINT RegisterIndex, CONST FLOAT *pConstantData, UINT RegisterCount)
{
  return m_pDevice->SetVertexShaderConstantF( RegisterIndex, pConstantData, RegisterCount);
}

HRESULT CPureDeviceStateManager::SetVertexShaderConstantI(UINT RegisterIndex, CONST INT *pConstantData, UINT RegisterCount)
{
  return m_pDevice->SetVertexShaderConstantI( RegisterIndex, pConstantData, RegisterCount);
}

HRESULT CPureDeviceStateManager::SetVertexShaderConstantB(UINT RegisterIndex, CONST BOOL *pConstantData, UINT RegisterCount)
{
  return m_pDevice->SetVertexShaderConstantB(RegisterIndex, pConstantData, RegisterCount);
}

HRESULT CPureDeviceStateManager::SetPixelShaderConstantF(UINT RegisterIndex, CONST FLOAT *pConstantData, UINT RegisterCount)
{
  return m_pDevice->SetPixelShaderConstantF(RegisterIndex, pConstantData, RegisterCount);
}

HRESULT CPureDeviceStateManager::SetPixelShaderConstantI(UINT RegisterIndex, CONST INT *pConstantData, UINT RegisterCount)
{
  return m_pDevice->SetPixelShaderConstantI(RegisterIndex, pConstantData, RegisterCount);
}

HRESULT CPureDeviceStateManager::SetPixelShaderConstantB(UINT RegisterIndex, CONST BOOL *pConstantData, UINT RegisterCount)
{
  return m_pDevice->SetPixelShaderConstantB(RegisterIndex, pConstantData, RegisterCount);
}

CStateManagerInterface* CStateManagerInterface::Create(LPDIRECT3DDEVICE9 pDevice)
{
  return new CPureDeviceStateManager(pDevice);
}

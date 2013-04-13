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

#pragma once
#ifndef _EFFECTSTATEMANAGER_H_
#define _EFFECTSTATEMANAGER_H_

#include "stdafx.h"

#pragma warning (push)
#pragma warning (disable : 4512) // 'class' : assignment operator could not be generated
#pragma warning (disable : 4702) // unreachable code
#include <vector>
#include <utility>
#pragma warning (pop)
using std::vector;
#pragma warning(default: 4995)

#define CACHED_STAGES 4

// template for the cache
template <typename _Kty, typename _Ty> class multicache
{
public:

  multicache(unsigned int scope)
  {
    m_vecCache.resize(scope);
  }

  struct cacheItem
  {
    _Ty value;
    bool set;
  };

protected:
  std::vector <cacheItem> m_vecCache;

public:
  inline void dirtyall()
  {
    m_vecCache.clear();
  }

  inline void dirty(_Kty key)
  {
    m_vecCache[key].set = false;
  }

  inline bool set_val(_Kty key, _Ty value)
  {
    if (m_vecCache[key].value == value && m_vecCache[key].set)
      return false;
    else
    {
      m_vecCache[key].value = value;
      m_vecCache[key].set = true;

      return true;
    }
  }
};

class CStateManagerInterface : public ID3DXEffectStateManager
{
public:

  virtual       ~CStateManagerInterface(){};
  virtual void  DirtyCachedValues() = 0;

  static CStateManagerInterface* CStateManagerInterface::Create(LPDIRECT3DDEVICE9 pDevice);
};

class CPureDeviceStateManager : public CStateManagerInterface
{
public:

  CPureDeviceStateManager(LPDIRECT3DDEVICE9 pDevice);
  virtual ~CPureDeviceStateManager();

  STDMETHOD(QueryInterface)(REFIID iid, LPVOID* ppv);
  STDMETHOD_(ULONG, AddRef)();
  STDMETHOD_(ULONG, Release)();

  void DirtyCachedValues();

  STDMETHOD(SetRenderState)(D3DRENDERSTATETYPE d3dRenderState, DWORD dwValue);
  STDMETHOD(SetSamplerState)(DWORD dwStage, D3DSAMPLERSTATETYPE d3dSamplerState, DWORD dwValue);
  STDMETHOD(SetTextureStageState)(DWORD dwStage, D3DTEXTURESTAGESTATETYPE d3dTextureStageState, DWORD dwValue);
  STDMETHOD(SetTexture)(DWORD dwStage, LPDIRECT3DBASETEXTURE9 pTexture);
  STDMETHOD(SetVertexShader)(LPDIRECT3DVERTEXSHADER9 pShader);
  STDMETHOD(SetPixelShader)(LPDIRECT3DPIXELSHADER9 pShader);
  STDMETHOD(SetFVF)(DWORD dwFVF);
  STDMETHOD(SetTransform)(D3DTRANSFORMSTATETYPE State, CONST D3DMATRIX *pMatrix);
  STDMETHOD(SetMaterial)(CONST D3DMATERIAL9 *pMaterial);
  STDMETHOD(SetLight)(DWORD Index, CONST D3DLIGHT9 *pLight);
  STDMETHOD(LightEnable)(DWORD Index, BOOL Enable);
  STDMETHOD(SetNPatchMode)(FLOAT NumSegments);
  STDMETHOD(SetVertexShaderConstantF)(UINT RegisterIndex, CONST FLOAT *pConstantData, UINT RegisterCount);
  STDMETHOD(SetVertexShaderConstantI)(UINT RegisterIndex, CONST INT *pConstantData, UINT RegisterCount);
  STDMETHOD(SetVertexShaderConstantB)(UINT RegisterIndex, CONST BOOL *pConstantData, UINT RegisterCount);
  STDMETHOD(SetPixelShaderConstantF)(UINT RegisterIndex, CONST FLOAT *pConstantData, UINT RegisterCount);
  STDMETHOD(SetPixelShaderConstantI)(UINT RegisterIndex, CONST INT *pConstantData, UINT RegisterCount);
  STDMETHOD(SetPixelShaderConstantB)(UINT RegisterIndex, CONST BOOL *pConstantData, UINT RegisterCount);

protected:
  typedef multicache <DWORD, DWORD> samplerStageCache;
  typedef multicache <DWORD, DWORD> textureStateStageCache;

protected:
  LPDIRECT3DDEVICE9 m_pDevice;
  LONG m_lRef;

  multicache <DWORD, DWORD>       m_cacheRenderStates;
  vector <samplerStageCache>      m_vecCacheSamplerStates;
  vector <textureStateStageCache> m_vecCacheTextureStates;
};

#endif

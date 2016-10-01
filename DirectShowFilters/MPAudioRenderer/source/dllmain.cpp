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

#include "stdafx.h"

#include "MpAudioRenderer.h"
#include "Settings.h"
#include "SettingsProp.h"

#include "alloctracing.h"

using namespace std;

void SetThreadName(DWORD dwThreadID, char* threadName);

const AMOVIESETUP_MEDIATYPE sudPinTypesIn[] =
{
  {&MEDIATYPE_Audio, &MEDIASUBTYPE_PCM},
  {&MEDIATYPE_Audio, &MEDIASUBTYPE_IEEE_FLOAT},
  {&MEDIATYPE_Audio, &KSDATAFORMAT_SUBTYPE_PCM},
  {&MEDIATYPE_Audio, &KSDATAFORMAT_SUBTYPE_IEEE_FLOAT},
};

const AMOVIESETUP_PIN sudpPins[] =
{
  {
    L"Input",
    TRUE,
    FALSE,
    FALSE,
    FALSE,
    &CLSID_NULL,
    NULL,
    4,
    sudPinTypesIn
  }
};

const AMOVIESETUP_FILTER sudFilter[] =
{
  {
    &__uuidof(CMPAudioRenderer),
    L"MediaPortal - Audio Renderer",
    0x30000000,
    NULL,
    sudpPins,
    CLSID_AudioRendererCategory
  }
};

CFactoryTemplate g_Templates[] =
{
  {
    sudFilter[0].strName,
    &__uuidof(CMPAudioRenderer),
    CMPAudioRenderer::CreateInstance,
    NULL,
    &sudFilter[0]
  },
  {
    L"MediaPortal Audio Renderer Properties",
    &CLSID_MPARSettingsProp,
    CSettingsProp::CreateInstance,
    NULL,
    NULL
  }
};

int g_cTemplates = sizeof(g_Templates) / sizeof(g_Templates[0]);

STDAPI DllRegisterServer()
{
  // Create the initial settings - currently disabeld as it is breaking installer for some reason

  // AudioRendererSettings settings;
  return AMovieDllRegisterServer2(TRUE);
}

STDAPI DllUnregisterServer()
{
  return AMovieDllRegisterServer2(FALSE);
}

HRESULT CopyWaveFormatEx(WAVEFORMATEXTENSIBLE** dst, const WAVEFORMATEXTENSIBLE* src)
{
  if (!src)
    return S_OK;

  if (!dst)
    return E_POINTER;

  int	size = sizeof(WAVEFORMATEXTENSIBLE) + src->Format.cbSize;

  *dst = (WAVEFORMATEXTENSIBLE *)new BYTE[size];

  if (!*dst)
    return E_OUTOFMEMORY;

  memcpy(*dst, src, size);

  return S_OK;
}

HRESULT ToWaveFormatExtensible(WAVEFORMATEXTENSIBLE** dst, WAVEFORMATEX* src)
{
  if (!src)
    return S_OK;

  if (!dst)
    return E_POINTER;

  int	size = sizeof(WAVEFORMATEXTENSIBLE);
  WAVEFORMATEXTENSIBLE *pwfe = (WAVEFORMATEXTENSIBLE *)new BYTE[size];
  
  if (!pwfe)
    return E_OUTOFMEMORY;

  //ASSERT(pwf->cbSize <= sizeof(WAVEFORMATEXTENSIBLE) - sizeof(WAVEFORMATEX));
  memcpy(pwfe, src, sizeof(WAVEFORMATEX));
  pwfe->Format.cbSize = sizeof(WAVEFORMATEXTENSIBLE) - sizeof(WAVEFORMATEX);
  switch(pwfe->Format.wFormatTag)
  {
  case WAVE_FORMAT_PCM:
    pwfe->SubFormat = KSDATAFORMAT_SUBTYPE_PCM;
    break;
  case WAVE_FORMAT_IEEE_FLOAT:
    pwfe->SubFormat = KSDATAFORMAT_SUBTYPE_IEEE_FLOAT;
    break;
  case WAVE_FORMAT_DOLBY_AC3_SPDIF:
    pwfe->SubFormat = KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL;
    break;
  default:
    delete[] pwfe;
    return VFW_E_TYPE_NOT_ACCEPTED;
  }
  if (pwfe->Format.nChannels >= 1 && pwfe->Format.nChannels <= 8)
  {
    pwfe->dwChannelMask = gdwDefaultChannelMask[pwfe->Format.nChannels];
    if (pwfe->dwChannelMask == 0)
    {
      delete[] pwfe;
      return VFW_E_TYPE_NOT_ACCEPTED;
    }
  }
  else
  {
    delete[] pwfe;
    return VFW_E_TYPE_NOT_ACCEPTED;
  }

  pwfe->Samples.wValidBitsPerSample = pwfe->Format.wBitsPerSample;
  pwfe->Format.wFormatTag = WAVE_FORMAT_EXTENSIBLE;
  *dst = pwfe;

  return S_OK;
}

// http://blogs.msdn.com/b/stevejs/archive/2005/12/19/505815.aspx

#include <windows.h>
#define MS_VC_EXCEPTION 0x406D1388
#pragma pack(push,8)
typedef struct tagTHREADNAME_INFO
{
   DWORD dwType; // Must be 0x1000.
   LPCSTR szName; // Pointer to name (in user addr space).
   DWORD dwThreadID; // Thread ID (-1=caller thread).
   DWORD dwFlags; // Reserved for future use, must be zero.
} THREADNAME_INFO;

#pragma pack(pop)

void SetThreadName(DWORD dwThreadID, char* threadName)
{
   Sleep(10);
   THREADNAME_INFO info;
   info.dwType = 0x1000;
   info.szName = threadName;
   info.dwThreadID = dwThreadID;
   info.dwFlags = 0;
   __try
   {
      RaiseException( MS_VC_EXCEPTION, 0, sizeof(info)/sizeof(ULONG_PTR), (ULONG_PTR*)&info );
   }
   __except(EXCEPTION_EXECUTE_HANDLER)
   {
   }
}
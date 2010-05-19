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

#include "MpAudioRenderer.h"

const AMOVIESETUP_MEDIATYPE sudPinTypesIn[] =
{
  {&GUID_NULL},
};

const AMOVIESETUP_PIN sudpPins[] =
{
  {L"Input", 
  FALSE, 
  FALSE, 
  FALSE, 
  FALSE, 
  &CLSID_NULL, 
  NULL, 
  1, 
  sudPinTypesIn},
};

const AMOVIESETUP_FILTER sudFilter[] =
{
	{&__uuidof(CMpcAudioRenderer), 
  L"MediaPortal - Audio Renderer", 
  0x30000000, 
  NULL,
  sudpPins},
};

CFactoryTemplate g_Templates[] =
{
  {sudFilter[0].strName, 
  &__uuidof(CMpcAudioRenderer), 
  CMpcAudioRenderer::CreateInstance, 
  NULL, 
  &sudFilter[0]},
};

int g_cTemplates = 1;

STDAPI DllRegisterServer()
{
	return AMovieDllRegisterServer2(TRUE);
}

STDAPI DllUnregisterServer()
{
	return AMovieDllRegisterServer2(FALSE);
}
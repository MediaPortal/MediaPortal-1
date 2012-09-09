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
#include "Globals.h"
#include "VolumeHandler.h"

#include "alloctracing.h"

CVolumeHandler::CVolumeHandler(LPUNKNOWN pUnk)
  : CBasicAudio(NAME("CBasicAudio"), pUnk)
{
}

CVolumeHandler::~CVolumeHandler()
{
}

STDMETHODIMP CVolumeHandler::put_Volume(long lVolume)
{
  Log("VolumeHandler::put_Volume %d", lVolume);
  return S_OK;
}

STDMETHODIMP CVolumeHandler::get_Volume(long* plVolume)
{
  Log("VolumeHandler::get_Volume");
  return S_OK;
}

STDMETHODIMP CVolumeHandler::put_Balance(long lBalance)
{
  Log("VolumeHandler::put_Balance %d", lBalance);
  return S_OK;
}

STDMETHODIMP CVolumeHandler::get_Balance(long* plBalance)
{
  Log("VolumeHandler::get_Balance");
  return S_OK;
}



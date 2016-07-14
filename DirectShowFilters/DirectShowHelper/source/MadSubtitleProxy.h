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

#pragma once

#include "stdafx.h"
#include "callback.h"
#include "DeviceState.h"
#include "mvrInterfaces.h"

class MadSubtitleProxy : public CUnknown, public ISubRenderCallback, public CCritSec
{
  public:
    MadSubtitleProxy(IVMR9Callback* pCallback, IMediaControl* pMediaControl);
    ~MadSubtitleProxy();

    DECLARE_IUNKNOWN;

    STDMETHOD(SetDevice)(IDirect3DDevice9* device);
    STDMETHOD(Render)(REFERENCE_TIME frameStart, int left, int top, int right, int bottom, int width, int height);

    void SetNewDevice(bool pNewDevice);
    bool GetNewDevice();

  private:

    HRESULT SetupMadDeviceState();

    IDirect3DDevice9* m_pMadD3DDev = nullptr;

    IVMR9Callback* m_pCallback;

    IMediaControl* m_pMediaControl;

    DeviceState m_deviceState;

    bool m_pNewDevice = false;

    int deviceNULL = 0;
    int counterBeforeProcessOSD = 0;
};


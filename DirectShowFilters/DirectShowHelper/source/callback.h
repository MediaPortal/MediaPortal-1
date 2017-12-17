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

#pragma once

DECLARE_INTERFACE_(IVMR9Callback, IUnknown)
{
  STDMETHOD(PresentImage)  (THIS_ DWORD cx, DWORD cy, DWORD arx, DWORD ary, DWORD pTexture, DWORD pSurface)PURE;
  STDMETHOD(SetSampleTime)(REFERENCE_TIME nsSampleTime)PURE;
  STDMETHOD(RenderGui)(DWORD cx, DWORD cy, DWORD arx, DWORD ary)PURE;
  STDMETHOD(RenderOverlay)(DWORD cx, DWORD cy, DWORD arx, DWORD ary)PURE;
  STDMETHOD(SetRenderTarget)(LONG pTarget)PURE;
  STDMETHOD(SetSubtitleDevice)(LONG pDevice)PURE;
  STDMETHOD(RenderSubtitle)(REFERENCE_TIME frameStart, int left, int top, int right, int bottom, int width, int height, int xOffsetInPixels)PURE;
  STDMETHOD(RenderSubtitleEx)(REFERENCE_TIME frameStart, RECT viewportRect, RECT croppedVideoRect, int xOffsetInPixels)PURE;
  STDMETHOD(RenderFrame)(int cx, int cy, int arx, int ary, LONG pTargetmadVr)PURE;
  STDMETHOD(GrabMadVrScreenshot)(LPVOID pTargetmadVrDib)PURE;
  STDMETHOD(GrabMadVrFrame)(LPVOID pTargetmadVrDib)PURE;
  STDMETHOD(GrabMadVrCurrentFrame)(LPVOID pTargetmadVrDib)PURE;
  STDMETHOD(ForceOsdUpdate)(BOOL pForce)PURE;
  STDMETHOD(IsFullScreen)()PURE;
  STDMETHOD(IsUiVisible)()PURE;
  STDMETHOD(RestoreDeviceSurface)(LPVOID pSurfaceDevice)PURE;
  STDMETHOD(ReduceMadvrFrame)()PURE;
  STDMETHOD(DestroyHWnd)(HWND phWnd)PURE;
};

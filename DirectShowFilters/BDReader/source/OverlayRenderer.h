/*
 *  Copyright (C) 2005-2011 Team MediaPortal
 *  http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#pragma once

#include "StdAfx.h"
#include <bluray.h>
#include <overlay.h>
#include <streams.h>
#include <D3d9.h>
#include "OSDTexture.h"

#define PALETTE_SIZE 256

class CLibBlurayWrapper;

class COverlayRenderer
{
public:
  COverlayRenderer(CLibBlurayWrapper* pLib);
  ~COverlayRenderer();

  void OverlayProc(const BD_OVERLAY* ov);
  void SetD3DDevice(IDirect3DDevice9* device);

private:

  void OpenOverlay(const BD_OVERLAY* pOv);
  void CloseOverlay(const int pPlane);

  void ClearArea(OSDTexture* pPlane, const BD_OVERLAY* pOv);
  void ClearOverlay();
  void DrawBitmap(OSDTexture* pPlane, const BD_OVERLAY* pOv);

  void DecodePalette(const BD_OVERLAY* ov);

  void CopyToFrontBuffer();
  void ResetDirtyRect();
  void AdjustDirtyRect(const BD_OVERLAY* pOv);

  void LogCommand(const BD_OVERLAY* ov);
  LPCTSTR CommandAsString(int pCmd);

  uint32_t m_palette[PALETTE_SIZE];

  CLibBlurayWrapper* m_pLib;
  IDirect3DDevice9* m_pD3DDevice;

  OSDTexture* m_pPlanes[2];
  OSDTexture* m_pPlanesBackbuffer[2];

  RECT m_dirtyRect;
};

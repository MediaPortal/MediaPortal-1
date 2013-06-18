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
#define NUM_OF_PLANES 2

class CLibBlurayWrapper;

class COverlayRenderer
{
public:
  COverlayRenderer(CLibBlurayWrapper* pLib);
  ~COverlayRenderer();

  void OverlayProc(const BD_OVERLAY* ov);
  void ARGBOverlayProc(const BD_ARGB_OVERLAY* ov);

  void SetD3DDevice(IDirect3DDevice9* device);

private:

  void OpenOverlay(const BD_OVERLAY* pOv);
  void OpenOverlay(const BD_ARGB_OVERLAY* pOv);
  void CloseOverlay(const uint8_t plane);

  void CreateFrontAndBackBuffers(uint8_t plane, uint16_t x, uint16_t y, uint16_t w, uint16_t h);

  void ClearArea(OSDTexture* pOsdTexture, const BD_OVERLAY* pOv);
  void ClearOverlay(const uint8_t plane);
  void DrawBitmap(OSDTexture* pOsdTexture, const BD_OVERLAY* pOv);
  void DrawARGBBitmap(OSDTexture* pOsdTexture, const BD_ARGB_OVERLAY* pOv);

  void DrawToTexture(OSDTexture* pOsdTexture, IDirect3DTexture9* pTexture, uint16_t x, uint16_t y, uint16_t w, uint16_t h);

  void DecodePalette(const BD_OVERLAY* ov);

  void CopyToFrontBuffer(const uint8_t plane);
  void ResetDirtyRect(const uint8_t plane);
  void ResetDirtyRect(const uint8_t plane, uint16_t w, uint16_t h);
  void AdjustDirtyRect(const uint8_t plane, uint16_t x, uint16_t y, uint16_t w, uint16_t h);

  void LogCommand(const BD_OVERLAY* ov);
  void LogARGBCommand(const BD_ARGB_OVERLAY* ov);
  char* CommandAsString(int cmd);
  char* ARGBCommandAsString(int cmd);

  uint32_t m_palette[PALETTE_SIZE];

  CLibBlurayWrapper* m_pLib;
  IDirect3DDevice9* m_pD3DDevice;

  OSDTexture* m_pPlanes[NUM_OF_PLANES];
  OSDTexture* m_pPlanesBackbuffer[NUM_OF_PLANES];

  RECT m_dirtyRect[NUM_OF_PLANES];
};

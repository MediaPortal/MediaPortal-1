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
#include <Vmr9.h>
#include <evr.h>
#include <evr9.h>

#define PALETTE_SIZE 256

class CLibBlurayWrapper;

class COverlayRenderer
{
public:
  COverlayRenderer(CLibBlurayWrapper* pLib);
  ~COverlayRenderer();

  void OverlayProc(const BD_OVERLAY* const ov);
  void SetD3DDevice(IDirect3DDevice9* device);

  bool IsMenuOpen();

private:
  
  void DecodePalette(const BD_OVERLAY* const ov);
  void CloseOverlay(bool pIsMenuOpen);

  bool m_bIsMenuOpen;

  uint32_t m_palette[PALETTE_SIZE];

  CLibBlurayWrapper* m_pLib;
  IDirect3DDevice9* m_pD3DDevice;
};

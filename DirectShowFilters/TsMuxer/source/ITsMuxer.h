/* 
 *  Copyright (C) 2005-2013 Team MediaPortal
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
#include <InitGuid.h>   // DEFINE_GUID()
#include <streams.h>    // IUnknown


// {8533d2d1-1be1-4262-b70a-432df592b903}
DEFINE_GUID(IID_ITS_MUXER,
            0x8533d2d1, 0x1be1, 0x4262, 0xb7, 0xa, 0x43, 0x2d, 0xf5, 0x92, 0xb9, 0x3);

DECLARE_INTERFACE_(ITsMuxer, IUnknown)
{
  STDMETHOD(ConfigureLogging)(THIS_ wchar_t* fileName)PURE;
  STDMETHOD_(void, DumpInput)(THIS_ long mask)PURE;
  STDMETHOD_(void, DumpOutput)(THIS_ bool enable)PURE;
  STDMETHOD(SetActiveComponents)(THIS_ bool video, bool audio, bool teletext, bool vps, bool wss)PURE;
};
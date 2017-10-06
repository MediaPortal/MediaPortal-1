/*
 *  Copyright (C) 2006-2008 Team MediaPortal
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
#include "ICallBackGrabber.h"


// {3a1ff7ad-08be-4fd7-885e-5a91accea9a2}
DEFINE_GUID(IID_IGRABBER,
            0x3a1ff7ad, 0x8be, 0x4fd7, 0x88, 0x5e, 0x5a, 0x91, 0xac, 0xce, 0xa9, 0xa2);

DECLARE_INTERFACE_(IGrabber, IUnknown)
{
  BEGIN_INTERFACE


  // IUnknown
  STDMETHOD(QueryInterface)(THIS_ REFIID riid, void** ppv)PURE;
  STDMETHOD_(unsigned long, AddRef)(THIS)PURE;
  STDMETHOD_(unsigned long, Release)(THIS)PURE;


  // IGrabber
  STDMETHOD_(void, SetCallBack)(THIS_ ICallBackGrabber* callBack)PURE;


  END_INTERFACE
};
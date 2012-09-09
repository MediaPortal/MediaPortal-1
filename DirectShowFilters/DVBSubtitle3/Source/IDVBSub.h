/*
 *	Copyright (C) 2006-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
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

#include <windows.h>
#include <xprtdefs.h>
#include <bdaiface.h>
#include <initguid.h>
#include <atlcomcli.h>
#include "SubStructs.h"

#pragma once

// {3B4C4F66-739F-452c-AFC4-1C039BED3299}
DEFINE_GUID(CLSID_DVBSub3, 
0x3b4c4f66, 0x739f, 0x452c, 0xaf, 0xc4, 0x1c, 0x3, 0x9b, 0xed, 0x32, 0x99);

// {1E00BDAA-44AB-460b-A2CB-4D554D771392}
DEFINE_GUID(IID_IDVBSubtitle3, 
0x1e00bdaa, 0x44ab, 0x460b, 0xa2, 0xcb, 0x4d, 0x55, 0x4d, 0x77, 0x13, 0x92);

// {4A4fAE7C-6095-11DC-8314-0800200C9A66}
DEFINE_GUID(IID_IDVBSubtitleSource, 
  0x4a4fae7c, 0x6095, 0x11dc, 0x83, 0x14, 0x08, 0x00, 0x20, 0x0c, 0x9a, 0x66);

DECLARE_INTERFACE_( IDVBSubtitleSource, IUnknown )
{
  STDMETHOD(SetBitmapCallback) ( int (CALLBACK *pSubtitleObserver)(SUBTITLE* sub) ) PURE;
  STDMETHOD(SetResetCallback)( int (CALLBACK *pResetObserver)() ) PURE;
  STDMETHOD(SetUpdateTimeoutCallback)( int (CALLBACK *pUpdateTimeoutObserver)(__int64* pTimeout) ) PURE; 
  STDMETHOD(StatusTest)( int testval ) PURE;
};

DECLARE_INTERFACE_( IDVBSubtitle, IUnknown )
{
  STDMETHOD(Test)( int status ) PURE;
  STDMETHOD(NotifyChannelChange)() PURE;
  STDMETHOD(SetSubtitlePid)( LONG pPid ) PURE;
  STDMETHOD(SetFirstPcr)( LONGLONG pPcr ) PURE;
  STDMETHOD(SeekDone)( CRefTime& rtSeek ) PURE;
  STDMETHOD(SetTimeCompensation)( CRefTime& rtCompensation ) PURE;
  STDMETHOD(SetHDMV)( bool pHDMV ) PURE;
};

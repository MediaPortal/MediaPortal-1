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

// {1CF3606B-6F89-4813-9D05-F9CA324CF2EA}
DEFINE_GUID(CLSID_DVBSub2, 
  0x1cf3606b, 0x6f89, 0x4813, 0x9d, 0x5, 0xf9, 0xca, 0x32, 0x4c, 0xf2, 0xea);

// {901C9084-246A-47c9-BBCD-F8F398D30AB0}
DEFINE_GUID(IID_IDVBSubtitle2, 
  0x901c9084, 0x246a, 0x47c9, 0xbb, 0xcd, 0xf8, 0xf3, 0x98, 0xd3, 0xa, 0xb0);

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
};

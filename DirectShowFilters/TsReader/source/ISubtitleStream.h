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

#include "StdAfx.h"

#include <xprtdefs.h>
#include <initguid.h>

#pragma once

// {43FED769-C5EE-46aa-912D-7EBDAE4EE93A}
DEFINE_GUID(IID_ISubtitleStream, 
  0x43fed769, 0xc5ee, 0x46aa, 0x91, 0x2d, 0x7e, 0xbd, 0xae, 0x4e, 0xe9, 0x3a);

const int SUBTITLESTREAM_EVENT_UPDATE = 0;

const DWORD64 SUBTITLESTREAM_EVENTVALUE_NONE = 0;

DECLARE_INTERFACE_( ISubtitleStream, IUnknown )
{
  STDMETHOD(SetSubtitleStream)( __int32 stream ) PURE;
  STDMETHOD(GetSubtitleStreamType)( __int32 stream, int &type ) PURE;
  STDMETHOD(GetSubtitleStreamCount)( __int32 &count ) PURE;
  STDMETHOD(GetCurrentSubtitleStream)( __int32 &stream ) PURE;
  STDMETHOD(GetSubtitleStreamLanguage)( __int32 stream,char* szLanguage ) PURE;
  STDMETHOD(SetSubtitleResetCallback)( int (CALLBACK *pSubUpdateCallback)(int count, void* opts, int* select)) PURE; 
};

/*
 *  Copyright (C) 2005-2009 Team MediaPortal
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
 
#include "StdAfx.h"

#include <xprtdefs.h>
#include <initguid.h>

#pragma once

const int TELETEXT_EVENT_SEEK_START = 0;
const int TELETEXT_EVENT_SEEK_END = 1;
const int TELETEXT_EVENT_RESET = 2;
//const int TELETEXT_EVENT_BUFFER_IN_UPDATE = 3;
//const int TELETEXT_EVENT_BUFFER_OUT_UPDATE = 4;
const int TELETEXT_EVENT_PACKET_PCR_UPDATE = 5;
const int TELETEXT_EVENT_CURRENT_PCR_UPDATE = 6;
//const int TELETEXT_EVENT_COMPENSATION_UPDATE = 7;
const int TELETEXT_EVENTVALUE_NONE = 0;
        
// {3AB7E208-7962-11DC-9F76-850456D89593}
DEFINE_GUID(IID_ITeletextSource, 
  0x3AB7E208, 0x7962, 0x11DC, 0x9F, 0x76, 0x85, 0x04, 0x56, 0xD8, 0x95, 0x93);


DECLARE_INTERFACE_( ITeletextSource, IUnknown )
{
  STDMETHOD(SetTeletextTSPacketCallBack) ( int (CALLBACK *pTeletextCallback)(byte*, int) ) PURE;
  STDMETHOD(SetTeletextEventCallback( int (CALLBACK *pResetCallback)(int eventcode, DWORD64 eval))) PURE; 
  STDMETHOD(SetTeletextServiceInfoCallback( int (CALLBACK *pServiceInfoCallback)(int page, byte type, byte lb1, byte lb2, byte lb3))) PURE; 
};
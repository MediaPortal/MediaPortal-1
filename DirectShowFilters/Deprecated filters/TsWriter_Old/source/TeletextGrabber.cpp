/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
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

#pragma warning(disable : 4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>

#include "teletextgrabber.h"
#include "TsHeader.h"


extern void LogDebug(const char *fmt, ...) ;


CTeletextGrabber::CTeletextGrabber(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsTxtGrabber"), pUnk)
{
	m_iPacketCounter=0;
	m_iTeletextPid=-1;
	m_bRunning=FALSE;
	m_pCallback=NULL;
	m_pBuffer = new byte[20000];
}
CTeletextGrabber::~CTeletextGrabber(void)
{
	delete[] m_pBuffer;
}


STDMETHODIMP CTeletextGrabber::SetTeletextPid( int teletextPid)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("TeletextGrabber: set pid:%x", teletextPid);
		m_iTeletextPid=teletextPid;
	}
	catch(...)
	{
		LogDebug("CTeletextGrabber::SetPmtPid exception");
	}
	return S_OK;
}

STDMETHODIMP CTeletextGrabber::SetCallBack( ITeletextCallBack* callback)
{
	CEnterCriticalSection enter(m_section);
	LogDebug("TeletextGrabber: set callback:%x", callback);
	m_pCallback=callback;
	return S_OK;
}

STDMETHODIMP CTeletextGrabber::Start( )
{
	CEnterCriticalSection enter(m_section);
	LogDebug("TeletextGrabber: start");
	m_iPacketCounter=0;
	m_bRunning=true;
	return S_OK;
}

STDMETHODIMP CTeletextGrabber::Stop( )
{
	CEnterCriticalSection enter(m_section);
	LogDebug("TeletextGrabber: stop");
	m_bRunning=false;
	return S_OK;
}

void CTeletextGrabber::OnTsPacket(byte* tsPacket)
{
	if (!m_bRunning) return;
	if (m_pCallback==NULL) return;
	if (m_iTeletextPid<=0) return;
	CEnterCriticalSection enter(m_section);
	CTsHeader header(tsPacket);
	if (header.SyncByte!=0x47) return;
	if (header.TransportError!=0) return;
	if (header.Pid!=m_iTeletextPid) return;
	if (header.AdaptionFieldOnly()) return;

	memcpy(&m_pBuffer[m_iPacketCounter*188], tsPacket,188);
	m_iPacketCounter++;
	if (m_iPacketCounter >= 25)
	{
		m_pCallback->OnTeletextReceived(m_pBuffer, m_iPacketCounter);
		m_iPacketCounter=0;
	}
}


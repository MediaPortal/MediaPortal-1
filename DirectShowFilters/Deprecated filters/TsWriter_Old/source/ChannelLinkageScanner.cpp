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

#include "ChannelLinkageScanner.h"


extern void LogDebug(const char *fmt, ...) ;

CChannelLinkageScanner::CChannelLinkageScanner(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsChannelLinkageScanner"), pUnk)
{
	m_pCallBack=NULL;
	m_bScanning=false;
}

CChannelLinkageScanner::~CChannelLinkageScanner(void)
{
}

STDMETHODIMP CChannelLinkageScanner::Start()
{
	CEnterCriticalSection enter(m_section);
	LogDebug("ChannelLinkageScanner: start");
	m_bScanning=true;
	m_ChannelLinkageParser.Start();
  	return S_OK;
}

STDMETHODIMP CChannelLinkageScanner::Reset()
{
	CEnterCriticalSection enter(m_section);
	LogDebug("ChannelLinkageScanner: reset");
	m_bScanning=false;
	m_ChannelLinkageParser.Reset();
  	return S_OK;
}

STDMETHODIMP CChannelLinkageScanner::GetChannelCount (THIS_ ULONG* channelCount)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		*channelCount=m_ChannelLinkageParser.GetChannelCount();
	}
	catch(...)
	{
		LogDebug("ChannelLinkageScanner: GetChannelCount exception");
	}
	return S_OK;
}

STDMETHODIMP CChannelLinkageScanner::GetChannel (THIS_ ULONG channelIndex, WORD* network_id, WORD* transport_id,WORD* service_id  )
{
	CEnterCriticalSection enter(m_section);
	try
	{
		m_ChannelLinkageParser.GetChannel(channelIndex,network_id,transport_id,service_id);
	}
	catch(...)
	{
		LogDebug("ChannelLinkageScanner: GetChannel exception");
	}
	return S_OK;
}

STDMETHODIMP CChannelLinkageScanner::GetLinkedChannelsCount (THIS_ ULONG channelIndex, ULONG* linkedChannelsCount)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		*linkedChannelsCount=m_ChannelLinkageParser.GetLinkedChannelsCount(channelIndex);
	}
	catch(...)
	{
		LogDebug("ChannelLinkageScanner: GetLinkedChannelsCount exception");
	}
	return S_OK;
}

STDMETHODIMP CChannelLinkageScanner::GetLinkedChannel (THIS_ ULONG channelIndex, ULONG linkIndex, WORD* network_id, WORD* transport_id,WORD* service_id, char** channelName  )
{
	CEnterCriticalSection enter(m_section);
	try
	{
		m_ChannelLinkageParser.GetLinkedChannel(channelIndex,linkIndex,network_id,transport_id,service_id,channelName);
	}
	catch(...)
	{
		LogDebug("ChannelLinkageScanner: GetLinkedChannel exception");
	}
	return S_OK;
}

STDMETHODIMP CChannelLinkageScanner::SetCallBack(IChannelLinkageCallback* callback)
{
	LogDebug("ChannelLinkageScanner: set callback");
	m_pCallBack=callback;
	return S_OK;
}


void CChannelLinkageScanner::OnTsPacket(byte* tsPacket)
{
	if (!m_bScanning) return;
	try
	{
      int pid=((tsPacket[1] & 0x1F) <<8)+tsPacket[2];
      if (pid!=PID_EPG) return;
      {
		m_header.Decode(tsPacket);
		CEnterCriticalSection enter(m_section);
	    m_ChannelLinkageParser.OnTsPacket(m_header,tsPacket);
      }
	  if (m_ChannelLinkageParser.IsScanningDone())
	  {
		m_bScanning=false;
		if (m_pCallBack!=NULL)
		{
		  LogDebug("ChannelLinkageScanner: do callback");
		  m_pCallBack->OnLinkageReceived();
		}
	  }
	}
	catch(...)
	{
		LogDebug("ChannelLinkageScanner exception");
	}
}

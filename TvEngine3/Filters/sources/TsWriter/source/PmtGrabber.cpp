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

#include "pmtgrabber.h"


extern void LogDebug(const char *fmt, ...) ;


CPmtGrabber::CPmtGrabber(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsPmtGrabber"), pUnk)
{
	m_pCallback=NULL;
	m_iPmtVersion=-1;
	m_iServiceId=0;
	memset(m_pmtPrevData,0,sizeof(m_pmtPrevData));
}
CPmtGrabber::~CPmtGrabber(void)
{
}


STDMETHODIMP CPmtGrabber::SetPmtPid( int pmtPid, long serviceId)
{
	try
	{
		CEnterCriticalSection enter(m_section);
		LogDebug("pmtgrabber: grab pmt:%x sid:%x", pmtPid,serviceId);
		CSectionDecoder::Reset();
		CSectionDecoder::SetPid(pmtPid);
		m_iPmtVersion=-1;
		m_iServiceId=serviceId;
		memset(m_pmtPrevData,0,sizeof(m_pmtPrevData));
	}
	catch(...)
	{
		LogDebug("CPmtGrabber::SetPmtPid exception");
	}
	return S_OK;
}

STDMETHODIMP CPmtGrabber::SetCallBack( IPMTCallback* callback)
{
	CEnterCriticalSection enter(m_section);
	LogDebug("pmtgrabber: set callback:%x", callback);
	m_pCallback=callback;
	return S_OK;
}

void CPmtGrabber::OnTsPacket(byte* tsPacket)
{
	if (m_pCallback==NULL) return;
  int pid=((tsPacket[1] & 0x1F) <<8)+tsPacket[2];
  if (pid != GetPid()) return;
	CEnterCriticalSection enter(m_section);
	CSectionDecoder::OnTsPacket(tsPacket);
}

void CPmtGrabber::OnNewSection(CSection& section)
{
	try
	{
		if (section.table_id!=2) return;
		//if (section.version_number == m_iPmtVersion) return;
		
	  CEnterCriticalSection enter(m_section);

		if (section.section_length<0 || section.section_length>=MAX_SECTION_LENGTH) return;

		long serviceId = section.table_id_extension;
		LogDebug("service_id=%d",serviceId);
    if (m_iPmtVersion<0)
		  LogDebug("pmtgrabber: got pmt %x sid:%x",GetPid(), serviceId);

		if (serviceId != m_iServiceId) 
		{	
			LogDebug("pmtgrabber: serviceid mismatch %d != %d",serviceId,m_iServiceId);
			return;
		}
		LogDebug("pmtgrabber: got pmt version:%d %d", section.version_number,m_iPmtVersion);
		m_iPmtVersion=section.version_number;
		m_iPmtLength=section.section_length;

		memcpy(m_pmtData,section.Data,m_iPmtLength);
		if (memcmp(m_pmtData,m_pmtPrevData,m_iPmtLength)!=0)
		{
			memcpy(m_pmtPrevData,m_pmtData,m_iPmtLength);
			if (m_pCallback!=NULL)
			{
				LogDebug("pmtgrabber: do calback");
				m_pCallback->OnPMTReceived();
			}
			m_pCallback=NULL;
		}
	}
	catch(...)
	{
		LogDebug("CPmtGrabber::OnNewSection exception");
	}
}

STDMETHODIMP CPmtGrabber::GetPMTData(BYTE *pmtData)
{
	try
	{
	  CEnterCriticalSection enter(m_section);
		if (m_iPmtLength>0)
		{
			memcpy(pmtData,m_pmtData,m_iPmtLength);
			return m_iPmtLength;
		}
	}
	catch(...)
	{
		LogDebug("CPmtGrabber::GetPMTData exception");
	}
	return 0;
}

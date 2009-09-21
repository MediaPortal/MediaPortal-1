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
  m_SeenPmtPid=0;
  m_bLookForAll=false;
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
		CSectionDecoder::Reset();
		LogDebug("pmtgrabber: grab pmt:%x sid:%x", pmtPid,serviceId);
  	CSectionDecoder::SetPid(pmtPid);
  	m_iPmtVersion=-1;
  	m_iServiceId=serviceId;
    m_bLookForAll=true;
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
  m_SeenPmtPid=((tsPacket[1] & 0x1F) <<8)+tsPacket[2];
  if (!m_bLookForAll && m_SeenPmtPid != GetPid()) return; // only check other packets if needed
	CEnterCriticalSection enter(m_section);
  CSectionDecoder::OnTsPacket(tsPacket, m_bLookForAll); // true to tell decoder passing back all sections to OnNewSection!
}

void CPmtGrabber::OnNewSection(CSection& section)
{
	try
	{
    // must be a PMT
    if (section.table_id!=2) return;
    CEnterCriticalSection enter(m_section);

		if (section.section_length<0 || section.section_length>=MAX_SECTION_LENGTH) return;
  
    // check if packet matches serviceid
    if (m_iServiceId != section.table_id_extension) return;
    if (m_SeenPmtPid != GetPid())
    {
      LogDebug("pmtgrabber: got sid:%x with other pmt:%x", m_iServiceId, m_SeenPmtPid);
      SetPid(m_SeenPmtPid);
    }
     
    if (m_iPmtVersion<0)
		  LogDebug("pmtgrabber: got pmt %x sid:%x",GetPid(), m_iServiceId);

    m_bLookForAll=false; // seen the requested service/pmt once; no need to parse all further packets
		m_iPmtLength=section.section_length;

		memcpy(m_pmtData,section.Data,m_iPmtLength);
		if (memcmp(m_pmtData,m_pmtPrevData,m_iPmtLength)!=0)
		{
			memcpy(m_pmtPrevData,m_pmtData,m_iPmtLength);
      // do a callback each time the version number changes. this also allows switching for "regional channels"
			if (m_pCallback!=NULL && m_iPmtVersion != section.version_number)
			{
        LogDebug("pmtgrabber: got new pmt version:%d %d, service_id:%d", section.version_number, m_iPmtVersion, m_iServiceId);
				LogDebug("pmtgrabber: do callback pid = %x",GetPid());
        m_pCallback->OnPMTReceived(GetPid());
			}
		}
 		m_iPmtVersion=section.version_number;
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

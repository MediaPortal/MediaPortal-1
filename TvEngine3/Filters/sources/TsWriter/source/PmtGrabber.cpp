/* 
 *	Copyright (C) 2005 Team MediaPortal
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
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>

#include "pmtgrabber.h"
#include "TsHeader.h"


extern void LogDebug(const char *fmt, ...) ;


CPmtGrabber::CPmtGrabber(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsPmtGrabber"), pUnk)
{
	m_pCallback=NULL;
	m_iPmtVersion=-1;
}
CPmtGrabber::~CPmtGrabber(void)
{
}


STDMETHODIMP CPmtGrabber::SetPmtPid( int pmtPid)
{
	try
	{
		LogDebug("pmtgrabber: grab pmt:%x", pmtPid);
		CSectionDecoder::Reset();
		CSectionDecoder::SetPid(pmtPid);
		CSectionDecoder::SetTableId(2);
		m_iPmtVersion=-1;
	}
	catch(...)
	{
		LogDebug("CPmtGrabber::SetPmtPid exception");
	}
	return S_OK;
}

STDMETHODIMP CPmtGrabber::SetCallBack( IPMTCallback* callback)
{
	LogDebug("pmtgrabber: set callback:%x", callback);
	m_pCallback=callback;
	return S_OK;
}

void CPmtGrabber::OnTsPacket(byte* tsPacket)
{
	if (m_pCallback==NULL) return;
	if (GetPid()<=0) return;
	
	CSectionDecoder::OnTsPacket(tsPacket);

}

void CPmtGrabber::OnNewSection(CSection& section)
{
	try
	{
		if (section.Version == m_iPmtVersion) return;
		LogDebug("pmtgrabber: got pmt version:%d %d", section.Version,m_iPmtVersion);
		m_iPmtVersion=section.Version;
		m_iPmtLength=section.SectionLength+3;

		CTsHeader header(section.Data);
		int start=header.PayLoadStart+1;
		memcpy(m_pmtData,&section.Data[start],m_iPmtLength);
		if (m_pCallback!=NULL)
		{
			LogDebug("pmtgrabber: do calback");
			m_pCallback->OnPMTReceived();
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
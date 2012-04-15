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
#include "PmtParser.h"

extern void LogDebug(const char *fmt, ...) ;


CPmtGrabber::CPmtGrabber(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsPmtGrabber"), pUnk)
{
	m_pCallback=NULL;
	m_iPmtVersion=-1;
	m_iServiceId=0;
  memset(m_pmtPrevSection.Data, 0, sizeof(m_pmtPrevSection.Data));
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
    if (pmtPid==0) // need to Grab PMT pid
    {      
  		LogDebug("pmtgrabber: search pmt from PAT for sid:%x", serviceId);
    }
    else
    {
  		LogDebug("pmtgrabber: grab pmt:%x sid:%x", pmtPid,serviceId);
    }
  	CSectionDecoder::SetPid(pmtPid);
  	m_iPmtVersion=-1;
  	m_iServiceId=serviceId;
    memset(m_pmtPrevSection.Data, 0, sizeof(m_pmtPrevSection.Data));
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
    // if only service ID given, lookup from PAT
    if (GetPid() == 0) // PID 0 is the PAT, so look for matching PMT
    {
      int PMTPid=m_patgrab.PATRequest(section, m_iServiceId);
      if (PMTPid>0)
      {
         SetPmtPid(PMTPid,m_iServiceId);
         SetPid(PMTPid);
      } 
      else
      {
        LogDebug("PMT Pid wasn't found on the PAT. Channel may have moved, try a new channel scan.");
      }
    }

    if (section.table_id!=2) return;

    CEnterCriticalSection enter(m_section);

		if (section.section_length<0 || section.section_length>=MAX_SECTION_LENGTH) return;

		long serviceId = section.table_id_extension;
    if (m_iPmtVersion<0)
		  LogDebug("pmtgrabber: got pmt %x sid:%x",GetPid(), serviceId);

		if (serviceId != m_iServiceId) 
		{	
			LogDebug("pmtgrabber: serviceid mismatch %x != %x",serviceId,m_iServiceId);
			return;
		}

		// The + 3 is because the PMT data includes the table ID, section
		// syntax indicator, and section length bytes on the front - we're
		// literally passing the *whole* PMT...
		m_iPmtLength=section.section_length + 3;

    // copy pmt data for passing it to tvserver
		memcpy(m_pmtData,section.Data,m_iPmtLength);

    // compare the current section data with the previous one
    if (memcmp(section.Data, m_pmtPrevSection.Data, m_iPmtLength)!=0)
		{
      bool pidsChanged = false;
      CPmtParser prevPmtParser;
      CPmtParser currPmtParser;
      
      prevPmtParser.SetPid(GetPid());
      prevPmtParser.DecodePmtPidTable(m_pmtPrevSection);
      currPmtParser.SetPid(GetPid());

	  // Check if failed - if corrupted it can crash tv service after the callback
      if(!currPmtParser.DecodePmtPidTable(section))
      {
         LogDebug("CPmtGrabber::OnNewSection() - Error decoding PMT from new section, bad signal?");
         return;
      }

      if (!(prevPmtParser.GetPidInfo() == currPmtParser.GetPidInfo()))
      {
        LogDebug("pmtgrabber: PMT pids changed from:");
        prevPmtParser.GetPidInfo().LogPIDs();
        LogDebug("pmtgrabber: PMT pids changed to:");
        currPmtParser.GetPidInfo().LogPIDs();
        pidsChanged = true;
      }
      m_pmtPrevSection=section;

      // do a callback each time the version number changes. this also allows switching for "regional channels"
			if (m_pCallback!=NULL && m_iPmtVersion != section.version_number)
			{
        LogDebug("pmtgrabber: got new pmt version:%x %x, service_id:%x", section.version_number, m_iPmtVersion, serviceId);
        // if the pids are different, then a callback is required.
        if (pidsChanged)
        {
				  LogDebug("pmtgrabber: do callback pid %x",GetPid());
          m_pCallback->OnPMTReceived(GetPid());
        }
        // otherwise no need to callback, i.e. if _only_ version number changes regulary...
        else 
        {
				  LogDebug("pmtgrabber: NO callback done because a/v pids still the same.");
        }
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

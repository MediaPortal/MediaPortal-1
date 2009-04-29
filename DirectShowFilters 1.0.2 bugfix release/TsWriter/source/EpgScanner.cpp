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

#include "EpgScanner.h"


extern void LogDebug(const char *fmt, ...) ;

CEpgScanner::CEpgScanner(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsEpgScanner"), pUnk)
{
  m_pCallBack=NULL;
	m_bGrabbing=false;
}

CEpgScanner::~CEpgScanner(void)
{
}

STDMETHODIMP CEpgScanner::SetCallBack(IEpgCallback* callback)
{
	LogDebug("epg: set callback");
  m_pCallBack=callback;
	return S_OK;
}

STDMETHODIMP CEpgScanner::Reset()
{
	CEnterCriticalSection enter(m_section);
	LogDebug("epg: reset");
	m_bGrabbing=false;
	m_epgParser.Reset();
  m_mhwParser.Reset();
	return S_OK;
}

STDMETHODIMP CEpgScanner::AbortGrabbing()
{
	CEnterCriticalSection enter(m_section);
	LogDebug("epg: abort grabbing");
	m_bGrabbing=false;
	m_epgParser.AbortGrabbing();
	m_mhwParser.AbortGrabbing();
	if (m_pCallBack!=NULL)
	{
		LogDebug("epg: do callback");
		m_pCallBack->OnEpgReceived();
	}
	return S_OK;
}

STDMETHODIMP CEpgScanner::GrabEPG()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("EpgScanner::GrabEPG");
		m_bGrabbing=true;
		m_epgParser.GrabEPG();
	}
	catch(...)
	{
		LogDebug("epg: GrabEPG exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::IsEPGReady(BOOL* yesNo)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		*yesNo=m_epgParser.IsEPGReady();
	}
	catch(...)
	{
		LogDebug("epg: IsEPGReady exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetEPGChannelCount( ULONG* channelCount)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("EpgScanner::GetEPGChannelCount");
		*channelCount=m_epgParser.GetEPGChannelCount( );
	}
	catch(...)
	{
		LogDebug("epg: GetEPGChannelCount exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetEPGEventCount( ULONG channelidx,  ULONG* eventCount)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("EpgScanner::GetEPGEventCount");
		*eventCount=m_epgParser.GetEPGEventCount( channelidx);
	}
	catch(...)
	{
		LogDebug("epg: GetEPGEventCount exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetEPGChannel( ULONG channelidx,  WORD* networkId,  WORD* transportid, WORD* service_id  )
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("EpgScanner::GetEPGChannel");
		m_epgParser.GetEPGChannel( channelidx,  networkId,  transportid, service_id  );
	}
	catch(...)
	{
		LogDebug("epg: GetEPGChannel exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetEPGEvent( ULONG channel,  ULONG eventidx,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** genre,int* starRating,char** classification    )
{
	CEnterCriticalSection enter(m_section);
	try
	{
    unsigned int eventid;
		m_epgParser.GetEPGEvent( channel,  eventidx, language,dateMJD, timeUTC, duration, genre,starRating,classification,&eventid   );
	}
	catch(...)
	{
		LogDebug("epg: GetEPGEvent exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetEPGLanguage(THIS_ ULONG channel, ULONG eventidx,ULONG languageIndex,ULONG* language,char** eventText, char** eventDescription,unsigned int* parentalRating    )
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("EpgScanner::GetEPGLanguage");
		m_epgParser.GetEPGLanguage( channel,  eventidx, languageIndex,language,eventText,eventDescription,parentalRating    );
	}
	catch(...)
	{
		LogDebug("epg: GetEPGLanguage exception");
	}
	return S_OK;
}

STDMETHODIMP CEpgScanner::GrabMHW()
{
	CEnterCriticalSection enter(m_section);
	try
	{
		LogDebug("EpgScanner::GrabMHW");
		m_bGrabbing=true;
		m_mhwParser.GrabEPG();
	}
	catch(...)
	{
		LogDebug("epg: GrabMHW exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::IsMHWReady(BOOL* yesNo)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		*yesNo=FALSE;
		if ( m_mhwParser.IsEPGReady()  )
		{
			*yesNo=TRUE;
			return S_OK;
		}
	}
	catch(...)
	{
		LogDebug("epg: IsMHWReady exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetMHWTitleCount(UINT* count)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		m_mhwParser.GetTitleCount(count);
    LogDebug("EpgScanner::GetMHWTitleCount:%d" ,(*count));
	}
	catch(...)
	{
		LogDebug("epg: GetMHWTitleCount exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetMHWTitle(UINT program, UINT* id, UINT* transportId, UINT* networkId, UINT* channelId, ULONG* programId, UINT* themeId, UINT* PPV, BYTE* Summaries, UINT* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName)
{	
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("EpgScanner::GetMHWTitle");
		m_mhwParser.GetTitle(program, id, transportId, networkId, channelId, programId, themeId, PPV, Summaries, duration, dateStart,timeStart,title,programName);
	}
	catch(...)
	{
		LogDebug("epg: GetMHWTitle exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetMHWChannel(UINT channelNr, UINT* channelId,UINT* networkId, UINT* transportId, char** channelName)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("EpgScanner::GetMHWChannel");
		m_mhwParser.GetChannel(channelNr,channelId, networkId, transportId, channelName);
	}
	catch(...)
	{
		LogDebug("epg: GetMHWChannel exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetMHWSummary(ULONG programId, char** summary)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("EpgScanner::GetMHWSummary");
		m_mhwParser.GetSummary(programId, summary);
	}
	catch(...)
	{
		LogDebug("epg: GetMHWSummary exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetMHWTheme(UINT themeId, char** theme)
{
	try
	{
		CEnterCriticalSection enter(m_section);
		//LogDebug("EpgScanner::GetMHWTheme");
		m_mhwParser.GetTheme(themeId, theme);
	}
	catch(...)
	{
		LogDebug("epg: GetMHWTheme exception");
	}
	return S_OK;
}

bool CEpgScanner::IsEPG_PID(int pid)
{
	return (pid==PID_EPG || pid==PID_DISH_EPG || pid==PID_BEV_EPG || pid==PID_EPG_PREMIERE_DIREKT || pid==PID_EPG_PREMIERE_SPORT);
}

bool CEpgScanner::IsMHW_PID(int pid)
{
	return (pid==PID_MHW1 || pid==PID_MHW2);
}

bool CEpgScanner::IsEIT_PID(int pid)
{
	return (IsEPG_PID(pid) || IsMHW_PID(pid));
}

void CEpgScanner::OnTsPacket(byte* tsPacket)
{
  if (!m_bGrabbing) return;
	try
	{
		int pid=((tsPacket[1] & 0x1F) <<8)+tsPacket[2];
		if (!IsEIT_PID(pid)) return;
		{
			m_header.Decode(tsPacket);
			CEnterCriticalSection enter(m_section);
			if (IsEPG_PID(pid))
				m_epgParser.OnTsPacket(m_header,tsPacket);
			else
				m_mhwParser.OnTsPacket(m_header,tsPacket);
			if (m_epgParser.IsEPGReady() && m_mhwParser.IsEPGReady())
			{
				LogDebug("epg: epg received");
				if (m_pCallBack!=NULL)
				{
					LogDebug("epg: do callback");
					m_pCallBack->OnEpgReceived();
				}
				m_bGrabbing=false;
			}
  	}
	}
	catch(...)
	{
		LogDebug("epg exception");
	}
}

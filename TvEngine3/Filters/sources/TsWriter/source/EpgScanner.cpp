/* 
 *	Copyright (C) 2006 Team MediaPortal
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
STDMETHODIMP CEpgScanner::GetEPGEventCount( ULONG channel,  ULONG* eventCount)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("EpgScanner::GetEPGEventCount");
		*eventCount=m_epgParser.GetEPGEventCount( channel);
	}
	catch(...)
	{
		LogDebug("epg: GetEPGEventCount exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  )
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("EpgScanner::GetEPGChannel");
		m_epgParser.GetEPGChannel( channel,  networkId,  transportid, service_id  );
	}
	catch(...)
	{
		LogDebug("epg: GetEPGChannel exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetEPGEvent( ULONG channel,  ULONG eventid,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** genre    )
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("EpgScanner::GetEPGEvent");
		m_epgParser.GetEPGEvent( channel,  eventid, language,dateMJD, timeUTC, duration, genre    );
	}
	catch(...)
	{
		LogDebug("epg: GetEPGEvent exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetEPGLanguage(THIS_ ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language,char** eventText, char** eventDescription    )
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("EpgScanner::GetEPGLanguage");
		m_epgParser.GetEPGLanguage( channel,  eventid, languageIndex,language,eventText,eventDescription    );
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
STDMETHODIMP CEpgScanner::GetMHWTitleCount(WORD* count)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("EpgScanner::GetMHWTitleCount");
		m_mhwParser.GetTitleCount(count);
	}
	catch(...)
	{
		LogDebug("epg: GetMHWTitleCount exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetMHWTitle(WORD program, WORD* id, WORD* transportId, WORD* networkId, WORD* channelId, WORD* programId, WORD* themeId, WORD* PPV, BYTE* Summaries, WORD* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName)
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
STDMETHODIMP CEpgScanner::GetMHWChannel(WORD channelNr, WORD* channelId,WORD* networkId, WORD* transportId, char** channelName)
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
STDMETHODIMP CEpgScanner::GetMHWSummary(WORD programId, char** summary)
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
STDMETHODIMP CEpgScanner::GetMHWTheme(WORD themeId, char** theme)
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


void CEpgScanner::OnTsPacket(byte* tsPacket)
{
	try
	{
		if (m_bGrabbing)
		{
      {//criticalsection
			  CEnterCriticalSection enter(m_section);
			  m_epgParser.OnTsPacket(tsPacket);
			  m_mhwParser.OnTsPacket(tsPacket);
			  if (m_epgParser.IsEPGReady() && m_mhwParser.IsEPGReady())
			  {
	        LogDebug("epg: epg received");
				  m_bGrabbing=false;
			  }
      }

      if (false==m_bGrabbing)
      {
        if (m_pCallBack!=NULL)
        {
	        LogDebug("epg: do callback");
          m_pCallBack->OnEpgReceived();
        }
      }

		}
	}
	catch(...)
	{
		LogDebug("epg exception");
	}
}
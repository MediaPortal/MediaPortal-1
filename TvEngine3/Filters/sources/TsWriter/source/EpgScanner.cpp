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
STDMETHODIMP CEpgScanner::GetEPGEvent( ULONG channel,  ULONG eventidx,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** genre    )
{
	CEnterCriticalSection enter(m_section);
	try
	{
    unsigned int eventid;
		m_epgParser.GetEPGEvent( channel,  eventidx, language,dateMJD, timeUTC, duration, genre,&eventid   );
	}
	catch(...)
	{
		LogDebug("epg: GetEPGEvent exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetEPGLanguage(THIS_ ULONG channel, ULONG eventidx,ULONG languageIndex,ULONG* language,char** eventText, char** eventDescription    )
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("EpgScanner::GetEPGLanguage");
		m_epgParser.GetEPGLanguage( channel,  eventidx, languageIndex,language,eventText,eventDescription    );
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
		m_mhwParser.GetTitleCount(count);
    LogDebug("EpgScanner::GetMHWTitleCount:%d" ,(*count));
	}
	catch(...)
	{
		LogDebug("epg: GetMHWTitleCount exception");
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetMHWTitle(WORD program, UINT* id, WORD* transportId, WORD* networkId, WORD* channelId, UINT* programId, WORD* themeId, WORD* PPV, BYTE* Summaries, WORD* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName)
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
STDMETHODIMP CEpgScanner::GetMHWSummary(UINT programId, char** summary)
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
  if (false==m_bGrabbing) return;
	try
	{
    /*
    CTsHeader header(tsPacket);
    if (header.Pid==0xd2 && header.PayloadUnitStart)
    {
      char buf[1255];
	    strcpy(buf,"");
	    for (int i=0; i < 30;++i)
	    {
		    char tmp[200];
		    sprintf(tmp,"%02.2x ", tsPacket[i]);
		    strcat(buf,tmp);
	    }
      LogDebug("pid:%x start:%x %s", header.Pid,header.PayLoadStart,buf);
    }*/

		if (m_bGrabbing)
		{
      int pid=((tsPacket[1] & 0x1F) <<8)+tsPacket[2];
      if (pid!=PID_EPG && pid!=PID_MHW1 && pid != PID_MHW2) return;
      {
        m_header.Decode(tsPacket);
			  CEnterCriticalSection enter(m_section);
        if (pid==PID_EPG)
			    m_epgParser.OnTsPacket(m_header,tsPacket);
        else
			    m_mhwParser.OnTsPacket(m_header,tsPacket);
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
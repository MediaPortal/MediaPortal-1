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

#include "EpgScanner.h"


extern void LogDebug(const char *fmt, ...) ;

CEpgScanner::CEpgScanner(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsEpgScanner"), pUnk)
{
}

CEpgScanner::~CEpgScanner(void)
{
}
STDMETHODIMP CEpgScanner::GrabEPG()
{
	m_epgParser.GrabEPG();
	return S_OK;
}
STDMETHODIMP CEpgScanner::IsEPGReady(BOOL* yesNo)
{
	*yesNo=m_epgParser.IsEPGReady();
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetEPGChannelCount( ULONG* channelCount)
{
	*channelCount=m_epgParser.GetEPGChannelCount( );
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetEPGEventCount( ULONG channel,  ULONG* eventCount)
{
	*eventCount=m_epgParser.GetEPGEventCount( channel);
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  )
{
	m_epgParser.GetEPGChannel( channel,  networkId,  transportid, service_id  );
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetEPGEvent( ULONG channel,  ULONG eventid,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** genre    )
{
	m_epgParser.GetEPGEvent( channel,  eventid, language,dateMJD, timeUTC, duration, genre    );

	return S_OK;
}
STDMETHODIMP CEpgScanner::GetEPGLanguage(THIS_ ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language,char** eventText, char** eventDescription    )
{
	m_epgParser.GetEPGLanguage( channel,  eventid, languageIndex,language,eventText,eventDescription    );
	return S_OK;
}

STDMETHODIMP CEpgScanner::GrabMHW()
{
	m_mhwParser.GrabEPG();
	return S_OK;
}
STDMETHODIMP CEpgScanner::IsMHWReady(BOOL* yesNo)
{
	*yesNo=FALSE;
	if ( m_mhwParser.IsEPGReady()  )
	{
		*yesNo=TRUE;
		return S_OK;
	}
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetMHWTitleCount(WORD* count)
{
	m_mhwParser.GetTitleCount(count);
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetMHWTitle(WORD program, WORD* id, WORD* transportId, WORD* networkId, WORD* channelId, WORD* programId, WORD* themeId, WORD* PPV, BYTE* Summaries, WORD* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName)
{	
	m_mhwParser.GetTitle(program, id, transportId, networkId, channelId, programId, themeId, PPV, Summaries, duration, dateStart,timeStart,title,programName);
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetMHWChannel(WORD channelNr, WORD* channelId,WORD* networkId, WORD* transportId, char** channelName)
{
	m_mhwParser.GetChannel(channelNr,channelId, networkId, transportId, channelName);
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetMHWSummary(WORD programId, char** summary)
{
	m_mhwParser.GetSummary(programId, summary);
	return S_OK;
}
STDMETHODIMP CEpgScanner::GetMHWTheme(WORD themeId, char** theme)
{
	m_mhwParser.GetTheme(themeId, theme);
	return S_OK;
}


void CEpgScanner::OnTsPacket(byte* tsPacket)
{
	m_epgParser.OnTsPacket(tsPacket);
	m_mhwParser.OnTsPacket(tsPacket);
}
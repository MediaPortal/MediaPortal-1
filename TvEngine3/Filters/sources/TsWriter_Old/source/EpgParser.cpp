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
#include <windows.h>
#include "EpgParser.h"
#pragma warning(disable : 4995)


extern void LogDebug(const char *fmt, ...) ;
CEpgParser::CEpgParser(void)
{
	// standard epg
	for (int i=0x4e; i <=0x6f;++i)
		AddSectionDecoder(PID_EPG,i);
	// DISH / BEV epg
	for (int i=0x80; i <=0xfe;++i) {
		AddSectionDecoder(PID_DISH_EPG,i);
		AddSectionDecoder(PID_BEV_EPG,i);
	}
	// Premiere DIREKT Portal
	AddSectionDecoder(PID_EPG_PREMIERE_DIREKT,0xA0);
	// Premiere SPORT Portal
	AddSectionDecoder(PID_EPG_PREMIERE_SPORT,0xA0);
}

CEpgParser::~CEpgParser(void)
{
	CEnterCriticalSection enter(m_section);
	for (int i=0; i < (int)m_vecDecoders.size();++i)
	{
		CSectionDecoder* pDecoder = m_vecDecoders[i];
		delete pDecoder;
	}
	m_vecDecoders.clear();
}

void CEpgParser::Reset()
{
	CEnterCriticalSection enter(m_section);
	for (int i=0; i < (int)m_vecDecoders.size();++i)
	{
		CSectionDecoder* pDecoder = m_vecDecoders[i];
		pDecoder->Reset();
	}
	m_bGrabbing=false;
	m_epgDecoder.ResetEPG();
}
void CEpgParser::GrabEPG()
{
	CEnterCriticalSection enter(m_section);
	LogDebug("epg:GrabEPG");
	Reset();
	m_bGrabbing=true;
	m_epgDecoder.GrabEPG();
}
bool CEpgParser::isGrabbing()
{
	CEnterCriticalSection enter(m_section);
	m_bGrabbing= m_epgDecoder.IsEPGGrabbing();
	return m_bGrabbing;
}
bool	CEpgParser::IsEPGReady()
{
	CEnterCriticalSection enter(m_section);
	bool result= m_epgDecoder.IsEPGReady();
	if (result)
	{
		m_bGrabbing=false;
	}
	return result;
}
ULONG	CEpgParser::GetEPGChannelCount( )
{
	CEnterCriticalSection enter(m_section);
	return m_epgDecoder.GetEPGChannelCount();
}
ULONG	CEpgParser::GetEPGEventCount( ULONG channel)
{
	CEnterCriticalSection enter(m_section);
	return m_epgDecoder.GetEPGEventCount(channel);
}
void	CEpgParser::GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  )
{
	CEnterCriticalSection enter(m_section);
	m_epgDecoder.GetEPGChannel(  channel,  networkId,  transportid, service_id  );
}
void	CEpgParser::GetEPGEvent( ULONG channel,  ULONG levent,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** strgenre ,int* starRating, char** classification, unsigned int* eventid   )
{
	CEnterCriticalSection enter(m_section);
	m_epgDecoder.GetEPGEvent(  channel, levent,language, dateMJD, timeUTC, duration, strgenre,starRating,classification, eventid       );
}
void  CEpgParser::GetEPGLanguage(ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language, char** eventText, char** eventDescription, unsigned int* parentalRating    )
{
	CEnterCriticalSection enter(m_section);
	m_epgDecoder.GetEPGLanguage(channel, eventid,languageIndex,language, eventText, eventDescription, parentalRating    );
}

void CEpgParser::AbortGrabbing()
{
	CEnterCriticalSection enter(m_section);
	m_bGrabbing=false;
	m_epgDecoder.AbortGrabbing();
}


void CEpgParser::OnTsPacket(CTsHeader& header, byte* tsPacket)
{
	if (m_bGrabbing==false) return;

	CEnterCriticalSection enter(m_section);
	for (int i=0; i < (int)m_vecDecoders.size();++i)
	{
		CSectionDecoder* pDecoder = m_vecDecoders[i];
		pDecoder->OnTsPacket(header,tsPacket);
	}
}

void CEpgParser::OnNewSection(int pid, int tableId, CSection& sections)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("epg new section pid:%x tableid:%x onid:%x sid:%x len:%x",pid,tableId,sections.NetworkId,sections.TransportId,sections.SectionLength);
    m_tsHeader.Decode(&sections.Data[0]);
    byte* section=&(sections.Data[m_tsHeader.PayLoadStart]);
		int sectionLength=sections.SectionLength;
		if (sectionLength>0)
		{
			if (pid==PID_EPG_PREMIERE_DIREKT || pid==PID_EPG_PREMIERE_SPORT)
				m_epgDecoder.DecodePremierePrivateEPG(section, sectionLength);
			else
				m_epgDecoder.DecodeEPG(section,	sectionLength);
		}
    else
    {
      int x=1;
    }
	}
	catch(...)
	{
		LogDebug("exception in CEpgParser::OnNewSection");
	}
}

void CEpgParser::AddSectionDecoder(int pid,int tableId)
{
	CSectionDecoder* pDecoder= new CSectionDecoder();
    pDecoder->SetPid(pid);
    pDecoder->SetTableId(tableId);
    pDecoder->EnableCrcCheck(false);
    m_vecDecoders.push_back(pDecoder);
	pDecoder->SetCallBack(this);
}

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
extern bool DisableCRCCheck();

CEpgParser::CEpgParser(void)
{
	// standard epg
	//for (int i=0x4e; i <=0x6f;++i)
	AddSectionDecoder(PID_EPG);
	// DISH / BEV epg
	//for (int i=0x80; i <=0xfe;++i) {
	AddSectionDecoder(PID_DISH_EPG);
	AddSectionDecoder(PID_BEV_EPG);
	//}
	//Freesat EPG
	AddSectionDecoder(PID_FREESAT_EPG);
	AddSectionDecoder(PID_FREESAT2_EPG);
	// Premiere DIREKT Portal
	AddSectionDecoder(PID_EPG_PREMIERE_DIREKT);
	// Premiere SPORT Portal
	AddSectionDecoder(PID_EPG_PREMIERE_SPORT);
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

bool CEpgParser::IsSectionWanted(int pid,int table_id)
{
	switch (pid)
	{
		case PID_EPG:
		case PID_FREESAT_EPG:
		case PID_FREESAT2_EPG:	
			return (table_id>=0x4e && table_id<=0x6f);
		case PID_DISH_EPG:
			return (table_id>=0x80 && table_id<=0xfe);
		case PID_BEV_EPG:
			return (table_id>=0x80 && table_id<=0xfe);
		case PID_EPG_PREMIERE_DIREKT:
			return (table_id==0xa0);
		case PID_EPG_PREMIERE_SPORT:
			return (table_id==0xa0);
	}
	return false;
}

void CEpgParser::OnNewSection(int pid,int tableId,CSection& section)
{
	CEnterCriticalSection enter(m_section);
	try
	{
		//LogDebug("epg new section pid:%x tableid:%x sid:%x len:%x",pid,tableId,section.TransportId,section.SectionLength);
		if (!IsSectionWanted(pid,tableId)) return;

		if (section.section_length>0)
		{
			if (pid==PID_EPG_PREMIERE_DIREKT || pid==PID_EPG_PREMIERE_SPORT)
				m_epgDecoder.DecodePremierePrivateEPG(section.Data, section.section_length);
			else
				m_epgDecoder.DecodeEPG(section.Data, section.section_length,pid);
		}
	}
	catch(...)
	{
		LogDebug("exception in CEpgParser::OnNewSection");
	}
}

void CEpgParser::AddSectionDecoder(int pid)
{
	CSectionDecoder* pDecoder= new CSectionDecoder();
  pDecoder->SetPid(pid);
	if (DisableCRCCheck())
		pDecoder->EnableCrcCheck(false);
	pDecoder->SetCallBack(this);
  m_vecDecoders.push_back(pDecoder);
}

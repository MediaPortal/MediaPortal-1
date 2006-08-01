#include <windows.h>
#include "EpgParser.h"

extern void LogDebug(const char *fmt, ...) ;
CEpgParser::CEpgParser(void)
{
	m_bGrabbing=false;
  for (int i=0x4e; i <=0x6f;++i)
  {
    CSectionDecoder* pDecoder= new CSectionDecoder();
    pDecoder->SetPid(0x12);
    pDecoder->SetTableId(i);
    m_vecDecoders.push_back(pDecoder);
		pDecoder->SetCallBack(this);
  }
}

CEpgParser::~CEpgParser(void)
{
	for (int i=0; i < m_vecDecoders.size();++i)
	{
		CSectionDecoder* pDecoder = m_vecDecoders[i];
		delete pDecoder;
	}
	m_vecDecoders.clear();
}

void CEpgParser::Reset()
{
	for (int i=0; i < m_vecDecoders.size();++i)
	{
		CSectionDecoder* pDecoder = m_vecDecoders[i];
		pDecoder->Reset();
	}
	m_bGrabbing=false;
	m_epgDecoder.ResetEPG();
}
void CEpgParser::GrabEPG()
{
	LogDebug("epg:GrabEPG");
	Reset();
	m_bGrabbing=true;
	m_epgDecoder.GrabEPG();
}
bool CEpgParser::isGrabbing()
{
	m_bGrabbing= m_epgDecoder.IsEPGGrabbing();
	return m_bGrabbing;
}
bool	CEpgParser::IsEPGReady()
{
	bool result= m_epgDecoder.IsEPGReady();
	if (result)
	{
		m_bGrabbing=false;
	}
	return result;
}
ULONG	CEpgParser::GetEPGChannelCount( )
{
	return m_epgDecoder.GetEPGChannelCount();
}
ULONG	CEpgParser::GetEPGEventCount( ULONG channel)
{
	return m_epgDecoder.GetEPGEventCount(channel);
}
void	CEpgParser::GetEPGChannel( ULONG channel,  WORD* networkId,  WORD* transportid, WORD* service_id  )
{
	m_epgDecoder.GetEPGChannel(  channel,  networkId,  transportid, service_id  );
}
void	CEpgParser::GetEPGEvent( ULONG channel,  ULONG levent,ULONG* language, ULONG* dateMJD, ULONG* timeUTC, ULONG* duration, char** strgenre    )
{
	m_epgDecoder.GetEPGEvent(  channel, levent,language, dateMJD, timeUTC, duration, strgenre    );
}
void  CEpgParser::GetEPGLanguage(ULONG channel, ULONG eventid,ULONG languageIndex,ULONG* language, char** eventText, char** eventDescription    )
{
	m_epgDecoder.GetEPGLanguage(channel, eventid,languageIndex,language, eventText, eventDescription    );
}


void CEpgParser::OnTsPacket(byte* tsPacket)
{
	if (m_bGrabbing==false) return;
	for (int i=0; i < m_vecDecoders.size();++i)
	{
		CSectionDecoder* pDecoder = m_vecDecoders[i];
		pDecoder->OnTsPacket(tsPacket);
	}
}

void CEpgParser::OnNewSection(int pid, int tableId, CSection& sections)
{
  //LogDebug("epg new section pid:%x tableid:%x onid:%x sid:%x",pid,tableId,sections.NetworkId,sections.TransportId);
  byte* section=&(sections.Data[5]);
  int sectionLength=sections.SectionLength;
  m_epgDecoder.DecodeEPG(section,	sectionLength);
}
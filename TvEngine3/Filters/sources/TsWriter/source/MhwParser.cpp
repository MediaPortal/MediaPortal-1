#include <windows.h>
#include <time.h>
#include "MhwParser.h"

extern void LogDebug(const char *fmt, ...) ;
CMhwParser::CMhwParser(void)
{
	m_bGrabbing=false;
	m_bDone=false;

  CSectionDecoder* pDecoder= new CSectionDecoder();
  pDecoder->SetPid(0xd2);
  pDecoder->SetTableId(0x90);
	pDecoder->SetCallBack(this);

  for (int i=0x70; i <=0x7f;++i)
  {
    CSectionDecoder* pDecoder= new CSectionDecoder();
    pDecoder->SetPid(0xd2);
    pDecoder->SetTableId(i);
    m_vecDecoders.push_back(pDecoder);
		pDecoder->SetCallBack(this);
  }
  for (int i=0x90; i <=0x92;++i)
  {
    CSectionDecoder* pDecoder= new CSectionDecoder();
    pDecoder->SetPid(0xd3);
    pDecoder->SetTableId(i);
    m_vecDecoders.push_back(pDecoder);
		pDecoder->SetCallBack(this);
  }
}

CMhwParser::~CMhwParser(void)
{
	for (int i=0; i < m_vecDecoders.size();++i)
	{
		CSectionDecoder* pDecoder = m_vecDecoders[i];
		delete pDecoder;
	}
	m_vecDecoders.clear();
}

void CMhwParser::Reset()
{
	for (int i=0; i < m_vecDecoders.size();++i)
	{
		CSectionDecoder* pDecoder = m_vecDecoders[i];
		pDecoder->Reset();
	}
	m_bGrabbing=false;
	m_bDone=false;
	m_mhwDecoder.Reset();
	m_TimeOutTimer=time(NULL);
}


void CMhwParser::OnTsPacket(byte* tsPacket)
{
	if (m_bGrabbing==false) return;

	for (int i=0; i < m_vecDecoders.size();++i)
	{
		CSectionDecoder* pDecoder = m_vecDecoders[i];
		pDecoder->OnTsPacket(tsPacket);
	}
}

void CMhwParser::OnNewSection(int pid, int tableId, CSection& sections)
{
  //LogDebug("mhw new section pid:%x tableid:%x onid:%x sid:%x",pid,tableId,sections.NetworkId,sections.TransportId);
  byte* section=&(sections.Data[5]);
  int sectionLength=sections.SectionLength;
  if (pid==0xd2)
  {
	  if (tableId==0x90 && (tableId >=0x70 && tableId <=0x7f) )
	  {
		  if ( m_mhwDecoder.ParseTitles(section,sectionLength))
		  {
			  m_TimeOutTimer=time(NULL);
		  }
	  }
  }
  if (pid==0xd3)
  {
	  if (tableId==0x90)
	  {
		  if (m_mhwDecoder.ParseSummaries(section,sectionLength))
		  {
			  m_TimeOutTimer=time(NULL);
		  }
	  }
	  if (tableId==0x91)
	  {
		  if (m_mhwDecoder.ParseChannels(section,sectionLength))
		  {
			  m_TimeOutTimer=time(NULL);
		  }
	  }
	  if (tableId==0x92)
	  {
		  if (m_mhwDecoder.ParseThemes(section,sectionLength))
		  {
			  m_TimeOutTimer=time(NULL);
		  }
	  }
  }
			
  int passed=time(NULL)-m_TimeOutTimer;
  if (passed>30)
  {
	  m_bDone=true;
	  m_bGrabbing=false;
  }
}
void CMhwParser::GrabEPG()
{
	Reset();
	m_bGrabbing=true;
	m_bDone=false;
	m_mhwDecoder.Reset();
	m_TimeOutTimer=time(NULL);
}
bool CMhwParser::isGrabbing()
{
	return m_bGrabbing;
}
bool	CMhwParser::IsEPGReady()
{
	int passed=time(NULL)-m_TimeOutTimer;
  if (passed>30)
  {
	  m_bDone=true;
	  m_bGrabbing=false;
  }
	return m_bDone;
}


void CMhwParser::GetTitleCount(WORD* count)
{
	*count=m_mhwDecoder.GetTitleCount();
}
void CMhwParser::GetTitle(WORD program, WORD* id, WORD* transportId, WORD* networkId, WORD* channelId, WORD* programId, WORD* themeId, WORD* PPV, BYTE* Summaries, WORD* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName)
{
	m_mhwDecoder.GetTitle(program, id, transportId, networkId, channelId, programId, themeId, PPV, Summaries, duration, dateStart, timeStart,title,programName);
}
void CMhwParser::GetChannel(WORD channelNr, WORD* channelId, WORD* networkId, WORD* transportId, char** channelName)
{
	m_mhwDecoder.GetChannel(channelNr, channelId,  networkId, transportId, channelName);
}
void CMhwParser::GetSummary(WORD programId, char** summary)
{
	m_mhwDecoder.GetSummary(programId, summary);
}
void CMhwParser::GetTheme(WORD themeId, char** theme)
{
	m_mhwDecoder.GetTheme(themeId, theme);
}
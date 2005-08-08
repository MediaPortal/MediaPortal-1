#include <windows.h>
#include ".\mhwparser.h"
extern void Log(const char *fmt, ...) ;

CMHWParser::CMHWParser(void)
{
}

CMHWParser::~CMHWParser(void)
{
}
void CMHWParser::Reset()
{
	m_vecChannels.clear();
	m_mapSummaries.clear();
	m_mapTitles.clear();
	m_vecThemes.clear();
}

void CMHWParser::ParseChannels(byte* data, int dataLen)
{
	if (data==NULL) return;
	if (dataLen<4) return;
	char buffer[1024];
	for(int n=4;n<dataLen;n+=22)
	{
		if( ((int)m_vecChannels.size())>=((dataLen-3)/22))
			break;
		if (n+6+16> dataLen) return;
		MHWChannel ch;
		ch.NetworkID=(data[n]<<8)+data[n+1];
		ch.TransponderID=(data[n+2]<<8)+data[n+3];
		ch.ChannelID=(data[n+4]<<8)+data[n+5];
		memcpy(buffer,&data[n+6],16);
		buffer[16]=0;
		ch.ChannelName=(char*)buffer;
		
		Log("mhw-epg: added channel '%s' prog:%d onid:0x%x tsid:0x%x",ch.ChannelName.c_str(),ch.ChannelID,ch.NetworkID,ch.TransponderID);
		m_vecChannels.push_back(ch);
	}// for(int n=0
//	Log("mhw-epg: channels:%d", m_vecChannels.size()); 
}
void CMHWParser::ParseSummaries(byte* data, int maxLen)
{
	if (data==NULL) return;

	if (maxLen < 12 || data[7] != 0xFF || data[8] != 0xFF || data[9] !=0xFF || data[10] >= 10) 
		return;	/* Invalid Data */

	int dataLen=((data[1]-0x70)<<8)+data[2];
	int n=0;
	MHWSummary sum;
	sum.ProgramID=(data[n+3]<<24)+(data[n+4]<<16)+(data[n+5]<<8)+data[n+6];
	sum.Description="";
	n+=11+(data[n+10]*7);

	if (n+(dataLen-n) >= maxLen || n>=dataLen || dataLen<=0) 
		return;
	char* buffer=new char[n+(dataLen-n)+10];
	strncpy(buffer,(const char*)&data[n],(dataLen-n));
	buffer[(dataLen-n)]=0;
	sum.Description=buffer;
	delete[] buffer;

	if(sum.ProgramID!=-1)
	{
		imapSummaries it=m_mapSummaries.find(sum.ProgramID);
		if (it==m_mapSummaries.end())
		{
			Log("mhw-epg: added summary with id 0x%x '%s'",sum.ProgramID,sum.Description.c_str());
			m_mapSummaries[sum.ProgramID]=sum;
		}
	}//if(m_summaryBuffer.Contains(sum)==false)
//	Log("mhw-epg: summaries:%d", m_mapSummaries.size());
}
void CMHWParser::ParseTitles(byte* data, int dataLen)
{
	if (data==NULL) 
		return;
	if (dataLen<42) 
		return;
	if(data[3]==0xff) 
		return;

	char buffer[30];
	MHWProgramm prg;
	prg.ChannelID=(data[3])-1;
	prg.ThemeID=data[4];
	int h=data[5] & 0x1F;
	int d=(data[5] & 0xE0)>>5;
	prg.Summaries=(data[6] & 0x80)==0?false:true;
	int m=data[6] >>2;
	prg.Duration=((data[9]<<8)+data[10]);// minutes

	memcpy(buffer,&data[11],23);
	buffer[23]=0;

	prg.Title=(char*)buffer;
	prg.PPV=(data[34]<<24)+(data[35]<<16)+(data[36]<<8)+data[37];
	prg.ID=(data[38]<<24)+(data[39]<<16)+(data[40]<<8)+data[41];
	// get time
	int d1=d;
	int h1=h;
	if (d1 == 7)
		d1 = 0;
	if (h1>15)
		h1 = h1-4;
	else if (h1>7)
		h1 = h1-2;
	else
		d1= (d1==6) ? 0 : d1+1;

	prg.timeStart=(h1<<16)+(m);
	prg.dateStart=(d1);

	//prg.Time=new DateTime(System.DateTime.Now.Ticks);
	//DateTime dayStart=new DateTime(System.DateTime.Now.Ticks);
	//dayStart=dayStart.Subtract(new TimeSpan(1,dayStart.Hour,dayStart.Minute,dayStart.Second,dayStart.Millisecond));
	//int day=(int)dayStart.DayOfWeek;
	
	//prg.Time=dayStart;
	//int minVal=(d1-day)*86400+h1*3600+m*60;
	//if(minVal<21600)
	//	minVal+=604800;

	//prg.Time=prg.Time.AddSeconds(minVal);

	Log("mhw-epg: added title %s id:%x chan:%d theme:%i sum:%d date:%x time:%02.2d:%02.2d:00 duration:%02.2d:%02.2d",
		prg.Title.c_str(),prg.ID,prg.ChannelID,prg.ThemeID,prg.Summaries,prg.dateStart,
		h1,m,
		(prg.Duration/60),(prg.Duration%60));
	m_mapTitles.push_back(prg);
//	Log("mhw-epg: titles:%d", m_mapTitles.size());
}
void CMHWParser::ParseThemes(byte* data, int dataLen)
{
	if(m_vecThemes.size()>0)
		return; // already got channles table
    if (data==NULL) return;

	int themesIndex = 3;
	int themesNames = 19;
	int theme=0;			
	int val=0;
	int count = (dataLen-19)/15;
	for (int i=0; i<count; i++)
	{
		if (themesIndex+theme>dataLen) return;
		if (data[themesIndex+theme] == i)	/* New theme */
		{
			val = (val+15) & 0xF0;
			theme++;
		}
		
		if (themesNames+15>dataLen) return;
		char buffer[20];
		memcpy(buffer,&data[themesNames],15);
		buffer[15]=0;
		MHWTheme th;
		th.ThemeText=(char*)buffer;
		th.ThemeIndex=val;
		m_vecThemes.push_back(th);
		//Log("mhw-epg: theme '%s' with id 0x%x found",th.ThemeText.c_str(),th.ThemeIndex);
		val++;
		themesNames+=15;
	}
	//Log("mhw-epg: themes:%d", m_vecThemes.size());
}

int CMHWParser::GetTitleCount()
{
	return m_mapTitles.size();
}

void CMHWParser::GetTitle(int program, WORD* id, WORD* transportId, WORD* networkId, WORD* channelId, WORD* programId, WORD* themeId, WORD* PPV, BYTE* Summaries, WORD* duration, ULONG* dateStart, ULONG* timeStart,char** title,char** programName)
{
	*id = 0;
	*transportId=0;
	*networkId=0;
	*channelId=0;
	*programId=0;
	*themeId=0;
	*PPV=0;
	*Summaries=0;
	*duration=0;
	*dateStart=0;
	*timeStart=0;
	*title="";
	*programName="";

	if (program >=(int)m_mapTitles.size()) return;
	MHWProgramm& prog=m_mapTitles[program];
	*id = prog.ID;
	*transportId=prog.TransportStreamID;
	*networkId=prog.NetworkID;
	*channelId=prog.ChannelID;
	*programId=prog.ID;
	*themeId=prog.ThemeID;
	*PPV=prog.PPV;
	*Summaries=prog.Summaries;
	*duration=prog.Duration;
	*dateStart=prog.dateStart;
	*timeStart=prog.timeStart;
	*title=(char*)prog.Title.c_str();
	//Log("mhw-epg: GetTitle(%d) size:%d chan:%d progid:%x '%s'", 
	//	program,m_mapTitles.size(),
	//	*channelId, *programId, *title);
}

void CMHWParser::GetChannel(WORD channelNr, WORD* channelId, WORD* networkId, WORD* transportId, char** channelName)
{
	*channelName="";
	
	if (channelNr>=m_vecChannels.size()) return;

	imapChannels it=m_vecChannels.begin();
	int count=0;
	while (count < channelNr) { ++it; ++count;}
	*networkId=it->NetworkID;
	*transportId=it->TransponderID;
	*channelId=it->ChannelID;
	*channelName=(char*)it->ChannelName.c_str();
	//Log("mhw-epg: GetChannel(%d) size:%d cid:%x onid:%x tsid:%x name:%s", channelNr,m_vecChannels.size(),*channelId,*networkId,*transportId,*channelName);
	return;
}

void CMHWParser::GetSummary(WORD programId, char** summary)
{
	*summary="";
	imapSummaries it=m_mapSummaries.find((int)programId);
	if (it!=m_mapSummaries.end())
	{
		*summary=(char*)it->second.Description.c_str();		
		//Log("mhw-epg: GetSummary(%x) size:%d found ", programId,m_mapSummaries.size());
	}
	else
		Log("mhw-epg: GetSummary(%x) size:%d not found", programId,m_mapSummaries.size());
}
void CMHWParser::GetTheme(WORD themeId, char** theme)
{
	*theme="";
	ivecThemes it =m_vecThemes.begin();
	while (it != m_vecThemes.end())
	{
		if (it->ThemeIndex==(int)themeId)
		{
			*theme=(char*)it->ThemeText.c_str();
			return;
		}
		++it;
	}
}

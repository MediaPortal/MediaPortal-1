#include <streams.h>
#include <bdatypes.h>
#include <time.h>
#include "TableGrabber.h"

extern void Log(const char *fmt, ...) ;
TableGrabber::TableGrabber(void)
{
	Reset();
}

TableGrabber::~TableGrabber(void)
{
}

bool TableGrabber::IsSectionGrabbed()
{
	return m_bSectionGrabbed;
}

void TableGrabber::SetTableId(int pid,int tableId)
{
	m_sectionTableID=tableId;
	m_pid=pid;
}
int   TableGrabber::Count()
{
	return m_mapSections.size();
}
byte* TableGrabber::GetTable(int section)
{
	if (section<0 || section>=(int)m_mapSections.size()) return NULL;
	imapSections it=m_mapSections.begin();
	int count=0;
	while (count < section) { it++; count++;}
	return it->second.byData;
}

int  TableGrabber::GetTableLen(int section)
{
	if (section<0 || section>=(int)m_mapSections.size()) return 0;
	imapSections it=m_mapSections.begin();
	int count=0;
	while (count < section) { it++; count++;};
	return it->second.iSize;
}


void TableGrabber::Reset()
{
	m_mapSections.clear();
	m_bSectionGrabbed=false;
	timeoutTimer=time(NULL);
}

void TableGrabber::OnPacket(byte* pbData,long lDataLen)
{
	if (m_bSectionGrabbed) return;
	int secsTimeOut=time(NULL)-timeoutTimer;
	if (secsTimeOut>30) 
	{
		Log("epg:timeout for pid:%x",m_pid);
		m_bSectionGrabbed=true;
		return;
	}
	ParseSection(pbData,lDataLen);
}

void TableGrabber::ParseSection(byte* pData, long lDataLen)
{
	DVBSectionHeader header;
	GetSectionHeader(pData,0,header);
	
	if (header.TableID==m_sectionTableID && (pData[1] >=0x70 && pData[1] <=0x7f) )
	{
		header.SectionLength+=3;
		if(header.SectionLength<1) return;
		if(header.SectionLength>4999) return;
		imapSections it;
		if (m_pid==0xd2)
		{
			if (pData[3]==0xff) return;
			if (header.SectionLength<42) return;
			ULONG prgid=(pData[38]<<24)+(pData[39]<<16)+(pData[40]<<8)+pData[41];
			//int channelID=(pData[3])-1;
			it=m_mapSections.find(prgid);
			if (it==m_mapSections.end())
			{
				TableSection newSection;
				memcpy(newSection.byData, pData,lDataLen);
				newSection.iSize=lDataLen;
				m_mapSections[prgid]=newSection;
				timeoutTimer=time(NULL);
			}
		}
		else
		{
			it=m_mapSections.find(header.SectionNumber);
			if (it==m_mapSections.end())
			{
				TableSection newSection;
				memcpy(newSection.byData, pData,lDataLen);
				newSection.iSize=lDataLen;
				m_mapSections[header.SectionNumber]=newSection;
				timeoutTimer=time(NULL);
			}			
		}
	}
}

void TableGrabber::GetSectionHeader(byte* data,int offset, DVBSectionHeader& header)
{
	if(data==NULL)
		return ;
	header.TableID = data[offset];
	header.SectionSyntaxIndicator = (data[offset+1]>>7) & 1;
	header.SectionLength = ((data[offset+1]& 0xF)<<8) + data[offset+2];
	header.TableIDExtension = (data[offset+3]<<8)+data[offset+4];
	header.VersionNumber = ((data[offset+5]>>1)&0x1F);
	header.CurrentNextIndicator = data[offset+5] & 1;
	header.SectionNumber = data[offset+6];
	header.LastSectionNumber = data[offset+7];
	header.HeaderExtB8B9=(data[offset+8]<<8)+data[offset+9];
	header.HeaderExtB10B11 = (data[offset+10]<<8)+data[offset+11];
	header.HeaderExtB12 = data[offset+12];
	header.HeaderExtB13 = data[offset+13];
}
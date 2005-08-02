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
	
	if (header.TableID==m_sectionTableID)
	{
		header.SectionLength+=3;
		if(header.SectionLength<1) return;
		if(header.SectionLength>65535) return;
		
		imapSections it=m_mapSections.find(header.SectionNumber);
		if (it==m_mapSections.end())
		{
			TableSection newSection;
			memcpy(newSection.byData, pData,lDataLen);
			newSection.iSize=lDataLen;
			m_mapSections[header.SectionNumber]=newSection;
			timeoutTimer=time(NULL);


			//Log("mhw:add pid:%x tid:%x section:%i len:%d",m_pid,header.TableID,header.SectionNumber,header.SectionLength);
			
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
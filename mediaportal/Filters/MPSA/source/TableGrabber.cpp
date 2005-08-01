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
	memset(m_tableBuffer,0,sizeof(m_tableBuffer));
	m_bufferPosition=0;
	m_bSectionGrabbed=false;
	m_lastContinuityCounter=0;
	timeoutTimer=time(NULL);
}

void TableGrabber::OnPacket(byte* pbData,long lDataLen)
{
	if (m_bSectionGrabbed) return;
	int secsTimeOut=time(NULL)-timeoutTimer;
	if (secsTimeOut>30) 
	{
		m_bSectionGrabbed=true;
		return;
	}

	for (long ptr = 0; ptr < lDataLen; ptr += 188)//main loop
	{
		Sections::TSHeader packetHeader;
		m_sectionUtils.GetTSHeader(&pbData[ptr],&packetHeader);
		if (packetHeader.Pid!=m_pid || packetHeader.SyncByte!=0x47) 
			continue;
	//	Log("pid:%x cont:%d adapt:%d ",packetHeader.Pid,packetHeader.PayloadUnitStart,packetHeader.ContinuityCounter,packetHeader.AdaptionControl);

		if(m_bufferPosition>0)
		{
			int counter=m_lastContinuityCounter;
			if(counter==15)
			counter=-1;
			if(counter+1!=packetHeader.ContinuityCounter)
			{
				//Log("dvb-demuxer: continuity counter dont match for pid {0}!={1}",counter+1,packetHeader.ContinuityCounter);
				m_bufferPosition=0;
			}
		}

		if(packetHeader.AdaptionControl==2)
		{
			//Log("ignore adapt=2");
			continue;
		}
		
		if(packetHeader.AdaptionControl==3)
		{
			//Log("ignore adapt=3");
			continue;
		}

		int offset=0;
		if(packetHeader.PayloadUnitStart==true && m_bufferPosition==0)
			offset=packetHeader.AdaptionControl+1;
		else if(packetHeader.PayloadUnitStart==true)
			offset=1;

		int tableId=pbData[ptr+4+offset];
		if (m_bufferPosition==0 && tableId!=m_sectionTableID)
		{
			//Log("invalid tableid %x!=%x",tableId,m_sectionTableID);
			continue;
		}

		if(m_bufferPosition+(184-offset)<=65535)
		{
			memcpy(&m_tableBuffer[m_bufferPosition], &pbData[ptr+4+offset],184-offset);
			m_bufferPosition+=(184-offset);
			m_lastContinuityCounter=packetHeader.ContinuityCounter;
			int sectionLength = ((m_tableBuffer[1]& 0xF)<<8) + m_tableBuffer[2];
			if(m_bufferPosition>=sectionLength && sectionLength>0)
			{
				ParseSection();
			}
		}
		else
		{
			ParseSection();
		}
	}
}

void TableGrabber::ParseSection()
{
	DVBSectionHeader header;
	GetSectionHeader(m_tableBuffer,0,header);
	if (header.TableID==m_sectionTableID)
	{
		header.SectionLength+=3;
		if(header.SectionLength<1) return;
		if(header.SectionLength>65535) return;
		
		imapSections it=m_mapSections.find(header.SectionNumber);
		if (it==m_mapSections.end())
		{
			TableSection newSection;
			memcpy(newSection.byData, m_tableBuffer,m_bufferPosition);
			newSection.iSize=m_bufferPosition;
			m_mapSections[header.SectionNumber]=newSection;
			timeoutTimer=time(NULL);
		}
	}
	m_bufferPosition=0;
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
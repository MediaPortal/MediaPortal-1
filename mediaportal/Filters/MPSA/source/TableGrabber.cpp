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
byte* TableGrabber::GetTable()
{
	return m_tableBuffer;
}

void TableGrabber::Reset()
{
	m_bufferPosition=0;
	m_bSectionGrabbed=false;
	m_lastContinuityCounter=0;
	m_sectionTableID=0;
}

void TableGrabber::OnPacket(byte* pbData,long lDataLen)
{
	if (m_bSectionGrabbed) return;
	for (long ptr = 0; ptr < lDataLen; ptr += 188)//main loop
	{
		Sections::TSHeader packetHeader;
		m_sectionUtils.GetTSHeader(&pbData[ptr],&packetHeader);
		if (packetHeader.Pid!=m_pid) continue;

		if(packetHeader.AdaptionControl==2)
		{
			//Log.Write("ignore adapt=2");
			continue;
		}
		if(packetHeader.AdaptionControl==3)
		{
			//Log.Write("ignore adapt=3");
			continue;
		}
		if(m_bufferPosition==0 && packetHeader.ContinuityCounter!=0)
		{
			continue;
		}

		int offset=0;
		if(packetHeader.PayloadUnitStart==true && m_bufferPosition==0)
			offset=packetHeader.AdaptionControl+1;
		else if(packetHeader.PayloadUnitStart==true)
			offset=1;

		int tableId=pbData[ptr+4+offset];
		if(m_bufferPosition==0 && m_sectionTableID!=tableId)
		{
			//Log.Write("ignore sectiontableid wrong");
			continue;
		}
		if(m_bufferPosition>0)
		{
			int counter=m_lastContinuityCounter;
			if(counter==15)
			counter=-1;
			if(counter+1!=packetHeader.ContinuityCounter)
			{
				//Log.Write("dvb-demuxer: continuity counter dont match for pid {0}!={1}",counter+1,m_packetHeader.ContinuityCounter);
				m_bufferPosition=0;
			}
		}

		if(m_bufferPosition+(184-offset)<=65535)
		{
			memcpy(&m_tableBuffer[m_bufferPosition], &pbData[ptr+4+offset],184-offset);
			m_bufferPosition+=(184-offset);
			m_lastContinuityCounter=packetHeader.ContinuityCounter;

			int sectionLength = ((m_tableBuffer[1]& 0xF)<<8) + m_tableBuffer[2];
			if(m_bufferPosition>=sectionLength && sectionLength>0)
			{
				m_bSectionGrabbed=true;
				return;
			}
		}
		else
		{
				m_bSectionGrabbed=true;
				return;
		}
	}
}
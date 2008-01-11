/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
#include ".\tsdemux.h"
#include "sections.h"
#include <streams.h>

extern void LogDebug(const char *fmt, ...) ;

//**********************************************************************
TsDemux::TsDemux(void)
{
	m_packetCount=0;
	m_pcrPrev=0;
	m_pcrStart=0;
	m_pcrNow=0;
	m_pcr_incr=0;
}

//**********************************************************************
TsDemux::~TsDemux(void)
{
	imapFilters it = m_mapFilters.begin();
	while (it != m_mapFilters.end())
	{
		MpegTSFilter *tss=it->second;
		delete tss;
		it=m_mapFilters.erase(it);
	}
}

//**********************************************************************
// demux TS packets and break them up in PES packets
// for each complete PES packet call DecodePesPacket()
//**********************************************************************
bool TsDemux::ParsePacket(byte* packet, bool& isStart)
{
    MpegTSFilter *tss=NULL;
    const byte *p, *p_end;
    int pid, cc, cc_ok, afc, is_start;
	
	if (packet[0] != 0x47) return false;
    pid = ((packet[1] & 0x1f) << 8) | packet[2];
	is_start = packet[1] & 0x40;
	isStart=(is_start!=0);
    cc = (packet[3] & 0xf);

	imapFilters it = m_mapFilters.find(pid);
	if (it==m_mapFilters.end())
	{
		//if (is_start)
		{
			tss = new MpegTSFilter();
			tss->m_pid=pid;
			tss->m_last_cc=cc;
			tss->m_scrambled=false;
			m_mapFilters[pid]=tss;
		}
	}
	else
	{
		tss = it->second;
	}
    if (!tss)
        return false;
    
	/* continuity check (currently not used) */
    cc_ok = (tss->m_last_cc < 0) || ((((tss->m_last_cc + 1) & 0x0f) == cc));
    tss->m_last_cc = cc;
    
	Sections sections;
	ULONGLONG pcr;
	if (S_OK==sections.CurrentPTS(NULL,packet,pcr))
	{
		if (m_pcrStart==0) 
		{
			m_pcrStart=pcr;
			m_pcrOff=0;
			m_pcrPrev=pcr;
			m_pcr_incr=0;
		}
		else
		{
			if (m_packetCount>0)
			{
				m_pcr_incr = (double)( (pcr - m_pcrPrev) / ((double)m_packetCount) );
			}
			else 
			{
				m_pcr_incr=0;
			}
		}
		m_pcrPrev=pcr;
		m_pcrOff=0;
		m_pcrNow=pcr;
		m_packetCount=0;
		
		//Sections::PTSTime time;
		//GetPCRTime(time);
		//LogDebug("pcr:%02.2d:%02.2d:%02.2d:%02.2d (%x)**",(DWORD)(DWORD)time.h,(DWORD)time.m,(DWORD)time.s,(DWORD)time.u, (DWORD)pcr);
	}
	else
	{
		m_pcrOff+=m_pcr_incr;
		m_pcrNow+=abs(m_pcrOff);
		m_pcrOff -= abs(m_pcrOff);
		m_packetCount++;
		//Sections::PTSTime time;
//		GetPCRTime(time);
//		LogDebug("pcr:%02.2d:%02.2d:%02.2d:%02.2d", (DWORD)time.h(DWORD),time.(DWORD)m,time.s,(DWORD)time.u);
	}

    /* skip adaptation field */
    afc = (packet[3] >> 4) & 3;
    p = packet + 4;
    if (afc == 0) /* reserved value */
        return false;
    if (afc == 2) /* adaptation field only */
        return false;
    if (afc == 3) {
        /* skip adapation field */
        p += p[0] + 1;	//offset=packet[4]+1
    }
	tss->m_scrambled=(packet[3] & 0xC0)!=0;
    /* if past the end of packet, ignore */
    p_end = packet + TS_PACKET_SIZE;
    if (p >= p_end)
        return false;
	if (tss->m_scrambled)
		return false;
	ParsePacket(tss,p,p_end-p,is_start);
	return (tss->m_videoFrameType==IFrame);
}

//**********************************************************************
// decode a TS packet
//**********************************************************************
void TsDemux::ParsePacket(MpegTSFilter* tss,const byte *buf, int buf_size, int is_start)
{
    const byte* p;
    int len, code;
	if (is_start) 
	{
        tss->m_state = PesHeader;
        tss->m_data_index = 0;

    }
    p = buf;
    while (buf_size > 0) 
	{
        switch(tss->m_state) 
		{
			case PesHeader:
				len = PES_START_SIZE - tss->m_data_index; // 9 bytes
				if (len > buf_size)
					len = buf_size;
				memcpy(tss->m_header + tss->m_data_index, p, len);
				tss->m_data_index += len;
				p += len;
				buf_size -= len;
				if (tss->m_data_index == PES_START_SIZE) 
				{
					/* we got all the PES or section header. We can now
					decide */
	#if 0
					av_hex_dump(tss->m_header, tss->m_data_index);
	#endif
					if (tss->m_header[0] == 0x00 && tss->m_header[1] == 0x00 &&
						tss->m_header[2] == 0x01) 
					{
						/* it must be an mpeg2 PES stream */
						code = tss->m_header[3] | 0x100;
						if (!((code >= 0x1c0 && code <= 0x1df) ||
							(code >= 0x1e0 && code <= 0x1ef) ||
							(code == 0x1bd)))
							goto skip;

						tss->m_state = PesHeaderFill;
						tss->m_total_size = (tss->m_header[4] << 8) | tss->m_header[5];
						/* NOTE: a zero total size means the PES size is
						unbounded */
						if (tss->m_total_size)
							tss->m_total_size += 6;
						tss->m_pes_header_size = tss->m_header[8] + 9;
					} 
					else 
					{
						/* otherwise, it should be a table */
						/* skip packet */
					skip:
						tss->m_state = PesSkip;
						continue;
					}
				}
            break;
				/**********************************************/
				/* PES packing parsing */
			case PesHeaderFill:
				len = tss->m_pes_header_size - tss->m_data_index;
				if (len > buf_size)
					len = buf_size;
				memcpy(tss->m_header + tss->m_data_index, p, len);
				tss->m_data_index += len;
				p += len;
				buf_size -= len;
				if (tss->m_data_index == tss->m_pes_header_size) 
				{
					const byte *r;
					unsigned int flags;

					flags = tss->m_header[7];
					r = tss->m_header + 9;
					tss->m_pts = AV_NOPTS_VALUE;
					tss->m_dts = AV_NOPTS_VALUE;
					if ((flags & 0xc0) == 0x80) 
					{
						tss->m_pts = get_pts(r);
						r += 5;
					} 
					else if ((flags & 0xc0) == 0xc0) 
					{
						tss->m_pts = get_pts(r);
						r += 5;
						tss->m_dts = get_pts(r);
						r += 5;
					}
					/* we got the full header. We parse it and get the payload */
					tss->m_state = PesPayLoad;
				}
            break;
			case PesPayLoad:
				if (tss->m_total_size) 
				{
					len = tss->m_total_size - tss->m_data_index;
					if (len > buf_size)
						len = buf_size;
				} 
				else 
				{
					len = buf_size;
				}
				if (len > 0) 
				{
					DecodePesPacket(tss, p, len);
				}
				buf_size = 0;
            break;

			case PesSkip:
				buf_size = 0;
            break;
        }
    }
}

//**********************************************************************
__int64 TsDemux::get_pts(const byte *p)
{
    __int64 pts;
    int val;

    pts = (__int64)((p[0] >> 1) & 0x07) << 30;
    val = (p[1] << 8) | p[2];
    pts |= (__int64)(val >> 1) << 15;
    val = (p[3] << 8) | p[4];
    pts |= (__int64)(val >> 1);
    return pts;
}

//**********************************************************************
// decode a complete PES packet
//**********************************************************************
void TsDemux::DecodePesPacket(MpegTSFilter* tss, const byte* pesPacket, int packetLen)
{
  int startcode=tss->m_header[3]|0x100;
  if (startcode >= 0x1e0 && startcode <= 0x1ef) 
  {
	  tss->m_streamType=VideoStream;
	  DecodeVideoPacket(tss, pesPacket, packetLen);
  } 
  else if (startcode >= 0x1c0 && startcode <= 0x1df) 
  {
     tss->m_streamType=AudioStream;
	 DecodeAudioPacket(tss,pesPacket,packetLen);
  } 
  else 
  {
	  tss->m_streamType=PrivateStream;
	  return;
  }
}


//**************************************************************************************************************************************
void TsDemux::DecodeAudioPacket(MpegTSFilter* tss, const byte* pesPacket, int packetLen)
{
	memcpy(&tss->m_audioPacket[tss->m_audioPacketLen],pesPacket,packetLen);
	tss->m_audioPacketLen+=packetLen;
}
//**************************************************************************************************************************************
void TsDemux::DecodeVideoPacket(MpegTSFilter* tss, const byte* pesPacket, int packetLen)
{
	if (tss->m_videoPacketLen+packetLen>=sizeof(tss->m_videoPacket))
	{
		int x=1;
	}
	memcpy(&tss->m_videoPacket[tss->m_videoPacketLen],pesPacket,packetLen);
	tss->m_videoPacketLen+=packetLen;

	int off=0;
	while (off < packetLen)
	{

		while (pesPacket[off]!=0x0 || pesPacket[off+1]!=0x0 || pesPacket[off+2]!=0x1)
		{
			off++;
			if (off+2>=packetLen) return;
		}
		int streamId=pesPacket[off+3];
		switch (streamId)
		{
			case 0xb3: // sequence header:
			{

				tss->m_videoWidth		 = (pesPacket[off+4]      <<4 )  + ((pesPacket[off+5]&0xf0)>>4);
				tss->m_videoHeight	     =((pesPacket[off+5]&0xf) <<8 )  + (pesPacket[off+6]);
				tss->m_videoAspectRatio  =(Mpeg2AspectRatio)((pesPacket[off+7]&0xf0)>>4 );
				tss->m_videoframeRate    = (pesPacket[off+7]&0xf);
				tss->m_videoBitRate	     = (pesPacket[off+8]      <<10)  + (pesPacket[off+9]<<2) + ((pesPacket[off+10]&0xc0)>>6);
				switch (tss->m_videoframeRate)
				{
					case 1: 
						tss->m_videoframeRate=23976;
					break;
					case 2: 
						tss->m_videoframeRate=24000;
					break;
					case 3: 
						tss->m_videoframeRate=25000;
					break;
					case 4: 
						tss->m_videoframeRate=29970;
					break;
					case 5: 
						tss->m_videoframeRate=30000;
					break;
					case 6: 
						tss->m_videoframeRate=50000;
					break;
					case 7: 
						tss->m_videoframeRate=59940;
					break;
					case 8: 
						tss->m_videoframeRate=60000;
					break;
					default:
						tss->m_videoframeRate=0;
					break;
				}
			}
			break;
			case 0xb5: // extension
			{
			}
			break;
			case 0xb2: //userdata
			{
				int x=1;
			}
			break;
			case 0xb8: //group of pictures
			{
				int x=1;
			}
			break;
			case 0x0: // Picture
			{
				m_bNewPicture=true;
				tss->m_videoFrameType= (Mpeg2FrameType )((pesPacket[off+5]>>3)&7);
				int x=1;
				if (tss->m_videoFrameType==IFrame)
				{
					static REFERENCE_TIME last=0;

					REFERENCE_TIME diff=m_pcrNow-last;
					Sections sections;
					Sections::PTSTime time;
					sections.PTSToPTSTime(diff,&time);

					REFERENCE_TIME refTime;
					refTime=MILLISECONDS_TO_100NS_UNITS(time.u);
					refTime+=MILLISECONDS_TO_100NS_UNITS(time.s*1000);
					refTime+=MILLISECONDS_TO_100NS_UNITS(time.m*1000LL*60L);
					refTime+=MILLISECONDS_TO_100NS_UNITS(time.h*1000LL*60L*60L);


					LogDebug("pcr:%02.2d:%02.2d:%02.2d:%02.2d (%x)**",(DWORD)(DWORD)time.h,(DWORD)time.m,(DWORD)time.s,(DWORD)time.u, (DWORD)refTime);
					last=m_pcrNow;


				}
			}
			break;
			case 0xb7: //sequence end
			{
				int x=1;
			}
			break;
			default:
				if (streamId>=0x01 && streamId <= 0xaf)
				{
					//slice
				}
			break;
		}
		off+=3;
	}
}

//**************************************************************************************************************************************
int TsDemux::GetVideoPacket(int videoPid,byte* packet)
{
	imapFilters it = m_mapFilters.find(videoPid);
	if (it==m_mapFilters.end()) return 0;
	MpegTSFilter* tss=it->second;
	memcpy(packet,tss->m_videoPacket,tss->m_videoPacketLen);
	int len=tss->m_videoPacketLen;
	tss->m_videoPacketLen=0;
	return len;
}

//**************************************************************************************************************************************
int TsDemux::GetAudioPacket(int audioPid,byte* packet)
{
	imapFilters it = m_mapFilters.find(audioPid);
	if (it==m_mapFilters.end()) return 0;
	MpegTSFilter* tss=it->second;
	memcpy(packet,tss->m_audioPacket,tss->m_audioPacketLen);
	int len=tss->m_audioPacketLen;
	tss->m_audioPacketLen=0;
	return len;
}

//**************************************************************************************************************************************
void TsDemux::GetVideoAttributes(int videoPid,int& videoWidth, int &videoHeight,Mpeg2AspectRatio& aspectRatio, __int64& averageTimePerFrame,long& videoBitRate,int& videoframeRate)
{
	videoPid=videoWidth=videoHeight=averageTimePerFrame=videoBitRate=videoframeRate=0;
	aspectRatio=forbidden;

	imapFilters it = m_mapFilters.find(videoPid);
	if (it==m_mapFilters.end()) return ;
	MpegTSFilter* tss=it->second;
	videoWidth=tss->m_videoWidth;
	videoHeight=tss->m_videoHeight;
	aspectRatio=tss->m_videoAspectRatio;
	averageTimePerFrame=tss->m_averageTimePerFrame;
	videoBitRate=tss->m_videoBitRate;
	videoframeRate=tss->m_videoframeRate;
}


//**************************************************************************************************************************************
void TsDemux::GetPCRTime(Sections::PTSTime& time)
{
	Sections sections;
	sections.PTSToPTSTime(m_pcrNow-m_pcrStart,&time);
}

//**************************************************************************************************************************************
void TsDemux::GetPCRReferenceTime(REFERENCE_TIME& refTime)
{
	Sections::PTSTime time;
	GetPCRTime(time);

	refTime=MILLISECONDS_TO_100NS_UNITS(time.u);
	refTime+=MILLISECONDS_TO_100NS_UNITS(time.s*1000);
	refTime+=MILLISECONDS_TO_100NS_UNITS(time.m*1000LL*60L);
	refTime+=MILLISECONDS_TO_100NS_UNITS(time.h*1000LL*60L*60L);

}
bool TsDemux::IsNewPicture()
{
	bool newPic=m_bNewPicture;
	m_bNewPicture=false;
	return newPic;
}

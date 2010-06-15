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


//**********************************************************************
TsDemux::TsDemux(void)
{
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
	isStart=is_start;

	imapFilters it = m_mapFilters.find(pid);
	if (it==m_mapFilters.end())
	{
		if (is_start)
		{
			tss = new MpegTSFilter();
			tss->pid=pid;
			tss->last_cc=-1;
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
    cc = (packet[3] & 0xf);
    cc_ok = (tss->last_cc < 0) || ((((tss->last_cc + 1) & 0x0f) == cc));
    tss->last_cc = cc;
    
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
	return (tss->frameType==IFrame);
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
        tss->state = MPEGTS_HEADER;
        tss->data_index = 0;
    }
    p = buf;
    while (buf_size > 0) 
	{
        switch(tss->state) 
		{
			case MPEGTS_HEADER:
				len = PES_START_SIZE - tss->data_index; // 9 bytes
				if (len > buf_size)
					len = buf_size;
				memcpy(tss->header + tss->data_index, p, len);
				tss->data_index += len;
				p += len;
				buf_size -= len;
				if (tss->data_index == PES_START_SIZE) 
				{
					/* we got all the PES or section header. We can now
					decide */
	#if 0
					av_hex_dump(tss->header, tss->data_index);
	#endif
					if (tss->header[0] == 0x00 && tss->header[1] == 0x00 &&
						tss->header[2] == 0x01) 
					{
						/* it must be an mpeg2 PES stream */
						code = tss->header[3] | 0x100;
						if (!((code >= 0x1c0 && code <= 0x1df) ||
							(code >= 0x1e0 && code <= 0x1ef) ||
							(code == 0x1bd)))
							goto skip;

						tss->state = MPEGTS_PESHEADER_FILL;
						tss->total_size = (tss->header[4] << 8) | tss->header[5];
						/* NOTE: a zero total size means the PES size is
						unbounded */
						if (tss->total_size)
							tss->total_size += 6;
						tss->pes_header_size = tss->header[8] + 9;
					} 
					else 
					{
						/* otherwise, it should be a table */
						/* skip packet */
					skip:
						tss->state = MPEGTS_SKIP;
						continue;
					}
				}
            break;
				/**********************************************/
				/* PES packing parsing */
			case MPEGTS_PESHEADER_FILL:
				len = tss->pes_header_size - tss->data_index;
				if (len > buf_size)
					len = buf_size;
				memcpy(tss->header + tss->data_index, p, len);
				tss->data_index += len;
				p += len;
				buf_size -= len;
				if (tss->data_index == tss->pes_header_size) 
				{
					const byte *r;
					unsigned int flags;

					flags = tss->header[7];
					r = tss->header + 9;
					tss->pts = AV_NOPTS_VALUE;
					tss->dts = AV_NOPTS_VALUE;
					if ((flags & 0xc0) == 0x80) 
					{
						tss->pts = get_pts(r);
						r += 5;
					} 
					else if ((flags & 0xc0) == 0xc0) 
					{
						tss->pts = get_pts(r);
						r += 5;
						tss->dts = get_pts(r);
						r += 5;
					}
					/* we got the full header. We parse it and get the payload */
					tss->state = MPEGTS_PAYLOAD;
				}
            break;
			case MPEGTS_PAYLOAD:
				if (tss->total_size) 
				{
					len = tss->total_size - tss->data_index;
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

			case MPEGTS_SKIP:
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
  int startcode=tss->header[3]|0x100;
  if (startcode >= 0x1e0 && startcode <= 0x1ef) 
  {
	  tss->streamType=STREAM_VIDEO;
	  DecodeVideoPacket(tss, pesPacket, packetLen);
  } 
  else if (startcode >= 0x1c0 && startcode <= 0x1df) 
  {
     tss->streamType=STREAM_AUDIO;
  } 
  else 
  {
	  tss->streamType=STREAM_PRIVATE;
	  return;
  }
}

void TsDemux::DecodeVideoPacket(MpegTSFilter* tss, const byte* pesPacket, int packetLen)
{
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
			break;
			case 0xb5: // extension
			break;
			case 0xb2: //userdata
			break;
			case 0xb8: //group of pictures
			break;
			case 0x0: // Picture
			{
				tss->frameType= (Mpeg2FrameType )((pesPacket[off+5]>>3)&7);
				int x=1;
			}
			break;
			case 0xb7: //sequence end
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

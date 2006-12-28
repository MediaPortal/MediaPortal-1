/* 
 *	Copyright (C) 2006 Team MediaPortal
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
#pragma warning(disable : 4995)
#include <windows.h>
#include "multiplexer.h"
void LogDebug(const char *fmt, ...) ;

#define PACK_SIZE                       2048
#define PACK_HEADER_FREQUENCY           1
#define SYSTEM_HEADER_FREQUENCY         40
#define PACK_HEADER_SIZE                14
#define MUX_RATE                        25961
#define PRIVATE_STREAM_1                0x1bd
#define MAX_INPUT_BUFFER_SIZE_AUDIO     4096
#define MAX_INPUT_BUFFER_SIZE_VIDEO     47104
#define PTS_DTS_LENGTH                  5
#define SYSTEM_HEADER_SIZE_BASE         12
#define SYSTEM_HEADER_SIZE_EXT          3

CMultiplexer::CMultiplexer()
{
	m_pCallback=NULL;
	m_pcrPid=-1;
  Reset();
}

CMultiplexer::~CMultiplexer()
{
  Reset();
}

void CMultiplexer::SetFileWriterCallBack(IFileWriter* callback)
{
	m_pCallback=callback;
}

void CMultiplexer::Reset()
{
	ClearStreams();
}

void CMultiplexer::ClearStreams()
{
	ivecPesDecoders it;
	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
		delete decoder;
	}
	m_pesDecoders.clear();
	m_pcrPid=-1;
	m_adaptionField.Pcr.Reset();
	m_bVideoStartFound=false;
	m_pcr.Reset();
}

void CMultiplexer::SetPcrPid(int pcrPid)
{
	m_pcrPid=pcrPid;
}

int CMultiplexer::GetPcrPid()
{
	return m_pcrPid;
}

void CMultiplexer::RemovePesStream(int pid)
{
	ivecPesDecoders it;
	it=m_pesDecoders.begin(); 
	while (it != m_pesDecoders.end())
	{
		CPesDecoder* decoder=*it;
		if (decoder->GetPid()== pid) 
		{
			delete[] decoder;
			m_pesDecoders.erase(it);
			return;
		}
		++it;
	}
  m_system_header_size=get_system_header_size();
}

void CMultiplexer::AddPesStream(int pid, bool isAudio, bool isVideo)
{
	ivecPesDecoders it;
	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
		if (decoder->GetPid()== pid) return;
	}
	
//	LogDebug("mux: add pes pid:%x", pid);
	int audioStreamId=0xc0;
	int videoStreamId=0xe0;

	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
		if (decoder->IsAudio()) audioStreamId++;
		if (decoder->IsVideo()) videoStreamId++;
	}

	CPesDecoder* decoder = new CPesDecoder(this);
	decoder->SetPid(pid);
	if (isAudio)
	{
		//LogDebug("mux pid:%x audio stream id:%x", pid,audioStreamId);
		decoder->SetStreamId(audioStreamId);
	}
	else if (isVideo)
	{
		//LogDebug("mux pid:%x video stream id:%x", pid,videoStreamId);
		decoder->SetStreamId(videoStreamId);
	}
	else
	{
		//LogDebug("mux pid:%x video stream id:-1", pid);
		decoder->SetStreamId(-1);
	}
	m_pesDecoders.push_back(decoder);

	//LogDebug("mux streams:%d", m_pesDecoders.size());
  m_system_header_size=get_system_header_size();
}


void CMultiplexer::OnTsPacket(byte* tsPacket)
{
	m_header.Decode(tsPacket);
	if (m_header.Pid==m_pcrPid)
	{
		m_adaptionField.Decode(m_header,tsPacket);
		if (m_adaptionField.PcrFlag)
		{
			m_pcr=m_adaptionField.Pcr;
    // printf("pcr:%s\n", m_pcr.ToString());
		}
	}
	if (m_pcr.PcrReferenceBase==0)
    return;

	ivecPesDecoders it;
	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
		decoder->OnTsPacket(tsPacket,m_pcr);
	}
}


int CMultiplexer::OnNewPesPacket(CPesDecoder* decoder)
{
  int streamId=-1;
  CPcr pcr;
	ivecPesDecoders it;
	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
    CPesPacket& packet=decoder->m_packet;
    if (packet.IsAvailable(1024)==false) return 0;
    
    if (pcr > packet.Pcr()  || streamId<0)
    {
      streamId=decoder->GetStreamId();
      pcr=packet.Pcr();
    }
  }

  if (streamId==decoder->GetStreamId())
  {
		CPesPacket& packet=decoder->m_packet;
		/*
		if (!m_bVideoStartFound)
		{
			if (decoder->GetStreamId()==0xe0)
			{
				if (packet.HasSequenceHeader())
					m_bVideoStartFound=true;
			}
		}
		if (!m_bVideoStartFound)
		{
			packet.Skip();
			return 0; 
		}*/
    mpeg_mux_write_packet(packet,decoder->GetStreamId());
  }
  return 0;
}

int CMultiplexer::get_packet_payload_size(CPesPacket& packet)
{
  int buf_index=0;
  if (((packet.packet_number % PACK_HEADER_FREQUENCY) == 0)) 
  {
      /* pack header size */
      buf_index += PACK_HEADER_SIZE;

      if ((packet.packet_number % SYSTEM_HEADER_FREQUENCY) == 0)
          buf_index += m_system_header_size;
  }

  if (!packet.IsStart())
  {
    /* packet header size */
    buf_index += 6;

    buf_index += 3;
    bool pts, dts;
    packet.NextPacketHasPtsDts(pts, dts);
    if (pts != 0) 
    {
      if (dts)
          buf_index += PTS_DTS_LENGTH + PTS_DTS_LENGTH;
      else
          buf_index += PTS_DTS_LENGTH;
    } 
  }
  return PACK_SIZE - buf_index;
}

int CMultiplexer::get_system_header_size()
{
  
  int buf_index = SYSTEM_HEADER_SIZE_BASE;
  int  private_stream_coded = 0;
	ivecPesDecoders it;
	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
    if (decoder->GetStreamId() < 0xc0)
    {
      if (private_stream_coded) continue;
      private_stream_coded = 1;
    }
    buf_index += SYSTEM_HEADER_SIZE_EXT;
  }
  return buf_index;
}
   

int CMultiplexer::WritePackHeader(byte* buf, CPcr& pcr)
{;
	UINT64 pcrHi=pcr.PcrReferenceBase;
	UINT64 pcrLow=pcr.PcrReferenceExtension;
  int muxRate=MUX_RATE;
	//pack header
	buf[0] = 0;
	buf[1] = 0;
	buf[2] = 1;
	buf[3] = 0xba;
	// 4             5     6         7      8       9        10        11       12       13
	//76543210  76543210 76543210 76543210 76543210 76543210 76543210 76543210 76543210 76543210
	//01PPPMPP  PPPPPPPP PPPPPMPP PPPPPPPP PPPPPMEE EEEEEEEM RRRRRRRR RRRRRRRR RRRRRRMM VVVVVSSS	 
	//4..9 = pcr 33bits / 9 bits
	buf[9]=(byte)(1  + ((pcrLow&0x7f)<<1)); pcrLow>>=7;
	buf[8]=(byte)(4  +  (pcrLow&0x3) + ((pcrHi&0x1f)<<3)); pcrHi>>=5;
	buf[7]=(byte)(0  +  (pcrHi&0xff)); pcrHi>>=8;
	buf[6]=(byte)(4  +  (pcrHi&0x3));pcrHi>>=2; buf[6]+=(byte)((pcrHi&0x1f)<<3);pcrHi>>=5;
	buf[5]=(byte)(0  +  (pcrHi&0xff));pcrHi>>=8;
	buf[4]=(byte)(0x44 + (pcrHi&0x3));pcrHi>>=2; buf[4]+=(byte)((pcrHi&7)<<3);


	//10..12 = mux rate
	buf[10] = (muxRate >> 14) & 0xff;
	buf[11] = (muxRate >> 6) & 0xff;
	buf[12] =((muxRate & 0x3f) << 2)+3;

	//13 pack stuffing length
	buf[13] = 0xf8;
  return PACK_HEADER_SIZE;
}
int CMultiplexer::WriteSystemHeader(byte* buf)
{
  // 0          1        2        3          4        5        6        7     8          9      10        11
  // 76543210 76543210 76543210 76543210 76543210 76543210 76543210 76543210 76543210 76543210 76543210 76543210 
  // hhhhhhhh hhhhhhhh hhhhhhhh hhhhhhhh llllllll llllllll 1rrrrrrr rrrrrrrr rrrrrrr1 aaaaaafc av1vvvvv rrrrrrrr
  //  0        0        1         bb       headerlength      ratebound                
  //
  
  ULONG muxRate=MUX_RATE;
  ULONG audioBound=0;
  ULONG videoBound=0;
	ivecPesDecoders it;
	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
    if (decoder->GetStreamId() >= 0xc0 && decoder->GetStreamId() <= 0xcf) audioBound++;
    if (decoder->GetStreamId() >= 0xe0 && decoder->GetStreamId() <= 0xef) videoBound++;
  }
  buf[0]=0;
  buf[1]=0;
  buf[2]=1;
  buf[3]=0xbb;
  buf[4]=0;
  buf[5]=0;
  buf[6]=(byte)(0x80 + ( (muxRate>>15)&0x7f ) );
  buf[7]=(byte)( (muxRate>>7)&0xff ) ;
  buf[8]=(byte)( 1+ (muxRate&0x7f<<1) );
  buf[9]=(byte)( ((audioBound&0x3f)<<2)) ; 
  buf[10]=(byte) (0x20+ (videoBound&0x1f));
  buf[11]=0xff;

  // 76543210 76543210 76543210
  // iiiiiiii 11bsssss ssssssss
  int offset=SYSTEM_HEADER_SIZE_BASE;
  int  private_stream_coded = 0;
	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
    int id=decoder->GetStreamId();
    if (id < 0xc0)
    {
      if (private_stream_coded)
          continue;
      private_stream_coded = 1;
      id = 0xbd;
    }
    buf[offset]=id; offset++;
    if (id < 0xe0)
    {
      /* audio */
      ULONG size=(MAX_INPUT_BUFFER_SIZE_AUDIO/128);
      buf[offset]=0xc0 + ((size>>8)&0x1f);offset++;
      buf[offset]=(size&0xff); offset++;
    }
    else
    {
      /* video */
      ULONG size=(MAX_INPUT_BUFFER_SIZE_VIDEO/128);
      buf[offset]=0xe0 + ((size>>8)&0x1f);offset++;
      buf[offset]=(size&0xff); offset++;
    }
  }

  /* patch packet size */
  buf[4] = (offset - 6) >> 8;
  buf[5] = (offset - 6) & 0xff;
 return offset;
}

/* Write an MPEG padding packet header. */
int CMultiplexer::WritePaddingHeader(byte* buf, int full_padding_size)
{
  int size = full_padding_size - 6;    /* subtract header length */

  buf[0] = (byte)(0);
  buf[1] = (byte)(0);
  buf[2] = (byte)(1);
  buf[3] = (byte)(0xbe);
  buf[4] = (byte)(size >> 8);
  buf[5] = (byte)(size & 0xff);
  return 6;
}
int CMultiplexer::WritePaddingPacket(byte* buf,int packet_bytes)
{
  int size, i;
  size = WritePaddingHeader(buf, packet_bytes);
  packet_bytes -= size;

  for(i=0;i<packet_bytes;i++)
  {
    buf[i+size]=0xff;
  }
  return packet_bytes;
}


void CMultiplexer::flush_packet(CPesPacket& packet, int streamId)
{
  byte *buf_ptr;
  int size;
  int packet_size;
  byte buffer[PACK_SIZE];
  int zero_trail_bytes = 0;
  int pad_packet_bytes = 0;
  buf_ptr = buffer;

	
	
  if (((packet.packet_number % PACK_HEADER_FREQUENCY) == 0)) 
  {
      size = WritePackHeader(buf_ptr, packet.Pcr());
      buf_ptr += size;

      if ((packet.packet_number % SYSTEM_HEADER_FREQUENCY) == 0) 
      {
          size = WriteSystemHeader(buf_ptr);
          buf_ptr += size;
      }
      
  }
  size = buf_ptr - buffer;

  packet_size = PACK_SIZE - size;
  int bytesRead;
  int pktLen=packet_size-6;
  byte* ptrSize=&buf_ptr[4];
  byte* ptrStreamId=&buf_ptr[3];
  bool isstart=false;
  if (packet.IsStart()==false)
  {
    //add pes header
    packet_size-=10;
    buf_ptr[0]=0;buf_ptr++;                               //0
    buf_ptr[0]=0;buf_ptr++;                               //1
    buf_ptr[0]=1;buf_ptr++;                               //2
    buf_ptr[0]=streamId;buf_ptr++;                        //3
    buf_ptr[0]=(byte)((pktLen>>8)&0xff);buf_ptr++; 				//4
    buf_ptr[0]=(byte)(pktLen&0xff);buf_ptr++;      				//5
    buf_ptr[0]=(byte)(0x80);buf_ptr++;                    //6
    buf_ptr[0]=(byte)(4);buf_ptr++;                       //7
		//buf_ptr[0]=(byte)(0);buf_ptr++;                       //7
		//buf_ptr[0]=(byte)(0);buf_ptr++;                       //8
    buf_ptr[0]=(byte)(1);buf_ptr++;                       //8
    buf_ptr[0]=(byte)(0x80);buf_ptr++;                    //9
  }
  else
  {
    isstart=true;
  }
   

	bytesRead=packet.Read(buf_ptr,packet_size);
	buf_ptr+=bytesRead;
	ptrStreamId[0]=(byte)streamId;
  ptrSize[0]=(byte)((pktLen>>8)&0xff); //4
  ptrSize[1]=(byte)(pktLen&0xff);      //5

  if (bytesRead!=packet_size)
  {
    int padding_bytes=packet_size-bytesRead;
    if (padding_bytes >=6)
    {
      WritePaddingPacket(buf_ptr, padding_bytes);
      pktLen-=padding_bytes;
    }
    else
    {
      pktLen-=padding_bytes;
			if (padding_bytes>0)
			{
				for (int i=0; i <padding_bytes;++i)
				{
					buf_ptr[0]=0xff;buf_ptr++;
				}
			}
    }
    ptrSize[0]=(byte)((pktLen>>8)&0xff); //4
    ptrSize[1]=(byte)(pktLen&0xff);      //5

  }

  if (m_pCallback!=NULL)
  {
    m_pCallback->Write(buffer,PACK_SIZE);
  }

  packet.packet_number++;
  //packet.nb_frames = 0;
  //packet.m_iFrameOffset = 0;
}

int CMultiplexer::mpeg_mux_write_packet(CPesPacket& packet, int streamId)
{
  int avail_size = get_packet_payload_size(packet);
  if (packet.IsAvailable(2*avail_size) )
  {
   flush_packet(packet,streamId);
  }
  return 0;
}
/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
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
	m_bFirstPacket=true;
	m_pcr.Reset();
	m_pcrStart.Reset();
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

void CMultiplexer::AddPesStream(int pid, bool isAc3, bool isAudio, bool isVideo)
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
	int ac3StreamId=0xbd;

	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
		if (decoder->IsAc3()) ac3StreamId++;
		else if (decoder->IsAudio()) audioStreamId++;
		else if (decoder->IsVideo()) videoStreamId++;
	}

	CPesDecoder* decoder = new CPesDecoder(this);
	decoder->SetPid(pid);
	if (isAc3)
	{
		//LogDebug("mux pid:%x audio stream id:%x", pid,audioStreamId);
		decoder->SetStreamId(ac3StreamId);
	}
	else if (isAudio)
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
  //did we see the start of a new video frame already ?
	if (!m_bVideoStartFound)
	{
    //then for each stream, check that
    //- 1st packet contains the start of a pes packet
    //- for the video stream, 1st packet should also contain the start of a  new video frame
		for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
		{
			CPesDecoder* decoder=*it;
			CPesPacket& packet=decoder->m_packet;
			while (packet.IsAvailable(800))
			{
				if (packet.IsStart()==false)
				{	
          //packet does not contain the start of a new pes packet. so skip it
					packet.Skip();
				}
				else 
				{
          //packet contains start of a pes packet. 
          if (decoder->GetStreamId()>=0xe0 && decoder->GetStreamId() <=0xef )
          {
            // video stream
            // does it also contain the start of a new video frame
					  if ( packet.HasSequenceHeader()==false)
					  {
              //no, then skip this packet too
						  packet.Skip();
					  }
					  else
					  {
              //packet holds the start of a new video frame 
						  break;
					  }
          }
          else
          {
            //audio or private stream
            if (packet.Pts().PcrReferenceBase!=0)
            {
              //packet contains a pts timestamp
              break;
            }
            else
            {
						  packet.Skip();
            }
          }
				}
			}
		}

    //next check if the first packet for all streams contains the start of a pes packet.
		for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
		{
			CPesDecoder* decoder=*it;
			CPesPacket& packet=decoder->m_packet;
			if (packet.IsAvailable(1024)==false) return 0; // no data available for this stream
			if (packet.IsStart()==false) return 0;  // packet does not contain a start of pes packet
      if (packet.Pts().PcrReferenceBase==0) return 0;//no pts timestamp
		}
    
    //next check first video packet contains start of video
		for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
		{
			CPesDecoder* decoder=*it;
			CPesPacket& packet=decoder->m_packet;
			if (decoder->GetStreamId()>=0xe0 && packet.HasSequenceHeader())
			{
				m_bVideoStartFound=true;
			}
		}
	}
	if (!m_bVideoStartFound) return 0;

	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
    CPesPacket& packet=decoder->m_packet;
    if (packet.IsAvailable(1024)==false) return 0;
		bool start=packet.IsStart();
    if (pcr > packet.Pcr()  || streamId<0)
    {
      streamId=decoder->GetStreamId();
      pcr=packet.Pcr();
    }
  }

  if (streamId==decoder->GetStreamId())
  {
		CPesPacket& packet=decoder->m_packet;
		
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

      //if ((packet.packet_number % SYSTEM_HEADER_FREQUENCY) == 0)
			if (m_bFirstPacket)
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
{
	if (m_pcrStart.PcrReferenceBase==0)
		m_pcrStart=pcr;
	CPcr correctedpcr=pcr-m_pcrStart;
	UINT64 pcrHi=correctedpcr.PcrReferenceBase;
	UINT64 pcrLow=correctedpcr.PcrReferenceExtension;
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
    if (decoder->IsAc3()) audioBound++;
    else if (decoder->GetStreamId() >= 0xc0 && decoder->GetStreamId() <= 0xcf) audioBound++;
    else if (decoder->GetStreamId() >= 0xe0 && decoder->GetStreamId() <= 0xef) videoBound++;
  }
  buf[0]=0;
  buf[1]=0;
  buf[2]=1;
  buf[3]=0xbb;
  buf[4]=0;
  buf[5]=0;
	//  6       7         8        9        10			11
	//76543210 76543210 76543210 76543210 76543210 76543210
	//mrrrrrrr rrrrrrrr rrrrrrrm aaaaaafc llmvvvvv pRRRRRRR
  buf[6]=(byte)(0x80 + ( (muxRate>>15)&0x7f ) );
  buf[7]=(byte)( (muxRate>>7)&0xff ) ;
  buf[8]=(byte)( 1+ (muxRate&0x7f<<1) );
  buf[9]=(byte)( 3+((audioBound&0x3f)<<2)) ; //fixed_flag + CSPS_flag
  buf[10]=(byte) (0xe0+ (videoBound&0x1f));	 //system_audio_lock_flag+system_video_lock_flag+markerbit
  buf[11]=0x7f;

  // 76543210 76543210 76543210
  // iiiiiiii 11bsssss ssssssss
  int offset=SYSTEM_HEADER_SIZE_BASE;
  for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
    int id=decoder->GetStreamId();
    buf[offset]=id; offset++;
    if ( decoder->IsAc3() )
    {
      /* ac3 */
      ULONG size=0;//(MAX_INPUT_BUFFER_SIZE_AUDIO/128);
      buf[offset]=id + ((size>>8)&0x1f);offset++;
      buf[offset]=(size&0xff); offset++;
    }
    else if (id < 0xc0)
    {
      /* private stream */
      ULONG size=0;//(MAX_INPUT_BUFFER_SIZE_AUDIO/128);
      buf[offset]=id + ((size>>8)&0x1f);offset++;
      buf[offset]=(size&0xff); offset++;
    }
    else if (id >=0xc0 && id <=0xcf)
    {
      /* mpeg audio */
      ULONG size=0;//(MAX_INPUT_BUFFER_SIZE_AUDIO/128);
      buf[offset]=0xc0 + ((size>>8)&0x1f);offset++;
      buf[offset]=(size&0xff); offset++;
    }
    else if (id>=0xe0 && id <=0xef)
    {
      /* video */
      ULONG size=0;//(MAX_INPUT_BUFFER_SIZE_VIDEO/128);
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

      //if ((packet.packet_number % SYSTEM_HEADER_FREQUENCY) == 0) 
			if (m_bFirstPacket)
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
	byte* ptrPesStart=&buf_ptr[0];
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
		PatchPtsDts(ptrPesStart,m_pcrStart);
    m_pCallback->Write(buffer,PACK_SIZE);
  }
	m_bFirstPacket=false;

  packet.packet_number++;
  //packet.nb_frames = 0;
  //packet.m_iFrameOffset = 0;
}
void CMultiplexer::PatchPtsDts(byte* pesHeader,CPcr& startPcr)
{
	CPcr pts;
	CPcr dts;
  if (!CPcr::DecodeFromPesHeader(pesHeader,pts,dts))
  {
		return ;
	}
	if (pts.IsValid)
	{
    CPcr ptsorg=pts;
		pts -= startPcr ;
		// 9       10        11        12      13
		//76543210 76543210 76543210 76543210 76543210
		//0011pppM pppppppp pppppppM pppppppp pppppppM 
//		LogDebug("pts: org:%s new:%s start:%s", ptsorg.ToString(),pts.ToString(),startPcr.ToString()); 
		byte marker=0x21;
		if (dts.PcrReferenceBase!=0) marker=0x31;
		pesHeader[13]=(byte)((( (pts.PcrReferenceBase&0x7f)<<1)+1));   pts.PcrReferenceBase>>=7;
		pesHeader[12]=(byte)(   (pts.PcrReferenceBase&0xff));				   pts.PcrReferenceBase>>=8;
		pesHeader[11]=(byte)((( (pts.PcrReferenceBase&0x7f)<<1)+1));   pts.PcrReferenceBase>>=7;
		pesHeader[10]=(byte)(   (pts.PcrReferenceBase&0xff));					 pts.PcrReferenceBase>>=8;
		pesHeader[9] =(byte)( (((pts.PcrReferenceBase&7)<<1)+marker)); 
    
	
		if (dts.IsValid)
		{
			CPcr dtsorg=dts;
			dts -= startPcr;
			// 14       15        16        17      18
			//76543210 76543210 76543210 76543210 76543210
			//0001pppM pppppppp pppppppM pppppppp pppppppM 
	//		LogDebug("dts: org:%s new:%s start:%s", dtsorg.ToString(),dts.ToString(),startPcr.ToString()); 
			pesHeader[18]=(byte)( (((dts.PcrReferenceBase&0x7f)<<1)+1));  dts.PcrReferenceBase>>=7;
			pesHeader[17]=(byte)(   (dts.PcrReferenceBase&0xff));				  dts.PcrReferenceBase>>=8;
			pesHeader[16]=(byte)( (((dts.PcrReferenceBase&0x7f)<<1)+1));  dts.PcrReferenceBase>>=7;
			pesHeader[15]=(byte)(   (dts.PcrReferenceBase&0xff));					dts.PcrReferenceBase>>=8;
			pesHeader[14]=(byte)( (((dts.PcrReferenceBase&7)<<1)+0x11)); 
		}
	
		//pts.Reset();
		//dts.Reset();
		//if (CPcr::DecodeFromPesHeader(pesHeader,pts,dts))
		//{
		//	LogDebug("pts:%s dts:%s", pts.ToString(),dts.ToString());
		//}
	}
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

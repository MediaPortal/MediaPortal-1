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
#define MUX_RATE                        0x1EB85
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
	if (m_pcr.PcrReferenceBase==0) return;

	ivecPesDecoders it;
	for (it=m_pesDecoders.begin(); it != m_pesDecoders.end();++it)
	{
		CPesDecoder* decoder=*it;
		decoder->OnTsPacket(tsPacket);
	}
}


int CMultiplexer::OnNewPesPacket(CPesDecoder* decoder, byte* data, int len)
{
	if (len<=0) return 0;

  CPcr::DecodeFromPesHeader(data, decoder->m_packet.pts, decoder->m_packet.dts);
  if (decoder->packet_number==0 && decoder->m_packet.pts.PcrReferenceBase==0) return len;
 // if (decoder->m_packet.pts.PcrReferenceBase!=0)
 //   printf("pts:%s\n", decoder->m_packet.pts.ToString());
  int headerSize=data[8]+9;
  mpeg_mux_write_packet(decoder,&data[headerSize],  len-headerSize);

  return len;
}

int CMultiplexer::get_packet_payload_size(CPesDecoder* decoder)
{
  CPesPacket& packet=decoder->m_packet;
  int buf_index=0;
  if (((decoder->packet_number % PACK_HEADER_FREQUENCY) == 0)) 
  {
      /* pack header size */
      buf_index += PACK_HEADER_SIZE;

      if ((decoder->packet_number % SYSTEM_HEADER_FREQUENCY) == 0)
          buf_index += m_system_header_size;
  }


  /* packet header size */
  buf_index += 6;

  buf_index += 3;
  if (packet.pts.PcrReferenceBase != 0) 
  {
    if (packet.pts != packet.dts && packet.dts.PcrReferenceBase!=0)
        buf_index += PTS_DTS_LENGTH + PTS_DTS_LENGTH;
    else
        buf_index += PTS_DTS_LENGTH;

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
   

int CMultiplexer::WritePackHeader(byte* buf)
{

	UINT64 pcrHi=m_pcr.PcrReferenceBase;
	UINT64 pcrLow=m_pcr.PcrReferenceExtension;
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
      buf[offset]=(byte)(0xc0 + ((size>>8)&0x1f));offset++;
      buf[offset]=(byte)(size&0xff); offset++;
    }
    else
    {
      /* video */
      ULONG size=(MAX_INPUT_BUFFER_SIZE_VIDEO/128);
      buf[offset]=(byte)(0xe0 + ((size>>8)&0x1f));offset++;
      buf[offset]=(byte)(size&0xff); offset++;
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

int CMultiplexer::mpeg_mux_write_packet(CPesDecoder* decoder,const byte *buf, int size)
{
    int len, avail_size;
    CPesPacket& packet=decoder->m_packet;

    /* we assume here that pts != AV_NOPTS_VALUE */
    /*
    new_start_pts = stream->start_pts;
    new_start_dts = stream->start_dts;

    if (stream->start_pts == AV_NOPTS_VALUE)
    {
        new_start_pts = pts;
        new_start_dts = dts;
    }*/
    avail_size = get_packet_payload_size(decoder);
    if (packet.buffer_ptr >= avail_size) 
    {
        /* unlikely case: outputing the pts or dts increase the packet
           size so that we cannot write the start of the next
           packet. In this case, we must flush the current packet with
           padding.
           Note: this always happens for the first audio and video packet
           in a VCD file, since they do not carry any data.*/
        flush_packet(decoder);
        packet.buffer_ptr = 0;
    }
    //stream->start_pts = new_start_pts;
    //stream->start_dts = new_start_dts;
    packet.nb_frames++;
    if (packet.m_iFrameOffset == 0)
        packet.m_iFrameOffset = packet.buffer_ptr;
    while (size > 0) 
    {
        avail_size = get_packet_payload_size(decoder);
        len = avail_size - packet.buffer_ptr;
        if (len > size)
            len = size;
        memcpy(packet.m_pData + packet.buffer_ptr, buf, len);
        packet.buffer_ptr += len;
        buf += len;
        size -= len;
        if (packet.buffer_ptr >= avail_size) 
        {
          //update_scr(ctx,stream_index,stream->start_pts);

          /* if packet full, we send it now */
          flush_packet(decoder);
          packet.buffer_ptr = 0;

          /* Make sure only the FIRST pes packet for this frame has a timestamp */
          packet.pts.Reset();//stream->start_pts = AV_NOPTS_VALUE;
          packet.dts.Reset();//stream->start_dts = AV_NOPTS_VALUE;
        }
    }
    return 0;
}

void CMultiplexer::flush_packet(CPesDecoder* decoder)
{
  byte *buf_ptr;
  int size, payload_size, startcode, id, stuffing_size, i, header_len;
  int packet_size;
  byte buffer[PACK_SIZE];
  int zero_trail_bytes = 0;
  int pad_packet_bytes = 0;
  id = decoder->GetStreamId();
  buf_ptr = buffer;
  CPesPacket& packet=decoder->m_packet;

  if (((decoder->packet_number % PACK_HEADER_FREQUENCY) == 0)) 
  {
      /* output pack and systems header if needed */
      size = WritePackHeader(buf_ptr);
      buf_ptr += size;

      if ((decoder->packet_number % SYSTEM_HEADER_FREQUENCY) == 0) 
      {
          size = WriteSystemHeader(buf_ptr);
          buf_ptr += size;
      }
      
  }
  size = buf_ptr - buffer;
  //put_buffer(&ctx->pb, buffer, size);

  packet_size = PACK_SIZE - size;

 
  packet_size -= pad_packet_bytes + zero_trail_bytes;

  if (packet_size > 0) 
  {
      /* packet header size */
      packet_size -= 6;

      /* packet header */
      header_len = 3;
      
      if (packet.pts.PcrReferenceBase != 0) 
      {
        if (packet.dts != packet.pts && packet.dts.PcrReferenceBase!=0)
              header_len += PTS_DTS_LENGTH + PTS_DTS_LENGTH;
          else
              header_len += PTS_DTS_LENGTH;
      } 

      payload_size = packet_size - header_len;
      if (id < 0xc0) 
      {
          startcode = PRIVATE_STREAM_1;
          payload_size -= 4;
          if (id >= 0xa0)
              payload_size -= 3;
      } 
      else 
      {
          startcode = 0x100 + id;
      }

      stuffing_size = payload_size - packet.buffer_ptr;
      if (stuffing_size < 0)
          stuffing_size = 0;

      //put_be32(&ctx->pb, startcode);
      buf_ptr[0]=0;buf_ptr++;
      buf_ptr[0]=0;buf_ptr++;
      buf_ptr[0]=1;buf_ptr++;
      buf_ptr[0]=(byte)(startcode&0xff);buf_ptr++;


      //put_be16(&ctx->pb, packet_size);
      buf_ptr[0]=((packet_size>>8)&0xff);buf_ptr++;
      buf_ptr[0]=(packet_size&0xff);buf_ptr++;


      //put_byte(&ctx->pb, 0x80); /* mpeg2 id */
      buf_ptr[0]=0x80;buf_ptr++;

      if (packet.pts.PcrReferenceBase != 0) 
      {
        if (packet.dts != packet.pts&& packet.dts.PcrReferenceBase!=0) 
          {
            byte marker=0x31;
            UINT64 pts=packet.pts.PcrReferenceBase;
            UINT64 dts=packet.dts.PcrReferenceBase;
            //put_byte(&ctx->pb, 0xc0); /* flags */
            //put_byte(&ctx->pb, header_len - 3 + stuffing_size);
            buf_ptr[0]=0xc0;buf_ptr++;
            buf_ptr[0]=(header_len - 3 + stuffing_size);buf_ptr++;
	          buf_ptr[4]=(byte)((((pts&0x7f)<<1)+1)); pts>>=7;
	          buf_ptr[3]=(byte)( (pts&0xff));				  pts>>=8;
	          buf_ptr[2]=(byte)((((pts&0x7f)<<1)+1)); pts>>=7;
	          buf_ptr[1]=(byte)((pts&0xff));					pts>>=8;
	          buf_ptr[0]=(byte)( (((pts&7)<<1)+marker)); 
            
	          buf_ptr[9]=(byte)((((dts&0x7f)<<1)+1)); dts>>=7;
	          buf_ptr[8]=(byte)( (dts&0xff));				  dts>>=8;
	          buf_ptr[7]=(byte)((((dts&0x7f)<<1)+1)); dts>>=7;
	          buf_ptr[6]=(byte)((dts&0xff));					dts>>=8;
	          buf_ptr[5]=(byte)( (((dts&7)<<1)+0x11)); 
            buf_ptr+=10;

            //put_timestamp(&ctx->pb, 0x03, pts);
            //put_timestamp(&ctx->pb, 0x01, dts);
          } 
          else 
          {
            byte marker=0x21;
            UINT64 pts=packet.pts.PcrReferenceBase;
            //put_byte(&ctx->pb, 0x80); /* flags */
            //put_byte(&ctx->pb, header_len - 3 + stuffing_size);
            buf_ptr[0]=0x80;buf_ptr++;
            buf_ptr[0]=(header_len - 3 + stuffing_size);buf_ptr++;
           // put_timestamp(&ctx->pb, 0x02, pts);
	          buf_ptr[4]=(byte)((((pts&0x7f)<<1)+1)); pts>>=7;
	          buf_ptr[3]=(byte)( (pts&0xff));				  pts>>=8;
	          buf_ptr[2]=(byte)((((pts&0x7f)<<1)+1)); pts>>=7;
	          buf_ptr[1]=(byte)((pts&0xff));					pts>>=8;
	          buf_ptr[0]=(byte)( (((pts&7)<<1)+marker)); 
            buf_ptr+=5;
          }
      } 
      else 
      {
          //put_byte(&ctx->pb, 0x00); /* flags */
          //put_byte(&ctx->pb, header_len - 3 + stuffing_size);
          buf_ptr[0]=0x0;buf_ptr++;
          buf_ptr[0]=(header_len - 3 + stuffing_size);buf_ptr++;
      }
       
/*
      if (startcode == PRIVATE_STREAM_1) 
      {
          put_byte(&ctx->pb, id);
          if (id >= 0xa0) 
          {
              // LPCM (XXX: check nb_frames) 
              put_byte(&ctx->pb, 7);
              put_be16(&ctx->pb, 4); // skip 3 header bytes 
              put_byte(&ctx->pb, stream->lpcm_header[0]);
              put_byte(&ctx->pb, stream->lpcm_header[1]);
              put_byte(&ctx->pb, stream->lpcm_header[2]);
          } 
          else 
          {
              // AC3 
              put_byte(&ctx->pb, packet.nb_frames);
              put_be16(&ctx->pb, packet.frame_start_offset);
          }
      }
*/
      for(i=0;i<stuffing_size;i++)
      {
        //put_byte(&ctx->pb, 0xff);
        buf_ptr[0]=0xff;buf_ptr++;
      }

      /* output data */
      //put_buffer(&ctx->pb, packet.m_pData, payload_size - stuffing_size);
      for (int i=0; i < payload_size - stuffing_size;++i)
      {
          buf_ptr[0]=packet.m_pData[i];
          buf_ptr++;
      }
  }

  if (pad_packet_bytes > 0)
  {
    WritePaddingPacket(buf_ptr, pad_packet_bytes);
  }

  for(int i=0;i<zero_trail_bytes;i++)
  {
    buf_ptr[0]=0x00;
    buf_ptr++;
  }

  if (m_pCallback!=NULL)
  {
    m_pCallback->Write(buffer,PACK_SIZE);
  }

  decoder->packet_number++;
  packet.nb_frames = 0;
  packet.m_iFrameOffset = 0;
}
/* 
 *	Copyright (C) 2005 Team MediaPortal
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
#pragma warning(disable:4996)
#pragma warning(disable:4995)
#include <streams.h>
#include "demultiplexer.h"
#include "buffer.h"
#include "adaptionfield.h"
#include "tsreader.h"
#include "audioPin.h"
#include "videoPin.h"
#include "subtitlePin.h"

#define OUTPUT_PACKET_LENGTH 0x6000
#define BUFFER_LENGTH        0x1000
extern void LogDebug(const char *fmt, ...) ;

CDeMultiplexer::CDeMultiplexer(CTsDuration& duration,CTsReaderFilter& filter)
:m_duration(duration)
,m_filter(filter)
{
  m_patParser.SetCallBack(this);
  m_pCurrentVideoBuffer = new CBuffer();
  m_pCurrentAudioBuffer = new CBuffer();
  m_pCurrentSubtitleBuffer = new CBuffer();
}
CDeMultiplexer::~CDeMultiplexer()
{
  Flush();
  delete m_pCurrentVideoBuffer;
  delete m_pCurrentAudioBuffer;
  delete m_pCurrentSubtitleBuffer;

}

void CDeMultiplexer::SetFileReader(FileReader* reader)
{
  m_reader=reader;
}

void CDeMultiplexer::Flush()
{
  LogDebug("demux:flushing");
  delete m_pCurrentVideoBuffer;
  delete m_pCurrentAudioBuffer;
  delete m_pCurrentSubtitleBuffer;
  
  ivecBuffers it =m_vecVideoBuffers.begin();
  while (it != m_vecVideoBuffers.end())
  {
    CBuffer* videoBuffer=*it;
    delete videoBuffer;
    it=m_vecVideoBuffers.erase(it);
  }
  it =m_vecAudioBuffers.begin();
  while (it != m_vecAudioBuffers.end())
  {
    CBuffer* AudioBuffer=*it;
    delete AudioBuffer;
    it=m_vecAudioBuffers.erase(it);
  }
  
  it =m_vecSubtitleBuffers.begin();
  while (it != m_vecSubtitleBuffers.end())
  {
    CBuffer* subtitleBuffer=*it;
    delete subtitleBuffer;
    it=m_vecSubtitleBuffers.erase(it);
  }
  m_pCurrentVideoBuffer = new CBuffer();
  m_pCurrentAudioBuffer = new CBuffer();
  m_pCurrentSubtitleBuffer = new CBuffer();
}

CBuffer* CDeMultiplexer::GetSubtitle()
{
	CAutoLock lock (&m_section);
  if (m_pids.SubtitlePid==0) return NULL;

  while (m_vecSubtitleBuffers.size()==0) 
  {
    ReadFromFile() ;
  }
  
  if (m_vecSubtitleBuffers.size()!=0)
  {
    ivecBuffers it =m_vecSubtitleBuffers.begin();
    CBuffer* subtitleBuffer=*it;
    m_vecSubtitleBuffers.erase(it);
    return subtitleBuffer;
  }
  return NULL;
}
CBuffer* CDeMultiplexer::GetVideo()
{
  
	CAutoLock lock (&m_section);
  if (m_pids.VideoPid==0)
  {
    ReadFromFile();
    return NULL;
  }
  while (m_vecVideoBuffers.size()==0) 
  {
    ReadFromFile() ;
  }
  
  if (m_vecVideoBuffers.size()!=0)
  {
    ivecBuffers it =m_vecVideoBuffers.begin();
    CBuffer* videoBuffer=*it;
    m_vecVideoBuffers.erase(it);
    return videoBuffer;
  }
  return NULL;
}

CBuffer* CDeMultiplexer::GetAudio()
{
	CAutoLock lock (&m_section);
  if (m_pids.AudioPid1==0)
  {
    ReadFromFile();
    return NULL;
  }
  while (m_vecAudioBuffers.size()==0) 
  {
    ReadFromFile() ;
  }
  if (m_vecAudioBuffers.size()!=0)
  {
    ivecBuffers it =m_vecAudioBuffers.begin();
    CBuffer* audiobuffer=*it;
    m_vecAudioBuffers.erase(it);
    return audiobuffer;
  }
  return NULL;
}

bool CDeMultiplexer::ReadFromFile()
{
  DWORD dwTick=GetTickCount();
    byte buffer[32712];
  while (true)
  {
    DWORD dwReadBytes;
    m_reader->Read(buffer,sizeof(buffer), &dwReadBytes);
    if (dwReadBytes > 0)
    {
      OnRawData(buffer,(int)dwReadBytes);
      return true;
    }
    else 
    {
      Sleep(20);
      if (GetTickCount() - dwTick >5000) break;
    }
  }
  return false;
}
void CDeMultiplexer::OnTsPacket(byte* tsPacket)
{
  CTsHeader header(tsPacket);
  m_patParser.OnTsPacket(tsPacket);
  if (m_pids.PcrPid==0) return;
  if (header.Pid==0) return;
  if (header.TransportError) return;

  if (header.Pid==m_pids.PcrPid)
  {
    CAdaptionField field;
    field.Decode(header,tsPacket);
    if (field.Pcr.IsValid)
    {
      m_streamPcr=field.Pcr;
    }
  }
  if (m_streamPcr.IsValid==false)
  {
    return;
  }
  if (header.Pid==m_pids.AudioPid1)
  {
    if (m_filter.GetAudioPin()->IsConnected())
    {
	    if ( false==header.AdaptionFieldOnly() ) 
	    {
        if ( header.PayloadUnitStart)
        {
          if (m_pCurrentAudioBuffer->Length()>0)
          {
            m_vecAudioBuffers.push_back(m_pCurrentAudioBuffer);
            m_pCurrentAudioBuffer = new CBuffer();
          }
          int pos=header.PayLoadStart;
          if (tsPacket[pos]==0&&tsPacket[pos+1]==0&&tsPacket[pos+2]==1)
          {
	          CPcr pts;
	          CPcr dts;
            if (CPcr::DecodeFromPesHeader(&tsPacket[pos],pts,dts))
            {
              m_pCurrentAudioBuffer->SetPts(pts);
            }
            int headerLen=9+tsPacket[pos+8];
            pos+=headerLen;
          }
          m_pCurrentAudioBuffer->SetPcr(m_streamPcr,m_duration.StartPcr());
          m_pCurrentAudioBuffer->Add(&tsPacket[pos],188-pos);
        }
        else if (m_pCurrentAudioBuffer->Length()>0)
        {
          int pos=header.PayLoadStart;
          if (m_pCurrentAudioBuffer->Length()+(188-pos)>=0x2000)
          {
            int copyLen=0x2000-m_pCurrentAudioBuffer->Length();
            m_pCurrentAudioBuffer->Add(&tsPacket[pos],copyLen);
            pos+=copyLen;
            m_vecAudioBuffers.push_back(m_pCurrentAudioBuffer);
            m_pCurrentAudioBuffer = new CBuffer();
          }
          m_pCurrentAudioBuffer->Add(&tsPacket[pos],188-pos); 
        }
      }
    }
 
  }
  if (header.Pid==m_pids.AudioPid2)
  {
  }
  if (header.Pid==m_pids.AC3Pid)
  {
  }
  if (header.Pid==m_pids.VideoPid)
  {
    if (m_filter.GetVideoPin()->IsConnected())
    {
	    if ( false==header.AdaptionFieldOnly() ) 
	    {
        if ( header.PayloadUnitStart)
        {
          if (m_pCurrentVideoBuffer->Length()>0)
          {
            m_vecVideoBuffers.push_back(m_pCurrentVideoBuffer);
            m_pCurrentVideoBuffer = new CBuffer();
          }
          int pos=header.PayLoadStart;
          if (tsPacket[pos]==0&&tsPacket[pos+1]==0&&tsPacket[pos+2]==1)
          {
	          CPcr pts;
	          CPcr dts;
            if (CPcr::DecodeFromPesHeader(&tsPacket[pos],pts,dts))
            {
              m_pCurrentVideoBuffer->SetPts(pts);
            }
            int headerLen=9+tsPacket[pos+8];
            pos+=headerLen;
          }
          m_pCurrentVideoBuffer->SetPcr(m_streamPcr,m_duration.StartPcr());
          m_pCurrentVideoBuffer->Add(&tsPacket[pos],188-pos);
        }
        else if (m_pCurrentVideoBuffer->Length()>0)
        {
          int pos=header.PayLoadStart;
          if (m_pCurrentVideoBuffer->Length()+(188-pos)>=0x2000)
          {
            int copyLen=0x2000-m_pCurrentVideoBuffer->Length();
            m_pCurrentVideoBuffer->Add(&tsPacket[pos],copyLen);
            pos+=copyLen;
            m_vecVideoBuffers.push_back(m_pCurrentVideoBuffer);
            m_pCurrentVideoBuffer = new CBuffer();
          }
          m_pCurrentVideoBuffer->Add(&tsPacket[pos],188-pos); 
        }
      }
    }
  }
  if (header.Pid==m_pids.SubtitlePid)
  {
    if (m_filter.GetSubtitlePin()->IsConnected())
    {
	    if ( false==header.AdaptionFieldOnly() ) 
	    {
        if ( header.PayloadUnitStart)
        {
          if (m_pCurrentSubtitleBuffer->Length()>0)
          {
            m_vecSubtitleBuffers.push_back(m_pCurrentSubtitleBuffer);
            m_pCurrentSubtitleBuffer = new CBuffer();
          }
          int pos=header.PayLoadStart;
          if (tsPacket[pos]==0&&tsPacket[pos+1]==0&&tsPacket[pos+2]==1)
          {
	          CPcr pts;
	          CPcr dts;
            if (CPcr::DecodeFromPesHeader(&tsPacket[pos],pts,dts))
            {
              m_pCurrentSubtitleBuffer->SetPts(pts);
            }
            int headerLen=9+tsPacket[pos+8];
            pos+=headerLen;
          }
          m_pCurrentSubtitleBuffer->SetPcr(m_streamPcr,m_duration.StartPcr());
          m_pCurrentSubtitleBuffer->Add(&tsPacket[pos],188-pos);
        }
        else if (m_pCurrentSubtitleBuffer->Length()>0)
        {
          int pos=header.PayLoadStart;
          if (m_pCurrentSubtitleBuffer->Length()+(188-pos)>=0x2000)
          {
            int copyLen=0x2000-m_pCurrentSubtitleBuffer->Length();
            m_pCurrentSubtitleBuffer->Add(&tsPacket[pos],copyLen);
            pos+=copyLen;
            m_vecSubtitleBuffers.push_back(m_pCurrentSubtitleBuffer);
            m_pCurrentSubtitleBuffer = new CBuffer();
          }
          m_pCurrentSubtitleBuffer->Add(&tsPacket[pos],188-pos); 
        }
      }
    }
  }
}
	
void CDeMultiplexer::OnNewChannel(CChannelInfo& info)
{
	CAutoLock lock (&m_section);
  CPidTable pids=info.PidTable;
  if (  m_pids.AudioPid1==pids.AudioPid1 &&
				m_pids.AudioPid2==pids.AudioPid2 &&
				m_pids.AudioPid3==pids.AudioPid3 &&
				m_pids.AudioPid4==pids.AudioPid4 &&
				m_pids.AudioPid5==pids.AudioPid5 &&
				m_pids.AudioPid6==pids.AudioPid6 &&
				m_pids.AudioPid7==pids.AudioPid7 &&
				m_pids.AC3Pid==pids.AC3Pid &&
				m_pids.PcrPid==pids.PcrPid &&
				m_pids.PmtPid==pids.PmtPid &&
				m_pids.SubtitlePid==pids.SubtitlePid)
	{
		if ( pids.videoServiceType==0x1b && m_pids.VideoPid==pids.VideoPid) return;
		if ( pids.videoServiceType==0x10 && m_pids.VideoPid==pids.VideoPid) return;
		if ( m_pids.VideoPid==pids.VideoPid) return;
	}
  m_pids=pids;
  LogDebug("New channel found");
  LogDebug(" video    pid:%x type:%x",m_pids.VideoPid,pids.videoServiceType);
  LogDebug(" audio1   pid:%x ",m_pids.AudioPid1);
  LogDebug(" audio2   pid:%x ",m_pids.AudioPid2);
  LogDebug(" audio3   pid:%x ",m_pids.AudioPid3);
  LogDebug(" audio4   pid:%x ",m_pids.AudioPid4);
  LogDebug(" audio5   pid:%x ",m_pids.AudioPid5);
  LogDebug(" audio6   pid:%x ",m_pids.AudioPid6);
  LogDebug(" audio7   pid:%x ",m_pids.AudioPid7);
  LogDebug(" Pcr      pid:%x ",m_pids.PcrPid);
  LogDebug(" Pmt      pid:%x ",m_pids.PmtPid);
  LogDebug(" Subtitle pid:%x ",m_pids.SubtitlePid);

}
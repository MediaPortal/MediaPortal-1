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

#define MAX_BUF_SIZE 300
#define OUTPUT_PACKET_LENGTH 0x6000e
#define BUFFER_LENGTH        0x1000
extern void LogDebug(const char *fmt, ...) ;
extern byte* MPEG1AudioFormat;

CDeMultiplexer::CDeMultiplexer(CTsDuration& duration,CTsReaderFilter& filter)
:m_duration(duration)
,m_filter(filter)
{
  m_patParser.SetCallBack(this);
  m_pCurrentVideoBuffer = new CBuffer();
  m_pCurrentAudioBuffer = new CBuffer();
  m_pCurrentSubtitleBuffer = new CBuffer();
  m_iAudioStream=0;
  m_audioPid=0;
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

CPidTable CDeMultiplexer::GetPidTable()
{
  return m_pids;
}

void CDeMultiplexer::SetAudioStream(int stream)
{
  m_iAudioStream=stream;   
}

int CDeMultiplexer::GetAudioStream()
{
  return m_iAudioStream;
}

void CDeMultiplexer::GetAudioStreamInfo(int stream,char* szName)
{
  if (stream==0)
  {
    szName[0]=m_pids.Lang1_1;
    szName[1]=m_pids.Lang1_2;
    szName[2]=m_pids.Lang1_3;
    szName[3]=0;
  }
  if (stream==1)
  {
    szName[0]=m_pids.Lang2_1;
    szName[1]=m_pids.Lang2_2;
    szName[2]=m_pids.Lang3_3;
    szName[3]=0;
  }
  if (stream==2)
  {
    szName[0]=m_pids.Lang3_1;
    szName[1]=m_pids.Lang3_2;
    szName[2]=m_pids.Lang3_3;
    szName[3]=0;
  }
  if (stream==3)
  {
    szName[0]=m_pids.Lang4_1;
    szName[1]=m_pids.Lang4_2;
    szName[2]=m_pids.Lang4_3;
    szName[3]=0;
  }
  if (stream==4)
  {
    szName[0]=m_pids.Lang5_1;
    szName[1]=m_pids.Lang5_2;
    szName[2]=m_pids.Lang5_3;
    szName[3]=0;
  }
  if (stream==5)
  {
    szName[0]=m_pids.Lang6_1;
    szName[1]=m_pids.Lang6_2;
    szName[2]=m_pids.Lang6_3;
    szName[3]=0;
  }
  if (stream==6)
  {
    szName[0]=m_pids.Lang7_1;
    szName[1]=m_pids.Lang7_2;
    szName[2]=m_pids.Lang7_3;
    szName[3]=0;
  }
  if (stream==7)
  {
    strcpy(szName,"AC3");
  }
}
int CDeMultiplexer::GetAudioStreamCount()
{
  int streamCount=0;
  if (m_pids.AudioPid1!=0) streamCount++;
  if (m_pids.AudioPid2!=0) streamCount++;
  if (m_pids.AudioPid3!=0) streamCount++;
  if (m_pids.AudioPid4!=0) streamCount++;
  if (m_pids.AudioPid5!=0) streamCount++;
  if (m_pids.AudioPid6!=0) streamCount++;
  if (m_pids.AudioPid7!=0) streamCount++;
  if (m_pids.AC3Pid!=0) streamCount++;
  return streamCount;
}

void CDeMultiplexer::GetAudioStreamType(int stream,CMediaType& pmt)
{
  if (stream <= 7)
  {
	    pmt.InitMediaType();
	    pmt.SetType      (& MEDIATYPE_Audio);
	    pmt.SetSubtype   (& MEDIASUBTYPE_MPEG2_AUDIO);
	    pmt.SetSampleSize(1);
	    pmt.SetTemporalCompression(FALSE);
	    pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      int i=sizeof(MPEG1AudioFormat);
	    pmt.SetFormat(MPEG1AudioFormat,sizeof(MPEG1AudioFormat));
      
  }
  else
  {
	    pmt.InitMediaType();
	    pmt.SetType      (& MEDIATYPE_Audio);
	    pmt.SetSubtype   (& MEDIASUBTYPE_DOLBY_AC3);
	    pmt.SetSampleSize(1);
	    pmt.SetTemporalCompression(FALSE);
	    pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      int i=sizeof(MPEG1AudioFormat);
	    pmt.SetFormat(MPEG1AudioFormat,sizeof(MPEG1AudioFormat));
  }
}

void CDeMultiplexer::Flush()
{
	CAutoLock lock (&m_section);
//  LogDebug("demux:flushing");
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
    if (!m_filter.IsFilterRunning()) return NULL;
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
  if (  m_audioPid==0)
  {
    ReadFromFile();
    return NULL;
  }
  while (m_vecAudioBuffers.size()==0) 
  {
    if (!m_filter.IsFilterRunning()) return NULL;
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

void CDeMultiplexer:: Start()
{
  DWORD dwBytesProcessed=0;
  while (ReadFromFile())
  {
    if (dwBytesProcessed>1000000 || GetAudioStreamCount()>0)
    {
      m_reader->SetFilePointer(0,FILE_BEGIN);
      Flush();
      m_streamPcr=m_duration.StartPcr();
      return;
    }
    dwBytesProcessed+=32712;
  }
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
      Sleep(100);
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

  //CAdaptionField field;
  //field.Decode(header,tsPacket);
  //if (field.Pcr.IsValid)
  //{
  //  LogDebug("pcr:%f", field.Pcr.ToClock());
  //}

  if (!m_duration.StartPcr().IsValid)
  {
    CAdaptionField field;
    field.Decode(header,tsPacket);
    if (field.Pcr.IsValid)
    {
      m_duration.Set(field.Pcr,field.Pcr);
    }
  }
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
  FillSubtitle(header,tsPacket);
  FillAudio(header,tsPacket);
  FillVideo(header,tsPacket);
  
}
void CDeMultiplexer::FillAudio(CTsHeader& header, byte* tsPacket)
{
  if (m_iAudioStream==0) m_audioPid=m_pids.AudioPid1;
  else if (m_iAudioStream==1) m_audioPid=m_pids.AudioPid2;
  else if (m_iAudioStream==2) m_audioPid=m_pids.AudioPid3;
  else if (m_iAudioStream==3) m_audioPid=m_pids.AudioPid4;
  else if (m_iAudioStream==4) m_audioPid=m_pids.AudioPid5;
  else if (m_iAudioStream==5) m_audioPid=m_pids.AudioPid6;
  else if (m_iAudioStream==6) m_audioPid=m_pids.AudioPid7;
  else if (m_iAudioStream==7) m_audioPid=m_pids.AC3Pid;
  if (m_audioPid==0 || m_audioPid != header.Pid) return;

  if (m_filter.GetAudioPin()->IsConnected())
  {
    if ( false==header.AdaptionFieldOnly() ) 
    {
      if ( header.PayloadUnitStart)
      {
        if (m_pCurrentAudioBuffer->Length()>0)
        {
          if (m_vecAudioBuffers.size()>MAX_BUF_SIZE) 
            m_vecAudioBuffers.erase(m_vecAudioBuffers.begin());
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
				if (pos>0 && pos < 188)
				{
					m_pCurrentAudioBuffer->SetPcr(m_streamPcr,m_duration.StartPcr());
					m_pCurrentAudioBuffer->Add(&tsPacket[pos],188-pos);
				}
      }
      else if (m_pCurrentAudioBuffer->Length()>0)
      {
        int pos=header.PayLoadStart;
        if (m_pCurrentAudioBuffer->Length()+(188-pos)>=0x2000)
        {
          int copyLen=0x2000-m_pCurrentAudioBuffer->Length();
          m_pCurrentAudioBuffer->Add(&tsPacket[pos],copyLen);
          pos+=copyLen;
          
          if (m_vecAudioBuffers.size()>MAX_BUF_SIZE) 
            m_vecAudioBuffers.erase(m_vecAudioBuffers.begin());
          m_vecAudioBuffers.push_back(m_pCurrentAudioBuffer);
          m_pCurrentAudioBuffer = new CBuffer();
        }
				if (pos>0 && pos < 188)
				{
					m_pCurrentAudioBuffer->Add(&tsPacket[pos],188-pos); 
				}
      }
    }
  }
}

void CDeMultiplexer::FillVideo(CTsHeader& header, byte* tsPacket)
{
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
            if (m_vecVideoBuffers.size()>MAX_BUF_SIZE) 
              m_vecVideoBuffers.erase(m_vecVideoBuffers.begin());
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
					if (pos>0 && pos < 188)
					{
						m_pCurrentVideoBuffer->SetPcr(m_streamPcr,m_duration.StartPcr());
						m_pCurrentVideoBuffer->Add(&tsPacket[pos],188-pos);
					}
        }
        else if (m_pCurrentVideoBuffer->Length()>0)
        {
          int pos=header.PayLoadStart;
          if (m_pCurrentVideoBuffer->Length()+(188-pos)>=0x2000)
          {
            int copyLen=0x2000-m_pCurrentVideoBuffer->Length();
            m_pCurrentVideoBuffer->Add(&tsPacket[pos],copyLen);
            pos+=copyLen;

            if (m_vecVideoBuffers.size()>MAX_BUF_SIZE) 
              m_vecVideoBuffers.erase(m_vecVideoBuffers.begin());
            m_vecVideoBuffers.push_back(m_pCurrentVideoBuffer);
            m_pCurrentVideoBuffer = new CBuffer();
          }
					if (pos>0 && pos < 188)
					{
						m_pCurrentVideoBuffer->Add(&tsPacket[pos],188-pos); 
					}
        }
      }
    }
  }
}
void CDeMultiplexer::FillSubtitle(CTsHeader& header, byte* tsPacket)
{
  if (header.Pid==m_pids.SubtitlePid || header.Pid==m_pids.PcrPid|| header.Pid==m_pids.PmtPid|| header.Pid==0)
  {
    if (m_filter.GetSubtitlePin()->IsConnected())
    {
	    if ( false==header.AdaptionFieldOnly() ) 
	    {
        if ( header.PayloadUnitStart)
        {
          if (m_pCurrentSubtitleBuffer->Length()>0)
          {
            if (m_vecSubtitleBuffers.size()>MAX_BUF_SIZE) 
              m_vecSubtitleBuffers.erase(m_vecSubtitleBuffers.begin());
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
          }
          m_pCurrentSubtitleBuffer->SetPcr(m_streamPcr,m_duration.StartPcr());
          m_pCurrentSubtitleBuffer->Add(tsPacket,188);
        }
        else if (m_pCurrentSubtitleBuffer->Length()>0)
        {
          if (m_pCurrentSubtitleBuffer->Length()+(188)>=0x2000)
          {
            m_pCurrentSubtitleBuffer->Add(tsPacket,188);
            if (m_vecSubtitleBuffers.size()>MAX_BUF_SIZE) 
              m_vecSubtitleBuffers.erase(m_vecSubtitleBuffers.begin());
            m_vecSubtitleBuffers.push_back(m_pCurrentSubtitleBuffer);
            m_pCurrentSubtitleBuffer = new CBuffer();
          }
          else
          {
            m_pCurrentSubtitleBuffer->Add(tsPacket,188); 
          }
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

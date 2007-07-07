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
#include "MediaFormats.h"

#define MAX_BUF_SIZE 300
#define OUTPUT_PACKET_LENGTH 0x6000e
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
  m_iAudioStream=0;
  m_audioPid=0;
  m_bScanning=false;
  m_bEndOfFile=false;
}

CDeMultiplexer::~CDeMultiplexer()
{
  Flush();
  delete m_pCurrentVideoBuffer;
  delete m_pCurrentAudioBuffer;
  delete m_pCurrentSubtitleBuffer;

}

void CDeMultiplexer::GetVideoMedia(CMediaType *pmt)
{
	pmt->InitMediaType();
	pmt->SetType      (& MEDIATYPE_Video);
	pmt->SetSubtype   (& MEDIASUBTYPE_MPEG2_VIDEO);
	pmt->SetFormatType(&FORMAT_MPEG2Video);
	pmt->SetSampleSize(1);
	pmt->SetTemporalCompression(FALSE);
	pmt->SetVariableSize();
	pmt->SetFormat(g_Mpeg2ProgramVideo,sizeof(g_Mpeg2ProgramVideo));
}

void CDeMultiplexer::GetH264Media(CMediaType *pmt)
{
	pmt->InitMediaType();
	pmt->SetType      (& MEDIATYPE_Video);
	pmt->SetSubtype   (& H264_SubType);
	pmt->SetFormatType(&FORMAT_VideoInfo);
	pmt->SetSampleSize(1);
	pmt->SetTemporalCompression(TRUE);
	pmt->SetVariableSize();
	pmt->SetFormat(H264VideoFormat,sizeof(H264VideoFormat));
}

void CDeMultiplexer::GetMpeg4Media(CMediaType *pmt)
{
	pmt->InitMediaType();
	pmt->SetType      (& MEDIATYPE_Video);
	pmt->SetSubtype   (&MPG4_SubType);
	pmt->SetFormatType(&FORMAT_MPEG2Video);
	pmt->SetSampleSize(1);
	pmt->SetTemporalCompression(TRUE);
	pmt->SetVariableSize();
	pmt->SetFormat(g_Mpeg2ProgramVideo,sizeof(g_Mpeg2ProgramVideo));
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
  if (stream< 0 || stream>=m_audioStreams.size()) return;

  m_iAudioStream=stream;   

  int oldAudioStreamType=0;
  if (m_iAudioStream>=0 && m_iAudioStream < m_audioStreams.size())
  {
    oldAudioStreamType=m_audioStreams[m_iAudioStream].audioType;
  }

  HRESULT isPlaying=IsPlaying();
  int newAudioStreamType=0;
  if (m_iAudioStream>=0 && m_iAudioStream < m_audioStreams.size())
  {
    newAudioStreamType=m_audioStreams[m_iAudioStream].audioType;
  }
  if (oldAudioStreamType != newAudioStreamType )
  {
    if (m_filter.GetAudioPin()->IsConnected())
    {
      // change audio pin media type
      if (DoStop()==S_OK ){while(IsStopped() == S_FALSE){Sleep(100); break;}}
      RenderFilterPin(m_filter.GetAudioPin());
    }
  }
  if (isPlaying)
  {
    DoStart();
  }

}

int CDeMultiplexer::GetAudioStream()
{
  return m_iAudioStream;
}

void CDeMultiplexer::GetAudioStreamInfo(int stream,char* szName)
{
  if (stream <0 || stream>=m_audioStreams.size())
  {
    szName[0]=szName[1]=szName[2]=0;
    return;
  }
    szName[0]=m_audioStreams[stream].language[0];
    szName[1]=m_audioStreams[stream].language[1];
    szName[2]=m_audioStreams[stream].language[2];
    szName[3]=m_audioStreams[stream].language[3];
}
int CDeMultiplexer::GetAudioStreamCount()
{
  return m_audioStreams.size();
}

void CDeMultiplexer::GetAudioStreamType(int stream,CMediaType& pmt)
{
  if (m_iAudioStream< 0 || m_iAudioStream >=m_audioStreams.size())
  {
	    pmt.InitMediaType();
	    pmt.SetType      (& MEDIATYPE_Audio);
	    pmt.SetSubtype   (& MEDIASUBTYPE_MPEG2_AUDIO);
	    pmt.SetSampleSize(1);
	    pmt.SetTemporalCompression(FALSE);
	    pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(MPEG1AudioFormat,sizeof(MPEG1AudioFormat));
      return;
  }

  switch (m_audioStreams[m_iAudioStream].audioType)
  {
    case SERVICE_TYPE_AUDIO_MPEG1:
	    pmt.InitMediaType();
	    pmt.SetType      (& MEDIATYPE_Audio);
	    pmt.SetSubtype   (& MEDIASUBTYPE_MPEG2_AUDIO);
	    pmt.SetSampleSize(1);
	    pmt.SetTemporalCompression(FALSE);
	    pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(MPEG1AudioFormat,sizeof(MPEG1AudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_MPEG2:
	    pmt.InitMediaType();
	    pmt.SetType      (& MEDIATYPE_Audio);
	    pmt.SetSubtype   (& MEDIASUBTYPE_MPEG2_AUDIO);
	    pmt.SetSampleSize(1);
	    pmt.SetTemporalCompression(FALSE);
	    pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(MPEG1AudioFormat,sizeof(MPEG1AudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_AC3:
	    pmt.InitMediaType();
	    pmt.SetType      (& MEDIATYPE_Audio);
	    pmt.SetSubtype   (& MEDIASUBTYPE_DOLBY_AC3);
	    pmt.SetSampleSize(1);
	    pmt.SetTemporalCompression(FALSE);
	    pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(MPEG1AudioFormat,sizeof(MPEG1AudioFormat));
      break;

  }
}

void CDeMultiplexer::GetVideoStreamType(CMediaType& pmt)
{
	pmt.InitMediaType();
  switch (m_pids.videoServiceType)
  {
    case SERVICE_TYPE_VIDEO_MPEG1:
      GetVideoMedia(&pmt);
    break;
    case SERVICE_TYPE_VIDEO_MPEG2:
      GetVideoMedia(&pmt);
    break;
    case SERVICE_TYPE_VIDEO_MPEG4:
      GetMpeg4Media(&pmt);
    break;
    case SERVICE_TYPE_VIDEO_H264:
      GetH264Media(&pmt);
    break;
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
    if (m_bEndOfFile) return NULL;
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
    if (m_bEndOfFile) return NULL;
		if (m_bPause) return NULL;
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
    if (m_bEndOfFile) return NULL;
		if (m_bPause) return NULL;
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
  m_bEndOfFile=false;
  m_bScanning=true;
  DWORD dwBytesProcessed=0;
  while (ReadFromFile())
  {
    if (dwBytesProcessed>1000000 || GetAudioStreamCount()>0)
    {
      m_reader->SetFilePointer(0,FILE_BEGIN);
      Flush();
      m_streamPcr.Reset();
      m_bScanning=false;
      return;
    }
    dwBytesProcessed+=32712;
  }
  m_streamPcr.Reset();
  m_bScanning=false;
}

bool CDeMultiplexer::EndOfFile()
{
  return m_bEndOfFile;
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
			
			if (m_bPause) return false;
      if (!m_filter.IsTimeShifting())
      {
        m_bEndOfFile=true;
        return false;
      }
      Sleep(100);
      if (GetTickCount() - dwTick >5000) break;
    }
		if (m_bPause) return false;
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
      m_duration.Set(field.Pcr,field.Pcr,field.Pcr);
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
  if (m_bScanning) return;

  FillSubtitle(header,tsPacket);
  FillAudio(header,tsPacket);
  FillVideo(header,tsPacket);
  
}
void CDeMultiplexer::FillAudio(CTsHeader& header, byte* tsPacket)
{
  if (m_iAudioStream<0 || m_iAudioStream>=m_audioStreams.size()) return;
  m_audioPid= m_audioStreams[m_iAudioStream].pid;
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
          m_pCurrentAudioBuffer->SetPcr(m_streamPcr,m_duration.StartPcr(),m_duration.MaxPcr());
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
						m_pCurrentVideoBuffer->SetPcr(m_streamPcr,m_duration.StartPcr(),m_duration.MaxPcr());
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
          m_pCurrentSubtitleBuffer->SetPcr(m_streamPcr,m_duration.StartPcr(),m_duration.MaxPcr());
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
  if (  m_pids.AudioPid1==pids.AudioPid1 && m_pids.AudioServiceType1==pids.AudioServiceType1 &&
				m_pids.AudioPid2==pids.AudioPid2 && m_pids.AudioServiceType2==pids.AudioServiceType2 &&
				m_pids.AudioPid3==pids.AudioPid3 && m_pids.AudioServiceType3==pids.AudioServiceType3 &&
				m_pids.AudioPid4==pids.AudioPid4 && m_pids.AudioServiceType4==pids.AudioServiceType4 &&
				m_pids.AudioPid5==pids.AudioPid5 && m_pids.AudioServiceType5==pids.AudioServiceType5 &&
				m_pids.AudioPid6==pids.AudioPid6 && m_pids.AudioServiceType6==pids.AudioServiceType6 &&
				m_pids.AudioPid7==pids.AudioPid7 && m_pids.AudioServiceType7==pids.AudioServiceType7 &&
				m_pids.AudioPid8==pids.AudioPid8 && m_pids.AudioServiceType8==pids.AudioServiceType8 &&
				m_pids.PcrPid==pids.PcrPid &&
				m_pids.PmtPid==pids.PmtPid &&
				m_pids.SubtitlePid==pids.SubtitlePid)
	{
		if ( pids.videoServiceType==m_pids.videoServiceType && m_pids.VideoPid==pids.VideoPid) return;
	}

  int oldVideoServiceType=m_pids.videoServiceType ;
  int oldAudioStreamType=0;
  if (m_iAudioStream>=0 && m_iAudioStream < m_audioStreams.size())
  {
    oldAudioStreamType=m_audioStreams[m_iAudioStream].audioType;
  }
  m_pids=pids;
  LogDebug("New channel found");
  LogDebug(" video    pid:%x type:%x",m_pids.VideoPid,pids.videoServiceType);
  LogDebug(" audio1   pid:%x type:%x ",m_pids.AudioPid1,m_pids.AudioServiceType1);
  LogDebug(" audio2   pid:%x type:%x ",m_pids.AudioPid2,m_pids.AudioServiceType2);
  LogDebug(" audio3   pid:%x type:%x ",m_pids.AudioPid3,m_pids.AudioServiceType3);
  LogDebug(" audio4   pid:%x type:%x ",m_pids.AudioPid4,m_pids.AudioServiceType4);
  LogDebug(" audio5   pid:%x type:%x ",m_pids.AudioPid5,m_pids.AudioServiceType5);
  LogDebug(" audio6   pid:%x type:%x ",m_pids.AudioPid6,m_pids.AudioServiceType6);
  LogDebug(" audio7   pid:%x type:%x ",m_pids.AudioPid7,m_pids.AudioServiceType7);
  LogDebug(" audio8   pid:%x type:%x ",m_pids.AudioPid8,m_pids.AudioServiceType8);
  LogDebug(" Pcr      pid:%x ",m_pids.PcrPid);
  LogDebug(" Pmt      pid:%x ",m_pids.PmtPid);
  LogDebug(" Subtitle pid:%x ",m_pids.SubtitlePid);

  if (m_pids.PcrPid>0x1)
  {
    m_duration.SetVideoPid(m_pids.PcrPid);
  }
  else if (m_pids.VideoPid>0x1)
  {
    m_duration.SetVideoPid(m_pids.VideoPid);
  }
  m_audioStreams.clear();
  
  if (m_pids.AudioPid1!=0) 
  {
    struct stAudioStream audio;
    audio.pid=m_pids.AudioPid1;
    audio.language[0]=m_pids.Lang1_1;
    audio.language[1]=m_pids.Lang1_2;
    audio.language[2]=m_pids.Lang1_3;
    audio.language[3]=0;
    audio.audioType=m_pids.AudioServiceType1;
    m_audioStreams.push_back(audio);
  }
  if (m_pids.AudioPid2!=0) 
  {
    struct stAudioStream audio;
    audio.pid=m_pids.AudioPid2;
    audio.language[0]=m_pids.Lang2_1;
    audio.language[1]=m_pids.Lang2_2;
    audio.language[2]=m_pids.Lang2_3;
    audio.language[3]=0;
    audio.audioType=m_pids.AudioServiceType2;
    m_audioStreams.push_back(audio);
  }
  if (m_pids.AudioPid3!=0) 
  {
    struct stAudioStream audio;
    audio.pid=m_pids.AudioPid3;
    audio.language[0]=m_pids.Lang3_1;
    audio.language[1]=m_pids.Lang3_2;
    audio.language[2]=m_pids.Lang3_3;
    audio.language[3]=0;
    audio.audioType=m_pids.AudioServiceType3;
    m_audioStreams.push_back(audio);
  }
  if (m_pids.AudioPid4!=0) 
  {
    struct stAudioStream audio;
    audio.pid=m_pids.AudioPid4;
    audio.language[0]=m_pids.Lang4_1;
    audio.language[1]=m_pids.Lang4_2;
    audio.language[2]=m_pids.Lang4_3;
    audio.language[3]=0;
    audio.audioType=m_pids.AudioServiceType4;
    m_audioStreams.push_back(audio);
  }
  if (m_pids.AudioPid5!=0) 
  {
    struct stAudioStream audio;
    audio.pid=m_pids.AudioPid5;
    audio.language[0]=m_pids.Lang5_1;
    audio.language[1]=m_pids.Lang5_2;
    audio.language[2]=m_pids.Lang5_3;
    audio.language[3]=0;
    audio.audioType=m_pids.AudioServiceType5;
    m_audioStreams.push_back(audio);
  }
  if (m_pids.AudioPid6!=0) 
  {
    struct stAudioStream audio;
    audio.pid=m_pids.AudioPid6;
    audio.language[0]=m_pids.Lang6_1;
    audio.language[1]=m_pids.Lang6_2;
    audio.language[2]=m_pids.Lang6_3;
    audio.language[3]=0;
    audio.audioType=m_pids.AudioServiceType6;
    m_audioStreams.push_back(audio);
  }
  if (m_pids.AudioPid7!=0) 
  {
    struct stAudioStream audio;
    audio.pid=m_pids.AudioPid7;
    audio.language[0]=m_pids.Lang7_1;
    audio.language[1]=m_pids.Lang7_2;
    audio.language[2]=m_pids.Lang7_3;
    audio.language[3]=0;
    audio.audioType=m_pids.AudioServiceType7;
    m_audioStreams.push_back(audio);
  }
  if (m_pids.AudioPid8!=0) 
  {
    struct stAudioStream audio;
    audio.pid=m_pids.AudioPid8;
    audio.language[0]=m_pids.Lang8_1;
    audio.language[1]=m_pids.Lang8_1;
    audio.language[2]=m_pids.Lang8_1;
    audio.language[3]=0;
    audio.audioType=m_pids.AudioServiceType8;
    m_audioStreams.push_back(audio);
  }

  HRESULT isPlaying=IsPlaying();
  if (oldVideoServiceType != m_pids.videoServiceType )
  {
    if (m_filter.GetVideoPin()->IsConnected())
    {
      // change video pin media type
      if (DoStop() ==S_OK){while(IsStopped() == S_FALSE){Sleep(100); break;}}
      RenderFilterPin(m_filter.GetVideoPin());
    }
  }
  
  int newAudioStreamType=0;
  if (m_iAudioStream>=0 && m_iAudioStream < m_audioStreams.size())
  {
    newAudioStreamType=m_audioStreams[m_iAudioStream].audioType;
  }

  if (oldAudioStreamType != newAudioStreamType )
  {
    if (m_filter.GetAudioPin()->IsConnected())
    {
      // change audio pin media type
      if (DoStop()==S_OK ){while(IsStopped() == S_FALSE){Sleep(100); break;}}
      RenderFilterPin(m_filter.GetAudioPin());
    }
  }
  if (isPlaying==S_OK)
  {
    DoStart();
  }
}
HRESULT CDeMultiplexer::DoStop()
{
	HRESULT hr = S_OK;

	FILTER_INFO Info;
	if (SUCCEEDED(m_filter.QueryFilterInfo(&Info)) && Info.pGraph != NULL)
	{
		// Get IMediaFilter interface
		IMediaFilter* pMediaFilter = NULL;
		hr = Info.pGraph->QueryInterface(IID_IMediaFilter, (void**)&pMediaFilter);
		if (!pMediaFilter)
		{
			Info.pGraph->Release();
			return m_filter.Stop();
		}
		else
			pMediaFilter->Release();

		IMediaControl *pMediaControl;
		if (SUCCEEDED(Info.pGraph->QueryInterface(IID_IMediaControl, (void **) &pMediaControl)))
		{
			hr = pMediaControl->Stop(); 
			pMediaControl->Release();
		}

		Info.pGraph->Release();

		if (FAILED(hr))
			return S_OK;
	}
	return S_OK;
}
HRESULT CDeMultiplexer::DoStart()
{
	HRESULT hr = S_OK;

	FILTER_INFO Info;
	if (SUCCEEDED(m_filter.QueryFilterInfo(&Info)) && Info.pGraph != NULL)
	{
		// Get IMediaFilter interface
		IMediaFilter* pMediaFilter = NULL;
		hr = Info.pGraph->QueryInterface(IID_IMediaFilter, (void**)&pMediaFilter);
		if (!pMediaFilter)
		{
			Info.pGraph->Release();
			return m_filter.Run(NULL);
		}
		else
			pMediaFilter->Release();

		IMediaControl *pMediaControl;
		if (SUCCEEDED(Info.pGraph->QueryInterface(IID_IMediaControl, (void **) &pMediaControl)))
		{
			hr = pMediaControl->Run();
			pMediaControl->Release();
		}

		Info.pGraph->Release();

		if (FAILED(hr))
			return S_OK;
	}
	return S_OK;
}

HRESULT CDeMultiplexer::IsStopped()
{
	HRESULT hr = S_FALSE;

	FILTER_STATE state = State_Stopped;

	FILTER_INFO Info;
	if (SUCCEEDED(m_filter.QueryFilterInfo(&Info)) && Info.pGraph != NULL)
	{
		// Get IMediaFilter interface
		IMediaFilter* pMediaFilter = NULL;
		hr = Info.pGraph->QueryInterface(IID_IMediaFilter, (void**)&pMediaFilter);
		if (!pMediaFilter)
		{
			Info.pGraph->Release();
			hr = m_filter.GetState(200, &state);
			if (state == State_Stopped)
			{
				if (hr == S_OK || hr == VFW_S_STATE_INTERMEDIATE || VFW_S_CANT_CUE)
					return S_OK;
			}
		}
		else
			pMediaFilter->Release();

		IMediaControl *pMediaControl;
		if (SUCCEEDED(Info.pGraph->QueryInterface(IID_IMediaControl, (void **) &pMediaControl)))
		{
			hr = pMediaControl->GetState(200, (OAFilterState*)&state);
			pMediaControl->Release();
		}

		Info.pGraph->Release();

		if (state == State_Stopped)
		{
			if (hr == S_OK || hr == VFW_S_STATE_INTERMEDIATE || VFW_S_CANT_CUE)
				return S_OK;
		}
	} 
	return S_FALSE;
}


HRESULT CDeMultiplexer::IsPlaying()
{

	HRESULT hr = S_FALSE;

	FILTER_STATE state = State_Stopped;

	FILTER_INFO Info;
	if (SUCCEEDED(m_filter.QueryFilterInfo(&Info)) && Info.pGraph != NULL)
	{
		// Get IMediaFilter interface
		IMediaFilter* pMediaFilter = NULL;
		hr = Info.pGraph->QueryInterface(IID_IMediaFilter, (void**)&pMediaFilter);
		if (!pMediaFilter)
		{
			Info.pGraph->Release();
			hr =  m_filter.GetState(200, &state);
			if (state == State_Running)
			{
				if (hr == S_OK || hr == VFW_S_STATE_INTERMEDIATE || VFW_S_CANT_CUE)
					return S_OK;
			}

			return S_FALSE;
		}
		else
			pMediaFilter->Release();

		IMediaControl *pMediaControl = NULL;
		if (SUCCEEDED(Info.pGraph->QueryInterface(IID_IMediaControl, (void **) &pMediaControl)))
		{
			hr = pMediaControl->GetState(200, (OAFilterState*)&state);
			pMediaControl->Release();
		}

		Info.pGraph->Release();
	
		if (state == State_Running)
		{
			if (hr == S_OK || hr == VFW_S_STATE_INTERMEDIATE || VFW_S_CANT_CUE)
				return S_OK;
		}
	}
	return S_FALSE;
}

HRESULT CDeMultiplexer::RenderFilterPin(CBasePin* pin)
{
  if ( pin->IsConnected())
  {
	  HRESULT hr = E_FAIL;
    pin->Disconnect();
    IFilterGraph* graph=m_filter.GetFilterGraph();
	  IGraphBuilder *pGraphBuilder;
	  if(SUCCEEDED(graph->QueryInterface(IID_IGraphBuilder, (void **) &pGraphBuilder)))
	  {
		  hr = pGraphBuilder->Render(pin);
		  pGraphBuilder->Release();
	  }

	  graph->Release();
	  return hr;
  }
  return S_OK;
}
bool CDeMultiplexer::IsPaused()
{
	return m_bPause;
}
	
void CDeMultiplexer::SetPause(bool onOff)
{
	m_bPause=onOff;
}
/*
 *  Copyright (C) 2005 Team MediaPortal
 *  http://www.team-mediaportal.com
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
#include "..\..\shared\adaptionfield.h"
#include "tsreader.h"
#include "audioPin.h"
#include "videoPin.h"
#include "subtitlePin.h"
#include "..\..\DVBSubtitle2\Source\IDVBSub.h"
#include "MediaFormats.h"
#include <cassert>

#define MAX_BUF_SIZE 8000
#define OUTPUT_PACKET_LENGTH 0x6000e
#define BUFFER_LENGTH        0x1000
#define FALLBACK_PACKETS_SD   300
#define FALLBACK_PACKETS_HD   1500
extern void LogDebug(const char *fmt, ...) ;

#define READ_SIZE (1316*30)

// *** UNCOMMENT THE NEXT LINE TO ENABLE DYNAMIC VIDEO PIN HANDLING!!!! ******
#define USE_DYNAMIC_PINS


extern int ShowBuffer ;


CDeMultiplexer::CDeMultiplexer(CTsDuration& duration,CTsReaderFilter& filter)
:m_duration(duration)
,m_filter(filter)
{
  m_patParser.SetCallBack(this);
  m_pCurrentVideoBuffer = new CBuffer();
  m_pCurrentAudioBuffer = new CBuffer();
  m_pCurrentSubtitleBuffer = new CBuffer();
  m_iAudioStream=0;
  m_iSubtitleStream=0;
  m_audioPid=0;
  m_currentSubtitlePid=0;
  m_bEndOfFile=false;
  m_bHoldAudio=false;
  m_bHoldVideo=false;
  m_bHoldSubtitle=false;
  m_iAudioIdx=-1;
  m_iPatVersion=-1;
  m_ReqPatVersion=-1;
  m_receivedPackets=0;
  m_bSetAudioDiscontinuity=false;
  m_bSetVideoDiscontinuity=false;
  m_reader=NULL;
  pTeletextEventCallback = NULL;
  pSubUpdateCallback = NULL;
  pTeletextPacketCallback = NULL;
  pTeletextServiceInfoCallback = NULL;
  m_iAudioReadCount = 0;
  //ReadAudioIndexFromRegistry();
  m_lastVideoPTS.IsValid=false;
  m_lastAudioPTS.IsValid=false;
  ResetMpeg2VideoInfo();
  m_mpegParserTriggerFormatChange=false;
  SetVideoChanging(false);
  m_DisableDiscontinuitiesFiltering = false ;

  m_AudioPrevCC = -1 ;
  m_FirstAudioSample=0x7FFFFFFF00000000LL ;
  m_LastAudioSample=0 ;

  m_VideoPrevCC = -1 ;
  m_bIframeFound=false;
  m_FirstVideoSample=0x7FFFFFFF00000000LL ;
  m_IframeSample=0x7FFFFFFF00000000LL ;
  m_LastVideoSample=0 ;
  m_LastDataFromRtsp=GetTickCount() ;
}

CDeMultiplexer::~CDeMultiplexer()
{
  Flush();
  delete m_pCurrentVideoBuffer;
  delete m_pCurrentAudioBuffer;
  delete m_pCurrentSubtitleBuffer;

  m_subtitleStreams.clear();
  m_audioStreams.clear();
}

void CDeMultiplexer::ResetMpeg2VideoInfo()
{
  memset(&m_mpeg2VideoInfo,0,sizeof(m_mpeg2VideoInfo)) ;
  m_mpeg2VideoInfo.hdr.dwReserved2=1;
}

int CDeMultiplexer::GetVideoServiceType()
{
  return m_pids.videoServiceType;
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
  if (m_mpeg2VideoInfo.hdr.dwReserved2==0)
    pmt->SetFormat((BYTE*)&m_mpeg2VideoInfo,sizeof(m_mpeg2VideoInfo));
  else
      pmt->SetFormat(g_Mpeg2ProgramVideo,sizeof(g_Mpeg2ProgramVideo));
}

void CDeMultiplexer::GetH264Media(CMediaType *pmt)
{
  pmt->InitMediaType();
  pmt->SetType      (& MEDIATYPE_Video);
  pmt->SetSubtype   (& H264_SubType);
  pmt->SetFormatType(&FORMAT_MPEG2Video);
  pmt->SetSampleSize(1);
  pmt->SetTemporalCompression(TRUE);
  pmt->SetVariableSize();
  if (m_mpeg2VideoInfo.hdr.dwReserved2==0)
    //causes framesize errors
  /*{
    VIDEOINFO vinfo;
    vinfo.rcSource.left=m_mpeg2VideoInfo.hdr.rcSource.left;
    vinfo.rcSource.bottom=m_mpeg2VideoInfo.hdr.rcSource.bottom;
    vinfo.dwBitRate=m_mpeg2VideoInfo.hdr.dwBitRate;
    pmt->SetFormat((BYTE*)&vinfo,sizeof(vinfo));
  }
  else
    pmt->SetFormat(H264VideoFormat,sizeof(H264VideoFormat));*/
    pmt->SetFormat((BYTE*)&m_mpeg2VideoInfo,sizeof(m_mpeg2VideoInfo));
  else
      pmt->SetFormat(g_Mpeg2ProgramVideo,sizeof(g_Mpeg2ProgramVideo));
}

void CDeMultiplexer::GetMpeg4Media(CMediaType *pmt)
{
  //this is actually H264 and NOT Mpeg4
  pmt->InitMediaType();
  pmt->SetType      (& MEDIATYPE_Video);
  pmt->SetSubtype   (&H264_SubType);
  pmt->SetFormatType(&FORMAT_MPEG2Video);
  pmt->SetSampleSize(1);
  pmt->SetTemporalCompression(TRUE);
  pmt->SetVariableSize();
  if (m_mpeg2VideoInfo.hdr.dwReserved2==0)
    pmt->SetFormat((BYTE*)&m_mpeg2VideoInfo,sizeof(m_mpeg2VideoInfo));
  else
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


/// This methods selects the audio stream specified
/// and updates the audio output pin media type if needed
bool CDeMultiplexer::SetAudioStream(int stream)
{
  //is stream index valid?
  if (stream< 0 || stream>=m_audioStreams.size())
    return S_FALSE;

  //get the current audio forma stream type
  int oldAudioStreamType=SERVICE_TYPE_AUDIO_MPEG2;
  if (m_iAudioStream>=0 && m_iAudioStream < m_audioStreams.size())
  {
    oldAudioStreamType=m_audioStreams[m_iAudioStream].audioType;
  }

  //set index
  m_iAudioStream=stream;

  //get the new audio stream type
  int newAudioStreamType=SERVICE_TYPE_AUDIO_MPEG2;
  if (m_iAudioStream>=0 && m_iAudioStream < m_audioStreams.size())
  {
    newAudioStreamType=m_audioStreams[m_iAudioStream].audioType;
  }

  //did it change?
  if (oldAudioStreamType != newAudioStreamType )
  {
    //yes, is the audio pin connected?
    if (m_filter.GetAudioPin()->IsConnected())
    {
      m_filter.OnMediaTypeChanged(1);
			SetVideoChanging(true) ;
    }
  }
  else
  {
    m_filter.GetAudioPin()->SetDiscontinuity(true);
  }

  return S_OK;
}

bool CDeMultiplexer::GetAudioStream(__int32 &audioIndex)
{
  audioIndex = m_iAudioStream;
  return S_OK;
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
  if (m_iAudioStream< 0 || stream >=m_audioStreams.size())
  {
    pmt.InitMediaType();
    pmt.SetType      (& MEDIATYPE_Audio);
    pmt.SetSubtype   (& MEDIASUBTYPE_MPEG2_AUDIO);
    pmt.SetSampleSize(1);
    pmt.SetTemporalCompression(FALSE);
    pmt.SetVariableSize();
    pmt.SetFormatType(&FORMAT_WaveFormatEx);
    pmt.SetFormat(MPEG2AudioFormat,sizeof(MPEG2AudioFormat));
    return;
  }

  switch (m_audioStreams[stream].audioType)
  {
    // MPEG1 shouldn't be mapped to MPEG2 audio as it will break Cyberlink audio codec
    // (and MPA is not working with the MPEG1 to MPEG2 mapping...)
    case SERVICE_TYPE_AUDIO_MPEG1:
      pmt.InitMediaType();
      pmt.SetType      (& MEDIATYPE_Audio);
      pmt.SetSubtype   (& MEDIASUBTYPE_MPEG1Payload);
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
      pmt.SetFormat(MPEG2AudioFormat,sizeof(MPEG2AudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_AAC:
      pmt.InitMediaType();
      pmt.SetType      (& MEDIATYPE_Audio);
      pmt.SetSubtype   (& MEDIASUBTYPE_AAC);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(AACAudioFormat,sizeof(AACAudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_LATM_AAC:
      pmt.InitMediaType();
      pmt.SetType      (& MEDIATYPE_Audio);
      pmt.SetSubtype   (& MEDIASUBTYPE_LATM_AAC);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(AACAudioFormat,sizeof(AACAudioFormat));
      break;
    case SERVICE_TYPE_AUDIO_AC3:
      pmt.InitMediaType();
      pmt.SetType      (& MEDIATYPE_Audio);
      pmt.SetSubtype   (& MEDIASUBTYPE_DOLBY_AC3);
      pmt.SetSampleSize(1);
      pmt.SetTemporalCompression(FALSE);
      pmt.SetVariableSize();
      pmt.SetFormatType(&FORMAT_WaveFormatEx);
      pmt.SetFormat(AC3AudioFormat,sizeof(AC3AudioFormat));
      break;
  }
}
// This methods selects the subtitle stream specified
bool CDeMultiplexer::SetSubtitleStream(__int32 stream)
{
  //is stream index valid?
  if (stream< 0 || stream>=m_subtitleStreams.size())
    return S_FALSE;

  //set index
  m_iSubtitleStream=stream;
  return S_OK;
}

bool CDeMultiplexer::GetCurrentSubtitleStream(__int32 &stream)
{

  stream = m_iSubtitleStream;
  return S_OK;
}

bool CDeMultiplexer::GetSubtitleStreamLanguage(__int32 stream,char* szLanguage)
{
  if (stream <0 || stream>=m_subtitleStreams.size())
  {
    szLanguage[0]=szLanguage[1]=szLanguage[2]=0;
    return S_FALSE;
  }
  szLanguage[0]=m_subtitleStreams[stream].language[0];
  szLanguage[1]=m_subtitleStreams[stream].language[1];
  szLanguage[2]=m_subtitleStreams[stream].language[2];
  szLanguage[3]=m_subtitleStreams[stream].language[3];

  return S_OK;
}
bool CDeMultiplexer::GetSubtitleStreamCount(__int32 &count)
{
  count = m_subtitleStreams.size();
  return S_OK;
}

bool CDeMultiplexer::SetSubtitleResetCallback( int (CALLBACK *cb)(int, void*, int*))
{
  //LogDebug("SetSubtitleResetCallback %X",cb);
  pSubUpdateCallback = cb;
  //(*pSubUpdateCallback)(SUBTITLESTREAM_EVENT_UPDATE,SUBTITLESTREAM_EVENTVALUE_NONE);
  return S_OK;
}

bool CDeMultiplexer::GetSubtitleStreamType(__int32 stream, __int32 &type)
{
  if (m_iSubtitleStream< 0 || m_iSubtitleStream >=m_subtitleStreams.size())
  {
    // invalid stream number
    return S_FALSE;
  }

  type = m_subtitleStreams[m_iSubtitleStream].subtitleType;
  return S_OK;
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

void CDeMultiplexer::FlushVideo()
{
  LogDebug("demux:flush video");
  CAutoLock lock (&m_sectionVideo);
  delete m_pCurrentVideoBuffer;
  ivecBuffers it =m_vecVideoBuffers.begin();
  while (it != m_vecVideoBuffers.end())
  {
    CBuffer* videoBuffer=*it;
    delete videoBuffer;
    it=m_vecVideoBuffers.erase(it);
    /*m_outVideoBuffer++;*/
  }
  // Clear PES temporary queue.
  it =m_t_vecVideoBuffers.begin();
  while (it != m_t_vecVideoBuffers.end())
  {
    CBuffer* VideoBuffer=*it;
    delete VideoBuffer;
    it=m_t_vecVideoBuffers.erase(it);
  }

  /*if(this->pTeletextEventCallback != NULL)
  {
    this->CallTeletextEventCallback(TELETEXT_EVENT_BUFFER_OUT_UPDATE,m_outVideoBuffer);
  }*/
  m_VideoPrevCC = -1 ;
  m_bIframeFound=false;
  m_FirstVideoSample=0x7FFFFFFF00000000LL ;
  m_IframeSample=0x7FFFFFFF00000000LL ;
  m_LastVideoSample=0 ;
  m_lastVideoPTS.IsValid=false;
  m_VideoValidPES = false ;
  m_pCurrentVideoBuffer = new CBuffer();

  Reset() ;  // PacketSync reset.
}

void CDeMultiplexer::FlushAudio()
{
  LogDebug("demux:flush audio");
  CAutoLock lock (&m_sectionAudio);
  delete m_pCurrentAudioBuffer;
  ivecBuffers it =m_vecAudioBuffers.begin();
  while (it != m_vecAudioBuffers.end())
  {
    CBuffer* AudioBuffer=*it;
    delete AudioBuffer;
    it=m_vecAudioBuffers.erase(it);
  }
  // Clear PES temporary queue.
  it =m_t_vecAudioBuffers.begin();
  while (it != m_t_vecAudioBuffers.end())
  {
    CBuffer* AudioBuffer=*it;
    delete AudioBuffer;
    it=m_t_vecAudioBuffers.erase(it);
  }

  m_AudioPrevCC = -1 ;
  m_FirstAudioSample=0x7FFFFFFF00000000LL ;
  m_LastAudioSample=0 ;
  m_lastAudioPTS.IsValid=false;
  m_AudioValidPES = false ;
  m_pCurrentAudioBuffer = new CBuffer();

  Reset() ;  // PacketSync reset.
}

void CDeMultiplexer::FlushSubtitle()
{
  LogDebug("demux:flush subtitle");
  CAutoLock lock (&m_sectionSubtitle);
  delete m_pCurrentSubtitleBuffer;
  ivecBuffers it = m_vecSubtitleBuffers.begin();
  while (it != m_vecSubtitleBuffers.end())
  {
    CBuffer* subtitleBuffer = *it;
    delete subtitleBuffer;
    it = m_vecSubtitleBuffers.erase(it);
  }
  m_pCurrentSubtitleBuffer = new CBuffer();
}

/// Flushes all buffers
void CDeMultiplexer::Flush()
{
  LogDebug("demux:flushing");

  m_iAudioReadCount = 0;
  m_LastDataFromRtsp = GetTickCount() ;
  bool holdAudio=HoldAudio();
  bool holdVideo=HoldVideo();
  bool holdSubtitle=HoldSubtitle();
  SetHoldAudio(true);
  SetHoldVideo(true);
  SetHoldSubtitle(true);
  FlushAudio();
  FlushVideo();
  FlushSubtitle();
  SetHoldAudio(holdAudio);
  SetHoldVideo(holdVideo);
  SetHoldSubtitle(holdSubtitle);
}

///
///Returns the next subtitle packet
// or NULL if there is none available
CBuffer* CDeMultiplexer::GetSubtitle()
{
  //if there is no subtitle pid, then simply return NULL
  if (m_currentSubtitlePid==0) return NULL;
  if (m_bEndOfFile) return NULL;
  if (m_bHoldSubtitle) return NULL;

  //are there subtitle packets in the buffer?
  if (m_vecSubtitleBuffers.size()!=0 )
  {
    //yup, then return the next one
    CAutoLock lock (&m_sectionSubtitle);
    ivecBuffers it =m_vecSubtitleBuffers.begin();
    CBuffer* subtitleBuffer=*it;
    m_vecSubtitleBuffers.erase(it);
    return subtitleBuffer;
  }
  //no subtitle packets available
  return NULL;
}

///
///Returns the next video packet
// or NULL if there is none available
CBuffer* CDeMultiplexer::GetVideo()
{
  //if there is no video pid, then simply return NULL
  if ((m_pids.VideoPid==0) || IsVideoChanging())
  {
    ReadFromFile(false,true);
    return NULL;
  }

  // when there are no video packets at the moment
  // then try to read some from the current file
  while ((m_vecVideoBuffers.size()==0) || (m_FirstVideoSample.m_time >= m_LastAudioSample.m_time))
  {
    //if filter is stopped or
    //end of file has been reached or
    //demuxer should stop getting video packets
    //then return NULL
    if (!m_filter.IsFilterRunning()) return NULL;
		if (m_filter.m_bStopping) return NULL;
    if (m_bEndOfFile) return NULL;
    if (m_bHoldVideo) return NULL;

    //else try to read some packets from the file
    if (ReadFromFile(false,true)<READ_SIZE) break ;
  }

  //are there video packets in the buffer?
  if (m_vecVideoBuffers.size()!=0 && !IsVideoChanging()) // && (m_FirstVideoSample.m_time < m_LastAudioSample.m_time))
  {
    CAutoLock lock (&m_sectionVideo);
    //yup, then return the next one
    ivecBuffers it =m_vecVideoBuffers.begin();
    if (it!=m_vecVideoBuffers.end())
    {
      CBuffer* videoBuffer=*it;
      m_vecVideoBuffers.erase(it);
    //m_outVideoBuffer++;
    //if(this->pTeletextEventCallback != NULL){
     // this->CallTeletextEventCallback(TELETEXT_EVENT_BUFFER_OUT_UPDATE,m_outVideoBuffer);
    //}
      return videoBuffer;
    }
  }

  //no video packets available
  return NULL;
}

///
///Returns the next audio packet
// or NULL if there is none available
CBuffer* CDeMultiplexer::GetAudio()
{
   if (m_iAudioStream == -1) return NULL;

  //if there is no audio pid, then simply return NULL
   if ((m_audioPid==0) || IsVideoChanging())
  {
    ReadFromFile(true,false);
    return NULL;
  }

  // when there are no audio packets at the moment
  // then try to read some from the current file
  while ((m_vecAudioBuffers.size()==0) ||
         ( m_filter.GetVideoPin()->IsConnected() &&
          (!m_bIframeFound || (m_IframeSample.Millisecs() + 150 >= m_LastAudioSample.Millisecs()))))
  {
  /*if (m_audioStreams.size() > 1)
  {
    if( m_iAudioReadCount > 10 && m_iAudioStream == 0) // no audio packets avail. lets try the next audio stream if any available
    {
    int previousAudioStream = m_iAudioStream;
    int newAudioStream = m_iAudioStream;
    if (m_audioStreams.size() > newAudioStream+1)
    {
      newAudioStream++; //try next avail stream
    }
    else
    {
      newAudioStream = 0; // try the first stream
    }

    if (previousAudioStream != newAudioStream)
    {
      LogDebug("demux:no audio found with stream index = %i, setting audio index = %i", previousAudioStream, newAudioStream);

      SetAudioStream( newAudioStream );
      m_iAudioReadCount = 0;
    }
    }
    else
    {
      m_iAudioReadCount++;
    }
  }*/

    //if filter is stopped or
    //end of file has been reached or
    //demuxer should stop getting audio packets
    //then return NULL
    if (!m_filter.IsFilterRunning()) return NULL;
		if (m_filter.m_bStopping) return NULL;

    if (m_bEndOfFile) return NULL;
    if (m_bHoldAudio) return NULL;

    if (ReadFromFile(true,false)<READ_SIZE) break ;
  }

  //are there audio packets in the buffer?
  if (m_vecAudioBuffers.size()==0) return NULL ;
	if (IsVideoChanging()) return NULL ;
  if ((!m_filter.GetVideoPin()->IsConnected()) ||
      ( m_filter.m_bLiveTv && m_bIframeFound && (m_FirstAudioSample.m_time + 1500000 < m_LastAudioSample.m_time)) ||
      (!m_filter.m_bLiveTv &&                  (m_IframeSample.m_time + 1500000 < m_LastAudioSample.m_time)))
  {
    //yup, then return the next one
    CAutoLock lock (&m_sectionAudio);

    ivecBuffers it =m_vecAudioBuffers.begin();
    CBuffer* audiobuffer=*it;
    m_vecAudioBuffers.erase(it);

    return audiobuffer;
  }
  //no audio packets available
  return NULL;
}


/// Starts the demuxer
/// This method will read the file until we found the pat/sdt
/// with all the audio/video pids
void CDeMultiplexer::Start()
{
  //reset some values
  m_receivedPackets=0;
  ResetMpeg2VideoInfo();
  m_mpegParserTriggerFormatChange=false;
  m_bEndOfFile=false;
  m_bHoldAudio=false;
  m_bHoldVideo=false;
  m_iPatVersion=-1;
  m_ReqPatVersion=-1;
  m_bSetAudioDiscontinuity=false;
  m_bSetVideoDiscontinuity=false;
  DWORD dwBytesProcessed=0;
  DWORD m_Time = GetTickCount() ;
  while ((GetTickCount() - m_Time) < 5000)
  {
    int BytesRead =ReadFromFile(false,false) ;
		if (BytesRead==0) Sleep(10) ;
    if (dwBytesProcessed>5000000 || GetAudioStreamCount()>0)
    {
      // dynamic pins are currently disabled
      #ifdef USE_DYNAMIC_PINS
      if ((m_mpeg2VideoInfo.hdr.dwReserved2!=0 && m_pids.VideoPid>1) && dwBytesProcessed<5000000)
      {
        dwBytesProcessed+=BytesRead;
        continue;
      }
      #endif
      m_reader->SetFilePointer(0,FILE_BEGIN);
      Flush();
      m_streamPcr.Reset();
      return;
    }
    dwBytesProcessed+=BytesRead;
  }
  m_streamPcr.Reset();
  m_iAudioReadCount=0;
}

void CDeMultiplexer::SetEndOfFile(bool bEndOfFile)
{
  m_bEndOfFile=bEndOfFile;
}
/// Returns true if we reached the end of the file
bool CDeMultiplexer::EndOfFile()
{
  return m_bEndOfFile;
}

/// This method reads the next READ_SIZE bytes from the file
/// and processes the raw data
/// When a TS packet has been discovered, OnTsPacket(byte* tsPacket) gets called
//  which in its turn deals with the packet
int CDeMultiplexer::ReadFromFile(bool isAudio, bool isVideo)
{
  CAutoLock lock (&m_sectionRead);
  if (m_reader==NULL) return false;
  byte buffer[READ_SIZE];
  int dwReadBytes=0;
  bool result=false;
  //if we are playing a RTSP stream
  if (m_reader->IsBuffer())
  {
    // and, the current buffer holds data
    int nBytesToRead = m_reader->HasData() ;
    if (nBytesToRead > sizeof(buffer)) nBytesToRead=sizeof(buffer);
    if (nBytesToRead)
    {
      //then read raw data from the buffer
      m_reader->Read(buffer, nBytesToRead, (DWORD*)&dwReadBytes);
      if (dwReadBytes > 0)
      {
        //yes, then process the raw data
        result=true;
        OnRawData(buffer,(int)dwReadBytes);
        m_LastDataFromRtsp = GetTickCount();
      }
    }
    else
    {
      if (!m_filter.IsTimeShifting())
      {
//          LogDebug("demux:endoffile...%d",GetTickCount()-m_LastDataFromRtsp );
           //set EOF flag and return
        if (GetTickCount()-m_LastDataFromRtsp > 2000)       // A bit crappy, but no better idea...
        {
          LogDebug("demux:endoffile");
          m_bEndOfFile=true;
          return 0;
        }
      }
    }
    return dwReadBytes;
  }
  else
  {
    if (m_filter.IsSeekingToEof()) return 0 ;

    //playing a local file.
    //read raw data from the file
    if (SUCCEEDED(m_reader->Read(buffer,sizeof(buffer), (DWORD*)&dwReadBytes)))
    {
      if ((m_filter.IsTimeShifting()) && (dwReadBytes < sizeof(buffer)))
      {
        extern int ShowBuffer ;
      }

      if (dwReadBytes > 0)
      {
        //succeeded, process data
        OnRawData(buffer,(int)dwReadBytes);
      }
      else
      {
        if (!m_filter.IsTimeShifting())
        {
          //set EOF flag and return
          LogDebug("demux:endoffile");
          m_bEndOfFile=true;
          return 0;
        }
      }

      //and return
      return dwReadBytes;
    }
    else
    {
      int x=123;
      LogDebug("Read failed...");
    }
  }
  //Failed to read any data
  if ( (isAudio && m_bHoldAudio) || (isVideo && m_bHoldVideo) )
  {
    //LogDebug("demux:paused %d %d",m_bHoldAudio,m_bHoldVideo);
    return 0;
  }
  return 0;
}
/// This method gets called via ReadFile() when a new TS packet has been received
/// if will :
///  - decode any new pat/pmt/sdt
///  - decode any audio/video packets and put the PES packets in the appropiate buffers
void CDeMultiplexer::OnTsPacket(byte* tsPacket)
{
  CTsHeader header(tsPacket);

  m_patParser.OnTsPacket(tsPacket);

	if (m_iPatVersion==-1) return ;								// First Pat not found

	// Wait for new PAT if required.
	if ((m_iPatVersion & 0x0F) != (m_ReqPatVersion & 0x0F))
	{
		if (m_ReqPatVersion==-1)										
		{																							// Now, unless channel change, 
			m_ReqPatVersion = m_iPatVersion ;						// Initialize Pat Request.
			m_WaitNewPatTmo = GetTickCount() ;					// Now, unless channel change request,timeout will be always true. 
		}
		if (GetTickCount() < m_WaitNewPatTmo) return ;		// Timeout not reached.
	}

  //if we have no PCR pid (yet) then there's nothing to decode, so return
  if (m_pids.PcrPid==0) return;

  if (header.Pid==0) return;
  if (header.TScrambling) return;

  //skip any packets with errors in it
  if (header.TransportError) return;

  if( m_pids.TeletextPid > 0 && m_pids.TeletextPid != m_currentTeletextPid )
  {
    IDVBSubtitle* pDVBSubtitleFilter(m_filter.GetSubtitleFilter());
    if( pTeletextServiceInfoCallback )
      {
      std::vector<TeletextServiceInfo>::iterator vit = m_pids.TeletextInfo.begin();
      while(vit != m_pids.TeletextInfo.end())
      {
        TeletextServiceInfo& info = *vit;
        LogDebug("Calling Teletext Service info callback");
        (*pTeletextServiceInfoCallback)(info.page, info.type, (byte)info.lang[0],(byte)info.lang[1],(byte)info.lang[2]);
        vit++;
      }
      m_currentTeletextPid = m_pids.TeletextPid;
    }
  }

  //Do we have a start pcr?
  //if (!m_duration.StartPcr().IsValid)
  //{
  //  //no, then decode the pcr
  //  CAdaptionField field;
  //  field.Decode(header,tsPacket);
  //  if (field.Pcr.IsValid)
  //  {
  //    //and we consider this PCR timestamp as the start of the file
  //    m_duration.Set(field.Pcr,field.Pcr,field.Pcr);
  //  }
  //}

  //is this the PCR pid ?
  if (header.Pid==m_pids.PcrPid)
  {
    //yep, does it have a PCR timestamp?
    CAdaptionField field;
    field.Decode(header,tsPacket);
    if (field.Pcr.IsValid)
    {
      //then update our stream pcr which holds the current playback timestamp
      m_streamPcr=field.Pcr;
    /*
      static float prevTime=0;
      float fTime=(float)m_streamPcr.ToClock();

      if (abs(prevTime-fTime)>=1)
      {
        LogDebug("pcr:%s", m_streamPcr.ToString());
        prevTime=fTime;
      }
    */
    }
  }

  //as long as we dont have a stream pcr timestamp we return
  if (m_streamPcr.IsValid==false)
  {
    return;
  }

  //process the ts packet further
  FillSubtitle(header,tsPacket);
  FillAudio(header,tsPacket);
  FillVideo(header,tsPacket);
  FillTeletext(header,tsPacket);
}

/// Validate TS packet discontinuity 
bool CDeMultiplexer::CheckContinuity(int prevCC, CTsHeader& header)
{
  if ((prevCC !=-1 ) && (prevCC != ((header.ContinuityCounter - 1) & 0x0F)))
  {
    return false;
  }
  return true;
}

/// This method will check if the tspacket is an audio packet
/// ifso, it decodes the PES audio packet and stores it in the audio buffers
void CDeMultiplexer::FillAudio(CTsHeader& header, byte* tsPacket)
{
  //LogDebug("FillAudio - audio PID %d", m_audioPid );

  if (m_iAudioStream<0 || m_iAudioStream>=m_audioStreams.size()) return;
  m_audioPid= m_audioStreams[m_iAudioStream].pid;
  if (m_audioPid==0 || m_audioPid != header.Pid) return;
  if (m_filter.GetAudioPin()->IsConnected()==false) return;
  if ( header.AdaptionFieldOnly() )return;

  if(!CheckContinuity(m_AudioPrevCC, header))
  {
    LogDebug("Audio Continuity error... %x ( prev %x )", header.ContinuityCounter, m_AudioPrevCC);
    m_AudioValidPES = m_DisableDiscontinuitiesFiltering;  
  }

  m_AudioPrevCC = header.ContinuityCounter;

  CAutoLock lock (&m_sectionAudio);
  //does tspacket contain the start of a pes packet?
  if (header.PayloadUnitStart)
  {
    //yes packet contains start of a pes packet.
    //does current buffer hold any data ?
    if (m_pCurrentAudioBuffer->Length() > 0)
    {
      m_t_vecAudioBuffers.push_back(m_pCurrentAudioBuffer);
      m_pCurrentAudioBuffer = new CBuffer();
    }

    if (m_t_vecAudioBuffers.size())
    {
      CBuffer *Cbuf=*m_t_vecAudioBuffers.begin() ;
      byte *p = Cbuf->Data() ;
      if ((p[0]==0) && (p[1]==0) && (p[2]==1))
      {
        //get pts/dts from pes header
        CPcr pts;
        CPcr dts;
        if (CPcr::DecodeFromPesHeader(p,0,pts,dts))
        {
          double diff;
          if (!m_lastAudioPTS.IsValid)
            m_lastAudioPTS=pts;
          if (m_lastAudioPTS>pts)
            diff=m_lastAudioPTS.ToClock()-pts.ToClock();
          else
            diff=pts.ToClock()-m_lastAudioPTS.ToClock();
          if (diff>10.0)
          {
						LogDebug("DeMultiplexer::FillAudio pts jump found : %f %f, %f", (float) diff, (float)pts.ToClock(), (float)m_lastAudioPTS.ToClock());
						m_AudioValidPES=false ;
          }
					else
	          m_lastAudioPTS=pts;

          Cbuf->SetPts(pts);
//          if (ShowBuffer)
//          {
//          CRefTime Ref ;
//            Cbuf->MediaTime(Ref) ;
//            LogDebug("Aud/Dmx : %03.3f", (float)Ref.Millisecs()/1000.0f);
//          }
        }
        //skip pes header
        int headerLen=9+p[8] ;
        int len = Cbuf->Length()-headerLen ;
        if (len > 0)
        {
          byte *ps = p+headerLen ;
          Cbuf->SetLength(len) ;
          while(len--) *p++ = *ps++ ;   // memcpy could be not safe.
        }
        else
        {
          LogDebug(" No data") ;
          m_AudioValidPES=false ;
        }
      }
      else
      {
        LogDebug("Pes header 0-0-1 fail") ;
        m_AudioValidPES=false ;
      }

      if (m_AudioValidPES)
      {
        if (m_bSetAudioDiscontinuity)
        {
          m_bSetAudioDiscontinuity=false;
          Cbuf->SetDiscontinuity();
        }

        Cbuf->SetPcr(m_duration.FirstStartPcr(),m_duration.MaxPcr());

        //yes, then move the full PES in main queue.
        while (m_t_vecAudioBuffers.size())
        {
          ivecBuffers it ;
          // Check if queue is no abnormally long..
          if (m_vecAudioBuffers.size()>MAX_BUF_SIZE)
          {
            ivecBuffers it = m_vecAudioBuffers.begin() ;
            delete *it ;
            m_vecAudioBuffers.erase(it);
          }
          it = m_t_vecAudioBuffers.begin() ;

          CRefTime Ref ;
          if((*it)->MediaTime(Ref))
          {
            if (Ref < m_FirstAudioSample) m_FirstAudioSample = Ref ;
            if (Ref > m_LastAudioSample) m_LastAudioSample = Ref ;
          }

          m_vecAudioBuffers.push_back(*it) ;
          m_t_vecAudioBuffers.erase(it);
        }
      }
      else
      {
        while (m_t_vecAudioBuffers.size())
        {
          ivecBuffers it ;
          it = m_t_vecAudioBuffers.begin() ;
          m_t_vecAudioBuffers.erase(it);
        }
        m_bSetAudioDiscontinuity=true ;
      }
    }
    m_AudioValidPES = true ;
  }

  if (m_AudioValidPES)
  {
    int pos=header.PayLoadStart;
    //packet contains rest of a pes packet
    //does the entire data in this tspacket fit in the current buffer ?
    if (m_pCurrentAudioBuffer->Length()+(188-pos)>=0x2000)
    {
      //no, then determine how many bytes do fit
      int copyLen=0x2000-m_pCurrentAudioBuffer->Length();
      //copy those bytes
      m_pCurrentAudioBuffer->Add(&tsPacket[pos],copyLen);
      pos+=copyLen;

      m_t_vecAudioBuffers.push_back(m_pCurrentAudioBuffer);
      //and create a new one
      m_pCurrentAudioBuffer = new CBuffer();
    }
    //copy (rest) data in current buffer
    if (pos>0 && pos < 188)
    {
      m_pCurrentAudioBuffer->Add(&tsPacket[pos],188-pos);
    }
  }
}

/// This method will check if the tspacket is an video packet
/// ifso, it decodes the PES video packet and stores it in the video buffers
void CDeMultiplexer::FillVideo(CTsHeader& header, byte* tsPacket)
{
  if (m_pids.VideoPid==0) return;
  if (header.Pid!=m_pids.VideoPid) return;

  if ( header.AdaptionFieldOnly() )return;

  if(!CheckContinuity(m_VideoPrevCC, header))
  {
    LogDebug("Video Continuity error... %x ( prev %x )", header.ContinuityCounter, m_VideoPrevCC);
    m_VideoValidPES = m_DisableDiscontinuitiesFiltering;  
  }

  m_VideoPrevCC = header.ContinuityCounter;

  CAutoLock lock (&m_sectionVideo);
  //does tspacket contain the start of a pes packet?
  if (header.PayloadUnitStart)
  {
    //yes packet contains start of a pes packet.
    //does current buffer hold any data ?
    if (m_pCurrentVideoBuffer->Length() > 0)
    {
      m_t_vecVideoBuffers.push_back(m_pCurrentVideoBuffer);
      m_pCurrentVideoBuffer = new CBuffer();
    }

    if (m_t_vecVideoBuffers.size())
    {
      CBuffer *Cbuf=*m_t_vecVideoBuffers.begin() ;
      byte *p = Cbuf->Data() ;
      if ((p[0]==0) && (p[1]==0) && (p[2]==1))
      {
        //get pts/dts from pes header
        CPcr pts;
        CPcr dts;
        if (CPcr::DecodeFromPesHeader(p,0,pts,dts))
        {
          double diff;
          if (!m_lastVideoPTS.IsValid)
            m_lastVideoPTS=pts;
          if (m_lastVideoPTS>pts)
            diff=m_lastVideoPTS.ToClock()-pts.ToClock();
          else
            diff=pts.ToClock()-m_lastVideoPTS.ToClock();
//          m_lastVideoPTS=pts;
          if (diff>10.0)
          {
            LogDebug("DeMultiplexer::FillVideo pts jump found : %f %f, %f", (float) diff, (float)pts.ToClock(), (float)m_lastVideoPTS.ToClock());
            m_VideoValidPES=false ;
          }
          else
          {
	        m_lastVideoPTS=pts;
          }

          Cbuf->SetPts(pts);

//          if (ShowBuffer)
//          {
//            CRefTime Ref ;
//            Cbuf->MediaTime(Ref) ;
//            LogDebug("Vid/Dmx : %03.3f, %d", (float)Ref.Millisecs()/1000.0f,dts.IsValid);
//          }
        }
        //skip pes header
        int headerLen=9+p[8] ;
        int len = Cbuf->Length()-headerLen ;
        if (len > 0)
        {
          byte *ps = p+headerLen ;
          Cbuf->SetLength(len) ;
          while(len--) *p++ = *ps++ ;   // memcpy could be not safe.
        }
        else
        {
          LogDebug(" No data") ;
          m_VideoValidPES=false ;
        }
      }
      else
      {
        LogDebug("Pes 0-0-1 fail") ;
        m_VideoValidPES=false ;
      }

      if (m_VideoValidPES)
      {
        Cbuf->SetPcr(m_duration.FirstStartPcr(),m_duration.MaxPcr());

        if (m_t_vecVideoBuffers.size())
        {
          ivecBuffers it ;
          // Check if queue is no abnormally long..
          if (m_vecVideoBuffers.size()>MAX_BUF_SIZE)
          {
            ivecBuffers it = m_vecVideoBuffers.begin() ;
            delete *it ;
            m_vecVideoBuffers.erase(it);
          }
          it = m_t_vecVideoBuffers.begin() ;

          // Search for GOP start
          bool Gop=false ;
          unsigned int marker = 0xffffffff;
          int offset=0 ;
          byte *p = (*it)->Data() ;
          if (m_pids.videoServiceType==SERVICE_TYPE_VIDEO_MPEG2)
          {
            for (; offset < 1000; offset++)
            {
              marker=(unsigned int)marker<<8;
              marker &= 0xffffff00;
              marker += p[offset];
              if (marker == 0x000001b3)
              {
                Gop=true ;
                break ;
              }
            }
            // Search for frame type (I, P, B )
                static char tc[]="xibpxxxxXIPBXXXX" ;
            marker = 0xffffffff;
            offset=0 ;
            p = (*it)->Data() ;
						(*it)->SetFrameType('!') ;
						(*it)->SetFrameCount(99) ;
						bool found=false;
            for (; offset < (*it)->Length()-2 ; offset++)
            {
              marker=(unsigned int)marker<<8;
              marker &= 0xffffff00;
              marker += p[offset];
              if (marker == 0x0100)
              {
//                static char tc[]="xibpxxxxXIPBXXXX" ;
//								if (!found)
					//			{
					//				found=true ;
                (*it)->SetFrameType(tc[((p[offset+2]>>3)&7)+(Gop<<3)]);
                (*it)->SetFrameCount((p[offset+2]>>6)+(p[offset+1]<<2)) ;
				//				}
		    //        LogDebug("Vid/Dmx : ------ %c %d", tc[((p[offset+2]>>3)&7)+(Gop<<3)], (p[offset+2]>>6)+(p[offset+1]<<2));

                break ;
              }
            }
//						          {
//            CRefTime Ref ;
//            (*it)->MediaTime(Ref) ;
//            LogDebug("Vid/Dmx : %03.3f %c %d", (float)Ref.Millisecs()/1000.0f, tc[((p[offset+2]>>3)&7)+(Gop<<3)], ((p[offset+2]>>3)&7)+(Gop<<3));
//          }

          }
          else
          {
            (*it)->SetFrameType('?');
            (*it)->SetFrameCount(0);
            for (; offset < 1000; offset++)
            {
              marker=(unsigned int)marker<<8;
              marker &= 0xffffff00;
              marker += p[offset];
              if ((marker == 0x00000001) && (((p[offset+1] & 0x9f)|0x0100) == 0x107))
              {
                Gop=true ;
                (*it)->SetFrameType('I');
                break ;
              }
            }
          }
//#ifdef TOO
          {
          int lastVidResX=m_mpeg2VideoInfo.hdr.rcSource.right;
          int lastVidResY=m_mpeg2VideoInfo.hdr.rcSource.bottom;
          m_mpegPesParser.OnTsPacket((*it)->Data(),(*it)->Length(),m_mpeg2VideoInfo,m_pids.videoServiceType);
          int streamType=m_mpeg2VideoInfo.hdr.dwReserved1;
          m_mpeg2VideoInfo.hdr.dwReserved1=0;
//          LogDebug("DeMultiplexer: %x video format: res=%dx%d aspectRatio=%d:%d bitrate=%d isInterlaced=%d",header.Pid,m_mpeg2VideoInfo.hdr.rcSource.right,m_mpeg2VideoInfo.hdr.rcSource.bottom,m_mpeg2VideoInfo.hdr.dwPictAspectRatioX,m_mpeg2VideoInfo.hdr.dwPictAspectRatioY,m_mpeg2VideoInfo.hdr.dwBitRate,(m_mpeg2VideoInfo.hdr.dwInterlaceFlags & AMINTERLACE_IsInterlaced==AMINTERLACE_IsInterlaced));
          if (lastVidResX!=m_mpeg2VideoInfo.hdr.rcSource.right || lastVidResY!=m_mpeg2VideoInfo.hdr.rcSource.bottom)
          {
            LogDebug("DeMultiplexer: %x video format changed: res=%dx%d aspectRatio=%d:%d bitrate=%d isInterlaced=%d",header.Pid,m_mpeg2VideoInfo.hdr.rcSource.right,m_mpeg2VideoInfo.hdr.rcSource.bottom,m_mpeg2VideoInfo.hdr.dwPictAspectRatioX,m_mpeg2VideoInfo.hdr.dwPictAspectRatioY,m_mpeg2VideoInfo.hdr.dwBitRate,(m_mpeg2VideoInfo.hdr.dwInterlaceFlags & AMINTERLACE_IsInterlaced)==AMINTERLACE_IsInterlaced);

//        {         // Dump of Ts packet that cause the format change.
//        int i= 0 ;
//        while(i<188)
//        {
//          LogDebug(" %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x %02x ",
//            tsPacket[0+i], tsPacket[1+i], tsPacket[2+i], tsPacket[3+i],
//            tsPacket[4+i], tsPacket[5+i], tsPacket[6+i], tsPacket[7+i],
//            tsPacket[8+i], tsPacket[9+i], tsPacket[10+i], tsPacket[11+i],
//            tsPacket[12+i], tsPacket[13+i], tsPacket[14+i], tsPacket[15+i]) ;
//        i+=16 ;
//        }
//        }
            if (m_mpegParserTriggerFormatChange)
            {
              LogDebug("DeMultiplexer: OnMediaFormatChange triggered by mpeg2Parser");
              SetVideoChanging(true);
              m_filter.OnMediaTypeChanged(3);
              m_mpegParserTriggerFormatChange=false;
            }
            LogDebug("DeMultiplexer: triggering OnVideoFormatChanged");
            m_filter.OnVideoFormatChanged(streamType,m_mpeg2VideoInfo.hdr.rcSource.right,m_mpeg2VideoInfo.hdr.rcSource.bottom,m_mpeg2VideoInfo.hdr.dwPictAspectRatioX,m_mpeg2VideoInfo.hdr.dwPictAspectRatioY,m_mpeg2VideoInfo.hdr.dwBitRate,(m_mpeg2VideoInfo.hdr.dwInterlaceFlags & AMINTERLACE_IsInterlaced)==AMINTERLACE_IsInterlaced);
          }
          else
          {
            if (m_mpegParserTriggerFormatChange && Gop)
//              m_receivedPackets++;
//            int packetsToFallBack=FALLBACK_PACKETS_SD;
//            if (m_pids.videoServiceType!=SERVICE_TYPE_VIDEO_MPEG2)
//              packetsToFallBack=FALLBACK_PACKETS_HD;
//            if (m_mpegParserTriggerFormatChange && m_receivedPackets>packetsToFallBack)
            {
              LogDebug("DeMultiplexer: Got GOP after the channel change was detected without correct mpeg header parsing, so we trigger the format change now.");
              m_filter.OnMediaTypeChanged(3);
              m_mpegParserTriggerFormatChange=false;
            }
          }
          }
//#endif
          //yes, then move the full PES in main queue.
          while (m_t_vecVideoBuffers.size())
          { 
            ivecBuffers it ;
            it = m_t_vecVideoBuffers.begin() ;

            if ((Gop || m_bIframeFound) && m_filter.GetVideoPin()->IsConnected())
            {
              CRefTime Ref ;
              if((*it)->MediaTime(Ref))
              {
                if (Gop && !m_bIframeFound)
                  m_IframeSample = Ref ;
                if (Ref < m_FirstVideoSample) m_FirstVideoSample = Ref ;
                if (Ref > m_LastVideoSample) m_LastVideoSample = Ref ;
              }
              m_bIframeFound=true ;
			        if (m_bSetVideoDiscontinuity)
			        {
						    m_bSetVideoDiscontinuity=false;
								(*it)->SetDiscontinuity();
							}
              m_vecVideoBuffers.push_back(*it) ;
              m_t_vecVideoBuffers.erase(it);
            }
            else
            {
              delete (*it) ;
              m_t_vecVideoBuffers.erase(it);
            }
          }
        }
      }
      else
      {
        while (m_t_vecVideoBuffers.size())
        {
          ivecBuffers it ;
          it = m_t_vecVideoBuffers.begin() ;
          delete (*it) ;
          m_t_vecVideoBuffers.erase(it);
        }
        m_bSetVideoDiscontinuity=true ;
      }
    }
    m_VideoValidPES = true ;
  }

  if (m_VideoValidPES)
  {
    int pos=header.PayLoadStart;
    //packet contains rest of a pes packet
    //does the entire data in this tspacket fit in the current buffer ?
    if (m_pCurrentVideoBuffer->Length()+(188-pos)>=0x10000)
    {
      //no, then determine how many bytes do fit
      int copyLen=0x10000-m_pCurrentVideoBuffer->Length();
      //copy those bytes
      m_pCurrentVideoBuffer->Add(&tsPacket[pos],copyLen);
      pos+=copyLen;
      m_t_vecVideoBuffers.push_back(m_pCurrentVideoBuffer);
      //and create a new one
      m_pCurrentVideoBuffer = new CBuffer();
    }

    //copy (rest) data in current buffer
    if (pos>0 && pos < 188)
    {
      m_pCurrentVideoBuffer->Add(&tsPacket[pos],188-pos);
    }
  }
}

/// This method will check if the tspacket is an subtitle packet
/// if so store it in the subtitle buffers
void CDeMultiplexer::FillSubtitle(CTsHeader& header, byte* tsPacket)
{
  if (m_filter.GetSubtitlePin()->IsConnected()==false) return;
  if (m_iSubtitleStream<0 || m_iSubtitleStream>=m_subtitleStreams.size()) return;

  // If current subtitle PID has changed notify the DVB sub filter
  if( m_subtitleStreams[m_iSubtitleStream].pid > 0 &&
    m_subtitleStreams[m_iSubtitleStream].pid != m_currentSubtitlePid )
  {
    IDVBSubtitle* pDVBSubtitleFilter(m_filter.GetSubtitleFilter());
    if( pDVBSubtitleFilter )
    {
      LogDebug("Calling SetSubtitlePid");
      pDVBSubtitleFilter->SetSubtitlePid(m_subtitleStreams[m_iSubtitleStream].pid);
      LogDebug(" done - SetSubtitlePid");
      LogDebug("Calling SetFirstPcr");
      pDVBSubtitleFilter->SetFirstPcr(m_duration.FirstStartPcr().PcrReferenceBase);
      LogDebug(" done - SetFirstPcr");
      m_currentSubtitlePid = m_subtitleStreams[m_iSubtitleStream].pid;
    }
  }

  if (m_currentSubtitlePid==0 || m_currentSubtitlePid != header.Pid) return;
  if ( header.AdaptionFieldOnly() ) return;

  CAutoLock lock (&m_sectionSubtitle);
  if ( false==header.AdaptionFieldOnly() )
  {
    if (header.PayloadUnitStart)
    {
      m_subtitlePcr = m_streamPcr;
      //LogDebug("FillSubtitle: PayloadUnitStart -- %lld", m_streamPcr.PcrReferenceBase );
    }
    if (m_vecSubtitleBuffers.size()>MAX_BUF_SIZE)
    {
      ivecBuffers it = m_vecSubtitleBuffers.begin() ;
      CBuffer* subtitleBuffer=*it;
      delete subtitleBuffer ;
      m_vecSubtitleBuffers.erase(it);
    }

    m_pCurrentSubtitleBuffer->SetPcr(m_duration.FirstStartPcr(),m_duration.MaxPcr());
    m_pCurrentSubtitleBuffer->SetPts(m_subtitlePcr);
    m_pCurrentSubtitleBuffer->Add(tsPacket,188);

    m_vecSubtitleBuffers.push_back(m_pCurrentSubtitleBuffer);

    m_pCurrentSubtitleBuffer = new CBuffer();
  }
}

void CDeMultiplexer::FillTeletext(CTsHeader& header, byte* tsPacket)
{
  if (m_pids.TeletextPid==0) return;
  if (header.Pid!=m_pids.TeletextPid) return;
  if ( header.AdaptionFieldOnly() ) return;

  if(pTeletextEventCallback != NULL)
  {
    //LogDebug("Compensation: %i",m_filter.Compensation.Millisecs());
    //(*pTeletextEventCallback)(TELETEXT_EVENT_COMPENSATION_UPDATE,m_filter.Compensation.Millisecs() * 90);
    (*pTeletextEventCallback)(TELETEXT_EVENT_PACKET_PCR_UPDATE,m_streamPcr.PcrReferenceBase - m_duration.FirstStartPcr().PcrReferenceBase - (m_filter.Compensation.Millisecs() * 90 ));
  }
  if(pTeletextPacketCallback != NULL)
  {
    (*pTeletextPacketCallback)(tsPacket,188);
  }
}

int CDeMultiplexer::GetVideoBufferPts(CRefTime & First, CRefTime & Last)
{
  First = m_FirstVideoSample ;
  Last = m_LastVideoSample ;
  return m_vecVideoBuffers.size() ;
}

int CDeMultiplexer::GetAudioBufferPts(CRefTime & First, CRefTime & Last)
{
  First = m_FirstAudioSample ;
  Last = m_LastAudioSample ;
  return m_vecAudioBuffers.size() ;
}


/*
void CDeMultiplexer::ReadAudioIndexFromRegistry()
{
  //get audio preference from registry key
  HKEY key;
  if (ERROR_SUCCESS==RegOpenKeyEx(HKEY_CURRENT_USER, "Software\\MediaPortal\\TsReader",0,KEY_READ,&key))
  {
    DWORD audioIdx=-1;
    DWORD keyType=REG_DWORD;
    DWORD dwSize=sizeof(DWORD);

  if (ERROR_SUCCESS==RegQueryValueEx(key, "audioIdx",0,&keyType,(LPBYTE)&audioIdx,&dwSize))
    {
      LogDebug ("read audioindex from registry : %i", audioIdx);
      m_iAudioIdx = audioIdx;
    }
    RegCloseKey(key);
  }
}
*/

/// This method gets called-back from the pat parser when a new PAT/PMT/SDT has been received
/// In this method we check if any audio/video/subtitle pid or format has changed
/// If not, we simply return
/// If something has changed we ask the MP to rebuild the graph
void CDeMultiplexer::OnNewChannel(CChannelInfo& info)
{
  //CAutoLock lock (&m_section);
  CPidTable pids=info.PidTable;

  if (info.PatVersion != m_iPatVersion)
  {
    LogDebug("OnNewChannel pat version:%d->%d",m_iPatVersion, info.PatVersion);
    m_iPatVersion=info.PatVersion;
    m_bSetAudioDiscontinuity=true;
    m_bSetVideoDiscontinuity=true;
    Flush();
  }
  else
  {
    //do we have at least an audio pid?
    if (pids.AudioPid1==0) return; // no? then return

    //check if something changed
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
          m_pids.SubtitlePid1==pids.SubtitlePid1 &&
          m_pids.SubtitlePid2==pids.SubtitlePid2 &&
          m_pids.SubtitlePid3==pids.SubtitlePid3 &&
          m_pids.SubtitlePid4==pids.SubtitlePid4 )
    {
      if ( pids.videoServiceType==m_pids.videoServiceType && m_pids.VideoPid==pids.VideoPid)
      {
        //nothing changed so return
        return;
      }
    }
  }

  //remember the old audio & video formats
  int oldVideoServiceType=m_pids.videoServiceType ;
  int oldAudioStreamType=SERVICE_TYPE_AUDIO_MPEG2;
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
  LogDebug(" Subtitle pid:%x ",m_pids.SubtitlePid1);
  LogDebug(" Subtitle pid:%x ",m_pids.SubtitlePid2);
  LogDebug(" Subtitle pid:%x ",m_pids.SubtitlePid3);
  LogDebug(" Subtitle pid:%x ",m_pids.SubtitlePid4);

  if(pTeletextEventCallback != NULL)
  {
    (*pTeletextEventCallback)(TELETEXT_EVENT_RESET,TELETEXT_EVENTVALUE_NONE);
  }

  IDVBSubtitle* pDVBSubtitleFilter(m_filter.GetSubtitleFilter());
  if( pDVBSubtitleFilter )
  {
    // Make sure that subtitle cache is reseted ( in filter & MP )
    pDVBSubtitleFilter->NotifyChannelChange();
  }

  //update audio streams etc..
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
/*
  if (m_iAudioStream>=m_audioStreams.size())
  {
    m_iAudioStream=0;
  }
*/
  m_subtitleStreams.clear();

  if (m_pids.SubtitlePid1!=0)
  {
    struct stSubtitleStream subtitle;
    subtitle.pid=m_pids.SubtitlePid1;
    subtitle.language[0]=m_pids.SubLang1_1;
    subtitle.language[1]=m_pids.SubLang1_2;
    subtitle.language[2]=m_pids.SubLang1_3;
    subtitle.language[3]=0;
    subtitle.subtitleType=0; // DVB subtitle
    m_subtitleStreams.push_back(subtitle);
  }

  if (m_pids.SubtitlePid2!=0)
  {
    struct stSubtitleStream subtitle;
    subtitle.pid=m_pids.SubtitlePid2;
    subtitle.language[0]=m_pids.SubLang2_1;
    subtitle.language[1]=m_pids.SubLang2_2;
    subtitle.language[2]=m_pids.SubLang2_3;
    subtitle.language[3]=0;
    subtitle.subtitleType=0; // DVB subtitle
    m_subtitleStreams.push_back(subtitle);
  }

  if (m_pids.SubtitlePid3!=0)
  {
    struct stSubtitleStream subtitle;
    subtitle.pid=m_pids.SubtitlePid3;
    subtitle.language[0]=m_pids.SubLang3_1;
    subtitle.language[1]=m_pids.SubLang3_2;
    subtitle.language[2]=m_pids.SubLang3_3;
    subtitle.language[3]=0;
    subtitle.subtitleType=0; // DVB subtitle
    m_subtitleStreams.push_back(subtitle);
  }

  if (m_pids.SubtitlePid4!=0)
  {
    struct stSubtitleStream subtitle;
    subtitle.pid=m_pids.SubtitlePid4;
    subtitle.language[0]=m_pids.SubLang4_1;
    subtitle.language[1]=m_pids.SubLang4_2;
    subtitle.language[2]=m_pids.SubLang4_3;
    subtitle.language[3]=0;
    subtitle.subtitleType=0; // DVB subtitle
    m_subtitleStreams.push_back(subtitle);
  }

  bool changed=false;
  bool videoChanged=false;
  //did the video format change?
  if (oldVideoServiceType != m_pids.videoServiceType )
  {
    //yes, is the video pin connected?
    if (m_filter.GetVideoPin()->IsConnected())
    {
      changed=true;
      videoChanged=true;
    }
  }

  m_iAudioStream = 0;

  LogDebug ("Setting initial audio index to : %i", m_iAudioStream);
  bool audioChanged=false;

  //get the new audio format
  int newAudioStreamType=SERVICE_TYPE_AUDIO_MPEG2;
  if (m_iAudioStream>=0 && m_iAudioStream < m_audioStreams.size())
  {
    newAudioStreamType=m_audioStreams[m_iAudioStream].audioType;
  }

  //did the audio format change?
  if (oldAudioStreamType != newAudioStreamType )
  {
    //yes, is the audio pin connected?
    if (m_filter.GetAudioPin()->IsConnected())
    {
      changed=true;
      audioChanged=true;
    }
  }

  //did audio/video format change?
  if (changed)
  {
    #ifdef USE_DYNAMIC_PINS
    // if we have a video stream and it's format changed, let the mpeg parser trigger the OnMediaTypeChanged
    if (m_pids.VideoPid>0x1 && videoChanged)
    {
      LogDebug("DeMultiplexer: We detected a new media type change which has a video stream, so we let the mpegParser trigger the event");
      m_receivedPackets=0;
      ResetMpeg2VideoInfo();
      m_mpegParserTriggerFormatChange=true;
      SetVideoChanging(true);
    }
    else
    {
      // notify the ITSReaderCallback. MP will then rebuild the graph
      LogDebug("DeMultiplexer: Audio media types changed. Trigger OnMediaTypeChanged()...");
      m_filter.OnMediaTypeChanged(1);
      SetVideoChanging(true); 
    }
    #else
    if (audioChanged && videoChanged)
      m_filter.OnMediaTypeChanged(3);
    else
      if (audioChanged)
        m_filter.OnMediaTypeChanged(1);
      else
        m_filter.OnMediaTypeChanged(2);
    #endif
  }

  //if we have more than 1 audio track available, tell host application that we are ready
  //to receive an audio track change.
  if (m_audioStreams.size() > 1)
  {
    LogDebug("OnRequestAudioChange()");
    m_filter.OnRequestAudioChange();
  }


  if( pSubUpdateCallback != NULL){
    int bitmap_index = -1;
    (*pSubUpdateCallback)(m_subtitleStreams.size(),(m_subtitleStreams.size() > 0 ? &m_subtitleStreams[0] : NULL),&bitmap_index);
    if(bitmap_index >= 0){
      LogDebug("Calling SetSubtitleStream from OnNewChannel:  %i", bitmap_index);
      SetSubtitleStream(bitmap_index);
    }
  }
}

///
/// Method which stops the graph
HRESULT CDeMultiplexer::DoStop()
{
  LogDebug("demux:DoStop");
  HRESULT hr = E_UNEXPECTED;

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
    } else {
      LogDebug("Could not get IMediaControl interface");
    }

    Info.pGraph->Release();

    if (FAILED(hr)) {
      LogDebug("Stopping graph failed with 0x%x", hr);
      return S_OK;
    }
  }
  return hr;
}

///
/// Method which starts the graph
HRESULT CDeMultiplexer::DoStart()
{
  LogDebug("demux:DoStart");
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

///
/// Returns if graph is stopped
HRESULT CDeMultiplexer::IsStopped()
{
  LogDebug("demux:IsStopped");
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


///
/// Returns if graph is playing
HRESULT CDeMultiplexer::IsPlaying()
{
  LogDebug("demux:IsPlaying");

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
bool CreateFilter(const WCHAR *Name, IBaseFilter **Filter, REFCLSID FilterCategory)
{
  HRESULT hr;

  // Create the system device enumerator.
  CComPtr<ICreateDevEnum> devenum;
  hr = devenum.CoCreateInstance(CLSID_SystemDeviceEnum);
  if (FAILED(hr))
    return false;

  // Create an enumerator for this category.
  CComPtr<IEnumMoniker> classenum;
  hr = devenum->CreateClassEnumerator(FilterCategory, &classenum, 0);
  if (hr != S_OK)
    return false;

  // Find the filter that matches the name given.
  CComVariant name(Name);
  CComPtr<IMoniker> moniker;
  while (classenum->Next(1, &moniker, 0) == S_OK)
  {
    CComPtr<IPropertyBag> properties;
    hr = moniker->BindToStorage(0, 0, IID_IPropertyBag, (void **)&properties);
    if (FAILED(hr))
      return false;

    CComVariant friendlyname;
    hr = properties->Read(L"FriendlyName", &friendlyname, 0);
    if (FAILED(hr))
      return false;

    if (name == friendlyname)
    {
      hr = moniker->BindToObject(0, 0, IID_IBaseFilter, (void **)Filter);
      return SUCCEEDED(hr);
    }
    moniker.Release();
  }

  // Couldn't find a matching filter.
  return false;
}
//
/// Create a filter by category and name, and add it to a filter
/// graph. Will enumerate all filters of the given category and
/// add the filter whose name matches, if any. If the filter could be
/// created but not added to the graph, the filter is destroyed.
///
/// @param Graph Filter graph.
/// @param Name of filter to create.
/// @param Filter Receives a pointer to the filter.
/// @param FilterCategory Filter category.
/// @param NameInGraph Name for the filter in the graph, or 0 for no
/// name.
///
/// @return true if successful.
bool AddFilter(IFilterGraph *Graph, const WCHAR *Name,
   IBaseFilter **Filter, REFCLSID FilterCategory,
   const WCHAR *NameInGraph)
{
  if (!CreateFilter(Name, Filter, FilterCategory))
    return false;

  if (FAILED(Graph->AddFilter(*Filter, NameInGraph)))
  {
    (*Filter)->Release();
    *Filter = 0;
    return false;
  }
  return true;
}

///Returns whether the demuxer is allowed to block in GetAudio() or not
bool CDeMultiplexer::HoldAudio()
{
  return m_bHoldAudio;
}

///Sets whether the demuxer may block in GetAudio() or not
void CDeMultiplexer::SetHoldAudio(bool onOff)
{
  LogDebug("demux:set hold audio:%d", onOff);
  m_bHoldAudio=onOff;
}

///Returns whether the demuxer is allowed to block in GetVideo() or not
bool CDeMultiplexer::HoldVideo()
{
  return m_bHoldVideo;
}

///Sets whether the demuxer may block in GetVideo() or not
void CDeMultiplexer::SetHoldVideo(bool onOff)
{
  LogDebug("demux:set hold video:%d", onOff);
  m_bHoldVideo=onOff;
}

///Returns whether the demuxer is allowed to block in GetSubtitle() or not
bool CDeMultiplexer::HoldSubtitle()
{
  return m_bHoldSubtitle;
}

///Sets whether the demuxer may block in GetSubtitle() or not
void CDeMultiplexer::SetHoldSubtitle(bool onOff)
{
  LogDebug("demux:set hold subtitle:%d", onOff);
  m_bHoldSubtitle=onOff;
}

void CDeMultiplexer::SetVideoChanging(bool onOff)
{
  LogDebug("demux:Wait for mpeg format change:%d", onOff);
  m_bVideoChanging=onOff;
}

bool CDeMultiplexer::IsVideoChanging()
{
  return m_bVideoChanging ;
}

void CDeMultiplexer::RequestNewPat(void)
{
	m_ReqPatVersion++ ;
	m_ReqPatVersion &= 0x0F ;
  LogDebug("Request new PAT = %d", m_ReqPatVersion) ;
	m_WaitNewPatTmo=GetTickCount()+10000 ;
}

void CDeMultiplexer::ClearRequestNewPat(void)
{
	m_ReqPatVersion=m_iPatVersion ;		// Used for AnalogTv or channel change fail.
}



void CDeMultiplexer::SetTeletextEventCallback(int (CALLBACK *pTeletextEventCallback)(int eventcode, DWORD64 eval))
{
  this->pTeletextEventCallback = pTeletextEventCallback;
}

void CDeMultiplexer::SetTeletextPacketCallback(int (CALLBACK *pTeletextPacketCallback)(byte*, int))
{
  this->pTeletextPacketCallback = pTeletextPacketCallback;
}

void CDeMultiplexer::SetTeletextServiceInfoCallback(int (CALLBACK *pTeletextSICallback)(int, byte,byte,byte,byte))
{
  this->pTeletextServiceInfoCallback = pTeletextSICallback;
}

void CDeMultiplexer::CallTeletextEventCallback(int eventCode,unsigned long int eventValue)
{
  if(pTeletextEventCallback != NULL)
  {
    //LogDebug("CallTeletextEventCallback %i %i", eventCode,eventValue);
    (*pTeletextEventCallback)(eventCode,eventValue);
  }
}

/*void CDeMultiplexer::SyncTeletext()
{
  if(pTeletextEventCallback!=NULL){
    IMediaSeeking * ptrMediaPos;
    LogDebug("StreamPCR %llu",m_streamPcr.PcrReferenceBase - m_duration.FirstStartPcr().PcrReferenceBase);

  if (SUCCEEDED(m_filter.GetFilterGraph()->QueryInterface(IID_IMediaSeeking , (void**)&ptrMediaPos) ) )
  {
    LONGLONG currentPos;
    ptrMediaPos->GetCurrentPosition(&currentPos);

    //LogDebug("CurrentPos %llu -> %llu",currentPos,( ( currentPos / 1000 ) * 9 ));
    ptrMediaPos->Release();

    (*pTeletextEventCallback)(TELETEXT_EVENT_CURRENT_PCR_UPDATE, m_streamPcr.PcrReferenceBase - m_duration.FirstStartPcr().PcrReferenceBase - m_filter.Compensation.Millisecs() * 90);

  }
  LogDebug("After get curpos");
  }
}*/

/*
 *  Copyright (C) 2005-2009 Team MediaPortal
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
#include <afx.h>
#include <afxwin.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "demultiplexer.h"
#include "buffer.h"
#include "..\..\shared\adaptionfield.h"
#include "tsreader.h"
#include "audioPin.h"
#include "videoPin.h"
#include "subtitlePin.h"
#include "..\..\DVBSubtitle2\Source\IDVBSub.h"
#include "mediaFormats.h"
#include "h264nalu.h"
#include <cassert>

#define MAX_BUF_SIZE 8000
#define BUFFER_LENGTH 0x1000
#define READ_SIZE (1316*30)

extern void LogDebug(const char *fmt, ...);

// *** UNCOMMENT THE NEXT LINE TO ENABLE DYNAMIC VIDEO PIN HANDLING!!!! ******
#define USE_DYNAMIC_PINS

extern int ShowBuffer;

#define DNew new // For MPC-HC source compatibility

class CH246IFrameScanner
{
private:
  bool m_bFound;
  bool m_bSeenEnough;

public:
  CH246IFrameScanner()
  {
    m_bFound = false;
    m_bSeenEnough = false;
  }

  bool SeenEnough()
  {
    return m_bSeenEnough;
  }

  bool Found()
  {
    return m_bFound && m_bSeenEnough;
  }

  void ProcessNALU(Packet *p)
  {
    ProcessNALU(p->GetData(), p->GetDataSize());
  }

  void ProcessNALU(byte *buffer, int len)
  {
    if (len < 5)
      return;

    switch (buffer[4] & 0x9f)
    {
    case 1: // Coded slice of a non-IDR picture
    case 2: // Coded slice data partition A
      {
        CGolombBuffer slice(buffer+5, len-5);
        slice.UExpGolombRead(); // skip first_mb_in_slice
        UINT slice_type = slice.UExpGolombRead();
        if (slice_type != 2 && // I slice
            slice_type != 4 && // SI slice
            slice_type != 7 && // I slice
            slice_type != 9)   // SI slice
        {
          m_bSeenEnough = true;
          m_bFound = false;
        }
        else
        {
          m_bFound = true;
        }
      }
      break;
    case 5: // Coded slice of an IDR picture
      m_bSeenEnough = m_bFound = true;
      return;
    case 9: // Access unit delimiter
      if (len > 5)
      {
        int primary_pic_type = (buffer[5] >> 5);
        m_bSeenEnough = m_bFound = (
           primary_pic_type == 0 ||
           primary_pic_type == 3 ||
           primary_pic_type == 5);
      }
      break;
    case 10: // End of sequence
      // TODO: raise EndOfGOP flag so next NAL is considered start of I-Frame
      break;
    }
  }
};

CDeMultiplexer::CDeMultiplexer(CTsDuration& duration,CTsReaderFilter& filter)
:m_duration(duration)
,m_filter(filter)
{
  m_patParser.SetCallBack(this);
  //Do not create here, as the size is adjusted dynamically to save memory
  //m_pCurrentVideoBuffer = new CBuffer();
  m_pCurrentVideoBuffer = NULL;
  m_pCurrentAudioBuffer = new CBuffer();
  m_pCurrentSubtitleBuffer = new CBuffer();
  m_iAudioStream = 0;
  m_iSubtitleStream = 0;
  m_audioPid = 0;
  m_currentSubtitlePid = 0;
  m_bEndOfFile = false;
  m_bHoldAudio = false;
  m_bHoldVideo = false;
  m_bHoldSubtitle = false;
  m_iAudioIdx = -1;
  m_iPatVersion = -1;
  m_ReqPatVersion = -1;
  m_receivedPackets = 0;
  m_bSetAudioDiscontinuity = false;
  m_bSetVideoDiscontinuity = false;
  m_reader = NULL;
  pTeletextEventCallback = NULL;
  pSubUpdateCallback = NULL;
  pTeletextPacketCallback = NULL;
  pTeletextServiceInfoCallback = NULL;
  m_iAudioReadCount = 0;
  m_lastVideoPTS.IsValid = false;
  m_lastAudioPTS.IsValid = false;
  m_mpegParserTriggerFormatChange = false;
  SetMediaChanging(false);
  SetAudioChanging(false);
  m_DisableDiscontinuitiesFiltering = false ;

  m_AudioPrevCC = -1;
  m_FirstAudioSample = 0x7FFFFFFF00000000LL;
  m_LastAudioSample = 0;

  m_WaitHeaderPES=-1 ;
  m_VideoPrevCC = -1;
  m_bIframeFound = false;
  m_FirstVideoSample = 0x7FFFFFFF00000000LL;
  m_IframeSample = 0x7FFFFFFF00000000LL;
  m_LastVideoSample = 0;
  m_LastDataFromRtsp = GetTickCount();
  m_mpegPesParser = new CMpegPesParser();
}

CDeMultiplexer::~CDeMultiplexer()
{
  Flush();
  delete m_pCurrentVideoBuffer;
  delete m_pCurrentAudioBuffer;
  delete m_pCurrentSubtitleBuffer;
  delete m_mpegPesParser;

  m_subtitleStreams.clear();
  m_audioStreams.clear();
}

int CDeMultiplexer::GetVideoServiceType()
{
  if(m_pids.videoPids.size() > 0)
  {
    return m_pids.videoPids[0].VideoServiceType;
  }
  else
  {
    return SERVICE_TYPE_VIDEO_UNKNOWN;
  }
}

void CDeMultiplexer::SetFileReader(FileReader* reader)
{
  m_reader = reader;
}

CPidTable CDeMultiplexer::GetPidTable()
{
  return m_pids;
}


/// This methods selects the audio stream specified
/// and updates the audio output pin media type if needed
bool CDeMultiplexer::SetAudioStream(int stream)
{
  LogDebug("SetAudioStream : %d",stream) ;
  //is stream index valid?
  if (stream < 0 || stream >= m_audioStreams.size())
    return S_FALSE;

  //get the current audio forma stream type
  int oldAudioStreamType = SERVICE_TYPE_AUDIO_MPEG2;
  if (m_iAudioStream >= 0 && m_iAudioStream < m_audioStreams.size())
  {
    oldAudioStreamType = m_audioStreams[m_iAudioStream].audioType;
  }
 
  //set index
  m_iAudioStream = stream;

  //get the new audio stream type
  int newAudioStreamType = SERVICE_TYPE_AUDIO_MPEG2;
  if (m_iAudioStream >= 0 && m_iAudioStream < m_audioStreams.size())
  {
    newAudioStreamType = m_audioStreams[m_iAudioStream].audioType;
  }

  //did it change?
  if (oldAudioStreamType != newAudioStreamType)
  {
    //yes, is the audio pin connected?
    if (m_filter.GetAudioPin()->IsConnected())
    {                                         // here, stream is not parsed yet
      if (!IsMediaChanging())             
      {
        LogDebug("SetAudioStream : OnMediaTypeChanged(1)") ;
        Flush() ;                   
        m_filter.OnMediaTypeChanged(1);
        SetMediaChanging(true);
      }
      else                                    // Mpeg parser info is required or audio graph is already rebuilding.
        LogDebug("SetAudioStream : Media already changing") ;   // just wait 1st GOP
    }
  }
  else
  {
    m_filter.GetAudioPin()->SetDiscontinuity(true);
  }

  SetAudioChanging(false) ;
  return S_OK;
}

bool CDeMultiplexer::GetAudioStream(__int32 &audioIndex)
{
  audioIndex = m_iAudioStream;
  return S_OK;
}

void CDeMultiplexer::GetAudioStreamInfo(int stream,char* szName)
{
  if (stream < 0 || stream>=m_audioStreams.size())
  {
    szName[0] = szName[1] = szName[2] = 0;
    return;
  }
  szName[0] = m_audioStreams[stream].language[0];
  szName[1] = m_audioStreams[stream].language[1];
  szName[2] = m_audioStreams[stream].language[2];
  szName[3] = m_audioStreams[stream].language[3];
  szName[4] = m_audioStreams[stream].language[4];
  szName[5] = m_audioStreams[stream].language[5];
  szName[6] = m_audioStreams[stream].language[6];  
}
int CDeMultiplexer::GetAudioStreamCount()
{
  return m_audioStreams.size();
}

void CDeMultiplexer::GetAudioStreamType(int stream,CMediaType& pmt)
{
  if (m_iAudioStream< 0 || stream >= m_audioStreams.size())
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
    case SERVICE_TYPE_AUDIO_DD_PLUS:
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
  if (stream < 0 || stream >= m_subtitleStreams.size())
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
  if (stream <0 || stream >= m_subtitleStreams.size())
  {
    szLanguage[0] = szLanguage[1] = szLanguage[2] = 0;
    return S_FALSE;
  }
  szLanguage[0] = m_subtitleStreams[stream].language[0];
  szLanguage[1] = m_subtitleStreams[stream].language[1];
  szLanguage[2] = m_subtitleStreams[stream].language[2];
  szLanguage[3] = m_subtitleStreams[stream].language[3];

  return S_OK;
}
bool CDeMultiplexer::GetSubtitleStreamCount(__int32 &count)
{
  count = m_subtitleStreams.size();
  return S_OK;
}

bool CDeMultiplexer::SetSubtitleResetCallback(int(CALLBACK *cb)(int, void*, int*))
{
  pSubUpdateCallback = cb;
  return S_OK;
}

bool CDeMultiplexer::GetSubtitleStreamType(__int32 stream, __int32 &type)
{
  if (m_iSubtitleStream< 0 || m_iSubtitleStream >= m_subtitleStreams.size())
  {
    // invalid stream number
    return S_FALSE;
  }

  type = m_subtitleStreams[m_iSubtitleStream].subtitleType;
  return S_OK;
}

void CDeMultiplexer::GetVideoStreamType(CMediaType &pmt)
{
  if( m_pids.videoPids.size() != 0 && m_mpegPesParser != NULL)
  {
    pmt = m_mpegPesParser->pmt;
  }
}

void CDeMultiplexer::FlushVideo()
{
  LogDebug("demux:flush video");
  CAutoLock lock (&m_sectionVideo);
  delete m_pCurrentVideoBuffer;
  m_pCurrentVideoBuffer = NULL;
  ivecBuffers it = m_vecVideoBuffers.begin();
  while (it != m_vecVideoBuffers.end())
  {
    CBuffer* videoBuffer = *it;
    delete videoBuffer;
    it = m_vecVideoBuffers.erase(it);
    /*m_outVideoBuffer++;*/
  }
  // Clear PES temporary queue.
  it = m_t_vecVideoBuffers.begin();
  while (it != m_t_vecVideoBuffers.end())
  {
    CBuffer* VideoBuffer = *it;
    delete VideoBuffer;
    it = m_t_vecVideoBuffers.erase(it);
  }

  m_p.Free();
  m_pl.RemoveAll();
  m_fHasAccessUnitDelimiters = false;

  m_VideoPrevCC = -1;
  m_bIframeFound = false;
  m_FirstVideoSample = 0x7FFFFFFF00000000LL;
  m_IframeSample = 0x7FFFFFFF00000000LL;
  m_LastVideoSample = 0;
  m_lastVideoPTS.IsValid = false;
  m_VideoValidPES = false;
  m_mVideoValidPES = false;
  m_WaitHeaderPES=-1 ;

  Reset();  // PacketSync reset.
}

void CDeMultiplexer::FlushAudio()
{
  LogDebug("demux:flush audio");
  CAutoLock lock (&m_sectionAudio);
  delete m_pCurrentAudioBuffer;
  ivecBuffers it = m_vecAudioBuffers.begin();
  while (it != m_vecAudioBuffers.end())
  {
    CBuffer* AudioBuffer = *it;
    delete AudioBuffer;
    it = m_vecAudioBuffers.erase(it);
  }
  // Clear PES temporary queue.
  it = m_t_vecAudioBuffers.begin();
  while (it != m_t_vecAudioBuffers.end())
  {
    CBuffer* AudioBuffer=*it;
    delete AudioBuffer;
    it=m_t_vecAudioBuffers.erase(it);
  }

  m_AudioPrevCC = -1;
  m_FirstAudioSample = 0x7FFFFFFF00000000LL;
  m_LastAudioSample = 0;
  m_lastAudioPTS.IsValid = false;
  m_AudioValidPES = false;
  m_pCurrentAudioBuffer = new CBuffer();

  Reset();  // PacketSync reset.
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
  m_LastDataFromRtsp = GetTickCount();
  bool holdAudio = HoldAudio();
  bool holdVideo = HoldVideo();
  bool holdSubtitle = HoldSubtitle();
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
  if (m_filter.GetVideoPin()->IsConnected() && ((m_iAudioStream == -1) || IsAudioChanging())) return NULL;

  //if there is no video pid, then simply return NULL
  if ((m_pids.videoPids.size() > 0 && m_pids.videoPids[0].Pid==0) || IsMediaChanging())
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
//    if (m_bHoldVideo) return NULL;

    //else try to read some packets from the file
    if (ReadFromFile(false,true)<READ_SIZE) break ;
  }

  //are there video packets in the buffer?
  if (m_vecVideoBuffers.size()!=0 && !IsMediaChanging()) // && (m_FirstVideoSample.m_time < m_LastAudioSample.m_time))
  {
    CAutoLock lock (&m_sectionVideo);
    //yup, then return the next one
    ivecBuffers it =m_vecVideoBuffers.begin();
    if (it!=m_vecVideoBuffers.end())
    {
      CBuffer* videoBuffer=*it;
      m_vecVideoBuffers.erase(it);
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
  if ((m_iAudioStream == -1) || IsAudioChanging()) return NULL;

  // if there is no audio pid, then simply return NULL
  if ((m_audioPid==0) || IsMediaChanging())
  {
    ReadFromFile(true,false);
    return NULL;
  }

  // when there are no audio packets at the moment
  // then try to read some from the current file
  while ((m_vecAudioBuffers.size()==0) || 
          ( m_filter.GetVideoPin()->IsConnected() )&&
          (!m_bIframeFound || (m_IframeSample.Millisecs() + 150 >= m_LastAudioSample.Millisecs())))
  {
    //if filter is stopped or
    //end of file has been reached or
    //demuxer should stop getting audio packets
    //then return NULL
    if (!m_filter.IsFilterRunning()) return NULL;
    if (m_filter.m_bStopping) return NULL;

    if (m_bEndOfFile) return NULL;
//    if (m_bHoldAudio) return NULL;

    if (ReadFromFile(true,false)<READ_SIZE) break ;
  }

  //are there audio packets in the buffer?
  if (m_vecAudioBuffers.size()==0) return NULL ;
  if (IsMediaChanging()) return NULL ;
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
  m_bStarting=true ;
  m_receivedPackets=0;
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
  while((GetTickCount() - m_Time) < 5000)
  {
    int BytesRead =ReadFromFile(false,false) ;
    if (BytesRead==0) Sleep(10) ;
    if (dwBytesProcessed>5000000 || GetAudioStreamCount()>0)
    {
      #ifdef USE_DYNAMIC_PINS
      if ((!m_mpegPesParser->basicVideoInfo.isValid && m_pids.videoPids.size() > 0 && m_pids.videoPids[0].Pid>1) && dwBytesProcessed<5000000)
      {
        dwBytesProcessed+=BytesRead;
        continue;
      }
      #endif
      m_reader->SetFilePointer(0,FILE_BEGIN);
      Flush();
      m_streamPcr.Reset();
      m_bStarting=false ;
      return;
    }
    dwBytesProcessed+=BytesRead;
  }
  m_streamPcr.Reset();
  m_iAudioReadCount=0;
  m_bStarting=false ;
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
//  if (m_bWeos) return 0 ;
//  if (IsAudioChanging()) return 0 ;          // Do not read any data during stream selection from MP C#
  if (m_filter.IsSeeking()) return 0;       // Ambass : to check
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
        //LogDebug("demux:endoffile...%d",GetTickCount()-m_LastDataFromRtsp );
        //set EOF flag and return
        if (GetTickCount()-m_LastDataFromRtsp > 2000) // A bit crappy, but no better idea...
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
//  if ( (isAudio && m_bHoldAudio) || (isVideo && m_bHoldVideo) )
//  {
    //LogDebug("demux:paused %d %d",m_bHoldAudio,m_bHoldVideo);
//    return 0;
//  }
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

  if (m_iPatVersion==-1)
  {
    // First Pat not found
    return;
  }

  // Wait for new PAT if required.
  if ((m_iPatVersion & 0x0F) != (m_ReqPatVersion & 0x0F))
  {
    if (m_ReqPatVersion==-1)                    
    {                                     // Now, unless channel change, 
       m_ReqPatVersion = m_iPatVersion;    // Initialize Pat Request.
       m_WaitNewPatTmo = GetTickCount();   // Now, unless channel change request,timeout will be always true. 
    }
    if (GetTickCount() < m_WaitNewPatTmo) 
    {
      // Timeout not reached.
      return;
    }
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

  if (IsAudioChanging() || m_iAudioStream<0 || m_iAudioStream>=m_audioStreams.size()) return;
  m_audioPid=m_audioStreams[m_iAudioStream].pid;
  if (m_audioPid==0 || m_audioPid != header.Pid) return;
  if (m_filter.GetAudioPin()->IsConnected()==false) return;
  if (header.AdaptionFieldOnly())return;

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
      CBuffer *Cbuf=*m_t_vecAudioBuffers.begin();
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
            m_AudioValidPES=false;
          }
          else
      {
            m_lastAudioPTS=pts;
          }

          Cbuf->SetPts(pts);

        }
        //skip pes header
        int headerLen=9+p[8] ;
        int len = Cbuf->Length()-headerLen;
        if (len > 0)
        {
          byte *ps = p+headerLen;
          Cbuf->SetLength(len);
          while(len--) *p++ = *ps++;   // memcpy could be not safe.
        }
        else
        {
          LogDebug(" No data");
          m_AudioValidPES=false;
        }
      }
      else
      {
        LogDebug("Pes header 0-0-1 fail");
        m_AudioValidPES=false;
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
            ivecBuffers it = m_vecAudioBuffers.begin();
            delete *it ;
            m_vecAudioBuffers.erase(it);
          }
          it = m_t_vecAudioBuffers.begin();

          CRefTime Ref;
          if((*it)->MediaTime(Ref))
          {
            if (Ref < m_FirstAudioSample) m_FirstAudioSample = Ref;
            if (Ref > m_LastAudioSample) m_LastAudioSample = Ref;
          }

          m_vecAudioBuffers.push_back(*it);
          m_t_vecAudioBuffers.erase(it);
        }
      }
      else
      {
        while (m_t_vecAudioBuffers.size())
        {
          ivecBuffers it ;
          it = m_t_vecAudioBuffers.begin();
          m_t_vecAudioBuffers.erase(it);
        }
        m_bSetAudioDiscontinuity=true;
      }
    }
    m_AudioValidPES = true;
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
void CDeMultiplexer::FillVideo(CTsHeader& header, byte* tsPacket)
{
  if (m_pids.videoPids.size() == 0 || m_pids.videoPids[0].Pid==0) return;
  if (header.Pid!=m_pids.videoPids[0].Pid) return;

  if ( header.AdaptionFieldOnly() ) return;

  if(!CheckContinuity(m_VideoPrevCC, header))
  {
    LogDebug("Video Continuity error... %x ( prev %x )", header.ContinuityCounter, m_VideoPrevCC);
    m_VideoValidPES = m_DisableDiscontinuitiesFiltering;  
  }

  m_VideoPrevCC = header.ContinuityCounter;

  CAutoLock lock (&m_sectionVideo);

  if( m_pids.videoPids[0].VideoServiceType==SERVICE_TYPE_VIDEO_MPEG1 ||
      m_pids.videoPids[0].VideoServiceType==SERVICE_TYPE_VIDEO_MPEG2 )
  {
    FillVideoMPEG2(header, tsPacket);
  }
  else
  {
//    ParseVideoH264(header, tsPacket) ;
    FillVideoH264(header, tsPacket);
  }
}

void CDeMultiplexer::FillVideoH264(CTsHeader& header, byte* tsPacket)
{
  int headerlen = header.PayLoadStart;

  CPcr pts;
  CPcr dts;

  if(!m_p)
  {
    m_p.Attach(DNew Packet());
    m_p->bDiscontinuity = false ;
    m_p->rtStart = Packet::INVALID_TIME;
  }
  
  if (header.PayloadUnitStart)
  {
    m_WaitHeaderPES = m_p->GetCount() ;
    m_mVideoValidPES = m_VideoValidPES ;
//    LogDebug("DeMultiplexer::FillVideo PayLoad Unit Start");
  }
  
  CAutoPtr<Packet> p(DNew Packet());
  
  if (headerlen < 188)
  {            
    int dataLen = 188-headerlen ;
    p->SetCount(dataLen);
    p->SetData(&tsPacket[headerlen],dataLen);

    m_p->Append(*p);
  }
  else
    return ;

  if (m_WaitHeaderPES >= 0)
  {
    int AvailablePESlength = m_p->GetCount()-m_WaitHeaderPES ;
    BYTE* start = m_p->GetData() + m_WaitHeaderPES ;
    if ((AvailablePESlength >= 9) && (AvailablePESlength >= 9+start[8]))
    { // full PES header is available.
      CPcr pts;
      CPcr dts;
      if ((start[0]==0) && 
        (start[1]==0) && 
        (start[2]==1))
      {                    
        m_VideoValidPES=true ;
        if (CPcr::DecodeFromPesHeader(start,0,pts,dts))
        {
          double diff;
          if (!m_lastVideoPTS.IsValid)
            m_lastVideoPTS=pts;
          if (m_lastVideoPTS>pts)
            diff=m_lastVideoPTS.ToClock()-pts.ToClock();
          else
            diff=pts.ToClock()-m_lastVideoPTS.ToClock();
          if (diff>10.0)
          {
            LogDebug("DeMultiplexer::FillVideo pts jump found : %f %f, %f", (float) diff, (float)pts.ToClock(), (float)m_lastVideoPTS.ToClock());
            m_VideoValidPES=false ;
          }
          else
          {
//            LogDebug("DeMultiplexer::FillVideo pts : %f ", (float)pts.ToClock());
            m_lastVideoPTS=pts;
          }
        }
        m_p->RemoveAt(m_WaitHeaderPES, 9+start[8]) ;
      }
      else
      {
        LogDebug("Pes 0-0-1 fail") ;
        m_VideoValidPES=false ;
      }

      m_p->rtStart = pts.IsValid ? (pts.PcrReferenceBase) : Packet::INVALID_TIME;

      m_WaitHeaderPES = -1 ;
    }
    else   
    {                             
      int FullLen = -1 ;   
      if (AvailablePESlength >= 9) FullLen = 9+start[8] ;   
      LogDebug("demux:vid Incomplete PES ( Avail %d / %d )", AvailablePESlength,FullLen) ;    
      return ;
    }
  }

  if (m_p->GetCount())
  {
    BYTE* start = m_p->GetData();
    BYTE* end = start + m_p->GetCount();

    while(start <= end-4 && *(DWORD*)start != 0x01000000) start++;

    while(start <= end-4)
    {
      BYTE* next = start+1;

      while(next <= end-4 && *(DWORD*)next != 0x01000000) next++;

      if(next >= end-4) break ;
        
      int size = next - start;

      CH264Nalu     Nalu;
      Nalu.SetBuffer (start, size, 0);

      CAutoPtr<Packet> p2;

      while (Nalu.ReadNext())
      {
        DWORD dwNalLength = 
          ((Nalu.GetDataLength() >> 24) & 0x000000ff) |
          ((Nalu.GetDataLength() >>  8) & 0x0000ff00) |
          ((Nalu.GetDataLength() <<  8) & 0x00ff0000) |
          ((Nalu.GetDataLength() << 24) & 0xff000000);
        CAutoPtr<Packet> p3(DNew Packet());

        p3->SetCount (Nalu.GetDataLength()+sizeof(dwNalLength));

        memcpy (p3->GetData(), &dwNalLength, sizeof(dwNalLength));
        memcpy (p3->GetData()+sizeof(dwNalLength), Nalu.GetDataBuffer(), Nalu.GetDataLength());

        if (p2 == NULL)
          p2 = p3;
        else
          p2->Append(*p3);
      }

      if((*(p2->GetData()+4)&0x1f) == 0x09) m_fHasAccessUnitDelimiters = true;
      if((*(p2->GetData()+4)&0x1f) == 0x09 || !m_fHasAccessUnitDelimiters && m_p->rtStart != Packet::INVALID_TIME)
      {
        if ((m_pl.GetCount()>0) && m_mVideoValidPES)
        {
          CAutoPtr<Packet> p(DNew Packet());
          p = m_pl.RemoveHead();
//          LogDebug("Output NALU Type: %d (%d)", p->GetAt(4)&0x1f,p->GetCount());
          CH246IFrameScanner iFrameScanner;
          iFrameScanner.ProcessNALU(p);
    
          while(m_pl.GetCount())
          {
            CAutoPtr<Packet> p2 = m_pl.RemoveHead();
            if (!iFrameScanner.SeenEnough())
              iFrameScanner.ProcessNALU(p2);
//          LogDebug("Output NALU Type: %d (%d)", p2->GetAt(4)&0x1f,p2->GetCount());
            p->Append(*p2);
          }
        
          CPcr timestamp;
          if(p->rtStart != Packet::INVALID_TIME )
          {
            timestamp.PcrReferenceBase = p->rtStart;
            timestamp.IsValid=true;
          }
//          LogDebug("frame len %d decoded PTS %f p timestamp %f", p->GetCount(), pts.ToClock(), timestamp.ToClock());
    
          int lastVidResX=m_mpegPesParser->basicVideoInfo.width;
          int lastVidResY=m_mpegPesParser->basicVideoInfo.height;
    
          bool Gop = m_mpegPesParser->OnTsPacket(p->GetData(), p->GetCount(),(m_pids.videoPids[0].VideoServiceType==SERVICE_TYPE_VIDEO_MPEG2));
    
          if ((Gop || m_bIframeFound) && m_filter.GetVideoPin()->IsConnected())
          {
            CRefTime Ref;
            CBuffer *pCurrentVideoBuffer = new CBuffer(p->GetCount());
            pCurrentVideoBuffer->Add(p->GetData(), p->GetCount());
            pCurrentVideoBuffer->SetPts(timestamp);   
            pCurrentVideoBuffer->SetPcr(m_duration.FirstStartPcr(),m_duration.MaxPcr());
            pCurrentVideoBuffer->MediaTime(Ref) ;
            // Must use p->rtStart as CPcr is UINT64 and INVALID_TIME is LONGLONG
            // Too risky to change CPcr implementation at this time 
            if(p->rtStart != Packet::INVALID_TIME)
            {
              if (Gop && !m_bIframeFound)
              {
                m_IframeSample = Ref;
                m_bIframeFound = true;
                LogDebug("  H.264 I-FRAME found %f ", m_IframeSample.Millisecs()/1000.0f);
              }
              if (Ref < m_FirstVideoSample) m_FirstVideoSample = Ref;
              if (Ref > m_LastVideoSample) m_LastVideoSample = Ref;
            }
    
            pCurrentVideoBuffer->SetFrameType(Gop? 'I':'?');
            pCurrentVideoBuffer->SetFrameCount(0);
            pCurrentVideoBuffer->SetVideoServiceType(m_pids.videoPids[0].VideoServiceType);
            if (m_bSetVideoDiscontinuity)
            {
              m_bSetVideoDiscontinuity=false;
              pCurrentVideoBuffer->SetDiscontinuity();
            }
    
            // ownership is transfered to vector
            m_vecVideoBuffers.push_back(pCurrentVideoBuffer);
          }
    
          if (lastVidResX!=m_mpegPesParser->basicVideoInfo.width || lastVidResY!=m_mpegPesParser->basicVideoInfo.height)
          {
            LogDebug("DeMultiplexer: %x video format changed: res=%dx%d aspectRatio=%d:%d fps=%d isInterlaced=%d",header.Pid,m_mpegPesParser->basicVideoInfo.width,m_mpegPesParser->basicVideoInfo.height,m_mpegPesParser->basicVideoInfo.arx,m_mpegPesParser->basicVideoInfo.ary,m_mpegPesParser->basicVideoInfo.fps,m_mpegPesParser->basicVideoInfo.isInterlaced);
            if (m_mpegParserTriggerFormatChange)
            {
              LogDebug("DeMultiplexer: OnMediaFormatChange triggered by mpeg2Parser");
              SetMediaChanging(true);
              m_filter.OnMediaTypeChanged(3);
              m_mpegParserTriggerFormatChange=false;
            }
            LogDebug("DeMultiplexer: triggering OnVideoFormatChanged");
            m_filter.OnVideoFormatChanged(m_mpegPesParser->basicVideoInfo.streamType,m_mpegPesParser->basicVideoInfo.width,m_mpegPesParser->basicVideoInfo.height,m_mpegPesParser->basicVideoInfo.arx,m_mpegPesParser->basicVideoInfo.ary,15000000,m_mpegPesParser->basicVideoInfo.isInterlaced);
          }
          else
          {
            if (m_mpegParserTriggerFormatChange && Gop)
            {
              LogDebug("DeMultiplexer: Got GOP after the channel change was detected without correct mpeg header parsing, so we trigger the format change now.");
              m_filter.OnMediaTypeChanged(3);
              m_mpegParserTriggerFormatChange=false;
            }
          }
        }  
        else
          m_bSetVideoDiscontinuity = !m_mVideoValidPES ;
        m_pl.RemoveAll() ;
          
        p2->bDiscontinuity = m_p->bDiscontinuity; m_p->bDiscontinuity = FALSE;
        p2->rtStart = m_p->rtStart; m_p->rtStart = Packet::INVALID_TIME;
      }
      else
      {
        p2->bDiscontinuity = FALSE;
        p2->rtStart = Packet::INVALID_TIME;
      }
      
//      LogDebug(".......> Store NALU length = %d (%d)", (*(p2->GetData()+4) & 0x1F), p2->GetCount()) ;
      m_pl.AddTail(p2);

      start = next;
    }
      
    if(start > m_p->GetData())
    {
      m_p->RemoveAt(0, start - m_p->GetData());
    }
  }
  return;
}

void CDeMultiplexer::FillVideoMPEG2(CTsHeader& header, byte* tsPacket)
{
  //does tspacket contain the start of a pes packet?
  if (header.PayloadUnitStart)
  {
    if (!m_pCurrentVideoBuffer)
    {
      m_pCurrentVideoBuffer = new CBuffer();
    }

    //yes packet contains start of a pes packet.
    //does current buffer hold any data ?
    if (m_pCurrentVideoBuffer->Length() > 0)
    {
      m_pCurrentVideoBuffer->SetVideoServiceType(m_pids.videoPids[0].VideoServiceType);
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
          if (m_pids.videoPids[0].VideoServiceType==SERVICE_TYPE_VIDEO_MPEG2)
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

                (*it)->SetFrameType(tc[((p[offset+2]>>3)&7)+(Gop<<3)]);
                (*it)->SetFrameCount((p[offset+2]>>6)+(p[offset+1]<<2)) ;

                break ;
              }
            }
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
          {
            int lastVidResX=m_mpegPesParser->basicVideoInfo.width;
            int lastVidResY=m_mpegPesParser->basicVideoInfo.height;

            bool parsed=m_mpegPesParser->OnTsPacket((*it)->Data(),(*it)->Length(),(m_pids.videoPids[0].VideoServiceType==SERVICE_TYPE_VIDEO_MPEG2));

            if (lastVidResX!=m_mpegPesParser->basicVideoInfo.width || lastVidResY!=m_mpegPesParser->basicVideoInfo.height)
            {
              LogDebug("DeMultiplexer: %x video format changed: res=%dx%d aspectRatio=%d:%d fps=%d isInterlaced=%d",header.Pid,m_mpegPesParser->basicVideoInfo.width,m_mpegPesParser->basicVideoInfo.height,m_mpegPesParser->basicVideoInfo.arx,m_mpegPesParser->basicVideoInfo.ary,m_mpegPesParser->basicVideoInfo.fps,m_mpegPesParser->basicVideoInfo.isInterlaced);

              if (m_mpegParserTriggerFormatChange)
              {
                LogDebug("DeMultiplexer: OnMediaFormatChange triggered by mpeg2Parser");
                SetMediaChanging(true);
                m_filter.OnMediaTypeChanged(3);
                m_mpegParserTriggerFormatChange=false;
              }
              LogDebug("DeMultiplexer: triggering OnVideoFormatChanged");
              m_filter.OnVideoFormatChanged(m_mpegPesParser->basicVideoInfo.streamType,m_mpegPesParser->basicVideoInfo.width,m_mpegPesParser->basicVideoInfo.height,m_mpegPesParser->basicVideoInfo.arx,m_mpegPesParser->basicVideoInfo.ary,15000000,m_mpegPesParser->basicVideoInfo.isInterlaced);
            }
            else
            {
              if (m_mpegParserTriggerFormatChange && Gop)
              {
                LogDebug("DeMultiplexer: Got GOP after the channel change was detected without correct mpeg header parsing, so we trigger the format change now.");
                m_filter.OnMediaTypeChanged(3);
                m_mpegParserTriggerFormatChange=false;
              }
            }
          }

          //yes, then move the full PES in main queue.
          while (m_t_vecVideoBuffers.size())
          { 
            ivecBuffers it ;
            it = m_t_vecVideoBuffers.begin();

            if ((Gop || m_bIframeFound) && m_filter.GetVideoPin()->IsConnected())
            {
              CRefTime Ref;
              if((*it)->MediaTime(Ref))
              {
                if (Gop && !m_bIframeFound) m_IframeSample = Ref;
                if (Ref < m_FirstVideoSample) m_FirstVideoSample = Ref;
                if (Ref > m_LastVideoSample) m_LastVideoSample = Ref;
              }
              m_bIframeFound = true;
              if (m_bSetVideoDiscontinuity)
              {
                m_bSetVideoDiscontinuity = false;
                (*it)->SetDiscontinuity();
              }
              m_vecVideoBuffers.push_back(*it);
              m_t_vecVideoBuffers.erase(it);
            }
            else
            {
              delete (*it);
              m_t_vecVideoBuffers.erase(it);
            }
          }
        }
      }
      else
      {
        while (m_t_vecVideoBuffers.size())
        {
          ivecBuffers it;
          it = m_t_vecVideoBuffers.begin();
          delete (*it);
          m_t_vecVideoBuffers.erase(it);
        }
        m_bSetVideoDiscontinuity=true;
      }
    }
    m_VideoValidPES = true;
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
      m_pCurrentVideoBuffer->SetVideoServiceType(m_pids.videoPids[0].VideoServiceType);
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
//    m_filter.m_bOnZap = true ;
  }
  else
  {
    // No audio streams or channel info was not changed
    if (pids.audioPids.size()==0 || m_pids == pids )
    { 
      return; // no
    }
  }

  //remember the old audio & video formats
  int oldVideoServiceType(-1);
  if(m_pids.videoPids.size()>0)
  {
    oldVideoServiceType=m_pids.videoPids[0].VideoServiceType;
  }
  int oldAudioStreamType=SERVICE_TYPE_AUDIO_MPEG2;
  if (m_iAudioStream>=0 && m_iAudioStream < m_audioStreams.size())
  {
    oldAudioStreamType=m_audioStreams[m_iAudioStream].audioType;
  }
  m_pids=pids;
  LogDebug("New channel found (PAT/PMT/SDT changed)");
  m_pids.LogPIDs();

  if(pTeletextEventCallback != NULL)
  {
    (*pTeletextEventCallback)(TELETEXT_EVENT_RESET,TELETEXT_EVENTVALUE_NONE);
  }

  IDVBSubtitle* pDVBSubtitleFilter(m_filter.GetSubtitleFilter());
  if( pDVBSubtitleFilter )
  {
    // Make sure that subtitle cache is reset ( in filter & MP )
    pDVBSubtitleFilter->NotifyChannelChange();
  }

  //update audio streams etc..
  if (m_pids.PcrPid>0x1)
  {
    m_duration.SetVideoPid(m_pids.PcrPid);
  }
  else if (m_pids.videoPids.size() > 0 && m_pids.videoPids[0].Pid>0x1)
  {
    m_duration.SetVideoPid(m_pids.videoPids[0].Pid);
  }
  m_audioStreams.clear();

  for(int i(0) ; i < m_pids.audioPids.size() ; i++)
  {
    struct stAudioStream audio;
    audio.pid=m_pids.audioPids[i].Pid;
    audio.language[0]=m_pids.audioPids[i].Lang[0];
    audio.language[1]=m_pids.audioPids[i].Lang[1];
    audio.language[2]=m_pids.audioPids[i].Lang[2];
    audio.language[3]=m_pids.audioPids[i].Lang[3];
    audio.language[4]=m_pids.audioPids[i].Lang[4];
    audio.language[5]=m_pids.audioPids[i].Lang[5];
    audio.language[6]=0;
    audio.audioType = m_pids.audioPids[i].AudioServiceType;
    m_audioStreams.push_back(audio);
  }

  m_subtitleStreams.clear();
  
  for(int i(0) ; i < m_pids.subtitlePids.size() ; i++)
  {
    struct stSubtitleStream subtitle;
    subtitle.pid=m_pids.subtitlePids[i].Pid;
    subtitle.language[0]=m_pids.subtitlePids[i].Lang[0];
    subtitle.language[1]=m_pids.subtitlePids[i].Lang[1];
    subtitle.language[2]=m_pids.subtitlePids[i].Lang[2];
    subtitle.language[3]=0;
    m_subtitleStreams.push_back(subtitle);
  }

  bool changed=false;
  bool videoChanged=false;
  //did the video format change?
  if (m_pids.videoPids.size() > 0 && oldVideoServiceType != m_pids.videoPids[0].VideoServiceType )
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
    if (m_pids.videoPids[0].Pid>0x1 && videoChanged)  
    {
      LogDebug("DeMultiplexer: We detected a new media type change which has a video stream, so we let the mpegParser trigger the event");
      m_receivedPackets=0;
      m_mpegParserTriggerFormatChange=true;
      SetMediaChanging(true);
    }
    else
    {
      // notify the ITSReaderCallback. MP will then rebuild the graph
      LogDebug("DeMultiplexer: Audio media types changed. Trigger OnMediaTypeChanged()...");
      m_filter.OnMediaTypeChanged(1);
      SetMediaChanging(true); 
//      SetAudioChanging(true);
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
    SetAudioChanging(true);
  }

  if( pSubUpdateCallback != NULL)
  {
    int bitmap_index = -1;
    (*pSubUpdateCallback)(m_subtitleStreams.size(),(m_subtitleStreams.size() > 0 ? &m_subtitleStreams[0] : NULL),&bitmap_index);
    if(bitmap_index >= 0)
    {
      LogDebug("Calling SetSubtitleStream from OnNewChannel:  %i", bitmap_index);
      SetSubtitleStream(bitmap_index);
    }
  }
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

void CDeMultiplexer::SetMediaChanging(bool onOff)
{
  CAutoLock lock (&m_sectionMediaChanging);
  LogDebug("demux:Wait for media format change:%d", onOff);
  m_bWaitForMediaChange=onOff;
  m_tWaitForMediaChange=GetTickCount() ;
}

bool CDeMultiplexer::IsMediaChanging(void)
{
  CAutoLock lock (&m_sectionMediaChanging);
  if (!m_bWaitForMediaChange) return false ;
  else
  {
    if (GetTickCount()-m_tWaitForMediaChange > 5000)
    {
      m_bWaitForMediaChange=false;
      LogDebug("demux: Alert: Wait for Media change cancelled on 5 secs timeout");
      return false ;
    }
  }
  return true ;
}

void CDeMultiplexer::SetAudioChanging(bool onOff)
{
  CAutoLock lock (&m_sectionAudioChanging);
  LogDebug("demux:Wait for Audio stream selection :%d", onOff);
  m_bWaitForAudioSelection=onOff;
  m_tWaitForAudioSelection=GetTickCount() ;
}

bool CDeMultiplexer::IsAudioChanging(void)
{
  CAutoLock lock (&m_sectionAudioChanging);
  if (!m_bWaitForAudioSelection) return false ;
  else
  {
    if (GetTickCount()-m_tWaitForAudioSelection > 5000)
    {
      m_bWaitForAudioSelection=false;
      LogDebug("demux: Alert: Wait for Audio stream selection cancelled on 5 secs timeout");
      return false ;
    }
  }
  return true ;
}

void CDeMultiplexer::RequestNewPat(void)
{
  m_ReqPatVersion++;
  m_ReqPatVersion &= 0x0F;
  LogDebug("Request new PAT = %d", m_ReqPatVersion);
  m_WaitNewPatTmo=GetTickCount()+10000;
}

void CDeMultiplexer::ClearRequestNewPat(void)
{
  m_ReqPatVersion=m_iPatVersion; // Used for AnalogTv or channel change fail.
}

bool CDeMultiplexer::IsNewPatReady(void)
{
  return ((m_ReqPatVersion & 0x0F) == (m_iPatVersion & 0x0F)) ? true : false ;
}

void CDeMultiplexer::ResetPatInfo(void)
{
  m_pids.Reset() ;
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
    (*pTeletextEventCallback)(eventCode,eventValue);
  }
}


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

#define MAX_BUF_SIZE 800
#define OUTPUT_PACKET_LENGTH 0x6000e
#define BUFFER_LENGTH        0x1000
extern void LogDebug(const char *fmt, ...) ;

#define READ_SIZE (1316*30)

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
  m_bHoldAudio=false;
  m_bHoldVideo=false;
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

/// This methods selects the audio stream specified
/// and updates the audio output pin media type if needed 
void CDeMultiplexer::SetAudioStream(int stream)
{
  //is stream index valid?
  if (stream< 0 || stream>=m_audioStreams.size()) return;//no..

  //get the current audio forma stream type
  int oldAudioStreamType=SERVICE_TYPE_AUDIO_MPEG2;
  if (m_iAudioStream>=0 && m_iAudioStream < m_audioStreams.size())
  {
    oldAudioStreamType=m_audioStreams[m_iAudioStream].audioType;
  }
 
  //set index
  m_iAudioStream=stream;   

  HRESULT isPlaying=IsPlaying();
  bool didStop=false;

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
      // yes, time to change audio pin media type

      //first stop the filter
      if (DoStop()==S_OK ){while(IsStopped() == S_FALSE){Sleep(100); break;}}

      //render the audio pin
      RenderFilterPin(m_filter.GetAudioPin());

      didStop=true;
    }
  }
  else
  {
    m_filter.GetAudioPin()->SetDiscontinuity(true);  
  }


  //if we where playing and stopped the graph
  if (isPlaying==S_OK && didStop)
  {
    //then start the graph again
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

/// Flushes all buffers 
///
void CDeMultiplexer::Flush()
{
  bool holdAudio=HoldAudio();
  bool holdVideo=HoldVideo();
  SetHoldAudio(true);
  SetHoldVideo(true);
  LogDebug("demux:flushing");
  ivecBuffers it;
  {
	  CAutoLock lock (&m_sectionAudio);
    delete m_pCurrentAudioBuffer;
    it =m_vecAudioBuffers.begin();
    while (it != m_vecAudioBuffers.end())
    {
      CBuffer* AudioBuffer=*it;
      delete AudioBuffer;
      it=m_vecAudioBuffers.erase(it);
    }
    m_pCurrentAudioBuffer = new CBuffer();
  }
  
  {
	  CAutoLock lock (&m_sectionVideo);
    delete m_pCurrentVideoBuffer;
    ivecBuffers it =m_vecVideoBuffers.begin();
    while (it != m_vecVideoBuffers.end())
    {
      CBuffer* videoBuffer=*it;
      delete videoBuffer;
      it=m_vecVideoBuffers.erase(it);
    }
    m_pCurrentVideoBuffer = new CBuffer();
  }

  {
	  CAutoLock lock (&m_sectionSubtitle);
    delete m_pCurrentSubtitleBuffer;
    it =m_vecSubtitleBuffers.begin();
    while (it != m_vecSubtitleBuffers.end())
    {
      CBuffer* subtitleBuffer=*it;
      delete subtitleBuffer;
      it=m_vecSubtitleBuffers.erase(it);
    }
    m_pCurrentSubtitleBuffer = new CBuffer();
  }
  SetHoldAudio(holdAudio);
  SetHoldVideo(holdVideo);
}

///
///Returns the next subtitle packet
// or NULL if there is none available
CBuffer* CDeMultiplexer::GetSubtitle()
{
	//if there is no subtitle pid, then simply return NULL
  if (m_pids.SubtitlePid==0) return NULL;

  // when there are no subtitle packets at the moment
  // then try to read some from the current file
  while (m_vecSubtitleBuffers.size()==0) 
  {
    ReadFromFile(false,false) ;

    //if we reached the end of the file, return NULL
    if (m_bEndOfFile) return NULL;
  }
  

  //are there subtitle packets in the buffer?
  if (m_vecSubtitleBuffers.size()!=0)
  {
    //yup, then return the next one
    CAutoLock lock (&m_sectionSubtitle);
    ivecBuffers it =m_vecSubtitleBuffers.begin();
    CBuffer* subtitleBuffer=*it;
    m_vecSubtitleBuffers.erase(it);
    return subtitleBuffer;
  }
  //no video packets available
  return NULL;
}

///
///Returns the next video packet
// or NULL if there is none available
CBuffer* CDeMultiplexer::GetVideo()
{ 

  //if there is no video pid, then simply return NULL
  if (m_pids.VideoPid==0)
  {
    ReadFromFile(false,true);
    return NULL;
  }

  // when there are no video packets at the moment
  // then try to read some from the current file
  while (m_vecVideoBuffers.size()==0) 
  {
    //if filter is stopped or 
    //end of file has been reached or
    //demuxer should stop getting video packets
    //then return NULL
    if (!m_filter.IsFilterRunning()) return NULL;
    if (m_bEndOfFile) return NULL;
		if (m_bHoldVideo) return NULL;

    //else try to read some packets from the file
    ReadFromFile(false,true) ;
  }
  
  //are there video packets in the buffer?
  if (m_vecVideoBuffers.size()!=0)
  {
	  CAutoLock lock (&m_sectionVideo);
    //yup, then return the next one
    ivecBuffers it =m_vecVideoBuffers.begin();
    CBuffer* videoBuffer=*it;
    m_vecVideoBuffers.erase(it);
    return videoBuffer;
  }

  //no video packets available
  return NULL;
}

///
///Returns the next audio packet
// or NULL if there is none available
CBuffer* CDeMultiplexer::GetAudio()
{
  //if there is no audio pid, then simply return NULL
  if (  m_audioPid==0)
  {
    ReadFromFile(true,false);
    return NULL;
  }

  // when there are no audio packets at the moment
  // then try to read some from the current file
  while (m_vecAudioBuffers.size()==0) 
  {
    //if filter is stopped or 
    //end of file has been reached or
    //demuxer should stop getting audio packets
    //then return NULL
    if (!m_filter.IsFilterRunning()) return NULL;
    if (m_bEndOfFile) return NULL;
		if (m_bHoldAudio) return NULL;
    ReadFromFile(true,false) ;
    //else try to read some packets from the file
  }
  //are there audio packets in the buffer?
  if (m_vecAudioBuffers.size()!=0)
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
  m_bEndOfFile=false;
  m_bHoldAudio=false;
  m_bHoldVideo=false;
  m_bScanning=true;
  DWORD dwBytesProcessed=0;
  while (ReadFromFile(false,false))
  {
    if (dwBytesProcessed>1000000 || GetAudioStreamCount()>0)
    {
      m_reader->SetFilePointer(0,FILE_BEGIN);
      Flush();
      m_streamPcr.Reset();
      m_bScanning=false;
      return;
    }
    dwBytesProcessed+=READ_SIZE;
  }
  m_streamPcr.Reset();
  m_bScanning=false;
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
bool CDeMultiplexer::ReadFromFile(bool isAudio, bool isVideo)
{
	CAutoLock lock (&m_sectionRead);
  
  DWORD dwTick=GetTickCount();
  byte buffer[READ_SIZE];
  while (true)
  {
    DWORD dwReadBytes;
    bool result=false;
    //if we are playing a RTSP stream
    if (m_reader->IsBuffer())
    {
      // and, the current buffer holds data
      while (m_reader->HasMoreData( sizeof(buffer) ) )
      {
        //then read raw data from the buffer
        m_reader->Read(buffer, sizeof(buffer), &dwReadBytes);
        //did it succeed?
        if (dwReadBytes > 0)
        {
          //yes, then process the raw data
          result=true;
          OnRawData(buffer,(int)dwReadBytes);
        }
        else
        {
          //failed to read data from the buffer?
          //LogDebug("NO read:%d",dwReadBytes);
          break;
        }

        //When needed, stop reading data so outputpin does not get blocked
        if (isAudio && m_bHoldAudio) return false;
        if (isVideo && m_bHoldVideo) return false;
      }

      //return if we succeeded
      if (result==true) 
        return true;
    }
    else
    {
      //playing a local file.
      //read raw data from the file
      m_reader->Read(buffer,sizeof(buffer), &dwReadBytes);
      if (dwReadBytes > 0)
      {
        //succeeded, process data
        OnRawData(buffer,(int)dwReadBytes);

        //and return
        return true;
      }
    }
    
		//Failed to read any data
    if ( (isAudio && m_bHoldAudio) || (isVideo && m_bHoldVideo) )
    {
      LogDebug("demux:paused %d %d",m_bHoldAudio,m_bHoldVideo);
      return false;
    }

    //if we are not timeshifting, this means we reached the end of the file
    if (!m_filter.IsTimeShifting())
    {
      //set EOF flag and return
      LogDebug("demux:endoffile");
      m_bEndOfFile=true;
      return false;
    }

    //failed to read data
    //we didnt reach the EOF
    //sleep 50 msecs and try again
    Sleep(50);

    //but when we didnt read data for >=5 secs, we return anyway to prevent lockups
    if (GetTickCount() - dwTick >=5000) break;
    
    if ( (isAudio && m_bHoldAudio) || (isVideo && m_bHoldVideo) )
      return false;
  }
  return false;
}
/// This method gets called via ReadFile() when a new TS packet has been received
/// if will :
///  - decode any new pat/pmt/sdt
///  - decode any audio/video packets and put the PES packets in the appropiate buffers
void CDeMultiplexer::OnTsPacket(byte* tsPacket)
{
  CTsHeader header(tsPacket);
  m_patParser.OnTsPacket(tsPacket);

  //if we have no PCR pid (yet) then there's nothing to decode, so return
  if (m_pids.PcrPid==0) return;

  if (header.Pid==0) return;

  //skip any packets with errors in it
  if (header.TransportError) return;

  //Do we have a start pcr?
  if (!m_duration.StartPcr().IsValid)
  {
    //no, then decode the pcr
    CAdaptionField field;
    field.Decode(header,tsPacket);
    if (field.Pcr.IsValid)
    {
      //and we consider this PCR timestamp as the start of the file
      m_duration.Set(field.Pcr,field.Pcr,field.Pcr);
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
  if (m_bScanning) return;

  //process the ts packet further
  FillSubtitle(header,tsPacket);
  FillAudio(header,tsPacket);
  FillVideo(header,tsPacket);
  }

/// This method will check if the tspacket is an audio packet
/// ifso, it decodes the PES audio packet and stores it in the audio buffers
void CDeMultiplexer::FillAudio(CTsHeader& header, byte* tsPacket)
{
  if (m_iAudioStream<0 || m_iAudioStream>=m_audioStreams.size()) return;
  m_audioPid= m_audioStreams[m_iAudioStream].pid;
  if (m_audioPid==0 || m_audioPid != header.Pid) return;
  if (m_filter.GetAudioPin()->IsConnected()==false) return;
	if ( header.AdaptionFieldOnly() ) return;

	CAutoLock lock (&m_sectionAudio);
  //does tspacket contain the start of a pes packet?
  if (header.PayloadUnitStart)
  {
    //yes packet contains start of a pes packet.
    //does current buffer hold any data ?
    if (m_pCurrentAudioBuffer->Length() > 0)
    {
      //yes, then store current buffer
      if (m_vecAudioBuffers.size()>MAX_BUF_SIZE) 
        m_vecAudioBuffers.erase(m_vecAudioBuffers.begin());
      m_vecAudioBuffers.push_back(m_pCurrentAudioBuffer);
      //and create a new one
      m_pCurrentAudioBuffer = new CBuffer();
    }

    int pos=header.PayLoadStart;
    //check for PES start code
    if (tsPacket[pos]==0&&tsPacket[pos+1]==0&&tsPacket[pos+2]==1) 
    {
      //get pts/dts from pes header
      CPcr pts;
      CPcr dts;
      if (CPcr::DecodeFromPesHeader(&tsPacket[pos],pts,dts))
      {
        m_pCurrentAudioBuffer->SetPts(pts);
      }
      //skip pes header
      int headerLen=9+tsPacket[pos+8];  
      pos+=headerLen;
    }

    //copy (rest) data in current buffer
		if (pos>0 && pos < 188)
		{
			m_pCurrentAudioBuffer->SetPcr(m_streamPcr,m_duration.StartPcr(),m_duration.MaxPcr());
			m_pCurrentAudioBuffer->Add(&tsPacket[pos],188-pos);
		}
  }
  else //if (m_pCurrentAudioBuffer->Length()>0)
  {
    //1111-1111-1-11-1-1100
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

      //store current buffer since its now full
      if (m_vecAudioBuffers.size()>MAX_BUF_SIZE) 
        m_vecAudioBuffers.erase(m_vecAudioBuffers.begin());
      m_vecAudioBuffers.push_back(m_pCurrentAudioBuffer);
      
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
  if (m_filter.GetVideoPin()->IsConnected()==false) return;
	if ( header.AdaptionFieldOnly() ) return;

	CAutoLock lock (&m_sectionVideo);
  //does tspacket contain the start of a pes packet?
  if (header.PayloadUnitStart)
  {
    //yes packet contains start of a pes packet.
    //does current buffer hold any data ?
    if (m_pCurrentVideoBuffer->Length() > 0)
    {
      //yes, then store current buffer
      if (m_vecVideoBuffers.size()>MAX_BUF_SIZE) 
        m_vecVideoBuffers.erase(m_vecVideoBuffers.begin());
      m_vecVideoBuffers.push_back(m_pCurrentVideoBuffer);
      //and create a new one
      m_pCurrentVideoBuffer = new CBuffer();
    }

    int pos=header.PayLoadStart;
    //check for PES start code
    if (tsPacket[pos]==0&&tsPacket[pos+1]==0&&tsPacket[pos+2]==1) 
    {
      //get pts/dts from pes header
      CPcr pts;
      CPcr dts;
      if (CPcr::DecodeFromPesHeader(&tsPacket[pos],pts,dts))
      {
        m_pCurrentVideoBuffer->SetPts(pts);
      }
      //skip pes header
      int headerLen=9+tsPacket[pos+8];  
      pos+=headerLen;
    }

    //copy (rest) data in current buffer
		if (pos>0 && pos < 188)
		{
			m_pCurrentVideoBuffer->SetPcr(m_streamPcr,m_duration.StartPcr(),m_duration.MaxPcr());
			m_pCurrentVideoBuffer->Add(&tsPacket[pos],188-pos);
		}
  }
  else //if (m_pCurrentVideoBuffer->Length()>0)
  {
    int pos=header.PayLoadStart;
    //packet contains rest of a pes packet
    //does the entire data in this tspacket fit in the current buffer ?
    if (m_pCurrentVideoBuffer->Length()+(188-pos)>=0x2000)
    {
      //no, then determine how many bytes do fit
      int copyLen=0x2000-m_pCurrentVideoBuffer->Length();
      //copy those bytes
      m_pCurrentVideoBuffer->Add(&tsPacket[pos],copyLen);
      pos+=copyLen;

      //store current buffer since its now full
      if (m_vecVideoBuffers.size()>MAX_BUF_SIZE) 
        m_vecVideoBuffers.erase(m_vecVideoBuffers.begin());
      m_vecVideoBuffers.push_back(m_pCurrentVideoBuffer);
      
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
/// ifso, it decodes the PES subtitle packet and stores it in the subtitle buffers
void CDeMultiplexer::FillSubtitle(CTsHeader& header, byte* tsPacket)
{
  if (m_pids.SubtitlePid==0) return;
  if (header.Pid!=m_pids.SubtitlePid) return;
  if (m_filter.GetSubtitlePin()->IsConnected()==false) return;
	if ( header.AdaptionFieldOnly() ) return;

	CAutoLock lock (&m_sectionSubtitle);
  //does tspacket contain the start of a pes packet?
  if (header.PayloadUnitStart)
  {
    //yes packet contains start of a pes packet.
    //does current buffer hold any data ?
    if (m_pCurrentSubtitleBuffer->Length() > 0)
    {
      //yes, then store current buffer
      if (m_vecSubtitleBuffers.size()>MAX_BUF_SIZE) 
        m_vecSubtitleBuffers.erase(m_vecSubtitleBuffers.begin());
      m_vecSubtitleBuffers.push_back(m_pCurrentSubtitleBuffer);
      //and create a new one
      m_pCurrentSubtitleBuffer = new CBuffer();
    }

    int pos=header.PayLoadStart;
    //check for PES start code
    if (tsPacket[pos]==0&&tsPacket[pos+1]==0&&tsPacket[pos+2]==1) 
    {
      //get pts/dts from pes header
      CPcr pts;
      CPcr dts;
      if (CPcr::DecodeFromPesHeader(&tsPacket[pos],pts,dts))
      {
        m_pCurrentSubtitleBuffer->SetPts(pts);
      }
      //skip pes header
      int headerLen=9+tsPacket[pos+8];  
      pos+=headerLen;
    }

    //copy (rest) data in current buffer
		if (pos>0 && pos < 188)
		{
			m_pCurrentSubtitleBuffer->SetPcr(m_streamPcr,m_duration.StartPcr(),m_duration.MaxPcr());
			m_pCurrentSubtitleBuffer->Add(&tsPacket[pos],188-pos);
		}
  }
  else //if (m_pCurrentSubtitleBuffer->Length()>0)
  {
    int pos=header.PayLoadStart;
    //packet contains rest of a pes packet
    //does the entire data in this tspacket fit in the current buffer ?
    if (m_pCurrentSubtitleBuffer->Length()+(188-pos)>=0x2000)
    {
      //no, then determine how many bytes do fit
      int copyLen=0x2000-m_pCurrentSubtitleBuffer->Length();
      //copy those bytes
      m_pCurrentSubtitleBuffer->Add(&tsPacket[pos],copyLen);
      pos+=copyLen;

      //store current buffer since its now full
      if (m_vecSubtitleBuffers.size()>MAX_BUF_SIZE) 
        m_vecSubtitleBuffers.erase(m_vecSubtitleBuffers.begin());
      m_vecSubtitleBuffers.push_back(m_pCurrentSubtitleBuffer);
      
      //and create a new one
      m_pCurrentSubtitleBuffer = new CBuffer();
    }

    //copy (rest) data in current buffer
		if (pos>0 && pos < 188)
		{
			m_pCurrentSubtitleBuffer->Add(&tsPacket[pos],188-pos); 
		}
  }
}	

/// This method gets called-back from the pat parser when a new PAT/PMT/SDT has been received
/// In this method we check if any audio/video/subtitle pid or format has changed
/// If not, we simply return
/// If something has changed we reconfigure the audio/video output pins if needed
void CDeMultiplexer::OnNewChannel(CChannelInfo& info)
{
	//CAutoLock lock (&m_section);
  CPidTable pids=info.PidTable;
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
				m_pids.SubtitlePid==pids.SubtitlePid)
	{
		if ( pids.videoServiceType==m_pids.videoServiceType && m_pids.VideoPid==pids.VideoPid) 
    {
      //nothing changed so return
      return;
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
  LogDebug(" Subtitle pid:%x ",m_pids.SubtitlePid);

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


  if (m_iAudioStream>=m_audioStreams.size())
  {
    m_iAudioStream=0;
  }

  bool changed=false;
  //did the video format change?
  if (oldVideoServiceType != m_pids.videoServiceType )
  {
    //yes, is the video pin connected?
    if (m_filter.GetVideoPin()->IsConnected())
    {
      changed=true;
    }
  }
  
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
    }
  }

  //did audio/video format change?
  if (changed)
  {
    //yes? then reconfigure the audio/video output pins
    //we do this in a seperate thread to prevent any lockups
    StartThread();
  }
}

///
/// Method which stops the graph
HRESULT CDeMultiplexer::DoStop()
{
  LogDebug("demux:DoStop");
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

///
/// Renders an output pin
HRESULT CDeMultiplexer::RenderFilterPin(CBasePin* pin)
{
  LogDebug("demux:RenderFilterPin");
  if ( pin->IsConnected())
  {
	  HRESULT hr = E_FAIL;
    IPin* pConnectedPin;
    PIN_INFO info;
    pin->ConnectedTo(&pConnectedPin);
    pConnectedPin->QueryPinInfo(&info);
    pConnectedPin->Release();
    IFilterGraph* graph=m_filter.GetFilterGraph();
    graph->Disconnect(pin);
    graph->RemoveFilter(info.pFilter);
    info.pFilter->Release();

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

void CDeMultiplexer::ThreadProc()
{
  LogDebug("demux:reconfigure graph");
  //remember if graph is playing
  HRESULT isPlaying=IsPlaying();

  //stop graph
  if (DoStop()==S_OK ){while(IsStopped() == S_FALSE){Sleep(100); break;}}
  
  //re-render the video output pin
  RenderFilterPin(m_filter.GetVideoPin());

  //re-render the audio output pin
  RenderFilterPin(m_filter.GetAudioPin());

  //if we where playing
  if (isPlaying==S_OK )
  {
    //then start the graph again
    DoStart();
  }
  LogDebug("demux:reconfigure graph done");
}
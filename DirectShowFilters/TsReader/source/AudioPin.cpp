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
#include <sbe.h>
#include "tsreader.h"
#include "audiopin.h"
#include "videopin.h"

#define MAX_TIME  86400000L
byte MPEG1AudioFormat[] = 
{
  0x50, 0x00,				//wFormatTag
  0x02, 0x00,				//nChannels
  0x80, 0xBB,	0x00, 0x00, //nSamplesPerSec
  0x00, 0x7D,	0x00, 0x00, //nAvgBytesPerSec
  0x00, 0x03,				//nBlockAlign
  0x00, 0x00,				//wBitsPerSample
  0x16, 0x00,				//cbSize
  0x02, 0x00,				//wValidBitsPerSample
  0x00, 0xE8,				//wSamplesPerBlock
  0x03, 0x00,				//wReserved
  0x01, 0x00,	0x01,0x00,  //dwChannelMask
  0x01, 0x00,	0x1C, 0x00, 0x00, 0x00,	0x00, 0x00, 0x00, 0x00, 0x00, 0x00

};
extern void LogDebug(const char *fmt, ...) ;

CAudioPin::CAudioPin(LPUNKNOWN pUnk, CTsReaderFilter *pFilter, HRESULT *phr,CCritSec* section) :
	CSourceStream(NAME("pinAudio"), phr, pFilter, L"Audio"),
	m_pTsReaderFilter(pFilter),
  CSourceSeeking(NAME("pinAudio"),pUnk,phr,section),
	m_section(section)
{
	m_refStartTime=m_rtStart;
	m_bDropPackets=false;
  m_bDropSeek=false;
  m_bConnected=false;
  m_bMeasureCompensation=false;
  m_bInFillBuffer=false;
	m_rtStart=0;
	m_dwSeekingCaps =
    AM_SEEKING_CanSeekAbsolute	|
	  AM_SEEKING_CanSeekForwards	|
	  AM_SEEKING_CanSeekBackwards	|
	  AM_SEEKING_CanGetStopPos	|
	  AM_SEEKING_CanGetDuration	|
    //AM_SEEKING_CanGetCurrentPos |
	  AM_SEEKING_Source;
  m_bSeeking=false;
  m_binUpdateFromSeek=false;
}

CAudioPin::~CAudioPin()
{
	LogDebug("pin:dtor()");
}
STDMETHODIMP CAudioPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
  if (riid == IID_IStreamBufferConfigure)
  {
    LogDebug("aud:IID_IStreamBufferConfigure()");
  }
  if (riid == IID_IStreamBufferInitialize)
  {
    LogDebug("aud:IID_IStreamBufferInitialize()");
  }
  if (riid == IID_IStreamBufferMediaSeeking||riid == IID_IStreamBufferMediaSeeking2)
  {
    LogDebug("aud:IID_IStreamBufferMediaSeeking()");
  }
  if (riid == IID_IStreamBufferSource)
  {
    LogDebug("aud:IID_IStreamBufferSource()");
  }
  if (riid == IID_IStreamBufferDataCounters)
  {
    LogDebug("aud:IID_IStreamBufferDataCounters()");
  }
  if (riid == IID_IMediaSeeking)
  {
    return CSourceSeeking::NonDelegatingQueryInterface( riid, ppv );
  }
  if (riid == IID_IMediaPosition)
  {
		return CSourceSeeking::NonDelegatingQueryInterface( riid, ppv );
  }
  return CSourceStream::NonDelegatingQueryInterface(riid, ppv);
}

HRESULT CAudioPin::GetMediaType(CMediaType *pmt)
{
  //LogDebug("aud:GetMediaType()");
  CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();

  int audioIndex = 0;
  demux.GetAudioStream(audioIndex);

  //demux.GetAudioStreamType(demux.GetAudioStream(), *pmt);
  demux.GetAudioStreamType(audioIndex, *pmt);
	return S_OK;
}

void CAudioPin::SetDiscontinuity(bool onOff)
{
  m_bDiscontinuity=onOff;
}

HRESULT CAudioPin::CheckConnect(IPin *pReceivePin)
{
  //LogDebug("aud:CheckConnect()");
  return CBaseOutputPin::CheckConnect(pReceivePin);
}

HRESULT CAudioPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
	HRESULT hr;
	CheckPointer(pAlloc, E_POINTER);
	CheckPointer(pRequest, E_POINTER);

	if (pRequest->cBuffers == 0)
	{
    pRequest->cBuffers = 30;
	}

	pRequest->cbBuffer = 8192;

	ALLOCATOR_PROPERTIES Actual;
	hr = pAlloc->SetProperties(pRequest, &Actual);
	if (FAILED(hr))
	{
    return hr;
	}

	if (Actual.cbBuffer < pRequest->cbBuffer)
	{
    return E_FAIL;
	}

	return S_OK;
}

HRESULT CAudioPin::CompleteConnect(IPin *pReceivePin)
{
  m_bInFillBuffer=false;
	LogDebug("aud:CompleteConnect()");
	HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
	if (SUCCEEDED(hr))
	{
		LogDebug("aud:CompleteConnect() done");
    m_bConnected=true;
	}
	else
	{
		LogDebug("aud:CompleteConnect() failed:%x",hr);
	}

  if (m_pTsReaderFilter->IsTimeShifting())
  {
    //m_rtDuration=CRefTime(MAX_TIME);
    REFERENCE_TIME refTime;
    m_pTsReaderFilter->GetDuration(&refTime);
    m_rtDuration=CRefTime(refTime);
  }
  else
  {
    REFERENCE_TIME refTime;
    m_pTsReaderFilter->GetDuration(&refTime);
    m_rtDuration=CRefTime(refTime);
  }
	//LogDebug("aud:CompleteConnect() ok");
	return hr;
}

HRESULT CAudioPin::BreakConnect()
{
  m_bConnected=false;
	//LogDebug("aud:BreakConnect()");
  return CSourceStream::BreakConnect();
}

HRESULT CAudioPin::FillBuffer(IMediaSample *pSample)
{
  try
  {		
    //get file-duration and set m_rtDuration
    GetDuration(NULL);
    //if the filter is currently seeking to a new position
    //or this pin is currently seeking to a new position then
    //we dont try to read any packets, but simply return...
    if (m_pTsReaderFilter->IsSeeking() || m_bSeeking)
	  {
			LogDebug("aud:isseeking");
		  Sleep(20);
      pSample->SetTime(NULL,NULL); 
	    pSample->SetActualDataLength(0);
      pSample->SetDiscontinuity(TRUE);
      pSample->SetSyncPoint(FALSE);
	    m_bInFillBuffer=false;	   
		  return NOERROR;
	  }

    //get next buffer from demultiplexer	
    m_bInFillBuffer=true;
	  CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
    CBuffer* buffer=NULL;
    while (buffer==NULL)
    {
      {
        CAutoLock lock(&m_bufferLock);
        buffer=demux.GetAudio();
      }
      if (buffer!=NULL) break;//got a buffer
      //no buffer ?
      //check if we're seeking
      if (m_pTsReaderFilter->IsSeeking() || m_bSeeking)
      {
        //yes, then return
        LogDebug("aud:isseeking2");
        Sleep(20);
        pSample->SetTime(NULL,NULL); 
        pSample->SetActualDataLength(0);
        pSample->SetDiscontinuity(TRUE);
        pSample->SetSyncPoint(FALSE);
        m_bInFillBuffer=false;
        return NOERROR;
      }
      //did we reach the end of the file
      if (demux.EndOfFile()) 
      {
         //then return
         LogDebug("aud:set eof");
         m_bInFillBuffer=false;
         return S_FALSE; //S_FALSE will notify the graph that end of file has been reached
      }
      Sleep(10);
    }//while (buffer==NULL)

    //do we need to set the discontinuity flag?
    if (m_bDiscontinuity)
    {
      //ifso, set it
      LogDebug("aud:set discontinuity");
      pSample->SetDiscontinuity(TRUE);
      m_bDiscontinuity=FALSE;
    }

    //if we got a new buffer
    if (buffer!=NULL)
    {
      if (buffer->GetDiscontinuity())
      {
        LogDebug("aud:set discontinuity");
        pSample->SetDiscontinuity(TRUE);
      }

      BYTE* pSampleBuffer;
      CRefTime cRefTime;
      //check if it has a timestamp
      if (buffer->MediaTime(cRefTime))
      {
        static float prevTime=0;
        // now comes the hard part ;-)
        // directshow expects a stream time. The stream time is reset to 0 when graph is started 
        // and after a seek operation so.. to get the stream time we get 
        // the buffer's timestamp
        // and subtract the pin's m_rtStart timestamp
        cRefTime -= m_rtStart;

        // next.. seeking is not perfect since the file does not contain a PCR for every micro second. 
        // even if we find the exact pcr time during seeking, the next start of a pes-header might start a few 
        // milliseconds later
        // We compensate this when m_bMeasureCompensation=true 
        // which is directly after seeking
        if (m_bMeasureCompensation )
        {
          //set flag to false so we dont keep compensating
          m_bMeasureCompensation=false;
          if ( m_pTsReaderFilter->GetVideoPin()->IsConnected() )
          {
            // set the current compensation
            m_pTsReaderFilter->Compensation=cRefTime;
            float fTime=(float)cRefTime.Millisecs();
            fTime/=1000.0f;
            LogDebug("aud:compensation:%03.3f",fTime);
            prevTime=-1;
            m_bSubtitleCompensationSet=false;

            IDVBSubtitle* pDVBSubtitleFilter(m_pTsReaderFilter->GetSubtitleFilter());
            if( pDVBSubtitleFilter )
            {
              pDVBSubtitleFilter->SetTimeCompensation(m_pTsReaderFilter->Compensation);
              m_bSubtitleCompensationSet=true;
            }
          }
        }
        //adjust the timestamp with the compensation
        cRefTime -=m_pTsReaderFilter->Compensation;

        if(!m_bSubtitleCompensationSet)
        {
          IDVBSubtitle* pDVBSubtitleFilter(m_pTsReaderFilter->GetSubtitleFilter());
          if(pDVBSubtitleFilter)
          {
            pDVBSubtitleFilter->SetTimeCompensation(m_pTsReaderFilter->Compensation);
            m_bSubtitleCompensationSet=true;
          }
        }
        //now we have the final timestamp, set timestamp in sample
        REFERENCE_TIME refTime=(REFERENCE_TIME)cRefTime;
        pSample->SetTime(&refTime,&refTime);  
        pSample->SetSyncPoint(TRUE);
        float fTime=(float)cRefTime.Millisecs();
        fTime/=1000.0f;
        
        //if (fTime<5)
        {
          //LogDebug("aud:gotbuffer:%d %03.3f",buffer->Length(),fTime);
          prevTime=fTime;
        } 
      } 
      else
      {
        //buffer has no timestamp
        //LogDebug("aud:gotbuffer:%d ",buffer->Length());
        pSample->SetTime(NULL,NULL);  
        pSample->SetSyncPoint(FALSE);
      }
      //copy buffer in sample
	    pSample->SetActualDataLength(buffer->Length());
      pSample->GetPointer(&pSampleBuffer);
      memcpy(pSampleBuffer,buffer->Data(),buffer->Length());
      //delete the buffer and return
      delete buffer;
      m_bInFillBuffer=false;
      return NOERROR;
    }
  }
  
  // Should we return something else than NOERROR when hitting an exception?
  catch(int e)
  {
    LogDebug("aud:fillbuffer exception %d", e);
  }
  catch(...)
  {
    LogDebug("aud:fillbuffer exception ...");
  }
  m_bInFillBuffer=false;
  return NOERROR;
}

bool CAudioPin::IsConnected()
{
  return m_bConnected;
}

HRESULT CAudioPin::ChangeStart()
{
  UpdateFromSeek();
	return S_OK;
}

HRESULT CAudioPin::ChangeStop()
{
  UpdateFromSeek();
	return S_OK;
}

HRESULT CAudioPin::ChangeRate()
{
  if( m_dRateSeeking <= 0 ) 
  {
    m_dRateSeeking = 1.0;  // Reset to a reasonable value.
    return E_FAIL;
  }
  UpdateFromSeek();
	return S_OK;
}

//******************************************************
/// Called when thread is about to start delivering data to the codec
/// 
HRESULT CAudioPin::OnThreadStartPlay()
{    
  //set discontinuity flag indicating to codec that the new data
  //is not belonging to any previous data
  m_bDiscontinuity=TRUE;
  m_bInFillBuffer=false;
  float fStart=(float)m_rtStart.Millisecs();
  fStart/=1000.0f;

  //tell demuxer to delete any audio packets it still might have
	CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
  demux.FlushAudio();
  LogDebug("aud:OnThreadStartPlay(%f) %02.2f", fStart,m_dRateSeeking);

  //set flag to compensate any differences in the stream time & file time
  m_bMeasureCompensation=true;

  //start playing
  DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
	return CSourceStream::OnThreadStartPlay( );
}

void CAudioPin::SetStart(CRefTime rtStartTime)
{
}
STDMETHODIMP CAudioPin::SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags, LONGLONG *pStop, DWORD StopFlags)
{
  return CSourceSeeking::SetPositions(pCurrent, CurrentFlags, pStop,  StopFlags);
}

//******************************************************
/// Returns true if a thread is currently executing in UpdateFromSeek()
/// 
bool CAudioPin::IsSeeking()
{
  return m_binUpdateFromSeek;
}

//******************************************************
/// UpdateFromSeek() called when need to seek to a specific timestamp in the file
/// m_rtStart contains the time we need to seek to...
/// 
void CAudioPin::UpdateFromSeek()
{  
  m_binUpdateFromSeek=true;
	CDeMultiplexer& demux=m_pTsReaderFilter->GetDemultiplexer();
  CTsDuration tsduration=m_pTsReaderFilter->GetDuration();
	//if (m_rtStart>m_rtDuration)
	//	m_rtStart=m_rtDuration;

  //there is a bug in directshow causing UpdateFromSeek() to be called multiple times 
  //directly after eachother
  //for a single seek operation. To 'fix' this we only perform the seeking operation
  //if we didnt do a seek in the last 5 seconds...
  if (GetTickCount()-m_seekTimer < 5000)
  {
    if (m_lastSeek==m_rtStart)
    {
      LogDebug("aud:skip seek");
      m_binUpdateFromSeek=false;	
      return;
    }
  }
  m_seekTimer=GetTickCount();

  //Note that the seek timestamp (m_rtStart) is done in the range
  //from earliest - latest from GetAvailable()
  //We however would like the seek timestamp to be in the range 0-fileduration
  m_lastSeek=m_rtStart;
  CRefTime rtSeek=m_rtStart;
  float seekTime=(float)rtSeek.Millisecs();
  seekTime/=1000.0f;
  
  //get the earliest timestamp available in the file
  float earliesTimeStamp=0;
  earliesTimeStamp= tsduration.StartPcr().ToClock() - tsduration.FirstStartPcr().ToClock();
  
  if (earliesTimeStamp<0) earliesTimeStamp=0;

  //correct the seek time
  seekTime -= earliesTimeStamp;
  if (seekTime < 0) seekTime=0;

  float duration=(float)m_rtDuration.Millisecs();
  duration /=1000.0f;
  LogDebug("aud seek to %f/%f", seekTime, duration);

  seekTime*=1000.0f;
  rtSeek = CRefTime((LONG)seekTime);

  //if another output pin is seeking, then wait until its finished
  m_bSeeking=true;
  while (m_pTsReaderFilter->IsSeeking()) Sleep(1);

  //tell demuxer to stop deliver audio data and wait until 
  //FillBuffer() finished
  demux.SetHoldAudio(true);
  while (m_bInFillBuffer) Sleep(1);  

  CAutoLock lock(&m_bufferLock);

  //if a pin-output thread exists...
  // GEMX: streaming: use old behaviour -> If ThreadExists do init a new seek else just seek (otherwise breaks channel changes while streaming)
  //       singleseat: always tell the filter that we are starting a seek operation - This fixes rewinding/forwarding
  if (ThreadExists() || !m_pTsReaderFilter->IsStreaming()) 
  {
    //tell the filter we are starting a seek operation
    m_pTsReaderFilter->SeekStart();
		
    //deliver a begin-flush to the codec filter so it stops asking for data
    DeliverBeginFlush();
		
    //stop the thread
    Stop();
		
    //do the seek...
    m_pTsReaderFilter->Seek(rtSeek,true);

    //deliver a end-flush to the codec filter so it will start asking for data again
    DeliverEndFlush();
		
    //tell filter we're done with seeking
    m_pTsReaderFilter->SeekDone(rtSeek);

    //set our start time
    //m_rtStart=rtSeek;
    
    // and restart the thread
    Run();
		//LogDebug("aud seek running");
  }
  else
  {
    //no thread running? then simply seek to the position
    m_pTsReaderFilter->Seek(rtSeek,false);
  }
	
  //tell demuxer to start deliver audio packets again
	demux.SetHoldAudio(false);
  //clear flags indiciating that the pin is seeking
  m_bSeeking=false;
  m_binUpdateFromSeek=false;
  LogDebug("aud seek done---");
}

//******************************************************
/// GetAvailable() returns 
/// pEarliest -> the earliest (pcr) timestamp in the file
/// pLatest   -> the latest (pcr) timestamp in the file
/// 
STDMETHODIMP CAudioPin::GetAvailable( LONGLONG * pEarliest, LONGLONG * pLatest )
{
  //LogDebug("aud:GetAvailable");
  //if we are timeshifting, the earliest/latest timestamp can change
  if (m_pTsReaderFilter->IsTimeShifting())
  {
    CTsDuration duration=m_pTsReaderFilter->GetDuration();
    if (pEarliest)
    {
      //return the startpcr, which is the earliest pcr timestamp available in the timeshifting file 
      double d2=duration.StartPcr().ToClock();
      d2*=1000.0f;
      CRefTime mediaTime((LONG)d2);
      *pEarliest= mediaTime;
    }
    if (pLatest)
    {
      //return the endpcr, which is the latest pcr timestamp available in the timeshifting file 
      double d2=duration.EndPcr().ToClock();
      d2*=1000.0f;
      CRefTime mediaTime((LONG)d2);
      *pLatest= mediaTime;
    }
    return S_OK;
  }

  //not timeshifting, then leave it to the default sourceseeking class
  //which returns earliest=0, latest=m_rtDuration
  return CSourceSeeking::GetAvailable( pEarliest, pLatest );
}

//******************************************************
/// Returns the file duration in REFERENCE_TIME
/// For nomal .ts files it returns the current pcr - first pcr in the file
/// for timeshifting files it returns the current pcr - the first pcr ever read
/// So the duration keeps growing, even if timeshifting files are wrapped and being resued!
//
STDMETHODIMP CAudioPin::GetDuration(LONGLONG *pDuration)
{
  //LogDebug("aud:GetDuration");
  if (m_pTsReaderFilter->IsTimeShifting())
  {
    CTsDuration duration=m_pTsReaderFilter->GetDuration();
    CRefTime totalDuration=duration.TotalDuration();
    m_rtDuration=totalDuration;
  }
  else
  {
    REFERENCE_TIME refTime;
    m_pTsReaderFilter->GetDuration(&refTime);
    m_rtDuration=CRefTime(refTime);
  }
  if (pDuration!=NULL)
  {
    return CSourceSeeking::GetDuration(pDuration);
  }
  return S_OK;
}

//******************************************************
/// GetCurrentPosition() simply returns that this is not implemented by this pin
/// reason is that only the audio/video renderer now exactly the 
/// current playing position and they do implement GetCurrentPosition()
/// 
STDMETHODIMP CAudioPin::GetCurrentPosition(LONGLONG *pCurrent)
{
  //LogDebug("aud:GetCurrentPosition");
  return E_NOTIMPL;//CSourceSeeking::GetCurrentPosition(pCurrent);
}

STDMETHODIMP CAudioPin::Notify(IBaseFilter * pSender, Quality q)
{
  return E_NOTIMPL;
}
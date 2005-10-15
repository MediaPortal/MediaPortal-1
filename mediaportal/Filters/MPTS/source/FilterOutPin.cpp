/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

#include <streams.h>
#include "MPTSFilter.h"
//#include "Mmsystem.h"
class CFilterOutPin;
extern void LogDebug(const char *fmt, ...) ;

CFilterOutPin::CFilterOutPin(LPUNKNOWN pUnk, CMPTSFilter *pFilter, FileReader *pFileReader, Sections *pSections, HRESULT *phr) :
	CSourceStream(NAME("PinObject"), phr, pFilter, L"Out"),
	CTimeShiftSeeking(NAME("MediaSeekingObject"), pUnk, phr, &m_cSharedState),
	m_pMPTSFilter(pFilter),
	m_pFileReader(pFileReader),
	m_pSections(pSections),m_bDiscontinuity(FALSE)
{
	LogDebug("pin:ctor()");
	CAutoLock cAutoLock(&m_cSharedState);
	m_dwSeekingCaps =	
						AM_SEEKING_CanSeekForwards  | AM_SEEKING_CanSeekBackwards |
						AM_SEEKING_CanGetStopPos    | AM_SEEKING_CanGetDuration   |
						AM_SEEKING_CanSeekAbsolute  | AM_SEEKING_CanGetCurrentPos;

	__int64 size;
	m_pFileReader->GetFileSize(&size);
	m_rtDuration = m_rtStop = m_pSections->pids.Duration;
	m_lTSPacketDeliverySize = 188*10;
	m_pBuffers = new CBuffers(m_pFileReader, &m_pSections->pids,m_lTSPacketDeliverySize);
	m_dRateSeeking = 1.0;
	m_bAboutToStop=false;
}

CFilterOutPin::~CFilterOutPin()
{
	LogDebug("pin:dtor()");
	CAutoLock cAutoLock(&m_cSharedState);
	m_pBuffers->Clear();
	delete m_pBuffers;
}
STDMETHODIMP CFilterOutPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
    if (riid == IID_IMediaSeeking)
    {
        return CTimeShiftSeeking::NonDelegatingQueryInterface( riid, ppv );
    }
    return CSourceStream::NonDelegatingQueryInterface(riid, ppv);
}

HRESULT CFilterOutPin::GetMediaType(CMediaType *pmt)
{
	CAutoLock cAutoLock(m_pFilter->pStateLock());

    CheckPointer(pmt, E_POINTER);

	pmt->InitMediaType();
	pmt->SetType      (& MEDIATYPE_Stream);
	pmt->SetSubtype   (& MEDIASUBTYPE_MPEG2_TRANSPORT);

    return S_OK;
}

HRESULT CFilterOutPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
    HRESULT hr;

	CAutoLock cAutoLock(m_pFilter->pStateLock());

    CheckPointer(pAlloc, E_POINTER);
    CheckPointer(pRequest, E_POINTER);

    if (pRequest->cBuffers == 0)
    {
        pRequest->cBuffers = 2;
    }

	pRequest->cbBuffer = m_lTSPacketDeliverySize;
	

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

HRESULT CFilterOutPin::CompleteConnect(IPin *pReceivePin)
{
	LogDebug("pin:CompleteConnect()");
	HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
	if (SUCCEEDED(hr))
	{
		m_pMPTSFilter->OnConnect();
	}
	return S_OK;
}
HRESULT CFilterOutPin::FillBuffer(IMediaSample *pSample)
{
  try
  {
	//LogDebug("FillBuffer()");
	  
	CAutoLock cAutoLock(&m_cSharedState);
	if (m_bAboutToStop) return E_FAIL;
	
	CheckPointer(pSample, E_POINTER);
	PBYTE pData;
	LONG lDataLength;
	HRESULT hr = pSample->GetPointer(&pData);
	if (FAILED(hr))
	{
		LogDebug("GetPointer() failed:%x",hr);
		m_pMPTSFilter->Log((char*)"pin: FillBuffer() error on getting pointer for sample",true);
	
		return hr;
	}
	lDataLength = pSample->GetActualDataLength();


		
	__int64 fileSize;
	do
	{
		if (m_bAboutToStop) return E_FAIL;
		int count=0;
		if (m_pMPTSFilter->m_pFileReader->m_hInfoFile!=INVALID_HANDLE_VALUE)
		{
			while (true)
			{	
				if (m_bAboutToStop) return E_FAIL;
				if ( m_pMPTSFilter->UpdatePids())
				{
					LogDebug("pin:pids changed");
				}
				if ( m_pFileReader->GetFilePointer() <= m_pSections->pids.fileStartPosition &&
					m_pFileReader->GetFilePointer() + lDataLength>=m_pSections->pids.fileStartPosition )
				{
					//LogDebug("pin:Wait %x/%x (%d)", (DWORD)m_pFileReader->GetFilePointer(),(DWORD)m_pSections->pids.fileStartPosition,count);
					count++;
					if (count >100) break;
					Sleep(50);
				}
				else break;
			}
			if (count>=100)
				LogDebug("pin:Wait %x/%x (%d)", (DWORD)m_pFileReader->GetFilePointer(),(DWORD)m_pSections->pids.fileStartPosition,count);
		}

		bool endOfFile=false;
		hr = m_pBuffers->Require(lDataLength,endOfFile);
		if (endOfFile)
		{
			if (m_pMPTSFilter->m_pFileReader->m_hInfoFile!=INVALID_HANDLE_VALUE)
			{
				LogDebug("output pin:EOF");
				m_pMPTSFilter->m_pFileReader->GetFileSize(&fileSize);
				count=0;
				while (true)
				{
					if (m_bAboutToStop) return E_FAIL;
					m_pMPTSFilter->UpdatePids();
					if (m_pSections->pids.fileStartPosition >= fileSize-(1024*1024) ||
						m_pSections->pids.fileStartPosition < lDataLength) 
					{
						LogDebug("waiteof pos:%x size:%x (%d)", m_pSections->pids.fileStartPosition,fileSize,count);
						count++;
						if (count >100) break;
						Sleep(50);
					}
					else break;
				}
				LogDebug("outputpin:end of file, writepos:%x slept:%i fsize:%x", m_pSections->pids.fileStartPosition,count,fileSize);
				m_bDiscontinuity=true;
			}
		}
					
		if (m_bAboutToStop) return E_FAIL;
	} while (hr==S_OK && m_pBuffers->Count() < lDataLength);
		

	if (FAILED(hr))
	{
		if (m_pMPTSFilter->m_pFileReader->m_hInfoFile==INVALID_HANDLE_VALUE)
		{
			LogDebug("outpin:end of file detected");
			return S_FALSE;//end of stream
		}
			
		//LogDebug("outpin: Require(%d) failed:0x%x",lDataLength,hr);
		//m_pMPTSFilter->Refresh();
		//return S_FALSE; // cant read = end of stream
	}

	m_pBuffers->DequeFromBuffer(pData, lDataLength);

	ULONGLONG pts=0;
	ULONGLONG ptsStart=0;
	Sections::TSHeader header;
	for(int i=0;i<lDataLength;i+=188)
	{
		if (m_bAboutToStop) return E_FAIL;
		int pid;
		m_pSections->GetTSHeader(&pData[i],&header);
		imapDiscontinuitySent it = m_mapDiscontinuitySent.find(header.Pid);
		if (header.Pid>0)
		{
			if (it==m_mapDiscontinuitySent.end())
			{
				LogDebug("new pid:%x", header.Pid);
				m_mapDiscontinuitySent[header.Pid]=false;
				if ( header.Pid==m_pSections->pids.VideoPid || 
					header.Pid==m_pSections->pids.AudioPid ||
					header.Pid==m_pSections->pids.AC3)
				{					
					m_iDiscCounter=3;
					m_bDiscontinuity=true;
				}
			}
			else if (it->second==true)
			{
				it->second=false;
				m_bDiscontinuity=true;
			}
		}

		byte scrambled=pData[i+3] & 0xC0;
		if (scrambled)
		{
			LogDebug("pid:%x SCRAMBLED PACKET???", header.Pid);
		}
		if(m_pSections->CurrentPTS(&pData[i],&pts,&pid)==S_OK)
		{
			if (pts>0)
			{
				//LogDebug("found  pts:%x %x-%x pid:%x", (DWORD)pts, (DWORD)m_pSections->pids.StartPTS,(DWORD)m_pSections->pids.EndPTS,header.Pid);
				if (m_iPESPid==0 && pts >= m_pSections->pids.StartPTS && pts <= m_pSections->pids.EndPTS)
				{
					LogDebug("found start pts:%x %x-%x pid:%x", (DWORD)pts, (DWORD)m_pSections->pids.StartPTS,(DWORD)m_pSections->pids.EndPTS,header.Pid);
					m_iPESPid=header.Pid;
				}
				if (m_iPESPid==header.Pid)
				{
					if (ptsStart==0) 
					{ 
						ptsStart=pts; 
						break;
					}
				}
			}
		}
	}
	
	if (ptsStart>0)
	{
		if (ptsStart < m_pSections->pids.StartPTS || ptsStart > (m_pSections->pids.EndPTS+ ((__int64)0x100000)) )
		{
			LogDebug("INVALID pts:%x %x-%x", (DWORD)ptsStart,(DWORD)m_pSections->pids.StartPTS ,(DWORD) m_pSections->pids.EndPTS);
		}
		UpdatePositions(ptsStart);

		REFERENCE_TIME rtStart = static_cast<REFERENCE_TIME>(ptsStart);
		REFERENCE_TIME rtStop  = static_cast<REFERENCE_TIME>(ptsStart+1);

		pSample->SetTime(&rtStart, &rtStop); 

		pSample->SetSyncPoint(TRUE);	
		m_prevPTS=ptsStart;
	}
	else
	{
		pSample->SetTime(NULL,NULL); 
		pSample->SetSyncPoint(FALSE);
	}

	if (m_iPESPid==0)
		pSample->SetPreroll(TRUE);
	else
		pSample->SetPreroll(FALSE);

	if (m_iDiscCounter>0)
	{
		m_iDiscCounter--;
		//m_bDiscontinuity=true;
	}

//	LogDebug("snd pkt pts:%x  pes:%x disc:%d", (DWORD)ptsStart, m_iPESPid, m_bDiscontinuity);
	if(m_bDiscontinuity) 
	{
		pSample->SetDiscontinuity(TRUE);
		m_bDiscontinuity = FALSE;
	}
  }
  catch(...)
  {
	LogDebug("pin:FillBuffer() exception");
  }
  return NOERROR;
}

HRESULT CFilterOutPin::OnThreadCreate( )
{
	LogDebug("pin:OnThreadCreate()");
	m_bAboutToStop=false;
    CAutoLock cAutoLockShared(&m_cSharedState);
	if(m_pFileReader->IsFileInvalid()==TRUE)
	{
		m_pFileReader->OpenFile();
	}
    return S_OK;
}

HRESULT CFilterOutPin::OnThreadStartPlay( )
{
   LogDebug("pin:OnThreadStartPlay()");
		
   m_iDiscCounter=3;
   m_bDiscontinuity=true;
   m_iPESPid=0;
   m_prevPTS=0;
   DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
   return CSourceStream::OnThreadStartPlay( );
}



HRESULT CFilterOutPin::ChangeStart()
{
	LogDebug("pin:ChangeStart() %x",(DWORD)m_rtStart);
	m_pMPTSFilter->SetFilePosition(m_rtStart);
    UpdateFromSeek();
    return S_OK;
}

HRESULT CFilterOutPin::ChangeStop()
{
	LogDebug("pin:ChangeStop()");
    UpdateFromSeek();
    return S_OK;
}

HRESULT CFilterOutPin::ChangeRate()
{
    {   // Scope for critical section lock.
        CAutoLock cAutoLockSeeking(CTimeShiftSeeking::m_pLock);
        if( m_dRateSeeking <= 0 ) {
            m_dRateSeeking = 1.0;  // Reset to a reasonable value.
            return E_FAIL;
        }
    }
    UpdateFromSeek();
	return S_OK;
}


void CFilterOutPin::UpdateFromSeek(void)
{
	LogDebug("pin:UpdateFromSeek()");
	if (ThreadExists())
	{
        DeliverBeginFlush();
        Stop();
		DeliverEndFlush();
		Run();
	}
	LogDebug("pin:UpdateFromSeek() done");
}

HRESULT CFilterOutPin::SetDuration(REFERENCE_TIME duration)
{
	LogDebug("pin:SetDuration()");
	CAutoLock lock(CTimeShiftSeeking::m_pLock);
	m_rtDuration = duration;
	m_rtStop = m_rtDuration;
	m_rtStart=0;
    return S_OK;
}

HRESULT CFilterOutPin::GetReferenceClock(IReferenceClock **pClock)
{
	HRESULT hr;
	FILTER_INFO		filterInfo;
	hr = m_pFilter->QueryFilterInfo(&filterInfo);

	if (filterInfo.pGraph)
	{
		// Get IMediaFilter interface
		IMediaFilter* pMediaFilter = NULL;
		hr = filterInfo.pGraph->QueryInterface(IID_IMediaFilter, (void**)&pMediaFilter);
		filterInfo.pGraph->Release();

		if (pMediaFilter)
		{
			// Get IReferenceClock interface
			hr = pMediaFilter->GetSyncSource(pClock);
			pMediaFilter->Release();
			return S_OK;
		}
	}
	return E_FAIL;
}
void CFilterOutPin::ResetBuffers(__int64 newPosition)
{
	CAutoLock cAutoLock(&m_cSharedState);
	LogDebug("Reset buffers");
	if (m_pBuffers==NULL) return;
	m_pBuffers->Clear();
	m_mapDiscontinuitySent.clear();
	m_pFileReader->SetFilePointer(newPosition,FILE_BEGIN);
   m_bDiscontinuity=true;
   m_prevPTS=0;
   m_iPESPid=0;
   m_iDiscCounter=3;
   m_rtCurrent=0;
   m_rtStop=0;
   m_rtDuration=0;
}

void CFilterOutPin::UpdatePositions(ULONGLONG& ptsStart)
{
	CRefTime rtStart,rtStop,rtDuration;

	Sections::PTSTime time;
	rtStart   =m_pSections->pids.StartPTS;
	rtDuration=m_pSections->pids.EndPTS-m_pSections->pids.StartPTS;

	if (ptsStart==0) 
		return;

	ptsStart -=rtStart;

	m_pSections->PTSToPTSTime(ptsStart,&time);
	ptsStart=((ULONGLONG)36000000000*time.h)+((ULONGLONG)600000000*time.m)+((ULONGLONG)10000000*time.s)+((ULONGLONG)1000*time.u);


	m_pSections->PTSToPTSTime(rtDuration,&time);
	rtDuration=((ULONGLONG)36000000000*time.h)+((ULONGLONG)600000000*time.m)+((ULONGLONG)10000000*time.s)+((ULONGLONG)1000*time.u);


	m_rtCurrent=ptsStart;
	m_rtStart=0;
	m_rtStop=rtDuration;
	m_rtDuration=rtDuration;
}
void CFilterOutPin::AboutToStop()
{			
	LogDebug("pin: AboutToStop()");
	m_bAboutToStop=true;
}
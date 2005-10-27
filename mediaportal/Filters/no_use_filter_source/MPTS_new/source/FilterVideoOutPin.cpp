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
#include "FilterVideoOutPin.h"

extern void LogDebug(const char *fmt, ...) ;

//
CFilterVideoPin::CFilterVideoPin(LPUNKNOWN pUnk, CMPTSFilter *pFilter, FileReader *pFileReader, Sections *pSections, HRESULT *phr) :
	CSourceStream(NAME("PinVideo"), phr, pFilter, L"Video (PES)"),
	CSourceSeeking(NAME("MediaSeekingObject"), pUnk, phr, &m_cSharedState),
	m_pMPTSFilter(pFilter),
	m_pFileReader(pFileReader),
	m_pSections(pSections),m_bDiscontinuity(FALSE)
{
	LogDebug("pin:ctor()");
	CAutoLock cAutoLock(&m_cSharedState);
	__int64 size;
	m_pFileReader->GetFileSize(&size);
	m_rtDuration = m_rtStop = 36560522000;
	m_lTSPacketDeliverySize = 188*100;
	m_pBuffers = new CBuffers(m_pFileReader, &m_pSections->pids,m_lTSPacketDeliverySize);
	m_dRateSeeking = 1.0;
	m_bAboutToStop=false;
}
STDMETHODIMP CFilterVideoPin::SetPositions(LONGLONG *pCurrent,DWORD CurrentFlags,LONGLONG *pStop,DWORD StopFlags)
{
	return CSourceSeeking::SetPositions(pCurrent,CurrentFlags,pStop,StopFlags);
}

ULONGLONG CFilterVideoPin::Process(BYTE *ms)
{
	CAutoLock cAutoLock(&m_cSharedState);
	CheckPointer(ms,E_POINTER);

	DWORD offset;
	DWORD pesOffset=0;
	DWORD pesMemPointer=0;
	DWORD cpyLen=0;
	Sections::TSHeader tsHeader;
	ULONGLONG ptsNow=0;
	//const DWORD sampleDataLen=ms->GetActualDataLength();
	BYTE *sampleData=ms;
//	BYTE samplePES[18800];


	ptsNow=0;
	for(offset=0;offset<18800;offset+=188)
	{
		if(offset+184>18800)
			break;
		m_pSections->GetTSHeader(sampleData+offset,&tsHeader);
		if(tsHeader.SyncByte!=0x47 || tsHeader.TransportError)
			continue;// no packet
		if(tsHeader.Pid==m_pSections->pids.VideoPid)
		{
			pesOffset=4;
			if( tsHeader.AdaptionControl == 0x02) continue;// no payload
			if( tsHeader.AdaptionControl == 0x03) 
				pesOffset+=sampleData[offset + 4];
			// copy len
			cpyLen=188-pesOffset;
			if(pesMemPointer+cpyLen>=18800 || offset+cpyLen>=18800)
				break;
			else
			{
				CopyMemory(m_samplePES+pesMemPointer,sampleData+offset+pesOffset,cpyLen);
			}
			
			pesMemPointer+=cpyLen;

			int pid;
			ULONGLONG pts;
			if(m_pSections->CurrentPTS(sampleData+offset,&pts,&pid)==S_OK)
			{
				if (pts>0)
				{
					ptsNow=pts;
				}
			}
		}

	}
	CopyMemory(sampleData,m_samplePES,pesMemPointer);

	//delete [] samplePES;
	//Deliver(ms);
	return pesMemPointer;
}

CFilterVideoPin::~CFilterVideoPin()
{
	LogDebug("pin:dtor()");
	CAutoLock cAutoLock(&m_cSharedState);
	m_pBuffers->Clear();
	delete m_pBuffers;
}
//
HRESULT CFilterVideoPin::FillBuffer(IMediaSample *pSample)
{
	CAutoLock cAutoLock(&m_cSharedState);
	CheckPointer(pSample, E_POINTER);
	PBYTE pData;
	LONG lDataLength;
	BYTE buffer[18800];
	BYTE audioBuffer[18800];

	REFERENCE_TIME start,stop;

	pSample->GetTime(&start,&stop);

	ULONGLONG pts=0;
	ULONGLONG ptsNow=0;
	HRESULT hr = pSample->GetPointer(&pData);
	
	//pSample->SetTime((REFERENCE_TIME*)&m_rtStart,(REFERENCE_TIME*)&m_rtStop);
	if (FAILED(hr))
	{
		LogDebug("FAILED: GetPointer() failed:%x",hr);
		return hr;
	}
	lDataLength = 18800;
	hr=GetData(buffer,lDataLength,true);
	if (hr!=S_OK) 
	{
		LogDebug("FAILED to get data from file");
		return S_FALSE;
	}
	CopyMemory(audioBuffer,buffer,18800);

	lDataLength=m_pMPTSFilter->m_pAudioPin->Process(audioBuffer,m_rtStart,m_rtStop);

	lDataLength=Process(buffer);
	if(lDataLength)
	{
		CopyMemory(pData,m_samplePES,lDataLength);

		pSample->SetActualDataLength(lDataLength);
		if(m_bDiscontinuity==TRUE)
		{
			m_bDiscontinuity=FALSE;
			pSample->SetDiscontinuity(TRUE);
		}
	}
	else
	{
		int x=1;
	}
	//

	return S_OK;
}

HRESULT CFilterVideoPin::SetDuration(REFERENCE_TIME duration)
{
	LogDebug("pin:SetDuration()");
	CAutoLock lock(CSourceSeeking::m_pLock);
	m_rtDuration = duration;
	m_rtStop = m_rtDuration;
	m_rtStart=0;
    return S_OK;
}
HRESULT CFilterVideoPin::OnThreadStartPlay()
{
	LogDebug("pin:OnThreadStartPlay() %x-%x pos:%x", (DWORD)m_rtStart, (DWORD)m_rtStop, (DWORD)m_pSections->m_pFileReader->GetFilePointer());
	m_bDiscontinuity=TRUE;
	m_pBuffers->Clear();
	HRESULT hr=DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
	hr=m_pMPTSFilter->m_pAudioPin->DeliverNewSegment(m_rtStart, m_rtStop, m_dRateSeeking);
	return CSourceStream::OnThreadStartPlay( );
}
HRESULT CFilterVideoPin::OnThreadCreate()
{
	LogDebug("pin:OnThreadCreate()");
	m_bAboutToStop=false;
    CAutoLock cAutoLockShared(&m_cSharedState);
	if(m_pFileReader->IsFileInvalid()==TRUE)
	{
		m_pFileReader->OpenFile();
	}
	return CSourceStream::OnThreadCreate();
}

HRESULT CFilterVideoPin::ChangeStart()
{
	if (ThreadExists())
	{
		DeliverBeginFlush();
		m_pMPTSFilter->m_pAudioPin->DeliverBeginFlush();
		Stop();
		DeliverEndFlush();
		m_pMPTSFilter->m_pAudioPin->DeliverEndFlush();
		Run();
		m_pMPTSFilter->SetFilePosition(m_rtStart);
	}
	return S_OK;
}
HRESULT CFilterVideoPin::ChangeStop()
{
	return S_OK;
}
HRESULT CFilterVideoPin::ChangeRate()
{
	return S_OK;
}
void CFilterVideoPin::UpdatePositions(ULONGLONG& ptsNow)
{
	int a=0;
}
// read the file
HRESULT CFilterVideoPin::GetData(byte* pData, int lDataLength, bool allowedToWait)
{
	HRESULT hr;
	__int64 fileSize;
	do
	{
		if (m_bAboutToStop) return S_FALSE;
		__int64 fileSize;
		m_pFileReader->GetFileSize(&fileSize);
		if (fileSize<=0) 
		{
			LogDebug("pin:filesize=0");
			return S_FALSE;
		}
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
					if (!allowedToWait) return S_FALSE;
				//	LogDebug("pin:Wait %x/%x (%d)", (DWORD)m_pFileReader->GetFilePointer(),(DWORD)m_pSections->pids.fileStartPosition,count);
					count++;
					if (count >100) break;
					Sleep(50);
				}
				else break;
			}
			//if (count>=100)
				//LogDebug("pin:Wait %x/%x (%d)", (DWORD)m_pFileReader->GetFilePointer(),(DWORD)m_pSections->pids.fileStartPosition,count);
		}

		bool endOfFile=false;
		hr = m_pBuffers->Require(lDataLength,endOfFile);
		if (endOfFile)
		{
			if (!allowedToWait) return S_FALSE;
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
				//m_bDiscontinuity=TRUE;
			}
		}
					
		if (m_bAboutToStop) return S_FALSE;
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
	return S_OK;
}


void CFilterVideoPin::ResetBuffers(__int64 newPosition)
{
	CAutoLock cAutoLock(&m_cSharedState);
	LogDebug("Reset buffers");
	m_pFileReader->SetFilePointer(newPosition,FILE_BEGIN);
}
//
//
STDMETHODIMP CFilterVideoPin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
	CAutoLock lock(CBaseOutputPin::m_pLock);
	CheckPointer(ppv,E_POINTER);
	if (riid == IID_IMediaSeeking)
    {
        return CSourceSeeking::NonDelegatingQueryInterface( riid, ppv );
    }
	if(riid==IID_IUnknown)
	{
        return CUnknown::NonDelegatingQueryInterface( riid, ppv );
	}
    return CSourceStream::NonDelegatingQueryInterface(riid, ppv);
}

HRESULT	CFilterVideoPin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *ppropInputRequest)
{
	HRESULT hr;
	CAutoLock lock(CBaseOutputPin::m_pLock);
    CheckPointer(pAlloc, E_POINTER);
    CheckPointer(ppropInputRequest, E_POINTER);

	if (ppropInputRequest->cBuffers == 0)
    {
        ppropInputRequest->cBuffers = 2;
    }

	ppropInputRequest->cbBuffer = 18800;
	
    ALLOCATOR_PROPERTIES Actual;
    hr = pAlloc->SetProperties(ppropInputRequest, &Actual);
    if (FAILED(hr))
    {
        return hr;
    }

    if (Actual.cbBuffer < ppropInputRequest->cbBuffer)
    {
        return E_FAIL;
    }

    return S_OK;
}

HRESULT CFilterVideoPin::CompleteConnect(IPin *pPin)
{
	CAutoLock lock(CBaseOutputPin::m_pLock);
	CheckPointer(pPin,E_POINTER);
	return CBaseOutputPin::CompleteConnect(pPin);
}
HRESULT	CFilterVideoPin::GetMediaType( CMediaType *pMediaType)
{
	CAutoLock lock(CBaseOutputPin::m_pLock);
    CheckPointer(pMediaType,E_POINTER); 

	pMediaType->InitMediaType();
	pMediaType->SetType(&MEDIATYPE_MPEG2_PES);
	pMediaType->SetSubtype(&MEDIASUBTYPE_MPEG2_VIDEO);
	pMediaType->SetFormatType(&FORMAT_MPEG2Video);
	pMediaType->SetFormat(Mpeg2ProgramVideo,sizeof(Mpeg2ProgramVideo));
	int len=sizeof(Mpeg2ProgramVideo);
	return S_OK;
}
//HRESULT	CFilterVideoPin::GetSample(IMediaSample** ppSample, long len)
//{
//	return S_OK;
//}
//HRESULT	CFilterVideoPin::Active()
//{
//	return S_OK;
//}

//HRESULT CFilterVideoPin::GetMediaType(CMediaType *pmt)
//{
//
//    return S_OK;
//}

//HRESULT CFilterVideoPin::CompleteConnect(IPin *pReceivePin)
//{
//
//	return S_OK;
//}
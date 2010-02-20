/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
	m_lTSPacketDeliverySize = 188;
	m_pBuffers = new CBuffers(m_pFileReader, &m_pSections->pids,m_lTSPacketDeliverySize);
	m_dRateSeeking = 1.0;
	m_bAboutToStop=false;
}
STDMETHODIMP CFilterVideoPin::SetPositions(LONGLONG *pCurrent,DWORD CurrentFlags,LONGLONG *pStop,DWORD StopFlags)
{
	return CSourceSeeking::SetPositions(pCurrent,CurrentFlags,pStop,StopFlags);
}

void CFilterVideoPin::Process(BYTE *ms, REFERENCE_TIME& ptsStart,REFERENCE_TIME& ptsEnd, int& m_videoSampleLen, int& m_audioSampleLen)
{
	m_videoSampleLen=m_audioSampleLen=0;
	CAutoLock cAutoLock(&m_cSharedState);
	for(int offset=0;offset<m_lTSPacketDeliverySize;offset+=188)
	{
		bool isStart;
		m_tsDemuxer.ParsePacket(ms+offset,isStart);
		if (ptsStart==0)
			m_tsDemuxer.GetPCRReferenceTime(ptsStart);

		m_videoSampleLen+=m_tsDemuxer.GetVideoPacket(m_pSections->pids.VideoPid,&m_videoBuffer[m_videoSampleLen]);
		m_audioSampleLen+=m_tsDemuxer.GetAudioPacket(m_pSections->pids.CurrentAudioPid,&m_audioBuffer[m_audioSampleLen]);
	}
	m_tsDemuxer.GetPCRReferenceTime(ptsEnd);
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
	do
	{
		lDataLength = m_lTSPacketDeliverySize;
		hr=GetData(buffer,lDataLength,true);
		if (hr!=S_OK) 
		{
			LogDebug("FAILED to get data from file");
			return S_FALSE;
		}
		CopyMemory(audioBuffer,buffer,m_lTSPacketDeliverySize);

		REFERENCE_TIME ptsStart=0, ptsEnd=0;
			
		
		bool sampleSend=false;
		CAutoLock cAutoLock(&m_cSharedState);
		for(int offset=0;offset<m_lTSPacketDeliverySize;offset+=188)
		{
			bool isStart;
			bool iframe=m_tsDemuxer.ParsePacket(buffer+offset,isStart);
			if (iframe)
			{
				m_tsDemuxer.GetPCRReferenceTime(ptsStart);
			}
			if (m_tsDemuxer.IsNewPicture() && m_videoSampleLen>0)
			{
				if (sampleSend==true)
				{
					int x=1;
				}
				sampleSend=true;
				CopyMemory(pData,m_videoBuffer,m_videoSampleLen);

				if (ptsStart>0) pSample->SetTime(&ptsStart,&ptsStart+1);
				pSample->SetActualDataLength(m_videoSampleLen);
				if(m_bDiscontinuity==TRUE)
				{
					m_bDiscontinuity=FALSE;
					pSample->SetDiscontinuity(TRUE);
				}
				m_videoSampleLen=0;
				
				if (m_audioSampleLen>0)
				{
					m_pMPTSFilter->m_pAudioPin->Process(m_audioBuffer,m_audioSampleLen,ptsStart, ptsEnd);
				}
				m_audioSampleLen=0;
			}

			m_videoSampleLen+=m_tsDemuxer.GetVideoPacket(m_pSections->pids.VideoPid,&m_videoBuffer[m_videoSampleLen]);
			m_audioSampleLen+=m_tsDemuxer.GetAudioPacket(m_pSections->pids.CurrentAudioPid,&m_audioBuffer[m_audioSampleLen]);
		}
		ptsEnd=ptsStart+1;
		
		if (sampleSend) return S_OK;

		if (m_audioSampleLen>0)
		{
			m_pMPTSFilter->m_pAudioPin->Process(m_audioBuffer,m_audioSampleLen,ptsStart, ptsEnd);
			m_audioSampleLen=0;
		}

		if (m_videoSampleLen>0)
		{
			CopyMemory(pData,m_videoBuffer,m_videoSampleLen);

			if (ptsStart>0) pSample->SetTime(&ptsStart,&ptsEnd);
			pSample->SetActualDataLength(m_videoSampleLen);
			if(m_bDiscontinuity==TRUE)
			{
				m_bDiscontinuity=FALSE;
				pSample->SetDiscontinuity(TRUE);
			}
			m_videoSampleLen=0;
			return S_OK;
		}
		//
	} while (true);
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
	SeekIFrame();
	m_audioSampleLen=m_videoSampleLen=0;
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
	HRESULT hr;
	if (ThreadExists())
	{
		DeliverBeginFlush();
		hr=m_pMPTSFilter->m_pAudioPin->DeliverBeginFlush();
		Stop();
		DeliverEndFlush();
		hr=m_pMPTSFilter->m_pAudioPin->DeliverEndFlush();
		m_pMPTSFilter->SetFilePosition(m_rtStart);
		Run();
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
//	__int64 fileSize;
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
        ppropInputRequest->cBuffers = 20;
    }

	ppropInputRequest->cbBuffer = sizeof(m_videoBuffer);
	
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
	pMediaType->SetType(&MEDIATYPE_Video);
	pMediaType->SetSubtype(&MEDIASUBTYPE_MPEG2_VIDEO);
	pMediaType->SetFormatType(&FORMAT_MPEG2Video);
	pMediaType->SetFormat(Mpeg2ProgramVideo,sizeof(Mpeg2ProgramVideo));
	int len=sizeof(Mpeg2ProgramVideo);
	return S_OK;
}
void CFilterVideoPin::SeekIFrame()
{
	try
	{
	if (m_pSections->pids.VideoPid<=0) return;
	m_pBuffers->Clear();
	// find first i-frame
	TsDemux tsDemuxer;
	__int64 startPointer=m_pFileReader->GetFilePointer();;
	__int64 filePointer=m_pFileReader->GetFilePointer();
	ULONGLONG pts;
	LogDebug("find iframe pos:%x",(DWORD)filePointer);
	BYTE pData[188];
	Sections::TSHeader header;
	bool iFrameFound=false;
	while (true)
	{
		HRESULT hr=GetData(pData,188,false);
		
		if (hr!=S_OK) 
		{
			LogDebug("FAILED : GetData() in seekiframe!");
			return ;
		}
		m_pSections->GetTSHeader(pData,&header);

		int pid=header.Pid;
		if(m_pSections->CurrentPTS("ts:",pData,pts)==S_OK)
		{
			if (pts>0)
			{
				if (pts >= (ULONGLONG)m_pSections->pids.StartPTS && pts <= (ULONGLONG)m_pSections->pids.EndPTS)
				{
					//LogDebug("pts:%x pid:%x", (DWORD)pts, header.Pid);
					m_iPESPid=header.Pid;
					UpdatePositions(pts);
				}
			}
		}

		if (header.Pid==m_pSections->pids.VideoPid)
		{
			bool isStart;
			if ( tsDemuxer.ParsePacket(pData, isStart))
			{
				if (isStart)
					startPointer=filePointer;
				filePointer=0;
				m_pFileReader->SetFilePointer(startPointer,FILE_BEGIN);	
				LogDebug("iframe found at pos:%x",startPointer);
				iFrameFound=true;
				break;
			}
			if (isStart)
				startPointer=filePointer;
		}
		filePointer+=188;
	}
	if (false==iFrameFound)
	{
			LogDebug("FAILED : Iframe not found!");
	}
	else LogDebug("Iframe found!");
	}
	catch(...)
	{
		LogDebug("FAILED : exception while seeking for iframe!");
	}
	
}

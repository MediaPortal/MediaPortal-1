/**
*  TSParserSourcePin.cpp
*  Copyright (C) 2003      bisswanger
*  Copyright (C) 2004-2006 bear
*  Copyright (C) 2005      nate
*
*  This file is part of TSParserSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSParserSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSParserSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSParserSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  bisswanger can be reached at WinSTB@hotmail.com
*    Homepage: http://www.winstb.de
*
*  bear and nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#include "stdafx.h"
#include "DvbFormats.h"
#include "TSParserSource.h"
#include "TSParserSourceGuids.h"
#include <math.h>
#include "global.h"

#define USE_EVENT
#ifndef USE_EVENT
#include "Mmsystem.h"
#endif

CTSParserSourcePin::CTSParserSourcePin(LPUNKNOWN pUnk, CTSParserSourceFilter *pFilter, HRESULT *phr) :
	CSourceStream(NAME("MPEG2 Source Output"), phr, pFilter, L"Out"),
	CSourceSeeking(NAME("MPEG2 Source Output"), pUnk, phr, &m_SeekLock),
	PidParser(pFilter->m_pFileReader),
	m_pTSParserSourceFilter(pFilter),
	m_bInjectMode(FALSE),
	m_bRateControl(FALSE)
{
	m_dwSeekingCaps =
		
    AM_SEEKING_CanSeekAbsolute	|
	AM_SEEKING_CanSeekForwards	|
	AM_SEEKING_CanSeekBackwards	|
	AM_SEEKING_CanGetCurrentPos	|
	AM_SEEKING_CanGetStopPos	|
	AM_SEEKING_CanGetDuration	|
//	AM_SEEKING_CanPlayBackwards	|
//	AM_SEEKING_CanDoSegments	|
	AM_SEEKING_Source;
/*	
						AM_SEEKING_CanSeekForwards  |
						AM_SEEKING_CanGetStopPos    |
						AM_SEEKING_CanGetDuration   |
						AM_SEEKING_CanSeekAbsolute;
*/
	m_bSeeking = FALSE;

	m_rtLastSeekStart = 0;

	m_llBasePCR = -1;
	m_llNextPCR = -1;
	m_llPrevPCR = -1;
	m_lNextPCRByteOffset = 0;
	m_lPrevPCRByteOffset = 0;

	if (m_pTSParserSourceFilter->m_pPidParser->get_ProgPinMode())
		m_lTSPacketDeliverySize = 65536/4;//102400;
	else
		m_lTSPacketDeliverySize = 65536/4;//188000;

	m_DataRate = 10000000;
	m_DataRateTotal = 0;
	m_BitRateCycle = 0;
	for (int i = 0; i < 256; i++) { 
		m_BitRateStore[i] = 0;
	}

	m_pTSBuffer = new CTSBuffer(this, m_pTSParserSourceFilter->m_pClock);
	m_pPids = new PidInfo();
	debugcount = 0;

	m_rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
	m_LastFileSize = 0;
	m_LastStartSize = 0;
	m_DemuxLock = FALSE;
	m_IntLastStreamTime = 0;
	m_rtTimeShiftPosition = 0;
	m_LastMultiFileStart = 0;
	m_LastMultiFileEnd = 0;
	m_bGetAvailableMode = FALSE;
	m_IntBaseTimePCR = 0;
	m_IntStartTimePCR = 0;
	m_IntCurrentTimePCR = 0;
	m_IntEndTimePCR = 0;
	m_biMpegDemux = 0;
	m_currPosition = 0;
}

CTSParserSourcePin::~CTSParserSourcePin()
{
	delete m_pPids;
	delete m_pTSBuffer;
}

STDMETHODIMP CTSParserSourcePin::NonDelegatingQueryInterface( REFIID riid, void ** ppv )
{
	CAutoLock cAutoLock(m_pFilter->pStateLock());

	if (riid == IID_ITSParserSource)
	{
		return GetInterface((ITSParserSource*)m_pTSParserSourceFilter, ppv);
	}
	if (riid == IID_IFileSourceFilter)
	{
		return GetInterface((IFileSourceFilter*)m_pTSParserSourceFilter, ppv);
	}
    if (riid == IID_IAMFilterMiscFlags)
    {
		return GetInterface((IAMFilterMiscFlags*)m_pTSParserSourceFilter, ppv);
    }
	if (riid == IID_IAMStreamSelect && m_pTSParserSourceFilter->get_AutoMode())
	{
		 GetInterface((IAMStreamSelect*)m_pTSParserSourceFilter, ppv);
	}
	if (riid == IID_IAsyncReader)
	{
		if ((!m_pTSParserSourceFilter->m_pPidParser->pids.pcr
			&& !m_pTSParserSourceFilter->get_AutoMode()
			&& m_pTSParserSourceFilter->m_pPidParser->get_ProgPinMode())
			&& m_pTSParserSourceFilter->m_pPidParser->get_AsyncMode())
		{
			return GetInterface((IAsyncReader*)m_pTSParserSourceFilter, ppv);
		}
	}
	if (riid == IID_IMediaSeeking)
    {
        return CSourceSeeking::NonDelegatingQueryInterface( riid, ppv );
    }
	return CSourceStream::NonDelegatingQueryInterface(riid, ppv);
}

STDMETHODIMP CTSParserSourcePin::IsFormatSupported(const GUID * pFormat)
{
    CheckPointer(pFormat, E_POINTER);
	CAutoLock cAutoLock(m_pFilter->pStateLock());

	if (*pFormat == TIME_FORMAT_MEDIA_TIME)
		return S_OK;
	else
		return E_FAIL;
}

STDMETHODIMP CTSParserSourcePin::QueryPreferredFormat(GUID *pFormat)
{
    CheckPointer(pFormat, E_POINTER);
	CAutoLock cAutoLock(m_pFilter->pStateLock());
	*pFormat = TIME_FORMAT_MEDIA_TIME;
	return S_OK;
}

HRESULT CTSParserSourcePin::GetMediaType(CMediaType *pmt)
{
    CheckPointer(pmt, E_POINTER);
	CAutoLock cAutoLock(m_pFilter->pStateLock());

	pmt->InitMediaType();
	pmt->SetType      (& MEDIATYPE_Stream);
	pmt->SetSubtype   (& MEDIASUBTYPE_MPEG2_TRANSPORT);

	//Set for Program mode
	if (m_pTSParserSourceFilter->m_pPidParser->get_ProgPinMode())
		pmt->SetSubtype   (& MEDIASUBTYPE_MPEG2_PROGRAM);
	else if (!m_pTSParserSourceFilter->m_pPidParser->pids.pcr)
		pmt->SetSubtype   (& MEDIASUBTYPE_MPEG2_TRANSPORT);
//		pmt->SetSubtype   (& MEDIASUBTYPE_MPEG2_PROGRAM);

    return S_OK;
}

HRESULT CTSParserSourcePin::GetMediaType(int iPosition, CMediaType *pMediaType)
{
	CAutoLock cAutoLock(m_pFilter->pStateLock());
    
	if(iPosition < 0)
    {
        return E_INVALIDARG;
    }
    if(iPosition > 0)
    {
        return VFW_S_NO_MORE_ITEMS;
    }

    CheckPointer(pMediaType,E_POINTER);
	
	pMediaType->InitMediaType();
	pMediaType->SetType      (& MEDIATYPE_Stream);
	pMediaType->SetSubtype   (& MEDIASUBTYPE_MPEG2_TRANSPORT);

	//Set for Program mode
	if (m_pTSParserSourceFilter->m_pPidParser->get_ProgPinMode())
		pMediaType->SetSubtype   (& MEDIASUBTYPE_MPEG2_PROGRAM);
	else if (!m_pTSParserSourceFilter->m_pPidParser->pids.pcr)
		pMediaType->SetSubtype   (& MEDIASUBTYPE_MPEG2_TRANSPORT);
//		pMediaType->SetSubtype   (& MEDIASUBTYPE_MPEG2_PROGRAM);
	
    return S_OK;
}


HRESULT CTSParserSourcePin::CheckMediaType(const CMediaType* pType)
{
    CheckPointer(pType, E_POINTER);
	CAutoLock cAutoLock(m_pFilter->pStateLock());

	m_pTSParserSourceFilter->m_pDemux->AOnConnect();
	if(MEDIATYPE_Stream == pType->majortype)
	{
		//Are we in Transport mode
		if (MEDIASUBTYPE_MPEG2_TRANSPORT == pType->subtype && !m_pTSParserSourceFilter->m_pPidParser->get_ProgPinMode())
			return S_OK;

		//Are we in Program mode
		else if (MEDIASUBTYPE_MPEG2_PROGRAM == pType->subtype && m_pTSParserSourceFilter->m_pPidParser->get_ProgPinMode())
			return S_OK;

		else if (MEDIASUBTYPE_MPEG2_PROGRAM == pType->subtype && !m_pTSParserSourceFilter->m_pPidParser->pids.pcr)
			return S_OK;
	}

    return S_FALSE;
}

HRESULT CTSParserSourcePin::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest)
{
    CheckPointer(pAlloc, E_POINTER);
    CheckPointer(pRequest, E_POINTER);

    CAutoLock cAutoLock(m_pFilter->pStateLock());

    HRESULT hr;

    // Ensure a minimum number of buffers
    if (pRequest->cBuffers == 0)
    {
        pRequest->cBuffers = 2;
    }
    //pRequest->cbBuffer = 188*16;
	pRequest->cbBuffer = m_lTSPacketDeliverySize;

    ALLOCATOR_PROPERTIES Actual;
    hr = pAlloc->SetProperties(pRequest, &Actual);
    if (FAILED(hr))
    {
        return hr;
    }

    // Is this allocator unsuitable?
    if (Actual.cbBuffer < pRequest->cbBuffer)
    {
        return E_FAIL;
    }

    return S_OK;
}

HRESULT CTSParserSourcePin::CheckConnect(IPin *pReceivePin)
{
	if(!pReceivePin)
		return E_INVALIDARG;

	m_biMpegDemux = FALSE;
    CAutoLock cAutoLock(m_pFilter->pStateLock());
	HRESULT hr = CBaseOutputPin::CheckConnect(pReceivePin);
	if (SUCCEEDED(hr))
	{
		PIN_INFO pInfo;
		if (SUCCEEDED(pReceivePin->QueryPinInfo(&pInfo)))
		{
			// Get an instance of the Demux control interface
			CComPtr<IMpeg2Demultiplexer> muxInterface;
			if(SUCCEEDED(pInfo.pFilter->QueryInterface (&muxInterface)))
			{
				// Create out new pin to make sure the interface is real
				CComPtr<IPin>pIPin = NULL;
				AM_MEDIA_TYPE pintype;
				ZeroMemory(&pintype, sizeof(AM_MEDIA_TYPE));
				hr = muxInterface->CreateOutputPin(&pintype, L"MS" ,&pIPin);
				if (SUCCEEDED(hr) && pIPin != NULL)
				{
					pInfo.pFilter->Release();
					hr = muxInterface->DeleteOutputPin(L"MS");
					if SUCCEEDED(hr)
						m_biMpegDemux = TRUE;
					else if (!m_pTSParserSourceFilter->get_AutoMode())
						hr = S_OK;

					return hr;
				}
			}

			//Test for a filter with "MPEG-2" on input pin label
			if (wcsstr(pInfo.achName, L"MPEG-2") != NULL)
			{
				pInfo.pFilter->Release();
				m_biMpegDemux = TRUE;
				return S_OK;
			}

			FILTER_INFO pFilterInfo;
			if (SUCCEEDED(pInfo.pFilter->QueryFilterInfo(&pFilterInfo)))
			{
				pInfo.pFilter->Release();
				pFilterInfo.pGraph->Release();

				//Test for an infinite tee filter
				if (wcsstr(pFilterInfo.achName, L"Tee") != NULL || wcsstr(pFilterInfo.achName, L"Flow") != NULL)
				{
					m_biMpegDemux = TRUE;
					return S_OK;
				}
				hr = E_FAIL;
			}
			else
				pInfo.pFilter->Release();

		}
		if(!m_pTSParserSourceFilter->get_AutoMode())
			return S_OK;
		else
			return E_FAIL;
	}
	return hr;
}

HRESULT CTSParserSourcePin::CompleteConnect(IPin *pReceivePin)
{
    CAutoLock cAutoLock(m_pFilter->pStateLock());

	HRESULT hr = CBaseOutputPin::CompleteConnect(pReceivePin);
	if (SUCCEEDED(hr))
	{
		m_pTSParserSourceFilter->OnConnect();
		m_rtDuration = m_pTSParserSourceFilter->m_pPidParser->pids.dur;
		m_rtStop = m_rtDuration;
		m_DataRate = m_pTSParserSourceFilter->m_pPidParser->pids.bitrate;
		m_IntBaseTimePCR = m_pTSParserSourceFilter->m_pPidParser->pids.start;
		m_IntStartTimePCR = m_pTSParserSourceFilter->m_pPidParser->pids.start;
		m_IntCurrentTimePCR = m_pTSParserSourceFilter->m_pPidParser->pids.start;
		m_IntEndTimePCR = m_pTSParserSourceFilter->m_pPidParser->pids.end;
	
		//Test if parser Locked
		if (!m_pTSParserSourceFilter->m_pPidParser->m_ParsingLock){
			//fix our pid values for this run
			m_pTSParserSourceFilter->m_pPidParser->m_ParsingLock = TRUE;
			m_pTSParserSourceFilter->m_pPidParser->pids.CopyTo(m_pPids); 
			m_bASyncModeSave = m_pTSParserSourceFilter->m_pPidParser->get_AsyncMode();
			m_PacketSave = m_pTSParserSourceFilter->m_pPidParser->get_PacketSize();
			m_PATVerSave = m_pTSParserSourceFilter->m_pPidParser->m_PATVersion;
			m_TSIDSave = m_pTSParserSourceFilter->m_pPidParser->m_TStreamID;
			m_PinTypeSave  = m_pTSParserSourceFilter->m_pPidParser->get_ProgPinMode();
			m_pTSParserSourceFilter->m_pPidParser->m_ParsingLock = FALSE;
		}
	}
	return hr;
}

HRESULT CTSParserSourcePin::BreakConnect()
{
    CAutoLock cAutoLock(m_pFilter->pStateLock());

	HRESULT hr = CBaseOutputPin::BreakConnect();
	if (FAILED(hr))
		return hr;

	DisconnectDemux();

	m_pTSParserSourceFilter->m_pFileReader->CloseFile();
	m_pTSParserSourceFilter->m_pFileDuration->CloseFile();

	m_pTSBuffer->SetFileReader(m_pTSParserSourceFilter->m_pFileReader);
	{
		CAutoLock fillLock(&m_FillLock);
		m_pTSBuffer->Clear();
	}
	m_bSeeking = FALSE;
	m_rtLastSeekStart = 0;
	m_llBasePCR = -1;
	m_llNextPCR = -1;
	m_llPrevPCR = -1;
	m_lNextPCRByteOffset = 0;
	m_lPrevPCRByteOffset = 0;
	m_biMpegDemux = FALSE;

	m_DataRate = m_pTSParserSourceFilter->m_pPidParser->pids.bitrate;
	if (!m_DataRate)
		m_DataRate = 10000000;

	m_DataRateTotal = 0;
	m_BitRateCycle = 0;
	for (int i = 0; i < 256; i++) { 
		m_BitRateStore[i] = 0;
	}
	m_rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
	m_LastStartSize = 0;
	m_LastFileSize = 0;
	m_DemuxLock = FALSE;
	m_IntLastStreamTime = 0;
	m_rtTimeShiftPosition = 0;
	m_LastMultiFileStart = 0;
	m_LastMultiFileEnd = 0;

	return hr;
}

BOOL CTSParserSourcePin::checkUpdateParser(int ver)
{
	return m_pTSBuffer->CheckUpdateParser(ver);
}

HRESULT CTSParserSourcePin::FillBuffer(IMediaSample *pSample)
{
	CheckPointer(pSample, E_POINTER);

	if (m_bSeeking)
	{
		return NOERROR;
	}


	//Test if parser Locked
	if (!m_pTSParserSourceFilter->m_pPidParser->m_ParsingLock){
		//fix our pid values for this run
		m_pTSParserSourceFilter->m_pPidParser->m_ParsingLock = TRUE;
		m_pTSParserSourceFilter->m_pPidParser->pids.CopyTo(m_pPids); 
		m_bASyncModeSave = m_pTSParserSourceFilter->m_pPidParser->get_AsyncMode();
		m_PacketSave = m_pTSParserSourceFilter->m_pPidParser->get_PacketSize();
		m_PATVerSave = m_pTSParserSourceFilter->m_pPidParser->m_PATVersion;
		m_TSIDSave = m_pTSParserSourceFilter->m_pPidParser->m_TStreamID;
		m_PinTypeSave  = m_pTSParserSourceFilter->m_pPidParser->get_ProgPinMode();
		m_pTSParserSourceFilter->m_pPidParser->m_ParsingLock = FALSE;
	}

//	m_pTSBuffer->StopBufferThread();
	CAutoLock lock(&m_FillLock);

	BoostThread Boost;

	// Access the sample's data buffer
	PBYTE pData;
	LONG lDataLength;
	HRESULT hr = pSample->GetPointer(&pData);
	if (FAILED(hr))
	{
		return hr;
	}
	lDataLength = pSample->GetActualDataLength();

	m_pTSBuffer->SetFileReader(m_pTSParserSourceFilter->m_pFileReader);
	hr = m_pTSBuffer->Require(lDataLength);
	if (FAILED(hr))
	{
		return S_FALSE;
	}

//*************************************************************************************************
//Old Capture format Additions

	__int64 firstPass = m_llPrevPCR;

//	cold start
	if ((!m_pPids->pcr && m_bASyncModeSave) || !m_pTSParserSourceFilter->m_pPidParser->pidArray.Count()) {

		if (firstPass == -1) {

			m_DataRate = m_pPids->bitrate;
			m_DataRateTotal = 0;
			m_BitRateCycle = 0;
			for (int i = 0; i < 256; i++) { 
				m_BitRateStore[i] = 0;
			}
		}

		__int64 intTimeDelta = 0;
		CRefTime cTime;
		m_pTSParserSourceFilter->StreamTime(cTime);
		if (m_IntLastStreamTime > RT_2_SECOND) {

			intTimeDelta = (__int64)(REFERENCE_TIME(cTime) - m_IntLastStreamTime);

			if (intTimeDelta > 0)
			{
				__int64 bitrate = ((__int64)lDataLength * (__int64)80000000) / intTimeDelta;
				AddBitRateForAverage(bitrate);
			}
				m_pPids->bitrate = m_DataRate;
		}

		{
			CAutoLock lock(&m_SeekLock);
			m_rtStart = REFERENCE_TIME(cTime) + m_rtLastSeekStart + intTimeDelta;
		}

		//Read from buffer
		m_pTSBuffer->SetFileReader(m_pTSParserSourceFilter->m_pFileReader);
		m_pTSBuffer->DequeFromBuffer(pData, lDataLength);

		m_IntLastStreamTime = REFERENCE_TIME(cTime);
		m_llPrevPCR = REFERENCE_TIME(cTime);

		return NOERROR;
	}
//*************************************************************************************************

	if (m_llPrevPCR == -1)
	{
		Debug(TEXT("Finding the next two PCRs\n"));
		m_llBasePCR = -1;
		m_lPrevPCRByteOffset = 0;
		hr = FindNextPCR(&m_llPrevPCR, &m_lPrevPCRByteOffset, 1000000);
		if (FAILED(hr))
			Debug(TEXT("Failed to find PCR 1\n"));


		m_lNextPCRByteOffset = -1;
		if (m_lPrevPCRByteOffset < lDataLength)
		{
			m_lNextPCRByteOffset = lDataLength;
			hr = FindPrevPCR(&m_llNextPCR, &m_lNextPCRByteOffset);
			if (FAILED(hr) || (m_lNextPCRByteOffset == m_lPrevPCRByteOffset))
				m_lNextPCRByteOffset = -1;
		}

		if (m_lNextPCRByteOffset == -1)
		{
			m_lNextPCRByteOffset = m_lPrevPCRByteOffset + m_PacketSave;
			hr = FindNextPCR(&m_llNextPCR, &m_lNextPCRByteOffset, 1000000);
			if (FAILED(hr))
				Debug(TEXT("Failed to find PCR 2\n"));
		}

		m_llPCRDelta = m_llNextPCR - m_llPrevPCR;
		m_lByteDelta = m_lNextPCRByteOffset - m_lPrevPCRByteOffset;
	}

	if (m_lNextPCRByteOffset < 0)
	{

		__int64 llNextPCR = 0;
		long lNextPCRByteOffset = 0;

		lNextPCRByteOffset = lDataLength;
		hr = FindPrevPCR(&llNextPCR, &lNextPCRByteOffset);

		if (FAILED(hr))
		{
			lNextPCRByteOffset = 0;
			hr = FindNextPCR(&llNextPCR, &lNextPCRByteOffset, 1000000);
		}

		if (SUCCEEDED(hr))
		{
			m_lPrevPCRByteOffset = m_lNextPCRByteOffset;
			m_llPrevPCR = m_llNextPCR;

			m_llNextPCR = llNextPCR;
			m_lNextPCRByteOffset = lNextPCRByteOffset;

			m_llPCRDelta = m_llNextPCR - m_llPrevPCR;
			m_lByteDelta = m_lNextPCRByteOffset - m_lPrevPCRByteOffset;

			//8bits per byte and convert to sec divide by pcr duration then average it
			if ((__int64)fillFunctions.ConvertPCRtoRT(m_llPCRDelta) > 0) 
			{
				__int64 bitrate = ((__int64)m_lByteDelta * (__int64)80000000) / (__int64)fillFunctions.ConvertPCRtoRT(m_llPCRDelta);
				AddBitRateForAverage(bitrate);

				//TCHAR sz[60];
				//wsprintf(sz, TEXT("bitrate %i\n"), bitrate);
				//Debug(sz);
			}
		}
		else
		{
			Debug(TEXT("Failed to find next PCR\n"));
			{
//				BrakeThread Brake;
				Sleep(100);
			}
//			Sleep(100); //delay to reduce cpu usage.
		}
	}

	//Calculate PCR
	__int64 pcrStart;
	if (m_lByteDelta > 0)
		pcrStart = m_llPrevPCR - (__int64)((__int64)(m_llPCRDelta * (__int64)m_lPrevPCRByteOffset) / (__int64)m_lByteDelta);
	else
	{
		Debug(TEXT("Invalid byte difference. Using previous PCR\n"));
		pcrStart = m_llPrevPCR;
	}

//*********************************************************************************

	//
	//Code for inserting a PAT every sample
	//

	//If no TSID then we won't have a PAT so create one

	if (m_bInjectMode && !m_TSIDSave && !m_PinTypeSave) 
	{
		m_pTSBuffer->SetFileReader(m_pTSParserSourceFilter->m_pFileReader);
		m_pTSBuffer->DequeFromBuffer(pData, lDataLength - m_PacketSave*3);

		ULONG pos = 0; 
		REFERENCE_TIME pcrPos = -1;

		//Get the first occurance of a timing packet
		hr = S_OK;
		hr = fillFunctions.FindNextPCR(m_pTSParserSourceFilter->m_pPidParser, pData, lDataLength - m_PacketSave*3, m_pPids, &pcrPos, &pos, 1); //Get the PCR
		//If we have one then load our PAT, PMT & PCR Packets	
		if(pcrPos && hr == S_OK)
		{
			memcpy(pData+pos+m_PacketSave*3, pData+pos, lDataLength - pos - m_PacketSave*3); 

			DVBFormat::LoadPATPacket(pData + pos, m_TSIDSave, m_pPids->sid, m_pPids->pmt);
			pos+= m_PacketSave;	//shift to the pmt packet

			//Also insert a pmt if we don't have one already
			if (!m_pPids->pmt){ 
				DVBFormat::LoadPMTPacket(pData + pos,
					  m_pPids->pcr - m_pPids->opcr,
					  m_pPids->vid,
					  m_pPids->aud,
					  m_pPids->aud2,
					  m_pPids->ac3,
					  m_pPids->ac3_2,
					  m_pPids->txt);
			}
			else {
				//load another PAT instead
				DVBFormat::LoadPATPacket(pData + pos, m_TSIDSave, m_pPids->sid, m_pPids->pmt);
			}

			pos+= m_PacketSave;	//shift to the pcr packet

			//Load in our own pcr if we need to
			if (m_pPids->opcr) {
				DVBFormat::LoadPCRPacket(pData + pos, m_pPids->pcr - m_pPids->opcr, pcrPos);
			}
			else {
				//load another PAT instead
				DVBFormat::LoadPATPacket(pData + pos, m_TSIDSave, m_pPids->sid, m_pPids->pmt);
			}
		}
		else
		{
			m_pTSBuffer->SetFileReader(m_pTSParserSourceFilter->m_pFileReader);
			m_pTSBuffer->DequeFromBuffer(pData + lDataLength - m_PacketSave*3, m_PacketSave*3);
		}

	}
	else
//*********************************************************************************

	{
		//Normal Read from buffer
		m_pTSBuffer->SetFileReader(m_pTSParserSourceFilter->m_pFileReader);
		m_pTSBuffer->DequeFromBuffer(pData, lDataLength);
	}

	m_lPrevPCRByteOffset -= lDataLength;
	m_lNextPCRByteOffset -= lDataLength;


	//Checking if basePCR is set
	if (m_llBasePCR == -1)
	{
		Debug(TEXT("Setting Base PCR\n"));
#ifdef USE_EVENT
		CComPtr<IReferenceClock> pReferenceClock;
		hr = Demux::GetReferenceClock(m_pTSParserSourceFilter, &pReferenceClock);
		if (pReferenceClock != NULL)
		{
			pReferenceClock->GetTime(&m_rtStartTime);
		}
		else
		{
			Debug(TEXT("Failed to find ReferenceClock. Sending sample now\n"));
			return S_OK;
		}
#else
		m_rtStartTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * 10000);
#endif
		m_llBasePCR = pcrStart;
	}

	// Calculate next event time
	//   rtStart is set relative to the time m_llBasePCR was last set.
	//     ie. on Run() and after each seek.
	//   m_rtStart is set relative to the begining of the file.
	//     this is so that IMediaSeeking can return the current position.
	pcrStart -= m_llBasePCR;
	CRefTime rtStart;
	rtStart = 0;
	if (pcrStart > -1)
	{
		rtStart = fillFunctions.ConvertPCRtoRT(pcrStart);
		CAutoLock lock(&m_SeekLock);
		m_rtStart = rtStart + m_rtLastSeekStart;
	}

	//DEBUG: Trying to change playback rate to 10% slower. Didn't work.
	//rtStart = rtStart + (rtStart/10);

	REFERENCE_TIME rtNextTime = rtStart + m_rtStartTime - 3910;// - 391000;

	if (m_bRateControl)
	{
		//Wait if necessary
#ifdef USE_EVENT
		CComPtr<IReferenceClock> pReferenceClock;
		hr = Demux::GetReferenceClock(m_pTSParserSourceFilter, &pReferenceClock);
		if (pReferenceClock != NULL)
		{
			REFERENCE_TIME rtCurrTime;
			pReferenceClock->GetTime(&rtCurrTime);

			if (rtCurrTime < rtNextTime && rtCurrTime+1000000 > rtNextTime)
			{
				HANDLE hEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
				DWORD dwAdviseCookie = 0;
				pReferenceClock->AdviseTime(0, rtNextTime, (HEVENT)hEvent, &dwAdviseCookie);
				DWORD dwWaitResult = WaitForSingleObject(hEvent, INFINITE);
				CloseHandle(hEvent);
			}
			else if (rtCurrTime+1000000 <= rtNextTime)
			{
fillFunctions.PrintTime(TEXT("rtCurrTime:"), (__int64)rtCurrTime, 10000, &debugcount);
fillFunctions.PrintTime(TEXT("rtNextTime:"), (__int64)rtNextTime, 10000, &debugcount);
				m_rtStartTime -= (__int64)(rtNextTime - rtCurrTime);
			}
			else 
			{
				m_rtStartTime += (__int64)(rtCurrTime - rtNextTime);
				TCHAR sz[100];
				wsprintf(sz, TEXT("Bursting - late by %i (%i)\n"), rtCurrTime - rtNextTime, (pcrStart+m_llBasePCR) - m_llPrevPCR);
				Debug(sz);
			}
		}
#else
		REFERENCE_TIME rtCurrTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * 10000);

		__int64 refPCRdiff = (__int64)((__int64)rtNextTime - (__int64)rtCurrTime);

		//Loop until current time passes calculated current time
		while(rtCurrTime < rtNextTime)
		{
			refPCRdiff = (__int64)((__int64)rtNextTime - (__int64)rtCurrTime);
			refPCRdiff = refPCRdiff / 100000;	//sleep for a tenth of the time
			if (refPCRdiff == 0)	//break out if the sleep is really short
				break;

			Sleep((DWORD)(refPCRdiff)); // Delay it

			//Update current time
			rtCurrTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * 10000);
		}
#endif
	}

/*
#if DEBUG
	{
		CAutoLock lock(&m_SeekLock);
		TCHAR sz[100];
		long duration1 = m_pTSParserSourceFilter->m_pPidParser->pids.dur / (__int64)RT_SECOND;
		long duration2 = m_pTSParserSourceFilter->m_pPidParser->pids.dur % (__int64)RT_SECOND;
		long start1 = m_rtStart.m_time / (__int64)RT_SECOND;
		long start2 = m_rtStart.m_time % (__int64)RT_SECOND;
		long stop1 = m_rtStop.m_time / (__int64)RT_SECOND;
		long stop2 = m_rtStop.m_time % (__int64)RT_SECOND;
		wsprintf(sz, TEXT("\t\t\tduration %10i.%07i\t\tstart %10i.%07i\t\tstop %10i.%07i\n"), duration1, duration2, start1, start2, stop1, stop2);
		Debug(sz);
	}
#endif
*/
	//Set sample time
	//pSample->SetTime(&rtStart, &rtStart);
//PrintTime(TEXT("FillBuffer"), (__int64)m_rtLastCurrentTime, 10000);

//*************************************************************************************************
//Old Capture format Additions

	//If no TSID then we won't have a PAT so create one
	if (m_bInjectMode && FALSE && !m_TSIDSave && firstPass == -1 && !m_PinTypeSave)
	{
		ULONG pos = 0; 
		REFERENCE_TIME pcrPos = -1;

		//Get the first occurance of a timing packet
		hr = S_OK;
		hr = fillFunctions.FindNextPCR(m_pTSParserSourceFilter->m_pPidParser, pData, lDataLength, m_pPids, &pcrPos, &pos, 1); //Get the PCR
		//If we have one then load our PAT, PMT & PCR Packets	
		if(pcrPos && hr == S_OK) {
			//Check if we can back up before timing packet	
			if (pos > (ULONG)m_PacketSave*2) {
			//back up before timing packet
				pos-= m_PacketSave*2;	//shift back to before pat & pmt packet
			}
		}
		else {
			// if no timing packet found then load the PAT at the start of the buffer
			pos=0;
		}

		DVBFormat::LoadPATPacket(pData + pos, m_TSIDSave, m_pPids->sid, m_pPids->pmt);
		pos+= m_PacketSave;	//shift to the pmt packet

		//Also insert a pmt if we don't have one already
		if (!m_pPids->pmt){ 
			DVBFormat::LoadPMTPacket(pData + pos,
				  m_pPids->pcr - m_pPids->opcr,
				  m_pPids->vid,
				  m_pPids->aud,
				  m_pPids->aud2,
				  m_pPids->ac3,
				  m_pPids->ac3_2,
				  m_pPids->txt);
		}
		else {
			//load another PAT instead
			DVBFormat::LoadPATPacket(pData + pos, m_TSIDSave, m_pPids->sid, m_pPids->pmt);
		}

		pos+= m_PacketSave;	//shift to the pcr packet

		//Load in our own pcr if we need to
		if (m_pPids->opcr) {
			DVBFormat::LoadPCRPacket(pData + pos, m_pPids->pcr - m_pPids->opcr, pcrPos);
		}

	}
/*
	//If we need to insert a new PCR packet
	if (m_pPids->opcr) {

		ULONG pos = 0, lastpos = 0;
		REFERENCE_TIME pcrPos = -1;
		hr = S_OK;
		while (hr == S_OK) {
			//Look for a timing packet
			hr = findNextOPCR(m_pTSParserSourceFilter->m_pPidParser, pData, lDataLength, m_pPids, &pcrPos, &pos, 1); //Get the PCR
			if (pcrPos) {
				//Insert our new PCR Packet
				LoadPCRPacket(pData + pos, m_pPids->pcr - m_pPids->opcr, pcrPos);
//PrintTime(TEXT("Insert PCR packet"), (__int64) pcrPos, 90);
//				break;
			}
			pos += m_PacketSave;

			if (pos > lastpos + 10*packet && pos + m_PacketSave < lDataLength){
//PrintTime(TEXT("delta PCR packet"), (__int64) m_llPCRDelta, 90);
				__int64 offset = (__int64)(m_llPCRDelta *(__int64)(pos - lastpos) / (__int64)m_lByteDelta);
//PrintTime(TEXT("offset PCR packet"), (__int64) offset, 90);
				LoadPCRPacket(pData + pos, m_pPids->pcr - m_pPids->opcr, pcrPos + offset);
				pos += m_PacketSave;
				lastpos = pos;
			}
		};
	}
*/
//*************************************************************************************************
	m_IntCurrentTimePCR = m_llPrevPCR;
	m_pTSParserSourceFilter->m_pPidParser->pids.bitrate = m_DataRate;

//	m_pTSBuffer->StartBufferThread();

	return NOERROR;
}

HRESULT CTSParserSourcePin::OnThreadStartPlay( )
{
	m_currPosition = m_pTSParserSourceFilter->m_pFileReader->getFilePointer();
	m_rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
	m_llPrevPCR = -1;
	m_pTSBuffer->SetFileReader(m_pTSParserSourceFilter->m_pFileReader);
	{
		CAutoLock fillLock(&m_FillLock);
		m_pTSBuffer->Clear();
	}
	m_DataRate = m_pTSParserSourceFilter->m_pPidParser->pids.bitrate;
	debugcount = 0;
	m_rtTimeShiftPosition = 0;
	m_LastMultiFileStart = 0;
	m_LastMultiFileEnd = 0;
	m_DataRate = m_pTSParserSourceFilter->m_pPidParser->pids.bitrate;

	CAutoLock lock(&m_SeekLock);
	//Test if parser Locked
	if (!m_pTSParserSourceFilter->m_pPidParser->m_ParsingLock){
		//fix our pid values for this run
		m_pTSParserSourceFilter->m_pPidParser->m_ParsingLock = TRUE;
		m_pTSParserSourceFilter->m_pPidParser->pids.CopyTo(m_pPids); 
		m_bASyncModeSave = m_pTSParserSourceFilter->m_pPidParser->get_AsyncMode();
		m_PacketSave = m_pTSParserSourceFilter->m_pPidParser->get_PacketSize();
		m_PATVerSave = m_pTSParserSourceFilter->m_pPidParser->m_PATVersion;
		m_TSIDSave = m_pTSParserSourceFilter->m_pPidParser->m_TStreamID;
		m_PinTypeSave  = m_pTSParserSourceFilter->m_pPidParser->get_ProgPinMode();
		m_pTSParserSourceFilter->m_pPidParser->m_ParsingLock = FALSE;
	}

    DeliverNewSegment(m_rtStart, m_rtStop, 1.0 );
	m_rtLastSeekStart = REFERENCE_TIME(m_rtStart);
	m_rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);

	return CSourceStream::OnThreadStartPlay( );
}

HRESULT CTSParserSourcePin::Run(REFERENCE_TIME tStart)
{
	CAutoLock fillLock(&m_FillLock);
	CAutoLock seekLock(&m_SeekLock);

	//Test if parser Locked
	if (!m_pTSParserSourceFilter->m_pPidParser->m_ParsingLock){
		//fix our pid values for this run
		m_pTSParserSourceFilter->m_pPidParser->m_ParsingLock = TRUE;
		m_pTSParserSourceFilter->m_pPidParser->pids.CopyTo(m_pPids); 
		m_bASyncModeSave = m_pTSParserSourceFilter->m_pPidParser->get_AsyncMode();
		m_PacketSave = m_pTSParserSourceFilter->m_pPidParser->get_PacketSize();
		m_PATVerSave = m_pTSParserSourceFilter->m_pPidParser->m_PATVersion;
		m_TSIDSave = m_pTSParserSourceFilter->m_pPidParser->m_TStreamID;
		m_PinTypeSave  = m_pTSParserSourceFilter->m_pPidParser->get_ProgPinMode();
		m_pTSParserSourceFilter->m_pPidParser->m_ParsingLock = FALSE;
	}

	CBasePin::m_tStart = tStart;
	BOOL bTimeMode;
	BOOL bTimeShifting = IsTimeShifting(m_pTSParserSourceFilter->m_pFileReader, &bTimeMode);
	if (bTimeMode)
	{
		m_rtStart = CBasePin::m_tStart;//m_rtLastSeekStart;
		m_pTSParserSourceFilter->ResetStreamTime();
	}
	else
		m_rtLastSeekStart = REFERENCE_TIME(m_rtStart);

	m_rtTimeShiftPosition = 0;
	m_LastMultiFileStart = 0;
	m_LastMultiFileEnd = 0;

	if (!m_bSeeking && !m_DemuxLock)
	{	
		CComPtr<IReferenceClock> pClock;
		Demux::GetReferenceClock(m_pTSParserSourceFilter, &pClock);
		SetDemuxClock(pClock);
//		m_pTSParserSourceFilter->NotifyEvent(EC_CLOCK_UNSET, NULL, NULL);
	}
	return CBaseOutputPin::Run(tStart);
}

STDMETHODIMP CTSParserSourcePin::GetCurrentPosition(LONGLONG *pCurrent)
{
//::OutputDebugString(TEXT("GetCurrentPosition In\n"));
	if (pCurrent)
	{
		CAutoLock seekLock(&m_SeekLock);

		//Get the FileReader Type
		WORD bMultiMode;
		m_pTSParserSourceFilter->m_pFileReader->get_ReaderMode(&bMultiMode);
		//Do MultiFile timeshifting mode
		if (m_bGetAvailableMode && bMultiMode)
		{
			*pCurrent = max(0, (__int64)positionFunctions.SubConvertPCRtoRT(m_IntCurrentTimePCR, m_IntBaseTimePCR));
//			*pCurrent = max(0, (__int64)positionFunctions.ConvertPCRtoRT(m_IntCurrentTimePCR));
			REFERENCE_TIME current, stop;
			return CSourceSeeking::GetPositions(&current, &stop);
		}

		REFERENCE_TIME stop;
		return GetPositions(pCurrent, &stop);
	}
	return CSourceSeeking::GetCurrentPosition(pCurrent);
}

STDMETHODIMP CTSParserSourcePin::GetPositions(LONGLONG *pCurrent, LONGLONG *pStop)
{
//::OutputDebugString(TEXT("GetPositions In\n"));
	if (pCurrent)
	{
		CAutoLock seekLock(&m_SeekLock);
		//Get the FileReader Type
		WORD bMultiMode;
		m_pTSParserSourceFilter->m_pFileReader->get_ReaderMode(&bMultiMode);

		//Do MultiFile timeshifting mode
		if(bMultiMode)
		{
			if (m_bGetAvailableMode)
			{
				*pCurrent = max(0, (__int64)positionFunctions.SubConvertPCRtoRT(m_IntCurrentTimePCR, m_IntBaseTimePCR));
				*pStop = max(0, (__int64)positionFunctions.SubConvertPCRtoRT(m_IntEndTimePCR, m_IntBaseTimePCR));
				REFERENCE_TIME current, stop;
				return CSourceSeeking::GetPositions(&current, &stop);
			}
			else
			{
				*pCurrent = max(0, (__int64)positionFunctions.SubConvertPCRtoRT(m_IntCurrentTimePCR, m_IntStartTimePCR));
				*pStop = max(0, (__int64)positionFunctions.SubConvertPCRtoRT(m_IntEndTimePCR, m_IntStartTimePCR));
				REFERENCE_TIME current, stop;
				return CSourceSeeking::GetPositions(&current, &stop);
			}
		}
		else
		{
			BOOL bTimeMode;
			BOOL bTimeShifting = IsTimeShifting(m_pTSParserSourceFilter->m_pFileReader, &bTimeMode);
			if (bTimeMode)
				*pCurrent = (REFERENCE_TIME)m_rtTimeShiftPosition;
			else
			{
				CRefTime cTime;
				m_pTSParserSourceFilter->StreamTime(cTime);
				*pCurrent = (REFERENCE_TIME)(m_rtLastSeekStart + REFERENCE_TIME(cTime));
			}
			REFERENCE_TIME current;
			return CSourceSeeking::GetPositions(&current, pStop);
		}
	}
	return CSourceSeeking::GetPositions(pCurrent, pStop);
}

STDMETHODIMP CTSParserSourcePin::SetPositions(LONGLONG *pCurrent, DWORD CurrentFlags
			     , LONGLONG *pStop, DWORD StopFlags)
{
//::OutputDebugString(TEXT("SetPositions In\n"));
	if(!m_rtDuration)
		return E_FAIL;

	BOOL bFileWasOpen = TRUE;
	if (m_pTSParserSourceFilter->m_pFileReader->IsFileInvalid())
	{
		HRESULT hr = m_pTSParserSourceFilter->m_pFileReader->OpenFile();
		if (FAILED(hr))
			return hr;

		bFileWasOpen = FALSE;
	}

	if (pCurrent)
	{
		//Get the FileReader Type
		WORD bMultiMode;
		m_pTSParserSourceFilter->m_pFileReader->get_ReaderMode(&bMultiMode);

		//Do MultiFile timeshifting mode
		if (m_bGetAvailableMode && bMultiMode)
		{
			REFERENCE_TIME rtStop = *pStop;
			REFERENCE_TIME rtCurrent = *pCurrent;
			if (CurrentFlags & AM_SEEKING_RelativePositioning)
			{
				CAutoLock lock(&m_SeekLock);
				rtCurrent += m_rtStart;
				CurrentFlags -= AM_SEEKING_RelativePositioning; //Remove relative flag
				CurrentFlags += AM_SEEKING_AbsolutePositioning; //Replace with absoulute flag
			}
			else
				rtCurrent = max(0, (__int64)((__int64)*pCurrent - max(0,(__int64)(seekFunctions.SubConvertPCRtoRT(m_IntStartTimePCR, m_IntBaseTimePCR)))));

			if (StopFlags & AM_SEEKING_RelativePositioning)
			{
				CAutoLock lock(&m_SeekLock);
				rtStop += m_rtStop;
				StopFlags -= AM_SEEKING_RelativePositioning; //Remove relative flag
				StopFlags += AM_SEEKING_AbsolutePositioning; //Replace with absoulute flag
			}
			else
				rtStop = max(0, (__int64)((__int64)*pStop - max(0,(__int64)(seekFunctions.SubConvertPCRtoRT(m_IntStartTimePCR, m_IntBaseTimePCR)))));

			HRESULT hr = setPositions(&rtCurrent, CurrentFlags, &rtStop, StopFlags);

//			if (!bFileWasOpen)
//				m_pTSParserSourceFilter->m_pFileReader->CloseFile();

			if (CurrentFlags & AM_SEEKING_ReturnTime)
				*pCurrent  = rtCurrent + max(0,(__int64)(seekFunctions.SubConvertPCRtoRT(m_IntStartTimePCR, m_IntBaseTimePCR)));

			if (StopFlags & AM_SEEKING_ReturnTime)
				*pStop  = rtStop + max(0,(__int64)(seekFunctions.SubConvertPCRtoRT(m_IntStartTimePCR, m_IntBaseTimePCR)));

			return hr;
		}
		return setPositions(pCurrent, CurrentFlags, pStop, StopFlags);
	}
	return CSourceSeeking::SetPositions(pCurrent, CurrentFlags, pStop, StopFlags);
}

HRESULT CTSParserSourcePin::setPositions(LONGLONG *pCurrent, DWORD CurrentFlags
			     , LONGLONG *pStop, DWORD StopFlags)
{
//::OutputDebugString(TEXT("setPositions In\n"));
//PrintTime(TEXT("setPositions"), (__int64) *pCurrent, 10000);
	if(!m_rtDuration)
		return E_FAIL;

	if (pCurrent)
	{
/*		WORD readonly = 0;
		m_pTSParserSourceFilter->m_pFileReader->get_ReadOnly(&readonly);
		if (readonly) {
			//wait for the Length Changed Event to complete
			REFERENCE_TIME rtCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
			while ((REFERENCE_TIME)(m_rtLastCurrentTime + (REFERENCE_TIME)MIN_FILE_SIZE) > rtCurrentTime) {
				rtCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
			}
		}
*/
		REFERENCE_TIME rtCurrent = *pCurrent;
		if (CurrentFlags & AM_SEEKING_RelativePositioning)
		{
//PrintTime(TEXT("setPositions/RelativePositioning"), (__int64) *pCurrent, 10000);
			CAutoLock lock(&m_SeekLock);
			rtCurrent += m_rtStart;
			CurrentFlags -= AM_SEEKING_RelativePositioning; //Remove relative flag
			CurrentFlags += AM_SEEKING_AbsolutePositioning; //Replace with absoulute flag
		}

		if (CurrentFlags & AM_SEEKING_PositioningBitsMask)
		{
//PrintTime(TEXT("setPositions/PositioningBitsMask"), (__int64) *pCurrent, 10000);
			CAutoLock lock(&m_SeekLock);
			m_rtStart = rtCurrent;
		}

		if (!(CurrentFlags & AM_SEEKING_NoFlush) && (CurrentFlags & AM_SEEKING_PositioningBitsMask))
		{
			m_bSeeking = TRUE;

			if(m_pTSParserSourceFilter->is_Active() && !m_DemuxLock)
				SetDemuxClock(NULL);

			//Test if parser Locked
			if (!m_pTSParserSourceFilter->m_pPidParser->m_ParsingLock){
				//fix our pid values for this run
				m_pTSParserSourceFilter->m_pPidParser->m_ParsingLock = TRUE;
				m_pTSParserSourceFilter->m_pPidParser->pids.CopyTo(m_pPids); 
				m_bASyncModeSave = m_pTSParserSourceFilter->m_pPidParser->get_AsyncMode();
				m_PacketSave = m_pTSParserSourceFilter->m_pPidParser->get_PacketSize();
				m_PATVerSave = m_pTSParserSourceFilter->m_pPidParser->m_PATVersion;
				m_TSIDSave = m_pTSParserSourceFilter->m_pPidParser->m_TStreamID;
				m_PinTypeSave  = m_pTSParserSourceFilter->m_pPidParser->get_ProgPinMode();
				m_pTSParserSourceFilter->m_pPidParser->m_ParsingLock = FALSE;
			}
			m_LastMultiFileStart = 0;
			m_LastMultiFileEnd = 0;

			if (m_pTSParserSourceFilter->IsActive())
				DeliverBeginFlush();
			CSourceStream::Stop();
			m_DataRate = m_pTSParserSourceFilter->m_pPidParser->pids.bitrate;
			m_llPrevPCR = -1;

			m_pTSBuffer->SetFileReader(m_pTSParserSourceFilter->m_pFileReader);
			m_pTSBuffer->Clear();
			SetAccuratePos(rtCurrent);
			if (CurrentFlags & AM_SEEKING_PositioningBitsMask)
			{
				CAutoLock lock(&m_SeekLock);
				m_rtStart = rtCurrent;
			}
			m_rtLastSeekStart = rtCurrent;
			if (m_pTSParserSourceFilter->IsActive())
				DeliverEndFlush();
			CSourceStream::Run();
			if (CurrentFlags & AM_SEEKING_ReturnTime)
				*pCurrent  = rtCurrent;

//			CAutoLock lock(&m_SeekLock);
			return CSourceSeeking::SetPositions(&rtCurrent, CurrentFlags, pStop, StopFlags);
		}
		if (CurrentFlags & AM_SEEKING_ReturnTime)
			*pCurrent  = rtCurrent;

		return CSourceSeeking::SetPositions(&rtCurrent, CurrentFlags, pStop, StopFlags);
	}
	return CSourceSeeking::SetPositions(pCurrent, CurrentFlags, pStop, StopFlags);
}


STDMETHODIMP CTSParserSourcePin::GetAvailable(LONGLONG *pEarliest, LONGLONG *pLatest)
{
//::OutputDebugString(TEXT("GetAvailable In\n"));
	m_bGetAvailableMode = TRUE;

	CAutoLock seekLock(&m_SeekLock);
	CheckPointer(pEarliest,E_POINTER);
	CheckPointer(pLatest,E_POINTER);

	if(!m_pTSParserSourceFilter->m_pFileReader)
	{
		if (m_rtDuration)
		{
			*pEarliest = max(0,(__int64)(positionFunctions.SubConvertPCRtoRT(m_IntStartTimePCR, m_IntBaseTimePCR)));
			*pLatest = max(0,(__int64)(positionFunctions.SubConvertPCRtoRT(m_IntEndTimePCR, m_IntBaseTimePCR)));
			return S_OK;
		}
		else
			return CSourceSeeking::GetAvailable(pEarliest, pLatest);


//		return CSourceSeeking::GetAvailable(pEarliest, pLatest);
	}

	//Get the FileReader Type
	WORD bMultiMode;
	m_pTSParserSourceFilter->m_pFileReader->get_ReaderMode(&bMultiMode);

	//Do MultiFile timeshifting mode
	if(bMultiMode)
	{
		*pEarliest = max(0,(__int64)(positionFunctions.SubConvertPCRtoRT(m_IntStartTimePCR, m_IntBaseTimePCR)));
		*pLatest = max(0,(__int64)(positionFunctions.SubConvertPCRtoRT(m_IntEndTimePCR, m_IntBaseTimePCR)));;
		return S_OK;
	}
	return CSourceSeeking::GetAvailable(pEarliest, pLatest);
}

HRESULT CTSParserSourcePin::ChangeStart()
{
//	UpdateFromSeek(TRUE);
	m_bSeeking = FALSE;
    return S_OK;
}

HRESULT CTSParserSourcePin::ChangeStop()
{
//	UpdateFromSeek();
	m_bSeeking = FALSE;
    return S_OK;
}

HRESULT CTSParserSourcePin::ChangeRate()
{
	REFERENCE_TIME start, stop;
	GetPositions(&start, &stop);
	IMediaSeeking *pMediaSeeking;
	if(m_pTSParserSourceFilter->GetFilterGraph() && SUCCEEDED(m_pTSParserSourceFilter->GetFilterGraph()->QueryInterface(IID_IMediaSeeking, (void **) &pMediaSeeking)))
	{
		pMediaSeeking->GetPositions(&start, &stop);
		pMediaSeeking->Release();
	}

    {   // Scope for critical section lock.
        CAutoLock cAutoLockSeeking(CSourceSeeking::m_pLock);
        if( m_dRateSeeking <= 0 ) {
            m_dRateSeeking = 1.0;  // Reset to a reasonable value.
            return E_INVALIDARG;
        }

		DeliverBeginFlush();
		m_pTSParserSourceFilter->m_pClock->SetClockRate(m_dRateSeeking);
		DeliverEndFlush();
    }

	if(m_pTSParserSourceFilter->GetFilterGraph() && SUCCEEDED(m_pTSParserSourceFilter->GetFilterGraph()->QueryInterface(IID_IMediaSeeking, (void **) &pMediaSeeking)))
	{
		pMediaSeeking->SetPositions(&start, AM_SEEKING_AbsolutePositioning , &stop, AM_SEEKING_NoPositioning);
		pMediaSeeking->Release();
	}

	//    UpdateFromSeek();
    return S_OK;
}

void CTSParserSourcePin::ClearBuffer(void)
{
	CAutoLock fillLock(&m_FillLock);
	m_pTSBuffer->SetFileReader(m_pTSParserSourceFilter->m_pFileReader);
	m_pTSBuffer->Clear();
}

void CTSParserSourcePin::UpdateFromSeek(BOOL updateStartPosition)
{
	if (CAMThread::ThreadExists())
	{	
		m_bSeeking = TRUE;
		CAutoLock fillLock(&m_FillLock);
		CAutoLock seekLock(&m_SeekLock);
		if(m_pTSParserSourceFilter->is_Active() && !m_DemuxLock)
			SetDemuxClock(NULL);
		DeliverBeginFlush();
		m_llPrevPCR = -1;
		if (updateStartPosition == TRUE)
		{
			m_pTSBuffer->SetFileReader(m_pTSParserSourceFilter->m_pFileReader);
			m_pTSBuffer->Clear();
			SetAccuratePos(m_rtStart);
			//m_pTSParserSourceFilter->FileSeek(m_rtStart);
			m_rtLastSeekStart = REFERENCE_TIME(m_rtStart);
		}
		DeliverEndFlush();
	}
	m_bSeeking = FALSE;
}


HRESULT CTSParserSourcePin::SetAccuratePos(REFERENCE_TIME seektime)
{
seekFunctions.PrintTime(TEXT("seekin"), (__int64) seektime, 10000, &debugcount);
	BoostThread Boost;

	HRESULT hr;

	//Set the file pointer as quick as possible incase the thread ends prior to our seek finishing
	__int64 fileStart, filelength = 0;
	m_pTSParserSourceFilter->m_pFileReader->GetFileSize(&fileStart, &filelength);
	__int64 nFileIndex = 0;
	if (m_pTSParserSourceFilter->m_pPidParser->pids.dur>>14)
		nFileIndex = filelength * (__int64)(seektime>>14) / (__int64)(m_pTSParserSourceFilter->m_pPidParser->pids.dur>>14);

	nFileIndex = (__int64)max(m_pTSParserSourceFilter->m_pPidParser->get_StartOffset(), (__int64)nFileIndex);
	m_pTSParserSourceFilter->m_pFileReader->setFilePointer((__int64)(nFileIndex - filelength), FILE_END);
	m_currPosition = m_pTSParserSourceFilter->m_pFileReader->getFilePointer();

	//Return as quick as possible if cold starting
	if(filelength < MIN_FILE_SIZE)
		return S_OK;

	nFileIndex = 0;
	m_pTSParserSourceFilter->ResetStreamTime();
	m_rtStart = seektime;
	m_rtLastSeekStart = REFERENCE_TIME(m_rtStart);
	m_rtTimeShiftPosition = seektime;

	WORD bMultiMode;
	m_pTSParserSourceFilter->m_pFileReader->get_ReaderMode(&bMultiMode);

	//Do MultiFile timeshifting mode
	if(bMultiMode)
	{
		if (m_bGetAvailableMode && ((__int64)(seektime + (__int64)RT_2_SECOND) > m_pTSParserSourceFilter->m_pPidParser->pids.dur))
		{
			seektime = max(0, m_pTSParserSourceFilter->m_pPidParser->pids.dur -(__int64)RT_2_SECOND);
			m_rtStart = seektime;
			m_rtLastSeekStart = REFERENCE_TIME(m_rtStart);
			m_rtTimeShiftPosition = seektime;
		}
	}
	else
	{
		BOOL bTimeMode;
		BOOL timeShifting = IsTimeShifting(m_pTSParserSourceFilter->m_pFileReader, &bTimeMode);

		//Prevent the filtergraph clock from approaching the end time
		if (bTimeMode && ((__int64)(seektime + (__int64)RT_2_SECOND*2) > m_pTSParserSourceFilter->m_pPidParser->pids.dur))
			seektime = max(0, m_pTSParserSourceFilter->m_pPidParser->pids.dur -(__int64)RT_2_SECOND*2);
	}

//***********************************************************************************************
//Old Capture format Additions

	if (!m_pTSParserSourceFilter->m_pPidParser->pids.pcr && m_pTSParserSourceFilter->m_pPidParser->get_AsyncMode()) {
//	if (!m_pPidParser->pids.pcr && !m_pPidParser->get_ProgPinMode()) {
		// Revert to old method
		// shifting right by 14 rounds the seek and duration time down to the
		// nearest multiple 16.384 ms. More than accurate enough for our seeks.
		nFileIndex = 0;

		if (m_pTSParserSourceFilter->m_pPidParser->pids.dur>>14)
			nFileIndex = filelength * (__int64)(seektime>>14) / (__int64)(m_pTSParserSourceFilter->m_pPidParser->pids.dur>>14);

		nFileIndex = (__int64)max(m_pTSParserSourceFilter->m_pPidParser->get_StartOffset(), (__int64)nFileIndex);
		m_pTSParserSourceFilter->m_pFileReader->setFilePointer((__int64)(nFileIndex - filelength), FILE_END);
		m_currPosition = m_pTSParserSourceFilter->m_pFileReader->getFilePointer();

		m_IntCurrentTimePCR = m_IntStartTimePCR + (__int64)((__int64)((__int64)seektime * (__int64)9) / (__int64)1000);
		return S_OK;
	}
//***********************************************************************************************

////Multireader fixed 	__int64	pcrDuration = (__int64)((__int64)((__int64)m_pTSParserSourceFilter->m_pPidParser->pids.dur * (__int64)9) / (__int64)1000);
	__int64	pcrDuration = (__int64)max(0, (__int64)((__int64)m_pTSParserSourceFilter->m_pPidParser->pids.end - (__int64)m_pTSParserSourceFilter->m_pPidParser->pids.start));
	if ((__int64)m_pTSParserSourceFilter->m_pPidParser->pids.end < (__int64)m_pTSParserSourceFilter->m_pPidParser->pids.start)
		pcrDuration = (__int64)((__int64)((__int64)m_pTSParserSourceFilter->m_pPidParser->pids.dur * (__int64)9) / (__int64)1000);
seekFunctions.PrintTime(TEXT("our pcr pid.start time for reference"), (__int64)m_pTSParserSourceFilter->m_pPidParser->pids.start, 90, &debugcount);
seekFunctions.PrintTime(TEXT("our pcr pid.end time for reference"), (__int64)m_pTSParserSourceFilter->m_pPidParser->pids.end, 90, &debugcount);
seekFunctions.PrintTime(TEXT("our pcr duration"), (__int64)pcrDuration, 90, &debugcount);

	//Get estimated time of seek as pcr 
	__int64	pcrDeltaSeekTime = (__int64)((__int64)((__int64)seektime * (__int64)9) / (__int64)1000);
seekFunctions.PrintTime(TEXT("our pcr Delta SeekTime"), (__int64)pcrDeltaSeekTime, 90, &debugcount);

//This is where we create a pcr time relative to the current stream position
//
	ULONG ulBytesRead = 0;
	long lDataLength = (long)min(filelength, MIN_FILE_SIZE*2);//1000000;
	PBYTE pData = new BYTE[MIN_FILE_SIZE*2];
	//Set Pointer to end of file to get end pcr
	m_pTSParserSourceFilter->m_pFileReader->setFilePointer((__int64) - ((__int64)lDataLength), FILE_END);
	hr = m_pTSParserSourceFilter->m_pFileReader->Read(pData, lDataLength, &ulBytesRead);
//	if (ulBytesRead != lDataLength)
	if (FAILED(hr))
	{
seekFunctions.PrintTime(TEXT("File Read Call failed"), (__int64)ulBytesRead, 90, &debugcount);

		delete[] pData;
		return S_FALSE;
	}

	ULONG posSavefirst = m_pTSParserSourceFilter->m_pPidParser->get_PacketSize();
	__int64 pcrFirstEndPos = 0;
	hr = S_OK;
	hr = seekFunctions.FindNextPCR(m_pTSParserSourceFilter->m_pPidParser, pData, ulBytesRead, &m_pTSParserSourceFilter->m_pPidParser->pids, &pcrFirstEndPos, &posSavefirst, 1); //Get the PCR
seekFunctions.PrintTime(TEXT("our pcr first end time for the pcr rate calculation"), (__int64)pcrFirstEndPos, 90, &debugcount);

	ULONG pos = 0;
	__int64 pcrEndPos = 0;
	pos = ulBytesRead - m_pTSParserSourceFilter->m_pPidParser->get_PacketSize();
	hr = S_OK;
	hr = seekFunctions.FindNextPCR(m_pTSParserSourceFilter->m_pPidParser, pData, ulBytesRead, &m_pTSParserSourceFilter->m_pPidParser->pids, &pcrEndPos, &pos, -1); //Get the PCR
seekFunctions.PrintTime(TEXT("our pcr end time for reference"), (__int64)pcrEndPos, 90, &debugcount);

	__int64	pcrSeekTime = pcrDeltaSeekTime + (__int64)(pcrEndPos - pcrDuration);
seekFunctions.PrintTime(TEXT("our predicted pcr position for seek"), (__int64)pcrSeekTime, 90, &debugcount);

	//Test if we have a pcr or if the pcr is less than rollover time
	if (FAILED(hr) || pcrEndPos == 0 || pcrSeekTime < 0) {
seekFunctions.PrintTime(TEXT("get lastpcr failed now using first pcr"), (__int64)m_IntStartTimePCR, 90, &debugcount);
	
		//Set seektime to position relative to first pcr
		pcrSeekTime = m_IntStartTimePCR + pcrDeltaSeekTime;

		//test if pcr time is now larger than file size
		if (pcrSeekTime > m_IntEndTimePCR || pcrEndPos == 0) {
//		if (pcrSeekTime > pcrDuration) {
seekFunctions.PrintTime(TEXT("get first pcr failed as well SEEK ERROR AT START"), (__int64) pcrSeekTime, 90, &debugcount);

			// Revert to old method
			// shifting right by 14 rounds the seek and duration time down to the
			// nearest multiple 16.384 ms. More than accurate enough for our seeks.
			nFileIndex = 0;

			if (m_pTSParserSourceFilter->m_pPidParser->pids.dur>>14)
				nFileIndex = filelength * (__int64)(seektime>>14) / (__int64)(m_pTSParserSourceFilter->m_pPidParser->pids.dur>>14);

			nFileIndex = (__int64)max(m_pTSParserSourceFilter->m_pPidParser->get_StartOffset(), (__int64)nFileIndex);
			m_pTSParserSourceFilter->m_pFileReader->setFilePointer((__int64)(nFileIndex - filelength), FILE_END);
			m_currPosition = m_pTSParserSourceFilter->m_pFileReader->getFilePointer();

			m_IntCurrentTimePCR = m_IntStartTimePCR + (__int64)((__int64)((__int64)seektime * (__int64)9) / (__int64)1000);
			delete[] pData;
			return S_OK;
		}
	}









	//Calculate our byte data rate from the last part of the file
	__int64 pcrByteRate = (__int64)((__int64)m_DataRate / (__int64)720);
TCHAR sz[128];
wsprintf(sz, TEXT("our pcr Byte Rate :%lu\n"), pcrByteRate);
::OutputDebugString(sz);
	if (pcrFirstEndPos && pcrEndPos && pcrEndPos > pcrFirstEndPos)
	{
		pcrByteRate = ((__int64)(pos - posSavefirst)* 1000/ (__int64)(pcrEndPos - pcrFirstEndPos));
wsprintf(sz, TEXT("our recalculated pcr Byte Rate :%lu\n"), pcrByteRate);
::OutputDebugString(sz);
	}















	//create our predicted file pointer position
	nFileIndex = (pcrDeltaSeekTime / (__int64)1000) * pcrByteRate;

	// set back so we can get last batch of data if at end of file
	if ((__int64)(nFileIndex + (__int64)lDataLength) > filelength)
		nFileIndex = (__int64)(filelength - (__int64)lDataLength);


	//Set Pointer to the predicted file position to get end pcr
	nFileIndex = max(0, nFileIndex);
	m_pTSParserSourceFilter->m_pFileReader->setFilePointer((__int64)(nFileIndex - filelength), FILE_END);
	m_pTSParserSourceFilter->m_pFileReader->Read(pData, lDataLength, &ulBytesRead);
	__int64 pcrPos;
	pos = 0;

	hr = S_OK;
	hr = seekFunctions.FindNextPCR(m_pTSParserSourceFilter->m_pPidParser, pData, ulBytesRead, &m_pTSParserSourceFilter->m_pPidParser->pids, &pcrPos, &pos, 1);
	nFileIndex += (__int64)pos;

	//compare our predicted file position to our predicted seektime and adjust file pointer
	if (pcrPos > pcrSeekTime) {
		nFileIndex -= (__int64)((__int64)((__int64)(pcrPos - pcrSeekTime) / (__int64)1000) * (__int64)pcrByteRate);
seekFunctions.PrintTime(TEXT("seek---------"), (__int64) pcrPos, 90, &debugcount);
	}
	else if (pcrSeekTime > pcrPos) {
			nFileIndex += (__int64)((__int64)((__int64)(pcrSeekTime - pcrPos) / (__int64)1000) * (__int64)pcrByteRate);
seekFunctions.PrintTime(TEXT("seek+++++++++++++"), (__int64) pcrPos, 90, &debugcount);
	}

//	lDataLength = min(filelength, MIN_FILE_SIZE*2);//1000000;

	//Now we are close so setup the a +/- 2meg buffer
	nFileIndex -= (__int64)(lDataLength / 2); //Centre buffer

	// set buffer start back from EOF so we can get last batch of data
	if ((nFileIndex + lDataLength) > filelength)
		nFileIndex = (__int64)(filelength - (__int64)lDataLength);

	nFileIndex = max(0, nFileIndex);
	m_pTSParserSourceFilter->m_pFileReader->setFilePointer((__int64)(nFileIndex - filelength), FILE_END);
	ulBytesRead = 0;
	m_pTSParserSourceFilter->m_pFileReader->Read(pData, lDataLength, &ulBytesRead);

	pcrPos = 0;
	pos = ulBytesRead / 2;//buffer the centre search

	hr = S_OK;		
	while (pcrSeekTime > pcrPos && hr == S_OK) {
		//Seek forwards
		pos += m_pTSParserSourceFilter->m_pPidParser->get_PacketSize();
		hr = seekFunctions.FindNextPCR(m_pTSParserSourceFilter->m_pPidParser, pData, ulBytesRead, &m_pTSParserSourceFilter->m_pPidParser->pids, &pcrPos, &pos, 1); //Get the PCR
seekFunctions.PrintTime(TEXT("seekfwdloop"), (__int64) pcrPos, 90, &debugcount);
	}
seekFunctions.PrintTime(TEXT("seekfwd"), (__int64) pcrPos, 90, &debugcount);

	//Store this pos for later use
	__int64 posSave = 0;
	if (SUCCEEDED(hr))
		posSave = pos; //Save this position if where past seek value
	else
		pos = ulBytesRead;//set buffer to end for backward search

	pos -= m_pTSParserSourceFilter->m_pPidParser->get_PacketSize();
	pcrPos -= 1; 

	hr = S_OK;		
	while (pcrPos > pcrSeekTime && hr == S_OK) {
		//Seek backwards
		hr = seekFunctions.FindNextPCR(m_pTSParserSourceFilter->m_pPidParser, pData, ulBytesRead, &m_pTSParserSourceFilter->m_pPidParser->pids, &pcrPos, &pos, -1); //Get the PCR
		pos -= m_pTSParserSourceFilter->m_pPidParser->get_PacketSize();
seekFunctions.PrintTime(TEXT("seekbackloop"), (__int64) pcrPos, 90, &debugcount);
	}
seekFunctions.PrintTime(TEXT("seekback"), (__int64) pcrPos, 90, &debugcount);

	// if we have backed up to correct pcr
	if (SUCCEEDED(hr)) {
		//Get mid position between pcr's only if in TS pin mode
		if (posSave) {
			//set mid way
			posSave -= (__int64)pos;
			pos += (ULONG)((__int64)posSave /(__int64)2);
		}

		//Set pointer to locale
		nFileIndex += (__int64)pos;

		m_IntCurrentTimePCR = pcrPos;
seekFunctions.PrintTime(TEXT("seekend"), (__int64) pcrPos, 90, &debugcount);
	}
	else
	{
		// Revert to old method
		// shifting right by 14 rounds the seek and duration time down to the
		// nearest multiple 16.384 ms. More than accurate enough for our seeks.
		nFileIndex = 0;

		if (m_pTSParserSourceFilter->m_pPidParser->pids.dur>>14)
			nFileIndex = filelength * (__int64)(seektime>>14) / (__int64)(m_pTSParserSourceFilter->m_pPidParser->pids.dur>>14);

		m_IntCurrentTimePCR = m_IntStartTimePCR + (__int64)((__int64)((__int64)seektime * (__int64)9) / (__int64)1000);
seekFunctions.PrintTime(TEXT("SEEK ERROR AT END"), (__int64)pcrPos, 90, &debugcount);
	}
		nFileIndex = max(m_pTSParserSourceFilter->m_pPidParser->get_StartOffset(), nFileIndex);
		m_pTSParserSourceFilter->m_pFileReader->setFilePointer((__int64)(nFileIndex - filelength), FILE_END);
		m_currPosition = m_pTSParserSourceFilter->m_pFileReader->getFilePointer();

	delete[] pData;

	return S_OK;
}

HRESULT CTSParserSourcePin::UpdateDuration(FileReader *pFileReader)
{
	HRESULT hr = E_FAIL;

//***********************************************************************************************
//Old Capture format Additions

	if(!m_pTSParserSourceFilter->m_pPidParser->pids.pcr && !m_pTSParserSourceFilter->m_pPidParser->get_ProgPinMode())
	{
		hr = S_FALSE;

		if (m_bSeeking)
			return hr;

		REFERENCE_TIME rtCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
		REFERENCE_TIME rtStop = 0;

		if (m_BitRateCycle < 50)
			m_DataRateSave = m_DataRate;

		//Calculate our time increase
		__int64 fileStart;
		__int64	fileSize = 0;
		pFileReader->GetFileSize(&fileStart, &fileSize);

		__int64 calcDuration = 0;
		if ((__int64)((__int64)m_DataRateSave / (__int64)8000) > 0)
		{
			calcDuration = (__int64)(fileSize / (__int64)((__int64)m_DataRateSave / (__int64)8000));
			calcDuration = (__int64)(calcDuration * (__int64)10000);
		}

		if ((__int64)m_pTSParserSourceFilter->m_pPidParser->pids.dur)
		{
			if (!m_bSeeking)
			{
				m_pTSParserSourceFilter->m_pPidParser->pids.dur = (REFERENCE_TIME)calcDuration;
				for (int i = 0; i < m_pTSParserSourceFilter->m_pPidParser->pidArray.Count(); i++)
				{
					m_pTSParserSourceFilter->m_pPidParser->pidArray[i].dur = m_pTSParserSourceFilter->m_pPidParser->pids.dur;
				}
				m_rtDuration = m_pTSParserSourceFilter->m_pPidParser->pids.dur;
				m_rtStop = m_pTSParserSourceFilter->m_pPidParser->pids.dur;
			}

			if ((REFERENCE_TIME)(m_rtLastCurrentTime + (REFERENCE_TIME)RT_SECOND) < rtCurrentTime) {
				//Get CSourceSeeking current time.
				CSourceSeeking::GetPositions(&rtCurrentTime, &rtStop);
				//Test if we had been seeking recently and wait 2sec if so.
				if ((REFERENCE_TIME)(m_rtLastSeekStart + (REFERENCE_TIME)RT_2_SECOND) < rtCurrentTime) {

					//Send event to update filtergraph clock.
					if (!m_bSeeking)
					{
						m_rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
						m_pTSParserSourceFilter->NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
						hr = S_OK;
					}
				}
			}
		}
parserFunctions.PrintTime(TEXT("UpdateDuration1"), (__int64) m_rtDuration, 10000, &debugcount);
		return hr;
	}

//***********************************************************************************************

	hr = S_FALSE;

	if (m_bSeeking)
		return hr;

	WORD readonly = 0;
	pFileReader->get_ReadOnly(&readonly);

	REFERENCE_TIME rtCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
	REFERENCE_TIME rtStop = 0;

	//check for duration every second of size change
	if (readonly && (REFERENCE_TIME)(m_rtLastCurrentTime + (REFERENCE_TIME)RT_SECOND) < rtCurrentTime)
	{
		//Get the FileReader Type
		WORD bMultiMode;
		pFileReader->get_ReaderMode(&bMultiMode);
		if(bMultiMode
			&& (REFERENCE_TIME)(m_rtLastCurrentTime + (REFERENCE_TIME)RT_SECOND) < rtCurrentTime)	//Do MultiFile timeshifting mode
		{
			//Get CSourceSeeking current time.
			CSourceSeeking::GetPositions(&rtCurrentTime, &rtStop);
			//Test if we had been seeking recently and wait 2sec if so.
//			if ((REFERENCE_TIME)(m_rtLastSeekStart + (REFERENCE_TIME)RT_2_SECOND) > rtCurrentTime
//				&& m_rtLastSeekStart < (REFERENCE_TIME)RT_2_SECOND)
//			{
//				if(m_rtLastSeekStart)// || rtCurrentTime)
//				{
////////				if (m_IntEndTimePCR != -1) //cold start
//parserFunctions.PrintTime(TEXT("UpdateDuration2"), (__int64) m_rtDuration, 10000, &debugcount);
//					return hr;
//				}
//			}

			//Check if Cold Start
			if(!m_IntBaseTimePCR && !m_IntStartTimePCR && !m_IntEndTimePCR)
			{
				m_LastMultiFileStart = -1;
				m_LastMultiFileEnd = -1;
			}

			BOOL bLengthChanged = FALSE;
			BOOL bStartChanged = FALSE;
			ULONG ulBytesRead = 0;
			__int64 pcrPos;
			ULONG pos = 0;

			// We'll use fileEnd instead of fileLength since fileLength /could/ be the same
			// even though fileStart and fileEnd have moved.
			__int64 fileStart, fileEnd, filelength;
			pFileReader->GetFileSize(&fileStart, &fileEnd);
			filelength = fileEnd;
			fileEnd += fileStart;
//			LONG lDataLength = m_lTSPacketDeliverySize;
			LONG lDataLength = (LONG)min(filelength/4, 2000000);
			lDataLength = max(m_lTSPacketDeliverySize, lDataLength);
			if (fileStart != m_LastMultiFileStart)
			{
				ulBytesRead = 0;
				pcrPos = -1;

				//Set Pointer to start of file to get end pcr
				pFileReader->setFilePointer(m_pTSParserSourceFilter->m_pPidParser->get_StartOffset(), FILE_BEGIN);
				PBYTE pData = new BYTE[lDataLength];
//				pFileReader->Read(pData, lDataLength, &ulBytesRead);
				if FAILED(hr = pFileReader->Read(pData, lDataLength, &ulBytesRead))
				{
					Debug(TEXT("Failed to read from start of file"));
				}

				if (ulBytesRead < (ULONG)lDataLength)
				{
					Debug(TEXT("Didn't read as much as it should have"));
				}

				hr = S_OK;
				pos = 0;
				hr = parserFunctions.FindNextPCR(m_pTSParserSourceFilter->m_pPidParser, pData, ulBytesRead, &m_pTSParserSourceFilter->m_pPidParser->pids, &pcrPos, &pos, 1); //Get the PCR
				delete[] pData;
				//park the Pointer to end of file 
//				pFileReader->setFilePointer( (__int64)max(0, (__int64)(m_pTSParserSourceFilter->m_pFileReader->getFilePointer() -(__int64)100000)), FILE_BEGIN);

				__int64	pcrDeltaTime = (__int64)(pcrPos - m_IntStartTimePCR);
				//Test if we have a pcr or if the pcr is less than rollover time
				if (FAILED(hr) || pcrDeltaTime < 0)
				{
					Debug(TEXT("Negative PCR Delta. This should only happen if there's a pcr rollover.\n"));
					parserFunctions.PrintTime(TEXT("Prev Start PCR"), m_IntStartTimePCR, 90, &debugcount);
					parserFunctions.PrintTime(TEXT("Start PCR"), pcrPos, 90, &debugcount);
					if(pcrPos)
					{
						m_IntStartTimePCR = pcrPos;
						m_IntBaseTimePCR = m_IntStartTimePCR;
					}

parserFunctions.PrintTime(TEXT("UpdateDuration3"), (__int64) m_rtDuration, 10000, &debugcount);
					return hr;
				}
				else
				{
					//Cold Start
					if (m_LastMultiFileStart == -1)
					{
						m_IntBaseTimePCR = pcrPos;
					}

					m_IntStartTimePCR = pcrPos;

					//update the times in the array
					for (int i = 0; i < m_pTSParserSourceFilter->m_pPidParser->pidArray.Count(); i++)
					{
						m_pTSParserSourceFilter->m_pPidParser->pidArray[i].start += pcrDeltaTime;
						if ((__int64)(m_pTSParserSourceFilter->m_pPidParser->pidArray[i].start) > MAX_PCR)
							m_pTSParserSourceFilter->m_pPidParser->pidArray[i].start -= MAX_PCR;
					}

					m_pTSParserSourceFilter->m_pPidParser->pids.start += pcrDeltaTime;
					if ((__int64)(m_pTSParserSourceFilter->m_pPidParser->pids.start) > MAX_PCR)
						m_pTSParserSourceFilter->m_pPidParser->pids.start -= MAX_PCR;

					m_LastMultiFileStart = fileStart;
					bStartChanged = TRUE;
					m_rtLastSeekStart = (__int64)max(0, (__int64)parserFunctions.SubConvertPCRtoRT(m_IntCurrentTimePCR, m_IntStartTimePCR));
					m_rtStart = m_rtLastSeekStart;
					m_pTSParserSourceFilter->ResetStreamTime();
				}
			};

			if (fileEnd != m_LastMultiFileEnd)
			{
				ulBytesRead = 0;
				pcrPos = -1;

				//Set Pointer to end of file to get end pcr
				pFileReader->setFilePointer((__int64)-lDataLength, FILE_END);
				PBYTE pData = new BYTE[lDataLength];
				if FAILED(hr = pFileReader->Read(pData, lDataLength, &ulBytesRead))
				{
					Debug(TEXT("Failed to read from end of file"));
				}

				if (ulBytesRead < (ULONG)lDataLength)
				{
					Debug(TEXT("Didn't read as much as it should have"));
				}

				pos = ulBytesRead - m_pTSParserSourceFilter->m_pPidParser->get_PacketSize();

				hr = S_OK;
				hr = parserFunctions.FindNextPCR(m_pTSParserSourceFilter->m_pPidParser, pData, ulBytesRead, &m_pTSParserSourceFilter->m_pPidParser->pids, &pcrPos, &pos, -1); //Get the PCR
				delete[] pData;
				//park the Pointer to end of file 
//				pFileReader->setFilePointer( (__int64)max(0, (__int64)(m_pTSParserSourceFilter->m_pFileReader->getFilePointer() -(__int64)100000)), FILE_BEGIN);

				__int64	pcrDeltaTime = (__int64)(pcrPos - m_IntEndTimePCR);
				//Test if we have a pcr or if the pcr is less than rollover time
				if (FAILED(hr) || pcrDeltaTime < 0)
				{
					Debug(TEXT("Negative PCR Delta. This should only happen if there's a pcr rollover.\n"));
					parserFunctions.PrintTime(TEXT("Prev End PCR"), m_IntEndTimePCR, 90, &debugcount);
					parserFunctions.PrintTime(TEXT("End PCR"), pcrPos, 90, &debugcount);
					if(pcrPos)
						m_IntEndTimePCR = pcrPos;
parserFunctions.PrintTime(TEXT("UpdateDuration5"), (__int64) m_rtDuration, 10000, &debugcount);
					return hr;
				}
				else
				{
					//Cold Start
					if (m_LastMultiFileEnd == -1 && pcrPos)
					{
						m_IntEndTimePCR = pcrPos;
						m_pTSParserSourceFilter->m_pPidParser->pids.dur = (__int64)parserFunctions.SubConvertPCRtoRT(m_IntEndTimePCR, m_IntStartTimePCR); 
						m_LastMultiFileEnd = fileEnd;
parserFunctions.PrintTime(TEXT("UpdateDuration4"), (__int64) m_pTSParserSourceFilter->m_pPidParser->pids.dur, 10000, &debugcount);
						return hr;
					}
					else
						m_IntEndTimePCR = pcrPos;

					bLengthChanged = TRUE;
				}

				//update the times in the array
				for (int i = 0; i < m_pTSParserSourceFilter->m_pPidParser->pidArray.Count(); i++)
				{
					m_pTSParserSourceFilter->m_pPidParser->pidArray[i].end += pcrDeltaTime;
					if ((__int64)(m_pTSParserSourceFilter->m_pPidParser->pidArray[i].end) > MAX_PCR)
						m_pTSParserSourceFilter->m_pPidParser->pidArray[i].end -= MAX_PCR;
				}

				m_pTSParserSourceFilter->m_pPidParser->pids.end += pcrDeltaTime;
				if ((__int64)(m_pTSParserSourceFilter->m_pPidParser->pids.end) > MAX_PCR)
					m_pTSParserSourceFilter->m_pPidParser->pids.end -= MAX_PCR;

				m_LastMultiFileEnd = fileEnd;
			}

			if ((bLengthChanged | bStartChanged) && !m_bSeeking)
			{	
				__int64	pcrDeltaTime;
				if(m_bGetAvailableMode)
					//Use this code to cause the end time to be relative to the base time.
					pcrDeltaTime = parserFunctions.SubtractPCR(m_IntEndTimePCR, m_IntBaseTimePCR);
				else
					//Use this code to cause the end time to be relative to the start time.
					pcrDeltaTime = parserFunctions.SubtractPCR(m_IntEndTimePCR, m_IntStartTimePCR);

				m_pTSParserSourceFilter->m_pPidParser->pids.dur = (__int64)parserFunctions.ConvertPCRtoRT(pcrDeltaTime);

				// update pid arrays
				for (int i = 0; i < m_pTSParserSourceFilter->m_pPidParser->pidArray.Count(); i++)
					m_pTSParserSourceFilter->m_pPidParser->pidArray[i].dur = m_pTSParserSourceFilter->m_pPidParser->pids.dur;

				m_rtDuration = m_pTSParserSourceFilter->m_pPidParser->pids.dur;

				if(m_bGetAvailableMode)
					//Use this code to cause the end time to be relative to the base time.
					m_rtStop = m_pTSParserSourceFilter->m_pPidParser->pids.dur;
				else
				{
					//Use this code to cause the end time to be relative to the start time.
					__int64 offset = max(0, (__int64)((__int64)m_rtStart - (__int64)m_rtDuration));
					if (offset)
						m_rtStop = parserFunctions.SubConvertPCRtoRT(m_IntCurrentTimePCR, m_IntStartTimePCR);
					else
						m_rtStop = m_pTSParserSourceFilter->m_pPidParser->pids.dur;
				}

parserFunctions.PrintTime(TEXT("UpdateDuration: m_IntBaseTimePCR"), (__int64)m_IntBaseTimePCR, 90, &debugcount);
parserFunctions.PrintTime(TEXT("UpdateDuration: m_IntStartTimePCR"), (__int64)m_IntStartTimePCR, 90, &debugcount);
parserFunctions.PrintTime(TEXT("UpdateDuration: m_IntEndTimePCR"), (__int64)m_IntEndTimePCR, 90, &debugcount);
parserFunctions.PrintTime(TEXT("UpdateDuration: pids.start"), (__int64)m_pTSParserSourceFilter->m_pPidParser->pids.start, 90, &debugcount);
parserFunctions.PrintTime(TEXT("UpdateDuration: pids.end"), (__int64)m_pTSParserSourceFilter->m_pPidParser->pids.end, 90, &debugcount);
parserFunctions.PrintTime(TEXT("UpdateDuration: pids.dur"), (__int64)m_pTSParserSourceFilter->m_pPidParser->pids.dur, 10000, &debugcount);

				if (!m_bSeeking)
				{
					m_rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
					m_pTSParserSourceFilter->NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
parserFunctions.PrintTime(TEXT("UpdateDuration6"), (__int64) m_rtDuration, 10000, &debugcount);
					return S_OK;
				}
			}
			return S_FALSE;
		}
		else // FileReader Mode
		{
			//check for duration every second of size change
			BOOL bTimeMode;
			BOOL bTimeShifting = IsTimeShifting(pFileReader, &bTimeMode);

			BOOL bLengthChanged = FALSE;

			//check for valid values
			if ((m_pTSParserSourceFilter->m_pPidParser->pids.pcr | m_pTSParserSourceFilter->m_pPidParser->get_ProgPinMode())
				&& m_IntEndTimePCR
				&& TRUE){
				//check for duration every second of size change
				if(((REFERENCE_TIME)(m_rtLastCurrentTime + (REFERENCE_TIME)RT_SECOND) < rtCurrentTime))
				{
					ULONG pos;
					__int64 pcrPos;
					ULONG ulBytesRead = 0;
					__int64 fileStart, fileEnd, filelength;
					pFileReader->GetFileSize(&fileStart, &fileEnd);
					filelength = fileEnd;
					fileEnd += fileStart;
		//			LONG lDataLength = m_lTSPacketDeliverySize;
					LONG lDataLength = (LONG)min(filelength/4, 2000000);
					lDataLength = max(m_lTSPacketDeliverySize, lDataLength);

					//Do a quick parse of duration if not seeking
					if (m_bSeeking)
						return hr;

					//Set Pointer to end of file to get end pcr
					pFileReader->setFilePointer((__int64)-lDataLength, FILE_END);
					PBYTE pData = new BYTE[lDataLength];
					pFileReader->Read(pData, lDataLength, &ulBytesRead);
					pos = ulBytesRead - m_pTSParserSourceFilter->m_pPidParser->get_PacketSize();

					hr = S_OK;
					hr = parserFunctions.FindNextPCR(m_pTSParserSourceFilter->m_pPidParser, pData, ulBytesRead, &m_pTSParserSourceFilter->m_pPidParser->pids, &pcrPos, &pos, -1); //Get the PCR
					delete[] pData;
	
					__int64	pcrDeltaTime = (__int64)(pcrPos - m_IntEndTimePCR);
					//Test if we have a pcr or if the pcr is less than rollover time
					if (FAILED(hr) || pcrDeltaTime < (__int64)0) {
						if(pcrPos)
							m_IntEndTimePCR = pcrPos;
						return hr;
					}

					m_IntEndTimePCR = pcrPos;
					m_IntStartTimePCR += pcrDeltaTime;
					//if not time shifting update the duration
					if (!bTimeMode)
						m_pTSParserSourceFilter->m_pPidParser->pids.dur += (__int64)parserFunctions.ConvertPCRtoRT(pcrDeltaTime);

					//update the times in the array
					for (int i = 0; i < m_pTSParserSourceFilter->m_pPidParser->pidArray.Count(); i++)
					{
						m_pTSParserSourceFilter->m_pPidParser->pidArray[i].end += pcrDeltaTime;
						if ((__int64)(m_pTSParserSourceFilter->m_pPidParser->pidArray[i].end) > MAX_PCR)
							m_pTSParserSourceFilter->m_pPidParser->pidArray[i].end -= MAX_PCR;
		
						// update the start time if shifting else update the duration
						if (bTimeMode)
						{
							m_pTSParserSourceFilter->m_pPidParser->pidArray[i].start += pcrDeltaTime;
							if ((__int64)(m_pTSParserSourceFilter->m_pPidParser->pidArray[i].start) > MAX_PCR)
								m_pTSParserSourceFilter->m_pPidParser->pidArray[i].start -= MAX_PCR;
						}
						else
							m_pTSParserSourceFilter->m_pPidParser->pidArray[i].dur = m_pTSParserSourceFilter->m_pPidParser->pids.dur;
					}
					m_pTSParserSourceFilter->m_pPidParser->pids.end += pcrDeltaTime;
					if ((__int64)(m_pTSParserSourceFilter->m_pPidParser->pids.end) > MAX_PCR)
						m_pTSParserSourceFilter->m_pPidParser->pids.end -= MAX_PCR;

					bLengthChanged = TRUE;
				}
			}
			else if ((REFERENCE_TIME)(m_rtLastCurrentTime + (REFERENCE_TIME)RT_SECOND) < rtCurrentTime
				&& TRUE)
			{
				//update all of the pid array times from a file parse.
				if (!m_bSeeking)
					m_pTSParserSourceFilter->m_pPidParser->RefreshDuration(TRUE, pFileReader);

				bLengthChanged = TRUE;
			}

			if (bLengthChanged)
			{
				if (!m_bSeeking)
				{
					//Set the filtergraph clock time to stationary position
					if (bTimeMode)
					{
//						__int64 current = (__int64)SubConvertPCRtoRT(max(0, (__int64)(m_IntEndTimePCR, m_IntCurrentTimePCR)));
//						current = max(0,(__int64)(m_pTSParserSourceFilter->m_pPidParser->pids.dur - current - (__int64)RT_SECOND)); 
						CRefTime cTime;
						m_pTSParserSourceFilter->StreamTime(cTime);
						REFERENCE_TIME current = (REFERENCE_TIME)(m_rtLastSeekStart + REFERENCE_TIME(cTime));
						//Set the position of the filtergraph clock if first time shift pass
						if (!m_rtTimeShiftPosition)
							m_rtTimeShiftPosition = (REFERENCE_TIME)min(current, m_pTSParserSourceFilter->m_pPidParser->pids.dur - (__int64)RT_SECOND);
						//  set clock to stop or update time if not first pass
//						m_rtStop = max(0, (__int64)(m_pTSParserSourceFilter->m_pPidParser->pids.dur - (__int64)SubConvertPCRtoRT((__int64)(m_IntEndTimePCR, m_IntCurrentTimePCR))));
						m_rtStop = max(m_rtTimeShiftPosition, m_rtLastSeekStart);
						if (!m_bSeeking)
						{
							m_rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
							m_pTSParserSourceFilter->NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
							hr = S_OK;
						}
					}
					else
					{
						// if was time shifting but not anymore such as filewriter pause or stop
						if(m_rtTimeShiftPosition){
							//reset the stream clock and last seek save value
							m_rtStart = m_rtTimeShiftPosition;
							m_rtStop = m_rtTimeShiftPosition;
							m_rtLastSeekStart = m_rtTimeShiftPosition;
							m_pTSParserSourceFilter->ResetStreamTime();
							m_rtTimeShiftPosition = 0;
						}
						else
						{
							if(bTimeShifting)
							{
								m_rtTimeShiftPosition = 0;
//								CRefTime cTime;
//								m_pTSParserSourceFilter->StreamTime(cTime);
//								REFERENCE_TIME rtCurrent = (REFERENCE_TIME)(m_rtLastSeekStart + REFERENCE_TIME(cTime));
								__int64 current = (__int64)max(0, (__int64)parserFunctions.SubConvertPCRtoRT(m_IntEndTimePCR, m_IntCurrentTimePCR));
								REFERENCE_TIME rtCurrent = (REFERENCE_TIME)(m_pTSParserSourceFilter->m_pPidParser->pids.dur - current); 
								m_rtDuration = m_pTSParserSourceFilter->m_pPidParser->pids.dur;
								m_rtStop = rtCurrent;
							}
							else
							{
								m_rtTimeShiftPosition = 0;
								m_rtDuration = m_pTSParserSourceFilter->m_pPidParser->pids.dur;
								m_rtStop = m_pTSParserSourceFilter->m_pPidParser->pids.dur;
							}

						}
												

					}

					//Send event to update filtergraph clock
					if (!m_bSeeking)
					{
						m_rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
						m_pTSParserSourceFilter->NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
						return S_OK;;
					}

				}

				if ((REFERENCE_TIME)(m_rtLastCurrentTime + (REFERENCE_TIME)RT_SECOND) < rtCurrentTime) {
					//Get CSourceSeeking current time.
					CSourceSeeking::GetPositions(&rtCurrentTime, &rtStop);
					//Test if we had been seeking recently and wait 2sec if so.
					if ((REFERENCE_TIME)(m_rtLastSeekStart + (REFERENCE_TIME)RT_2_SECOND) < rtCurrentTime) {

						//Send event to update filtergraph clock.
						if (!m_bSeeking)
						{
							m_rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
							m_pTSParserSourceFilter->NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
							return S_OK;;
						}
					}
				}

			}
			else
				return S_FALSE;
		}
	}
	return S_OK;
}

void CTSParserSourcePin::WaitPinLock(void)
{
//	CAutoLock fillLock(&m_FillLock);
	CAutoLock lock(&m_SeekLock);
	{
	}
}

HRESULT CTSParserSourcePin::SetDuration(REFERENCE_TIME duration)
{
	CAutoLock fillLock(&m_FillLock);
	CAutoLock lock(&m_SeekLock);

	m_rtDuration = duration;
	m_rtStop = m_rtDuration;

    return S_OK;
}

REFERENCE_TIME CTSParserSourcePin::getPCRPosition(void)
{
	return positionFunctions.ConvertPCRtoRT(m_IntCurrentTimePCR);
}

BOOL CTSParserSourcePin::IsTimeShifting(FileReader *pFileReader, BOOL *timeMode)
{
	WORD readonly = 0;
	pFileReader->get_ReadOnly(&readonly);
	__int64	fileStart, fileSize = 0;
	pFileReader->GetFileSize(&fileStart, &fileSize);
	*timeMode = (m_LastFileSize == fileSize) & (fileStart ? TRUE : FALSE) & (readonly ? TRUE : FALSE) & (m_LastStartSize != fileStart);
	m_LastFileSize = fileSize;
	m_LastStartSize = fileStart;
	return (fileStart ? TRUE : FALSE) & (readonly ? TRUE : FALSE);
}

BOOL CTSParserSourcePin::get_InjectMode()
{
	return m_bInjectMode;
}

void CTSParserSourcePin::set_InjectMode(BOOL bInjectMode)
{
	m_bInjectMode = bInjectMode;
}

BOOL CTSParserSourcePin::get_RateControl()
{
	return m_bRateControl;
}

void CTSParserSourcePin::set_RateControl(BOOL bRateControl)
{
	m_bRateControl = bRateControl;
}

HRESULT CTSParserSourcePin::FindNextPCR(__int64 *pcrtime, long *byteOffset, long maxOffset)
{
	CAutoLock fillLock(&m_FillLock);
	HRESULT hr = E_FAIL;

	long bytesToRead = m_lTSPacketDeliverySize + m_PacketSave;	//Read an extra packet to make sure we don't miss a PCR that spans a gap.
	BYTE *pData = new BYTE[bytesToRead];

	while (*byteOffset < maxOffset)
	{
		bytesToRead = min(bytesToRead, maxOffset-*byteOffset);

		m_pTSBuffer->SetFileReader(m_pTSParserSourceFilter->m_pFileReader);
		hr = m_pTSBuffer->ReadFromBuffer(pData, bytesToRead, *byteOffset);
		if (FAILED(hr))
			break;

		ULONG pos = 0;
		hr = fillFunctions.FindFirstPCR(m_pTSParserSourceFilter->m_pPidParser, pData, bytesToRead, m_pPids, pcrtime, &pos);
		if (SUCCEEDED(hr))
		{
			*byteOffset += pos;
			break;
		}

		*byteOffset += m_lTSPacketDeliverySize;
	};

	delete[] pData;
	return hr;
}


HRESULT CTSParserSourcePin::FindPrevPCR(__int64 *pcrtime, long *byteOffset)
{
	CAutoLock fillLock(&m_FillLock);
	HRESULT hr = E_FAIL;

	long bytesToRead = m_lTSPacketDeliverySize + m_PacketSave; //Read an extra packet to make sure we don't miss a PCR that spans a gap.
	BYTE *pData = new BYTE[bytesToRead];

	while (*byteOffset > 0)
	{
		bytesToRead = min(m_lTSPacketDeliverySize, *byteOffset);
		*byteOffset -= bytesToRead;

		bytesToRead += m_PacketSave;

		m_pTSBuffer->SetFileReader(m_pTSParserSourceFilter->m_pFileReader);
		hr = m_pTSBuffer->ReadFromBuffer(pData, bytesToRead, *byteOffset);
		if (FAILED(hr))
			break;

		ULONG pos = 0;
		hr = fillFunctions.FindLastPCR(m_pTSParserSourceFilter->m_pPidParser, pData, bytesToRead, m_pPids, pcrtime, &pos);
		if (SUCCEEDED(hr))
		{
			*byteOffset += pos;
			break;
		}

		//*byteOffset -= m_lTSPacketDeliverySize;
	};

	delete[] pData;
	return hr;
}

long CTSParserSourcePin::get_BitRate()
{
    return m_DataRate;
}

void CTSParserSourcePin::set_BitRate(long rate)
{
    m_DataRate = rate;
}

void  CTSParserSourcePin::AddBitRateForAverage(__int64 bitratesample)
{
	if (bitratesample < (__int64)1)
		return;

	//Replace the old value with the new value
	m_DataRateTotal += bitratesample - m_BitRateStore[m_BitRateCycle];
	
	//If the previous value is not set then the total not made up from 256 values yet.
	if (m_BitRateStore[m_BitRateCycle] == 0)
		m_DataRate = (long)(m_DataRateTotal / (__int64)(m_BitRateCycle+1));
	else
		m_DataRate = (long)(m_DataRateTotal / (__int64)256);
		
	//Store the new value
	m_BitRateStore[m_BitRateCycle] = bitratesample;

	//Rotate array
	m_BitRateCycle++;
	if (m_BitRateCycle > 255)
		m_BitRateCycle = 0;

}

void CTSParserSourcePin::Debug(LPCTSTR lpOutputString)
{
	TCHAR sz[200];
	wsprintf(sz, TEXT("%05i - %s"), debugcount, lpOutputString);
	::OutputDebugString(sz);
	debugcount++;
}

HRESULT CTSParserSourcePin::DisconnectDemux()
{
	// Parse only the existing Mpeg2 Demultiplexer Filter
	// in the filter graph, we do this by looking for filters
	// that implement the IMpeg2Demultiplexer interface while
	// the count is still active.
	CFilterList FList(NAME("MyList"));  // List to hold the downstream peers.
	if (SUCCEEDED(Demux::GetPeerFilters(m_pTSParserSourceFilter, PINDIR_OUTPUT, FList)) && FList.GetHeadPosition())
	{
		IBaseFilter* pFilter = NULL;
		POSITION pos = FList.GetHeadPosition();
		pFilter = FList.Get(pos);
		while (SUCCEEDED(Demux::GetPeerFilters(pFilter, PINDIR_OUTPUT, FList)) && pos)
		{
			pFilter = FList.GetNext(pos);
		}

		pos = FList.GetHeadPosition();

		while (pos)
		{
			pFilter = FList.GetNext(pos);
			if(pFilter != NULL)
			{
				//Keep a reference for later destroy, will not add the same object
				Demux::AddFilterUnique(m_pTSParserSourceFilter->m_FilterRefList, pFilter);
				// Get an instance of the Demux control interface
				CComPtr<IMpeg2Demultiplexer> muxInterface;
				if(SUCCEEDED(pFilter->QueryInterface (&muxInterface)))
				{
					DisconnectOutputPins(pFilter);
					muxInterface.Release();
				}

				pFilter = NULL;
			}
		}
	}

	//Clear the filter list;
	POSITION pos = FList.GetHeadPosition();
	while (pos){

		if (FList.Get(pos) != NULL)
				FList.Get(pos)->Release();

		FList.Remove(pos);
		pos = FList.GetHeadPosition();
	}

	return S_OK;
}

HRESULT CTSParserSourcePin::DisconnectOutputPins(IBaseFilter *pFilter)
{
	CComPtr<IPin> pOPin;
	PIN_DIRECTION  direction;
	// Enumerate the Demux pins
	CComPtr<IEnumPins> pIEnumPins;
	if (SUCCEEDED(pFilter->EnumPins(&pIEnumPins)))
	{

		ULONG pinfetch(0);
		while(pIEnumPins->Next(1, &pOPin, &pinfetch) == S_OK)
		{

			pOPin->QueryDirection(&direction);
			if(direction == PINDIR_OUTPUT)
			{
				// Get an instance of the Demux control interface
				CComPtr<IMpeg2Demultiplexer> muxInterface;
				if(SUCCEEDED(pFilter->QueryInterface (&muxInterface)))
				{
					LPWSTR pinName = L"";
					pOPin->QueryId(&pinName);
					muxInterface->DeleteOutputPin(pinName);
					muxInterface.Release();
				}
				else
				{
					IPin *pIPin = NULL;
					pOPin->ConnectedTo(&pIPin);
					if (pIPin)
					{
						pOPin->Disconnect();
						pIPin->Disconnect();
						pIPin->Release();
					}
				}

			}
			pOPin.Release();
			pOPin = NULL;
		}
	}
	return S_OK;
}

HRESULT CTSParserSourcePin::DisconnectInputPins(IBaseFilter *pFilter)
{
	CComPtr<IPin> pIPin;
	PIN_DIRECTION  direction;
	// Enumerate the Demux pins
	CComPtr<IEnumPins> pIEnumPins;
	if (SUCCEEDED(pFilter->EnumPins(&pIEnumPins)))
	{

		ULONG pinfetch(0);
		while(pIEnumPins->Next(1, &pIPin, &pinfetch) == S_OK)
		{

			pIPin->QueryDirection(&direction);
			if(direction == PINDIR_INPUT)
			{
				IPin *pOPin = NULL;
				pIPin->ConnectedTo(&pOPin);
				if (pOPin)
				{
					pOPin->Disconnect();
					pIPin->Disconnect();
					pOPin->Release();
				}
			}
			pIPin.Release();
			pIPin = NULL;
		}
	}
	return S_OK;
}

HRESULT CTSParserSourcePin::SetDemuxClock(IReferenceClock *pClock)
{
	// Parse only the existing Mpeg2 Demultiplexer Filter
	// in the filter graph, we do this by looking for filters
	// that implement the IMpeg2Demultiplexer interface while
	// the count is still active.
	CFilterList FList(NAME("MyList"));  // List to hold the downstream peers.
	if (SUCCEEDED(Demux::GetPeerFilters(m_pTSParserSourceFilter, PINDIR_OUTPUT, FList)) && FList.GetHeadPosition())
	{
		IBaseFilter* pFilter = NULL;
		POSITION pos = FList.GetHeadPosition();
		pFilter = FList.Get(pos);
		while (SUCCEEDED(Demux::GetPeerFilters(pFilter, PINDIR_OUTPUT, FList)) && pos)
		{
			pFilter = FList.GetNext(pos);
		}

		pos = FList.GetHeadPosition();

		while (pos)
		{
			pFilter = FList.GetNext(pos);
			if(pFilter != NULL)
			{
				//Keep a reference for later destroy, will not add the same object
                Demux::AddFilterUnique(m_pTSParserSourceFilter->m_FilterRefList, pFilter);
				// Get an instance of the Demux control interface
				IMpeg2Demultiplexer* muxInterface = NULL;
				if(SUCCEEDED(pFilter->QueryInterface (&muxInterface)))
				{
//***********************************************************************************************
//Old Capture format Additions
//					if (m_pPidParser->pids.pcr && m_pTSParserSourceFilter->get_AutoMode()) // && !m_pPidParser->pids.opcr) 
//***********************************************************************************************
//					if (TRUE && m_biMpegDemux && m_pTSParserSourceFilter->get_AutoMode())
					if (TRUE && m_biMpegDemux)
						pFilter->SetSyncSource(pClock);
					muxInterface->Release();
				}
				pFilter = NULL;
			}
		}
	}

	//Clear the filter list;
	POSITION pos = FList.GetHeadPosition();
	while (pos){

		if (FList.Get(pos) != NULL)
				FList.Get(pos)->Release();

		FList.Remove(pos);
		pos = FList.GetHeadPosition();
	}

	return S_OK;
}

HRESULT CTSParserSourcePin::ReNewDemux()
{
	// Parse only the existing Mpeg2 Demultiplexer Filter
	// in the filter graph, we do this by looking for filters
	// that implement the IMpeg2Demultiplexer interface while
	// the count is still active.
	CFilterList FList(NAME("MyList"));  // List to hold the downstream peers.
	if (SUCCEEDED(Demux::GetPeerFilters(m_pTSParserSourceFilter, PINDIR_OUTPUT, FList)) && FList.GetHeadPosition())
	{
		IBaseFilter* pFilter = NULL;
		POSITION pos = FList.GetHeadPosition();
		pFilter = FList.Get(pos);
		while (SUCCEEDED(Demux::GetPeerFilters(pFilter, PINDIR_OUTPUT, FList)) && pos)
		{
			pFilter = FList.GetNext(pos);
		}

		pos = FList.GetHeadPosition();

		//Reconnect the Tee filter
		PIN_INFO PinInfo;
		PinInfo.pFilter = NULL;
		IPin *pTPin = NULL;
		if (SUCCEEDED((IPin*)this->ConnectedTo(&pTPin)))
		{
			if (SUCCEEDED(pTPin->QueryPinInfo(&PinInfo)))
			{
				IMpeg2Demultiplexer* muxInterface = NULL;
				if(SUCCEEDED(PinInfo.pFilter->QueryInterface (&muxInterface)))
				{
					muxInterface->Release();
					PinInfo.pFilter->Release();
					PinInfo.pFilter = NULL;
				}
				else
				{
					DisconnectInputPins(PinInfo.pFilter);
					DisconnectOutputPins(PinInfo.pFilter);
					//Reconnect the Tee Filter pins
					IGraphBuilder *pGraphBuilder;
					if(m_pTSParserSourceFilter->GetFilterGraph() && SUCCEEDED((IBaseFilter*)m_pTSParserSourceFilter->GetFilterGraph()->QueryInterface(IID_IGraphBuilder, (void **) &pGraphBuilder)))
					{
						pGraphBuilder->Connect((IPin*)this, pTPin);
						pGraphBuilder->Release();
					}
				}
			}
			pTPin->Release();
		}

		while (pos)
		{
			pFilter = FList.GetNext(pos);
			if(pFilter != NULL)
			{
				//Keep a reference for later destroy, will not add the same object
                Demux::AddFilterUnique(m_pTSParserSourceFilter->m_FilterRefList, pFilter);
				// Get an instance of the Demux control interface
				IMpeg2Demultiplexer* muxInterface = NULL;
				if(SUCCEEDED(pFilter->QueryInterface (&muxInterface)))
				{
					muxInterface->Release();

					//Get the Demux Filter Info
					CLSID ClsID;
					pFilter->GetClassID(&ClsID);
					LPWSTR pName = new WCHAR[128];
					FILTER_INFO FilterInfo;
					if (SUCCEEDED(pFilter->QueryFilterInfo(&FilterInfo)))
					{
						memcpy(pName, FilterInfo.achName, 128);
						pFilter->SetSyncSource(NULL);
						IPin *pIPin = NULL;
						IPin *pOPin = NULL;

						if (!PinInfo.pFilter)
						{
							GetPinConnection(pFilter, &pIPin, &pOPin);
							if (pIPin) pIPin->Release();
						}

						FilterInfo.pGraph->RemoveFilter(pFilter);
						FilterInfo.pGraph->Release();

						//Replace the Demux Filter
						pFilter = NULL;
						if (SUCCEEDED(CoCreateInstance(ClsID, NULL, CLSCTX_INPROC_SERVER, IID_IBaseFilter, reinterpret_cast<void**>(&pFilter))))
						{
							if (SUCCEEDED(FilterInfo.pGraph->AddFilter(pFilter, pName)))
							{
								Demux::AddFilterUnique(m_pTSParserSourceFilter->m_FilterRefList, pFilter);
								IMpeg2Demultiplexer* muxInterface = NULL;
								if(SUCCEEDED(pFilter->QueryInterface (&muxInterface)))
									muxInterface->Release();

								if (PinInfo.pFilter)
								{
									RenderOutputPin(PinInfo.pFilter);
									m_pTSParserSourceFilter->OnConnect();
									RenderOutputPins(pFilter);
								}
								else
								{
									pIPin = NULL;
									IPin *pNPin = NULL;
									GetPinConnection(pFilter, &pIPin, &pNPin);
									if (pNPin) pNPin->Release();

									IGraphBuilder *pGraphBuilder;
									if(SUCCEEDED(FilterInfo.pGraph->QueryInterface(IID_IGraphBuilder, (void **) &pGraphBuilder)))
									{
										pGraphBuilder->Connect(pOPin, pIPin);
										m_pTSParserSourceFilter->OnConnect();
										RenderOutputPins(pFilter);
										pGraphBuilder->Release();
										if (pOPin) pOPin->Release();
										if (pIPin) pIPin->Release();
									}
								}
							}
						}
					}
					if (pName) delete[] pName;
				}
				pFilter = NULL;
			}
		}
		if (PinInfo.pFilter)
			PinInfo.pFilter->Release();
	}

	//Clear the filter list;
	POSITION pos = FList.GetHeadPosition();
	while (pos){

		if (FList.Get(pos) != NULL)
				FList.Get(pos)->Release();

		FList.Remove(pos);
		pos = FList.GetHeadPosition();
	}

	return S_OK;
}

HRESULT CTSParserSourcePin::GetPinConnection(IBaseFilter *pFilter, IPin **ppIPin, IPin **ppOPin)
{
	HRESULT hr = E_FAIL;

	if (!pFilter || !ppOPin || !ppIPin)
		return hr;

	IPin *pOPin = NULL;
	IPin *pIPin = NULL;

	FILTER_INFO FilterInfo;
	if (SUCCEEDED(pFilter->QueryFilterInfo(&FilterInfo)))
	{
		PIN_DIRECTION  direction;
		// Enumerate the Filter pins
		CComPtr<IEnumPins> pIEnumPins;
		if (SUCCEEDED(pFilter->EnumPins(&pIEnumPins))){

			ULONG pinfetch(0);
			while(pIEnumPins->Next(1, &pIPin, &pinfetch) == S_OK){

				pIPin->QueryDirection(&direction);
				if(direction == PINDIR_INPUT){

					*ppIPin = pIPin;
					hr = pIPin->ConnectedTo(&pOPin);
					*ppOPin = pOPin;
					FilterInfo.pGraph->Release();
					return hr;
				}
			}
			pIPin->Release();
			pIPin = NULL;
		}
		FilterInfo.pGraph->Release();
	}
	return hr;
}

HRESULT CTSParserSourcePin::RenderOutputPins(IBaseFilter *pFilter)
{
	HRESULT hr = E_FAIL;

	if (!pFilter)
		return hr;

	IPin *pOPin = NULL;
	FILTER_INFO FilterInfo;
	if (SUCCEEDED(pFilter->QueryFilterInfo(&FilterInfo)))
	{
		PIN_DIRECTION  direction;
		// Enumerate the Filter pins
		CComPtr<IEnumPins> pIEnumPins;
		if (SUCCEEDED(pFilter->EnumPins(&pIEnumPins))){

			ULONG pinfetch(0);
			while(pIEnumPins->Next(1, &pOPin, &pinfetch) == S_OK){

				pOPin->QueryDirection(&direction);
				if(direction == PINDIR_OUTPUT)
				{
					IPin *pIPin = NULL;
					pOPin->ConnectedTo(&pIPin);
					if (!pIPin)
					{
						//Render this Filter Output
						IGraphBuilder *pGraphBuilder;
						if(SUCCEEDED(FilterInfo.pGraph->QueryInterface(IID_IGraphBuilder, (void **) &pGraphBuilder)))
						{
							hr = pGraphBuilder->Render(pOPin);
							pGraphBuilder->Release();
						}
					}
					else
						pIPin->Release();
				}
				pOPin->Release();
				pOPin = NULL;
			};
		}
		FilterInfo.pGraph->Release();
	}
	return hr;
}

HRESULT CTSParserSourcePin::RenderOutputPin(IBaseFilter *pFilter)
{
	HRESULT hr = E_FAIL;

	if (!pFilter)
		return hr;

	IPin *pOPin = NULL;
	FILTER_INFO FilterInfo;
	if (SUCCEEDED(pFilter->QueryFilterInfo(&FilterInfo)))
	{
		PIN_DIRECTION  direction;
		// Enumerate the Filter pins
		CComPtr<IEnumPins> pIEnumPins;
		if (SUCCEEDED(pFilter->EnumPins(&pIEnumPins))){

			ULONG pinfetch(0);
			while(pIEnumPins->Next(1, &pOPin, &pinfetch) == S_OK)
			{
				pOPin->QueryDirection(&direction);
				if(direction == PINDIR_OUTPUT)
				{
					IPin *pIPin = NULL;
					pOPin->ConnectedTo(&pIPin);
					if (!pIPin)
					{
						//Render this Filter Output
						IGraphBuilder *pGraphBuilder;
						if(SUCCEEDED(FilterInfo.pGraph->QueryInterface(IID_IGraphBuilder, (void **) &pGraphBuilder)))
						{
							pGraphBuilder->Render(pOPin);
							pGraphBuilder->Release();
							pOPin->Release();
							break;
						}
					}
					else
						pIPin->Release();
				}
				pOPin->Release();
				pOPin = NULL;
			};
		}
		FilterInfo.pGraph->Release();
	}
	return hr;
}
#include <stdafx.h>
#include <streams.h>
#include <initguid.h>
#include <bdaiface.h>
#include "CTTPremiumOutputPin.h"
#include "CTTPremiumSource.h"
#include "CTTEnumPIDMap.h"
#include "TSDataFilter.h"

//
// CTTPremiumOutputPin constructor
//
CTTPremiumOutputPin::CTTPremiumOutputPin(TCHAR *pName, CTTPremiumSource *pTTPremiumSource, HRESULT *phr,
																	 LPCWSTR pPinName, AM_MEDIA_TYPE* pMt) :
		m_pTTPremiumSource(pTTPremiumSource),
		m_Version(1),
		m_bufferSize(0),
    m_threadStarted(false),
		CSourceStream(pName, phr, pTTPremiumSource, pPinName)
{
  ASSERT(pTTPremiumSource);

	if (pMt->majortype == MEDIATYPE_MPEG2_SECTIONS)
	{
		m_bufferSize = 1024 * 16; // 16k
	}
	else if (pMt->majortype == MEDIATYPE_Video || pMt->majortype == MEDIATYPE_Audio)
	{
		m_bufferSize = 1024 * 512; // 512k
	}
}

//
// CTTPremiumOutputPin destructor
//
CTTPremiumOutputPin::~CTTPremiumOutputPin()
{
	m_pTTPremiumSource = NULL;
	m_PIDs.clear();
	for (std::map<ULONG, TSDataFilter*>::iterator it = m_PIDToDataFilter.begin(); it != m_PIDToDataFilter.end(); it++)
	{
			// We are about to remove the last pin that uses this PID
			if ((*it).second->RefCount() == 1)
			{
        // Erase it from the global map
				m_pTTPremiumSource->m_PIDs.erase((*it).first);
				(*it).second->Stop();
			}

			(*it).second->RemoveRef(this);
			m_PIDToDataFilter.erase(it);
	}
	m_PIDToDataFilter.clear();
}

//
// Get a PID at a specified index
//
HRESULT CTTPremiumOutputPin::GetPID(int iPosition, PID_MAP *pPID)
{
	if (iPosition < 0 || iPosition >= m_PIDs.size())
	{
		return S_FALSE;
	}

	*pPID = m_PIDs[iPosition];
	return S_OK;
}

//
// DecideBufferSize
//
// This has to be present to override the PURE virtual class base function
//
HRESULT CTTPremiumOutputPin::DecideBufferSize(IMemAllocator *pMemAllocator, ALLOCATOR_PROPERTIES * pRequest)
{
	HRESULT hr = S_OK;
	DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumOutputPin::DecideBufferSize() - request, cBuffers: %d, cbBuffer: %d, cbAlign: %d, cbPrefix: %d"), 
	pRequest->cBuffers, pRequest->cbBuffer, pRequest->cbAlign, pRequest->cbPrefix));
	if (m_bufferSize)
	{
		pRequest->cBuffers = 1;  
		pRequest->cbBuffer = m_bufferSize;
		pRequest->cbAlign = 1 ;  
		pRequest->cbPrefix = 0 ;  
		//DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumOutputPin::DecideBufferSize() - setting new sizes, cBuffers: %d, cbBuffer: %d, cbAlign: %d, cbPrefix: %d"), 
		//	pRequest->cBuffers, pRequest->cbBuffer, pRequest->cbAlign, pRequest->cbPrefix));
		ALLOCATOR_PROPERTIES Actual;  
		hr = pMemAllocator->SetProperties(pRequest, &Actual);  
		if (FAILED(hr))
		{
			//DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumOutputPin::DecideBufferSize() - failed to SetProperties - hr: %X "), hr));
			return hr;
		}

		hr = pMemAllocator->Commit();
		if (FAILED(hr))
		{
			//DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumOutputPin::DecideBufferSize() - failed to Commit - hr: %X"), hr));
		}
	}
	return hr;
}

//
// Fill the sample buffer
//
HRESULT CTTPremiumOutputPin::FillBuffer(IMediaSample *pSample)
{
  // Don't do anything here as we actually asynchronously 
  // push data to the connected pin
  Sleep(1000);
	return NOERROR;
}

//
// Map PID data to the pin
//
HRESULT CTTPremiumOutputPin::MapPID(ULONG culPID, ULONG* pulPID, MEDIA_SAMPLE_CONTENT contentType)
{
  CAutoLock lock_it(m_pLock);

	HRESULT hr = NOERROR;
	for (ULONG i = 0; i < culPID; i++)
	{
		ULONG pid = pulPID[i];
	
		// Setup the PID map and buffer size
		PID_MAP pidMap;
		pidMap.MediaSampleContent = contentType;
		pidMap.ulPID = pid;

		CDVBTSFilter::FILTERTYPE type = CDVBTSFilter::NO_FILTER_TYPE;
		BYTE *filterData = NULL;
		BYTE *filterMask = NULL;
		int filterDataSize = 0;

		//DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumOutputPin::MapPID() - contentType: %d, pid: %d"), contentType, pid));

		if (contentType == MEDIA_MPEG2_PSI)
		{
			type = CDVBTSFilter::SECTION_FILTER;
			filterData = new BYTE[1];
			filterMask = new BYTE[1];
			filterData[0] = 0x00;
			filterMask[0] = 0x80;
			filterDataSize = 1;
		}
		else if (contentType == MEDIA_ELEMENTARY_STREAM)
		{
			type = CDVBTSFilter::STREAMING_FILTER;
		}

		std::map<ULONG, TSDataFilter*>::iterator it = m_pTTPremiumSource->m_PIDs.find(pid);
		TSDataFilter* ts = NULL;
		// We are not already streaming this PID
		if (it == m_pTTPremiumSource->m_PIDs.end())
		{
			ts = new TSDataFilter(m_bufferSize);
		}
		else
		{
			ts = (*it).second;
		}

		std::map<ULONG, TSDataFilter*>::iterator mit = m_PIDToDataFilter.find(pid);
		// Make sure we don't already have the PID mapped to this pin
		if (mit == m_PIDToDataFilter.end())
		{
			ts->AddRef(this);

			// Add it to the global map
			m_pTTPremiumSource->m_PIDs.insert(std::make_pair(pid, ts));
			// Add it to our PID map
			m_PIDToDataFilter.insert(std::make_pair(pid, ts));
			// Add the PID to our list
			m_PIDs.push_back(pidMap);

			DVB_ERROR error = ts->SetFilter(type, pid, filterData, filterMask, filterDataSize);
			if (error != DVB_ERR_NONE)
			{
				//DbgLog((LOG_TRACE, 3, TEXT("CTTPremiumSource::MapPID() - failed to start filter - pid: %d, error: %d"), pid, error));
			}

      if (m_threadStarted)
      {
			  ts->Start();
      }
		}
	}

	m_Version++;

	return NOERROR;
}

//
// Check the media type
//
HRESULT CTTPremiumOutputPin::CheckMediaType(const CMediaType *pmt)
{
	CAutoLock lock_it(m_pLock);

	if (pmt->majortype == MEDIATYPE_MPEG2_SECTIONS || 
		pmt->majortype == MEDIATYPE_Video ||  
		pmt->majortype == MEDIATYPE_Audio)
	{
		return S_OK;
	}
	return S_FALSE;
}

//
// Check the media type
//
HRESULT CTTPremiumOutputPin::GetMediaType(int iPosition, CMediaType *pMediaType)
{
  CAutoLock lock_it(m_pLock);

	if (iPosition < 0) return E_INVALIDARG; 
	if (iPosition > 2) return VFW_S_NO_MORE_ITEMS; 
 
	CheckPointer(pMediaType, E_POINTER); 
 
	if (iPosition == 0) 
	{ 
		pMediaType->SetFormatType(&FORMAT_None); 
		pMediaType->SetType(&MEDIATYPE_MPEG2_SECTIONS); 
		pMediaType->SetSubtype(&MEDIASUBTYPE_NULL); 
	}
	else if (iPosition == 1)
	{
		pMediaType->SetFormatType(&FORMAT_None); 
		pMediaType->SetType(&MEDIATYPE_Video); 
		pMediaType->SetSubtype(&MEDIASUBTYPE_NULL); 
	}
	else if (iPosition == 2)
	{
		pMediaType->SetFormatType(&FORMAT_None); 
		pMediaType->SetType(&MEDIATYPE_Audio); 
		pMediaType->SetSubtype(&MEDIASUBTYPE_NULL); 
	}
 
	return (S_OK); 
}

//
// Unmap PID data from the pin
//
HRESULT CTTPremiumOutputPin::UnmapPID(ULONG culPID, ULONG* pulPID)
{
  CAutoLock lock_it(m_pLock);
	//DbgLog((LOG_TRACE, 3, TEXT("UnmapPID count: %d, pid: %d"), culPID, pulPID[0]));

	for (ULONG i = 0; i < culPID; i++)
	{
		ULONG pid = pulPID[i];
		std::map<ULONG, TSDataFilter*>::iterator it = m_pTTPremiumSource->m_PIDs.find(pid);
		// Someone is streaming this PID
		if (it != m_pTTPremiumSource->m_PIDs.end())
		{
			//DbgLog((LOG_TRACE, 3, TEXT("UnmapPID, found PID in global list")));
			// We are streaming this PID
			std::map<ULONG, TSDataFilter*>::iterator mit = m_PIDToDataFilter.find(pid);
			if (mit != m_PIDToDataFilter.end())
			{
				//DbgLog((LOG_TRACE, 3, TEXT("UnmapPID, found PID in pins list")));
				// We are about to remove the last pin that uses this PID
				if ((*mit).second->RefCount() == 1)
				{
					//DbgLog((LOG_TRACE, 3, TEXT("UnmapPID, last ref to TSDataFilter, erasing from global list and stopping TSDataFilter")));
					m_pTTPremiumSource->m_PIDs.erase(it);
					(*mit).second->Stop();
				}

				(*mit).second->RemoveRef(this);
				m_PIDToDataFilter.erase(mit);
			}
		}
	}

	m_Version++;

	return NOERROR;
}

//
// Return the PID map
//
HRESULT CTTPremiumOutputPin::EnumPIDMap(IEnumPIDMap** ppEnum)
{
  CAutoLock lock_it(m_pLock);

  CheckPointer(ppEnum,E_POINTER);
  ValidateReadWritePtr(ppEnum,sizeof(IEnumPIDMap *));

  // Create a new ref counted enumerator
  *ppEnum = new CTTEnumPIDMap(this, NULL);
  if (*ppEnum == NULL) 
  {
    return E_OUTOFMEMORY;
  }

  return NOERROR;
}

//
// Return the interface(s) that we support
//
STDMETHODIMP CTTPremiumOutputPin::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
  CAutoLock lock_it(m_pLock);

  CheckPointer(ppv, E_POINTER);

	// Do we have this interface
	if (riid == IID_IMPEG2PIDMap)
	{
		return GetInterface((IMPEG2PIDMap*)this, ppv);
	}
  return CSourceStream::NonDelegatingQueryInterface(riid, ppv);
}

//
//
HRESULT CTTPremiumOutputPin::OnThreadCreate(void)
{
	std::map<ULONG, TSDataFilter*>::iterator it = m_PIDToDataFilter.begin();
	for (; it != m_PIDToDataFilter.end(); it++)
	{
		(*it).second->Start();
	}
  m_threadStarted = true;
	return CSourceStream::OnThreadCreate();
}


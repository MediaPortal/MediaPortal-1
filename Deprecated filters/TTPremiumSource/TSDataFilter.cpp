#include <stdafx.h>

#include <streams.h>
#include <initguid.h>
#include <bdaiface.h>
#include "CTTPremiumSource.h"
#include "CTTPremiumOutputPin.h"
#include "TSDataFilter.h"

//
//
TSDataFilter::TSDataFilter(int bufferSize) : CDVBTSFilter(bufferSize), m_isRunning(false), 
															  m_bufferSize(bufferSize)
{
	m_lock = new Lock();
}

//
//
TSDataFilter::~TSDataFilter()
{
	delete m_lock;
}

//
//
void TSDataFilter::Start()
{
	m_isRunning = true;
}

//
//
void TSDataFilter::Stop()
{
	if (m_isRunning)
	{
		CDVBTSFilter::ResetFilter();
	}

	m_isRunning = false;
}

//
//
void TSDataFilter::AddRef(CTTPremiumOutputPin *pPin)
{
	m_lock->lock();
  m_watchers.push_back(pPin);
	m_lock->unlock();
}

//
//
void TSDataFilter::RemoveRef(CTTPremiumOutputPin *pPin)
{
	m_lock->lock();
	for (std::vector<CTTPremiumOutputPin *>::iterator it = m_watchers.begin(); it != m_watchers.end(); it++)
  {
    if ((*it) == pPin)
    {
      m_watchers.erase(it);
      break;
    }
  }
  bool done = (m_watchers.size() == 0);
	m_lock->unlock();

	if (done)
	{
    Stop();
		delete this;
	}
}

//
//
void TSDataFilter::OnDataArrival(BYTE* Buff, int len)
{
	if (m_isRunning)
	{
		m_lock->lock();

    for (std::vector<CTTPremiumOutputPin *>::iterator it = m_watchers.begin(); it != m_watchers.end(); it++)
    {
      HRESULT hr = SendData((*it), Buff, len);
      while (FAILED(hr))
      {
        Sleep(10);
        hr = SendData((*it), Buff, len);
      }
    }
		m_lock->unlock();
	}
}

HRESULT TSDataFilter::SendData(CTTPremiumOutputPin *outputPin, BYTE *buff, int len)
{
  IMediaSample *pSample;
  HRESULT hr = outputPin->GetDeliveryBuffer(&pSample,NULL,NULL,0);
  if (FAILED(hr)) 
  {
    return hr;
  }

	BYTE *pBuffer = NULL;
	pSample->GetPointer(&pBuffer);  
	int sampleLen = pSample->GetSize();  
	pSample->SetActualDataLength(len);
  if (sampleLen < len)
  {
    // Skip it
    return S_OK;
  }

  memcpy(pBuffer, buff, len);
  hr = outputPin->Deliver(pSample);
  pSample->Release();

  // downstream filter returns S_FALSE if it wants us to
  // stop or an error if it's reporting an error.
  if (hr != S_OK)
  {
    DbgLog((LOG_TRACE, 2, TEXT("Deliver() returned %08x; stopping"), hr));
    return S_OK;
  }
}

/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#include "stdafx.h"
#include "MPUrlSourceSplitterOutputPin.h"
#include "OutputPinAllocator.h"
#include "LockMutex.h"
#include "OutputPinMediaSample.h"

#ifdef _DEBUG
#define MODULE_NAME                                               L"MPUrlSourceSplitterOutputPind"
#else
#define MODULE_NAME                                               L"MPUrlSourceSplitterOutputPin"
#endif

#define METHOD_GET_MEDIA_TYPE_NAME                                L"GetMediaType()"
#define METHOD_CONNECT_NAME                                       L"Connect()"
#define METHOD_DECIDE_ALLOCATOR_NAME                              L"DecideAllocator()"
#define METHOD_DECIDE_BUFFER_SIZE_NAME                            L"DecideBufferSize()"
#define METHOD_ACTIVE_NAME                                        L"Active()"
#define METHOD_INACTIVE_NAME                                      L"Inactive()"
#define METHOD_CHECK_MEDIA_TYPE_NAME                              L"CheckMediaType()"

#define METHOD_THREAD_PROC_NAME                                   L"ThreadProc()"
#define METHOD_QUEUE_END_OF_STREAM_NAME                           L"QueueEndOfStream()"

#define METHOD_PIN_MESSAGE_FORMAT                                 L"%s: %s: pin '%s', %s"
#define METHOD_PIN_START_FORMAT                                   L"%s: %s: pin '%s', Start"
#define METHOD_PIN_END_FORMAT                                     L"%s: %s: pin '%s', End"
#define METHOD_PIN_END_FAIL_RESULT_FORMAT                         L"%s: %s: pin '%s', End, Fail, result: 0x%08X"

CMPUrlSourceSplitterOutputPin::CMPUrlSourceSplitterOutputPin(CMediaTypeCollection *mediaTypes, LPCWSTR pName, CBaseFilter *pFilter, CCritSec *pLock, HRESULT *phr)
  : CBaseOutputPin(NAME("MediaPortal Url Source Splitter Output Pin"), pFilter, pLock, phr, pName)
{
  this->mediaPackets = NULL;
  this->filter = NULL;
  this->mediaTypes = NULL;
  this->streamPid = STREAM_PID_UNSPECIFIED;
  this->flushing = false;
  this->mediaTypeToSend = NULL;
  
  if (phr != NULL)
  {
    if (SUCCEEDED(*phr))
    {
      this->mediaPackets = new COutputPinPacketCollection();
      this->filter = dynamic_cast<IFilter *>(pFilter);
      this->mediaPacketsLock = CreateMutex(NULL, FALSE, NULL);
      this->mediaTypes = new CMediaTypeCollection();

      CHECK_POINTER_HRESULT(*phr, mediaTypes, *phr, E_INVALIDARG);

      CHECK_POINTER_HRESULT(*phr, this->mediaPackets, *phr, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*phr, this->filter, *phr, E_INVALIDARG);
      CHECK_POINTER_HRESULT(*phr, this->mediaPacketsLock, *phr, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*phr, this->mediaTypes, *phr, E_OUTOFMEMORY);

      if (SUCCEEDED(*phr))
      {
        *phr = this->mediaTypes->Append(mediaTypes) ? (*phr) : E_OUTOFMEMORY;
      }
    }
  }

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->filter, this->filter->GetLogger()->Log(LOGGER_INFO, METHOD_PIN_END_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, this->m_pName));
}

CMPUrlSourceSplitterOutputPin::~CMPUrlSourceSplitterOutputPin()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->filter, this->filter->GetLogger()->Log(LOGGER_INFO, METHOD_PIN_START_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME, this->m_pName));

  CAMThread::CallWorker(CMD_EXIT);
  CAMThread::Close();

  FREE_MEM_CLASS(this->mediaPackets);
  FREE_MEM_CLASS(this->mediaTypes);
  FREE_MEM_CLASS(this->mediaTypeToSend);

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->m_pAllocator, this->m_pAllocator->Release());

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->mediaPacketsLock, CloseHandle(this->mediaPacketsLock));
  this->mediaPacketsLock = NULL;

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->filter, this->filter->GetLogger()->Log(LOGGER_INFO, METHOD_PIN_END_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME, this->m_pName));

  // filter is only reference, it is not needed to remove
  this->filter = NULL;
}

// IUnknown interface implementation

STDMETHODIMP CMPUrlSourceSplitterOutputPin::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
  CheckPointer(ppv, E_POINTER);

  *ppv = NULL;

  return 
    __super::NonDelegatingQueryInterface(riid, ppv);
}

// CBasePin methods

HRESULT CMPUrlSourceSplitterOutputPin::CheckMediaType(const CMediaType* pmt)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, pmt);

  if (SUCCEEDED(result))
  {
    result = E_INVALIDARG;

    for (unsigned int i = 0; i < this->mediaTypes->Count(); i++)
    {
      CMediaType *mediaType = this->mediaTypes->GetItem(i);

      if ((*mediaType) == (*pmt))
      {
        result = S_OK;
        break;
      }
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), this->filter->GetLogger()->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_CHECK_MEDIA_TYPE_NAME, this->m_pName, result));
  return result;
}

HRESULT CMPUrlSourceSplitterOutputPin::GetMediaType(int iPosition, CMediaType* pMediaType)
{
  HRESULT result = S_OK;

  CHECK_CONDITION_HRESULT(result, (iPosition < 0), E_INVALIDARG, result);
  CHECK_CONDITION_HRESULT(result, (((unsigned int)iPosition) >= this->mediaTypes->Count()), VFW_S_NO_MORE_ITEMS, result);

  if (result == S_OK)
  {
    *pMediaType = *this->mediaTypes->GetItem((unsigned int)iPosition);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), this->filter->GetLogger()->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_GET_MEDIA_TYPE_NAME, this->m_pName, result));
  return result;
}

STDMETHODIMP CMPUrlSourceSplitterOutputPin::Connect(IPin *pReceivePin, const AM_MEDIA_TYPE *pMediaType)
{
  HRESULT result = __super::Connect(pReceivePin, pMediaType);
  CHECK_CONDITION_EXECUTE(FAILED(result), this->filter->GetLogger()->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_CONNECT_NAME, this->m_pName, result));

  return result;
}

STDMETHODIMP CMPUrlSourceSplitterOutputPin::Notify(IBaseFilter* pSender, Quality q)
{
  return E_NOTIMPL;
}

// CBaseOutputPin methods

HRESULT CMPUrlSourceSplitterOutputPin::DecideAllocator(IMemInputPin * pPin, IMemAllocator **pAlloc)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, pPin);
  CHECK_POINTER_DEFAULT_HRESULT(result, pAlloc);

  if (SUCCEEDED(result))
  {
    ALLOCATOR_PROPERTIES prop;
    ZeroMemory(&prop, sizeof(ALLOCATOR_PROPERTIES));

    // ignore result of call pin's GetAllocatorRequirements() method
    pPin->GetAllocatorRequirements(&prop);

    if (SUCCEEDED(result))
    {
      // if input pin doesn't care about alignment, then set it to 1
      if (prop.cbAlign == 0)
      {
        prop.cbAlign = 1;
      }

      *pAlloc = new COutputPinAllocator(NULL, &result);
      CHECK_POINTER_HRESULT(result, *pAlloc, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        (*pAlloc)->AddRef();
        result = this->DecideBufferSize(*pAlloc, &prop);

        if (SUCCEEDED(result))
        {
          result = pPin->NotifyAllocator(*pAlloc, FALSE);
        }
      }

      if (FAILED(result))
      {
        // there is problem with our allocator or pin doesn't accept our allocator
        // try to use pin's allocator

        CHECK_CONDITION_NOT_NULL_EXECUTE(*pAlloc, (*pAlloc)->Release());
        *pAlloc = NULL;

        result = pPin->GetAllocator(pAlloc);
        if (SUCCEEDED(result))
        {
          result = this->DecideBufferSize(*pAlloc, &prop);

          if (SUCCEEDED(result))
          {
            result = pPin->NotifyAllocator(*pAlloc, TRUE);
          }
        }
      }

      if (FAILED(result))
      {
        (*pAlloc)->Release();
        *pAlloc = NULL;
      }
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), this->filter->GetLogger()->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_DECIDE_ALLOCATOR_NAME, this->m_pName, result));
  return result;
}

HRESULT CMPUrlSourceSplitterOutputPin::DecideBufferSize(IMemAllocator* pAlloc, ALLOCATOR_PROPERTIES* pProperties)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, pAlloc);
  CHECK_POINTER_DEFAULT_HRESULT(result, pProperties);

  if (SUCCEEDED(result))
  {
    pProperties->cBuffers = max(pProperties->cBuffers, OUTPUT_PIN_BUFFERS_RECOMMENDED);
    pProperties->cbBuffer = max(pProperties->cbBuffer, OUTPUT_PIN_BUFFERS_LENGTH_RECOMMENDED);
    pProperties->cbAlign = ALLOCATOR_ALIGNMENT_REQUIRED;
    pProperties->cbPrefix = ALLOCATOR_PREFIX_REQUIRED;

    // sanity checks
    ALLOCATOR_PROPERTIES test;
    CHECK_HRESULT_EXECUTE(result, pAlloc->SetProperties(pProperties, &test));
    CHECK_CONDITION_HRESULT(result, (test.cbBuffer < pProperties->cbBuffer), E_FAIL, result);
    CHECK_CONDITION_HRESULT(result, (test.cBuffers >= pProperties->cBuffers), result, E_OUTOFMEMORY);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), this->filter->GetLogger()->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_DECIDE_BUFFER_SIZE_NAME, this->m_pName, result));
  return result;
}

HRESULT CMPUrlSourceSplitterOutputPin::Active()
{
  CAutoLock cAutoLock(m_pLock);
  HRESULT result = S_OK;

  if (this->IsConnected())
  {
    result = CAMThread::Create() ? result : E_FAIL;
  }

  CHECK_HRESULT_EXECUTE(result, __super::Active());

  CHECK_CONDITION_EXECUTE(FAILED(result), this->filter->GetLogger()->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_ACTIVE_NAME, this->m_pName, result));
  return result;
}

HRESULT CMPUrlSourceSplitterOutputPin::Inactive()
{
  CAutoLock cAutoLock(m_pLock);
  HRESULT result = S_OK;

  CAMThread::CallWorker(CMD_EXIT);
  CAMThread::Close();

  // clear media packets, we are going inactive
  this->mediaPackets->Clear();

  CHECK_HRESULT_EXECUTE(result, __super::Inactive());

  CHECK_CONDITION_EXECUTE(FAILED(result), this->filter->GetLogger()->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_INACTIVE_NAME, this->m_pName, result));
  return result;
}

HRESULT CMPUrlSourceSplitterOutputPin::DeliverBeginFlush()
{
  HRESULT result = S_OK;

  this->flushing = true;
  result = this->IsConnected() ? this->GetConnected()->BeginFlush() : S_OK;
  CAMThread::CallWorker(CMD_BEGIN_FLUSH);

  // clear all media packets
  this->mediaPackets->Clear();

  return result;
}

HRESULT CMPUrlSourceSplitterOutputPin::DeliverEndFlush()
{
  HRESULT result = S_OK;

  CAMThread::CallWorker(CMD_END_FLUSH);
  result = this->IsConnected() ? this->GetConnected()->EndFlush() : S_OK;
  this->flushing = false;

  return result;
}

HRESULT CMPUrlSourceSplitterOutputPin::QueuePacket(COutputPinPacket *packet, DWORD timeout)
{
  HRESULT result = S_OK;

  {
    CLockMutex lock(this->mediaPacketsLock, timeout);
    result = (lock.IsLocked()) ? S_OK : VFW_E_TIMEOUT;

    if (SUCCEEDED(result))
    {
      if (this->mediaTypeToSend != NULL)
      {
        packet->SetMediaType(CreateMediaType(this->mediaTypeToSend));
        FREE_MEM_CLASS(this->mediaTypeToSend);
      }

      result = this->mediaPackets->Add(packet) ? result : E_FAIL;
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitterOutputPin::QueueEndOfStream()
{
  HRESULT result = S_OK;
  this->filter->GetLogger()->Log(LOGGER_INFO, METHOD_PIN_START_FORMAT, MODULE_NAME, METHOD_QUEUE_END_OF_STREAM_NAME, this->m_pName);

  COutputPinPacket *endOfStream = new COutputPinPacket();
  CHECK_POINTER_HRESULT(result, endOfStream, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    endOfStream->SetEndOfStream(true);

    result = this->QueuePacket(endOfStream, INFINITE);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(endOfStream));

  this->filter->GetLogger()->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_QUEUE_END_OF_STREAM_NAME, this->m_pName, result);
  return result;
}

HRESULT CMPUrlSourceSplitterOutputPin::DeliverNewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate)
{
  HRESULT result = S_OK;

  if (ThreadExists())
  {
    result = __super::DeliverNewSegment(tStart, tStop, dRate);

    /*if (SUCCEEDED(result))
    {
      MakeISCRHappy();
    }*/
  }

  return result;
}

/* get methods */

unsigned int CMPUrlSourceSplitterOutputPin::GetStreamPid(void)
{
  return this->streamPid;
}

/* set methods */

void CMPUrlSourceSplitterOutputPin::SetStreamPid(unsigned int streamPid)
{
  this->streamPid = streamPid;
}

bool CMPUrlSourceSplitterOutputPin::SetNewMediaTypes(CMediaTypeCollection *mediaTypes)
{
  this->mediaTypes->Clear();
  return this->mediaTypes->Append(mediaTypes);
}

/* other methods */

HRESULT CMPUrlSourceSplitterOutputPin::DeliverPlay()
{
  CAMThread::CallWorker(CMD_PLAY);
  return S_OK;
}

HRESULT CMPUrlSourceSplitterOutputPin::DeliverPause()
{
  CAMThread::CallWorker(CMD_PAUSE);
  return S_OK;
}

HRESULT CMPUrlSourceSplitterOutputPin::SendMediaType(CMediaType *mediaType)
{
  HRESULT result = S_OK;
  FREE_MEM_CLASS(this->mediaTypeToSend);
  this->mediaTypeToSend = new CMediaType(*mediaType, &result);
  CHECK_POINTER_HRESULT(result, this->mediaTypeToSend, result, E_OUTOFMEMORY);

  return result;
}

/* protected methods */

DWORD CMPUrlSourceSplitterOutputPin::ThreadProc()
{
  SetThreadName(-1, "CMPUrlSourceSplitterOutputPin ThreadProc");
  SetThreadPriority(this->m_hThread, THREAD_PRIORITY_BELOW_NORMAL);

  for(DWORD cmd = (DWORD)-1; ; cmd = GetRequest())
  {
    switch (cmd)
    {
    case CMD_EXIT:
      this->filter->GetLogger()->Log(LOGGER_INFO, METHOD_PIN_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, L"CMD_EXIT");
      break;
    case CMD_BEGIN_FLUSH:
      this->filter->GetLogger()->Log(LOGGER_INFO, METHOD_PIN_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, L"CMD_BEGIN_FLUSH");
      break;
    case CMD_END_FLUSH:
      this->filter->GetLogger()->Log(LOGGER_INFO, METHOD_PIN_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, L"CMD_END_FLUSH");
      break;
    case CMD_PLAY:
      this->filter->GetLogger()->Log(LOGGER_INFO, METHOD_PIN_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, L"CMD_PLAY");
      break;
    case CMD_PAUSE:
      this->filter->GetLogger()->Log(LOGGER_INFO, METHOD_PIN_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, L"CMD_PAUSE");
      break;
    case (DWORD)-1:
      // ignore, it means no command
      this->filter->GetLogger()->Log(LOGGER_INFO, METHOD_PIN_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, L"no command");
      break;
    default:
      this->filter->GetLogger()->Log(LOGGER_INFO, L"%s: %s: pin '%s', unknown command: %d", MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, cmd);
      break;
    }

    if (cmd == CMD_EXIT)
    {
      Reply(S_OK);
      break;
    }

    if (cmd != (DWORD)-1)
    {
      Reply(S_OK);
    }

    // sleep will be disabled in case of media packets ready to delivery
    bool sleepAllowed = true;
    while (!CheckRequest(&cmd))
    {
      sleepAllowed = true;

      if (cmd == CMD_PLAY)
      {
        if (!this->flushing)
        {
          HRESULT result = S_OK;
          COutputPinPacket *packet = NULL;

          {
            // wait only 10 ms to lock mutex
            // if it fail, just wait
            CLockMutex lock(this->mediaPacketsLock, 10);

            if (lock.IsLocked())
            {
              if (this->mediaPackets->Count() > 0)
              {
                packet = this->mediaPackets->GetItem(0);
              }
            }
          }

          if (packet != NULL)
          {
            if (packet->IsEndOfStream())
            {
              this->DeliverEndOfStream();

              if (SUCCEEDED(result))
              {
                CLockMutex lock(this->mediaPacketsLock, INFINITE);

                // remove processed packet
                this->mediaPackets->Remove(0);
              }
            }
            else
            {
              ASSERT(packet->GetBuffer() != NULL);
              IMediaSample *sample = NULL;
              bool notDeletePacket = false;

              result = this->GetDeliveryBuffer(&sample, NULL, NULL, AM_GBF_NOWAIT);

              if (SUCCEEDED(result))
              {
                IOutputPinMediaSample *outputPinMediaSample = NULL;

                result = sample->QueryInterface(&outputPinMediaSample);

                if (SUCCEEDED(result))
                {
                  // using our allocator
                  CHECK_HRESULT_EXECUTE(result, outputPinMediaSample->SetPacket(packet));

                  COM_SAFE_RELEASE(outputPinMediaSample);
                }
                else if (result == E_NOINTERFACE)
                {
                  // doesn't using our allocator
                  result = COutputPinMediaSample::SetPacket(sample, packet);

                  if (SUCCEEDED(result))
                  {
                    if (packet->GetBuffer()->GetBufferOccupiedSpace() != result)
                    {
                      packet->GetBuffer()->RemoveFromBuffer(result);
                      notDeletePacket = true;
                    }
                  }
                }
              }

              CHECK_HRESULT_EXECUTE(result, this->Deliver(sample));
              CHECK_CONDITION_NOT_NULL_EXECUTE(sample, COM_SAFE_RELEASE(sample));

              if (SUCCEEDED(result) && (!notDeletePacket))
              {
                CLockMutex lock(this->mediaPacketsLock, INFINITE);

                // remove processed packet
                this->mediaPackets->Remove(0);
              }

              if (result == VFW_E_TIMEOUT)
              {
                // it just means that no buffer is free
                // just wait for free buffer
                result = S_OK;
              }
            }

            // don't sleep and process next output packet
            sleepAllowed = false;
          }

          if (FAILED(result))
          {
            this->filter->GetLogger()->Log(LOGGER_ERROR, L"%s: %s: pin '%s', result: 0x%08X", MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, result);
          }
        }
      }

      // we don't do anything in CMD_BEGIN_FLUSH or CMD_END_FLUSH command

      CHECK_CONDITION_EXECUTE(sleepAllowed, Sleep(1));
    }
  }

  return S_OK;
}
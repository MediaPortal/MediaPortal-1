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

#include "moreuuids.h"
#include "H264Nalu.h"

#pragma warning( push )
#pragma warning( disable : 4018 )
#pragma warning( disable : 4244 )
extern "C" {
#define AVCODEC_X86_MATHOPS_H
#include "libavcodec/get_bits.h"
}
#pragma warning( pop )

#define AAC_ADTS_HEADER_SIZE                                      7

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

CMPUrlSourceSplitterOutputPin::CMPUrlSourceSplitterOutputPin(CMediaTypeCollection *mediaTypes, LPCWSTR pName, CBaseFilter *pFilter, CCritSec *pLock, HRESULT *phr, const wchar_t *containerFormat)
  : CBaseOutputPin(NAME("MediaPortal Url Source Splitter Output Pin"), pFilter, pLock, phr, pName)
{
  this->mediaPackets = NULL;
  this->filter = NULL;
  this->mediaTypes = NULL;
  this->streamPid = STREAM_PID_UNSPECIFIED;
  this->flushing = false;
  this->mediaTypeToSend = NULL;
  this->flags = OUTPUT_PIN_FLAG_NONE;
  this->mediaTypeSubType = GUID_NULL;
  this->h264Buffer = new COutputPinPacket();
  this->h264PacketCollection = new COutputPinPacketCollection();
  this->outputPinDataLength = 0;

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
      CHECK_POINTER_HRESULT(*phr, this->h264Buffer, *phr, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*phr, this->h264PacketCollection, *phr, E_OUTOFMEMORY);

      if (SUCCEEDED(*phr))
      {
        *phr = this->mediaTypes->Append(mediaTypes) ? (*phr) : E_OUTOFMEMORY;
      }

      if (SUCCEEDED(*phr) && (containerFormat != NULL))
      {
        this->flags |= (wcscmp(L"mpegts", containerFormat) == 0) ? OUTPUT_PIN_FLAG_CONTAINER_MPEG_TS : OUTPUT_PIN_FLAG_NONE;
        this->flags |= (wcscmp(L"mpeg", containerFormat) == 0) ? OUTPUT_PIN_FLAG_CONTAINER_MPEG : OUTPUT_PIN_FLAG_NONE;
        this->flags |= (wcscmp(L"wtv", containerFormat) == 0) ? OUTPUT_PIN_FLAG_CONTAINER_WTV : OUTPUT_PIN_FLAG_NONE;
        this->flags |= (wcscmp(L"asf", containerFormat) == 0) ? OUTPUT_PIN_FLAG_CONTAINER_ASF : OUTPUT_PIN_FLAG_NONE;
        this->flags |= (wcscmp(L"ogg", containerFormat) == 0) ? OUTPUT_PIN_FLAG_CONTAINER_OGG : OUTPUT_PIN_FLAG_NONE;
        this->flags |= (wcscmp(L"matroska", containerFormat) == 0) ? OUTPUT_PIN_FLAG_CONTAINER_MATROSKA : OUTPUT_PIN_FLAG_NONE;
        this->flags |= (wcscmp(L"avi", containerFormat) == 0) ? OUTPUT_PIN_FLAG_CONTAINER_AVI : OUTPUT_PIN_FLAG_NONE;
        this->flags |= (wcscmp(L"mp4", containerFormat) == 0) ? OUTPUT_PIN_FLAG_CONTAINER_MP4 : OUTPUT_PIN_FLAG_NONE;
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
  FREE_MEM_CLASS(this->h264Buffer);
  FREE_MEM_CLASS(this->h264PacketCollection);

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

  uint64_t flushedDataLength = 0;
  for (unsigned int i = 0; i < this->mediaPackets->Count(); i++)
  {
    if (this->mediaPackets->GetItem(i)->GetBuffer() != NULL)
    {
      flushedDataLength += (uint64_t)this->mediaPackets->GetItem(i)->GetBuffer()->GetBufferOccupiedSpace();
    }
  }

  this->filter->GetLogger()->Log(LOGGER_INFO, L"%s: %s: pin '%s', sent data length: %llu", MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, this->outputPinDataLength);
  this->filter->GetLogger()->Log(LOGGER_INFO, L"%s: %s: pin '%s', flushed data length: %llu", MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, flushedDataLength);

  this->outputPinDataLength = 0;

  // clear all media packets
  this->mediaPackets->Clear();

  // flush parser
  FREE_MEM_CLASS(this->h264Buffer);
  this->h264PacketCollection->Clear();

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

      if (packet->IsEndOfStream())
      {
        // add packet to output packet collection
        result = this->mediaPackets->Add(packet) ? result : E_OUTOFMEMORY;
      }
      else
      {
        // parse packet (if necessary)
        result = this->Parse(this->m_mt.subtype, packet);
      }
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

bool CMPUrlSourceSplitterOutputPin::IsContainerMpegTs(void)
{
  return this->IsFlags(OUTPUT_PIN_FLAG_CONTAINER_MPEG_TS);
}

bool CMPUrlSourceSplitterOutputPin::IsContainerMpeg(void)
{
  return this->IsFlags(OUTPUT_PIN_FLAG_CONTAINER_MPEG);
}

bool CMPUrlSourceSplitterOutputPin::IsContainerWtv(void)
{
  return this->IsFlags(OUTPUT_PIN_FLAG_CONTAINER_WTV);
}

bool CMPUrlSourceSplitterOutputPin::IsContainerAsf(void)
{
  return this->IsFlags(OUTPUT_PIN_FLAG_CONTAINER_ASF);
}

bool CMPUrlSourceSplitterOutputPin::IsContainerOgg(void)
{
  return this->IsFlags(OUTPUT_PIN_FLAG_CONTAINER_OGG);
}

bool CMPUrlSourceSplitterOutputPin::IsContainerMatroska(void)
{
  return this->IsFlags(OUTPUT_PIN_FLAG_CONTAINER_MATROSKA);
}

bool CMPUrlSourceSplitterOutputPin::IsContainerAvi(void)
{
  return this->IsFlags(OUTPUT_PIN_FLAG_CONTAINER_AVI);
}

bool CMPUrlSourceSplitterOutputPin::IsContainerMp4(void)
{
  return this->IsFlags(OUTPUT_PIN_FLAG_CONTAINER_MP4);
}

bool CMPUrlSourceSplitterOutputPin::HasAccessUnitDelimiters(void)
{
  return this->IsFlags(OUTPUT_PIN_FLAG_HAS_ACCESS_UNIT_DELIMITERS);
}

bool CMPUrlSourceSplitterOutputPin::IsPGSDropState(void)
{
  return this->IsFlags(OUTPUT_PIN_FLAG_PGS_DROP_STATE);
}

bool CMPUrlSourceSplitterOutputPin::IsFlags(unsigned int flags)
{
  return ((this->flags & flags) == flags);
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

                  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->outputPinDataLength += (uint64_t)packet->GetBuffer()->GetBufferOccupiedSpace());
                }
                else if (result == E_NOINTERFACE)
                {
                  // doesn't using our allocator
                  result = COutputPinMediaSample::SetPacket(sample, packet);

                  if (SUCCEEDED(result))
                  {
                    this->outputPinDataLength += (uint64_t)result;

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

  this->filter->GetLogger()->Log(LOGGER_INFO, L"%s: %s: pin '%s', sent data length: %llu", MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, this->outputPinDataLength);

  return S_OK;
}

#define MOVE_TO_H264_START_CODE(b, e) while(b <= e-4 && !((*(DWORD *)b == 0x01000000) || ((*(DWORD *)b & 0x00FFFFFF) == 0x00010000))) b++; if((b <= e-4) && *(DWORD *)b == 0x01000000) b++;

HRESULT CMPUrlSourceSplitterOutputPin::Parse(GUID subType, COutputPinPacket *packet)
{
  HRESULT result = (packet != NULL) ? S_OK : E_INVALIDARG;

  if (SUCCEEDED(result) && (subType != this->mediaTypeSubType))
  {
    this->mediaTypeSubType = subType;

    FREE_MEM_CLASS(this->h264Buffer);
    this->h264PacketCollection->Clear();
  }

  if (SUCCEEDED(result))
  {
    if (packet->IsPacketParsed())
    {
      // add packet to output packet collection
      result = this->mediaPackets->Add(packet) ? result : E_OUTOFMEMORY;
    }
    else if (this->mediaTypeSubType == MEDIASUBTYPE_AVC1 &&
      (this->IsContainerMpegTs() || this->IsContainerMpeg() || this->IsContainerWtv() || this->IsContainerAsf() || ((this->IsContainerOgg() || this->IsContainerMatroska()) && packet->IsH264AnnexB())))
    {
      if (this->h264Buffer == NULL)
      {
        // initialize H264 Annex B buffer with current output pin packet data
        this->h264Buffer = new COutputPinPacket();
        CHECK_POINTER_HRESULT(result, this->h264Buffer, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, this->h264Buffer->CreateBuffer(packet->GetBuffer()->GetBufferSize()), result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          // copy packet data to H264 buffer
          this->h264Buffer->SetStreamPid(packet->GetStreamPid());
          this->h264Buffer->SetDiscontinuity(packet->IsDiscontinuity());
          this->h264Buffer->SetSyncPoint(packet->IsSyncPoint());
          this->h264Buffer->SetStartTime(packet->GetStartTime());
          this->h264Buffer->SetEndTime(packet->GetEndTime());
          this->h264Buffer->SetMediaType(packet->GetMediaType());

          // reset incoming packet data
          packet->SetDiscontinuity(false);
          packet->SetSyncPoint(false);
          packet->SetStartTime(COutputPinPacket::INVALID_TIME);
          packet->SetEndTime(COutputPinPacket::INVALID_TIME);
          packet->SetMediaType(NULL);
        }

        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(this->h264Buffer));
      }

      // add packet data to H264 buffer (in case of error, no data are added)
      CHECK_CONDITION_HRESULT(result, this->h264Buffer->GetBuffer()->AddToBufferWithResize(packet->GetBuffer()) == packet->GetBuffer()->GetBufferOccupiedSpace(), result, E_OUTOFMEMORY);

      if (SUCCEEDED(result) && (this->h264Buffer->GetBuffer()->GetBufferOccupiedSpace() > 0))
      {
        unsigned int bufferSize = this->h264Buffer->GetBuffer()->GetBufferOccupiedSpace();
        ALLOC_MEM_DEFINE_SET(buffer, unsigned char, bufferSize, 0);
        CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          this->h264Buffer->GetBuffer()->CopyFromBuffer(buffer, bufferSize);

          unsigned char *start = buffer;
          unsigned char *end = buffer + bufferSize;

          MOVE_TO_H264_START_CODE(start, end);

          unsigned int h264PacketCollectionCount = this->h264PacketCollection->Count();
          while (SUCCEEDED(result) && (start <= (end - 4)))
          {
            unsigned char *next = start + 1;
            MOVE_TO_H264_START_CODE(next, end);

            // end of buffer reached
            if (next >= (end - 4))
            {
              break;
            }

            unsigned int size = next - start;
            CH264Nalu nalu;
            nalu.SetBuffer(start, size, 0);

            COutputPinPacket *packetToCollection = new COutputPinPacket();
            CHECK_POINTER_HRESULT(result, packetToCollection, result, E_OUTOFMEMORY);
            CHECK_CONDITION_HRESULT(result, packetToCollection->CreateBuffer(this->h264Buffer->GetBuffer()->GetBufferOccupiedSpace()), result, E_OUTOFMEMORY);

            while (SUCCEEDED(result) && nalu.ReadNext())
            {
              unsigned int tempSize = nalu.GetDataLength() + 4;
              ALLOC_MEM_DEFINE_SET(temp, unsigned char, tempSize, 0);
              CHECK_POINTER_HRESULT(result, temp, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                // write size of the NALU in big endian
                AV_WB32(temp, nalu.GetDataLength());
                memcpy(temp + 4, nalu.GetDataBuffer(), nalu.GetDataLength());

                result = (packetToCollection->GetBuffer()->AddToBufferWithResize(temp, tempSize) == tempSize) ? result : E_OUTOFMEMORY;
              }

              FREE_MEM(temp);
            }

            if (FAILED(result) || (packetToCollection->GetBuffer()->GetBufferOccupiedSpace() == 0))
            {
              // no data or error
              FREE_MEM_CLASS(packetToCollection);
              break;
            }

            if (SUCCEEDED(result))
            {
              // no error and we have some data

              packetToCollection->SetStreamPid(this->h264Buffer->GetStreamPid());
              packetToCollection->SetDiscontinuity(this->h264Buffer->IsDiscontinuity());
              packetToCollection->SetSyncPoint(this->h264Buffer->IsSyncPoint());
              packetToCollection->SetStartTime(this->h264Buffer->GetStartTime());
              packetToCollection->SetEndTime(this->h264Buffer->GetEndTime());
              packetToCollection->SetMediaType(this->h264Buffer->GetMediaType());

              this->h264Buffer->SetDiscontinuity(false);
              this->h264Buffer->SetSyncPoint(false);
              this->h264Buffer->SetStartTime(COutputPinPacket::INVALID_TIME);
              this->h264Buffer->SetEndTime(COutputPinPacket::INVALID_TIME);
              this->h264Buffer->SetMediaType(NULL);

              // add to H264 packet collection
              result = this->h264PacketCollection->Add(packetToCollection) ? result : E_OUTOFMEMORY;

              if (SUCCEEDED(result))
              {
                if (packet->GetStartTime() != COutputPinPacket::INVALID_TIME)
                {
                  this->h264Buffer->SetStartTime(packet->GetStartTime());
                  this->h264Buffer->SetEndTime(packet->GetEndTime());

                  packet->SetStartTime(COutputPinPacket::INVALID_TIME);
                  packet->SetEndTime(COutputPinPacket::INVALID_TIME);
                }

                if (packet->IsDiscontinuity())
                {
                  this->h264Buffer->SetDiscontinuity(true);
                  packet->SetDiscontinuity(false);
                }

                if (packet->IsSyncPoint())
                {
                  this->h264Buffer->SetSyncPoint(true);
                  packet->SetSyncPoint(false);
                }

                if (this->h264Buffer->GetMediaType() != NULL)
                {
                  DeleteMediaType(this->h264Buffer->GetMediaType());
                  this->h264Buffer->SetMediaType(NULL);
                }

                this->h264Buffer->SetMediaType(packet->GetMediaType());
                packet->SetMediaType(NULL);
              }
            }

            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packetToCollection));

            start = next;
          }

          if (SUCCEEDED(result))
          {
            if (start > buffer)
            {
              this->h264Buffer->GetBuffer()->RemoveFromBufferAndMove(start - buffer);
            }
          }
          else
          {
            // error occured, clean this->h264PacketCollection to previous state
            while (this->h264PacketCollection->Count() != h264PacketCollectionCount)
            {
              this->h264PacketCollection->Remove(this->h264PacketCollection->Count() - 1);
            }
          }
        }

        FREE_MEM(buffer);
      }

      // if no error, delete processed packet
      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), FREE_MEM_CLASS(packet));

      // process H264 packet collection and queue output packets if possible

      if (SUCCEEDED(result))
      {
        // output packet is processed
        // any error in next code is ignored, this->h264PacketCollection will be processed with next Parse() call

        unsigned int nextPacketIndex = 0;
        do
        {
          REFERENCE_TIME packetStart = COutputPinPacket::INVALID_TIME;
          REFERENCE_TIME packetEnd = COutputPinPacket::INVALID_TIME;
          nextPacketIndex = 0;

          // skip first packet
          for (unsigned int i = 1; (SUCCEEDED(result) && (i < this->h264PacketCollection->Count())); i++)
          {
            COutputPinPacket *temp = this->h264PacketCollection->GetItem(i);
            ALLOC_MEM_DEFINE_SET(buffer, unsigned char, temp->GetBuffer()->GetBufferOccupiedSpace(), 0);
            CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              temp->GetBuffer()->CopyFromBuffer(buffer, temp->GetBuffer()->GetBufferOccupiedSpace());

              if ((buffer[4] & 0x1F) == 0x09)
              {
                this->SetHasAccessUnitDelimiters(true);
              }

              if (((buffer[4] & 0x1F) == 0x09) || ((!this->HasAccessUnitDelimiters()) && (temp->GetStartTime() != COutputPinPacket::INVALID_TIME)))
              {
                nextPacketIndex = i;

                if ((temp->GetStartTime() == COutputPinPacket::INVALID_TIME) && (packetStart != COutputPinPacket::INVALID_TIME))
                {
                  temp->SetStartTime(packetStart);
                  temp->SetEndTime(packetEnd);
                }

                break;
              }

              if (packetStart == COutputPinPacket::INVALID_TIME)
              {
                packetStart = temp->GetStartTime();
                packetEnd = temp->GetEndTime();
              }
            }

            FREE_MEM(buffer);
          }

          if (SUCCEEDED(result) && (nextPacketIndex != 0))
          {
            COutputPinPacket *queuePacket = new COutputPinPacket();
            CHECK_POINTER_HRESULT(result, queuePacket, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              // count needed memory for output packet
              unsigned int neededSpace = 0;
              for (unsigned int i = 0; (SUCCEEDED(result) && (i < nextPacketIndex)); i++)
              {
                COutputPinPacket *temp = this->h264PacketCollection->GetItem(i);

                neededSpace += temp->GetBuffer()->GetBufferOccupiedSpace();
              }

              // copy data from first packet in H264 collection
              COutputPinPacket *firstPacket = this->h264PacketCollection->GetItem(0);
              result = queuePacket->CreateBuffer(neededSpace) ? result : E_OUTOFMEMORY;

              if (SUCCEEDED(result))
              {
                queuePacket->GetBuffer()->AddToBufferWithResize(firstPacket->GetBuffer());

                queuePacket->SetStartTime(firstPacket->GetStartTime());
                queuePacket->SetEndTime(firstPacket->GetEndTime());
                queuePacket->SetStreamPid(firstPacket->GetStreamPid());
                queuePacket->SetMediaType(firstPacket->GetMediaType());
                // clear media type in first packet to avoid of crash in freeing memory
                firstPacket->SetMediaType(NULL);
                queuePacket->SetFlags(firstPacket->GetFlags());
              }
            }

            // copy data from H264 packet collection until next packet index
            for (unsigned int i = 1; (SUCCEEDED(result) && (i < nextPacketIndex)); i++)
            {
              COutputPinPacket *temp = this->h264PacketCollection->GetItem(i);
              queuePacket->GetBuffer()->AddToBufferWithResize(temp->GetBuffer());
            }

            // add packet to output collection
            CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->mediaPackets->Add(queuePacket) ? result : E_OUTOFMEMORY);

            // delete processed H264 packets
            for (unsigned int i = 0; (SUCCEEDED(result) && (i < nextPacketIndex)); i++)
            {
              this->h264PacketCollection->Remove(0);
            }

            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(queuePacket));
          }
        }
        while (nextPacketIndex != 0);

        // ignore error, output packet is already processed
        result = S_OK;
      }
    }
    else if (this->mediaTypeSubType == MEDIASUBTYPE_HDMVSUB)
    {
      unsigned int bufferSize = packet->GetBuffer()->GetBufferOccupiedSpace();
      ALLOC_MEM_DEFINE_SET(buffer, unsigned char, bufferSize, 0);
      CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

      unsigned int pgsBufferOccupied = 0;
      ALLOC_MEM_DEFINE_SET(pgsBuffer, unsigned char, bufferSize, 0);
      CHECK_POINTER_HRESULT(result, pgsBuffer, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        packet->GetBuffer()->CopyFromBuffer(buffer, bufferSize);

        unsigned char *bufferStart = buffer;
        unsigned char *bufferEnd = buffer + bufferSize;
        unsigned char segmentType;
        unsigned int segmentLength;

        if (bufferSize < 3)
        {
          // too short PGS packet
          // if no error, flag packet as parsed
          // if error occur while adding to collection, next time it will be directly added to collection without parsing
          packet->SetFlags(packet->GetFlags() | OUTPUT_PIN_PACKET_FLAG_PACKET_PARSED);

          // add packet to output packet collection
          result = this->mediaPackets->Add(packet) ? result : E_OUTOFMEMORY;
        }
        else
        {
          while ((bufferStart + 3) < bufferEnd)
          {
            const unsigned char *segmentStart = bufferStart;
            const unsigned int segmentBufferLength = bufferEnd - bufferStart;

            segmentType = AV_RB8(bufferStart);
            segmentLength = AV_RB16(bufferStart + 1);

            if (segmentLength > (segmentBufferLength - 3))
            {
              // segment length is bigger then input buffer
              segmentLength = segmentBufferLength - 3;
            }

            bufferStart += 3;

            // presentation segment
            if ((segmentType == 0x16) && (segmentLength > 10))
            {
              // segment layout
              // 2 bytes width
              // 2 bytes height
              // 1 unknown byte
              // 2 bytes id
              // 1 byte composition state (0x00 = normal, 0x40 = ACQU_POINT (?), 0x80 = epoch start (new frame), 0xC0 = epoch continue)
              // 2 unknown bytes
              // 1 byte object number

              unsigned char objectNumber = bufferStart[10];

              if (objectNumber == 0)
              {
                this->SetPGSDropState(false);
              }
              else if (segmentLength >= 0x13)
              {
                // 1 byte window_id
                // 1 byte object_cropped_flag: 0x80, forced_on_flag = 0x040, 6bit reserved
                unsigned char forcedFlag = bufferStart[14];
                this->SetPGSDropState(!(forcedFlag & 0x40));
                // 2 bytes x
                // 2 bytes y
                // total length = 19 bytes
              }
            }

            if (!this->IsPGSDropState())
            {
              memcpy(pgsBuffer + pgsBufferOccupied, segmentStart, segmentLength + 3);
              pgsBufferOccupied += segmentLength + 3;
            }

            bufferStart += segmentLength;
          }

          if (pgsBufferOccupied > 0)
          {
            packet->GetBuffer()->ClearBuffer();
            packet->GetBuffer()->AddToBuffer(pgsBuffer, pgsBufferOccupied);
          }
          else
          {
            FREE_MEM_CLASS(packet);
          }
        }
      }

      FREE_MEM(buffer);
      FREE_MEM(pgsBuffer);
    }
    else if (this->mediaTypeSubType == MEDIASUBTYPE_HDMV_LPCM_AUDIO)
    {
      // add packet to output packet collection, if successful, change it's data
      result = this->mediaPackets->Add(packet) ? result : E_OUTOFMEMORY;

      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), packet->GetBuffer()->RemoveFromBuffer(4));
    }
    else if (packet->IsPacketMovText())
    {
      unsigned int bufferSize = packet->GetBuffer()->GetBufferOccupiedSpace();

      if (bufferSize > 2)
      {
        ALLOC_MEM_DEFINE_SET(buffer, unsigned char, bufferSize, 0);
        CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          packet->GetBuffer()->CopyFromBuffer(buffer, bufferSize);
          unsigned int size = (buffer[0] << 8) | buffer[1];

          if (size <= (bufferSize - 2))
          {
            packet->GetBuffer()->ClearBuffer();
            packet->GetBuffer()->AddToBuffer(buffer + 2, size);

            // if no error, flag packet as parsed
            // if error occur while adding to collection, next time it will be directly added to collection without parsing
            packet->SetFlags(packet->GetFlags() | OUTPUT_PIN_PACKET_FLAG_PACKET_PARSED);

            // add packet to output packet collection
            result = this->mediaPackets->Add(packet) ? result : E_OUTOFMEMORY;
          }
          else
          {
            FREE_MEM_CLASS(packet);
          }
        }

        FREE_MEM(buffer);
      }
      else
      {
        FREE_MEM_CLASS(packet);
      }
    }
    else if (this->mediaTypeSubType == MEDIASUBTYPE_AAC && ((!this->IsContainerMatroska()) && (!this->IsContainerMp4())))
    {
      unsigned int bufferSize = packet->GetBuffer()->GetBufferOccupiedSpace();
      ALLOC_MEM_DEFINE_SET(buffer, unsigned char, bufferSize, 0);
      CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        packet->GetBuffer()->CopyFromBuffer(buffer, bufferSize);

        GetBitContext gb;
        init_get_bits(&gb, buffer, AAC_ADTS_HEADER_SIZE * 8);

        // check if its really ADTS
        if (get_bits(&gb, 12) == 0xFFF)
        {
          skip_bits1(&gb);              /* id */
          skip_bits(&gb, 2);            /* layer */
          int crc_abs = get_bits1(&gb); /* protection_absent */

          packet->GetBuffer()->RemoveFromBuffer(AAC_ADTS_HEADER_SIZE + 2*!crc_abs);
        }

        // if no error, flag packet as parsed
        // if error occur while adding to collection, next time it will be directly added to collection without parsing
        packet->SetFlags(packet->GetFlags() | OUTPUT_PIN_PACKET_FLAG_PACKET_PARSED);
      }

      FREE_MEM(buffer);

      CHECK_CONDITION_HRESULT(result, this->mediaPackets->Add(packet), result, E_OUTOFMEMORY);
    }
    else
    {
      // add packet to output packet collection
      result = this->mediaPackets->Add(packet) ? result : E_OUTOFMEMORY;
    }
  }

  return result;
}

void CMPUrlSourceSplitterOutputPin::SetHasAccessUnitDelimiters(bool hasAccessUnitDelimiters)
{
  this->flags &= ~OUTPUT_PIN_FLAG_HAS_ACCESS_UNIT_DELIMITERS;
  this->flags |= (hasAccessUnitDelimiters) ? OUTPUT_PIN_FLAG_HAS_ACCESS_UNIT_DELIMITERS : OUTPUT_PIN_FLAG_NONE;
}

void CMPUrlSourceSplitterOutputPin::SetPGSDropState(bool pgsDropState)
{
  this->flags &= ~OUTPUT_PIN_FLAG_PGS_DROP_STATE;
  this->flags |= (pgsDropState) ? OUTPUT_PIN_FLAG_PGS_DROP_STATE : OUTPUT_PIN_FLAG_NONE;
}
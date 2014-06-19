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
#include "StaticLogger.h"
#include "Parameters.h"

#include <Shlwapi.h>

#ifdef _DEBUG
#define MODULE_NAME                                               L"MPUrlSourceSplitterOutputPind"
#else
#define MODULE_NAME                                               L"MPUrlSourceSplitterOutputPin"
#endif

CMPUrlSourceSplitterOutputPin::CMPUrlSourceSplitterOutputPin(LPCWSTR pName, CBaseFilter *pFilter, CCritSec *pLock, HRESULT *phr, CLogger *logger, CParameterCollection *parameters, CMediaTypeCollection *mediaTypes)
  : CBaseOutputPin(NAME("MediaPortal Url Source Splitter Output Pin"), pFilter, pLock, phr, pName), CFlags()
{
  this->mediaPackets = NULL;
  this->parameters = NULL;
  this->mediaTypes = NULL;
  this->demuxerId = DEMUXER_ID_UNSPECIFIED;
  this->streamPid = STREAM_PID_UNSPECIFIED;
  this->flushing = false;
  this->mediaTypeToSend = NULL;
  this->mediaTypeSubType = GUID_NULL;
  this->outputPinDataLength = 0;
  this->lastStoreTime = 0;
  this->cacheFile = NULL;

  this->dumpData = NULL;
  this->dumpDataBufferOccupied = 0;
  this->dumpDataBufferSize = 0;

  this->dumpMetadata = NULL;
  this->dumpMetadataBufferOccupied = 0;
  this->dumpMetadataBufferCount = 0;

  this->dumpDataCounter = 0;

  if ((phr != NULL) && (SUCCEEDED(*phr)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*phr, logger);
    CHECK_POINTER_DEFAULT_HRESULT(*phr, parameters);
    CHECK_POINTER_DEFAULT_HRESULT(*phr, mediaTypes);

    if (SUCCEEDED(*phr))
    {
      this->logger = logger;
      this->parameters = parameters;

      this->mediaPackets = new COutputPinPacketCollection(phr);
      this->mediaPacketsLock = CreateMutex(NULL, FALSE, NULL);
      this->mediaTypes = new CMediaTypeCollection(phr);
      this->cacheFile = new CCacheFile(phr);

      CHECK_POINTER_HRESULT(*phr, this->mediaPackets, *phr, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*phr, this->mediaPacketsLock, *phr, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*phr, this->mediaTypes, *phr, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*phr, this->cacheFile, *phr, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(*phr, this->mediaTypes->Append(mediaTypes), *phr, E_OUTOFMEMORY);
    }
  }

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_PIN_END_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, this->m_pName));
}

CMPUrlSourceSplitterOutputPin::~CMPUrlSourceSplitterOutputPin()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_PIN_START_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME, this->m_pName));

  CAMThread::CallWorker(CMD_EXIT);
  CAMThread::Close();

  FREE_MEM_CLASS(this->cacheFile);
  FREE_MEM_CLASS(this->mediaPackets);
  FREE_MEM_CLASS(this->mediaTypes);
  FREE_MEM_CLASS(this->mediaTypeToSend);

  FREE_MEM(this->dumpData);
  FREE_MEM(this->dumpMetadata);

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->m_pAllocator, this->m_pAllocator->Release());

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->mediaPacketsLock, CloseHandle(this->mediaPacketsLock));
  this->mediaPacketsLock = NULL;

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_PIN_END_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME, this->m_pName));

  // logger and parameters are only reference, it is not needed to remove
  this->logger = NULL;
  this->parameters = NULL;
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

  CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_CHECK_MEDIA_TYPE_NAME, this->m_pName, result));
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

  CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_GET_MEDIA_TYPE_NAME, this->m_pName, result));
  return result;
}

STDMETHODIMP CMPUrlSourceSplitterOutputPin::Connect(IPin *pReceivePin, const AM_MEDIA_TYPE *pMediaType)
{
  HRESULT result = __super::Connect(pReceivePin, pMediaType);
  CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_CONNECT_NAME, this->m_pName, result));

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

  CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_DECIDE_ALLOCATOR_NAME, this->m_pName, result));
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

  CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_DECIDE_BUFFER_SIZE_NAME, this->m_pName, result));
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

  CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_ACTIVE_NAME, this->m_pName, result));
  return result;
}

HRESULT CMPUrlSourceSplitterOutputPin::Inactive()
{
  CAutoLock cAutoLock(m_pLock);
  HRESULT result = S_OK;

  CAMThread::CallWorker(CMD_EXIT);
  CAMThread::Close();

  // clear media packets, we are going inactive
  this->cacheFile->RemoveItems(this->mediaPackets, 0, this->mediaPackets->Count());
  this->mediaPackets->Clear();

  CHECK_HRESULT_EXECUTE(result, __super::Inactive());

  CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_INACTIVE_NAME, this->m_pName, result));
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

  this->logger->Log(LOGGER_INFO, L"%s: %s: pin '%s', sent data length: %llu", MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, this->outputPinDataLength);
  this->logger->Log(LOGGER_INFO, L"%s: %s: pin '%s', flushed data length: %llu", MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, flushedDataLength);

  this->DumpDataAndDumpDataSizes();
  this->dumpDataCounter++;

  this->outputPinDataLength = 0;

  // clear all media packets
  this->cacheFile->RemoveItems(this->mediaPackets, 0, this->mediaPackets->Count());
  this->mediaPackets->Clear();

  return result;
}

HRESULT CMPUrlSourceSplitterOutputPin::DeliverEndFlush()
{
  HRESULT result = S_OK;

  CAMThread::CallWorker(CMD_END_FLUSH);
  result = this->IsConnected() ? this->GetConnected()->EndFlush() : S_OK;
  this->flushing = false;
  this->flags &= ~MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_END_OF_STREAM;

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

      // add packet to output packet collection
      result = this->mediaPackets->Add(packet) ? result : E_OUTOFMEMORY;

      if (packet->IsEndOfStream())
      {
        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->flags |= MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_END_OF_STREAM);
      }
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitterOutputPin::QueueEndOfStream()
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_PIN_START_FORMAT, MODULE_NAME, METHOD_QUEUE_END_OF_STREAM_NAME, this->m_pName);

  COutputPinPacket *endOfStream = new COutputPinPacket(&result);
  CHECK_POINTER_HRESULT(result, endOfStream, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    endOfStream->SetEndOfStream(true);

    result = this->QueuePacket(endOfStream, INFINITE);
  }

  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->flags |= MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_END_OF_STREAM);
  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(endOfStream));

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_PIN_END_FORMAT : METHOD_PIN_END_FAIL_RESULT_FORMAT, MODULE_NAME, METHOD_QUEUE_END_OF_STREAM_NAME, this->m_pName, result);
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

unsigned int CMPUrlSourceSplitterOutputPin::GetDemuxerId(void)
{
  return this->demuxerId;
}

unsigned int CMPUrlSourceSplitterOutputPin::GetStreamPid(void)
{
  return this->streamPid;
}

/* set methods */

void CMPUrlSourceSplitterOutputPin::SetDemuxerId(unsigned int demuxerId)
{
  this->demuxerId = demuxerId;
}

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

bool CMPUrlSourceSplitterOutputPin::IsEndOfStream(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_END_OF_STREAM);
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
      this->logger->Log(LOGGER_INFO, METHOD_PIN_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, L"CMD_EXIT");
      break;
    case CMD_BEGIN_FLUSH:
      this->logger->Log(LOGGER_INFO, METHOD_PIN_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, L"CMD_BEGIN_FLUSH");
      break;
    case CMD_END_FLUSH:
      this->logger->Log(LOGGER_INFO, METHOD_PIN_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, L"CMD_END_FLUSH");
      break;
    case CMD_PLAY:
      this->logger->Log(LOGGER_INFO, METHOD_PIN_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, L"CMD_PLAY");
      break;
    case CMD_PAUSE:
      this->logger->Log(LOGGER_INFO, METHOD_PIN_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, L"CMD_PAUSE");
      break;
    case (DWORD)-1:
      // ignore, it means no command
      this->logger->Log(LOGGER_INFO, METHOD_PIN_MESSAGE_FORMAT, MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, L"no command");
      break;
    default:
      this->logger->Log(LOGGER_INFO, L"%s: %s: pin '%s', unknown command: %d", MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, cmd);
      break;
    }

    if (cmd == CMD_PLAY)
    {
      // we receive CMD_PLAY command
      // just check dumping data flag in filter configuration

      this->flags &= ~MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_DUMPING_DATA_AND_SIZES;
      this->flags |= (this->parameters->GetValueBool(PARAMETER_NAME_DUMP_OUTPUT_PIN_RAW_DATA, true, PARAMETER_NAME_DUMP_OUTPUT_PIN_RAW_DATA_DEFAULT)) ? MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_DUMPING_DATA_AND_SIZES : MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_NONE;

      if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_DUMPING_DATA_AND_SIZES))
      {
        if (this->dumpData == NULL)
        {
          this->dumpData = ALLOC_MEM_SET(this->dumpData, uint8_t, OUTPUT_PIN_DUMP_DATA_LENGTH, 0);
          this->dumpDataBufferOccupied = 0;
          this->dumpDataBufferSize = OUTPUT_PIN_DUMP_DATA_LENGTH;
        }

        if (this->dumpMetadata == NULL)
        {
          this->dumpMetadata = ALLOC_MEM_SET(this->dumpMetadata, DumpMetadata, OUTPUT_PIN_DUMP_METADATA_COUNT, 0);
          this->dumpMetadataBufferOccupied = 0;
          this->dumpMetadataBufferCount = OUTPUT_PIN_DUMP_METADATA_COUNT;
        }

        if ((this->dumpData == NULL) || (this->dumpMetadata == NULL))
        {
          // error while allocating memory
          // do not dump data, but continue in work
          this->flags &= ~MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_DUMPING_DATA_AND_SIZES;
          FREE_MEM(this->dumpData);
          FREE_MEM(this->dumpMetadata);

          this->dumpDataBufferOccupied = 0;
          this->dumpDataBufferSize = 0;
          this->dumpMetadataBufferOccupied = 0;
          this->dumpMetadataBufferCount = 0;
        }
      }
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

      if ((cmd == CMD_PLAY) && (this->IsConnected()))
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
                if (this->cacheFile->LoadItems(this->mediaPackets, 0, true, false))
                {
                  packet = this->mediaPackets->GetItem(0);

                  // we don't want to remove content of output pin packet from memory
                  packet->SetNoCleanUpFromMemory(true);
                }
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
                this->cacheFile->RemoveItems(this->mediaPackets, 0, 1);
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

                  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), notDeletePacket = (packet->GetBuffer()->GetBufferOccupiedSpace() != result));
                }
              }

              CHECK_HRESULT_EXECUTE(result, this->Deliver(sample));

              if (SUCCEEDED(result))
              {
                unsigned int sampleSize = sample->GetSize();
                
                // count send data to output pin
                this->outputPinDataLength += (uint64_t)sampleSize;

                if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_DUMPING_DATA_AND_SIZES))
                {
                  // we are dumping data, we must copy output data to temporary buffer

                  if (((this->dumpDataBufferOccupied + sampleSize) >= this->dumpDataBufferSize) ||
                    ((this->dumpMetadataBufferOccupied + 1) >= this->dumpMetadataBufferCount))
                  {
                    // we need more memory in dump data buffer or in dump data sizes buffer
                    // flush all dump data and its sizes to dump files
                    this->DumpDataAndDumpDataSizes();
                  }

                  packet->GetBuffer()->CopyFromBuffer(this->dumpData + this->dumpDataBufferOccupied, sampleSize);
                  this->dumpDataBufferOccupied += sampleSize;

                  // fill metadata
                  DumpMetadata *metadata = (this->dumpMetadata + this->dumpMetadataBufferOccupied);

                  metadata->size = sampleSize;
                  GetLocalTime(&metadata->time);

                  this->dumpMetadataBufferOccupied++;
                }

                if (!notDeletePacket)
                {
                  // this flag is set only in case of partially send data (e.g. buffer is smaller than data, so we must remove send data, but media packet is preserved to send remaining data)
                  packet->GetBuffer()->RemoveFromBuffer(sampleSize);
                }
              }

              CHECK_CONDITION_NOT_NULL_EXECUTE(sample, COM_SAFE_RELEASE(sample));

              if (SUCCEEDED(result) && (!notDeletePacket))
              {
                CLockMutex lock(this->mediaPacketsLock, INFINITE);

                // remove processed packet
                this->cacheFile->RemoveItems(this->mediaPackets, 0, 1);
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
            this->logger->Log(LOGGER_ERROR, L"%s: %s: pin '%s', result: 0x%08X", MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, result);
          }
        }
      }

      // store media packets to temporary file
      if ((cmd == CMD_PLAY) && ((GetTickCount() - this->lastStoreTime) > CACHE_FILE_LOAD_TO_MEMORY_TIME_SPAN_DEFAULT))
      {
        this->lastStoreTime = GetTickCount();

        if (this->cacheFile->GetCacheFile() == NULL)
        {
          wchar_t *storeFilePath = this->GetStoreFile();
          CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, this->cacheFile->SetCacheFile(storeFilePath));
          FREE_MEM(storeFilePath);
        }

        {
          // wait only 10 ms to lock mutex
          CLockMutex lock(this->mediaPacketsLock, 10);

          if (lock.IsLocked())
          {
            // store all media packets (which are not stored) to file
            if ((this->cacheFile->GetCacheFile() != NULL) && (this->mediaPackets->Count() != 0))
            {
              this->cacheFile->StoreItems(this->mediaPackets, this->lastStoreTime, false);
            }
          }
        }
      }

      // we don't do anything in CMD_BEGIN_FLUSH or CMD_END_FLUSH command

      CHECK_CONDITION_EXECUTE(sleepAllowed, Sleep(1));
    }
  }

  this->logger->Log(LOGGER_INFO, L"%s: %s: pin '%s', sent data length: %llu", MODULE_NAME, METHOD_THREAD_PROC_NAME, this->m_pName, this->outputPinDataLength);
  this->DumpDataAndDumpDataSizes();

  return S_OK;
}

void CMPUrlSourceSplitterOutputPin::DumpDataAndDumpDataSizes(void)
{
  if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_DUMPING_DATA_AND_SIZES))
  {
    CStaticLoggerContext *context = this->logger->GetStaticLoggerContext();

    if (context != NULL)
    {
      wchar_t *contextLogFile = Duplicate(context->GetLogFile());
      PathRemoveFileSpec(contextLogFile);

      if (contextLogFile != NULL)
      {
        wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
        wchar_t *dumpDataFileName = FormatString(L"%s\\MPUrlSourceSplitter-%s-%s-%08u.dump", contextLogFile, guid, this->m_pName, this->dumpDataCounter);
        wchar_t *dumpMetadataFileName = FormatString(L"%s\\MPUrlSourceSplitter-%s-%s-%08u.metadata", contextLogFile, guid, this->m_pName, this->dumpDataCounter);

        HANDLE hDumpData = CreateFile(dumpDataFileName, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_ALWAYS, FILE_FLAG_WRITE_THROUGH, NULL);
        HANDLE hDumpMetadata = CreateFile(dumpMetadataFileName, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_ALWAYS, FILE_FLAG_WRITE_THROUGH, NULL);

        if ((hDumpData != INVALID_HANDLE_VALUE) && (hDumpMetadata != INVALID_HANDLE_VALUE))
        {
          // move to end of files
          LARGE_INTEGER distanceToMove;
          distanceToMove.QuadPart = 0;

          SetFilePointerEx(hDumpData, distanceToMove, NULL, FILE_END);
          SetFilePointerEx(hDumpMetadata, distanceToMove, NULL, FILE_END);

          // write data to file
          DWORD written = 0;
          WriteFile(hDumpData, this->dumpData, this->dumpDataBufferOccupied, &written, NULL);

          written = 0;
          WriteFile(hDumpMetadata, this->dumpMetadata, this->dumpMetadataBufferOccupied * sizeof(DumpMetadata), &written, NULL);
        }

        CHECK_CONDITION_EXECUTE(hDumpData != INVALID_HANDLE_VALUE, CloseHandle(hDumpData));
        hDumpData = INVALID_HANDLE_VALUE;

        CHECK_CONDITION_EXECUTE(hDumpMetadata != INVALID_HANDLE_VALUE, CloseHandle(hDumpMetadata));
        hDumpMetadata = INVALID_HANDLE_VALUE;

        FREE_MEM(guid);
        FREE_MEM(dumpDataFileName);
        FREE_MEM(dumpMetadataFileName);
      }

      FREE_MEM(contextLogFile);
    }
  }

  this->dumpDataBufferOccupied = 0;
  this->dumpMetadataBufferOccupied = 0;
}

wchar_t *CMPUrlSourceSplitterOutputPin::GetStoreFile(void)
{
  wchar_t *result = NULL;
  const wchar_t *folder = this->parameters->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

  if (folder != NULL)
  {
    result = FormatString(L"%smpurlsourcesplitter_output_pin_%02u_%02u.temp", folder, this->GetDemuxerId(), this->GetStreamPid());
  }

  return result;
}
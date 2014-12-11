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

#include "StdAfx.h"

#include "Muxer.h"
#include "LockMutex.h"

#include <process.h>

#ifdef _DEBUG
#define MODULE_NAME                                                         L"Muxerd"
#else
#define MODULE_NAME                                                         L"Muxer"
#endif

CMuxer::CMuxer(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CFlags()
{
  this->logger = NULL;
  this->configuration = NULL;
  this->outputPacketCollection = NULL;
  this->outputPacketMutex = NULL;
  this->muxerWorkerThread = NULL;
  this->muxerWorkerShouldExit = false;
  this->muxerError = S_OK;
  this->flushing = false;
  this->muxerPacketCollection = NULL;
  this->muxerPacketMutex = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, logger);
    CHECK_POINTER_DEFAULT_HRESULT(*result, configuration);

    if (SUCCEEDED(*result))
    {
      this->logger = logger;

      this->muxerPacketCollection = new COutputPinPacketCollection(result);
      CHECK_POINTER_HRESULT(*result, this->muxerPacketCollection, *result, E_OUTOFMEMORY);

      this->outputPacketCollection = new COutputPinPacketCollection(result);
      CHECK_POINTER_HRESULT(*result, this->outputPacketCollection, *result, E_OUTOFMEMORY);

      this->configuration = new CParameterCollection(result);
      CHECK_POINTER_HRESULT(*result, this->configuration, *result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(*result, this->configuration->Append(configuration), *result, E_OUTOFMEMORY);

      this->outputPacketMutex = CreateMutex(NULL, FALSE, NULL);
      this->muxerPacketMutex = CreateMutex(NULL, FALSE, NULL);

      CHECK_POINTER_HRESULT(*result, this->outputPacketMutex, *result, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*result, this->muxerPacketMutex, *result, E_OUTOFMEMORY);
    }
  }
}

CMuxer::~CMuxer(void)
{
  // destroy muxer worker (if not finished earlier)
  this->DestroyMuxerWorker();

  FREE_MEM_CLASS(this->muxerPacketCollection);
  FREE_MEM_CLASS(this->outputPacketCollection);

  if (this->outputPacketMutex != NULL)
  {
    CloseHandle(this->outputPacketMutex);
    this->outputPacketMutex = NULL;
  }

  if (this->muxerPacketMutex != NULL)
  {
    CloseHandle(this->muxerPacketMutex);
    this->muxerPacketMutex = NULL;
  }

  FREE_MEM_CLASS(this->configuration);
}

/* get methods */

HRESULT CMuxer::GetOutputPinPacket(COutputPinPacket *packet)
{
  // S_FALSE means no packet
  HRESULT result = S_FALSE;
  CHECK_POINTER_DEFAULT_HRESULT(result, packet);

  if (SUCCEEDED(result))
  {
    CLockMutex lock(this->outputPacketMutex, INFINITE);

    COutputPinPacket *outputPacket = this->outputPacketCollection->GetItem(0);
    if (outputPacket != NULL)
    {
      if (!outputPacket->IsEndOfStream())
      {
        CHECK_CONDITION_HRESULT(result, packet->GetBuffer()->InitializeBuffer(outputPacket->GetBuffer()->GetBufferOccupiedSpace()), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), packet->GetBuffer()->AddToBufferWithResize(outputPacket->GetBuffer()));
      }

      packet->SetStreamPid(outputPacket->GetStreamPid());
      packet->SetDemuxerId(outputPacket->GetDemuxerId());
      packet->SetStartTime(outputPacket->GetStartTime());
      packet->SetEndTime(outputPacket->GetEndTime());
      packet->SetFlags(outputPacket->GetFlags());
      packet->SetMediaType(outputPacket->GetMediaType());
      packet->SetEndOfStream(outputPacket->IsEndOfStream(), outputPacket->GetEndOfStreamResult());
      outputPacket->SetMediaType(NULL);

      if (SUCCEEDED(result))
      {
        this->outputPacketCollection->CCollection::Remove(0);
        result = S_OK;
      }
    }
  }

  return result;
}

HRESULT CMuxer::GetMuxerError(void)
{
  return this->muxerError;
}

/* set methods */

/* other methods */

HRESULT CMuxer::StartMuxer(void)
{
  return this->CreateMuxerWorker();
}

HRESULT CMuxer::BeginFlush(void)
{
  HRESULT result = S_OK;

  this->flushing = true;

  {
    // clear muxer packets
    CLockMutex lock(this->muxerPacketMutex, INFINITE);

    this->muxerPacketCollection->Clear();
  }

  {
    // clear output packets
    CLockMutex lock(this->outputPacketMutex, INFINITE);

    this->outputPacketCollection->Clear();
  }

  return result;
}

HRESULT CMuxer::EndFlush(void)
{
  HRESULT result = S_OK;

  this->flushing = false;
  this->flags &= ~MUXER_FLAG_END_OF_STREAM;

  return result;
}

HRESULT CMuxer::QueuePacket(COutputPinPacket *packet, DWORD timeout)
{
  HRESULT result = S_OK;

  {
    CLockMutex lock(this->muxerPacketMutex, timeout);
    result = (lock.IsLocked()) ? S_OK : VFW_E_TIMEOUT;

    if (SUCCEEDED(result))
    {
      // add packet to muxer packet collection
      result = this->muxerPacketCollection->Add(packet) ? result : E_OUTOFMEMORY;

      if (packet->IsEndOfStream())
      {
        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->flags |= MUXER_FLAG_END_OF_STREAM);
      }
    }
  }

  return result;
}

HRESULT CMuxer::QueueEndOfStream(HRESULT endOfStreamResult)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_QUEUE_END_OF_STREAM_NAME);

  COutputPinPacket *endOfStream = new COutputPinPacket(&result);
  CHECK_POINTER_HRESULT(result, endOfStream, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    endOfStream->SetEndOfStream(true, endOfStreamResult);

    result = this->QueuePacket(endOfStream, INFINITE);
  }

  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->flags |= MUXER_FLAG_END_OF_STREAM);
  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(endOfStream));

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_QUEUE_END_OF_STREAM_NAME, result);
  return result;
}

bool CMuxer::IsEndOfStream(void)
{
  return this->IsSetFlags(MUXER_FLAG_END_OF_STREAM);
}

/* protected methods */

/* muxer worker methods */

unsigned int WINAPI CMuxer::MuxerWorker(LPVOID lpParam)
{
  CMuxer *caller = (CMuxer *)lpParam;
  caller->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_MUXER_WORKER_NAME);

  while (SUCCEEDED(caller->muxerError) && (!caller->muxerWorkerShouldExit))
  {
    if (!caller->flushing)
    {
      caller->muxerError = caller->MuxerWorkerInternal();
    }

    Sleep(1);
  }
  
  caller->logger->Log(SUCCEEDED(caller->muxerError) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(caller->muxerError) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_MUXER_WORKER_NAME, caller->muxerError);

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}

HRESULT CMuxer::CreateMuxerWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_CREATE_MUXER_WORKER_NAME);

  this->muxerWorkerShouldExit = false;

  this->muxerWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CMuxer::MuxerWorker, this, 0, NULL);

  if (this->muxerWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, _beginthreadex() error: 0x%08X", MODULE_NAME, METHOD_CREATE_MUXER_WORKER_NAME, result);
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_CREATE_MUXER_WORKER_NAME, result);
  return result;
}

HRESULT CMuxer::DestroyMuxerWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DESTROY_MUXER_WORKER_NAME);

  // wait for the muxer worker thread to exit      
  if (this->muxerWorkerThread != NULL)
  {
    this->muxerWorkerShouldExit = true;

    if (WaitForSingleObject(this->muxerWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_DESTROY_MUXER_WORKER_NAME, L"thread didn't exit, terminating thread");
      TerminateThread(this->muxerWorkerThread, 0);
    }
    CloseHandle(this->muxerWorkerThread);
  }

  this->muxerWorkerThread = NULL;
  this->muxerWorkerShouldExit = false;

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DESTROY_MUXER_WORKER_NAME, result);
  return result;
}
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

#include "Demuxer.h"
#include "LockMutex.h"
#include "ErrorCodes.h"
#include "StreamPackage.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackageDataResponse.h"

#include <process.h>

#ifdef _DEBUG
#define MODULE_NAME                                                         L"Demuxerd"
#else
#define MODULE_NAME                                                         L"Demuxer"
#endif

CDemuxer::CDemuxer(HRESULT *result, CLogger *logger, IDemuxerOwner *filter, CParameterCollection *configuration)
  : CFlags()
{
  this->logger = NULL;
  this->filter = NULL;
  this->demuxerContextBufferPosition = 0;
  this->createDemuxerWorkerShouldExit = false;
  this->createDemuxerWorkerThread = NULL;
  this->demuxerId = 0;
  this->configuration = NULL;
  this->outputPacketCollection = NULL;
  this->outputPacketMutex = NULL;
  this->demuxingWorkerThread = NULL;
  this->demuxingWorkerShouldExit = false;
  this->createDemuxerError = S_OK;
  this->demuxerContextRequestId = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, logger);
    CHECK_POINTER_DEFAULT_HRESULT(*result, filter);
    CHECK_POINTER_DEFAULT_HRESULT(*result, configuration);

    if (SUCCEEDED(*result))
    {
      this->logger = logger;
      this->filter = filter;

      this->outputPacketCollection = new COutputPinPacketCollection(result);
      CHECK_POINTER_HRESULT(*result, this->outputPacketCollection, *result, E_OUTOFMEMORY);

      this->configuration = new CParameterCollection(result);
      CHECK_POINTER_HRESULT(*result, this->configuration, *result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(*result, this->configuration->Append(configuration), *result, E_OUTOFMEMORY);

      this->outputPacketMutex = CreateMutex(NULL, FALSE, NULL);

      CHECK_POINTER_HRESULT(*result, this->outputPacketMutex, *result, E_OUTOFMEMORY);
    }
  }
}

CDemuxer::~CDemuxer(void)
{
  // destroy create demuxer worker (if not finished earlier)
  this->DestroyCreateDemuxerWorker();

  // destroy demuxing worker (if not finished earlier)
  this->DestroyDemuxingWorker();

  FREE_MEM_CLASS(this->outputPacketCollection);

  if (this->outputPacketMutex != NULL)
  {
    CloseHandle(this->outputPacketMutex);
    this->outputPacketMutex = NULL;
  }

  this->demuxerContextBufferPosition = 0;

  FREE_MEM_CLASS(this->configuration);
}

/* get methods */

HRESULT CDemuxer::GetOutputPinPacket(COutputPinPacket *packet)
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

unsigned int CDemuxer::GetDemuxerId(void)
{
  return this->demuxerId;
}

IDemuxerOwner *CDemuxer::GetDemuxerOwner(void)
{
  return this->filter;
}

HRESULT CDemuxer::GetCreateDemuxerError(void)
{
  return this->createDemuxerError;
}

/* set methods */

void CDemuxer::SetDemuxerId(unsigned int demuxerId)
{
  this->demuxerId = demuxerId;
}

void CDemuxer::SetPauseSeekStopRequest(bool pauseSeekStopRequest)
{
  this->flags &= ~(DEMUXER_FLAG_DISABLE_DEMUXING | DEMUXER_FLAG_DISABLE_READING);
  this->flags |= pauseSeekStopRequest ? DEMUXER_FLAG_DISABLE_READING : DEMUXER_FLAG_NONE;
  this->filter->SetPauseSeekStopMode(pauseSeekStopRequest ? PAUSE_SEEK_STOP_MODE_DISABLE_READING : PAUSE_SEEK_STOP_MODE_NONE);
}

/* other methods */

bool CDemuxer::IsCreatedDemuxer(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_CREATED_DEMUXER);
}

bool CDemuxer::IsCreateDemuxerWorkerFinished(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED);
}

bool CDemuxer::HasStartedCreatingDemuxer(void)
{
  return (this->createDemuxerWorkerThread != NULL);
}

bool CDemuxer::IsEndOfStreamOutputPacketQueued(void)
{
  return this->IsSetFlags(DEMUXER_FLAG_END_OF_STREAM_OUTPUT_PACKET_QUEUED);
}

HRESULT CDemuxer::StartCreatingDemuxer(void)
{
  HRESULT result = S_OK;

  if (SUCCEEDED(result) && (!this->IsCreateDemuxerWorkerFinished()) && (this->createDemuxerWorkerThread == NULL))
  {
    result = this->CreateCreateDemuxerWorker();
  }

  return result;
}

HRESULT CDemuxer::StartDemuxing(void)
{
  HRESULT result = this->IsCreatedDemuxer() ? S_OK : E_NOT_VALID_STATE;

  if (SUCCEEDED(result))
  {
    result = this->CreateDemuxingWorker();
  }

  return result;
}

/* protected methods */

HRESULT CDemuxer::DemuxerReadPosition(int64_t position, uint8_t *buffer, int length, uint64_t flags)
{
  HRESULT result = S_OK;
  CHECK_CONDITION(result, length >= 0, S_OK, E_INVALIDARG);
  CHECK_POINTER_DEFAULT_HRESULT(result, buffer);

  if (SUCCEEDED(result) && (length > 0))
  {
    bool repeatRequest = true;
    while (SUCCEEDED(result) && repeatRequest)
    {
      CStreamPackage *package = new CStreamPackage(&result);
      CHECK_POINTER_HRESULT(result, package, result, E_OUTOFMEMORY);

      unsigned int requestId = this->demuxerContextRequestId++;
      if (SUCCEEDED(result))
      {
        CStreamPackageDataRequest *request = new CStreamPackageDataRequest(&result);
        CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          request->SetAnyDataLength((flags & STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_DATA_LENGTH) == STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_DATA_LENGTH);
          request->SetAnyNonZeroDataLength((flags & STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_NONZERO_DATA_LENGTH) == STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_NONZERO_DATA_LENGTH);
          request->SetId(requestId);
          request->SetStreamId(this->demuxerId);
          request->SetStart(position);
          request->SetLength((unsigned int)length);

          package->SetRequest(request);
        }

        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(request));
      }

      if (this->IsSetFlags(DEMUXER_FLAG_PENDING_DISCONTINUITY))
      {
        if (this->IsSetFlags(DEMUXER_FLAG_PENDING_DISCONTINUITY_WITH_REPORT))
        {
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, request %u, start: %lld, length: %d, discontinuity reported", MODULE_NAME, METHOD_DEMUXER_READ_POSITION_NAME, this->demuxerId, requestId, position, length);
        }

        // do not report discontinuity again, until discontinuity is reset
        this->flags &= ~DEMUXER_FLAG_PENDING_DISCONTINUITY_WITH_REPORT;
        result = E_CONNECTION_LOST_TRYING_REOPEN;
      }

      CHECK_HRESULT_EXECUTE(result, this->filter->ProcessStreamPackage(package));
      CHECK_HRESULT_EXECUTE(result, package->GetError());

      if (result == E_PAUSE_SEEK_STOP_MODE_DISABLE_READING)
      {
        // it was disabled reading from protocol (by some reason)
        // send new request after some time

        result = S_OK;

        if (this->IsSetFlags(DEMUXER_FLAG_DISABLE_DEMUXING_WITH_RETURN_TO_DEMUXING_WORKER))
        {
          // we are requested to immediately return to demuxing worker
          result = E_PAUSE_SEEK_STOP_MODE_DISABLE_READING;
        }
      }
      else if (SUCCEEDED(result))
      {
        repeatRequest = false;

        // successfully processed stream package request
        CStreamPackageDataResponse *response = dynamic_cast<CStreamPackageDataResponse *>(package->GetResponse());

        response->GetBuffer()->CopyFromBuffer(buffer, response->GetBuffer()->GetBufferOccupiedSpace());
        result = response->GetBuffer()->GetBufferOccupiedSpace();

        if (response->IsDiscontinuity())
        {
          this->flags |= DEMUXER_FLAG_PENDING_DISCONTINUITY;

          if (result != 0)
          {
            this->flags |= DEMUXER_FLAG_PENDING_DISCONTINUITY_WITH_REPORT;
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: stream %u, request %u, start: %lld, length: %d, pending discontinuity", MODULE_NAME, METHOD_DEMUXER_READ_POSITION_NAME, this->demuxerId, requestId, position, length);
          }
        }
      }
      else
      {
        repeatRequest = false;

        // error occured, log and return
        CHECK_CONDITION_EXECUTE(result != E_CONNECTION_LOST_TRYING_REOPEN, this->logger->Log(LOGGER_WARNING, L"%s: %s: stream %u, request %u, start: %lld, length: %d, error: 0x%08X", MODULE_NAME, METHOD_DEMUXER_READ_POSITION_NAME, this->demuxerId, requestId, position, length, result));
      }

      FREE_MEM_CLASS(package);
      CHECK_CONDITION_EXECUTE(SUCCEEDED(result) && repeatRequest, Sleep(1));
    }
  }

  return result;
}

unsigned int WINAPI CDemuxer::CreateDemuxerWorker(LPVOID lpParam)
{
  CDemuxer *caller = (CDemuxer *)lpParam;

  caller->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXER_WORKER_NAME, caller->demuxerId);

  HRESULT result = S_OK;
  while (SUCCEEDED(result) && (!caller->createDemuxerWorkerShouldExit) && (!caller->IsCreatedDemuxer()))
  {
    if (!caller->IsCreatedDemuxer())
    {
      caller->demuxerContextBufferPosition = 0;

      result = caller->CreateDemuxerInternal();
      caller->createDemuxerError = result;

      if (SUCCEEDED(result))
      {
        caller->flags |= DEMUXER_FLAG_CREATED_DEMUXER;
        break;
      }
      else
      {
        caller->CleanupDemuxerInternal();
      }
    }

    if (!caller->IsCreatedDemuxer())
    {
      Sleep(100);
    }
  }

  caller->logger->Log(LOGGER_INFO, METHOD_DEMUXER_END_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXER_WORKER_NAME, caller->demuxerId);
  caller->flags |= DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED;

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}

HRESULT CDemuxer::CreateCreateDemuxerWorker(void)
{
  HRESULT result = S_OK;

  this->flags &= ~DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED;

  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_CREATE_CREATE_DEMUXER_WORKER_NAME, this->demuxerId);

  this->createDemuxerWorkerShouldExit = false;

  this->createDemuxerWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CDemuxer::CreateDemuxerWorker, this, 0, NULL);

  if (this->createDemuxerWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, _beginthreadex() error: 0x%08X", MODULE_NAME, METHOD_CREATE_CREATE_DEMUXER_WORKER_NAME, this->demuxerId, result);
    this->flags |= DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED;
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_CREATE_CREATE_DEMUXER_WORKER_NAME, this->demuxerId, result);
  return result;
}

HRESULT CDemuxer::DestroyCreateDemuxerWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_DESTROY_CREATE_DEMUXER_WORKER_NAME, this->demuxerId);

  this->createDemuxerWorkerShouldExit = true;
  this->filter->SetPauseSeekStopMode(PAUSE_SEEK_STOP_MODE_DISABLE_READING);
  this->flags |= DEMUXER_FLAG_DISABLE_DEMUXING_WITH_RETURN_TO_DEMUXING_WORKER;
  this->flags &= ~DEMUXER_FLAG_DISABLE_READING;

  // wait for the create demuxer worker thread to exit
  if (this->createDemuxerWorkerThread != NULL)
  {
    if (WaitForSingleObject(this->createDemuxerWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_DESTROY_CREATE_DEMUXER_WORKER_NAME, this->demuxerId, L"thread didn't exit, terminating thread");
      TerminateThread(this->createDemuxerWorkerThread, 0);
    }
    CloseHandle(this->createDemuxerWorkerThread);
  }

  this->createDemuxerWorkerThread = NULL;
  this->createDemuxerWorkerShouldExit = false;
  this->flags |= DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED;

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DESTROY_CREATE_DEMUXER_WORKER_NAME, this->demuxerId, result);
  return result;
}

unsigned int WINAPI CDemuxer::DemuxingWorker(LPVOID lpParam)
{
  CDemuxer *caller = (CDemuxer *)lpParam;
  caller->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_DEMUXING_WORKER_NAME, caller->demuxerId);

  while (!caller->demuxingWorkerShouldExit)
  {
    caller->DemuxingWorkerInternal();

    Sleep(1);
  }

  caller->logger->Log(LOGGER_INFO, METHOD_DEMUXER_END_FORMAT, MODULE_NAME, METHOD_DEMUXING_WORKER_NAME, caller->demuxerId);

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}

HRESULT CDemuxer::CreateDemuxingWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXING_WORKER_NAME, this->demuxerId);

  this->demuxingWorkerShouldExit = false;

  this->demuxingWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CDemuxer::DemuxingWorker, this, 0, NULL);

  if (this->demuxingWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: stream %u, _beginthreadex() error: 0x%08X", MODULE_NAME, METHOD_CREATE_DEMUXING_WORKER_NAME, this->demuxerId, result);
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_CREATE_DEMUXING_WORKER_NAME, this->demuxerId, result);
  return result;
}

HRESULT CDemuxer::DestroyDemuxingWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_START_FORMAT, MODULE_NAME, METHOD_DESTROY_DEMUXING_WORKER_NAME, this->demuxerId);

  // wait for the receive data worker thread to exit      
  if (this->demuxingWorkerThread != NULL)
  {
    this->filter->SetPauseSeekStopMode(PAUSE_SEEK_STOP_MODE_DISABLE_READING);
    this->flags |= DEMUXER_FLAG_DISABLE_DEMUXING_WITH_RETURN_TO_DEMUXING_WORKER;
    this->flags &= ~DEMUXER_FLAG_DISABLE_READING;
    // wait until DemuxingWorker() confirm
    while (!this->IsSetFlags(DEMUXER_FLAG_DISABLE_DEMUXING))
    {
      Sleep(1);
    }

    this->demuxingWorkerShouldExit = true;

    if (WaitForSingleObject(this->demuxingWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_DEMUXER_MESSAGE_FORMAT, MODULE_NAME, METHOD_DESTROY_DEMUXING_WORKER_NAME, this->demuxerId, L"thread didn't exit, terminating thread");
      TerminateThread(this->demuxingWorkerThread, 0);
    }
    CloseHandle(this->demuxingWorkerThread);
  }

  this->demuxingWorkerThread = NULL;
  this->demuxingWorkerShouldExit = false;

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_DEMUXER_END_FORMAT : METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT, MODULE_NAME, METHOD_DESTROY_DEMUXING_WORKER_NAME, this->demuxerId, result);
  return result;
}
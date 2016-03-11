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

#include "MPIPTVSourceStream.h"
#include "ProtocolInterface.h"
#include "Utilities.h"
#include "PatParser.h"
#include "PmtParser.h"

#include <Shlwapi.h>
#include <ShlObj.h>
#include <stdio.h>

#define MODULE_NAME                                               _T("MPIPTVSourceStream")

#define METHOD_LOAD_PLUGINS_NAME                                  _T("LoadPlugins()")
#define METHOD_DECIDE_BUFFER_SIZE_NAME                            _T("DecideBufferSize()")
#define METHOD_ON_THREAD_START_PLAY_NAME                          _T("OnThreadStartPlay()")
#define METHOD_ON_THREAD_DESTROY_NAME                             _T("OnThreadDestroy()")
#define METHOD_RECEIVE_DATA_WORKER_NAME                           _T("ReceiveDataWorker()")
#define METHOD_DO_BUFFER_PROCESSING_LOOP_NAME                     _T("DoBufferProcessingLoop()")
#define METHOD_RUN_NAME                                           _T("Run()")

CMPIPTVSourceStream::CMPIPTVSourceStream(HRESULT *phr, CSource *pFilter, CParameterCollection *configuration)
  : CSourceStream(NAME(_T("MediaPortal IPTV Stream")), phr, pFilter, L"Out")
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME);

  this->lockMutex = CreateMutex(NULL, FALSE, NULL);
  this->activeProtocol = NULL;
  this->totalReceived = 0;
  this->threadShouldExit = false;
  this->status = STATUS_NONE;
  this->runMethodExecuted = 0;

  this->configuration = new CParameterCollection();
  if (configuration != NULL)
  {
    this->configuration->Append(configuration);
  }

  this->configuration->LogCollection(&this->logger, LOGGER_VERBOSE, MODULE_NAME, METHOD_CONSTRUCTOR_NAME);
  this->dumpRawTS = this->configuration->GetValueBool(CONFIGURATION_DUMP_RAW_TS, true, DUMP_RAW_TS_DEFAULT);
  this->analyzeDiscontinuity = this->configuration->GetValueBool(CONFIGURATION_ANALYZE_DISCONTINUITY, true, ANALYZE_DISCONTINUITY_DEFAULT);
  this->canChangeSidAndPid = true;
  this->newPid = PID_DEFAULT;
  this->newSid = SID_DEFAULT;
  this->oldPid = PID_DEFAULT;
  this->oldSid = SID_DEFAULT;

  this->keepPidValues = ALLOC_MEM_SET(this->keepPidValues, unsigned int, PID_COUNT, KEEP_PID_IN_STREAM);
  if (this->keepPidValues == NULL)
  {
    this->logger.Log(LOGGER_WARNING, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, _T("cannot allocate enough memory for keeped PID values, PID keeping disabled"));
  }

  this->protocolImplementations = NULL;
  this->dllTotal = 0;
  this->LoadPlugins();

  this->pidCounters = ALLOC_MEM(unsigned int, PID_COUNT);
  if (this->pidCounters == NULL)
  {
    this->logger.Log(LOGGER_WARNING, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, _T("cannot allocate enough memory for PID counters, discontinuity checking disabled"));
  }

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME);
}

CMPIPTVSourceStream::~CMPIPTVSourceStream(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME);

  if (this->protocolImplementations != NULL)
  {
    for(unsigned int i = 0; i < this->dllTotal; i++)
    {
      this->logger.Log(LOGGER_INFO, _T("%s: %s: destroying protocol: %s"), MODULE_NAME, METHOD_DESTRUCTOR_NAME, protocolImplementations[i].protocol);

      if (protocolImplementations[i].pImplementation != NULL)
      {
        protocolImplementations[i].destroyProtocolInstance(protocolImplementations[i].pImplementation);
        protocolImplementations[i].pImplementation = NULL;
        protocolImplementations[i].destroyProtocolInstance = NULL;
      }
      if (protocolImplementations[i].protocol != NULL)
      {
        CoTaskMemFree(protocolImplementations[i].protocol);
        protocolImplementations[i].protocol = NULL;
      }
      if (protocolImplementations[i].hLibrary != NULL)
      {
        FreeLibrary(protocolImplementations[i].hLibrary);
        protocolImplementations[i].hLibrary = NULL;
      }
    }
    this->dllTotal = 0;
  }

  FREE_MEM(this->keepPidValues);
  FREE_MEM(this->protocolImplementations);

  if (this->configuration != NULL)
  {
    delete this->configuration;
    this->configuration = NULL;
  }

  FREE_MEM(this->pidCounters);

  if (this->lockMutex != NULL)
  {
    CloseHandle(this->lockMutex);
    this->lockMutex = NULL;
  }

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME);
}

void CMPIPTVSourceStream::LoadPlugins()
{
  logger.Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_LOAD_PLUGINS_NAME);

  unsigned int maxPlugins = this->configuration->GetValueLong(CONFIGURATION_MAX_PLUGINS, true, MAX_PLUGINS_DEFAULT);
  // check value
  maxPlugins = (maxPlugins < 0) ? MAX_PLUGINS_DEFAULT : maxPlugins;

  this->protocolImplementations = ALLOC_MEM(ProtocolImplementation, maxPlugins);
  if (this->protocolImplementations != NULL)
  {
    WIN32_FIND_DATA info;
    HANDLE h;

    ALLOC_MEM_DEFINE_SET(strDllPath, TCHAR, _MAX_PATH, 0);
    ALLOC_MEM_DEFINE_SET(strDllSearch, TCHAR, _MAX_PATH, 0);

    GetModuleFileName(NULL, strDllPath, _MAX_PATH);
    PathRemoveFileSpec(strDllPath);

    _tcscat_s(strDllPath, _MAX_PATH, _T("\\"));
    _tcscpy_s(strDllSearch, _MAX_PATH, strDllPath);
    _tcscat_s(strDllSearch, _MAX_PATH, _T("mpiptv_*.dll"));

    logger.Log(LOGGER_VERBOSE, _T("%s: %s: search path: %s"), MODULE_NAME, METHOD_LOAD_PLUGINS_NAME, strDllPath);
    // add plugins directory to search path
    SetDllDirectory(strDllPath);

    h = FindFirstFile(strDllSearch, &info);
    if (h != INVALID_HANDLE_VALUE) 
    {
      do 
      {
        BOOL result = TRUE;
        ALLOC_MEM_DEFINE_SET(strDllName, TCHAR, _MAX_PATH, 0);

        _tcscpy_s(strDllName, _MAX_PATH, strDllPath);
        _tcscat_s(strDllName, _MAX_PATH, info.cFileName);

        // load library
        logger.Log(LOGGER_INFO, _T("%s: %s: loading library: %s"), MODULE_NAME, METHOD_LOAD_PLUGINS_NAME, strDllName);
        HINSTANCE hLibrary = LoadLibrary(strDllName);        
        if (hLibrary == NULL)
        {
          logger.Log(LOGGER_ERROR, _T("%s: %s: library '%s' not loaded"), MODULE_NAME, METHOD_LOAD_PLUGINS_NAME, strDllName);
          result = FALSE;
        }

        if (result)
        {
          // find CreateProtocolInstance() function
          // find DestroyProtocolInstance() function
          PIProtocol pIProtocol = NULL;
          CREATEPROTOCOLINSTANCE createProtocolInstance;
          DESTROYPROTOCOLINSTANCE destroyProtocolInstance;

          createProtocolInstance = (CREATEPROTOCOLINSTANCE)GetProcAddress(hLibrary, "CreateProtocolInstance");
          destroyProtocolInstance = (DESTROYPROTOCOLINSTANCE)GetProcAddress(hLibrary, "DestroyProtocolInstance");

          if (createProtocolInstance == NULL)
          {
            logger.Log(LOGGER_ERROR, _T("%s: %s: cannot find CreateProtocolInstance() function, error: %d"), MODULE_NAME, METHOD_LOAD_PLUGINS_NAME, GetLastError());
            result = FALSE;
          }
          if (destroyProtocolInstance == NULL)
          {
            logger.Log(LOGGER_ERROR, _T("%s: %s: cannot find DestroyProtocolInstance() function, error: %d"), MODULE_NAME, METHOD_LOAD_PLUGINS_NAME, GetLastError());
            result = FALSE;
          }

          if (result)
          {
            // create protocol instance
            pIProtocol = (PIProtocol)createProtocolInstance();
            if (pIProtocol == NULL)
            {
              logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_LOAD_PLUGINS_NAME, _T("cannot create protocol implementation instance"));
              result = FALSE;
            }

            if (result)
            {
              // library is loaded and protocol implementation is instanced
              protocolImplementations[dllTotal].hLibrary = hLibrary;
              protocolImplementations[dllTotal].pImplementation = pIProtocol;
              protocolImplementations[dllTotal].protocol = pIProtocol->GetProtocolName();
              protocolImplementations[dllTotal].supported = false;
              protocolImplementations[dllTotal].destroyProtocolInstance = destroyProtocolInstance;

              if (protocolImplementations[dllTotal].protocol == NULL)
              {
                // error occured while getting protocol name
                logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_LOAD_PLUGINS_NAME, _T("cannot get protocol name"));
                protocolImplementations[dllTotal].destroyProtocolInstance(protocolImplementations[dllTotal].pImplementation);

                protocolImplementations[dllTotal].hLibrary = NULL;
                protocolImplementations[dllTotal].pImplementation = NULL;
                protocolImplementations[dllTotal].protocol = NULL;
                protocolImplementations[dllTotal].supported = false;
                protocolImplementations[dllTotal].destroyProtocolInstance = NULL;

                result = FALSE;
              }
            }

            if (result)
            {
              // initialize protocol implementation
              CParameterCollection *parameters = new CParameterCollection();
              // add global configuration parameters
              parameters->Append(this->configuration);
              // add protocol specific parameters
              CParameterCollection *protocolSpecific = GetConfiguration(&this->logger, MODULE_NAME, METHOD_LOAD_PLUGINS_NAME, protocolImplementations[dllTotal].protocol);
              parameters->Append(protocolSpecific);

              delete protocolSpecific;

              // insert IPTV buffer size parameter (if needed)
              if (!parameters->Contains(CONFIGURATION_IPTV_BUFFER_SIZE, true))
              {
                ALLOC_MEM_DEFINE_SET(value, TCHAR, 40, 0);
                if (value != NULL)
                {
                  if (!_ltot_s(IPTV_BUFFER_SIZE_DEFAULT, value, 40, 10))
                  {
                    // successful conversion
                    parameters->Add(new CParameter(CONFIGURATION_IPTV_BUFFER_SIZE, value));
                  }
                }
                FREE_MEM(value);
              }

              // insert IPTV buffer count parameter (if needed)
              if (!parameters->Contains(CONFIGURATION_IPTV_BUFFER_COUNT, true))
              {
                // insert IPTV buffer count parameter
                ALLOC_MEM_DEFINE_SET(value, TCHAR, 40, 0);
                if (value != NULL)
                {
                  if (!_ltot_s(IPTV_BUFFER_COUNT_DEFAULT, value, 40, 10))
                  {
                    // successful conversion
                    parameters->Add(new CParameter(CONFIGURATION_IPTV_BUFFER_COUNT, value));
                  }
                }
                FREE_MEM(value);
              }

              // initialize protocol
              int initialized = protocolImplementations[dllTotal].pImplementation->Initialize(this->lockMutex, parameters);
              // delete collection of parameters
              delete parameters;

              if (initialized == STATUS_OK)
              {
                TCHAR *guid = ConvertGuidToString(protocolImplementations[dllTotal].pImplementation->GetInstanceId());
                logger.Log(LOGGER_INFO, _T("%s: %s: protocol '%s' successfully instanced, id: %s"), MODULE_NAME, METHOD_LOAD_PLUGINS_NAME, protocolImplementations[dllTotal].protocol, guid);
                FREE_MEM(guid);
                dllTotal++;
              }
              else
              {
                logger.Log(LOGGER_INFO, _T("%s: %s: protocol '%s' not initialized"), MODULE_NAME, METHOD_LOAD_PLUGINS_NAME, protocolImplementations[dllTotal].protocol);
                protocolImplementations[dllTotal].destroyProtocolInstance(protocolImplementations[dllTotal].pImplementation);
              }
            }
          }

          if (!result)
          {
            // any error occured while loading protocol
            // free library and continue with another
            FreeLibrary(hLibrary);
          }
        }

        FREE_MEM(strDllName);
        if (this->dllTotal == maxPlugins)
        {
          break;
        }
      } while (FindNextFile(h, &info));
      FindClose(h);
    } 

    logger.Log(LOGGER_INFO, _T("%s: %s: found protocols: %u"), MODULE_NAME, METHOD_LOAD_PLUGINS_NAME, dllTotal);

    FREE_MEM(strDllPath);
    FREE_MEM(strDllSearch);
  }
  else
  {
    logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_LOAD_PLUGINS_NAME, _T("cannot allocate memory for protocol implementations"));
  }

  logger.Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_LOAD_PLUGINS_NAME);
}

HRESULT CMPIPTVSourceStream::GetMediaType(__inout CMediaType *pMediaType) 
{
  pMediaType->majortype = MEDIATYPE_Stream;
  pMediaType->subtype = MEDIASUBTYPE_MPEG2_TRANSPORT;

  return S_OK;
}

HRESULT CMPIPTVSourceStream::DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *pRequest) 
{
  HRESULT result = S_OK;
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DECIDE_BUFFER_SIZE_NAME);

  HRESULT hr;
  CAutoLock cAutoLock(m_pFilter->pStateLock());

  CheckPointer(pAlloc, E_POINTER);
  CheckPointer(pRequest, E_POINTER);

  long iptvBufferSize = this->configuration->GetValueLong(CONFIGURATION_IPTV_BUFFER_SIZE, true, IPTV_BUFFER_SIZE_DEFAULT);
  long iptvBufferCount = this->configuration->GetValueLong(CONFIGURATION_IPTV_BUFFER_COUNT, true, IPTV_BUFFER_COUNT_DEFAULT);

  // check value
  iptvBufferSize = (iptvBufferSize <= 0) ? IPTV_BUFFER_SIZE_DEFAULT : iptvBufferSize;
  iptvBufferCount = (iptvBufferCount <= 0) ? IPTV_BUFFER_COUNT_DEFAULT : iptvBufferCount;

  // ensure a minimum number of buffers
  pRequest->cBuffers = iptvBufferCount;
  pRequest->cbBuffer = iptvBufferSize;
  this->logger.Log(LOGGER_INFO, _T("%s: %s: requesting buffers: %d, size of each buffer: %d"), MODULE_NAME, METHOD_DECIDE_BUFFER_SIZE_NAME, pRequest->cBuffers, pRequest->cbBuffer);

  ALLOCATOR_PROPERTIES allocatorProperties;
  hr = pAlloc->SetProperties(pRequest, &allocatorProperties);
  if (FAILED(hr)) 
  {
    this->logger.Log(LOGGER_ERROR, _T("%s: %s: SetProperties() failed, return code: %d"), MODULE_NAME, METHOD_DECIDE_BUFFER_SIZE_NAME, hr);
    result = hr;
  }

  if (result == S_OK)
  {
    this->logger.Log(LOGGER_INFO, _T("%s: %s: returned buffers: %d, size of each buffer: %d"), MODULE_NAME, METHOD_DECIDE_BUFFER_SIZE_NAME, allocatorProperties.cBuffers, allocatorProperties.cbBuffer);

    // is this allocator unsuitable?
    if ((allocatorProperties.cBuffers < pRequest->cBuffers) || (allocatorProperties.cbBuffer < pRequest->cbBuffer))
    {
      this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_DECIDE_BUFFER_SIZE_NAME, _T("not enough buffers or not enough space in buffers in allocator"));
      result = E_FAIL;
    }
  }

  this->logger.Log(LOGGER_INFO, (result == S_OK) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, MODULE_NAME, METHOD_DECIDE_BUFFER_SIZE_NAME);
  return result;
}

HRESULT CMPIPTVSourceStream::OnThreadCreate(void) 
{
  return NOERROR;
}

HRESULT CMPIPTVSourceStream::OnThreadStartPlay(void) 
{
  HRESULT result = S_OK;
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_ON_THREAD_START_PLAY_NAME);
  totalReceived = 0;

  if (this->lockMutex == NULL)
  {
    this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_ON_THREAD_START_PLAY_NAME, _T("cannot create mutex"));
    result = E_FAIL;
  }

  if (result == S_OK)
  {
    // start winsock worker thread
    this->hReceiveDataWorkerThread = CreateThread( 
      NULL,                                   // default security attributes
      0,                                      // use default stack size  
      &CMPIPTVSourceStream::ReceiveDataWorker,// thread function name
      this,                                   // argument to thread function 
      0,                                      // use default creation flags 
      &dwReceiveDataWorkerThreadId);          // returns the thread identifier

    if (this->hReceiveDataWorkerThread == NULL)
    {
      // thread not created
      this->logger.Log(LOGGER_ERROR, _T("%s: %s: CreateThread() error: %d"), MODULE_NAME, METHOD_ON_THREAD_START_PLAY_NAME, GetLastError());
      result = E_FAIL;
    }
  }

  if (result == S_OK)
  {
    if (!SetThreadPriority(::GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL))
    {
      this->logger.Log(LOGGER_WARNING, _T("%s: %s: cannot set thread priority for main thread, error: %u"), MODULE_NAME, METHOD_ON_THREAD_START_PLAY_NAME, GetLastError());
    }
    if (!SetThreadPriority(this->hReceiveDataWorkerThread, THREAD_PRIORITY_TIME_CRITICAL))
    {
      this->logger.Log(LOGGER_WARNING, _T("%s: %s: cannot set thread priority for receive data thread, error: %u"), MODULE_NAME, METHOD_ON_THREAD_START_PLAY_NAME, GetLastError());
    }
  }

  this->logger.Log(LOGGER_INFO, (result == S_OK) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, MODULE_NAME, METHOD_ON_THREAD_START_PLAY_NAME);
  return result;
}

HRESULT CMPIPTVSourceStream::FillBuffer(IMediaSample *pSamp)
{
  HRESULT result = S_OK;
  this->logger.Log(LOGGER_DATA, METHOD_START_FORMAT, MODULE_NAME, METHOD_FILL_BUFFER_NAME);
  CheckPointer(pSamp, E_POINTER);

  // Now get buffers address
  char *pData;
  long cbData;

  pSamp->SetActualDataLength(0);

  if (pSamp->GetPointer((BYTE **)&pData) != S_OK) 
  {
    this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_FILL_BUFFER_NAME, _T("unable to get mediaSample pointer"));
    result = E_FAIL;
  }

  if (result == S_OK)
  {
    cbData = pSamp->GetSize();
    if (cbData <= 0) 
    {
      this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_FILL_BUFFER_NAME, _T("unable to get size of mediaSample buffer"));
      result = E_FAIL;
    }
  }

  if (result == S_OK)
  {
    if (this->activeProtocol != NULL)
    {
      unsigned int postedData = this->activeProtocol->FillBuffer(pSamp, pData, cbData);

      if ((postedData > 0) && (this->keepPidValues != NULL))
      {
        unsigned int expectedSize = (postedData / DVB_PACKET_SIZE) * DVB_PACKET_SIZE;
        if (expectedSize == postedData)
        {
          ALLOC_MEM_DEFINE_SET(tempBuffer, char, postedData, 0);
          unsigned int tempPostedData = 0;

          if (tempBuffer != NULL)
          {
            for(unsigned int i = 0; (i < ((postedData / DVB_PACKET_SIZE))); i++)
            {
              unsigned char *tsPacket = (unsigned char *)pData + i * DVB_PACKET_SIZE;
              if (tsPacket[0] == SYNC_BYTE)
              {
                PmtParser *pmtParser = new PmtParser(&this->crc32);
                if (pmtParser != NULL)
                {
                  if (pmtParser->SetPmtData(tsPacket, DVB_PACKET_SIZE))
                  {
                    if (pmtParser->IsValidPacket())
                    {
                      this->keepPidValues[pmtParser->GetPacketPid()] |= KEEP_PID_IN_STREAM;
                      if (pmtParser->GetPcrPid() != UINT_MAX)
                      {
                        this->keepPidValues[pmtParser->GetPcrPid()] |= KEEP_PID_IN_STREAM;
                      }

                      unsigned int j = 0;
                      while (j < pmtParser->GetPmtStreamDescriptions()->Count())
                      {
                        PmtStreamDescription *streamDescription = pmtParser->GetPmtStreamDescriptions()->GetItem(j);

                        if ((this->keepPidValues[streamDescription->GetStreamPid()] & KEEP_PID_IN_PMT) == 0)
                        {
                          pmtParser->GetPmtStreamDescriptions()->Remove(j);
                        }
                        else
                        {
                          j++;
                        }
                      }

                      if (pmtParser->RecalculateSectionLength())
                      {
                        unsigned char *pmtPacket = pmtParser->GetPmtPacket();
                        if (pmtPacket != NULL)
                        {
                          memcpy(tsPacket, pmtPacket, DVB_PACKET_SIZE);
                        }
                        FREE_MEM(pmtPacket);
                      }
                    }
                  }

                  delete pmtParser;
                }

                unsigned int tsPacketPid = (tsPacket[1] & 0x1F) << 8;
                tsPacketPid |= tsPacket[2];

                if (((this->keepPidValues[tsPacketPid] & KEEP_PID_IN_STREAM) != 0) || (tsPacketPid == 68))
                {
                  // copy TS packet if its PID is allowed
                  memcpy(tempBuffer + tempPostedData, tsPacket, DVB_PACKET_SIZE);
                  tempPostedData += DVB_PACKET_SIZE;
                }
              }
            }

            if (tempPostedData > 0)
            {
              memcpy(pData, tempBuffer, tempPostedData);
            }
            pSamp->SetActualDataLength(tempPostedData);
            postedData = tempPostedData;

            FREE_MEM(tempBuffer);
          }
        }
      }

      if ((postedData > 0) && (this->canChangeSidAndPid) && (this->newSid != SID_DEFAULT) && (this->newPid != PID_DEFAULT))
      {
        unsigned int expectedSize = (postedData / DVB_PACKET_SIZE) * DVB_PACKET_SIZE;
        if (expectedSize != postedData)
        {
          this->logger.Log(LOGGER_WARNING, _T("%s: %s: expected size %u and posted size %u are not same, changing SID and PID disabled"), MODULE_NAME, METHOD_FILL_BUFFER_NAME, expectedSize, postedData);
          this->canChangeSidAndPid = false;
        }
        else
        {
          for(unsigned int i = 0; (i < ((postedData / DVB_PACKET_SIZE)) && (this->canChangeSidAndPid)); i++)
          {
            unsigned char *tsPacket = (unsigned char *)pData + i * DVB_PACKET_SIZE;
            if (tsPacket[0] == SYNC_BYTE)
            {
              // synchronization byte correct
              unsigned int pid = ((tsPacket[1] & 0x1F) << 8) + tsPacket[2];

              if (pid < PID_COUNT)
              {
                // first look for MPEG TS packet with PAT
                if (pid == PID_PAT)
                {
                  // packet is PAT packet
                  // changing SID and PID is allowed only for stream with one channel
                  // analyse PAT if there is only one PMT
                  PatParser *patParser = new PatParser(&this->crc32);
                  if (patParser != NULL)
                  {
                    if (patParser->SetPatData(tsPacket, DVB_PACKET_SIZE))
                    {
                      if (patParser->IsValidPacket())
                      {
                        unsigned int programNumberCount = patParser->GetProgramNumberCount();
                        if (programNumberCount == 1)
                        {
                          unsigned int channelSid = patParser->GetProgramNumber(0);
                          unsigned int channelPid = patParser->GetPid(0);

                          if ((channelSid != UINT_MAX) && (channelPid != UINT_MAX))
                          {
                            this->oldSid = channelSid;
                            this->oldPid = channelPid;
                          }

                          if ((this->oldSid != SID_DEFAULT) && (this->oldPid != PID_DEFAULT) && (this->newSid != SID_DEFAULT) && (this->newPid != PID_DEFAULT))
                          {
                            // change SID and PID in PAT
                            // recalculate CRC32
                            bool setSid = patParser->SetProgramNumber(0, this->newSid);
                            bool setPid = patParser->SetPid(0, this->newPid);
                            bool setCrc32 = patParser->RecalculateCrc32();

                            if (setSid && setPid && setCrc32)
                            {
                              unsigned char *patPacket = patParser->GetPatPacket();
                              if (patPacket != NULL)
                              {
                                memcpy(tsPacket, patPacket, DVB_PACKET_SIZE);
                              }
                              FREE_MEM(patPacket);
                            }
                            else
                            {
                              this->logger.Log(LOGGER_WARNING, _T("%s: %s: cannot set SID, PID or recalculate CRC32, changing SID and PID disabled"), MODULE_NAME, METHOD_FILL_BUFFER_NAME);
                              this->canChangeSidAndPid = false;
                            }
                          }
                        }
                        else
                        {
                          this->logger.Log(LOGGER_WARNING, _T("%s: %s: program number count is not one: %u, changing SID and PID disabled"), MODULE_NAME, METHOD_FILL_BUFFER_NAME, programNumberCount);
                          this->canChangeSidAndPid = false;
                        }
                      }
                      else
                      {
                        this->logger.Log(LOGGER_WARNING, _T("%s: %s: not valid PAT packet, ignoring"), MODULE_NAME, METHOD_FILL_BUFFER_NAME);
                      }
                    }
                    delete patParser;
                  }
                  else
                  {
                    this->logger.Log(LOGGER_WARNING, _T("%s: %s: cannot create PAT parser, changing SID and PID disabled"), MODULE_NAME, METHOD_FILL_BUFFER_NAME);
                    this->canChangeSidAndPid = false;
                  }
                }

                if (pid == this->oldPid)
                {
                  // PID of MPEG TS packet is same as old PID
                  // this PID have to be changed to new PID
                  // checksum doesn't have to be recalculated

                  PmtParser *pmtParser = new PmtParser(&this->crc32);
                  if (pmtParser != NULL)
                  {
                    if (pmtParser->SetPmtData(tsPacket, DVB_PACKET_SIZE))
                    {
                      if (pmtParser->IsValidPacket())
                      {
                        // change SID and PID in PMT
                        // recalculate CRC32
                        bool setSid = pmtParser->SetProgramNumber(newSid);
                        bool setPid = pmtParser->SetPacketPid(this->newPid);
                        bool setCrc32 = pmtParser->RecalculateCrc32();

                        if (setSid && setPid && setCrc32)
                        {
                          unsigned char *pmtPacket = pmtParser->GetPmtPacket();
                          if (pmtPacket != NULL)
                          {
                            memcpy(tsPacket, pmtPacket, DVB_PACKET_SIZE);
                          }
                          FREE_MEM(pmtPacket);
                        }
                        else
                        {
                          this->logger.Log(LOGGER_WARNING, _T("%s: %s: cannot set SID, PID or recalculate CRC32, changing SID and PID disabled"), MODULE_NAME, METHOD_FILL_BUFFER_NAME);
                          this->canChangeSidAndPid = false;
                        }
                      }
                      else
                      {
                        this->logger.Log(LOGGER_WARNING, _T("%s: %s: not valid PMT packet, ignoring"), MODULE_NAME, METHOD_FILL_BUFFER_NAME);
                      }
                    }
                    delete pmtParser;
                  }
                  else
                  {
                    this->logger.Log(LOGGER_WARNING, _T("%s: %s: cannot create PMT parser, changing SID and PID disabled"), MODULE_NAME, METHOD_FILL_BUFFER_NAME);
                    this->canChangeSidAndPid = false;
                  }
                }
              }
              else
              {
                this->logger.Log(LOGGER_WARNING, _T("%s: %s: wrong PID, changing SID and PID disabled, PID: %u"), MODULE_NAME, METHOD_FILL_BUFFER_NAME, pid);
                this->canChangeSidAndPid = false;
              }
            }
            else
            {
              this->logger.Log(LOGGER_WARNING, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_FILL_BUFFER_NAME, _T("synchronization byte is not correct, changing SID and PID disabled"));
              this->canChangeSidAndPid = false;
            }
          }

          // now look for MPEG TS packet with PMT and replace old PID with new PID
        }
      }

      if (this->dumpRawTS && (postedData > 0))
      {
        TCHAR *folder = GetTvServerFolder();
        TCHAR *guid = ConvertGuidToString(this->activeProtocol->GetInstanceId());
        if ((folder != NULL) && (guid != NULL))
        {
          TCHAR *fileName = FormatString(_T("%slog\\mpiptv_output_dump_%s.ts"), folder, guid);
          if (fileName != NULL)
          {
            // we have raw TS file path
            FILE *stream = NULL;
            if (_tfopen_s(&stream, fileName, _T("ab")) == 0)
            {
              fwrite(pData, 1, postedData, stream);
              fclose(stream);
            }
          }
          FREE_MEM(fileName);
        }
        FREE_MEM(folder);
        FREE_MEM(guid);
      }

      if ((postedData > 0) && (this->analyzeDiscontinuity))
      {
        unsigned int expectedSize = (postedData / DVB_PACKET_SIZE) * DVB_PACKET_SIZE;
        if (expectedSize != postedData)
        {
          this->logger.Log(LOGGER_WARNING, _T("%s: %s: expected size %u and posted size %u are not same, analyze discontinuity disabled"), MODULE_NAME, METHOD_FILL_BUFFER_NAME, expectedSize, postedData);
          this->analyzeDiscontinuity = false;
        }
        else
        {
          // if expected size is same as posted data size
          if (this->pidCounters != NULL)
          {
            // analyze discontinuity only if were posted whole DVB packets
            for(unsigned int i = 0; (i < ((postedData / DVB_PACKET_SIZE)) && (this->analyzeDiscontinuity)); i++)
            {
              unsigned char *tsPacket = (unsigned char *)pData + i * DVB_PACKET_SIZE;
              if (tsPacket[0] == SYNC_BYTE)
              {
                // synchronization byte correct
                unsigned int pid = ((tsPacket[1] & 0x1F) << 8) + tsPacket[2];
                unsigned int currentCounter = (tsPacket[3] & 0x0F);
                unsigned int adaptationField = (tsPacket[3] & 0x30) >> 4;

                // check for right values
                // skip NULL packets (PID 0x1FFF)
                if ((pid < PID_COUNT) && (currentCounter <= 0x0F) && (adaptationField <= 0x03) && (pid != PID_NULL))
                {
                  // PID counters are incremented only if payload is present (adaptation field is 1 or 3)
                  if ((adaptationField == 0x01) || (adaptationField == 0x03))
                  {
                    // check if PID counter is set
                    if (this->pidCounters[pid] != (-1))
                    {
                      // PID counter for 'pid' is already set

                      unsigned int expectedCounter = (this->pidCounters[pid] + 1) & 0x0F;
                      if (expectedCounter != currentCounter)
                      {
                        this->logger.Log(LOGGER_WARNING, _T("%s: %s: discontinuity detected, PID: %u, expected counter: %u, packet counter: %u"), MODULE_NAME, METHOD_FILL_BUFFER_NAME, pid, expectedCounter, currentCounter);
                      }
                    }

                    // in all cases set PID counter
                    this->pidCounters[pid] = currentCounter;
                  }
                }
                else if (pid != PID_NULL)
                {
                  this->logger.Log(LOGGER_WARNING, _T("%s: %s: wrong PID, current counter or adaptation field, analyze discontinuity disabled, PID: %u, current counter: %u, adaptation field: %u"), MODULE_NAME, METHOD_FILL_BUFFER_NAME, pid, currentCounter, adaptationField);
                  this->analyzeDiscontinuity = false;
                }
              }
              else
              {
                this->logger.Log(LOGGER_WARNING, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_FILL_BUFFER_NAME, _T("synchronization byte is not correct, analyze discontinuity disabled"));
                this->analyzeDiscontinuity = false;
              }
            }
          }
        }
      }

      this->totalReceived += postedData;
    }
  }

  this->logger.Log(LOGGER_DATA, (result == S_OK) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, MODULE_NAME, METHOD_FILL_BUFFER_NAME);

  return result;
}

DWORD WINAPI CMPIPTVSourceStream::ReceiveDataWorker( LPVOID lpParam ) 
{
  CMPIPTVSourceStream* caller;
  caller = (CMPIPTVSourceStream *)lpParam;
  caller->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_RECEIVE_DATA_WORKER_NAME);
  unsigned int attempts = 0;
  bool stopReceiveingData = FALSE;

  // clear PID counters
  if (caller->pidCounters != NULL)
  {
    for(unsigned int i = 0; i < PID_COUNT; i++)
    {
      caller->pidCounters[i] = -1;
    }
  }

  while ((!caller->threadShouldExit) && (!stopReceiveingData))
  {
    Sleep(1);

    if (caller->activeProtocol != NULL)
    {
      unsigned int maximumAttempts = caller->activeProtocol->GetOpenConnectionMaximumAttempts();

      // if in active protocol is opened connection than receive data
      // if not than open connection
      if (caller->activeProtocol->IsConnected())
      {
        caller->activeProtocol->ReceiveData(&caller->threadShouldExit);
      }
      else
      {
        if (attempts < maximumAttempts)
        {
          int result = caller->activeProtocol->OpenConnection();
          switch (result)
          {
          case STATUS_OK:
            // set attempts to zero
            attempts = 0;
            break;
          case STATUS_ERROR_NO_RETRY:
            caller->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, _T("cannot open connection"));
            caller->status = STATUS_NO_DATA_ERROR;
            stopReceiveingData = TRUE;
            break;
          default:
            // increase attempts
            attempts++;
            break;
          }
        }
        else
        {
          caller->logger.Log(LOGGER_ERROR, _T("%s: %s: maximum attempts of opening connection reached, attempts: %u, maximum attempts: %u"), MODULE_NAME, METHOD_RECEIVE_DATA_WORKER_NAME, attempts, maximumAttempts);
          caller->status = STATUS_NO_DATA_ERROR;
          stopReceiveingData = true;
        }
      }
    }
  }

  caller->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_RECEIVE_DATA_WORKER_NAME);
  return S_OK;
}

HRESULT CMPIPTVSourceStream::DoBufferProcessingLoop(void)
{
  HRESULT result = S_FALSE;
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_DO_BUFFER_PROCESSING_LOOP_NAME);
  Command com;
  
  DWORD conditionalAccessWaitingTimeout = this->configuration->GetValueLong(CONFIGURATION_CONDITIONAL_ACCESS_WAITING_TIMEOUT, true, CONDITIONAL_ACCESS_WAITING_TIMEOUT_DEFAULT);
  conditionalAccessWaitingTimeout = (conditionalAccessWaitingTimeout < 0) ? CONDITIONAL_ACCESS_WAITING_TIMEOUT_DEFAULT : conditionalAccessWaitingTimeout;

  if (this->OnThreadStartPlay() != NOERROR)
  {
    this->status = STATUS_INITIALIZE_ERROR;
    // error occured while starting thread
  }
  else
  {
    this->status = STATUS_INITIALIZED;
    // suppress data from send to another filter
    bool suppressData = true;
    bool finishWork = false;
    do 
    {
      while ((!CheckRequest(&com)) && (!finishWork))
      {
        WaitForSingleObject(this->lockMutex, INFINITE);
        unsigned int occupiedSpace = 0;
        if (this->activeProtocol != NULL)
        {
          this->activeProtocol->GetSafeBufferSizes(this->lockMutex, NULL, &occupiedSpace, NULL);
        }
        if (occupiedSpace > 0)
        {
          this->logger.Log(LOGGER_DATA, _T("%s: %s: occupied space: %u"), MODULE_NAME, METHOD_DO_BUFFER_PROCESSING_LOOP_NAME, occupiedSpace);
        }
        ReleaseMutex(this->lockMutex);

        if ((suppressData) && (this->runMethodExecuted != 0))
        {
          if ((GetTickCount() - this->runMethodExecuted) >= conditionalAccessWaitingTimeout)
          {
            suppressData = false;
            this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_DO_BUFFER_PROCESSING_LOOP_NAME, _T("stop suppressing data"));
          }
        }

        if (occupiedSpace > 0)
        {
          this->status = STATUS_RECEIVING_DATA;

          IMediaSample *pSample;
          HRESULT hr = GetDeliveryBuffer(&pSample, NULL, NULL, 0);
          if (FAILED(hr)) 
          {
            this->logger.Log(LOGGER_ERROR, _T("%s: %s: GetDeliveryBuffer() error: %08X"), MODULE_NAME, METHOD_DO_BUFFER_PROCESSING_LOOP_NAME, hr);
            Sleep(10);
            continue;
            // go round again
            // perhaps the error will go away or the allocator is decommited and we will be asked to exit soon
          }

          // Virtual function user will override.
          hr = FillBuffer(pSample);
          if (suppressData)
          {
            // if waiting for MDAPI to initialize then ignore packets
            pSample->Release();
          }
          else if (hr == S_OK) 
          {
            if( pSample->GetActualDataLength() > 0)
            {
              this->logger.Log(LOGGER_DATA, _T("%s: %s: deliver sample, data length: %d"), MODULE_NAME, METHOD_DO_BUFFER_PROCESSING_LOOP_NAME, pSample->GetActualDataLength());
              hr = Deliver(pSample);     
            }

            pSample->Release();

            // downstream filter returns S_FALSE if it wants us to stop
            // or an error if it's reporting an error
            if(hr != S_OK)
            {
              this->logger.Log(LOGGER_ERROR, _T("%s: %s: Deliver() error: %08X"), MODULE_NAME, METHOD_DO_BUFFER_PROCESSING_LOOP_NAME, hr);
              result = S_OK;
              finishWork = true;
            }
          } 
          else if (hr == S_FALSE) 
          {
            // derived class wants us to stop pushing data
            pSample->Release();
            DeliverEndOfStream();
            result = S_OK;
            finishWork = true;
          } 
          else 
          {
            // derived class encountered an error
            pSample->Release();
            this->logger.Log(LOGGER_ERROR, _T("%s: %s: %s error: %08lX"), MODULE_NAME, METHOD_DO_BUFFER_PROCESSING_LOOP_NAME, METHOD_FILL_BUFFER_NAME, hr);
            DeliverEndOfStream();
            m_pFilter->NotifyEvent(EC_ERRORABORT, hr, 0);
            result = hr;
            finishWork = true;
          }
          // all paths release the sample
        }

        Sleep(1);
      }

      // For all commands sent to us there must be a Reply call!
      if (com == CMD_RUN || com == CMD_PAUSE) 
      {
        Reply(NOERROR);
      } 
      else if (com != CMD_STOP) 
      {
        Reply((DWORD) E_UNEXPECTED);
        this->logger.Log(LOGGER_WARNING, _T("%s: %s: unexpected command: %d"), MODULE_NAME, METHOD_DO_BUFFER_PROCESSING_LOOP_NAME, com);
      }
    } while ((com != CMD_STOP) && (!finishWork));
  }

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_DO_BUFFER_PROCESSING_LOOP_NAME);
  return result;
}

HRESULT CMPIPTVSourceStream::OnThreadDestroy(void) 
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_ON_THREAD_DESTROY_NAME); 
  this->threadShouldExit = true;

  // wait for the receive data worker thread to exit      
  if (this->hReceiveDataWorkerThread != NULL)
  {
    if (WaitForSingleObject(this->hReceiveDataWorkerThread, 1000) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_ON_THREAD_DESTROY_NAME, _T("thread didn't exit, terminating thread"));
      TerminateThread(this->hReceiveDataWorkerThread, 0);
    }
  }

  this->hReceiveDataWorkerThread = NULL;

  // close active protocol connection
  if (this->activeProtocol != NULL)
  {
    if (this->activeProtocol->IsConnected())
    {
      this->activeProtocol->CloseConnection();
    }

    this->activeProtocol = NULL;
  }

  this->logger.Log(LOGGER_INFO, _T("%s: %s: total read bytes %u"), MODULE_NAME, METHOD_ON_THREAD_DESTROY_NAME, totalReceived);
  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_NAME, METHOD_ON_THREAD_DESTROY_NAME);

  this->threadShouldExit = FALSE;
  return NOERROR;
}

HRESULT CMPIPTVSourceStream::Run(REFERENCE_TIME tStart)
{
  // set result value
  int result = RUN_NO_ERROR;

  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_NAME, METHOD_RUN_NAME);

  // set analyze discontinuity flag
  // it can be false if analysis fail in some way
  this->analyzeDiscontinuity = this->configuration->GetValueBool(CONFIGURATION_ANALYZE_DISCONTINUITY, true, ANALYZE_DISCONTINUITY_DEFAULT);

  DWORD ticks = GetTickCount();
  DWORD timeout = 0;

  if (this->activeProtocol != NULL)
  {
    // get receive data timeout for active protocol
    timeout = this->activeProtocol->GetReceiveDataTimeout();
    TCHAR *protocolName = this->activeProtocol->GetProtocolName();
    this->logger.Log(LOGGER_INFO, _T("%s: %s: active protocol '%s' timeout: %d (ms)"), MODULE_NAME, METHOD_RUN_NAME, protocolName, timeout);
    FREE_MEM(protocolName);
  }
  else
  {
    this->logger.Log(LOGGER_WARNING, METHOD_MESSAGE_FORMAT, MODULE_NAME, METHOD_RUN_NAME, _T("no active protocol"));
  }

  // wait for receiving data, timeout or exit
  while ((this->status != STATUS_RECEIVING_DATA) && (this->status != STATUS_NO_DATA_ERROR) && ((GetTickCount() - ticks) <= timeout) && (!this->threadShouldExit))
  {
    Sleep(1);
  }

  switch(this->status)
  {
  case STATUS_NONE:
    result = RUN_ERROR_UNEXPECTED;
    break;
  case STATUS_INITIALIZE_ERROR:
    result = RUN_ERROR_INITIALIZE;
    break;
  case STATUS_INITIALIZED:
  case STATUS_NO_DATA_ERROR:
    result = RUN_ERROR_NO_DATA_AVAILABLE;
    break;
  case STATUS_RECEIVING_DATA:
    result = RUN_NO_ERROR;
    this->runMethodExecuted = GetTickCount();
    break;
  default:
    result = RUN_ERROR_UNEXPECTED;
    break;
  }

  this->logger.Log(LOGGER_INFO, _T("%s: %s: result: %08X"), MODULE_NAME, METHOD_RUN_NAME, result);
  this->logger.Log(LOGGER_INFO, (result == RUN_NO_ERROR) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, MODULE_NAME, METHOD_RUN_NAME, result);
  return result;
}

bool CMPIPTVSourceStream::Load(const TCHAR *url, const CParameterCollection *parameters)
{
  if (this->keepPidValues != NULL)
  {
    for (unsigned int i = 0; i < PID_COUNT; i++)
    {
      this->keepPidValues[i] = KEEP_PID_IN_PMT | KEEP_PID_IN_STREAM;
    }
  }

  if (parameters != NULL)
  {
    this->newSid = ((CParameterCollection *)parameters)->GetValueLong(CONFIGURATION_SID_VALUE, true, UINT_MAX);
    this->newPid = ((CParameterCollection *)parameters)->GetValueLong(CONFIGURATION_PID_VALUE, true, UINT_MAX);
    this->oldSid = SID_DEFAULT;
    this->oldPid = PID_DEFAULT;
    this->canChangeSidAndPid = true;

    this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: changed SID: %u, changed PID: %u"), MODULE_NAME, _T("Load()"), this->newSid, this->newPid);

    if (((CParameterCollection *)parameters)->Contains(CONFIGURATION_KEEP_PID_VALUE, true))
    {
      if (this->keepPidValues != NULL)
      {
        for (unsigned int i = 0; i < PID_COUNT; i++)
        {
          if ((i <= MAX_RESERVED_PID) || (i == PID_NULL))
          {
            // reserved PIDs and NULL packets will be transmitted always
            this->keepPidValues[i] = KEEP_PID_IN_STREAM;
          }
          else
          {
            this->keepPidValues[i] = NOT_KEEP_PID;
          }
        }

        for (unsigned int i = 0; i < ((CParameterCollection *)parameters)->Count(); i++)
        {
          CParameter *parameter = ((CParameterCollection *)parameters)->GetItem(i);

          if (parameter != NULL)
          {
            if (_tcsicmp(parameter->GetName(), CONFIGURATION_KEEP_PID_VALUE) == 0)
            {
              TCHAR *end = NULL;
              long valueLong = _tcstol(parameter->GetValue(), &end, 10);
              if ((valueLong == 0) && (parameter->GetValue() == end))
              {
                // error while converting
                valueLong = UINT_MAX;
              }

              if (valueLong != UINT_MAX)
              {
                this->keepPidValues[(unsigned int)valueLong] = KEEP_PID_IN_PMT | KEEP_PID_IN_STREAM;
                this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: keeping PID: %u"), MODULE_NAME, _T("Load()"), (unsigned int)valueLong);
              }
            }
          }
        }
      }
    }
  }

  // for each protocol run ParseUrl() method
  // those which return STATUS_OK supports protocol
  // set active protocol to first implementation
  bool retval = false;
  for(unsigned int i = 0; i < this->dllTotal; i++)
  {
    if (protocolImplementations[i].pImplementation != NULL)
    {
      protocolImplementations[i].supported = (protocolImplementations[i].pImplementation->ParseUrl(url, parameters) == STATUS_OK);
      if ((protocolImplementations[i].supported) && (!retval))
      {
        // active protocol wasn't set yet
        this->activeProtocol = protocolImplementations[i].pImplementation;
      }

      retval |= protocolImplementations[i].supported;
    }
  }

  return retval;
}

GUID CMPIPTVSourceStream::GetInstanceId(void)
{
  return this->logger.loggerInstance;
}

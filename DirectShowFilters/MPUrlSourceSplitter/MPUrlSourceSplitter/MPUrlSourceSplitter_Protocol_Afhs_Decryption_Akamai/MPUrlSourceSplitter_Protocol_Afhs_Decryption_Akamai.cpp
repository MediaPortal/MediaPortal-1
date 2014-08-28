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

#include "MPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai.h"
#include "MPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai_Parameters.h"
#include "MPUrlSourceSplitter_Protocol_Afhs_Parameters.h"
#include "AfhsDecryptionPluginConfiguration.h"
#include "Parameters.h"
#include "ErrorCodes.h"
#include "formatUrl.h"
#include "VersionInfo.h"

// decryption implementation name
#ifdef _DEBUG
#define DECRYPTION_IMPLEMENTATION_NAME                                        L"MPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamaid"
#else
#define DECRYPTION_IMPLEMENTATION_NAME                                        L"MPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai"
#endif

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai *protocol = (CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai *)plugin;

    delete protocol;
  }
}

CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CAfhsDecryptionPlugin(result, logger, configuration)
{
  this->akamaiFlashInstance = NULL;
  this->lastKeyUrl = NULL;
  this->lastKey = NULL;
  this->lastKeyLength = 0;
  this->akamaiGuid = NULL;
  this->akamaiSwfFileName = NULL;
  this->lastTimestamp = 0;
  this->sessionID = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, DECRYPTION_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI);
    if (version != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, DECRYPTION_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
    }
    FREE_MEM(version);

    version = CCurlInstance::GetCurlVersion();
    if (version != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, DECRYPTION_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
    }
    FREE_MEM(version);

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, DECRYPTION_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::~CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, DECRYPTION_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  FREE_MEM_CLASS(this->akamaiFlashInstance);
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->akamaiSwfFileName, DeleteFile(this->akamaiSwfFileName));

  FREE_MEM(this->lastKeyUrl);
  FREE_MEM(this->lastKey);
  FREE_MEM(this->akamaiGuid);
  FREE_MEM(this->akamaiSwfFileName);
  FREE_MEM(this->sessionID);

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, DECRYPTION_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));
}

// CPlugin implementation

const wchar_t *CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::GetName(void)
{
  return AFHS_PROTOCOL_DECRYPTION_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::Initialize(CPluginConfiguration *configuration)
{
  CAfhsDecryptionPluginConfiguration *decryptionConfiguration = (CAfhsDecryptionPluginConfiguration *)configuration;

  HRESULT result = S_OK;
  CHECK_POINTER_HRESULT(result, decryptionConfiguration, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    this->configuration->Clear();

    CHECK_CONDITION_HRESULT(result, this->configuration->Append(decryptionConfiguration->GetConfiguration()), result, E_OUTOFMEMORY);

    this->configuration->LogCollection(this->logger, LOGGER_VERBOSE, DECRYPTION_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);
  }

  return result;
}

// CAfhsDecryptionPlugin implementation

HRESULT CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::GetDecryptionResult(CAfhsDecryptionContext *decryptionContext)
{
  if (this->decryptionResult == DECRYPTION_RESULT_PENDING)
  {
    if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_FLAG_DATA_ANALYSED) &&
      this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_FLAG_KEY_REQUEST_PENDING))
    {
      //if (this->receivedDataAnalysed && this->keyRequestPending)
      //{
      //  // data were analysed and key request is pending
      //  result = DecryptionResult_Pending;

      //  CSegmentFragment *keySegmentFragment = context->GetSegmentsFragments()->GetItem(context->GetSegmentsFragments()->Count() - 1);
      //  if (keySegmentFragment->IsDownloaded())
      //  {
      //    // key request is on the end of segments and fragments collection
      //    // check status of download
      //    // if success then extract key, in another case report error

      //    FREE_MEM(this->lastKey);
      //    this->GetDecryptionKeyFromSegmentFragment(keySegmentFragment, &this->lastKey, &this->lastKeyLength);

      //    if (this->lastKey == NULL)
      //    {
      //      FREE_MEM(this->lastKey);
      //      result = DecryptionResult_Error;
      //    }
      //    else
      //    {
      //      context->SetSegmentFragmentToDownload(1);
      //      this->receivedDataCanBeDecrypted = true;
      //      result = DecryptionResult_Known;
      //    }

      //    context->GetSegmentsFragments()->Remove(context->GetSegmentsFragments()->Count() - 1);
      //    this->keyRequestPending = false;
      //  }
      //}
    }

    if (SUCCEEDED(this->decryptionResult) &&
      (!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_FLAG_DATA_ANALYSED)))
    {
      // data are analysed
      // by default we don't known pattern
      this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_FLAG_DATA_ANALYSED;
      this->decryptionResult = DECRYPTION_RESULT_NOT_KNOWN;

      CIndexedAfhsSegmentFragmentCollection *indexedEncryptedSegmentFragments = new CIndexedAfhsSegmentFragmentCollection(&this->decryptionResult);
      CHECK_CONDITION_EXECUTE(SUCCEEDED(this->decryptionResult), this->decryptionResult = decryptionContext->GetSegmentsFragments()->GetEncryptedStreamFragments(indexedEncryptedSegmentFragments));

      CHECK_CONDITION_EXECUTE(SUCCEEDED(this->decryptionResult), this->decryptionResult = DECRYPTION_RESULT_NOT_KNOWN);
      CHECK_CONDITION_HRESULT(this->decryptionResult, indexedEncryptedSegmentFragments->Count() != 0, this->decryptionResult, E_AFHS_AKAMAI_DECRYPTOR_INVALID_COUNT_OF_ENCRYPTED_SEGMENT_FRAGMENTS);

      if (SUCCEEDED(this->decryptionResult))
      {
        CIndexedAfhsSegmentFragment *indexedEncryptedSegmentFragment = indexedEncryptedSegmentFragments->GetItem(0);
        CAfhsSegmentFragment *currentEncryptedFragment = indexedEncryptedSegmentFragment->GetItem();

        CParsedMediaDataBox *parsedMediaDataBox = new CParsedMediaDataBox(&this->decryptionResult);
        CHECK_POINTER_HRESULT(this->decryptionResult, parsedMediaDataBox, this->decryptionResult, E_OUTOFMEMORY);

        CHECK_CONDITION_EXECUTE(SUCCEEDED(this->decryptionResult), this->decryptionResult = this->ParseMediaDataBox(parsedMediaDataBox, decryptionContext, currentEncryptedFragment));
        CHECK_CONDITION_EXECUTE(SUCCEEDED(this->decryptionResult), this->decryptionResult = DECRYPTION_RESULT_NOT_KNOWN);

        if (SUCCEEDED(this->decryptionResult) && parsedMediaDataBox->IsMediaDataBox() && (parsedMediaDataBox->GetAkamaiGuid() != NULL) && (parsedMediaDataBox->GetAkamaiFlvPackets()->Count() > 0))
        {
          // it is media data box
          // specified akamai GUID
          // at least one akamai FLV packet

          FREE_MEM(this->akamaiGuid);
          this->akamaiGuid = Duplicate(parsedMediaDataBox->GetAkamaiGuid());
          CHECK_POINTER_HRESULT(this->decryptionResult, this->akamaiGuid, this->decryptionResult, E_OUTOFMEMORY);

          if (SUCCEEDED(this->decryptionResult))
          {
            // key url have to be in first FLV packet
            CAkamaiFlvPacket *packet = parsedMediaDataBox->GetAkamaiFlvPackets()->GetItem(0);

            if (packet->HasKey())
            {
              if ((CompareWithNull(this->lastKeyUrl, packet->GetKeyUrl()) != 0) || (this->lastKey == NULL))
              {
                // download new decryption key from url
                CHECK_CONDITION_HRESULT(this->decryptionResult, !decryptionContext->GetCurlInstance()->IsLockedCurlInstance(), this->decryptionResult, E_AFHS_AKAMAI_DECRYPTOR_CANNOT_DOWNLOAD_KEY);

                if (SUCCEEDED(this->decryptionResult))
                {
                  FREE_MEM(this->lastKeyUrl);
                  FREE_MEM(this->sessionID);
                  this->sessionID = Duplicate(packet->GetSessionId());
                  this->lastKeyUrl = Duplicate(packet->GetKeyUrl());

                  CHECK_POINTER_HRESULT(this->decryptionResult, this->sessionID, this->decryptionResult, E_OUTOFMEMORY);
                  CHECK_POINTER_HRESULT(this->decryptionResult, this->lastKeyUrl, this->decryptionResult, E_OUTOFMEMORY);

                  if (SUCCEEDED(this->decryptionResult))
                  {
                    wchar_t *segmentFragmentUrl = decryptionContext->GetSegmentsFragments()->GetSegmentFragmentUrl(currentEncryptedFragment);
                    CHECK_POINTER_HRESULT(this->decryptionResult, segmentFragmentUrl, this->decryptionResult, E_OUTOFMEMORY);

                    if (SUCCEEDED(this->decryptionResult))
                    {
                      wchar_t *keyUrl = this->GetKeyUrlFromUrl(segmentFragmentUrl, packet->GetKeyUrl(), this->akamaiGuid);
                      CHECK_POINTER_HRESULT(this->decryptionResult, keyUrl, this->decryptionResult, E_OUTOFMEMORY);

                      if (SUCCEEDED(this->decryptionResult))
                      {
                        CAfhsDownloadRequest *request = new CAfhsDownloadRequest(&this->decryptionResult);
                        CHECK_POINTER_HRESULT(this->decryptionResult, request, this->decryptionResult, E_OUTOFMEMORY);

                        if (SUCCEEDED(this->decryptionResult))
                        {
                          // set finish time, all methods must return before finish time
                          unsigned int finishTime = UINT_MAX;
                          if (SUCCEEDED(this->decryptionResult))
                          {
                            finishTime = decryptionContext->GetConfiguration()->GetValueUnsignedInt(PARAMETER_NAME_FINISH_TIME, true, UINT_MAX);
                            if (finishTime != UINT_MAX)
                            {
                              unsigned int currentTime = GetTickCount();
                              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: finish time specified, current time: %u, finish time: %u, diff: %u (ms)", DECRYPTION_IMPLEMENTATION_NAME, METHOD_GET_DECRYPTION_RESULT_NAME, currentTime, finishTime, finishTime - currentTime);
                              decryptionContext->GetConfiguration()->Remove(PARAMETER_NAME_FINISH_TIME, true);
                            }
                          }

                          request->SetFinishTime(finishTime);
                          request->SetReceivedDataTimeout(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_AFHS_OPEN_CONNECTION_TIMEOUT, true, AFHS_OPEN_CONNECTION_TIMEOUT_DEFAULT));
                          request->SetNetworkInterfaceName(this->configuration->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));

                          CHECK_CONDITION_HRESULT(this->decryptionResult, request->SetUrl(keyUrl), this->decryptionResult, E_OUTOFMEMORY);
                          CHECK_CONDITION_HRESULT(this->decryptionResult, request->SetCookie(this->configuration->GetValue(PARAMETER_NAME_AFHS_COOKIE, true, NULL)), this->decryptionResult, E_OUTOFMEMORY);
                          request->SetHttpVersion(this->configuration->GetValueLong(PARAMETER_NAME_AFHS_VERSION, true, HTTP_VERSION_DEFAULT));
                          request->SetIgnoreContentLength((this->configuration->GetValueLong(PARAMETER_NAME_AFHS_IGNORE_CONTENT_LENGTH, true, HTTP_IGNORE_CONTENT_LENGTH_DEFAULT) == 1L));
                          CHECK_CONDITION_HRESULT(this->decryptionResult, request->SetReferer(this->configuration->GetValue(PARAMETER_NAME_AFHS_REFERER, true, NULL)), this->decryptionResult, E_OUTOFMEMORY);
                          CHECK_CONDITION_HRESULT(this->decryptionResult, request->SetUserAgent(this->configuration->GetValue(PARAMETER_NAME_AFHS_USER_AGENT, true, NULL)), this->decryptionResult, E_OUTOFMEMORY);

                          if (SUCCEEDED(this->decryptionResult))
                          {
                            if (SUCCEEDED(decryptionContext->GetCurlInstance()->LockCurlInstance(this)))
                            {
                              if (SUCCEEDED(decryptionContext->GetCurlInstance()->Initialize(request)))
                              {
                                // all parameters set
                                // start receiving data

                                if (SUCCEEDED(decryptionContext->GetCurlInstance()->StartReceivingData()))
                                {
                                  decryptionContext->GetCurlInstance()->SetConnectionState(Opening);
                                  this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_FLAG_KEY_REQUEST_PENDING;
                                }
                                else
                                {
                                  decryptionContext->GetCurlInstance()->SetConnectionState(OpeningFailed);

                                  // we must unlock CURL instance, because we don't use it more
                                  decryptionContext->GetCurlInstance()->UnlockCurlInstance(this);

                                  // try it later
                                  this->flags &= ~MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_FLAG_DATA_ANALYSED;
                                  this->decryptionResult = DECRYPTION_RESULT_PENDING;
                                }
                              }
                              else
                              {
                                decryptionContext->GetCurlInstance()->SetConnectionState(InitializeFailed);

                                // we must unlock CURL instance, because we don't use it more
                                decryptionContext->GetCurlInstance()->UnlockCurlInstance(this);

                                // try it later
                                this->flags &= ~MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_FLAG_DATA_ANALYSED;
                                this->decryptionResult = DECRYPTION_RESULT_PENDING;
                              }
                            }
                            else
                            {
                              decryptionContext->GetCurlInstance()->SetConnectionState(InitializeFailed);
                              this->logger->Log(LOGGER_WARNING, L"%s: %s: cannot lock CURL instance, owner: 0x%p, lock count: %u", DECRYPTION_IMPLEMENTATION_NAME, METHOD_GET_DECRYPTION_RESULT_NAME, decryptionContext->GetCurlInstance()->GetOwner(), decryptionContext->GetCurlInstance()->GetOwnerLockCount());

                              // try it later
                              this->flags &= ~MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_FLAG_DATA_ANALYSED;
                              this->decryptionResult = DECRYPTION_RESULT_PENDING;
                            }
                          }
                        }

                        FREE_MEM_CLASS(request);
                      }

                      FREE_MEM(keyUrl);
                    }

                    FREE_MEM(segmentFragmentUrl);
                  }
                }

                CHECK_CONDITION_EXECUTE(SUCCEEDED(this->decryptionResult), this->decryptionResult = DECRYPTION_RESULT_PENDING);
              }
            }
          }
        }

        FREE_MEM_CLASS(parsedMediaDataBox);
      }

      FREE_MEM_CLASS(indexedEncryptedSegmentFragments);
    }

    // check if we have valid and initialized flash instance
    if (SUCCEEDED(this->decryptionResult) &&
      this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_FLAG_DATA_ANALYSED) &&
      this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_FLAG_DATA_CAN_BE_DECRYPTED) &&
      (this->akamaiFlashInstance == NULL))
    {
      // flash instance is not initialized
      CLinearBuffer *akamaiDecryptorResource = this->GetResource(L"AKAMAI_DECRYPTOR", L"DATA");
      CHECK_POINTER_HRESULT(this->decryptionResult, akamaiDecryptorResource, this->decryptionResult, E_AFHS_AKAMAI_DECRYPTOR_CANNOT_LOAD_DECRYPTOR);
      
      if (SUCCEEDED(this->decryptionResult))
      {
        // save akamai decryptor to filesystem and create flash instance
        FREE_MEM(this->akamaiSwfFileName);
        this->akamaiSwfFileName = this->GetAkamaiSwfFileName();
        CHECK_POINTER_HRESULT(this->decryptionResult, this->akamaiSwfFileName, this->decryptionResult, E_AFHS_AKAMAI_DECRYPTOR_NO_DECRYPTOR_FILE_NAME);

        if (SUCCEEDED(this->decryptionResult))
        {
          // open or create file
          HANDLE akamaiSwfFile = CreateFile(this->akamaiSwfFileName, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_ALWAYS, FILE_FLAG_WRITE_THROUGH, NULL);
          CHECK_CONDITION_HRESULT(this->decryptionResult, akamaiSwfFile != INVALID_HANDLE_VALUE, this->decryptionResult, E_AFHS_AKAMAI_DECRYPTOR_CANNOT_SAVE_DECRYPTOR);

          if (SUCCEEDED(this->decryptionResult))
          {
            ALLOC_MEM_DEFINE_SET(buffer, uint8_t, akamaiDecryptorResource->GetBufferOccupiedSpace(), 0);
            CHECK_POINTER_HRESULT(this->decryptionResult, buffer, this->decryptionResult, E_OUTOFMEMORY);

            if (SUCCEEDED(this->decryptionResult))
            {
              akamaiDecryptorResource->CopyFromBuffer(buffer, akamaiDecryptorResource->GetBufferOccupiedSpace());

              // write data to swf file file
              unsigned long written = 0;

              CHECK_CONDITION_HRESULT(this->decryptionResult, WriteFile(akamaiSwfFile, buffer, akamaiDecryptorResource->GetBufferOccupiedSpace(), &written, NULL) != 0, this->decryptionResult, HRESULT_FROM_WIN32(GetLastError()));
              CHECK_CONDITION_HRESULT(this->decryptionResult, written == akamaiDecryptorResource->GetBufferOccupiedSpace(), this->decryptionResult, E_AFHS_AKAMAI_DECRYPTOR_CANNOT_SAVE_DECRYPTOR);
            }

            FREE_MEM(buffer);

            CloseHandle(akamaiSwfFile);
            akamaiSwfFile = INVALID_HANDLE_VALUE;
          }
        }
      }

      CHECK_CONDITION_EXECUTE(FAILED(this->decryptionResult), this->logger->Log(LOGGER_INFO, L"%s: %s: cannot save akamai decryption wrapper, error: 0x%08X", DECRYPTION_IMPLEMENTATION_NAME, METHOD_GET_DECRYPTION_RESULT_NAME, this->decryptionResult));

      if (SUCCEEDED(this->decryptionResult))
      {
        this->akamaiFlashInstance = new CAkamaiFlashInstance(&this->decryptionResult, this->logger, DECRYPTION_IMPLEMENTATION_NAME, this->akamaiSwfFileName);
        CHECK_POINTER_HRESULT(this->decryptionResult, this->akamaiFlashInstance, this->decryptionResult, E_OUTOFMEMORY);

        if (SUCCEEDED(this->decryptionResult))
        {
          this->decryptionResult = this->akamaiFlashInstance->Initialize();

          CHECK_CONDITION_EXECUTE(FAILED(this->decryptionResult), this->logger->Log(LOGGER_INFO, L"%s: %s: cannot initialize flash instance, error: 0x%08X", DECRYPTION_IMPLEMENTATION_NAME, METHOD_GET_DECRYPTION_RESULT_NAME, this->decryptionResult));
          CHECK_CONDITION_EXECUTE(SUCCEEDED(this->decryptionResult), this->decryptionResult = DECRYPTION_RESULT_PENDING);
        }

        CHECK_CONDITION_EXECUTE(FAILED(this->decryptionResult), FREE_MEM_CLASS(this->akamaiFlashInstance));
      }

      FREE_MEM_CLASS(akamaiDecryptorResource);
    }

    if (SUCCEEDED(this->decryptionResult) &&
      this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_FLAG_DATA_ANALYSED) &&
      this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_AKAMAI_FLAG_DATA_CAN_BE_DECRYPTED) &&
      (this->akamaiFlashInstance != NULL))
    {
      AkamaiDecryptorState state = this->akamaiFlashInstance->GetState();
      if (state == AkamaiDecryptorState_NotInitialized)
      {
        this->akamaiFlashInstance->SetDecryptionModuleUrl(this->configuration->GetValue(PARAMETER_NAME_PROTOCOL_AFHS_DECRYPTION_AKAMAI_MODULE_URL, true, PARAMETER_NAME_PROTOCOL_AFHS_DECRYPTION_AKAMAI_MODULE_URL_DEFAULT));
        this->decryptionResult = DECRYPTION_RESULT_PENDING;
      }
      else if (state == AkamaiDecryptorState_Pending)
      {
        this->decryptionResult = DECRYPTION_RESULT_PENDING;
      }
      else if (state == AkamaiDecryptorState_Error)
      {
        // decryptor in error state, decryption impossible
        this->logger->Log(LOGGER_ERROR, L"%s: %s: decryption plugin '%s' in error state, error: '%s'", DECRYPTION_IMPLEMENTATION_NAME, METHOD_GET_DECRYPTION_RESULT_NAME, this->GetName(), this->akamaiFlashInstance->GetError());
        this->decryptionResult = E_AFHS_AKAMAI_DECRYPTOR_GENERAL_ERROR;
      }
      else if (state == AkamaiDecryptorState_Ready)
      {
        this->logger->Log(LOGGER_INFO, L"%s: %s: decryption plugin '%s' in ready state", DECRYPTION_IMPLEMENTATION_NAME, METHOD_GET_DECRYPTION_RESULT_NAME, this->GetName());
        this->decryptionResult = DECRYPTION_RESULT_KNOWN;
      }
      else if (state == AkamaiDecryptorState_Undefined)
      {
        // decryptor in undefined state, decryption impossible
        this->logger->Log(LOGGER_ERROR, L"%s: %s: decryption plugin '%s' in undefined state", DECRYPTION_IMPLEMENTATION_NAME, METHOD_GET_DECRYPTION_RESULT_NAME, this->GetName());
        this->decryptionResult = E_AFHS_AKAMAI_DECRYPTOR_GENERAL_ERROR;
      }
    }
  }

  return this->decryptionResult;
}

unsigned int CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::GetDecryptionScore(void)
{
  return 100;
}

void CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::ClearSession(void)
{
  __super::ClearSession();

  FREE_MEM_CLASS(this->akamaiFlashInstance);
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->akamaiSwfFileName, DeleteFile(this->akamaiSwfFileName));

  FREE_MEM(this->lastKeyUrl);
  FREE_MEM(this->lastKey);
  this->lastKeyLength = 0;
  FREE_MEM(this->akamaiGuid);
  FREE_MEM(this->akamaiSwfFileName);
  this->lastTimestamp = 0;
  FREE_MEM(this->sessionID);
}

HRESULT CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::DecryptSegmentFragments(CAfhsDecryptionContext *decryptionContext)
{
  /*HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, decryptionContext);
  CHECK_POINTER_HRESULT(result, decryptionContext->GetSegmentsFragments(), result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    CIndexedAfhsSegmentFragmentCollection *indexedEncryptedSegmentFragments = new CIndexedAfhsSegmentFragmentCollection(&result);
    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = decryptionContext->GetSegmentsFragments()->GetEncryptedStreamFragments(indexedEncryptedSegmentFragments));

    for (unsigned int i = 0; (SUCCEEDED(result) && (i < indexedEncryptedSegmentFragments->Count())); i++)
    {
      CIndexedAfhsSegmentFragment *indexedEncryptedSegmentFragment = indexedEncryptedSegmentFragments->GetItem(i);
      CAfhsSegmentFragment *currentEncryptedFragment = indexedEncryptedSegmentFragment->GetItem();

      currentEncryptedFragment->SetEncrypted(false, UINT_MAX);
      currentEncryptedFragment->SetDecrypted(true, UINT_MAX);

      decryptionContext->GetSegmentsFragments()->UpdateIndexes(indexedEncryptedSegmentFragment->GetItemIndex(), 1);
    }

    FREE_MEM_CLASS(indexedEncryptedSegmentFragments);
  }

  return result;*/

  return E_NOTIMPL;
}

/* protected methods */

CLinearBuffer *CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::GetResource(const wchar_t *name, const wchar_t *type)
{
  HRESULT result = S_OK;
  CLinearBuffer *resourceData = NULL;
  HMODULE module = GetModuleHandle(L"MPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai.dll");
  HRSRC resourceBlockHandle = FindResource(module, name, type);

  if (resourceBlockHandle != NULL)
  {
    HGLOBAL dataHandle = LoadResource(module, resourceBlockHandle);
    if (dataHandle != NULL)
    {
      DWORD size = SizeofResource(module, resourceBlockHandle);
      if (size != 0)
      {
        LPVOID data = LockResource(dataHandle);
        if (data != NULL)
        {
          resourceData = new CLinearBuffer(&result, size);
          CHECK_POINTER_HRESULT(result, resourceData, result, E_OUTOFMEMORY);

          CHECK_CONDITION_EXECUTE(SUCCEEDED(result), resourceData->AddToBuffer((const unsigned char*)data, size));
        }
      }
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(resourceData));
  return resourceData;
}

wchar_t *CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::GetAkamaiSwfFileName(void)
{
  wchar_t *result = NULL;
  const wchar_t *folder = this->configuration->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

  if (folder != NULL)
  {
    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
    if (guid != NULL)
    {
      result = FormatString(L"%smpurlsourcesplitter_protocol_afhs_decryption_akamai_%s.swf", folder, guid);
    }
    FREE_MEM(guid);
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::ParseMediaDataBox(CParsedMediaDataBox *parsedMediaDataBox, CAfhsDecryptionContext *context, CAfhsSegmentFragment *segmentFragment)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, parsedMediaDataBox);
  CHECK_POINTER_DEFAULT_HRESULT(result, context);
  CHECK_POINTER_DEFAULT_HRESULT(result, segmentFragment);

  if (SUCCEEDED(result))
  {
      int index = IndexOf(context->GetManifestUrl(), AKAMAI_GUID_URL_PART);
      if (index != (-1))
      {
        wchar_t *akamaiGuid = Substring(context->GetManifestUrl(), (unsigned int)index + AKAMAI_GUID_URL_PART_LENGTH);
        CHECK_POINTER_HRESULT(result, akamaiGuid, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, parsedMediaDataBox->SetAkamaiGuid(akamaiGuid), result, E_OUTOFMEMORY);
        FREE_MEM(akamaiGuid);
      }

      if (SUCCEEDED(result))
      {
        // parse segment and fragment for media data box
        CMediaDataBox *mediaDataBox = NULL;
        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->GetMediaDataBox(&mediaDataBox, segmentFragment));

        if (SUCCEEDED(result))
        {
          parsedMediaDataBox->SetMediaDataBox(true);

          if (SUCCEEDED(result) && (mediaDataBox->GetPayloadSize() != 0))
          {
            CLinearBuffer *mediaDataBoxPayload = new CLinearBuffer(&result, (unsigned int)mediaDataBox->GetPayloadSize());
            CHECK_POINTER_HRESULT(result, mediaDataBoxPayload, result, E_OUTOFMEMORY);

            CHECK_CONDITION_EXECUTE(SUCCEEDED(result), mediaDataBoxPayload->AddToBuffer((unsigned char *)mediaDataBox->GetPayload(), (uint32_t)mediaDataBox->GetPayloadSize()));

            while (SUCCEEDED(result) && (mediaDataBoxPayload->GetBufferOccupiedSpace() != 0))
            {
              // parse media data box for known akamai pattern
              CAkamaiFlvPacket *akamaiFlvPacket = NULL;
              result = this->GetAkamaiFlvPacket(&akamaiFlvPacket, mediaDataBoxPayload);

              if (SUCCEEDED(result))
              {
                CHECK_CONDITION_HRESULT(result, parsedMediaDataBox->GetAkamaiFlvPackets()->Add(akamaiFlvPacket), result, E_OUTOFMEMORY);
                mediaDataBoxPayload->RemoveFromBuffer(akamaiFlvPacket->GetSize());
              }
              CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(akamaiFlvPacket));
            }

            FREE_MEM_CLASS(mediaDataBoxPayload);
          }
        }

        CHECK_CONDITION_EXECUTE(result == E_AFHS_AKAMAI_DECRYPTOR_NOT_CREATED_BOX, result = S_OK);
        CHECK_CONDITION_EXECUTE(result == E_AFHS_AKAMAI_DECRYPTOR_NOT_FLV_PACKET, result = S_OK);
        CHECK_CONDITION_EXECUTE(result == E_AFHS_AKAMAI_DECRYPTOR_NOT_AKAMAI_FLV_PACKET, result = S_OK);

        FREE_MEM_CLASS(mediaDataBox);
      }
  }

  return result;
}

wchar_t *CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::GetKeyUrlFromUrl(const wchar_t *segmentFragmentUrl, const wchar_t *packetUrl, const wchar_t *akamaiGuid)
{
  wchar_t *result = NULL;

  int index = IndexOf(packetUrl, L"://");
  if (index == (-1))
  {
    // no host in packet url
    wchar_t *host = GetHost(segmentFragmentUrl);
    if (host != NULL)
    {
      result = FormatAbsoluteUrl(host, packetUrl);
    }
    FREE_MEM(host);
  }
  else
  {
    // host in packet url
    result = Duplicate(packetUrl);
  }

  // add to result akamai GUID
  wchar_t *tempResult = FormatString(L"%s?guid=%s", result, akamaiGuid);
  FREE_MEM(result);
  result = tempResult;

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::GetMediaDataBox(CMediaDataBox **mediaDataBox, CAfhsSegmentFragment *segmentFragment)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, mediaDataBox);
  CHECK_POINTER_DEFAULT_HRESULT(result, segmentFragment);

  if (SUCCEEDED(result))
  {
    CBoxCollection *boxes = NULL;
    result = this->GetBoxes(&boxes, segmentFragment);
    CHECK_POINTER_HRESULT(result, boxes, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      // find media data box
      CMediaDataBox *tempMediaDataBox = dynamic_cast<CMediaDataBox *>(boxes->GetBox(MEDIA_DATA_BOX_TYPE, true));
      CHECK_POINTER_HRESULT(result, tempMediaDataBox, result, E_AFHS_AKAMAI_DECRYPTOR_NOT_FOUND_MEDIA_DATA_BOX);
      
      if (SUCCEEDED(result))
      {
        *mediaDataBox = new CMediaDataBox(&result);
        CHECK_POINTER_HRESULT(result, *mediaDataBox, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, (*mediaDataBox)->SetPayload(tempMediaDataBox->GetPayload(), (uint32_t)tempMediaDataBox->GetPayloadSize()), result, E_OUTOFMEMORY);
      }
    }

    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(*mediaDataBox));
    FREE_MEM_CLASS(boxes);
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::GetAkamaiFlvPacket(CAkamaiFlvPacket **akamaiFlvPacket, CLinearBuffer *buffer)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, akamaiFlvPacket);
  CHECK_POINTER_DEFAULT_HRESULT(result, buffer);

  if (SUCCEEDED(result))
  {
    *akamaiFlvPacket = new CAkamaiFlvPacket(&result);
    CHECK_POINTER_HRESULT(result, *akamaiFlvPacket, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, (*akamaiFlvPacket)->ParsePacket(buffer) == FLV_PARSE_RESULT_OK, result, E_AFHS_AKAMAI_DECRYPTOR_NOT_FLV_PACKET);
    CHECK_CONDITION_HRESULT(result, (*akamaiFlvPacket)->IsAkamaiFlvPacket(), result, E_AFHS_AKAMAI_DECRYPTOR_NOT_AKAMAI_FLV_PACKET);

    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(*akamaiFlvPacket));
  }
  
  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::GetBoxes(CBoxCollection **boxes, CAfhsSegmentFragment *segmentFragment)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, boxes);
  CHECK_POINTER_DEFAULT_HRESULT(result, segmentFragment);
  CHECK_CONDITION_HRESULT(result, segmentFragment->GetBuffer() != NULL, result, E_INVALIDARG);
  CHECK_CONDITION_HRESULT(result, segmentFragment->GetBuffer()->GetBufferOccupiedSpace() != 0, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    *boxes = new CBoxCollection(&result);
    CHECK_POINTER_HRESULT(result, *boxes, result, E_OUTOFMEMORY);

    CBoxFactory *factory = new CBoxFactory(&result);
    CHECK_POINTER_HRESULT(result, factory, result, E_OUTOFMEMORY);

    // for box factory we need buffer, position and length
    uint32_t position = 0;
    uint32_t length = segmentFragment->GetBuffer()->GetBufferOccupiedSpace();

    ALLOC_MEM_DEFINE_SET(buffer, uint8_t, length, 0);
    CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      segmentFragment->GetBuffer()->CopyFromBuffer(buffer, length);

      while (SUCCEEDED(result) && (position < length))
      {
        CBox *box = factory->CreateBox(buffer + position, length - position);
        CHECK_POINTER_HRESULT(result, box, result, E_AFHS_AKAMAI_DECRYPTOR_NOT_CREATED_BOX);

        CHECK_CONDITION_HRESULT(result, (*boxes)->Add(box), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), position += (uint32_t)box->GetSize());

        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(box));
      }
    }

    FREE_MEM(buffer);
    FREE_MEM_CLASS(factory);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(*boxes));
  }

  return result;
}
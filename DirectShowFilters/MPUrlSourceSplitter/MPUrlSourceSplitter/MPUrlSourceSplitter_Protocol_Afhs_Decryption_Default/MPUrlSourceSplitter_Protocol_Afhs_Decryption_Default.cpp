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

#include "MPUrlSourceSplitter_Protocol_Afhs_Decryption_Default.h"
#include "MPUrlSourceSplitter_Protocol_Afhs_Decryption_Default_Parameters.h"
#include "AfhsDecryptionPluginConfiguration.h"
#include "VersionInfo.h"

// decryption implementation name
#ifdef _DEBUG
#define DECRYPTION_IMPLEMENTATION_NAME                                        L"MPUrlSourceSplitter_Protocol_Afhs_Decryption_Defaultd"
#else
#define DECRYPTION_IMPLEMENTATION_NAME                                        L"MPUrlSourceSplitter_Protocol_Afhs_Decryption_Default"
#endif

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default *protocol = (CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default *)plugin;

    delete protocol;
  }
}

CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default::CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CAfhsDecryptionPlugin(result, logger, configuration)
{
  //this->lockCurlMutex = NULL;
  //this->lockMutex = NULL;
  //this->mainCurlInstance = NULL;
  //this->streamLength = 0;
  //this->connectionState = None;
  //this->segmentFragments = NULL;
  //this->cacheFile = NULL;
  //this->lastStoreTime = 0;
  //this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
  //this->segmentFragmentDownloading = UINT_MAX;
  //this->segmentFragmentProcessing = UINT_MAX;
  //this->segmentFragmentToDownload = UINT_MAX;
  //this->segmentFragmentDecrypting = UINT_MAX;
  //this->decryptionHoster = NULL;
  //this->manifest = NULL;
  //this->headerAndMetaPacketSize = 0;
  //this->segmentFragmentZeroTimestamp = 0;
  
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, DECRYPTION_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

  //  this->lockMutex = CreateMutex(NULL, FALSE, NULL);
  //  this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
  //  this->cacheFile = new CCacheFile(result);
  //  this->segmentFragments = new CAfhsSegmentFragmentCollection(result);
  //  this->decryptionHoster = new CAfhsDecryptionHoster(result, logger, configuration);
  //  this->manifest = new CF4MManifest(result);

  //  CHECK_POINTER_HRESULT(*result, this->lockMutex, *result, E_OUTOFMEMORY);
  //  CHECK_POINTER_HRESULT(*result, this->lockCurlMutex, *result, E_OUTOFMEMORY);
  //  CHECK_POINTER_HRESULT(*result, this->cacheFile, *result, E_OUTOFMEMORY);
  //  CHECK_POINTER_HRESULT(*result, this->segmentFragments, *result, E_OUTOFMEMORY);
  //  CHECK_POINTER_HRESULT(*result, this->decryptionHoster, *result, E_OUTOFMEMORY);
  //  CHECK_POINTER_HRESULT(*result, this->manifest, *result, E_OUTOFMEMORY);

  //  //CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(*result), this->decryptionHoster->LoadPlugins(), *result);

  //  // create CURL instance
  //  this->mainCurlInstance = new CAfhsCurlInstance(result, this->logger, this->lockCurlMutex, DECRYPTION_IMPLEMENTATION_NAME, L"Main");
  //  CHECK_POINTER_HRESULT(*result, this->mainCurlInstance, *result, E_OUTOFMEMORY);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_DEFAULT, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_DEFAULT);
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

CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default::~CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, DECRYPTION_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  //// because segments and fragments can be changed in decryption hoster, collection of segments and fragments have to be released before decryption hoster
  //// in other case it can lead to access violation exception, due to virtual function table is allocated in memory space of decryption hoster
  //FREE_MEM_CLASS(this->segmentFragments);

  //FREE_MEM_CLASS(this->decryptionHoster);
  //FREE_MEM_CLASS(this->mainCurlInstance);
  //FREE_MEM_CLASS(this->cacheFile);
  //FREE_MEM_CLASS(this->manifest);

  //if (this->lockMutex != NULL)
  //{
  //  CloseHandle(this->lockMutex);
  //  this->lockMutex = NULL;
  //}

  //if (this->lockCurlMutex != NULL)
  //{
  //  CloseHandle(this->lockCurlMutex);
  //  this->lockCurlMutex = NULL;
  //}

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, DECRYPTION_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));
}

// CPlugin implementation

const wchar_t *CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default::GetName(void)
{
  return AFHS_PROTOCOL_DECRYPTION_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default::Initialize(CPluginConfiguration *configuration)
{
  CAfhsDecryptionPluginConfiguration *decryptionConfiguration = (CAfhsDecryptionPluginConfiguration *)configuration;
  //HRESULT result = ((this->lockMutex != NULL) && (this->configuration != NULL) && (this->logger != NULL)) ? S_OK : E_NOT_VALID_STATE;

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

HRESULT CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default::GetDecryptionResult(void)
{
  return DECRYPTION_RESULT_KNOWN;
}

unsigned int CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default::GetDecryptionScore(void)
{
  // return lowest possible score
  return 1;
}
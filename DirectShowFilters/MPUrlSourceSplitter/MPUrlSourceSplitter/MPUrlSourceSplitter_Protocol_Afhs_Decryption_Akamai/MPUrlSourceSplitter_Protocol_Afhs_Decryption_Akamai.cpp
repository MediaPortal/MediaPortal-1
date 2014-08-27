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
#include "AfhsDecryptionPluginConfiguration.h"
#include "Parameters.h"
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
  this->initializeAkamaiFlashInstanceResult = E_NOT_VALID_STATE;
  this->akamaiFlashInstance = NULL;
  this->lastKeyUrl = NULL;
  this->lastKey = NULL;
  this->lastKeyLength = 0;
  this->akamaiGuid = NULL;
  this->akamaiSwfFile = NULL;
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

  if (this->akamaiSwfFile != NULL)
  {
    DeleteFile(this->akamaiSwfFile);
  }

  FREE_MEM(this->lastKeyUrl);
  FREE_MEM(this->lastKey);
  FREE_MEM(this->akamaiGuid);
  FREE_MEM(this->akamaiSwfFile);
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
  return DECRYPTION_RESULT_NOT_KNOWN;
}

unsigned int CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::GetDecryptionScore(void)
{
  // return lowest possible score
  return 1;
}


void CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::ClearSession(void)
{
  __super::ClearSession();

  FREE_MEM_CLASS(this->akamaiFlashInstance);

  if (this->akamaiSwfFile != NULL)
  {
    DeleteFile(this->akamaiSwfFile);
  }

  this->initializeAkamaiFlashInstanceResult = E_NOT_VALID_STATE;
  FREE_MEM(this->lastKeyUrl);
  FREE_MEM(this->lastKey);
  this->lastKeyLength = 0;
  FREE_MEM(this->akamaiGuid);
  FREE_MEM(this->akamaiSwfFile);
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

wchar_t *CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Akamai::GetAkamaiSwfFile(void)
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
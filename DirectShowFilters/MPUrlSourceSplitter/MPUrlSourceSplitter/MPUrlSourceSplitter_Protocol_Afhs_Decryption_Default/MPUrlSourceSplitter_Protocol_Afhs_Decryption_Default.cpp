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
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, DECRYPTION_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

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

HRESULT CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default::GetDecryptionResult(CAfhsDecryptionContext *decryptionContext)
{
  return DECRYPTION_RESULT_KNOWN;
}

unsigned int CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default::GetDecryptionScore(void)
{
  // return lowest possible score
  return 1;
}

HRESULT CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default::DecryptSegmentFragments(CAfhsDecryptionContext *decryptionContext)
{
  HRESULT result = S_OK;
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

  return result;
}
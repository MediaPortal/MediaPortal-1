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

#include "MPUrlSourceSplitter_Protocol_M3u8_Decryption_None.h"
#include "MPUrlSourceSplitter_Protocol_M3u8_Decryption_None_Parameters.h"
#include "M3u8DecryptionPluginConfiguration.h"
#include "VersionInfo.h"
#include "ErrorCodes.h"
#include "Parameters.h"

// decryption implementation name
#ifdef _DEBUG
#define DECRYPTION_IMPLEMENTATION_NAME                                        L"MPUrlSourceSplitter_Protocol_M3u8_Decryption_Noned"
#else
#define DECRYPTION_IMPLEMENTATION_NAME                                        L"MPUrlSourceSplitter_Protocol_M3u8_Decryption_None"
#endif

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_M3u8_Decryption_None(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Protocol_M3u8_Decryption_None *protocol = (CMPUrlSourceSplitter_Protocol_M3u8_Decryption_None *)plugin;

    delete protocol;
  }
}

CMPUrlSourceSplitter_Protocol_M3u8_Decryption_None::CMPUrlSourceSplitter_Protocol_M3u8_Decryption_None(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CM3u8DecryptionPlugin(result, logger, configuration)
{
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, DECRYPTION_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_DECRYPTION_NONE, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_DECRYPTION_NONE);
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

CMPUrlSourceSplitter_Protocol_M3u8_Decryption_None::~CMPUrlSourceSplitter_Protocol_M3u8_Decryption_None()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, DECRYPTION_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, DECRYPTION_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));
}

// CPlugin implementation

const wchar_t *CMPUrlSourceSplitter_Protocol_M3u8_Decryption_None::GetName(void)
{
  return M3U8_PROTOCOL_DECRYPTION_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_M3u8_Decryption_None::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_M3u8_Decryption_None::Initialize(CPluginConfiguration *configuration)
{
  CM3u8DecryptionPluginConfiguration *decryptionConfiguration = (CM3u8DecryptionPluginConfiguration *)configuration;

  HRESULT result = S_OK;
  CHECK_POINTER_HRESULT(result, decryptionConfiguration, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    this->configuration->Clear();

    CHECK_CONDITION_HRESULT(result, this->configuration->Append(decryptionConfiguration->GetConfiguration()), result, E_OUTOFMEMORY);

    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_SPLITTER, true, PARAMETER_NAME_SPLITTER_DEFAULT) ? PLUGIN_FLAG_SPLITTER : M3U8_DECRYPTION_PLUGIN_FLAG_NONE;
    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_IPTV, true, PARAMETER_NAME_IPTV_DEFAULT) ? PLUGIN_FLAG_IPTV : M3U8_DECRYPTION_PLUGIN_FLAG_NONE;

    this->configuration->LogCollection(this->logger, LOGGER_VERBOSE, DECRYPTION_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);
  }

  return result;
}

// CM3u8DecryptionPlugin implementation

HRESULT CMPUrlSourceSplitter_Protocol_M3u8_Decryption_None::DecryptStreamFragments(CM3u8DecryptionContext *decryptionContext)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, decryptionContext);
  CHECK_POINTER_HRESULT(result, decryptionContext->GetStreamFragments(), result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    CIndexedM3u8StreamFragmentCollection *indexedEncryptedStreamFragments = new CIndexedM3u8StreamFragmentCollection(&result);
    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = decryptionContext->GetStreamFragments()->GetEncryptedStreamFragments(indexedEncryptedStreamFragments));

    bool decrypted = false;
    for (unsigned int i = 0; (SUCCEEDED(result) && (i < indexedEncryptedStreamFragments->Count())); i++)
    {
      CIndexedM3u8StreamFragment *indexedEncryptedStreamFragment = indexedEncryptedStreamFragments->GetItem(i);
      CM3u8StreamFragment *currentEncryptedFragment = indexedEncryptedStreamFragment->GetItem();

      if (currentEncryptedFragment->GetFragmentEncryption()->IsEncryptionNone())
      {
        currentEncryptedFragment->SetEncrypted(false, UINT_MAX);
        currentEncryptedFragment->SetDecrypted(true, UINT_MAX);

        decryptionContext->GetStreamFragments()->UpdateIndexes(indexedEncryptedStreamFragment->GetItemIndex(), 1);
        decrypted = true;
      }
      else if (decrypted)
      {
        // at least one stream fragment is decrypted, return successfully
        break;
      }
      else
      {
        // not supported decryption method for stream fragment
        result = E_M3U8_DECRYPTION_METHOD_NOT_SUPPORTED;
      }
    }

    FREE_MEM_CLASS(indexedEncryptedStreamFragments);
  }

  return result;
}
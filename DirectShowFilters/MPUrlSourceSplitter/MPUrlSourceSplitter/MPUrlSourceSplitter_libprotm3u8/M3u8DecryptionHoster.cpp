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

#pragma warning(push)
// disable warning: 'INT8_MIN' : macro redefinition
// warning is caused by stdint.h and intsafe.h, which both define same macro
#pragma warning(disable:4005)

#include "M3u8DecryptionHoster.h"
#include "ErrorCodes.h"
#include "M3u8DecryptionPluginConfiguration.h"
#include "M3u8DecryptionHosterPluginMetadata.h"
#include "Parameters.h"

#pragma warning(pop)

#define METHOD_DECRYPT_STREAM_FRAGMENTS_NAME                          L"DecryptStreamFragments()"

CM3u8DecryptionHoster::CM3u8DecryptionHoster(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CHoster(result, logger, configuration, L"M3u8DecryptionHoster", L"mpurlsourcesplitter_protocol_m3u8_decryption_*.dll")
{
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, MODULE_M3U8_DECRYPTION_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME, this);


    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_M3U8_DECRYPTION_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CM3u8DecryptionHoster::~CM3u8DecryptionHoster(void)
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_M3U8_DECRYPTION_HOSTER_NAME, METHOD_DESTRUCTOR_NAME));

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_M3U8_DECRYPTION_HOSTER_NAME, METHOD_DESTRUCTOR_NAME));
}

/* get methods */

/* set methods */

/* other methods */

void CM3u8DecryptionHoster::ClearSession(void)
{
  __super::ClearSession();
}

HRESULT CM3u8DecryptionHoster::LoadPlugins(void)
{
  HRESULT result = __super::LoadPlugins();
  CHECK_CONDITION_HRESULT(result, this->hosterPluginMetadataCollection->Count() != 0, result, E_M3U8_NO_DECRYPTOR_LOADED);

  CHECK_CONDITION_EXECUTE(result == E_M3U8_NO_DECRYPTOR_LOADED, this->logger->Log(LOGGER_ERROR, L"%s: %s: no M3U8 decryption plugin loaded", this->hosterName, METHOD_LOAD_PLUGINS_NAME));
  return result;
}

HRESULT CM3u8DecryptionHoster::DecryptStreamFragments(CM3u8DecryptionContext *decryptionContext)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, decryptionContext);

  if (SUCCEEDED(result))
  {
    bool decrypted = false;

    for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->hosterPluginMetadataCollection->Count())); i++)
    {
      CM3u8DecryptionHosterPluginMetadata *metadata = (CM3u8DecryptionHosterPluginMetadata *)this->hosterPluginMetadataCollection->GetItem(i);
      CM3u8DecryptionPlugin *decryptor = dynamic_cast<CM3u8DecryptionPlugin *>(metadata->GetPlugin());

      HRESULT res = decryptor->DecryptStreamFragments(decryptionContext);

      if (res == S_OK)
      {
        // some or none stream fragments decrypted
        decrypted = true;
        break;
      }
      else if (res == E_M3U8_DECRYPTION_METHOD_NOT_SUPPORTED)
      {
        // decryptor doesn't support decryption method for stream fragment, try another decryptor
      }
      else
      {
        // another error code
        result = res;
      }
    }

    CHECK_CONDITION_HRESULT(result, decrypted, result, E_M3U8_DECRYPTION_METHOD_NOT_SUPPORTED);
  }

  return result;
}

/* protected methods */

CHosterPluginMetadata *CM3u8DecryptionHoster::CreateHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName)
{
  CM3u8DecryptionHosterPluginMetadata *decryptorMetadata = NULL;

  if ((result != NULL) && SUCCEEDED(*result))
  {
    decryptorMetadata = new CM3u8DecryptionHosterPluginMetadata(result, logger, configuration, hosterName, pluginLibraryFileName);
    CHECK_POINTER_HRESULT(*result, decryptorMetadata, *result, E_OUTOFMEMORY);

    CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(decryptorMetadata));
  }

  return decryptorMetadata;
}

CPluginConfiguration *CM3u8DecryptionHoster::CreatePluginConfiguration(HRESULT *result, CParameterCollection *configuration)
{
  CM3u8DecryptionPluginConfiguration *decryptionConfiguration = NULL;

  if ((result != NULL) && SUCCEEDED(*result))
  {
    decryptionConfiguration = new CM3u8DecryptionPluginConfiguration(result, configuration);
    CHECK_POINTER_HRESULT(*result, decryptionConfiguration, *result, E_OUTOFMEMORY);

    CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(decryptionConfiguration));
  }

  return decryptionConfiguration;
}
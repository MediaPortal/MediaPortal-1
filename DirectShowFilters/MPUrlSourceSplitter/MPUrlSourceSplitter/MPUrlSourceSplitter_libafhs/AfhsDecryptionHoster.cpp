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

#include "AfhsDecryptionHoster.h"
#include "ErrorCodes.h"
#include "AfhsDecryptionPluginConfiguration.h"
#include "AfhsDecryptionHosterPluginMetadata.h"

CAfhsDecryptionHoster::CAfhsDecryptionHoster(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CHoster(result, logger, configuration, L"ParserHoster", L"mpurlsourcesplitter_afhs_decryption_*.dll")
{
  this->activeDecryptor = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, MODULE_AFHS_DECRYPTION_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME, this);
    

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_AFHS_DECRYPTION_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CAfhsDecryptionHoster::~CAfhsDecryptionHoster(void)
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_AFHS_DECRYPTION_HOSTER_NAME, METHOD_DESTRUCTOR_NAME));

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_AFHS_DECRYPTION_HOSTER_NAME, METHOD_DESTRUCTOR_NAME));
}

/* get methods */

/* set methods */

/* other methods */

HRESULT CAfhsDecryptionHoster::ClearSession(void)
{
  for (unsigned int i = 0; i < this->hosterPluginMetadataCollection->Count(); i++)
  {
    CAfhsDecryptionHosterPluginMetadata *metadata = (CAfhsDecryptionHosterPluginMetadata *)this->hosterPluginMetadataCollection->GetItem(i);
    CAfhsDecryptionPlugin *decryptor = (CAfhsDecryptionPlugin *)metadata->GetPlugin();

    this->logger->Log(LOGGER_INFO, L"%s: %s: reseting decryption plugin: %s", this->hosterName, METHOD_CLEAR_SESSION_NAME, decryptor->GetName());

    metadata->ClearSession();
    decryptor->ClearSession();
  }

  return S_OK;
}

HRESULT CAfhsDecryptionHoster::LoadPlugins(void)
{
  HRESULT result = __super::LoadPlugins();
  CHECK_CONDITION_HRESULT(result, this->hosterPluginMetadataCollection->Count() != 0, result, E_AFHS_NO_DECRYPTOR_LOADED);

  CHECK_CONDITION_EXECUTE(result == E_AFHS_NO_DECRYPTOR_LOADED, this->logger->Log(LOGGER_ERROR, L"%s: %s: no AFHS decryption plugin loaded", this->hosterName, METHOD_LOAD_PLUGINS_NAME));
  return result;
}

/* protected methods */

CHosterPluginMetadata *CAfhsDecryptionHoster::CreateHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName)
{
  CAfhsDecryptionHosterPluginMetadata *decryptorMetadata = NULL;

  if ((result != NULL) && SUCCEEDED(*result))
  {
    decryptorMetadata = new CAfhsDecryptionHosterPluginMetadata(result, logger, configuration, hosterName, pluginLibraryFileName);
    CHECK_POINTER_HRESULT(*result, decryptorMetadata, *result, E_OUTOFMEMORY);
  
    CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(decryptorMetadata));
  }

  return decryptorMetadata;
}

CPluginConfiguration *CAfhsDecryptionHoster::CreatePluginConfiguration(HRESULT *result, CParameterCollection *configuration)
{
  CAfhsDecryptionPluginConfiguration *decryptionConfiguration = NULL;

  if ((result != NULL) && SUCCEEDED(*result))
  {
    decryptionConfiguration = new CAfhsDecryptionPluginConfiguration(result, configuration);
    CHECK_POINTER_HRESULT(*result, decryptionConfiguration, *result, E_OUTOFMEMORY);
  
    CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(decryptionConfiguration));
  }

  return decryptionConfiguration;
}
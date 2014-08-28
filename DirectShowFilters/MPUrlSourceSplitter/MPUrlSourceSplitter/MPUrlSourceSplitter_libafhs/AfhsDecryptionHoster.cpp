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

#include "AfhsDecryptionHoster.h"
#include "ErrorCodes.h"
#include "AfhsDecryptionPluginConfiguration.h"
#include "AfhsDecryptionHosterPluginMetadata.h"
#include "Parameters.h"

#pragma warning(pop)

#define METHOD_DECRYPT_SEGMENT_FRAGMENTS_NAME                         L"DecryptSegmentFragments()"

CAfhsDecryptionHoster::CAfhsDecryptionHoster(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CHoster(result, logger, configuration, L"AfhsDecryptionHoster", L"mpurlsourcesplitter_protocol_afhs_decryption_*.dll")
{
  this->activeDecryptor = NULL;
  this->flags |= AFHS_DECRYPTION_HOSTER_FLAG_PENDING_DECRYPTOR;

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

void CAfhsDecryptionHoster::ClearSession(void)
{
  __super::ClearSession();

  this->flags = AFHS_DECRYPTION_HOSTER_FLAG_PENDING_DECRYPTOR;
}

HRESULT CAfhsDecryptionHoster::LoadPlugins(void)
{
  HRESULT result = __super::LoadPlugins();
  CHECK_CONDITION_HRESULT(result, this->hosterPluginMetadataCollection->Count() != 0, result, E_AFHS_NO_DECRYPTOR_LOADED);

  CHECK_CONDITION_EXECUTE(result == E_AFHS_NO_DECRYPTOR_LOADED, this->logger->Log(LOGGER_ERROR, L"%s: %s: no AFHS decryption plugin loaded", this->hosterName, METHOD_LOAD_PLUGINS_NAME));
  return result;
}

HRESULT CAfhsDecryptionHoster::DecryptSegmentFragments(CAfhsDecryptionContext *decryptionContext)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, decryptionContext);

  if (SUCCEEDED(result))
  {
    if (this->IsSetFlags(AFHS_DECRYPTION_HOSTER_FLAG_PENDING_DECRYPTOR))
    {
      bool pendingDecryptor = true;
      unsigned int endTicks = decryptionContext->GetConfiguration()->GetValueUnsignedInt(PARAMETER_NAME_FINISH_TIME, true, UINT_MAX);

      while (SUCCEEDED(result) && pendingDecryptor && (GetTickCount() < endTicks))
      {
        // check if there is any pending parser

        pendingDecryptor = false;
        for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->hosterPluginMetadataCollection->Count())); i++)
        {
          CAfhsDecryptionHosterPluginMetadata *metadata = (CAfhsDecryptionHosterPluginMetadata *)this->hosterPluginMetadataCollection->GetItem(i);

          if (metadata->IsDecryptorStillPending())
          {
            HRESULT decryptionResult = metadata->GetDecryptionResult(decryptionContext);

            switch(decryptionResult)
            {
            case DECRYPTION_RESULT_PENDING:
              pendingDecryptor = true;
              break;
            case DECRYPTION_RESULT_NOT_KNOWN:
              this->logger->Log(LOGGER_INFO, L"%s: %s: decryptor '%s' doesn't recognize stream", MODULE_AFHS_DECRYPTION_HOSTER_NAME, METHOD_DECRYPT_SEGMENT_FRAGMENTS_NAME, metadata->GetPlugin()->GetName());
              break;
            case DECRYPTION_RESULT_KNOWN:
              this->logger->Log(LOGGER_INFO, L"%s: %s: decryptor '%s' recognizes stream, score: %u", MODULE_AFHS_DECRYPTION_HOSTER_NAME, METHOD_DECRYPT_SEGMENT_FRAGMENTS_NAME, metadata->GetPlugin()->GetName(), metadata->GetDecryptionScore());
              break;
            default:
              this->logger->Log(LOGGER_WARNING, L"%s: %s: decryptor '%s' returns error: 0x%08X", MODULE_AFHS_DECRYPTION_HOSTER_NAME, METHOD_DECRYPT_SEGMENT_FRAGMENTS_NAME, metadata->GetPlugin()->GetName(), decryptionResult);
              result = decryptionResult;
              break;
            }
          }
        }

        // sleep some time, get other threads chance to work
        if (SUCCEEDED(result) && pendingDecryptor)
        {
          Sleep(1);
        }
      }

      //if (SUCCEEDED(result) && pendingDecryptor)
      //{
      //  // timeout reached, some decryptor(s) is (are) still pending
      //  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->hosterPluginMetadataCollection->Count())); i++)
      //  {
      //    CAfhsDecryptionHosterPluginMetadata *metadata = (CAfhsDecryptionHosterPluginMetadata *)this->hosterPluginMetadataCollection->GetItem(i);

      //    if (metadata->IsDecryptorStillPending())
      //    {
      //      this->logger->Log(LOGGER_ERROR, L"%s: %s: decryptor '%s' still pending", MODULE_AFHS_DECRYPTION_HOSTER_NAME, METHOD_DECRYPT_SEGMENT_FRAGMENTS_NAME, metadata->GetPlugin()->GetName());
      //    }
      //  }

      //  result = E_DECRYPTOR_STILL_PENDING;
      //}

      this->flags &= ~AFHS_DECRYPTION_HOSTER_FLAG_PENDING_DECRYPTOR;
      this->flags |= pendingDecryptor ? AFHS_DECRYPTION_HOSTER_FLAG_PENDING_DECRYPTOR : AFHS_DECRYPTION_HOSTER_FLAG_NONE;

      if (SUCCEEDED(result) && (!this->IsSetFlags(AFHS_DECRYPTION_HOSTER_FLAG_PENDING_DECRYPTOR)))
      {
        // we don't have timeout, also no pending decryptor
        // find decryptor with highest score

        unsigned int highestScore = 0;
        for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->hosterPluginMetadataCollection->Count())); i++)
        {
          CAfhsDecryptionHosterPluginMetadata *metadata = (CAfhsDecryptionHosterPluginMetadata *)this->hosterPluginMetadataCollection->GetItem(i);

          if ((metadata->GetDecryptionResult(decryptionContext) == DECRYPTION_RESULT_KNOWN) && (metadata->GetDecryptionScore() > highestScore))
          {
            highestScore = metadata->GetDecryptionScore();
            this->activeDecryptor = dynamic_cast<CAfhsDecryptionPlugin *>(metadata->GetPlugin());
          }
        }

        CHECK_POINTER_HRESULT(result, this->activeDecryptor, result, E_AFHS_NO_ACTIVE_DECRYPTOR);
        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->logger->Log(LOGGER_INFO, L"%s: %s: active decryptor: '%s'", MODULE_AFHS_DECRYPTION_HOSTER_NAME, METHOD_DECRYPT_SEGMENT_FRAGMENTS_NAME, this->activeDecryptor->GetName()));
      }
    }
    
    if (SUCCEEDED(result) && (!this->IsSetFlags(AFHS_DECRYPTION_HOSTER_FLAG_PENDING_DECRYPTOR)))
    {
      result = this->activeDecryptor->DecryptSegmentFragments(decryptionContext);
    }
  }

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
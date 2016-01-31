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

#include "MPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128.h"
#include "MPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128_Parameters.h"
#include "MPUrlSourceSplitter_Protocol_M3u8_Parameters.h"
#include "M3u8DecryptionPluginConfiguration.h"
#include "VersionInfo.h"
#include "ErrorCodes.h"
#include "Parameters.h"
#include "BufferHelper.h"

#include <openssl/evp.h>
#include <openssl/err.h>

// decryption implementation name
#ifdef _DEBUG
#define DECRYPTION_IMPLEMENTATION_NAME                                        L"MPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128d"
#else
#define DECRYPTION_IMPLEMENTATION_NAME                                        L"MPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128"
#endif

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128 *protocol = (CMPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128 *)plugin;

    delete protocol;
  }
}

CMPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128::CMPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CM3u8DecryptionPlugin(result, logger, configuration)
{
  this->decryptionKey = NULL;
  this->decryptionKeyUri = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, DECRYPTION_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    this->decryptionKey = ALLOC_MEM_SET(this->decryptionKey, uint8_t, AES128_KEY_LENGTH, 0);
    CHECK_POINTER_HRESULT(*result, this->decryptionKey, *result, E_OUTOFMEMORY);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_DECRYPTION_AES128, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_DECRYPTION_AES128);
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

CMPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128::~CMPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, DECRYPTION_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  FREE_MEM(this->decryptionKey);
  FREE_MEM(this->decryptionKeyUri);

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, DECRYPTION_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));
}

// CPlugin implementation

const wchar_t *CMPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128::GetName(void)
{
  return M3U8_PROTOCOL_DECRYPTION_NAME;
}

void CMPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128::ClearSession(void)
{
  __super::ClearSession();

  FREE_MEM(this->decryptionKeyUri);
}

// CM3u8DecryptionPlugin implementation

HRESULT CMPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128::DecryptStreamFragments(CM3u8DecryptionContext *decryptionContext)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, decryptionContext);
  CHECK_POINTER_HRESULT(result, decryptionContext->GetStreamFragments(), result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    if (SUCCEEDED(result) && (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_DECRYPTION_AES128_FLAG_KEY_REQUEST_PENDING)))
    {
      if (decryptionContext->GetCurlInstance()->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA)
      {
        // all data received (key), check for error

        if (SUCCEEDED(decryptionContext->GetCurlInstance()->GetM3u8DownloadResponse()->GetResultError()))
        {
          CHECK_CONDITION_HRESULT(result, decryptionContext->GetCurlInstance()->GetM3u8DownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace() == AES128_KEY_LENGTH, result, E_M3U8_AES128_DECRYPTION_INVALID_KEY_LENGTH);

          if (SUCCEEDED(result))
          {
            // decryption key received

            decryptionContext->GetCurlInstance()->GetM3u8DownloadResponse()->GetReceivedData()->CopyFromBuffer(this->decryptionKey, AES128_KEY_LENGTH);
            SET_STRING_HRESULT(this->decryptionKeyUri, decryptionContext->GetCurlInstance()->GetHttpDownloadRequest()->GetUrl(), result);
          }
        }
        else
        {
          // error occured while receiving key
          // it's bad, we're done
          // unlock CURL instance, setting connection state to ProtocolConnectionState::None and return
          // filter will think that connection is not opened
          // filter stops trying to open connection after maximum connection reopen time
        }

        // we must unlock CURL instance, because we don't use it more
        decryptionContext->GetCurlInstance()->UnlockCurlInstance(this);
        decryptionContext->GetCurlInstance()->SetConnectionState(None);
        this->flags &= ~MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_DECRYPTION_AES128_FLAG_KEY_REQUEST_PENDING;

        if (FAILED(decryptionContext->GetCurlInstance()->GetM3u8DownloadResponse()->GetResultError()))
        {
          // failed to get decryption key
          // filter tries to reopen connection after some time
          return result;
        }
      }
    }

    if (SUCCEEDED(result) && (!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_DECRYPTION_AES128_FLAG_KEY_REQUEST_PENDING)))
    {
      CIndexedM3u8StreamFragmentCollection *indexedEncryptedStreamFragments = new CIndexedM3u8StreamFragmentCollection(&result);
      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = decryptionContext->GetStreamFragments()->GetEncryptedStreamFragments(indexedEncryptedStreamFragments));

      bool decrypted = false;
      for (unsigned int i = 0; (SUCCEEDED(result) && (i < indexedEncryptedStreamFragments->Count())); i++)
      {
        CIndexedM3u8StreamFragment *indexedEncryptedStreamFragment = indexedEncryptedStreamFragments->GetItem(i);
        CM3u8StreamFragment *currentEncryptedFragment = indexedEncryptedStreamFragment->GetItem();

        if (currentEncryptedFragment->GetFragmentEncryption()->IsEncryptionAes128())
        {
          // check if we have a key to decrypt fragment
          // if not, we have to first download decryption key

          if (CompareWithNull(this->decryptionKeyUri, currentEncryptedFragment->GetFragmentEncryption()->GetEncryptionKeyUri()) != 0)
          {
            // we have to download decryption key

            if (!decryptionContext->GetCurlInstance()->IsLockedCurlInstance())
            {
              CM3u8DownloadRequest *request = new CM3u8DownloadRequest(&result);
              CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                // set finish time, all methods must return before finish time
                unsigned int finishTime = UINT_MAX;
                if (SUCCEEDED(result))
                {
                  finishTime = decryptionContext->GetConfiguration()->GetValueUnsignedInt(PARAMETER_NAME_FINISH_TIME, true, UINT_MAX);
                  if (finishTime != UINT_MAX)
                  {
                    unsigned int currentTime = GetTickCount();
                    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: finish time specified, current time: %u, finish time: %u, diff: %u (ms)", DECRYPTION_IMPLEMENTATION_NAME, METHOD_DECRYPT_STREAM_FRAGMENTS_NAME, currentTime, finishTime, finishTime - currentTime);
                    decryptionContext->GetConfiguration()->Remove(PARAMETER_NAME_FINISH_TIME, true);
                  }
                }

                request->SetFinishTime(finishTime);
                request->SetReceivedDataTimeout(decryptionContext->GetConfiguration()->GetValueUnsignedInt(PARAMETER_NAME_HTTP_OPEN_CONNECTION_TIMEOUT, true, this->IsIptv() ? HTTP_OPEN_CONNECTION_TIMEOUT_DEFAULT_IPTV : HTTP_OPEN_CONNECTION_TIMEOUT_DEFAULT_SPLITTER));
                request->SetNetworkInterfaceName(decryptionContext->GetConfiguration()->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));

                CHECK_CONDITION_HRESULT(result, request->SetUrl(currentEncryptedFragment->GetFragmentEncryption()->GetEncryptionKeyUri()), result, E_OUTOFMEMORY);
                CHECK_CONDITION_HRESULT(result, request->SetCookie(decryptionContext->GetConfiguration()->GetValue(PARAMETER_NAME_HTTP_COOKIE, true, NULL)), result, E_OUTOFMEMORY);
                request->SetHttpVersion(decryptionContext->GetConfiguration()->GetValueLong(PARAMETER_NAME_HTTP_VERSION, true, HTTP_VERSION_DEFAULT));
                request->SetIgnoreContentLength((decryptionContext->GetConfiguration()->GetValueLong(PARAMETER_NAME_HTTP_IGNORE_CONTENT_LENGTH, true, HTTP_IGNORE_CONTENT_LENGTH_DEFAULT) == 1L));
                CHECK_CONDITION_HRESULT(result, request->SetReferer(decryptionContext->GetConfiguration()->GetValue(PARAMETER_NAME_HTTP_REFERER, true, NULL)), result, E_OUTOFMEMORY);
                CHECK_CONDITION_HRESULT(result, request->SetUserAgent(decryptionContext->GetConfiguration()->GetValue(PARAMETER_NAME_HTTP_USER_AGENT, true, NULL)), result, E_OUTOFMEMORY);

                if (decryptionContext->GetConfiguration()->GetValueBool(PARAMETER_NAME_HTTP_SERVER_AUTHENTICATE, true, HTTP_SERVER_AUTHENTICATE_DEFAULT))
                {
                  const wchar_t *serverUserName = decryptionContext->GetConfiguration()->GetValue(PARAMETER_NAME_HTTP_SERVER_USER_NAME, true, NULL);
                  const wchar_t *serverPassword = decryptionContext->GetConfiguration()->GetValue(PARAMETER_NAME_HTTP_SERVER_PASSWORD, true, NULL);

                  CHECK_POINTER_HRESULT(result, serverUserName, result, E_AUTH_NO_SERVER_USER_NAME);
                  CHECK_POINTER_HRESULT(result, serverUserName, result, E_AUTH_NO_SERVER_PASSWORD);

                  CHECK_CONDITION_HRESULT(result, request->SetAuthentication(true, serverUserName, serverPassword), result, E_OUTOFMEMORY);
                }

                if (decryptionContext->GetConfiguration()->GetValueBool(PARAMETER_NAME_HTTP_PROXY_SERVER_AUTHENTICATE, true, HTTP_PROXY_SERVER_AUTHENTICATE_DEFAULT))
                {
                  const wchar_t *proxyServer = decryptionContext->GetConfiguration()->GetValue(PARAMETER_NAME_HTTP_PROXY_SERVER, true, NULL);
                  const wchar_t *proxyServerUserName = decryptionContext->GetConfiguration()->GetValue(PARAMETER_NAME_HTTP_PROXY_SERVER_USER_NAME, true, NULL);
                  const wchar_t *proxyServerPassword = decryptionContext->GetConfiguration()->GetValue(PARAMETER_NAME_HTTP_PROXY_SERVER_PASSWORD, true, NULL);
                  unsigned short proxyServerPort = (unsigned short)decryptionContext->GetConfiguration()->GetValueUnsignedInt(PARAMETER_NAME_HTTP_PROXY_SERVER_PORT, true, HTTP_PROXY_SERVER_PORT_DEFAULT);
                  unsigned int proxyServerType = decryptionContext->GetConfiguration()->GetValueUnsignedInt(PARAMETER_NAME_HTTP_PROXY_SERVER_TYPE, true, HTTP_PROXY_SERVER_TYPE_DEFAULT);

                  CHECK_POINTER_HRESULT(result, proxyServer, result, E_AUTH_NO_PROXY_SERVER);
                  CHECK_POINTER_HRESULT(result, proxyServerUserName, result, E_AUTH_NO_SERVER_USER_NAME);
                  CHECK_POINTER_HRESULT(result, proxyServerPassword, result, E_AUTH_NO_SERVER_PASSWORD);

                  CHECK_CONDITION_HRESULT(result, request->SetProxyAuthentication(true, proxyServer, proxyServerPort, proxyServerType, proxyServerUserName, proxyServerPassword), result, E_OUTOFMEMORY);
                }

                if (SUCCEEDED(result))
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
                        this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_DECRYPTION_AES128_FLAG_KEY_REQUEST_PENDING;
                      }
                      else
                      {
                        decryptionContext->GetCurlInstance()->SetConnectionState(OpeningFailed);

                        // we must unlock CURL instance, because we don't use it more
                        decryptionContext->GetCurlInstance()->UnlockCurlInstance(this);
                      }
                    }
                    else
                    {
                      decryptionContext->GetCurlInstance()->SetConnectionState(InitializeFailed);

                      // we must unlock CURL instance, because we don't use it more
                      decryptionContext->GetCurlInstance()->UnlockCurlInstance(this);
                    }
                  }
                  else
                  {
                    decryptionContext->GetCurlInstance()->SetConnectionState(InitializeFailed);
                    this->logger->Log(LOGGER_WARNING, L"%s: %s: cannot lock CURL instance, owner: 0x%p, lock count: %u", DECRYPTION_IMPLEMENTATION_NAME, METHOD_DECRYPT_STREAM_FRAGMENTS_NAME, decryptionContext->GetCurlInstance()->GetOwner(), decryptionContext->GetCurlInstance()->GetOwnerLockCount());
                  }
                }
              }

              FREE_MEM_CLASS(request);
            }

            // we can't continue with decrypting, we need new decryption key
            break;
          }

          // the initialization vector is specified in fragment
          // it can be directly specified or it is sequence number

          ALLOC_MEM_DEFINE_SET(initializationVector, uint8_t, INITIALIZATION_VECTOR_LENGTH, 0);
          CHECK_POINTER_HRESULT(result, initializationVector, result, E_OUTOFMEMORY);

          ALLOC_MEM_DEFINE_SET(aesDecryptionContext, EVP_CIPHER_CTX, 1, 0);
          CHECK_POINTER_HRESULT(result, aesDecryptionContext, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            if (currentEncryptedFragment->GetFragmentEncryption()->GetEncryptionInitializationVector() != NULL)
            {
              // initialization vector specified, just copy it

              memcpy(initializationVector, currentEncryptedFragment->GetFragmentEncryption()->GetEncryptionInitializationVector(), INITIALIZATION_VECTOR_LENGTH);
            }
            else
            {
              // initialization vector is not specified
              // create initialization vector from sequence number

              WBE32(initializationVector, 12, currentEncryptedFragment->GetFragment());
            }

            if (SUCCEEDED(result))
            {
              EVP_CIPHER_CTX_init(aesDecryptionContext);
              CHECK_CONDITION_HRESULT(result, EVP_DecryptInit_ex(aesDecryptionContext, EVP_aes_128_cbc(), NULL, this->decryptionKey, initializationVector) == 1, result, E_M3U8_AES128_DECRYPTION_FAILED_TO_INITIALIZE);

              unsigned int encryptedDataLength = currentEncryptedFragment->GetBuffer()->GetBufferOccupiedSpace();

              ALLOC_MEM_DEFINE_SET(decryptedData, uint8_t, encryptedDataLength, 0);
              CHECK_POINTER_HRESULT(result, decryptedData, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                int firstDataLength = 0;
                int secondDataLength = 0;

                CHECK_CONDITION_HRESULT(result, EVP_DecryptUpdate(aesDecryptionContext, decryptedData, &firstDataLength, currentEncryptedFragment->GetBuffer()->GetInternalBuffer(), encryptedDataLength) == 1, result, E_M3U8_AES128_DECRYPTION_FAILED_TO_DECRYPT_DATA);
                CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: EVP_DecryptUpdate() error: 0x%08X", DECRYPTION_IMPLEMENTATION_NAME, METHOD_DECRYPT_STREAM_FRAGMENTS_NAME, ERR_get_error()));

                if (SUCCEEDED(result))
                {
                  CHECK_CONDITION_HRESULT(result, EVP_DecryptFinal_ex(aesDecryptionContext, decryptedData + firstDataLength, &secondDataLength) == 1, result, E_M3U8_AES128_DECRYPTION_FAILED_TO_FINALIZE_DECRYPTION);
                  CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: EVP_DecryptFinal_ex() error: 0x%08X", DECRYPTION_IMPLEMENTATION_NAME, METHOD_DECRYPT_STREAM_FRAGMENTS_NAME, ERR_get_error()));

                  if (SUCCEEDED(result))
                  {
                    CHECK_CONDITION_HRESULT(result, (firstDataLength + secondDataLength) > 0, result, E_M3U8_AES128_DECRYPTION_DECRYPTED_DATA_LOWER_THAN_ZERO);

                    // decrypted data length should be lower than or equal to encrypted data length
                    unsigned int decryptedDataLength = (unsigned int)(firstDataLength + secondDataLength);

                    CHECK_CONDITION_HRESULT(result, decryptedDataLength <= encryptedDataLength, result, E_M3U8_AES128_DECRYPTION_DECRYPTED_DATA_GREATER_THAN_ENCRYPTED_DATA);

                    if (SUCCEEDED(result))
                    {
                      currentEncryptedFragment->GetBuffer()->ClearBuffer();
                      CHECK_CONDITION_HRESULT(result, currentEncryptedFragment->GetBuffer()->AddToBuffer(decryptedData, decryptedDataLength) == decryptedDataLength, result, E_OUTOFMEMORY);

                      currentEncryptedFragment->SetEncrypted(false, UINT_MAX);
                      currentEncryptedFragment->SetDecrypted(true, UINT_MAX);

                      decryptionContext->GetStreamFragments()->UpdateIndexes(indexedEncryptedStreamFragment->GetItemIndex(), 1);
                      decrypted = true;
                    }
                  }
                }
              }

              // cleanup
              FREE_MEM(decryptedData);
              EVP_CIPHER_CTX_cleanup(aesDecryptionContext);
            }
          }

          // cleanup
          FREE_MEM(aesDecryptionContext);
          FREE_MEM(initializationVector);
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
  }

  return result;
}

/* protected methods */

const wchar_t *CMPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128::GetModuleName(void)
{
  return DECRYPTION_IMPLEMENTATION_NAME;
}
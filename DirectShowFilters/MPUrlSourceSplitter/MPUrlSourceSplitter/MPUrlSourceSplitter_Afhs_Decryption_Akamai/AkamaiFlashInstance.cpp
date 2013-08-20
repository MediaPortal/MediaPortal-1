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

#include "AkamaiFlashInstance.h"
#include "conversions.h"
#include "BufferHelper.h"

#include "TcpServer.h"
#include "TcpSocketContext.h"

#include <wchar.h>

#define METHOD_GET_DECRYPTED_DATA_NAME                                L"GetDecryptedData()"

CAkamaiFlashInstance::CAkamaiFlashInstance(CLogger *logger, const wchar_t *instanceName, const wchar_t *swfFilePath)
  : CFlashInstance(logger, instanceName, swfFilePath)
{
  this->error = NULL;
  this->decryptorErrorCode = UINT_MAX;
  this->decryptorError = NULL;
  this->server = new CTcpServer();
}

CAkamaiFlashInstance::~CAkamaiFlashInstance(void)
{
  FREE_MEM_CLASS(this->server);
  FREE_MEM(this->error);
  FREE_MEM(this->decryptorError);
}

/* get methods */

AkamaiDecryptorState CAkamaiFlashInstance::GetState(void)
{
  wchar_t *queryResult = this->GetResult(L"<invoke name=\"GetState\" returntype=\"xml\"></invoke>");
  AkamaiDecryptorState result = AkamaiDecryptorState_Undefined;

  if (queryResult != NULL)
  {
    if (wcscmp(queryResult, L"NotInitialized") == 0)
    {
      result = AkamaiDecryptorState_NotInitialized;
    }
    else if (wcscmp(queryResult, L"Pending") == 0)
    {
      result = AkamaiDecryptorState_Pending;
    }
    else if (wcscmp(queryResult, L"Ready") == 0)
    {
      result = AkamaiDecryptorState_Ready;
    }
    else if (wcscmp(queryResult, L"Error") == 0)
    {
      result = AkamaiDecryptorState_Error;

      // we are in error state, get error text
      FREE_MEM(this->error);
      this->error = Duplicate(this->GetResult(L"<invoke name=\"GetError\" returntype=\"xml\"></invoke>"));
    }
  }

  return result;
}

const wchar_t *CAkamaiFlashInstance::GetError(void)
{
  return this->error;
}

int SpecialIndexOf(const wchar_t *buffer, unsigned int start, wchar_t c)
{
  int index = 0;
  while ((buffer[start + index] != L'\0') && (buffer[start + index] != c))
  {
    index++;
  }
  return (buffer[start + index] != L'\0') ? index : (-1);
}

CDecryptedDataCollection *CAkamaiFlashInstance::GetDecryptedData(const uint8_t *key, unsigned int keyLength, CEncryptedDataCollection *encryptedDataCollection)
{
  CDecryptedDataCollection *result = new CDecryptedDataCollection();
  FREE_MEM(this->decryptorError);
  bool continueProcessing = true;

  CSocketContext *acceptedConnection = NULL;

  // there must be at least one accepted and not closed client connection
  HRESULT res = S_OK;
  for (unsigned int i = 0; (i < this->server->GetServers()->Count()); i++)
  {
    CTcpSocketContext *tcpSocketContext = dynamic_cast<CTcpSocketContext *>(this->server->GetServers()->GetItem(i));

    // there must be at least one accepted and not closed client connection
    if (tcpSocketContext->GetAcceptedConnections()->Count() != 0)
    {
      acceptedConnection = tcpSocketContext->GetAcceptedConnections()->GetItem(tcpSocketContext->GetAcceptedConnections()->Count() - 1);

      if (acceptedConnection->IsClosed())
      {
        // close all connections and create new server
        res = this->server->StopListening();
        CHECK_CONDITION_EXECUTE(FAILED(res), this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot stop listening, result: 0x%08X", this->instanceName, METHOD_GET_DECRYPTED_DATA_NAME, result));

        if (SUCCEEDED(res))
        {
          // initialize server
          res = this->InitializeTcpServer(COMMUNICATION_START_PORT, COMMUNICATION_STOP_PORT, COMMUNICATION_CLIENT_QUEUE);
          CHECK_CONDITION_EXECUTE(FAILED(res), this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot initialize servers, result: 0x%08X", this->instanceName, METHOD_GET_DECRYPTED_DATA_NAME, result));
        }

        if (SUCCEEDED(res))
        {
          // start listening
          res = this->server->StartListening();
          CHECK_CONDITION_EXECUTE(FAILED(res), this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot start listening, result: 0x%08X", this->instanceName, METHOD_GET_DECRYPTED_DATA_NAME, result));
        }

        if (SUCCEEDED(res))
        {
          // set communication port to flash instance
          // all TCP servers are listening on same port, just choose one (first) server
          res = this->SetCommPort(this->server->GetServers()->GetItem(0)->GetIpAddress()->GetPort());
          CHECK_CONDITION_EXECUTE(FAILED(res), this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot set communication port with Flash instance, result: 0x%08X", this->instanceName, METHOD_GET_DECRYPTED_DATA_NAME, result));
        }

        // we don't have accepted connection
        acceptedConnection = NULL;
      }
    }
  }
  continueProcessing &= SUCCEEDED(res);

  bool incomingPendingConnection = false;
  DWORD currentTime = GetTickCount();

  while ((acceptedConnection == NULL) && continueProcessing && (!incomingPendingConnection) && ((GetTickCount() - currentTime) <= COMMUNICATION_MAX_WAIT_TIME))
  {
    for (unsigned int i = 0; (continueProcessing && (!incomingPendingConnection) && (i < this->server->GetServers()->Count())); i++)
    {
      CTcpSocketContext *tcpSocketContext = dynamic_cast<CTcpSocketContext *>(this->server->GetServers()->GetItem(i));

      HRESULT res = tcpSocketContext->IsPendingIncomingConnection();
      continueProcessing &= SUCCEEDED(res);
      incomingPendingConnection = (res == S_FALSE);

      if (incomingPendingConnection)
      {
        continueProcessing &= SUCCEEDED(tcpSocketContext->AcceptPendingIncomingConnection());

        CHECK_CONDITION_EXECUTE(continueProcessing, acceptedConnection = tcpSocketContext->GetAcceptedConnections()->GetItem(tcpSocketContext->GetAcceptedConnections()->Count() - 1));
      }
    }

    if (continueProcessing && (!incomingPendingConnection))
    {
      Sleep(1);
    }
  }

  continueProcessing &= (acceptedConnection != NULL);
  CHECK_CONDITION_EXECUTE(!continueProcessing, this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, this->instanceName, METHOD_GET_DECRYPTED_DATA_NAME, L"no accepted connection"));

  // now we have accepted connection from client if continueProcessing is true

  unsigned int invokeMessageLength = 68 + 17 + 30;
  unsigned int socketDataLength = 0;
  unsigned int encryptedDataRawLength = 0;

  ALLOC_MEM_DEFINE_SET(encryptedDataCountStr, wchar_t, UINT8_MAX, 0);
  continueProcessing &= (encryptedDataCountStr != NULL);

  ALLOC_MEM_DEFINE_SET(encryptedRawDataLengthStr, wchar_t, UINT8_MAX, 0);
  continueProcessing &= (encryptedRawDataLengthStr != NULL);

  if (continueProcessing)
  {
    // key is also in data, just add it
    continueProcessing &= (_itow_s(encryptedDataCollection->Count() + 1, encryptedDataCountStr, UINT8_MAX, 10) == 0);
  }

  if (continueProcessing)
  {
    socketDataLength = 4 + keyLength;
    encryptedDataRawLength = keyLength;

    for (unsigned int i = 0; i < encryptedDataCollection->Count(); i++)
    {
      // size + encrypted data length
      unsigned int itemLength = encryptedDataCollection->GetItem(i)->GetEncryptedLength();
      socketDataLength += 4 + itemLength;
      encryptedDataRawLength += itemLength;
    }

    ALLOC_MEM_DEFINE_SET(socketData, uint8_t, socketDataLength, 0);
    continueProcessing &= (socketData != NULL);

    if (continueProcessing)
    {
      // fill socket data buffer
      // add key length in BIG ENDIAN encoding, add key
      unsigned int socketDataPosition = 0;

      WBE32INC(socketData, socketDataPosition, keyLength);
      memcpy(socketData + socketDataPosition, key, keyLength);
      socketDataPosition += keyLength;

      // add encrypted data : size (in BIG ENDIAN) + content
      for (unsigned int i = 0; i < encryptedDataCollection->Count(); i++)
      {
        CEncryptedData *data = encryptedDataCollection->GetItem(i);

        WBE32INC(socketData, socketDataPosition, data->GetEncryptedLength());
        memcpy(socketData + socketDataPosition, data->GetEncryptedData(), data->GetEncryptedLength());
        socketDataPosition += data->GetEncryptedLength();
      }

      unsigned int encryptedDataCountLength = wcslen(encryptedDataCountStr);

      invokeMessageLength += encryptedDataCountLength;

      // for memory space we need one extra null character at end
      invokeMessageLength++;

      if (continueProcessing)
      {
        continueProcessing &= (_itow_s(encryptedDataRawLength, encryptedRawDataLengthStr, UINT8_MAX, 10) == 0);
      }

      if (continueProcessing)
      {
        unsigned int encryptedDataRawLengthStrLength = wcslen(encryptedRawDataLengthStr);
        invokeMessageLength += encryptedDataRawLengthStrLength;

        // fastest string concatenation is direct memory copying

        ALLOC_MEM_DEFINE_SET(request, wchar_t, invokeMessageLength, 0);
        if (request != NULL)
        {
          // add first part of request
          unsigned int position = 0;
          wmemcpy(request + position, L"<invoke name=\"GetDecryptedData\" returntype=\"xml\"><arguments><string>", 68);
          position += 68;

          // add encrypted data count
          wmemcpy(request + position, encryptedDataCountStr, encryptedDataCountLength);
          position += encryptedDataCountLength;

          // add encrypted data length
          wmemcpy(request + position, L"</string><string>", 17);
          position += 17;

          wmemcpy(request + position, encryptedRawDataLengthStr, encryptedDataRawLengthStrLength);
          position += encryptedDataRawLengthStrLength;

          // add end of request
          wmemcpy(request + position, L"</string></arguments></invoke>", 30);
          position += 30;

          // send all prepared data to flash instance, wait max COMMUNICATION_MAX_WAIT_TIME ms
          if (continueProcessing)
          {
            DWORD currentTime = GetTickCount();
            unsigned int sentDataLength = 0;
            unsigned int lastSentDataLength = 0;

            res = S_OK;
            while (SUCCEEDED(res) && (sentDataLength < socketDataLength) && ((GetTickCount() - currentTime) <= COMMUNICATION_MAX_WAIT_TIME))
            {
              res = acceptedConnection->Send((const char *)(socketData + sentDataLength), socketDataLength - sentDataLength, &lastSentDataLength);
              
              if (continueProcessing)
              {
                sentDataLength += lastSentDataLength;
              }

              if (continueProcessing && (sentDataLength < socketDataLength))
              {
                Sleep(1);
              }
            }

            continueProcessing &= SUCCEEDED(res);
            continueProcessing &= (sentDataLength == socketDataLength);

            CHECK_CONDITION_EXECUTE(FAILED(res), this->logger->Log(LOGGER_ERROR, L"%s: %s: sent data length not equal to computed data length, sent: %u, computed: %u, result: 0x%08X", this->instanceName, METHOD_GET_DECRYPTED_DATA_NAME, sentDataLength, socketDataLength, res));
          }

          // wait until all data are read at flash instance
          if (continueProcessing)
          {
            DWORD currentTime = GetTickCount();
            wchar_t *commDataLength = FormatString(L"%u", socketDataLength);
            continueProcessing &= (commDataLength != NULL);

            while (continueProcessing && ((GetTickCount() - currentTime) <= COMMUNICATION_MAX_WAIT_TIME))
            {
              wchar_t *queryResult = this->GetResult(L"<invoke name=\"GetCommDataLength\" returntype=\"xml\"></invoke>");

              if (wcscmp(queryResult, commDataLength) != 0)
              {
                Sleep(10);
              }
              else
              {
                break;
              }
            }

            continueProcessing &= (wcscmp(queryResult, commDataLength) == 0);
            CHECK_CONDITION_EXECUTE(!continueProcessing, this->logger->Log(LOGGER_ERROR, L"%s: %s: data length in Flash instance not equal to computed data length, flash: %s, computed: %s", this->instanceName, METHOD_GET_DECRYPTED_DATA_NAME, queryResult, commDataLength));
            FREE_MEM(commDataLength);
          }

          if (continueProcessing)
          {
            wchar_t *queryResult = this->GetResult(request);

            // result data are delimited by '|'
            // each response consist of 2 fields : decrypted data count, raw decrypted data length

            unsigned int decodedDataCount = 0;        // decoded data count (should be encodedDataCount - 1)
            unsigned int decodedDataRawLength = 0;    // decoded data raw length (decoded raw data + error strings)
            unsigned int length = 0;                  // length of string variables
            position = 0;                             // parsing position
            int index = 0;

            if (continueProcessing)
            {
              // get first part = decoded data count
              index = SpecialIndexOf(queryResult, position, L'|');
              continueProcessing &= (index != -1);
            }

            if (continueProcessing)
            {
              index += position;

              length = index - position + 1;

              ALLOC_MEM_DEFINE_SET(decodedDataCountStr, wchar_t, length, 0);
              continueProcessing &= (decodedDataCountStr != NULL);

              if (continueProcessing)
              {
                wmemcpy(decodedDataCountStr, queryResult + position, (length - 1));

                decodedDataCount = GetValueUnsignedInt(decodedDataCountStr, 0);
              }

              FREE_MEM(decodedDataCountStr);
              position = index + 1;
            }

            if (continueProcessing)
            {
              // get second part = decoded data raw length
              index = SpecialIndexOf(queryResult, position, L'|');
              continueProcessing &= (index != -1);
            }

            if (continueProcessing)
            {
              index += position;

              length = index - position + 1;

              ALLOC_MEM_DEFINE_SET(decodedDataRawLengthStr, wchar_t, length, 0);
              continueProcessing &= (decodedDataRawLengthStr != NULL);

              if (continueProcessing)
              {
                wmemcpy(decodedDataRawLengthStr, queryResult + position, (length - 1));

                decodedDataRawLength = GetValueUnsignedInt(decodedDataRawLengthStr, 0);
              }

              FREE_MEM(decodedDataRawLengthStr);
              position = index + 1;
            }

            if (continueProcessing)
            {
              // each data packet consists of:
              // 4 bytes = length of decoded data in BIG ENDIAN encoding
              // length of decoded data bytes = decoded data
              // 4 bytes = error code in BIG ENDIAN encoding
              // 4 bytes = error string length in BIG endian encoding
              // error string length bytes = error string in UTF8
              unsigned int receivedDataLength = decodedDataCount * 12 + decodedDataRawLength;

              ALLOC_MEM_DEFINE_SET(buffer, uint8_t, receivedDataLength, 0);
              continueProcessing &= (buffer != NULL);

              // read all data from flash instance, wait max COMMUNICATION_MAX_WAIT_TIME ms
              if (continueProcessing)
              {
                DWORD currentTime = GetTickCount();
                unsigned int readDataLength = 0;
                unsigned int lastReadDataLength = 0;

                res = S_OK;
                while (continueProcessing && (readDataLength < receivedDataLength) && ((GetTickCount() - currentTime) <= COMMUNICATION_MAX_WAIT_TIME))
                {
                  res = acceptedConnection->Receive((char *)(buffer + readDataLength), receivedDataLength - readDataLength, &lastReadDataLength);
                  continueProcessing &= SUCCEEDED(res);

                  if (continueProcessing)
                  {
                    readDataLength += lastReadDataLength;
                  }

                  if (continueProcessing && (readDataLength < receivedDataLength))
                  {
                    Sleep(1);
                  }
                }

                continueProcessing &= (readDataLength == receivedDataLength);
                CHECK_CONDITION_EXECUTE(!continueProcessing, this->logger->Log(LOGGER_ERROR, L"%s: %s: received data length not equal to computed data length, received: %u, computed: %u, result: 0x%08X", this->instanceName, METHOD_GET_DECRYPTED_DATA_NAME, readDataLength, receivedDataLength, res));
              }

              if (continueProcessing)
              {
                // received all data without any error
                // parse it

                // each data packet consists of:
                // 4 bytes = length of decoded data in BIG ENDIAN encoding
                // length of decoded data bytes = decoded data
                // 4 bytes = error code in BIG ENDIAN encoding
                // 4 bytes = error string length in BIG endian encoding
                // error string length bytes = error string in UTF8

                position = 0;
                for (unsigned int i = 0; i < decodedDataCount; i++)
                {
                  unsigned int decodedDataLength = 0;
                  unsigned int errorCode = 0;
                  unsigned int errorStringLength = 0;

                  RBE32INC(buffer, position, decodedDataLength);

                  ALLOC_MEM_DEFINE_SET(decodedData, uint8_t, decodedDataLength, 0);
                  continueProcessing &= (decodedData != NULL);

                  if (continueProcessing)
                  {
                    memcpy(decodedData, buffer + position, decodedDataLength);
                    position += decodedDataLength;
                  }

                  if (continueProcessing)
                  {
                    RBE32INC(buffer, position, errorCode);
                    RBE32INC(buffer, position, errorStringLength);

                    ALLOC_MEM_DEFINE_SET(errorString, uint8_t, errorStringLength, 0);
                    continueProcessing &= (errorString != NULL);

                    if (continueProcessing)
                    {
                      memcpy(errorString, buffer + position, errorStringLength);
                      position += errorStringLength;

                      // now we have all data
                      continueProcessing &= result->Add(decodedData, decodedDataLength, errorCode, (char *)errorString);
                    }

                    if (!continueProcessing)
                    {
                      FREE_MEM(errorString);
                    }
                  }

                  if (!continueProcessing)
                  {
                    FREE_MEM(decodedData);
                  }
                }
              }

              FREE_MEM(buffer);
            }

            if (!continueProcessing)
            {
              result->Clear();
            }
          }
        }
        FREE_MEM(request);
      }
    }

    FREE_MEM(socketData);
  }
  FREE_MEM(encryptedDataCountStr);
  FREE_MEM(encryptedRawDataLengthStr);

  return result;
}

uint32_t CAkamaiFlashInstance::GetDecryptorErrorCode(void)
{
  return this->decryptorErrorCode;
}

const wchar_t *CAkamaiFlashInstance::GetDecryptorError(void)
{
  return this->decryptorError;
}

/* set methods */

void CAkamaiFlashInstance::SetDecryptionModuleUrl(const wchar_t *url)
{
  // set decryption module url
  wchar_t *query = FormatString(L"<invoke name=\"SetDecryptionModuleUrl\" returntype=\"xml\"><arguments><string>%s</string></arguments></invoke>", url);
  wchar_t *queryResult = this->GetResult(query);
  FREE_MEM(query);
  
  // initialize decryption module
  queryResult = this->GetResult(L"<invoke name=\"Init\" returntype=\"xml\"></invoke>");
}

/* other methods */

HRESULT CAkamaiFlashInstance::Initialize(void)
{
  HRESULT result = __super::Initialize();
  CHECK_POINTER_HRESULT(result, this->server, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    result = this->InitializeTcpServer(COMMUNICATION_START_PORT, COMMUNICATION_STOP_PORT, COMMUNICATION_CLIENT_QUEUE);

    CHECK_CONDITION_HRESULT(result, this->server->GetServers()->Count() > 0, result, E_OUTOFMEMORY);
  }

  if (SUCCEEDED(result))
  {
    // all TCP servers are listening on same port, just choose one (first) server
    result = this->SetCommPort(this->server->GetServers()->GetItem(0)->GetIpAddress()->GetPort());
  }

  return result;
}

HRESULT CAkamaiFlashInstance::InitializeTcpServer(unsigned int startPort, unsigned int endPort, unsigned int queueSize)
{
  HRESULT result = S_OK;

  for (unsigned int port = startPort; port < endPort ; port++)
  {
    result = server->Initialize(AF_UNSPEC, port, queueSize);

    if (SUCCEEDED(result))
    {
      result = server->StartListening();
    }

    if (SUCCEEDED(result))
    {
      break;
    }
  }

  return result;
}

HRESULT CAkamaiFlashInstance::SetCommPort(WORD port)
{
  HRESULT result = S_OK;
  // set communication port to flash instance
  wchar_t *query = FormatString(L"<invoke name=\"SetCommPort\" returntype=\"xml\"><arguments><string>%u</string></arguments></invoke>", port);
  CHECK_POINTER_HRESULT(result, query, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    this->GetResult(query);
  }
  FREE_MEM(query);

  return result;
}
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

#include "stdafx.h"

#pragma warning(push)
// disable warning: 'INT8_MIN' : macro redefinition
// warning is caused by stdint.h and intsafe.h, which both define same macro
#pragma warning(disable:4005)

#include "MPUrlSourceSplitter_Parser_M3U8.h"
#include "ParserPluginConfiguration.h"
#include "StreamPackage.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackageDataResponse.h"
#include "StreamInformationCollection.h"
#include "MPUrlSourceSplitter_Protocol_M3u8_Parameters.h"
#include "MPUrlSourceSplitter_Protocol_Http_Parameters.h"
#include "Parameters.h"
#include "formatUrl.h"
#include "MediaPlaylist.h"
#include "MediaPlaylistFactory.h"
#include "PlaylistVersion.h"
#include "VersionInfo.h"
#include "ErrorCodes.h"
#include "compress_zlib.h"
#include "base64.h"

#pragma warning(pop)

// parser implementation name
#ifdef _DEBUG
#define PARSER_IMPLEMENTATION_NAME                                            L"MPUrlSourceSplitter_Parser_M3U8d"
#else
#define PARSER_IMPLEMENTATION_NAME                                            L"MPUrlSourceSplitter_Parser_M3U8"
#endif

// 32 KB of data to request at start
#define MP_URL_SOURCE_SPLITTER_PARSER_M3U8_DATA_LENGTH_DEFAULT                32768

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Parser_M3U8(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Parser_M3U8 *parserPlugin = (CMPUrlSourceSplitter_Parser_M3U8 *)plugin;

    delete parserPlugin;
  }
}

CMPUrlSourceSplitter_Parser_M3U8::CMPUrlSourceSplitter_Parser_M3U8(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CParserPlugin(result, logger, configuration)
{
  this->lastReceivedLength = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PARSER_M3U8, DATE_INFO_MP_URL_SOURCE_SPLITTER_PARSER_M3U8);
    if (version != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
    }
    FREE_MEM(version);

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CMPUrlSourceSplitter_Parser_M3U8::~CMPUrlSourceSplitter_Parser_M3U8()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));
}

// CParserPlugin

HRESULT CMPUrlSourceSplitter_Parser_M3U8::GetParserResult(void)
{
  if (this->parserResult == PARSER_RESULT_PENDING)
  {
    CStreamInformationCollection *streams = new CStreamInformationCollection(&this->parserResult);
    CHECK_POINTER_HRESULT(this->parserResult, streams, this->parserResult, E_OUTOFMEMORY);

    CHECK_HRESULT_EXECUTE(this->parserResult, this->protocolHoster->GetStreamInformation(streams));

    if (SUCCEEDED(this->parserResult) && (streams->Count() == 1))
    {
      CStreamPackage *package = new CStreamPackage(&this->parserResult);
      CHECK_POINTER_HRESULT(this->parserResult, package, this->parserResult, E_OUTOFMEMORY);

      if (SUCCEEDED(this->parserResult))
      {
        unsigned int requestLength = MP_URL_SOURCE_SPLITTER_PARSER_M3U8_DATA_LENGTH_DEFAULT;
        bool receivedSameLength = false;
        this->parserResult = PARSER_RESULT_PENDING;

        while ((this->parserResult == PARSER_RESULT_PENDING) && (!receivedSameLength))
        {
          package->Clear();

          CStreamPackageDataRequest *request = new CStreamPackageDataRequest(&this->parserResult);
          CHECK_POINTER_HRESULT(this->parserResult, request, this->parserResult, E_OUTOFMEMORY);

          if (SUCCEEDED(this->parserResult))
          {
            request->SetStart(0);
            request->SetLength(requestLength);
            request->SetAnyNonZeroDataLength(true);

            package->SetRequest(request);
          }

          CHECK_CONDITION_EXECUTE(FAILED(this->parserResult), FREE_MEM_CLASS(request));
          CHECK_HRESULT_EXECUTE(this->parserResult, this->protocolHoster->ProcessStreamPackage(package));

          if (SUCCEEDED(this->parserResult))
          {
            this->parserResult = PARSER_RESULT_PENDING;
            CStreamPackageDataResponse *response = dynamic_cast<CStreamPackageDataResponse *>(package->GetResponse());

            if (package->IsError())
            {
              // TO DO: check type of error

              this->parserResult = PARSER_RESULT_NOT_KNOWN;
            }

            if (response != NULL)
            {
              receivedSameLength = (response->GetBuffer()->GetBufferOccupiedSpace() == this->lastReceivedLength);
              if (!receivedSameLength)
              {
                // try parse data
                unsigned int length = response->GetBuffer()->GetBufferOccupiedSpace() + 2;
                ALLOC_MEM_DEFINE_SET(buffer, unsigned char, length, 0);
                CHECK_POINTER_HRESULT(this->parserResult, buffer, this->parserResult, E_OUTOFMEMORY);

                if (SUCCEEDED(this->parserResult))
                {
                  response->GetBuffer()->CopyFromBuffer(buffer, length - 2);

  //                if (((buffer[0] == 0xFF) && (buffer[1] == 0xFE)) ||
  //                  ((buffer[1] == 0xFF) && (buffer[0] == 0xFE)))
  //                {
  //                  // input is probably in UTF-16 (Unicode)
  //                  char *temp = ConvertUnicodeToUtf8((wchar_t *)(buffer + 2));
  //                  FREE_MEM(buffer);
  //                  buffer = (unsigned char *)temp;

  //                  length = (buffer != NULL) ? strlen(temp) : 0;
  //                }

                  wchar_t *tempBuffer = ConvertToUnicodeA((const char *)buffer);

                  if (tempBuffer != NULL)
                  {
                    unsigned int tempBufferLength = wcslen(tempBuffer);

                    if (tempBufferLength != 0)
                    {
                      CMediaPlaylistFactory *factory = new CMediaPlaylistFactory(&this->parserResult);
                      CHECK_POINTER_HRESULT(this->parserResult, factory, this->parserResult, E_OUTOFMEMORY);

                      if (SUCCEEDED(this->parserResult))
                      {
                        CMediaPlaylist *mediaPlaylist = factory->CreateMediaPlaylist(&this->parserResult, tempBuffer, tempBufferLength);

                        if (SUCCEEDED(this->parserResult) || IS_M3U8_ERROR(this->parserResult))
                        {
                          if ((mediaPlaylist != NULL) && (mediaPlaylist->IsSetFlags(PLAYLIST_FLAG_DETECTED_HEADER)))
                          {
                            CHECK_CONDITION_HRESULT(this->parserResult, mediaPlaylist->GetDetectedVersion() <= PLAYLIST_LAST_KNOWN_VERSION, this->parserResult, E_M3U8_NOT_SUPPORTED_PLAYLIST_VERSION);

                            // check error code and if received all data (complete m3u8 playlist)
                            if (response->IsNoMoreDataAvailable())
                            {
                              this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, L"M3U8 file");
                              this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, tempBuffer);

                              this->parserResult = SUCCEEDED(this->parserResult) ? PARSER_RESULT_KNOWN : this->parserResult;

                              if (this->parserResult == PARSER_RESULT_KNOWN)
                              {
                                // extract cookies from current protocol connection parameters
                                CParameterCollection *usedCookies = new CParameterCollection(&this->parserResult);
                                CHECK_POINTER_HRESULT(this->parserResult, usedCookies, this->parserResult, E_OUTOFMEMORY);

                                if (SUCCEEDED(this->parserResult))
                                {
                                  CParameterCollection *protocolConnectionParameters = new CParameterCollection(&this->parserResult);
                                  CHECK_POINTER_HRESULT(this->parserResult, protocolConnectionParameters, this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_EXECUTE(SUCCEEDED(this->parserResult), this->parserResult = this->protocolHoster->GetConnectionParameters(protocolConnectionParameters));

                                  if (SUCCEEDED(protocolConnectionParameters))
                                  {
                                    unsigned int currentCookiesCount = protocolConnectionParameters->GetValueUnsignedInt(PARAMETER_NAME_HTTP_COOKIES_COUNT, true, 0);

                                    for (unsigned int i = 0; (SUCCEEDED(this->parserResult) && (i < currentCookiesCount)); i++)
                                    {
                                      wchar_t *httpCookieName = FormatString(HTTP_COOKIE_FORMAT_PARAMETER_NAME, i);
                                      CHECK_POINTER_HRESULT(this->parserResult, httpCookieName, this->parserResult, E_OUTOFMEMORY);

                                      if (SUCCEEDED(this->parserResult))
                                      {
                                        const wchar_t *cookieValue = protocolConnectionParameters->GetValue(httpCookieName, true, NULL);
                                        CHECK_POINTER_HRESULT(this->parserResult, cookieValue, this->parserResult, E_OUTOFMEMORY);

                                        CHECK_CONDITION_HRESULT(this->parserResult, usedCookies->Add(L"", cookieValue), this->parserResult, E_OUTOFMEMORY);
                                      }

                                      FREE_MEM(httpCookieName);
                                    }
                                  }

                                  FREE_MEM_CLASS(protocolConnectionParameters);
                                }

                                if (SUCCEEDED(this->parserResult))
                                {
                                  wchar_t *replacedUrl = ReplaceSchema(this->connectionParameters->GetValue(PARAMETER_NAME_URL, true, NULL), L"m3u8");
                                  CHECK_POINTER_HRESULT(this->parserResult, replacedUrl, this->parserResult, E_OUTOFMEMORY);

                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_COOKIE, true, PARAMETER_NAME_M3U8_COOKIE), this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_IGNORE_CONTENT_LENGTH, true, PARAMETER_NAME_M3U8_IGNORE_CONTENT_LENGTH), this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_OPEN_CONNECTION_TIMEOUT, true, PARAMETER_NAME_M3U8_OPEN_CONNECTION_TIMEOUT), this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_OPEN_CONNECTION_SLEEP_TIME, true, PARAMETER_NAME_M3U8_OPEN_CONNECTION_SLEEP_TIME), this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_TOTAL_REOPEN_CONNECTION_TIMEOUT, true, PARAMETER_NAME_M3U8_TOTAL_REOPEN_CONNECTION_TIMEOUT), this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_REFERER, true, PARAMETER_NAME_M3U8_REFERER), this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_USER_AGENT, true, PARAMETER_NAME_M3U8_USER_AGENT), this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_VERSION, true, PARAMETER_NAME_M3U8_VERSION), this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_URL, true, PARAMETER_NAME_M3U8_PLAYLIST_URL), this->parserResult, E_OUTOFMEMORY);

                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Update(PARAMETER_NAME_URL, true, replacedUrl), this->parserResult, E_OUTOFMEMORY);

                                  // copy current cookies parameters
                                  if (SUCCEEDED(this->parserResult) && (usedCookies != NULL) && (usedCookies->Count() != 0))
                                  {
                                    // first add count of cookies
                                    wchar_t *cookiesCountValue = FormatString(L"%u", usedCookies->Count());
                                    CHECK_POINTER_HRESULT(this->parserResult, cookiesCountValue, this->parserResult, E_OUTOFMEMORY);

                                    CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Update(PARAMETER_NAME_M3U8_COOKIES_COUNT, true, cookiesCountValue), this->parserResult, E_OUTOFMEMORY);
                                    for (unsigned int i = 0; (SUCCEEDED(this->parserResult) && (i < usedCookies->Count())); i++)
                                    {
                                      CParameter *cookie = usedCookies->GetItem(i);

                                      wchar_t *name = FormatString(M3U8_COOKIE_FORMAT_PARAMETER_NAME, i);
                                      CHECK_POINTER_HRESULT(this->parserResult, name, this->parserResult, E_OUTOFMEMORY);

                                      CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Update(name, true, cookie->GetValue()), this->parserResult, E_OUTOFMEMORY);
                                      FREE_MEM(name);
                                    }

                                    FREE_MEM(cookiesCountValue);
                                  }

                                  // add playlist content to connection parameters

                                  if (SUCCEEDED(this->parserResult))
                                  {
                                    // compress
                                    uint8_t *compressed = NULL;
                                    uint32_t compressedSize = 0;

                                    this->parserResult = compress_zlib((const uint8_t *)tempBuffer, (tempBufferLength + 1) * sizeof(wchar_t), &compressed, &compressedSize, -1);
                                    CHECK_CONDITION_EXECUTE(SUCCEEDED(this->parserResult), this->parserResult = PARSER_RESULT_KNOWN);

                                    if (SUCCEEDED(this->parserResult))
                                    {
                                      char *compressedBase64Encoded = NULL;
                                      this->parserResult = base64_encode(compressed, compressedSize, &compressedBase64Encoded);
                                      CHECK_CONDITION_EXECUTE(SUCCEEDED(this->parserResult), this->parserResult = PARSER_RESULT_KNOWN);

                                      if (SUCCEEDED(this->parserResult))
                                      {
                                        wchar_t *encoded = ConvertToUnicodeA(compressedBase64Encoded);
                                        CHECK_POINTER_HRESULT(this->parserResult, encoded, this->parserResult, E_OUTOFMEMORY);

                                        CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Add(PARAMETER_NAME_M3U8_PLAYLIST_CONTENT, encoded), this->parserResult, E_OUTOFMEMORY);

                                        FREE_MEM(encoded);
                                      }

                                      FREE_MEM(compressedBase64Encoded);
                                    }

                                    FREE_MEM(compressed);
                                  }
                                }

                                CHECK_CONDITION_EXECUTE(SUCCEEDED(this->parserResult), this->parserResult = PARSER_RESULT_KNOWN);
                              }
                            }
                            else
                            {
                              this->logger->Log(LOGGER_WARNING, L"%s: %s: M3U8 file probably not complete, M3U8 parse error: 0x%08X", PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, this->parserResult);
                              this->parserResult = PARSER_RESULT_PENDING;
                            }
                          }
                          else
                          {
                            this->parserResult = PARSER_RESULT_NOT_KNOWN;
                          }
                        }

                        FREE_MEM_CLASS(mediaPlaylist);
                      }


                      FREE_MEM_CLASS(factory);
                    }
                    else
                    {
                      this->parserResult = PARSER_RESULT_NOT_KNOWN;
                    }
                  }
                  else
                  {
                    this->parserResult = PARSER_RESULT_NOT_KNOWN;
                  }

                  FREE_MEM(tempBuffer);
                }

                FREE_MEM(buffer);
                requestLength *= 2;
              }

              this->lastReceivedLength = response->GetBuffer()->GetBufferOccupiedSpace();
            }
          }
        }
      }

      FREE_MEM_CLASS(package);
    }
    else
    {
      // M3U8 parser doesn't support multiple stream
      this->parserResult = PARSER_RESULT_NOT_KNOWN;
    }
    
    FREE_MEM_CLASS(streams);
  }

  return this->parserResult;
}

unsigned int CMPUrlSourceSplitter_Parser_M3U8::GetParserScore(void)
{
  return 100;
}

CParserPlugin::Action CMPUrlSourceSplitter_Parser_M3U8::GetAction(void)
{
  return GetNewConnection;
}

// CPlugin

const wchar_t *CMPUrlSourceSplitter_Parser_M3U8::GetName(void)
{
  return PARSER_NAME;
}

HRESULT CMPUrlSourceSplitter_Parser_M3U8::Initialize(CPluginConfiguration *configuration)
{
  HRESULT result = __super::Initialize(configuration);

  if (SUCCEEDED(result))
  {
    CParserPluginConfiguration *parserConfiguration = (CParserPluginConfiguration *)configuration;
    CHECK_POINTER_HRESULT(result, parserConfiguration, result, E_INVALIDARG);
  }

  if (SUCCEEDED(result))
  {
    this->configuration->LogCollection(this->logger, LOGGER_VERBOSE, PARSER_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);
  }

  return result;
}

// ISeeking interface

// IDemuxerOwner interface

// ISimpleProtocol interface
  
void CMPUrlSourceSplitter_Parser_M3U8::ClearSession(void)
{
  __super::ClearSession();

  this->lastReceivedLength = 0;
}

// IProtocol interface

/* protected methods */

wchar_t *CMPUrlSourceSplitter_Parser_M3U8::GetStoreFile(const wchar_t *extension)
{
  return NULL;
}
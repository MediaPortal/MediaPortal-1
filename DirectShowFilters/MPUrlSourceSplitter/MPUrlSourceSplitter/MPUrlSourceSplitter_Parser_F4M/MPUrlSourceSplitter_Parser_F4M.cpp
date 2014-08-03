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

#include "MPUrlSourceSplitter_Parser_F4M.h"
#include "ParserPluginConfiguration.h"
#include "StreamPackage.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackageDataResponse.h"
#include "StreamInformationCollection.h"
#include "MediaCollection.h"
#include "MPUrlSourceSplitter_Protocol_Afhs_Parameters.h"
#include "MPUrlSourceSplitter_Protocol_Http_Parameters.h"
#include "Parameters.h"
#include "BootstrapInfoBox.h"
#include "formatUrl.h"
#include "F4MManifest.h"
#include "F4M_Elements.h"
#include "F4MBootstrapInfoCollection.h"
#include "VersionInfo.h"
#include "ErrorCodes.h"

#pragma warning(pop)

// parser implementation name
#ifdef _DEBUG
#define PARSER_IMPLEMENTATION_NAME                                            L"MPUrlSourceSplitter_Parser_F4Md"
#else
#define PARSER_IMPLEMENTATION_NAME                                            L"MPUrlSourceSplitter_Parser_F4M"
#endif

// 32 KB of data to request at start
#define MP_URL_SOURCE_SPLITTER_PARSER_F4M_DATA_LENGTH_DEFAULT                 32768

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Parser_F4M(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Parser_F4M *parserPlugin = (CMPUrlSourceSplitter_Parser_F4M *)plugin;

    delete parserPlugin;
  }
}

CMPUrlSourceSplitter_Parser_F4M::CMPUrlSourceSplitter_Parser_F4M(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CParserPlugin(result, logger, configuration)
{
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);
    this->parserResult = PARSER_RESULT_PENDING;

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PARSER_F4M, DATE_INFO_MP_URL_SOURCE_SPLITTER_PARSER_F4M);
    if (version != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
    }
    FREE_MEM(version);

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CMPUrlSourceSplitter_Parser_F4M::~CMPUrlSourceSplitter_Parser_F4M()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));
}

// CParserPlugin

HRESULT CMPUrlSourceSplitter_Parser_F4M::GetParserResult(void)
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
        unsigned int requestLength = MP_URL_SOURCE_SPLITTER_PARSER_F4M_DATA_LENGTH_DEFAULT;
        uint64_t lastReceivedLength = 0;
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
            request->SetAnyDataLength(true);

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
              receivedSameLength = (response->GetBuffer()->GetBufferOccupiedSpace() == lastReceivedLength);
              if (!receivedSameLength)
              {
                // try parse data
                unsigned int length = response->GetBuffer()->GetBufferOccupiedSpace() + 2;
                ALLOC_MEM_DEFINE_SET(buffer, unsigned char, length, 0);
                CHECK_POINTER_HRESULT(this->parserResult, buffer, this->parserResult, E_OUTOFMEMORY);

                if (SUCCEEDED(this->parserResult))
                {
                  response->GetBuffer()->CopyFromBuffer(buffer, length - 2);

                  if (((buffer[0] == 0xFF) && (buffer[1] == 0xFE)) ||
                    ((buffer[1] == 0xFF) && (buffer[0] == 0xFE)))
                  {
                    // input is probably in UTF-16 (Unicode)
                    char *temp = ConvertUnicodeToUtf8((wchar_t *)(buffer + 2));
                    FREE_MEM(buffer);
                    buffer = (unsigned char *)temp;

                    length = (buffer != NULL) ? strlen(temp) : 0;
                  }

                  CF4MManifest *manifest = new CF4MManifest(&this->parserResult);
                  CHECK_POINTER_HRESULT(this->parserResult, manifest, this->parserResult, E_OUTOFMEMORY);

                  if (SUCCEEDED(this->parserResult))
                  {
                    if (manifest->Parse((char *)buffer))
                    {
                      this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, L"F4M manifest");
                      wchar_t *f4mBuffer = ConvertUtf8ToUnicode((char *)buffer);
                      if (f4mBuffer != NULL)
                      {
                        this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, f4mBuffer);
                      }
                      FREE_MEM(f4mBuffer);

                      // parse bootstrap info
                      // bootstrap info should have information about segments, fragments and seeking information

                      // extract cookies from connection parameters
                      CParameterCollection *usedCookies = new CParameterCollection(&this->parserResult);
                      CHECK_POINTER_HRESULT(this->parserResult, usedCookies, this->parserResult, E_OUTOFMEMORY);

                      if (SUCCEEDED(this->parserResult))
                      {
                        unsigned int currentCookiesCount = connectionParameters->GetValueUnsignedInt(PARAMETER_NAME_HTTP_COOKIES_COUNT, true, 0);

                        for (unsigned int i = 0; (SUCCEEDED(this->parserResult) && (i < currentCookiesCount)); i++)
                        {
                          wchar_t *httpCookieName = FormatString(HTTP_COOKIE_FORMAT_PARAMETER_NAME, i);
                          CHECK_POINTER_HRESULT(this->parserResult, httpCookieName, this->parserResult, E_OUTOFMEMORY);

                          if (SUCCEEDED(this->parserResult))
                          {
                            const wchar_t *cookieValue = connectionParameters->GetValue(httpCookieName, true, NULL);
                            CHECK_POINTER_HRESULT(this->parserResult, cookieValue, this->parserResult, E_OUTOFMEMORY);

                            CHECK_CONDITION_HRESULT(this->parserResult, usedCookies->Add(L"", cookieValue), this->parserResult, E_OUTOFMEMORY);
                            CHECK_CONDITION_EXECUTE(FAILED(this->parserResult), FREE_MEM_CLASS(cookieValue));
                          }

                          FREE_MEM(httpCookieName);
                        }
                      }

                      if (SUCCEEDED(this->parserResult))
                      {
                        wchar_t *baseUrl = GetBaseUrl(this->connectionParameters->GetValue(PARAMETER_NAME_URL, true, NULL));
                        CHECK_POINTER_HRESULT(this->parserResult, baseUrl, this->parserResult, E_OUTOFMEMORY);

                        CF4MBootstrapInfoCollection *bootstrapInfoCollection = new CF4MBootstrapInfoCollection(&this->parserResult);
                        CMediaCollection *mediaCollection = new CMediaCollection(&this->parserResult);

                        CHECK_POINTER_HRESULT(this->parserResult, bootstrapInfoCollection, this->parserResult, E_OUTOFMEMORY);
                        CHECK_POINTER_HRESULT(this->parserResult, mediaCollection, this->parserResult, E_OUTOFMEMORY);

                        if (SUCCEEDED(this->parserResult))
                        {
                          // bootstrap info profile have to be 'named' (F4M_ELEMENT_BOOTSTRAPINFO_ATTRIBUTE_PROFILE_VALUE_NAMED)
                          for (unsigned int i = 0; (SUCCEEDED(this->parserResult) && (i < manifest->GetBootstrapInfoCollection()->Count())); i++)
                          {
                            CF4MBootstrapInfo *f4mBootstrapInfo = manifest->GetBootstrapInfoCollection()->GetItem(i);

                            if ((f4mBootstrapInfo->GetProfile() != NULL) && (wcscmp(f4mBootstrapInfo->GetProfile(), F4M_ELEMENT_BOOTSTRAPINFO_ATTRIBUTE_PROFILE_VALUE_NAMEDW) == 0))
                            {
                              CF4MBootstrapInfo *bootstrapInfo = new CF4MBootstrapInfo();

                              if (bootstrapInfo != NULL)
                              {
                                bootstrapInfo->SetId(f4mBootstrapInfo->GetId());
                                bootstrapInfo->SetProfile(f4mBootstrapInfo->GetProfile());
                                bootstrapInfo->SetUrl(f4mBootstrapInfo->GetUrl());
                                bootstrapInfo->SetValue(f4mBootstrapInfo->GetValue());

                                if (bootstrapInfo->IsValid())
                                {
                                  if (!bootstrapInfoCollection->Add(bootstrapInfo))
                                  {
                                    FREE_MEM_CLASS(bootstrapInfo);
                                  }
                                }
                                else
                                {
                                  this->logger->Log(LOGGER_WARNING, L"%s: %s: bootstrap info is not valid, id: %s", PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, bootstrapInfo->GetId());
                                  FREE_MEM_CLASS(bootstrapInfo);
                                }
                              }
                            }
                            else
                            {
                              this->logger->Log(LOGGER_WARNING, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, L"bootstrap info profile is not 'named'");
                            }
                          }

                          // we should have url
                          // we exclude piece of media with drmAdditionalHeaderId
                          bool hasDrm = false;
                          for (unsigned int i = 0; (SUCCEEDED(this->parserResult) && (i < manifest->GetMediaCollection()->Count())); i++)
                          {
                            CF4MMedia *f4mMedia = manifest->GetMediaCollection()->GetItem(i);
                            hasDrm |= (f4mMedia->GetDrmAdditionalHeaderId() != NULL);
                            if ((f4mMedia->GetUrl() != NULL) && (f4mMedia->GetDrmAdditionalHeaderId() == NULL))
                            {
                              CMedia *media = new CMedia(
                                &this->parserResult,
                                f4mMedia->GetUrl(),
                                f4mMedia->GetBitrate(),
                                f4mMedia->GetWidth(),
                                f4mMedia->GetHeight(),
                                f4mMedia->GetDrmAdditionalHeaderId(),
                                f4mMedia->GetBootstrapInfoId(),
                                f4mMedia->GetDvrInfoId(),
                                f4mMedia->GetGroupSpecifier(),
                                f4mMedia->GetMulticastStreamName(),
                                f4mMedia->GetMetadata());

                              CHECK_POINTER_HRESULT(this->parserResult, media, this->parserResult, E_OUTOFMEMORY);
                              CHECK_CONDITION_HRESULT(this->parserResult, mediaCollection->Add(media), this->parserResult, E_OUTOFMEMORY);
                              CHECK_CONDITION_EXECUTE(FAILED(this->parserResult), FREE_MEM_CLASS(f4mMedia));
                            }
                            else
                            {
                              this->logger->Log(LOGGER_WARNING, L"%s: %s: piece of media doesn't have url ('%s') or has DRM additional header ID ('%s')", PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, f4mMedia->GetUrl(), f4mMedia->GetDrmAdditionalHeaderId());
                            }
                          }

                          if ((mediaCollection->Count() == 0) && (hasDrm))
                          {
                            // there is no piece of media and rest of them have DRM
                            this->parserResult = PARSER_RESULT_DRM_PROTECTED;
                          }

                          if (SUCCEEDED(this->parserResult))
                          {
                            if (!IsNullOrEmptyOrWhitespace(manifest->GetBaseUrl()->GetBaseUrl()))
                            {
                              FREE_MEM(baseUrl);
                              baseUrl = GetBaseUrl(manifest->GetBaseUrl()->GetBaseUrl());

                              CHECK_CONDITION_HRESULT(this->parserResult, !IsNullOrEmpty(baseUrl), this->parserResult, E_F4M_BASE_URL_NULL_OR_EMPTY);

                              CHECK_CONDITION_EXECUTE(SUCCEEDED(this->parserResult), this->logger->Log(LOGGER_VERBOSE, L"%s: %s: changed base URL: %s", PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, baseUrl));
                            }
                          }

                          CHECK_CONDITION_HRESULT(this->parserResult, bootstrapInfoCollection->Count() != 0, this->parserResult, E_F4M_NO_BOOTSTRAP_INFO_PROFILE);

                          if (SUCCEEDED(this->parserResult))
                          {
                            unsigned int i = 0;
                            while (i < mediaCollection->Count())
                            {
                              CMedia *media = mediaCollection->GetItem(i);
                              if (!bootstrapInfoCollection->Contains(media->GetBootstrapInfoId(), false))
                              {
                                this->logger->Log(LOGGER_ERROR, L"%s: %s: no bootstrap info '%s' for media '%s'", PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, media->GetBootstrapInfoId(), media->GetUrl());
                                mediaCollection->Remove(i);
                              }
                              else
                              {
                                i++;
                              }
                            }
                          }

                          CHECK_CONDITION_HRESULT(this->parserResult, mediaCollection->Count() != 0, this->parserResult, E_F4M_NO_PIECE_OF_MEDIA);

                          if (SUCCEEDED(this->parserResult))
                          {
                            // at least one media with bootstrap info and without DRM
                            // find media with highest bitrate

                            while (mediaCollection->Count() != 0)
                            {
                              unsigned int bitrate = 0;
                              unsigned int i = 0;
                              CMedia *mediaWithHighestBitstream = NULL;
                              unsigned int mediaWithHighestBitstreamIndex = UINT_MAX;
                              this->parserResult = PARSER_RESULT_KNOWN;

                              for (unsigned int i = 0; i < mediaCollection->Count(); i++)
                              {
                                CMedia *media = mediaCollection->GetItem(i);
                                if (media->GetBitrate() > bitrate)
                                {
                                  mediaWithHighestBitstream = media;
                                  mediaWithHighestBitstreamIndex = i;
                                  bitrate = media->GetBitrate();
                                }
                              }

                              if ((mediaWithHighestBitstream == NULL) && (mediaCollection->Count() != 0))
                              {
                                // if no piece of media chosen, then choose first media (if possible)
                                mediaWithHighestBitstream = mediaCollection->GetItem(0);
                                mediaWithHighestBitstreamIndex = 0;
                              }

                              CHECK_POINTER_HRESULT(this->parserResult, mediaWithHighestBitstream, this->parserResult, E_F4M_NO_PIECE_OF_MEDIA);
                              CHECK_POINTER_HRESULT(this->parserResult, mediaWithHighestBitstream->GetUrl(), this->parserResult, E_F4M_NO_MEDIA_URL);

                              // add media url into connection parameters
                              CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Add(PARAMETER_NAME_AFHS_MEDIA_PART_URL, mediaWithHighestBitstream->GetUrl()), this->parserResult, E_OUTOFMEMORY);

                              if (SUCCEEDED(this->parserResult) && (mediaWithHighestBitstream->GetMetadata() != NULL))
                              {
                                // add media metadata into connection parameters
                                CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Add(PARAMETER_NAME_AFHS_MEDIA_METADATA, mediaWithHighestBitstream->GetMetadata()), this->parserResult, E_OUTOFMEMORY);
                              }

                              if (SUCCEEDED(this->parserResult))
                              {
                                // add bootstrap info into connection parameters
                                CF4MBootstrapInfo *bootstrapInfo = bootstrapInfoCollection->GetBootstrapInfo(mediaWithHighestBitstream->GetBootstrapInfoId(), false);
                                CHECK_POINTER_HRESULT(this->parserResult, bootstrapInfo, this->parserResult, E_F4M_NO_BOOTSTRAP_INFO);

                                if (SUCCEEDED(this->parserResult))
                                {
                                  CHECK_POINTER_HRESULT(this->parserResult, bootstrapInfo->GetValue(), this->parserResult, E_F4M_NO_BOOTSTRAP_INFO_VALUE);

                                  if (FAILED(this->parserResult) && (bootstrapInfo->GetUrl() != NULL))
                                  {
                                    this->logger->Log(LOGGER_INFO, L"%s: %s: bootstrap info doesn't have value but has url, we need to download bootstrap info from '%s'", PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, bootstrapInfo->GetUrl());

                                    this->parserResult = bootstrapInfo->SetBaseUrl(baseUrl) ? PARSER_RESULT_KNOWN : E_OUTOFMEMORY;

                                    if (SUCCEEDED(this->parserResult))
                                    {
                                      HRESULT downloadResult = bootstrapInfo->DownloadBootstrapInfo(
                                        this->logger,
                                        PARSER_IMPLEMENTATION_NAME,
                                        this->connectionParameters->GetValueUnsignedInt(PARAMETER_NAME_FINISH_TIME, true, GetTickCount() + this->connectionParameters->GetValueUnsignedInt(PARAMETER_NAME_HTTP_OPEN_CONNECTION_TIMEOUT, true, HTTP_OPEN_CONNECTION_TIMEOUT_DEFAULT)),
                                        this->connectionParameters->GetValue(PARAMETER_NAME_HTTP_REFERER, true, NULL),
                                        this->connectionParameters->GetValue(PARAMETER_NAME_HTTP_USER_AGENT, true, NULL),
                                        this->connectionParameters->GetValue(PARAMETER_NAME_HTTP_COOKIE, true, NULL),
                                        usedCookies,
                                        this->connectionParameters->GetValue(PARAMETER_NAME_INTERFACE, true, NULL)
                                        );

                                      this->logger->Log(LOGGER_INFO, L"%s: %s: bootstrap info download result: 0x%08X", PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, downloadResult);
                                      CHECK_CONDITION_EXECUTE(SUCCEEDED(downloadResult), this->logger->Log(LOGGER_INFO, L"%s: %s: bootstrap info BASE64 encoded value: '%s'", PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, bootstrapInfo->GetValue()));
                                      CHECK_CONDITION_EXECUTE(FAILED(downloadResult), this->parserResult = downloadResult);
                                    }
                                  }

                                  if (SUCCEEDED(this->parserResult))
                                  {
                                    // but before adding, decode bootstrap info (just for sure if it is valid)
                                    HRESULT decodeResult = bootstrapInfo->GetDecodeResult();
                                    CHECK_CONDITION_EXECUTE(FAILED(decodeResult), this->parserResult = decodeResult);

                                    if (SUCCEEDED(this->parserResult))
                                    {
                                      CBootstrapInfoBox *bootstrapInfoBox = new CBootstrapInfoBox(&this->parserResult);
                                      CHECK_POINTER_HRESULT(this->parserResult, bootstrapInfoBox, this->parserResult, E_OUTOFMEMORY);

                                      CHECK_CONDITION_HRESULT(this->parserResult, bootstrapInfoBox->Parse(bootstrapInfo->GetDecodedValue(), bootstrapInfo->GetDecodedValueLength()), this->parserResult, E_F4M_CANNOT_PARSE_BOOTSTRAP_INFO_BOX);
                                      CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Add(PARAMETER_NAME_AFHS_BOOTSTRAP_INFO, bootstrapInfo->GetValue()), this->parserResult, E_OUTOFMEMORY);

                                      if (SUCCEEDED(this->parserResult) && bootstrapInfo->HasUrl())
                                      {
                                        wchar_t *bootstrapInfoUrl = FormatAbsoluteUrl(baseUrl, bootstrapInfo->GetUrl());
                                        CHECK_POINTER_HRESULT(this->parserResult, bootstrapInfoUrl, this->parserResult, E_OUTOFMEMORY);

                                        CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Add(PARAMETER_NAME_AFHS_BOOTSTRAP_INFO_URL, bootstrapInfoUrl), this->parserResult, E_OUTOFMEMORY);
                                        FREE_MEM(bootstrapInfoUrl);
                                      }

                                      FREE_MEM_CLASS(bootstrapInfoBox);
                                    }
                                  }
                                }
                              
                                CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Add(PARAMETER_NAME_AFHS_BASE_URL, baseUrl), this->parserResult, E_OUTOFMEMORY);
                                if (SUCCEEDED(this->parserResult))
                                {
                                  wchar_t *replacedUrl = ReplaceString(baseUrl, L"http://", L"afhs://");
                                  CHECK_POINTER_HRESULT(this->parserResult, replacedUrl, this->parserResult, E_OUTOFMEMORY);
                                  CHECK_POINTER_HRESULT(this->parserResult, wcsstr(replacedUrl, L"afhs://"), this->parserResult, E_F4M_ONLY_HTTP_PROTOCOL_SUPPORTED_IN_BASE_URL);

                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Update(PARAMETER_NAME_URL, true, replacedUrl), this->parserResult, E_OUTOFMEMORY);

                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_COOKIE, true, PARAMETER_NAME_AFHS_COOKIE), this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_IGNORE_CONTENT_LENGTH, true, PARAMETER_NAME_AFHS_IGNORE_CONTENT_LENGTH), this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_OPEN_CONNECTION_TIMEOUT, true, PARAMETER_NAME_AFHS_OPEN_CONNECTION_TIMEOUT), this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_OPEN_CONNECTION_SLEEP_TIME, true, PARAMETER_NAME_AFHS_OPEN_CONNECTION_SLEEP_TIME), this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_TOTAL_REOPEN_CONNECTION_TIMEOUT, true, PARAMETER_NAME_AFHS_TOTAL_REOPEN_CONNECTION_TIMEOUT), this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_REFERER, true, PARAMETER_NAME_AFHS_REFERER), this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_USER_AGENT, true, PARAMETER_NAME_AFHS_USER_AGENT), this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_VERSION, true, PARAMETER_NAME_AFHS_VERSION), this->parserResult, E_OUTOFMEMORY);
                                  CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_URL, true, PARAMETER_NAME_AFHS_MANIFEST_URL), this->parserResult, E_OUTOFMEMORY);

                                  // copy current cookies parameters
                                  if (SUCCEEDED(this->parserResult) && (usedCookies != NULL) && (usedCookies->Count() != 0))
                                  {
                                    // first add count of cookies
                                    wchar_t *cookiesCountValue = FormatString(L"%u", usedCookies->Count());
                                    CHECK_POINTER_HRESULT(this->parserResult, cookiesCountValue, this->parserResult, E_OUTOFMEMORY);

                                    CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Update(PARAMETER_NAME_AFHS_COOKIES_COUNT, true, cookiesCountValue), this->parserResult, E_OUTOFMEMORY);
                                    for (unsigned int i = 0; (SUCCEEDED(this->parserResult) && (i < usedCookies->Count())); i++)
                                    {
                                      CParameter *cookie = usedCookies->GetItem(i);

                                      wchar_t *name = FormatString(AFHS_COOKIE_FORMAT_PARAMETER_NAME, i);
                                      CHECK_POINTER_HRESULT(this->parserResult, name, this->parserResult, E_OUTOFMEMORY);

                                      CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Update(name, true, cookie->GetValue()), this->parserResult, E_OUTOFMEMORY);
                                      FREE_MEM(name);
                                    }

                                    FREE_MEM(cookiesCountValue);
                                  }

                                  if (SUCCEEDED(this->parserResult))
                                  {
                                    // add manifest content to connection parameters
                                    wchar_t *content = ConvertToUnicodeA((const char *)buffer);
                                    CHECK_POINTER_HRESULT(this->parserResult, content, this->parserResult, E_OUTOFMEMORY);

                                    CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Update(PARAMETER_NAME_AFHS_MANIFEST_CONTENT, true, content), this->parserResult, E_OUTOFMEMORY);
                                    FREE_MEM(content);
                                  }
                                }
                              }

                              if (FAILED(this->parserResult))
                              {
                                // error occured while processing last piece of media
                                // remove it and try to find another
                                mediaCollection->Remove(mediaWithHighestBitstreamIndex);

                                // remove all AFHS parameters from connection parameters
                                this->connectionParameters->Remove(PARAMETER_NAME_AFHS_BASE_URL, true);
                                this->connectionParameters->Remove(PARAMETER_NAME_AFHS_MEDIA_PART_URL, true);
                                this->connectionParameters->Remove(PARAMETER_NAME_AFHS_MEDIA_METADATA, true);
                                this->connectionParameters->Remove(PARAMETER_NAME_AFHS_BOOTSTRAP_INFO, true);
                                this->connectionParameters->Remove(PARAMETER_NAME_AFHS_BOOTSTRAP_INFO_URL, true);

                                this->connectionParameters->Remove(PARAMETER_NAME_AFHS_COOKIE, true);
                                this->connectionParameters->Remove(PARAMETER_NAME_AFHS_IGNORE_CONTENT_LENGTH, true);

                                this->connectionParameters->Remove(PARAMETER_NAME_AFHS_OPEN_CONNECTION_TIMEOUT, true);
                                this->connectionParameters->Remove(PARAMETER_NAME_AFHS_OPEN_CONNECTION_SLEEP_TIME, true);
                                this->connectionParameters->Remove(PARAMETER_NAME_AFHS_TOTAL_REOPEN_CONNECTION_TIMEOUT, true);

                                this->connectionParameters->Remove(PARAMETER_NAME_AFHS_REFERER, true);
                                this->connectionParameters->Remove(PARAMETER_NAME_AFHS_USER_AGENT, true);
                                this->connectionParameters->Remove(PARAMETER_NAME_AFHS_VERSION, true);
                                this->connectionParameters->Remove(PARAMETER_NAME_AFHS_MANIFEST_URL, true);
                                this->connectionParameters->Remove(PARAMETER_NAME_AFHS_MANIFEST_CONTENT, true);

                                this->connectionParameters->Remove(PARAMETER_NAME_AFHS_COOKIES_COUNT, true);
                              }
                              else
                              {
                                // we finished, we have media and bootstrap info
                                break;
                              }
                            }
                          }
                        }

                        FREE_MEM(baseUrl);
                        FREE_MEM_CLASS(bootstrapInfoCollection);
                        FREE_MEM_CLASS(mediaCollection);
                      }

                      FREE_MEM_CLASS(usedCookies);
                    }
                    else if (manifest->IsXml() && (manifest->GetParseError() != 0))
                    {
                      // we have XML declaration, it is valid XML file, just not complete
                      this->logger->Log(LOGGER_WARNING, L"%s: %s: XML file probably not complete, XML parse error: %d", PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, manifest->GetParseError());
                    }
                    else
                    {
                      // not F4M manifest or XML file
                      this->parserResult = PARSER_RESULT_NOT_KNOWN;
                    }
                  }

                  FREE_MEM_CLASS(manifest);
                }

                FREE_MEM(buffer);
                requestLength *= 2;
              }

              lastReceivedLength = response->GetBuffer()->GetBufferOccupiedSpace();
            }
          }
        }
      }

      FREE_MEM_CLASS(package);
      FREE_MEM_CLASS(streams);
    }
    else
    {
      // F4M parser doesn't support multiple stream
      this->parserResult = PARSER_RESULT_NOT_KNOWN;
    }
  }

  return this->parserResult;
}

unsigned int CMPUrlSourceSplitter_Parser_F4M::GetParserScore(void)
{
  return 100;
}

CParserPlugin::Action CMPUrlSourceSplitter_Parser_F4M::GetAction(void)
{
  return GetNewConnection;
}

// CPlugin

const wchar_t *CMPUrlSourceSplitter_Parser_F4M::GetName(void)
{
  return PARSER_NAME;
}

GUID CMPUrlSourceSplitter_Parser_F4M::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Parser_F4M::Initialize(CPluginConfiguration *configuration)
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

unsigned int CMPUrlSourceSplitter_Parser_F4M::GetSeekingCapabilities(void)
{
  return this->protocolHoster->GetSeekingCapabilities();
}

int64_t CMPUrlSourceSplitter_Parser_F4M::SeekToTime(unsigned int streamId, int64_t time)
{
  return this->protocolHoster->SeekToTime(streamId, time);
}

void CMPUrlSourceSplitter_Parser_F4M::SetPauseSeekStopMode(unsigned int pauseSeekStopMode)
{
  this->protocolHoster->SetPauseSeekStopMode(pauseSeekStopMode);
}

// IDemuxerOwner interface

int64_t CMPUrlSourceSplitter_Parser_F4M::GetDuration(void)
{
  return this->protocolHoster->GetDuration();
}

HRESULT CMPUrlSourceSplitter_Parser_F4M::ProcessStreamPackage(CStreamPackage *streamPackage)
{
  return this->protocolHoster->ProcessStreamPackage(streamPackage);
}

// ISimpleProtocol interface

unsigned int CMPUrlSourceSplitter_Parser_F4M::GetOpenConnectionTimeout(void)
{
  return this->protocolHoster->GetOpenConnectionTimeout();
}

unsigned int CMPUrlSourceSplitter_Parser_F4M::GetOpenConnectionSleepTime(void)
{
  return this->protocolHoster->GetOpenConnectionSleepTime();
}

unsigned int CMPUrlSourceSplitter_Parser_F4M::GetTotalReopenConnectionTimeout(void)
{
  return this->protocolHoster->GetTotalReopenConnectionTimeout();
}

HRESULT CMPUrlSourceSplitter_Parser_F4M::StartReceivingData(CParameterCollection *parameters)
{
  return E_NOTIMPL;
}

HRESULT CMPUrlSourceSplitter_Parser_F4M::StopReceivingData(void)
{
  return E_NOTIMPL;
}

HRESULT CMPUrlSourceSplitter_Parser_F4M::QueryStreamProgress(CStreamProgress *streamProgress)
{
  return this->protocolHoster->QueryStreamProgress(streamProgress);
}
  
HRESULT CMPUrlSourceSplitter_Parser_F4M::ClearSession(void)
{
  return __super::ClearSession();
}

void CMPUrlSourceSplitter_Parser_F4M::ReportStreamTime(uint64_t streamTime, uint64_t streamPosition)
{
  this->protocolHoster->ReportStreamTime(streamTime, streamPosition);
}

HRESULT CMPUrlSourceSplitter_Parser_F4M::GetStreamInformation(CStreamInformationCollection *streams)
{
  return this->protocolHoster->GetStreamInformation(streams);
}

// IProtocol interface

ProtocolConnectionState CMPUrlSourceSplitter_Parser_F4M::GetConnectionState(void)
{
  return None;
}

HRESULT CMPUrlSourceSplitter_Parser_F4M::ParseUrl(const CParameterCollection *parameters)
{
  return E_NOTIMPL;
}

HRESULT CMPUrlSourceSplitter_Parser_F4M::ReceiveData(CStreamPackage *streamPackage)
{
  return E_NOTIMPL;
}
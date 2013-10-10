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
#include "VersionInfo.h"
#include "MediaCollection.h"
#include "MPUrlSourceSplitter_Protocol_Afhs_Parameters.h"
#include "MPUrlSourceSplitter_Protocol_Http_Parameters.h"
#include "Parameters.h"
#include "BootstrapInfoBox.h"
#include "formatUrl.h"
#include "F4MManifest.h"
#include "F4M_Elements.h"
#include "F4MBootstrapInfoCollection.h"

#pragma warning(pop)

// parser implementation name
#ifdef _DEBUG
#define PARSER_IMPLEMENTATION_NAME                                            L"MPUrlSourceSplitter_Parser_F4Md"
#else
#define PARSER_IMPLEMENTATION_NAME                                            L"MPUrlSourceSplitter_Parser_F4M"
#endif

PIPlugin CreatePluginInstance(CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Parser_F4M(logger, configuration);
}

void DestroyPluginInstance(PIPlugin pProtocol)
{
  if (pProtocol != NULL)
  {
    CMPUrlSourceSplitter_Parser_F4M *pClass = (CMPUrlSourceSplitter_Parser_F4M *)pProtocol;
    delete pClass;
  }
}

CMPUrlSourceSplitter_Parser_F4M::CMPUrlSourceSplitter_Parser_F4M(CLogger *logger, CParameterCollection *configuration)
{
  this->connectionParameters = new CParameterCollection();
  if (configuration != NULL)
  {
    this->connectionParameters->Append(configuration);
  }

  this->logger = new CLogger(logger);
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);

  wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PARSER_F4M, DATE_INFO_MP_URL_SOURCE_SPLITTER_PARSER_F4M);
  if (version != NULL)
  {
    this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
  }
  FREE_MEM(version);

  this->storedMediaPackets = new CMediaPacketCollection();

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
}

CMPUrlSourceSplitter_Parser_F4M::~CMPUrlSourceSplitter_Parser_F4M()
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  FREE_MEM_CLASS(this->connectionParameters);
  FREE_MEM_CLASS(this->storedMediaPackets);

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  FREE_MEM_CLASS(this->logger);
}

// IParser interface

HRESULT CMPUrlSourceSplitter_Parser_F4M::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  this->connectionParameters->Clear();
  this->storedMediaPackets->Clear();

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
  return S_OK;
}

ParseResult CMPUrlSourceSplitter_Parser_F4M::ParseMediaPackets(CMediaPacketCollection *mediaPackets, CParameterCollection *connectionParameters)
{
  ParseResult result = ParseResult_NotKnown;
  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME);

  if (mediaPackets != NULL)
  {
    if (this->storedMediaPackets->Append(mediaPackets))
    {
      unsigned int length = 0;
      for (unsigned int i = 0; i < this->storedMediaPackets->Count(); i++)
      {
        CMediaPacket *mp = this->storedMediaPackets->GetItem(i);
        length += mp->GetBuffer()->GetBufferOccupiedSpace();
      }
      length += 2;

      ALLOC_MEM_DEFINE_SET(buffer, unsigned char, length, 0);
      if ((buffer != NULL) && (length > 1))
      {
        unsigned int bufferPosition = 0;
        for (unsigned int i = 0; i < this->storedMediaPackets->Count(); i++)
        {
          CMediaPacket *mp = this->storedMediaPackets->GetItem(i);
          unsigned int bufferOccupiedSpace = mp->GetBuffer()->GetBufferOccupiedSpace();
          mp->GetBuffer()->CopyFromBuffer(buffer + bufferPosition, bufferOccupiedSpace);
          bufferPosition += bufferOccupiedSpace;
        }

        if (((buffer[0] == 0xFF) && (buffer[1] == 0xFE)) ||
            ((buffer[1] == 0xFF) && (buffer[0] == 0xFE)))
        {
          // input is probably in UTF-16 (Unicode)
          char *temp = ConvertUnicodeToUtf8((wchar_t *)(buffer + 2));
          FREE_MEM(buffer);
          buffer = (unsigned char *)temp;

          length = (buffer != NULL) ? strlen(temp) : 0;
        }

        CF4MManifest *manifest = new CF4MManifest();
        if (manifest != NULL)
        {
          if (manifest->Parse((char *)buffer))
          {
            this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"F4M manifest");
            wchar_t *f4mBuffer = ConvertUtf8ToUnicode((char *)buffer);
            if (f4mBuffer != NULL)
            {
              this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, f4mBuffer);
            }
            FREE_MEM(f4mBuffer);

            // parse bootstrap info
            // bootstrap info should have information about segments, fragments and seeking information

            // extract cookies from connection parameters
            CParameterCollection *usedCookies = new CParameterCollection();
            bool continueParsing = (usedCookies != NULL);

            if (connectionParameters != NULL)
            {
              unsigned int currentCookiesCount = connectionParameters->GetValueUnsignedInt(PARAMETER_NAME_HTTP_COOKIES_COUNT, true, 0);

              for (unsigned int i = 0; (continueParsing && (i < currentCookiesCount)); i++)
              {
                wchar_t *httpCookieName = FormatString(HTTP_COOKIE_FORMAT_PARAMETER_NAME, i);
                continueParsing &= (httpCookieName != NULL);

                if (continueParsing)
                {
                  const wchar_t *cookieValue = connectionParameters->GetValue(httpCookieName, true, NULL);
                  continueParsing &= (cookieValue != NULL);

                  if (continueParsing)
                  {
                    CParameter *cookie = new CParameter(L"", cookieValue);
                    continueParsing &= (cookie != NULL);

                    if (continueParsing)
                    {
                      continueParsing &= usedCookies->Add(cookie);
                    }

                    if (!continueParsing)
                    {
                      FREE_MEM_CLASS(cookie);
                    }
                  }
                }

                FREE_MEM(httpCookieName);
              }
            }

            wchar_t *baseUrl = GetBaseUrl(this->connectionParameters->GetValue(PARAMETER_NAME_URL, true, NULL));
            continueParsing &= (baseUrl != NULL);
            
            CF4MBootstrapInfoCollection *bootstrapInfoCollection = new CF4MBootstrapInfoCollection();
            CMediaCollection *mediaCollection = new CMediaCollection();

            continueParsing &= ((bootstrapInfoCollection != NULL) && (mediaCollection != NULL));

            if (continueParsing)
            {
              // bootstrap info profile have to be 'named' (F4M_ELEMENT_BOOTSTRAPINFO_ATTRIBUTE_PROFILE_VALUE_NAMED)
              for (unsigned int i = 0; i < manifest->GetBootstrapInfoCollection()->Count(); i++)
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
                      this->logger->Log(LOGGER_WARNING, L"%s: %s: bootstrap info is not valid, id: %s", PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, bootstrapInfo->GetId());
                      FREE_MEM_CLASS(bootstrapInfo);
                    }
                  }
                }
                else
                {
                  this->logger->Log(LOGGER_WARNING, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"bootstrap info profile is not 'named'");
                }
              }

              // we should have url
              // we exclude piece of media with drmAdditionalHeaderId
              bool hasDrm = false;
              for (unsigned int i = 0; i < manifest->GetMediaCollection()->Count(); i++)
              {
                CF4MMedia *f4mMedia = manifest->GetMediaCollection()->GetItem(i);
                hasDrm |= (f4mMedia->GetDrmAdditionalHeaderId() != NULL);
                if ((f4mMedia->GetUrl() != NULL) && (f4mMedia->GetDrmAdditionalHeaderId() == NULL))
                {
                  CMedia *media = new CMedia(
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

                  if (media != NULL)
                  {
                    if (!mediaCollection->Add(media))
                    {
                      FREE_MEM_CLASS(f4mMedia);
                    }
                  }
                }
                else
                {
                  this->logger->Log(LOGGER_WARNING, L"%s: %s: piece of media doesn't have url ('%s') or has DRM additional header ID ('%s')", PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, f4mMedia->GetUrl(), f4mMedia->GetDrmAdditionalHeaderId());
                }
              }

              if ((mediaCollection->Count() == 0) && (hasDrm))
              {
                // there is no piece of media and rest of them have DRM
                result = ParseResult_DrmProtected;
                continueParsing = false;
              }

              if (!IsNullOrEmptyOrWhitespace(manifest->GetBaseUrl()->GetBaseUrl()))
              {
                FREE_MEM(baseUrl);
                baseUrl = GetBaseUrl(manifest->GetBaseUrl()->GetBaseUrl());

                if (baseUrl == NULL)
                {
                  // cannot get base url
                  this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot get base url");
                  continueParsing = false;
                }
                else if (IsNullOrEmpty(baseUrl))
                {
                  // base url is empty
                  this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"base url is empty");
                  continueParsing = false;
                }

                if (continueParsing)
                {
                  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: changed base URL: %s", PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, baseUrl);
                }
              }

              if (continueParsing && (bootstrapInfoCollection->Count() == 0))
              {
                this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"no bootstrap info profile");
                continueParsing = false;
              }

              if (continueParsing)
              {
                unsigned int i = 0;
                while (i < mediaCollection->Count())
                {
                  CMedia *media = mediaCollection->GetItem(i);
                  if (!bootstrapInfoCollection->Contains(media->GetBootstrapInfoId(), false))
                  {
                    this->logger->Log(LOGGER_ERROR, L"%s: %s: no bootstrap info '%s' for media '%s'", PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, media->GetBootstrapInfoId(), media->GetUrl());
                    mediaCollection->Remove(i);
                  }
                  else
                  {
                    i++;
                  }
                }
              }

              if (continueParsing && (mediaCollection->Count() == 0))
              {
                this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"no piece of media");
                continueParsing = false;
              }

              if (continueParsing)
              {
                // at least one media with bootstrap info and without DRM
                // find media with highest bitrate

                while (mediaCollection->Count() != 0)
                {
                  unsigned int bitrate = 0;
                  unsigned int i = 0;
                  CMedia *mediaWithHighestBitstream = NULL;
                  unsigned int mediaWithHighestBitstreamIndex = UINT_MAX;
                  continueParsing = true;

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

                  if (mediaWithHighestBitstream != NULL)
                  {
                    continueParsing &= (mediaWithHighestBitstream->GetUrl() != NULL);

                    if (continueParsing)
                    {
                      // add media url into connection parameters
                      CParameter *mediaUrlParameter = new CParameter(PARAMETER_NAME_AFHS_MEDIA_PART_URL, mediaWithHighestBitstream->GetUrl());
                      continueParsing &= (mediaUrlParameter != NULL);

                      if (continueParsing)
                      {
                        continueParsing &= this->connectionParameters->Add(mediaUrlParameter);

                        if (!continueParsing)
                        {
                          this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot add media URL parameter into connection parameters");
                        }
                      }
                      else
                      {
                        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot create media URL parameter");
                      }

                      if ((!continueParsing) && (mediaUrlParameter != NULL))
                      {
                        // cleanup, cannot add media URL parameter into connection parameters
                        FREE_MEM_CLASS(mediaUrlParameter);
                      }
                    }

                    if (continueParsing)
                    {
                      if (mediaWithHighestBitstream->GetMetadata() != NULL)
                      {
                        // add media metadata into connection parameters
                        CParameter *mediaMetadataParameter = new CParameter(PARAMETER_NAME_AFHS_MEDIA_METADATA, mediaWithHighestBitstream->GetMetadata());
                        continueParsing &= (mediaMetadataParameter != NULL);

                        if (continueParsing)
                        {
                          continueParsing &= this->connectionParameters->Add(mediaMetadataParameter);

                          if (!continueParsing)
                          {
                            this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot add media metadata parameter into connection parameters");
                          }
                        }
                        else
                        {
                          this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot create media metadata parameter");
                        }

                        if ((!continueParsing) && (mediaMetadataParameter != NULL))
                        {
                          // cleanup, cannot add media metadata parameter into connection parameters
                          FREE_MEM_CLASS(mediaMetadataParameter);
                        }
                      }
                    }

                    if (continueParsing)
                    {
                      // add bootstrap info into connection parameters
                      CF4MBootstrapInfo *bootstrapInfo = bootstrapInfoCollection->GetBootstrapInfo(mediaWithHighestBitstream->GetBootstrapInfoId(), false);
                      if (bootstrapInfo != NULL)
                      {
                        continueParsing &= (bootstrapInfo->GetValue() != NULL);

                        if ((!continueParsing) && (bootstrapInfo->GetUrl() != NULL))
                        {
                          this->logger->Log(LOGGER_INFO, L"%s: %s: bootstrap info doesn't have value but has url, we need to download bootstrap info from '%s'", PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, bootstrapInfo->GetUrl());

                          continueParsing = bootstrapInfo->SetBaseUrl(baseUrl);
                          HRESULT downloadResult = bootstrapInfo->DownloadBootstrapInfo(
                            this->logger,
                            PARSER_IMPLEMENTATION_NAME,
                            this->connectionParameters->GetValueUnsignedInt(PARAMETER_NAME_FINISH_TIME, true, GetTickCount() + this->connectionParameters->GetValueUnsignedInt(PARAMETER_NAME_HTTP_RECEIVE_DATA_TIMEOUT, true, HTTP_RECEIVE_DATA_TIMEOUT_DEFAULT)),
                            this->connectionParameters->GetValue(PARAMETER_NAME_HTTP_REFERER, true, NULL),
                            this->connectionParameters->GetValue(PARAMETER_NAME_HTTP_USER_AGENT, true, NULL),
                            this->connectionParameters->GetValue(PARAMETER_NAME_HTTP_COOKIE, true, NULL),
                            usedCookies,
                            this->connectionParameters->GetValue(PARAMETER_NAME_INTERFACE, true, NULL)
                            );

                          this->logger->Log(LOGGER_INFO, L"%s: %s: bootstrap info download result: 0x%08X", PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, downloadResult);
                          if (SUCCEEDED(result))
                          {
                            this->logger->Log(LOGGER_INFO, L"%s: %s: bootstrap info BASE64 encoded value: '%s'", PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, bootstrapInfo->GetValue());
                          }
                          continueParsing &= SUCCEEDED(downloadResult);
                        }

                        if (continueParsing)
                        {
                          // but before adding, decode bootstrap info (just for sure if it is valid)
                          HRESULT decodeResult = bootstrapInfo->GetDecodeResult();
                          continueParsing &= SUCCEEDED(decodeResult);

                          if (continueParsing)
                          {
                            CBootstrapInfoBox *bootstrapInfoBox = new CBootstrapInfoBox();
                            continueParsing &= (bootstrapInfoBox != NULL);

                            if (continueParsing)
                            {
                              continueParsing &= bootstrapInfoBox->Parse(bootstrapInfo->GetDecodedValue(), bootstrapInfo->GetDecodedValueLength());

                              if (continueParsing)
                              {
                                // create and add connection parameter
                                CParameter *bootstrapInfoParameter = new CParameter(PARAMETER_NAME_AFHS_BOOTSTRAP_INFO, bootstrapInfo->GetValue());
                                continueParsing &= (bootstrapInfoParameter != NULL);

                                if (continueParsing)
                                {
                                  continueParsing &= this->connectionParameters->Add(bootstrapInfoParameter);

                                  if (!continueParsing)
                                  {
                                    this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot add bootstrap info parameter into connection parameters");
                                  }
                                }
                                else
                                {
                                  this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot create bootstrap info parameter");
                                }

                                if ((!continueParsing) && (bootstrapInfoParameter != NULL))
                                {
                                  // cleanup, cannot add bootstrap info parameter into connection parameters
                                  FREE_MEM_CLASS(bootstrapInfoParameter);
                                }
                              }
                              else
                              {
                                this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot parse bootstrap info box");
                              }

                              if (continueParsing && (bootstrapInfo->HasUrl()))
                              {
                                wchar_t *bootstrapInfoUrl = FormatAbsoluteUrl(baseUrl, bootstrapInfo->GetUrl());
                                continueParsing &= (bootstrapInfoUrl != NULL);

                                if (continueParsing)
                                {
                                  // create and add connection parameter
                                  CParameter *bootstrapInfoUrlParameter = new CParameter(PARAMETER_NAME_AFHS_BOOTSTRAP_INFO_URL, bootstrapInfoUrl);
                                  continueParsing &= (bootstrapInfoUrlParameter != NULL);

                                  if (continueParsing)
                                  {
                                    continueParsing &= this->connectionParameters->Add(bootstrapInfoUrlParameter);
                                  }

                                  if (!continueParsing)
                                  {
                                    FREE_MEM_CLASS(bootstrapInfoUrlParameter);
                                  }
                                }
                                FREE_MEM(bootstrapInfoUrl);
                              }
                            }
                            else
                            {
                              this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"not enough memory for bootstrap info box");
                            }

                            FREE_MEM_CLASS(bootstrapInfoBox);
                          }
                          else
                          {
                            this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot decode bootstrap info BASE64 value, reason: 0x%08X", PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, decodeResult);
                          }
                        }
                      }
                      else
                      {
                        this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot find bootstrap info '%s' for media '%s'", PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, mediaWithHighestBitstream->GetBootstrapInfoId(), mediaWithHighestBitstream->GetUrl());
                        continueParsing = false;
                      }
                    }

                    if (continueParsing)
                    {
                      // add base URL into connection parameters
                      CParameter *baseUrlParameter = new CParameter(PARAMETER_NAME_AFHS_BASE_URL, baseUrl);
                      continueParsing &= (baseUrlParameter != NULL);

                      if (continueParsing)
                      {
                        continueParsing &= this->connectionParameters->Add(baseUrlParameter);

                        if (!continueParsing)
                        {
                          this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot add base URL parameter into connection parameters");
                        }
                      }
                      else
                      {
                        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot create base URL parameter");
                      }

                      if ((!continueParsing) && (baseUrlParameter != NULL))
                      {
                        // cleanup, cannot add base URL parameter into connection parameters
                        FREE_MEM_CLASS(baseUrlParameter);
                      }
                    }

                    if (continueParsing)
                    {
                      wchar_t *replacedUrl = ReplaceString(baseUrl, L"http://", L"afhs://");
                      if (replacedUrl != NULL)
                      {
                        if (wcsstr(replacedUrl, L"afhs://") != NULL)
                        {
                          CParameter *urlParameter = new CParameter(PARAMETER_NAME_URL, replacedUrl);
                          if (urlParameter != NULL)
                          {
                            continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_COOKIE, true, PARAMETER_NAME_AFHS_COOKIE);
                            continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_IGNORE_CONTENT_LENGTH, true, PARAMETER_NAME_AFHS_IGNORE_CONTENT_LENGTH);
                            continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_RECEIVE_DATA_TIMEOUT, true, PARAMETER_NAME_AFHS_RECEIVE_DATA_TIMEOUT);
                            continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_REFERER, true, PARAMETER_NAME_AFHS_REFERER);
                            continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_USER_AGENT, true, PARAMETER_NAME_AFHS_USER_AGENT);
                            continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_VERSION, true, PARAMETER_NAME_AFHS_VERSION);
                            continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_URL, true, PARAMETER_NAME_AFHS_MANIFEST_URL);

                            // copy current cookies parameters
                            if ((usedCookies != NULL) && (usedCookies->Count() != 0))
                            {
                              // first add count of cookies
                              wchar_t *cookiesCountValue = FormatString(L"%u", usedCookies->Count());
                              continueParsing &= (cookiesCountValue != NULL);

                              if (continueParsing)
                              {
                                CParameter *cookiesCount = new CParameter(PARAMETER_NAME_AFHS_COOKIES_COUNT, cookiesCountValue);
                                continueParsing &= (cookiesCount != NULL);

                                if (continueParsing)
                                {
                                  continueParsing &= this->connectionParameters->Update(PARAMETER_NAME_AFHS_COOKIES_COUNT, true, cookiesCount);
                                }

                                if (!continueParsing)
                                {
                                  FREE_MEM_CLASS(cookiesCount);
                                }
                              }

                              if (continueParsing)
                              {
                                for (unsigned int i = 0; (continueParsing && (i < usedCookies->Count())); i++)
                                {
                                  CParameter *cookie = usedCookies->GetItem(i);
                                  wchar_t *name = FormatString(AFHS_COOKIE_FORMAT_PARAMETER_NAME, i);
                                  continueParsing &= (name != NULL);

                                  if (continueParsing)
                                  {
                                    CParameter *cookieToAdd = new CParameter(name, cookie->GetValue());
                                    continueParsing = (cookieToAdd != NULL);

                                    if (continueParsing)
                                    {
                                      continueParsing &= this->connectionParameters->Update(name, true, cookieToAdd);
                                    }

                                    if (!continueParsing)
                                    {
                                      FREE_MEM_CLASS(cookieToAdd);
                                    }
                                  }

                                  FREE_MEM(name);
                                }
                              }
                              FREE_MEM(cookiesCountValue);
                            }

                            if (continueParsing)
                            {
                              wchar_t *content = ConvertToUnicodeA((const char *)buffer);
                              continueParsing &= (content != NULL);
                              if (continueParsing)
                              {
                                // add manifest content to connection parameters
                                CParameter *manifestContent = new CParameter(PARAMETER_NAME_AFHS_MANIFEST_CONTENT, content);
                                continueParsing &= (manifestContent != NULL);

                                if (continueParsing)
                                {
                                  continueParsing &= this->connectionParameters->Add(manifestContent);
                                }

                                if (!continueParsing)
                                {
                                  FREE_MEM_CLASS(manifestContent);
                                }
                              }
                              FREE_MEM(content);
                            }

                            if (continueParsing)
                            {
                              this->connectionParameters->Remove(PARAMETER_NAME_URL, true);
                              continueParsing &= this->connectionParameters->Add(urlParameter);
                            }

                            if (continueParsing)
                            {
                              result = ParseResult_Known;
                            }
                            else
                            {
                              this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot add new URL parameter into connection parameters");
                            }
                          }
                          else
                          {
                            this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot create new URL parameter");
                            continueParsing = false;
                          }
                        }
                        else
                        {
                          this->logger->Log(LOGGER_ERROR, L"%s: %s: only HTTP protocol supported in base URL: %s", PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, baseUrl);
                          continueParsing = false;
                        }
                      }
                      else
                      {
                        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot specify AFHS protocol");
                        continueParsing = false;
                      }
                      FREE_MEM(replacedUrl);
                    }
                  }
                  else
                  {
                    // this should not happen, just for sure
                    this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"no piece of media with highest bitrate");
                  }

                  if (!continueParsing)
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
                    this->connectionParameters->Remove(PARAMETER_NAME_AFHS_RECEIVE_DATA_TIMEOUT, true);
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
          else if (manifest->IsXml() && (manifest->GetParseError() != 0))
          {
            // we have XML declaration, it is valid XML file, just not complete
            this->logger->Log(LOGGER_WARNING, L"%s: %s: XML file probably not complete, XML parse error: %d", PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, manifest->GetParseError());
            result = ParseResult_Pending;
          }
        }
        FREE_MEM_CLASS(manifest);
      }
      FREE_MEM(buffer);
    }
  }

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitter_Parser_F4M::SetConnectionParameters(const CParameterCollection *parameters)
{
  if (parameters != NULL)
  {
    this->connectionParameters->Append((CParameterCollection *)parameters);
  }
  return S_OK;
}

Action CMPUrlSourceSplitter_Parser_F4M::GetAction(void)
{
  return Action_GetNewConnection;
}

HRESULT CMPUrlSourceSplitter_Parser_F4M::GetConnectionParameters(CParameterCollection *parameters)
{
  HRESULT result = (parameters == NULL) ? E_POINTER : S_OK;

  if (SUCCEEDED(result))
  {
    parameters->Append(this->connectionParameters);
  }

  return result;
}

CMediaPacketCollection *CMPUrlSourceSplitter_Parser_F4M::GetStoredMediaPackets(void)
{
  return this->storedMediaPackets;
}

// IPlugin interface

const wchar_t *CMPUrlSourceSplitter_Parser_F4M::GetName(void)
{
  return PARSER_NAME;
}

GUID CMPUrlSourceSplitter_Parser_F4M::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Parser_F4M::Initialize(PluginConfiguration *configuration)
{
  if (configuration == NULL)
  {
    return E_POINTER;
  }

  ParserPluginConfiguration *parserPluginConfiguration = (ParserPluginConfiguration *)configuration;

  this->connectionParameters->Clear();
  if (parserPluginConfiguration->configuration != NULL)
  {
    this->logger->SetParameters(configuration->configuration);
    this->connectionParameters->Append(parserPluginConfiguration->configuration);
  }
  this->connectionParameters->LogCollection(this->logger, LOGGER_VERBOSE, PARSER_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);

  return S_OK;
}

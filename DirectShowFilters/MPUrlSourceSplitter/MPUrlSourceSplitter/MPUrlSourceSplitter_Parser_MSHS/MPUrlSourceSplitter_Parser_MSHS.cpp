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

#include "MPUrlSourceSplitter_Parser_Mshs.h"
#include "ParserPluginConfiguration.h"
#include "StreamPackage.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackageDataResponse.h"
#include "StreamInformationCollection.h"
#include "MPUrlSourceSplitter_Protocol_Mshs_Parameters.h"
#include "MPUrlSourceSplitter_Protocol_Http_Parameters.h"
#include "Parameters.h"
#include "VersionInfo.h"
#include "ErrorCodes.h"
#include "formatUrl.h"
#include "base64.h"
#include "compress_zlib.h"

#pragma warning(pop)

// parser implementation name
#ifdef _DEBUG
#define PARSER_IMPLEMENTATION_NAME                                            L"MPUrlSourceSplitter_Parser_Mshsd"
#else
#define PARSER_IMPLEMENTATION_NAME                                            L"MPUrlSourceSplitter_Parser_Mshs"
#endif

// 32 KB of data to request at start
#define MP_URL_SOURCE_SPLITTER_PARSER_MSHS_DATA_LENGTH_DEFAULT                32768

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Parser_Mshs(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Parser_Mshs *parserPlugin = (CMPUrlSourceSplitter_Parser_Mshs *)plugin;

    delete parserPlugin;
  }
}

CMPUrlSourceSplitter_Parser_Mshs::CMPUrlSourceSplitter_Parser_Mshs(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CParserPlugin(result, logger, configuration)
{
  this->lastReceivedLength = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PARSER_MSHS, DATE_INFO_MP_URL_SOURCE_SPLITTER_PARSER_MSHS);
    if (version != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
    }
    FREE_MEM(version);

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CMPUrlSourceSplitter_Parser_Mshs::~CMPUrlSourceSplitter_Parser_Mshs()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));
}

// CParserPlugin

HRESULT CMPUrlSourceSplitter_Parser_Mshs::GetParserResult(void)
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
        unsigned int requestLength = MP_URL_SOURCE_SPLITTER_PARSER_MSHS_DATA_LENGTH_DEFAULT;
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

                  if (((buffer[0] == 0xFF) && (buffer[1] == 0xFE)) ||
                    ((buffer[1] == 0xFF) && (buffer[0] == 0xFE)))
                  {
                    // input is probably in UTF-16 (Unicode)
                    char *temp = ConvertUnicodeToUtf8((wchar_t *)(buffer + 2));
                    FREE_MEM(buffer);
                    buffer = (unsigned char *)temp;

                    length = (buffer != NULL) ? strlen(temp) : 0;
                  }

                  CMshsManifest *manifest = new CMshsManifest(&this->parserResult);
                  CHECK_POINTER_HRESULT(this->parserResult, manifest, this->parserResult, E_OUTOFMEMORY);

                  if (SUCCEEDED(this->parserResult))
                  {
                    if (manifest->Parse((char *)buffer))
                    {
                      this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, L"MSHS manifest");
                      this->parserResult = PARSER_RESULT_KNOWN;

                      wchar_t *mshsBuffer = ConvertUtf8ToUnicode((char *)buffer);
                      if (mshsBuffer != NULL)
                      {
                        this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, mshsBuffer);
                      }
                      FREE_MEM(mshsBuffer);

                      // pass also protected stream
                      // maybe protocol will sometime decrypt stream

                      // check for appropriate audio and video streams
                      CMshsManifestStreamBox *video = NULL;
                      CMshsManifestStreamBox *audio = NULL;

                      for (unsigned int i = 0; i < manifest->GetSmoothStreamingMedia()->GetStreams()->Count(); i++)
                      {
                        CMshsManifestStreamBox *stream = manifest->GetSmoothStreamingMedia()->GetStreams()->GetItem(i);

                        if (stream->IsVideo() && (video == NULL))
                        {
                          video = stream;
                        }

                        if (stream->IsAudio() && (audio == NULL))
                        {
                          audio = stream;
                        }
                      }

                      CHECK_CONDITION_HRESULT(this->parserResult, ((video != NULL) || (audio != NULL)), this->parserResult, E_MSHS_NO_VIDEO_OR_AUDIO_STREAM_PRESENT);

                      if (SUCCEEDED(this->parserResult))
                      {
                        // add base url parameter
                        wchar_t *baseUrl = GetBaseUrl(this->connectionParameters->GetValue(PARAMETER_NAME_URL, true, NULL));
                        CHECK_CONDITION_HRESULT(this->parserResult, baseUrl, this->parserResult, E_OUTOFMEMORY);

                        CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Add(PARAMETER_NAME_MSHS_BASE_URL, baseUrl), this->parserResult, E_OUTOFMEMORY);
                        FREE_MEM(baseUrl);
                      }

                      if (SUCCEEDED(this->parserResult))
                      {
                        // check tracks for supported video and audio tracks
                        unsigned int i = 0;

                        while (SUCCEEDED(this->parserResult) && (i < manifest->GetSmoothStreamingMedia()->GetStreams()->Count()))
                        {
                          CMshsManifestStreamBox *stream = manifest->GetSmoothStreamingMedia()->GetStreams()->GetItem(i);

                          if (stream->IsVideo())
                          {
                            unsigned int j = 0;
                            while (SUCCEEDED(this->parserResult) && (j < stream->GetTracks()->Count()))
                            {
                              bool supportedTrack = false;
                              CMshsManifestTrackBox *track = stream->GetTracks()->GetItem(j);

                              for (int k = 0; k < TOTAL_SUPPORTED_VIDEO_TRACKS; k++)
                              {
                                if (!IsNullOrEmptyOrWhitespace(track->GetFourCC()))
                                {
                                  if (wcscmp(track->GetFourCC(), SUPPORTED_VIDEO_TRACKS[k]) == 0)
                                  {
                                    supportedTrack = true;
                                    break;
                                  }
                                }
                              }

                              if (!supportedTrack)
                              {
                                stream->GetTracks()->Remove(j);
                              }
                              else
                              {
                                j++;
                              }
                            }

                            if (stream->GetTracks()->Count() == 0)
                            {
                              // remove stream which doesn't have any track
                              manifest->GetSmoothStreamingMedia()->GetStreams()->Remove(i);
                            }
                            else
                            {
                              i++;
                            }
                          }
                          else if (stream->IsAudio())
                          {
                            unsigned int j = 0;
                            while (SUCCEEDED(this->parserResult) && (j < stream->GetTracks()->Count()))
                            {
                              bool supportedTrack = false;
                              CMshsManifestTrackBox *track = stream->GetTracks()->GetItem(j);

                              for (int k = 0; k < TOTAL_SUPPORTED_AUDIO_TRACKS; k++)
                              {
                                if (!IsNullOrEmptyOrWhitespace(track->GetFourCC()))
                                {
                                  if (wcscmp(track->GetFourCC(), SUPPORTED_AUDIO_TRACKS[k]) == 0)
                                  {
                                    supportedTrack = true;
                                    break;
                                  }
                                }
                              }

                              if (!supportedTrack)
                              {
                                stream->GetTracks()->Remove(j);
                              }
                              else
                              {
                                j++;
                              }
                            }

                            if (stream->GetTracks()->Count() == 0)
                            {
                              // remove stream which doesn't have any track
                              manifest->GetSmoothStreamingMedia()->GetStreams()->Remove(i);
                            }
                            else
                            {
                              i++;
                            }
                          }
                          else
                          {
                            // not needed stream
                            manifest->GetSmoothStreamingMedia()->GetStreams()->Remove(i);
                          }
                        }

                        if (SUCCEEDED(this->parserResult))
                        {
                          // leave only video and audio tracks with highest bitrate
                          for (unsigned int i = 0; (SUCCEEDED(this->parserResult) && (i < manifest->GetSmoothStreamingMedia()->GetStreams()->Count())); i++)
                          {
                            CMshsManifestStreamBox *stream = manifest->GetSmoothStreamingMedia()->GetStreams()->GetItem(i);

                            uint32_t maxBitrate = 0;
                            unsigned int maxBitrateIndex = 0;
                            for (unsigned int j = 0; (SUCCEEDED(this->parserResult) && (j < stream->GetTracks()->Count())); j++)
                            {
                              CMshsManifestTrackBox *track = stream->GetTracks()->GetItem(j);
                              if (track->GetBitrate() > maxBitrate)
                              {
                                maxBitrate = track->GetBitrate();
                                maxBitrateIndex = j;
                              }
                            }

                            // remove everything except track with max bitrate
                            for (unsigned int j = 0; (SUCCEEDED(this->parserResult) && ((maxBitrateIndex + 1) < stream->GetTracks()->Count())); j++)
                            {
                              stream->GetTracks()->Remove(maxBitrateIndex + 1);
                            }
                            for (unsigned int j = 0; (SUCCEEDED(this->parserResult) && (j < maxBitrateIndex)); j++)
                            {
                              stream->GetTracks()->Remove(0);
                            }
                          }
                        }

                        bool containsVideoStream = false;
                        bool containsAudioStream = false;
                        for (unsigned int i = 0; i < manifest->GetSmoothStreamingMedia()->GetStreams()->Count(); i++)
                        {
                          CMshsManifestStreamBox *stream = manifest->GetSmoothStreamingMedia()->GetStreams()->GetItem(i);

                          containsVideoStream |= stream->IsVideo();
                          containsAudioStream |= stream->IsAudio();
                        }

                        CHECK_CONDITION_HRESULT(this->parserResult, (containsVideoStream && containsAudioStream), this->parserResult, E_MSHS_NO_VIDEO_OR_AUDIO_STREAM_PRESENT);
                      }

                      if (SUCCEEDED(this->parserResult))
                      {
                        unsigned int smoothStreamingMediaBoxSize = (unsigned int)manifest->GetSmoothStreamingMedia()->GetSize();
                        ALLOC_MEM_DEFINE_SET(smoothStreamingMediaBox, uint8_t, smoothStreamingMediaBoxSize, 0);
                        CHECK_POINTER_HRESULT(this->parserResult, smoothStreamingMediaBox, this->parserResult, E_OUTOFMEMORY);

                        CHECK_CONDITION_HRESULT(this->parserResult, manifest->GetSmoothStreamingMedia()->GetBox(smoothStreamingMediaBox, smoothStreamingMediaBoxSize), this->parserResult, E_OUTOFMEMORY);

                        if (SUCCEEDED(this->parserResult))
                        {
                          // compress
                          uint8_t *compressedSmoothStreamingMediaBox = NULL;
                          uint32_t compressedSize = 0;

                          this->parserResult = compress_zlib(smoothStreamingMediaBox, smoothStreamingMediaBoxSize, &compressedSmoothStreamingMediaBox, &compressedSize, -1);
                          CHECK_CONDITION_EXECUTE(SUCCEEDED(this->parserResult), this->parserResult = PARSER_RESULT_KNOWN);

                          if (SUCCEEDED(this->parserResult))
                          {
                            char *compressedSmoothStreamingMediaBoxBase64Encoded = NULL;
                            this->parserResult = base64_encode(compressedSmoothStreamingMediaBox, compressedSize, &compressedSmoothStreamingMediaBoxBase64Encoded);
                            CHECK_CONDITION_EXECUTE(SUCCEEDED(this->parserResult), this->parserResult = PARSER_RESULT_KNOWN);

                            if (SUCCEEDED(this->parserResult))
                            {
                              wchar_t *encoded = ConvertToUnicodeA(compressedSmoothStreamingMediaBoxBase64Encoded);
                              CHECK_POINTER_HRESULT(this->parserResult, encoded, this->parserResult, E_OUTOFMEMORY);

                              CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Add(PARAMETER_NAME_MSHS_MANIFEST, encoded), this->parserResult, E_OUTOFMEMORY);

                              FREE_MEM(encoded);
                            }

                            FREE_MEM(compressedSmoothStreamingMediaBoxBase64Encoded);
                          }
                        }

                        FREE_MEM(smoothStreamingMediaBox);
                      }

                      if (SUCCEEDED(this->parserResult))
                      {
                        wchar_t *replacedUrl = ReplaceString(this->connectionParameters->GetValue(PARAMETER_NAME_URL, true, NULL), L"http://", L"mshs://");
                        CHECK_POINTER_HRESULT(this->parserResult, replacedUrl, this->parserResult, E_OUTOFMEMORY);
                        CHECK_POINTER_HRESULT(this->parserResult, wcsstr(replacedUrl, L"mshs://"), this->parserResult, E_MSHS_ONLY_HTTP_PROTOCOL_SUPPORTED_IN_URL);


                        CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_COOKIE, true, PARAMETER_NAME_MSHS_COOKIE), this->parserResult, E_OUTOFMEMORY);
                        CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_IGNORE_CONTENT_LENGTH, true, PARAMETER_NAME_MSHS_IGNORE_CONTENT_LENGTH), this->parserResult, E_OUTOFMEMORY);
                        CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_OPEN_CONNECTION_TIMEOUT, true, PARAMETER_NAME_MSHS_OPEN_CONNECTION_TIMEOUT), this->parserResult, E_OUTOFMEMORY);
                        CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_OPEN_CONNECTION_SLEEP_TIME, true, PARAMETER_NAME_MSHS_OPEN_CONNECTION_SLEEP_TIME), this->parserResult, E_OUTOFMEMORY);
                        CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_TOTAL_REOPEN_CONNECTION_TIMEOUT, true, PARAMETER_NAME_MSHS_TOTAL_REOPEN_CONNECTION_TIMEOUT), this->parserResult, E_OUTOFMEMORY);
                        CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_REFERER, true, PARAMETER_NAME_MSHS_REFERER), this->parserResult, E_OUTOFMEMORY);
                        CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_USER_AGENT, true, PARAMETER_NAME_MSHS_USER_AGENT), this->parserResult, E_OUTOFMEMORY);
                        CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_VERSION, true, PARAMETER_NAME_MSHS_VERSION), this->parserResult, E_OUTOFMEMORY);

                        CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Update(PARAMETER_NAME_URL, true, replacedUrl), this->parserResult, E_OUTOFMEMORY);

                        if (SUCCEEDED(this->parserResult))
                        {
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

                          // copy current cookies parameters
                          if (SUCCEEDED(this->parserResult) && (usedCookies != NULL) && (usedCookies->Count() != 0))
                          {
                            // first add count of cookies
                            wchar_t *cookiesCountValue = FormatString(L"%u", usedCookies->Count());
                            CHECK_POINTER_HRESULT(this->parserResult, cookiesCountValue, this->parserResult, E_OUTOFMEMORY);

                            CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Update(PARAMETER_NAME_MSHS_COOKIES_COUNT, true, cookiesCountValue), this->parserResult, E_OUTOFMEMORY);
                            for (unsigned int i = 0; (SUCCEEDED(this->parserResult) && (i < usedCookies->Count())); i++)
                            {
                              CParameter *cookie = usedCookies->GetItem(i);

                              wchar_t *name = FormatString(MSHS_COOKIE_FORMAT_PARAMETER_NAME, i);
                              CHECK_POINTER_HRESULT(this->parserResult, name, this->parserResult, E_OUTOFMEMORY);

                              CHECK_CONDITION_HRESULT(this->parserResult, this->connectionParameters->Update(name, true, cookie->GetValue()), this->parserResult, E_OUTOFMEMORY);
                              FREE_MEM(name);
                            }

                            FREE_MEM(cookiesCountValue);
                          }

                          FREE_MEM_CLASS(usedCookies);
                        }
                      }
                    }
                    else if (manifest->IsXml() && (manifest->GetParseError() != 0))
                    {
                      // we have XML declaration, it is valid XML file, just not complete
                      this->logger->Log(LOGGER_WARNING, L"%s: %s: XML file probably not complete, XML parse error: %d", PARSER_IMPLEMENTATION_NAME, METHOD_GET_PARSER_RESULT_NAME, manifest->GetParseError());
                    }
                    else
                    {
                      // not MSHS manifest or XML file
                      this->parserResult = PARSER_RESULT_NOT_KNOWN;
                    }
                  }

                  FREE_MEM_CLASS(manifest);
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
      FREE_MEM_CLASS(streams);
    }
    else
    {
      // MSHS parser doesn't support multiple stream
      this->parserResult = PARSER_RESULT_NOT_KNOWN;
    }
  }

  return this->parserResult;
}

unsigned int CMPUrlSourceSplitter_Parser_Mshs::GetParserScore(void)
{
  return 100;
}

CParserPlugin::Action CMPUrlSourceSplitter_Parser_Mshs::GetAction(void)
{
  return GetNewConnection;
}

// CPlugin

const wchar_t *CMPUrlSourceSplitter_Parser_Mshs::GetName(void)
{
  return PARSER_NAME;
}

GUID CMPUrlSourceSplitter_Parser_Mshs::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Parser_Mshs::Initialize(CPluginConfiguration *configuration)
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

unsigned int CMPUrlSourceSplitter_Parser_Mshs::GetSeekingCapabilities(void)
{
  return this->protocolHoster->GetSeekingCapabilities();
}

int64_t CMPUrlSourceSplitter_Parser_Mshs::SeekToTime(unsigned int streamId, int64_t time)
{
  return this->protocolHoster->SeekToTime(streamId, time);
}

void CMPUrlSourceSplitter_Parser_Mshs::SetPauseSeekStopMode(unsigned int pauseSeekStopMode)
{
  this->protocolHoster->SetPauseSeekStopMode(pauseSeekStopMode);
}

// IDemuxerOwner interface

int64_t CMPUrlSourceSplitter_Parser_Mshs::GetDuration(void)
{
  return this->protocolHoster->GetDuration();
}

HRESULT CMPUrlSourceSplitter_Parser_Mshs::ProcessStreamPackage(CStreamPackage *streamPackage)
{
  return this->protocolHoster->ProcessStreamPackage(streamPackage);
}

// ISimpleProtocol interface

unsigned int CMPUrlSourceSplitter_Parser_Mshs::GetOpenConnectionTimeout(void)
{
  return this->protocolHoster->GetOpenConnectionTimeout();
}

unsigned int CMPUrlSourceSplitter_Parser_Mshs::GetOpenConnectionSleepTime(void)
{
  return this->protocolHoster->GetOpenConnectionSleepTime();
}

unsigned int CMPUrlSourceSplitter_Parser_Mshs::GetTotalReopenConnectionTimeout(void)
{
  return this->protocolHoster->GetTotalReopenConnectionTimeout();
}

HRESULT CMPUrlSourceSplitter_Parser_Mshs::StartReceivingData(CParameterCollection *parameters)
{
  return E_NOTIMPL;
}

HRESULT CMPUrlSourceSplitter_Parser_Mshs::StopReceivingData(void)
{
  return E_NOTIMPL;
}

HRESULT CMPUrlSourceSplitter_Parser_Mshs::QueryStreamProgress(CStreamProgress *streamProgress)
{
  return this->protocolHoster->QueryStreamProgress(streamProgress);
}
  
void CMPUrlSourceSplitter_Parser_Mshs::ClearSession(void)
{
  __super::ClearSession();

  this->lastReceivedLength = 0;
}

void CMPUrlSourceSplitter_Parser_Mshs::ReportStreamTime(uint64_t streamTime, uint64_t streamPosition)
{
  this->protocolHoster->ReportStreamTime(streamTime, streamPosition);
}

HRESULT CMPUrlSourceSplitter_Parser_Mshs::GetStreamInformation(CStreamInformationCollection *streams)
{
  return this->protocolHoster->GetStreamInformation(streams);
}

// IProtocol interface

ProtocolConnectionState CMPUrlSourceSplitter_Parser_Mshs::GetConnectionState(void)
{
  return None;
}

HRESULT CMPUrlSourceSplitter_Parser_Mshs::ParseUrl(const CParameterCollection *parameters)
{
  return E_NOTIMPL;
}

HRESULT CMPUrlSourceSplitter_Parser_Mshs::ReceiveData(CStreamPackage *streamPackage)
{
  return E_NOTIMPL;
}

//#pragma warning(push)
//// disable warning: 'INT8_MIN' : macro redefinition
//// warning is caused by stdint.h and intsafe.h, which both define same macro
//#pragma warning(disable:4005)
//
//
//#include "MPUrlSourceSplitter_Parser_MSHS.h"
//#include "VersionInfo.h"
//#include "formatUrl.h"
//#include "MSHSManifest.h"
//#include "MPUrlSourceSplitter_Protocol_Http_Parameters.h"
//#include "MPUrlSourceSplitter_Protocol_Mshs_Parameters.h"
//#include "Parameters.h"
//
//#include "base64.h"
//#include "compress_zlib.h"
//
//#include <stdio.h>
//
//#pragma warning(pop)
//
//// parser implementation name
//#ifdef _DEBUG
//#define PARSER_IMPLEMENTATION_NAME                                            L"MPUrlSourceSplitter_Parser_MSHSd"
//#else
//#define PARSER_IMPLEMENTATION_NAME                                            L"MPUrlSourceSplitter_Parser_MSHS"
//#endif
//
//PIPlugin CreatePluginInstance(CLogger *logger, CParameterCollection *configuration)
//{
//  return new CMPUrlSourceSplitter_Parser_MSHS(logger, configuration);
//}
//
//void DestroyPluginInstance(PIPlugin pProtocol)
//{
//  if (pProtocol != NULL)
//  {
//    CMPUrlSourceSplitter_Parser_MSHS *pClass = (CMPUrlSourceSplitter_Parser_MSHS *)pProtocol;
//    delete pClass;
//  }
//}
//
//CMPUrlSourceSplitter_Parser_MSHS::CMPUrlSourceSplitter_Parser_MSHS(CLogger *logger, CParameterCollection *configuration)
//{
//  this->connectionParameters = new CParameterCollection();
//  if (configuration != NULL)
//  {
//    this->connectionParameters->Append(configuration);
//  }
//
//  this->logger = new CLogger(logger);
//  this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);
//
//  wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PARSER_MSHS, DATE_INFO_MP_URL_SOURCE_SPLITTER_PARSER_MSHS);
//  if (version != NULL)
//  {
//    this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
//  }
//  FREE_MEM(version);
//
//  this->storedMediaPackets = new CMediaPacketCollection();
//
//  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
//}
//
//CMPUrlSourceSplitter_Parser_MSHS::~CMPUrlSourceSplitter_Parser_MSHS()
//{
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);
//
//  FREE_MEM_CLASS(this->connectionParameters);
//  FREE_MEM_CLASS(this->storedMediaPackets);
//
//  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);
//
//  FREE_MEM_CLASS(this->logger);
//}
//
//// IParser interface
//
//HRESULT CMPUrlSourceSplitter_Parser_MSHS::ClearSession(void)
//{
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
//
//  this->connectionParameters->Clear();
//  this->storedMediaPackets->Clear();
//
//  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
//  return S_OK;
//}
//
//ParseResult CMPUrlSourceSplitter_Parser_MSHS::ParseMediaPackets(unsigned int streamId, CMediaPacketCollection *mediaPackets, CParameterCollection *connectionParameters)
//{
//  ParseResult result = ParseResult_NotKnown;
//  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME);
//
//  if (mediaPackets != NULL)
//  {
//    if (this->storedMediaPackets->Append(mediaPackets))
//    {
//      unsigned int length = 0;
//      for (unsigned int i = 0; i < this->storedMediaPackets->Count(); i++)
//      {
//        CMediaPacket *mp = this->storedMediaPackets->GetItem(i);
//        length += mp->GetBuffer()->GetBufferOccupiedSpace();
//      }
//      length += 2;
//
//      ALLOC_MEM_DEFINE_SET(buffer, unsigned char, length, 0);
//      if ((buffer != NULL) && (length > 2))
//      {
//        unsigned int bufferPosition = 0;
//        for (unsigned int i = 0; i < this->storedMediaPackets->Count(); i++)
//        {
//          CMediaPacket *mp = this->storedMediaPackets->GetItem(i); 
//          unsigned int bufferOccupiedSpace = mp->GetBuffer()->GetBufferOccupiedSpace();
//          mp->GetBuffer()->CopyFromBuffer(buffer + bufferPosition, bufferOccupiedSpace);
//          bufferPosition += bufferOccupiedSpace;
//        }
//
//        if (((buffer[0] == 0xFF) && (buffer[1] == 0xFE)) ||
//            ((buffer[1] == 0xFF) && (buffer[0] == 0xFE)))
//        {
//          // input is probably in UTF-16 (Unicode)
//          char *temp = ConvertUnicodeToUtf8((wchar_t *)(buffer + 2));
//          FREE_MEM(buffer);
//          buffer = (unsigned char *)temp;
//
//          length = (buffer != NULL) ? strlen(temp) : 0;
//        }
//
//        CMSHSManifest *manifest = new CMSHSManifest();
//        if (manifest != NULL)
//        {
//          if (manifest->Parse((char *)buffer))
//          {
//            this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"MSHS manifest");
//            wchar_t *mshsBuffer = ConvertUtf8ToUnicode((char *)buffer);
//            if (mshsBuffer != NULL)
//            {
//              this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, mshsBuffer);
//            }
//            FREE_MEM(mshsBuffer);
//
//            if (!manifest->GetSmoothStreamingMedia()->IsProtected())
//            {
//              CMSHSStream *video = NULL;
//              CMSHSStream *audio = NULL;
//
//              for (unsigned int i = 0; i < manifest->GetSmoothStreamingMedia()->GetStreams()->Count(); i++)
//              {
//                CMSHSStream *stream = manifest->GetSmoothStreamingMedia()->GetStreams()->GetItem(i);
//
//                if (stream->IsVideo() && (video == NULL))
//                {
//                  video = stream;
//                }
//                
//                if (stream->IsAudio() && (audio == NULL))
//                {
//                  audio = stream;
//                }
//              }
//
//              bool continueParsing = ((video != NULL) || (audio != NULL));
//
//              if (continueParsing)
//              {
//                // add base url parameter
//                wchar_t *baseUrl = GetBaseUrl(this->connectionParameters->GetValue(PARAMETER_NAME_URL, true, NULL));
//                continueParsing &= (baseUrl != NULL);
//
//                if (continueParsing)
//                {
//                  CParameter *baseUrlParameter = new CParameter(PARAMETER_NAME_MSHS_BASE_URL, baseUrl);
//                  continueParsing &= (baseUrlParameter != NULL);
//
//                  if (continueParsing)
//                  {
//                    continueParsing &= this->connectionParameters->Add(baseUrlParameter);
//                  }
//
//                  if (!continueParsing)
//                  {
//                    FREE_MEM_CLASS(baseUrlParameter);
//                  }
//                }
//
//                FREE_MEM(baseUrl);
//              }
//
//              if (continueParsing)
//              {
//                // check tracks for supported video and audio tracks
//                unsigned int i = 0;
//
//                while (continueParsing && (i < manifest->GetSmoothStreamingMedia()->GetStreams()->Count()))
//                {
//                  CMSHSStream *stream = manifest->GetSmoothStreamingMedia()->GetStreams()->GetItem(i);
//
//                  if (stream->IsVideo())
//                  {
//                    unsigned int j = 0;
//                    while (continueParsing && (j < stream->GetTracks()->Count()))
//                    {
//                      bool supportedTrack = false;
//                      CMSHSTrack *track = stream->GetTracks()->GetItem(j);
//
//                      for (int k = 0; k < TOTAL_SUPPORTED_VIDEO_TRACKS; k++)
//                      {
//                        if (!IsNullOrEmptyOrWhitespace(track->GetFourCC()))
//                        {
//                          if (wcscmp(track->GetFourCC(), SUPPORTED_VIDEO_TRACKS[k]) == 0)
//                          {
//                            supportedTrack = true;
//                            break;
//                          }
//                        }
//                      }
//
//                      if (!supportedTrack)
//                      {
//                        stream->GetTracks()->Remove(j);
//                      }
//                      else
//                      {
//                        j++;
//                      }
//                    }
//
//                    if (stream->GetTracks()->Count() == 0)
//                    {
//                      // remove stream which doesn't have any track
//                      manifest->GetSmoothStreamingMedia()->GetStreams()->Remove(i);
//                    }
//                    else
//                    {
//                      i++;
//                    }
//                  }
//                  else if (stream->IsAudio())
//                  {
//                    unsigned int j = 0;
//                    while (continueParsing && (j < stream->GetTracks()->Count()))
//                    {
//                      bool supportedTrack = false;
//                      CMSHSTrack *track = stream->GetTracks()->GetItem(j);
//
//                      for (int k = 0; k < TOTAL_SUPPORTED_AUDIO_TRACKS; k++)
//                      {
//                        if (!IsNullOrEmptyOrWhitespace(track->GetFourCC()))
//                        {
//                          if (wcscmp(track->GetFourCC(), SUPPORTED_AUDIO_TRACKS[k]) == 0)
//                          {
//                            supportedTrack = true;
//                            break;
//                          }
//                        }
//                      }
//
//                      if (!supportedTrack)
//                      {
//                        stream->GetTracks()->Remove(j);
//                      }
//                      else
//                      {
//                        j++;
//                      }
//                    }
//
//                    if (stream->GetTracks()->Count() == 0)
//                    {
//                      // remove stream which doesn't have any track
//                      manifest->GetSmoothStreamingMedia()->GetStreams()->Remove(i);
//                    }
//                    else
//                    {
//                      i++;
//                    }
//                  }
//                  else
//                  {
//                    // not needed stream
//                    manifest->GetSmoothStreamingMedia()->GetStreams()->Remove(i);
//                  }
//                }
//
//                if (continueParsing)
//                {
//                  // leave only video and audio tracks with highest bitrate
//                  for (unsigned int i = 0; (continueParsing && (i < manifest->GetSmoothStreamingMedia()->GetStreams()->Count())); i++)
//                  {
//                    CMSHSStream *stream = manifest->GetSmoothStreamingMedia()->GetStreams()->GetItem(i);
//
//                    uint32_t maxBitrate = 0;
//                    unsigned int maxBitrateIndex = 0;
//                    for (unsigned int j = 0; (continueParsing && (j < stream->GetTracks()->Count())); j++)
//                    {
//                      CMSHSTrack *track = stream->GetTracks()->GetItem(j);
//                      if (track->GetBitrate() > maxBitrate)
//                      {
//                        maxBitrate = track->GetBitrate();
//                        maxBitrateIndex = j;
//                      }
//                    }
//
//                    // remove everything except track with max bitrate
//                    for (unsigned int j = 0; (continueParsing && ((maxBitrateIndex + 1) < stream->GetTracks()->Count())); j++)
//                    {
//                      stream->GetTracks()->Remove(maxBitrateIndex + 1);
//                    }
//                    for (unsigned int j = 0; (continueParsing && (j < maxBitrateIndex)); j++)
//                    {
//                      stream->GetTracks()->Remove(0);
//                    }
//                  }
//                }
//
//                bool containsVideoStream = false;
//                bool containsAudioStream = false;
//                for (unsigned int i = 0; i < manifest->GetSmoothStreamingMedia()->GetStreams()->Count(); i++)
//                {
//                  CMSHSStream *stream = manifest->GetSmoothStreamingMedia()->GetStreams()->GetItem(i);
//
//                  containsVideoStream |= stream->IsVideo();
//                  containsAudioStream |= stream->IsAudio();
//                }
//
//                continueParsing &= (containsVideoStream && containsAudioStream);
//              }
//
//              if (continueParsing)
//              {
//                uint32_t serializeSize = manifest->GetSmoothStreamingMedia()->GetSerializeSize();
//                ALLOC_MEM_DEFINE_SET(serializedManifest, uint8_t, serializeSize, 0);
//                continueParsing &= (serializedManifest != NULL);
//
//                if (continueParsing)
//                {
//                  continueParsing &= manifest->GetSmoothStreamingMedia()->Serialize(serializedManifest);
//
//                  if (!continueParsing)
//                  {
//                    this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot serialize manifest");
//                  }
//                }
//
//                if (continueParsing)
//                {
//                  // compress serialized manifest
//                  uint8_t *compressedManifest = NULL;
//                  uint32_t compressedLength = 0;
//
//                  HRESULT compressionResult = compress_zlib(serializedManifest, serializeSize, &compressedManifest, &compressedLength, -1);
//                  continueParsing &= SUCCEEDED(compressionResult);
//
//                  if (continueParsing)
//                  {
//                    char *compressedManifestBase64Encoded = NULL;
//                    continueParsing &= SUCCEEDED(base64_encode(compressedManifest, compressedLength, &compressedManifestBase64Encoded));
//
//                    if (continueParsing)
//                    {
//                      wchar_t *encoded = ConvertToUnicodeA(compressedManifestBase64Encoded);
//                      continueParsing &= (encoded != NULL);
//
//                      if (continueParsing)
//                      {
//                        CParameter *manifestParameter = new CParameter(PARAMETER_NAME_MSHS_MANIFEST, encoded);
//                        continueParsing &= (manifestParameter != NULL);
//
//                        if (continueParsing)
//                        {
//                          continueParsing &= this->connectionParameters->Add(manifestParameter);
//
//                          if (!continueParsing)
//                          {
//                            this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot add MSHS manifest parameter to connection parameters");
//                          }
//                        }
//                        else
//                        {
//                          this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot create MSHS manifest parameter");
//                        }
//
//                        if (!continueParsing)
//                        {
//                          FREE_MEM_CLASS(manifestParameter);
//                        }
//                      }
//                      else
//                      {
//                        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot convert encoded compressed manifest");
//                      }
//
//                      FREE_MEM(encoded);
//                    }
//                    else
//                    {
//                      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot encode compressed manifest");
//                    }
//                  }
//
//                  if (FAILED(result))
//                  {
//                    this->logger->Log(LOGGER_ERROR, L"%s: %s: manifest compression failed: 0x%08X", PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, compressionResult);
//                  }
//                  FREE_MEM(compressedManifest);
//                }
//
//                FREE_MEM(serializedManifest);
//              }
//
//              if (continueParsing)
//              {
//                if (continueParsing)
//                {
//                  wchar_t *replacedUrl = ReplaceString(this->connectionParameters->GetValue(PARAMETER_NAME_URL, true, NULL), L"http://", L"mshs://");
//                  if (replacedUrl != NULL)
//                  {
//                    if (wcsstr(replacedUrl, L"mshs://") != NULL)
//                    {
//                      CParameter *urlParameter = new CParameter(PARAMETER_NAME_URL, replacedUrl);
//                      if (urlParameter != NULL)
//                      {
//                        continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_COOKIE, true, PARAMETER_NAME_MSHS_COOKIE);
//                        continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_IGNORE_CONTENT_LENGTH, true, PARAMETER_NAME_MSHS_IGNORE_CONTENT_LENGTH);
//                        continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_RECEIVE_DATA_TIMEOUT, true, PARAMETER_NAME_MSHS_RECEIVE_DATA_TIMEOUT);
//                        continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_REFERER, true, PARAMETER_NAME_MSHS_REFERER);
//                        continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_USER_AGENT, true, PARAMETER_NAME_MSHS_USER_AGENT);
//                        continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_VERSION, true, PARAMETER_NAME_MSHS_VERSION);
//
//                        if (continueParsing)
//                        {
//                          this->connectionParameters->Remove(PARAMETER_NAME_URL, true);
//                          continueParsing &= this->connectionParameters->Add(urlParameter);
//                        }
//
//                        if (continueParsing)
//                        {
//                          result = ParseResult_Known;
//                        }
//                        else
//                        {
//                          this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot add new URL parameter into connection parameters");
//                        }
//                      }
//                      else
//                      {
//                        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot create new URL parameter");
//                        continueParsing = false;
//                      }
//                    }
//                    else
//                    {
//                      this->logger->Log(LOGGER_ERROR, L"%s: %s: only HTTP protocol supported in base URL: %s", PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, this->connectionParameters->GetValue(PARAMETER_NAME_URL, true, NULL));
//                      continueParsing = false;
//                    }
//                  }
//                  else
//                  {
//                    this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot specify MSHS protocol");
//                    continueParsing = false;
//                  }
//                  FREE_MEM(replacedUrl);
//                }
//
//
//                if (!continueParsing)
//                {
//                  // remove all MSHS parameters from connection parameters
//                  this->connectionParameters->Remove(PARAMETER_NAME_MSHS_BASE_URL, true);
//                  this->connectionParameters->Remove(PARAMETER_NAME_MSHS_MANIFEST, true);
//
//                  this->connectionParameters->Remove(PARAMETER_NAME_MSHS_COOKIE, true);
//                  this->connectionParameters->Remove(PARAMETER_NAME_MSHS_IGNORE_CONTENT_LENGTH, true);
//                  this->connectionParameters->Remove(PARAMETER_NAME_MSHS_RECEIVE_DATA_TIMEOUT, true);
//                  this->connectionParameters->Remove(PARAMETER_NAME_MSHS_REFERER, true);
//                  this->connectionParameters->Remove(PARAMETER_NAME_MSHS_USER_AGENT, true);
//                  this->connectionParameters->Remove(PARAMETER_NAME_MSHS_VERSION, true);
//                }
//              }
//              else
//              {
//                // media doesn't have any audio or video
//                this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"stream without video or audio");
//              }
//            }
//            else
//            {
//              // stream is protected, unsupported
//              this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"stream is protected, unsupported");
//              result = ParseResult_DrmProtected;
//            }
//          }
//          else if (manifest->IsXml() && (manifest->GetParseError() != 0))
//          {
//            // we have XML declaration, it is valid XML file, just not complete
//            this->logger->Log(LOGGER_WARNING, L"%s: %s: XML file probably not complete, XML parse error: %d", PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, manifest->GetParseError());
//            result = ParseResult_Pending;
//          }
//        }
//        FREE_MEM_CLASS(manifest);
//      }
//      FREE_MEM(buffer);
//    }
//  }
//
//  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, result);
//  return result;
//}
//
//HRESULT CMPUrlSourceSplitter_Parser_MSHS::SetConnectionParameters(const CParameterCollection *parameters)
//{
//  if (parameters != NULL)
//  {
//    this->connectionParameters->Append((CParameterCollection *)parameters);
//  }
//  return S_OK;
//}
//
//Action CMPUrlSourceSplitter_Parser_MSHS::GetAction(void)
//{
//  return Action_GetNewConnection;
//}
//
//HRESULT CMPUrlSourceSplitter_Parser_MSHS::GetConnectionParameters(CParameterCollection *parameters)
//{
//  HRESULT result = (parameters == NULL) ? E_POINTER : S_OK;
//
//  if (SUCCEEDED(result))
//  {
//    parameters->Append(this->connectionParameters);
//  }
//
//  return result;
//}
//
//// IPlugin interface
//
//const wchar_t *CMPUrlSourceSplitter_Parser_MSHS::GetName(void)
//{
//  return PARSER_NAME;
//}
//
//GUID CMPUrlSourceSplitter_Parser_MSHS::GetInstanceId(void)
//{
//  return this->logger->GetLoggerInstanceId();
//}
//
//HRESULT CMPUrlSourceSplitter_Parser_MSHS::Initialize(PluginConfiguration *configuration)
//{
//  if (configuration == NULL)
//  {
//    return E_POINTER;
//  }
//
//  ParserPluginConfiguration *parserPluginConfiguration = (ParserPluginConfiguration *)configuration;
//
//  this->connectionParameters->Clear();
//  if (parserPluginConfiguration->configuration != NULL)
//  {
//    this->logger->SetParameters(configuration->configuration);
//    this->connectionParameters->Append(parserPluginConfiguration->configuration);
//  }
//  this->connectionParameters->LogCollection(this->logger, LOGGER_VERBOSE, PARSER_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);
//
//  return S_OK;
//}

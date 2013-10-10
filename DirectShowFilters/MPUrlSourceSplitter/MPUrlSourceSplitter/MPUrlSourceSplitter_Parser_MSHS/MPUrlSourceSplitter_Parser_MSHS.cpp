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


#include "MPUrlSourceSplitter_Parser_MSHS.h"
#include "VersionInfo.h"
#include "formatUrl.h"
#include "MSHSManifest.h"
#include "MPUrlSourceSplitter_Protocol_Http_Parameters.h"
#include "MPUrlSourceSplitter_Protocol_Mshs_Parameters.h"
#include "Parameters.h"

#include "base64.h"
#include "compress_zlib.h"

#include <stdio.h>

#pragma warning(pop)

// parser implementation name
#ifdef _DEBUG
#define PARSER_IMPLEMENTATION_NAME                                            L"MPUrlSourceSplitter_Parser_MSHSd"
#else
#define PARSER_IMPLEMENTATION_NAME                                            L"MPUrlSourceSplitter_Parser_MSHS"
#endif

PIPlugin CreatePluginInstance(CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Parser_MSHS(logger, configuration);
}

void DestroyPluginInstance(PIPlugin pProtocol)
{
  if (pProtocol != NULL)
  {
    CMPUrlSourceSplitter_Parser_MSHS *pClass = (CMPUrlSourceSplitter_Parser_MSHS *)pProtocol;
    delete pClass;
  }
}

CMPUrlSourceSplitter_Parser_MSHS::CMPUrlSourceSplitter_Parser_MSHS(CLogger *logger, CParameterCollection *configuration)
{
  this->connectionParameters = new CParameterCollection();
  if (configuration != NULL)
  {
    this->connectionParameters->Append(configuration);
  }

  this->logger = new CLogger(logger);
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);

  wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PARSER_MSHS, DATE_INFO_MP_URL_SOURCE_SPLITTER_PARSER_MSHS);
  if (version != NULL)
  {
    this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
  }
  FREE_MEM(version);

  this->storedMediaPackets = new CMediaPacketCollection();

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
}

CMPUrlSourceSplitter_Parser_MSHS::~CMPUrlSourceSplitter_Parser_MSHS()
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  FREE_MEM_CLASS(this->connectionParameters);
  FREE_MEM_CLASS(this->storedMediaPackets);

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  FREE_MEM_CLASS(this->logger);
}

// IParser interface

HRESULT CMPUrlSourceSplitter_Parser_MSHS::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  this->connectionParameters->Clear();
  this->storedMediaPackets->Clear();

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
  return S_OK;
}

ParseResult CMPUrlSourceSplitter_Parser_MSHS::ParseMediaPackets(CMediaPacketCollection *mediaPackets, CParameterCollection *connectionParameters)
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
      if ((buffer != NULL) && (length > 2))
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

        CMSHSManifest *manifest = new CMSHSManifest();
        if (manifest != NULL)
        {
          if (manifest->Parse((char *)buffer))
          {
            this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"MSHS manifest");
            wchar_t *mshsBuffer = ConvertUtf8ToUnicode((char *)buffer);
            if (mshsBuffer != NULL)
            {
              this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, mshsBuffer);
            }
            FREE_MEM(mshsBuffer);

            if (!manifest->GetSmoothStreamingMedia()->IsProtected())
            {
              CMSHSStream *video = NULL;
              CMSHSStream *audio = NULL;

              for (unsigned int i = 0; i < manifest->GetSmoothStreamingMedia()->GetStreams()->Count(); i++)
              {
                CMSHSStream *stream = manifest->GetSmoothStreamingMedia()->GetStreams()->GetItem(i);

                if (stream->IsVideo() && (video == NULL))
                {
                  video = stream;
                }
                
                if (stream->IsAudio() && (audio == NULL))
                {
                  audio = stream;
                }
              }

              bool continueParsing = ((video != NULL) || (audio != NULL));

              if (continueParsing)
              {
                // add base url parameter
                wchar_t *baseUrl = GetBaseUrl(this->connectionParameters->GetValue(PARAMETER_NAME_URL, true, NULL));
                continueParsing &= (baseUrl != NULL);

                if (continueParsing)
                {
                  CParameter *baseUrlParameter = new CParameter(PARAMETER_NAME_MSHS_BASE_URL, baseUrl);
                  continueParsing &= (baseUrlParameter != NULL);

                  if (continueParsing)
                  {
                    continueParsing &= this->connectionParameters->Add(baseUrlParameter);
                  }

                  if (!continueParsing)
                  {
                    FREE_MEM_CLASS(baseUrlParameter);
                  }
                }

                FREE_MEM(baseUrl);
              }

              if (continueParsing)
              {
                // check tracks for supported video and audio tracks
                unsigned int i = 0;

                while (continueParsing && (i < manifest->GetSmoothStreamingMedia()->GetStreams()->Count()))
                {
                  CMSHSStream *stream = manifest->GetSmoothStreamingMedia()->GetStreams()->GetItem(i);

                  if (stream->IsVideo())
                  {
                    unsigned int j = 0;
                    while (continueParsing && (j < stream->GetTracks()->Count()))
                    {
                      bool supportedTrack = false;
                      CMSHSTrack *track = stream->GetTracks()->GetItem(j);

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
                    while (continueParsing && (j < stream->GetTracks()->Count()))
                    {
                      bool supportedTrack = false;
                      CMSHSTrack *track = stream->GetTracks()->GetItem(j);

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

                if (continueParsing)
                {
                  // leave only video and audio tracks with highest bitrate
                  for (unsigned int i = 0; (continueParsing && (i < manifest->GetSmoothStreamingMedia()->GetStreams()->Count())); i++)
                  {
                    CMSHSStream *stream = manifest->GetSmoothStreamingMedia()->GetStreams()->GetItem(i);

                    uint32_t maxBitrate = 0;
                    unsigned int maxBitrateIndex = 0;
                    for (unsigned int j = 0; (continueParsing && (j < stream->GetTracks()->Count())); j++)
                    {
                      CMSHSTrack *track = stream->GetTracks()->GetItem(j);
                      if (track->GetBitrate() > maxBitrate)
                      {
                        maxBitrate = track->GetBitrate();
                        maxBitrateIndex = j;
                      }
                    }

                    // remove everything except track with max bitrate
                    for (unsigned int j = 0; (continueParsing && ((maxBitrateIndex + 1) < stream->GetTracks()->Count())); j++)
                    {
                      stream->GetTracks()->Remove(maxBitrateIndex + 1);
                    }
                    for (unsigned int j = 0; (continueParsing && (j < maxBitrateIndex)); j++)
                    {
                      stream->GetTracks()->Remove(0);
                    }
                  }
                }

                bool containsVideoStream = false;
                bool containsAudioStream = false;
                for (unsigned int i = 0; i < manifest->GetSmoothStreamingMedia()->GetStreams()->Count(); i++)
                {
                  CMSHSStream *stream = manifest->GetSmoothStreamingMedia()->GetStreams()->GetItem(i);

                  containsVideoStream |= stream->IsVideo();
                  containsAudioStream |= stream->IsAudio();
                }

                continueParsing &= (containsVideoStream && containsAudioStream);
              }

              if (continueParsing)
              {
                uint32_t serializeSize = manifest->GetSmoothStreamingMedia()->GetSerializeSize();
                ALLOC_MEM_DEFINE_SET(serializedManifest, uint8_t, serializeSize, 0);
                continueParsing &= (serializedManifest != NULL);

                if (continueParsing)
                {
                  continueParsing &= manifest->GetSmoothStreamingMedia()->Serialize(serializedManifest);

                  if (!continueParsing)
                  {
                    this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot serialize manifest");
                  }
                }

                if (continueParsing)
                {
                  // compress serialized manifest
                  uint8_t *compressedManifest = NULL;
                  uint32_t compressedLength = 0;

                  HRESULT compressionResult = compress_zlib(serializedManifest, serializeSize, &compressedManifest, &compressedLength, -1);
                  continueParsing &= SUCCEEDED(compressionResult);

                  if (continueParsing)
                  {
                    char *compressedManifestBase64Encoded = NULL;
                    continueParsing &= SUCCEEDED(base64_encode(compressedManifest, compressedLength, &compressedManifestBase64Encoded));

                    if (continueParsing)
                    {
                      wchar_t *encoded = ConvertToUnicodeA(compressedManifestBase64Encoded);
                      continueParsing &= (encoded != NULL);

                      if (continueParsing)
                      {
                        CParameter *manifestParameter = new CParameter(PARAMETER_NAME_MSHS_MANIFEST, encoded);
                        continueParsing &= (manifestParameter != NULL);

                        if (continueParsing)
                        {
                          continueParsing &= this->connectionParameters->Add(manifestParameter);

                          if (!continueParsing)
                          {
                            this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot add MSHS manifest parameter to connection parameters");
                          }
                        }
                        else
                        {
                          this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot create MSHS manifest parameter");
                        }

                        if (!continueParsing)
                        {
                          FREE_MEM_CLASS(manifestParameter);
                        }
                      }
                      else
                      {
                        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot convert encoded compressed manifest");
                      }

                      FREE_MEM(encoded);
                    }
                    else
                    {
                      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot encode compressed manifest");
                    }
                  }

                  if (FAILED(result))
                  {
                    this->logger->Log(LOGGER_ERROR, L"%s: %s: manifest compression failed: 0x%08X", PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, compressionResult);
                  }
                  FREE_MEM(compressedManifest);
                }

                FREE_MEM(serializedManifest);
              }

              if (continueParsing)
              {
                if (continueParsing)
                {
                  wchar_t *replacedUrl = ReplaceString(this->connectionParameters->GetValue(PARAMETER_NAME_URL, true, NULL), L"http://", L"mshs://");
                  if (replacedUrl != NULL)
                  {
                    if (wcsstr(replacedUrl, L"mshs://") != NULL)
                    {
                      CParameter *urlParameter = new CParameter(PARAMETER_NAME_URL, replacedUrl);
                      if (urlParameter != NULL)
                      {
                        continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_COOKIE, true, PARAMETER_NAME_MSHS_COOKIE);
                        continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_IGNORE_CONTENT_LENGTH, true, PARAMETER_NAME_MSHS_IGNORE_CONTENT_LENGTH);
                        continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_RECEIVE_DATA_TIMEOUT, true, PARAMETER_NAME_MSHS_RECEIVE_DATA_TIMEOUT);
                        continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_REFERER, true, PARAMETER_NAME_MSHS_REFERER);
                        continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_USER_AGENT, true, PARAMETER_NAME_MSHS_USER_AGENT);
                        continueParsing &= this->connectionParameters->CopyParameter(PARAMETER_NAME_HTTP_VERSION, true, PARAMETER_NAME_MSHS_VERSION);

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
                      this->logger->Log(LOGGER_ERROR, L"%s: %s: only HTTP protocol supported in base URL: %s", PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, this->connectionParameters->GetValue(PARAMETER_NAME_URL, true, NULL));
                      continueParsing = false;
                    }
                  }
                  else
                  {
                    this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"cannot specify MSHS protocol");
                    continueParsing = false;
                  }
                  FREE_MEM(replacedUrl);
                }


                if (!continueParsing)
                {
                  // remove all MSHS parameters from connection parameters
                  this->connectionParameters->Remove(PARAMETER_NAME_MSHS_BASE_URL, true);
                  this->connectionParameters->Remove(PARAMETER_NAME_MSHS_MANIFEST, true);

                  this->connectionParameters->Remove(PARAMETER_NAME_MSHS_COOKIE, true);
                  this->connectionParameters->Remove(PARAMETER_NAME_MSHS_IGNORE_CONTENT_LENGTH, true);
                  this->connectionParameters->Remove(PARAMETER_NAME_MSHS_RECEIVE_DATA_TIMEOUT, true);
                  this->connectionParameters->Remove(PARAMETER_NAME_MSHS_REFERER, true);
                  this->connectionParameters->Remove(PARAMETER_NAME_MSHS_USER_AGENT, true);
                  this->connectionParameters->Remove(PARAMETER_NAME_MSHS_VERSION, true);
                }
              }
              else
              {
                // media doesn't have any audio or video
                this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"stream without video or audio");
              }
            }
            else
            {
              // stream is protected, unsupported
              this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PARSER_IMPLEMENTATION_NAME, METHOD_PARSE_MEDIA_PACKETS_NAME, L"stream is protected, unsupported");
              result = ParseResult_DrmProtected;
            }
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

HRESULT CMPUrlSourceSplitter_Parser_MSHS::SetConnectionParameters(const CParameterCollection *parameters)
{
  if (parameters != NULL)
  {
    this->connectionParameters->Append((CParameterCollection *)parameters);
  }
  return S_OK;
}

Action CMPUrlSourceSplitter_Parser_MSHS::GetAction(void)
{
  return Action_GetNewConnection;
}

HRESULT CMPUrlSourceSplitter_Parser_MSHS::GetConnectionParameters(CParameterCollection *parameters)
{
  HRESULT result = (parameters == NULL) ? E_POINTER : S_OK;

  if (SUCCEEDED(result))
  {
    parameters->Append(this->connectionParameters);
  }

  return result;
}

CMediaPacketCollection *CMPUrlSourceSplitter_Parser_MSHS::GetStoredMediaPackets(void)
{
  return this->storedMediaPackets;
}

// IPlugin interface

const wchar_t *CMPUrlSourceSplitter_Parser_MSHS::GetName(void)
{
  return PARSER_NAME;
}

GUID CMPUrlSourceSplitter_Parser_MSHS::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Parser_MSHS::Initialize(PluginConfiguration *configuration)
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

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

#include "MshsManifest.h"
#include "MshsManifestElements.h"

#include "conversions.h"
#include "tinyxml2.h"

CMshsManifest::CMshsManifest(HRESULT *result)
{
  this->isXml = false;
  this->parseError = XML_NO_ERROR;
  this->smoothStreamingMedia = NULL;
}

/* get methods */

CMshsManifest::~CMshsManifest(void)
{
  FREE_MEM_CLASS(this->smoothStreamingMedia);
}

int CMshsManifest::GetParseError(void)
{
  return this->parseError;
}

CMshsManifestSmoothStreamingMediaBox *CMshsManifest::GetSmoothStreamingMedia(void)
{
  return this->smoothStreamingMedia;
}

/* set methods */

/* other methods */

bool CMshsManifest::IsXml(void)
{
  return this->isXml;
}

bool CMshsManifest::Parse(const char *buffer)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, buffer);

  if (SUCCEEDED(result))
  {
    this->isXml = false;
    this->parseError = XML_NO_ERROR;

    FREE_MEM_CLASS(this->smoothStreamingMedia);
    this->smoothStreamingMedia = new CMshsManifestSmoothStreamingMediaBox(&result);

    CHECK_POINTER_HRESULT(result, this->smoothStreamingMedia, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      XMLDocument *document = new XMLDocument();
      CHECK_POINTER_HRESULT(result, document, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        // parse buffer, if no error, continue in parsing
        this->parseError = document->Parse(buffer);

        if (this->parseError == XML_NO_ERROR)
        {
          this->isXml = true;

          XMLElement *manifest = document->FirstChildElement(MSHS_ELEMENT_MANIFEST);
          CHECK_POINTER_HRESULT(result, manifest, result, E_OUTOFMEMORY);

          if (SUCCEEDED(manifest))
          {
            // correct MSHS manifest, continue in parsing

            // check manifest attributes
            const char *value = manifest->Attribute(MSHS_ELEMENT_MANIFEST_ATTRIBUTE_MAJOR_VERSION);
            if (value != NULL)
            {
              this->smoothStreamingMedia->SetMajorVersion(GetValueUnsignedIntA(value, MANIFEST_MAJOR_VERSION));
            }
            value = manifest->Attribute(MSHS_ELEMENT_MANIFEST_ATTRIBUTE_MINOR_VERSION);
            if (value != NULL)
            {
              this->smoothStreamingMedia->SetMinorVersion(GetValueUnsignedIntA(value, MANIFEST_MINOR_VERSION));
            }
            value = manifest->Attribute(MSHS_ELEMENT_MANIFEST_ATTRIBUTE_TIMESCALE);
            if (value != NULL)
            {
              this->smoothStreamingMedia->SetTimeScale(GetValueUnsignedInt64A(value, MANIFEST_TIMESCALE_DEFAULT));
            }
            value = manifest->Attribute(MSHS_ELEMENT_MANIFEST_ATTRIBUTE_DURATION);
            if (value != NULL)
            {
              this->smoothStreamingMedia->SetDuration(GetValueUnsignedInt64A(value, 0));
            }

            // it seems that major and minor version are filled be remote servers with different numbers as specification requested
            //CHECK_CONDITION_HRESULT(result, ((this->smoothStreamingMedia->GetMajorVersion() == MANIFEST_MAJOR_VERSION) && (this->smoothStreamingMedia->GetMinorVersion() == MANIFEST_MINOR_VERSION)), result, E_FAIL);

            if (SUCCEEDED(result))
            {
              XMLElement *child = manifest->FirstChildElement();

              if (child != NULL)
              {
                do
                {
                  if (strcmp(child->Name(), MSHS_ELEMENT_PROTECTION) == 0)
                  {
                    // protection element, parse it and add to protection collection
                    XMLElement *protectionHeader = child->FirstChildElement(MSHS_ELEMENT_PROTECTION_ELEMENT_PROTECTION_HEADER);
                    if (protectionHeader != NULL)
                    {
                      wchar_t *systemId = ConvertUtf8ToUnicode(protectionHeader->Attribute(MSHS_ELEMENT_PROTECTION_ELEMENT_PROTECTION_HEADER_ATTRIBUTE_SYSTEMID));
                      wchar_t *content = ConvertUtf8ToUnicode(protectionHeader->GetText());

                      CMshsManifestProtectionBox *protection = new CMshsManifestProtectionBox(&result);
                      CHECK_POINTER_HRESULT(result, protection, result, E_OUTOFMEMORY);

                      if (SUCCEEDED(result))
                      {
                        protection->SetSystemId(ConvertStringToGuid(systemId));
                        CHECK_CONDITION_HRESULT(result, protection->SetContent(content), result, E_OUTOFMEMORY);

                        CHECK_CONDITION_HRESULT(result, this->smoothStreamingMedia->GetProtections()->Add(protection), result, E_OUTOFMEMORY);
                      }

                      FREE_MEM(systemId);
                      FREE_MEM(content);

                      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(protection));
                    }
                  }

                  if (strcmp(child->Name(), MSHS_ELEMENT_STREAM) == 0)
                  {
                    // stream element

                    wchar_t *type = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_TYPE));
                    wchar_t *subType = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_SUBTYPE));;
                    wchar_t *url = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_URL));
                    wchar_t *timeScale = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_STREAM_TIMESCALE));
                    wchar_t *name = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_NAME));
                    wchar_t *numberOfFragments = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_NUMBER_OF_FRAGMENTS));
                    wchar_t *numberOfTracks = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_NUMBER_OF_TRACKS));
                    wchar_t *maxWidth = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_STREAM_MAX_WIDTH));
                    wchar_t *maxHeight = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_STREAM_MAX_HEIGHT));
                    wchar_t *displayWidth = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_DISPLAY_WIDTH));
                    wchar_t *displayHeight = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_DISPLAY_HEIGHT));

                    CMshsManifestStreamBox *stream = new CMshsManifestStreamBox(&result);
                    CHECK_POINTER_HRESULT(result, stream, result, E_OUTOFMEMORY);

                    if (SUCCEEDED(result))
                    {
                      stream->SetTimeScale(GetValueUnsignedInt64(timeScale, this->smoothStreamingMedia->GetTimeScale()));
                      stream->SetMaxWidth(GetValueUnsignedInt(maxWidth, 0));
                      stream->SetMaxHeight(GetValueUnsignedInt(maxHeight, 0));
                      stream->SetDisplayWidth(GetValueUnsignedInt(displayWidth, 0));
                      stream->SetDisplayHeight(GetValueUnsignedInt(displayHeight, 0));
                      CHECK_CONDITION_HRESULT(result, stream->SetType(type), result, E_OUTOFMEMORY);
                      CHECK_CONDITION_HRESULT(result, stream->SetSubType(subType), result, E_OUTOFMEMORY);
                      CHECK_CONDITION_HRESULT(result, stream->SetUrl(url), result, E_OUTOFMEMORY);
                      CHECK_CONDITION_HRESULT(result, stream->SetName(name), result, E_OUTOFMEMORY);
                    }

                    FREE_MEM(type);
                    FREE_MEM(subType);
                    FREE_MEM(url);
                    FREE_MEM(timeScale);
                    FREE_MEM(name);
                    FREE_MEM(numberOfFragments);
                    FREE_MEM(numberOfTracks);
                    FREE_MEM(maxWidth);
                    FREE_MEM(maxHeight);
                    FREE_MEM(displayWidth);
                    FREE_MEM(displayHeight);

                    XMLElement *streamChild = child->FirstChildElement();
                    if (SUCCEEDED(result) && (streamChild != NULL))
                    {
                      // go through all child nodes in stream to acquire tracks or stream fragments
                      uint32_t trackIndex = 0;
                      uint32_t streamFragmentNumber = 0;
                      uint64_t streamFragmentTime = 0;

                      do
                      {
                        if (strcmp(streamChild->Name(), MSHS_ELEMENT_STREAM_ELEMENT_TRACK) == 0)
                        {
                          // track, parse it and add to stream

                          wchar_t *index = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_INDEX));
                          wchar_t *bitrate = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_BITRATE));
                          wchar_t *maxWidth = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_MAX_WIDTH));
                          wchar_t *maxHeight = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_MAX_HEIGHT));
                          wchar_t *codecPrivateData = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_CODEC_PRIVATE_DATA));
                          wchar_t *samplingRate = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_SAMPLING_RATE));
                          wchar_t *channels = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_CHANNELS));
                          wchar_t *bitsPerSample = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_BITS_PER_SAMPLE));
                          wchar_t *packetSize = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_PACKET_SIZE));
                          wchar_t *audioTag = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_AUDIO_TAG));
                          wchar_t *fourCC = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_FOURCC));
                          wchar_t *nalUnitLengthField = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_NAL_UNIT_LENGTH_FIELD));

                          CMshsManifestTrackBox *track = new CMshsManifestTrackBox(&result);
                          CHECK_POINTER_HRESULT(result, track, result, E_OUTOFMEMORY);

                          if (SUCCEEDED(result))
                          {
                            track->SetIndex(GetValueUnsignedInt(index, trackIndex++));
                            track->SetBitrate(GetValueUnsignedInt(bitrate, 0));
                            track->SetMaxWidth(GetValueUnsignedInt(maxWidth, 0));
                            track->SetMaxHeight(GetValueUnsignedInt(maxHeight, 0));
                            track->SetSamplingRate(GetValueUnsignedInt(samplingRate, 0));
                            track->SetChannels(GetValueUnsignedInt(channels, 0));
                            track->SetBitsPerSample(GetValueUnsignedInt(bitsPerSample, 0));
                            track->SetPacketSize(GetValueUnsignedInt(packetSize, 0));
                            track->SetAudioTag(GetValueUnsignedInt(audioTag, 0));
                            track->SetNalUnitLengthField(GetValueUnsignedInt(nalUnitLengthField, MSHS_NAL_UNIT_LENGTH_DEFAULT));
                            CHECK_CONDITION_HRESULT(result, track->SetCodecPrivateData(codecPrivateData), result, E_OUTOFMEMORY);
                            CHECK_CONDITION_HRESULT(result, track->SetFourCC(fourCC), result, E_OUTOFMEMORY);
                          }

                          XMLElement *customAttributes = streamChild->FirstChildElement();
                          if (SUCCEEDED(result) && (customAttributes != NULL))
                          {
                            // go through all child nodes in track to acquire custom attributes
                            do
                            {
                              if (strcmp(customAttributes->Name(), MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ELEMENT_CUSTOM_ATTRIBUTES) == 0)
                              {
                                // custom attributes, it is collection, parse it and add to track
                                XMLElement *attribute = customAttributes->FirstChildElement();
                                do
                                {
                                  if (strcmp(attribute->Name(), MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ELEMENT_CUSTOM_ATTRIBUTES_ELEMENT_ATTRIBUTE) == 0)
                                  {
                                    // attribute element, parse it and add to track
                                    wchar_t *name = ConvertUtf8ToUnicode(attribute->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ELEMENT_CUSTOM_ATTRIBUTES_ELEMENT_ATTRIBUTE_ATTRIBUTE_NAME));
                                    wchar_t *value = ConvertUtf8ToUnicode(attribute->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ELEMENT_CUSTOM_ATTRIBUTES_ELEMENT_ATTRIBUTE_ATTRIBUTE_VALUE));

                                    CMshsManifestCustomAttributeBox *attr = new CMshsManifestCustomAttributeBox(&result);
                                    CHECK_POINTER_HRESULT(result, attr, result, E_OUTOFMEMORY);

                                    CHECK_CONDITION_HRESULT(result, attr->SetName(name), result, E_OUTOFMEMORY);
                                    CHECK_CONDITION_HRESULT(result, attr->SetValue(value), result, E_OUTOFMEMORY);

                                    FREE_MEM(name);
                                    FREE_MEM(value);

                                    CHECK_CONDITION_HRESULT(result, track->GetCustomAttributes()->Add(attr), result, E_OUTOFMEMORY);
                                    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(attr));
                                  }
                                }
                                while (SUCCEEDED(result) && ((attribute = attribute->NextSiblingElement()) != NULL));
                              }
                            }
                            while (SUCCEEDED(result) && ((customAttributes = customAttributes->NextSiblingElement()) != NULL));
                          }

                          FREE_MEM(index);
                          FREE_MEM(bitrate);
                          FREE_MEM(maxWidth);
                          FREE_MEM(maxHeight);
                          FREE_MEM(codecPrivateData);
                          FREE_MEM(samplingRate);
                          FREE_MEM(channels);
                          FREE_MEM(bitsPerSample);
                          FREE_MEM(packetSize);
                          FREE_MEM(audioTag);
                          FREE_MEM(fourCC);
                          FREE_MEM(nalUnitLengthField);

                          CHECK_CONDITION_HRESULT(result, stream->GetTracks()->Add(track), result, E_OUTOFMEMORY);
                          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(track));
                        }

                        if (strcmp(streamChild->Name(), MSHS_ELEMENT_STREAM_ELEMENT_STREAM_FRAGMENT) == 0)
                        {
                          // stream fragment, parse it and add to stream fragment collection
                          wchar_t *fragmentNumber = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_STREAM_FRAGMENT_ATTRIBUTE_FRAGMENT_NUMBER));
                          wchar_t *fragmentDuration = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_STREAM_FRAGMENT_ATTRIBUTE_FRAGMENT_DURATION));
                          wchar_t *fragmentTime = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_STREAM_FRAGMENT_ATTRIBUTE_FRAGMENT_TIME));

                          CMshsManifestStreamFragmentBox *streamFragment = new CMshsManifestStreamFragmentBox(&result);
                          CHECK_POINTER_HRESULT(result, streamFragment, result, E_OUTOFMEMORY);

                          if (SUCCEEDED(result))
                          {
                            streamFragment->SetFragmentNumber(GetValueUnsignedInt(fragmentNumber, streamFragmentNumber++));
                            streamFragment->SetFragmentDuration(GetValueUnsignedInt64(fragmentDuration, 0));
                            streamFragment->SetFragmentTime(GetValueUnsignedInt64(fragmentTime, streamFragmentTime));

                            streamFragmentTime = streamFragment->GetFragmentTime() + streamFragment->GetFragmentDuration();
                          }

                          FREE_MEM(fragmentNumber);
                          FREE_MEM(fragmentDuration);
                          FREE_MEM(fragmentTime);

                          CHECK_CONDITION_HRESULT(result, stream->GetStreamFragments()->Add(streamFragment), result, E_OUTOFMEMORY);
                          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(streamFragment));
                        }
                      }
                      while (SUCCEEDED(result) && ((streamChild = streamChild->NextSiblingElement()) != NULL));
                    }

                    CHECK_CONDITION_HRESULT(result, this->smoothStreamingMedia->GetStreams()->Add(stream), result, E_OUTOFMEMORY);
                    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(stream));
                  }
                }
                while (SUCCEEDED(result) && ((child = child->NextSiblingElement()) != NULL));
              }
            }
          }
        }
        else
        {
          XMLNode *child = document->FirstChild();
          if (child != NULL)
          {
            XMLDeclaration *declaration = child->ToDeclaration();
            if (declaration != NULL)
            {
              this->isXml = true;
            }
          }

          result = E_FAIL;
        }
      }

      FREE_MEM_CLASS(document);
    }

  //bool result = false;
  //bool continueParsing = ((this->smoothStreamingMedia != NULL) && (this->smoothStreamingMedia->GetProtections() != NULL));

  //if (continueParsing && (buffer != NULL))
  //{
  //  this->smoothStreamingMedia->GetProtections()->Clear();

  //  XMLDocument *document = new XMLDocument();

  //  if (document != NULL)
  //  {
  //    // parse buffer, if no error, continue in parsing
  //    this->parseError = document->Parse(buffer);
  //    if (this->parseError == XML_NO_ERROR)
  //    {
  //      this->isXml = true;

  //      XMLElement *manifest = document->FirstChildElement(MSHS_ELEMENT_MANIFEST);
  //      if (manifest != NULL)
  //      {
  //        // correct MSHS manifest, continue in parsing

  //        // check manifest attributes
  //        const char *value = manifest->Attribute(MSHS_ELEMENT_MANIFEST_ATTRIBUTE_MAJOR_VERSION);
  //        if (value != NULL)
  //        {
  //          this->smoothStreamingMedia->SetMajorVersion(GetValueUnsignedIntA(value, MANIFEST_MAJOR_VERSION));
  //        }
  //        value = manifest->Attribute(MSHS_ELEMENT_MANIFEST_ATTRIBUTE_MINOR_VERSION);
  //        if (value != NULL)
  //        {
  //          this->smoothStreamingMedia->SetMinorVersion(GetValueUnsignedIntA(value, MANIFEST_MINOR_VERSION));
  //        }
  //        value = manifest->Attribute(MSHS_ELEMENT_MANIFEST_ATTRIBUTE_TIMESCALE);
  //        if (value != NULL)
  //        {
  //          this->smoothStreamingMedia->SetTimeScale(GetValueUnsignedInt64A(value, MANIFEST_TIMESCALE_DEFAULT));
  //        }
  //        value = manifest->Attribute(MSHS_ELEMENT_MANIFEST_ATTRIBUTE_DURATION);
  //        if (value != NULL)
  //        {
  //          this->smoothStreamingMedia->SetDuration(GetValueUnsignedInt64A(value, 0));
  //        }

  //        // it seems that major and minor version are filled be remote servers with different numbers as specification requested
  //        //continueParsing &= ((this->smoothStreamingMedia->GetMajorVersion() == MANIFEST_MAJOR_VERSION) && (this->smoothStreamingMedia->GetMinorVersion() == MANIFEST_MINOR_VERSION));

  //        if (continueParsing)
  //        {
  //          XMLElement *child = manifest->FirstChildElement();
  //          if (child != NULL)
  //          {
  //            do
  //            {
  //              if (strcmp(child->Name(), MSHS_ELEMENT_PROTECTION) == 0)
  //              {
  //                // protection element, parse it and add to protection collection
  //                XMLElement *protectionHeader = child->FirstChildElement(MSHS_ELEMENT_PROTECTION_ELEMENT_PROTECTION_HEADER);
  //                if (protectionHeader != NULL)
  //                {
  //                  wchar_t *systemId = ConvertUtf8ToUnicode(protectionHeader->Attribute(MSHS_ELEMENT_PROTECTION_ELEMENT_PROTECTION_HEADER_ATTRIBUTE_SYSTEMID));
  //                  wchar_t *content = ConvertUtf8ToUnicode(protectionHeader->GetText());

  //                  CMSHSProtection *protection = new CMSHSProtection();
  //                  continueParsing &= (protection != NULL);

  //                  if (continueParsing)
  //                  {
  //                    protection->SetSystemId(ConvertStringToGuid(systemId));
  //                    continueParsing &= protection->SetContent(content);

  //                    if (continueParsing)
  //                    {
  //                      continueParsing &= this->smoothStreamingMedia->GetProtections()->Add(protection);
  //                    }
  //                  }

  //                  FREE_MEM(systemId);
  //                  FREE_MEM(content);

  //                  if (!continueParsing)
  //                  {
  //                    FREE_MEM_CLASS(protection);
  //                  }
  //                }
  //              }

  //              if (strcmp(child->Name(), MSHS_ELEMENT_STREAM) == 0)
  //              {
  //                // stream element

  //                wchar_t *type = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_TYPE));
  //                wchar_t *subType = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_SUBTYPE));;
  //                wchar_t *url = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_URL));
  //                wchar_t *timeScale = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_STREAM_TIMESCALE));
  //                wchar_t *name = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_NAME));
  //                wchar_t *numberOfFragments = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_NUMBER_OF_FRAGMENTS));
  //                wchar_t *numberOfTracks = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_NUMBER_OF_TRACKS));
  //                wchar_t *maxWidth = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_STREAM_MAX_WIDTH));
  //                wchar_t *maxHeight = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_STREAM_MAX_HEIGHT));
  //                wchar_t *displayWidth = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_DISPLAY_WIDTH));
  //                wchar_t *displayHeight = ConvertUtf8ToUnicode(child->Attribute(MSHS_ELEMENT_STREAM_ATTRIBUTE_DISPLAY_HEIGHT));

  //                CMSHSStream *stream = new CMSHSStream();
  //                continueParsing &= (stream != NULL);

  //                if (continueParsing)
  //                {
  //                  continueParsing &= stream->SetType(type);
  //                  continueParsing &= stream->SetSubType(subType);
  //                  continueParsing &= stream->SetUrl(url);
  //                  stream->SetTimeScale(GetValueUnsignedInt64(timeScale, this->smoothStreamingMedia->GetTimeScale()));
  //                  continueParsing &= stream->SetName(name);
  //                  stream->SetMaxWidth(GetValueUnsignedInt(maxWidth, 0));
  //                  stream->SetMaxHeight(GetValueUnsignedInt(maxHeight, 0));
  //                  stream->SetDisplayWidth(GetValueUnsignedInt(displayWidth, 0));
  //                  stream->SetDisplayHeight(GetValueUnsignedInt(displayHeight, 0));

  //                  if (continueParsing)
  //                  {
  //                    continueParsing &= this->smoothStreamingMedia->GetStreams()->Add(stream);
  //                  }
  //                }

  //                FREE_MEM(type);
  //                FREE_MEM(subType);
  //                FREE_MEM(url);
  //                FREE_MEM(timeScale);
  //                FREE_MEM(name);
  //                FREE_MEM(numberOfFragments);
  //                FREE_MEM(numberOfTracks);
  //                FREE_MEM(maxWidth);
  //                FREE_MEM(maxHeight);
  //                FREE_MEM(displayWidth);
  //                FREE_MEM(displayHeight);

  //                XMLElement *streamChild = child->FirstChildElement();
  //                if (continueParsing && (streamChild != NULL))
  //                {
  //                  // go through all child nodes in stream to acquire tracks or stream fragments
  //                  uint32_t trackIndex = 0;
  //                  uint32_t streamFragmentNumber = 0;
  //                  uint64_t streamFragmentTime = 0;
  //                  do
  //                  {
  //                    if (strcmp(streamChild->Name(), MSHS_ELEMENT_STREAM_ELEMENT_TRACK) == 0)
  //                    {
  //                      // track, parse it and add to stream

  //                      wchar_t *index = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_INDEX));
  //                      wchar_t *bitrate = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_BITRATE));
  //                      wchar_t *maxWidth = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_MAX_WIDTH));
  //                      wchar_t *maxHeight = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_MAX_HEIGHT));
  //                      wchar_t *codecPrivateData = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_CODEC_PRIVATE_DATA));
  //                      wchar_t *samplingRate = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_SAMPLING_RATE));
  //                      wchar_t *channels = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_CHANNELS));
  //                      wchar_t *bitsPerSample = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_BITS_PER_SAMPLE));
  //                      wchar_t *packetSize = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_PACKET_SIZE));
  //                      wchar_t *audioTag = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_AUDIO_TAG));
  //                      wchar_t *fourCC = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_FOURCC));
  //                      wchar_t *nalUnitLengthField = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_NAL_UNIT_LENGTH_FIELD));

  //                      CMSHSTrack *track = new CMSHSTrack();
  //                      continueParsing &= (track != NULL);

  //                      if (continueParsing)
  //                      {
  //                        track->SetIndex(GetValueUnsignedInt(index, trackIndex++));
  //                        track->SetBitrate(GetValueUnsignedInt(bitrate, 0));
  //                        track->SetMaxWidth(GetValueUnsignedInt(maxWidth, 0));
  //                        track->SetMaxHeight(GetValueUnsignedInt(maxHeight, 0));
  //                        continueParsing &= track->SetCodecPrivateData(codecPrivateData);
  //                        track->SetSamplingRate(GetValueUnsignedInt(samplingRate, 0));
  //                        track->SetChannels(GetValueUnsignedInt(channels, 0));
  //                        track->SetBitsPerSample(GetValueUnsignedInt(bitsPerSample, 0));
  //                        track->SetPacketSize(GetValueUnsignedInt(packetSize, 0));
  //                        track->SetAudioTag(GetValueUnsignedInt(audioTag, 0));
  //                        continueParsing &= track->SetFourCC(fourCC);
  //                        track->SetNalUnitLengthField(GetValueUnsignedInt(nalUnitLengthField, MSHS_NAL_UNIT_LENGTH_DEFAULT));

  //                        if (continueParsing)
  //                        {
  //                          continueParsing &= stream->GetTracks()->Add(track);
  //                        }
  //                      }

  //                      XMLElement *customAttributes = streamChild->FirstChildElement();
  //                      if (continueParsing && (customAttributes != NULL))
  //                      {
  //                        // go through all child nodes in track to acquire custom attributes
  //                        do
  //                        {
  //                          if (strcmp(customAttributes->Name(), MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ELEMENT_CUSTOM_ATTRIBUTES) == 0)
  //                          {
  //                            // custom attributes, it is collection, parse it and add to track
  //                            XMLElement *attribute = customAttributes->FirstChildElement();
  //                            do
  //                            {
  //                              if (strcmp(attribute->Name(), MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ELEMENT_CUSTOM_ATTRIBUTES_ELEMENT_ATTRIBUTE) == 0)
  //                              {
  //                                // attribute element, parse it and add to track
  //                                wchar_t *name = ConvertUtf8ToUnicode(attribute->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ELEMENT_CUSTOM_ATTRIBUTES_ELEMENT_ATTRIBUTE_ATTRIBUTE_NAME));
  //                                wchar_t *value = ConvertUtf8ToUnicode(attribute->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ELEMENT_CUSTOM_ATTRIBUTES_ELEMENT_ATTRIBUTE_ATTRIBUTE_VALUE));

  //                                CMSHSCustomAttribute *attr = new CMSHSCustomAttribute();
  //                                continueParsing &= (attr != NULL);

  //                                if (continueParsing)
  //                                {
  //                                  continueParsing &= attr->SetName(name);
  //                                  continueParsing &= attr->SetValue(value);

  //                                  if (continueParsing)
  //                                  {
  //                                    continueParsing &= track->GetCustomAttributes()->Add(attr);
  //                                  }
  //                                }

  //                                FREE_MEM(name);
  //                                FREE_MEM(value);

  //                                if (!continueParsing)
  //                                {
  //                                  FREE_MEM_CLASS(attr);
  //                                }
  //                              }
  //                            }
  //                            while (continueParsing && ((attribute = attribute->NextSiblingElement()) != NULL));
  //                          }
  //                        }
  //                        while (continueParsing && ((customAttributes = customAttributes->NextSiblingElement()) != NULL));
  //                      }

  //                      FREE_MEM(index);
  //                      FREE_MEM(bitrate);
  //                      FREE_MEM(maxWidth);
  //                      FREE_MEM(maxHeight);
  //                      FREE_MEM(codecPrivateData);
  //                      FREE_MEM(samplingRate);
  //                      FREE_MEM(channels);
  //                      FREE_MEM(bitsPerSample);
  //                      FREE_MEM(packetSize);
  //                      FREE_MEM(audioTag);
  //                      FREE_MEM(fourCC);
  //                      FREE_MEM(nalUnitLengthField);

  //                      if (!continueParsing)
  //                      {
  //                        FREE_MEM_CLASS(track);
  //                      }
  //                    }

  //                    if (strcmp(streamChild->Name(), MSHS_ELEMENT_STREAM_ELEMENT_STREAM_FRAGMENT) == 0)
  //                    {
  //                      // stream fragment, parse it and add to stream fragment collection
  //                      wchar_t *fragmentNumber = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_STREAM_FRAGMENT_ATTRIBUTE_FRAGMENT_NUMBER));
  //                      wchar_t *fragmentDuration = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_STREAM_FRAGMENT_ATTRIBUTE_FRAGMENT_DURATION));
  //                      wchar_t *fragmentTime = ConvertUtf8ToUnicode(streamChild->Attribute(MSHS_ELEMENT_STREAM_ELEMENT_STREAM_FRAGMENT_ATTRIBUTE_FRAGMENT_TIME));

  //                      CMSHSStreamFragment *streamFragment = new CMSHSStreamFragment();
  //                      continueParsing &= (streamFragment != NULL);

  //                      if (continueParsing)
  //                      {
  //                        streamFragment->SetFragmentNumber(GetValueUnsignedInt(fragmentNumber, streamFragmentNumber++));
  //                        streamFragment->SetFragmentDuration(GetValueUnsignedInt64(fragmentDuration, 0));
  //                        streamFragment->SetFragmentTime(GetValueUnsignedInt64(fragmentTime, streamFragmentTime));

  //                        streamFragmentTime = streamFragment->GetFragmentTime() + streamFragment->GetFragmentDuration();

  //                        if (continueParsing)
  //                        {
  //                          continueParsing &= stream->GetStreamFragments()->Add(streamFragment);
  //                        }
  //                      }

  //                      FREE_MEM(fragmentNumber);
  //                      FREE_MEM(fragmentDuration);
  //                      FREE_MEM(fragmentTime);

  //                      if (!continueParsing)
  //                      {
  //                        FREE_MEM_CLASS(streamFragment);
  //                      }
  //                    }
  //                  }
  //                  while (continueParsing && ((streamChild = streamChild->NextSiblingElement()) != NULL));
  //                }

  //                if (!continueParsing)
  //                {
  //                  FREE_MEM_CLASS(stream);
  //                }
  //              }
  //            }
  //            while (continueParsing && ((child = child->NextSiblingElement()) != NULL));
  //          }
  //        }
  //      }

  //      result = continueParsing;
  //    }
  //    else
  //    {
  //      XMLNode *child = document->FirstChild();
  //      if (child != NULL)
  //      {
  //        XMLDeclaration *declaration = child->ToDeclaration();
  //        if (declaration != NULL)
  //        {
  //          this->isXml = true;
  //        }
  //      }
  //    }
  //  }

  //  FREE_MEM_CLASS(document);
  //}
  }

  return SUCCEEDED(result);
}
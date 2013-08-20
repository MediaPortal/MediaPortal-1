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

#pragma once

#ifndef __MSHS_ELEMENTS_DEFINED
#define __MSHS_ELEMENTS_DEFINED

// The root element in the document is <SmoothStreamingMedia>.
#define MSHS_ELEMENT_MANIFEST                                                 "SmoothStreamingMedia"

#define MSHS_ELEMENT_MANIFEST_ATTRIBUTE_MAJOR_VERSION                         "MajorVersion"
#define MSHS_ELEMENT_MANIFEST_ATTRIBUTE_MINOR_VERSION                         "MinorVersion"
#define MSHS_ELEMENT_MANIFEST_ATTRIBUTE_TIMESCALE                             "TimeScale"
#define MSHS_ELEMENT_MANIFEST_ATTRIBUTE_DURATION                              "Duration"

#define MSHS_ELEMENT_PROTECTION                                               "Protection"
#define MSHS_ELEMENT_PROTECTION_ELEMENT_PROTECTION_HEADER                     "ProtectionHeader"

#define MSHS_ELEMENT_PROTECTION_ELEMENT_PROTECTION_HEADER_ATTRIBUTE_SYSTEMID  "SystemID"

#define MSHS_ELEMENT_STREAM                                                   "StreamIndex"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_TYPE                                    "Type"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_TYPE_VALUE_VIDEO                        "video"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_TYPE_VALUE_AUDIO                        "audio"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_TYPE_VALUE_TEXT                         "text"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_TYPE_VALUE_VIDEOW                       L"video"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_TYPE_VALUE_AUDIOW                       L"audio"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_TYPE_VALUE_TEXTW                        L"text"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_SUBTYPE                                 "Subtype"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_STREAM_TIMESCALE                        "TimeScale"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_NAME                                    "Name"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_NUMBER_OF_FRAGMENTS                     "Chunks"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_NUMBER_OF_TRACKS                        "QualityLevels"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_URL                                     "Url"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_STREAM_MAX_WIDTH                        "MaxWidth"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_STREAM_MAX_HEIGHT                       "MaxHeight"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_DISPLAY_WIDTH                           "DisplayWidth"
#define MSHS_ELEMENT_STREAM_ATTRIBUTE_DISPLAY_HEIGHT                          "DisplayHeight"

#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK                                     "QualityLevel"
#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_INDEX                     "Index"
#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_BITRATE                   "Bitrate"
#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_MAX_WIDTH                 "MaxWidth"
#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_MAX_HEIGHT                "MaxHeight"
#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_CODEC_PRIVATE_DATA        "CodecPrivateData"
#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_SAMPLING_RATE             "SamplingRate"
#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_CHANNELS                  "Channels"
#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_BITS_PER_SAMPLE           "BitsPerSample"
#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_PACKET_SIZE               "PacketSize"
#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_AUDIO_TAG                 "AudioTag"
#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_FOURCC                    "FourCC"
#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ATTRIBUTE_NAL_UNIT_LENGTH_FIELD     "NALUnitLengthField"

#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ELEMENT_CUSTOM_ATTRIBUTES                                       "CustomAttributes"
#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ELEMENT_CUSTOM_ATTRIBUTES_ELEMENT_ATTRIBUTE                     "Attribute"
#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ELEMENT_CUSTOM_ATTRIBUTES_ELEMENT_ATTRIBUTE_ATTRIBUTE_NAME      "Name"
#define MSHS_ELEMENT_STREAM_ELEMENT_TRACK_ELEMENT_CUSTOM_ATTRIBUTES_ELEMENT_ATTRIBUTE_ATTRIBUTE_VALUE     "Value"

#define MSHS_ELEMENT_STREAM_ELEMENT_STREAM_FRAGMENT                                                       "c"
#define MSHS_ELEMENT_STREAM_ELEMENT_STREAM_FRAGMENT_ATTRIBUTE_FRAGMENT_NUMBER                             "n"
#define MSHS_ELEMENT_STREAM_ELEMENT_STREAM_FRAGMENT_ATTRIBUTE_FRAGMENT_DURATION                           "d"
#define MSHS_ELEMENT_STREAM_ELEMENT_STREAM_FRAGMENT_ATTRIBUTE_FRAGMENT_TIME                               "t"

#endif
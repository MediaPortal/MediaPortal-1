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

#ifndef __F4M_ELEMENTS_DEFINED
#define __F4M_ELEMENTS_DEFINED

// The root element in the document is <manifest>.
#define F4M_ELEMENT_MANIFEST                                      "manifest"
#define F4M_ELEMENT_MANIFEST_ATTRIBUTE_XMLNS                      "xmlns"
#define F4M_ELEMENT_MANIFEST_ATTRIBUTE_XMLNS_VALUE                "http://ns.adobe.com/f4m/1.0"

// The <streamType> element is a string representing the way in which the media is streamed. Valid values include "live",
// "recorded", and "liveOrRecorded". It is assumed that all representations of the media have the same stream type, hence
// its placement under the document root. It is optional.
#define F4M_ELEMENT_STREAMTYPE                                    "streamType"

// The <deliveryType> element indicates the means by which content is delivered to the player. Valid values include "streaming"
// and "progressive". It is optional. If unspecified, then the delivery type is inferred from the media protocol. For media with
// an RTMP protocol, the default deliveryType is "streaming". For media with an HTTP protocol, the default deliveryType is
// also "streaming". In the latter case, the <bootstrapInfo> field must be present.
#define F4M_ELEMENT_DELIVERYTYPE                                  "deliveryType"
#define F4M_ELEMENT_DELIVERYTYPE_VALUE_STREAMING                  "streaming"
#define F4M_ELEMENT_DELIVERYTYPE_VALUE_PROGRESSIVE                "progressive"
#define F4M_ELEMENT_DELIVERYTYPE_VALUE_STREAMINGW                 L"streaming"
#define F4M_ELEMENT_DELIVERYTYPE_VALUE_PROGRESSIVEW               L"progressive"

// The <baseURL> element contains the base URL for all relative (HTTP-based) URLs in the manifest. It is optional. When
// specified, its value is prepended to all relative URLs (i.e. those URLs that don't begin with "http://" or "https://"
// within the manifest file. (Such URLs may include <media> URLs, <bootstrapInfo> URLs, and <drmAdditionalHeader> URLs.)
#define F4M_ELEMENT_BASEURL                                       "baseURL"

// The <duration> element represents the duration of the media, in seconds. It is assumed that all representations of the media
// have the same duration, hence its placement under the document root. It is optional. For live or DVR content, the duration
// represents the total expected time of the media, not the current duration of the media.
#define F4M_ELEMENT_DURATION                                      "duration"

// The <drmAdditionalHeader> element represents the DRM AdditionalHeader needed for DRM authentication. It contains either
// a BASE64 encoded representation of, or a URL to, the DRM AdditionalHeader (including the serialized
// "|AdditionalHeader" string). It is optional.
#define F4M_ELEMENT_DRMADDITIONALHEADER                           "drmAdditionalHeader"

// The ID of this <drmAdditionalHeader> element. It is optional. If it is not specified, then this header will apply to
// all <media> elements that don't have a drmAdditionalHeaderId property. If it is specified, then this header will apply only
// to those <media> elements that use the same ID in their drmAdditionalHeaderId property.
#define F4M_ELEMENT_DRMADDITIONALHEADER_ATTRIBUTE_ID              "id"

// A URL to a file containing the raw DRM AdditionalHeader. Either the url attribute or the inline BASE64 header (but not both)
// must be specified. If a specified URL is non-absolute, then it must be relative to the manifest file itself.
#define F4M_ELEMENT_DRMADDITIONALHEADER_ATTRIBUTE_URL             "url"

// The <bootstrapInfo> element represents all information needed to bootstrap playback of HTTP streamed media. It contains
// either a BASE64 encoded representation of, or a URL to, the bootstrap information in the format that corresponds to
// the bootstrap profile. It is optional.
#define F4M_ELEMENT_BOOTSTRAPINFO                                 "bootstrapInfo"

// The ID of this <bootstrapInfo> element. It is optional. If it is not specified, then this bootstrapping block will apply to
// all <media> elements that don't have a bootstrapInfoId property. If it is specified, then this bootstrapping block will apply
// only to those <media> elements that use the same ID in their bootstrapInfoId property.
#define F4M_ELEMENT_BOOTSTRAPINFO_ATTRIBUTE_ID                    "id"

// The profile, or type of bootstrapping represented by this element. For the Named Access profile, use "named". For other
// bootstrapping profiles, use some other string (i.e. the field is extensible). It is required.
#define F4M_ELEMENT_BOOTSTRAPINFO_ATTRIBUTE_PROFILE               "profile"
#define F4M_ELEMENT_BOOTSTRAPINFO_ATTRIBUTE_PROFILE_VALUE_NAMED   "named"
#define F4M_ELEMENT_BOOTSTRAPINFO_ATTRIBUTE_PROFILE_VALUE_NAMEDW  L"named"

// A URL to a file containing the raw bootstrap info. Either the url attribute or the inline BASE64 bootstrap info
// (but not both) must be specified. If a specified URL is non-absolute, then it must be relative to the manifest file itself.
#define F4M_ELEMENT_BOOTSTRAPINFO_ATTRIBUTE_URL                   "url"

// The <media> element represents one representation of the piece of media. Each representation of the same piece of media
// will have a corresponding <media> element. There must be at least one <media> element.
#define F4M_ELEMENT_MEDIA                                         "media"

// The URL of the media file. It is required. If a specified URL is non-absolute, then it must be relative to the baseURL
// (if specified) or the manifest file itself (if no baseURL is present).
#define F4M_ELEMENT_MEDIA_ATTRIBUTE_URL                           "url"

// The bitrate of the media file, in kilobits per second. If only one <media> element is in the manifest file, then
// the bitrate attribute is optional. If more than one <media> element is in the manifest file, then the bitrate attribute
// is required for each element.
#define F4M_ELEMENT_MEDIA_ATTRIBUTE_BITRATE                       "bitrate"

// The intrinsic width of the media file, in pixels. It is optional.
#define F4M_ELEMENT_MEDIA_ATTRIBUTE_WIDTH                         "width"

// The intrinsic height of the media file, in pixels. It is optional.
#define F4M_ELEMENT_MEDIA_ATTRIBUTE_HEIGHT                        "height"

// The ID of a <drmAdditionalHeader> element which contains the DRM AdditionalHeader for this media file. It is optional.
#define F4M_ELEMENT_MEDIA_ATTRIBUTE_DRMADDITTIONALHEADERID        "drmAdditionalHeaderId"

// The ID of a <bootstrapInfo> element which contains the bootstrap info that this media file should use. It is optional.
// If this attribute is present, then the <url> attribute may represent the base URL of the actual media.
#define F4M_ELEMENT_MEDIA_ATTRIBUTE_BOOTSTRAPINFOID               "bootstrapInfoId"

// The ID of a <dvrInfo> element which contains the DVRInfo that this media file should use. It is optional.
#define F4M_ELEMENT_MEDIA_ATTRIBUTE_DVRINFOID                     "dvrInfoId"

// The group specifier for multicast media. It is optional. Multicast is only supported over RTMFP, and only for a single
// (non-MBR) stream. If specified, then the "url" attribute will contain the RTMFP connection URL and the "multicastStreamName"
// attribute must also be specified.
#define F4M_ELEMENT_MEDIA_ATTRIBUTE_GROUPSPEC                     "groupspec"

// The stream name for multicast media. It is optional. Multicast is only supported over RTMFP, and only for a
// single (non-MBR) stream. If specified, then the "url" attribute will contain the RTMFP connection URL and the "groupspec"
// attribute must also be specified.
#define F4M_ELEMENT_MEDIA_ATTRIBUTE_MULTICASTSTREAMNAME           "multicastStreamName"

// The <metadata> element represents the stream metadata (i.e. the metadata that is typically dispatched in the onMetaData event)
// for one representation of the piece of media. It contains a BASE64 encoded representation of the stream metadata for the given
// representation. It is an optional child element of <media>.
#define F4M_ELEMENT_MEDIA_ELEMENT_METADATA                        "metadata"

#endif
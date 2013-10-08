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

#ifndef __MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_PARAMETERS_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_PARAMETERS_DEFINED

#define PARAMETER_NAME_AFHS_RECEIVE_DATA_TIMEOUT                              L"AfhsReceiveDataTimeout"
#define PARAMETER_NAME_AFHS_REFERER                                           L"AfhsReferer"
#define PARAMETER_NAME_AFHS_USER_AGENT                                        L"AfhsUserAgent"
#define PARAMETER_NAME_AFHS_COOKIE                                            L"AfhsCookie"
#define PARAMETER_NAME_AFHS_VERSION                                           L"AfhsVersion"
#define PARAMETER_NAME_AFHS_IGNORE_CONTENT_LENGTH                             L"AfhsIgnoreContentLength"
#define PARAMETER_NAME_AFHS_SEGMENT_FRAGMENT_URL_EXTRA_PARAMETERS             L"AfhsSegmentFragmentUrlExtraParameters"

#define PARAMETER_NAME_AFHS_BOOTSTRAP_INFO                                    L"AfhsBootstrapInfo"
#define PARAMETER_NAME_AFHS_BASE_URL                                          L"AfhsBaseUrl"
#define PARAMETER_NAME_AFHS_MEDIA_PART_URL                                    L"AfhsMediaPartUrl"
#define PARAMETER_NAME_AFHS_MEDIA_METADATA                                    L"AfhsMediaMetadata"
#define PARAMETER_NAME_AFHS_BOOTSTRAP_INFO_URL                                L"AfhsMediaBootstrapInfoUrl"
#define PARAMETER_NAME_AFHS_MANIFEST_URL                                      L"AfhsManifestUrl"
#define PARAMETER_NAME_AFHS_MANIFEST_CONTENT                                  L"AfhsManifestContent"

#define PARAMETER_NAME_AFHS_COOKIES_COUNT                                     L"AfhsCookiesCount"
#define AFHS_COOKIE_FORMAT_PARAMETER_NAME                                     L"AfhsCookie%08u"

// we should get data in twenty seconds
#define AFHS_RECEIVE_DATA_TIMEOUT_DEFAULT                                     20000
#define MINIMUM_RECEIVED_DATA_FOR_SPLITTER                                    1 * 1024 * 1024

#endif

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

#ifndef __MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_PARAMETERS_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_PARAMETERS_DEFINED

#define PARAMETER_NAME_M3U8_OPEN_CONNECTION_TIMEOUT                   L"M3u8OpenConnectionTimeout"
#define PARAMETER_NAME_M3U8_OPEN_CONNECTION_SLEEP_TIME                L"M3u8OpenConnectionSleepTime"
#define PARAMETER_NAME_M3U8_TOTAL_REOPEN_CONNECTION_TIMEOUT           L"M3u8TotalReopenConnectionTimeout"

#define PARAMETER_NAME_M3U8_REFERER                                   L"M3u8Referer"
#define PARAMETER_NAME_M3U8_USER_AGENT                                L"M3u8UserAgent"
#define PARAMETER_NAME_M3U8_COOKIE                                    L"M3u8Cookie"
#define PARAMETER_NAME_M3U8_VERSION                                   L"M3u8Version"
#define PARAMETER_NAME_M3U8_IGNORE_CONTENT_LENGTH                     L"M3u8IgnoreContentLength"

#define PARAMETER_NAME_M3U8_COOKIES_COUNT                             L"M3u8CookiesCount"
#define M3U8_COOKIE_FORMAT_PARAMETER_NAME                             L"M3u8Cookie%08u"

#define PARAMETER_NAME_M3U8_PLAYLIST_URL                              L"M3u8PlaylistUrl"
#define PARAMETER_NAME_M3U8_PLAYLIST_CONTENT                          L"M3u8PlaylistContent"

// we should get data in twenty seconds (splitter)
#define M3U8_OPEN_CONNECTION_TIMEOUT_DEFAULT_SPLITTER                 20000
#define M3U8_OPEN_CONNECTION_SLEEP_TIME_DEFAULT_SPLITTER              0
#define M3U8_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT_SPLITTER         60000

// we should get data in five seconds (iptv)
#define M3U8_OPEN_CONNECTION_TIMEOUT_DEFAULT_IPTV                     5000
#define M3U8_OPEN_CONNECTION_SLEEP_TIME_DEFAULT_IPTV                  0
#define M3U8_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT_IPTV             60000

#endif

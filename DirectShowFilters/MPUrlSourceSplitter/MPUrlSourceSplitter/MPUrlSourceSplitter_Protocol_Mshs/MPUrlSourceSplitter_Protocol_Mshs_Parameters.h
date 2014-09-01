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

#ifndef __MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_PARAMETERS_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_PARAMETERS_DEFINED

#define PARAMETER_NAME_MSHS_REFERER                                   L"MshsReferer"
#define PARAMETER_NAME_MSHS_USER_AGENT                                L"MshsUserAgent"
#define PARAMETER_NAME_MSHS_COOKIE                                    L"MshsCookie"
#define PARAMETER_NAME_MSHS_VERSION                                   L"MshsVersion"
#define PARAMETER_NAME_MSHS_IGNORE_CONTENT_LENGTH                     L"MshsIgnoreContentLength"

#define PARAMETER_NAME_MSHS_BASE_URL                                  L"MshsBaseUrl"
#define PARAMETER_NAME_MSHS_MANIFEST                                  L"MshsManifest"

#define PARAMETER_NAME_MSHS_OPEN_CONNECTION_TIMEOUT                   L"MshsOpenConnectionTimeout"
#define PARAMETER_NAME_MSHS_OPEN_CONNECTION_SLEEP_TIME                L"MshsOpenConnectionSleepTime"
#define PARAMETER_NAME_MSHS_TOTAL_REOPEN_CONNECTION_TIMEOUT           L"MshsTotalReopenConnectionTimeout"

#define PARAMETER_NAME_MSHS_COOKIES_COUNT                             L"MshsCookiesCount"
#define MSHS_COOKIE_FORMAT_PARAMETER_NAME                             L"MshsCookie%08u"

// we should get data in twenty seconds
#define MSHS_OPEN_CONNECTION_TIMEOUT_DEFAULT                          20000
#define MSHS_OPEN_CONNECTION_SLEEP_TIME_DEFAULT                       0
#define MSHS_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT                  60000


#endif

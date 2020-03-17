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

#ifndef __MP_URL_SOURCE_SPLITTER_PROTOCOL_MMS_PARAMETERS_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PROTOCOL_MMS_PARAMETERS_DEFINED

#define PARAMETER_NAME_MMS_REFERER                                    L"MmsReferer"
#define PARAMETER_NAME_MMS_USER_AGENT                                 L"MmsUserAgent"
#define PARAMETER_NAME_MMS_COOKIE                                     L"MmsCookie"
#define PARAMETER_NAME_MMS_VERSION                                    L"MmsVersion"
#define PARAMETER_NAME_MMS_IGNORE_CONTENT_LENGTH                      L"MmsIgnoreContentLength"

#define PARAMETER_NAME_MMS_OPEN_CONNECTION_TIMEOUT                    L"MmsOpenConnectionTimeout"
#define PARAMETER_NAME_MMS_OPEN_CONNECTION_SLEEP_TIME                 L"MmsOpenConnectionSleepTime"
#define PARAMETER_NAME_MMS_TOTAL_REOPEN_CONNECTION_TIMEOUT            L"MmsTotalReopenConnectionTimeout"

// we should get data in twenty seconds
#define MMS_OPEN_CONNECTION_TIMEOUT_DEFAULT                           20000
#define MMS_OPEN_CONNECTION_SLEEP_TIME_DEFAULT                        0
#define MMS_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT                   60000

#endif

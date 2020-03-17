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

#ifndef __MP_URL_SOURCE_SPLITTER_PROTOCOL_UDP_PARAMETERS_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PROTOCOL_UDP_PARAMETERS_DEFINED

#define PARAMETER_NAME_UDP_OPEN_CONNECTION_TIMEOUT                    L"UdpOpenConnectionTimeout"
#define PARAMETER_NAME_UDP_OPEN_CONNECTION_SLEEP_TIME                 L"UdpOpenConnectionSleepTime"
#define PARAMETER_NAME_UDP_TOTAL_REOPEN_CONNECTION_TIMEOUT            L"UdpTotalReopenConnectionTimeout"

// specify check interval for incoming data
#define PARAMETER_NAME_UDP_RECEIVE_DATA_CHECK_INTERVAL                L"UdpReceiveDataCheckInterval"

// we should get data in two seconds (splitter)
#define UDP_OPEN_CONNECTION_TIMEOUT_DEFAULT_SPLITTER                  2000
#define UDP_OPEN_CONNECTION_SLEEP_TIME_DEFAULT_SPLITTER               0
#define UDP_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT_SPLITTER          60000

// we should get data in one seconds (iptv)
#define UDP_OPEN_CONNECTION_TIMEOUT_DEFAULT_IPTV                      1000
#define UDP_OPEN_CONNECTION_SLEEP_TIME_DEFAULT_IPTV                   0
#define UDP_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT_IPTV              60000

// we check if we are receiving data each 500 ms
#define UDP_RECEIVE_DATA_CHECK_INTERVAL_DEFAULT_SPLITTER              500
#define UDP_RECEIVE_DATA_CHECK_INTERVAL_DEFAULT_IPTV                  500

#endif

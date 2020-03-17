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

#ifndef __MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_PARAMETERS_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_PARAMETERS_DEFINED

#define PARAMETER_NAME_RTSP_OPEN_CONNECTION_TIMEOUT                   L"RtspOpenConnectionTimeout"
#define PARAMETER_NAME_RTSP_OPEN_CONNECTION_SLEEP_TIME                L"RtspOpenConnectionSleepTime"
#define PARAMETER_NAME_RTSP_TOTAL_REOPEN_CONNECTION_TIMEOUT           L"RtspTotalReopenConnectionTimeout"

#define PARAMETER_NAME_RTSP_MULTICAST_PREFERENCE                      L"RtspMulticastPreference"
#define PARAMETER_NAME_RTSP_UDP_PREFERENCE                            L"RtspUdpPreference"
#define PARAMETER_NAME_RTSP_SAME_CONNECTION_TCP_PREFERENCE            L"RtspSameConnectionTcpPreference"
#define PARAMETER_NAME_RTSP_CLIENT_PORT_MIN                           L"RtspClientPortMin"
#define PARAMETER_NAME_RTSP_CLIENT_PORT_MAX                           L"RtspClientPortMax"
#define PARAMETER_NAME_RTSP_IGNORE_RTP_PAYLOAD_TYPE                   L"RtspIgnoreRtpPayloadType"

// we should get data in twenty seconds (splitter)
#define RTSP_OPEN_CONNECTION_TIMEOUT_DEFAULT_SPLITTER                 20000
#define RTSP_OPEN_CONNECTION_SLEEP_TIME_DEFAULT_SPLITTER              0
#define RTSP_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT_SPLITTER         60000

// we should get data in one and half seconds (iptv)
#define RTSP_OPEN_CONNECTION_TIMEOUT_DEFAULT_IPTV                     1500
#define RTSP_OPEN_CONNECTION_SLEEP_TIME_DEFAULT_IPTV                  0
#define RTSP_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT_IPTV             60000

#define RTSP_MULTICAST_PREFERENCE_DEFAULT                             2
#define RTSP_UDP_PREFERENCE_DEFAULT                                   1
#define RTSP_SAME_CONNECTION_TCP_PREFERENCE_DEFAULT                   0

#define RTSP_CLIENT_PORT_MAX_DEFAULT                                  65535

#define RTSP_IGNORE_RTP_PAYLOAD_TYPE_DEFAULT                          false

#endif

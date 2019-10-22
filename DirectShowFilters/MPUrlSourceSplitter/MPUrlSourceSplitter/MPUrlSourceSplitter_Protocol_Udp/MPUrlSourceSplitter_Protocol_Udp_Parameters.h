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

#include "Ipv4Header_Constants.h"

#define PARAMETER_NAME_UDP_OPEN_CONNECTION_TIMEOUT                    L"UdpOpenConnectionTimeout"
#define PARAMETER_NAME_UDP_OPEN_CONNECTION_SLEEP_TIME                 L"UdpOpenConnectionSleepTime"
#define PARAMETER_NAME_UDP_TOTAL_REOPEN_CONNECTION_TIMEOUT            L"UdpTotalReopenConnectionTimeout"

// specify check interval for incoming data
#define PARAMETER_NAME_UDP_RECEIVE_DATA_CHECK_INTERVAL                L"UdpReceiveDataCheckInterval"

// we should get data in two seconds (splitter)
#define UDP_OPEN_CONNECTION_TIMEOUT_DEFAULT_SPLITTER                  20000
#define UDP_OPEN_CONNECTION_SLEEP_TIME_DEFAULT_SPLITTER               0
#define UDP_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT_SPLITTER          60000

// we should get data in one seconds (iptv)
#define UDP_OPEN_CONNECTION_TIMEOUT_DEFAULT_IPTV                      1000
#define UDP_OPEN_CONNECTION_SLEEP_TIME_DEFAULT_IPTV                   0
#define UDP_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT_IPTV              60000

// we check if we are receiving data each 500 ms
#define UDP_RECEIVE_DATA_CHECK_INTERVAL_DEFAULT_SPLITTER              500
#define UDP_RECEIVE_DATA_CHECK_INTERVAL_DEFAULT_IPTV                  500

// very specific UDP options, all of them requires that user is member or Administrators group (due to using raw sockets)

#define PARAMETER_NAME_UDP_IPV4_DSCP                                  L"UdpDscp"
#define PARAMETER_NAME_UDP_IPV4_ECN                                   L"UdpEcn"
#define PARAMETER_NAME_UDP_IPV4_IDENTIFICATION                        L"UdpIdentification"
#define PARAMETER_NAME_UDP_IPV4_DONT_FRAGMENT                         L"UdpDontFragment"
#define PARAMETER_NAME_UDP_IPV4_MORE_FRAGMNETS                        L"UdpMoreFragments"
#define PARAMETER_NAME_UDP_IPV4_TTL                                   L"UdpTtl"
#define PARAMETER_NAME_UDP_IPV4_PROTOCOL                              L"UdpProtocol"
#define PARAMETER_NAME_UDP_IPV4_OPTIONS                               L"UdpOptions"

#define UDP_IPV4_DSCP_MIN                                             IPV4_HEADER_DSCP_MIN
#define UDP_IPV4_DSCP_MAX                                             IPV4_HEADER_DSCP_MAX
#define UDP_IPV4_DSCP_DEFAULT                                         IPV4_HEADER_DSCP_DEFAULT

#define UDP_IPV4_ECN_MIN                                              IPV4_HEADER_ECN_MIN
#define UDP_IPV4_ECN_MAX                                              IPV4_HEADER_ECN_MAX
#define UDP_IPV4_ECN_DEFAULT                                          IPV4_HEADER_ECN_DEFAULT

#define UDP_IPV4_IDENTIFICATION_MIN                                   IPV4_HEADER_IDENTIFICATION_MIN
#define UDP_IPV4_IDENTIFICATION_MAX                                   IPV4_HEADER_IDENTIFICATION_MAX

#define UDP_IPV4_TTL_MIN                                              IPV4_HEADER_TTL_MIN
#define UDP_IPV4_TTL_MAX                                              IPV4_HEADER_TTL_MAX
#define UDP_IPV4_TTL_DEFAULT                                          IPV4_HEADER_TTL_DEFAULT

#define UDP_IPV4_PROTOCOL_MIN                                         IPV4_HEADER_PROTOCOL_MIN
#define UDP_IPV4_PROTOCOL_MAX                                         IPV4_HEADER_PROTOCOL_MAX

#endif

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

#ifndef __IPV4_HEADER_CONSTANTS_DEFINED
#define __IPV4_HEADER_CONSTANTS_DEFINED

#define IPV4_HEADER_DSCP_MIN                                                  0x00
#define IPV4_HEADER_DSCP_MAX                                                  0x3F
#define IPV4_HEADER_DSCP_DEFAULT                                              0x00

#define IPV4_HEADER_ECN_MIN                                                   0x00
#define IPV4_HEADER_ECN_MAX                                                   0x03
#define IPV4_HEADER_ECN_DEFAULT                                               0x00

#define IPV4_HEADER_IDENTIFICATION_MIN                                        0x0000
#define IPV4_HEADER_IDENTIFICATION_MAX                                        0xFFFF

#define IPV4_HEADER_TTL_MIN                                                   0x00
#define IPV4_HEADER_TTL_MAX                                                   0xFF
#define IPV4_HEADER_TTL_DEFAULT                                               0x20

#define IPV4_HEADER_PROTOCOL_MIN                                              0x00
#define IPV4_HEADER_PROTOCOL_MAX                                              0xFF
#define IPV4_HEADER_PROTOCOL_DEFAULT                                          0x00

#define IPV4_HEADER_UNSPECIFIED_PROTOCOL                                      0x00
#define IPV4_HEADER_ICMP_PROTOCOL                                             0x01
#define IPV4_HEADER_IGMP_PROTOCOL                                             0x02
#define IPV4_HEADER_TCP_PROTOCOL                                              0x06
#define IPV4_HEADER_UDP_PROTOCOL                                              0x11

#define IPV4_HEADER_LENGTH_MIN                                                0x14

#endif

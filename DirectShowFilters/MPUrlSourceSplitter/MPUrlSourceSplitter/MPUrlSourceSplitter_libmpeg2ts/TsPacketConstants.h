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

#ifndef __TS_PACKET_CONSTANTS_DEFINED
#define __TS_PACKET_CONSTANTS_DEFINED

#define TS_PACKET_SIZE                                                          188

#define TS_PACKET_HEADER_LENGTH                                                 4

#define TS_PACKET_MAXIMUM_PAYLOAD_SIZE                                          (TS_PACKET_SIZE - TS_PACKET_HEADER_LENGTH)
#define TS_PACKET_MAXIMUM_CONTINUITY_COUNTER                                    0x0F

#define TS_PACKET_SYNC_BYTE                                                     0x47

#define TS_PACKET_PID_NULL                                                      0x1FFF
#define TS_PACKET_MAX_RESERVED_PID                                              0x000F
#define TS_PACKET_PID_COUNT                                                     (TS_PACKET_PID_NULL + 1)

#define TS_PACKET_TRANSPORT_SCRAMBLING_CONTROL_NOT_SCRAMBLED                    0x00

#define TS_PACKET_ADAPTATION_FIELD_CONTROL_RESERVED                             0x00
#define TS_PACKET_ADAPTATION_FIELD_CONTROL_ONLY_PAYLOAD                         0x01
#define TS_PACKET_ADAPTATION_FIELD_CONTROL_ONLY_ADAPTATION_FIELD                0x02
#define TS_PACKET_ADAPTATION_FIELD_CONTROL_ADAPTATION_FIELD_WITH_PAYLOAD        0x03

#define TS_PACKET_NULL_PAYLOAD_BYTE                                             0xFF
#define TS_PACKET_STUFFING_BYTE                                                 0xFF

#endif
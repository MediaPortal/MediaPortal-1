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

#define TS_PACKET_SIZE                                                188

#define TS_PACKET_HEADER_LENGTH                                       4

#define TS_PACKET_SYNC_BYTE                                           0x47

#define TS_PACKET_PID_NULL                                            0x1FFF
#define TS_PACKET_MAX_RESERVED_PID                                    0x000F
#define TS_PACKET_PID_COUNT                                           (TS_PACKET_PID_NULL + 1)

#endif
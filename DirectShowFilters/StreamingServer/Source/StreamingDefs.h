/* 
 *  Copyright (C) 2016 Team MediaPortal
 *  http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#pragma once

//Basic TS packet defines
#define TRANSPORT_PACKET_SIZE 188
#define TRANSPORT_SYNC_BYTE 0x47

//Defines for TS data in network packets (for 'TsMPEG2TransportFileServerMediaSubsession.cpp')
#define TRANSPORT_PACKETS_PER_NETWORK_PACKET 7
// This amount of data must fit within a network packet payload (<= 1472 bytes for UDP)
#define PREFERRED_FRAME_SIZE (TRANSPORT_PACKETS_PER_NETWORK_PACKET * TRANSPORT_PACKET_SIZE)

//Defines for 'TSBuffer.cpp' (used for file reading)
#define TV_BUFFER_ITEMS 32
#define RADIO_BUFFER_ITEMS 2

//Limit contiguous null TS packet sending to 15 sec maximum
#define NULL_TS_TIMEOUT 15000

//NULL packet byte[3] value - not scrambled, payload only, continuity 0x7
#define NULL_TS_CONTINUITY_BYTE 0x17

//Minimum of 500ms of data buffered in file after an 'empty' read
#define MIN_FILE_BUFFER_TIME 500

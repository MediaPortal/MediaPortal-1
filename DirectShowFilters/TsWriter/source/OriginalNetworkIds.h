/*
 *  Copyright (C) 2006-2008 Team MediaPortal
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

// Virgin Media Cable - UK DVB-C
#define ORIGINAL_NETWORK_ID_VIRGIN_MEDIA    0xf020

// Astra 28.2E - UK satellite
#define ORIGINAL_NETWORK_ID_FREESAT         0x003b

// Hotbird 13E - European satellite
#define ORIGINAL_NETWORK_ID_POLSAT_CYFRA_NC 0x0071
#define ORIGINAL_NETWORK_ID_GROUPE_CANALP   0x00b0  // Groupe CANAL+
#define ORIGINAL_NETWORK_ID_CANALPLUS_START 0x00c0
#define ORIGINAL_NETWORK_ID_CANALPLUS_END   0x00cd
#define ORIGINAL_NETWORK_ID_MEDIASET        0x0110
#define ORIGINAL_NETWORK_ID_EUTELSAT_13E_1  0x013e
#define ORIGINAL_NETWORK_ID_EUTELSAT_13E_2  0x013f
#define ORIGINAL_NETWORK_ID_ARABSAT         0x02be  // Arab Satellite Communications Organization
#define ORIGINAL_NETWORK_ID_SKY_ITALIA      0xfbff

// Orbit Showtime Network
#define ORIGINAL_NETWORK_ID_OSN_1           0x002c
#define ORIGINAL_NETWORK_ID_OSN_2           0x006e
#define ORIGINAL_NETWORK_ID_EUTELSAT_7W     0x077e
#define ORIGINAL_NETWORK_ID_NILESAT_101     0x0800
#define ORIGINAL_NETWORK_ID_IRDETO_MUX_SYS  0x5000

// 156E & 160E - New Zealand & Australia satellite
#define ORIGINAL_NETWORK_ID_TVNZ            0x002f  // Freeview Satellite
#define ORIGINAL_NETWORK_ID_OPTUS_B3_156E_1 0x0069  // Foxtel
#define ORIGINAL_NETWORK_ID_FOXTEL          0x00a8  // (NDS)
#define ORIGINAL_NETWORK_ID_SKY_NZ          0x00a9  // (NDS)
#define ORIGINAL_NETWORK_ID_OPTUS_NETWORKS  0x0fff  // VAST AU
#define ORIGINAL_NETWORK_ID_OPTUS_B3_156E_2 0x1000  // Foxtel

// EchoStar/Dish - North American satellite
#define ORIGINAL_NETWORK_ID_DISH_NETWORK    0x1001
#define ORIGINAL_NETWORK_ID_DISH_61_5W      0x1002
#define ORIGINAL_NETWORK_ID_DISH_83W        0x1003
#define ORIGINAL_NETWORK_ID_DISH_119W       0x1004
#define ORIGINAL_NETWORK_ID_DISH_121W       0x1005
#define ORIGINAL_NETWORK_ID_DISH_148W       0x1006
#define ORIGINAL_NETWORK_ID_DISH_175W       0x1007
#define ORIGINAL_NETWORK_ID_DISH_W          0x1008
#define ORIGINAL_NETWORK_ID_DISH_X          0x1009
#define ORIGINAL_NETWORK_ID_DISH_Y          0x100a
#define ORIGINAL_NETWORK_ID_DISH_Z          0x100b
#define ORIGINAL_NETWORK_ID_DISH_START      ORIGINAL_NETWORK_ID_DISH_NETWORK
#define ORIGINAL_NETWORK_ID_DISH_END        ORIGINAL_NETWORK_ID_DISH_Z

// Australian broadcasters
#define ORIGINAL_NETWORK_ID_ABC             0x1010
#define ORIGINAL_NETWORK_ID_SBS             0x1011
#define ORIGINAL_NETWORK_ID_NINE_NETWORK    0x1012
#define ORIGINAL_NETWORK_ID_7_NETWORK       0x1013
#define ORIGINAL_NETWORK_ID_NETWORK_10      0x1014
#define ORIGINAL_NETWORK_ID_WIN_TV          0x1015
#define ORIGINAL_NETWORK_ID_PRIME_TV        0x1016
#define ORIGINAL_NETWORK_ID_SOUTHERN_CROSS  0x1017  // Southern Cross Broadcasting
#define ORIGINAL_NETWORK_ID_TELECASTERS     0x1018
#define ORIGINAL_NETWORK_ID_NBN_TV          0x1019
#define ORIGINAL_NETWORK_ID_IMPARJA_TV      0x101a
// 0x101b - 0x101f: reserved
#define ORIGINAL_NETWORK_ID_AU_START        ORIGINAL_NETWORK_ID_ABC
#define ORIGINAL_NETWORK_ID_AU_END          0x101f
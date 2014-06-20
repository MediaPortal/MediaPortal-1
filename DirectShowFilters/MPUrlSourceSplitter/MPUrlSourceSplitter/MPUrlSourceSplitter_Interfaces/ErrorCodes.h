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

#ifndef __ERROR_CODES_DEFINED
#define __ERROR_CODES_DEFINED

#include "WinError.h"

// each method should use HRESULT as valid error code
// if not possible, common error codes are defined here
// each module can define other error codes
// after changing error code appropriate files in OnlineVideos have to be changed

#define IS_OUR_ERROR(error)                                                     ((error & 0xFFFFFF00) == 0x80000000)

// common error codes

#define E_INVALID_CONFIGURATION                                                 -1
#define E_URL_NOT_SPECIFIED                                                     -2
#define E_CONVERT_STRING_ERROR                                                  -3
#define E_CANNOT_LOAD_PLUGIN_LIBRARY                                            -4
#define E_INVALID_PLUGIN                                                        -5
#define E_CANNOT_CREATE_PLUGIN                                                  -6
#define E_NOT_FOUND_INTERFACE_NAME                                              -7
#define E_STREAM_COUNT_UNKNOWN                                                  -8
#define E_INVALID_STREAM_PACKAGE_REQUEST                                        -9

// parser error codes

#define E_PARSER_STILL_PENDING                                                  -20
#define E_NO_PARSER_LOADED                                                      -21
#define E_NO_ACTIVE_PARSER                                                      -22

// protocol error codes

#define E_NO_PROTOCOL_LOADED                                                    -30
#define E_NO_ACTIVE_PROTOCOL                                                    -31
#define E_CONNECTION_LOST_CANNOT_REOPEN                                         -32
#define E_NO_MORE_DATA_AVAILABLE                                                -33
#define E_PAUSE_SEEK_STOP_MODE_DISABLE_READING                                  -34
#define E_CANNOT_START_RECEIVING_DATA                                           -35

// seeking error codes

#define E_SEEK_METHOD_NOT_SUPPORTED                                             -40
#define E_NO_STREAM_TO_SEEK                                                     -41
#define E_NOT_FOUND_SEEK_INDEX_ENTRY                                            -42
#define E_NOT_FOUND_ANY_FLV_PACKET                                              -43
#define E_NO_MPEG_TS_POSITION_TO_SEEK                                           -44
#define E_NOT_FOUND_MINIMUM_TIMESTAMP                                           -45
#define E_NOT_FOUND_MAXIMUM_TIMESTAMP                                           -46
#define E_NOT_FOUND_TIMESTAMP                                                   -47
#define E_POSITION_LIMIT_OVER_MAXIMUM_POSITION                                  -48
#define E_MINIMUM_TIMESTAMP_GREATER_THAN_MAXIMUM_TIMESTAMP                      -49
#define E_SEEK_INDEX_ENTRY_EXISTS                                               -50

// HTTP protocol error codes

#define E_HTTP_CANNOT_SET_COOKIES                                               -100
#define E_HTTP_CANNOT_INITIALIZE                                                -101

#endif
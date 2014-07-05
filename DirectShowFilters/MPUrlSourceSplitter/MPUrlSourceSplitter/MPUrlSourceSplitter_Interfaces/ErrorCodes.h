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
#include "vfwmsgs.h"

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
#define E_INVALID_STREAM_ID                                                     -10
#define E_CANNOT_GET_MODULE_FILE_NAME                                           -11
#define E_CONNECTION_LOST_TRYING_REOPEN                                         -12

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

// RTSP protocol error codes

#define E_RTSP_NO_PUBLIC_OPTIONS_RESPONSE_HEADER                                -110
#define E_RTSP_NOT_ALL_REQUIRED_METHODS                                         -111
#define E_RTSP_DESCRIBE_CONTENT_LENGTH_ZERO                                     -112
#define E_RTSP_DESCRIBE_CONTENT_TYPE_NOT_FOUND                                  -113
#define E_RTSP_CONTENT_HEADER_TYPE_NOT_ALLOWED                                  -114
#define E_RTSP_SESSION_DESCRIPTION_PARSE_ERROR                                  -115
#define E_RTSP_NO_MEDIA_DESCRIPTIONS_IN_SESSION_DESCRIPTION                     -116
#define E_RTSP_NO_TRANSPORT_HEADER                                              -117
#define E_RTSP_NOT_TCP_TRANSPORT_HEADER                                         -118
#define E_RTSP_SAME_CONNECTION_TRANSPORT_NOT_SUPPORTED                          -119
#define E_RTSP_BAD_OR_NOT_IMPLEMENTED_TRANSPORT                                 -120
#define E_RTSP_NO_TRACKS                                                        -121
#define E_RTSP_NOT_UDP_TRANSPORT_HEADER                                         -122
#define E_RTSP_CLIENT_PORTS_NOT_SAME_AS_REQUESTED                               -123
#define E_RTSP_CANNOT_NEGOTIATE_ANY_TRANSPORT                                   -124
#define E_RTSP_NOT_SPECIFIED_REQUEST_SEQUENCE_NUMBER                            -125
#define E_RTSP_NO_RESPONSE_FOR_REQUEST                                          -126
#define E_RTSP_BAD_SESSION_ID                                                   -127
#define E_RTSP_NOT_SPECIFIED_RESPONSE_SEQUENCE_NUMBER                           -128
#define E_RTSP_REQUEST_AND_RESPONSE_SEQUENCE_NUMBERS_NOT_EQUAL                  -129
#define E_RTSP_STATUS_CODE_NOT_SUCCESS                                          -130
#define E_RTSP_NO_DATA_OR_CONTROL_CLIENT_PORT                                   -131
#define E_RTSP_NO_RTP_OR_RTCP_PACKET                                            -132
#define E_RTSP_INVALID_PACKET_FOR_PORT                                          -133
#define E_RTSP_NOT_INTERLEAVED_PACKET_NOT_VALID_RTSP_RESPONSE                   -134
#define E_RTSP_NOT_SET_SENDER_SYNCHRONIZATION_SOURCE_IDENTIFIER                 -135
#define E_RTSP_NO_ENDPOINT_FOUND                                                -136
#define E_RTSP_SENT_DATA_LENGTH_NOT_SAME_AS_RTCP_PACKET_LENGTH                  -137

// UDP/RTP protocol error codes

#define E_UDP_NO_DATA_RECEIVED                                                  -140

#endif
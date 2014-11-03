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

// our error code is error code with highest bit set, set lowest two bytes, except second byte equal to 0F (in this case it is error code from CURL)
FORCEINLINE bool IS_OUR_ERROR(HRESULT error) { return (((error & 0xFFFFF000) == 0x80000000) && ((error & 0x00000F00) != 0x00000F00)); }

// common error codes

#define E_INVALID_CONFIGURATION                                                 -1
#define E_URL_NOT_SPECIFIED                                                     -2
#define E_CONVERT_STRING_ERROR                                                  -3
#define E_CANNOT_LOAD_PLUGIN_LIBRARY                                            -4
#define E_INVALID_PLUGIN                                                        -5
#define E_INVALID_PLUGIN_TYPE                                                   -6
#define E_CANNOT_CREATE_PLUGIN                                                  -7
#define E_NOT_FOUND_INTERFACE_NAME                                              -8
#define E_STREAM_COUNT_UNKNOWN                                                  -9
#define E_INVALID_STREAM_PACKAGE_REQUEST                                        -10
#define E_INVALID_STREAM_PACKAGE_RESPONSE                                       -11
#define E_INVALID_STREAM_ID                                                     -12
#define E_CANNOT_GET_MODULE_FILE_NAME                                           -13
#define E_CONNECTION_LOST_TRYING_REOPEN                                         -14
#define E_PARSE_PARAMETERS_NOT_ENOUGH_MEMORY_FOR_TOKEN                          -15
#define E_PARSE_PARAMETERS_NOT_ENOUGH_MEMORY_FOR_PARAMETER_NAME                 -16
#define E_PARSE_PARAMETERS_NOT_ENOUGH_MEMORY_FOR_PARAMETER_VALUE                -17
#define E_PARSE_PARAMETERS_CANNOT_GET_UNESCAPED_VALUE                           -18

// parser general error codes

#define E_PARSER_STILL_PENDING                                                  -20
#define E_NO_PARSER_LOADED                                                      -21
#define E_NO_ACTIVE_PARSER                                                      -22
#define E_DRM_PROTECTED                                                         -23

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
#define E_NOT_FOUND_MINIMUM_TIMESTAMP                                           -44
#define E_NOT_FOUND_MAXIMUM_TIMESTAMP                                           -45
#define E_NOT_FOUND_TIMESTAMP                                                   -46
#define E_POSITION_LIMIT_OVER_MAXIMUM_POSITION                                  -47
#define E_MINIMUM_TIMESTAMP_GREATER_THAN_MAXIMUM_TIMESTAMP                      -48
#define E_SEEK_INDEX_ENTRY_EXISTS                                               -49

// specific parser error code

// F4M parser error codes

#define E_F4M_BASE_URL_NULL_OR_EMPTY                                            -60
#define E_F4M_NO_BOOTSTRAP_INFO_PROFILE                                         -61
#define E_F4M_NO_PIECE_OF_MEDIA                                                 -62
#define E_F4M_NO_MEDIA_URL                                                      -63
#define E_F4M_NO_BOOTSTRAP_INFO                                                 -64
#define E_F4M_NO_BOOTSTRAP_INFO_VALUE                                           -65
#define E_F4M_CANNOT_PARSE_BOOTSTRAP_INFO_BOX                                   -66
#define E_F4M_ONLY_HTTP_PROTOCOL_SUPPORTED_IN_BASE_URL                          -67

// MSHS parser error codes

#define E_MSHS_NO_VIDEO_OR_AUDIO_STREAM_PRESENT                                 -68
#define E_MSHS_ONLY_HTTP_PROTOCOL_SUPPORTED_IN_URL                              -69

// MPEG2 TS parser error codes

#define E_MPEG2TS_CANNOT_PARSE_PACKET                                           -70
#define E_MPEG2TS_EMPTY_SECTION_AND_PSI_PACKET_WITHOUT_NEW_SECTION              -71
#define E_MPEG2TS_INCOMPLETE_SECTION                                            -72
#define E_MPEG2TS_SECTION_INVALID_CRC32                                         -73
#define E_MPEG2TS_CANNOT_SPLIT_SECTION_INTO_PSI_PACKETS                         -74
#define E_MPEG2TS_SECTION_BIGGER_THAN_ORIGINAL_SECTION                          -75
#define E_MPEG2TS_ONLY_ONE_PROGRAM_ALLOWED                                      -76

// M3U8 parser error codes

#define E_M3U8_NOT_VALID_ITEM_FOUND                                             -80
#define E_M3U8_NOT_VALID_GENERAL_TAG_FOUND                                      -81
#define E_M3U8_NOT_VALID_PLAYLIST_ITEM_FOUND                                    -82
#define E_M3U8_NOT_VALID_TAG_FOUND                                              -83
#define E_M3U8_NOT_VALID_COMMENT_TAG_FOUND                                      -84
#define E_M3U8_NOT_PLAYLIST                                                     -85
#define E_M3U8_NOT_SUPPORTED_PLAYLIST_VERSION                                   -86
#define E_M3U8_NOT_VALID_PLAYLIST                                               -87
#define E_M3U8_NOT_SUPPORTED_PLAYLIST_ITEM                                      -88
#define E_M3U8_NOT_SUPPORTED_TAG                                                -89
#define E_M3U8_NO_PLAYLIST_ITEM_FOR_TAG                                         -90

FORCEINLINE bool IS_M3U8_ERROR(HRESULT error) { return ((error >= E_M3U8_NO_PLAYLIST_ITEM_FOR_TAG) && (error <= E_M3U8_NOT_VALID_ITEM_FOUND)); }

// specific protocol error codes

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
#define E_RTSP_NO_DATA_RECEIVED_BETWEEN_RECEIVER_REPORTS                        -138

// UDP/RTP protocol error codes

#define E_UDP_NO_DATA_RECEIVED                                                  -140

// RTMP protocol error codes

#define E_RTMP_BAD_URL_FORMAT                                                   -150
#define E_RTMP_CONNECT_FAILED                                                   -151
#define E_RTMP_CONNECT_STREAM_FAILED                                            -152
#define E_RTMP_GENERAL_READ_ERROR                                               -153

// AFHS protocol error codes
#define E_AFHS_NO_DECRYPTOR_LOADED                                              -160
#define E_AFHS_CANNOT_PARSE_BOOTSTRAP_INFO_BOX                                  -161
#define E_AFHS_CANNOT_GET_SEGMENT_FRAGMENTS_FROM_BOOTSTRAP_INFO_BOX             -162
#define E_AFHS_NOT_FOUND_SEGMENT_FRAGMENT_IN_LIVE_STREAM                        -163
#define E_AFHS_CANNOT_DECODE_METADATA                                           -164
#define E_AFHS_CANNOT_CREATE_METADATA_FLV_PACKET                                -165
#define E_AFHS_CANNOT_PARSE_BOX                                                 -166
#define E_AFHS_CANNOT_PARSE_MEDIA_DATA_BOX                                      -167
#define E_AFHS_BOX_SIZE_ZERO                                                    -168
#define E_AFHS_DECRYPTED_DATA_SIZE_ZERO                                         -169
#define E_AFHS_NO_ACTIVE_DECRYPTOR                                              -170
#define E_AFHS_DECRYPTOR_STILL_PENDING                                          -171

// AFHS akamai decryptor error codes

#define E_AFHS_AKAMAI_DECRYPTOR_CANNOT_LOAD_DECRYPTOR                           -175
#define E_AFHS_AKAMAI_DECRYPTOR_NO_DECRYPTOR_FILE_NAME                          -176
#define E_AFHS_AKAMAI_DECRYPTOR_CANNOT_SAVE_DECRYPTOR                           -177
#define E_AFHS_AKAMAI_DECRYPTOR_GENERAL_ERROR                                   -178
#define E_AFHS_AKAMAI_DECRYPTOR_INVALID_COUNT_OF_ENCRYPTED_SEGMENT_FRAGMENTS    -179
#define E_AFHS_AKAMAI_DECRYPTOR_NOT_FLV_PACKET                                  -181
#define E_AFHS_AKAMAI_DECRYPTOR_NOT_AKAMAI_FLV_PACKET                           -182
#define E_AFHS_AKAMAI_DECRYPTOR_NOT_FOUND_MEDIA_DATA_BOX                        -183
#define E_AFHS_AKAMAI_DECRYPTOR_NOT_CREATED_BOX                                 -184
#define E_AFHS_AKAMAI_DECRYPTOR_INVALID_KEY_LENGTH                              -185
#define E_AFHS_AKAMAI_DECRYPTOR_DECRYPTED_DATA_NOT_EQUAL_TO_ENCRYPTED_DATA      -186
#define E_AFHS_AKAMAI_DECRYPTOR_CANNOT_CREATE_DECRYPTED_FLV_PACKET              -187

// MSHS protocol error codes

#define E_MSHS_NO_VIDEO_AND_AUDIO_FRAGMENT                                      -192
#define E_MSHS_NO_FRAGMENT_URL                                                  -193
#define E_MSHS_CANNOT_PARSE_STREAMING_MEDIA_BOX                                 -194
#define E_MSHS_CANNOT_PARSE_MOVIE_FRAGMENT_BOX                                  -195
#define E_MSHS_CANNOT_GET_TRACK_HEADER_FRAGMENT_BOX_FROM_MOVIE_FRAGMENT_BOX     -196
#define E_MSHS_CANNOT_PARSE_TRACK_FRAGMENT_HEADER_BOX                           -197
#define E_MSHS_NOT_FOUND_SPS_START                                              -198
#define E_MSHS_NOT_FOUND_PPS_START                                              -199
#define E_MSHS_CANNOT_CONVERT_SPS                                               -200
#define E_MSHS_CANNOT_CONVERT_PPS                                               -201
#define E_MSHS_INVALID_FRAGMENT_SIZE                                            -202
#define E_MSHS_INVALID_BOX                                                      -203
#define E_MSHS_INVALID_MOVIE_FRAGMENT_BOX                                       -204
#define E_MSHS_CANNOT_GET_VIDEO_FRAGMENT_INDEX                                  -205
#define E_MSHS_CANNOT_GET_AUDIO_FRAGMENT_INDEX                                  -206

// M3U8 protocol error codes

#define E_M3U8_CANNOT_GET_STREAM_FRAGMENTS_FROM_MEDIA_PLAYLIST                  -210

#endif
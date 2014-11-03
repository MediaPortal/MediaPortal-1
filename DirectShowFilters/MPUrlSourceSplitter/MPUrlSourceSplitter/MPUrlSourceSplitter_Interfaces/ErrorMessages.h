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

#ifndef __ERROR_MESSAGES_DEFINED
#define __ERROR_MESSAGES_DEFINED

#include "ErrorCodes.h"

struct ErrorMessage
{
  HRESULT code;
  const wchar_t *message;
};

ErrorMessage ERROR_MESSAGES[] = {

  // common error codes

  { E_INVALID_CONFIGURATION, L"Not specified configuration parameters." },
  { E_URL_NOT_SPECIFIED, L"Url is not specified." },
  { E_CONVERT_STRING_ERROR, L"Error occured while converting string." },
  { E_CANNOT_LOAD_PLUGIN_LIBRARY, L"Cannot load plugin library." },
  { E_INVALID_PLUGIN, L"Invalid plugin." },
  { E_INVALID_PLUGIN_TYPE, L"Invalid plugin type." },
  { E_CANNOT_CREATE_PLUGIN, L"Cannot create plugin." },
  { E_NOT_FOUND_INTERFACE_NAME, L"The network interface is not found." },
  { E_STREAM_COUNT_UNKNOWN, L"The stream count is invalid." },
  { E_INVALID_STREAM_PACKAGE_REQUEST, L"Invalid stream package request." },
  { E_INVALID_STREAM_PACKAGE_RESPONSE, L"Invalid stream package response." },
  { E_INVALID_STREAM_ID, L"Invalid stream ID for stream package request." },
  { E_CANNOT_GET_MODULE_FILE_NAME, L"Cannot get module file name." },
  { E_CONNECTION_LOST_TRYING_REOPEN, L"Conection lost, trying to reopen." },
  { E_PARSE_PARAMETERS_NOT_ENOUGH_MEMORY_FOR_TOKEN, L"Not enough memory for parameter name and value." },
  { E_PARSE_PARAMETERS_NOT_ENOUGH_MEMORY_FOR_PARAMETER_NAME, L"Not enough memory for parameter name." },
  { E_PARSE_PARAMETERS_NOT_ENOUGH_MEMORY_FOR_PARAMETER_VALUE, L"Not enough memory for parameter value." },
  { E_PARSE_PARAMETERS_CANNOT_GET_UNESCAPED_VALUE, L"Cannot unescape parameter value." },

  // parser general error codes

  { E_PARSER_STILL_PENDING, L"At least one parser still doesn't return known stream or unknown stream state." },
  { E_NO_PARSER_LOADED, L"No parser available." },
  { E_NO_ACTIVE_PARSER, L"No active parser for stream." },
  { E_DRM_PROTECTED, L"DRM protected stream, cannot decrypt." },

  // protocol error codes

  { E_NO_PROTOCOL_LOADED, L"No protocol loaded." },
  { E_NO_ACTIVE_PROTOCOL, L"No active protocol for stream." },
  { E_CONNECTION_LOST_CANNOT_REOPEN, L"Connection is lost, cannot reopen." },
  { E_NO_MORE_DATA_AVAILABLE, L"No more data available." },
  { E_PAUSE_SEEK_STOP_MODE_DISABLE_READING, L"Disabled reading stream when seeking." },

  // seeking error codes

  { E_SEEK_METHOD_NOT_SUPPORTED, L"Seeking method is not supported." },
  { E_NO_STREAM_TO_SEEK, L"There is no stream to perform seek." },
  { E_NOT_FOUND_SEEK_INDEX_ENTRY, L"Seek index for stream is created, but not found place to seek in stream." },
  { E_NOT_FOUND_ANY_FLV_PACKET, L"Not found any FLV packet in stream." },
  { E_NOT_FOUND_MINIMUM_TIMESTAMP, L"Not found minimum timestamp for stream." },
  { E_NOT_FOUND_MAXIMUM_TIMESTAMP, L"Not found maximum timestamp for stream." },
  { E_NOT_FOUND_TIMESTAMP, L"Not found timestamp to seek in stream." },
  { E_POSITION_LIMIT_OVER_MAXIMUM_POSITION, L"Position limit over maximum position." },
  { E_MINIMUM_TIMESTAMP_GREATER_THAN_MAXIMUM_TIMESTAMP, L"Minimum timestamp is greater than maximum timestamp." },
  { E_SEEK_INDEX_ENTRY_EXISTS, L"Seek index entry exists, new entry cannot be added." },

  // specific parser error code

  // F4M parser error codes

  { E_F4M_BASE_URL_NULL_OR_EMPTY, L"Base url is not specified." },
  { E_F4M_NO_BOOTSTRAP_INFO_PROFILE, L"No bootstrap info profile." },
  { E_F4M_NO_PIECE_OF_MEDIA, L"No stream to play." },
  { E_F4M_NO_MEDIA_URL, L"Stream URL is not specified." },
  { E_F4M_NO_BOOTSTRAP_INFO, L"No bootstrap info to get stream fragments." },
  { E_F4M_NO_BOOTSTRAP_INFO_VALUE, L"No bootstrap info value to get stream fragments." },
  { E_F4M_CANNOT_PARSE_BOOTSTRAP_INFO_BOX, L"Cannot parse bootstrap info." },
  { E_F4M_ONLY_HTTP_PROTOCOL_SUPPORTED_IN_BASE_URL, L"Only HTTP protocol is supported for Adobe HTTP Dynamic Streaming." },

  // MSHS parser error codes

  { E_MSHS_NO_VIDEO_OR_AUDIO_STREAM_PRESENT, L"Missing audio or video for Microsoft Smooth Streaming stream." },
  { E_MSHS_ONLY_HTTP_PROTOCOL_SUPPORTED_IN_URL, L"Only HTTP protocol is supported for Microsoft Smooth Streaming." },

  // MPEG2 TS parser error codes

  { E_MPEG2TS_CANNOT_PARSE_PACKET, L"Cannot parse MPEG2 Transport Stream packet." },
  { E_MPEG2TS_EMPTY_SECTION_AND_PSI_PACKET_WITHOUT_NEW_SECTION, L"Empty section and Program Specific Information packet without new section." },
  { E_MPEG2TS_INCOMPLETE_SECTION, L"Incomplete section." },
  { E_MPEG2TS_SECTION_INVALID_CRC32, L"The CRC32 for section is invalid." },
  { E_MPEG2TS_CANNOT_SPLIT_SECTION_INTO_PSI_PACKETS, L"Cannot split section into Program Specific Information packets." },
  { E_MPEG2TS_SECTION_BIGGER_THAN_ORIGINAL_SECTION, L"Modified section has more Program Specific Information packets than original section." },
  { E_MPEG2TS_ONLY_ONE_PROGRAM_ALLOWED, L"Only one program allowed in MPEG2 Transport Stream." },

  // M3U8 parser error codes

  { E_M3U8_NOT_VALID_ITEM_FOUND, L"Not valid M3U8 item found." },
  { E_M3U8_NOT_VALID_GENERAL_TAG_FOUND, L"Not valid M3U8 general tag found." },
  { E_M3U8_NOT_VALID_PLAYLIST_ITEM_FOUND, L"Not valid M3U8 playlist item found." },
  { E_M3U8_NOT_VALID_TAG_FOUND, L"Not valid M3U8 tag found." },
  { E_M3U8_NOT_VALID_COMMENT_TAG_FOUND, L"Not valid M3U8 comment tag found." },
  { E_M3U8_NOT_PLAYLIST, L"Not M3U8 playlist." },
  { E_M3U8_NOT_SUPPORTED_PLAYLIST_VERSION, L"Not supported playlist version." },
  { E_M3U8_NOT_VALID_PLAYLIST, L"Not valid M3U8 playlist." },
  { E_M3U8_NOT_SUPPORTED_PLAYLIST_ITEM, L"Playlist item is not support in specified playlist version." },
  { E_M3U8_NOT_SUPPORTED_TAG, L"Tag is not support in specified playlist version." },
  { E_M3U8_NO_PLAYLIST_ITEM_FOR_TAG, L"Missing playlist item to apply tag." },

  // specific protocol error codes

  // HTTP protocol error codes

  { E_HTTP_CANNOT_SET_COOKIES, L"Cannot set HTTP cookies." },

  // RTSP protocol error codes

  { E_RTSP_NO_PUBLIC_OPTIONS_RESPONSE_HEADER, L"Response to OPTIONS request without PUBLIC header." },
  { E_RTSP_NOT_ALL_REQUIRED_METHODS, L"One of required methods (DESCRIBE, SETUP, PLAY and TEARDOWN) not specified PUBLIC header." },
  { E_RTSP_DESCRIBE_CONTENT_LENGTH_ZERO, L"DESCRIBE response content cannot be zero length." },
  { E_RTSP_DESCRIBE_CONTENT_TYPE_NOT_FOUND, L"Not found content type of DESCRIBE response." },
  { E_RTSP_CONTENT_HEADER_TYPE_NOT_ALLOWED, L"DESCRIBE response content type is not allowed." },

  { E_RTSP_SESSION_DESCRIPTION_PARSE_ERROR, L"Session description parse error." },
  { E_RTSP_NO_MEDIA_DESCRIPTIONS_IN_SESSION_DESCRIPTION, L"No media descriptions in session description, no stream to play." },
  { E_RTSP_NO_TRANSPORT_HEADER, L"No transport header in SETUP response." },
  { E_RTSP_NOT_TCP_TRANSPORT_HEADER, L"No TCP transport header in SETUP response." },
  { E_RTSP_SAME_CONNECTION_TRANSPORT_NOT_SUPPORTED, L"Interleaved stream is not supported by server." },

  { E_RTSP_BAD_OR_NOT_IMPLEMENTED_TRANSPORT, L"Bad or not implemented RTSP transport." },
  { E_RTSP_NO_TRACKS, L"No RTSP tracks to play." },
  { E_RTSP_NOT_UDP_TRANSPORT_HEADER, L"No UDP transport header in SETUP response." },
  { E_RTSP_CLIENT_PORTS_NOT_SAME_AS_REQUESTED, L"Client ports are not same as requested." },
  { E_RTSP_CANNOT_NEGOTIATE_ANY_TRANSPORT, L"Cannot negotiate any transport with RTSP server." },

  { E_RTSP_NOT_SPECIFIED_REQUEST_SEQUENCE_NUMBER, L"Not specified RTSP request sequence number." },
  { E_RTSP_NO_RESPONSE_FOR_REQUEST, L"No response for RTSP request." },
  { E_RTSP_BAD_SESSION_ID, L"Bad session ID in RTSP response." },
  { E_RTSP_NOT_SPECIFIED_RESPONSE_SEQUENCE_NUMBER, L"Not specified RTSP response sequence number." },
  { E_RTSP_REQUEST_AND_RESPONSE_SEQUENCE_NUMBERS_NOT_EQUAL, L"RTSP request and response sequence numbers not same." },

  { E_RTSP_STATUS_CODE_NOT_SUCCESS, L"RTSP status code not success." },
  { E_RTSP_NO_DATA_OR_CONTROL_CLIENT_PORT, L"Not client data port or client control port, unknown port." },
  { E_RTSP_NO_RTP_OR_RTCP_PACKET, L"Not RTP or RTCP packet." },
  { E_RTSP_INVALID_PACKET_FOR_PORT, L"Invalid packet for port." },
  { E_RTSP_NOT_INTERLEAVED_PACKET_NOT_VALID_RTSP_RESPONSE, L"Not interleaved packet and not valid RTSP response." },

  { E_RTSP_NOT_SET_SENDER_SYNCHRONIZATION_SOURCE_IDENTIFIER, L"Sender synchronization source identifier (SSRC) is not set." },
  { E_RTSP_NO_ENDPOINT_FOUND, L"Not endpoint found to send receiver report and source description." },
  { E_RTSP_SENT_DATA_LENGTH_NOT_SAME_AS_RTCP_PACKET_LENGTH, L"Not send all data for receiver report and source description." },
  { E_RTSP_NO_DATA_RECEIVED_BETWEEN_RECEIVER_REPORTS, L"No data received between receiver reports." },

  // UDP/RTP protocol error codes

  { E_UDP_NO_DATA_RECEIVED, L"No data received for specified amount of time." },

  // RTMP protocol error codes

  { E_RTMP_BAD_URL_FORMAT, L"Bad URL format for RTMP protocol." },
  { E_RTMP_CONNECT_FAILED, L"RTMP connect to remote server failed." },
  { E_RTMP_CONNECT_STREAM_FAILED, L"Cannot connect to RTMP stream." },
  { E_RTMP_GENERAL_READ_ERROR, L"General RTMP read error." },


  // AFHS protocol error codes

  { E_AFHS_NO_DECRYPTOR_LOADED, L"No decryptor loaded." },
  { E_AFHS_CANNOT_PARSE_BOOTSTRAP_INFO_BOX, L"Cannot parse bootstrap info." },
  { E_AFHS_CANNOT_GET_SEGMENT_FRAGMENTS_FROM_BOOTSTRAP_INFO_BOX, L"Cannot get segments and fragments from bootstrap info box." },
  { E_AFHS_NOT_FOUND_SEGMENT_FRAGMENT_IN_LIVE_STREAM, L"Not found segment and fragment in live stream to play." },
  { E_AFHS_CANNOT_DECODE_METADATA, L"cannot decode metadata." },
  { E_AFHS_CANNOT_CREATE_METADATA_FLV_PACKET, L"Cannot create metadata FLV packet." },
  { E_AFHS_CANNOT_PARSE_BOX, L"Cannot parse box." },
  { E_AFHS_CANNOT_PARSE_MEDIA_DATA_BOX, L"Cannot parse media data box." },
  { E_AFHS_BOX_SIZE_ZERO, L"Box size cannot be zero." },
  { E_AFHS_DECRYPTED_DATA_SIZE_ZERO, L"Decrypted data size cannot be zero length." },
  { E_AFHS_NO_ACTIVE_DECRYPTOR, L"No active decryptor for stream." },
  { E_AFHS_DECRYPTOR_STILL_PENDING, L"At least one decryptor still doesn't return known stream or unknown stream state." },

  // AFHS akamai decryptor error codes

  { E_AFHS_AKAMAI_DECRYPTOR_CANNOT_LOAD_DECRYPTOR, L"Cannot get decryptor from internal resources." },
  { E_AFHS_AKAMAI_DECRYPTOR_NO_DECRYPTOR_FILE_NAME, L"Cannot get decryptor file name." },
  { E_AFHS_AKAMAI_DECRYPTOR_CANNOT_SAVE_DECRYPTOR, L"Cannot save decryptor to file system." },
  { E_AFHS_AKAMAI_DECRYPTOR_GENERAL_ERROR, L"General decryptor error." },
  { E_AFHS_AKAMAI_DECRYPTOR_INVALID_COUNT_OF_ENCRYPTED_SEGMENT_FRAGMENTS, L"Encrypted segment and fragment count must be greater than zero." },
  { E_AFHS_AKAMAI_DECRYPTOR_NOT_FLV_PACKET, L"Not FLV packet." },
  { E_AFHS_AKAMAI_DECRYPTOR_NOT_AKAMAI_FLV_PACKET, L"No Akamai FLV packet." },
  { E_AFHS_AKAMAI_DECRYPTOR_NOT_FOUND_MEDIA_DATA_BOX, L"Not found media data box in segment fragment." },
  { E_AFHS_AKAMAI_DECRYPTOR_NOT_CREATED_BOX, L"Not craeted box, probably invalid box in stream." },
  { E_AFHS_AKAMAI_DECRYPTOR_INVALID_KEY_LENGTH, L"Decryption key length must be greater than zero." },
  { E_AFHS_AKAMAI_DECRYPTOR_DECRYPTED_DATA_NOT_EQUAL_TO_ENCRYPTED_DATA, L"Decrypted FLV packet count is not equal to encrypted FLV packet count." },
  { E_AFHS_AKAMAI_DECRYPTOR_CANNOT_CREATE_DECRYPTED_FLV_PACKET, L"Cannot create decrypted FLV packet." },

  // MSHS protocol error codes

  { E_MSHS_NO_VIDEO_AND_AUDIO_FRAGMENT, L"Stream without audio and video." },
  { E_MSHS_NO_FRAGMENT_URL, L"Stream fragment without URL." },
  { E_MSHS_CANNOT_PARSE_STREAMING_MEDIA_BOX, L"Cannot parse streaming media box." },
  { E_MSHS_CANNOT_PARSE_MOVIE_FRAGMENT_BOX, L"Cannot parse movie fragment box." },
  { E_MSHS_CANNOT_GET_TRACK_HEADER_FRAGMENT_BOX_FROM_MOVIE_FRAGMENT_BOX, L"Cannot get track header fragment box from movie fragment box." },
  { E_MSHS_CANNOT_PARSE_TRACK_FRAGMENT_HEADER_BOX, L"Cannot parse track fragment header box." },
  { E_MSHS_NOT_FOUND_SPS_START, L"Not found SPS start in codec private data." },
  { E_MSHS_NOT_FOUND_PPS_START, L"Not found PPS start in codec private data." },
  { E_MSHS_CANNOT_CONVERT_SPS, L"Cannot convert SPS from hexadecimal notation." },
  { E_MSHS_CANNOT_CONVERT_PPS, L"Cannot convert PPS from hexadecimal notation." },
  { E_MSHS_INVALID_FRAGMENT_SIZE, L"Stream fragment size must be greater than zero." },
  { E_MSHS_INVALID_BOX, L"Invalid box in stream fragment." },
  { E_MSHS_INVALID_MOVIE_FRAGMENT_BOX, L"Movie fragment box size must be greater than zero." },
  { E_MSHS_CANNOT_GET_VIDEO_FRAGMENT_INDEX, L"Cannot get video fragment index." },
  { E_MSHS_CANNOT_GET_AUDIO_FRAGMENT_INDEX, L"Cannot get audio fragment index." },

  // M3U8 protocol error codes

  { E_M3U8_CANNOT_GET_STREAM_FRAGMENTS_FROM_MEDIA_PLAYLIST, L"Cannot get stream fragments from media playlist." },

  // last item
  { 0, NULL }
};

#endif
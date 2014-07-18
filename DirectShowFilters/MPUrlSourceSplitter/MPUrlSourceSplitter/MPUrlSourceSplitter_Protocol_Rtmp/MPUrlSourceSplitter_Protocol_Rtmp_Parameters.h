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

#ifndef __MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_PARAMETERS_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_PARAMETERS_DEFINED

/* CONNECTION PARAMETERS */

// These options define the content of the RTMP Connect request packet.
// If correct values are not provided, the media server will reject the connection attempt.

// Name of application to connect to on the RTMP server.
// Overrides the app in the RTMP URL.
// Sometimes the librtmp URL parser cannot determine the app name automatically,
// so it must be given explicitly using this option. 
#define PARAMETER_NAME_RTMP_APP                                               L"RtmpApp"

// URL of the target stream. Defaults to rtmp[t][e|s]://host[:port]/app. 
#define PARAMETER_NAME_RTMP_TC_URL                                            L"RtmpTcUrl"

// URL of the web page in which the media was embedded. By default no value will be sent. 
#define PARAMETER_NAME_RTMP_PAGE_URL                                          L"RtmpPageUrl"

// URL of the SWF player for the media. By default no value will be sent.
#define PARAMETER_NAME_RTMP_SWF_URL                                           L"RtmpSwfUrl"

// Version of the Flash plugin used to run the SWF player. The default is "WIN 10,0,32,18".
#define PARAMETER_NAME_RTMP_FLASHVER                                          L"RtmpFlashVer"

// Authentication string to be appended to the connect string
#define PARAMETER_NAME_RTMP_AUTH                                              L"RtmpAuth"

// Append arbitrary AMF data to the Connect message.
// The type must be B for Boolean, N for number, S for string, O for object, or Z for null.
// For Booleans the data must be either 0 or 1 for FALSE or TRUE, respectively.
// Likewise for Objects the data must be 0 or 1 to end or begin an object, respectively.
// Data items in subobjects may be named, by prefixing the type with 'N' and specifying the name before the value,
// e.g. NB:myFlag:1. This option may be used multiple times to construct arbitrary AMF sequences. E.g.
// conn=B:1 conn=S:authMe conn=O:1 conn=NN:code:1.23 conn=NS:flag:ok conn=O:0
#define PARAMETER_NAME_RTMP_ARBITRARY_DATA                                    L"RtmpArbitraryData"

/* SESSION PARAMETERS */

// These options take effect after the Connect request has succeeded.

// Overrides the playpath parsed from the RTMP URL.
// Sometimes the rtmpdump URL parser cannot determine the correct playpath automatically,
// so it must be given explicitly using this option.
#define PARAMETER_NAME_RTMP_PLAY_PATH                                         L"RtmpPlayPath"

// If the value is 1 or TRUE, issue a set_playlist command before sending the play command.
// The playlist will just contain the current playpath.
// If the value is 0 or FALSE, the set_playlist command will not be sent.
// The default is FALSE. 
#define PARAMETER_NAME_RTMP_PLAYLIST                                          L"RtmpPlaylist"

// Specify that the media is a live stream. No resuming or seeking in live streams is possible.
#define PARAMETER_NAME_RTMP_LIVE                                              L"RtmpLive"

// Name of live stream to subscribe to. Defaults to playpath.
#define PARAMETER_NAME_RTMP_SUBSCRIBE                                         L"RtmpSubscribe"

// Set buffer time to num milliseconds.
#define PARAMETER_NAME_RTMP_BUFFER                                            L"RtmpBuffer"

// Timeout the session after num seconds without receiving any data from the server.
//#define PARAMETER_NAME_RTMP_RECEIVE_DATA_TIMEOUT                            L"RtmpReceiveDataTimeout"

/* SECURITY PARAMETERS */

// These options handle additional authentication requests from the server.

// Key for SecureToken response, used if the server requires SecureToken authentication.
#define PARAMETER_NAME_RTMP_TOKEN                                             L"RtmpToken"

// JSON token used by legacy Justin.tv servers. Invokes NetStream.Authenticate.UsherToken
#define PARAMETER_NAME_RTMP_JTV                                               L"RtmpJtv"

// If the value is 1 or TRUE, the SWF player is retrieved from the specified swfUrl for performing SWF Verification.
// The SWF hash and size (used in the verification step) are computed automatically.
// Also the SWF information is cached in a .swfinfo file in the user's home directory,
// so that it doesn't need to be retrieved and recalculated every time.
// The .swfinfo file records the SWF URL, the time it was fetched,
// the modification timestamp of the SWF file, its size, and its hash.
// By default, the cached info will be used for 30 days before re-checking. 
#define PARAMETER_NAME_RTMP_SWF_VERIFY                                        L"RtmpSwfVerify"

#define PARAMETER_NAME_RTMP_OPEN_CONNECTION_TIMEOUT                           L"RtmpOpenConnectionTimeout"
#define PARAMETER_NAME_RTMP_OPEN_CONNECTION_SLEEP_TIME                        L"RtmpOpenConnectionSleepTime"
#define PARAMETER_NAME_RTMP_TOTAL_REOPEN_CONNECTION_TIMEOUT                   L"RtmpTotalReopenConnectionTimeout"

// define default values for RTMP protocol

#define RTMP_APP_DEFAULT                                                      NULL
#define RTMP_TC_URL_DEFAULT                                                   NULL
#define RTMP_PAGE_URL_DEFAULT                                                 NULL
#define RTMP_SWF_URL_DEFAULT                                                  NULL
#define RTMP_FLASH_VER_DEFAULT                                                NULL
#define RTMP_AUTH_DEFAULT                                                     NULL
#define RTMP_ARBITRARY_DATA_DEFAULT                                           NULL
#define RTMP_PLAY_PATH_DEFAULT                                                NULL
#define RTMP_PLAYLIST_DEFAULT                                                 false
#define RTMP_LIVE_DEFAULT                                                     false
#define RTMP_SUBSCRIBE_DEFAULT                                                NULL
#define RTMP_START_DEFAULT                                                    0
#define RTMP_STOP_DEFAULT                                                     INT64_MAX
#define RTMP_BUFFER_DEFAULT                                                   30000
#define RTMP_TOKEN_DEFAULT                                                    NULL
#define RTMP_JTV_DEFAULT                                                      NULL
#define RTMP_SWF_VERIFY_DEFAULT                                               false

// we should get data in twenty seconds
#define RTMP_OPEN_CONNECTION_TIMEOUT_DEFAULT                                  20000
#define RTMP_OPEN_CONNECTION_SLEEP_TIME_DEFAULT                               0
#define RTMP_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT                          60000

#endif

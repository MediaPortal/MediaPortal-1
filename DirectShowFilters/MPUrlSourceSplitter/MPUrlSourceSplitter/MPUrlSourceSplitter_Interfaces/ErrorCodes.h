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

#define E_INVALID_CONFIGURATION                                                 -1
#define E_URL_NOT_SPECIFIED                                                     -2
#define E_RESULT_DATA_LENGTH_BIGGER_THAN_REQUESTED                              -3
#define E_NO_MORE_DATA_AVAILABLE                                                -4
#define E_REQUESTED_DATA_AFTER_TOTAL_LENGTH                                     -5
#define E_TIMEOUT                                                               VFW_E_TIMEOUT
#define E_DEMUXER_WORKER_STOP_REQUEST                                           -6

// parser hoster error codes

#define E_NO_DATA_AVAILABLE                                                     -20
#define E_PARSER_STILL_PENDING                                                  -21
#define E_DRM_PROTECTED                                                         -22
#define E_UNKNOWN_STREAM_TYPE                                                   -23
#define E_CONNECTION_LOST_CANNOT_REOPEN                                         -24

// protocol hoster error codes

#define E_NO_PROTOCOL_LOADED                                                    -30
#define E_NO_ACTIVE_PROTOCOL                                                    -31

// IFilterState interface

// if demuxer is not created, all data are received and demuxer worker finished its work
#define E_DEMUXER_NOT_CREATED_ALL_DATA_RECEIVED_DEMUXER_WORKER_FINISHED         -10
#define E_CONVERT_STRING_ERROR                                                  -11


#endif
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

#ifndef __MP_URL_SOURCE_SPLITTER_PARSER_MSHS_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PARSER_MSHS_DEFINED

#include "IParserPlugin.h"

#include "MSHSTrack.h"

#define PARSER_NAME                                                           L"MSHS"

#define TOTAL_SUPPORTED_VIDEO_TRACKS                                          1
wchar_t *SUPPORTED_VIDEO_TRACKS[TOTAL_SUPPORTED_VIDEO_TRACKS] =               { MSHS_FOURCC_H264 };

#define TOTAL_SUPPORTED_AUDIO_TRACKS                                          1
wchar_t *SUPPORTED_AUDIO_TRACKS[TOTAL_SUPPORTED_AUDIO_TRACKS] =               { MSHS_FOURCC_AACL };

class CMPUrlSourceSplitter_Parser_MSHS : public IParserPlugin
{
public:
  // constructor
  // create instance of CMPUrlSourceSplitter_Parser_MSHS class
  CMPUrlSourceSplitter_Parser_MSHS(CLogger *logger, CParameterCollection *configuration);

  // destructor
  ~CMPUrlSourceSplitter_Parser_MSHS(void);

  // IParser interface

  // clears current parser session
  // @return : S_OK if successfull
  HRESULT ClearSession(void);

  // parses media packets
  // @param streamId : the stream ID to parse media packets
  // @param mediaPackets : media packet collection to parse
  // @param connectionParameters : current connection parameters
  // @return : one of ParseResult values
  ParseResult ParseMediaPackets(unsigned int streamId, CMediaPacketCollection *mediaPackets, CParameterCollection *connectionParameters);

  // sets current connection url and parameters
  // @param parameters : the collection of url and connection parameters
  // @return : S_OK if successful
  HRESULT SetConnectionParameters(const CParameterCollection *parameters);

  // gets parser action after parser recognizes pattern in stream
  // @return : one of Action values
  Action GetAction(void);

  // gets (fills) connection url and parameters
  // @param parameters : the collection of url and connection parameters to fill
  // @return : S_OK if successful
  HRESULT GetConnectionParameters(CParameterCollection *parameters);

  // IPlugin interface

  // return reference to null-terminated string which represents plugin name
  // function have to allocate enough memory for plugin name string
  // errors should be logged to log file and returned NULL
  // @return : reference to null-terminated string
  const wchar_t *GetName(void);

  // get plugin instance ID
  // @return : GUID, which represents instance identifier or GUID_NULL if error
  GUID GetInstanceId(void);

  // initialize plugin implementation with configuration parameters
  // @param configuration : the reference to additional configuration parameters (created by plugin's hoster class)
  // @return : S_OK if successfull
  HRESULT Initialize(PluginConfiguration *configuration);

protected:
  CLogger *logger;

  // holds connection parameters
  CParameterCollection *connectionParameters;

  // holds stored media packets
  CMediaPacketCollection *storedMediaPackets;
};

#endif

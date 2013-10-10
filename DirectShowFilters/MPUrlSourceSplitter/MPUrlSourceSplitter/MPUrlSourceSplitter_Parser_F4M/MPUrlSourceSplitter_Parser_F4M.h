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

#ifndef __MPURLSOURCESPLITTER_PARSER_F4M_DEFINED
#define __MPURLSOURCESPLITTER_PARSER_F4M_DEFINED

#include "IParserPlugin.h"

#define PARSER_NAME                                               L"F4M"

class CMPUrlSourceSplitter_Parser_F4M : public IParserPlugin
{
public:
  // constructor
  // create instance of CMPUrlSourceSplitter_Parser_F4M class
  CMPUrlSourceSplitter_Parser_F4M(CLogger *logger, CParameterCollection *configuration);

  // destructor
  ~CMPUrlSourceSplitter_Parser_F4M(void);

  // IParser interface

  // clears current parser session
  // @return : S_OK if successfull
  HRESULT ClearSession(void);

  // parses media packets
  // @param mediaPackets : media packet collection to parse
  // @param connectionParameters : current connection parameters
  // @return : one of ParseResult values
  ParseResult ParseMediaPackets(CMediaPacketCollection *mediaPackets, CParameterCollection *connectionParameters);

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

  // gets stored media packets (in case that parser plugin returned ParseResult_Pending)
  // @return : stored media packets collection
  CMediaPacketCollection *GetStoredMediaPackets(void);

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

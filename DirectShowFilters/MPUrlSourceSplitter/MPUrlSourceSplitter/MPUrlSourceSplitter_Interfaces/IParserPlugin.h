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

#ifndef __IPARSER_PLUGIN_DEFINED
#define __IPARSER_PLUGIN_DEFINED

#include "IPlugin.h"
#include "MediaPacketCollection.h"

#define METHOD_PARSE_MEDIA_PACKETS_NAME                                           L"ParseMediaPackets()"
#define METHOD_CLEAR_SESSION_NAME                                                 L"ClearSession()"

struct ParserPluginConfiguration : public PluginConfiguration
{
};

enum ParseResult
{
  ParseResult_Unspecified,
  ParseResult_NotKnown,
  ParseResult_Pending,
  ParseResult_Known,
  ParseResult_DrmProtected
} ;

enum Action
{
  Action_Unspecified,
  Action_GetNewConnection
};

// defines interface for stream parser implementation
// each stream parser implementation will be in separate library and MUST implement this interface
struct IParserPlugin : public IPlugin
{
  // clears current parser session
  // @return : S_OK if successfull
  virtual HRESULT ClearSession(void) = 0;

  // parses media packets
  // @param mediaPackets : media packet collection to parse
  // @param connectionParameters : current connection parameters
  // @return : one of ParseResult values
  virtual ParseResult ParseMediaPackets(CMediaPacketCollection *mediaPackets, CParameterCollection *connectionParameters) = 0;

  // sets current connection url and parameters
  // @param parameters : the collection of url and connection parameters
  // @return : S_OK if successful
  virtual HRESULT SetConnectionParameters(const CParameterCollection *parameters) = 0;

  // gets parser action after parser recognizes pattern in stream
  // @return : one of Action values
  virtual Action GetAction(void) = 0;

  // gets (fills) connection url and parameters
  // @param parameters : the collection of url and connection parameters to fill
  // @return : S_OK if successful
  virtual HRESULT GetConnectionParameters(CParameterCollection *parameters) = 0;

  // gets stored media packets (in case that parser plugin returned ParseResult_Pending)
  // @return : stored media packets collection
  virtual CMediaPacketCollection *GetStoredMediaPackets(void) = 0;
};

typedef IParserPlugin* PIParserPlugin;

#endif
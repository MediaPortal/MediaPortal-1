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

#ifndef __PARSER_PLUGIN_DEFINED
#define __PARSER_PLUGIN_DEFINED

#include "Plugin.h"
#include "IDemuxerOwner.h"
#include "ProtocolHoster.h"
#include "IProtocol.h"

#define METHOD_GET_PARSER_RESULT_NAME                                 L"GetParserResult()"

#define PARSER_PLUGIN_FLAG_NONE                                       PLUGIN_FLAG_NONE

#define PARSER_PLUGIN_FLAG_LAST                                       (PLUGIN_FLAG_LAST + 0)

#define PARSER_RESULT_PENDING                                         1
#define PARSER_RESULT_NOT_KNOWN                                       2
#define PARSER_RESULT_KNOWN                                           S_OK
#define PARSER_RESULT_DRM_PROTECTED                                   E_DRM_PROTECTED

class CParserPlugin : public CPlugin, virtual public IDemuxerOwner, virtual public IProtocol
{
public:
  CParserPlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  virtual ~CParserPlugin(void);

  enum Action
  {
    ParseStream,
    GetNewConnection
  };

  // CPlugin

  // initialize plugin implementation with configuration parameters
  // @param configuration : the reference to additional configuration parameters (created by plugin's hoster class)
  // @return : S_OK if successfull, error code otherwise
  virtual HRESULT Initialize(CPluginConfiguration *configuration);

  // IDemuxerOwner interface

  // gets duration of stream in ms
  // @return : stream duration in ms or DURATION_LIVE_STREAM in case of live stream or DURATION_UNSPECIFIED if duration is unknown
  virtual int64_t GetDuration(void) = 0;

  // retrieves the progress of the stream reading operation
  // @param streamProgress : reference to instance of class that receives the stream progress
  // @return : S_OK if successful, VFW_S_ESTIMATED if returned values are estimates, E_INVALIDARG if stream ID is unknown, E_UNEXPECTED if unexpected error
  virtual HRESULT QueryStreamProgress(CStreamProgress *streamProgress) = 0;

  // IProtocol interface

  // gets current connection parameters (can be different as supplied connection parameters)
  // @param parameters : the reference to parameter collection to be filled with connection parameters
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT GetConnectionParameters(CParameterCollection *parameters);

  // ISimpleProtocol interface

  // clear current session
  // @return : S_OK if successfull
  virtual HRESULT ClearSession(void);

  // ISeeking interface

  /* get methods */

  // gets parser result about current stream
  // @return : one of PARSER_RESULT values
  virtual HRESULT GetParserResult(void);

  // gets parser score if parser result is PARSER_RESULT_KNOWN
  // @return : parser score (parser with highest score is set as active parser)
  virtual unsigned int GetParserScore(void) = 0;

  // gets parser action after parser recognizes stream
  // @return : one of Action values
  virtual Action GetAction(void) = 0;

  /* set methods */

  // sets current connection url and parameters
  // @param parameters : the collection of url and connection parameters
  // @return : S_OK if successful
  virtual HRESULT SetConnectionParameters(const CParameterCollection *parameters);

  /* other methods */

protected:
  // holds logger instance
  CLogger *logger;
  // holds configuration
  CParameterCollection *configuration;
  // holds protocol hoster - only reference, do not cleanup !
  CProtocolHoster *protocolHoster;
  // holds parser result
  HRESULT parserResult;
  // holds connection parameters
  CParameterCollection *connectionParameters;

  /* methods */
};

#endif
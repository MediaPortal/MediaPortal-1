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

#include "ParserPlugin.h"
#include "MshsManifest.h"

#define PARSER_NAME                                                   L"PARSER_MSHS"

#define TOTAL_SUPPORTED_VIDEO_TRACKS                                  1
wchar_t *SUPPORTED_VIDEO_TRACKS[TOTAL_SUPPORTED_VIDEO_TRACKS] =       { MSHS_FOURCC_H264 };

#define TOTAL_SUPPORTED_AUDIO_TRACKS                                  1
wchar_t *SUPPORTED_AUDIO_TRACKS[TOTAL_SUPPORTED_AUDIO_TRACKS] =       { MSHS_FOURCC_AACL };

class CMPUrlSourceSplitter_Parser_Mshs : public CParserPlugin
{
public:
  // constructor
  // create instance of CMPUrlSourceSplitter_Parser_Mshs class
  CMPUrlSourceSplitter_Parser_Mshs(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  // destructor
  virtual ~CMPUrlSourceSplitter_Parser_Mshs(void);

  // CParserPlugin

  // gets parser result about current stream
  // @return : one of ParserResult values
  virtual HRESULT GetParserResult(void);

  // gets parser score if parser result is Known
  // @return : parser score (parser with highest score is set as active parser)
  virtual unsigned int GetParserScore(void);

  // gets parser action after parser recognizes stream
  // @return : one of Action values
  virtual Action GetAction(void);

  // CPlugin

  // return reference to null-terminated string which represents plugin name
  // errors should be logged to log file and returned NULL
  // @return : reference to null-terminated string
  virtual const wchar_t *GetName(void);

  // initialize plugin implementation with configuration parameters
  // @param configuration : the reference to additional configuration parameters (created by plugin's hoster class)
  // @return : S_OK if successfull, error code otherwise
  virtual HRESULT Initialize(CPluginConfiguration *configuration);

  // ISeeking interface

  // IDemuxerOwner interface

  // ISimpleProtocol interface

  // clears current session
  virtual void ClearSession(void);

  // IProtocol interface

protected:

  unsigned int lastReceivedLength;
};

#endif

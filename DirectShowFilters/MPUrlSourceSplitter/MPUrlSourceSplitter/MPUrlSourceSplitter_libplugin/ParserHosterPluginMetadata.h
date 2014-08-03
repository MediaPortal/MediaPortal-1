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

#ifndef __PARSER_HOSTER_PLUGIN_METADATA_DEFINED
#define __PARSER_HOSTER_PLUGIN_METADATA_DEFINED

#include "HosterPluginMetadata.h"
#include "ParserPlugin.h"

class CParserHosterPluginMetadata : public CHosterPluginMetadata
{
public:
  CParserHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName);
  virtual ~CParserHosterPluginMetadata(void);

  /* get methods */

  // gets parser result about current stream
  // @return : one of ParserResult values
  virtual HRESULT GetParserResult(void);

  // gets parser score if parser result is Known
  // @return : parser score (parser with highest score is set as active parser)
  virtual unsigned int GetParserScore(void);

  /* set methods */

  /* other methods */

  // clear current session
  // @return : S_OK if successfull
  virtual HRESULT ClearSession(void);

  // tests if last parser result is Pending
  // @return : true if last parser result is Pending, false otherwise
  virtual bool IsParserStillPending(void);

protected:
  // holds parser result
  HRESULT parserResult;

  /* methods */
};

#endif
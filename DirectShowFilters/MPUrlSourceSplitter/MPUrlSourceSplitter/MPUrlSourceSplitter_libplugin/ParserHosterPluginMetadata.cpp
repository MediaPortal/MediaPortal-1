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

#include "StdAfx.h"

#pragma warning(push)
// disable warning: 'INT8_MIN' : macro redefinition
// warning is caused by stdint.h and intsafe.h, which both define same macro
#pragma warning(disable:4005)

#include "ParserHosterPluginMetadata.h"
#include "ParserPlugin.h"
#include "ErrorCodes.h"

#pragma warning(pop)

CParserHosterPluginMetadata::CParserHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName)
  : CHosterPluginMetadata(result, logger, configuration, hosterName, pluginLibraryFileName)
{
  this->parserResult = PARSER_RESULT_PENDING;
}

CParserHosterPluginMetadata::~CParserHosterPluginMetadata(void)
{
}

/* get methods */

HRESULT CParserHosterPluginMetadata::GetParserResult(void)
{
  if (this->parserResult == PARSER_RESULT_PENDING)
  {
    CParserPlugin *parser = dynamic_cast<CParserPlugin *>(this->plugin);

    this->parserResult = (parser != NULL) ? parser->GetParserResult() : PARSER_RESULT_PENDING;
  }

  return this->parserResult;
}

unsigned int CParserHosterPluginMetadata::GetParserScore(void)
{
  CParserPlugin *parser = dynamic_cast<CParserPlugin *>(this->plugin);

  return (parser != NULL) ? parser->GetParserScore() : 0;
}

/* set methods */

/* other methods */

HRESULT CParserHosterPluginMetadata::CheckPlugin(void)
{
  CParserPlugin *parserPlugin = dynamic_cast<CParserPlugin *>(this->plugin);

  return (parserPlugin != NULL) ? S_OK : E_INVALID_PLUGIN_TYPE;
}

void CParserHosterPluginMetadata::ClearSession(void)
{
  __super::ClearSession();

  this->parserResult = PARSER_RESULT_PENDING;
}

bool CParserHosterPluginMetadata::IsParserStillPending(void)
{
  return (this->parserResult == PARSER_RESULT_PENDING);
}

/* protected methods */

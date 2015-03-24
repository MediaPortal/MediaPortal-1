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

#include "ParserPluginConfiguration.h"

CParserPluginConfiguration::CParserPluginConfiguration(HRESULT *result, CParameterCollection *configuration, CProtocolHoster *protocolHoster)
  : CPluginConfiguration(result, configuration)
{
  this->protocolHoster = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, protocolHoster);

    if (SUCCEEDED(*result))
    {
      this->protocolHoster = protocolHoster;
    }
  }
}

CParserPluginConfiguration::~CParserPluginConfiguration(void)
{
}

/* get methods */

CProtocolHoster *CParserPluginConfiguration::GetProtocolHoster(void)
{
  return this->protocolHoster;
}

/* set methods */

/* other methods */

/* protected methods */

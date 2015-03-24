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

#include "Plugin.h"

#pragma warning(pop)

CPlugin::CPlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CFlags()
{
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, logger);
    CHECK_POINTER_DEFAULT_HRESULT(*result, configuration);
  }
}

CPlugin::~CPlugin(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CPlugin::IsSplitter(void)
{
  return this->IsSetFlags(PLUGIN_FLAG_SPLITTER);
}

bool CPlugin::IsIptv(void)
{
  return this->IsSetFlags(PLUGIN_FLAG_IPTV);
}

void CPlugin::ClearSession(void)
{
  this->flags = PLUGIN_FLAG_NONE;
}

/* protected methods */

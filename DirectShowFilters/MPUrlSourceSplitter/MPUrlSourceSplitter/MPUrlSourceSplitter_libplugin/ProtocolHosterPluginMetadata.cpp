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

#include "ProtocolHosterPluginMetadata.h"

CProtocolHosterPluginMetadata::CProtocolHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName)
  : CHosterPluginMetadata(result, logger, configuration, hosterName, pluginLibraryFileName)
{
}

CProtocolHosterPluginMetadata::~CProtocolHosterPluginMetadata(void)
{
}

/* get methods */

/* set methods */

void CProtocolHosterPluginMetadata::SetSupported(bool supported)
{
  this->flags &= ~PROTOCOL_HOSTER_PLUGIN_METADATA_FLAG_SUPPORTED;
  this->flags |= (supported) ? PROTOCOL_HOSTER_PLUGIN_METADATA_FLAG_SUPPORTED : PROTOCOL_HOSTER_PLUGIN_METADATA_FLAG_NONE;
}

/* other methods */

bool CProtocolHosterPluginMetadata::IsSupported(void)
{
  return this->IsSetFlags(PROTOCOL_HOSTER_PLUGIN_METADATA_FLAG_SUPPORTED);
}

/* protected methods */

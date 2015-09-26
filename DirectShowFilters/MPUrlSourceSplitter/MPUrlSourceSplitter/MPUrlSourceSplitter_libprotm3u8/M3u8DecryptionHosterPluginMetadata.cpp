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

#include "M3u8DecryptionHosterPluginMetadata.h"
#include "M3u8DecryptionPlugin.h"
#include "ErrorCodes.h"

#pragma warning(pop)

CM3u8DecryptionHosterPluginMetadata::CM3u8DecryptionHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName)
  : CHosterPluginMetadata(result, logger, configuration, hosterName, pluginLibraryFileName)
{
}

CM3u8DecryptionHosterPluginMetadata::~CM3u8DecryptionHosterPluginMetadata(void)
{
}

/* get methods */

/* set methods */

/* other methods */

HRESULT CM3u8DecryptionHosterPluginMetadata::CheckPlugin(void)
{
  CM3u8DecryptionPlugin *decryptionPlugin = dynamic_cast<CM3u8DecryptionPlugin *>(this->plugin);

  return (decryptionPlugin != NULL) ? S_OK : E_INVALID_PLUGIN_TYPE;
}

void CM3u8DecryptionHosterPluginMetadata::ClearSession(void)
{
  __super::ClearSession();
}

/* protected methods */

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

#ifndef __M3U8_DECRYPTION_HOSTER_PLUGIN_METADATA_DEFINED
#define __M3U8_DECRYPTION_HOSTER_PLUGIN_METADATA_DEFINED

#include "HosterPluginMetadata.h"
#include "M3u8DecryptionPlugin.h"
#include "M3u8DecryptionContext.h"

class CM3u8DecryptionHosterPluginMetadata : public CHosterPluginMetadata
{
public:
  CM3u8DecryptionHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName);
  virtual ~CM3u8DecryptionHosterPluginMetadata(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // checks plugin (basicly checks plugin type)
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT CheckPlugin(void);

  // clears current session
  virtual void ClearSession(void);

protected:

  /* methods */
};

#endif
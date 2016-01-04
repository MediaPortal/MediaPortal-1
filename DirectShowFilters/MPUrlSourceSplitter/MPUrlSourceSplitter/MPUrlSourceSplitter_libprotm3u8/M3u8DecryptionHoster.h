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

#ifndef __M3U8_DECRYPTION_HOSTER_DEFINED
#define __M3U8_DECRYPTION_HOSTER_DEFINED

#include "Hoster.h"
#include "M3u8DecryptionPlugin.h"
#include "M3u8DecryptionContext.h"

#define MODULE_M3U8_DECRYPTION_HOSTER_NAME                            L"M3u8DecryptionHoster"

#define M3U8_DECRYPTION_HOSTER_FLAG_NONE                              HOSTER_FLAG_NONE

#define M3U8_DECRYPTION_HOSTER_FLAG_LAST                              (HOSTER_FLAG_LAST + 0)

class CM3u8DecryptionHoster : public CHoster
{
public:
  CM3u8DecryptionHoster(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  virtual ~CM3u8DecryptionHoster(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // clears current session
  virtual void ClearSession(void);

  // loads plugins from directory
  // @return : S_OK if successful, E_M3U8_NO_DECRYPTOR_LOADED if no decryptor loaded, error code otherwise
  virtual HRESULT LoadPlugins(void);

  // decrypts encrypted stream fragments
  // @param decryptionContext : M3U8 decryption context
  // @return : S_OK if successful, E_M3U8_DECRYPTION_METHOD_NOT_SUPPORTED if on first encrypted stream fragment is not supported encryption method, error code otherwise
  HRESULT DecryptStreamFragments(CM3u8DecryptionContext *decryptionContext);

protected:

  /* methods */

  // creates hoster plugin metadata
  // @param result : the reference to result
  // @param logger : the reference to logger
  // @param configuration : the reference to configuration
  // @param hosterName : the hoster name
  // @param pluginLibraryFileName : the plugin library file name
  // @return : hoster plugin metadata instance
  virtual CHosterPluginMetadata *CreateHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName);

  // creates plugin configuration
  // @param result : the reference to result
  // @param configuration : the collection of parameters
  // @return : plugin configuration instance
  virtual CPluginConfiguration *CreatePluginConfiguration(HRESULT *result, CParameterCollection *configuration);
};

#endif
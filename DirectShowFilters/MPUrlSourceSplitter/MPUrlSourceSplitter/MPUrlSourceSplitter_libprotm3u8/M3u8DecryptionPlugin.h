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

#ifndef __M3U8_DECRYPTION_PLUGIN_DEFINED
#define __M3U8_DECRYPTION_PLUGIN_DEFINED

#include "Plugin.h"
#include "M3u8DecryptionContext.h"

#define METHOD_DECRYPT_STREAM_FRAGMENTS_NAME                          L"DecryptStreamFragments()"

#define M3U8_DECRYPTION_PLUGIN_FLAG_NONE                              PLUGIN_FLAG_NONE

#define M3U8_DECRYPTION_PLUGIN_FLAG_LAST                              (PLUGIN_FLAG_LAST + 0)

class CM3u8DecryptionPlugin : public CPlugin
{
public:
  CM3u8DecryptionPlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  virtual ~CM3u8DecryptionPlugin();

  // CPlugin

  // initialize plugin implementation with configuration parameters
  // @param configuration : the reference to additional configuration parameters (created by plugin's hoster class)
  // @return : S_OK if successfull, error code otherwise
  virtual HRESULT Initialize(CPluginConfiguration *configuration);

  /* get methods */

  /* set methods */

  /* other methods */

  // decrypts encrypted stream fragments
  // @param decryptionContext : M3U8 decryption context
  // @return : S_OK if successful, E_M3U8_DECRYPTION_METHOD_NOT_SUPPORTED if on first encrypted stream fragment is not supported encryption method, error code otherwise
  virtual HRESULT DecryptStreamFragments(CM3u8DecryptionContext *decryptionContext) = 0;

protected:

  /* methods */
};

#endif
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

#ifndef __AFHS_DECRYPTION_HOSTER_DEFINED
#define __AFHS_DECRYPTION_HOSTER_DEFINED

#include "Hoster.h"
#include "AfhsDecryptionPlugin.h"
#include "AfhsDecryptionContext.h"

#define MODULE_AFHS_DECRYPTION_HOSTER_NAME                            L"AfhsDecryptionHoster"

#define AFHS_DECRYPTION_HOSTER_FLAG_NONE                              HOSTER_FLAG_NONE

#define AFHS_DECRYPTION_HOSTER_FLAG_PENDING_DECRYPTOR                 (1 << (HOSTER_FLAG_LAST + 0))

#define AFHS_DECRYPTION_HOSTER_FLAG_LAST                              (HOSTER_FLAG_LAST + 1)

class CAfhsDecryptionHoster : public CHoster
{
public:
  CAfhsDecryptionHoster(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  virtual ~CAfhsDecryptionHoster(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // clears current session
  virtual void ClearSession(void);

  // loads plugins from directory
  // @return : S_OK if successful, E_AFHS_NO_DECRYPTOR_LOADED if no decryptor loaded, error code otherwise
  virtual HRESULT LoadPlugins(void);

  // decrypts encrypted segments fragments
  // @param decryptionContext : AFHS decryption context
  // @return : S_OK if successful, error code otherwise
  HRESULT DecryptSegmentFragments(CAfhsDecryptionContext *decryptionContext);

protected:
  // reference to active decryptor
  CAfhsDecryptionPlugin *activeDecryptor;

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
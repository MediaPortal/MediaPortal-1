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

#ifndef __AFHS_DECRYPTION_HOSTER_PLUGIN_METADATA_DEFINED
#define __AFHS_DECRYPTION_HOSTER_PLUGIN_METADATA_DEFINED

#include "HosterPluginMetadata.h"
#include "AfhsDecryptionPlugin.h"

class CAfhsDecryptionHosterPluginMetadata : public CHosterPluginMetadata
{
public:
  CAfhsDecryptionHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName);
  virtual ~CAfhsDecryptionHosterPluginMetadata(void);

  /* get methods */

  // gets decryption result about current stream
  // @return : one of DECRYPTION_RESULT values
  virtual HRESULT GetDecryptionResult(void);

  // gets decryption score if decryptor result is DECRYPTION_RESULT_KNOWN
  // @return : decryption score (decryptor with highest score is set as active decryptor)
  virtual unsigned int GetDecryptionScore(void);

  /* set methods */

  /* other methods */

  // clear current session
  // @return : S_OK if successfull
  virtual HRESULT ClearSession(void);

  // tests if last decryptor result is DECRYPTION_RESULT_PENDING
  // @return : true if last decryptor result is DECRYPTION_RESULT_PENDING, false otherwise
  virtual bool IsDecryptorStillPending(void);

protected:
  // holds decryption result
  HRESULT decryptionResult;

  /* methods */
};

#endif
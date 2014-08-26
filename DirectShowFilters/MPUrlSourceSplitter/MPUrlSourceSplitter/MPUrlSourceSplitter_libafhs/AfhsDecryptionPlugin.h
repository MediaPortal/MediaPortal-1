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

#ifndef __AFHS_DECRYPTION_PLUGIN_DEFINED
#define __AFHS_DECRYPTION_PLUGIN_DEFINED

#include "Plugin.h"

#ifndef METHOD_CLEAR_SESSION_NAME
#define METHOD_CLEAR_SESSION_NAME                                     L"ClearSession()"
#endif

#define AFHS_DECRYPTION_PLUGIN_FLAG_NONE                              PLUGIN_FLAG_NONE

#define AFHS_DECRYPTION_PLUGIN_FLAG_LAST                              (PLUGIN_FLAG_LAST + 0)

#define DECRYPTION_RESULT_PENDING                                     1
#define DECRYPTION_RESULT_NOT_KNOWN                                   2
#define DECRYPTION_RESULT_KNOWN                                       S_OK

class CAfhsDecryptionPlugin : public CPlugin
{
public:
  CAfhsDecryptionPlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  virtual ~CAfhsDecryptionPlugin(void);

  // CPlugin

  // initialize plugin implementation with configuration parameters
  // @param configuration : the reference to additional configuration parameters (created by plugin's hoster class)
  // @return : S_OK if successfull, error code otherwise
  virtual HRESULT Initialize(CPluginConfiguration *configuration);

  /* get methods */

  // gets decryption result about current stream
  // @return : one of DECRYPTION_RESULT values
  virtual HRESULT GetDecryptionResult(void);

  // gets decryption score if decryptor result is DECRYPTION_RESULT_KNOWN
  // @return : decryption score (decryptor with highest score is set as active decryptor)
  virtual unsigned int GetDecryptionScore(void) = 0;

  /* set methods */

  /* other methods */

  // clear current session
  // @return : S_OK if successfull
  virtual HRESULT ClearSession(void);

protected:
  // holds logger instance
  CLogger *logger;
  // holds configuration
  CParameterCollection *configuration;
  // holds decryption result
  HRESULT decryptionResult;

  /* methods */
};

#endif
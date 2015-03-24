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

#ifndef __MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_DEFAULT_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_DEFAULT_DEFINED

#include "Logger.h"
#include "AfhsDecryptionPlugin.h"
#include "AfhsCurlInstance.h"
#include "AfhsSegmentFragmentCollection.h"
#include "AfhsDecryptionHoster.h"

#define AFHS_PROTOCOL_DECRYPTION_NAME                                                     L"AFHS_DECRYPTION_DEFAULT"

#define MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_DEFAULT_FLAG_NONE                 AFHS_DECRYPTION_PLUGIN_FLAG_NONE

#define MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_DECRYPTION_DEFAULT_FLAG_LAST                 (AFHS_DECRYPTION_PLUGIN_FLAG_LAST + 0)

class CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default : public CAfhsDecryptionPlugin
{
public:
  // constructor
  CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  virtual ~CMPUrlSourceSplitter_Protocol_Afhs_Decryption_Default(void);

  // CPlugin implementation

  // return reference to null-terminated string which represents plugin name
  // errors should be logged to log file and returned NULL
  // @return : reference to null-terminated string
  virtual const wchar_t *GetName(void);

  // get plugin instance ID
  // @return : GUID, which represents instance identifier or GUID_NULL if error
  virtual GUID GetInstanceId(void);

  // initialize plugin implementation with configuration parameters
  // @param configuration : the reference to additional configuration parameters (created by plugin's hoster class)
  // @return : S_OK if successfull, error code otherwise
  virtual HRESULT Initialize(CPluginConfiguration *configuration);

  // CAfhsDecryptionPlugin implementation

  // gets decryption result about current stream
  // @param decryptionContext : AFHS decryption context
  // @return : one of DECRYPTION_RESULT values
  virtual HRESULT GetDecryptionResult(CAfhsDecryptionContext *decryptionContext);

  // gets decryption score if decryptor result is DECRYPTION_RESULT_KNOWN
  // @return : decryption score (decryptor with highest score is set as active decryptor)
  virtual unsigned int GetDecryptionScore(void);

  // decrypts encrypted segment fragments
  // @param decryptionContext : AFHS decryption context
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT DecryptSegmentFragments(CAfhsDecryptionContext *decryptionContext);

protected:

  /* methods */
};

#endif

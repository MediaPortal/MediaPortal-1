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

#ifndef __MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_DECRYPTION_AES128_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_DECRYPTION_AES128_DEFINED

#include "Logger.h"
#include "M3u8DecryptionPlugin.h"
#include "M3u8CurlInstance.h"
#include "M3u8StreamFragmentCollection.h"
#include "M3u8DecryptionHoster.h"

#define M3U8_PROTOCOL_DECRYPTION_NAME                                                     L"M3U8_DECRYPTION_AES128"

#define MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_DECRYPTION_AES128_FLAG_NONE                  M3U8_DECRYPTION_PLUGIN_FLAG_NONE

#define MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_DECRYPTION_AES128_FLAG_KEY_REQUEST_PENDING   (1 << (M3U8_DECRYPTION_PLUGIN_FLAG_LAST + 0))

#define MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_DECRYPTION_AES128_FLAG_LAST                  (M3U8_DECRYPTION_PLUGIN_FLAG_LAST + 1)

// 128-bit is 16 bytes
#define AES128_KEY_LENGTH                                                                 16

class CMPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128 : public CM3u8DecryptionPlugin
{
public:
  // constructor
  CMPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  virtual ~CMPUrlSourceSplitter_Protocol_M3u8_Decryption_Aes128(void);

  // CPlugin implementation

  // return reference to null-terminated string which represents plugin name
  // errors should be logged to log file and returned NULL
  // @return : reference to null-terminated string
  virtual const wchar_t *GetName(void);

  // clears current session
  virtual void ClearSession(void);

  // CM3u8DecryptionPlugin implementation

  // decrypts encrypted stream fragments
  // @param decryptionContext : M3U8 decryption context
  // @return : S_OK if successful, E_M3U8_DECRYPTION_METHOD_NOT_SUPPORTED if on first encrypted stream fragment is not supported encryption method, error code otherwise
  virtual HRESULT DecryptStreamFragments(CM3u8DecryptionContext *decryptionContext);

protected:
  // holds last decryption key
  uint8_t *decryptionKey;
  // holds last decryption key URI
  wchar_t *decryptionKeyUri;

  /* methods */

  // get module name for Initialize() method
  // @return : module name
  virtual const wchar_t *GetModuleName(void);
};

#endif

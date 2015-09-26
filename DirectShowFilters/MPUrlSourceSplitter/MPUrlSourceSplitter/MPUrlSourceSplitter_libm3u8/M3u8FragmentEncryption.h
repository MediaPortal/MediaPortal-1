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

#ifndef __M3U8_FRAGMENT_ENCRYPTION_DEFINED
#define __M3U8_FRAGMENT_ENCRYPTION_DEFINED

#include "Flags.h"

#define M3U8_FRAGMENT_ENCRYPTION_FLAG_NONE                            FLAGS_NONE

#define M3U8_FRAGMENT_ENCRYPTION_FLAG_METHOD_NONE                     (1 << (FLAGS_LAST + 0))
#define M3U8_FRAGMENT_ENCRYPTION_FLAG_METHOD_AES128                   (1 << (FLAGS_LAST + 1))
#define M3U8_FRAGMENT_ENCRYPTION_FLAG_METHOD_SAMPLE_AES               (1 << (FLAGS_LAST + 2))
#define M3U8_FRAGMENT_ENCRYPTION_FLAG_KEY_FORMAT_IDENTITY             (1 << (FLAGS_LAST + 3))
#define M3U8_FRAGMENT_ENCRYPTION_FLAG_KEY_FORMAT_VERSIONS_DEFAULT     (1 << (FLAGS_LAST + 4))

#define M3U8_FRAGMENT_ENCRYPTION_FLAG_LAST                            (FLAGS_LAST + 5)

#define ENCRYPTION_KEY_FORMAT_IDENTITY                                L"identity"
#define ENCRYPTION_KEY_FORMAT_VERSIONS_DEFAULT                        L"1"

#define INITIALIZATION_VECTOR_LENGTH                                  16

class CM3u8FragmentEncryption : public CFlags
{
public:
  CM3u8FragmentEncryption(HRESULT *result);
  virtual ~CM3u8FragmentEncryption();

  /* get methods */

  // gets key URI
  // @return : key URI or NULL if not specified
  const wchar_t *GetEncryptionKeyUri(void);

  // gets initialization vector
  // @return : initialization vector or NULL if not specified
  const uint8_t *GetEncryptionInitializationVector(void);

  // gets key format
  // @return : key format or NULL if not specified
  const wchar_t *GetEncryptionKeyFormat(void);

  // gets key format versions
  // @return : key format versions or NULL if not specified
  const wchar_t *GetEncryptionKeyFormatVersions(void);

  /* set methods */

  // sets if fragment is not encrypted
  // @param encryptionNone : true if not encrypted, false otherwise
  void SetEncryptionNone(bool encryptionNone);

  // sets if fragment is AES-128 encrypted
  // @param encryptionAes128 : true if AES-128 encrypted, false otherwise
  void SetEncryptionAes128(bool encryptionAes128);

  // sets if fragment is SAMPLE-AES encrypted
  // @param encryptionSampleAes : true if SAMPLE-AES encrypted, false otherwise
  void SetEncryptionSampleAes(bool encryptionSampleAes);

  // sets encryption key URI
  // @param encryptionKeyUri : encryption key URI to set or NULL if none
  // @return : true if successful, false otherwise
  bool SetEncryptionKeyUri(const wchar_t *encryptionKeyUri);

  // sets encryption initialization vector
  // @param initializationVector : the initialization vector or NULL if none (default)
  // @return : true if successful, false otherwise
  bool SetEncryptionInitializationVector(const uint8_t *initializationVector);

  // sets encryption key format
  // @param keyFormat : the encryption key format or NULL if none (default)
  // @return : true if successful, false otherwise
  bool SetEncryptionKeyFormat(const wchar_t *keyFormat);

  // sets encryption key format versions
  // @param keyFormatVersions : the encryption key format versions or NULL if none (default)
  // @return : true if successful, false otherwise
  bool SetEncryptionKeyFormatVersions(const wchar_t *keyFormatVersions);

  /* other methods */

  // tests if fragment is not encrypted
  // @return : true if not encrypted, false otherwise
  bool IsEncryptionNone(void);

  // tests if fragment is AES-128 encrypted
  // @return : true if AES-128 encrypted, false otherwise
  bool IsEncryptionAes128(void);

  // tests if fragment is SAMPLE-AES encrypted
  // @return : true if SAMPLE-AES encrypted, false otherwise
  bool IsEncryptionSampleAes(void);

  // tests if fragment key format is "identity"
  // @return : true if key format is "identity", false otherwise
  bool IsEncryptionKeyFormatIdentity(void);

  // tests if fragment key format versions is default
  // @return : true if key format versions is default, false otherwise
  bool IsEncryptionKeyFormatVersionsDefault(void);

  // deeply clones current instance
  // @return : deep clone of current instance or NULL if error
  CM3u8FragmentEncryption *Clone(void);

protected:

  // holds key URI (if specified)
  wchar_t *keyUri;

  // holds initialization vector (if specified)
  uint8_t *iv;

  // holds key format (if specified)
  wchar_t *keyFormat;

  // holds key format versions (if specified)
  wchar_t *keyFormatVersions;

  /* methods */

  // gets new instance of item
  // @return : new item instance or NULL if error
  CM3u8FragmentEncryption *CreateItem(void);

  // deeply clones current instance
  // @param item : the item instance to clone
  // @return : true if successful, false otherwise
  bool InternalClone(CM3u8FragmentEncryption *item);
};

#endif
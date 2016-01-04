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

#include "stdafx.h"

#include "M3u8FragmentEncryption.h"

CM3u8FragmentEncryption::CM3u8FragmentEncryption(HRESULT *result)
  : CFlags()
{
  this->keyUri = NULL;
  this->iv = NULL;
  this->keyFormat = NULL;
  this->keyFormatVersions = NULL;

  // fragment is not encrypted
  this->flags |= M3U8_FRAGMENT_ENCRYPTION_FLAG_METHOD_NONE | M3U8_FRAGMENT_ENCRYPTION_FLAG_KEY_FORMAT_IDENTITY | M3U8_FRAGMENT_ENCRYPTION_FLAG_KEY_FORMAT_VERSIONS_DEFAULT;
}

CM3u8FragmentEncryption::~CM3u8FragmentEncryption()
{
  FREE_MEM(this->keyUri);
  FREE_MEM(this->iv);
  FREE_MEM(this->keyFormat);
  FREE_MEM(this->keyFormatVersions);
}

/* get methods */

const wchar_t *CM3u8FragmentEncryption::GetEncryptionKeyUri(void)
{
  return this->keyUri;
}

const uint8_t *CM3u8FragmentEncryption::GetEncryptionInitializationVector(void)
{
  return this->iv;
}

const wchar_t *CM3u8FragmentEncryption::GetEncryptionKeyFormat(void)
{
  return this->keyFormat;
}

const wchar_t *CM3u8FragmentEncryption::GetEncryptionKeyFormatVersions(void)
{
  return this->keyFormatVersions;
}

/* set methods */

void CM3u8FragmentEncryption::SetEncryptionNone(bool encryptionNone)
{
  this->flags &= ~M3U8_FRAGMENT_ENCRYPTION_FLAG_METHOD_NONE;
  this->flags |= encryptionNone ? M3U8_FRAGMENT_ENCRYPTION_FLAG_METHOD_NONE : M3U8_FRAGMENT_ENCRYPTION_FLAG_NONE;
}

void CM3u8FragmentEncryption::SetEncryptionAes128(bool encryptionAes128)
{
  this->flags &= ~M3U8_FRAGMENT_ENCRYPTION_FLAG_METHOD_AES128;
  this->flags |= encryptionAes128 ? M3U8_FRAGMENT_ENCRYPTION_FLAG_METHOD_AES128 : M3U8_FRAGMENT_ENCRYPTION_FLAG_NONE;
}

void CM3u8FragmentEncryption::SetEncryptionSampleAes(bool encryptionSampleAes)
{
  this->flags &= ~M3U8_FRAGMENT_ENCRYPTION_FLAG_METHOD_SAMPLE_AES;
  this->flags |= encryptionSampleAes ? M3U8_FRAGMENT_ENCRYPTION_FLAG_METHOD_SAMPLE_AES : M3U8_FRAGMENT_ENCRYPTION_FLAG_NONE;
}

bool CM3u8FragmentEncryption::SetEncryptionKeyUri(const wchar_t *encryptionKeyUri)
{
  SET_STRING_RETURN_WITH_NULL(this->keyUri, encryptionKeyUri);
}

bool CM3u8FragmentEncryption::SetEncryptionInitializationVector(const uint8_t *initializationVector)
{
  bool result = true;
  FREE_MEM(this->iv);

  if (initializationVector != NULL)
  {
    this->iv = ALLOC_MEM_SET(this->iv, uint8_t, INITIALIZATION_VECTOR_LENGTH, 0);
    result &= (this->iv != NULL);

    CHECK_CONDITION_EXECUTE(result, memcpy(this->iv, initializationVector, INITIALIZATION_VECTOR_LENGTH));
  }

  return result;
}

bool CM3u8FragmentEncryption::SetEncryptionKeyFormat(const wchar_t *keyFormat)
{
  bool result = true;

  this->flags &= ~M3U8_FRAGMENT_ENCRYPTION_FLAG_KEY_FORMAT_IDENTITY;
  SET_STRING_AND_RESULT_WITH_NULL(this->keyFormat, keyFormat, result);

  this->flags |= (result && ((this->keyFormat == NULL) || (wcscmp(this->keyFormat, ENCRYPTION_KEY_FORMAT_IDENTITY) == 0))) ? M3U8_FRAGMENT_ENCRYPTION_FLAG_KEY_FORMAT_IDENTITY : M3U8_FRAGMENT_ENCRYPTION_FLAG_NONE;

  return result;
}

bool CM3u8FragmentEncryption::SetEncryptionKeyFormatVersions(const wchar_t *keyFormatVersions)
{
  bool result = true;

  this->flags &= ~M3U8_FRAGMENT_ENCRYPTION_FLAG_KEY_FORMAT_VERSIONS_DEFAULT;
  SET_STRING_AND_RESULT_WITH_NULL(this->keyFormatVersions, keyFormatVersions, result);

  this->flags |= (result && ((this->keyFormatVersions == NULL) || (wcscmp(this->keyFormatVersions, ENCRYPTION_KEY_FORMAT_VERSIONS_DEFAULT) == 0))) ? M3U8_FRAGMENT_ENCRYPTION_FLAG_KEY_FORMAT_VERSIONS_DEFAULT : M3U8_FRAGMENT_ENCRYPTION_FLAG_NONE;

  return result;
}

/* other methods */

bool CM3u8FragmentEncryption::IsEncryptionNone(void)
{
  return this->IsSetFlags(M3U8_FRAGMENT_ENCRYPTION_FLAG_METHOD_NONE);
}

bool CM3u8FragmentEncryption::IsEncryptionAes128(void)
{
  return this->IsSetFlags(M3U8_FRAGMENT_ENCRYPTION_FLAG_METHOD_AES128);
}

bool CM3u8FragmentEncryption::IsEncryptionSampleAes(void)
{
  return this->IsSetFlags(M3U8_FRAGMENT_ENCRYPTION_FLAG_METHOD_SAMPLE_AES);
}

bool CM3u8FragmentEncryption::IsEncryptionKeyFormatIdentity(void)
{
  return this->IsSetFlags(M3U8_FRAGMENT_ENCRYPTION_FLAG_KEY_FORMAT_IDENTITY);
}

bool CM3u8FragmentEncryption::IsEncryptionKeyFormatVersionsDefault(void)
{
  return this->IsSetFlags(M3U8_FRAGMENT_ENCRYPTION_FLAG_KEY_FORMAT_VERSIONS_DEFAULT);
}

CM3u8FragmentEncryption *CM3u8FragmentEncryption::Clone(void)
{
  CM3u8FragmentEncryption *result = this->CreateItem();

  if (result != NULL)
  {
    if (!this->InternalClone(result))
    {
      FREE_MEM_CLASS(result);
    }
  }

  return result;
}

/* protected methods */

CM3u8FragmentEncryption *CM3u8FragmentEncryption::CreateItem(void)
{
  HRESULT result = S_OK;
  CM3u8FragmentEncryption *item = new CM3u8FragmentEncryption(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CM3u8FragmentEncryption::InternalClone(CM3u8FragmentEncryption *item)
{
  bool result = (item != NULL);

  if (result)
  {
    item->flags = this->flags;

    result &= item->SetEncryptionKeyUri(this->keyUri);
    result &= item->SetEncryptionInitializationVector(this->iv);
    result &= item->SetEncryptionKeyFormat(this->keyFormat);
    result &= item->SetEncryptionKeyFormatVersions(this->keyFormatVersions);
  }

  return result;
}
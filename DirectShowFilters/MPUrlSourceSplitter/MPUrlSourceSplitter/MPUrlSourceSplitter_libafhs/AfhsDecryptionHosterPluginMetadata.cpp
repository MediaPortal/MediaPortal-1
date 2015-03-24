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

#include "StdAfx.h"

#pragma warning(push)
// disable warning: 'INT8_MIN' : macro redefinition
// warning is caused by stdint.h and intsafe.h, which both define same macro
#pragma warning(disable:4005)

#include "AfhsDecryptionHosterPluginMetadata.h"
#include "AfhsDecryptionPlugin.h"
#include "ErrorCodes.h"

#pragma warning(pop)

CAfhsDecryptionHosterPluginMetadata::CAfhsDecryptionHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName)
  : CHosterPluginMetadata(result, logger, configuration, hosterName, pluginLibraryFileName)
{
  this->decryptionResult = DECRYPTION_RESULT_PENDING;
}

CAfhsDecryptionHosterPluginMetadata::~CAfhsDecryptionHosterPluginMetadata(void)
{
}

/* get methods */

HRESULT CAfhsDecryptionHosterPluginMetadata::GetDecryptionResult(CAfhsDecryptionContext *decryptionContext)
{
  if (this->decryptionResult == DECRYPTION_RESULT_PENDING)
  {
    CAfhsDecryptionPlugin *decryptor = dynamic_cast<CAfhsDecryptionPlugin *>(this->plugin);

    this->decryptionResult = (decryptor != NULL) ? decryptor->GetDecryptionResult(decryptionContext) : DECRYPTION_RESULT_PENDING;
  }

  return this->decryptionResult;
}

unsigned int CAfhsDecryptionHosterPluginMetadata::GetDecryptionScore(void)
{
  CAfhsDecryptionPlugin *decryptor = dynamic_cast<CAfhsDecryptionPlugin *>(this->plugin);

  return (decryptor != NULL) ? decryptor->GetDecryptionScore() : 0;
}

/* set methods */

/* other methods */

HRESULT CAfhsDecryptionHosterPluginMetadata::CheckPlugin(void)
{
  CAfhsDecryptionPlugin *decryptionPlugin = dynamic_cast<CAfhsDecryptionPlugin *>(this->plugin);

  return (decryptionPlugin != NULL) ? S_OK : E_INVALID_PLUGIN_TYPE;
}

void CAfhsDecryptionHosterPluginMetadata::ClearSession(void)
{
  __super::ClearSession();

  this->decryptionResult = DECRYPTION_RESULT_PENDING;
}

bool CAfhsDecryptionHosterPluginMetadata::IsDecryptorStillPending(void)
{
  return (this->decryptionResult == DECRYPTION_RESULT_PENDING);
}

/* protected methods */

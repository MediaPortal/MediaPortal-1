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

#include "AfhsDecryptionPlugin.h"
#include "AfhsDecryptionPluginConfiguration.h"

#pragma warning(pop)

CAfhsDecryptionPlugin::CAfhsDecryptionPlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CPlugin(result, logger, configuration)
{
  this->decryptionResult = DECRYPTION_RESULT_PENDING;

  /*if ((result != NULL) && (SUCCEEDED(*result)))
  {
  }*/
}

CAfhsDecryptionPlugin::~CAfhsDecryptionPlugin(void)
{
}

// CPlugin

HRESULT CAfhsDecryptionPlugin::Initialize(CPluginConfiguration *configuration)
{
  HRESULT result = __super::Initialize(configuration);

  if (SUCCEEDED(result))
  {
    CAfhsDecryptionPluginConfiguration *decryptionConfiguration = dynamic_cast<CAfhsDecryptionPluginConfiguration *>(configuration);
    CHECK_POINTER_HRESULT(result, decryptionConfiguration, result, E_INVALIDARG);
  }

  return result;
}

HRESULT CAfhsDecryptionPlugin::GetDecryptionResult(CAfhsDecryptionContext *decryptionContext)
{
  return this->decryptionResult;
}

/* get methods */

/* set methods */

/* other methods */

void CAfhsDecryptionPlugin::ClearSession(void)
{
  __super::ClearSession();

  this->decryptionResult = DECRYPTION_RESULT_PENDING;
}

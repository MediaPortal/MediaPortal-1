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

#pragma warning(push)
// disable warning: 'INT8_MIN' : macro redefinition
// warning is caused by stdint.h and intsafe.h, which both define same macro
#pragma warning(disable:4005)

#include "M3u8DecryptionPlugin.h"
#include "M3u8DecryptionPluginConfiguration.h"

#pragma warning(pop)


CM3u8DecryptionPlugin::CM3u8DecryptionPlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CPlugin(result, logger, configuration)
{
  this->logger = NULL;
  this->configuration = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger = new CLogger(result, logger);
    this->configuration = new CParameterCollection(result);

    CHECK_POINTER_HRESULT(*result, this->logger, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->configuration, *result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(*result, this->configuration->Append(configuration), *result, E_OUTOFMEMORY);
  }
}

CM3u8DecryptionPlugin::~CM3u8DecryptionPlugin()
{
  FREE_MEM_CLASS(this->configuration);
  FREE_MEM_CLASS(this->logger);
}

// CPlugin

HRESULT CM3u8DecryptionPlugin::Initialize(CPluginConfiguration *configuration)
{
  CM3u8DecryptionPluginConfiguration *decryptionConfiguration = dynamic_cast<CM3u8DecryptionPluginConfiguration *>(configuration);
  HRESULT result = ((this->configuration != NULL) && (this->logger != NULL)) ? S_OK : E_NOT_VALID_STATE;
  CHECK_POINTER_HRESULT(result, decryptionConfiguration, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    this->configuration->Clear();

    CHECK_CONDITION_HRESULT(result, this->configuration->Append(decryptionConfiguration->GetConfiguration()), result, E_OUTOFMEMORY);
  }

  return result;
}

/* get methods */

/* set methods */

/* other methods */

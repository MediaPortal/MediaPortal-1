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
  /*if ((result != NULL) && (SUCCEEDED(*result)))
  {
  }*/
}

CM3u8DecryptionPlugin::~CM3u8DecryptionPlugin()
{
}

// CPlugin

HRESULT CM3u8DecryptionPlugin::Initialize(CPluginConfiguration *configuration)
{
  HRESULT result = __super::Initialize(configuration);

  if (SUCCEEDED(result))
  {
    CM3u8DecryptionPluginConfiguration *decryptionConfiguration = dynamic_cast<CM3u8DecryptionPluginConfiguration *>(configuration);
    CHECK_POINTER_HRESULT(result, decryptionConfiguration, result, E_INVALIDARG);
  }

  return result;
}

/* get methods */

/* set methods */

/* other methods */

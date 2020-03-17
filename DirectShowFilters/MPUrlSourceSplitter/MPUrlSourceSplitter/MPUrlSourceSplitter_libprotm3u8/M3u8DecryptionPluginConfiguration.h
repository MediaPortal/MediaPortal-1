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

#ifndef __M3U8_DECRYPTION_PLUGIN_CONFIGURATION_DEFINED
#define __M3U8_DECRYPTION_PLUGIN_CONFIGURATION_DEFINED

#include "PluginConfiguration.h"

class CM3u8DecryptionPluginConfiguration : public CPluginConfiguration
{
public:
  CM3u8DecryptionPluginConfiguration(HRESULT *result, CParameterCollection *configuration);
  virtual ~CM3u8DecryptionPluginConfiguration(void);

  /* get methods */

  /* set methods */

  /* other methods */

protected:

  /* methods */
};

#endif
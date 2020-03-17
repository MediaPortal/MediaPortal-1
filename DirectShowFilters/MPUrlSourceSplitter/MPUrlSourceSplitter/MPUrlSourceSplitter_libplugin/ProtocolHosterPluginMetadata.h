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

#ifndef __PROTOCOL_HOSTER_PLUGIN_METADATA_DEFINED
#define __PROTOCOL_HOSTER_PLUGIN_METADATA_DEFINED

#include "HosterPluginMetadata.h"

#define PROTOCOL_HOSTER_PLUGIN_METADATA_FLAG_NONE                     HOSTER_PLUGIN_METADATA_FLAG_NONE

#define PROTOCOL_HOSTER_PLUGIN_METADATA_FLAG_SUPPORTED                (1 << (HOSTER_PLUGIN_METADATA_FLAG_LAST + 0))

#define PROTOCOL_HOSTER_PLUGIN_METADATA_FLAG_LAST                     (HOSTER_PLUGIN_METADATA_FLAG_LAST + 1)

class CProtocolHosterPluginMetadata : public CHosterPluginMetadata
{
public:
  CProtocolHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName);
  virtual ~CProtocolHosterPluginMetadata(void);

  /* get methods */

  /* set methods */

  // sets if protocol plugin supports URL
  // @param supported : true if protocol plugin supports URL, false otherwise
  virtual void SetSupported(bool supported);

  /* other methods */

  // checks plugin (basicly checks plugin type)
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT CheckPlugin(void);

  // tests if protocol plugin supports URL
  // @return : true if protocol plugin supports URL, false otherwise
  virtual bool IsSupported(void);

protected:
  /* methods */
};

#endif
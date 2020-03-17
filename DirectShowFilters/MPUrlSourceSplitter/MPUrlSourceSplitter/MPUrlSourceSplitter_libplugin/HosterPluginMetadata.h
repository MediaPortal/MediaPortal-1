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

#ifndef __HOSTER_PLUGIN_METADATA_DEFINED
#define __HOSTER_PLUGIN_METADATA_DEFINED

#include "Logger.h"
#include "ParameterCollection.h"
#include "Plugin.h"
#include "Flags.h"

#define HOSTER_PLUGIN_METADATA_FLAG_NONE                              FLAGS_NONE

#define HOSTER_PLUGIN_METADATA_FLAG_LAST                              (FLAGS_LAST + 0)

class CHosterPluginMetadata : public CFlags
{
public:
  CHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName);
  virtual ~CHosterPluginMetadata(void);

  /* get methods */

  // gets plugin
  // @return : plugin or NULL if error
  virtual CPlugin *GetPlugin(void);

  /* set methods */

  /* other methods */

  // checks plugin (basicly checks plugin type)
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT CheckPlugin(void) = 0;

  // clears current session
  virtual void ClearSession(void);

protected:
  // holds logger
  CLogger *logger;
  // holds hoster name
  wchar_t *hosterName;
  // holds library file name
  wchar_t *pluginLibraryFileName;
  // holds library handle
  HINSTANCE library;
  // holds plugin
  CPlugin *plugin;
  // holds destroy plugin method address
  DESTROYPLUGIN DestroyPlugin;

  /* methods */
};

#endif
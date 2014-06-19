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

#ifndef __HOSTER_DEFINED
#define __HOSTER_DEFINED

#include "HosterPluginMetadataCollection.h"
#include "Flags.h"

#define MODULE_FILE_NAME                                                      L"MPUrlSourceSplitter.ax"

#define MODULE_HOSTER_NAME                                                    L"Hoster"

#define METHOD_LOAD_PLUGINS_NAME                                              L"LoadPlugins()"
// maximum count of plugins
#define MAX_PLUGINS_DEFAULT                                                   256

#define HOSTER_FLAG_NONE                                                      FLAGS_NONE

#define HOSTER_FLAG_LAST                                                      (FLAGS_LAST + 0)

class CHoster : public CFlags
{
public:
  CHoster(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *hosterSearchPattern);
  virtual ~CHoster(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // loads plugins from directory
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT LoadPlugins(void);

protected:
  // logger for logging purposes
  CLogger *logger;
  // configuration provided by filter
  CParameterCollection *configuration;

  // the name of module implementing hoster
  wchar_t *hosterName;
  // the hoster search pattern in directory
  wchar_t *hosterSearchPattern;

  // holds hoster plugin metadata collection
  CHosterPluginMetadataCollection *hosterPluginMetadataCollection;

  /* methods */

  // creates hoster plugin metadata
  // @param result : the reference to result
  // @param logger : the reference to logger
  // @param configuration : the reference to configuration
  // @param hosterName : the hoster name
  // @param pluginLibraryFileName : the plugin library file name
  // @result : hoster plugin metadata instance
  virtual CHosterPluginMetadata *CreateHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName) = 0;

  // creates plugin configuration
  // @param result : the reference to result
  // @param configuration : the collection of parameters
  // @result : plugin configuration instance
  virtual CPluginConfiguration *CreatePluginConfiguration(HRESULT *result, CParameterCollection *configuration) = 0;
};

#endif
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

#include "IPlugin.h"
#include "Logger.h"
#include "ParameterCollection.h"

#define MODULE_FILE_NAME                                                      L"MPUrlSourceSplitter.ax"

#define MODULE_HOSTER_NAME                                                    L"Hoster"

#define METHOD_LOAD_PLUGINS_NAME                                              L"LoadPlugins()"
#define METHOD_APPEND_PLUGIN_IMPLEMENTATION_NAME                              L"AppendPluginImplementation()"
#define METHOD_REMOVE_PLUGIN_IMPLEMENTATION_NAME                              L"RemovePluginImplementation()"

// maximum count of plugins
#define MAX_PLUGINS_DEFAULT                                                   256

struct PluginImplementation
{
  wchar_t *name;
  HINSTANCE hLibrary;
  PIPlugin pImplementation;
  DESTROYPLUGININSTANCE destroyPluginInstance;
};

class CHoster
{
public:
  CHoster(CLogger *logger, CParameterCollection *configuration, const wchar_t *moduleName, const wchar_t *moduleSearchPattern);
  virtual ~CHoster(void);

  // loads plugins from directory
  virtual void LoadPlugins(void);

  // this method must be called before destructor is called
  virtual void RemoveAllPlugins(void);

protected:
  // logger for logging purposes
  CLogger *logger;
  // configuration provided by filter
  CParameterCollection *configuration;
  // handle to MPUrlSourceSplitter.ax
  HMODULE mainModuleHandle;

  // the name of module implementing hoster
  wchar_t *moduleName;
  // the module search pattern in directory
  wchar_t *moduleSearchPattern;

  // array of available plugin implementations
  PluginImplementation *pluginImplementations;
  unsigned int pluginImplementationsCount;

  // allocates memory for plugin implementations in specific hoster
  // @param maxPlugins : the maximum plugins for hoster
  // @return : allocated memory or NULL if error
  virtual PluginImplementation *AllocatePluginsMemory(unsigned int maxPlugins);

  // gets plugins implementation at specified position
  // @param position : the plugin position
  // @return : reference to plugin implementation or NULL if error
  virtual PluginImplementation *GetPluginImplementation(unsigned int position);

  // appends plugin implementation to end of plugins implementations
  // @param plugin : reference to plugin implementation structure
  // @return : true if successful, false otherwise (in that case MUST be called RemovePluginImplementation() method)
  virtual bool AppendPluginImplementation(HINSTANCE hLibrary, DESTROYPLUGININSTANCE destroyPluginInstance, PIPlugin plugin);

  // removes last plugin implementation
  virtual void RemovePluginImplementation(void); 

  // gets plugin configuration for Initialize() method
  // @return : plugin configuration
  virtual PluginConfiguration *GetPluginConfiguration(void);
};

#endif
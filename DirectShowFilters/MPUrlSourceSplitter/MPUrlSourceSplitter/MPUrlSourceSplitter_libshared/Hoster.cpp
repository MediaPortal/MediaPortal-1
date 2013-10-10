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

#include "Hoster.h"
#include "Parameters.h"

#include <Shlwapi.h>
#include <Shlobj.h>

#pragma warning(pop)

CHoster::CHoster(CLogger *logger, CParameterCollection *configuration, const wchar_t *moduleName, const wchar_t *moduleSearchPattern)
{
  this->moduleName = Duplicate(moduleName);
  this->logger = logger;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME);

  this->moduleSearchPattern = Duplicate(moduleSearchPattern);
  this->pluginImplementationsCount = 0;
  this->pluginImplementations = NULL;
  this->mainModuleHandle = GetModuleHandle(MODULE_FILE_NAME);
  this->configuration = new CParameterCollection();
  if (configuration != NULL)
  {
    this->configuration->Append(configuration);
  }

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME);
}

CHoster::~CHoster(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_HOSTER_NAME, METHOD_DESTRUCTOR_NAME);

  FREE_MEM(this->pluginImplementations);
  FREE_MEM_CLASS(this->configuration);

  FREE_MEM(this->moduleSearchPattern);
  FREE_MEM(this->moduleName);

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_HOSTER_NAME, METHOD_DESTRUCTOR_NAME);
}

void CHoster::LoadPlugins(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->moduleName, METHOD_LOAD_PLUGINS_NAME);

  unsigned int maxPlugins = this->configuration->GetValueLong(PARAMETER_NAME_MAX_PLUGINS, true, MAX_PLUGINS_DEFAULT);
  maxPlugins = (maxPlugins < 0) ? MAX_PLUGINS_DEFAULT : maxPlugins;

  if (maxPlugins > 0)
  {
    this->pluginImplementations = this->AllocatePluginsMemory(maxPlugins);
    if (this->pluginImplementations != NULL)
    {
      WIN32_FIND_DATA info;
      HANDLE h;

      ALLOC_MEM_DEFINE_SET(strDllPath, wchar_t, _MAX_PATH, 0);
      ALLOC_MEM_DEFINE_SET(strDllSearch, wchar_t, _MAX_PATH, 0);

      GetModuleFileName(this->mainModuleHandle, strDllPath, _MAX_PATH);
      PathRemoveFileSpec(strDllPath);

      wcscat_s(strDllPath, _MAX_PATH, L"\\");
      wcscpy_s(strDllSearch, _MAX_PATH, strDllPath);
      wcscat_s(strDllSearch, _MAX_PATH, this->moduleSearchPattern);

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: search path: '%s', search pattern: '%s'", this->moduleName, METHOD_LOAD_PLUGINS_NAME, strDllPath, this->moduleSearchPattern);
      // add plugins directory to search path
      SetDllDirectory(strDllPath);

      h = FindFirstFile(strDllSearch, &info);
      if (h != INVALID_HANDLE_VALUE) 
      {
        do 
        {
          BOOL result = TRUE;
          ALLOC_MEM_DEFINE_SET(strDllName, wchar_t, _MAX_PATH, 0);

          wcscpy_s(strDllName, _MAX_PATH, strDllPath);
          wcscat_s(strDllName, _MAX_PATH, info.cFileName);

          // load library
          this->logger->Log(LOGGER_INFO, L"%s: %s: loading library: %s", this->moduleName, METHOD_LOAD_PLUGINS_NAME, strDllName);
          HINSTANCE hLibrary = LoadLibrary(strDllName);        
          if (hLibrary == NULL)
          {
            this->logger->Log(LOGGER_ERROR, L"%s: %s: library '%s' not loaded", this->moduleName, METHOD_LOAD_PLUGINS_NAME, strDllName);
            result = FALSE;
          }

          if (result)
          {
            // find CreatePluginInstance() function
            // find DestroyPluginInstance() function
            PIPlugin pIPlugin = NULL;
            CREATEPLUGININSTANCE createPluginInstance;
            DESTROYPLUGININSTANCE destroyPluginInstance;

            createPluginInstance = (CREATEPLUGININSTANCE)GetProcAddress(hLibrary, "CreatePluginInstance");
            destroyPluginInstance = (DESTROYPLUGININSTANCE)GetProcAddress(hLibrary, "DestroyPluginInstance");

            if (createPluginInstance == NULL)
            {
              this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot find CreatePluginInstance() function, error: %d", this->moduleName, METHOD_LOAD_PLUGINS_NAME, GetLastError());
              result = FALSE;
            }
            if (destroyPluginInstance == NULL)
            {
              this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot find DestroyPluginInstance() function, error: %d", this->moduleName, METHOD_LOAD_PLUGINS_NAME, GetLastError());
              result = FALSE;
            }

            if (result)
            {
              // create plugin instance
              pIPlugin = (PIPlugin)createPluginInstance(this->logger, this->configuration);
              if (pIPlugin == NULL)
              {
                this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, this->moduleName, METHOD_LOAD_PLUGINS_NAME, L"cannot create plugin implementation instance");
                result = FALSE;
              }

              if (result)
              {
                // library is loaded and plugin implementation is instanced
                if (!this->AppendPluginImplementation(hLibrary, destroyPluginInstance, pIPlugin))
                {
                  this->RemovePluginImplementation();
                  result = FALSE;
                }
              }

              if (result)
              {
                // initialize plugin implementation
                // we don't have plugin specific parameters
                // all parameters are supplied with calling IFileSourceFilter.Load() method

                // initialize plugin
                PluginImplementation *implementation = this->GetPluginImplementation(this->pluginImplementationsCount - 1);
                PluginConfiguration *configuration = this->GetPluginConfiguration();
                HRESULT initialized = implementation->pImplementation->Initialize(configuration);
                FREE_MEM(configuration);

                if (SUCCEEDED(initialized))
                {
                  wchar_t *guid = ConvertGuidToString(implementation->pImplementation->GetInstanceId());
                  this->logger->Log(LOGGER_INFO, L"%s: %s: plugin '%s' successfully instanced, id: %s", this->moduleName, METHOD_LOAD_PLUGINS_NAME, implementation->name, guid);
                  FREE_MEM(guid);
                }
                else
                {
                  this->logger->Log(LOGGER_INFO, L"%s: %s: plugin '%s' not initialized, error: 0x%08X", this->moduleName, METHOD_LOAD_PLUGINS_NAME, implementation->name, initialized);
                  this->RemovePluginImplementation();
                }
              }
            }

            if (!result)
            {
              // any error occured while loading parser
              // free library and continue with another
              FreeLibrary(hLibrary);
            }
          }

          FREE_MEM(strDllName);
          if (this->pluginImplementationsCount == maxPlugins)
          {
            break;
          }
        } while (FindNextFile(h, &info));
        FindClose(h);
      } 

      this->logger->Log(LOGGER_INFO, L"%s: %s: found plugins: %u", this->moduleName, METHOD_LOAD_PLUGINS_NAME, this->pluginImplementationsCount);

      FREE_MEM(strDllPath);
      FREE_MEM(strDllSearch);
    }
    else
    {
      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, this->moduleName, METHOD_LOAD_PLUGINS_NAME, L"cannot allocate memory for plugin implementations");
    }
  }

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, this->moduleName, METHOD_LOAD_PLUGINS_NAME);
}

PluginImplementation *CHoster::AllocatePluginsMemory(unsigned int maxPlugins)
{
  return ALLOC_MEM(PluginImplementation, maxPlugins);
}

PluginImplementation *CHoster::GetPluginImplementation(unsigned int position)
{
  if ((this->pluginImplementations != NULL) && (position < this->pluginImplementationsCount))
  {
    return (this->pluginImplementations + position);
  }

  return NULL;
}

bool CHoster::AppendPluginImplementation(HINSTANCE hLibrary, DESTROYPLUGININSTANCE destroyPluginInstance, PIPlugin plugin)
{
  bool result = false;

  this->pluginImplementationsCount++;

  PluginImplementation *implementation = this->GetPluginImplementation(this->pluginImplementationsCount - 1);
  if (implementation != NULL)
  {
    implementation->hLibrary = hLibrary;
    implementation->destroyPluginInstance = destroyPluginInstance;
    implementation->pImplementation = plugin;
    implementation->name = Duplicate(plugin->GetName());

    if (implementation->name != NULL)
    {
      result = true;
    }
    else
    {
      // error occured while getting plugin name
      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, this->moduleName, METHOD_APPEND_PLUGIN_IMPLEMENTATION_NAME, L"cannot get plugin name");
    }
  }
  else
  {
    // error occured while getting plugin implementation
    this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, this->moduleName, METHOD_APPEND_PLUGIN_IMPLEMENTATION_NAME, L"cannot get plugin implementation");
  }

  return result;
}

void CHoster::RemovePluginImplementation(void)
{
  PluginImplementation *implementation = this->GetPluginImplementation(this->pluginImplementationsCount - 1);
  if (implementation != NULL)
  {
    this->logger->Log(LOGGER_INFO, L"%s: %s: destroying plugin: %s", this->moduleName, METHOD_REMOVE_PLUGIN_IMPLEMENTATION_NAME, implementation->name);

    if (implementation->pImplementation != NULL)
    {
      implementation->destroyPluginInstance(implementation->pImplementation);
      implementation->pImplementation = NULL;
      implementation->destroyPluginInstance = NULL;
    }
    FREE_MEM(implementation->name);
    if (implementation->hLibrary != NULL)
    {
      FreeLibrary(implementation->hLibrary);
      implementation->hLibrary = NULL;
    }

    this->pluginImplementationsCount--;
  }
}

void CHoster::RemoveAllPlugins(void)
{
  while (this->pluginImplementationsCount != 0)
  {
    this->RemovePluginImplementation();
  }
}

PluginConfiguration *CHoster::GetPluginConfiguration(void)
{
  ALLOC_MEM_DEFINE_SET(pluginConfiguration, PluginConfiguration, 1, 0);
  if (pluginConfiguration != NULL)
  {
    pluginConfiguration->configuration = this->configuration;
  }

  return pluginConfiguration;
}
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

#include "HosterPluginMetadata.h"
#include "ErrorCodes.h"

#define METHOD_HOSTER_PLUGIN_METADATA_CONSTRUCTOR_NAME                        L"HosterPluginMetadata: ctor()"
#define METHOD_HOSTER_PLUGIN_METADATA_DESTRUCTOR_NAME                         L"HosterPluginMetadata: dtor()"

CHosterPluginMetadata::CHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName)
  : CFlags()
{
  this->logger = NULL;
  this->hosterName = NULL;
  this->pluginLibraryFileName = NULL;
  this->library = NULL;
  this->plugin = NULL;
  this->DestroyPlugin = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, logger);
    CHECK_POINTER_DEFAULT_HRESULT(*result, configuration);
    CHECK_POINTER_DEFAULT_HRESULT(*result, hosterName);
    CHECK_POINTER_DEFAULT_HRESULT(*result, pluginLibraryFileName);

    if (SUCCEEDED(*result))
    {
      this->logger = logger;
      this->hosterName = Duplicate(hosterName);
      this->pluginLibraryFileName = Duplicate(pluginLibraryFileName);

      CHECK_POINTER_HRESULT(*result, this->hosterName, *result, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*result, this->pluginLibraryFileName, *result, E_OUTOFMEMORY);
    }

    if (SUCCEEDED(*result))
    {
      this->library = LoadLibrary(pluginLibraryFileName);
      if (library == NULL)
      {
        this->logger->Log(LOGGER_ERROR, L"%s: %s: library '%s' not loaded", this->hosterName, METHOD_HOSTER_PLUGIN_METADATA_CONSTRUCTOR_NAME, this->pluginLibraryFileName);
        *result = E_CANNOT_LOAD_PLUGIN_LIBRARY;
      }
    }

    if (SUCCEEDED(*result))
    {
      // find CreatePlugin() method
      // find DestroyPlugin() method
      CREATEPLUGIN CreatePlugin = NULL;

      CreatePlugin = (CREATEPLUGIN)GetProcAddress(library, "CreatePlugin");
      if (CreatePlugin == NULL)
      {
        this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot find CreatePlugin() method, error: %d", this->hosterName, METHOD_HOSTER_PLUGIN_METADATA_CONSTRUCTOR_NAME, GetLastError());
        *result = E_INVALID_PLUGIN;
      }

      this->DestroyPlugin = (DESTROYPLUGIN)GetProcAddress(library, "DestroyPlugin");
      if (this->DestroyPlugin == NULL)
      {
        this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot find DestroyPlugin() method, error: %d", this->hosterName, METHOD_HOSTER_PLUGIN_METADATA_CONSTRUCTOR_NAME, GetLastError());
        *result = E_INVALID_PLUGIN;
      }

      CHECK_CONDITION_HRESULT(*result, this->logger->RegisterModule(pluginLibraryFileName), *result, E_OUTOFMEMORY);

      if (SUCCEEDED(*result))
      {
        // create plugin
        this->plugin = CreatePlugin(result, this->logger, configuration);
        if (FAILED(*result) || (plugin == NULL))
        {
          this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot create plugin, error: 0x%08X", this->hosterName, METHOD_HOSTER_PLUGIN_METADATA_CONSTRUCTOR_NAME, *result);
          *result = E_CANNOT_CREATE_PLUGIN;
        }
      }
    }
  }
}

CHosterPluginMetadata::~CHosterPluginMetadata(void)
{
  if (this->plugin != NULL)
  {
    CHECK_CONDITION_NOT_NULL_EXECUTE(logger, this->logger->Log(LOGGER_INFO, L"%s: %s: destroying plugin: '%s'", this->hosterName, METHOD_HOSTER_PLUGIN_METADATA_DESTRUCTOR_NAME, plugin->GetName()));

    if (this->DestroyPlugin != NULL)
    {
      // destroy plugin
      this->DestroyPlugin(this->plugin);
    }

    this->plugin = NULL;
    this->DestroyPlugin = NULL;
  }

  if (this->library != NULL)
  {
    // free library from memory
    FreeLibrary(this->library);

    this->library = NULL;
  }

  FREE_MEM(this->hosterName);
  FREE_MEM(this->pluginLibraryFileName);
}

/* get methods */

CPlugin *CHosterPluginMetadata::GetPlugin(void)
{
  return this->plugin;
}

/* set methods */

/* other methods */

void CHosterPluginMetadata::ClearSession(void)
{
  this->flags = HOSTER_PLUGIN_METADATA_FLAG_NONE;
}

/* protected methods */
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
#include "ErrorCodes.h"

#include <Shlwapi.h>
#include <Shlobj.h>

#pragma warning(pop)

CHoster::CHoster(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *hosterSearchPattern)
  : CFlags()
{
  this->logger = NULL;
  this->configuration = NULL;
  this->hosterName = NULL;
  this->hosterSearchPattern = NULL;
  this->hosterPluginMetadataCollection = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, logger);
    CHECK_POINTER_DEFAULT_HRESULT(*result, configuration);
    CHECK_POINTER_DEFAULT_HRESULT(*result, hosterName);

    if (SUCCEEDED(*result))
    {
      this->logger = logger;
      this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, MODULE_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME, this);
      
      this->hosterName = Duplicate(hosterName);
      this->hosterSearchPattern = Duplicate(hosterSearchPattern);
      this->configuration = new CParameterCollection(result);
      this->hosterPluginMetadataCollection = new CHosterPluginMetadataCollection(result);

      CHECK_POINTER_HRESULT(*result, this->hosterName, *result, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*result, this->hosterSearchPattern, *result, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*result, this->configuration, *result, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*result, this->hosterPluginMetadataCollection, *result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(*result, this->configuration->Append(configuration), *result, E_OUTOFMEMORY);

      this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME);
    }
  }
}

CHoster::~CHoster(void)
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_HOSTER_NAME, METHOD_DESTRUCTOR_NAME, this));

  FREE_MEM_CLASS(this->hosterPluginMetadataCollection);
  FREE_MEM_CLASS(this->configuration);
  FREE_MEM(this->hosterSearchPattern);
  FREE_MEM(this->hosterName);

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_HOSTER_NAME, METHOD_DESTRUCTOR_NAME));
}

/* get methods */

/* set  methods */

/* other methods */

HRESULT CHoster::LoadPlugins(void)
{
  HRESULT result = ((this->logger != NULL) && (this->configuration != NULL) && (this->hosterName != NULL) && (this->hosterSearchPattern != NULL) && (this->hosterPluginMetadataCollection != NULL)) ? S_OK : E_NOT_VALID_STATE;
  
  if (SUCCEEDED(result))
  {
    this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->hosterName, METHOD_LOAD_PLUGINS_NAME);

    this->hosterPluginMetadataCollection->Clear();

    unsigned int maxPlugins = this->configuration->GetValueLong(PARAMETER_NAME_MAX_PLUGINS, true, MAX_PLUGINS_DEFAULT);
    maxPlugins = (maxPlugins < 0) ? MAX_PLUGINS_DEFAULT : maxPlugins;

    CHECK_CONDITION_HRESULT(result, this->hosterPluginMetadataCollection->EnsureEnoughSpace(maxPlugins), result, E_OUTOFMEMORY);

    if (SUCCEEDED(result) && (maxPlugins > 0))
    {
      WIN32_FIND_DATA pluginFileInfo;
      HANDLE pluginFileHandle;

      ALLOC_MEM_DEFINE_SET(pluginPath, wchar_t, MAX_PATH, 0);
      ALLOC_MEM_DEFINE_SET(pluginSearchPattern, wchar_t, MAX_PATH, 0);
      CHECK_POINTER_HRESULT(result, pluginPath, result, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(result, pluginSearchPattern, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, GetModuleFileName(GetModuleHandle(MODULE_FILE_NAME), pluginPath, MAX_PATH) != 0, result, E_CANNOT_GET_MODULE_FILE_NAME);

      if (SUCCEEDED(result))
      {
        PathRemoveFileSpec(pluginPath);

        wcscat_s(pluginPath, MAX_PATH, L"\\");
        wcscpy_s(pluginSearchPattern, MAX_PATH, pluginPath);
        wcscat_s(pluginSearchPattern, MAX_PATH, this->hosterSearchPattern);

        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: search path: '%s', search pattern: '%s'", this->hosterName, METHOD_LOAD_PLUGINS_NAME, pluginPath, this->hosterSearchPattern);
        // add plugins directory to search path
        SetDllDirectory(pluginPath);

        pluginFileHandle = FindFirstFile(pluginSearchPattern, &pluginFileInfo);
        if (pluginFileHandle != INVALID_HANDLE_VALUE) 
        {
          do
          {
            ALLOC_MEM_DEFINE_SET(pluginLibraryFileName, wchar_t, MAX_PATH, 0);

            wcscpy_s(pluginLibraryFileName, MAX_PATH, pluginPath);
            wcscat_s(pluginLibraryFileName, MAX_PATH, pluginFileInfo.cFileName);

            // load library
            this->logger->Log(LOGGER_INFO, L"%s: %s: loading library: %s", this->hosterName, METHOD_LOAD_PLUGINS_NAME, pluginLibraryFileName);

            CHosterPluginMetadata *hosterPluginMetadata = this->CreateHosterPluginMetadata(&result, this->logger, this->configuration, this->hosterName, pluginLibraryFileName);
            CHECK_POINTER_HRESULT(result, hosterPluginMetadata, result, E_OUTOFMEMORY);

            CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = hosterPluginMetadata->CheckPlugin());

            if (SUCCEEDED(result))
            {
              // initialize plugin
              CPluginConfiguration *pluginConfiguration = this->CreatePluginConfiguration(&result, this->configuration);
              CHECK_POINTER_HRESULT(result, pluginConfiguration, result, E_OUTOFMEMORY);

              CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), hosterPluginMetadata->GetPlugin()->Initialize(pluginConfiguration), result);

              FREE_MEM_CLASS(pluginConfiguration);

              CHECK_CONDITION_HRESULT(result, this->hosterPluginMetadataCollection->Add(hosterPluginMetadata), result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                wchar_t *guid = ConvertGuidToString(hosterPluginMetadata->GetPlugin()->GetInstanceId());
                this->logger->Log(LOGGER_INFO, L"%s: %s: plugin '%s' successfully initialized, id: %s", this->hosterName, METHOD_LOAD_PLUGINS_NAME, hosterPluginMetadata->GetPlugin()->GetName(), guid);
                FREE_MEM(guid);
              }
              else
              {
                this->logger->Log(LOGGER_INFO, L"%s: %s: plugin '%s' not initialized, error: 0x%08X", this->hosterName, METHOD_LOAD_PLUGINS_NAME, hosterPluginMetadata->GetPlugin()->GetName(), result);
                FREE_MEM_CLASS(hosterPluginMetadata);
              }
            }
            else if (result == E_INVALID_PLUGIN_TYPE)
            {
              // plugin type is invalid for hoster, probably is valid for other hoster
              // we can continue
              this->logger->Log(LOGGER_WARNING, L"%s: %s: plugin '%s' has invalid type for hoster, ignoring plugin", this->hosterName, METHOD_LOAD_PLUGINS_NAME, hosterPluginMetadata->GetPlugin()->GetName());

              FREE_MEM_CLASS(hosterPluginMetadata);
              result = S_OK;
            }

            FREE_MEM(pluginLibraryFileName);

            if (this->hosterPluginMetadataCollection->Count() == maxPlugins)
            {
              break;
            }
          }
          while (FindNextFile(pluginFileHandle, &pluginFileInfo));

          FindClose(pluginFileHandle);
        }

        this->logger->Log(LOGGER_INFO, L"%s: %s: found plugins: %u", this->hosterName, METHOD_LOAD_PLUGINS_NAME, this->hosterPluginMetadataCollection->Count());

        FREE_MEM(pluginPath);
        FREE_MEM(pluginSearchPattern);
      }
    }
  }

  return result;
}

/* protected methods */
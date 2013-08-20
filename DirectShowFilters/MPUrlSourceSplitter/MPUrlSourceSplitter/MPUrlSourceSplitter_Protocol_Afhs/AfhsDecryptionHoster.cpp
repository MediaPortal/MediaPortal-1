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

#include "AfhsDecryptionHoster.h"

CAfhsDecryptionHoster::CAfhsDecryptionHoster(CLogger *logger, CParameterCollection *configuration)
  : CHoster(logger, configuration, L"AfhsDecryptionHoster", L"mpurlsourcesplitter_afhs_decryption_*.dll")
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_AFHS_DECRYPTION_PLUGIN_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME);

  this->activeDecryptionPlugin = NULL;
  this->decidedDecryptionPlugin = false;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_AFHS_DECRYPTION_PLUGIN_HOSTER_NAME, METHOD_CONSTRUCTOR_NAME);
}

CAfhsDecryptionHoster::~CAfhsDecryptionHoster(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, MODULE_AFHS_DECRYPTION_PLUGIN_HOSTER_NAME, METHOD_DESTRUCTOR_NAME);

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, MODULE_AFHS_DECRYPTION_PLUGIN_HOSTER_NAME, METHOD_DESTRUCTOR_NAME);
}

// hoster methods
PluginImplementation *CAfhsDecryptionHoster::AllocatePluginsMemory(unsigned int maxPlugins)
{
  return ALLOC_MEM(AfhsDecryptionPluginImplementation, maxPlugins);
}

PluginImplementation *CAfhsDecryptionHoster::GetPluginImplementation(unsigned int position)
{
  if ((this->pluginImplementations != NULL) && (position < this->pluginImplementationsCount))
  {
    return (((AfhsDecryptionPluginImplementation *)this->pluginImplementations) + position);
  }

  return NULL;
}

bool CAfhsDecryptionHoster::AppendPluginImplementation(HINSTANCE hLibrary, DESTROYPLUGININSTANCE destroyPluginInstance, PIPlugin plugin)
{
  bool result = __super::AppendPluginImplementation(hLibrary, destroyPluginInstance, plugin);
  if (result)
  {
    AfhsDecryptionPluginImplementation *implementation = (AfhsDecryptionPluginImplementation *)this->GetPluginImplementation(this->pluginImplementationsCount - 1);
    implementation->result = DecryptionResult_Unspecified;
  }

  return result;
}

void CAfhsDecryptionHoster::RemovePluginImplementation(void)
{
  __super::RemovePluginImplementation();
}

PluginConfiguration *CAfhsDecryptionHoster::GetPluginConfiguration(void)
{
  ALLOC_MEM_DEFINE_SET(pluginConfiguration, AfhsDecryptionPluginConfiguration, 1, 0);
  if (pluginConfiguration != NULL)
  {
    pluginConfiguration->configuration = this->configuration;
  }

  return pluginConfiguration;
}

// IPlugin interface implementation

const wchar_t *CAfhsDecryptionHoster::GetName(void)
{
  return (this->activeDecryptionPlugin != NULL) ? this->activeDecryptionPlugin->GetName() : NULL;
}

GUID CAfhsDecryptionHoster::GetInstanceId(void)
{
  return GUID_NULL;
}

HRESULT CAfhsDecryptionHoster::Initialize(PluginConfiguration *configuration)
{
  HRESULT result = S_OK;
  for(unsigned int i = 0; (SUCCEEDED(result) && (i < this->pluginImplementationsCount)); i++)
  {
    AfhsDecryptionPluginImplementation *pluginImplementation = (AfhsDecryptionPluginImplementation *)this->GetPluginImplementation(i);
    IAfhsDecryptionPlugin *decryptionPlugin = (IAfhsDecryptionPlugin *)pluginImplementation->pImplementation;

    result = decryptionPlugin->Initialize(configuration);
  }
  return result;
}

// IAfhsDecryptionPlugin interface implementation

HRESULT CAfhsDecryptionHoster::ClearSession(void)
{
  if (this->pluginImplementations != NULL)
  {
    for(unsigned int i = 0; i < this->pluginImplementationsCount; i++)
    {
      AfhsDecryptionPluginImplementation *pluginImplementation = (AfhsDecryptionPluginImplementation *)this->GetPluginImplementation(i);
      this->logger->Log(LOGGER_INFO, L"%s: %s: reseting decryption plugin: %s", this->moduleName, METHOD_CLEAR_SESSION_NAME, pluginImplementation->name);

      if (pluginImplementation->pImplementation != NULL)
      {
        IAfhsDecryptionPlugin *decryptionPlugin = (IAfhsDecryptionPlugin *)pluginImplementation->pImplementation;
        decryptionPlugin->ClearSession();
      }
      pluginImplementation->result = DecryptionResult_Unspecified;
    }
  }

  this->activeDecryptionPlugin = NULL;
  this->decidedDecryptionPlugin = false;

  return S_OK;
}

HRESULT CAfhsDecryptionHoster::ProcessSegmentsAndFragments(CAfhsDecryptionContext *context)
{
  HRESULT result = S_OK;

  if (this->decidedDecryptionPlugin)
  {
    // all decryption plugins returned valid values
    if (this->activeDecryptionPlugin != NULL)
    {
      result = this->activeDecryptionPlugin->ProcessSegmentsAndFragments(context);
    }
    else
    {
      // no active decryption plugin
      // just process every unprocessed segment and fragment
      for (unsigned int i = 0; i < context->GetSegmentsFragments()->Count(); i++)
      {
        CSegmentFragment *segmentFragment = context->GetSegmentsFragments()->GetItem(i);

        if (segmentFragment->IsDownloaded() && (!segmentFragment->IsProcessed()))
        {
          segmentFragment->SetProcessed(true);
        }
      }
    }
  }
  else if (this->pluginImplementationsCount != 0)
  {
    // some plugins still waiting with results
    bool pendingPlugin = false;

    // send segments and fragments to decryption plugins
    for (unsigned int i = 0; (i < this->pluginImplementationsCount) && (this->activeDecryptionPlugin == NULL); i++)
    {
      AfhsDecryptionPluginImplementation *implementation = (AfhsDecryptionPluginImplementation *)this->GetPluginImplementation(i);
      IAfhsDecryptionPlugin *plugin = (IAfhsDecryptionPlugin *)implementation->pImplementation;

      if ((implementation->result == DecryptionResult_Unspecified) ||
        (implementation->result == DecryptionResult_Pending))
      {
        bool skipPendingMessage = (implementation->result == DecryptionResult_Pending);
        // if decryption plugin returned DecryptionResult::NotKnown result than decryption plugin surely 
        // doesn't recognize any pattern in segments and fragments

        DecryptionResult pluginDecryptionResult = plugin->Supported(context);
        implementation->result = pluginDecryptionResult;

        switch(pluginDecryptionResult)
        {
        case DecryptionResult_Unspecified:
          this->logger->Log(LOGGER_WARNING, L"%s: %s: decryption plugin '%s' return unspecified result", MODULE_AFHS_DECRYPTION_PLUGIN_HOSTER_NAME, METHOD_PROCESS_SEGMENTS_AND_FRAGMENTS_NAME, implementation->name);
          break;
        case DecryptionResult_NotKnown:
          this->logger->Log(LOGGER_INFO, L"%s: %s: decryption plugin '%s' doesn't recognize any pattern", MODULE_AFHS_DECRYPTION_PLUGIN_HOSTER_NAME, METHOD_PROCESS_SEGMENTS_AND_FRAGMENTS_NAME, implementation->name);
          break;
        case DecryptionResult_Pending:
          if (!skipPendingMessage)
          {
            this->logger->Log(LOGGER_INFO, L"%s: %s: decryption plugin '%s' waits for more data", MODULE_AFHS_DECRYPTION_PLUGIN_HOSTER_NAME, METHOD_PROCESS_SEGMENTS_AND_FRAGMENTS_NAME, implementation->name);
          }
          pendingPlugin = true;
          break;
        case DecryptionResult_Known:
          this->logger->Log(LOGGER_INFO, L"%s: %s: decryption plugin '%s' recognizes pattern", MODULE_AFHS_DECRYPTION_PLUGIN_HOSTER_NAME, METHOD_PROCESS_SEGMENTS_AND_FRAGMENTS_NAME, implementation->name);
          this->activeDecryptionPlugin = plugin;
          break;
        case DecryptionResult_Error:
          this->logger->Log(LOGGER_INFO, L"%s: %s: decryption plugin '%s' return error", MODULE_AFHS_DECRYPTION_PLUGIN_HOSTER_NAME, METHOD_PROCESS_SEGMENTS_AND_FRAGMENTS_NAME, implementation->name);
          break;
        default:
          this->logger->Log(LOGGER_WARNING, L"%s: %s: decryption plugin '%s' return unknown result", MODULE_AFHS_DECRYPTION_PLUGIN_HOSTER_NAME, METHOD_PROCESS_SEGMENTS_AND_FRAGMENTS_NAME, implementation->name);
          break;
        }
      }
    }

    this->decidedDecryptionPlugin = (!pendingPlugin) || (this->activeDecryptionPlugin != NULL);
  }
  else
  {
    // no decryption plugin available, so mark all downloaded segments and fragments as processed
    this->decidedDecryptionPlugin = true;
  }

  return result;
}
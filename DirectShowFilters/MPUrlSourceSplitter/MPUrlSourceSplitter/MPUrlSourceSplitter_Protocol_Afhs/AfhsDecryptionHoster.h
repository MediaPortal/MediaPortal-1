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

#ifndef __AFHS_DECRYPTION_HOSTER_DEFINED
#define __AFHS_DECRYPTION_HOSTER_DEFINED

#include "Hoster.h"
#include "IAfhsSimpleDecryptionPlugin.h"
#include "IAfhsDecryptionPlugin.h"
#include "AfhsDecryptionContext.h"

#define MODULE_AFHS_DECRYPTION_PLUGIN_HOSTER_NAME                             L"AfhsDecryptionHoster"

struct AfhsDecryptionPluginImplementation : public PluginImplementation
{
  DecryptionResult result;
};

class CAfhsDecryptionHoster : public CHoster, public IAfhsSimpleDecryptionPlugin
{
public:
  CAfhsDecryptionHoster(CLogger *logger, CParameterCollection *configuration);
  ~CAfhsDecryptionHoster(void);

  // IAfhsSimpleDecryptionPlugin interface implementation

  // clears decryption plugin session
  // @return : S_OK if successfull
  HRESULT ClearSession(void);

  // process segments and fragments
  // @param context : decryption context of AFHS protocol
  // @result : S_OK if successful, error code otherwise
  HRESULT ProcessSegmentsAndFragments(CAfhsDecryptionContext *context);

  // IPlugin interface implementation

  // returns reference to null-terminated string which represents active protocol plugin name
  // @return : reference to null-terminated string
  const wchar_t *GetName(void);

  // get plugin instance ID
  // @return : always GUID_NULL
  GUID GetInstanceId(void);

  // initialize plugin implementation with configuration parameters
  // @param configuration : the reference to additional configuration parameters (created by plugin's hoster class)
  // @return : always E_NOTIMPL
  HRESULT Initialize(PluginConfiguration *configuration);

protected:
  // hoster methods

  // allocates memory for plugin implementations in specific hoster
  // @param maxPlugins : the maximum plugins for hoster
  // @return : allocated memory or NULL if error
  PluginImplementation *AllocatePluginsMemory(unsigned int maxPlugins);

  // gets plugins implementation at specified position
  // @param position : the plugin position
  // @return : reference to plugin implementation or NULL if error
  PluginImplementation *GetPluginImplementation(unsigned int position);

  // appends parser implementation to end of parsers implementations
  // @param plugin : reference to parser implementation structure
  // @return : true if successful, false otherwise (in that case MUST be called RemovePluginImplementation() method)
  bool AppendPluginImplementation(HINSTANCE hLibrary, DESTROYPLUGININSTANCE destroyPluginInstance, PIPlugin plugin);

  // removes last plugin implementation
  void RemovePluginImplementation(void); 

  // gets parser configuration for Initialize() method
  // @return : parser configuration
  PluginConfiguration *GetPluginConfiguration(void);

  // stores active decryption plugin
  PIAfhsDecryptionPlugin activeDecryptionPlugin;

  // specifies if decryption plugin is decided
  // is set when all decryption plugins returns valid values
  bool decidedDecryptionPlugin;
};

#endif
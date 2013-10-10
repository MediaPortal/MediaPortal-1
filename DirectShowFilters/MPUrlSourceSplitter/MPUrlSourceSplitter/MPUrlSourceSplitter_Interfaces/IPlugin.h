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

#ifndef __IPLUGIN_DEFINED
#define __IPLUGIN_DEFINED

#include "ParameterCollection.h"
#include "Logger.h"

#include <streams.h>

#define METHOD_INITIALIZE_NAME                                                L"Initialize()"

struct PluginConfiguration
{
  // the reference to additional configuration parameters (can be same parameters which were passed while creating instance), can be NULL
  CParameterCollection *configuration;
};

// defines interface for plugin implementation
// each plugin implementation will be in separate library and MUST implement this interface
struct IPlugin
{
public:
  // return reference to null-terminated string which represents plugin name
  // errors should be logged to log file and returned NULL
  // @return : reference to null-terminated string
  virtual const wchar_t *GetName(void) = 0;

  // get plugin instance ID
  // @return : GUID, which represents instance identifier or GUID_NULL if error
  virtual GUID GetInstanceId(void) = 0;

  // initialize plugin implementation with configuration parameters
  // @param configuration : the reference to additional configuration parameters (created by plugin's hoster class)
  // @return : S_OK if successfull
  virtual HRESULT Initialize(PluginConfiguration *configuration) = 0;
};

typedef IPlugin* PIPlugin;

extern "C"
{
  PIPlugin CreatePluginInstance(CLogger *logger, CParameterCollection *configuration);
  typedef PIPlugin (*CREATEPLUGININSTANCE)(CLogger *logger, CParameterCollection *configuration);

  void DestroyPluginInstance(PIPlugin pPlugin);
  typedef void (*DESTROYPLUGININSTANCE)(PIPlugin);
}

#endif
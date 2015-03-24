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

#ifndef __PLUGIN_DEFINED
#define __PLUGIN_DEFINED

#include "Logger.h"
#include "ParameterCollection.h"
#include "PluginConfiguration.h"
#include "Flags.h"

#define METHOD_INITIALIZE_NAME                                        L"Initialize()"

#ifndef METHOD_CLEAR_SESSION_NAME
#define METHOD_CLEAR_SESSION_NAME                                     L"ClearSession()"
#endif

#define MINIMUM_RECEIVED_DATA_FOR_SPLITTER                            1 * 1024 * 1024

#define PLUGIN_FLAG_NONE                                              FLAGS_NONE

#define PLUGIN_FLAG_SPLITTER                                          (1 << (FLAGS_LAST + 0))
#define PLUGIN_FLAG_IPTV                                              (1 << (FLAGS_LAST + 1))

#define PLUGIN_FLAG_LAST                                              (FLAGS_LAST + 2)

class CPlugin : public CFlags
{
public:
  CPlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  virtual ~CPlugin(void);

  /* get methods */

  // return reference to null-terminated string which represents plugin name
  // errors should be logged to log file and returned NULL
  // @return : reference to null-terminated string
  virtual const wchar_t *GetName(void) = 0;

  // get plugin instance ID
  // @return : GUID, which represents instance identifier or GUID_NULL if error
  virtual GUID GetInstanceId(void) = 0;

  /* set methods */

  /* other methods */

  // tests if working as splitter filter
  // @return : true if working as splitter filter, false otherwise
  virtual bool IsSplitter(void);

  // tests if working as IPTV filter
  // @return : true if working as IPTV filter, false otherwise
  virtual bool IsIptv(void);

  // initialize plugin implementation with configuration parameters
  // @param configuration : the reference to additional configuration parameters (created by plugin's hoster class)
  // @return : S_OK if successfull, error code otherwise
  virtual HRESULT Initialize(CPluginConfiguration *configuration) = 0;

  // clears current session
  virtual void ClearSession(void);

protected:

  /* methods */
};

extern "C"
{
  CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  typedef CPlugin *(*CREATEPLUGIN)(HRESULT *result, CLogger *logger, CParameterCollection *configuration);

  void DestroyPlugin(CPlugin *plugin);
  typedef void (*DESTROYPLUGIN)(CPlugin *plugin);
}

#endif
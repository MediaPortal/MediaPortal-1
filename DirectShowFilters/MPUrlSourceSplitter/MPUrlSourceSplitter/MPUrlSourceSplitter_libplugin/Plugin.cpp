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

#pragma warning(push)
// disable warning: 'INT8_MIN' : macro redefinition
// warning is caused by stdint.h and intsafe.h, which both define same macro
#pragma warning(disable:4005)

#include "Plugin.h"
#include "Parameters.h"

#pragma warning(pop)

CPlugin::CPlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CFlags()
{
  this->logger = NULL;
  this->configuration = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, logger);
    CHECK_POINTER_DEFAULT_HRESULT(*result, configuration);

    this->logger = new CLogger(result, logger);
    this->configuration = new CParameterCollection(result);

    CHECK_POINTER_HRESULT(*result, this->logger, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->configuration, *result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(*result, this->configuration->Append(configuration), *result, E_OUTOFMEMORY);
  }
}

CPlugin::~CPlugin(void)
{
  FREE_MEM_CLASS(this->configuration);
  FREE_MEM_CLASS(this->logger);
}

/* get methods */

GUID CPlugin::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

/* set methods */

/* other methods */

HRESULT CPlugin::Initialize(CPluginConfiguration *configuration)
{
  HRESULT result = ((this->configuration != NULL) && (this->logger != NULL)) ? S_OK : E_NOT_VALID_STATE;
  CHECK_POINTER_HRESULT(result, configuration, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    this->configuration->Clear();

    CHECK_CONDITION_HRESULT(result, this->configuration->Append(configuration->GetConfiguration()), result, E_OUTOFMEMORY);

    this->configuration->LogCollection(this->logger, LOGGER_VERBOSE, this->GetModuleName(), METHOD_INITIALIZE_NAME);

    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_SPLITTER, true, PARAMETER_NAME_SPLITTER_DEFAULT) ? PLUGIN_FLAG_SPLITTER : PLUGIN_FLAG_NONE;
    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_IPTV, true, PARAMETER_NAME_IPTV_DEFAULT) ? PLUGIN_FLAG_IPTV : PLUGIN_FLAG_NONE;
    this->flags |= (this->configuration->GetValue(PARAMETER_NAME_DOWNLOAD_FILE_NAME, true, NULL) != NULL) ? PLUGIN_FLAG_DOWNLOADING : PLUGIN_FLAG_NONE;
  }

  return result;
}


bool CPlugin::IsSplitter(void)
{
  return this->IsSetFlags(PLUGIN_FLAG_SPLITTER);
}

bool CPlugin::IsIptv(void)
{
  return this->IsSetFlags(PLUGIN_FLAG_IPTV);
}

bool CPlugin::IsDownloading(void)
{
  return this->IsSetFlags(PLUGIN_FLAG_DOWNLOADING);
}

void CPlugin::ClearSession(void)
{
  this->flags = PLUGIN_FLAG_NONE;
}

/* protected methods */

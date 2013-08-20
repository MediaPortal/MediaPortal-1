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

#ifndef __AFHS_SIMPLE_DECRYPTION_PLUGIN_DEFINED
#define __AFHS_SIMPLE_DECRYPTION_PLUGIN_DEFINED

#include "IPlugin.h"
#include "AfhsDecryptionContext.h"

#define METHOD_CLEAR_SESSION_NAME                                             L"ClearSession()"
#define METHOD_PROCESS_SEGMENTS_AND_FRAGMENTS_NAME                            L"ProcessSegmentsAndFragments()"

// defines interface for AFHS simple decryption plugin implementation
struct IAfhsSimpleDecryptionPlugin : public IPlugin
{
  // clears decryption plugin session
  // @return : S_OK if successfull
  virtual HRESULT ClearSession(void) = 0;

  // process segments and fragments
  // @param context : decryption context of AFHS protocol
  // @result : S_OK if successful, S_FALSE means that next call of ProcessSegmentsAndFragments() method should be
  // after finished downloading of next segment and fragment, error code otherwise
  virtual HRESULT ProcessSegmentsAndFragments(CAfhsDecryptionContext *context) = 0;
};

typedef IAfhsSimpleDecryptionPlugin* PIAfhsSimpleDecryptionPlugin;

#endif
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

#ifndef __IFILTER_DEFINED
#define __IFILTER_DEFINED

#include "ISeeking.h"
#include "Logger.h"

#include <stdint.h>

// defines interface for filter
struct IFilter : public ISeeking
{
  // gets logger instance
  // @return : logger instance or NULL if error
  virtual CLogger *GetLogger(void) = 0;

  // gets total length of stream in bytes
  // @param totalLength : reference to total length variable
  // @return : S_OK if success, VFW_S_ESTIMATED if total length is not surely known, error code if error
  virtual HRESULT GetTotalLength(int64_t *totalLength) = 0;

  // gets available length of stream in bytes
  // @param availableLength : reference to available length variable
  // @return : S_OK if success, error code if error
  virtual HRESULT GetAvailableLength(int64_t *availableLength) = 0;
};

#endif
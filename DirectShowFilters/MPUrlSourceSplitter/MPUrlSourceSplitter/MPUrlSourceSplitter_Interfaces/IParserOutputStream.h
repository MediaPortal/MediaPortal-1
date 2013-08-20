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

#ifndef __IPARSER_OUTPUT_STREAM_DEFINED
#define __IPARSER_OUTPUT_STREAM_DEFINED

#include "IOutputStream.h"

// defines interface for stream output
struct IParserOutputStream : public IOutputStream
{
  // tests if filter is downloading
  // @return : true if downloading, false otherwise
  virtual bool IsDownloading(void) = 0;

  // finishes download with specified result
  // @param result : the result of download
  virtual void FinishDownload(HRESULT result) = 0;
};

#endif
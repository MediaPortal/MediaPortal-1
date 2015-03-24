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

#ifndef __STREAM_INFO_DEFINED
#define __STREAM_INFO_DEFINED

#include "MediaTypeCollection.h"

class CStreamInfo
{
public:
  CStreamInfo(HRESULT *result);
  CStreamInfo(HRESULT *result, AVFormatContext *formatContext, AVStream *stream, const wchar_t *containerFormat);

  ~CStreamInfo(void);

  /* get methods */

  // gets media types for stream
  // @return : media types for stream or NULL if error
  CMediaTypeCollection *GetMediaTypes(void);

  // gets stream description
  // @return : stream description or NULL if error
  const wchar_t *GetStreamDescription(void);

  /* set method */

  // sets stream description
  // @param streamDescription : stream description to set
  // @return : true if successful, false otherwise
  bool SetStreamDescription(const wchar_t *streamDescription);

  /* other methods */

protected:

  // holds media types for stream
  CMediaTypeCollection *mediaTypes;

  wchar_t *streamDescription;

  wchar_t *containerFormat;

  HRESULT CreateAudioMediaType(AVFormatContext *formatContext, AVStream *stream);
  HRESULT CreateVideoMediaType(AVFormatContext *formatContext, AVStream *stream);
  HRESULT CreateSubtitleMediaType(AVFormatContext *formatContext, AVStream *stream);
};

#endif
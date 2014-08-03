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

#ifndef __QUALITY_SEGMENT_URL_MODIFIER_DEFINED
#define __QUALITY_SEGMENT_URL_MODIFIER_DEFINED

class CQualitySegmentUrlModifier
{
public:
  // initializes a new instance of CQualitySegmentUrlModifier class
  // @param qualitySegmentUrlModifier : the quality segment url modifier
  CQualitySegmentUrlModifier(HRESULT *result, const wchar_t *qualitySegmentUrlModifier);

  // destructor
  ~CQualitySegmentUrlModifier(void);

  // gets quality segment url modifier
  // @return : quality segment url modifier or NULL if error
  const wchar_t *GetQualitySegmentUrlModifier(void);

private:
  // stores quality segment url modifier
  wchar_t *qualitySegmentUrlModifier;
};

#endif
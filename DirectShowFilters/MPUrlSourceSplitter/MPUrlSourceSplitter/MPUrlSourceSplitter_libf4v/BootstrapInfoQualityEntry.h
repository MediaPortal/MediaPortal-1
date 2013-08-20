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

#ifndef __BOOTSTRAP_INFO_QUALITY_ENTRY_DEFINED
#define __BOOTSTRAP_INFO_QUALITY_ENTRY_DEFINED

class CBootstrapInfoQualityEntry
{
public:
  // initializes a new instance of CBootstrapInfoQualityEntry class
  // @param qualityEntry : the quality entry
  CBootstrapInfoQualityEntry(const wchar_t *qualityEntry);

  // destructor
  ~CBootstrapInfoQualityEntry(void);

  // gets quality entry
  // @return : quality entry or NULL if error
  const wchar_t *GetQualityEntry(void);

private:
  // stores quality entry
  wchar_t *qualityEntry;
};

#endif
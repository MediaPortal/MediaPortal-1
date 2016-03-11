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

#ifndef __PMTSTREAMDESCRIPTION_DEFINED
#define __PMTSTREAMDESCRIPTION_DEFINED

#include "MPIPTVSourceExports.h"

class MPIPTVSOURCE_API PmtStreamDescription
{
public:
  PmtStreamDescription(const unsigned char *data, unsigned int length);
  ~PmtStreamDescription(void);

  // gets description length
  // @return : description length or UINT_MAX if error
  unsigned int GetDescriptionLength();

  // gets description data
  // caller is responsible for freeing memory
  // @return : description data or NULL if error
  unsigned char *GetDescription();

  // gets stream type
  // @return : the stream type or UINT_MAX if error
  unsigned int GetStreamType();

  // gets stream PID
  // @return : the stream PID or UINT_MAX if error
  unsigned int GetStreamPid();

  // gets stream descriptor length
  // @return : the stream descriptor length or UINT_MAX if error
  unsigned int GetStreamDescriptorLength();

  // gets stream descriptor
  // caller is responsible for freeing memory
  // @return : the stream descriptor or NULL if error
  unsigned char *GetStreamDescriptor();

  // tests if PMT stream descritpion is valid
  // @return : true if successful, false otherwise
  bool IsValid();

protected:
  // stream description data
  unsigned char *description;
  // length of stream description
  unsigned int length;
};

#endif
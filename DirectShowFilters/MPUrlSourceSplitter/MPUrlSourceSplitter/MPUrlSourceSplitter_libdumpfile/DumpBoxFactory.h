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

#ifndef __DUMP_BOX_FACTORY_DEFINED
#define __DUMP_BOX_FACTORY_DEFINED

#include "BoxFactory.h"

class CDumpBoxFactory : public CBoxFactory
{
public:
  CDumpBoxFactory(HRESULT *result);
  virtual ~CDumpBoxFactory(void);

  // creates box based on type
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @return : box or NULL if error
  virtual CBox *CreateBox(const uint8_t *buffer, uint32_t length);
};

#endif
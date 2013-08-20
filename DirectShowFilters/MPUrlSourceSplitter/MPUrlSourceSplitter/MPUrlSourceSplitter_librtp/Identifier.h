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

#ifndef __IDENTIFIER_DEFINED
#define __IDENTIFIER_DEFINED

class CIdentifier
{
public:
  // initializes a new instance of CIdentifier class
  CIdentifier(void);
  virtual ~CIdentifier(void);

  /* get methods */

  // gets identifier
  // @return : identifier
  virtual unsigned int GetIdentifier(void);

  /* set methods */

  // sets identifier
  // @param identifier : identifier to set
  virtual void SetIdentifier(unsigned int identifier);

  /* other methods */

protected:

  // holds identifier
  unsigned int identifier;
};

#endif
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

#ifndef __CONTRIBUTE_SOURCE_IDENTIFIER_DEFINED
#define __CONTRIBUTE_SOURCE_IDENTIFIER_DEFINED

class CContributeSourceIdentifier
{
public:
  // initializes a new instance of CContributeSourceIdentifier class
  CContributeSourceIdentifier(void);

  // initializes a new instance of CContributeSourceIdentifier class
  // @param identifier : contribute source identifier to set
  CContributeSourceIdentifier(unsigned int identifier);

  ~CContributeSourceIdentifier(void);

  /* get methods */

  // gets contribute source identifier
  // @return : contribute source identifier
  unsigned int GetIdentifier(void);

  /* set methods */

  // sets contribute source identifier
  // @param identifier : contribute source identifier to set
  void SetIdentifier(unsigned int identifier);

  /* other methods */

  // deeply clones current instance
  // @return : deep clone of current instance or NULL if error
  CContributeSourceIdentifier *Clone(void);

protected:

  // holds contribute source identifier
  unsigned int identifier;
};

#endif
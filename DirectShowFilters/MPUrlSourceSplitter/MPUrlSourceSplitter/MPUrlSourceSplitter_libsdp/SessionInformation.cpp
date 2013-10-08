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

#include "StdAfx.h"

#include "SessionInformation.h"

CSessionInformation::CSessionInformation(void)
{
  this->sessionInformation = NULL;
}

CSessionInformation::~CSessionInformation(void)
{
  FREE_MEM(this->sessionInformation);
}

/* get methods */

const wchar_t *CSessionInformation::GetSessionInformation(void)
{
  return this->sessionInformation;
}

/* set methods */

/* other methods */

void CSessionInformation::Clear(void)
{
  __super::Clear();

  FREE_MEM(this->sessionInformation);
}

unsigned int CSessionInformation::Parse(const wchar_t *buffer, unsigned int length)
{
  unsigned int tempResult = __super::Parse(buffer, length);
  unsigned int result = (tempResult > SESSION_TAG_SIZE) ? tempResult : 0;

  if (result != 0)
  {
    // successful parsing of session tag
    // compare it to our session tag
    result = (wcscmp(this->originalTag, TAG_SESSION_INFORMATION) == 0) ? result : 0;
    result = (this->tagContent != NULL) ? result : 0;
  }

  if (result != 0)
  {
    this->instanceTag = Duplicate(TAG_SESSION_INFORMATION);
    this->sessionInformation = Duplicate(this->tagContent);
    
    result = (this->instanceTag != NULL) ? result : 0;
    result = (this->sessionInformation != NULL) ? result : 0;
  }

  return result;
}
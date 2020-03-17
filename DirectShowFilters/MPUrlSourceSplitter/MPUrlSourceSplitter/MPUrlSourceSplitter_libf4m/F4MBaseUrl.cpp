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

#include "F4MBaseUrl.h"

CF4MBaseUrl::CF4MBaseUrl(void)
{
  this->baseUrl = NULL;
}

CF4MBaseUrl::~CF4MBaseUrl(void)
{
  FREE_MEM(this->baseUrl);
}

/* get methods */

const wchar_t *CF4MBaseUrl::GetBaseUrl(void)
{
  return this->baseUrl;
}

 /* set methods */

bool CF4MBaseUrl::SetBaseUrl(const wchar_t *baseUrl)
{
  SET_STRING_RETURN_WITH_NULL(this->baseUrl, baseUrl);
}

/* other methods */

void CF4MBaseUrl::Clear(void)
{
  FREE_MEM(this->baseUrl);
}
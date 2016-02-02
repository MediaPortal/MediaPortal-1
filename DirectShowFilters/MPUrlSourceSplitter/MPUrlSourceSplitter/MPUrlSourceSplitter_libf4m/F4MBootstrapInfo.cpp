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

#include "F4MBootstrapInfo.h"

#include "base64.h"
#include "formatUrl.h"
#include "HttpCurlInstance.h"

CF4MBootstrapInfo::CF4MBootstrapInfo(void)
{
  this->id = NULL;
  this->profile = NULL;
  this->url = NULL;
  this->value = NULL;
}

CF4MBootstrapInfo::~CF4MBootstrapInfo(void)
{
  FREE_MEM(this->id);
  FREE_MEM(this->profile);
  FREE_MEM(this->url);
  FREE_MEM(this->value);
}

/* get methods */

const wchar_t *CF4MBootstrapInfo::GetId(void)
{
  return this->id;
}

const wchar_t *CF4MBootstrapInfo::GetProfile(void)
{
  return this->profile;
}

 const wchar_t *CF4MBootstrapInfo::GetUrl(void)
 {
   return this->url;
 }

const wchar_t *CF4MBootstrapInfo::GetValue(void)
{
  return this->value;
}

/* set methods */

bool CF4MBootstrapInfo::SetId(const wchar_t *id)
{
  SET_STRING_RETURN_WITH_NULL(this->id, id);
}

bool CF4MBootstrapInfo::SetProfile(const wchar_t *profile)
{
  SET_STRING_RETURN_WITH_NULL(this->profile, profile);
}

bool CF4MBootstrapInfo::SetUrl(const wchar_t *url)
{
  SET_STRING_RETURN_WITH_NULL(this->url, url);
}

bool CF4MBootstrapInfo::SetValue(const wchar_t *value)
{
  SET_STRING_RETURN_WITH_NULL(this->value, value);
}

/* other methods */

bool CF4MBootstrapInfo::IsValid(void)
{
  return ((this->id != NULL) && (this->profile != NULL) && (((this->url != NULL) && (this->value == NULL)) || ((this->url == NULL) && (this->value != NULL))));
}

bool CF4MBootstrapInfo::HasUrl(void)
{
  return (this->url != NULL);
}

bool CF4MBootstrapInfo::HasValue(void)
{
  return (this->value != NULL);
}

void CF4MBootstrapInfo::Clear(void)
{
  FREE_MEM(this->id);
  FREE_MEM(this->profile);
  FREE_MEM(this->url);
  FREE_MEM(this->value);
}
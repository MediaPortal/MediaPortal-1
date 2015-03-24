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

#include "Origin.h"
#include "conversions.h"

COrigin::COrigin(HRESULT *result)
  : CSessionTag(result)
{
  this->username = NULL;
  this->sessionId = NULL;
  this->version = 0;
  this->networkType = NULL;
  this->addressType = NULL;
  this->address = NULL;
}

COrigin::~COrigin(void)
{
  FREE_MEM(this->username);
  FREE_MEM(this->sessionId);
  FREE_MEM(this->networkType);
  FREE_MEM(this->addressType);
  FREE_MEM(this->address);
}

/* get methods */

const wchar_t *COrigin::GetUserName(void)
{
  return this->username;
}

const wchar_t *COrigin::GetSessionId(void)
{
  return this->sessionId;
}

unsigned int COrigin::GetVersion(void)
{
  return this->version;
}

const wchar_t *COrigin::GetNetworkType(void)
{
  return this->networkType;
}

const wchar_t *COrigin::GetAddressType(void)
{
  return this->addressType;
}

const wchar_t *COrigin::GetAddress(void)
{
  return this->address;
}

/* set methods */

/* other methods */

bool COrigin::IsUserNameNotSpecified(void)
{
  return this->IsSetFlags(ORIGIN_FLAG_USER_NAME_NOT_SPECIFIED);
}

bool COrigin::IsNetworkTypeInternet(void)
{
  return this->IsSetFlags(ORIGIN_FLAG_NETWORK_TYPE_INTERNET);
}

bool COrigin::IsAddressTypeIPV4(void)
{
  return this->IsSetFlags(ORIGIN_FLAG_ADDRESS_TYPE_IPV4);
}

bool COrigin::IsAddressTypeIPV6(void)
{
  return this->IsSetFlags(ORIGIN_FLAG_ADDRESS_TYPE_IPV6);
}

void COrigin::Clear(void)
{
  __super::Clear();

  FREE_MEM(this->username);
  FREE_MEM(this->sessionId);
  FREE_MEM(this->networkType);
  FREE_MEM(this->addressType);
  FREE_MEM(this->address);
}

unsigned int COrigin::Parse(const wchar_t *buffer, unsigned int length)
{
  unsigned int tempResult = __super::Parse(buffer, length);
  unsigned int result = (tempResult > SESSION_TAG_SIZE) ? tempResult : 0;

  if (result != 0)
  {
    // successful parsing of session tag
    // compare it to our session tag
    result = (wcscmp(this->originalTag, TAG_ORIGIN) == 0) ? result : 0;
    result = (this->tagContent != NULL) ? result : 0;
  }

  if (result != 0)
  {
    this->instanceTag = Duplicate(TAG_ORIGIN);
    unsigned int tagContentLength = result - SESSION_TAG_SIZE - 1;
    result = (this->instanceTag != NULL) ? result : 0;

    // user name
    unsigned int position = 0;
    int index = IndexOf(this->tagContent, tagContentLength, L" ", 1);
    result = (index > 0) ? result : 0;

    if (result != 0)
    {
      this->username = Substring(this->tagContent, position, index);
      result = (this->username != NULL) ? result : 0;
    }

    if (result != 0)
    {
      position += index + 1;
    }

    // session ID
    index = IndexOf(this->tagContent + position, tagContentLength - position, L" ", 1);
    result = (index > 0) ? result : 0;

    if (result != 0)
    {
      this->sessionId = Substring(this->tagContent, position, index);
      result = (this->sessionId != NULL) ? result : 0;
    }

    if (result != 0)
    {
      position += index + 1;
    }

    // version
    index = IndexOf(this->tagContent + position, tagContentLength - position, L" ", 1);
    result = (index > 0) ? result : 0;

    if (result != 0)
    {
      this->version = GetValueUint(this->tagContent + position, ORIGIN_VERSION_DEFAULT);
    }

    if (result != 0)
    {
      position += index + 1;
    }

    // network type
    index = IndexOf(this->tagContent + position, tagContentLength - position, L" ", 1);
    result = (index > 0) ? result : 0;

    if (result != 0)
    {
      this->networkType = Substring(this->tagContent, position, index);
      result = (this->networkType != NULL) ? result : 0;
    }

    if (result != 0)
    {
      position += index + 1;
    }

    // address type
    index = IndexOf(this->tagContent + position, tagContentLength - position, L" ", 1);
    result = (index > 0) ? result : 0;

    if (result != 0)
    {
      this->addressType = Substring(this->tagContent, position, index);
      result = (this->addressType != NULL) ? result : 0;
    }

    if (result != 0)
    {
      position += index + 1;
    }

    // address
    result = (position < tagContentLength) ? result : 0;
    if (result != 0)
    {
      this->address = Substring(this->tagContent, position, tagContentLength - position);
      result = (this->address != NULL) ? result : 0;
    }

    // set flags
    if (result != 0)
    {
      if (wcscmp(this->username, ORIGIN_USER_NAME_NOT_SPECIFIED) == 0)
      {
        this->flags |= ORIGIN_FLAG_USER_NAME_NOT_SPECIFIED;
      }

      if (wcscmp(this->networkType, ORIGIN_NETWORK_TYPE_INTERNET) == 0)
      {
        this->flags |= ORIGIN_FLAG_NETWORK_TYPE_INTERNET;
      }

      if (wcscmp(this->addressType, ORIGIN_ADDRESS_TYPE_IPV4) == 0)
      {
        this->flags |= ORIGIN_FLAG_ADDRESS_TYPE_IPV4;
      }

      if (wcscmp(this->addressType, ORIGIN_ADDRESS_TYPE_IPV6) == 0)
      {
        this->flags |= ORIGIN_FLAG_ADDRESS_TYPE_IPV6;
      }
    }
  }

  return result;
}
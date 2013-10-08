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

#include "ConnectionData.h"

CConnectionData::CConnectionData(void)
  : CSessionTag()
{
  this->flags = FLAG_CONNECTION_DATA_NONE;
  this->addressType = NULL;
  this->connectionAddress = NULL;
  this->networkType = NULL;
}

CConnectionData::~CConnectionData(void)
{
  FREE_MEM(this->networkType);
  FREE_MEM(this->addressType);
  FREE_MEM(this->connectionAddress);
}

/* get methods */

const wchar_t *CConnectionData::GetNetworkType(void)
{
  return this->networkType;
}

const wchar_t *CConnectionData::GetAddressType(void)
{
  return this->addressType;
}

const wchar_t *CConnectionData::GetConnectionAddress(void)
{
  return this->connectionAddress;
}

/* set methods */

/* other methods */

bool CConnectionData::IsNetworkTypeInternet(void)
{
  return ((this->flags & FLAG_CONNECTION_DATA_NETWORK_TYPE_INTERNET) != 0);
}

bool CConnectionData::IsAddressTypeIPV4(void)
{
  return ((this->flags & FLAG_CONNECTION_DATA_ADDRESS_TYPE_IPV4) != 0);
}

void CConnectionData::Clear(void)
{
  __super::Clear();

  FREE_MEM(this->networkType);
  FREE_MEM(this->addressType);
  FREE_MEM(this->connectionAddress);
  this->flags = FLAG_CONNECTION_DATA_NONE;
}

unsigned int CConnectionData::Parse(const wchar_t *buffer, unsigned int length)
{
  unsigned int tempResult = __super::Parse(buffer, length);
  unsigned int result = (tempResult > SESSION_TAG_SIZE) ? tempResult : 0;

  if (result != 0)
  {
    // successful parsing of session tag
    // compare it to our session tag
    result = (wcscmp(this->originalTag, TAG_CONNECTION_DATA) == 0) ? result : 0;
    result = (this->tagContent != NULL) ? result : 0;
  }

  if (result != 0)
  {
    this->instanceTag = Duplicate(TAG_CONNECTION_DATA);
    unsigned int tagContentLength = result - SESSION_TAG_SIZE - 1;
    result = (this->instanceTag != NULL) ? result : 0;
    
    // network type
    unsigned int position = 0;
    int index = IndexOf(this->tagContent, tagContentLength, L" ", 1);
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

    // connection address
    result = (position < tagContentLength) ? result : 0;
    if (result != 0)
    {
      this->connectionAddress = Substring(this->tagContent, position, tagContentLength - position);
      result = (this->connectionAddress != NULL) ? result : 0;
    }

    // set flags
    if (result != 0)
    {
      if (wcscmp(this->networkType, CONNECTION_DATA_NETWORK_TYPE_INTERNET) == 0)
      {
        this->flags |= FLAG_CONNECTION_DATA_NETWORK_TYPE_INTERNET;
      }

      if (wcscmp(this->addressType, CONNECTION_DATA_ADDRESS_TYPE_IPV4) == 0)
      {
        this->flags |= FLAG_CONNECTION_DATA_ADDRESS_TYPE_IPV4;
      }
    }
  }

  return result;
}
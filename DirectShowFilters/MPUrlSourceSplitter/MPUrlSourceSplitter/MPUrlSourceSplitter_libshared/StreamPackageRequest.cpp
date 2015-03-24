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

#include "StreamPackageRequest.h"

CStreamPackageRequest::CStreamPackageRequest(HRESULT *result)
  : CFlags()
{
  this->id = 0;
  this->streamId = 0;
  this->startTime = 0;
}

CStreamPackageRequest::~CStreamPackageRequest(void)
{
}

/* get methods */

unsigned int CStreamPackageRequest::GetId(void)
{
  return this->id;
}

unsigned int CStreamPackageRequest::GetStreamId(void)
{
  return this->streamId;
}

unsigned int CStreamPackageRequest::GetStartTime(void)
{
  return this->startTime;
}

/* set methods */

void CStreamPackageRequest::SetId(unsigned int id)
{
  this->id = id;
}

void CStreamPackageRequest::SetStreamId(unsigned int streamId)
{
  this->streamId = streamId;
}

void CStreamPackageRequest::SetStartTime(unsigned int startTime)
{
  this->startTime = startTime;
}

/* other methods */

CStreamPackageRequest *CStreamPackageRequest::Clone(void)
{
  CStreamPackageRequest *result = this->CreatePackageRequest();

  if (result != NULL)
  {
    if (!this->InternalClone(result))
    {
      FREE_MEM_CLASS(result);
    }
  }

  return result;
}

/* protected methods */

bool CStreamPackageRequest::InternalClone(CStreamPackageRequest *item)
{
  bool result = (item != NULL);

  if (result)
  {
    item->flags = this->flags;
    item->id = this->id;
    item->streamId = this->streamId;
    item->startTime = this->startTime;
  }

  return result;
}


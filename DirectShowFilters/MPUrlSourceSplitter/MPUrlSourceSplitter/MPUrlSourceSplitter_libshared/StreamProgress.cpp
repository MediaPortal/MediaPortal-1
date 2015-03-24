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

#include "StreamProgress.h"

CStreamProgress::CStreamProgress(void)
{
  this->total = 0;
  this->current = 0;
  this->streamId = 0;
}

CStreamProgress::~CStreamProgress(void)
{
}

/* get methods */

int64_t CStreamProgress::GetTotalLength(void)
{
  return this->total;
}

int64_t CStreamProgress::GetCurrentLength(void)
{
  return this->current;
}

unsigned int CStreamProgress::GetStreamId(void)
{
  return this->streamId;
}

/* set methods */

void CStreamProgress::SetTotalLength(int64_t totalLength)
{
  this->total = totalLength;
}

void CStreamProgress::SetCurrentLength(int64_t currentLength)
{
  this->current = currentLength;
}

void CStreamProgress::SetStreamId(unsigned int streamId)
{
  this->streamId = streamId;
}

/* other methods */

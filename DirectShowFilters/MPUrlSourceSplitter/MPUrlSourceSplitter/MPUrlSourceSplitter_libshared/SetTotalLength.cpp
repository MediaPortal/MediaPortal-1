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

#include "SetTotalLength.h"

CSetTotalLength::CSetTotalLength(void)
{
  this->Clear();
}

CSetTotalLength::~CSetTotalLength(void)
{
}

/* get methods */

int64_t CSetTotalLength::GetTotalLength(void)
{
  return this->total;
}

bool CSetTotalLength::IsEstimate(void)
{
  return this->estimate;
}

/* set methods */

void CSetTotalLength::SetTotalLength(int64_t totalLength)
{
  this->total = totalLength;
  this->setValue = true;
}

void CSetTotalLength::SetTotalLength(int64_t totalLength, bool estimate)
{
  this->SetTotalLength(totalLength);
  this->SetEstimate(estimate);
}


void CSetTotalLength::SetEstimate(bool estimate)
{
  this->estimate = estimate;
  this->setValue = true;
}

/* other methods */

bool CSetTotalLength::IsSet(void)
{
  return this->setValue;
}

void CSetTotalLength::Clear(void)
{
  this->total = 0;
  this->estimate = false;
  this->setValue = false;
}
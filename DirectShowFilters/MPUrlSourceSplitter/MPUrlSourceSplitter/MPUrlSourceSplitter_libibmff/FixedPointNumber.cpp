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

#include "FixedPointNumber.h"

CFixedPointNumber::CFixedPointNumber(HRESULT *result, uint8_t integerPartSize, uint8_t fractionPartSize)
{
  this->integerPartSize = (integerPartSize <= INTEGER_PART_SIZE_MAX) ? integerPartSize : 0;
  this->fractionPartSize = (fractionPartSize <= FRACTION_PART_SIZE_MAX) ? fractionPartSize : 0;

  this->integerPart = 0;
  this->fractionPart = 0;

  this->integerPartMaximum = GetMaximum(this->integerPartSize);
  this->fractionPartMaximum = GetMaximum(this->fractionPartSize);
}

CFixedPointNumber::~CFixedPointNumber(void)
{
}

/* get methods */

uint32_t CFixedPointNumber::GetIntegerPart(void)
{
  return this->integerPart;
}

uint32_t CFixedPointNumber::GetFractionPart(void)
{
  return this->fractionPart;
}

uint64_t CFixedPointNumber::GetNumber(void)
{
  return (((uint64_t)this->integerPart << this->fractionPartSize) + (uint64_t)this->fractionPart);
}

/* set methods */

bool CFixedPointNumber::SetIntegerPart(uint32_t integerPart)
{
  if (integerPart <= this->integerPartMaximum)
  {
    this->integerPart = integerPart;
    return true;
  }

  return false;
}

bool CFixedPointNumber::SetFractionPart(uint32_t fractionPart)
{
  if (fractionPart <= this->fractionPartMaximum)
  {
    this->fractionPart = fractionPart;
    return true;
  }

  return false;
}

bool CFixedPointNumber::SetNumber(uint64_t number)
{
  uint64_t fractionPart = (number & this->fractionPartMaximum);
  uint64_t integerPart = (number >> this->fractionPartSize);

  if (integerPart <= this->integerPartMaximum)
  {
    this->fractionPart = (uint32_t)fractionPart;
    this->integerPart = (uint32_t)integerPart;
    return true;
  }

  return false;
}

/* other methods */

uint64_t CFixedPointNumber::GetMaximum(uint8_t size)
{
  uint64_t result = 0;

  for (uint8_t i = 0; i < size; i++)
  {
    result <<= 1;
    result++;
  }

  return result;
}
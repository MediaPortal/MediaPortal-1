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

#include "Matrix.h"

CMatrix::CMatrix(void)
  : CCollection()
{
  CFixedPointNumber *number = NULL;

  // a
  number = new CFixedPointNumber(17, 16);
  __super::Add(number);

  // b
  number = new CFixedPointNumber(17, 16);
  __super::Add(number);

  // u
  number = new CFixedPointNumber(3, 30);
  __super::Add(number);

  // c
  number = new CFixedPointNumber(17, 16);
  __super::Add(number);

  // d
  number = new CFixedPointNumber(17, 16);
  __super::Add(number);

  // v
  number = new CFixedPointNumber(3, 30);
  __super::Add(number);

  // x
  number = new CFixedPointNumber(17, 16);
  __super::Add(number);

  // y
  number = new CFixedPointNumber(17, 16);
  __super::Add(number);

  // w
  number = new CFixedPointNumber(3, 30);
  __super::Add(number);
}

CMatrix::~CMatrix(void)
{
  __super::Clear();
}

CFixedPointNumber *CMatrix::Clone(CFixedPointNumber *item)
{
  return NULL;
}

CFixedPointNumber *CMatrix::GetNumber(const wchar_t c)
{
  switch (c)
  {
  case L'a':
    return this->GetItem(0);
  case L'b':
    return this->GetItem(1);
  case L'u':
    return this->GetItem(2);
  case L'c':
    return this->GetItem(3);
  case L'd':
    return this->GetItem(4);
  case L'v':
    return this->GetItem(5);
  case L'x':
    return this->GetItem(6);
  case L'y':
    return this->GetItem(7);
  case L'w':
    return this->GetItem(8);
  default:
    return NULL;
  }
}

bool CMatrix::Add(CFixedPointNumber *item)
{
  return false;
}

bool CMatrix::Append(CCollection<CFixedPointNumber> *collection)
{
  return false;
}

void CMatrix::Clear(void)
{
}

bool CMatrix::Remove(unsigned int index)
{
  return false;
}

bool CMatrix::Remove(const wchar_t *key, void *context)
{
  return false;
}
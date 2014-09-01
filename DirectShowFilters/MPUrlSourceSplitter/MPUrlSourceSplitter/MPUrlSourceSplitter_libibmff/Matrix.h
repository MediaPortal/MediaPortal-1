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

#pragma once

#ifndef __MATRIX_DEFINED
#define __MATRIX_DEFINED

#include "Collection.h"
#include "FixedPointNumber.h"

class CMatrix : public CCollection<CFixedPointNumber>
{
public:
  CMatrix(HRESULT *result);
  ~CMatrix(void);

  // gets number identified by character (a, b, c, d, u, v, w, x, y, z)
  // @return : fixed number identified by character or NULL if error
  CFixedPointNumber *GetNumber(const wchar_t c);

  // add item to collection
  // @param item : the reference to item to add
  // @return : always false
  virtual bool Add(CFixedPointNumber *item);

  // append collection of items
  // @param collection : the reference to collection to add
  // @return : always false
  virtual bool Append(CCollection<CFixedPointNumber> *collection);

  // clear collection of items
  virtual void Clear(void);

  // remove item with specified index from collection
  // @param index : the index of item to remove
  // @return : always false
  virtual bool Remove(unsigned int index);

  // remove item with specified key from collection
  // @param key : key of item to remove
  // @param context : the reference to user defined context
  // @return : always false
  virtual bool Remove(const wchar_t *key, void *context);

protected:

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  CFixedPointNumber *Clone(CFixedPointNumber *item);
};

#endif
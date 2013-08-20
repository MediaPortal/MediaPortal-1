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

#ifndef __SERIALIZABLE_COLLECTION_DEFINED
#define __SERIALIZABLE_COLLECTION_DEFINED

#include "KeyedCollection.h"
#include "BufferHelper.h"

#include <stdint.h>

template <class TItem, class TItemKey> class CSerializableCollection : public CKeyedCollection<TItem, TItemKey>
{
public:
  // create new instance of CSerializableCollection class
  CSerializableCollection();

  virtual ~CSerializableCollection(void);

  // gets necessary buffer length for serializing instance
  // @return : necessary size for buffer
  virtual uint32_t GetSerializeSize(void);

  // serialize instance into buffer, buffer must be allocated before and must have necessary size
  // @param buffer : buffer which stores serialized instance
  // @return : true if successful, false otherwise
  virtual bool Serialize(uint8_t *buffer);

  // deserializes instance
  // @param : buffer which stores serialized instance
  // @return : true if successful, false otherwise
  virtual bool Deserialize(const uint8_t *buffer);
};

// implementation

template <class TItem, class TItemKey> CSerializableCollection<TItem, TItemKey>::CSerializableCollection()
  : CKeyedCollection<TItem, TItemKey>()
{
}

template <class TItem, class TItemKey> CSerializableCollection<TItem, TItemKey>::~CSerializableCollection(void)
{
}

template <class TItem, class TItemKey> uint32_t CSerializableCollection<TItem, TItemKey>::GetSerializeSize(void)
{
  uint32_t result = 4;

  for (unsigned int i = 0; i < this->Count(); i++)
  {
    result += this->GetItem(i)->GetSerializeSize();
  }

  return result;
}

template <class TItem, class TItemKey> bool CSerializableCollection<TItem, TItemKey>::Serialize(uint8_t *buffer)
{
  bool result = (buffer != NULL);
  uint32_t position = 0;

  if (result)
  {
    WBE32INC(buffer, position, this->Count());

    for (unsigned int i = 0; (result && (i < this->Count())); i++)
    {
      result &= this->GetItem(i)->Serialize(buffer + position);
      position += this->GetItem(i)->GetSerializeSize();
    }
  }

  return result;
}

template <class TItem, class TItemKey> bool CSerializableCollection<TItem, TItemKey>::Deserialize(const uint8_t *buffer)
{
  bool result = (buffer != NULL);
  uint32_t position = 0;

  if (result)
  {
    RBE32INC_DEFINE(buffer, position, count, unsigned int);

    for (unsigned int i = 0; (result && (i < count)); i++)
    {
      TItem *item = new TItem();
      result &= (item != NULL);

      if (result)
      {
        result &= item->Deserialize(buffer + position);
        position += item->GetSerializeSize();

        if (result)
        {
          result &= this->Add(item);
        }
      }

      if (!result)
      {
        FREE_MEM_CLASS(item);
      }
    }
  }

  return result;
}

#endif


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

#ifndef __DECRYPTED_DATA_COLLECTION_DEFINED
#define __DECRYPTED_DATA_COLLECTION_DEFINED

#include "Collection.h"
#include "DecryptedData.h"

class CDecryptedDataCollection : public CCollection<CDecryptedData>
{
public:
  CDecryptedDataCollection(void);
  ~CDecryptedDataCollection(void);

  // adds decrypted data to collection
  // @param decryptedData : decrypted data to add
  // @param decryptedLength : decrypted data length to add
  // @param errorCode : decryptor error code
  // @param error : decryptor error (int UTF8)
  // @return : true if successful, false otherwise
  bool Add(uint8_t *decryptedData, unsigned int decryptedLength, uint32_t errorCode, char *error);

protected:

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  CDecryptedData *Clone(CDecryptedData *item);
};

#endif
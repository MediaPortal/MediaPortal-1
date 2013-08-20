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
#include "EncryptedDataCollection.h"

CEncryptedDataCollection::CEncryptedDataCollection(void)
  : CCollection()
{
}

CEncryptedDataCollection::~CEncryptedDataCollection(void)
{
}

bool CEncryptedDataCollection::Add(uint8_t *encryptedData, unsigned int encryptedLength, CAkamaiFlvPacket *akamaiFlvPacket)
{
  CEncryptedData *data = new CEncryptedData();
  bool result = (data != NULL);
  if (result)
  {
    data->SetAkamaiFlvPacket(akamaiFlvPacket);
    data->SetEncryptedLength(encryptedLength);
    data->SetEncryptedData(encryptedData);

    if (result)
    {
      result = __super::Add(data);
    }
  }

  if (!result)
  {
    data->SetEncryptedData(NULL);
    data->SetAkamaiFlvPacket(NULL);

    FREE_MEM_CLASS(data);
  }
  return result;
}

CEncryptedData *CEncryptedDataCollection::Clone(CEncryptedData *item)
{
  return NULL;
}
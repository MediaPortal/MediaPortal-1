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

#include "ParsedMediaDataBox.h"

CParsedMediaDataBox::CParsedMediaDataBox(HRESULT *result)
{
  this->akamaiGuid = NULL;
  this->isMediaDataBox = false;
  this->akamaiFlvPacketCollection = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->akamaiFlvPacketCollection = new CAkamaiFlvPacketCollection(result);

    CHECK_POINTER_HRESULT(*result, this->akamaiFlvPacketCollection, *result, E_OUTOFMEMORY);
  }
}

CParsedMediaDataBox::~CParsedMediaDataBox(void)
{
  FREE_MEM(this->akamaiGuid);
  FREE_MEM_CLASS(this->akamaiFlvPacketCollection);
}

/* get methods */

const wchar_t *CParsedMediaDataBox::GetAkamaiGuid(void)
{
  return this->akamaiGuid;
}

CAkamaiFlvPacketCollection *CParsedMediaDataBox::GetAkamaiFlvPackets(void)
{
  return this->akamaiFlvPacketCollection;
}

/* set methods */

bool CParsedMediaDataBox::SetAkamaiGuid(const wchar_t *akamaiGuid)
{
  SET_STRING_RETURN_WITH_NULL(this->akamaiGuid, akamaiGuid);
}

void CParsedMediaDataBox::SetMediaDataBox(bool isMediaDataBox)
{
  this->isMediaDataBox = isMediaDataBox;
}

/* other methods */

bool CParsedMediaDataBox::IsMediaDataBox(void)
{
  return this->isMediaDataBox;
}
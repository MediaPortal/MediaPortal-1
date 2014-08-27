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

#include "DecryptedData.h"

CDecryptedData::CDecryptedData(void)
{
  this->decryptedData = NULL;
  this->decryptedLength = 0;
  this->error = NULL;
  this->errorCode = 0;
}

CDecryptedData::~CDecryptedData(void)
{
  FREE_MEM(this->decryptedData);
  FREE_MEM(this->error);
}

/* get methods */

uint8_t *CDecryptedData::GetDecryptedData(void)
{
  return this->decryptedData;
}

unsigned int CDecryptedData::GetDecryptedLength(void)
{
  return this->decryptedLength;
}

uint32_t CDecryptedData::GetErrorCode(void)
{
  return this->errorCode;
}

const char *CDecryptedData::GetError(void)
{
  return this->error;
}

/* set methods */

void CDecryptedData::SetDecryptedData(uint8_t *decryptedData, unsigned int decryptedLength)
{
  this->decryptedData = decryptedData;
  this->decryptedLength = decryptedLength;
}

void CDecryptedData::SetErrorCode(uint32_t errorCode)
{
  this->errorCode = errorCode;
}

void CDecryptedData::SetError(char *error)
{
  this->error = error;
}

/* other methods */

/* protected methods */
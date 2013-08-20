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

#ifndef __DECRYPTED_DATA_DEFINED
#define __DECRYPTED_DATA_DEFINED

class CDecryptedData
{
public:
  CDecryptedData(void);
  ~CDecryptedData(void);

  /* get methods */

  // gets decrypted data
  // @return : decrypted data
  uint8_t *GetDecryptedData(void);

  // gets decrypted data length
  // @return : decrypted data length
  unsigned int GetDecryptedLength(void);

  // gets decryptor error code
  // @return : decryptor error code
  uint32_t GetErrorCode(void);

  // gets decryptor error (in UTF8)
  // @return : decryptor error (in UTF8)
  const char *GetError(void);

  /* set methods */

  // sets decrypted data
  // @param decryptedData : decrypted data to set
  // @param decryptedLength : length of decrypted data to set
  void SetDecryptedData(uint8_t *decryptedData, unsigned int decryptedLength);

  // sets decryptor error code
  // @param errorCode : decryptor error code to set
  void SetErrorCode(uint32_t errorCode);

  // sets decryptor error (in UTF8)
  // @param error: decryptor error (in UTF8) to set
  void SetError(char *error);

  /* other methods */

protected:

  // holds decrypted data
  uint8_t *decryptedData;

  // holds decrypted data length
  unsigned int decryptedLength;

  // holds decryptor error code
  uint32_t errorCode;

  // holds decryptor error in UTF8
  char *error;
};

#endif
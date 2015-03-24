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

#ifndef __PROGRAM_DEFINITION_DEFINED
#define __PROGRAM_DEFINITION_DEFINED

#include <stdint.h>

class CProgramDefinition
{
public:
  CProgramDefinition(HRESULT *result);
  ~CProgramDefinition(void);

  /* get methods */

  // gets stream type
  // @return : stream type
  unsigned int GetStreamType(void);

  // gets elementary PID
  // @return : elementary PID
  unsigned int GetElementaryPID(void);

  // gets ES info size
  // @return : ES info size
  unsigned int GetEsInfoSize(void);

  // gets ES info descriptor
  // @return : ES info descriptor or NULL if GetEsInfoSize() is zero
  const uint8_t *GetEsInfoDescriptor(void);

  /* set methods */

  // sets stream type
  // @param streamType : the stream type to set
  void SetStreamType(unsigned int streamType);

  // sets elementary PID
  // @param elementaryPID : the elementary PID to set
  void SetElementaryPID(unsigned int elementaryPID);

  // sets ES info descriptor
  // @param descriptor : descriptor to set
  // @param size : the size of descriptor
  // @return : true if successful, false otherwise
  bool SetEsInfoDescriptor(const uint8_t *descriptor, unsigned int size);
  
  /* other methods */

  // deeply clones current instance
  // @return : deep clone of current instance or NULL if error
  CProgramDefinition *Clone(void);

protected:
  // program definition structure
  // stream_type        8 bits
  // reserved           3 bits
  // elementary_PID     13 bits
  // reserved           4 bits
  // ES_info_length     12 bits
  // descriptor         ES_info_length bytes

  uint8_t streamType;
  uint16_t elementaryPID;
  uint16_t esInfoSize;
  uint8_t *esInfoDescriptor;

  /* methods */
};

#endif
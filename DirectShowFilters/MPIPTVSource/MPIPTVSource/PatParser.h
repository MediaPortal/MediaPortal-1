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

#ifndef __PATPARSER_DEFINED
#define __PATPARSER_DEFINED

#include "MPIPTVSourceExports.h"
#include "Crc32.h"

class MPIPTVSOURCE_API PatParser
{
public:
  PatParser(Crc32 *crc32);
  ~PatParser(void);

  // sets PAT data
  // @param data : the pointer to raw PAT packet to set
  // @param length : the length of raw PAT packet
  // @return : true if successful, false otherwise
  bool SetPatData(const unsigned char *data, unsigned int length);

  // gets MPEG TS PAT packet
  // caller is responsible for freeing memory
  // @return : pointer to MPEG TS PAT packet or NULL if error
  unsigned char *GetPatPacket();

  // gets PAT table ID (shall be zero)
  // @return : PAT table ID or UINT_MAX if error
  unsigned int GetTableId();

  // gets section syntax indicator (shall be true)
  bool GetSectionSyntaxIndicator();

  // gets section length in bytes
  // @return : section length or UINT_MAX if error
  unsigned int GetSectionLength();

  // gets transport stream id
  // @return : transport stream ID or UINT_MAX if error
  unsigned int GetTransportStreamId();

  // gets version number
  // the version number shall be incremented by 1 modulo 32 whenever the definition of the PAT changes
  // when the GetCurrentNextIndicator() is true, then the GetVersionNmber() shall be that
  // of the currently applicable PAT
  // when the GetCurrentNextIndicator() is false, then the GetVersionNumber() shall be that
  // of the next applicable PAT
  // @return : version number or UINT_MAX if error
  unsigned int GetVersionNumber();

  // gets current next indicator
  bool GetCurrentNextIndicator();

  // gets section number
  // the section number of the first section in the PAT shall be 0x00
  // it shall be incremented by 1 with each additional section in the PAT
  // @return : section number or UINT_MAX if error
  unsigned int GetSectionNumber();

  // gets last section number
  // the number of the last section (that is, the section with the highest section_number) of the complete PAT
  // @return : last section number or UINT_MAX if error
  unsigned int GetLastSectionNumber();

  // gets program number count
  // @return : the count of programs or UINT_MAX if error
  unsigned int GetProgramNumberCount();

  // gets program number (SID) at specified position
  // program number specifies the program to which the PID is applicable
  // when set to 0x0000, then the PID reference shall be the network PID
  // for all other cases the value of this field is user defined
  // this field shall not take any single value more than once within one version of the PAT
  // @param position : the position of program
  // @return : the program number or UINT_MAX if error
  unsigned int GetProgramNumber(unsigned int position);

  // sets program number (SID) at specified position
  // @param position : the position of program
  // @param programNumber : the program number (SID) to set
  // @return : true if successful, false otherwise
  bool SetProgramNumber(unsigned int position, unsigned int programNumber);

  // gets program PID at specified position
  // @param position : the position of program
  // @return : program PID or UINT_MAX if error
  unsigned int GetPid(unsigned int position);

  // sets program PID at specified position
  // @param position : the position of program
  // @param pid : the PID to set
  // @return : true if successful, false otherwise
  bool SetPid(unsigned int position, unsigned int pid);

  // tests if packet is valid
  // @return : true if valid, false otherwise
  bool IsValidPacket();

  // gets packet CRC32
  // @return : packet CRC32
  unsigned int GetCrc32();

  // sets packet CRC32
  // @return : true if successful, false otherwise
  bool SetCrc32(unsigned int crc32);

  // tests if CRC32 is correct
  // @return : true if correct, false otherwise
  bool CheckCrc32();

  // recalculate packet CRC32
  // @return : true if successful, false otherwise
  bool RecalculateCrc32();

private:
  unsigned char *packet;
  unsigned int packetLength;

  // instance of CRC32
  Crc32 *crc32;
  // specifies if CRC32 instance have been created or passed as reference
  bool crc32created;
};

#endif
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

#ifndef __PMTPARSER_DEFINED
#define __PMTPARSER_DEFINED

#include "MPIPTVSourceExports.h"
#include "Crc32.h"
#include "PmtStreamDescriptionCollection.h"

class MPIPTVSOURCE_API PmtParser
{
public:
  PmtParser(Crc32 *crc32);
  ~PmtParser(void);

  // sets PMT data
  // @param data : the pointer to raw PMT packet to set
  // @param length : the length of raw PMT packet
  // @return : true if successful, false otherwise
  bool SetPmtData(const unsigned char *data, unsigned int length);

  // gets MPEG TS PMT packet
  // caller is responsible for freeing memory
  // @return : pointer to MPEG TS PMT packet or NULL if error
  unsigned char *GetPmtPacket();

  // gets MPEG TS packet ID
  // @return : return packet ID or UINT_MAX if error
  unsigned int GetPacketPid();

  // sets MPEG TS packet ID
  // @param pid : the MPEG TS packet ID to set
  // @return : true if successful, false otherwise
  bool SetPacketPid(unsigned int pid);

  // gets PMT table ID (shall be zero)
  // @return : PMT table ID or UINT_MAX if error
  unsigned int GetTableId();

  // gets section syntax indicator (shall be true)
  bool GetSectionSyntaxIndicator();

  // gets section length in bytes
  // @return : section length or UINT_MAX if error
  unsigned int GetSectionLength();

  // gets program number (SID)
  // @return : program number (SID) or UINT_MAX if error
  unsigned int GetProgramNumber();

  // sets program number (SID)
  // @param programNumber : the program number (SID) to set
  // @return : true if successful, false otherwise
  bool SetProgramNumber(unsigned int programNumber);

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

  // gets PCR PID
  // the PID of the Transport Stream packets which shall contain the PCR fields
  // valid for the program specified by program_number
  // if no PCR is associated with a program definition for private
  // streams, then this field shall take the value of 0x1FFF
  // @return : the PCR PID or UINT_MAX if error
  unsigned int GetPcrPid();

  // gets program info length
  // @return : program info length or UINT_MAX if error
  unsigned int GetProgramInfoLength();

  // gets program info descriptor
  // caller is responsible for freeing memory
  // @return : the program info descriptor or NULL if error
  unsigned char *GetProgramInfo();

  // gets all streams data length
  // @return : the all streams data length or UINT_MAX if error
  unsigned int GetStreamDataLength();

  // gets stream count
  // @return : the count of streams or UINT_MAX if error
  unsigned int GetStreamCount();

  // gets stream type at specified position
  // @param position : the position of stream
  // @return : the stream type or UINT_MAX if error
  unsigned int GetStreamType(unsigned int position);

  // gets stream PID at specified position
  // @param position : the position of stream
  // @return : the stream PID or UINT_MAX if error
  unsigned int GetStreamPid(unsigned int position);

  // gets stream descriptor length at specified position
  // @param position : the position of stream
  // @return : the stream descriptor length or UINT_MAX if error
  unsigned int GetStreamDescriptorLength(unsigned int position);

  // gets stream descriptor at specified position
  // caller is responsible for freeing memory
  // @param position : the position of stream
  // @return : the stream descriptor or NULL if error
  unsigned char *GetStreamDescriptor(unsigned int position);

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

  // gets PMT stream description collection
  // @return : PMT stream description collection
  PmtStreamDescriptionCollection *GetPmtStreamDescriptions(void);

  // recalculates section length, must be called after changing stream description collection
  // @ return : true if successful, false otherwise
  bool RecalculateSectionLength();

  // sets section length
  // @param length : the length of section
  // @return : true if successful, false otherwise
  bool SetSectionLength(unsigned int length);

private:
  unsigned char *packet;
  unsigned int packetLength;

  // instance of CRC32
  Crc32 *crc32;
  // specifies if CRC32 instance have been created or passed as reference
  bool crc32created;

  unsigned int packetCrc32;

  PmtStreamDescriptionCollection *streamDescriptions;

  bool ParsePmtData(const unsigned char *data, unsigned int length);
};

#endif
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

#ifndef __TRANSPORT_STREAM_PROGRAM_MAP_SECTION_DEFINED
#define __TRANSPORT_STREAM_PROGRAM_MAP_SECTION_DEFINED

#include "Section.h"
#include "ProgramDefinitionCollection.h"
#include "DescriptorCollection.h"

#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_FLAG_NONE                                      SECTION_FLAG_NONE

#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_FLAG_LAST                                      (SECTION_FLAG_LAST + 0)

#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_TABLE_ID                                       0x00000002

#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_RESERVED_MASK                                  0x03
#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_RESERVED_SHIFT                                 6

#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_VERSION_NUMBER_MASK                            0x1F
#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_VERSION_NUMBER_SHIFT                           1

#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_CURRENT_NEXT_INDICATOR_MASK                    0x01
#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_CURRENT_NEXT_INDICATOR_SHIFT                   0

#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PCR_PID_MASK                                   0x1FFF
#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PCR_PID_SHIFT                                  0

#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_DESCRIPTORS_SIZE_MASK                          0x0FFF
#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_DESCRIPTORS_SIZE_SHIFT                         0

#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PROGRAM_DEFINITION_ELEMENTARY_PID_MASK         0x1FFF
#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PROGRAM_DEFINITION_ELEMENTARY_PID_SHIFT        0

#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PROGRAM_DEFINITION_ES_INFO_LENGTH_MASK         0x0FFF
#define TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PROGRAM_DEFINITION_ES_INFO_LENGTH_SHIFT        0

class CTransportStreamProgramMapSection : public CSection
{
public:
  CTransportStreamProgramMapSection(HRESULT *result);
  virtual ~CTransportStreamProgramMapSection(void);

  /* get methods */

  // gets program number
  // @return : program number
  unsigned int GetProgramNumber(void);

  // gets version
  // @return : version
  unsigned int GetVersion(void);

  // gets section number
  // @return : section number
  unsigned int GetSectionNumber(void);

  // gets last section number
  // @return : last section number
  unsigned int GetLastSectionNumber(void);

  // gets PCR PID
  // @return : PCR PID
  unsigned int GetPcrPID(void);

  // gets descriptors
  // @return : decriptor collections
  CDescriptorCollection *GetDescriptors(void);

  // gets program definitions
  // @return : program definitions
  CProgramDefinitionCollection *GetProgramDefinitions(void);

  /* set methods */

  // sets program number
  // @param : the program number to set
  void SetProgramNumber(unsigned int programNumber);

  /* other methods */

  // tests if current next indicator is set
  // @return : true if current next indicator is set, false otherwise
  bool IsCurrentNextIndicator(void);

  // parses specified PSI packet
  // @param psiPacket : the PSI packet to parse
  // @param startFromSectionPayload : the section payload index to start parsing
  // @return : S_OK if successfull, S_FALSE if more PSI packets are needed to complete section, error code otherwise
  virtual HRESULT Parse(CProgramSpecificInformationPacket *psiPacket, unsigned int startFromSectionPayload);

  // clears current instance to its default state
  virtual void Clear(void);

protected:
  // transport stream program map section structure (after section structure)
  // program_number               16 bits
  // reserved                     2 bits
  // version_number               5 bits
  // current_next_indicator       1 bit
  // section_number               8 bits
  // last_section_number          8 bits
  // reserved                     3 bits
  // PCR_PID                      13 bits
  // reserved                     4 bits
  // program_info_length          12 bits
  // descriptor                   program_info_length bytes
  // for (i = 0; i < N; i++)
  // {
  //    stream_type               8 bits
  //    reserved                  3 bits
  //    elementary_PID            13 bits
  //    reserved                  4 bits
  //    ES_info_length            12 bits
  //    descriptor                ES_info_length bytes
  // }

  uint16_t programNumber;
  uint8_t reservedVersionNumberCurrentNextIndicator;
  uint8_t sectionNumber;
  uint8_t lastSectionNumber;
  uint16_t pcrPID;

  CProgramDefinitionCollection *programDefinitions;
  CDescriptorCollection *descriptors;

  /* methods */

  // gets new instance of section
  // @return : new section instance or NULL if error
  virtual CSection *CreateItem(void);

  // deeply clones current instance
  // @param item : the section instance to clone
  // @return : true if successful, false otherwise
  virtual bool InternalClone(CSection *item);

  // gets whole section size
  // method is called to determine section size for storing section into buffer
  // @return : size of section
  virtual unsigned int GetSectionCalculatedSize(void);

  // gets whole section into payload
  // @return : number of bytes written into buffer, zero if not successful
  virtual unsigned int GetSectionInternal(void);

  // checks table ID against actual table ID
  // @return : true if table ID is valid, false otherwise
  virtual bool CheckTableId(void);
};

#endif
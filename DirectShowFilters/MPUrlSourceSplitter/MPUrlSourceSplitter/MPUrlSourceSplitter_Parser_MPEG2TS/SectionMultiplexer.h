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

#ifndef __SECTION_MULTIPLEXER_DEFINED
#define __SECTION_MULTIPLEXER_DEFINED

#include "Flags.h"
#include "SectionCollection.h"
#include "Mpeg2tsStreamFragmentContextCollection.h"
#include "ProgramSpecificInformationPacket.h"

#define SECTION_MULTIPLEXER_FLAG_NONE                                 FLAGS_NONE

#define SECTION_MULTIPLEXER_FLAG_LAST                                 (FLAGS_LAST + 0)

class CSectionMultiplexer : public CFlags
{
public:
  CSectionMultiplexer(HRESULT *result, unsigned int pid, unsigned int requestedPid, unsigned int continuityCounter);
  virtual ~CSectionMultiplexer();

  /* get methods */

  // gets multiplexer PID
  // @return : multiplexer PID
  unsigned int GetPID(void);

  // gets multiplexer requested PID
  // @return : multiplexer requested PID
  unsigned int GetRequestedPID(void);

  // gets continuity counter
  // @return : multiplexer continuity counter
  unsigned int GetContinuityCounter(void);

  /* set methods */

  /* other methods */

  // adds stream fragment context with specified stream fragment and packet index
  // @param streamFragment : the MPEG2 TS stream fragment to add context
  // @param packetIndex : the MPEG2 TS packet index
  // @param sectionPayloads : count of detected section payloads in MPEG2 TS packet
  // @return : true if successful, false otherwise
  bool AddStreamFragmentContext(CMpeg2tsStreamFragment *streamFragment, unsigned int packetIndex, unsigned int sectionPayloads);

  // adds section to multiplexer
  // @param section : the section to add to multiplexer
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT AddSection(CSection *section) = 0;

  // multiplexes available sections into stream fragments
  // return : S_OK if successful, error code otherwise
  virtual HRESULT MultiplexSections(void);

  // flushes stream fragment contexts and replaces MPEG2 TS packets with NULL MPEG2 TS packets
  // return : S_OK if successful, error code otherwise
  virtual HRESULT FlushStreamFragmentContexts(void);

protected:
  // holds pid for multiplexer
  unsigned int pid;
  // holds requested pid for multiplexer
  unsigned int requestedPid;
  // holds continuity counter
  unsigned int continuityCounter;
  // holds stream fragment contexts
  CMpeg2tsStreamFragmentContextCollection *streamFragmentContexts;
  // holds sections for multiplexing
  CSectionCollection *sections;

  // holds unused section payload count
  unsigned int sectionPayloadCount;
  // holds current MPEG2 TS packet, if still some data can fit
  CProgramSpecificInformationPacket *currentPacket;

  /* methods */

  // increases reference count in stream fragment
  // @param streamFragment : the MPEG2 TS stream fragment to increase reference count
  virtual void IncreaseReferenceCount(CMpeg2tsStreamFragment *streamFragment) = 0;

  // decreases reference count in stream fragment
  // @param streamFragment : the MPEG2 TS stream fragment to decrease reference count
  virtual void DecreaseReferenceCount(CMpeg2tsStreamFragment *streamFragment) = 0;
};

#endif
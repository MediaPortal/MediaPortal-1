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

#ifndef __MPEG2TS_STREAM_FRAGMENT_DEFINED
#define __MPEG2TS_STREAM_FRAGMENT_DEFINED

#include "StreamFragment.h"
#include "ProgramAssociationSectionPacketContextCollection.h"
#include "TransportStreamProgramMapSectionPacketContextCollection.h"

#define MPEG2TS_STREAM_FRAGMENT_FLAG_NONE                                                             STREAM_FRAGMENT_FLAG_NONE

// stream fragment processing workflow
// 1. ready for align
// 2. aligned
// 3. discontinuity processed
// 4A. program association section detection finished
// 4B. transport stream program map section detection finished
// 5A. program association section updated
// 5B. transport stream program map section updated
// 6. processed, loaded to memory, ...

// states 1, 2 and 3 are mutually exclusive, can be set only one of them
// states 4A and 4B can be set both
// states 5A and 5B can be set both
// states 4A and 5A are mutually exclusive, can be set one of them
// states 4B and 5B are mutually exclusive, can be set one of them
// states 5A and 5B are exclusive with state 6, transition to state 6 can happen only from both states 5A and 5B

#define MPEG2TS_STREAM_FRAGMENT_FLAG_READY_FOR_ALIGN                                                  (1 << (STREAM_FRAGMENT_FLAG_LAST + 0))
#define MPEG2TS_STREAM_FRAGMENT_FLAG_ALIGNED                                                          (1 << (STREAM_FRAGMENT_FLAG_LAST + 1))
#define MPEG2TS_STREAM_FRAGMENT_FLAG_DISCONTINUITY_PROCESSED                                          (1 << (STREAM_FRAGMENT_FLAG_LAST + 2))

#define MPEG2TS_STREAM_FRAGMENT_FLAG_PROGRAM_ASSOCIATION_SECTION_DETECTION_FINISHED                   (1 << (STREAM_FRAGMENT_FLAG_LAST + 3))
#define MPEG2TS_STREAM_FRAGMENT_FLAG_PROGRAM_ASSOCIATION_SECTION_UPDATED                              (1 << (STREAM_FRAGMENT_FLAG_LAST + 4))

#define MPEG2TS_STREAM_FRAGMENT_FLAG_TRANSPORT_STREAM_PROGRAM_MAP_SECTION_DETECTION_FINISHED          (1 << (STREAM_FRAGMENT_FLAG_LAST + 5))
#define MPEG2TS_STREAM_FRAGMENT_FLAG_TRANSPORT_STREAM_PROGRAM_MAP_SECTION_UPDATED                     (1 << (STREAM_FRAGMENT_FLAG_LAST + 6))

#define MPEG2TS_STREAM_FRAGMENT_FLAG_LAST                                                             (STREAM_FRAGMENT_FLAG_LAST + 7)

class CMpeg2tsStreamFragment : public CStreamFragment
{
public:
  CMpeg2tsStreamFragment(HRESULT *result);
  virtual ~CMpeg2tsStreamFragment(void);

  /* get methods */

  // gets request start position within protocol stream
  // @return : request start position within protocol stream or STREAM_FRAGMENT_START_POSITION_NOT_SET if not set
  int64_t GetRequestStartPosition(void);

  // gets program association section packet contexts
  // @return : program association section packet contexts
  CProgramAssociationSectionPacketContextCollection *GetProgramAssociationSectionPacketContexts(void);

  // gets transport stream program map section packet contexts
  // @return : transport stream program map section packet contexts
  CTransportStreamProgramMapSectionPacketContextCollection *GetTransportStreamProgramMapSectionPacketContexts(void);

  /* set methods */

  // sets request start position within protocol stream
  // @param requestStartPosition : request start position within protocol stream to set
  void SetRequestStartPosition(int64_t requestStartPosition);

  // sets ready for align flag
  // @param readyForAlign : true if ready for align, false otherwise
  // @param streamFragmentIndex : the index of stream fragment (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  void SetReadyForAlign(bool readyForAlign, unsigned int streamFragmentIndex);

  // sets aligned flag
  // @param aligned : true if aligned, false otherwise
  // @param streamFragmentIndex : the index of stream fragment (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  void SetAligned(bool aligned, unsigned int streamFragmentIndex);

  // sets discontinuity processed flag
  // @param discontinuityProcessed : true if discontinuity processed, false otherwise
  // @param streamFragmentIndex : the index of stream fragment (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  void SetDiscontinuityProcessed(bool discontinuityProcessed, unsigned int streamFragmentIndex);

  // sets program association section detection finished flag
  // @param programAssociationSectionDetectionFinished : true if program association section detection finished, false otherwise
  // @param streamFragmentIndex : the index of stream fragment (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  void SetProgramAssociationSectionDetectionFinished(bool programAssociationSectionDetectionFinished, unsigned int streamFragmentIndex);

  // sets program association section updated flag
  // @param programAssociationSectionUpdated : true if program association section updated, false otherwise
  // @param streamFragmentIndex : the index of stream fragment (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  void SetProgramAssociationSectionUpdated(bool programAssociationSectionUpdated, unsigned int streamFragmentIndex);

  // sets transport stream map section detection finished flag
  // @param transportStreamMapSectionDetectionFinished : true if transport stream map section detection finished, false otherwise
  // @param streamFragmentIndex : the index of stream fragment (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  void SetTransportStreamMapSectionDetectionFinished(bool transportStreamMapSectionDetectionFinished, unsigned int streamFragmentIndex);

  // sets transport stream map section updated flag
  // @param transportStreamMapSectionUpdated : true if transport stream map section updated, false otherwise
  // @param streamFragmentIndex : the index of stream fragment (used for updating indexes), UINT_MAX for ignoring update (but indexes MUST be updated later)
  void SetTransportStreamMapSectionUpdated(bool transportStreamMapSectionUpdated, unsigned int streamFragmentIndex);

  /* other methods */

  // tests if fragment has set request start position (in protocol stream)
  // @return : true if fragment has set request start position, false otherwise
  bool IsSetRequestStartPosition(void);

  // tests if fragment is ready for aligning
  // @return : true if fragment is ready for aligning, false otherwise
  bool IsReadyForAlign(void);

  // tests if fragment is aligned
  // @return : true if fragment is aligned, false otherwise
  bool IsAligned(void);

  // tests if stream fragment is at least in aligned state
  // @return : true if fragment is at least aligned, false otherwise
  bool IsAtLeastAligned(void);

  // tests if fragment is processed for discontinuity
  // @return : true if fragment is processed for discontinuity, false otherwise
  bool IsDiscontinuityProcessed(void);

  // tests if program association section detection finished
  // @return : true if program association section detection finished, false otherwise
  bool IsProgramAssociationSectionDetectionFinished(void);

  // tests if program association section updated
  // @return : true if program association section updated, false otherwise
  bool IsProgramAssociationSectionUpdated(void);

  // tests if transport stream map section detection finished
  // @return : true if transport stream map section detection finished, false otherwise
  bool IsTransportStreamMapSectionDetectionFinished(void);

  // tests if transport stream map section updated
  // @return : true if transport stream map section updated, false otherwise
  bool IsTransportStreamMapSectionUpdated(void);

protected:
  // holds request start position within protocol stream
  int64_t requestStartPosition;
  // holds program association section packet contexts
  CProgramAssociationSectionPacketContextCollection *programAssociationSectionPacketContexts;
  // holds transport stream program map section packet contexts
  CTransportStreamProgramMapSectionPacketContextCollection *transportStreamProgramMapSectionPacketContexts;

  /* methods */

  // gets new instance of cache file item
  // @return : new cache file item instance or NULL if error
  virtual CFastSearchItem *CreateItem(void);

  // deeply clones current instance
  // @param item : the cache file item instance to clone
  // @return : true if successful, false otherwise
  virtual bool InternalClone(CFastSearchItem *item);
};

#endif
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

#include "Mpeg2tsStreamFragment.h"
#include "FastSearchItemCollection.h"

CMpeg2tsStreamFragment::CMpeg2tsStreamFragment(HRESULT *result)
  : CStreamFragment(result)
{
  this->requestStartPosition = STREAM_FRAGMENT_START_POSITION_NOT_SET;
  this->programAssociationSectionPacketContexts = NULL;
  this->transportStreamProgramMapSectionPacketContexts = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->programAssociationSectionPacketContexts = new CProgramAssociationSectionPacketContextCollection(result);
    this->transportStreamProgramMapSectionPacketContexts = new CTransportStreamProgramMapSectionPacketContextCollection(result);

    CHECK_POINTER_HRESULT(*result, this->programAssociationSectionPacketContexts, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->transportStreamProgramMapSectionPacketContexts, *result, E_OUTOFMEMORY);
  }
}

CMpeg2tsStreamFragment::~CMpeg2tsStreamFragment(void)
{
  FREE_MEM_CLASS(this->programAssociationSectionPacketContexts);
  FREE_MEM_CLASS(this->transportStreamProgramMapSectionPacketContexts);
}

/* get methods */

int64_t CMpeg2tsStreamFragment::GetRequestStartPosition(void)
{
  return this->requestStartPosition;
}

CProgramAssociationSectionPacketContextCollection *CMpeg2tsStreamFragment::GetProgramAssociationSectionPacketContexts(void)
{
  return this->programAssociationSectionPacketContexts;
}

CTransportStreamProgramMapSectionPacketContextCollection *CMpeg2tsStreamFragment::GetTransportStreamProgramMapSectionPacketContexts(void)
{
  return this->transportStreamProgramMapSectionPacketContexts;
}

/* set methods */

void CMpeg2tsStreamFragment::SetRequestStartPosition(int64_t requestStartPosition)
{
  this->requestStartPosition = requestStartPosition;
}

void CMpeg2tsStreamFragment::SetReadyForAlign(bool readyForAlign, unsigned int streamFragmentIndex)
{
  this->flags &= ~MPEG2TS_STREAM_FRAGMENT_FLAG_READY_FOR_ALIGN;
  this->flags |= (readyForAlign) ? MPEG2TS_STREAM_FRAGMENT_FLAG_READY_FOR_ALIGN : MPEG2TS_STREAM_FRAGMENT_FLAG_NONE;

  if ((this->owner != NULL) && (streamFragmentIndex != UINT_MAX))
  {
    this->owner->UpdateIndexes(streamFragmentIndex);
  }
}

void CMpeg2tsStreamFragment::SetAligned(bool aligned, unsigned int streamFragmentIndex)
{
  this->flags &= ~MPEG2TS_STREAM_FRAGMENT_FLAG_ALIGNED;
  this->flags |= (aligned) ? MPEG2TS_STREAM_FRAGMENT_FLAG_ALIGNED : MPEG2TS_STREAM_FRAGMENT_FLAG_NONE;

  if ((this->owner != NULL) && (streamFragmentIndex != UINT_MAX))
  {
    this->owner->UpdateIndexes(streamFragmentIndex);
  }
}

void CMpeg2tsStreamFragment::SetDiscontinuityProcessed(bool discontinuityProcessed, unsigned int streamFragmentIndex)
{
  this->flags &= ~MPEG2TS_STREAM_FRAGMENT_FLAG_DISCONTINUITY_PROCESSED;
  this->flags |= (discontinuityProcessed) ? MPEG2TS_STREAM_FRAGMENT_FLAG_DISCONTINUITY_PROCESSED : MPEG2TS_STREAM_FRAGMENT_FLAG_NONE;

  if ((this->owner != NULL) && (streamFragmentIndex != UINT_MAX))
  {
    this->owner->UpdateIndexes(streamFragmentIndex);
  }
}

void CMpeg2tsStreamFragment::SetProgramAssociationSectionDetectionFinished(bool programAssociationSectionDetectionFinished, unsigned int streamFragmentIndex)
{
  this->flags &= ~MPEG2TS_STREAM_FRAGMENT_FLAG_PROGRAM_ASSOCIATION_SECTION_DETECTION_FINISHED;
  this->flags |= (programAssociationSectionDetectionFinished) ? MPEG2TS_STREAM_FRAGMENT_FLAG_PROGRAM_ASSOCIATION_SECTION_DETECTION_FINISHED : MPEG2TS_STREAM_FRAGMENT_FLAG_NONE;

  if ((this->owner != NULL) && (streamFragmentIndex != UINT_MAX))
  {
    this->owner->UpdateIndexes(streamFragmentIndex);
  }
}

void CMpeg2tsStreamFragment::SetProgramAssociationSectionUpdated(bool programAssociationSectionUpdated, unsigned int streamFragmentIndex)
{
  this->flags &= ~MPEG2TS_STREAM_FRAGMENT_FLAG_PROGRAM_ASSOCIATION_SECTION_UPDATED;
  this->flags |= (programAssociationSectionUpdated) ? MPEG2TS_STREAM_FRAGMENT_FLAG_PROGRAM_ASSOCIATION_SECTION_UPDATED : MPEG2TS_STREAM_FRAGMENT_FLAG_NONE;

  if ((this->owner != NULL) && (streamFragmentIndex != UINT_MAX))
  {
    this->owner->UpdateIndexes(streamFragmentIndex);
  }
}

void CMpeg2tsStreamFragment::SetTransportStreamMapSectionDetectionFinished(bool transportStreamMapSectionDetectionFinished, unsigned int streamFragmentIndex)
{
  this->flags &= ~MPEG2TS_STREAM_FRAGMENT_FLAG_TRANSPORT_STREAM_PROGRAM_MAP_SECTION_DETECTION_FINISHED;
  this->flags |= (transportStreamMapSectionDetectionFinished) ? MPEG2TS_STREAM_FRAGMENT_FLAG_TRANSPORT_STREAM_PROGRAM_MAP_SECTION_DETECTION_FINISHED : MPEG2TS_STREAM_FRAGMENT_FLAG_NONE;

  if ((this->owner != NULL) && (streamFragmentIndex != UINT_MAX))
  {
    this->owner->UpdateIndexes(streamFragmentIndex);
  }
}

void CMpeg2tsStreamFragment::SetTransportStreamMapSectionUpdated(bool transportStreamMapSectionUpdated, unsigned int streamFragmentIndex)
{
  this->flags &= ~MPEG2TS_STREAM_FRAGMENT_FLAG_TRANSPORT_STREAM_PROGRAM_MAP_SECTION_UPDATED;
  this->flags |= (transportStreamMapSectionUpdated) ? MPEG2TS_STREAM_FRAGMENT_FLAG_TRANSPORT_STREAM_PROGRAM_MAP_SECTION_UPDATED : MPEG2TS_STREAM_FRAGMENT_FLAG_NONE;

  if ((this->owner != NULL) && (streamFragmentIndex != UINT_MAX))
  {
    this->owner->UpdateIndexes(streamFragmentIndex);
  }
}

/* other methods */

bool CMpeg2tsStreamFragment::IsSetRequestStartPosition(void)
{
  return (this->requestStartPosition != STREAM_FRAGMENT_START_POSITION_NOT_SET);
}

bool CMpeg2tsStreamFragment::IsReadyForAlign(void)
{
  return this->IsSetFlags(MPEG2TS_STREAM_FRAGMENT_FLAG_READY_FOR_ALIGN);
}

bool CMpeg2tsStreamFragment::IsAligned(void)
{
  return this->IsSetFlags(MPEG2TS_STREAM_FRAGMENT_FLAG_ALIGNED);
}

bool CMpeg2tsStreamFragment::IsAtLeastAligned(void)
{
  return this->IsSetAnyOfFlags(
    MPEG2TS_STREAM_FRAGMENT_FLAG_ALIGNED | 
    MPEG2TS_STREAM_FRAGMENT_FLAG_DISCONTINUITY_PROCESSED | 
    MPEG2TS_STREAM_FRAGMENT_FLAG_PROGRAM_ASSOCIATION_SECTION_DETECTION_FINISHED | 
    MPEG2TS_STREAM_FRAGMENT_FLAG_PROGRAM_ASSOCIATION_SECTION_UPDATED |
    MPEG2TS_STREAM_FRAGMENT_FLAG_TRANSPORT_STREAM_PROGRAM_MAP_SECTION_DETECTION_FINISHED |
    MPEG2TS_STREAM_FRAGMENT_FLAG_TRANSPORT_STREAM_PROGRAM_MAP_SECTION_UPDATED |
    STREAM_FRAGMENT_FLAG_PROCESSED);
}

bool CMpeg2tsStreamFragment::IsDiscontinuityProcessed(void)
{
  return this->IsSetFlags(MPEG2TS_STREAM_FRAGMENT_FLAG_DISCONTINUITY_PROCESSED);
}

bool CMpeg2tsStreamFragment::IsProgramAssociationSectionDetectionFinished(void)
{
  return this->IsSetFlags(MPEG2TS_STREAM_FRAGMENT_FLAG_PROGRAM_ASSOCIATION_SECTION_DETECTION_FINISHED);
}

bool CMpeg2tsStreamFragment::IsProgramAssociationSectionUpdated(void)
{
  return this->IsSetFlags(MPEG2TS_STREAM_FRAGMENT_FLAG_PROGRAM_ASSOCIATION_SECTION_UPDATED);
}

bool CMpeg2tsStreamFragment::IsTransportStreamMapSectionDetectionFinished(void)
{
  return this->IsSetFlags(MPEG2TS_STREAM_FRAGMENT_FLAG_TRANSPORT_STREAM_PROGRAM_MAP_SECTION_DETECTION_FINISHED);
}

bool CMpeg2tsStreamFragment::IsTransportStreamMapSectionUpdated(void)
{
  return this->IsSetFlags(MPEG2TS_STREAM_FRAGMENT_FLAG_TRANSPORT_STREAM_PROGRAM_MAP_SECTION_UPDATED);
}

/* protected methods */

CFastSearchItem *CMpeg2tsStreamFragment::CreateItem(void)
{
  HRESULT result = S_OK;
  CMpeg2tsStreamFragment *item = new CMpeg2tsStreamFragment(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  return item;
}

bool CMpeg2tsStreamFragment::InternalClone(CFastSearchItem *item)
{
  bool result = __super::InternalClone(item);
  
  if (result)
  {
    CMpeg2tsStreamFragment *fragment = dynamic_cast<CMpeg2tsStreamFragment *>(item);
    result &= (fragment != NULL);

    if (result)
    {
      fragment->requestStartPosition = this->requestStartPosition;
      
      // do not clone program association section packet contexts
      // do not clone transport stream program map section packet contexts
    }
  }

  return result;
}
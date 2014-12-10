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

#ifndef __DEFAULT_DEMUXER_DEFINED
#define __DEFAULT_DEMUXER_DEFINED

#include "Demuxer.h"

#define DEFAULT_DEMUXER_FLAG_NONE                                     DEMUXER_FLAG_NONE

#define DEFAULT_DEMUXER_FLAG_LAST                                     (DEMUXER_FLAG_LAST + 0)

class CDefaultDemuxer : public CDemuxer
{
public:
  CDefaultDemuxer(HRESULT *result, CLogger *logger, IDemuxerOwner *filter, CParameterCollection *configuration);
  virtual ~CDefaultDemuxer(void);

  /* CDemuxer methods */

  // gets duration for stream
  virtual int64_t GetDuration(void);

  // gets position for specified stream time (in ms)
  // @param streamTime : the stream time (in ms) to get position in stream
  // @return : the position in stream
  virtual uint64_t GetPositionForStreamTime(uint64_t streamTime);

  /* get methods */

  /* set methods */

  /* other methods */

protected:

  /* methods */

  // creates demuxer
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT CreateDemuxerInternal(void);

  // cleans up demuxer
  virtual void CleanupDemuxerInternal(void);

  // demuxing worker internal method executed from DemuxingWorker() method
  virtual void DemuxingWorkerInternal(void);

  // gets next output pin packet
  // @param packet : pointer to output packet
  // @return : S_OK if successful, S_FALSE if no output pin packet available, error code otherwise
  virtual HRESULT GetNextPacketInternal(COutputPinPacket *packet);
};

#endif
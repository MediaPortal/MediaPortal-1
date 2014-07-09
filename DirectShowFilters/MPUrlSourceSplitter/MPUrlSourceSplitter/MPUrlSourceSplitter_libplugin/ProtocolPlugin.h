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

#ifndef __PROTOCOL_PLUGIN_DEFINED
#define __PROTOCOL_PLUGIN_DEFINED

#pragma warning(push)
// disable warning: 'INT8_MIN' : macro redefinition
// warning is caused by stdint.h and intsafe.h, which both define same macro
#pragma warning(disable:4005)

#include "Plugin.h"
#include "IProtocol.h"

#pragma warning(pop)

#define PROTOCOL_PLUGIN_FLAG_NONE                                     PLUGIN_FLAG_NONE

#define PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_SPECIFIED                    (1 << (PLUGIN_FLAG_LAST + 0))
#define PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_DETECTED                     (1 << (PLUGIN_FLAG_LAST + 1))
#define PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH                        (1 << (PLUGIN_FLAG_LAST + 2))
#define PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED                  (1 << (PLUGIN_FLAG_LAST + 3))
#define PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED                  (1 << (PLUGIN_FLAG_LAST + 4))
#define PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED                    (1 << (PLUGIN_FLAG_LAST + 5))
#define PROTOCOL_PLUGIN_FLAG_CONNECTION_LOST_CANNOT_REOPEN            (1 << (PLUGIN_FLAG_LAST + 6))

#define PROTOCOL_PLUGIN_FLAG_LAST                                     (PLUGIN_FLAG_LAST + 7)

class CProtocolPlugin : public CPlugin, public IProtocol
{
public:
  CProtocolPlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  virtual ~CProtocolPlugin(void);

  // IProtocol interface

  // ISimpleProtocol interface

  // clear current session
  // @return : S_OK if successfull
  virtual HRESULT ClearSession(void);

  // reports actual stream time to protocol
  // @param streamTime : the actual stream time in ms to report to protocol
  // @param streamPosition : the actual stream position (related to stream time) to report to protocol
  virtual void ReportStreamTime(uint64_t streamTime, uint64_t streamPosition);

  // ISeeking interface

  // set pause, seek or stop mode
  // in such mode are reading operations disabled
  // @param pauseSeekStopMode : one of PAUSE_SEEK_STOP_MODE values
  virtual void SetPauseSeekStopMode(unsigned int pauseSeekStopMode);

  /* get methods */

  /* set methods */

  /* other methods */

  // tests if stream is specified as live stream by configuration
  // @return : true if stream is specified as live stream, false otherwise
  virtual bool IsLiveStreamSpecified(void);

  // tests if stream is detected as live stream
  // @return : true if stream is detected as live stream
  virtual bool IsLiveStreamDetected(void);

  // tests if stream is specified or detected as live stream
  // @return : true if stream is specified or detected as live stream
  virtual bool IsLiveStream(void);

  // tests if stream length was set
  // @return : true if stream length was set, false otherwise
  virtual bool IsSetStreamLength(void);

  // tests if stream length is estimated
  // @return : true if stream length is estimated, false otherwise
  virtual bool IsStreamLengthEstimated(void);

  // tests if whole stream is downloaded (no gaps)
  // @return : true if whole stream is downloaded
  virtual bool IsWholeStreamDownloaded(void);

  // tests if end of stream is reached (but it can be with gaps)
  // @return : true if end of stream reached, false otherwise
  virtual bool IsEndOfStreamReached(void);

  // tests if connection was lost and can't be opened again
  // @return : true if connection was lost and can't be opened again, false otherwise
  virtual bool IsConnectionLostCannotReopen(void);

protected:
  // holds logger instance
  CLogger *logger;
  // holds configuration
  CParameterCollection *configuration;
  // holds reported stream time and position
  uint64_t reportedStreamTime;
  uint64_t reportedStreamPosition;
  // holds pause, seek or stop mode
  volatile unsigned int pauseSeekStopMode;

  /* methods */
};

#endif
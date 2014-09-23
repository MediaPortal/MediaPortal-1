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

#ifndef __MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_DEFINED

#include "ParserPlugin.h"
#include "CacheFile.h"
#include "Mpeg2tsStreamFragmentCollection.h"
#include "DiscontinuityParser.h"

#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_NONE               PARSER_PLUGIN_FLAG_NONE

#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_RECEIVE_DATA                     (1 << (PARSER_PLUGIN_FLAG_LAST + 0))

#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_DETECT_DISCONTINUITY             (1 << (PARSER_PLUGIN_FLAG_LAST + 1))
#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_ALIGN_TO_MPEG2TS_PACKET          (1 << (PARSER_PLUGIN_FLAG_LAST + 2))

#define MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_FLAG_LAST               (PARSER_PLUGIN_FLAG_LAST + 3)

#define PARSER_NAME                                                   L"PARSER_MPEG2TS"

class CMPUrlSourceSplitter_Parser_Mpeg2TS : public CParserPlugin
{
public:
  // constructor
  // create instance of CMPUrlSourceSplitter_Parser_Mpeg2TS class
  CMPUrlSourceSplitter_Parser_Mpeg2TS(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  // destructor
  virtual ~CMPUrlSourceSplitter_Parser_Mpeg2TS(void);

  // CParserPlugin

  // gets parser result about current stream
  // @return : one of ParserResult values
  virtual HRESULT GetParserResult(void);

  // gets parser score if parser result is Known
  // @return : parser score (parser with highest score is set as active parser)
  virtual unsigned int GetParserScore(void);

  // gets parser action after parser recognizes stream
  // @return : one of Action values
  virtual Action GetAction(void);

  // sets current connection url and parameters
  // @param parameters : the collection of url and connection parameters
  // @return : S_OK if successful
  virtual HRESULT SetConnectionParameters(const CParameterCollection *parameters);

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

  // CPlugin

  // return reference to null-terminated string which represents plugin name
  // errors should be logged to log file and returned NULL
  // @return : reference to null-terminated string
  virtual const wchar_t *GetName(void);

  // initialize plugin implementation with configuration parameters
  // @param configuration : the reference to additional configuration parameters (created by plugin's hoster class)
  // @return : S_OK if successfull, error code otherwise
  virtual HRESULT Initialize(CPluginConfiguration *configuration);

  // ISeeking interface

  // request protocol implementation to receive data from specified time (in ms) for specified stream
  // this method is called with same time for each stream in protocols with multiple streams
  // @param streamId : the stream ID to receive data from specified time
  // @param time : the requested time (zero is start of stream)
  // @return : time (in ms) where seek finished or lower than zero if error
  virtual int64_t SeekToTime(unsigned int streamId, int64_t time);

  // set pause, seek or stop mode
  // in such mode are reading operations disabled
  // @param pauseSeekStopMode : one of PAUSE_SEEK_STOP_MODE values
  virtual void SetPauseSeekStopMode(unsigned int pauseSeekStopMode);

  // IDemuxerOwner interface

  // process stream package request
  // @param streamPackage : the stream package request to process
  // @return : S_OK if successful, error code only in case when error is not related to processing request
  virtual HRESULT ProcessStreamPackage(CStreamPackage *streamPackage);

  // retrieves the progress of the stream reading operation
  // @param streamProgress : reference to instance of class that receives the stream progress
  // @return : S_OK if successful, VFW_S_ESTIMATED if returned values are estimates, E_INVALIDARG if stream ID is unknown, E_UNEXPECTED if unexpected error
  virtual HRESULT QueryStreamProgress(CStreamProgress *streamProgress);

  // ISimpleProtocol interface

  // starts receiving data from specified url and configuration parameters
  // @param parameters : the url and parameters used for connection
  // @return : S_OK if url is loaded, false otherwise
  virtual HRESULT StartReceivingData(CParameterCollection *parameters);

  // request protocol implementation to cancel the stream reading operation
  // @return : S_OK if successful
  virtual HRESULT StopReceivingData(void);
  
  // clears current session
  virtual void ClearSession(void);

  // IProtocol interface

protected:
  // holds last received length of data when requesting parser result
  unsigned int lastReceivedLength;
  // mutex for locking access to file, buffer, ...
  HANDLE mutex;

  // holds stream fragments
  CMpeg2tsStreamFragmentCollection *streamFragments;
  // holds last store time to cache file
  unsigned int lastStoreTime;
  // holds cache file
  CCacheFile *cacheFile;
  // holds last processed size from last store time
  unsigned int lastProcessedSize;
  unsigned int currentProcessedSize;
  // holds which fragment is currently downloading (UINT_MAX means none)
  unsigned int streamFragmentDownloading;
  // holds which fragment have to be downloaded
  // (UINT_MAX means next fragment, always reset after started download of fragment)
  unsigned int streamFragmentToDownload;

  // the length of stream
  int64_t streamLength;

  // holds stream package for processing (only reference, not deep clone)
  CStreamPackage *streamPackage;

  // holds pause, seek or stop mode
  volatile unsigned int pauseSeekStopMode;

  // holds position offset added to stream length 
  int64_t positionOffset;

  // holds discontinuity parser
  CDiscontinuityParser *discontinuityParser;

  /* received data worker */

  HANDLE receiveDataWorkerThread;
  volatile bool receiveDataWorkerShouldExit;

  /* methods */

  // gets store file name
  // @param extension : the extension of store file
  // @return : store file name or NULL if error
  wchar_t *GetStoreFile(const wchar_t *extension);

  // gets byte position in buffer
  // it is always reset on seek
  // @return : byte position in buffer
  int64_t GetBytePosition(void);

  /* receive data worker */

  // creates receive data worker
  // @return : S_OK if successful
  HRESULT CreateReceiveDataWorker(void);

  // destroys receive data worker
  // @return : S_OK if successful
  HRESULT DestroyReceiveDataWorker(void);

  static unsigned int WINAPI ReceiveDataWorker(LPVOID lpParam);
};

#endif

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

#ifndef __MP_URL_SOURCE_SPLITTER_DEFINED
#define __MP_URL_SOURCE_SPLITTER_DEFINED

#include "IDownload.h"
#include "IOutputPinFilter.h"
#include "IFilterState.h"
#include "MPUrlSourceSplitterOutputPinCollection.h"
#include "ParserHoster.h"
#include "MediaPacketCollection.h"
#include "AsyncRequest.h"
#include "DemuxerCollection.h"

const GUID GUID_MP_IPTV_SOURCE            = { 0xD3DD4C59, 0xD3A7, 0x4B82, 0x97, 0x27, 0x7B, 0x92, 0x03, 0xEB, 0x67, 0xC0 };
const GUID GUID_MP_URL_SOURCE_SPLITTER    = { 0x59ED045A, 0xA938, 0x4A09, 0xA8, 0xA6, 0x82, 0x31, 0xF5, 0x83, 0x42, 0x59 };

#define MP_IPTV_SOURCE_GLOBAL_MUTEX_NAME                              L"Global\\MPIptvSource"
#define MP_URL_SOURCE_SPLITTER_GLOBAL_MUTEX_NAME                      L"Global\\MPUrlSourceSplitter"

#define MP_IPTV_SOURCE_LOG_FILE                                       L"log\\MPIPTVSource.log"
#define MP_IPTV_SOURCE_LOG_BACKUP_FILE                                L"log\\MPIPTVSource.bak"

#define MP_URL_SOURCE_SPLITTER_LOG_FILE                               L"log\\MPUrlSourceSplitter.log"
#define MP_URL_SOURCE_SPLITTER_LOG_BACKUP_FILE                        L"log\\MPUrlSourceSplitter.bak"

#define MP_URL_SOURCE_SPLITTER_FLAG_NONE                              0x00000000
#define MP_URL_SOURCE_SPLITTER_FLAG_AS_IPTV                           0x00000001
#define MP_URL_SOURCE_SPLITTER_FLAG_AS_SPLITTER                       0x00000002
// specifies if we are downloading file, in that case we don't delete file on end
#define MP_URL_SOURCE_SPLITTER_FLAG_DOWNLOADING_FILE                  0x00000004
// specifies if filter is processing live stream or not
#define MP_URL_SOURCE_SPLITTER_FLAG_LIVE_STREAM                       0x00000008
// specifies if filter finished asynchronous downloading
#define MP_URL_SOURCE_SPLITTER_FLAG_ASYNC_DOWNLOAD_FINISHED           0x00000010
// specifies if filter called download callback
#define MP_URL_SOURCE_SPLITTER_FLAG_DOWNLOAD_CALLBACK_CALLED          0x00000020
// specifies that filter is in Enable() method - this method is for changing streams and there are called method which affects receiving data (Stop(), Run())
#define MP_URL_SOURCE_SPLITTER_FLAG_ENABLED_METHOD_ACTIVE             0x00000040
// specifies that playback started and is not stopped
#define MP_URL_SOURCE_SPLITTER_FLAG_PLAYBACK_STARTED                  0x00000080
// specifies that filter can report stream time to protocol
#define MP_URL_SOURCE_SPLITTER_FLAG_REPORT_STREAM_TIME                0x00000100

class CMPUrlSourceSplitter 
  : public CBaseFilter
  , public CCritSec
  , protected CAMThread
  , public IFileSourceFilter
  //, public IParserOutputStream
  , public IMediaSeeking
  , public IAMStreamSelect
  //, public ILAVFSettingsInternal
  //, public IObjectWithSite
  //, public IBufferInfo
  , public IDownload
  , public IDownloadCallback
  , public IOutputPinFilter
  , public IFilterState
{
public:
  ~CMPUrlSourceSplitter();

  static CUnknown * WINAPI CreateInstanceIptvSource(LPUNKNOWN lpunk, HRESULT* phr);
  static CUnknown * WINAPI CreateInstanceUrlSourceSplitter(LPUNKNOWN lpunk, HRESULT* phr);

  // IUnknown interface
  DECLARE_IUNKNOWN;
  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);

  // CBaseFilter methods

  // retrieves the number of pins
  // @return : the actual number of pins
  int GetPinCount(void);

  // retrieves a pin
  // @param n : the zero-based index of the pin
  // @return : pointer to the nth pin on this filter or NULL if not exists
  CBasePin *GetPin(int n);

  // retrieves the class identifier
  // @param pClsID : pointer to a variable that receives the class identifier
  // @return : S_OK if successful, E_POINTER if NULL argument specified
  STDMETHODIMP GetClassID(CLSID* pClsID);

  // retrieves the filter's state (running, stopped, or paused)
  // @param dwMSecs : time-out interval, in milliseconds
  // @param State : pointer to a variable that receives a member of the FILTER_STATE enumerated type, indicating the filter's state
  // @return : 
  // S_OK if successful
  // E_POINTER if NULL argument specified
  // VFW_S_CANT_CUE if paused and can't deliver data
  // error code otherwise
  STDMETHODIMP GetState(DWORD dwMSecs, __out FILTER_STATE *State);

  // stops the filter
  // @return : S_OK if successful, error code otherwise
  STDMETHODIMP Stop();

  // pauses the filter
  // @return : S_OK if successful, error code otherwise
  STDMETHODIMP Pause();

  // runs the filter
  // @param tStart : reference time corresponding to stream time 0
  // @return : S_OK if successful, error code otherwise
  STDMETHODIMP Run(REFERENCE_TIME tStart);

  // IFileSourceFilter
  
  // loads the source filter with the stream
  // @param pszFileName : pointer to the URI of stream to open
  // @param pmt : pointer to the media type of the stream, this can be NULL
  // @return : S_OK if successful, one of error codes from ErrorCodes.h or error code otherwise
  STDMETHODIMP Load(LPCOLESTR pszFileName, const AM_MEDIA_TYPE * pmt);

  // retrieves the current stream
  // @param ppszFileName : address of a pointer that receives the URI of stream, as an OLESTR type
  // @param pmt : pointer to an AM_MEDIA_TYPE structure that receives the media type, this parameter can by NULL, in which case the method does not return the media type
  // @return :
  // S_OK if successful
  // E_FAIL if no stream opened
  // E_POINTER if ppszFileName is NULL
  STDMETHODIMP GetCurFile(LPOLESTR *ppszFileName, AM_MEDIA_TYPE *pmt);

  // IOutputStream interface

  // notifies output stream about stream count
  // @param streamCount : the stream count
  // @param liveStream : true if stream(s) are live, false otherwise
  // @return : S_OK if successful, false otherwise
  HRESULT SetStreamCount(unsigned int streamCount, bool liveStream);

  // pushes stream received data to filter
  // @param streamId : the stream ID to push stream received data
  // @param streamReceivedData : the stream received data to push to filter
  // @return : S_OK if successful, error code otherwise
  HRESULT PushStreamReceiveData(unsigned int streamId, CStreamReceiveData *streamReceiveData);

  // IParserOutputStream interface

  // tests if filter is downloading
  // @return : true if downloading, false otherwise
  bool IsDownloading(void);

  // finishes download with specified result
  // @param result : the result of download
  void FinishDownload(HRESULT result);

  // IMediaSeeking
  STDMETHODIMP GetCapabilities(DWORD* pCapabilities);
  STDMETHODIMP CheckCapabilities(DWORD* pCapabilities);
  STDMETHODIMP IsFormatSupported(const GUID* pFormat);
  STDMETHODIMP QueryPreferredFormat(GUID* pFormat);
  STDMETHODIMP GetTimeFormat(GUID* pFormat);
  STDMETHODIMP IsUsingTimeFormat(const GUID* pFormat);
  STDMETHODIMP SetTimeFormat(const GUID* pFormat);
  STDMETHODIMP GetDuration(LONGLONG* pDuration);
  STDMETHODIMP GetStopPosition(LONGLONG* pStop);
  STDMETHODIMP GetCurrentPosition(LONGLONG* pCurrent);
  STDMETHODIMP ConvertTimeFormat(LONGLONG* pTarget, const GUID* pTargetFormat, LONGLONG Source, const GUID* pSourceFormat);
  STDMETHODIMP SetPositions(LONGLONG* pCurrent, DWORD dwCurrentFlags, LONGLONG* pStop, DWORD dwStopFlags);
  STDMETHODIMP GetPositions(LONGLONG* pCurrent, LONGLONG* pStop);
  STDMETHODIMP GetAvailable(LONGLONG* pEarliest, LONGLONG* pLatest);
  STDMETHODIMP SetRate(double dRate);
  STDMETHODIMP GetRate(double* pdRate);
  STDMETHODIMP GetPreroll(LONGLONG* pllPreroll);

  // IAMStreamSelect
  STDMETHODIMP Count(DWORD *pcStreams);
  STDMETHODIMP Enable(long lIndex, DWORD dwFlags);
  STDMETHODIMP Info(long lIndex, AM_MEDIA_TYPE **ppmt, DWORD *pdwFlags, LCID *plcid, DWORD *pdwGroup, WCHAR **ppszName, IUnknown **ppObject, IUnknown **ppUnk);

  // IAMOpenProgress interface
  STDMETHODIMP QueryProgress(LONGLONG *pllTotal, LONGLONG *pllCurrent);
  STDMETHODIMP AbortOperation(void);

  // IDownload interface
  STDMETHODIMP Download(LPCOLESTR uri, LPCOLESTR fileName);
  STDMETHODIMP DownloadAsync(LPCOLESTR uri, LPCOLESTR fileName, IDownloadCallback *downloadCallback);

  // IDownloadCallback interface
  void STDMETHODCALLTYPE OnDownloadCallback(HRESULT downloadResult);

  // IOutputPinFilter interface

  // gets filter parameters
  // @return : filter parameters or NULL if error
  CParameterCollection *GetConfiguration(void);

  // IFilter interface

  // gets duration of stream in ms
  // @return : stream duration in ms or DURATION_LIVE_STREAM in case of live stream or DURATION_UNSPECIFIED if duration is unknown
  int64_t GetDuration(void);

  // get timeout (in ms) for receiving data
  // @return : timeout (in ms) for receiving data
  unsigned int GetReceiveDataTimeout(void);

  // retrieves the progress of the stream reading operation
  // @param streamProgress : reference to instance of class that receives the stream progress
  // @return : S_OK if successful, VFW_S_ESTIMATED if returned values are estimates, E_INVALIDARG if stream ID is unknown, E_UNEXPECTED if unexpected error
  HRESULT QueryStreamProgress(CStreamProgress *streamProgress);
  
  // retrieves available lenght of stream
  // @param available : reference to instance of class that receives the available length of stream, in bytes
  // @return : S_OK if successful, other error codes if error
  HRESULT QueryStreamAvailableLength(CStreamAvailableLength *availableLength);

  // ISeeking interface

  // gets seeking capabilities of protocol
  // @return : bitwise combination of SEEKING_METHOD flags
  unsigned int GetSeekingCapabilities(void);

  // request protocol implementation to receive data from specified time (in ms) for specified stream
  // this method is called with same time for each stream in protocols with multiple streams
  // @param streamId : the stream ID to receive data from specified time
  // @param time : the requested time (zero is start of stream)
  // @return : time (in ms) where seek finished or lower than zero if error
  int64_t SeekToTime(unsigned int streamId, int64_t time);

  // request protocol implementation to receive data from specified position to specified position
  // @param start : the requested start position (zero is start of stream)
  // @param end : the requested end position, if end position is lower or equal to start position than end position is not specified
  // @return : position where seek finished or lower than zero if error
  int64_t SeekToPosition(int64_t start, int64_t end);

  // sets if protocol implementation have to supress sending data with specified stream ID to filter
  // @param streamId : the stream ID to supress data
  // @param supressData : true if protocol have to supress sending data to filter, false otherwise
  void SetSupressData(unsigned int streamId, bool supressData);

  // IFilterState interface

  // tests if filter is ready to connect output pins
  // @param ready : reference to variable that holds ready state
  // @return : S_OK if successful
  STDMETHODIMP IsFilterReadyToConnectPins(bool *ready);

  // get cache file name
  // @param path : reference to string which will hold path to cache file name
  // @return : S_OK if successful (*path can be NULL), E_POINTER if path is NULL
  STDMETHODIMP GetCacheFileName(wchar_t **path);

  enum { CMD_EXIT, CMD_SEEK, CMD_PAUSE, CMD_PLAY };

protected:
  // initializes a new instance of CMPUrlSourceSplitter class
  // this method is called with specific parameters from CreateInstanceIptvSource() method or from CreateInstanceUrlSourceSplitter()
  CMPUrlSourceSplitter(LPCSTR pName, LPUNKNOWN pUnk, const IID &clsid, HRESULT *phr);

  // holds logger instance
  CLogger *logger;

  // holds output pins (one for MP IPTV Source, one or more for MP Url Source Splitter)
  CMPUrlSourceSplitterOutputPinCollection *outputPins;

  // holds last command sent to filter (one of CMD_ values)
  int lastCommand;

  // holds if filter want to call CAMThread::CallWorker() with CMD_PAUSE, CMD_SEEK, CMD_STOP values
  volatile bool pauseSeekStopRequest;

  // holds various flags
  unsigned int flags;

  // holds mutex for access to demuxers collection
  HANDLE demuxersMutex;
  // holds collection of demuxers
  CDemuxerCollection *demuxers;

  // holds asynchronous download result and callback
  wchar_t* downloadFileName;
  HRESULT asyncDownloadResult;
  IDownloadCallback *asyncDownloadCallback;

  // configuration provided by filter
  CParameterCollection *configuration;

  // holds parsers (parser hoster hosts protocol hoster)
  CParserHoster *parserHoster;

  // holds create all demuxer thread handle
  HANDLE createAllDemuxersWorkerThread;
  // specifies if all demuxer worker should exit
  volatile bool createAllDemuxersWorkerShouldExit;

  // demuxer times
  REFERENCE_TIME demuxStart, demuxNewStart;

  /* methods */

  // CAMThread
  DWORD ThreadProc();

  void DeliverBeginFlush();
  void DeliverEndFlush();

  // tests if filter works as IPTV filter
  // @return : true if works as IPTV filter, false otherwise
  bool IsIptv(void);

  // tests if filter works as splitter
  // @return : true if works as splitter, false otherwise
  bool IsSplitter(void);

  // tests if various combination of flags is set
  // @return : true if combination of flags is set, false otherwise
  bool IsSetFlag(unsigned int flags);

  // sets combination of flags
  // @param flags : the combination of flags to set
  void SetFlags(unsigned int flags);

  // tests if filter is downloading file, in that case we don't delete file on end
  // @return : true if filter is downloading file, false otherwise
  bool IsDownloadingFile(void);

  // tests if filter is processing live stream
  // @return : true if filter is processing live stream, false otherwise
  bool IsLiveStream(void);

  // tests if filter finished asynchronous download
  // @return : true if filter finished asynchronous download, false otherwise
  bool IsAsyncDownloadFinished(void);

  // tests if filter called download callback
  // @return : true if filter called download callback, false otherwise
  bool IsDownloadCallbackCalled(void);

  // tests if FLAG_MP_URL_SOURCE_SPLITTER_ENABLED_METHOD_ACTIVE flag is set
  // @return : true if flag is set, false otherwise
  bool IsEnabledMethodActive(void);

  // tests if FLAG_MP_URL_SOURCE_SPLITTER_PLAYBACK_STARTED flag is set
  // @return : true if flag is set, false otherwise
  bool IsPlaybackStarted(void);

  // tests if FLAG_MP_URL_SOURCE_SPLITTER_REPORT_STREAM_TIME flag is set
  // @return : true if flag is set, false otherwise
  bool CanReportStreamTime(void);

  // gets parser hoster status
  // @return : one of STATUS_* values or error code if error
  HRESULT GetParserHosterStatus(void);

  // gets next output pin packet
  // @param packet : pointer to output packet
  // @param demuxerId : the demuxer ID to get packet
  // @return : S_OK if successful, S_FALSE if no output pin packet available, error code otherwise
  HRESULT GetNextPacket(COutputPinPacket *packet, unsigned int demuxerId);

  // FFmpeg log callback
  static void FFmpegLogCallback(void *ptr, int log_level, const char *format, va_list vl);

  // initializes input pin and loads url based on configuration
  // @result : S_OK if successful
  STDMETHODIMP Load();

  // parses parameters from specified string
  // @param parameters : null-terminated string with specified parameters
  // @return : reference to variable holding collection of parameters or NULL if error
  CParameterCollection *ParseParameters(const wchar_t *parameters);

  /* create all demuxers worker methods */

  // create all demuxer worker thread method
  static unsigned int WINAPI CreateAllDemuxersWorker(LPVOID lpParam);

  // creates create all demuxers worker
  // @return : S_OK if successful
  HRESULT CreateCreateAllDemuxersWorker(void);

  // destroys create all demuxers worker
  // @return : S_OK if successful
  HRESULT DestroyCreateAllDemuxersWorker(void);

  // set pause, seek or stop request flag (in filter and also in all demuxers)
  void SetPauseSeekStopRequest(bool pauseSeekStopRequest);

  // clears current session, initializes filter to state after creating instance
  // @return : S_OK if successfull
  void ClearSession(void);
};

#endif


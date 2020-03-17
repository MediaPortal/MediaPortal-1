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

#ifndef __MP_URL_SOURCE_SPLITTER_INPUT_PIN_DEFINED
#define __MP_URL_SOURCE_SPLITTER_INPUT_PIN_DEFINED

#include "Logger.h"
#include "IProtocol.h"
#include "IDownload.h"
#include "IFilter.h"
#include "IParserOutputStream.h"

#include "AsyncRequest.h"
#include "MediaPacketCollection.h"
#include "StreamAvailableLength.h"
#include "ParserHoster.h"
#include "OutputPinPacket.h"

//#include <set>

class CMPUrlSourceSplitter;
class CAsyncRequest;
class CMediaPacketCollection;

class CMPUrlSourceSplitterInputPin
  : public CUnknown
  , public CCritSec
  , public IFileSourceFilter
  , public IParserOutputStream
  , public IDownload
  , public IDownloadCallback
  , public IFilter
  , public IMediaSeeking
{
public:
  CMPUrlSourceSplitterInputPin(CLogger *logger, wchar_t *pName, CMPUrlSourceSplitter *pFilter, CCritSec* pLock, HRESULT *phr);
  ~CMPUrlSourceSplitterInputPin(void);

  // IUnknown
  DECLARE_IUNKNOWN
  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);

  // IFileSourceFilter
  STDMETHODIMP Load(LPCOLESTR pszFileName, const AM_MEDIA_TYPE * pmt);
  STDMETHODIMP GetCurFile(LPOLESTR *ppszFileName, AM_MEDIA_TYPE *pmt);

  // IAMOpenProgress interface
  STDMETHODIMP QueryProgress(LONGLONG *pllTotal, LONGLONG *pllCurrent);
  STDMETHODIMP AbortOperation(void);

  // IDownload interface
  STDMETHODIMP Download(LPCOLESTR uri, LPCOLESTR fileName);
  STDMETHODIMP DownloadAsync(LPCOLESTR uri, LPCOLESTR fileName, IDownloadCallback *downloadCallback);

  // IDownloadCallback interface
  void STDMETHODCALLTYPE OnDownloadCallback(HRESULT downloadResult);

  // IOutputStream interface

  // sets total length of stream to output pin
  // @param total : total length of stream in bytes
  // @param estimate : specifies if length is estimate
  // @return : S_OK if successful
  HRESULT SetTotalLength(int64_t total, bool estimate);

  // pushes media packets to filter
  // @param mediaPackets : collection of media packets to push to filter
  // @return : S_OK if successful
  HRESULT PushMediaPackets(CMediaPacketCollection *mediaPackets);

  // notifies output stream that end of stream was reached
  // this method can be called only when protocol support SEEKING_METHOD_POSITION
  // @param streamPosition : the last valid stream position
  // @return : S_OK if successful
  HRESULT EndOfStreamReached(int64_t streamPosition);

  // IParserOutputStream interface

  // tests if filter is downloading
  // @return : true if downloading, false otherwise
  bool IsDownloading(void);

  // finishes download with specified result
  // @param result : the result of download
  void FinishDownload(HRESULT result);

  // IFilter interface

  // gets logger instance
  // @return : logger instance or NULL if error
  CLogger *GetLogger(void);

  // gets total length of stream in bytes
  // @param totalLength : reference to total length variable
  // @return : S_OK if success, VFW_S_ESTIMATED if total length is not surely known, error code if error
  HRESULT GetTotalLength(int64_t *totalLength);

  // gets available length of stream in bytes
  // @param availableLength : reference to available length variable
  // @return : S_OK if success, error code if error
  HRESULT GetAvailableLength(int64_t *availableLength);

  // ISeeking interface

  // gets seeking capabilities of protocol
  // @return : bitwise combination of SEEKING_METHOD flags
  unsigned int GetSeekingCapabilities(void);

  // seeks to time (in ms)
  // @return : time in ms where seek finished or lower than zero if error
  int64_t SeekToTime(int64_t time);

  // request protocol implementation to receive data from specified position to specified position
  // @param start : the requested start position (zero is start of stream)
  // @param end : the requested end position, if end position is lower or equal to start position than end position is not specified
  // @return : position where seek finished or lower than zero if error
  int64_t SeekToPosition(int64_t start, int64_t end);

  // sets if protocol have to supress sending data to filter
  // @param supressData : true if protocol have to supress sending data to filter, false otherwise
  void SetSupressData(bool supressData);

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

  /* other methods */

  // gets parser hoster status
  // @return : one of STATUS_* values or error code if error
  HRESULT GetParserHosterStatus(void);

  // gets next output pin packet
  // @param packet : pointer to output packet
  // @return : S_OK if successful, S_FALSE if no output pin packet available, error code otherwise
  HRESULT GetNextPacket(COutputPinPacket *packet);

  // gets store file path
  // @return : store file path or NULL if store file is not used
  const wchar_t *GetStoreFilePath(void);

protected:

private:
  // configuration provided by filter
  CParameterCollection *configuration;
  // logger for logging purposes
  CLogger *logger;
  // the parent of this pin
  CMPUrlSourceSplitter *filter;
  // collection of media packets
  CMediaPacketCollection *mediaPacketCollection;
  // mutex for accessing media packets
  HANDLE mediaPacketMutex;
  // holds last received media packet time
  DWORD lastReceivedMediaPacketTime;

  int64_t totalLength;
  bool estimate;

  // specifies if all data are received (all data from stream has been received - it doesn't mean that has been stored to file)
  bool allDataReceived;
  // specifies if total length data are received (that means that we received end of stream, but not all data - there can be still gaps in stream)
  bool totalLengthReceived;

  CParserHoster *parserHoster;

  // initializes input pin and loads url based on configuration
  // @result : S_OK if successful
  STDMETHODIMP Load();

  // parses parameters from specified string
  // @param parameters : null-terminated string with specified parameters
  // @return : reference to variable holding collection of parameters or NULL if error
  CParameterCollection *ParseParameters(const wchar_t *parameters);

  // retrieves the total length of the stream
  // @param total : pointer to a variable that receives the length of the stream, in bytes
  // @param available : pointer to a variable that receives the portion of the stream that is currently available, in bytes
  // @return : S_OK if success, VFW_S_ESTIMATED if values are estimates, E_UNEXPECTED if error
  STDMETHODIMP Length(LONGLONG *total, LONGLONG *available);

  // retrieves available lenght of stream
  // @param available : reference to instance of class that receives the available length of stream, in bytes
  // @return : S_OK if successful, other error codes if error
  HRESULT QueryStreamAvailableLength(CStreamAvailableLength *availableLength);

  // get timeout (in ms) for receiving data
  // @return : timeout (in ms) for receiving data
  unsigned int GetReceiveDataTimeout(void);

  friend class CMPUrlSourceSplitter;

  /*
  AVIOContext *GetAVIOContext(void);
  void ReleaseAVIOContext(void);

  // IUnknown
  DECLARE_IUNKNOWN
  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);

  STDMETHODIMP BeginFlush();
  STDMETHODIMP EndFlush();

  // IFileSourceFilter
  STDMETHODIMP Load(LPCOLESTR pszFileName, const AM_MEDIA_TYPE * pmt);
  STDMETHODIMP GetCurFile(LPOLESTR *ppszFileName, AM_MEDIA_TYPE *pmt);

  // IAMOpenProgress interface
  STDMETHODIMP QueryProgress(LONGLONG *pllTotal, LONGLONG *pllCurrent);
  STDMETHODIMP AbortOperation(void);

  // IDownload interface
  STDMETHODIMP Download(LPCOLESTR uri, LPCOLESTR fileName);
  STDMETHODIMP DownloadAsync(LPCOLESTR uri, LPCOLESTR fileName, IDownloadCallback *downloadCallback);

  // IDownloadCallback interface
  void STDMETHODCALLTYPE OnDownloadCallback(HRESULT downloadResult);

  // IOutputStream interface

  // sets total length of stream to output pin
  // @param total : total length of stream in bytes
  // @param estimate : specifies if length is estimate
  // @return : S_OK if successful
  HRESULT SetTotalLength(int64_t total, bool estimate);

  // pushes media packets to filter
  // @param mediaPackets : collection of media packets to push to filter
  // @return : S_OK if successful
  HRESULT PushMediaPackets(CMediaPacketCollection *mediaPackets);

  // notifies output stream that end of stream was reached
  // this method can be called only when protocol support SEEKING_METHOD_POSITION
  // @param streamPosition : the last valid stream position
  // @return : S_OK if successful
  HRESULT EndOfStreamReached(int64_t streamPosition);

  // IParserOutputStream interface

  // tests if filter is downloading
  // @return : true if downloading, false otherwise
  bool IsDownloading(void);

  // finishes download with specified result
  // @param result : the result of download
  void FinishDownload(HRESULT result);

  // request protocol implementation to cancel the stream reading operation
  // @return : S_OK if successful
  HRESULT AbortStreamReceive(void);

  // retrieves the progress of the stream reading operation
  // @param total : reference to a variable that receives the length of the entire stream, in bytes
  // @param current : reference to a variable that receives the length of the downloaded portion of the stream, in bytes
  // @return : S_OK if successful, VFW_S_ESTIMATED if returned values are estimates, E_UNEXPECTED if unexpected error
  HRESULT QueryStreamProgress(LONGLONG *total, LONGLONG *current);

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

  REFERENCE_TIME GetStart(void);
  REFERENCE_TIME GetStop(void);
  REFERENCE_TIME GetCurrent(void);
  REFERENCE_TIME GetNewStart(void);
  REFERENCE_TIME GetNewStop(void);
  double GetPlayRate(void);
  BOOL GetStopValid(void);

  void SetStart(REFERENCE_TIME time);
  void SetStop(REFERENCE_TIME time);
  void SetCurrent(REFERENCE_TIME time);
  void SetNewStart(REFERENCE_TIME time);
  void SetNewStop(REFERENCE_TIME time);
  void SetPlayRate(double rate);
  void SetStopValid(BOOL valid);

  // IFilter interface

  // gets logger instance
  // @return : logger instance or NULL if error
  CLogger *GetLogger(void);

  // gets total length of stream in bytes
  // @param totalLength : reference to total length variable
  // @return : S_OK if success, VFW_S_ESTIMATED if total length is not surely known, error code if error
  HRESULT GetTotalLength(int64_t *totalLength);

  // gets available length of stream in bytes
  // @param availableLength : reference to available length variable
  // @return : S_OK if success, error code if error
  HRESULT GetAvailableLength(int64_t *availableLength);

  // ISeeking interface

  // gets seeking capabilities of protocol
  // @return : bitwise combination of SEEKING_METHOD flags
  unsigned int GetSeekingCapabilities(void);

  // seeks to time (in ms)
  // @return : time in ms where seek finished or lower than zero if error
  int64_t SeekToTime(int64_t time);

  // request protocol implementation to receive data from specified position to specified position
  // @param start : the requested start position (zero is start of stream)
  // @param end : the requested end position, if end position is lower or equal to start position than end position is not specified
  // @return : position where seek finished or lower than zero if error
  int64_t SeekToPosition(int64_t start, int64_t end);

  // sets if protocol have to supress sending data to filter
  // @param supressData : true if protocol have to supress sending data to filter, false otherwise
  void SetSupressData(bool supressData);

  // gets parser hoster status
  // @return : one of STATUS_* values or error code if error
  HRESULT GetParserHosterStatus(void);

protected:
  static int Read(void *opaque, uint8_t *buf, int buf_size);
  static int64_t Seek(void *opaque, int64_t offset, int whence);

  LONGLONG m_llBufferPosition;

private:
  // Times
  REFERENCE_TIME m_rtStart, m_rtStop, m_rtCurrent, m_rtNewStart, m_rtNewStop;
  double m_dRate;
  BOOL m_bStopValid;

  // Seeking
  REFERENCE_TIME m_rtLastStart, m_rtLastStop;
  std::set<void *> m_LastSeekers;

  // specifies if demuxer has been created
  bool createdDemuxer;

  AVIOContext *m_pAVIOContext;

  wchar_t* downloadFileName;

  bool asyncDownloadFinished;
  HRESULT asyncDownloadResult;
  IDownloadCallback *asyncDownloadCallback;

  // configuration provided by filter
  CParameterCollection *configuration;
  // logger for logging purposes
  CLogger *logger;
  // the parent of this pin
  CLAVSplitter *filter;
  // holds current read request
  CAsyncRequest *currentReadRequest;
  // collection of media packets
  CMediaPacketCollection *mediaPacketCollection;
  // mutex for accessing requests
  HANDLE requestMutex;
  // mutex for accessing media packets
  HANDLE mediaPacketMutex;
  // request ID for async requests
  unsigned int requestId;
  // holds last received media packet time
  DWORD lastReceivedMediaPacketTime;

  int64_t totalLength;
  bool estimate;

  // file path for storing received data to file
  wchar_t *storeFilePath;
  // specifies if we are downloading file
  // in that case we don't delete file on end
  bool downloadingFile;
  // specifies if download callback has been called
  bool downloadCallbackCalled;

  // specifies if all data are received (all data from stream has been received - it doesn't mean that has been stored to file)
  bool allDataReceived;
  // specifies if total length data are received (that means that we received end of stream, but not all data - there can be still gaps in stream)
  bool totalLengthReceived;

  CParserHoster *parserHoster;

  // initializes input pin and loads url based on configuration
  // @result : S_OK if successful
  STDMETHODIMP Load();

  // parses parameters from specified string
  // @param parameters : null-terminated string with specified parameters
  // @return : reference to variable holding collection of parameters or NULL if error
  CParameterCollection *ParseParameters(const wchar_t *parameters);

  HRESULT Request(CAsyncRequest **request, int64_t position, LONG length, BYTE *buffer, DWORD_PTR userData);

  // handle for thread which makes relation between CMediaPacket and CAsyncRequest
  HANDLE hAsyncRequestProcessingThread;
  DWORD dwAsyncRequestProcessingThreadId;
  bool asyncRequestProcessingShouldExit;
  static DWORD WINAPI AsyncRequestProcessWorker(LPVOID lpParam);

  // check async request and media packet values agains not valid values
  // @param request : async request
  // @param mediaPacket : media packet
  // @param mediaPacketDataStart : the reference to variable that holds data start within media packet (if successful)
  // @param mediaPacketDataLength : the reference to variable that holds data length within media packet (if successful)
  // @param startPosition : start position of data
  // @return : S_OK if successful, error code otherwise
  HRESULT CheckValues(CAsyncRequest *request, CMediaPacket *mediaPacket, unsigned int *mediaPacketDataStart, unsigned int *mediaPacketDataLength, int64_t startPosition);

  // creates async request worker
  // @return : S_OK if successful
  HRESULT CreateAsyncRequestProcessWorker(void);

  // destroys async request worker
  // @return : S_OK if successful
  HRESULT DestroyAsyncRequestProcessWorker(void);

  // performs a synchronous read
  // the method blocks until the request is completed
  // the stream position and the buffer address do not have to be aligned
  // if the request is not aligned, the method performs a buffered read operation
  // @param position : specifies the byte offset at which to begin reading, the method fails if this value is beyond the end of the stream
  // @param length : specifies the number of bytes to read
  // @param buffer : reference to a buffer that receives the data
  // @return : S_OK if successful, error code or length of data
  STDMETHODIMP SyncRead(int64_t position, LONG length, BYTE* buffer);

  // retrieves the total length of the stream
  // @param total : pointer to a variable that receives the length of the stream, in bytes
  // @param available : pointer to a variable that receives the portion of the stream that is currently available, in bytes
  // @return : S_OK if success, VFW_S_ESTIMATED if values are estimates, E_UNEXPECTED if error
  STDMETHODIMP Length(LONGLONG *total, LONGLONG *available);

  // retrieves available lenght of stream
  // @param available : reference to instance of class that receives the available length of stream, in bytes
  // @return : S_OK if successful, other error codes if error
  HRESULT QueryStreamAvailableLength(CStreamAvailableLength *availableLength);

  // get timeout (in ms) for receiving data
  // @return : timeout (in ms) for receiving data
  unsigned int GetReceiveDataTimeout(void);

  HANDLE hCreateDemuxerWorkerThread;
  DWORD dwCreateDemuxerWorkerThreadId;
  bool demuxerWorkerShouldExit;
  static DWORD WINAPI DemuxerWorker(LPVOID lpParam);

  // creates demuxer worker
  // @return : S_OK if successful
  HRESULT CreateDemuxerWorker(void);

  // destroys demuxer worker
  // @return : S_OK if successful
  HRESULT DestroyDemuxerWorker(void);

  // stores if demuxer worker finished its operation
  bool demuxerWorkerFinished;

  friend class CLAVSplitter;
  STDMETHODIMP SetPositionsInternal(void *caller, LONGLONG* pCurrent, DWORD dwCurrentFlags, LONGLONG* pStop, DWORD dwStopFlags);

  // gets store file path based on configuration
  // creates folder structure if not created
  // @return : store file or NULL if error
  wchar_t *GetStoreFile(void);

  // specifies if filter is processing live stream or not
  bool liveStream;
  */
};

#endif
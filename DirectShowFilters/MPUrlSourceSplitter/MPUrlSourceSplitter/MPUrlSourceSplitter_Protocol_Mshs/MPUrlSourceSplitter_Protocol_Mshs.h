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

#ifndef __MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_DEFINED

#include "Logger.h"
#include "ProtocolPlugin.h"
#include "MshsCurlInstance.h"
#include "MshsStreamFragmentCollection.h"
#include "CacheFile.h"

#define PROTOCOL_NAME                                                         L"MSHS"

#define TOTAL_SUPPORTED_PROTOCOLS                                             1
wchar_t *SUPPORTED_PROTOCOLS[TOTAL_SUPPORTED_PROTOCOLS] =                     { L"MSHS" };

#define MINIMUM_RECEIVED_DATA_FOR_SPLITTER                                    1 * 1024 * 1024

#define MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_NONE                        PROTOCOL_PLUGIN_FLAG_NONE

// only closes curl instance (stop receive data in curl instance), but stays in memory
#define MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_CLOSE_CURL_INSTANCE         (1 << (PROTOCOL_PLUGIN_FLAG_LAST + 0))
// stop receiving data flag cannot be set without close curl instance flag
// specifies that after closing curl instance is called StopReceivingData() method
#define MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_STOP_RECEIVING_DATA         (1 << (PROTOCOL_PLUGIN_FLAG_LAST + 1))

//#define MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SET_FIRST_TIMESTAMP         (1 << (PROTOCOL_PLUGIN_FLAG_LAST + 2))
//#define MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SKIP_HEADER_AND_META        (1 << (PROTOCOL_PLUGIN_FLAG_LAST + 3))
//
//#define MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SET_VIDEO_CORRECTION        (1 << (PROTOCOL_PLUGIN_FLAG_LAST + 4))
//#define MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SET_AUDIO_CORRECTION        (1 << (PROTOCOL_PLUGIN_FLAG_LAST + 5))

#define MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_LAST                        (PROTOCOL_PLUGIN_FLAG_LAST + 2)




/* old implementation */

//#include "Logger.h"
//#include "IProtocolPlugin.h"
//#include "LinearBufferCollection.h"
//#include "HttpCurlInstance.h"
//#include "StreamFragmentCollection.h"
//
//#include "MSHSSmoothStreamingMedia.h"
//#include "Box.h"
//#include "FileTypeBox.h"
//#include "TrackFragmentHeaderBox.h"
//#include "MovieBox.h"
//#include "TrackBox.h"
//#include "HandlerBox.h"
//#include "TrackHeaderBox.h"
//#include "MediaBox.h"
//#include "MediaHeaderBox.h"
//#include "MediaInformationBox.h"
//#include "VideoMediaHeaderBox.h"
//#include "SoundMediaHeaderBox.h"
//#include "DataInformationBox.h"
//#include "DataReferenceBox.h"
//#include "DataEntryUrlBox.h"
//#include "SampleTableBox.h"
//#include "SampleDescriptionBox.h"
//#include "TimeToSampleBox.h"
//#include "SampleToChunkBox.h"
//#include "ChunkOffsetBox.h"
//#include "MovieExtendsBox.h"
//#include "TrackExtendsBox.h"
//#include "VisualSampleEntryBox.h"
//#include "AudioSampleEntryBox.h"
//#include "AVCConfigurationBox.h"
//#include "ESDBox.h"
//#include "FragmentedIndexBox.h"
//#include "FragmentedIndexTrackBox.h"
//
//#include <curl/curl.h>
//
//#include <WinSock2.h>
//
//#define PROTOCOL_NAME                                                         L"MSHS"
//
//#define TOTAL_SUPPORTED_PROTOCOLS                                             1
//wchar_t *SUPPORTED_PROTOCOLS[TOTAL_SUPPORTED_PROTOCOLS] =                     { L"MSHS" };
//
//#define MINIMUM_RECEIVED_DATA_FOR_SPLITTER                                    1 * 1024 * 1024
//
//class CMPUrlSourceSplitter_Protocol_Mshs : public IProtocolPlugin
//{
//public:
//  // constructor
//  // create instance of CMPUrlSourceSplitter_Protocol_Mshs class
//  CMPUrlSourceSplitter_Protocol_Mshs(CLogger *logger, CParameterCollection *configuration);
//
//  // destructor
//  ~CMPUrlSourceSplitter_Protocol_Mshs(void);
//
//  // IProtocol interface
//
//  // test if connection is opened
//  // @return : true if connected, false otherwise
//  bool IsConnected(void);
//
//  // parse given url to internal variables for specified protocol
//  // errors should be logged to log file
//  // @param parameters : the url and connection parameters
//  // @return : S_OK if successfull
//  HRESULT ParseUrl(const CParameterCollection *parameters);
//
//  // receives data and stores them into receive data parameter
//  // the method should fill receiveData parameter with relevant data and finish
//  // the method can't block call (method is called within thread which can be terminated anytime)
//  // @param receiveData : received data
//  // @result: S_OK if successful, error code otherwise
//  HRESULT ReceiveData(CReceiveData *receiveData);
//
//  // gets current connection parameters (can be different as supplied connection parameters)
//  // @return : current connection parameters or NULL if error
//  CParameterCollection *GetConnectionParameters(void);
//
//  // ISimpleProtocol interface
//
//  // get timeout (in ms) for receiving data
//  // @return : timeout (in ms) for receiving data
//  unsigned int GetReceiveDataTimeout(void);
//
//  // starts receiving data from specified url and configuration parameters
//  // @param parameters : the url and parameters used for connection
//  // @return : S_OK if url is loaded, false otherwise
//  HRESULT StartReceivingData(CParameterCollection *parameters);
//
//  // request protocol implementation to cancel the stream reading operation
//  // @return : S_OK if successful
//  HRESULT StopReceivingData(void);
//
//  // retrieves the progress of the stream reading operation
//  // @param streamProgress : reference to instance of class that receives the stream progress
//  // @return : S_OK if successful, VFW_S_ESTIMATED if returned values are estimates, E_INVALIDARG if stream ID is unknown, E_UNEXPECTED if unexpected error
//  HRESULT QueryStreamProgress(CStreamProgress *streamProgress);
//  
//  // retrieves available lenght of stream
//  // @param available : reference to instance of class that receives the available length of stream, in bytes
//  // @return : S_OK if successful, other error codes if error
//  HRESULT QueryStreamAvailableLength(CStreamAvailableLength *availableLength);
//
//  // clear current session
//  // @return : S_OK if successfull
//  HRESULT ClearSession(void);
//
//  // gets duration of stream in ms
//  // @return : stream duration in ms or DURATION_LIVE_STREAM in case of live stream or DURATION_UNSPECIFIED if duration is unknown
//  int64_t GetDuration(void);
//
//  // reports actual stream time to protocol
//  // @param streamTime : the actual stream time in ms to report to protocol
//  void ReportStreamTime(uint64_t streamTime);
//
//  // ISeeking interface
//
//  // gets seeking capabilities of protocol
//  // @return : bitwise combination of SEEKING_METHOD flags
//  unsigned int GetSeekingCapabilities(void);
//
//  // request protocol implementation to receive data from specified time (in ms)
//  // @param time : the requested time (zero is start of stream)
//  // @return : time (in ms) where seek finished or lower than zero if error
//  int64_t SeekToTime(int64_t time);
//
//  // sets if protocol implementation have to supress sending data to filter
//  // @param supressData : true if protocol have to supress sending data to filter, false otherwise
//  void SetSupressData(bool supressData);
//
//  // IPlugin interface
//
//  // return reference to null-terminated string which represents plugin name
//  // function have to allocate enough memory for plugin name string
//  // errors should be logged to log file and returned NULL
//  // @return : reference to null-terminated string
//  const wchar_t *GetName(void);
//
//  // get plugin instance ID
//  // @return : GUID, which represents instance identifier or GUID_NULL if error
//  GUID GetInstanceId(void);
//
//  // initialize plugin implementation with configuration parameters
//  // @param configuration : the reference to additional configuration parameters (created by plugin's hoster class)
//  // @return : S_OK if successfull
//  HRESULT Initialize(PluginConfiguration *configuration);
//
//protected:
//  CLogger *logger;
//
//  // holds various parameters supplied by caller
//  CParameterCollection *configurationParameters;
//
//  // holds receive data timeout
//  unsigned int receiveDataTimeout;
//
//  // the lenght of stream
//  LONGLONG streamLength;
//
//  // holds if length of stream was set
//  bool setLength;
//  // holds if end of stream was set
//  bool setEndOfStream;
//
//  // stream time
//  int64_t streamTime;
//
//  // specifies position in buffer
//  // it is always reset on seek
//  int64_t bytePosition;
//
//  // mutex for locking access to file, buffer, ...
//  HANDLE lockMutex;
//
//  // mutex for locking access to internal buffer of CURL instance
//  HANDLE lockCurlMutex;
//
//  // main instance of CURL
//  CHttpCurlInstance *mainCurlInstance;
//
//  // holds is ISO based media file format header reconstructed
//  bool reconstructedHeader;
//
//  // specifies if whole stream is downloaded
//  bool wholeStreamDownloaded;
//  // specifies if seeking (cleared when first data arrive)
//  bool seekingActive;
//  // specifies if filter requested supressing data
//  bool supressData;
//
//  // holds which stream fragment is currently downloading (UINT_MAX means none)
//  unsigned int streamFragmentDownloading;
//  // holds which stream fragment is currently processed
//  unsigned int streamFragmentProcessing;
//  // holds which stream fragment have to be downloaded
//  // (UINT_MAX means next stream fragment, always reset after started download of stream fragment)
//  unsigned int streamFragmentToDownload;
//
//  // buffer for processing data before are send to filter
//  CLinearBuffer *bufferForProcessing;
//
//  // holds stream fragments and urls
//  CStreamFragmentCollection *streamFragments;
//
//  // holds smooth streaming media for further processing
//  CMSHSSmoothStreamingMedia *streamingMedia;
//
//  // specifies if last fragment was downloaded
//  //bool lastFragmentDownloaded;
//
//  //uint32_t videoTrackId;
//  CTrackFragmentHeaderBox *videoTrackFragmentHeaderBox;
//
//  //uint32_t audioTrackId;
//  CTrackFragmentHeaderBox *audioTrackFragmentHeaderBox;
//
//  // gets first not downloaded stream fragment
//  // @param requested : start index for searching
//  // @return : index of first not downloaded stream fragment or UINT_MAX if not exists
//  unsigned int GetFirstNotDownloadedStreamFragment(unsigned int start);
//
//  // gets stream fragments collection created from manifest
//  // @param logger : the logger for logging purposes
//  // @param methodName : the name of method calling GetStreamFragmentsFromManifest()
//  // @param configurationParameters : the configuration parameters
//  // @param manifest : manifest to create stream fragments collection
//  // @param logCollection : specifies if result collection should be logged
//  // @return : stream fragments collection created from manifest or NULL if error
//  CStreamFragmentCollection *GetStreamFragmentsFromManifest(CLogger *logger, const wchar_t *methodName, CParameterCollection *configurationParameters, CMSHSSmoothStreamingMedia *manifest, bool logCollection);
//
//  // formats url
//  // @param baseUrl : the base url (manifest url)
//  // @param urlPattern : the url pattern
//  // @param track : track for which is url formatted
//  // @param fragment : fragment for which is url formatted
//  // @return : url or NULL if error
//  wchar_t *FormatUrl(const wchar_t *baseUrl, const wchar_t *urlPattern, CMSHSTrack *track, CMSHSStreamFragment *fragment);
//
//  // creates file type box
//  // @return : file type box or NULL if error
//  CFileTypeBox *CreateFileTypeBox(void);
//
//  // gets track fragment header box from linear buffer (video or audio CURL instance)
//  // @param buffer : buffer to get track fragment header box
//  // @param trackID : the track ID to set to track fragment, if UINT_MAX than track ID is not changed
//  // @return : track fragment header box or NULL if error
//  CTrackFragmentHeaderBox *GetTrackFragmentHeaderBox(CLinearBuffer *buffer, unsigned int trackID);
//
//  // stores box into buffer
//  // @param box : box to store in buffer
//  // @param buffer : the buffer
//  // @return : true if successful, false otherwise
//  bool PutBoxIntoBuffer(CBox *box, CLinearBuffer *buffer);
//
//  // gets movie box
//  // @param media : smooth streaming media
//  // @param videoFragmentHeaderBox : video fragment header box
//  // @param audioFragmentHeaderBox : audio fragment header box
//  // @return : movie box or NULL if error
//  CMovieBox *GetMovieBox(CMSHSSmoothStreamingMedia *media, CTrackFragmentHeaderBox *videoFragmentHeaderBox, CTrackFragmentHeaderBox *audioFragmentHeaderBox);
//
//  // gets video track box
//  // @param media : smooth streaming media
//  // @param streamIndex : the index of stream
//  // @param trackIndex : the index of track in stream
//  // @param fragmentHeaderBox : fragment header box
//  // @return : video track box or NULL if error
//  CTrackBox *GetVideoTrackBox(CMSHSSmoothStreamingMedia *media, unsigned int streamIndex, unsigned int trackIndex, CTrackFragmentHeaderBox *fragmentHeaderBox);
//
//  // gets audio track box
//  // @param media : smooth streaming media
//  // @param fragmentHeaderBox : fragment header box
//  // @return : audio track box or NULL if error
//  CTrackBox *GetAudioTrackBox(CMSHSSmoothStreamingMedia *media, unsigned int streamIndex, unsigned int trackIndex, CTrackFragmentHeaderBox *fragmentHeaderBox);
//
//  CDataInformationBox *GetDataInformationBox(void);
//
//  CHandlerBox *GetHandlerBox(uint32_t handlerType, const wchar_t *handlerName);
//
//  CSampleTableBox *GetVideoSampleTableBox(CMSHSSmoothStreamingMedia *media, unsigned int streamIndex, unsigned int trackIndex, CTrackFragmentHeaderBox *fragmentHeaderBox);
//
//  CSampleTableBox *GetAudioSampleTableBox(CMSHSSmoothStreamingMedia *media, unsigned int streamIndex, unsigned int trackIndex, CTrackFragmentHeaderBox *fragmentHeaderBox);
//
//  CFragmentedIndexBox *GetFragmentedIndexBox(CMSHSSmoothStreamingMedia *media, uint32_t videoTrackId, uint32_t audioTrackId, uint64_t timestamp);
//
//  CFragmentedIndexTrackBox *GetFragmentedIndexTrackBox(CMSHSSmoothStreamingMedia *media, unsigned int streamIndex, uint32_t trackId, uint64_t timestamp);
//
//  // gets store file path based on configuration
//  // creates folder structure if not created
//  // @return : store file or NULL if error
//  wchar_t *GetStoreFile(void);
//
//  // holds store file path
//  wchar_t *storeFilePath;
//  // holds last store time of storing stream fragments to file
//  DWORD lastStoreTime;
//
//  // specifies if we are still connected
//  bool isConnected;
//
//  // fills buffer for processing with stream fragment data (stored in memory or in store file)
//  // @param streamFragments : stream fragments collection
//  // @param streamFragmentProcessing : stream fragment to get data
//  // @param storeFile : the name of store file
//  // @return : buffer for processing with filled data, NULL otherwise
//  CLinearBuffer *FillBufferForProcessing(CStreamFragmentCollection *streamFragments, unsigned int streamFragmentProcessing, wchar_t *storeFile);
//
//  // holds last used track ID
//  unsigned int lastTrackID;
//
//  // holds filter actual stream time
//  uint64_t reportedStreamTime;
//};

#endif

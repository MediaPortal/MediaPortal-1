
/* 
 *	Copyright (C) 2005 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#pragma once
#include "multifilereader.h"
#include "pcrdecoder.h"
#include "demultiplexer.h"
#include "TsDuration.h"
#include "TSThread.h"
#include "rtspclient.h"
#include "memorybuffer.h"
#include "..\..\DVBSubtitle3\Source\IDVBSub.h"
#include "ISubtitleStream.h"
#include "IAudioStream.h"
#include "ITeletextSource.h"
#include <map>

#define AUDIO_CHANGE 0x1
#define VIDEO_CHANGE 0x2

#define INIT_SHOWBUFFERVIDEO 3
#define INIT_SHOWBUFFERAUDIO 3

//Buffer control and start-of-play timing control constants
#define FS_TIM_LIM (2000*10000) //2 seconds in hns units
#define FS_ADDON_LIM (1000*10000) //1 second in hns units (must not be zero)
#define INITIAL_BUFF_DELAY 0      // ms units
#define AV_READY_DELAY 0     // ms units
#define PRESENT_DELAY (300*10000) // hns units - timestamp compensation offset

using namespace std;

//Macro for replacing timeGetTime()
//The macro is used to avoid having to handle timeGetTime() rollover issues in the body of the code
//m_tGTStartTime is initialised in CTsReaderFilter::CTsReaderFilter() when filter is loaded
#define GET_TIME_NOW() (timeGetTime() - m_tGTStartTime)

class CSubtitlePin;
class CAudioPin;
class CVideoPin;
class CTsReader;
class CTsReaderFilter;

DEFINE_GUID(CLSID_TSReader, 0xb9559486, 0xe1bb, 0x45d3, 0xa2, 0xa2, 0x9a, 0x7a, 0xfe, 0x49, 0xb2, 0x3f);
DEFINE_GUID(IID_ITSReader, 0xb9559486, 0xe1bb, 0x45d3, 0xa2, 0xa2, 0x9a, 0x7a, 0xfe, 0x49, 0xb2, 0x4f);
//DEFINE_GUID(IID_ITSReaderAudioChange, 0xb9559486, 0xe1bb, 0x45d3, 0xa2, 0xa2, 0x9a, 0x7a, 0xfe, 0x49, 0xb2, 0x5f);

// CLSIDs used to identify connected filters
// {04FE9017-F873-410e-871E-AB91661A4EF7}
DEFINE_GUID(CLSID_FFDSHOWVIDEO, 0x04fe9017, 0xf873, 0x410e, 0x87, 0x1e, 0xab, 0x91, 0x66, 0x1a, 0x4e, 0xf7);
// {0B390488-D80F-4a68-8408-48DC199F0E97}
DEFINE_GUID(CLSID_FFDSHOWDXVA, 0xb0eff97, 0xc750, 0x462c, 0x94, 0x88, 0xb1, 0xe, 0x7d, 0x87, 0xf1, 0xa6);
// {DBF9000E-F08C-4858-B769-C914A0FBB1D7}
DEFINE_GUID(CLSID_FFDSHOWSUBTITLES, 0xdbf9000e, 0xf08c, 0x4858, 0xb7, 0x69, 0xc9, 0x14, 0xa0, 0xfb, 0xb1, 0xd7);
// {[uuid("62D767FE-4F1B-478B-B350-8ACE9E4DB00E")]}
DEFINE_GUID(CLSID_LAVCUVID, 0x62D767FE, 0x4F1B, 0x478B, 0xB3, 0x50, 0x8A, 0xCE, 0x9E, 0x4D, 0xB0, 0x0E);
// [uuid("EE30215D-164F-4A92-A4EB-9D4C13390F9F")]
DEFINE_GUID(CLSID_LAVVIDEO, 0xEE30215D, 0x164F, 0x4A92, 0xA4, 0xEB, 0x9D, 0x4C, 0x13, 0x39, 0x0F, 0x9F);


DECLARE_INTERFACE_(ITSReaderCallback, IUnknown)
{
	STDMETHOD(OnMediaTypeChanged) (int mediaTypes)PURE;	
	STDMETHOD(OnVideoFormatChanged) (int streamType,int width,int height,int aspectRatioX,int aspectRatioY,int bitrate,int isInterlaced)PURE;	
};

DECLARE_INTERFACE_(ITSReaderAudioChange, IUnknown)
{	
	STDMETHOD(OnRequestAudioChange) (THIS_)PURE;
};

  MIDL_INTERFACE("b9559486-e1bb-45d3-a2a2-9a7afe49b24f")
  ITSReader : public IUnknown
  {
  public:
      virtual HRESULT STDMETHODCALLTYPE SetGraphCallback(ITSReaderCallback* pCallback) = 0;		        
			virtual HRESULT STDMETHODCALLTYPE SetRequestAudioChangeCallback(ITSReaderAudioChange* pCallback) = 0;
      virtual HRESULT STDMETHODCALLTYPE SetRelaxedMode(BOOL relaxedReading) = 0;
      virtual void    STDMETHODCALLTYPE OnZapping(int info) = 0;
      virtual void    STDMETHODCALLTYPE OnGraphRebuild(int info) = 0;
      virtual void    STDMETHODCALLTYPE SetMediaPosition(REFERENCE_TIME MediaPos) = 0;

		  //virtual HRESULT STDMETHODCALLTYPE GetVideoFormat(int *width,int *height, int *aspectRatioX,int *aspectRatioY,int *bitrate,int *interlaced) PURE;
  };

class CTsReaderFilter : public CSource, 
						public TSThread, 
						public IFileSourceFilter, 
            public IAMFilterMiscFlags, 
						public IAMStreamSelect, 
						public ISubtitleStream, 
						public ITeletextSource,						
						public IAudioStream,
						public ITSReader
{
public:
  DECLARE_IUNKNOWN
  static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

private:
  CTsReaderFilter(IUnknown *pUnk, HRESULT *phr);
  ~CTsReaderFilter();
  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

  // Pin enumeration
  CBasePin * GetPin(int n);
  int GetPinCount();

  // Open and close the file as necessary
public:
  STDMETHODIMP Run(REFERENCE_TIME tStart);
  STDMETHODIMP Pause();
  STDMETHODIMP Stop();
  STDMETHODIMP GetState(DWORD dwMilliSecsTimeout, FILTER_STATE *pState);

private:
  // IAMFilterMiscFlags
  virtual ULONG STDMETHODCALLTYPE		GetMiscFlags();

  //IAMStreamSelect
  STDMETHODIMP Count(DWORD* streamCount);
  STDMETHODIMP Enable(long index, DWORD flags);
  STDMETHODIMP Info( long lIndex,AM_MEDIA_TYPE **ppmt,DWORD *pdwFlags, LCID *plcid, DWORD *pdwGroup, WCHAR **ppszName, IUnknown **ppObject, IUnknown **ppUnk);	

  //IAudioStream
  //STDMETHODIMP SetAudioStream(__int32 stream);	
  STDMETHODIMP GetAudioStream(__int32 &stream);

  //ISubtitleStream
  STDMETHODIMP SetSubtitleStream(__int32 stream);
  STDMETHODIMP GetSubtitleStreamType(__int32 stream, int &type);
  STDMETHODIMP GetSubtitleStreamCount(__int32 &count);
  STDMETHODIMP GetCurrentSubtitleStream(__int32 &stream);
  STDMETHODIMP GetSubtitleStreamLanguage(__int32 stream,char* szLanguage);
  STDMETHODIMP SetSubtitleResetCallback( int (CALLBACK *pSubUpdateCallback)(int count, void* opts, int* select)); 

  //ITeletextSource
  STDMETHODIMP SetTeletextTSPacketCallBack ( int (CALLBACK *pPacketCallback)(byte*, int));
  STDMETHODIMP SetTeletextEventCallback (int (CALLBACK *EventCallback)(int,DWORD64) ); 
  STDMETHODIMP SetTeletextServiceInfoCallback (int (CALLBACK *pServiceInfoCallback)(int,byte,byte,byte,byte) ); 

public:
  // ITSReader
  STDMETHODIMP	  SetGraphCallback(ITSReaderCallback* pCallback);
  STDMETHODIMP	  SetRequestAudioChangeCallback(ITSReaderAudioChange* pCallback);
  STDMETHODIMP	  SetRelaxedMode(BOOL relaxedReading);

  void STDMETHODCALLTYPE  OnZapping(int info);
  void STDMETHODCALLTYPE  OnGraphRebuild(int info);
  void STDMETHODCALLTYPE  SetMediaPosition(REFERENCE_TIME MediaPos);
	
  // IFileSourceFilter
  STDMETHODIMP    Load(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt);
  STDMETHODIMP    GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt);
  STDMETHODIMP    GetDuration(REFERENCE_TIME *dur);
  double          GetStartTime();
  bool            IsFilterRunning();
  CDeMultiplexer& GetDemultiplexer();
  void            Seek(CRefTime&  seekTime, bool seekInFile);
//  void            SeekDone(CRefTime& refTime);
//  void            SeekStart();
  HRESULT         SeekPreStart(CRefTime& rtSeek);
  bool            SetSeeking(bool onOff);
  void            SetWaitDataAfterSeek(bool onOff);
  double          UpdateDuration();
  CAudioPin*      GetAudioPin();
  CVideoPin*      GetVideoPin();
  CSubtitlePin*   GetSubtitlePin();
  IDVBSubtitle*   GetSubtitleFilter();
  bool            IsTimeShifting();
  bool            IsRTSP();
  bool            IsUNCfile();
  bool            IsLiveTV();
  CTsDuration&    GetDuration();
  FILTER_STATE    State() {return m_State;};
  void            DeltaCompensation(REFERENCE_TIME deltaComp);
  void            SetCompensation(CRefTime newComp);
  CRefTime        GetCompensation();
  CRefTime        Compensation;
  CRefTime        AddVideoComp;
  void            OnMediaTypeChanged(int mediaTypes);
  void            OnRequestAudioChange();
  void            OnVideoFormatChanged(int streamType,int width,int height,int aspectRatioX,int aspectRatioY,int bitrate,int isInterlaced);
  bool            IsStreaming();

  bool            IsSeeking();
  int             SeekingDone();
  bool            IsStopping();
  bool            IsWaitDataAfterSeek();

  DWORD           m_lastPause;
  bool            m_bStreamCompensated;
  CRefTime        m_ClockOnStart;
  bool            m_bForcePosnUpdate;

  REFERENCE_TIME  m_RandomCompensation;
  REFERENCE_TIME  m_MediaPos;
  REFERENCE_TIME  m_BaseTime;
  REFERENCE_TIME  m_LastTime;
  
  bool            m_bLiveTv;
  bool            m_bStopping;
  bool            m_bWaitForSeek;
	
  void GetTime(REFERENCE_TIME *Time);
  void GetMediaPosition(REFERENCE_TIME *pMediaTime);

  bool            m_bOnZap;
  bool            m_bForceSeekOnStop;
  bool            m_bRenderingClockTooFast;
  bool            m_bForceSeekAfterRateChange;
  bool            m_bSeekAfterRcDone;

  int             m_ShowBufferAudio;
  int             m_ShowBufferVideo;

  CLSID           m_videoDecoderCLSID;
  bool            m_bFastSyncFFDShow;
  bool            m_EnableSlowMotionOnZapping;
  bool            m_bDisableVidSizeRebuildMPEG2;
  bool            m_bDisableVidSizeRebuildH264;
  bool            m_bDisableAddPMT;

  CLSID           GetCLSIDFromPin(IPin* pPin);
  
  void            SetErrorAbort();
  bool            CheckAudioCallback();
  bool            CheckCallback();
  void            CheckForMPAR();
  bool            m_bMPARinGraph;
protected:
  void ThreadProc();

private:
  void    SetDuration();
  HRESULT AddGraphToRot(IUnknown *pUnkGraph);
  HRESULT FindSubtitleFilter();
  void    RemoveGraphFromRot();
  void    SetMediaPosnUpdate(REFERENCE_TIME MediaPos);
  void    BufferingPause(bool longPause);
  void    ReadRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data);
  void    WriteRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data);
    
  CAudioPin*	    m_pAudioPin;
  CVideoPin*	    m_pVideoPin;
  CSubtitlePin*	  m_pSubtitlePin;
  WCHAR           m_fileName[1024];
  CCritSec        m_section;
  CCritSec        m_CritSecDuration;
  CCritSec        m_GetTimeLock;
  CCritSec        m_GetCompLock;
  FileReader*     m_fileReader;
  FileReader*     m_fileDuration;
  CTsDuration     m_duration;
  CBaseReferenceClock* m_referenceClock;
  CDeMultiplexer  m_demultiplexer;
  DWORD           m_dwGraphRegister;

  CRTSPClient     m_rtspClient;
  CMemoryBuffer   m_buffer;
  DWORD           m_tickCount;
  CRefTime        m_seekTime;
  CRefTime        m_absSeekTime;
  bool            m_bTimeShifting;
  bool            m_bRecording;
  IDVBSubtitle*   m_pDVBSubtitle;
  ITSReaderCallback* m_pCallback;
  ITSReaderAudioChange* m_pRequestAudioCallback;

  bool            m_bAnalog;
  bool            m_bStoppedForUnexpectedSeek ;
  bool            m_bPauseOnClockTooFast;
  DWORD           m_MPmainThreadID;
  bool            m_isUNCfile;
  CCritSec        m_sectionSeeking;

};


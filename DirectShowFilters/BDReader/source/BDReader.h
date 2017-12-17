/*
 *  Copyright (C) 2005-2011 Team MediaPortal
 *  http://www.team-mediaportal.com
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
#include <bluray.h>
#include <map>
#include <set>

#include "pcrdecoder.h"
#include "demultiplexer.h"
#include "IAudioStream.h"
#include "LibBlurayWrapper.h"
#include "BDEventObserver.h"

//#define LOG_VIDEO_PIN_SAMPLES
//#define LOG_AUDIO_PIN_SAMPLES
//#define LOG_DEMUXER_AUDIO_SAMPLES
//#define LOG_DEMUXER_VIDEO_SAMPLES

//#define LOG_SEEK_INFORMATION

#define CONVERT_90KHz_DS(x) (REFERENCE_TIME)(x * 111 + x / 9)
#define CONVERT_DS_90KHz(x) (REFERENCE_TIME)(x / 100 - x / 1000)

#define IDLE_SLEEP_DURATION 40

using namespace std;

class CAudioPin;
class CVideoPin;
class CBDReader;
class CBDReaderFilter;

struct OSDTexture;

#define AM_SEEKING_FakeSeek 0x40

DEFINE_GUID(CLSID_BDReader, 0x79a37017, 0x3178, 0x4859, 0x80, 0x79, 0xec, 0xb9, 0xd5, 0x46, 0xfe, 0xb2);
DEFINE_GUID(IID_IBDReader, 0x79a37017, 0x3178, 0x4859, 0x80, 0x79, 0xec, 0xb9, 0xd5, 0x46, 0xfe, 0xc2);

DECLARE_INTERFACE_(IBDReaderCallback, IUnknown)
{
  STDMETHOD(OnMediaTypeChanged)(int videoRate, int videoFormat, int audioFormat)PURE;	
  STDMETHOD(OnBDEvent)(BD_EVENT event)PURE;
  STDMETHOD(OnOSDUpdate)(OSDTexture texture)PURE;
  STDMETHOD(OnClockChange)(REFERENCE_TIME duration, REFERENCE_TIME position)PURE;
};

DECLARE_INTERFACE_(IBDReaderAudioChange, IUnknown)
{	
	STDMETHOD(OnRequestAudioChange)(THIS_)PURE;
};

MIDL_INTERFACE("79A37017-3178-4859-8079-ECB9D546FEC2")
IBDReader : public IUnknown
{
public:
    virtual HRESULT STDMETHODCALLTYPE SetGraphCallback(IBDReaderCallback* pCallback) = 0;
    virtual HRESULT STDMETHODCALLTYPE Action(int key) = 0;
    virtual HRESULT STDMETHODCALLTYPE SetAngle(UINT8 angle) = 0;
    virtual HRESULT STDMETHODCALLTYPE SetChapter(UINT32 chapter) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetAngle(UINT8* angle) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetChapter(UINT32* chapter) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetTitleCount(UINT32* count) = 0;
    virtual BLURAY_TITLE_INFO* STDMETHODCALLTYPE GetTitleInfo(UINT32 pIndex) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetCurrentClipStreamInfo(BLURAY_STREAM_INFO* stream) = 0;
    virtual HRESULT STDMETHODCALLTYPE FreeTitleInfo(BLURAY_TITLE_INFO* info) = 0;
    virtual void    STDMETHODCALLTYPE OnGraphRebuild(int info) = 0;
    virtual void    STDMETHODCALLTYPE ForceTitleBasedPlayback(bool force, UINT32 pTitle) = 0;
    virtual void    STDMETHODCALLTYPE SetD3DDevice(IDirect3DDevice9* device) = 0;
    virtual void    STDMETHODCALLTYPE SetBDPlayerSettings(bd_player_settings settings) = 0;
    virtual HRESULT STDMETHODCALLTYPE Start() = 0;
    virtual HRESULT STDMETHODCALLTYPE MouseMove(UINT16 x, UINT16 y) = 0;
    virtual HRESULT STDMETHODCALLTYPE SetVideoDecoder(int format, GUID* decoder) = 0;
    virtual HRESULT STDMETHODCALLTYPE SetVC1Override(GUID* subtype) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetAudioChannelCount(long lIndex) = 0;
};

enum DS_CMD_ID
{
  REBUILD,
  SEEK,
  PAUSE,
  RESUME
};

class CBDReaderFilter : public CSource, 
                        public IFileSourceFilter, 
                        public IAMFilterMiscFlags, 
                        public IAMStreamSelect, 
                        public IAudioStream,
                        public IBDReader,
                        public BDEventObserver,
                        public CCritSec
{
public:
  DECLARE_IUNKNOWN
  static CUnknown* WINAPI CreateInstance(LPUNKNOWN punk, HRESULT* phr);

private:
  CBDReaderFilter(IUnknown* pUnk, HRESULT* phr);
  ~CBDReaderFilter();
  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);

  // Pin enumeration
  CBasePin * GetPin(int n);
  int GetPinCount();

public:
  STDMETHODIMP Run(REFERENCE_TIME tStart);
  STDMETHODIMP Pause();
  STDMETHODIMP Stop();

private:
  // IAMFilterMiscFlags
  virtual ULONG STDMETHODCALLTYPE GetMiscFlags();

  // IAMStreamSelect
  STDMETHODIMP Count(DWORD* streamCount);
  STDMETHODIMP Enable(long index, DWORD flags);
  STDMETHODIMP Info(long lIndex, AM_MEDIA_TYPE** ppmt, DWORD* pdwFlags, LCID* plcid, DWORD* pdwGroup, WCHAR** ppszName, IUnknown** ppObject, IUnknown** ppUnk);	

  // IAudioStream
  STDMETHODIMP GetAudioStream(__int32 &stream);

public:
  // IBDReader
  STDMETHODIMP SetGraphCallback(IBDReaderCallback* pCallback);
  STDMETHODIMP Action(int key);
  STDMETHODIMP SetAngle(UINT8 angle);
  STDMETHODIMP SetChapter(UINT32 chapter);
  STDMETHODIMP GetAngle(UINT8* angle);
  STDMETHODIMP GetChapter(UINT32* chapter);
  STDMETHODIMP GetTitleCount(UINT32* count);
  BLURAY_TITLE_INFO* STDMETHODCALLTYPE GetTitleInfo(UINT32 pIndex);
  STDMETHODIMP GetCurrentClipStreamInfo(BLURAY_STREAM_INFO* stream);
  STDMETHODIMP FreeTitleInfo(BLURAY_TITLE_INFO* info);
  STDMETHODIMP Start();
  STDMETHODIMP MouseMove(UINT16 x, UINT16 y);
  STDMETHODIMP SetVideoDecoder(int format, GUID* decoder);
  STDMETHODIMP SetVC1Override(GUID* subtype);
  STDMETHODIMP GetAudioChannelCount(long lIndex);

  void STDMETHODCALLTYPE OnGraphRebuild(int info);
  void STDMETHODCALLTYPE ForceTitleBasedPlayback(bool force, UINT32 pTitle);
  void STDMETHODCALLTYPE SetD3DDevice(IDirect3DDevice9* device);
  void STDMETHODCALLTYPE SetBDPlayerSettings(bd_player_settings settings);

  // BDEventObserver
  void HandleBDEvent(BD_EVENT& pEv);
  void HandleOSDUpdate(OSDTexture& pTexture);

  // IFileSourceFilter
  STDMETHODIMP    Load(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt);
  STDMETHODIMP    GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt);
  CDeMultiplexer& GetDemultiplexer();
  void            Seek(REFERENCE_TIME rtAbsSeek);
  CAudioPin*      GetAudioPin();
  CVideoPin*      GetVideoPin();
  FILTER_STATE    State() {return m_State;}

  // IMediaSeeking
  STDMETHODIMP GetDuration(LONGLONG* pDuration);
  STDMETHODIMP SetPositions(LONGLONG* pCurrent, DWORD dwCurrentFlags, LONGLONG* pStop, DWORD dwStopFlags);
  STDMETHODIMP SetPositionsInternal(void *caller, LONGLONG* pCurrent, DWORD dwCurrentFlags, LONGLONG* pStop, DWORD dwStopFlags);
  
  // Extended
  STDMETHODIMP GetTime(REFERENCE_TIME* pTime);

  bool IsStopping();

  void IssueCommand(DS_CMD_ID pCommand, REFERENCE_TIME pTime);
  void TriggerOnMediaChanged();
  void OnPlaybackPositionChange();
  void ResetPlaybackOffset(REFERENCE_TIME pSeekAmount);
  void SetTitleDuration(REFERENCE_TIME pTitleDuration);

  CLibBlurayWrapper lib;

  bool  m_bStopping;

private:

  struct DS_CMD
  {
    DS_CMD_ID id;
    CRefTime refTime;
  };

  void DeliverBeginFlush();
  void DeliverEndFlush();

  REFERENCE_TIME GetScr();

  CAudioPin*      m_pAudioPin;
  CVideoPin*      m_pVideoPin;
  WCHAR           m_fileName[1024];
  CCritSec        m_section;
  CDeMultiplexer  m_demultiplexer;

  IBDReaderCallback* m_pCallback;
  IBDReaderAudioChange* m_pRequestAudioCallback;

  DWORD           m_MPmainThreadID;

  IMediaSeeking*  m_pMediaSeeking;
  IMediaControl*  m_pMediaControl;

  char            m_pathToBD[MAX_PATH];

  // Seek thread
  static DWORD WINAPI CommandThreadEntryPoint(LPVOID lpParameter);
  DWORD WINAPI        CommandThread();

  vector<DS_CMD>  m_commandQueue;
  CCritSec        m_csCommandQueue;

  typedef vector<DS_CMD>::iterator ivecCommandQueue;
  
  HANDLE          m_hCommandThread;
  HANDLE          m_hCommandEvent;
  HANDLE          m_hStopCommandThreadEvent;
  DWORD           m_dwThreadId;

  REFERENCE_TIME m_rtPlaybackOffset;
  REFERENCE_TIME m_rtSeekPosition;
  REFERENCE_TIME m_rtTitleDuration;
  REFERENCE_TIME m_rtCurrentTime;
  CCritSec       m_csClock;

  // Times
  REFERENCE_TIME m_rtStart;
  REFERENCE_TIME m_rtStop;
  REFERENCE_TIME m_rtCurrent;
  REFERENCE_TIME m_rtNewStart; 
  REFERENCE_TIME m_rtNewStop;
  double m_dRate;

  // Seeking
  REFERENCE_TIME m_rtLastStart;
  REFERENCE_TIME m_rtLastStop;
  std::set<void *> m_lastSeekers;
  bool m_bFirstSeek;

  bool m_bHandleSeekEvent;
  bool m_bForceTitleBasedPlayback;

  bool m_bRebuildOngoing;
  CAMEvent m_eRebuild;

  bool m_pLibPaused;
  bool m_pMadVRPausedInit;
};

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
#include "pcrdecoder.h"
#include "demultiplexer.h"
#include "..\..\DVBSubtitle3\Source\IDVBSub.h"
#include "ISubtitleStream.h"
#include "IAudioStream.h"
#include <bluray.h>
#include "LibBlurayWrapper.h"
#include "BDEventObserver.h"
#include <map>

using namespace std;

class CSubtitlePin;
class CAudioPin;
class CVideoPin;
class CBDReader;
class CBDReaderFilter;

struct OSDTexture;

DEFINE_GUID(CLSID_BDReader, 0x79a37017, 0x3178, 0x4859, 0x80, 0x79, 0xec, 0xb9, 0xd5, 0x46, 0xfe, 0xb2);
DEFINE_GUID(IID_IBDReader, 0x79a37017, 0x3178, 0x4859, 0x80, 0x79, 0xec, 0xb9, 0xd5, 0x46, 0xfe, 0xc2);

DECLARE_INTERFACE_(IBDReaderCallback, IUnknown)
{
	STDMETHOD(OnMediaTypeChanged)(int mediaTypes)PURE;	
	STDMETHOD(OnVideoFormatChanged)(int streamType, int width, int height, int aspectRatioX, int aspectRatioY, int bitrate, int isInterlaced)PURE;
  STDMETHOD(OnBDEvent)(BD_EVENT event)PURE;
  STDMETHOD(OnOSDUpdate)(OSDTexture texture)PURE;
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
    virtual HRESULT STDMETHODCALLTYPE FreeTitleInfo(BLURAY_TITLE_INFO* info) = 0;
    virtual void    STDMETHODCALLTYPE OnGraphRebuild(int info) = 0;
    virtual void    STDMETHODCALLTYPE ForceTitleBasedPlayback(bool force, UINT32 pTitle) = 0;
    virtual void    STDMETHODCALLTYPE SetD3DDevice(IDirect3DDevice9* device) = 0;
    virtual void    STDMETHODCALLTYPE SetBDPlayerSettings(bd_player_settings settings) = 0;
    virtual HRESULT STDMETHODCALLTYPE Start();
    virtual HRESULT STDMETHODCALLTYPE MouseMove(UINT16 x, UINT16 y);
};

class CBDReaderFilter : public CSource, 
                        public IFileSourceFilter, 
                        public IAMFilterMiscFlags, 
                        public IAMStreamSelect, 
                        public ISubtitleStream, 
                        public IAudioStream,
                        public IBDReader,
                        public BDEventObserver
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
  STDMETHODIMP Info( long lIndex, AM_MEDIA_TYPE** ppmt, DWORD* pdwFlags, LCID* plcid, DWORD* pdwGroup, WCHAR** ppszName, IUnknown** ppObject, IUnknown** ppUnk);	

  // IAudioStream
  STDMETHODIMP GetAudioStream(__int32 &stream);

  // ISubtitleStream
  STDMETHODIMP SetSubtitleStream(__int32 stream);
  STDMETHODIMP GetSubtitleStreamType(__int32 stream, int &type);
  STDMETHODIMP GetSubtitleStreamCount(__int32 &count);
  STDMETHODIMP GetCurrentSubtitleStream(__int32 &stream);
  STDMETHODIMP GetSubtitleStreamLanguage(__int32 stream,char* szLanguage);
  STDMETHODIMP SetSubtitleResetCallback( int (CALLBACK *pSubUpdateCallback)(int count, void* opts, int* select)); 

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
  STDMETHODIMP FreeTitleInfo(BLURAY_TITLE_INFO* info);
  STDMETHODIMP Start();
	STDMETHODIMP MouseMove(UINT16 x, UINT16 y);

  void STDMETHODCALLTYPE OnGraphRebuild(int info);
  void STDMETHODCALLTYPE ForceTitleBasedPlayback(bool force, UINT32 pTitle);
  void STDMETHODCALLTYPE SetD3DDevice(IDirect3DDevice9* device);
  void STDMETHODCALLTYPE SetBDPlayerSettings(bd_player_settings settings);

  // BDEventObserver
  void HandleBDEvent(BD_EVENT& pEv, UINT64 pPos);
  void HandleOSDUpdate(OSDTexture& pTexture);
  void HandleMenuStateChange(bool pVisible);

  // IFileSourceFilter
  STDMETHODIMP    Load(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt);
  STDMETHODIMP    GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt);
  STDMETHODIMP    GetDuration(REFERENCE_TIME *pDuration);
  CDeMultiplexer& GetDemultiplexer();
  void            Seek(CRefTime&  seekTime, bool seekInFile);
  void            SeekDone(CRefTime& refTime);
  void            SeekStart();
  void            SeekPreStart(CRefTime& rtSeek);
  CAudioPin*      GetAudioPin();
  CVideoPin*      GetVideoPin();
  CSubtitlePin*   GetSubtitlePin();
  IDVBSubtitle*   GetSubtitleFilter();
  FILTER_STATE    State() {return m_State;}
  void            OnMediaTypeChanged(int mediaTypes);
  void            OnVideoFormatChanged(int streamType, int width, int height, int aspectRatioX, int aspectRatioY, int bitrate, int isInterlaced);
  void            RefreshStreamPosition(CRefTime pPos);

  bool            IsSeeking();
  int             SeekingDone();
  bool            IsStopping();

  DWORD           m_lastPause;

  CLibBlurayWrapper lib;

  bool            m_bStopping;
  int             m_WaitForSeekToEof;
  bool            m_bForceSeekOnStop;
  bool            m_bForceSeekAfterRateChange;
  bool            m_bSeekAfterRcDone;

  bool            m_bStreamCompensated;
  CRefTime        m_rtCompensation;

private:
  HRESULT FindSubtitleFilter();

  CAudioPin*      m_pAudioPin;
  CVideoPin*      m_pVideoPin;
  CSubtitlePin*	  m_pSubtitlePin;
  WCHAR           m_fileName[1024];
  CCritSec        m_section;
  CDeMultiplexer  m_demultiplexer;

  DWORD           m_tickCount;
  CRefTime        m_seekTime;
  CRefTime        m_absSeekTime;
  IBaseFilter*    m_pEvr;
  IDVBSubtitle*   m_pDVBSubtitle;
  IBDReaderCallback* m_pCallback;
  IBDReaderAudioChange* m_pRequestAudioCallback;

  bool            m_bStoppedForUnexpectedSeek;
  DWORD           m_MPmainThreadID;

  IMediaSeeking*  m_pMediaSeeking;

  char            m_pathToBD[MAX_PATH];

  // Seek thread
  static DWORD WINAPI SeekThreadEntryPoint(LPVOID lpParameter);
  DWORD WINAPI        SeekThread();
  
  CRefTime        m_fakeSeekPos;
  HANDLE          m_hSeekThread;
  HANDLE          m_hSeekEvent;
  HANDLE          m_hStopSeekThreadEvent;
  DWORD           m_dwThreadId;
  bool            m_bIgnoreLibSeeking;

  bool            m_bIssueRebuild;
};


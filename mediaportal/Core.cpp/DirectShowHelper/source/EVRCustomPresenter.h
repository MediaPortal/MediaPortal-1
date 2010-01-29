// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#include <queue>
#include <dxva2api.h>
#include <evr.h>

#include "callback.h"
#include "myqueue.h"

using namespace std;
#define CHECK_HR(hr, msg) if (FAILED(hr)) Log(msg);
#define SAFE_RELEASE(p) { if(p) { (p)->Release(); (p)=NULL; } }

#define NUM_SURFACES 5
#define NB_JITTER 125

// magic numbers
#define DEFAULT_FRAME_TIME 200000 // used when fps information is not provided (PAL interlaced == 50fps)

// uncomment the //Log to enable extra logging
#define LOG_TRACE //Log

// Macro for locking 
#define TIME_LOCK(obj, crit, name)  \
  LONGLONG then = GetCurrentTimestamp(); \
  CAutoLock lock(obj); \
  LONGLONG diff = GetCurrentTimestamp() - then; \
  if (diff >= crit) { \
  Log("Critical lock time for %s was %.2f ms", name, (double)diff/10000); \
  }


class MPEVRCustomPresenter;

enum MP_RENDER_STATE
{
	MP_RENDER_STATE_STARTED = 1,
	MP_RENDER_STATE_STOPPED,
	MP_RENDER_STATE_PAUSED,
	MP_RENDER_STATE_SHUTDOWN
};


typedef struct _SchedulerParams
{
	MPEVRCustomPresenter* pPresenter;
	CCritSec csLock;
	CAMEvent eHasWork;
	BOOL bDone;
} SchedulerParams;

class MPEVRCustomPresenter
	: public IMFVideoDeviceID,
	public IMFTopologyServiceLookupClient,
	public IMFVideoPresenter,
	public IMFGetService,
	public IMFAsyncCallback,
	public IQualProp,
	public IMFRateSupport,
	public IMFVideoDisplayControl,
	public IEVRTrustedVideoPlugin,
	public IMFVideoPositionMapper,
	public CCritSec
{

public:
  MPEVRCustomPresenter(IVMR9Callback* callback, IDirect3DDevice9* direct3dDevice,HMONITOR monitor, IBaseFilter* EVRFilter);
  virtual ~MPEVRCustomPresenter();

  //IQualProp (stub)
  virtual HRESULT STDMETHODCALLTYPE get_FramesDroppedInRenderer(int *pcFrames);
  virtual HRESULT STDMETHODCALLTYPE get_FramesDrawn(int *pcFramesDrawn);
  virtual HRESULT STDMETHODCALLTYPE get_AvgFrameRate(int *piAvgFrameRate);
  virtual HRESULT STDMETHODCALLTYPE get_Jitter(int *iJitter);
  virtual HRESULT STDMETHODCALLTYPE get_AvgSyncOffset(int *piAvg);
  virtual HRESULT STDMETHODCALLTYPE get_DevSyncOffset(int *piDev);

  //IMFAsyncCallback
  virtual HRESULT STDMETHODCALLTYPE GetParameters(DWORD *pdwFlags, DWORD *pdwQueue);
  virtual HRESULT STDMETHODCALLTYPE Invoke(IMFAsyncResult *pAsyncResult);

  //IMFGetService
  virtual HRESULT STDMETHODCALLTYPE GetService(REFGUID guidService, REFIID riid, LPVOID *ppvObject);

  //IMFVideoDeviceID
  virtual HRESULT STDMETHODCALLTYPE GetDeviceID(IID* pDeviceID);

  //IMFTopologyServiceLookupClient
  virtual HRESULT STDMETHODCALLTYPE InitServicePointers(IMFTopologyServiceLookup *pLookup);
  virtual HRESULT STDMETHODCALLTYPE ReleaseServicePointers();

  //IMFVideoPresenter
  virtual HRESULT STDMETHODCALLTYPE GetCurrentMediaType(IMFVideoMediaType** ppMediaType);
  virtual HRESULT STDMETHODCALLTYPE ProcessMessage(MFVP_MESSAGE_TYPE eMessage, ULONG_PTR ulParam);

  //IMFClockState
  virtual HRESULT STDMETHODCALLTYPE OnClockStart(MFTIME hnsSystemTime, LONGLONG llClockStartOffset);
  virtual HRESULT STDMETHODCALLTYPE OnClockStop(MFTIME hnsSystemTime);
  virtual HRESULT STDMETHODCALLTYPE OnClockPause(MFTIME hnsSystemTime);
  virtual HRESULT STDMETHODCALLTYPE OnClockRestart(MFTIME hnsSystemTime);
  virtual HRESULT STDMETHODCALLTYPE OnClockSetRate(MFTIME hnsSystemTime, float flRate);

  //IMFRateSupport
  virtual HRESULT STDMETHODCALLTYPE GetSlowestRate(MFRATE_DIRECTION eDirection, BOOL fThin, float *pflRate);
  virtual HRESULT STDMETHODCALLTYPE GetFastestRate(MFRATE_DIRECTION eDirection, BOOL fThin, float *pflRate);
  virtual HRESULT STDMETHODCALLTYPE IsRateSupported(BOOL fThin, float flRate,float *pflNearestSupportedRate);
  virtual HRESULT STDMETHODCALLTYPE GetNativeVideoSize(SIZE *pszVideo, SIZE *pszARVideo);
  virtual HRESULT STDMETHODCALLTYPE GetIdealVideoSize(SIZE *pszMin, SIZE *pszMax);
  virtual HRESULT STDMETHODCALLTYPE SetVideoPosition(const MFVideoNormalizedRect *pnrcSource, const LPRECT prcDest);
  virtual HRESULT STDMETHODCALLTYPE GetVideoPosition(MFVideoNormalizedRect *pnrcSource, LPRECT prcDest);
  virtual HRESULT STDMETHODCALLTYPE SetAspectRatioMode(DWORD dwAspectRatioMode);
  virtual HRESULT STDMETHODCALLTYPE GetAspectRatioMode(DWORD *pdwAspectRatioMode);
  virtual HRESULT STDMETHODCALLTYPE SetVideoWindow(HWND hwndVideo);
  virtual HRESULT STDMETHODCALLTYPE GetVideoWindow(HWND *phwndVideo);
  virtual HRESULT STDMETHODCALLTYPE RepaintVideo(void);
  virtual HRESULT STDMETHODCALLTYPE GetCurrentImage(BITMAPINFOHEADER *pBih, BYTE **pDib, DWORD *pcbDib, LONGLONG *pTimeStamp);
  virtual HRESULT STDMETHODCALLTYPE SetBorderColor(COLORREF Clr);
  virtual HRESULT STDMETHODCALLTYPE GetBorderColor(COLORREF *pClr);
  virtual HRESULT STDMETHODCALLTYPE SetRenderingPrefs(DWORD dwRenderFlags);
  virtual HRESULT STDMETHODCALLTYPE GetRenderingPrefs(DWORD *pdwRenderFlags);
  virtual HRESULT STDMETHODCALLTYPE SetFullscreen(BOOL fFullscreen);
  virtual HRESULT STDMETHODCALLTYPE GetFullscreen(BOOL *pfFullscreen);
  virtual HRESULT STDMETHODCALLTYPE IsInTrustedVideoMode (BOOL *pYes);
  virtual HRESULT STDMETHODCALLTYPE CanConstrict (BOOL *pYes);
  virtual HRESULT STDMETHODCALLTYPE SetConstriction(DWORD dwKPix);
  virtual HRESULT STDMETHODCALLTYPE DisableImageExport(BOOL bDisable);

 // IMFVideoPositionMapper 
  virtual HRESULT STDMETHODCALLTYPE MapOutputCoordinateToInputStream(float xOut,float yOut,DWORD dwOutputStreamIndex,DWORD dwInputStreamIndex,float* pxIn,float* pyIn);

 // IUnknown
  virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppvObject);
  virtual ULONG STDMETHODCALLTYPE AddRef();
  virtual ULONG STDMETHODCALLTYPE Release();

  HRESULT        CheckForScheduledSample(LONGLONG *pNextSampleTime, REFERENCE_TIME hnsLastSleepTime);
  BOOL           CheckForInput();
  HRESULT        ProcessInputNotify(int* samplesProcessed);
  void           SetFrameSkipping(bool onOff);
  REFERENCE_TIME GetFrameDuration();
  double         GetRefreshRate();
  double         GetDisplayCycle();
  double         GetCycleDifference();
  double         GetDetectedFrameTime();

  // Settings
  void           EnableDrawStats(bool enable);
  void           ResetEVRStatCounters();
  void           ResetTraceStats(); // Reset all tracing stats

  void           NotifyRateChange(double pRate);
  void           NotifyDVDMenuState(bool pIsInMenu);

friend class StatsRenderer;

protected:
  void           EstimateRefreshTimings();
  bool           ImmediateCheckForInput();
  void           LogStats();
  void           ReleaseSurfaces();
  HRESULT        Paint(CComPtr<IDirect3DSurface9> pSurface);
  HRESULT        SetMediaType(CComPtr<IMFMediaType> pType, BOOL* pbHasChanged);
  void           ReAllocSurfaces();
  HRESULT        LogOutputTypes();
  HRESULT        GetAspectRatio(CComPtr<IMFMediaType> pType, int* piARX, int* piARY);
  HRESULT        CreateProposedOutputType(IMFMediaType* pMixerType, IMFMediaType** pType);
  HRESULT        RenegotiateMediaOutputType();
  BOOL           CheckForEndOfStream();
  void           StartWorkers();
  void           StopWorkers();
  void           StartThread(PHANDLE handle, SchedulerParams* pParams, UINT (CALLBACK *ThreadProc)(void*), UINT* threadId, int threadPriority);
  void           EndThread(HANDLE hThread, SchedulerParams* params);
  void           NotifyThread(SchedulerParams* params);
  void           NotifyScheduler();
  void           NotifyWorker();
  HRESULT        GetTimeToSchedule(IMFSample* pSample, LONGLONG* pDelta);
  void           Flush();
  void           ScheduleSample(IMFSample* pSample);
  IMFSample*     PeekSample();
  BOOL           PopSample();
  HRESULT        TrackSample(IMFSample *pSample);
  HRESULT        GetFreeSample(IMFSample** ppSample);
  void           ReturnSample(IMFSample* pSample, BOOL tryNotify);
  void           ResetStatistics();
  HRESULT        PresentSample(IMFSample* pSample);
  void           CorrectSampleTime(IMFSample* pSample);
  void           GetRefreshRateDwm();

  CComPtr<IDirect3DTexture9>        m_pVideoTexture;
  CComPtr<IDirect3DSurface9>        m_pVideoSurface;
  CComPtr<IDirect3DDevice9>         m_pD3DDev;
  IVMR9Callback*                    m_pCallback;
  CComPtr<IDirect3DDeviceManager9>  m_pDeviceManager;
  CComPtr<IMediaEventSink>          m_pEventSink;
  CComPtr<IMFClock>                 m_pClock;
  CComPtr<IMFTransform>             m_pMixer;
  CComPtr<IMFMediaType>             m_pMediaType;
  CComPtr<IDirect3DTexture9>        textures[NUM_SURFACES];
  CComPtr<IDirect3DSurface9>        surfaces[NUM_SURFACES];
  CComPtr<IMFSample>                samples[NUM_SURFACES];
  CCritSec                          m_lockSamples;
  CCritSec                          m_lockScheduledSamples;
  int                               m_iFreeSamples;
  IMFSample*                        m_vFreeSamples[NUM_SURFACES];
  CMyQueue<IMFSample*>              m_qScheduledSamples;
  SchedulerParams                   m_schedulerParams;
  SchedulerParams                   m_workerParams;
  BOOL                              m_bSchedulerRunning;
  HANDLE                            m_hScheduler;
  HANDLE                            m_hWorker;
  UINT                              m_uSchedulerThreadId;
  UINT                              m_uWorkerThreadId;
  UINT                              m_iResetToken;
  float                             m_fRate;
  long                              m_refCount;
  HMONITOR                          m_hMonitor;
  int                               m_iVideoWidth;
  int                               m_iVideoHeight;
  int                               m_iARX;
  int                               m_iARY;
  BOOL                              m_bInputAvailable;
  BOOL                              m_bEndStreaming;
  BOOL                              m_bFlush;
  int                               m_iFramesDrawn;
  int                               m_iFramesDropped;
  bool                              m_bFrameSkipping;
  double                            m_fSeekRate;
  bool                              m_bScrubbing;
  bool                              m_bFirstFrame;
  bool                              m_bDVDMenu;
  MP_RENDER_STATE                   m_state;
  double                            m_fAvrFps;						            // Estimate the real FPS
  LONGLONG                          m_pllJitter [NB_JITTER];		      // Jitter buffer for stats
  LONGLONG                          m_llLastPerf;
  int                               m_nNextJitter;
  REFERENCE_TIME                    m_rtTimePerFrame;
  LONGLONG                          m_llLastWorkerNotification;

  int       m_nNextSyncOffset;
  LONGLONG  nsSampleTime;

	double    m_fJitterStdDev;				// Estimate the Jitter std dev
	double    m_fJitterMean;
	double    m_fSyncOffsetStdDev;
	double    m_fSyncOffsetAvr;
	double    m_DetectedRefreshRate;

  LONGLONG  m_MaxJitter;
  LONGLONG  m_MinJitter;
  LONGLONG  m_MaxSyncOffset;
  LONGLONG  m_MinSyncOffset;
  LONGLONG  m_pllSyncOffset [NB_JITTER];		// Jitter buffer for stats
  unsigned long m_uSyncGlitches;

	LONGLONG  m_PaintTimeMin;
	LONGLONG  m_PaintTimeMax;
  LONGLONG	m_PaintTime;

  bool      m_bResetStats;
  bool      m_bDrawStats;

  D3DDISPLAYMODE  m_displayMode;
  double          m_dD3DRefreshRate;
  double          m_dD3DRefreshCycle;

  // Functions to trace timing performance
  void OnVBlankFinished(bool fAll, LONGLONG periodStart, LONGLONG periodEnd);
  void CalculateJitter(LONGLONG PerfCounter);

  double m_dDetectedScanlineTime;
  double m_dEstRefreshCycle; 
  double m_dOptimumDisplayCycle;
  double m_dFrameCycle;
  double m_dCycleDifference;
  double m_rasterSyncOffset;
  double m_pllRasterSyncOffset[NB_JITTER];

  StatsRenderer* m_pStatsRenderer; 

  // dshowhelper owns this
  IBaseFilter*  m_EVRFilter;

  // Used for detecting the real frame duration
  LONGLONG      m_LastScheduledUncorrectedSampleTime;
  double        m_LastScheduledSampleTimeFP;
  LONGLONG      m_DetectedFrameTimeHistory[30];
  double        m_DetectedFrameTimeHistoryHistory[100];
  int           m_DetectedFrameTimePos;
  double        m_DetectedFrameRate;
  double        m_DetectedFrameTime;
  double        m_DetectedFrameTimeStdDev;
  bool          m_bCorrectedFrameTime;
  bool          m_DetectedLock;
};

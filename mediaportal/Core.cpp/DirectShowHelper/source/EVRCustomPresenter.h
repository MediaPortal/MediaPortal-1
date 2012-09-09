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

#include "stdafx.h"

#include <queue>
#include <dxva2api.h>
#include <evr.h>
#include <Mferror.h>
#include "OuterEVR.h"
#include "IAVSyncClock.h"
#include "callback.h"
#include "myqueue.h"

using namespace std;
#define CHECK_HR(hr, msg) if (FAILED(hr)) Log(msg);
#define SAFE_RELEASE(p) { if(p) { (p)->Release(); (p)=NULL; } }

//Disables MP audio renderer functions if true
#define NO_MP_AUD_REND false

#define NUM_SURFACES 5
#define NB_JITTER 125
#define NB_RFPSIZE 64
#define NB_DFTHSIZE 64
#define NB_CFPSIZE 16
#define NB_PCDSIZE 32
#define LF_THRESH_LOW 3
#define LF_THRESH (LF_THRESH_LOW + 1)
#define LF_THRESH_HIGH (LF_THRESH + 3)
#define FRAME_PROC_THRESH 30
#define FRAME_PROC_THRSH2 60
#define DFT_THRESH 0.007
#define NUM_PHASE_DEVIATIONS 32
#define FILTER_LIST_SIZE 9

// magic numbers
#define DEFAULT_FRAME_TIME 200000 // used when fps information is not provided (PAL interlaced == 50fps)

// uncomment the //Log to enable extra logging
#define LOG_TRACE //Log

// Change to 'true' to enable extra logging of processing latencies
#define LOG_DELAYS false

// uncomment the //Log to enable extra logging
#define LOG_LATEFR //Log

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

enum FPS_SOURCE_METHOD
{
  FPS_SOURCE_ADAPTIVE = 0,
  FPS_SOURCE_SAMPLE_TIMESTAMP,
  FPS_SOURCE_SAMPLE_DURATION,
  FPS_SOURCE_EVR_MIXER
};

typedef struct 
{
  UINT     maxScanLine;
  UINT     minVisScanLine;
  UINT     maxVisScanLine;    
  double   dDetectedScanlineTime;
  double   dEstRefreshCycle; 
  bool     estRefreshLock;
} DisplayParams;

typedef struct _SchedulerParams
{
  MPEVRCustomPresenter* pPresenter;
  CCritSec csLock;
  CAMEvent eHasWork;   //Urgent event
  CAMEvent eHasWorkLP; //Low-priority event
  CAMEvent eTimerEnd;  //Timer thread event
  BOOL bDone;
  long iPause;
  BOOL bPauseAck;
  LONGLONG llTime;     //Timer target time
} SchedulerParams;

class MPEVRCustomPresenter : 
  public CUnknown, 
  public IMFVideoDeviceID,
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
  MPEVRCustomPresenter(IVMR9Callback* pCallback, IDirect3DDevice9* direct3dDevice, HMONITOR monitor, IBaseFilter** EVRFilter, BOOL pIsWin7);
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
  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);
  HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppvObject);
  ULONG STDMETHODCALLTYPE AddRef();
  ULONG STDMETHODCALLTYPE Release();
  ULONG STDMETHODCALLTYPE NonDelegatingAddRef();
  ULONG STDMETHODCALLTYPE NonDelegatingRelease();

  // IBaseFilter delegate
  bool GetState( DWORD dwMilliSecsTimeout, FILTER_STATE* State, HRESULT& pReturnValue);

  HRESULT        CheckForScheduledSample(LONGLONG *pTargetTime, LONGLONG lastSleepTime, BOOL *idleWait);
  BOOL           CheckForInput(bool setInAvail);
  HRESULT        ProcessInputNotify(int* samplesProcessed,  bool setInAvail);
  void           SetFrameSkipping(bool onOff);
  REFERENCE_TIME GetFrameDuration();
  double         GetRefreshRate();
  double         GetDisplayCycle();
  double         GetCycleDifference();
  double         GetDetectedFrameTime();
  double         GetRealFramePeriod();
  double         GetVideoFramePeriod(FPS_SOURCE_METHOD fpsSource);
  void           GetFrameRateRatio();

  void           NotifyTimer(LONGLONG targetTime);
  void           NotifySchedulerTimer();

  // Release EVR callback (C# side)
  void           ReleaseCallback();

  // Settings
  void           EnableDrawStats(bool enable);
  void           ResetEVRStatCounters();
  void           ResetTraceStats(); // Reset tracing stats
  void           ResetFrameStats(); // Reset frame stats
  
  void           NotifyRateChange(double pRate);
  void           NotifyDVDMenuState(bool pIsInMenu);
  void           UpdateDisplayFPS();

  bool           m_bScrubbing;
  bool           m_bZeroScrub;

friend class StatsRenderer;

protected:
  void           GetAVSyncClockInterface();
  void           SetupAudioRenderer();
  void           AdjustAVSync(double currentPhaseDiff);
  int            MeasureScanLines(LONGLONG startTime, double *times, double *scanLines, int n, UINT* maxScanLine);
  BOOL           EstimateRefreshTimings(int numFrames, int threadPriority);
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
  void           PauseThread(HANDLE hThread, SchedulerParams* params);
  void           WakeThread(HANDLE hThread, SchedulerParams* params);
  void           NotifyThread(SchedulerParams* params, bool setWork, bool setWorkLP, LONGLONG llTime);
  void           NotifyScheduler(bool forceWake);
  void           NotifyWorker(bool setInAvail);
  HRESULT        GetTimeToSchedule(IMFSample* pSample, LONGLONG* pDelta, LONGLONG *hnsSystemTime);
  void           Flush(BOOL forced);
  void           DoFlush(BOOL forced);
  void           ScheduleSample(IMFSample* pSample);
  IMFSample*     PeekSample();
  BOOL           PopSample();
  int            CheckQueueCount();
  HRESULT        TrackSample(IMFSample *pSample);
  HRESULT        GetFreeSample(IMFSample** ppSample);
  void           ReturnSample(IMFSample* pSample, BOOL tryNotify);
  HRESULT        PresentSample(IMFSample* pSample);
  void           VideoFpsFromSample(IMFSample* pSample);
  void           GetRealRefreshRate();
  LONGLONG       GetDelayToRasterTarget(LONGLONG *targetTime, LONGLONG *offsetTime);
  void           DwmEnableMMCSSOnOff(bool enable);
  bool           BufferMoreSamples();

  HRESULT EnumFilters(IFilterGraph *pGraph);
  bool GetFilterNames();

  CComPtr<IDirect3DDevice9>         m_pD3DDev;
  IVMR9Callback*                    m_pCallback;
  CComPtr<IDirect3DDeviceManager9>  m_pDeviceManager;
  CComPtr<IMediaEventSink>          m_pEventSink;
  CComPtr<IMFClock>                 m_pClock;
  CComPtr<IMFTransform>             m_pMixer;
  CComPtr<IMFMediaType>             m_pMediaType;
  CComPtr<IMediaSeeking>            m_pMediaSeeking;
  CComPtr<IDirect3DTexture9>        textures[NUM_SURFACES];
  CComPtr<IDirect3DSurface9>        surfaces[NUM_SURFACES];
  CComPtr<IMFSample>                samples[NUM_SURFACES];
  CCritSec                          m_lockSamples;
  CCritSec                          m_lockScheduledSamples;
  CCritSec                          m_lockRasterData;
  CCritSec                          m_lockRefreshEstimator;
  CCritSec                          m_lockCallback;
  int                               m_iFreeSamples;
  IMFSample*                        m_vFreeSamples[NUM_SURFACES];
  CMyQueue<IMFSample*>              m_qScheduledSamples;
  SchedulerParams                   m_schedulerParams;
  SchedulerParams                   m_workerParams;
  SchedulerParams                   m_timerParams;
  BOOL                              m_bSchedulerRunning;
  HANDLE                            m_hScheduler;
  HANDLE                            m_hWorker;
  HANDLE                            m_hTimer;
  UINT                              m_uSchedulerThreadId;
  UINT                              m_uWorkerThreadId;
  UINT                              m_uTimerThreadId;
  UINT                              m_iResetToken;
  float                             m_fRate;
  long                              m_refCount;
  HMONITOR                          m_hMonitor;
  int                               m_iVideoWidth;
  int                               m_iVideoHeight;
  int                               m_iARX;
  int                               m_iARY;
  BOOL                              m_bInputAvailable;
  BOOL                              m_bFirstInputNotify;
  BOOL                              m_bEndStreaming;
  bool                              m_bEndBuffering;
  bool                              m_bNewSegment;
  bool                              m_bFlush;
  bool                              m_bDoPreBuffering;
  int                               m_iFramesDrawn;
  int                               m_iFramesDropped;
  bool                              m_bFrameSkipping;
  double                            m_fSeekRate;
//  bool                              m_bScrubbing;
//  bool                              m_bZeroScrub;
  bool                              m_bFirstFrame;
  bool                              m_bDVDMenu;
  MP_RENDER_STATE                   m_state;
  double                            m_fAvrFps;						            // Estimate the real FPS
  LONGLONG                          m_pllJitter [NB_JITTER];		      // Jitter buffer for stats
  LONGLONG                          m_llLastPerf;
  int                               m_nNextJitter;
  REFERENCE_TIME                    m_rtTimePerFrame;
  LONGLONG                          m_llLastWorkerNotification;

  LONGLONG                          m_pllRFP [NB_RFPSIZE];   // timestamp buffer for estimating real frame period
  LONGLONG                          m_llLastRFPts;
  int                               m_nNextRFP;
  double                            m_fRFPStdDev;				// Estimate the real frame period std dev
  double                            m_fRFPMean;

  LONGLONG                          m_pllCFP [NB_CFPSIZE];   // timestamp buffer for estimating real frame period
  LONGLONG                          m_llLastCFPts;
  int                               m_nNextCFP;
  LONGLONG                          m_fCFPMean;
  LONGLONG                          m_llCFPSumAvg;

  double                            m_pllPCD [NB_PCDSIZE];   // timestamp buffer for estimating pres/sys clock delta
  LONGLONG                          m_llLastPCDprsTs;
  LONGLONG                          m_llLastPCDsysTs;
  int                               m_nNextPCD;
  double                            m_fPCDMean;
  double                            m_fPCDSumAvg;
	
	
  int                               m_iFramesHeld;
  int                               m_iLateFrames;
  int                               m_iFramesProcessed;
 

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
  void CalculateRealFramePeriod(LONGLONG timeStamp);
  void CalculateNSTStats(LONGLONG timeStamp);
  void CalculatePresClockDelta(LONGLONG presTime, LONGLONG sysTime);

  bool QueryFpsFromVideoMSDecoder();
  bool ExtractAvgTimePerFrame(const AM_MEDIA_TYPE* pmt, REFERENCE_TIME& rtAvgTimePerFrame);

  double m_dOptimumDisplayCycle;
  double m_dFrameCycle;
  double m_dCycleDifference;
  double m_rasterSyncOffset;
  double m_pllRasterSyncOffset[NB_JITTER];
  UINT   m_LastStartOfPaintScanline;
  UINT   m_LastEndOfPaintScanline;

  DisplayParams m_displayParams;

  UINT   m_rasterLimitLow; 
  UINT   m_rasterTargetPosn;
  UINT   m_rasterLimitHigh;
  UINT   m_rasterLimitTop;
  UINT   m_rasterLimitNP;

  double m_dEstRefCycDiff; 
  
  LONGLONG m_SampDuration;

  StatsRenderer* m_pStatsRenderer; 

  IBaseFilter*  m_EVRFilter;

  // Used for detecting the real frame duration
  LONGLONG      m_LastScheduledUncorrectedSampleTime;
  LONGLONG      m_DetectedFrameTimeHistory[NB_DFTHSIZE];
  LONGLONG      m_DectedSum;
  int           m_DetectedFrameTimePos;
  double        m_DetectedFrameTime;
  double        m_DetdFrameTimeLast;
  double        m_DetectedFrameTimeStdDev;
  bool          m_DetectedLock;
  double        m_DetFrameTimeAve;

  // Used for detecting the average video sample duration
  LONGLONG      m_DetSampleHistory[NB_DFTHSIZE];
  LONGLONG      m_DetSampleSum;
  double        m_DetSampleAve;

  int           m_frameRateRatio;
  int           m_rawFRRatio;
  
  int           m_qGoodPopCnt;
  int           m_qBadPopCnt;
  int           m_qGoodPutCnt;
  int           m_qBadPutCnt;
  int           m_qBadSampTimCnt;
  int           m_qCorrSampTimCnt;
  
  LONGLONG      m_stallTime;
  LONGLONG      m_earliestPresentTime;
  LONGLONG      m_lastPresentTime;
  LONGLONG      m_lastDelayErr;

  char          m_filterNames[FILTER_LIST_SIZE][MAX_FILTER_NAME];
  int           m_numFilters;
  
  BOOL          m_bIsWin7;
  bool          m_bMsVideoCodec;
  
  IAVSyncClock* m_pAVSyncClock;
  double        m_dBias;
  double        m_dMaxBias;
  double        m_dMinBias;
  bool          m_bBiasAdjustmentDone;
  double        m_dPhaseDeviations[NUM_PHASE_DEVIATIONS];
  int           m_nNextPhDev;
  double        m_sumPhaseDiff;
  double        m_dVariableFreq;
  double        m_dPreviousVariableFreq;
  unsigned int  m_iClockAdjustmentsDone;
  double        m_avPhaseDiff;

  COuterEVR*    m_pOuterEVR;

  LONGLONG      m_streamDuration;

  CAMEvent      m_SampleAddedEvent;
  CAMEvent      m_EndOfStreamingEvent;

  CAMEvent      m_bFlushDone;

  // CheckShutdown: 
  //     Returns MF_E_SHUTDOWN if the presenter is shutdown.
  //     Call this at the start of any methods that should fail after shutdown.
  inline HRESULT CheckShutdown() const 
  {
    if (m_state == MP_RENDER_STATE_SHUTDOWN)
      return MF_E_SHUTDOWN;
    else
      return S_OK;
  }

  // IsActive: The "active" state is started or paused.
  inline BOOL IsActive() const
  {
    return ((m_state == MP_RENDER_STATE_STARTED) || (m_state == MP_RENDER_STATE_PAUSED));
  }
};

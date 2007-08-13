#pragma once

#pragma warning(push, 2)
#pragma warning(disable : 4995)

#include <vector>
#include <queue>
#include <dxva2api.h>
#include "callback.h"
#pragma warning(pop)
using namespace std;

#define CHECK_HR(hr, msg) if ( FAILED(hr) ) Log( msg );
#define SAFE_RELEASE(p)      { if(p) { (p)->Release(); (p)=NULL; } }

#define NUM_SURFACES 5

class EVRCustomPresenter;

enum RENDER_STATE
{
	RENDER_STATE_STARTED,
	RENDER_STATE_STOPPED,
	RENDER_STATE_PAUSED,
	RENDER_STATE_SHUTDOWN
};

typedef struct _SchedulerParams
{
	EVRCustomPresenter* pPresenter;
	CCritSec csLock;
	CAMEvent eHasWork;
	BOOL bDone;
	int iTimerSet;
} SchedulerParams;

class EVRCustomPresenter
	: public IMFVideoDeviceID,
	public IMFTopologyServiceLookupClient,
	public IMFVideoPresenter,
	public IMFGetService,
	public IMFAsyncCallback,
	public IQualProp,
	public IMFRateSupport,
	public CCritSec
{

public:
	EVRCustomPresenter(IVMR9Callback* callback, IDirect3DDevice9* direct3dDevice,HMONITOR monitor);
    virtual ~EVRCustomPresenter();
	//IQualProp (stub)
    virtual HRESULT STDMETHODCALLTYPE get_FramesDroppedInRenderer(int *pcFrames);
    virtual HRESULT STDMETHODCALLTYPE get_FramesDrawn(int *pcFramesDrawn) ;     
    virtual HRESULT STDMETHODCALLTYPE get_AvgFrameRate(int *piAvgFrameRate);    
    virtual HRESULT STDMETHODCALLTYPE get_Jitter(int *iJitter) ;     
    virtual HRESULT STDMETHODCALLTYPE get_AvgSyncOffset(int *piAvg) ;
    virtual HRESULT STDMETHODCALLTYPE get_DevSyncOffset(int *piDev) ;

	//IMFAsyncCallback
    virtual HRESULT STDMETHODCALLTYPE GetParameters( 
        /* [out] */ __RPC__out DWORD *pdwFlags,
        /* [out] */ __RPC__out DWORD *pdwQueue);
    
    virtual HRESULT STDMETHODCALLTYPE Invoke( 
        /* [in] */ __RPC__in_opt IMFAsyncResult *pAsyncResult);

	//IMFGetService
	virtual HRESULT STDMETHODCALLTYPE GetService( 
            /* [in] */  REFGUID guidService,
            /* [in] */  REFIID riid,
            /* [iid_is][out] */ LPVOID *ppvObject);


	//IMFVideoDeviceID
	virtual HRESULT STDMETHODCALLTYPE GetDeviceID(IID* pDeviceID);

	//IMFTopologyServiceLookupClient
	virtual HRESULT STDMETHODCALLTYPE InitServicePointers(IMFTopologyServiceLookup *pLookup);
	virtual HRESULT STDMETHODCALLTYPE ReleaseServicePointers();

	//IMFVideoPresenter
	virtual HRESULT STDMETHODCALLTYPE GetCurrentMediaType(IMFVideoMediaType** ppMediaType);
	virtual HRESULT STDMETHODCALLTYPE ProcessMessage( 
            MFVP_MESSAGE_TYPE eMessage,
            ULONG_PTR ulParam);

	//IMFClockState
        virtual HRESULT STDMETHODCALLTYPE OnClockStart( 
            /* [in] */ MFTIME hnsSystemTime,
            /* [in] */ LONGLONG llClockStartOffset);
        
        virtual HRESULT STDMETHODCALLTYPE OnClockStop( 
            /* [in] */ MFTIME hnsSystemTime);
        
        virtual HRESULT STDMETHODCALLTYPE OnClockPause( 
            /* [in] */ MFTIME hnsSystemTime);
        
        virtual HRESULT STDMETHODCALLTYPE OnClockRestart( 
            /* [in] */ MFTIME hnsSystemTime);
        
        virtual HRESULT STDMETHODCALLTYPE OnClockSetRate( 
            /* [in] */ MFTIME hnsSystemTime,
            /* [in] */ float flRate);


	//IMFRateSupport
    virtual HRESULT STDMETHODCALLTYPE GetSlowestRate( 
        /* [in] */ MFRATE_DIRECTION eDirection,
        /* [in] */ BOOL fThin,
		/* [out] */ float *pflRate);
    
    virtual HRESULT STDMETHODCALLTYPE GetFastestRate( 
        /* [in] */ MFRATE_DIRECTION eDirection,
        /* [in] */ BOOL fThin,
        /* [out] */ float *pflRate);
    
    virtual HRESULT STDMETHODCALLTYPE IsRateSupported( 
        /* [in] */ BOOL fThin,
        /* [in] */ float flRate,
        /* [unique][out][in] */ float *pflNearestSupportedRate);
    

    // IUnknown
    virtual HRESULT STDMETHODCALLTYPE QueryInterface( 
        REFIID riid,
        void** ppvObject);

    virtual ULONG STDMETHODCALLTYPE AddRef();
    virtual ULONG STDMETHODCALLTYPE Release();

	//Local
	void ReleaseCallBack();

	HRESULT CheckForScheduledSample(LONGLONG *pNextSampleTime, DWORD msLastSleepTime);
	//returns true if there was some input to be processed
	BOOL CheckForInput();
	HRESULT ProcessInputNotify();
protected:
	void ReleaseSurfaces();
	void Paint(CComPtr<IDirect3DSurface9> pSurface);
	HRESULT SetMediaType(CComPtr<IMFMediaType> pType);
	void ReAllocSurfaces();
	HRESULT LogOutputTypes();
	HRESULT GetAspectRatio(CComPtr<IMFMediaType> pType, int* piARX, int* piARY);
	HRESULT CreateProposedOutputType(IMFMediaType* pMixerType, IMFMediaType** pType);
	HRESULT RenegotiateMediaOutputType();
	BOOL CheckForEndOfStream();
	void	StartWorkers();
	void	StopWorkers();
	void	StartThread(PHANDLE handle, SchedulerParams** ppParams,
					UINT (CALLBACK *ThreadProc)(void*), UINT* threadId);
	void	EndThread(HANDLE hThread, SchedulerParams* params);
	void    NotifyThread(SchedulerParams* params);
	void    NotifyScheduler();
	void    NotifyWorker();
	HRESULT GetTimeToSchedule(CComPtr<IMFSample> pSample, LONGLONG* pDelta);
	void  Flush();
	void ScheduleSample(CComPtr<IMFSample> pSample);
	HRESULT TrackSample(IMFSample *pSample);
	HRESULT GetFreeSample(CComPtr<IMFSample>& ppSample);
	void	ReturnSample(CComPtr<IMFSample> pSample);
	HRESULT PresentSample(CComPtr<IMFSample> pSample);
	void    ResetStatistics();

    CComPtr<IDirect3DDevice9> m_pD3DDev;
	IVMR9Callback* m_pCallback;
	CComPtr<IDirect3DDeviceManager9> m_pDeviceManager;
	CComPtr<IMediaEventSink> m_pEventSink;
	CComPtr<IMFClock> m_pClock;
	CComPtr<IMFTransform> m_pMixer;
	CComPtr<IMFMediaType> m_pMediaType;
	CComPtr<IDirect3DSwapChain9> chains[NUM_SURFACES];
	CComPtr<IDirect3DTexture9> textures[NUM_SURFACES];
	CComPtr<IDirect3DSurface9> surfaces[NUM_SURFACES];
	CComPtr<IMFSample> samples[NUM_SURFACES];
	CCritSec m_lockSamples;
	vector<CComPtr<IMFSample>> m_vFreeSamples;
	queue<CComPtr<IMFSample>> m_vScheduledSamples;
	SchedulerParams *m_schedulerParams;
	SchedulerParams *m_workerParams;
	BOOL		  m_bSchedulerRunning;
	HANDLE		  m_hScheduler;
	HANDLE		  m_hWorker;
	UINT		  m_uSchedulerThreadId;
	UINT		  m_uWorkerThreadId;
	UINT          m_iResetToken;
	float		  m_fRate;
	long		  m_refCount;
	//int			  m_surfaceCount;
	HMONITOR	  m_hMonitor;
	int   m_iVideoWidth, m_iVideoHeight;
	int   m_iARX, m_iARY;
	double m_fps ;
	BOOL		m_bfirstFrame;
	BOOL		m_bfirstInput;
	BOOL		m_bInputAvailable;
	LONG		m_lInputAvailable;
	BOOL		m_bendStreaming;
	int m_iFramesDrawn, m_iFramesDropped, m_iJitter;
	LONGLONG m_hnsLastFrameTime, m_hnsTotalDiff;
	RENDER_STATE m_state;
};

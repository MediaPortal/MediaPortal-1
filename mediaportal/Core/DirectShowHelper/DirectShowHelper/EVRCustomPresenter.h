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

#define NUM_SURFACES 6

class EVRCustomPresenter;

typedef struct _SchedulerParams
{
	EVRCustomPresenter* pPresenter;
	CCritSec csLock;
	CAMEvent eHasWork;
	BOOL bDone;
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
    void UseOffScreenSurface(bool yesNo);
  bool IsInstalled();
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

	HRESULT CheckForScheduledSample(LONGLONG *pNextSampleTime);
protected:
	void Paint(IDirect3DSurface9* pSurface);
	void DeleteSurfaces();
	void ReAllocSurfaces();
	HRESULT RenegotiateMediaInputType();
	HRESULT RenegotiateMediaOutputType();
	HRESULT ProcessInputNotify();
	void	StartScheduler();
	void	StopScheduler();
	void    NotifyScheduler();
	HRESULT GetTimeToSchedule(IMFSample* pSample, LONGLONG* pDelta);
	void  Flush();
	void ScheduleSample(IMFSample* pSample);
	HRESULT TrackSample(IMFSample *pSample);
	HRESULT GetFreeSample(IMFSample **ppSample);
	void	ReturnSample(IMFSample *pSample, BOOL bCheckForWork);
	HRESULT PresentSample(IMFSample *pSample);

    CComPtr<IDirect3DDevice9> m_pD3DDev;
	IVMR9Callback* m_pCallback;
	CComPtr<IDirect3DDeviceManager9> m_pDeviceManager;
	IMediaEventSink* m_pEventSink;
	IMFClock* m_pClock;
	IMFTransform* m_pMixer;
	IMFMediaType* m_pMediaType;
	IDirect3DSwapChain9* chains[NUM_SURFACES];
	IDirect3DSurface9* surfaces[NUM_SURFACES];
	IMFSample* samples[NUM_SURFACES];
	vector<IMFSample*> m_vFreeSamples;
	queue<IMFSample*> m_vScheduledSamples;
	SchedulerParams *m_schedulerParams;
	BOOL		  m_bSchedulerRunning;
	HANDLE		  m_hScheduler;
	UINT		  m_uThreadId;
	UINT          m_iResetToken;
	float		  m_fRate;
	long		  m_refCount;
	int			  m_surfaceCount;
	HMONITOR	  m_hMonitor;
	int   m_iVideoWidth, m_iVideoHeight;
	int   m_iARX, m_iARY;
	double m_fps ;
	BOOL		m_bfirstFrame;
	BOOL		m_bfirstInput;
  HMODULE m_hModuleMF;

  typedef HRESULT __stdcall TMFGetService(IUnknown* punkObject,REFGUID guidService,REFIID riid,LPVOID* ppvObject);
	TMFGetService* m_pMFGetService;
};

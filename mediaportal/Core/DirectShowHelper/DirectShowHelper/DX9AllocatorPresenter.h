#pragma once
#include "DirectShowHelper.h"

class CVMR9AllocatorPresenter
	: public CUnknown
	, public CCritSec
	, public IVMRSurfaceAllocator9
	, public IVMRImagePresenter9
{

public:
	CVMR9AllocatorPresenter(IDirect3DDevice9* direct3dDevice, IVMRSurfaceAllocatorNotify9* vmr9Filter,IVMR9Callback* callback,HMONITOR monitor);

	DECLARE_IUNKNOWN
    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);

    // IVMRSurfaceAllocator9
    STDMETHODIMP InitializeDevice(DWORD_PTR dwUserID, VMR9AllocationInfo* lpAllocInfo, DWORD* lpNumBuffers);
    STDMETHODIMP TerminateDevice(DWORD_PTR dwID);
    STDMETHODIMP GetSurface(DWORD_PTR dwUserID, DWORD SurfaceIndex, DWORD SurfaceFlags, IDirect3DSurface9** lplpSurface);
    STDMETHODIMP AdviseNotify(IVMRSurfaceAllocatorNotify9* lpIVMRSurfAllocNotify);

    // IVMRImagePresenter9
    STDMETHODIMP StartPresenting(DWORD_PTR dwUserID);
    STDMETHODIMP StopPresenting(DWORD_PTR dwUserID);
    STDMETHODIMP PresentImage(DWORD_PTR dwUserID, VMR9PresentationInfo* lpPresInfo);

	void DrawTexture(FLOAT fx, FLOAT fy, FLOAT nw, FLOAT nh, FLOAT uoff, FLOAT voff, FLOAT umax, FLOAT vmax, long color);
	CSize GetVideoSize(bool fCorrectAR=true);
	void Deinit();

protected:
	STDMETHODIMP_(bool) Paint(bool fAll);

	void DeleteSurfaces();
	virtual HRESULT AllocSurfaces();

	CComPtr<IVMRSurfaceAllocatorNotify9> m_pIVMRSurfAllocNotify;
	CComPtr<IDirect3DTexture9> m_pVideoTexture;
	CComPtr<IDirect3DSurface9> m_pVideoSurface;
    CComPtr<IDirect3DDevice9> m_pD3DDev;
	CComPtr<IDirect3D9> m_pD3D;
	CComPtr<IDirect3DSurface9> m_pVideoSurfaceOff;
	CComPtr<IDirect3DSurface9> m_pVideoSurfaceYUY2;
	
	IDirect3DSurface9* m_pSurfaces[20];
	int			  m_surfaceCount;
	HMONITOR	  m_hMonitor;
	IVMR9Callback* m_pCallback;
	CSize m_NativeVideoSize;
	CSize m_AspectRatio ;
	CRect m_WindowRect;
	CRect m_VideoRect;
	bool m_fVMRSyncFix;
	double m_fps ;
	D3DTEXTUREFILTERTYPE m_Filter;
};

#pragma once
#include "stdafx.h"
#pragma warning(push, 2)
#pragma warning(disable : 4995)

#include <vector>
#pragma warning(pop)
using namespace std;

DECLARE_INTERFACE_(IVMR9Callback, IUnknown)
{
	STDMETHOD(PresentImage)  (THIS_ WORD cx, WORD cy, WORD arx, WORD ary, DWORD dwImg)PURE;
	STDMETHOD(PresentSurface)(THIS_ WORD cx, WORD cy, WORD arx, WORD ary, DWORD dwImg)PURE;
};

class CVMR9AllocatorPresenter
	: public CCritSec
	, public IVMRSurfaceAllocator9
	, public IVMRImagePresenter9
{

public:
	CVMR9AllocatorPresenter(IDirect3DDevice9* direct3dDevice,IVMR9Callback* callback,HMONITOR monitor);
    virtual ~CVMR9AllocatorPresenter();

    // IVMRSurfaceAllocator9
    virtual HRESULT STDMETHODCALLTYPE  InitializeDevice(DWORD_PTR dwUserID, VMR9AllocationInfo* lpAllocInfo, DWORD* lpNumBuffers);
    virtual HRESULT STDMETHODCALLTYPE  TerminateDevice(DWORD_PTR dwID);
    virtual HRESULT STDMETHODCALLTYPE  GetSurface(DWORD_PTR dwUserID, DWORD SurfaceIndex, DWORD SurfaceFlags, IDirect3DSurface9** lplpSurface);
    virtual HRESULT STDMETHODCALLTYPE  AdviseNotify(IVMRSurfaceAllocatorNotify9* lpIVMRSurfAllocNotify);

    // IVMRImagePresenter9
    virtual HRESULT STDMETHODCALLTYPE  StartPresenting(DWORD_PTR dwUserID);
    virtual HRESULT STDMETHODCALLTYPE  StopPresenting(DWORD_PTR dwUserID);
    virtual HRESULT STDMETHODCALLTYPE  PresentImage(DWORD_PTR dwUserID, VMR9PresentationInfo* lpPresInfo);

    // IUnknown
    virtual HRESULT STDMETHODCALLTYPE QueryInterface( 
        REFIID riid,
        void** ppvObject);

    virtual ULONG STDMETHODCALLTYPE AddRef();
    virtual ULONG STDMETHODCALLTYPE Release();
	void DrawTexture(FLOAT fx, FLOAT fy, FLOAT nw, FLOAT nh, FLOAT uoff, FLOAT voff, FLOAT umax, FLOAT vmax, long color);
	void ReleaseCallBack();

protected:
	void Paint(IDirect3DSurface9* pSurface,SIZE aspecRatio);
	void DeleteSurfaces();

	CComPtr<IVMRSurfaceAllocatorNotify9> m_pIVMRSurfAllocNotify;

    CComPtr<IDirect3DDevice9> m_pD3DDev;
	CComPtr<IDirect3D9> m_pD3D;
	vector<CComPtr<IDirect3DSurface9> >     m_pSurfaces;

	long		  m_refCount;
	int			  m_surfaceCount;
	HMONITOR	  m_hMonitor;
	IVMR9Callback* m_pCallback;
	int   m_iVideoWidth, m_iVideoHeight;
	int   m_iARX, m_iARY;
	//CRect m_WindowRect;
	//CRect m_VideoRect;
	bool m_fVMRSyncFix;
	double m_fps ;
	long   previousEndFrame;
	D3DTEXTUREFILTERTYPE m_Filter;
	bool m_bfirstFrame;
};

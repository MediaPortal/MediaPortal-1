// VMR9Helper.h : Declaration of the CVMR9Helper

#pragma once
#include "resource.h"       // main symbols
#include "DirectShowHelper.h"
#include "DX9AllocatorPresenter.h"


class ATL_NO_VTABLE CVMR9Helper : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CVMR9Helper, &CLSID_VMR9Helper>,
	public IVMR9Helper
{
public:
	CVMR9Helper()
		: m_pDevice(NULL)
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_VMR9HELPER)


BEGIN_COM_MAP(CVMR9Helper)
	COM_INTERFACE_ENTRY(IVMR9Helper)
END_COM_MAP()


	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}
	
	void FinalRelease() 
	{
	}

public:

	STDMETHOD(Init)(IVMR9Callback* callback, DWORD dwD3DDevice, IBaseFilter* vmr9Filter,DWORD monitor);
	STDMETHOD(Deinit)(void);
protected:
	CComPtr<IVMRSurfaceAllocator9>  g_allocator;
	CComPtr<IBaseFilter>			m_pVMR9Filter;
	LPDIRECT3DDEVICE9				m_pDevice;
public:
	STDMETHOD(Version)(void);
	STDMETHOD(GetVideoSize)(ULONG* Width, ULONG* Height);
};

OBJECT_ENTRY_AUTO(__uuidof(VMR9Helper), CVMR9Helper)
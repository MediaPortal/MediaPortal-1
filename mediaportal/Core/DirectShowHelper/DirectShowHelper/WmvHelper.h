// WmvHelper.h : Declaration of the CWmvHelper

#pragma once
#include "resource.h"       // main symbols

#include "DirectShowHelper.h"


// CWmvHelper

class ATL_NO_VTABLE CWmvHelper : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CWmvHelper, &CLSID_WmvHelper>,
	public IWmvHelper
{
public:
	CWmvHelper()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_WMVHELPER)


BEGIN_COM_MAP(CWmvHelper)
	COM_INTERFACE_ENTRY(IWmvHelper)
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
	STDMETHOD(SetProfile)(IConfigAsfWriter* asfWriter, ULONG bitrate, ULONG fps, ULONG screenX, ULONG screenY);
};

OBJECT_ENTRY_AUTO(__uuidof(WmvHelper), CWmvHelper)

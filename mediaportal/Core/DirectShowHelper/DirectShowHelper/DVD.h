// DVD.h : Declaration of the CDVD

#pragma once
#include "resource.h"       // main symbols

#include "DirectShowHelper.h"


// CDVD

class ATL_NO_VTABLE CDVD : 
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CDVD, &CLSID_DVD>,
	public IDispatchImpl<IDVD, &IID_IDVD, &LIBID_DirectShowHelperLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CDVD()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_DVD)


BEGIN_COM_MAP(CDVD)
	COM_INTERFACE_ENTRY(IDVD)
	COM_INTERFACE_ENTRY(IDispatch)
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

	STDMETHOD(Reset)(BSTR strPath);
};

OBJECT_ENTRY_AUTO(__uuidof(DVD), CDVD)

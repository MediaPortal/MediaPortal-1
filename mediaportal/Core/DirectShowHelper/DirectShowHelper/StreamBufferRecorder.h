// StreamBufferRecorder.h : Declaration of the CStreamBufferRecorder

#pragma once
#include "resource.h"       // main symbols

#include "DirectShowHelper.h"


// CStreamBufferRecorder

class ATL_NO_VTABLE CStreamBufferRecorder : 
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CStreamBufferRecorder, &CLSID_StreamBufferRecorder>,
	public IDispatchImpl<IStreamBufferRecorder, &IID_IStreamBufferRecorder, &LIBID_DirectShowHelperLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CStreamBufferRecorder()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_STREAMBUFFERRECORDER)


BEGIN_COM_MAP(CStreamBufferRecorder)
	COM_INTERFACE_ENTRY(IStreamBufferRecorder)
	COM_INTERFACE_ENTRY(IDispatch)
END_COM_MAP()


	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}
	
	void FinalRelease() ;

public:
	STDMETHOD(Create)(IBaseFilter* streamBufferSink, BSTR strPath, DWORD dwRecordingType);
	STDMETHOD(Start)(LONG startTime);
	STDMETHOD(Stop)(void);
protected:
	CComQIPtr<IStreamBufferRecordControl> m_recordControl;
public:
	STDMETHOD(SetAttributeString)(BSTR strName, BSTR strValue);
	STDMETHOD(SetAttributeDWORD)(BSTR strName, ULONG dwValue);
};

OBJECT_ENTRY_AUTO(__uuidof(StreamBufferRecorder), CStreamBufferRecorder)

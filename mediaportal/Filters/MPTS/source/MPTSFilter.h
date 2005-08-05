/*
	MediaPortal TS-SourceFilter by Agree

	
*/
#ifndef __MPTsFilter
#define __MPTsFilter
class CMPTSFilter;

#include <objbase.h>
#include "FilterOutPin.h"
#include "StreamPids.h"
#include "Sections.h"
#include "FileReader.h"
#include "SplitterSetup.h"

// guids:

// filter
DEFINE_GUID(CLSID_MPTSFilter,
0xA3556F1E, 0x787B, 0x12C4, 0x91, 0x00, 0x01, 0xAF, 0x31, 0x3A, 0xC9, 0x00);
// interface
DEFINE_GUID(IID_IMPTSControl,
0xA3556F1E, 0x787B, 0x12C4, 0x91, 0x00, 0x01, 0xAF, 0x31, 0x3A, 0xC9, 0x10);

// interface
DECLARE_INTERFACE_(IMPTSControl, IUnknown)
{
	STDMETHOD(Refresh)()PURE;
};

// filter
class CMPTSFilter;

class CMPTSFilter : public CSource,public IFileSourceFilter,public IMPTSControl
{
	//friend class CFilterOutPin;
public:
	DECLARE_IUNKNOWN
	static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

private:

	CMPTSFilter(IUnknown *pUnk, HRESULT *phr);
	~CMPTSFilter();

	STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

public:
	// Pin enumeration
	CBasePin * GetPin(int n);
	int GetPinCount();
	STDMETHODIMP Run(REFERENCE_TIME tStart);
	STDMETHODIMP Pause();
	STDMETHODIMP SetSyncClock(void);
	STDMETHODIMP Stop();
	HRESULT OnConnect();
	STDMETHODIMP Refresh();
	HRESULT RefreshPids();
	HRESULT RefreshDuration();
	HRESULT GetFileSize(__int64 *pfilesize);
	STDMETHODIMP Log(char* text,bool crlf);
	STDMETHODIMP Log(__int64 value,bool crlf);
	HRESULT SetFilePosition(REFERENCE_TIME seek);
	void UpdatePids();
protected:
	// IFileSourceFilter
	STDMETHODIMP Load(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt);
	STDMETHODIMP GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt);
	STDMETHODIMP GetDuration(REFERENCE_TIME *dur);
protected:
	CFilterOutPin *m_pPin;
	__int64 logFilePos; 
	HANDLE  m_logFileHandle;
	Sections *m_pSections;
	FileReader *m_pFileReader;
	SplitterSetup *m_pDemux;
	CCritSec m_Lock;
	BOOL m_setPosition;


};

#endif
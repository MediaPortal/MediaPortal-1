#pragma once

#include <vector>
#include <map>

class CTTPremiumSource;

// Class for the TTPremium Source filter's Output pins.
class CTTPremiumOutputPin : public CSourceStream, public IMPEG2PIDMap
{
public:
  // Constructor and destructor
  CTTPremiumOutputPin(TCHAR *pObjName, CTTPremiumSource *pTTPremiumSource, HRESULT *phr, LPCWSTR pPinName, AM_MEDIA_TYPE* pMt);
  virtual ~CTTPremiumOutputPin();

	// Extra functions needed by the CTTEnumPIDMap class
	ULONG GetPIDVersion()  { return m_Version; }
	HRESULT GetPID(int iPosition, PID_MAP *pPID);

	// Override the methods that need to be implemented from CSourceStream
	virtual HRESULT DecideBufferSize(IMemAllocator *pAlloc, ALLOCATOR_PROPERTIES *ppropInputRequest);
	virtual HRESULT FillBuffer(IMediaSample *pSample);
	virtual HRESULT OnThreadCreate(void);
	virtual HRESULT CheckMediaType(const CMediaType *pmt);
	virtual HRESULT GetMediaType(int iPosition, CMediaType *pMediaType);

	// Implement the methods for IMPEG2PIDMap
	STDMETHODIMP MapPID(ULONG culPID, ULONG* pulPID, MEDIA_SAMPLE_CONTENT MediaSampleContent);
	STDMETHODIMP UnmapPID(ULONG culPID, ULONG* pulPID);
	STDMETHODIMP EnumPIDMap(IEnumPIDMap** ppEnum);

	// Implement the methods of IUnknown
  DECLARE_IUNKNOWN

  // Overriden to say what interfaces we support where
  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

private:
  friend class CTTPremiumSource;
  friend class TSDataFilter;

  CTTPremiumSource *m_pTTPremiumSource;	// Main filter object pointer
	ULONG m_Version;
	ULONG m_bufferSize;
  bool m_threadStarted;
	std::vector<PID_MAP> m_PIDs;
	std::map<ULONG, TSDataFilter*> m_PIDToDataFilter;
};

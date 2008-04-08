#pragma once

#include "TTPremiumTypes.h"
#include "ITTPremiumSource.h"
#include "DVBCommon.h"
#include "DVBTSFilter.h"
#include "DVBFrontend.h"
#include "DVBBoardControl.h"
#include "DVBAVCtrl.h"
#include <map>

// Filter GUID
// { 022B8142-0946-11cf-BCB1-444553540000 }
DEFINE_GUID(CLSID_TTPremiumSource,
0x22b8142, 0x946, 0x11cf, 0xbc, 0xb1, 0x44, 0x45, 0x53, 0x54, 0x0, 0x0);

class TSDataFilter;
class CTTPremiumOutputPin;

// Class for the TTPremiumSource filter
class CTTPremiumSource : public IMpeg2Demultiplexer, public ITTPremiumSource, public CSource
{
public:
  CTTPremiumSource(TCHAR *pName,LPUNKNOWN pUnk,HRESULT *hr);
  virtual ~CTTPremiumSource();

	// Implement the methods of ITTPremiumSource
	STDMETHODIMP Init();
	STDMETHODIMP Close();
	STDMETHODIMP GetNetworkType(NetworkType &network);
	STDMETHODIMP Tune(ULONG frequency, ULONG symbolRate, ULONG polarity, ULONG LNBKhz, ULONG LNBFreq,
									 BOOL LBNPower, ULONG diseq, ModType modulation, BandwidthType bwType, BOOL specInv);
	STDMETHODIMP GetSignalState(BOOL &locked, ULONG &quality, ULONG &level);

	// Implement the methods of IMpeg2Demultiplexer
	STDMETHODIMP CreateOutputPin(AM_MEDIA_TYPE* pMediaType, LPWSTR pszPinName, IPin** ppIPin);
	STDMETHODIMP SetOutputPinMediaType(LPWSTR pszPinName, AM_MEDIA_TYPE* pMediaType);
	STDMETHODIMP DeleteOutputPin( LPWSTR pszPinName);

	// Implement the methods of IUnknown
  DECLARE_IUNKNOWN

  // Overriden to say what interfaces we support where
  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

  // Function needed for the class factory
  static CUnknown * WINAPI CreateInstance(LPUNKNOWN pUnk, HRESULT *phr);

private:
	friend class CTTPremiumOutputPin;
	std::map<ULONG, TSDataFilter*> m_PIDs;

protected:
  CDVBBoardControl *m_boardControl;
	CDVBFrontend *m_frontend;
	CDVBAVControl *m_AVControl;
};


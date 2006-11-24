#include "ttBdaDrvApi.h"

#pragma once

// {B0AB5587-DCEC-49f4-B1AA-06EF58DBF1D3}
DEFINE_GUID(IID_ITechnoTrend, 0xb0ab5587, 0xdcec, 0x49f4, 0xb1, 0xaa, 0x6, 0xef, 0x58, 0xdb, 0xf1, 0xd3);

DECLARE_INTERFACE_(ITechnoTrend, IUnknown)
{
  STDMETHOD(SetTunerFilter)(THIS_ IBaseFilter* tunerFilter)PURE;
	STDMETHOD(IsTechnoTrend)(THIS_ BOOL* yesNo)PURE;
	STDMETHOD(IsCamReady)(THIS_ BOOL* yesNo)PURE;
	STDMETHOD(SetAntennaPower)(THIS_ BOOL onOff)PURE;
	STDMETHOD(SetDisEqc)(THIS_ int diseqcType, int hiband, int vertical)PURE;
	STDMETHOD(DescrambleService)(THIS_ int serviceId,BOOL* succeeded)PURE;
};

class CTechnotrend: public CUnknown, public ITechnoTrend
{
public:
  CTechnotrend(LPUNKNOWN pUnk, HRESULT *phr);
	~CTechnotrend(void);
  DECLARE_IUNKNOWN
  
  STDMETHODIMP SetTunerFilter(IBaseFilter* tunerFilter);
	STDMETHODIMP IsTechnoTrend( BOOL* yesNo);
	STDMETHODIMP IsCamReady( BOOL* yesNo);
	STDMETHODIMP SetAntennaPower( BOOL onOff);
	STDMETHODIMP SetDisEqc( int diseqcType, int hiband, int vertical);
	STDMETHODIMP DescrambleService( int serviceId,BOOL* succeeded);

  void OnCaChange(BYTE  nSlot,BYTE  nReplyTag,WORD  wStatus);
  void OnSlotChange(BYTE nSlot,BYTE nStatus,TYP_SLOT_INFO* csInfo);
private:
  bool        GetDeviceID(IBaseFilter* tunerFilter, UINT& deviceId);
  HANDLE      m_hBdaApi;
  int         m_slotStatus;
  DEVICE_CAT  m_deviceType;
  int         m_ciStatus;
  TS_CiCbFcnPointer m_technoTrendStructure;
};

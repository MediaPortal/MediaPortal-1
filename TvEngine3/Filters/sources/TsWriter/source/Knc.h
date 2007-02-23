#pragma once
#include "knc/KNCBDACI.h"

// {C71E2EFA-2439-4dbe-A1F7-935ADC37A4EC}
DEFINE_GUID(IID_IKNC, 0xc71e2efa, 0x2439, 0x4dbe, 0xa1, 0xf7, 0x93, 0x5a, 0xdc, 0x37, 0xa4, 0xec);

DECLARE_INTERFACE_(IKNC, IUnknown)
{
  STDMETHOD(SetTunerFilter)(THIS_ IBaseFilter* tunerFilter)PURE;
	STDMETHOD(IsKNC)(THIS_ BOOL* yesNo)PURE;
	STDMETHOD(IsCamReady)(THIS_ BOOL* yesNo)PURE;
	STDMETHOD(SetDisEqc)(THIS_ int diseqcType, int hiband, int vertical)PURE;
	STDMETHOD(DescrambleService)(THIS_ BYTE* PMT, int PMTLength,BOOL* succeeded)PURE;
	STDMETHOD(DescrambleMultiple)(THIS_ WORD* pNrs, int NrOfOfPrograms,BOOL* succeeded)PURE;
};

class CKnc: public CUnknown, public IKNC, public CKNCBDACICallback
{
public:
  CKnc(LPUNKNOWN pUnk, HRESULT *phr);
	~CKnc(void);
  DECLARE_IUNKNOWN
  
  STDMETHODIMP SetTunerFilter(IBaseFilter* tunerFilter);
	STDMETHODIMP IsKNC( BOOL* yesNo);
	STDMETHODIMP IsCamReady( BOOL* yesNo);
	STDMETHODIMP SetDisEqc( int diseqcType, int hiband, int vertical);
	STDMETHODIMP DescrambleService( BYTE* PMT, int PMTLength,BOOL* succeeded);
	STDMETHODIMP DescrambleMultiple(WORD* pNrs, int NrOfOfPrograms,BOOL* succeeded);

public:
	virtual void OnKncCiState(UCHAR slot,int State, LPCTSTR lpszMessage) ;
	virtual void OnKncCiOpenDisplay(UCHAR slot) ;
	virtual void OnKncCiMenu(UCHAR slot,LPCTSTR lpszTitle, LPCTSTR lpszSubTitle, LPCTSTR lpszBottom, UINT nNumChoices) ;
	virtual void OnKncCiMenuChoice(UCHAR slot,UINT nChoice, LPCTSTR lpszText) ;
	virtual void OnKncCiRequest(UCHAR slot,BOOL bBlind, UINT nAnswerLength, LPCTSTR lpszText) ;
	virtual void OnKncCiCloseDisplay(UCHAR slot,UINT nDelay) ;
private:
  CKNCBDACI* m_pKNC;
  bool m_bIsKNC;
};

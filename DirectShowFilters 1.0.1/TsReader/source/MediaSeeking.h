#pragma once


class AM_NOVTABLE CMediaSeeking :
    public IMediaSeeking,
    public CUnknown
{

public:

    DECLARE_IUNKNOWN;
    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void **ppv);

    // IMediaSeeking methods

    STDMETHODIMP IsFormatSupported(const GUID * pFormat);
    STDMETHODIMP QueryPreferredFormat(GUID *pFormat);
    STDMETHODIMP SetTimeFormat(const GUID * pFormat);
    STDMETHODIMP IsUsingTimeFormat(const GUID * pFormat);
    STDMETHODIMP GetTimeFormat(GUID *pFormat);
    STDMETHODIMP GetDuration(LONGLONG *pDuration);
    STDMETHODIMP GetStopPosition(LONGLONG *pStop);
    STDMETHODIMP GetCurrentPosition(LONGLONG *pCurrent);
    STDMETHODIMP GetCapabilities( DWORD * pCapabilities );
    STDMETHODIMP CheckCapabilities( DWORD * pCapabilities );
    STDMETHODIMP ConvertTimeFormat( LONGLONG * pTarget, const GUID * pTargetFormat,
                                    LONGLONG    Source, const GUID * pSourceFormat );

    STDMETHODIMP SetPositions( LONGLONG * pCurrent,  DWORD CurrentFlags
			     , LONGLONG * pStop,  DWORD StopFlags );

    STDMETHODIMP GetPositions( LONGLONG * pCurrent, LONGLONG * pStop );

    STDMETHODIMP GetAvailable( LONGLONG * pEarliest, LONGLONG * pLatest );
    STDMETHODIMP SetRate( double dRate);
    STDMETHODIMP GetRate( double * pdRate);
    STDMETHODIMP GetPreroll(LONGLONG *pPreroll);


protected:

    // ctor
  CMediaSeeking(const TCHAR *, LPUNKNOWN, HRESULT*, CCritSec *);

    // we call this to notify changes. Override to handle them
    virtual HRESULT ChangeStart() PURE;
    virtual HRESULT ChangeStop() PURE;
    virtual HRESULT ChangeRate() PURE;

    CRefTime m_rtDuration;      // length of stream
    CRefTime m_rtStart;         // source will start here
    CRefTime m_rtStop;          // source will stop here
    double m_dRateSeeking;

    // seeking capabilities
    DWORD m_dwSeekingCaps;

    CCritSec * m_pLock;
};

#pragma once

class CTTPremiumOutputPin;

// Class for the TTPremium Enumerated PID Map
class CTTEnumPIDMap : public IEnumPIDMap    // The interface we support
{
public:
    CTTEnumPIDMap(CTTPremiumOutputPin *pPin, CTTEnumPIDMap *pEnum);
    virtual ~CTTEnumPIDMap();

    // IUnknown
    STDMETHODIMP QueryInterface(REFIID riid, void **ppv);
    STDMETHODIMP_(ULONG) AddRef();
    STDMETHODIMP_(ULONG) Release();

    // IEnumMediaTypes
    STDMETHODIMP Next(ULONG cPIDs,				// place this many PIDs...
									PID_MAP *pPIDs,		// ...in this array
									ULONG * pcFetched    // actual count passed
									);

    STDMETHODIMP Skip(ULONG cPIDs);
    STDMETHODIMP Reset();
    STDMETHODIMP Clone(IEnumPIDMap **ppEnum);

private:
    int m_Position;								// Current ordinal position
    CTTPremiumOutputPin *m_pPin;     // The pin who owns us
    LONG m_Version;							// PIDs version value
    LONG m_cRef;
#ifdef DEBUG
    DWORD m_dwCookie;
#endif

    BOOL AreWeOutOfSync();
};

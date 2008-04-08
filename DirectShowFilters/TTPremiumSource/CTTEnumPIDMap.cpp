#include <stdafx.h>
#include <streams.h>
#include <initguid.h>
#include <bdaiface.h>
#include "CTTEnumPIDMap.h"
#include "CTTPremiumOutputPin.h"

//
// Constructor
//
CTTEnumPIDMap:: CTTEnumPIDMap(CTTPremiumOutputPin *pPin,  CTTEnumPIDMap *pEnum) :
    m_Position(0),
    m_pPin(pPin),
    m_cRef(1)
{
#ifdef DEBUG
    m_dwCookie = DbgRegisterObjectCreation("CTTEnumPIDMap", 0);
#endif

    // We must be owned by a pin derived from CTTPremiumOutputPin
    ASSERT(pPin != NULL);

    // Hold a reference count on our pin 
    m_pPin->AddRef();

    // Are we creating a new enumerator 
    if (pEnum == NULL) 
	{
        m_Version = m_pPin->GetPIDVersion();
        return;
    }

    m_Position = pEnum->m_Position;
    m_Version = pEnum->m_Version;
}

//
// Destructor
//
CTTEnumPIDMap::~CTTEnumPIDMap()
{
#ifdef DEBUG
    DbgRegisterObjectDestruction(m_dwCookie);
#endif
    m_pPin->Release();
}


//
// Override this to say what interfaces we support
//
STDMETHODIMP CTTEnumPIDMap::QueryInterface(REFIID riid,void **ppv)
{
    CheckPointer(ppv, E_POINTER);

    // Do we have this interface
    if (riid == IID_IEnumMediaTypes)
	{
        return GetInterface((IEnumPIDMap*) this, ppv);
    }
	else if (riid == IID_IUnknown) 
	{
        return GetInterface((IUnknown*) this, ppv);
	}
	else 
	{
        *ppv = NULL;
        return E_NOINTERFACE;
    }
}

//
// Increment the ref count for this instance
//
STDMETHODIMP_(ULONG) CTTEnumPIDMap::AddRef()
{
    return InterlockedIncrement(&m_cRef);
}

//
// Decrement the ref count for this instance and delete if ref count has reached 0
//
STDMETHODIMP_(ULONG) CTTEnumPIDMap::Release()
{
    ULONG cRef = InterlockedDecrement(&m_cRef);
    if (cRef == 0) 
	{
        delete this;
    }
    return cRef;
}

//
// Make a snapshot copy of this instance
//
STDMETHODIMP CTTEnumPIDMap::Clone(IEnumPIDMap **ppEnum)
{
    CheckPointer(ppEnum,E_POINTER);
    ValidateReadWritePtr(ppEnum,sizeof(IEnumPIDMap *));

	HRESULT hr = NOERROR;
    // Check we are still in sync with the pin
    if (AreWeOutOfSync() == TRUE) 
	{
        *ppEnum = NULL;
        hr = VFW_E_ENUM_OUT_OF_SYNC;
    } 
	else 
	{
        *ppEnum = new CTTEnumPIDMap(m_pPin, this);
        if (*ppEnum == NULL) 
		{
            hr =  E_OUTOFMEMORY;
        }
    }
    return hr;
}

//
// Enumerate the next pin
//
STDMETHODIMP CTTEnumPIDMap::Next(ULONG cPIDs, PID_MAP* pPIDs, ULONG *pcFetched)
{
    CheckPointer(pPIDs,E_POINTER);
	ValidateReadWritePtr(pPIDs, cPIDs * sizeof(PID_MAP));
    
	// Check we are still in sync with the pin
    if (AreWeOutOfSync() == TRUE) 
	{
        return VFW_E_ENUM_OUT_OF_SYNC;
    }

    if (pcFetched != NULL) 
	{
        ValidateWritePtr(pcFetched, sizeof(ULONG));
        *pcFetched = 0;           // default unless we succeed
    }
    // now check that the parameter is valid 
    else if (cPIDs > 1) // pcFetched == NULL
	{     
        return E_INVALIDARG;
    }

    ULONG cFetched = 0;           // increment as we get each one.

    while (cPIDs) 
	{
        CMediaType cmt;

        HRESULT hr = m_pPin->GetPID(m_Position, pPIDs);
		m_Position++;
        if (S_OK != hr) 
		{
            break;
        }

        pPIDs++;
        cFetched++;
        cPIDs--;
    }

    if (pcFetched != NULL) 
	{
        *pcFetched = cFetched;
    }

    return ( cPIDs == 0 ? NOERROR : S_FALSE );
}

//
// Skip over N PIDs
//
STDMETHODIMP CTTEnumPIDMap::Skip(ULONG cPIDs)
{
    //  If we're skipping 0 elements we're guaranteed to skip the
    //  correct number of elements
    if (cPIDs == 0) 
	{
        return S_OK;
    }

    // Check we are still in sync with the pin 
    if (AreWeOutOfSync() == TRUE) 
	{
        return VFW_E_ENUM_OUT_OF_SYNC;
    }

    m_Position += cPIDs;

    // See if we're over the end 
    PID_MAP pid;
    return S_OK == m_pPin->GetPID(m_Position - 1, &pid) ? S_OK : S_FALSE;
}

//
// Set the position back to the start
//
STDMETHODIMP CTTEnumPIDMap::Reset()

{
    m_Position = 0;

    // Bring the enumerator back into step with the current state.  This
    // may be a noop but ensures that the enumerator will be valid on the
    // next call.
    m_Version = m_pPin->GetPIDVersion();
    return NOERROR;
}

//
// Are we out of sync with the parent pin?
//
BOOL CTTEnumPIDMap::AreWeOutOfSync()
{
	return (m_pPin->GetPIDVersion() == m_Version ? FALSE : TRUE);
}


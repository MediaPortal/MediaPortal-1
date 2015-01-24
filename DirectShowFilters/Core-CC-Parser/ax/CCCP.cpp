#include "StdAfx.h"

#include <initguid.h>
#if (1100 > _MSC_VER)
#include <olectlid.h>
#else
#include <olectl.h>
#endif

#include "CCPGUIDs.h"
#include "CCPProps.h"

#include "CCCP.h"

//------------------------------------------------------------------------
// Implementation
//------------------------------------------------------------------------

// Self-registration data structures

const AMOVIESETUP_MEDIATYPE
sudPassThroughType = { &CCcFilter::m_guidPassThroughMediaMajor
                     /*, &CCcFilter::m_guidPassThroughMediaSubtype*/ };

const AMOVIESETUP_MEDIATYPE
sudPinLine21Type =   { &CLine21OutputPin::m_guidMediaMajor
                     , &CLine21OutputPin::m_guidMediaSubtype };

const AMOVIESETUP_PIN
psudPins[] = { { (LPWSTR)CCcFilter::m_szInput 					// Obsolete, not used.
               , FALSE					// bRendered
               , FALSE					// bOutput
               , FALSE					// bZero
               , FALSE					// bMany
			   , &GUID_NULL				// Obsolete.
			   , NULL					// Obsolete.
               , 1						// nTypes
               , &sudPassThroughType	// lpTypes
               }
             , { (LPWSTR)CCcFilter::m_szPassThrough					// Obsolete, not used.
               , FALSE					// bRendered
               , TRUE					// bOutput
               , FALSE					// bZero
               , FALSE					// bMany
			   , &GUID_NULL				// Obsolete.
			   , NULL					// Obsolete.
               , 1						// nTypes
               , &sudPassThroughType    // lpTypes
               }
             , { (LPWSTR)CLine21OutputPin::m_szName					// Obsolete, not used.
               , FALSE					// bRendered
               , TRUE					// bOutput
               , FALSE					// bZero
               , FALSE					// bMany
			   , &GUID_NULL				// Obsolete.
			   , NULL					// Obsolete.
               , 1						// nTypes
               , &sudPinLine21Type      // lpTypes
               }
             };

#define CCP_NAME L"Core CC Parser"

const AMOVIESETUP_FILTER
sudCcParser = { &CLSID_CcParser                // class id
              , CCP_NAME                     // strName
              , MERIT_DO_NOT_USE               // dwMerit
              , _countof(psudPins)             // nPins
              , psudPins                       // lpPin
              , CLSID_LegacyAmFilterCategory   // Filter category
              };

// Needed for the CreateInstance mechanism
CFactoryTemplate g_Templates[]= { { CCP_NAME
                                  , &CLSID_CcParser
                                  , CCcFilter::CreateInstance
                                  , NULL
                                  , &sudCcParser
                                  }
                                , { CCP_NAME L" Property Page"
                                  , &CLSID_CcPProp
                                  , CCcParserProperties::CreateInstance
                                  }
                                };

int g_cTemplates = _countof(g_Templates);

int CCcFilter::m_nInstanceCount = 0;


//
// CreateInstance
//
// Override CClassFactory method.
// Provide the way for COM to create a CCcFilter object.
//
CUnknown * WINAPI CCcFilter::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
    ASSERT(phr);
    
    CCcFilter *pNewObject = new CCcFilter(NAME("CcParser Filter"), punk, phr);
    if (pNewObject == NULL) {
        if (phr)
            *phr = E_OUTOFMEMORY;
    }

    return pNewObject;

} // CreateInstance



//
// NonDelegatingQueryInterface
//
// Override CUnknown method.
// Reveal our persistent stream, property pages and ICcParser interfaces.
// Anyone can call our private interface so long as they know the private UUID.
//
STDMETHODIMP CCcFilter::NonDelegatingQueryInterface(REFIID riid, void **ppv)
{
    CheckPointer(ppv,E_POINTER);

    if (riid == IID_ICcParser) {
        return GetInterface((ICcParser *) this, ppv);

    } else if (riid == IID_ISpecifyPropertyPages) {
        return GetInterface((ISpecifyPropertyPages *) this, ppv);

    } else if (riid == IID_IPersistStream) {
        return GetInterface((IPersistStream *) this, ppv);

    } else {
        // Pass the buck
        return CTransInPlaceFilter::NonDelegatingQueryInterface(riid, ppv);
    }

} // NonDelegatingQueryInterface


// GetClassID
//
// Override CBaseMediaFilter method for interface IPersist
// Part of the persistent file support.  We must supply our class id
// which can be saved in a graph file and used on loading a graph with
// a gargle in it to instantiate this filter via CoCreateInstance.
//
STDMETHODIMP CCcFilter::GetClassID(CLSID *pClsid)
{
    CheckPointer(pClsid,E_POINTER);
    
    *pClsid = CLSID_CcParser;
    return NOERROR;

} // GetClassID


//
// SizeMax
//
// Override CPersistStream method.
// State the maximum number of bytes we would ever write in a file
// to save our properties.
//
enum{ iCurrentMajorVersion = 1, cDataInts = 3 };

DWORD CCcFilter::GetSoftwareVersion() 
{ 
	return iCurrentMajorVersion; 
}

int CCcFilter::SizeMax()
{
	return 12*sizeof(WCHAR) * cDataInts;
}


//
// WriteToStream
//
// Override CPersistStream method.
// Write our properties to the stream.
//
HRESULT CCcFilter::WriteToStream(IStream *pStream)
{
	int iChannel = 0;
	AM_LINE21_CCSERVICE iService = AM_L21_CCSERVICE_None;
	ICcParser_CCTYPE iXformType = ICcParser_CCTYPE_None;

	VERIFY( SUCCEEDED( get_Channel( &iChannel )));
	VERIFY( SUCCEEDED( get_Service( &iService )));
	VERIFY( SUCCEEDED( get_XformType( &iXformType )));

	RETURN_FAILED( WriteInt(pStream, cDataInts));

	RETURN_FAILED( WriteInt(pStream, iChannel ));
	RETURN_FAILED( WriteInt(pStream, iService ));
	RETURN_FAILED( WriteInt(pStream, iXformType ));
	
	CPersistStream::SetDirty( FALSE );

	return NOERROR;
}

HRESULT CCcFilter::ReadFromStream(IStream *pStream)
{
	int iChannel = 0;
	AM_LINE21_CCSERVICE iService = AM_L21_CCSERVICE_Caption1;
	ICcParser_CCTYPE iXformType = ICcParser_CCTYPE_ATSC_A53;

	if( mPS_dwFileVersion = iCurrentMajorVersion )
	{
		HRESULT hr;
		int iData = ReadInt(pStream, hr);

		if( SUCCEEDED(hr))
		{
			int iDataInts = iData;

			if( iDataInts > 0 )
			{
				iData = ReadInt(pStream, hr);
				if( SUCCEEDED(hr))
					iChannel = iData;

				iDataInts--;
			}

			if( iDataInts > 0 )
			{
				iData = ReadInt(pStream, hr);
				if( SUCCEEDED(hr))
					iService = (AM_LINE21_CCSERVICE)iData;

				iDataInts--;
			}

			if( iDataInts > 0 )
			{
				iData = ReadInt(pStream, hr);
				if( SUCCEEDED(hr))
					iXformType = (ICcParser_CCTYPE)iData;

				iDataInts--;
			}
		}
	}

	VERIFY( SUCCEEDED( put_Channel( iChannel )));
	VERIFY( SUCCEEDED( put_Service( iService )));
	VERIFY( SUCCEEDED( put_XformType( iXformType )));

	CPersistStream::SetDirty( FALSE );

	return NOERROR;
}



// ==============Implementation of the private ICcParser interface ==========
// ==================== needed to support the property page ===============

// ==============Implementation of the IPropertypages Interface ===========

//
// GetPages
//
STDMETHODIMP CCcFilter::GetPages(CAUUID * pPages)
{
    CheckPointer(pPages,E_POINTER);

    pPages->cElems = 1;
    pPages->pElems = (GUID *) CoTaskMemAlloc(pPages->cElems*sizeof(GUID));
    if (pPages->pElems == NULL) {
        return E_OUTOFMEMORY;
    }

    pPages->pElems[0] = CLSID_CcPProp;
    //pPages->pElems[1] = CLSID_CcPProp;

    return NOERROR;

} // GetPages


////////////////////////////////////////////////////////////////////////
//
// Exported entry points for registration and unregistration 
// (in this case they only call through to default implementations).
//
////////////////////////////////////////////////////////////////////////

STDAPI DllRegisterServer()
{
  return AMovieDllRegisterServer2( TRUE );
}


STDAPI DllUnregisterServer()
{
  return AMovieDllRegisterServer2( FALSE );
}

//
// DllEntryPoint
//
extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);

BOOL APIENTRY DllMain(HANDLE hModule, 
                      DWORD  dwReason, 
                      LPVOID lpReserved)
{
	return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);
}

#pragma warning(disable: 4514) // "unreferenced inline function has been removed"




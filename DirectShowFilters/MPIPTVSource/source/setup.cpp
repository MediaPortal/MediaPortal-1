
#include <streams.h>
#include <initguid.h>
#include "mpiptvsource.h"

#define g_wszPushSource     L"MediaPortal IPTV Source Filter"

// Filter setup data
const AMOVIESETUP_MEDIATYPE iptvOpPinTypes =
{
	&MEDIATYPE_Stream,              // Major type
	&MEDIASUBTYPE_MPEG2_TRANSPORT   // Minor type
};

const AMOVIESETUP_PIN iptvOutputPin = 
{
    L"Output",      // Obsolete, not used.
    FALSE,          // Is this pin rendered?
    TRUE,           // Is it an output pin?
    FALSE,          // Can the filter create zero instances?
    FALSE,          // Does the filter create multiple instances?
    &CLSID_NULL,    // Obsolete.
    NULL,           // Obsolete.
    1,              // Number of media types.
    &iptvOpPinTypes // Pointer to media types.
};

const AMOVIESETUP_FILTER iptvPushSource =
{
    &CLSID_MPIptvSource,    // Filter CLSID
    g_wszPushSource,        // String name
    MERIT_DO_NOT_USE,       // Filter merit
    1,                      // Number pins
    &iptvOutputPin          // Pin details
};

// List of class IDs and creator functions for the class factory. This
// provides the link between the OLE entry point in the DLL and an object
// being created. The class factory will call the static CreateInstance.
// We provide a set of filters in this one DLL.

CFactoryTemplate g_Templates[1] = 
{
    { 
      g_wszPushSource,                    // Name
      &CLSID_MPIptvSource,                // CLSID
      CMPIptvSource::CreateInstance,      // Method to create an instance of MyComponent
      NULL,                               // Initialization function
      &iptvPushSource                     // Set-up information (for filters)
    }
};

int g_cTemplates = sizeof(g_Templates) / sizeof(g_Templates[0]);    



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


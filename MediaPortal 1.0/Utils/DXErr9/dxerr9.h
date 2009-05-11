// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the TEST_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// TEST_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef DXERR9_EXPORTS
#define DXERR9_API __declspec(dllexport)
#else
#define DXERR9_API __declspec(dllimport)
#endif



extern "C" DXERR9_API LPCWSTR GetDXErrorString(HRESULT DXError);
extern "C" DXERR9_API LPCWSTR GetDXErrorDescription(HRESULT DXError);
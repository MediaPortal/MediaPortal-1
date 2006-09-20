// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the DSHOWHELPER_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// DSHOWHELPER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef DSHOWHELPER_EXPORTS
#define DSHOWHELPER_API __declspec(dllexport)
#else
#define DSHOWHELPER_API __declspec(dllimport)
#endif

// This class is exported from the dshowhelper.dll
class DSHOWHELPER_API Cdshowhelper {
public:
	Cdshowhelper(void);
	// TODO: add your methods here.
};

extern DSHOWHELPER_API int ndshowhelper;

DSHOWHELPER_API int fndshowhelper(void);

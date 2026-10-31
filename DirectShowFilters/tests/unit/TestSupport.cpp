#include <windows.h>
#include <crtdbg.h>
#include "CrtAsserts.h"

// The shared filter code calls LogDebug(); the tests do not need the output.
void LogDebug(const char* /*fmt*/, ...)
{
}

#ifdef _DEBUG
namespace
{
  int g_crtAsserts = 0;

  int CrtReportHook(int type, char* /*message*/, int* returnValue)
  {
    if (type != _CRT_ASSERT)
      return FALSE;
    g_crtAsserts++;
    *returnValue = 0;                                // no break into the debugger
    return TRUE;                                     // handled: no dialog
  }

  struct CrtHookInstaller
  {
    CrtHookInstaller() { _CrtSetReportHook(CrtReportHook); }
  } g_installer;
}
#endif

int CrtAssertsSeen()
{
#ifdef _DEBUG
  return g_crtAsserts;
#else
  return 0;
#endif
}

void ResetCrtAsserts()
{
#ifdef _DEBUG
  g_crtAsserts = 0;
#endif
}

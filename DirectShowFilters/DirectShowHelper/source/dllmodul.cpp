#include "stdafx.h"

extern void StopLogger();
static int processCounter = 0;

extern "C"
BOOL WINAPI DllMain(HINSTANCE /*hInstance*/, DWORD dwReason, LPVOID /*lpReserved*/)
{
  if (dwReason == DLL_PROCESS_ATTACH)
  {
    processCounter++;
  }
  else if (dwReason == DLL_PROCESS_DETACH)
  {
    processCounter--;
    if (processCounter == 0)
      StopLogger();
  }
  return TRUE;
}

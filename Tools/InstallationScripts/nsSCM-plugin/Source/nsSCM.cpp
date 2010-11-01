/*********************************************************************************************************
 *
 *  Module Name:	nsSCM.cpp
 *
 *  Abstract:		NSIS Service Control Manager
 *
 *  Author:		Vyacheslav I. Levtchenko (mail-to: sl@r-tt.com, sl@eltrast.ru)
 *
 *  Revision History:	20.10.2003	started
 *
 *  Classes, methods and structures:
 *
 *  TODO:
 *
 *********************************************************************************************************/

#include <windows.h>

#include "debug.h"
#include "exdll.h"

static SC_HANDLE schSCManager = (SC_HANDLE)INVALID_HANDLE_VALUE;
#define CHECK_HANDLE(schSCManager) if (INVALID_HANDLE_VALUE == schSCManager) { RET_ERROR(); }

#if 0
// Service Types (Bit Mask)
#define SERVICE_KERNEL_DRIVER          0x00000001
#define SERVICE_FILE_SYSTEM_DRIVER     0x00000002
#define SERVICE_ADAPTER                0x00000004
#define SERVICE_RECOGNIZER_DRIVER      0x00000008

#define SERVICE_DRIVER                 (SERVICE_KERNEL_DRIVER | \
                                        SERVICE_FILE_SYSTEM_DRIVER | \
                                        SERVICE_RECOGNIZER_DRIVER)

#define SERVICE_WIN32_OWN_PROCESS      0x00000010
#define SERVICE_WIN32_SHARE_PROCESS    0x00000020
#define SERVICE_WIN32                  (SERVICE_WIN32_OWN_PROCESS | \
                                        SERVICE_WIN32_SHARE_PROCESS)

#define SERVICE_INTERACTIVE_PROCESS    0x00000100

#define SERVICE_TYPE_ALL               (SERVICE_WIN32   | \
                                        SERVICE_ADAPTER | \
                                        SERVICE_DRIVER  | \
                                        SERVICE_INTERACTIVE_PROCESS)

// Start Type
#define SERVICE_BOOT_START             0x00000000
#define SERVICE_SYSTEM_START           0x00000001
#define SERVICE_AUTO_START             0x00000002
#define SERVICE_DEMAND_START           0x00000003
#define SERVICE_DISABLED               0x00000004

// Error control type
#define SERVICE_ERROR_IGNORE           0x00000000
#define SERVICE_ERROR_NORMAL           0x00000001
#define SERVICE_ERROR_SEVERE           0x00000002
#define SERVICE_ERROR_CRITICAL         0x00000003

#endif

// 1 str			    2 str			      3	num	     4	num	  5 str				6 str			7 str		     8 str	     9 str
// <name of service: startstop name> <name to display: display in SCM> <service type> <start type> <service's binary:filepath> <load order group: name> <dependencies: name> <account: name> <password: str>

NSISFunction(Install)
{
  EXDLL_INIT ();
  CHECK_HANDLE (schSCManager);
  char temp [128];

  BOOL rc = false;
  DWORD Tag = 0xDEADBEAF;

  // 1 
  char *ServiceName = STRNEW ();

  if (popstring (ServiceName))
   {
    STRDEL (ServiceName);
    RET_DWORD (rc, Tag);
   }

  // 2
  char *ServiceDisplay = STRNEW ();

  if (popstring (ServiceDisplay))
   {
    STRDEL (ServiceDisplay);
    STRDEL (ServiceName);
    RET_DWORD (rc, Tag);
   }

  // 3
  if (popstring (temp))
   {
    STRDEL (ServiceDisplay);
    STRDEL (ServiceName);
    RET_DWORD (rc, Tag);
   }

  DWORD ServiceType = ns_atoi (temp);

  // 4
  if (popstring (temp))
   {
    STRDEL (ServiceDisplay);
    STRDEL (ServiceName);
    RET_DWORD (rc, Tag);
   }

  DWORD StartType = ns_atoi (temp);

  // 5
  char *ServiceFile = STRNEW ();

  if (popstring (ServiceFile))
   {
    STRDEL (ServiceFile);
    STRDEL (ServiceDisplay);
    STRDEL (ServiceName);
    RET_DWORD (rc, Tag);
   }

  // 6
  char *LoadGroup = STRNEW ();

  if (popstring (LoadGroup))
   {
    STRDEL (LoadGroup);
    STRDEL (ServiceFile);
    STRDEL (ServiceDisplay);
    STRDEL (ServiceName);
    RET_DWORD (rc, Tag);
   }

  // 7
  char* Depend = STRNEW ();

  if (popstring (Depend))
   {
    STRDEL (Depend);
    STRDEL (LoadGroup);
    STRDEL (ServiceFile);
    STRDEL (ServiceDisplay);
    STRDEL (ServiceName);
    RET_DWORD (rc, Tag);
   }

  /* fixup end of multistring */
  DWORD len = strlen (Depend);
  Depend [len + 1] = 0;

  /* replace comma separator on null separator */
  for (DWORD i = 0; i < len; i++) if (',' == Depend [i]) Depend [i] = 0;

  // 8
  char* Account = STRNEW ();

  if (popstring (Account))
   {
    STRDEL (Account);
    STRDEL (Depend);
    STRDEL (LoadGroup);
    STRDEL (ServiceFile);
    STRDEL (ServiceDisplay);
    STRDEL (ServiceName);
    RET_DWORD (rc, Tag);
   }

  // 9
  char* Password = STRNEW ();

  if (popstring (Password))
   {
    STRDEL (Password);
    STRDEL (Account);
    STRDEL (Depend);
    STRDEL (LoadGroup);
    STRDEL (ServiceFile);
    STRDEL (ServiceDisplay);
    STRDEL (ServiceName);
    RET_DWORD (rc, Tag);
   }

  /* NOTE: This creates an entry for a standalone driver. If this
   *       is modified for use with a driver that requires a Tag,
   *       Group, and/or Dependencies, it may be necessary to
   *       query the registry for existing driver information
   *       (in order to determine a unique Tag, etc.).
   */
  SC_HANDLE schService = CreateService (schSCManager,		// SCManager database
					ServiceName,		// name of service
					ServiceDisplay,		// name to display
					SERVICE_ALL_ACCESS,	// desired access
					ServiceType,		// service type
					StartType,		// start type
					SERVICE_ERROR_NORMAL,	// error control type
					ServiceFile,		// service's binary
					LoadGroup,		// ordering group
					*LoadGroup ? &Tag : NULL,// tag identifier
					*Depend    ? Depend : NULL,// dependencies
					*Account   ? Account: NULL,// account
					*Password  ? Password:NULL);// password

  STRDEL (Password);
  STRDEL (Account);
  STRDEL (Depend);
  STRDEL (LoadGroup);
  STRDEL (ServiceFile);
  STRDEL (ServiceDisplay);
  STRDEL (ServiceName);

  if (NULL == schService)
   {
    Tag = GetLastError ();
    RET_DWORD (rc, Tag);
   }
  else
   {
    CloseServiceHandle (schService);
    rc = true;
    RET_DWORD (rc, Tag);
   }
}

// 1
// <name of service:startstop name>

NSISFunction(Start)
{
  EXDLL_INIT ();
  CHECK_HANDLE (schSCManager);

  // 1 
  char *ServiceName = STRNEW ();

  if (popstring (ServiceName))
   {
    STRDEL (ServiceName);
    RET_ERROR ();
   }

  SC_HANDLE schService = OpenService (schSCManager, ServiceName, SERVICE_ALL_ACCESS);
  STRDEL (ServiceName);

  if (NULL == schService)
   {
    RET_ERROR ();
   }

  BOOL rc = StartService (schService, 0, NULL) || ERROR_SERVICE_ALREADY_RUNNING == GetLastError ();
  CloseServiceHandle (schService);

  RET (rc);
}

// 1
// <name of service:startstop name>

NSISFunction(Stop)
{
  EXDLL_INIT ();
  CHECK_HANDLE (schSCManager);

  // 1 
  char *ServiceName = STRNEW ();

  if (popstring (ServiceName))
   {
    STRDEL (ServiceName);
    RET_ERROR ();
   }

  SC_HANDLE schService = OpenService (schSCManager, ServiceName, SERVICE_ALL_ACCESS);
  STRDEL (ServiceName);

  if (NULL == schService)
   {
    RET_ERROR ();
   }

  SERVICE_STATUS serviceStatus;
  BOOL rc = ControlService (schService, SERVICE_CONTROL_STOP, &serviceStatus);
  CloseServiceHandle (schService);

  RET (rc);
}

#if 0
//
// Service State -- for CurrentState
//
#define SERVICE_STOPPED                0x00000001
#define SERVICE_START_PENDING          0x00000002
#define SERVICE_STOP_PENDING           0x00000003
#define SERVICE_RUNNING                0x00000004
#define SERVICE_CONTINUE_PENDING       0x00000005
#define SERVICE_PAUSE_PENDING          0x00000006
#define SERVICE_PAUSED                 0x00000007
#endif

// 1
// <name of service:startstop name>

NSISFunction(QueryStatus)
{
  EXDLL_INIT ();
  CHECK_HANDLE (schSCManager);

  SERVICE_STATUS serviceStatus;
  serviceStatus.dwCurrentState = SERVICE_STOPPED;

  BOOL rc = false;

  // 1 
  char *ServiceName = STRNEW ();

  if (popstring (ServiceName))
   {
    STRDEL (ServiceName);
    RET_DWORD (rc, serviceStatus.dwCurrentState);
   }

  SC_HANDLE schService = OpenService (schSCManager, ServiceName, SERVICE_ALL_ACCESS);
  STRDEL (ServiceName);

  if (NULL == schService)
   {
    RET_DWORD (rc, serviceStatus.dwCurrentState);
   }

  rc = QueryServiceStatus (schService, &serviceStatus);
  CloseServiceHandle (schService);

  RET_DWORD (rc, serviceStatus.dwCurrentState);
}

// 1
// <name of service:startstop name>

NSISFunction(Remove)
{
  EXDLL_INIT ();
  CHECK_HANDLE (schSCManager);

  // 1 
  char *ServiceName = STRNEW ();

  if (popstring (ServiceName))
   {
    STRDEL (ServiceName);
    RET_ERROR ();
   }

  SC_HANDLE schService = OpenService (schSCManager, ServiceName, SERVICE_ALL_ACCESS);
  STRDEL (ServiceName);

  if (NULL == schService)
   {
    RET_ERROR ();
   }

  BOOL rc = DeleteService (schService);
  CloseServiceHandle (schService);

  RET (rc);
}

extern "C" BOOL WINAPI _DllMainCRTStartup (HANDLE hInst, ULONG ul_reason_for_call, LPVOID lpReserved)
{
  brk ();

  switch (ul_reason_for_call)
   {
    case DLL_THREAD_ATTACH:
    case DLL_PROCESS_ATTACH: schSCManager = OpenSCManager (NULL, NULL, SC_MANAGER_ALL_ACCESS); break;
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH: if (INVALID_HANDLE_VALUE != schSCManager) CloseServiceHandle (schSCManager); break;
   }

  return TRUE;
}

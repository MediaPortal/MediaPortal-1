
; Methods:
  /* Service Types (Bit Mask) */
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

  /* Start Type */
  #define SERVICE_BOOT_START             0x00000000
  #define SERVICE_SYSTEM_START           0x00000001
  #define SERVICE_AUTO_START             0x00000002
  #define SERVICE_DEMAND_START           0x00000003
  #define SERVICE_DISABLED               0x00000004

  /* Error control type */
  #define SERVICE_ERROR_IGNORE           0x00000000
  #define SERVICE_ERROR_NORMAL           0x00000001
  #define SERVICE_ERROR_SEVERE           0x00000002
  #define SERVICE_ERROR_CRITICAL         0x00000003

  ; 1 str			    2 str			      3	num	     4	num	  5 str				6 str			 7 str			8 str	     9 str
  ; <name of service: startstop name> <name to display: display in SCM> <service type> <start type> <service's binary:filepath> <load order group: name> <dependencies: name>	<account: name> <password: str>
  nsSCM::Install /NOUNLOAD <parameters>
  Pop $0 ; return error/success
  Pop $1 ; return GetLastError/tag

  ; <name of service:startstop name>
  nsSCM::Start /NOUNLOAD <parameters>
  Pop $0 ; return error/success

  ; <name of service:startstop name>
  nsSCM::QueryStatus /NOUNLOAD <parameters>
  Pop $0 ; return error/success
  Pop $1 ; return service status

  /* Service State -- for CurrentState */
  #define SERVICE_STOPPED                0x00000001
  #define SERVICE_START_PENDING          0x00000002
  #define SERVICE_STOP_PENDING           0x00000003
  #define SERVICE_RUNNING                0x00000004
  #define SERVICE_CONTINUE_PENDING       0x00000005
  #define SERVICE_PAUSE_PENDING          0x00000006
  #define SERVICE_PAUSED                 0x00000007

  ; <name of service:startstop name>
  nsSCM::Stop /NOUNLOAD <parameters>
  Pop $0 ; return error/success

  ; <name of service:startstop name>
  nsSCM::Remove /NOUNLOAD <parameters>
  Pop $0 ; return error/success

; Samples:
  ; Driver (boot stage starting)
  nsSCM::Install /NOUNLOAD "XXX" "XXX driver" 1 0 "$SYSDIR\drivers\XXX.sys" "" "" "" ""
  Pop $0 ; return error/success

  ; Driver (sscm stage starting)
  nsSCM::Install /NOUNLOAD "XXX" "XXX driver" 1 1 "$SYSDIR\drivers\XXX.sys" "" "" "" ""
  Pop $0 ; return error/success

  ; Driver (manual starting)
  nsSCM::Install /NOUNLOAD "XXX" "XXX driver" 1 3 "$SYSDIR\drivers\XXX.sys" "" "" "" ""
  Pop $0 ; return error/success

  ; Service (auto starting)
  nsSCM::Install /NOUNLOAD "XXX" "XXX service" 16 2 "$PROGRAMFILES\${PRJ_NAME}\XXX.exe" "" "" "" ""
  Pop $0 ; return error/success

  ; Service (manual starting)
  nsSCM::Install /NOUNLOAD "XXX" "XXX service" 16 3 "$PROGRAMFILES\${PRJ_NAME}\XXX.exe" "" "" "" ""
  Pop $0 ; return error/success

  nsSCM::Start /NOUNLOAD "XXX"
  Pop $0 ; return error/success

  nsSCM::QueryStatus /NOUNLOAD
  Pop $0 ; return error/success
  Pop $1 ; return service status

  IntCmp $1 4 lbl_Return ; check on running

						sl at eltrast.ru

NSIS Simple Service Plugin

This plugin contains basic service functions like start, stop the 
service or checking the service status. It also contains advanced 
service functions for example setting the service description, changed
the logon account, granting or removing the service logon privilege.




== Short Reference ==


SimpleSC::InstallService [name_of_service] [display_name] [service_type] [start_type] [service_commandline] [dependencies] [account] [password]
SimpleSC::RemoveService [name_of_service]

SimpleSC::StartService [name_of_service] [arguments]
SimpleSC::StopService [name_of_service] 
SimpleSC::PauseService [name_of_service] 
SimpleSC::ContinueService [name_of_service]
SimpleSC::RestartService [name_of_service] [arguments]
SimpleSC::ExistsService [name_of_service]

SimpleSC::GetServiceDisplayName [name_of_service]
SimpleSC::GetServiceName [display_name]
SimpleSC::GetServiceStatus [name_of_service]
SimpleSC::GetServiceBinaryPath [name_of_service]

SimpleSC::SetServiceDescription [name_of_service] [service_description]
SimpleSC::SetServiceStartType [name_of_service] [start_type]
SimpleSC::SetServiceLogon [name_of_service] [account] [password]

SimpleSC::GrantServiceLogonPrivilege [account]
SimpleSC::RemoveServiceLogonPrivilege [account]

SimpleSC::ServiceIsPaused [name_of_service]
SimpleSC::ServiceIsRunning [name_of_service]
SimpleSC::ServiceIsStopped [name_of_service]

SimpleSC::GetErrorMessage [error_code]


Parameters:

name_of_service - The name of the service used for Start/Stop commands and all further commands 

display_name - The name as shown in the service control manager applet in system control 

service_type - One of the following codes 
  1 - SERVICE_KERNEL_DRIVER - Driver service.
  2 - SERVICE_FILE_SYSTEM_DRIVER - File system driver service.
  16 - SERVICE_WIN32_OWN_PROCESS - Service that runs in its own process. (Should be used in most cases)
  32 - SERVICE_WIN32_SHARE_PROCESS - Service that shares a process with one or more other services. 
  256 - SERVICE_INTERACTIVE_PROCESS - The service can interact with the desktop. 
      Note: If you specify either SERVICE_WIN32_OWN_PROCESS or SERVICE_WIN32_SHARE_PROCESS, 
            and the service is running in the context of the LocalSystem account, 
            you can also specify this value. 
            Example: SERVICE_WIN32_OWN_PROCESS or SERVICE_INTERACTIVE_PROCESS - (16 or 256) = 272
      Note: Services cannot directly interact with a user as of Windows Vista. 
            Therefore, this technique should not be used in new code.
            See for more information: http://msdn2.microsoft.com/en-us/library/ms683502(VS.85).aspx          

start_type - one of the following codes 
  0 - SERVICE_BOOT_START - Driver boot stage start 
  1 - SERVICE_SYSTEM_START - Driver scm stage start 
  2 - SERVICE_AUTO_START - Service auto start (Should be used in most cases)
  3 - SERVICE_DEMAND_START - Driver/service manual start 
  4 - SERVICE_DISABLED - Driver/service disabled

service_commandline - The path to the binary including all necessary parameters 
dependencies - Needed services, controls which services have to be started before this one; use the forward slash "/" to add more more than one service
account - The username/account which should be used 
password - Password of the aforementioned account to be able to logon as a service 
           Note: If you do not specify account/password, the local system account will be used to run the service

arguments - Arguments passed to the service main function. 
            Note: Driver services do not receive these arguments.

error_code - Error code of a function

service_description - The description as shown in the service control manager applet in system control 




== The Sample Script ==


; Install a service - ServiceType own process - StartType automatic - NoDependencies - Logon as System Account
  SimpleSC::InstallService "MyService" "My Service Display Name" "16" "2" "C:\MyPath\MyService.exe" "" "" ""
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)

; Install a service - ServiceType interact with desktop - StartType automatic - Dependencies on "Windows Time Service" (w32time) and "WWW Publishing Service" (w3svc) - Logon as System Account
  SimpleSC::InstallService "MyService" "My Service Display Name" "272" "2" "C:\MyPath\MyService.exe" "w32time/w3svc" "" ""
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)

; Remove a service
  SimpleSC::RemoveService "MyService"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)

; Start a service
  SimpleSC::StartService "MyService" ""
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)

; Start a service with two arguments "/param1=true" "/param2=1"
  SimpleSC::StartService "MyService" "/param1=true /param2=1"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)
 
; Start a service with two arguments "-p param1" "-param2"
  SimpleSC::StartService "MyService" '"-p param1" -param2'
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)

; Stop a service
  SimpleSC::StopService "MyService"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)

; Pause a service
  SimpleSC::PauseService "MyService"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)

; Continue a service
  SimpleSC::ContinueService "MyService"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)

; Restart a service
  SimpleSC::RestartService "MyService" ""
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)

; Restart a service with two arguments "/param1=true" "/param2=1"
  SimpleSC::RestartService "MyService" "/param1=true /param2=1"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)

; Start a service with two arguments "-p param1" "-param2"
  SimpleSC::RestartService "MyService" '"-p param1" -param2'
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)

; Check if the service exists
  SimpleSC::ExistsService "MyService"
  Pop $0 ; returns an errorcode if the service doesn´t exists (<>0)/service exists (0)

; Get the displayname of a service
  SimpleSC::GetServiceDisplayName "MyService"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)
  Pop $1 ; return the displayname of the service

; Get the servicename of a service by the displayname
  SimpleSC::GetServiceName "MyService"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)
  Pop $1 ; return the servicename of the service

; Get the current status of a service
  SimpleSC::GetServiceStatus "MyService"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)
  Pop $1 ; return the status of the service
  ; Valid values are:
  ; 1 - SERVICE_STOPPED
  ; 2 - SERVICE_START_PENDING
  ; 3 - SERVICE_STOP_PENDING
  ; 4 - SERVICE_RUNNING
  ; 5 - SERVICE_CONTINUE_PENDING
  ; 6 - SERVICE_PAUSE_PENDING
  ; 7 - SERVICE_PAUSED

; Get the binary path of a service
  SimpleSC::GetServiceBinaryPath "MyService"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)
  Pop $1 ; return the binary path of the service

; Set the description of a service (Not supported on Windows NT 4.0)
  SimpleSC::SetServiceDescription "MyService" "Sample Description"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)

; Set the starttype to automatic of a service
  SimpleSC::SetServiceStartType "MyService" "2"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)

; Sets the service logon to a user and grant the user the "SeServiceLogonPrivilege"
  SimpleSC::SetServiceLogon "MyService" "MyServiceUser" "MyServiceUserPassword"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)
  IntCmp $0 0 +1 Done Done ; If successful grant the service logon privilege to "MyServiceUser"
    ; Note: Every serviceuser must have the ServiceLogonPrivilege to start the service
    SimpleSC::GrantServiceLogonPrivilege "MyServiceUser"
    Pop $0 ; returns an errorcode (<>0) otherwise success (0)
  Done:

; Remove the "SeServiceLogonPrivilege" from a user
  SimpleSC::RemoveServiceLogonPrivilege "MyServiceUser"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)

; Check if the service is paused
  SimpleSC::ServiceIsPaused "MyService"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)
  Pop $1 ; return 1 (service is paused) - return 0 (service is not paused)

; Check if the service is running
  SimpleSC::ServiceIsRunning "MyService"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)
  Pop $1 ; return 1 (service is running) - return 0 (service is not running)

; Check if the service is stopped
  SimpleSC::ServiceIsStopped "MyService"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)
  Pop $1 ; return 1 (service is stopped) - return 0 (service is not stopped)

; Show the error message if a function fails 
  SimpleSC::StopService "MyService"
  Pop $0 ; returns an errorcode (<>0) otherwise success (0)
  IntCmp $0 0 Done +1 +1 
    Push $0
    SimpleSC::GetErrorMessage
    Pop $0
    MessageBox MB_OK|MB_ICONSTOP "Stopping fails - Reason: $0"
  Done:




== Important Notes ==
- The function "SetServiceLogon" only works if the servicetype is 
  "SERVICE_WIN32_OWN_PROCESS"
- The functions "GetServiceDisplayName" or "SetServiceDisplayName" are only 
  available on systems higher than Windows NT. 
- If you change the logon of an service to a new user you have to grant him 
  the Service Logon Privilege. Otherwise the service cannot be started by 
  the user you have assigned.
- The functions StartService, StopService, PauseService and ContinueService uses
  a timeout of 30 seconds. This means the function must be executed within 30 seconds, 
  otherwise the functions will return an error.
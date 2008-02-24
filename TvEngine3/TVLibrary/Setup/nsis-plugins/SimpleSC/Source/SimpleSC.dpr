library SimpleSC;

uses
  NSIS, Windows, ServiceControl, LSASecurityControl, SysUtils;

function BoolToStr(Value: Boolean): String;
begin
  if Value then
    result := '1'
  else
    result := '0';
end;

procedure InstallService(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  DisplayName: String;
  ServiceType: Cardinal;
  StartType: Cardinal;
  BinaryPath: String;
  Dependencies: String;
  Username: String;
  Password: String;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;
  DisplayName := PopString;
  ServiceType := StrToInt(PopString);
  StartType := StrToInt(PopString);
  BinaryPath := PopString;
  Dependencies := PopString;
  Username := PopString;
  Password := PopString;

  ServiceResult := IntToStr(ServiceControl.InstallService(ServiceName, DisplayName, ServiceType, StartType, BinaryPath, Dependencies, Username, Password));
  PushString(ServiceResult);
end;

procedure RemoveService(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;

  ServiceResult := IntToStr(ServiceControl.RemoveService(ServiceName));
  PushString(ServiceResult);
end;

procedure StartService(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  ServiceArguments: String;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;
  ServiceArguments := PopString;
  ServiceResult := IntToStr(ServiceControl.StartService(ServiceName, ServiceArguments));
  PushString(ServiceResult);
end;

procedure StopService(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;
  ServiceResult := IntToStr(ServiceControl.StopService(ServiceName));
  PushString(ServiceResult)
end;

procedure PauseService(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;
  ServiceResult := IntToStr(ServiceControl.PauseService(ServiceName));
  PushString(ServiceResult)
end;

procedure ContinueService(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;
  ServiceResult := IntToStr(ServiceControl.ContinueService(ServiceName));
  PushString(ServiceResult)
end;

procedure GetServiceName(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
Var
  DisplayName: String;
  ServiceResult: String;
  ServiceName: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  DisplayName := PopString;
  ServiceResult := IntToStr(ServiceControl.GetServiceName(DisplayName, ServiceName));
  PushString(ServiceName);
  PushString(ServiceResult);
end;

procedure GetServiceDisplayName(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
Var
  ServiceName: String;
  DisplayName: String;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;
  ServiceResult := IntToStr(ServiceControl.GetServiceDisplayName(ServiceName, DisplayName));
  PushString(DisplayName);
  PushString(ServiceResult);
end;

procedure GetServiceStatus(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  Status: DWORD;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;
  ServiceResult := IntToStr(ServiceControl.GetServiceStatus(ServiceName, Status));
  PushString(IntToStr(Status));
  PushString(ServiceResult);
end;

procedure GetServiceBinaryPath(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  BinaryPath: String;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;
  ServiceResult := IntToStr(ServiceControl.GetServiceBinaryPath(ServiceName, BinaryPath));
  PushString(BinaryPath);
  PushString(ServiceResult);
end;

procedure GetServiceStartType(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  StartType: DWORD;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;
  ServiceResult := IntToStr(ServiceControl.GetServiceStartType(ServiceName, StartType));
  PushString(IntToStr(StartType));
  PushString(ServiceResult);
end;

procedure SetServiceDescription(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  Description: String;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;
  Description := PopString;
  ServiceResult := IntToStr(ServiceControl.SetServiceDescription(ServiceName, Description));
  PushString(ServiceResult);
end;

procedure SetServiceStartType(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  ServiceStartType: DWORD;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;
  ServiceStartType := StrToInt(PopString);
  ServiceResult := IntToStr(ServiceControl.SetServiceStartType(ServiceName, ServiceStartType));
  PushString(ServiceResult);
end;

procedure SetServiceLogon(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  Username: String;
  Password: String;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;
  Username := PopString;
  Password := PopString;

  ServiceResult := IntToStr(ServiceControl.SetServiceLogon(ServiceName, Username, Password));
  PushString(ServiceResult);
end;

procedure ServiceIsRunning(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  IsRunning: Boolean;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;
  ServiceResult := IntToStr(ServiceControl.ServiceIsRunning(ServiceName, IsRunning));
  PushString(BoolToStr(IsRunning));
  PushString(ServiceResult);
end;

procedure ServiceIsStopped(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  IsStopped: Boolean;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;
  ServiceResult := IntToStr(ServiceControl.ServiceIsStopped(ServiceName, IsStopped));
  PushString(BoolToStr(IsStopped));
  PushString(ServiceResult);
end;

procedure ServiceIsPaused(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  IsPaused: Boolean;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;
  ServiceResult := IntToStr(ServiceControl.ServiceIsPaused(ServiceName, IsPaused));
  PushString(BoolToStr(IsPaused));
  PushString(ServiceResult);
end;

procedure RestartService(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  ServiceArguments: String;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;
  ServiceArguments := PopString;
  ServiceResult := IntToStr(ServiceControl.RestartService(ServiceName, ServiceArguments));
  PushString(ServiceResult);
end;

procedure ExistsService(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ServiceName: String;
  ServiceResult: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ServiceName := PopString;

  ServiceResult := IntToStr(ServiceControl.ExistsService(ServiceName));
  PushString(ServiceResult);
end;

procedure GrantServiceLogonPrivilege(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  AccountName: String;
  LSAResult: String;
const
  SE_SERVICE_LOGON_RIGHT = 'SeServiceLogonRight';
begin
  Init(hwndParent, string_size, variables, stacktop);

  AccountName := PopString;

  LSAResult := IntToStr(LSASecurityControl.GrantPrivilege(AccountName, SE_SERVICE_LOGON_RIGHT));
  PushString(LSAResult);
end;

procedure RemoveServiceLogonPrivilege(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  AccountName: String;
  LSAResult: String;
const
  SE_SERVICE_LOGON_RIGHT = 'SeServiceLogonRight';
begin
  Init(hwndParent, string_size, variables, stacktop);

  AccountName := PopString;

  LSAResult := IntToStr(LSASecurityControl.RemovePrivilege(AccountName, SE_SERVICE_LOGON_RIGHT));
  PushString(LSAResult);
end;

procedure GetErrorMessage(const hwndParent: HWND; const string_size: integer;
  const variables: PChar; const stacktop: pointer); cdecl;
var
  ErrorCode: Integer;
  ErrorMessage: String;
begin
  Init(hwndParent, string_size, variables, stacktop);

  ErrorCode := StrToInt(PopString);

  ErrorMessage := ServiceControl.GetErrorMessage(ErrorCode);
  PushString(ErrorMessage);
end;

exports InstallService;
exports RemoveService;
exports StartService;
exports StopService;
exports PauseService;
exports ContinueService;
exports GetServiceName;
exports GetServiceDisplayName;
exports GetServiceStatus;
exports GetServiceBinaryPath;
exports GetServiceStartType;
exports SetServiceDescription;
exports SetServiceStartType;
exports SetServiceLogon;
exports ServiceIsRunning;
exports ServiceIsStopped;
exports ServiceIsPaused;
exports RestartService;
exports ExistsService;
exports GrantServiceLogonPrivilege;
exports RemoveServiceLogonPrivilege;
exports GetErrorMessage;

end.

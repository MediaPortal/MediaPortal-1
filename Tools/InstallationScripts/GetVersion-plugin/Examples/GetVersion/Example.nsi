Name "GetVersion Example"
OutFile "GetVersion.exe"
ShowInstDetails show

Page InstFiles

Section

 GetVersion::WindowsName
  Pop $R0
  DetailPrint "WindowsName:"
  DetailPrint "  $R0"

 GetVersion::WindowsType
  Pop $R0
  DetailPrint "WindowsType:"
  DetailPrint "  $R0"

 GetVersion::WindowsVersion
  Pop $R0
  DetailPrint "WindowsVersion:"
  DetailPrint "  $R0"

 GetVersion::WindowsServerName
  Pop $R0
  DetailPrint "WindowsServerName:"
  DetailPrint "  $R0"

 GetVersion::WindowsPlatformId
  Pop $R0
  DetailPrint "WindowsPlatformId:"
  DetailPrint "  $R0"

 GetVersion::WindowsPlatformArchitecture
  Pop $R0
  DetailPrint "WindowsPlatformArchitecture:"
  DetailPrint "  $R0"

 GetVersion::WindowsServicePack
  Pop $R0
  DetailPrint "WindowsServicePack:"
  DetailPrint "  $R0"

 GetVersion::WindowsServicePackBuild
  Pop $R0
  DetailPrint "WindowsServicePackBuild:"
  DetailPrint "  $R0"

SectionEnd
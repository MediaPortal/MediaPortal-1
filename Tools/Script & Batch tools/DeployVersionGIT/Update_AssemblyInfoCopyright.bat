IF NOT "%1"=="" (
  SET ARCH=%1
) ELSE (
  SET ARCH=x86
)

.\DeployVersionSVN\bin\%ARCH%\Release\DeployVersionGIT.exe /UpdateCopyright="2005-2026 Team MediaPortal" /git=..\..\..\mediaportal > update.log
.\DeployVersionSVN\bin\%ARCH%\Release\DeployVersionGIT.exe /UpdateCopyright="2005-2026 Team MediaPortal" /git=..\..\..\TvEngine3\TVLibrary >> update.log
.\DeployVersionSVN\bin\%ARCH%\Release\DeployVersionGIT.exe /UpdateCopyright="2005-2026 Team MediaPortal" /git=..\..\..\Common-MP-TVE3 >> update.log

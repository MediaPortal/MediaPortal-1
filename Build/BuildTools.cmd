@ECHO OFF

SETLOCAL ENABLEDELAYEDEXPANSION

:: Build
FOR %%p IN ("%PROGRAMFILES(x86)%" "%PROGRAMFILES%") DO (
  FOR %%s IN (2019 2022) DO (
    FOR %%e IN (Community Professional Enterprise BuildTools) DO (
      SET PF=%%p
      SET PF=!PF:"=!
      SET MSBUILD_PATH="!PF!\Microsoft Visual Studio\%%s\%%e\MSBuild\Current\Bin\MSBuild.exe"
      IF EXIST "!MSBUILD_PATH!" GOTO :BUILD
    )
  )
)

:BUILD

ECHO.
ECHO MSBuild Location: %MSBUILD_PATH%
ECHO.

:: DeployVersionGIT
ECHO - Build DeployVersionGIT ...
%MSBUILD_PATH% /target:Rebuild /property:Configuration=Release "%GIT_ROOT%\Tools\Script & Batch tools\DeployVersionGIT\DeployVersionGIT.sln"
ECHO.

:: SetRights
ECHO - Build SetRights ...
%MSBUILD_PATH% /target:Rebuild /property:Configuration=Release "%GIT_ROOT%\Tools\Script & Batch tools\SetRights\SetRights.sln"
ECHO.

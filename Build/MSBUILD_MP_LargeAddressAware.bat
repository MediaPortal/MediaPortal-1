@ECHO ON
REM set other MP related paths
set GIT_ROOT=..
set MediaPortal="%GIT_ROOT%\mediaportal"

SET CURRENTPATH=%~dp0
SET CURRENTLETTER=%~d0
cd %~dp0

echo.
echo Make MediaPortal 2GB LARGEADDRESSAWARE...
%CURRENTLETTER%
cd %CURRENTPATH%
REM call "%VS140COMNTOOLS%vsvars32.bat"
call "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat"
EditBin.exe %MediaPortal%\MediaPortal.Application\bin\%1\MediaPortal.exe /LARGEADDRESSAWARE

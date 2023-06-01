@ECHO OFF

if [%1]==[] (set ARCH=x86) ELSE (set ARCH=%1)
call "MSBUILD_Rebuild_Release_MediaPortal.bat" Debug %ARCH%

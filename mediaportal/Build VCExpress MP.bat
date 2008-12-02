@echo off


::   MP VC express solution build


REM Select program path based on current machine environment

set progpath=%ProgramFiles%
if not "%ProgramFiles(x86)%".=="". set progpath=%ProgramFiles(x86)%

@echo on

"%progpath%\Microsoft Visual Studio 8\Common7\IDE\vcsexpress.exe" /build Release MediaPortal.VC#Express.sln
xbmc\postbuild.bat

@echo off

REM Select program path based on current machine environment

set progpath=%ProgramFiles%
if not "%ProgramFiles(x86)%".=="". set progpath=%ProgramFiles(x86)%

@echo on

"%progpath%\Microsoft Visual Studio 8\Common7\IDE\devenv.com" /rebuild "Release|x86" MediaPortal.sln
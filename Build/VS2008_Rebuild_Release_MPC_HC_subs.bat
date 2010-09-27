@ECHO OFF

rem build init
set project=MPC_HC_subs
call BuildInit.bat %1

rem build
echo.
echo Building %project%
"%progpath%\Microsoft Visual Studio 9.0\Common7\IDE\devenv.com" "..\mediaportal\Core.cpp\mpc-hc_subs.sln" /Rebuild "%BUILD_TYPE%" >> %log%

@echo off
setlocal
if "%1"=="" goto noconfiguration
set release=%1
goto start
:noconfiguration
set release=Release
echo No configuration specified, defaulting to Release...
echo.
:start
echo Standby while running the tests from the %release% folder...
echo.
echo.
echo.
pushd MediaPortal.Tests
nunit-console bin\%release%\MediaPortal.Tests.dll /xml results.xml
nunitreport results.xml nunit-results.html
popd
start MediaPortal.Tests\nunit-results.html

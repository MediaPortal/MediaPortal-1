@ECHO OFF

REM detect if BUILD_TYPE should be release or debug
if not %1!==Debug! goto RELEASE
:DEBUG
set BUILD_TYPE=Debug
goto START
:RELEASE
set BUILD_TYPE=Release
goto START


:START
REM Select program path based on current machine environment
set progpath=%ProgramFiles%
if not "%ProgramFiles(x86)%".=="". set progpath=%ProgramFiles(x86)%


echo.
echo -= dshowhelper - Owlsroost =-
echo -= build mode: %BUILD_TYPE% =-
echo. > build.log
echo.


echo.
echo Building dshowhelper - Owlsroost...
"%progpath%\Microsoft Visual Studio 9.0\Common7\IDE\devenv.com" ".\dshowhelper\dshowhelper.sln" /Rebuild "%BUILD_TYPE%" >> build.log

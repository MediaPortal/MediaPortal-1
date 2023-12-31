@ECHO OFF
if [%1]==[] (set ARCH=x86) ELSE (set ARCH=%1)
"%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" /target:Rebuild /property:Configuration=Release;Platform=%ARCH% MediaPortal.sln"

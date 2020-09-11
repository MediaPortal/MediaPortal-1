REM Select program path based on current machine environment
set progpath=%ProgramFiles%
if not "%ProgramFiles(x86)%".=="". set progpath=%ProgramFiles(x86)%

REM Define MSbuild path
set MSBUILD_PATH=%progpath%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe

REM Download NuGet packages
@"%MSBUILD_PATH%" RestorePackages.targets  > Nuget_Restore.log
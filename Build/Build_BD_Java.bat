if not defined GIT_ROOT (
call .\BuildInit.bat
)

if not defined ant_home set ant_home=%NugetPackages%\ANT.1.10.7\tools

call %ant_home%\bin\ant -f %LibblurayJAR% -Dsrc_awt=:java-j2se

rem previous command backup
rem pushd %GIT_ROOT%\src\libbluray\bdj
rem call %ant_home%\bin\ant -Dsrc_awt=:java-j2se
rem popd
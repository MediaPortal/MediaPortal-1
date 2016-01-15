xcopy "..\..\DirectShowFilters\bin\Release\*.*" "_TvPluginDebug\MediaPortal\" /R /Y

cd "..\..\DirectShowFilters\bin\Release\"
pushd %MYPATH0%
set MYPATH0=%CD%
regsvr32 %CD%\cdxareader.ax /s
regsvr32 %CD%\mmaacd.ax /s
regsvr32 %CD%\PDMpgMux.ax /s
popd
cd "..\..\DVBSubtitle3\bin\Release\"
pushd %MYPATH1%
set MYPATH1=%CD%
regsvr32 %CD%\DVBSub3.ax /s
popd
cd "..\..\..\MPAudioswitcher\bin\Release\"
pushd %MYPATH2%
set MYPATH2=%CD%
regsvr32 %CD%\MPAudioSwitcher.ax /s
popd
cd "..\..\..\MPWriter\bin\Release\"
pushd %MYPATH3%
set MYPATH3=%CD%
regsvr32 %CD%\MPFileWriter.ax /s
popd
cd "..\..\..\TsReader\bin\Release\"
pushd %MYPATH4%
set MYPATH4=%CD%
regsvr32 %CD%\TsReader.ax /s
popd
cd "..\..\..\TsWriter\bin\Release\"
pushd %MYPATH5%
set MYPATH5=%CD%
regsvr32 %CD%\TsWriter.ax /s
popd

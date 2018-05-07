@echo off
rem set TARGET=Debug
set TARGET=Release

rem make sure that packages are restored, especially MediaInfo needs to be available!
@"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" ..\..\..\Build\RestorePackages.targets

cd MediaPortal.TvEngine.Core3
mkdir References
mkdir References\ProgramData
mkdir lib

rem Files not from build process, contained in repository
rem xcopy "..\..\TvService\bin\%TARGET%\castle.config" "References" /R /Y
rem xcopy "..\..\TvService\bin\%TARGET%\gentle.config" "References" /R /Y
rem xcopy "..\..\TvService\bin\%TARGET%\System.Data.SQLite.DLL" "References" /R /Y

xcopy "..\..\TvService\bin\%TARGET%\Castle.Core.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Castle.Windsor.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Common.Utils.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\DirectShowLib.dll" "References" /R /Y

xcopy "..\..\TvService\bin\%TARGET%\Gentle.*.dll" "References" /R /Y

xcopy "..\..\TvService\bin\%TARGET%\dxerr9.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\hauppauge.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\hcwWinTVCI.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\HttpServer.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Ionic.Zip.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\KNCBDACTRL.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\log4net.config" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\log4net.dll" "References" /R /Y
rem Morpheus_xx, 2016-03-05: Changed by MP1.14, needs to be adjusted in case of version changes!
rem xcopy "..\..\TvService\bin\%TARGET%\MediaInfo.dll" "References" /R /Y
xcopy "..\..\..\..\Packages\MediaInfo.0.7.69\MediaInfo.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\MySql.Data.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\PluginBase.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\PowerScheduler.Interfaces.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\SetupControls.dll" "References" /R /Y
xcopy "..\..\SetupTv\bin\%TARGET%\SetupTv.exe" "References" /R /Y
xcopy "..\..\SetupTv\bin\%TARGET%\SetupTv.exe.config" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\StreamingServer.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TeVii.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\ttBdaDrvApi_Dll.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\ttdvbacc.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvBusinessLayer.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvControl.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TVDatabase.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TVLibrary.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvLibrary.Interfaces.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvLibrary.Utils.dll" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvService.exe" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvService.exe.config" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvSetupLog.config" "References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvThumbnails.dll" "References" /R /Y

rem Note: Following files are required by TVE3, but they are contained in the MP2 host already. So we exclude them here, as the package is intended for MP2 use only.
rem xcopy "..\..\TvService\bin\%TARGET%\MediaPortal.Utilities.dll" "References" /R /Y
rem xcopy "..\..\TvService\bin\%TARGET%\UPnP.dll" "References" /R /Y

xcopy "..\..\TvService\bin\%TARGET%\Plugins\Blaster.exe" "References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\Blaster.exe.config" "References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\ComSkipLauncher.dll" "References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\ConflictsManager.dll" "References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\Ionic.Zip.dll" "References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\PluginBase.dll" "References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\PowerScheduler.dll" "References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\PowerScheduler.Interfaces.dll" "References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\ServerBlaster.dll" "References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\TvMovie.dll" "References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\WebEPG.dll" "References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\WebEPGImport.dll" "References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\XmlTvImport.dll" "References\Plugins\" /R /Y

REM #######################################
REM 	DirectShow filters
REM #######################################

xcopy "..\..\..\..\DirectShowFilters\MPWriter\bin\%TARGET%\MPFileWriter.ax" "References" /R /Y
xcopy "..\..\..\..\DirectShowFilters\TsMuxer\bin\%TARGET%\TsMuxer.ax" "References" /R /Y
xcopy "..\..\..\..\DirectShowFilters\StreamingServer\bin\%TARGET%\StreamingServer.dll" "References" /R /Y
xcopy "..\..\..\..\DirectShowFilters\MPIPTVSource\bin\%TARGET%\*.*" "References" /R /Y
xcopy "..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTVSource\MPIPTVSource.ini" "References" /R /Y
xcopy "..\..\..\..\DirectShowFilters\TsReader\bin\%TARGET%\TsReader.ax" "References" /R /Y
xcopy "..\..\..\..\DirectShowFilters\TsWriter\bin\%TARGET%\TsWriter.ax" "References" /R /Y
xcopy "..\..\..\..\DirectShowFilters\DXerr9\bin\%TARGET%\dxerr9.dll" "References" /R /Y
xcopy "..\..\..\..\DirectShowFilters\bin\Release\PDMpgMux.ax" "References" /R /Y

REM #######################################
REM 	Libraries
REM #######################################
xcopy "..\..\TvService\bin\%TARGET%\SetupControls.dll" "lib" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvService.exe" "lib" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Common.Utils.dll" "lib" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\PluginBase.dll" "lib" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\PluginBase.dll" "lib" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvBusinessLayer.dll" "lib" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvControl.dll" "lib" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TVDatabase.dll" "lib" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TVLibrary.dll" "lib" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvLibrary.Interfaces.dll" "lib" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvLibrary.Utils.dll" "lib" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvThumbnails.dll" "lib" /R /Y


REM #######################################
REM 	ProgramData base files
REM #######################################

:zip
rmdir /S /Q _tmpzip
mkdir _tmpzip
REM Thumbs folder is not created by code, but tried to delete from. So add it here as dummy.
mkdir _tmpzip\thumbs
xcopy /S /Q "..\..\TvServer.Base\TuningParameters" _tmpzip\TuningParameters\*.*
xcopy /S /Q "..\..\TvServer.Base\WebEPG" _tmpzip\WebEPG\*.*
xcopy /S /Q "..\..\TvServer.Base\xmltv" _tmpzip\xmltv\*.*
xcopy /S /Q "..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTVSource\MPIPTVSource.ini" _tmpzip\*.*
xcopy /S /Q "References\gentle.config" _tmpzip\*.*
del References\ProgramData\ProgramData.zip
powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('_tmpzip', 'References\ProgramData\ProgramData.zip'); }"
rmdir /S /Q _tmpzip

nuget pack MediaPortal.TvEngine.Core3.nuspec -OutputDirectory ..
cd ..
@echo off
rem set TARGET=Debug
set TARGET=Release

rem make sure that packages are restored, especially MediaInfo needs to be available!
@"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" ..\..\..\Build\RestorePackages.targets

cd MediaPortal.TvEngine.Core3
mkdir content\References
mkdir content\References\ProgramData
mkdir lib

rem Files not from build process, contained in repository
rem xcopy "..\..\TvService\bin\%TARGET%\castle.config" "content\References" /R /Y
rem xcopy "..\..\TvService\bin\%TARGET%\gentle.config" "content\References" /R /Y
rem xcopy "..\..\TvService\bin\%TARGET%\System.Data.SQLite.DLL" "content\References" /R /Y

xcopy "..\..\TvService\bin\%TARGET%\Castle.Core.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Castle.Windsor.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Common.Utils.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\DirectShowLib.dll" "content\References" /R /Y

xcopy "..\..\TvService\bin\%TARGET%\Gentle.*.dll" "content\References" /R /Y

xcopy "..\..\TvService\bin\%TARGET%\dxerr9.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\hauppauge.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\hcwWinTVCI.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\HttpServer.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Ionic.Zip.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\KNCBDACTRL.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\log4net.config" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\log4net.dll" "content\References" /R /Y
rem Morpheus_xx, 2016-03-05: Changed by MP1.14, needs to be adjusted in case of version changes!
rem xcopy "..\..\TvService\bin\%TARGET%\MediaInfo.dll" "content\References" /R /Y
xcopy "..\..\..\..\Packages\MediaInfo.0.7.69\MediaInfo.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\MySql.Data.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\PluginBase.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\PowerScheduler.Interfaces.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\SetupControls.dll" "content\References" /R /Y
xcopy "..\..\SetupTv\bin\%TARGET%\SetupTv.exe" "content\References" /R /Y
xcopy "..\..\SetupTv\bin\%TARGET%\SetupTv.exe.config" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\StreamingServer.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TeVii.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\ttBdaDrvApi_Dll.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\ttdvbacc.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvBusinessLayer.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvControl.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TVDatabase.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TVLibrary.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvLibrary.Interfaces.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvLibrary.Utils.dll" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvService.exe" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvService.exe.config" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvSetupLog.config" "content\References" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\TvThumbnails.dll" "content\References" /R /Y

rem Note: Following files are required by TVE3, but they are contained in the MP2 host already. So we exclude them here, as the package is intended for MP2 use only.
rem xcopy "..\..\TvService\bin\%TARGET%\MediaPortal.Utilities.dll" "content\References" /R /Y
rem xcopy "..\..\TvService\bin\%TARGET%\UPnP.dll" "content\References" /R /Y

xcopy "..\..\TvService\bin\%TARGET%\Plugins\Blaster.exe" "content\References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\Blaster.exe.config" "content\References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\ComSkipLauncher.dll" "content\References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\ConflictsManager.dll" "content\References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\Ionic.Zip.dll" "content\References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\PluginBase.dll" "content\References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\PowerScheduler.dll" "content\References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\PowerScheduler.Interfaces.dll" "content\References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\ServerBlaster.dll" "content\References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\TvMovie.dll" "content\References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\WebEPG.dll" "content\References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\WebEPGImport.dll" "content\References\Plugins\" /R /Y
xcopy "..\..\TvService\bin\%TARGET%\Plugins\XmlTvImport.dll" "content\References\Plugins\" /R /Y

REM #######################################
REM 	DirectShow filters
REM #######################################

xcopy "..\..\..\..\DirectShowFilters\MPWriter\bin\%TARGET%\MPFileWriter.ax" "content\References" /R /Y
xcopy "..\..\..\..\DirectShowFilters\TsMuxer\bin\%TARGET%\TsMuxer.ax" "content\References" /R /Y
xcopy "..\..\..\..\DirectShowFilters\StreamingServer\bin\%TARGET%\StreamingServer.dll" "content\References" /R /Y
xcopy "..\..\..\..\DirectShowFilters\MPIPTVSource\bin\%TARGET%\*.*" "content\References" /R /Y
xcopy "..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTVSource\MPIPTVSource.ini" "content\References" /R /Y
xcopy "..\..\..\..\DirectShowFilters\TsReader\bin\%TARGET%\TsReader.ax" "content\References" /R /Y
xcopy "..\..\..\..\DirectShowFilters\TsWriter\bin\%TARGET%\TsWriter.ax" "content\References" /R /Y
xcopy "..\..\..\..\DirectShowFilters\DXerr9\bin\%TARGET%\dxerr9.dll" "content\References" /R /Y
xcopy "..\..\..\..\DirectShowFilters\bin\Release\PDMpgMux.ax" "content\References" /R /Y

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
xcopy /S /Q "content\References\gentle.config" _tmpzip\*.*
del content\References\ProgramData\ProgramData.zip
powershell.exe -nologo -noprofile -command "& { Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::CreateFromDirectory('_tmpzip', 'content\References\ProgramData\ProgramData.zip'); }"
rmdir /S /Q _tmpzip

nuget pack MediaPortal.TvEngine.Core3.nuspec -OutputDirectory ..
cd ..
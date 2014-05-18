REM %1 = Solution Directory
REM %2 = $(OutDir) bin/Debug bin/Release
REM %3 = $(TargetDir)

REM Identify configuration path for <=XP or >=Vista
if exist %ProgramData%\nul (
	set ConfigPath=%ProgramData%
) else (
	set ConfigPath="%AllUsersProfile%\Application Data"
)

xcopy "%1..\ExternalBinaries\*.*"  /s /i /y

xcopy "%1Plugins\ComSkipLauncher\%2Mediaportal.TV.Server.Plugins.ComSkipLauncher.*" "%3Plugins\" /C/Y/S/D
xcopy "%1Plugins\ConflictsManager\%2Mediaportal.TV.Server.Plugins.ConflictsManager.*" "%3Plugins\" /C/Y/S/D
rem xcopy "%1Plugins\PluginBase\%2Mediaportal.TV.Server.Plugins.Base.*" "%3"Plugins\ /C/Y/S/D
xcopy "%1Plugins\PowerScheduler\%2Mediaportal.TV.Server.Plugins.PowerScheduler.dll" "%3Plugins\" /C/Y/S/D
xcopy "%1Plugins\PowerScheduler\%2Mediaportal.TV.Server.Plugins.PowerScheduler.pdb" "%3Plugins\" /C/Y/S/D
rem xcopy "%1Plugins\PowerScheduler\%2Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.dll" "%3\Plugins\" /C/Y/S/D
rem xcopy "%1Plugins\PowerScheduler\%2Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.pdb" "%3\Plugins\" /C/Y/S/D
xcopy "%1Plugins\ServerBlaster\ServerBlaster\%2Mediaportal.TV.Server.Plugins.ServerBlaster.*" "%3Plugins\" /C/Y/S/D
xcopy "%1Plugins\ServerBlaster\ServerBlaster.Learn\%2Mediaportal.TV.Server.Plugins.ServerBlaster.Learn.*" "%3Plugins\" /C/Y/S/D
rem xcopy "%1Plugins\TvMovie\%2Mediaportal.TV.Server.Plugins.TvMovie.*" "%3Plugins\" /C/Y/S/D
xcopy "%1Plugins\WebEPG\WebEPG\%2Mediaportal.TV.Server.Plugins.WebEPG.*" "%3Plugins\" /C/Y/S/D
xcopy "%1Plugins\WebEPG\WebEPGPlugin\%2Mediaportal.TV.Server.Plugins.WebEPGImport.*" "%3Plugins\" /C/Y/S/D
xcopy "%1Plugins\XmlTvImport\%2Mediaportal.TV.Server.Plugins.XmlTvImport.*" "%3"Plugins\ /C/Y/S/D
xcopy "%1Plugins\XmlTvImport\%2Ionic.Zip.Reduced.dll" "%3"Plugins\ /C/Y/S/D

xcopy "%1\SetupControls\%2Mediaportal.TV.Server.SetupControls.*" "%3" /C/Y/S/D
xcopy "%1..\..\..\..\Common-MP-TVE3\PowerScheduler.Interfaces\%2Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces" "%3" /C/Y/S/D
xcopy "%1RuleBasedScheduler\%2Mediaportal.TV.Server.RuleBasedScheduler.*" "%3" /C/Y/S/D

xcopy "%1Plugins\CustomDevices\Anysee\%2Mediaportal.TV.Server.Plugins.TunerExtension.Anysee.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\AVerMedia\%2Mediaportal.TV.Server.Plugins.TunerExtension.AVerMedia.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\Compro\%2Mediaportal.TV.Server.Plugins.TunerExtension.Compro.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\Conexant\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\DigitalDevices\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\DigitalEverywhere\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\DvbSky\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\DvbWorld\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\Geniatech\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\Genpix\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\GenpixOpenSource\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\HauppaugeBda\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\HauppaugeEcp\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\HauppaugeEncoder\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\HauppaugeRemote\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\Knc\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\MdPlugin\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\MicrosoftAtscQam\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\MicrosoftBdaDiseqc\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\MicrosoftEncoder\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\MicrosoftOldDiseqc\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\MicrosoftPidFilter\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\NetUp\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\Omicom\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\Prof\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\ProfUsb\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\Realtek\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\SmarDtvUsbCi\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\TechnoTrend\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\TeVii\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\Turbosight\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\TurbosightRemote\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\Twinhan\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\ViXS\%2Mediaportal.TV.Server.Plugins.TunerExtension.*" "%3Plugins\CustomDevices\" /C/Y/S/D

xcopy "%1Plugins\CustomDevices\Anysee\%2CIAPI.*" "%3Plugins\CustomDevices\Resources\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\HauppaugeEcp\%2HauppaugeEcp.*" "%3Plugins\CustomDevices\Resources\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\Knc\%2KNCBDACTRL.*" "%3Plugins\CustomDevices\Resources\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\TechnoTrend\%2ttBdaDrvApi_Dll.*" "%3Plugins\CustomDevices\Resources\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\TeVii\%2TeVii.*" "%3Plugins\CustomDevices\Resources\" /C/Y/S/D
xcopy "%1Plugins\CustomDevices\Turbosight\%2TbsCIapi.*" "%3Plugins\CustomDevices\Resources\" /C/Y/S/D

xcopy "%1TvLibrary.IntegrationProvider.Interfaces\%2*.dll" "%3" /C/Y/S/D
xcopy "%1TvLibrary.IntegrationProvider.Interfaces\%2*.pdb" "%3" /C/Y/S/D
xcopy "%1TvLibrary.Integration.MP1\%2*.dll" "%3" /C/Y/S/D
xcopy "%1TvLibrary.Integration.MP1\%2*.pdb" "%3" /C/Y/S/D
xcopy "%1TvLibrary.Integration.MP1\%2*.dll" "%1..\..\..\..\mediaportal\MediaPortal.Application\%2" /C/Y/S/D
xcopy "%1TvLibrary.Integration.MP1\%2*.pdb" "%1..\..\..\..\mediaportal\MediaPortal.Application\%2" /C/Y/S/D

xcopy "%1..\..\..\..\DirectShowFilters\DXErr9\bin\Release\dxerr9*.dll" . /Y /D
xcopy "%1..\..\..\..\DirectShowFilters\MPIPTVSource\bin\Release\MPIPTV_FILE*.dll" . /Y /D
xcopy "%1..\..\..\..\DirectShowFilters\MPIPTVSource\bin\Release\MPIPTV_HTTP*.dll" . /Y /D
xcopy "%1..\..\..\..\DirectShowFilters\MPIPTVSource\bin\Release\MPIPTV_RTP*.dll" . /Y /D
xcopy "%1..\..\..\..\DirectShowFilters\MPIPTVSource\bin\Release\MPIPTV_RTSP*.dll" . /Y /D
xcopy "%1..\..\..\..\DirectShowFilters\MPIPTVSource\bin\Release\MPIPTV_UDP*.dll" . /Y /D
xcopy "%1..\..\..\..\DirectShowFilters\MPIPTVSource\bin\Release\MPIPTVSource*.ax" . /Y /D
xcopy "%1..\..\..\..\DirectShowFilters\StreamingServer\bin\Release\StreamingServer*.dll" . /Y /D
xcopy "%1..\..\..\..\DirectShowFilters\TsMuxer\bin\Release\TsMuxer*.ax" . /Y /D
xcopy "%1..\..\..\..\DirectShowFilters\TsWriter\bin\Release\TsWriter*.ax" . /Y /D

REM SetupTV
xcopy "%1TvLibrary.Integration.MP1\%2Mediaportal.TV.Server.TVLibrary.Integration.MP1*.pdb" "%1SetupTv\%2Integration\" /C/Y/S
xcopy "%1TvLibrary.Integration.MP1\%2Mediaportal.TV.Server.TVLibrary.Integration.MP1*.dll" "%1SetupTv\%2Integration\" /C/Y/S

REM TuningParameters
xcopy %1TVServer.Base\TuningParameters\*.* %ConfigPath%\"Team MediaPortal\\MediaPortal TV Server\TuningParameters\" /E /Y /D /Q

xcopy %1TVServer.Base\WebEPG\*.* %ConfigPath%\"Team MediaPortal\\MediaPortal TV Server\WebEPG\" /E /Y /D /Q
xcopy %1TVServer.Base\xmltv\*.* %ConfigPath%\"Team MediaPortal\\MediaPortal TV Server\xmltv\" /E /Y /D /Q
xcopy "%1..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTVSource\MPIPTVSource*.ini" %ConfigPath%\"Team MediaPortal\\MediaPortal TV Server\" /E /Y /D /Q


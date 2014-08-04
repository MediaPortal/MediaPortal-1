REM %1 = Solution Directory
REM %2 = $(PlatformName) (eg. x86)
REM %3 = $(ConfigurationName) (eg. Release, Debug)
REM %4 = $(TargetDir)

REM Identify configuration path for <=XP or >=Vista
if exist %ProgramData%\nul (
	set ConfigPath=%ProgramData%
) else (
	set ConfigPath="%AllUsersProfile%\Application Data"
)


rem --- external binaries ---
xcopy "%1..\ExternalBinaries\*" "%4" /Y/D


rem --- integration ---
xcopy "%1TvLibrary.Integration.MP1\bin\%3\Mediaportal.TV.Server.TVLibrary.Integration*" "%4" /Y/D
xcopy "%1TvLibrary.Integration.MP1\bin\%3\Castle.Facilities.*" "%4" /Y/D
xcopy "%1TvLibrary.Integration.MP1\bin\%3\Castle.Services.*" "%4" /Y/D


rem --- plugins ---
xcopy "%1Plugins\ComSkipLauncher\bin\%3\Mediaportal.TV.Server.Plugins.ComSkipLauncher.*" "%4Plugins\" /Y/D
xcopy "%1Plugins\ConflictsManager\bin\%3\Mediaportal.TV.Server.Plugins.ConflictsManager.*" "%4Plugins\" /Y/D
rem xcopy "%1Plugins\PluginBase\bin\%3\Mediaportal.TV.Server.Plugins.Base.*" "%4"Plugins\ /Y/D
xcopy "%1Plugins\PowerScheduler\bin\%3\Mediaportal.TV.Server.Plugins.PowerScheduler.dll" "%4Plugins\" /Y/D
xcopy "%1Plugins\PowerScheduler\bin\%3\Mediaportal.TV.Server.Plugins.PowerScheduler.pdb" "%4Plugins\" /Y/D
rem xcopy "%1Plugins\PowerScheduler\bin\%3\Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.dll" "%4\Plugins\" /Y/D
rem xcopy "%1Plugins\PowerScheduler\bin\%3\Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.pdb" "%4\Plugins\" /Y/D
xcopy "%1Plugins\ServerBlaster\ServerBlaster\bin\%3\Mediaportal.TV.Server.Plugins.ServerBlaster.*" "%4Plugins\" /Y/D
xcopy "%1Plugins\ServerBlaster\ServerBlaster.Learn\bin\%3\Mediaportal.TV.Server.Plugins.ServerBlaster.Learn.*" "%4Plugins\" /Y/D
rem xcopy "%1Plugins\TvMovie\bin\%3\Mediaportal.TV.Server.Plugins.TvMovie.*" "%4Plugins\" /Y/D
xcopy "%1Plugins\WebEPG\WebEPG\bin\%3\Mediaportal.TV.Server.Plugins.WebEPG.*" "%4Plugins\" /Y/D
xcopy "%1Plugins\WebEPG\WebEPGPlugin\bin\%3\Mediaportal.TV.Server.Plugins.WebEPGImport.*" "%4Plugins\" /Y/D
xcopy "%1Plugins\XmlTvImport\bin\%3\Mediaportal.TV.Server.Plugins.XmlTvImport.*" "%4"Plugins\ /Y/D
xcopy "%1Plugins\XmlTvImport\bin\%3\Ionic.Zip.Reduced.dll" "%4"Plugins\ /Y/D


rem --- tuner extensions ---
xcopy "%1Plugins\CustomDevices\Anysee\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Anysee.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\AutumnWave\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.AutumnWave.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\AVerMedia\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.AVerMedia.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\Compro\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Compro.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\Conexant\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Conexant.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\DigitalDevices\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.DigitalDevices.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\DigitalEverywhere\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.DigitalEverywhere.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\DvbSky\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.DvbSky.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\DvbWorld\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.DvbWorld.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\Geniatech\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Geniatech.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\Genpix\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Genpix.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\GenpixOpenSource\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.GenpixOpenSource.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\HauppaugeBda\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeBda.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\HauppaugeEcp\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeEcp.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\HauppaugeEncoder\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeEncoder.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\HauppaugeRemote\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeRemote.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\Knc\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Knc.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\Kworld\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Kworld.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\MdPlugin\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.MdPlugin.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\MicrosoftAtscQam\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftAtscQam.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\MicrosoftBdaDiseqc\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBdaDiseqc.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\MicrosoftEncoder\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftEncoder.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\MicrosoftOldDiseqc\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftOldDiseqc.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\MicrosoftPidFilter\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftPidFilter.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\NetUp\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.NetUp.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\Omicom\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Omicom.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\Prof\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Prof.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\ProfUsb\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.ProfUsb.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\Realtek\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Realtek.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\SmarDtvUsbCi\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\TechnoTrend\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.TechnoTrend.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\TeVii\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.TeVii.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\Turbosight\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Turbosight.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\TurbosightRemote\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.TurbosightRemote.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\Twinhan\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\TwinhanHidRemote\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.TwinhanHidRemote.*" "%4Plugins\CustomDevices\" /Y/D
xcopy "%1Plugins\CustomDevices\ViXS\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.ViXS.*" "%4Plugins\CustomDevices\" /Y/D


rem --- tuner extension resources ---
xcopy "%1Plugins\CustomDevices\Anysee\bin\%3\CIAPI.*" "%4Plugins\CustomDevices\Resources\" /Y/D
xcopy "%1Plugins\CustomDevices\HauppaugeEcp\bin\%3\HauppaugeEcp.*" "%4Plugins\CustomDevices\Resources\" /Y/D
xcopy "%1Plugins\CustomDevices\Knc\bin\%3\KNCBDACTRL.*" "%4Plugins\CustomDevices\Resources\" /Y/D
xcopy "%1Plugins\CustomDevices\TechnoTrend\bin\%3\ttBdaDrvApi_Dll.*" "%4Plugins\CustomDevices\Resources\" /Y/D
xcopy "%1Plugins\CustomDevices\TeVii\bin\%3\TeVii.*" "%4Plugins\CustomDevices\Resources\" /Y/D
xcopy "%1Plugins\CustomDevices\Turbosight\bin\%3\TbsCIapi.*" "%4Plugins\CustomDevices\Resources\" /Y/D
xcopy "%1Plugins\CustomDevices\TurbosightRemote\bin\%3\TbsNxpIrRcReceiver.*" "%4Plugins\CustomDevices\Resources\" /Y/D


rem --- unmanaged dependencies ---
xcopy "%1..\..\..\..\DirectShowFilters\MPIPTVSource\bin\%3\*.ax" "%4" /Y/D
xcopy "%1..\..\..\..\DirectShowFilters\MPIPTVSource\bin\%3\*.dll" "%4" /Y/D
xcopy "%1..\..\..\..\DirectShowFilters\MPIPTVSource\bin\%3\*.pdb" "%4" /Y/D
xcopy "%1..\..\..\..\DirectShowFilters\StreamingServer\bin\%3\*.dll" "%4" /Y/D
xcopy "%1..\..\..\..\DirectShowFilters\StreamingServer\bin\%3\*.pdb" "%4" /Y/D
xcopy "%1..\..\..\..\DirectShowFilters\TsMuxer\bin\%3\*.ax" "%4" /Y/D
xcopy "%1..\..\..\..\DirectShowFilters\TsMuxer\bin\%3\*.pdb" "%4" /Y/D
xcopy "%1..\..\..\..\DirectShowFilters\TsWriter\bin\%3\*.ax" "%4" /Y/D
xcopy "%1..\..\..\..\DirectShowFilters\TsWriter\bin\%3\*.pdb" "%4" /Y/D


rem --- other ---
xcopy "%1\SetupControls\bin\%3\Mediaportal.TV.Server.SetupControls.*" "%4" /Y/D
xcopy "%1..\..\..\..\Common-MP-TVE3\PowerScheduler.Interfaces\bin\%3\Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.*" "%4" /Y/D
xcopy "%1RuleBasedScheduler\bin\%3\Mediaportal.TV.Server.RuleBasedScheduler.*" "%4" /Y/D


rem why are these needed?
rem xcopy "%1TvLibrary.Integration.MP1\bin\%3\*.dll" "%1..\..\..\..\mediaportal\MediaPortal.Application\bin\%3\" /Y/D
rem xcopy "%1TvLibrary.Integration.MP1\bin\%3\*.pdb" "%1..\..\..\..\mediaportal\MediaPortal.Application\bin\%3\" /Y/D


rem should we really replace installed files?
rem xcopy %1TVServer.Base\WebEPG\*.* %ConfigPath%\"Team MediaPortal\\MediaPortal TV Server\WebEPG\" /E /Y /D /Q
rem xcopy %1TVServer.Base\xmltv\*.* %ConfigPath%\"Team MediaPortal\\MediaPortal TV Server\xmltv\" /E /Y /D /Q
rem xcopy "%1..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTVSource\MPIPTVSource*.ini" %ConfigPath%\"Team MediaPortal\\MediaPortal TV Server\" /E /Y /D /Q
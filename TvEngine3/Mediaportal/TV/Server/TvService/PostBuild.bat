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



rem --- integration ---
xcopy "%1TvLibrary.Integration.MP1\bin\%3\Mediaportal.TV.Server.TVLibrary.Integration*" "%4" /Y/D
xcopy "%1TvLibrary.Integration.MP1\bin\%3\Castle.Facilities.*" "%4" /Y/D
xcopy "%1TvLibrary.Integration.MP1\bin\%3\Castle.Services.*" "%4" /Y/D


rem --- plugins ---
xcopy "%1Plugins\ComSkipLauncher\bin\%3\Mediaportal.TV.Server.Plugins.ComSkipLauncher.*" "%4Plugins\" /Y/D
rem xcopy "%1Plugins\PluginBase\bin\%3\Mediaportal.TV.Server.Plugins.Base.*" "%4"Plugins\ /Y/D
xcopy "%1Plugins\PowerScheduler\bin\%3\Mediaportal.TV.Server.Plugins.PowerScheduler.dll" "%4Plugins\" /Y/D
xcopy "%1Plugins\PowerScheduler\bin\%3\Mediaportal.TV.Server.Plugins.PowerScheduler.pdb" "%4Plugins\" /Y/D
rem xcopy "%1Plugins\PowerScheduler\bin\%3\Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.dll" "%4\Plugins\" /Y/D
rem xcopy "%1Plugins\PowerScheduler\bin\%3\Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.pdb" "%4\Plugins\" /Y/D
xcopy "%1Plugins\ServerBlaster\ServerBlaster\bin\%3\Mediaportal.TV.Server.Plugins.ServerBlaster.*" "%4Plugins\" /Y/D
xcopy "%1Plugins\ServerBlaster\ServerBlaster.Learn\bin\%3\Mediaportal.TV.Server.Plugins.ServerBlaster.Learn.*" "%4Plugins\" /Y/D
xcopy "%1Plugins\TvMovieImport\bin\%3\Mediaportal.TV.Server.Plugins.TvMovieImport.*" "%4Plugins\" /Y/D
xcopy "%1Plugins\WebEPG\WebEPG\bin\%3\Mediaportal.TV.Server.Plugins.WebEPG.*" "%4Plugins\" /Y/D
xcopy "%1Plugins\WebEPG\WebEpg.Utils\bin\%3\Mediaportal.TV.Server.Plugins.WebEpg.Utils.*" "%4Plugins\" /Y/D
xcopy "%1Plugins\WebEPG\WebEPGPlugin\bin\%3\Mediaportal.TV.Server.Plugins.WebEPGImport.*" "%4Plugins\" /Y/D
xcopy "%1Plugins\XmlTvImport\bin\%3\Mediaportal.TV.Server.Plugins.XmlTvImport.*" "%4"Plugins\ /Y/D
xcopy "%1Plugins\XmlTvImport\bin\%3\Ionic.Zip.Reduced.dll" "%4"Plugins\ /Y/D


rem --- tuner extensions ---
xcopy "%1Plugins\TunerExtensions\Anysee\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Anysee.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\AutumnWave\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.AutumnWave.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\AVerMedia\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.AVerMedia.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\Compro\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Compro.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\Conexant\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Conexant.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\DigitalDevices\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.DigitalDevices.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\DigitalEverywhere\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.DigitalEverywhere.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\DirecTvShef\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.DirecTvShef.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\DvbSky\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.DvbSky.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\DvbWorld\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.DvbWorld.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\Empia\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Empia.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\Geniatech\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Geniatech.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\Genpix\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Genpix.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\GenpixOpenSource\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.GenpixOpenSource.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\HauppaugeBda\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeBda.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\HauppaugeBlaster\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeBlaster.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\HauppaugeEcp\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeEcp.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\HauppaugeEncoder\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeEncoder.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\HauppaugeRemote\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeRemote.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\Knc\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Knc.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\Kworld\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Kworld.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\MdPlugin\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.MdPlugin.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\MicrosoftAtscQam\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftAtscQam.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\MicrosoftBdaDiseqc\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBdaDiseqc.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\MicrosoftBlaster\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\MicrosoftEncoder\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftEncoder.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\MicrosoftOldDiseqc\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftOldDiseqc.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\MicrosoftPidFilter\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftPidFilter.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\MicrosoftStreamSelector\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftStreamSelector.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\NetUp\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.NetUp.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\Omicom\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Omicom.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\Prof\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Prof.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\ProfUsb\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.ProfUsb.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\Realtek\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Realtek.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\SmarDtvUsbCi\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.SmarDtvUsbCi.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\TechnoTrend\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.TechnoTrend.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\TeVii\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.TeVii.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\TeViiBda\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.TeViiBda.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\Turbosight\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Turbosight.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\Twinhan\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\UsbUirt\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.UsbUirt.*" "%4Plugins\TunerExtensions\" /Y/D
xcopy "%1Plugins\TunerExtensions\ViXS\bin\%3\Mediaportal.TV.Server.Plugins.TunerExtension.ViXS.*" "%4Plugins\TunerExtensions\" /Y/D


rem --- tuner extension resources ---
xcopy "%1Plugins\TunerExtensions\Anysee\bin\%3\CIAPI.*" "%4Plugins\TunerExtensions\Resources\" /Y/D
xcopy "%1Plugins\TunerExtensions\HauppaugeEcp\bin\%3\HauppaugeEcp.*" "%4Plugins\TunerExtensions\Resources\" /Y/D
xcopy "%1Plugins\TunerExtensions\Knc\bin\%3\KNCBDACTRL.*" "%4Plugins\TunerExtensions\Resources\" /Y/D
xcopy "%1Plugins\TunerExtensions\TechnoTrend\bin\%3\ttBdaDrvApi_Dll.*" "%4Plugins\TunerExtensions\Resources\" /Y/D
xcopy "%1Plugins\TunerExtensions\TeVii\bin\%3\TeVii.*" "%4Plugins\TunerExtensions\Resources\" /Y/D
xcopy "%1Plugins\TunerExtensions\Turbosight\bin\%3\TbsCIapi.*" "%4Plugins\TunerExtensions\Resources\" /Y/D
xcopy "%1Plugins\TunerExtensions\Turbosight\bin\%3\TbsNxpIrRcReceiver.*" "%4Plugins\TunerExtensions\Resources\" /Y/D


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
xcopy "%1..\..\..\..\Common-MP-TVE3\PowerScheduler.Interfaces\bin\%3\Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.*" "%4" /Y/D
xcopy "%1TVDatabase\EntityModel\bin\%3\MySql.Data.*" "%4" /Y/D
xcopy "%1RuleBasedScheduler\bin\%3\Mediaportal.TV.Server.RuleBasedScheduler.*" "%4" /Y/D
xcopy "%1SetupControls\bin\%3\Mediaportal.TV.Server.SetupControls.*" "%4" /Y/D


rem why are these needed?
rem xcopy "%1TvLibrary.Integration.MP1\bin\%3\*.dll" "%1..\..\..\..\mediaportal\MediaPortal.Application\bin\%3\" /Y/D
rem xcopy "%1TvLibrary.Integration.MP1\bin\%3\*.pdb" "%1..\..\..\..\mediaportal\MediaPortal.Application\bin\%3\" /Y/D


rem should we really replace installed files?
rem xcopy %1TVServer.Base\WebEPG\*.* %ConfigPath%\"Team MediaPortal\\MediaPortal TV Server\WebEPG\" /E /Y /D /Q
rem xcopy "%1..\..\..\..\DirectShowFilters\MPIPTVSource\MPIPTVSource\MPIPTVSource*.ini" %ConfigPath%\"Team MediaPortal\\MediaPortal TV Server\" /E /Y /D /Q
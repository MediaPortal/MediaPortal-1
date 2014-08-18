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
md "%4Integration"
xcopy "%1TvLibrary.Integration.MP1\bin\%3\Mediaportal.TV.Server.TVLibrary.Integration*" "%4Integration" /Y/D
xcopy "%1TvLibrary.Integration.MP1\bin\%3\Castle.Facilities.*" "%4" /Y/D
xcopy "%1TvLibrary.Integration.MP1\bin\%3\Castle.Services.*" "%4" /Y/D


rem --- other ---
xcopy "%1..\..\..\..\Common-MP-TVE3\PowerScheduler.Interfaces\bin\%3\Mediaportal.TV.Server.Plugins.PowerScheduler.Interfaces.*" "%4" /Y/D
xcopy "%1TVLibrary.Utils\bin\%3\Mediaportal.TV.Server.TVLibrary.Utils.*" "%4" /Y/D


rem should we really replace installed files?
rem xcopy %1TVServer.Base\TuningParameters\*.* %ConfigPath%\"Team MediaPortal\\MediaPortal TV Server\TuningParameters\" /E /Y /D /Q
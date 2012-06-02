REM %1 = Solution Directory
REM %2 = $(ConfigurationName) Debug/Release


REM Identify configuration path for <=XP or >=Vista
if exist %ProgramData%\nul (
	set ConfigPath="%ProgramData%" 
) else (
	set ConfigPath="%AllUsersProfile%\Application Data"
)

REM Check for Microsoft Antispyware .BAT bug
if exist .\kernel32.dll exit 1

REM Hack to remove dll not needed in root 
del *.dll
del *.ax
REM Support
xcopy %1\MediaPortal.Support\bin\%2\MediaPortal.Support.* . /Y /D

REM Configuration
xcopy %1\Configuration\Wizards\*.* Wizards\ /Y /D
xcopy %1\Configuration\bin\%2\Configuration.* . /Y /D
xcopy %1\Configuration\WinCustomControls\bin\%2\WinCustomControls.* . /Y /D

REM Core
xcopy %1\core\bin\%2\DirectShowLib.* . /Y /D
xcopy %1\core\bin\%2\Core.* . /Y /D

REM Databases
xcopy %1\databases\bin\%2\databases.* . /Y /D

REM Common Utils
xcopy %1\..\Common-MP-TVE3\Common.Utils\bin\%2\Common.Utils.* . /Y /D

REM Plugins

REM SubtitlePlugin
xcopy %1\SubtitlePlugins\bin\%2\SubtitlePlugins.* plugins\subtitle\ /Y /D

REM ExternalPlayers
xcopy %1\ExternalPlayers\bin\%2\ExternalPlayers.* plugins\ExternalPlayers\ /Y /D

REM WindowPlugins
xcopy %1\WindowPlugins\bin\%2\WindowPlugins.* plugins\Windows\ /Y /D

REM Dialogs
xcopy %1\Dialogs\bin\%2\Dialogs.* plugins\Windows\ /Y /D

REM ProcessPlugins
xcopy %1\ProcessPlugins\bin\%2\ProcessPlugins.* plugins\process\ /Y /D
xcopy %1\ProcessPlugins\MusicShareWatcher\MusicShareWatcherHelper\bin\%2\MusicShareWatcherHelper.* . /Y /D
xcopy %1\ProcessPlugins\MusicShareWatcher\MusicShareWatcher\bin\%2\MusicShareWatcher.exe . /Y /D

REM MiniDisplayLibrary
xcopy %1\MiniDisplayLibrary\bin\%2\MiniDisplayLibrary.* . /Y /D

REM RemotePlugins
xcopy %1\RemotePlugins\bin\%2\RemotePlugins.* . /Y /D

REM Remotes
xcopy %1\RemotePlugins\Remotes\HcwRemote\HCWHelper\bin\%2\HCWHelper.* . /Y /D
xcopy %1\RemotePlugins\Remotes\X10Remote\Interop.X10.dll . /Y /D

REM Utils
xcopy %1\Utils\bin\%2\Utils.dll . /Y /D

REM Copy all new files from base
xcopy %1\MediaPortal.Base\*.* . /E /R /Y /D

REM Language
xcopy %1\MediaPortal.Base\Language\*.* %ConfigPath%\"Team MediaPortal\MediaPortal\Language\" /E /Y /D

REM Skins
xcopy %1\MediaPortal.Base\Skin\*.* %ConfigPath%\"Team MediaPortal\MediaPortal\Skin\" /E /Y /D

REM Copy all dll files from cpp solution 
xcopy %1\Core.cpp\DirectShowHelper\bin\%2\dshowhelper.dll . /Y /D
xcopy %1\Core.cpp\Win7RefreshRateHelper\bin\%2\Win7RefreshRateHelper.dll . /Y /D
xcopy %1\Core.cpp\DXUtil\bin\%2\dxutil.dll . /Y /D
xcopy %1\Core.cpp\fontEngine\bin\%2\fontEngine.dll . /Y /D
xcopy %1\Core.cpp\mpc-hc_subs\bin\%2\mpcSubs.dll . /Y /D

if /I "%2" EQU "DEBUG" (
xcopy %1\Core.cpp\DirectShowHelper\bin\%2\dshowhelper.pdb . /Y /D
xcopy %1\Core.cpp\Win7RefreshRateHelper\bin\%2\Win7RefreshRateHelper.pdb . /Y /D
xcopy %1\Core.cpp\fontEngine\bin\%2\fontEngine.pdb . /Y /D
xcopy %1\Core.cpp\mpc-hc_subs\bin\%2\mpcSubs.pdb . /Y /D
)

REM Copy one dll from DirectShowFilters folder
xcopy %1\..\DirectShowFilters\DXErr9\bin\%2\dxerr9.dll . /Y /D

REM mpWatchDog
xcopy %1\WatchDog\bin\%2\WatchDog.exe . /Y /D
xcopy %1\WatchDog\bin\%2\DaggerLib.dll . /Y /D
xcopy %1\WatchDog\bin\%2\DaggerLib.DSGraphEdit.dll . /Y /D
xcopy %1\WatchDog\bin\%2\DirectShowLib-2005.dll . /Y /D
xcopy %1\WatchDog\bin\%2\MediaFoundation.dll . /Y /D

REM MPTray
xcopy %1\MPTray\bin\%2\MPTray.* . /Y /D

REM MPE
xcopy %1\MPE\MpeCore\bin\%2\MpeCore.* . /Y /D
xcopy %1\MPE\MpeInstaller\bin\%2\MpeInstaller.* . /Y /D
xcopy %1\MPE\MpeMaker\bin\%2\MpeMaker.* . /Y /D
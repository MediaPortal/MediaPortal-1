REM %1 = Solution Directory
REM %2 = $(ConfigurationName) Debug/Release

REM Check for Microsoft Antispyware .BAT bug
if exist .\kernel32.dll exit 1

REM Hack to remove dll not needed in root 
del *.dll
del *.ax

REM Support
xcopy /y %1\MediaPortal.Support\bin\%2\MediaPortal.Support.* .


REM Configuration
xcopy /y %1\Configuration\Wizards\*.* Wizards\
xcopy /y %1\Configuration\bin\%2\Configuration.* .


REM TvGuideScheduler
xcopy /y %1\TVGuideScheduler\bin\%2\TVGuideScheduler.* .

REM Core
xcopy /y %1\core\bin\%2\DirectShowLib.* .
xcopy /y %1\core\bin\%2\Core.* .
xcopy /y %1\core\directshowhelper\directshowhelper\Release\dshowhelper.dll .
xcopy /y %1\core\DXUtil\Release\DXUtil.dll .
xcopy /y %1\core\fontengine\fontengine\%2\fontengine.* .

REM TvCapture
xcopy /y %1\tvcapture\bin\%2\tvcapture.* .

REM Databases
xcopy /y %1\databases\bin\%2\databases.* .

REM Plugins

REM SubtitlePlugin
xcopy /y %1\SubtitlePlugins\bin\%2\SubtitlePlugins.* plugins\subtitle\

REM ExternalPlayers
xcopy /y %1\ExternalPlayers\bin\%2\ExternalPlayers.* plugins\ExternalPlayers\

REM WindowPlugins
xcopy /y %1\WindowPlugins\bin\%2\WindowPlugins.* plugins\Windows\

REM Dialogs
xcopy /y %1\Dialogs\bin\%2\Dialogs.* plugins\Windows\

REM ProcessPlugins
xcopy /y %1\ProcessPlugins\bin\%2\ProcessPlugins.* plugins\process\
xcopy /y %1\ProcessPlugins\MusicShareWatcher\MusicShareWatcherHelper\bin\%2\MusicShareWatcherHelper.* .
xcopy /y %1\ProcessPlugins\MusicShareWatcher\MusicShareWatcher\bin\%2\MusicShareWatcher.exe .

REM MiniDisplayLibrary
xcopy /y %1\MiniDisplayLibrary\bin\%2\MiniDisplayLibrary.* .

REM RemotePlugins
xcopy /y %1\RemotePlugins\bin\%2\RemotePlugins.* .

REM Remotes
xcopy /y %1\RemotePlugins\Remotes\HcwRemote\HCWHelper\bin\%2\HCWHelper.* .
xcopy /y %1\RemotePlugins\Remotes\X10Remote\Interop.X10.dll .

REM Utils
xcopy /y %1\Utils\bin\%2\Utils.dll .

REM WebEPG
xcopy /y %1\WebEPG\WebEPG\bin\%2\WebEPG.dll .
copy %1\WebEPG\WebEPG-xmltv\bin\%2\WebEPG-xmltv.exe WebEPG.exe
xcopy /y %1\WebEPG\WebEPG-conf\bin\%2\WebEPG-conf.exe .

rem C#scripts
rem don't need to be copied seperate, those files are already in MediaPortal.Base
rem xcopy /y %1\scripts\*.* scripts\
rem xcopy /y %1\scripts\imdb\*.* scripts\imdb\


rem MyBurner plugin dependencies
xcopy /y %1\WindowPlugins\GUIBurner\madlldlib.dll .
xcopy /y %1\XPImapiBurner\bin\%2\XPBurnComponent.dll .
REM xcopy /y %1\WindowPlugins\GUIBurner\XPBurnComponent.dll .



REM Copy All new files from base
xcopy %1\MediaPortal.Base\*.* . /E /R /Y /D

REM mpWatchDog
xcopy /y %1\WatchDog\bin\%2\WatchDog.exe .
xcopy /y %1\WatchDog\bin\%2\DaggerLib.dll .
xcopy /y %1\WatchDog\bin\%2\DaggerLib.DSGraphEdit.dll .
xcopy /y %1\WatchDog\bin\%2\DirectShowLib-2005.dll .
xcopy /y %1\WatchDog\bin\%2\MediaFoundation.dll .

REM MPInstaller
xcopy /y %1\MPInstaller\bin\%2\MPInstaller.Library.* .
xcopy /y %1\MPInstaller\bin\%2\MPInstaller.* .

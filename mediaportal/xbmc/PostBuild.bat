REM %1 = Solution Directory
REM %2 = $(ConfigurationName) Debug/Release

REM Check for Microsoft Antispyware .BAT bug
if exist .\kernel32.dll exit 1

REM Support
xcopy /y %1\MediaPortal.Support\bin\%2\MediaPortal.Support.* .


REM Configuration
xcopy /y %1\Configuration\Wizards\*.* Wizards
xcopy /y %1\Configuration\bin\%2\Configuration.* .


REM TvGuideScheduler
xcopy /y %1\TVGuideScheduler\bin\%2\TVGuideScheduler.* .

REM Core
xcopy /y %1\core\bin\%2\DirectShowLib.* .
xcopy /y %1\core\bin\%2\Core.* .
xcopy /y %1\core\directshowhelper\directshowhelper\%2\dshowhelper.dll .
xcopy /y %1\core\DXUtil\%2\DXUtil.dll .
xcopy /y %1\core\fontengine\fontengine\%2\fontengine.dll .
//xcopy /y %1\core\fontengine\fontengine\%2\fontengine.pdb .

REM TvCapture
xcopy /y %1\tvcapture\bin\%2\tvcapture.* .

REM Databases
xcopy /y %1\databases\bin\%2\databases.* .

REM Plugins

REM SubtitlePlugin
xcopy /y %1\SubtitlePlugins\bin\%2\SubtitlePlugins.* plugins\subtitle

REM TagReader
xcopy /y %1\TagReaderPlugins\bin\%2\TagReaderPlugins.* plugins\TagReaders

REM ExternalPlayers
xcopy /y %1\ExternalPlayers\bin\%2\ExternalPlayers.* plugins\ExternalPlayers

REM WindowPlugins
xcopy /y %1\WindowPlugins\bin\%2\WindowPlugins.* plugins\Windows

REM Dialogs
xcopy /y %1\Dialogs\bin\%2\Dialogs.* plugins\Windows

REM ProcessPlugins
xcopy /y %1\ProcessPlugins\bin\%2\ProcessPlugins.dll plugins\process\
xcopy /y %1\ProcessPlugins\bin\%2\ProcessPlugins.pdb  plugins\process\
xcopy /y %1\ProcessPlugins\MusicShareWatcher\MusicShareWatcherHelper\bin\%2\MusicShareWatcherHelper.* .
xcopy /y %1\ProcessPlugins\MusicShareWatcher\MusicShareWatcher\bin\%2\MusicShareWatcher.exe .

REM RemotePlugins
xcopy /y %1\RemotePlugins\bin\%2\RemotePlugins.* .

REM Remotes
xcopy /y %1\RemotePlugins\Remotes\HcwRemote\HCWHelper\bin\%2\HCWHelper.* .
xcopy /y %1\RemotePlugins\Remotes\X10Remote\Interop.X10.dll .

REM TTPremiumBoot
xcopy /y %1\TTPremiumBoot\*.* TTPremiumBoot\.
xcopy /y %1\TTPremiumBoot\21\*.* TTPremiumBoot\21\.
xcopy /y %1\TTPremiumBoot\24\*.* TTPremiumBoot\24\.
xcopy /y %1\TTPremiumBoot\24Data\*.* TTPremiumBoot\24Data\.

REM Utils
xcopy /y %1\Utils\bin\%2\Utils.dll .

REM WebEPG
xcopy /y %1\WebEPG\WebEPG\bin\%2\WebEPG.dll .
xcopy /y %1\WebEPG\WebEPG-xmltv\bin\%2\WebEPG-xmltv.exe WebEPG.exe
xcopy /y %1\WebEPG\WebEPG-conf\bin\%2\WebEPG-conf.exe .

rem MyDreambox plugin dependencies
xcopy /y %1\WindowPlugins\GUIMyDreambox\AxInterop.AXVLC.dll .
xcopy /y %1\WindowPlugins\GUIMyDreambox\Interop.AXVLC.dll .

rem MyBurner plugin dependencies
xcopy /y %1\WindowPlugins\GUIBurner\XPBurnComponent.dll .
xcopy /y %1\WindowPlugins\GUIBurner\madlldlib.dll .


REM Copy All new files from base
xcopy %1\MediaPortal.Base\*.* . /E /R /Y /D

REM Register
regsvr32 /s MPSA.ax
regsvr32 /s TSFileSource.ax
regsvr32 /s MPTSWriter.ax
regsvr32 /s cdxareader.ax
regsvr32 /s TTPremiumSource.ax
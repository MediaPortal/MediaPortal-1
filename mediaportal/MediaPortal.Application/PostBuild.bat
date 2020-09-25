REM %1 = Solution Directory
REM %2 = $(ConfigurationName) Debug/Release

set GIT_ROOT=%~dp0..\..\
set Build="%GIT_ROOT%\Build"

REM Identify configuration path for <=XP or >=Vista
if exist %ProgramData%\nul (
	set ConfigPath="%ProgramData%"
) else (
	set ConfigPath="%AllUsersProfile%\Application Data"
)

REM Check for Microsoft Antispyware .BAT bug
if exist .\kernel32.dll exit 1

REM Hack to remove dll not needed in root 
REM del *.dll
REM del *.ax
REM Support
xcopy %1\MediaPortal.Support\bin\%2\MediaPortal.Support.* . /Y /D

REM Configuration
xcopy %1\Configuration\Wizards\*.* Wizards\ /Y /D
xcopy %1\Configuration\bin\%2\Configuration.* . /Y /D
xcopy %1\Configuration\WinCustomControls\bin\%2\WinCustomControls.* . /Y /D
xcopy %1\..\TvEngine3\TVLibrary\TvLibrary.Interfaces\bin\%2\TvLibrary.Interfaces.* . /Y /D

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

REM LastFMLibrary
xcopy %1\LastFMLibrary\bin\%2\LastFMLibrary.* . /Y /D

REM ExternalPlayers
xcopy %1\ExternalPlayers\bin\%2\ExternalPlayers.* plugins\ExternalPlayers\ /Y /D

REM WindowPlugins
xcopy %1\WindowPlugins\GUIDisc\bin\%2\GUIDisc.* plugins\Windows\ /Y /D
xcopy %1\WindowPlugins\GUIDVD\bin\%2\GUIDVD.* plugins\Windows\ /Y /D
xcopy %1\WindowPlugins\GUIHome\bin\%2\GUIHome.* plugins\Windows\ /Y /D
xcopy %1\WindowPlugins\GUILastFMRadio\bin\%2\GUILastFMRadio.* plugins\Windows\ /Y /D
xcopy %1\WindowPlugins\GUIMusic\bin\%2\GUIMusic.* plugins\Windows\ /Y /D
xcopy %1\WindowPlugins\GUISudoku\bin\%2\GUISudoku.* plugins\Windows\ /Y /D
xcopy %1\WindowPlugins\GUIPictures\bin\%2\GUIPictures.* plugins\Windows\ /Y /D
xcopy %1\WindowPlugins\GUIRSSFeed\bin\%2\GUIRSSFeed.* plugins\Windows\ /Y /D
xcopy %1\WindowPlugins\GUISettings\bin\%2\GUISettings.* plugins\Windows\ /Y /D
xcopy %1\WindowPlugins\GUITetris\bin\%2\GUITetris.* plugins\Windows\ /Y /D
xcopy %1\WindowPlugins\GUITopbar\bin\%2\GUITopbar.* plugins\Windows\ /Y /D
xcopy %1\WindowPlugins\GUIVideos\bin\%2\GUIVideos.* plugins\Windows\ /Y /D
xcopy %1\WindowPlugins\GUIWikipedia\bin\%2\GUIWikipedia.* plugins\Windows\ /Y /D
xcopy %1\WindowPlugins\Common.GUIPlugins\bin\%2\Common.GUIPlugins.* plugins\Windows\ /Y /D

REM Dialogs
xcopy %1\Dialogs\bin\%2\Dialogs.* plugins\Windows\ /Y /D

REM ProcessPlugins
xcopy %1\ProcessPlugins\bin\%2\ProcessPlugins.* plugins\process\ /Y /D
xcopy %1\ProcessPlugins\MiniDisplay\bin\%2\MiniDisplayPlugin.* plugins\process\ /Y /D
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
xcopy %1Exclude.txt .
xcopy %1\MediaPortal.Base\*.* . /E /R /Y /D /EXCLUDE:Exclude.txt
del Exclude.txt

REM Language
xcopy %1\MediaPortal.Base\Language\*.* %ConfigPath%\"Team MediaPortal\MediaPortal\Language\" /E /Y /D

REM Skins
xcopy %1\MediaPortal.Base\Skin\*.* %ConfigPath%\"Team MediaPortal\MediaPortal\Skin\" /E /Y /D

REM Copy all dll files from cpp solution 
xcopy %1\..\DirectShowFilters\DirectShowHelper\bin\%2\dshowhelper.dll . /Y /D
xcopy %1\..\DirectShowFilters\Win7RefreshRateHelper\bin\%2\Win7RefreshRateHelper.dll . /Y /D
xcopy %1\..\DirectShowFilters\DXUtil\bin\%2\dxutil.dll . /Y /D
xcopy %1\..\DirectShowFilters\fontEngine\bin\%2\fontEngine.dll . /Y /D
xcopy %1\..\DirectShowFilters\mpc-hc_subs\bin\%2\mpcSubs.dll . /Y /D

if /I "%2" EQU "DEBUG" (
xcopy %1\..\DirectShowFilters\DirectShowHelper\bin\%2\dshowhelper.pdb . /Y /D
xcopy %1\..\DirectShowFilters\Win7RefreshRateHelper\bin\%2\Win7RefreshRateHelper.pdb . /Y /D
xcopy %1\..\DirectShowFilters\fontEngine\bin\%2\fontEngine.pdb . /Y /D
xcopy %1\..\DirectShowFilters\mpc-hc_subs\bin\%2\mpcSubs.pdb . /Y /D
)

REM bluray.dll - odd source folder is 
if /I "%2" EQU "RELEASE" (
xcopy %1\..\DirectShowFilters\bin_Win32\libbluray.dll . /Y /D
)

if /I "%2" EQU "DEBUG" (
xcopy %1\..\DirectShowFilters\bin_Win32d\libbluray.dll . /Y /D
)

ren libbluray.dll bluray.dll

REM Copy one dll from DirectShowFilters folder
xcopy %1\..\DirectShowFilters\DXErr9\bin\%2\dxerr9.dll . /Y /D

REM Copy bluray dll from DirectShowFilters folder
xcopy %1\..\DirectShowFilters\BDReader\libbluray\bluray.dll . /Y /D
xcopy %1\..\libbluray\src\.libs\libbluray-.jar . /Y /D
ren libbluray-.jar libbluray.jar
copy libbluray.jar libbluray-j2se-1.1.2.jar /Y

REM Copy bluray awt extension from DirectShowFilters folder
if not exist .\awt\ mkdir awt
xcopy %1\..\libbluray\src\.libs\libbluray-awt-.jar .\awt\ /Y /D
ren .\awt\libbluray-awt-.jar libbluray.jar

REM freetype.dll - odd source folder is 
if /I "%2" EQU "RELEASE" (
xcopy %1\..\libbluray\3rd_party\freetype2\objs\Win32\Release\freetype.dll . /Y /D
)

if /I "%2" EQU "DEBUG" (
xcopy %1\..\libbluray\3rd_party\freetype2\objs\Win32\Debug\freetype.dll . /Y /D
)

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

REM Nuget 
xcopy %1\Databases\bin\%2\HtmlAgilityPack.* . /Y /D
xcopy %1\..\Packages\Sqlite.3.33.0\sqlite.dll . /Y /D

REM MediaInfo - 
xcopy %1\..\Packages\MediaInfo.Wrapper.20.8.0\lib\net40\MediaInfo.Wrapper.dll . /Y /D
REM - commented because provided on bin folder by nuget target setting during building. 
REM xcopy %1\..\Packages\MediaInfo.Native.20.8.1\build\native\x86\MediaInfo.dll . /Y /D
REM xcopy %1\..\Packages\MediaInfo.Native.20.8.1\build\native\x86\lib*.dll . /Y /D

REM Exif
xcopy %1\..\Packages\MetadataExtractor.2.4.3\lib\net35\MetadataExtractor.dll . /Y /D
xcopy %1\..\Packages\XmpCore.6.1.10\lib\net35\XmpCore.dll . /Y /D

REM ffmpeg 
xcopy %1\..\Packages\FFmpeg.Win32.Static.4.1.1.1\ffmpeg\ffmpeg.exe MovieThumbnailer\ /Y /D

REM Bass Core
xcopy %1\core\bin\%2\Bass.Net.dll . /Y /D
xcopy %1\core\bin\%2\BassRegistration.dll . /Y /D
xcopy %1\..\Packages\BASSCombined.2.4.15\content\x86\bass.dll . /Y /D

REM Bass AddOns
xcopy %1\..\Packages\BASSCombined.2.4.15\content\x86\bass_fx.dll . /Y /D
xcopy %1\..\Packages\BASSCombined.2.4.15\content\x86\bass_vst.dll . /Y /D
xcopy %1\..\Packages\BASSCombined.2.4.15\content\x86\bass_wadsp.dll . /Y /D
xcopy %1\..\Packages\BASSCombined.2.4.15\content\x86\bassasio.dll . /Y /D
xcopy %1\..\Packages\BASSCombined.2.4.15\content\x86\basscd.dll . /Y /D
xcopy %1\..\Packages\BASSCombined.2.4.15\content\x86\bassmix.dll . /Y /D
xcopy %1\..\Packages\BASSCombined.2.4.15\content\x86\basswasapi.dll . /Y /D
xcopy %1\..\Packages\BASSCombined.2.4.15\content\x86\plugins\OptimFROG.dll . /Y /D

REM Bass AudioDecoders
xcopy %1\..\Packages\BASSCombined.2.4.15\content\x86\plugins\bass*.dll "MusicPlayer\plugins\audio decoders\" /Y /D

REM iMON Display 
xcopy %1\..\Packages\MediaPortal-iMON-Display.1.1.0\lib\iMONDisplay.dll . /Y /D
xcopy %1\..\Packages\MediaPortal-iMON-Display.1.1.0\lib\iMONDisplayWrapper.dll . /Y /D

REM taglib-sharp
xcopy %1\..\Packages\MediaPortal.TagLib.2.2.0.2\lib\net40\TagLibSharp.dll ./Y /D

REM SharpLibHid
REM Provided with Nuget to bin folder during build
REM xcopy %1\..\Packages\SharpLibHid.1.4.4\lib\net40\SharpLibHid.dll . /Y /D

REM REM SharpLibWin32
REM Provided with Nuget to bin folder during build
REM xcopy %1\..\Packages\SharpLibWin32.0.2.1\lib\net20\SharpLibWin32.dll . /Y /D

REM System.Management.Automation
xcopy %1\..\Packages\System.Management.Automation.6.1.7601.17515\lib\net40\System.Management.Automation.dll . /Y /D

REM SharpLibDisplay
xcopy %1\..\Packages\SharpLibDisplay.0.3.4\lib\net40\SharpLibDisplay.dll . /Y /D

REM Naudio
xcopy %1\..\Packages\NAudio.1.10.0\lib\net35\NAudio.dll . /Y /D

REM CSCore
xcopy %1\..\Packages\CSCore.1.2.1.2\lib\net35-client\CSCore.dll . /Y /D

REM Enable >2GB for 32 bit process
call %Build%\MSBUILD_MP_LargeAddressAware.bat %2

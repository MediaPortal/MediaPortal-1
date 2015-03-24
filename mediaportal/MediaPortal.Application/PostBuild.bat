REM %1 = Solution Directory
REM %2 = $(ConfigurationName) Debug/Release

REM Identify configuration path for <=XP or >=Vista
if exist %ProgramData%\nul (
	set ConfigPath="%ProgramData%" 
) else (
	set ConfigPath="%AllUsersProfile%\Application Data"
)

set GIT_ROOT=%~dp0..\..\
set Build="%GIT_ROOT%\Build"

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
xcopy %1\..\Packages\MediaInfo.0.7.69\MediaInfo.dll . /Y /D

REM ffmpeg 
xcopy %1\..\Packages\ffmpeg.2.1.1\ffmpeg.exe MovieThumbnailer\ /Y /D

REM iMON Display 
xcopy %1\..\Packages\MediaPortal-iMON-Display.1.0.1\lib\iMONDisplay.dll . /Y /D
xcopy %1\..\Packages\MediaPortal-iMON-Display.1.0.1\lib\iMONDisplayWrapper.dll . /Y /D

REM Bass Core
xcopy %1\..\Packages\BASS.2.4.10\bass.dll . /Y /D
xcopy %1\..\Packages\BASS.NET.2.4.10.3\lib\net40\Bass.Net.dll . /Y /D

REM Bass AddOns
xcopy %1\..\Packages\bass.asio.1.3.0.2\bassasio.dll . /Y /D
xcopy %1\..\Packages\bass.fx.2.4.10.1\bass_fx.dll . /Y /D
xcopy %1\..\Packages\bass.mix.2.4.7.2\bassmix.dll . /Y /D
xcopy %1\..\Packages\bass.vst.2.4.5\bass_vst.dll . /Y /D
xcopy %1\..\Packages\bass.wadsp.2.4.1\bass_wadsp.dll . /Y /D
xcopy %1\..\Packages\bass.wasapi.2.4.0.2\basswasapi.dll . /Y /D
xcopy %1\..\Packages\bass.ofr.2.4.0.2\OptimFROG.dll . /Y /D

REM Bass AudioDecoders
xcopy %1\..\Packages\bass.aac.2.4.4.4\bass_aac.dll "MusicPlayer\plugins\audio decoders\" /Y /D
xcopy %1\..\Packages\bass.ac3.2.4.0.3\bass_ac3.dll "MusicPlayer\plugins\audio decoders\" /Y /D
xcopy %1\..\Packages\bass.alac.2.4.3\bass_alac.dll "MusicPlayer\plugins\audio decoders\" /Y /D
xcopy %1\..\Packages\bass.ape.2.4.1\bass_ape.dll "MusicPlayer\plugins\audio decoders\" /Y /D
xcopy %1\..\Packages\bass.dsd.0.0.1\bassdsd.dll "MusicPlayer\plugins\audio decoders\" /Y /D
xcopy %1\..\Packages\bass.mpc.2.4.1.1\bass_mpc.dll "MusicPlayer\plugins\audio decoders\" /Y /D
xcopy %1\..\Packages\bass.ofr.2.4.0.2\bass_ofr.dll "MusicPlayer\plugins\audio decoders\" /Y /D
xcopy %1\..\Packages\bass.spx.2.4.2\bass_spx.dll "MusicPlayer\plugins\audio decoders\" /Y /D
xcopy %1\..\Packages\bass.tta.2.4.0\bass_tta.dll "MusicPlayer\plugins\audio decoders\" /Y /D
xcopy %1\..\Packages\bass.cd.2.4.5\basscd.dll "MusicPlayer\plugins\audio decoders\" /Y /D
xcopy %1\..\Packages\bass.flac.2.4.1\bassflac.dll "MusicPlayer\plugins\audio decoders\" /Y /D
xcopy %1\..\Packages\bass.midi.2.4.8\bassmidi.dll "MusicPlayer\plugins\audio decoders\" /Y /D
xcopy %1\..\Packages\bass.opus.2.4.1.3\bassopus.dll "MusicPlayer\plugins\audio decoders\" /Y /D
xcopy %1\..\Packages\bass.wma.2.4.4\basswma.dll "MusicPlayer\plugins\audio decoders\" /Y /D
xcopy %1\..\Packages\bass.wv.2.4.4\basswv.dll "MusicPlayer\plugins\audio decoders\" /Y /D

REM iMON Display 
xcopy %1\..\Packages\MediaPortal-iMON-Display.1.1.0\lib\iMONDisplay.dll . /Y /D
xcopy %1\..\Packages\MediaPortal-iMON-Display.1.1.0\lib\iMONDisplayWrapper.dll . /Y /D

REM taglib-sharp
xcopy %1\..\Packages\MediaPortal.TagLib.2.0.3.8\lib\taglib-sharp.dll ./Y /D

REM SharpLibHid
xcopy %1\..\Packages\SharpLibHid.1.0.4\lib\net20\SharpLibHid.dll . /Y /D

REM Enable >2GB for 32 bit process
call %Build%\MSBUILD_MP_LargeAddressAware.bat %2

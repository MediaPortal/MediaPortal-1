rem Check for Microsoft Antispyware .BAT bug
if exist .\kernel32.dll exit 1

cd
if not exist plugins mkdir plugins
if not exist plugins\windows mkdir plugins\windows
if not exist plugins\TagReaders mkdir plugins\TagReaders
if not exist plugins\subtitle mkdir plugins\subtitle
if not exist plugins\ExternalPlayers mkdir plugins\ExternalPlayers
if not exist plugins\process mkdir plugins\process
if not exist Wizards mkdir Wizards

del /F /Q plugins\windows\*.*
del /F /Q plugins\tagreaders\*.*
del /F /Q plugins\subtitle\*.*
del /F /Q plugins\ExternalPlayers\*.*
del /F /Q plugins\process\*.*
del *.dll
del *.ax

if exist ..\..\..\MediaPortal.Base\lame_enc.dll copy ..\..\..\MediaPortal.Base\lame_enc.dll .
copy ..\..\..\MediaPortal.Base\MPSA.ax .
copy ..\..\..\MediaPortal.Base\TSFileSource.ax .
copy ..\..\..\MediaPortal.Base\MPTSWriter.ax .
copy ..\..\..\MediaPortal.Base\cdxareader.ax .
copy ..\..\..\MediaPortal.Base\ttBdaDrvApi_Dll.dll .
regsvr32 /s MPSA.ax
regsvr32 /s TSFileSource.ax
regsvr32 /s MPTSWriter.ax
regsvr32 /s cdxareader.ax

if not exist TTPremiumBoot\. mkdir TTPremiumBoot
if not exist TTPremiumBoot\21\. mkdir TTPremiumBoot\21
if not exist TTPremiumBoot\24\. mkdir TTPremiumBoot\24
if not exist TTPremiumBoot\24Data\. mkdir TTPremiumBoot\24Data
copy ..\..\..\TTPremiumBoot\*.* TTPremiumBoot\.
copy ..\..\..\TTPremiumBoot\21\*.* TTPremiumBoot\21\.
copy ..\..\..\TTPremiumBoot\24\*.* TTPremiumBoot\24\.
copy ..\..\..\TTPremiumBoot\24Data\*.* TTPremiumBoot\24Data\.
copy ..\..\..\MediaPortal.Base\TTPremiumSource.ax .
copy ..\..\..\MediaPortal.Base\ttdvbacc.dll .
regsvr32 /s TTPremiumSource.ax

copy ..\..\..\MediaPortal.Support\bin\debug\MediaPortal.Support.dll .
copy ..\..\..\MediaPortal.Support\bin\debug\MediaPortal.Support.pdb .
copy ..\..\..\MediaPortal.Support\bin\debug\ICSharpCode.SharpZipLib.dll .
copy ..\..\..\RemotePlugins\Remotes\HcwRemote\HCWHelper\bin\debug\HCWHelper.exe .
copy ..\..\..\RemotePlugins\Remotes\HcwRemote\HCWHelper\bin\debug\HCWHelper.pdb .
copy ..\..\..\RemotePlugins\Remotes\X10Remote\AxInterop.X10.dll .
copy ..\..\..\RemotePlugins\Remotes\X10Remote\Interop.X10.dll .

copy ..\..\..\core\directshowhelper\directshowhelper\release\dshowhelper.dll .
copy ..\..\..\core\DXUtil\release\DXUtil.dll .
copy ..\..\..\core\fontengine\fontengine\debug\fontengine.dll .
if exist ..\..\..\core\fontengine\fontengine\debug\fontengine.pdb copy ..\..\..\core\fontengine\fontengine\debug\fontengine.pdb .
rem copy ..\..\..\MediaPortal.Base\Interop.DirectShowHelperLib.dll .
copy ..\..\..\MediaPortal.Base\mfc71.dll .
copy ..\..\..\MediaPortal.Base\msvcp71.dll .
copy ..\..\..\MediaPortal.Base\msvcr71.dll .
rem copy ..\..\..\MediaPortal.Base\AxInterop.MOZILLACONTROLLib.dll .
rem copy ..\..\..\MediaPortal.Base\Interop.MOZILLACONTROLLib.dll .
copy ..\..\..\MediaPortal.Base\Microsoft.ApplicationBlocks*.dll .
copy ..\..\..\MediaPortal.Base\d3dx9_26.dll .
copy ..\..\..\MediaPortal.Base\Microsoft.DirectX.Direct3D.dll .
copy ..\..\..\MediaPortal.Base\Microsoft.DirectX.Direct3DX.dll .
copy ..\..\..\MediaPortal.Base\Microsoft.DirectX.DirectDraw.dll .
copy ..\..\..\MediaPortal.Base\Microsoft.DirectX.dll .
copy ..\..\..\MediaPortal.Base\Microsoft.DirectX.DirectInput.dll .
copy ..\..\..\MediaPortal.Base\KCS.Utilities.dll .
rem copy ..\..\..\MediaPortal.Base\X10Plugin.* .
copy ..\..\..\MediaPortal.Base\X10Unified.* .
copy ..\..\..\MediaPortal.Base\xAPMessage.dll .
copy ..\..\..\MediaPortal.Base\xAPTransport.dll .
copy ..\..\..\Configuration\Wizards\*.* Wizards
copy ..\..\..\Configuration\bin\debug\Configuration.exe .
copy ..\..\..\Configuration\bin\debug\Configuration.exe.config .
copy ..\..\..\Configuration\bin\debug\Configuration.pdb .
copy ..\..\..\TVGuideScheduler\bin\debug\TVGuideScheduler.exe .
copy ..\..\..\TVGuideScheduler\bin\debug\TVGuideScheduler.pdb .
rem copy ..\..\..\MediaPortal.Base\ECP2Assembly.dll .
copy ..\..\..\MediaPortal.Base\edtftpnet-1.2.2.dll .
copy ..\..\..\MediaPortal.Base\dvblib.dll .
rem copy ..\..\..\MediaPortal.Base\*.tpl .
copy ..\..\..\MediaPortal.Base\Interop.WMEncoderLib.dll .
copy ..\..\..\MediaPortal.Base\Interop.TunerLib.dll .
copy ..\..\..\MediaPortal.Base\Interop.iTunesLib.dll .
copy ..\..\..\MediaPortal.Base\Microsoft.Office.Interop.Outlook.dll .

copy ..\..\..\core\bin\debug\DirectShowLib.dll .
copy ..\..\..\core\bin\debug\DirectShowLib.pdb .
copy ..\..\..\core\bin\debug\Core.dll .
copy ..\..\..\core\bin\debug\Core.pdb .
copy ..\..\..\tvcapture\bin\debug\tvcapture.dll .
copy ..\..\..\tvcapture\bin\debug\tvcapture.pdb .
copy ..\..\..\databases\bin\debug\databases.dll .
copy ..\..\..\databases\bin\debug\databases.pdb .
copy ..\..\..\SubtitlePlugins\bin\debug\SubtitlePlugins.dll plugins\subtitle
copy ..\..\..\SubtitlePlugins\bin\debug\SubtitlePlugins.pdb plugins\subtitle
copy ..\..\..\TagReaderPlugins\bin\debug\TagReaderPlugins.dll plugins\TagReaders
copy ..\..\..\TagReaderPlugins\bin\debug\TagReaderPlugins.pdb plugins\TagReaders
copy ..\..\..\ExternalPlayers\bin\debug\ExternalPlayers.dll plugins\ExternalPlayers
copy ..\..\..\ExternalPlayers\bin\debug\ExternalPlayers.pdb plugins\ExternalPlayers
copy ..\..\..\WindowPlugins\bin\debug\WindowPlugins.dll plugins\Windows
copy ..\..\..\WindowPlugins\bin\debug\WindowPlugins.pdb plugins\Windows
copy ..\..\..\MediaPortal.Base\XihSolutions.DotMSN.dll plugins\Windows
copy ..\..\..\Dialogs\bin\debug\Dialogs.dll plugins\Windows
copy ..\..\..\Dialogs\bin\debug\Dialogs.pdb plugins\Windows
copy ..\..\..\ProcessPlugins\bin\debug\ProcessPlugins.dll plugins\process\
copy ..\..\..\ProcessPlugins\bin\debug\ProcessPlugins.pdb  plugins\process\
copy ..\..\..\ProcessPlugins\MusicShareWatcher\MusicShareWatcherHelper\bin\Debug\MusicShareWatcherHelper.dll .
copy ..\..\..\ProcessPlugins\MusicShareWatcher\MusicShareWatcherHelper\bin\Debug\MusicShareWatcherHelper.pdb .
copy ..\..\..\ProcessPlugins\MusicShareWatcher\MusicShareWatcher\bin\Debug\MusicShareWatcher.exe .
copy ..\..\..\RemotePlugins\bin\debug\RemotePlugins.dll .


copy ..\..\..\MediaPortal.Base\sqlite.dll .
copy ..\..\..\MediaPortal.Base\TaskScheduler.dll .
copy ..\..\..\MediaPortal.Base\AxInterop.WMPLib.dll .
copy ..\..\..\MediaPortal.Base\Interop.WMPLib.dll .
copy ..\..\..\MediaPortal.Base\dxerr9.dll .

copy ..\..\..\WebEPG\WebEPG\bin\debug\WebEPG.dll .
copy ..\..\..\Utils\bin\debug\Utils.dll .

copy ..\..\..\WebEPG\WebEPG-xmltv\bin\debug\WebEPG-xmltv.exe WebEPG.exe
copy ..\..\..\WebEPG\WebEPG-conf\bin\debug\WebEPG-conf.exe .
copy ..\..\..\WebEPG\WebEPG-channels\bin\debug\WebEPG-channels.exe .
rem ---------------------
rem Begin ExternalDisplay
rem ---------------------
rem - plugin LCD driver DLLs
rem usbuirt driver should only reside in windows\system32
rem copy ..\..\..\MediaPortal.Base\FTD2XX.DLL .
copy ..\..\..\MediaPortal.Base\SG_VFD.dll .
copy ..\..\..\MediaPortal.Base\dlportio.dll .
rem if not exist LUI\. mkdir LUI
rem copy ..\..\..\MediaPortal.Base\LUI.dll LUI\.
copy ..\..\..\MediaPortal.Base\Communications.dll .
copy ..\..\..\MediaPortal.Base\Interop.GIRDERLib.dll .
copy ..\..\..\MediaPortal.Base\MediaPadLayer.dll .

rem Begin BASS Music Engine dependancies
copy ..\..\..\MediaPortal.Base\bass.dll .
copy ..\..\..\MediaPortal.Base\Bass.Net.dll .
copy ..\..\..\MediaPortal.Base\bass_vis.dll .
copy ..\..\..\MediaPortal.Base\bass_fx.dll .
copy ..\..\..\MediaPortal.Base\bassmix.dll .
copy ..\..\..\MediaPortal.Base\bass_vst.dll .
copy ..\..\..\MediaPortal.Base\bass_wadsp.dll .
copy ..\..\..\MediaPortal.Base\mpviz.dll .
copy ..\..\..\MediaPortal.Base\BassRegistration.dll .
rem End BASS Music Engine dependancies

rem MyDreambox plugin dependencies
copy ..\..\..\WindowPlugins\GUIMyDreambox\AxInterop.AXVLC.dll .
copy ..\..\..\WindowPlugins\GUIMyDreambox\Interop.AXVLC.dll .

rem - LCDHype drivers
xcopy ..\Release\plugins\process\LCDDrivers\*.* plugins\process\LCDDrivers /E /I /R /K /Y
rem - Copy ExternalDisplay graphics
xcopy ..\release\Thumbs\ExternalDisplay\*.* .\Thumbs\ExternalDisplay /E /I /R /K /Y
rem -------------------
rem End ExternalDisplay
rem -------------------

rem MyBurner plugin dependencies
copy ..\..\..\WindowPlugins\GUIBurner\XPBurnComponent.dll .
copy ..\..\..\WindowPlugins\GUIBurner\madlldlib.dll .


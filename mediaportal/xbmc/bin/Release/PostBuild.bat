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

copy ..\..\..\MediaPortal.Support\bin\Release\MediaPortal.Support.dll .
copy ..\..\..\MediaPortal.Base\ICSharpCode.SharpZipLib.dll .
copy ..\..\..\RemotePlugins\Remotes\HcwRemote\HCWHelper\bin\Release\HCWHelper.exe .
rem copy ..\..\..\RemotePlugins\Remotes\X10Remote\AxInterop.X10.dll .
copy ..\..\..\RemotePlugins\Remotes\X10Remote\Interop.X10.dll .
copy ..\..\..\ProcessPlugins\MusicShareWatcher\MusicShareWatcherHelper\bin\Release\MusicShareWatcherHelper.dll .
copy ..\..\..\ProcessPlugins\MusicShareWatcher\MusicShareWatcher\bin\Release\MusicShareWatcher.exe .
if exist ..\..\..\MediaPortal.Base\lame_enc.dll copy ..\..\..\MediaPortal.Base\lame_enc.dll .
copy ..\..\..\MediaPortal.Base\MPSA.ax .
copy ..\..\..\MediaPortal.Base\TSFileSource.ax .
copy ..\..\..\MediaPortal.Base\MPTSWriter.ax .
copy ..\..\..\MediaPortal.Base\cdxareader.ax .
copy ..\..\..\MediaPortal.Base\ttBdaDrvApi_Dll.dll .
copy ..\..\..\MediaPortal.Base\hauppauge.dll.
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

copy ..\..\..\core\directshowhelper\directshowhelper\release\dshowhelper.dll .
copy ..\..\..\core\DXUtil\release\DXUtil.dll .
copy ..\..\..\core\fontengine\fontengine\release\fontengine.dll .
rem copy ..\..\..\MediaPortal.Base\Interop.DirectShowHelperLib.dll .
rem copy ..\..\..\MediaPortal.Base\AxInterop.MOZILLACONTROLLib.dll .
rem copy ..\..\..\MediaPortal.Base\Interop.MOZILLACONTROLLib.dll .
copy ..\..\..\MediaPortal.Base\mfc71.dll .
copy ..\..\..\MediaPortal.Base\msvcp71.dll .
copy ..\..\..\MediaPortal.Base\msvcr71.dll .
copy ..\..\..\MediaPortal.Base\Microsoft.ApplicationBlocks*.dll .
copy ..\..\..\MediaPortal.Base\d3dx9_30.dll .
copy ..\..\..\MediaPortal.Base\Microsoft.DirectX.Direct3D.dll .
copy ..\..\..\MediaPortal.Base\Microsoft.DirectX.Direct3DX.dll .
copy ..\..\..\MediaPortal.Base\Microsoft.DirectX.DirectDraw.dll .
copy ..\..\..\MediaPortal.Base\Microsoft.DirectX.dll .
copy ..\..\..\MediaPortal.Base\Microsoft.DirectX.DirectInput.dll .
rem ExternalDisplay plugin LCD driver DLLs

rem usbuirt driver should only reside in windows\system32
rem copy ..\..\..\MediaPortal.Base\FTD2XX.DLL .
copy ..\..\..\MediaPortal.Base\SG_VFD.dll .
copy ..\..\..\MediaPortal.Base\dlportio.dll .
rem if not exist LUI\. mkdir LUI
rem copy ..\..\..\MediaPortal.Base\LUI.dll LUI\.
copy ..\..\..\MediaPortal.Base\Communications.dll .
copy ..\..\..\MediaPortal.Base\Interop.GIRDERLib.dll .
copy ..\..\..\MediaPortal.Base\MediaPadLayer.dll .
rem 
copy ..\..\..\MediaPortal.Base\KCS.Utilities.dll .
rem copy ..\..\..\MediaPortal.Base\X10Plugin.* .
copy ..\..\..\MediaPortal.Base\X10Unified.* .
copy ..\..\..\MediaPortal.Base\xAPMessage.dll .
copy ..\..\..\MediaPortal.Base\xAPTransport.dll .
rem copy ..\..\..\MediaPortal.Base\ECP2Assembly.dll .
copy ..\..\..\MediaPortal.Base\edtftpnet-1.2.2.dll .
copy ..\..\..\MediaPortal.Base\dvblib.dll .
copy ..\..\..\MediaPortal.Base\Interop.WMEncoderLib.dll .
copy ..\..\..\MediaPortal.Base\Interop.TunerLib.dll .
copy ..\..\..\MediaPortal.Base\Interop.iTunesLib.dll .
copy ..\..\..\MediaPortal.Base\Microsoft.Office.Interop.Outlook.dll .

copy ..\..\..\Configuration\Wizards\*.* Wizards
copy ..\..\..\Configuration\bin\Release\Configuration.exe .
copy ..\..\..\Configuration\bin\Release\Configuration.exe.config .
copy ..\..\..\TVGuideScheduler\bin\Release\TVGuideScheduler.exe .

copy ..\..\..\DirectShowLib\bin\Release\DirectShowLib.dll .
copy ..\..\..\core\bin\Release\Core.dll .
copy ..\..\..\tvcapture\bin\release\tvcapture.dll .
copy ..\..\..\databases\bin\release\databases.dll .
copy ..\..\..\SubtitlePlugins\bin\release\SubtitlePlugins.dll plugins\subtitle
copy ..\..\..\TagReaderPlugins\bin\release\TagReaderPlugins.dll plugins\TagReaders
copy ..\..\..\ExternalPlayers\bin\release\ExternalPlayers.dll plugins\ExternalPlayers
copy ..\..\..\WindowPlugins\bin\release\WindowPlugins.dll plugins\Windows
copy ..\..\..\MediaPortal.Base\XihSolutions.DotMSN.dll plugins\Windows
copy ..\..\..\ProcessPlugins\bin\release\ProcessPlugins.dll plugins\process\
copy ..\..\..\Dialogs\bin\release\Dialogs.dll plugins\Windows
copy ..\..\..\RemotePlugins\bin\release\RemotePlugins.dll .
copy ..\..\..\MediaPortal.Base\sqlite.dll .
copy ..\..\..\MediaPortal.Base\TaskScheduler.dll .
copy ..\..\..\MediaPortal.Base\AxInterop.WMPLib.dll .
copy ..\..\..\MediaPortal.Base\Interop.WMPLib.dll .
copy ..\..\..\MediaPortal.Base\dxerr9.dll .

copy ..\..\..\WebEPG\WebEPG\bin\Release\WebEPG.dll .
copy ..\..\..\Utils\bin\Release\Utils.dll .

copy ..\..\..\WebEPG\WebEPG-xmltv\bin\Release\WebEPG-xmltv.exe WebEPG.exe
copy ..\..\..\WebEPG\WebEPG-conf\bin\Release\WebEPG-conf.exe .
rem copy ..\..\..\WebEPG\WebEPG-channels\bin\Release\WebEPG-channels.exe .

rem Begin BASS Music Engine dependancies
copy ..\..\..\MediaPortal.Base\bass.dll .
copy ..\..\..\MediaPortal.Base\Bass.Net.dll .
copy ..\..\..\MediaPortal.Base\bass_vis.dll .
copy ..\..\..\MediaPortal.Base\bass_fx.dll .
copy ..\..\..\MediaPortal.Base\bassmix.dll .
copy ..\..\..\MediaPortal.Base\bassasio.dll .
copy ..\..\..\MediaPortal.Base\bass_vst.dll .
copy ..\..\..\MediaPortal.Base\bass_wadsp.dll .
copy ..\..\..\MediaPortal.Base\mpviz.dll .
copy ..\..\..\MediaPortal.Base\BassRegistration.dll .
rem End BASS Music Engine dependancies

rem MyDreambox plugin dependencies
copy ..\..\..\WindowPlugins\GUIMyDreambox\AxInterop.AXVLC.dll .
copy ..\..\..\WindowPlugins\GUIMyDreambox\Interop.AXVLC.dll .

rem MyBurner plugin dependencies
copy ..\..\..\WindowPlugins\GUIBurner\XPBurnComponent.dll .
copy ..\..\..\WindowPlugins\GUIBurner\madlldlib.dll .


@if exist postbuild2.bat call postbuild2.bat

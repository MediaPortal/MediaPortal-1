rem Check for Microsoft Antispyware .BAT bug
if exist .\kernel32.dll exit 1

cd
mkdir plugins
mkdir plugins\windows
mkdir plugins\TagReaders
mkdir plugins\subtitle
mkdir plugins\ExternalPlayers
mkdir plugins\process
mkdir Wizards

del /F /Q plugins\windows\*.*
del /F /Q plugins\tagreaders\*.*
del /F /Q plugins\subtitle\*.*
del /F /Q plugins\ExternalPlayers\*.*
del /F /Q plugins\process\*.*
del *.dll
del *.ax

copy ..\..\..\MPSA.ax .
copy ..\..\..\TsFileSource.ax .
copy ..\..\..\MPTSWriter.ax .
regsvr32 /s MPSA.ax
regsvr32 /s TSFileSource.ax.ax
regsvr32 /s MPTSWriter.ax
copy ..\..\..\core\directshowhelper\directshowhelper\release\dshowhelper.dll .
copy ..\..\..\core\fontengine\fontengine\release\fontengine.dll .
copy ..\..\..\Interop.DirectShowHelperLib.dll .
copy ..\..\..\AxInterop.MOZILLACONTROLLib.dll .
copy ..\..\..\Interop.MOZILLACONTROLLib.dll .
copy ..\..\..\mfc71.dll .
copy ..\..\..\msvcp71.dll .
copy ..\..\..\msvcr71.dll .
copy ..\..\..\Microsoft.ApplicationBlocks*.dll .
copy ..\..\..\d3dx9_26.dll .
copy ..\..\..\Microsoft.DirectX.Direct3D.dll .
copy ..\..\..\Microsoft.DirectX.Direct3DX.dll .
copy ..\..\..\Microsoft.DirectX.DirectDraw.dll .
copy ..\..\..\Microsoft.DirectX.dll .
copy ..\..\..\Microsoft.DirectX.DirectInput.dll .
rem ExternalDisplay plugin LCD driver DLLs
copy ..\..\..\FTD2XX.DLL .
copy ..\..\..\SG_VFD.dll .
if not exist LUI\. mkdir LUI
copy ..\..\..\LUI.dll LUI\.
copy ..\..\..\Communications.dll .
copy ..\..\..\Interop.GIRDERLib.dll .
copy ..\..\..\MediaPadLayer.dll .
rem 
copy ..\..\..\KCS.Utilities.dll .
copy ..\..\..\X10Plugin.* .
copy ..\..\..\X10Unified.* .
copy ..\..\..\xAPMessage.dll .
copy ..\..\..\xAPTransport.dll .
copy ..\..\..\mbm5.dll .
copy ..\..\..\madlldlib.dll .
copy ..\..\..\ECP2Assembly.dll .
copy ..\..\..\edtftpnet-1.1.8.dll .
copy ..\..\..\dvblib.dll .
copy ..\..\..\Interop.WMEncoderLib.dll .
copy ..\..\..\Interop.TunerLib.dll .
copy ..\..\..\Interop.iTunesLib.dll .
copy ..\..\..\Microsoft.Office.Interop.Outlook.dll .
copy ..\..\..\XPBurnComponent.dll .

copy ..\..\..\Configuration\Wizards\*.* Wizards
copy ..\..\..\Configuration\bin\Release\Configuration.exe .
copy ..\..\..\TVGuideScheduler\bin\Release\TVGuideScheduler.exe .

copy ..\..\..\core\bin\Release\Core.dll .
copy ..\..\..\tvcapture\bin\release\tvcapture.dll .
copy ..\..\..\databases\bin\release\databases.dll .
copy ..\..\..\SubtitlePlugins\bin\release\SubtitlePlugins.dll plugins\subtitle
copy ..\..\..\TagReaderPlugins\bin\release\TagReaderPlugins.dll plugins\TagReaders
copy ..\..\..\ExternalPlayers\bin\release\ExternalPlayers.dll plugins\ExternalPlayers
copy ..\..\..\WindowPlugins\bin\release\WindowPlugins.dll plugins\Windows
copy ..\..\..\WindowPlugins\GUIMSNPlugin\DotMSN.dll plugins\Windows
copy ..\..\..\ProcessPlugins\bin\release\ProcessPlugins.dll plugins\process\
copy ..\..\..\Dialogs\bin\release\Dialogs.dll plugins\Windows
copy ..\..\..\RemotePlugins\bin\release\RemotePlugins.dll .
copy ..\..\..\sqlite.dll .
copy ..\..\..\tag.exe .
copy ..\..\..\tag.cfg .
copy ..\..\..\TaskScheduler.dll .
copy ..\..\..\AxInterop.WMPLib.dll .
copy ..\..\..\Interop.WMPLib.dll .
copy ..\..\..\FireDTVKeyMap.XML .
copy ..\..\..\FireDTVKeyMap.XML.Schema .


copy ..\..\..\WebEPG\WebEPG\bin\Release\WebEPG.dll .
copy ..\..\..\Utils\bin\Release\Utils.dll .

copy ..\..\..\WebEPG\WebEPG\bin\Release\WebEPG.dll WebEPG\
copy ..\..\..\WebEPG\WebEPG-xmltv\bin\Release\WebEPG-xmltv.exe WebEPG.exe
copy ..\..\..\WebEPG\WebEPG-conf\bin\Release\WebEPG-conf.exe WebEPG\
copy ..\..\..\WebEPG\WebEPG-channels\bin\Release\WebEPG-channels.exe WebEPG\

@if exist postbuild2.bat call postbuild2.bat

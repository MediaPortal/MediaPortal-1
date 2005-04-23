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
copy ..\..\..\core\directshowhelper\directshowhelper\release\directshowhelper.dll .
regsvr32 /s directshowhelper.dll
copy ..\..\..\core\fontengine\fontengine\release\fontengine.dll .
copy ..\..\..\Interop.DirectShowHelperLib.dll .
copy ..\..\..\mfc71.dll .
copy ..\..\..\msvcp71.dll .
copy ..\..\..\msvcr71.dll .
copy ..\..\..\Microsoft.ApplicationBlocks*.dll .
copy ..\..\..\Microsoft.DirectX.Direct3D.dll .
copy ..\..\..\Microsoft.DirectX.Direct3DX.dll .
copy ..\..\..\Microsoft.DirectX.DirectDraw.dll .
copy ..\..\..\Microsoft.DirectX.dll .
copy ..\..\..\FTD2XX.DLL .
copy ..\..\..\SG_VFD.dll .
if not exist LUI\. mkdir LUI
copy ..\..\..\LUI.dll LUI\.
copy ..\..\..\Communications.dll .
copy ..\..\..\KCS.Utilities.dll .
copy ..\..\..\X10Plugin.* .
copy ..\..\..\X10Unified.* .
copy ..\..\..\xAPMessage.dll .
copy ..\..\..\xAPTransport.dll .
copy ..\..\..\mbm5.dll .
copy ..\..\..\madlldlib.dll .
copy ..\..\..\ECP2Assembly.dll .
copy ..\..\..\edtftpnet-1.1.3.dll .
copy ..\..\..\dvblib.dll .
copy ..\..\..\*.tpl .
copy ..\..\..\Interop.WMEncoderLib.dll .
copy ..\..\..\Interop.TunerLib.dll .
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

copy ..\..\..\sqlite.dll .
copy ..\..\..\SQLiteClient.dll .
copy ..\..\..\tag.exe .
copy ..\..\..\tag.cfg .
copy ..\..\..\TaskScheduler.dll .
copy ..\..\..\AxInterop.WMPLib.dll .

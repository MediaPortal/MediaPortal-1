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

copy ..\..\..\core\directshowhelper\directshowhelper\debug\directshowhelper.dll .
regsvr32 /s directshowhelper.dll
copy ..\..\..\core\fontengine\fontengine\debug\fontengine.dll .
copy ..\..\..\core\fontengine\fontengine\debug\fontengine.pdb .
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
copy ..\..\..\Configuration\Wizards\*.* Wizards
copy ..\..\..\Configuration\bin\debug\Configuration.exe .
copy ..\..\..\Configuration\bin\debug\Configuration.pdb .
copy ..\..\..\TVGuideScheduler\bin\debug\TVGuideScheduler.exe .
copy ..\..\..\TVGuideScheduler\bin\debug\TVGuideScheduler.pdb .
copy ..\..\..\mbm5.dll .
copy ..\..\..\madlldlib.dll .
copy ..\..\..\ECP2Assembly.dll .
copy ..\..\..\edtftpnet-1.1.3.dll .
copy ..\..\..\dvblib.dll .
copy ..\..\..\*.tpl .
copy ..\..\..\Interop.WMEncoderLib.dll .
copy ..\..\..\Interop.TunerLib.dll .
copy ..\..\..\XPBurnComponent.dll .

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
copy ..\..\..\WindowPlugins\GUIMSNPlugin\DotMSN.dll plugins\Windows

copy ..\..\..\ProcessPlugins\bin\debug\ProcessPlugins.dll plugins\process\
copy ..\..\..\ProcessPlugins\bin\debug\ProcessPlugins.pdb  plugins\process\

copy ..\..\..\sqlite.dll .
copy ..\..\..\SQLiteClient.dll .
copy ..\..\..\tag.exe .
copy ..\..\..\tag.cfg .
copy ..\..\..\TaskScheduler.dll .
copy ..\..\..\AxInterop.WMPLib.dll .

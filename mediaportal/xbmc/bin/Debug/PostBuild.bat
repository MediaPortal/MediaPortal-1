cd
mkdir plugins
mkdir plugins\windows
mkdir plugins\TagReaders
mkdir plugins\subtitle
mkdir plugins\ExternalPlayers
mkdir Wizards

del /F /Q plugins\windows\*.*
del /F /Q plugins\tagreaders\*.*
del /F /Q plugins\subtitle\*.*
del /F /Q plugins\ExternalPlayers\*.*
del *.dll
copy ..\..\..\Microsoft.ApplicationBlocks*.dll .
copy ..\..\..\Microsoft.DirectX.Direct3D.dll .
copy ..\..\..\Microsoft.DirectX.Direct3DX.dll .
copy ..\..\..\Microsoft.DirectX.DirectDraw.dll .
copy ..\..\..\Microsoft.DirectX.dll .
copy ..\..\..\Configuration\Wizards\*.* Wizards
copy ..\..\..\Configuration\bin\debug\Configuration.exe .
copy ..\..\..\Configuration\bin\debug\Configuration.pdb .
copy ..\..\..\TVGuideScheduler\bin\debug\TVGuideScheduler.exe .
copy ..\..\..\TVGuideScheduler\bin\debug\TVGuideScheduler.pdb .
copy ..\..\..\mbm5.dll .
copy ..\..\..\edtftpnet-1.1.3.dll .
copy ..\..\..\dvblib.dll .
copy ..\..\..\astra.tpl .
copy ..\..\..\Interop.WMEncoderLib.dll .
copy ..\..\..\Interop.TunerLib.dll .

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
copy ..\..\..\ProcessPlugins\bin\debug\ProcessPlugins.dll.pdb  plugins\process\

copy ..\..\..\sqlite.dll .
copy ..\..\..\SQLiteClient.dll .
copy ..\..\..\tag.exe .
copy ..\..\..\tag.cfg .
copy ..\..\..\TaskScheduler.dll .
copy ..\..\..\AxInterop.WMPLib.dll .

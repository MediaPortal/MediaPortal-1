mkdir plugins
mkdir plugins\windows
mkdir plugins\TagReaders
mkdir plugins\subtitle
mkdir plugins\ExternalPlayers

copy ..\..\..\GUIRSSFeed\bin\Debug\GUIRSSFeed.dll plugins\windows
copy ..\..\..\GUIPrograms\bin\Debug\GUIPrograms.dll plugins\windows
copy ..\..\..\GUIRadio\bin\Debug\GUIRadio.dll plugins\windows
copy ..\..\..\GUIMusic\bin\Debug\GUIMusic.dll plugins\windows
copy ..\..\..\GUIMusic\Freedb\bin\Debug\Freedb.dll plugins\windows
copy ..\..\..\GUIMusic\Ripper\bin\Debug\Ripper.dll plugins\windows
copy ..\..\..\Dialogs\bin\Debug\Dialogs.dll plugins\windows
copy ..\..\..\GUIPictures\bin\Debug\GUIPictures.dll plugins\windows
copy ..\..\..\GUITV\bin\Debug\GUITV.dll plugins\windows
copy ..\..\..\GUIVideoFiles\bin\Debug\GUIVideoFiles.dll plugins\windows
copy ..\..\..\GUIVideoFullScreen\bin\Debug\GUIVideoFullScreen.dll plugins\windows
copy ..\..\..\home\bin\Debug\home.dll plugins\windows
copy ..\..\..\SetupScreens\bin\Debug\SetupScreens.dll plugins\windows
copy ..\..\..\GUIWeather\bin\Debug\GUIWeather.dll plugins\windows
copy ..\..\..\SMIReader\bin\Debug\SMIReader.dll plugins\subtitle
copy ..\..\..\SRTReader\bin\Debug\SRTReader.dll plugins\subtitle
copy ..\..\..\RadioDatabase\bin\Debug\RadioDatabase.dll .
copy ..\..\..\MusicDatabase\bin\Debug\MusicDatabase.dll .
copy ..\..\..\PictureDatabase\bin\Debug\PictureDatabase.dll .
copy ..\..\..\VideoDatabase\bin\Debug\VideoDatabase.dll .
copy ..\..\..\TVDatabase\bin\Debug\TVDatabase.dll .
copy ..\..\..\TVCapture\bin\Debug\TVCapture.dll .
copy ..\..\..\GUITopbar\bin\debug\GUITopbar.dll plugins\windows
copy ..\..\..\TagReader\bin\Debug\TagReader.dll .
copy ..\..\..\sqlite.dll .
copy ..\..\..\SQLiteClient.dll .
copy ..\..\..\tag.exe .
copy ..\..\..\tag.cfg .


copy ..\..\..\mp3TagReader\bin\Debug\mp3TagReader.dll plugins\TagReaders
copy ..\..\..\mp3TagReader\NZLib\bin\Debug\zlib.dll plugins\TagReaders
copy ..\..\..\MultiTagReader\bin\Debug\MultiTagReader.dll plugins\TagReaders
copy ..\..\..\DShowNET\bin\Debug\DShowNET.dll .
copy ..\..\..\DirectX.Capture\bin\Debug\DirectX.Capture.dll .

copy ..\..\..\WinampExternalPlayer\bin\Debug\WinampExternalPlayer.dll plugins\ExternalPlayers
copy ..\..\..\WinampExternalPlayer\bin\Debug\WinampExternalPlayer.pdb plugins\ExternalPlayers


copy ..\..\..\GUIPrograms\bin\Debug\GUIPrograms.pdb plugins\windows
copy ..\..\..\GUIRSSFeed\bin\Debug\GUIRSSFeed.pdb plugins\windows
copy ..\..\..\GUIRadio\bin\Debug\GUIRadio.pdb plugins\windows
copy ..\..\..\GUIMusic\bin\Debug\GUIMusic.pdb plugins\windows
copy ..\..\..\GUIMusic\Freedb\bin\Debug\Freedb.pdb plugins\windows
copy ..\..\..\GUIMusic\Ripper\bin\Debug\Ripper.pdb plugins\windows
copy ..\..\..\Dialogs\bin\Debug\Dialogs.pdb  plugins\windows
copy ..\..\..\GUIPictures\bin\Debug\GUIPictures.pdb plugins\windows
copy ..\..\..\GUITV\bin\Debug\GUITV.pdb plugins\windows
copy ..\..\..\GUIVideoFiles\bin\Debug\GUIVideoFiles.pdb plugins\windows
copy ..\..\..\GUIVideoFullScreen\bin\Debug\GUIVideoFullScreen.pdb plugins\windows
copy ..\..\..\home\bin\Debug\home.pdb plugins\windows
copy ..\..\..\SetupScreens\bin\Debug\SetupScreens.pdb plugins\windows
copy ..\..\..\GUIWeather\bin\Debug\GUIWeather.pdb plugins\windows
copy ..\..\..\SMIReader\bin\Debug\SMIReader.pdb plugins\subtitle
copy ..\..\..\SRTReader\bin\Debug\SRTReader.pdb plugins\subtitle
copy ..\..\..\mp3TagReader\bin\Debug\mp3TagReader.pdb plugins\TagReaders
copy ..\..\..\MultiTagReader\bin\Debug\MultiTagReader.pdb plugins\TagReaders
copy ..\..\..\RadioDatabase\bin\Debug\RadioDatabase.pdb .
copy ..\..\..\MusicDatabase\bin\Debug\MusicDatabase.pdb .
copy ..\..\..\PictureDatabase\bin\Debug\PictureDatabase.pdb .
copy ..\..\..\VideoDatabase\bin\Debug\VideoDatabase.pdb .
copy ..\..\..\TVDatabase\bin\Debug\TVDatabase.pdb .
copy ..\..\..\DShowNET\bin\Debug\DShowNET.pdb .
copy ..\..\..\DirectX.Capture\bin\Debug\DirectX.Capture.pdb .
copy ..\..\..\TVCapture\bin\Debug\TVCapture.pdb .
copy ..\..\..\TagReader\bin\Debug\TagReader.pdb .
copy ..\..\..\GUITopbar\bin\debug\GUITopbar.pdb plugins\windows
cd
mkdir plugins
mkdir plugins\windows
mkdir plugins\TagReaders
mkdir plugins\subtitle

copy ..\..\..\Dialogs\bin\Release\Dialogs.dll .
copy ..\..\..\GUIMusic\bin\Release\GUIMusic.dll plugins\windows
copy ..\..\..\GUIPictures\bin\Release\GUIPictures.dll plugins\windows
copy ..\..\..\GUITV\bin\Release\GUITV.dll plugins\windows
copy ..\..\..\GUIVideoFiles\bin\Release\GUIVideoFiles.dll plugins\windows
copy ..\..\..\GUIVideoFullScreen\bin\Release\GUIVideoFullScreen.dll plugins\windows
copy ..\..\..\home\bin\Release\home.dll plugins\windows
copy ..\..\..\SetupScreens\bin\Release\SetupScreens.dll plugins\windows
copy ..\..\..\GUIWeather\bin\Release\GUIWeather.dll plugins\windows
copy ..\..\..\SMIReader\bin\Release\SMIReader.dll plugins\subtitle
copy ..\..\..\SRTReader\bin\Release\SRTReader.dll plugins\subtitle


copy ..\..\..\MusicDatabase\bin\release\MusicDatabase.dll .
copy ..\..\..\PictureDatabase\bin\release\PictureDatabase.dll .
copy ..\..\..\VideoDatabase\bin\release\VideoDatabase.dll .
copy ..\..\..\tvdatabase\bin\release\tvdatabase.dll .
copy ..\..\..\tvcapture\bin\release\tvcapture.dll .

copy ..\..\..\DShowNET\bin\Release\DShowNET.dll .
copy ..\..\..\DirectX.Capture\bin\Release\DirectX.Capture.dll .
copy ..\..\..\sqlite.dll .
copy ..\..\..\SQLiteClient.dll .

copy ..\..\..\TagReader\bin\Release\TagReader.dll .

copy ..\..\..\mp3TagReader\bin\Release\mp3TagReader.dll plugins\TagReaders
copy ..\..\..\mp3TagReader\NZLib\bin\release\zlib.dll plugins\TagReaders
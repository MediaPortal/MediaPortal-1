cd
mkdir plugins
mkdir plugins\windows
mkdir plugins\TagReaders
mkdir plugins\subtitle
mkdir plugins\ExternalPlayers
mkdir Wizards

copy ..\..\..\Configuration\Wizards\*.* Wizards
copy ..\..\..\USBUIRT\bin\Release\USBUIRT.dll .
copy ..\..\..\Configuration\bin\Release\Configuration.exe .
copy ..\..\..\Dialogs\bin\Release\Dialogs.dll .
copy ..\..\..\GUIRecipies\bin\Release\GUIRecipies.dll plugins\windows
copy ..\..\..\GUIPrograms\bin\Release\GUIPrograms.dll plugins\windows
copy ..\..\..\GUIRSSFeed\bin\Release\GUIRSSFeed.dll plugins\windows
copy ..\..\..\GUIRadio\bin\Release\GUIRadio.dll plugins\windows
copy ..\..\..\GUIMusic\bin\Release\GUIMusic.dll plugins\windows
copy ..\..\..\GUIMusic\Freedb\bin\Release\Freedb.dll plugins\windows
copy ..\..\..\GUIMusic\Ripper\bin\Release\Ripper.dll .
copy ..\..\..\GUIPictures\bin\Release\GUIPictures.dll plugins\windows
copy ..\..\..\GUITV\bin\Release\GUITV.dll plugins\windows
copy ..\..\..\GUIVideoFiles\bin\Release\GUIVideoFiles.dll plugins\windows
copy ..\..\..\GUIVideoFullScreen\bin\Release\GUIVideoFullScreen.dll plugins\windows
copy ..\..\..\home\bin\Release\home.dll plugins\windows
copy ..\..\..\SetupScreens\bin\Release\SetupScreens.dll plugins\windows
copy ..\..\..\GUIAlarm\bin\Release\GUIAlarm.dll plugins\windows
copy ..\..\..\GUIWeather\bin\Release\GUIWeather.dll plugins\windows
copy ..\..\..\GUIMyMail\bin\Release\MyMailPlugin.dll plugins\windows
copy ..\..\..\SMIReader\bin\Release\SMIReader.dll plugins\subtitle
copy ..\..\..\SRTReader\bin\Release\SRTReader.dll plugins\subtitle
copy ..\..\..\TVGuideScheduler\bin\Release\TVGuideScheduler.exe .

copy ..\..\..\RadioDatabase\bin\release\RadioDatabase.dll .
copy ..\..\..\MusicDatabase\bin\release\MusicDatabase.dll .
copy ..\..\..\PictureDatabase\bin\release\PictureDatabase.dll .
copy ..\..\..\VideoDatabase\bin\release\VideoDatabase.dll .
copy ..\..\..\tvdatabase\bin\release\tvdatabase.dll .
copy ..\..\..\tvcapture\bin\release\tvcapture.dll .
copy ..\..\..\GUITopbar\bin\release\GUITopbar.dll plugins\windows

copy ..\..\..\DShowNET\bin\Release\DShowNET.dll .
copy ..\..\..\DirectX.Capture\bin\Release\DirectX.Capture.dll .
copy ..\..\..\sqlite.dll .
copy ..\..\..\SQLiteClient.dll .
copy ..\..\..\tag.exe .
copy ..\..\..\tag.cfg .
copy ..\..\..\TaskScheduler.dll .
copy ..\..\..\mmedia\bin\Release\yeti.mmedia.dll plugins\TagReaders
copy ..\..\..\wmfsdk\bin\Release\yeti.wmfsdk.dll plugins\TagReaders


copy ..\..\..\TagReader\bin\Release\TagReader.dll .

copy ..\..\..\mp4TagReader\bin\Release\mp4TagReader.dll plugins\TagReaders
copy ..\..\..\mp3TagReader\bin\Release\mp3TagReader.dll plugins\TagReaders
copy ..\..\..\mp3TagReader\NZLib\bin\release\zlib.dll plugins\TagReaders
copy ..\..\..\MultiTagReader\bin\Release\MultiTagReader.dll plugins\TagReaders
copy ..\..\..\WmaTagReader\bin\Release\WmaTagReader.dll plugins\TagReaders
copy ..\..\..\WinampExternalPlayer\bin\Release\WinampExternalPlayer.dll plugins\ExternalPlayers
copy ..\..\..\FoobarExternalPlayer\bin\Release\FoobarExternalPlayer.dll plugins\ExternalPlayers
@echo off
if not exist ..\musicdatabaseV10.db3 goto help 

echo Converting the V8 Database to V10. Please wait ....
sqlite3 ..\musicdatabaseV10.db3 < MusicDBConvertFrom8To10.sql
echo Conversion finished
echo Now Please run a Import from Configuration to update the AlbumArtists
goto end

:help
echo Please run Configuration.exe first to create a V10 Database. 
echo Then WITHOUT doing an import, start the batch file again
:end
Pause
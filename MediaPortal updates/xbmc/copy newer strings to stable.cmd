REM This will copy newer strings.xml files into the stable build
PAUSE
cd bin\Release\skin
xcopy /F /W /S /D /Y ..\..\..\..\..\MP-trunk\xbmc\bin\Release\skin\*

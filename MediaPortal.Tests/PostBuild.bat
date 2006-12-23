cd bin\Debug
if not exist plugins mkdir plugins
if not exist plugins\TagReaders mkdir plugins\TagReaders

del /F /Q plugins\tagreaders\*.*

rem -------------------
rem Copy the Tagreader Plugin, needed to test the Tagreader
rem -------------------
copy ..\..\..\TagReaderPlugins\bin\debug\TagReaderPlugins.dll plugins\TagReaders
copy ..\..\..\TagReaderPlugins\bin\debug\TagReaderPlugins.pdb plugins\TagReaders

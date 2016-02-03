rem cd bin\%2
xcopy %1\..\Packages\Sqlite.3.10.0\sqlite.dll . /R /Y
xcopy %1\MediaPortal.Base\MediaInfo.* . /R /Y
xcopy %1\MediaPortal.Base\ssleay32.* . /R /Y
xcopy %1\MediaPortal.Base\libcurl.* . /R /Y
xcopy %1\MediaPortal.Base\libeay32.* . /R /Y
xcopy %1\..\DirectShowFilters\DirectShowHelper\bin\%2\dshowhelper.dll . /Y /D

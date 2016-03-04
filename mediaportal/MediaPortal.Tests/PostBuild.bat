rem cd bin\%2
xcopy %1\..\Packages\Sqlite.3.10.0\sqlite.dll . /R /Y
xcopy %1\..\Packages\MP-1-MediaInfolib.1.0.0\MediaInfo.* . /R /Y
xcopy %1\..\Packages\MP-1-MediaInfolib.1.0.0\ssleay32.* . /R /Y
xcopy %1\..\Packages\MP-1-MediaInfolib.1.0.0\libcurl.* . /R /Y
xcopy %1\..\Packages\MP-1-MediaInfolib.1.0.0\libeay32.* . /R /Y
xcopy %1\..\DirectShowFilters\DirectShowHelper\bin\%2\dshowhelper.dll . /Y /D

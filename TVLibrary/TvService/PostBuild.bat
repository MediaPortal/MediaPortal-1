REM %1 = Solution Directory
REM %2 = $(ConfigurationName) Debug/Release


rem xcopy /y %1\MediaPortal.Support\bin\%2\MediaPortal.Support.* .

xcopy %1\DirectShowLib\bin\%2\*.* . /R /Y
xcopy %1\Plugins\PowerScheduler\PowerScheduler.Interfaces\bin\%2\*.* . /R /Y
xcopy %1\Plugins\PowerScheduler\bin\%2\*.* Plugins\ /R /Y
xcopy %1\Plugins\ServerBlaster\ServerBlaster (Learn)\bin\%2\*.* Plugins\ /R /Y
xcopy %1\Plugins\PluginBase\bin\%2\*.* Plugins\ /R /Y
xcopy %1\Plugins\ComSkipLauncher\bin\%2\*.* Plugins\ /R /Y
xcopy %1\Plugins\ConflictsManager\bin\%2\*.* Plugins\ /R /Y
xcopy %1\Plugins\ServerBlaster\ServerBlaster\bin\%2\*.* Plugins\ /R /Y
xcopy %1\Plugins\TvMovie\bin\%2\*.* Plugins\ /R /Y
xcopy %1\Plugins\XmlTvImport\bin\%2\*.* Plugins\ /R /Y
xcopy %1\TVServer.Base\*.* . /R /Y
xcopy %1\SetupControls\bin\%2\*.* . /R /Y
xcopy %1\..\..\DirectShowFilters\StreamingServer\bin\%2\*.* . /R /Y
REM %1 = Solution Directory
REM %2 = $(ConfigurationName) Debug/Release


rem xcopy /y %1\MediaPortal.Support\bin\%2\MediaPortal.Support.* .

xcopy %1\DirectShowLib\bin\%2\*.* . /R /Y /D
xcopy %1\..\..\Common-MP-TVE3\PowerScheduler.Interfaces\bin\%2\*.* . /R /Y /D
xcopy %1\Plugins\PowerScheduler\bin\%2\*.* Plugins\ /R /Y /D
xcopy %1\Plugins\ServerBlaster\"ServerBlaster (Learn)"\bin\%2\*.* Plugins\ /R /Y /D
xcopy %1\Plugins\PluginBase\bin\%2\*.* Plugins\ /R /Y /D
xcopy %1\Plugins\ComSkipLauncher\bin\%2\*.* Plugins\ /R /Y /D
xcopy %1\Plugins\ConflictsManager\bin\%2\*.* Plugins\ /R /Y /D
xcopy %1\Plugins\ServerBlaster\ServerBlaster\bin\%2\*.* Plugins\ /R /Y /D
xcopy %1\Plugins\TvMovie\bin\%2\*.* Plugins\ /R /Y /D
xcopy %1\Plugins\XmlTvImport\bin\%2\*.* Plugins\ /R /Y /D
xcopy %1\Plugins\WebEPG\WebEPGPlugin\bin\%2\*.* Plugins\ /R /Y /D
xcopy %1\TVServer.Base\*.* . /R /Y /E /D
xcopy %1\SetupControls\bin\%2\*.* . /R /Y /D
xcopy %1\..\..\DirectShowFilters\StreamingServer\bin\%2\*.* . /R /Y /D
xcopy %1\..\..\DirectShowFilters\DXErr9\bin\%2\*.* . /R /Y /D
xcopy %1\SetupTv\bin\%2\SetupTv.* . /R /Y /D
xcopy %1\TvLibrary.Utils\bin\%2\*.* /R /Y /D
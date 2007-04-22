REM %1 = Solution Directory
REM %2 = $(ConfigurationName) Debug/Release

REM Copy All new files from base
xcopy %1\ProjectInfinity.Base\*.* . /E /R /Y /D

REM Controls
xcopy /y %1\ProjectInfinity.Controls\bin\%2\ProjectInfinity.Controls.dll .

REM Utilities
xcopy /y %1\ProjectInfinity.Utilities\bin\%2\ProjectInfinity.Utilities.dll .

REM Copy Plugins
REM MyTv
xcopy /y %1\mytv\MyTv\bin\%2\*.dll .
xcopy /y %1\mytv\MyTv\bin\%2\gentle.config .

REM Menu
xcopy /y %1\Plugins\Menu\bin\%2\Menu.dll Plugins\Menu\

REM MyPictures
xcopy /y %1\Plugins\MyPictures\bin\%2\MyPictures.dll Plugins\Menu\

REM MyVideos
xcopy /y %1\Plugins\MyVideos\bin\%2\MyVideos.dll .

REM MyVideos
xcopy /y %1\Plugins\MyWeather\bin\%2\MyWeather.dll .
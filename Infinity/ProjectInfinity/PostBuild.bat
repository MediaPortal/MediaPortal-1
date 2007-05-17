REM %1 = Solution Directory
REM %2 = $(ConfigurationName) Debug/Release

REM Copy All new files from base
xcopy %1\ProjectInfinity.Base\*.* . /E /R /Y /D

REM Controls
xcopy /y %1\ProjectInfinity.Controls\bin\%2\ProjectInfinity.Controls.* .

REM Utilities
xcopy /y %1\ProjectInfinity.Utilities\bin\%2\ProjectInfinity.Utilities.* .

REM Copy Plugins
REM MyTv
xcopy /y %1\dialogs\bin\%2\dialogs.* .
xcopy /y %1\keyboard\bin\%2\keyboard.* .
xcopy /y %1\Plugins\mytv\MyTv\bin\%2\mytv.* plugins\mytv\
xcopy /y %1\Plugins\mytv\MyTv\bin\%2\gentle*.* .
xcopy /y %1\Plugins\mytv\MyTv\bin\%2\log4net.* .
xcopy /y %1\Plugins\mytv\MyTv\bin\%2\MySql.Data.* .
xcopy /y %1\Plugins\mytv\MyTv\bin\%2\TvControl.* .
xcopy /y %1\Plugins\mytv\MyTv\bin\%2\TVDatabase.* .
xcopy /y %1\Plugins\mytv\MyTv\bin\%2\TvBusinessLayer.* .
xcopy /y %1\Plugins\mytv\MyTv\bin\%2\TvLibrary.Interfaces.* .
xcopy /y %1\Plugins\mytv\MyTv\bin\%2\gentle.config .

REM Menu
xcopy /y %1\Plugins\Menu\bin\%2\Menu.* Plugins\Menu\

REM MyVideos
xcopy /y %1\Plugins\MyVideos\bin\%2\MyVideos.* Plugins\MyVideo\

REM MyWeather
xcopy /y %1\Plugins\MyWeather\bin\%2\MyWeather.* Plugins\MyWeather\


REM MyPictures
xcopy /y %1\Plugins\MyPictures\bin\%2\MyPictures.* Plugins\MyPictures\


REM NowPlaying
xcopy /y %1\Plugins\NowPlaying\bin\%2\NowPlaying.* Plugins\NowPlaying\

REM MyVideos
xcopy /y %1\Plugins\MediaModule\bin\%2\MediaModule.* Plugins\MediaModule\
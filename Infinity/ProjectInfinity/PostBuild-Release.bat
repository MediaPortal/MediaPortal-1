REM $1 = Solution Directory
REM $2 = $(ConfigurationName) Debug/Release

REM Copy All new files from base
xcopy $1\ProjectInfinity.Base\*.* . /E /R /Y /D

REM Controls
xcopy /y $1\ProjectInfinity.Controls\bin\$2\ProjectInfinity.Controls.dll .

REM Utilities
xcopy /y $1\ProjectInfinity.Utilities\bin\$2\ProjectInfinity.Utilities.dll .

REM Copy Plugins
REM MyTv
xcopy /y $1\MyTv\bin\$2\*.dll .
xcopy /y $1\MyTv\bin\$2\gentle.config" .

REM MyVideos
xcopy /y $1\MyVideos\bin\$2\MyVideos.dll .

REM MyVideos
xcopy /y $1\MyWeather\bin\$2\MyWeather.dll .
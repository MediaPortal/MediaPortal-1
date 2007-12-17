REM %1 = Solution Directory
REM %2 = $(ConfigurationName) Debug/Release

REM Copy All new files from base
xcopy %1\ProjectInfinity.Base\*.* . /E /R /Y /D

REM Controls Copy of Media Library files
xcopy /y %1\MediaLibrary\bin\MediaLibrary.dll .
xcopy /y %1\MediaLibrary\bin\MediaManager.exe* .

REM Copy All Media Library plugins
xcopy %1\MediaLibrary\bin\Plugins\*.* Plugins /E /R /Y /D

REM Controls
xcopy /y %1\ProjectInfinity.Controls\bin\%2\ProjectInfinity.Controls.* .

REM Utilities
xcopy /y %1\ProjectInfinity.Utilities\bin\%2\ProjectInfinity.Utilities.* .

REM dialogs & Keyboards
xcopy /y %1\dialogs\bin\%2\dialogs.* .
xcopy /y %1\keyboard\bin\%2\keyboard.* .

REM All Projects now copy their own files 
REM see Project properties -> Build Events -> Post-Build command Line

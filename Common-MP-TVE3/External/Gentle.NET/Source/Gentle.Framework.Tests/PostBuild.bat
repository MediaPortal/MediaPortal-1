REM %1 = Configuration (debug/release)
REM %2 = Solution output directory
REM %3 = Target directory

if "%1" == "Debug" GOTO :Skip

xcopy /Y "%2Gentle.Common.dll" "%3"
xcopy /Y "%2Gentle.Framework.dll" "%3"
xcopy /Y "%2Gentle.Provider.MySQL.dll" "%3"
xcopy /Y "%2Gentle.Provider.SQLServer.dll" "%3"

:Skip

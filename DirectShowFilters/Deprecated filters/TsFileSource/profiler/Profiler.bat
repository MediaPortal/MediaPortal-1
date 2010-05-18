@echo off

COPY "%~1" save.ax

PREP /OM /AT "%~1"
rem PREP /OM /AT /SF "?FillBuffer@CTSFileSourcePin@@UAEJPAUIMediaSample@@@Z" "%~1"

COPY "%~dp1%~n1._x" "%~1"
PROFILE /I "%~dp1%~n1" /O "%~dp1%~n1" %2 %3 %4 %5 %6
PREP /M "%~dp1%~n1"

COPY save.ax "%~1"

PLIST /ST "%~dp1%~n1" > profile-time.log
PLIST /SC "%~dp1%~n1" > profile-hitcount.log
PLIST /SNS "%~dp1%~n1" > profile-coveragesummary.log
PLIST /STC "%~dp1%~n1" > profile-timechild.log


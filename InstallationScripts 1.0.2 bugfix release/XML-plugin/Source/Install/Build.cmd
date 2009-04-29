@ECHO OFF
Set MSSDK=D:\Program Files\Microsoft Platform SDK
Set VCDIR=D:\Program Files\Microsoft Visual C++ Toolkit 2003

::###################################::
Set PATH=%VCDIR%\bin;%MSSDK%\bin;%PATH%
Set INCLUDE=%MSSDK%\Include;%VCDIR%\include;%INCLUDE%
Set LIB=%MSSDK%\lib;%VCDIR%\lib;%LIB%

rc /r /Fo"Install.res" "Res\Install.rc"
cl /O1 Install.cpp /link kernel32.lib user32.lib advapi32.lib gdi32.lib shell32.lib Install.res /SUBSYSTEM:WINDOWS /MACHINE:I386 /OPT:NOWIN98 /NODEFAULTLIB /ENTRY:_WinMain

del Install.obj
del Install.res
@PAUSE

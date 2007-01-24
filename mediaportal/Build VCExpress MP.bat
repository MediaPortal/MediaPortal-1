::  rebuild and time MP build

time /t

"%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\vcsexpress.exe" /rebuild Release MediaPortal.VC#Express.sln

time /t > buildtime.txt

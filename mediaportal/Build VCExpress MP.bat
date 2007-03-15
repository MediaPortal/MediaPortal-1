::  rebuild and time MP build

time /t > buildtime.txt

"%ProgramFiles%\Microsoft Visual Studio 8\Common7\IDE\vcsexpress.exe" /build Release MediaPortal.VC#Express.sln

time /t > > buildtime.txt

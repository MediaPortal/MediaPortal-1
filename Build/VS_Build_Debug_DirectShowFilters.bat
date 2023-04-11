@echo off

if [%1]==[] (set ARCH=x86) ELSE (set ARCH=%1)
call "VS_build_DirectShowFilters.bat" debug build %ARCH%

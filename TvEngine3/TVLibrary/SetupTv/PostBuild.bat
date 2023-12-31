REM %1 = Solution Directory
REM %2 = $(ConfigurationName) Debug/Release


rem moved to own project and copy to TVService bin folder as postbuild event
xcopy %1\SetupTv\bin\%2\SetupTv.* %1\TVservice\bin\%2\* /R /Y /D
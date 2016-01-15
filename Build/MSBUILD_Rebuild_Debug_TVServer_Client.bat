@ECHO OFF

call "MSBUILD_Rebuild_Release_MediaPortal.bat" Release
call "MSBUILD_Rebuild_Release_TVServer_Client.bat" Debug
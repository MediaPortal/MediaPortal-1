# Microsoft Developer Studio Project File - Name="nsisXML" - Package Owner=<4>
# Microsoft Developer Studio Generated Build File, Format Version 6.00
# ** DO NOT EDIT **

# TARGTYPE "Win32 (x86) Dynamic-Link Library" 0x0102

CFG=nsisXML - Win32 Debug
!MESSAGE This is not a valid makefile. To build this project using NMAKE,
!MESSAGE use the Export Makefile command and run
!MESSAGE 
!MESSAGE NMAKE /f "nsisXML.mak".
!MESSAGE 
!MESSAGE You can specify a configuration when running NMAKE
!MESSAGE by defining the macro CFG on the command line. For example:
!MESSAGE 
!MESSAGE NMAKE /f "nsisXML.mak" CFG="nsisXML - Win32 Debug"
!MESSAGE 
!MESSAGE Possible choices for configuration are:
!MESSAGE 
!MESSAGE "nsisXML - Win32 Release" (based on "Win32 (x86) Dynamic-Link Library")
!MESSAGE "nsisXML - Win32 Debug" (based on "Win32 (x86) Dynamic-Link Library")
!MESSAGE "nsisXML - Win32 Release UNICODE" (based on "Win32 (x86) Dynamic-Link Library")
!MESSAGE 

# Begin Project
# PROP AllowPerConfigDependencies 0
# PROP Scc_ProjName ""
# PROP Scc_LocalPath ""
CPP=cl.exe
MTL=midl.exe
RSC=rc.exe

!IF  "$(CFG)" == "nsisXML - Win32 Release"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 0
# PROP BASE Output_Dir "Release"
# PROP BASE Intermediate_Dir "Release"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 0
# PROP Output_Dir "Release"
# PROP Intermediate_Dir "Release"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /MD /W3 /GX /Zi /O1 /D "NDEBUG" /D "WIN32" /D "_WINDOWS" /D "_MBCS" /YX /FD /c
# ADD CPP /nologo /MD /W3 /GX /Zi /O1 /D "NDEBUG" /D "WIN32" /D "_WINDOWS" /D "_MBCS" /YX /FD /c
# ADD BASE MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x409 /d "NDEBUG"
# ADD RSC /l 0x409 /d "NDEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib shlwapi.lib /nologo /dll /machine:I386 /OPT:REF /OPT:NOWIN98
# ADD LINK32 kernel32.lib user32.lib shlwapi.lib /nologo /dll /machine:I386 /out:"../bin/nsisXML.dll" /OPT:REF /OPT:NOWIN98
# Begin Special Build Tool
IntDir=.\Release
ProjDir=.
TargetDir=..\bin
SOURCE="$(InputPath)"
PostBuild_Desc=Building Sample Installer...
PostBuild_Cmds="%ProgramFiles%\NSIS\Unicode\makensis" /V2 "/DTARGETDIR=$(TargetDir)" "/DINTDIR=$(IntDir)" "$(ProjDir)\Sample.nsi"
# End Special Build Tool

!ELSEIF  "$(CFG)" == "nsisXML - Win32 Debug"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 1
# PROP BASE Output_Dir "Debug"
# PROP BASE Intermediate_Dir "Debug"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 1
# PROP Output_Dir "Debug"
# PROP Intermediate_Dir "Debug"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /MTd /W3 /Gm /GX /ZI /Od /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /YX /FD /GZ /c
# ADD CPP /nologo /W3 /Gm /GX /ZI /Od /D "_DEBUG" /D "WIN32" /D "_WINDOWS" /D "_MBCS" /FR /YX /FD /GZ /c
# ADD BASE MTL /nologo /D "_DEBUG" /mktyplib203 /win32
# ADD MTL /nologo /D "_DEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x409 /d "_DEBUG"
# ADD RSC /l 0x409 /d "_DEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /dll /debug /machine:I386 /pdbtype:sept
# ADD LINK32 kernel32.lib user32.lib shlwapi.lib /nologo /dll /debug /machine:I386 /pdbtype:sept
# Begin Special Build Tool
IntDir=.\Debug
ProjDir=.
TargetDir=.\Debug
SOURCE="$(InputPath)"
PostBuild_Desc=Building Sample Installer...
PostBuild_Cmds="%ProgramFiles%\NSIS\makensis" /V4 "/DTARGETDIR=$(TargetDir)" "/DINTDIR=$(IntDir)" "$(ProjDir)\Sample.nsi"
# End Special Build Tool

!ELSEIF  "$(CFG)" == "nsisXML - Win32 Release UNICODE"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 0
# PROP BASE Output_Dir "Release_UNICODE"
# PROP BASE Intermediate_Dir "Release_UNICODE"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 0
# PROP Output_Dir "Release_UNICODE"
# PROP Intermediate_Dir "Release_UNICODE"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /MD /W3 /GX /Zi /O1 /D "NDEBUG" /D "WIN32" /D "_WINDOWS" /D "_UNICODE" /D "UNICODE" /YX /FD /c
# ADD CPP /nologo /MD /W3 /GX /Zi /O1 /D "NDEBUG" /D "WIN32" /D "_WINDOWS" /D "_UNICODE" /D "UNICODE" /YX /FD /c
# ADD BASE MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x409 /d "NDEBUG"
# ADD RSC /l 0x409 /d "NDEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib shlwapi.lib /nologo /dll /machine:I386 /OPT:REF /OPT:NOWIN98
# ADD LINK32 kernel32.lib user32.lib shlwapi.lib /nologo /dll /machine:I386 /out:"../binU/nsisXML.dll" /OPT:REF /OPT:NOWIN98
# Begin Special Build Tool
IntDir=.\Release_UNICODE
ProjDir=.
TargetDir=..\binU
SOURCE="$(InputPath)"
PostBuild_Desc=Building Sample Installer...
PostBuild_Cmds="%ProgramFiles%\NSIS\Unicode\makensis" /V2 "/DTARGETDIR=$(TargetDir)" "/DINTDIR=$(IntDir)" "$(ProjDir)\Sample.nsi"
# End Special Build Tool

!ENDIF 

# Begin Target

# Name "nsisXML - Win32 Release"
# Name "nsisXML - Win32 Debug"
# Name "nsisXML - Win32 Release UNICODE"
# Begin Source File

SOURCE=.\License.txt
# End Source File
# Begin Source File

SOURCE=.\nsisXML.cpp
# End Source File
# Begin Source File

SOURCE=.\ReadMe.txt
# End Source File
# Begin Source File

SOURCE=.\Sample.nsi

!IF  "$(CFG)" == "nsisXML - Win32 Release"

# Begin Custom Build
ProjDir=.
InputPath=.\Sample.nsi

"$(ProjDir)\Sample.exe" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
	rem Force Post-Build Step

# End Custom Build

!ELSEIF  "$(CFG)" == "nsisXML - Win32 Debug"

# Begin Custom Build
ProjDir=.
InputPath=.\Sample.nsi

"$(ProjDir)\Sample.exe" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
	rem Force Post-Build Step

# End Custom Build

!ELSEIF  "$(CFG)" == "nsisXML - Win32 Release UNICODE"

# Begin Custom Build
ProjDir=.
InputPath=.\Sample.nsi

"$(ProjDir)\Sample.exe" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
	rem Force Post-Build Step

# End Custom Build

!ENDIF 

# End Source File
# End Target
# End Project

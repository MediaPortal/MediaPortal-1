Option Explicit

' *********************************************
' This function checks if a process is running 
' *********************************************
Function IsProcessRunning( strProcess )
    Dim Process, strObject
    IsProcessRunning = False
    strObject   = "winmgmts:\\.\root\cimv2"
    For Each Process in GetObject( strObject ).InstancesOf( "win32_process" )
		If UCase( Process.name ) = UCase( strProcess ) Then
            IsProcessRunning = True
            Exit Function
        End If
    Next
End Function


Dim   Shell, Shell2, objFolder, objFolderItem, logpath, logname, logold, lognew, syspath, FileSys, LogFile, TmpFile, strEcho, prockill, result, process

' Values taken from http://msdn.microsoft.com/en-us/library/bb774096(VS.85).aspx
Const ssfCOMMONAPPDATA = 35
Const ssfSYSTEM        = 37

process   = "MediaPortal"

Set Shell         = CreateObject("WScript.Shell")
Set Shell2        = CreateObject("Shell.Application")
Set objFolder     = Shell2.Namespace(ssfCOMMONAPPDATA)
Set objFolderItem = objFolder.Self
Set FileSys       = CreateObject("Scripting.FileSystemObject")

logpath           = FileSys.BuildPath(objFolderItem.Path, "Team MediaPortal\MediaPortal\log")
logname           = FileSys.GetBaseName(Wscript.ScriptName)
lognew            = FileSys.BuildPath(logpath, logname + ".log")
logold            = FileSys.BuildPath(logpath, logname + ".bak")

Set objFolder     = Shell2.Namespace(ssfSYSTEM)
Set objFolderItem = objFolder.Self

syspath           = objFolderItem.Path

If FileSys.FileExists(lognew) Then
  Set TmpFile = FileSys.GetFile(lognew)
  TmpFile.Copy(logold)
End If

' Clean up for all log files to avoid confusion
If FileSys.FileExists(lognew) Then
  Set TmpFile = FileSys.GetFile(lognew)
  TmpFile.Delete
End If

If not FileSys.FolderExists(logpath) Then
  FileSys.CreateFolder logpath
End If

Set LogFile = FileSys.CreateTextFile(lognew, True)

strEcho = Date() & "-" & Time() & ": Starting """ & Wscript.ScriptName & """ with """ & Wscript.FullName & """ (v. " & Wscript.Version & ")"
LogFile.writeline strEcho

strEcho = Date() & "-" & Time() & ": Looking for ""tskill.exe"" in """ & syspath & """"
LogFile.writeline strEcho

If FileSys.FileExists(FileSys.BuildPath(syspath, "tskill.exe")) Then

  strEcho = Date() & "-" & Time() & ": Kill utility will be ""tskill"""
  LogFile.writeline strEcho
  ' Using tskill to fix Mantis issue 1529
  prockill = "tskill " & process

Else

  strEcho = Date() & "-" & Time() & ": ""tskill.exe"" not found "
  LogFile.writeline strEcho
  strEcho = Date() & "-" & Time() & ": Kill utility will be ""taskkill"""
  LogFile.writeline strEcho	
  prockill = "taskkill /F /IM " & process & ".exe"

End If

strEcho = Date() & "-" & Time() & ": Executing """ & prockill & """"
LogFile.writeline strEcho	
result = Shell.Run (prockill, 0, True)
strEcho = Date() & "-" & Time() & ": Killed  """ & process & """ (Exit code=" & result & ")"
LogFile.writeline strEcho	

' Check for MediaPortal still running
do
  WScript.Sleep(100)
loop while IsProcessRunning ( process & ".exe" ) 

strEcho = Date() & "-" & Time() & ": Executing """ & process & """"
LogFile.writeline strEcho	
result = Shell.Run (process, 1, False)
strEcho = Date() & "-" & Time() & ": Started """ & process & """ (Exit code=" & result & ")" & vbcrlf
LogFile.writeline strEcho	

LogFile.Close

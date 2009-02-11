Option Explicit

Dim   Shell, Shell2, objFolder, objFolderItem, logpath, logold, lognew, syspath, FileSys, LogFile, TmpFile, strEcho, prockill, result, process

' Values taken from http://msdn.microsoft.com/en-us/library/bb774096(VS.85).aspx
Const ssfCOMMONAPPDATA = 35
Const ssfSYSTEM        = 37

process   = "MediaPortal"

Set Shell         = CreateObject("WScript.Shell")
Set Shell2        = CreateObject("Shell.Application")
Set objFolder     = Shell2.Namespace(ssfCOMMONAPPDATA)
Set objFolderItem = objFolder.Self

logpath           = objFolderItem.Path & "\Team MediaPortal\MediaPortal\log\"
lognew            = logpath + "\" + Wscript.ScriptName + ".log"
logold            = logpath + "\" + Wscript.ScriptName + ".bak"

Set objFolder     = Shell2.Namespace(ssfSYSTEM)
Set objFolderItem = objFolder.Self

syspath           = objFolderItem.Path

Set FileSys = CreateObject("Scripting.FileSystemObject")
If FileSys.FileExists(lognew) Then
	Set TmpFile = FileSys.GetFile(lognew)
  TmpFile.Copy(logold)
End If

' Clean up for all log files to avoid confusion
If FileSys.FileExists(logpath + "\restart.log") Then
	Set TmpFile = FileSys.GetFile(logpath + "\restart.log")
  TmpFile.Delete
End If

Set LogFile = FileSys.CreateTextFile(lognew,1)

strEcho = Date() & "-" & Time() & ": Starting """ & Wscript.ScriptName & """ with """ & Wscript.FullName & """ (v. " & Wscript.Version & ")"
LogFile.writeline strEcho

strEcho = Date() & "-" & Time() & ": Looking for ""tskill.exe"" in """ & syspath & """"
LogFile.writeline strEcho
If FileSys.FileExists(syspath + "\tskill.exe") Then

  strEcho = Date() & "-" & Time() & ": Kill utility will be ""tskill"""
  LogFile.writeline strEcho
  ' Using tskill to fix Mantis issue 1529
  prockill = "tskill " & process

Else

  strEcho = Date() & "-" & Time() & ": ""tskill.exe"" not found "
  LogFile.writeline strEcho
  strEcho = Date() & "-" & Time() & ": Kill utility will be ""taskkill"""
  LogFile.writeline strEcho	
  prockill = "taskkill /T /F /IM " & process & ".exe"

End If

strEcho = Date() & "-" & Time() & ": Executing """ & prockill & """"
LogFile.writeline strEcho	
result = Shell.Run (prockill, 0, True)
strEcho = Date() & "-" & Time() & ": Killed  """ & process & """ (Exit code=" & result & ")"
LogFile.writeline strEcho	

strEcho = Date() & "-" & Time() & ": Executing """ & process & """"
LogFile.writeline strEcho	
result = Shell.Run (process, 1, False)
strEcho = Date() & "-" & Time() & ": Started """ & process & """ (Exit code=" & result & ")" & vbcrlf
LogFile.writeline strEcho	

LogFile.Close

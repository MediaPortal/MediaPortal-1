Option Explicit

Dim Shell, FileSys, SysPath, LogFile, strEcho, prockill, log, result, process

Set Shell   = CreateObject("WScript.Shell")

process   = "MediaPortal"
log       = Shell.ExpandEnvironmentStrings("%ALLUSERSPROFILE%") + "\Team MediaPortal\MediaPortal\log\restart.log"

Set FileSys = CreateObject("Scripting.FileSystemObject")
Set SysPath = Filesys.GetSpecialFolder(1)                ' system32 folder
Set LogFile = FileSys.CreateTextFile(log,1)


strEcho = vbcrlf & Date() & "-" & Time() & ": Starting -" & Wscript.ScriptName & "-"
LogFile.writeline strEcho

If FileSys.FileExists(SysPath + "tskill.exe") Then
	
  strEcho = vbcrlf & Date() & "-" & Time() & ": ""tskill"" will be used"
  LogFile.writeline strEcho
  ' Using tskill to fix Mantis issue 1529
  prockill = "tskill " & process

Else

  strEcho = Date() & "-" & Time() & ": ""taskkill"" will be used"
  LogFile.writeline strEcho	
  prockill = "taskkill /T /F /IM " & process & ".exe"

End If

strEcho = Date() & "-" & Time() & ": Run -" & prockill & "-"
LogFile.writeline strEcho	
result = Shell.Run (prockill, 0, True)
strEcho = Date() & "-" & Time() & ": " & process & " killed   , exit code=" & result
LogFile.writeline strEcho	

strEcho = Date() & "-" & Time() & ": Run -" & process & "-"
LogFile.writeline strEcho	
result = Shell.Run (process, 1, False)
strEcho = Date() & "-" & Time() & ": " & process & " started  , exit code=" & result & vbcrlf
LogFile.writeline strEcho	

LogFile.Close

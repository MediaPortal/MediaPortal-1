Option Explicit
Dim process
Dim shell

set shell=createobject("WScript.Shell")

' This doesnt work always when resuming from S3 
'For Each process in GetObject("winmgmts:").ExecQuery("select * from Win32_Process where name='MediaPortal.exe'")
'	process.Terminate(0)
'Next

'WScript.Sleep 1750


' try to find a way to wait until the task is killed. This way we could use the 
' taskkill without /F and that would allow MP to save settings as its not forced to terminate

' shell.Run "taskkill /T /F /IM Mediaportal.exe", 0, true

' Using tskill to fix Mantis issue 1529
shell.Run "tskill Mediaportal", 0, true
shell.Run "mediaportal"


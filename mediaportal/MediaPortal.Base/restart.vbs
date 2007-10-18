Option Explicit
Dim process
Dim shell

For Each process in GetObject("winmgmts:").ExecQuery("select * from Win32_Process where name='MediaPortal.exe'")
	process.Terminate(0)
Next

set shell=createobject("WScript.Shell")
shell.Run "mediaportal"

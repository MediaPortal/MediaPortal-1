Option Explicit

Dim shell, filesys, syspath

Set Shell = createobject("WScript.Shell")
Set filesys = CreateObject("Scripting.FileSystemObject")
Set syspath  =  filesys.GetSpecialFolder(1)							' system32 folder
If filesys.FileExists(syspath + "tskill.exe") Then

  ' Using tskill to fix Mantis issue 1529
  Shell.Run "tskill Mediaportal", 0, true

Else
	
  Shell.Run "taskkill /T /F /IM Mediaportal.exe", 0, true

End If
Shell.Run "Mediaportal"

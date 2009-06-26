'On Error Resume Next

Set oWshShell = Wscript.CreateObject("Wscript.Shell")
Set oFSO = Wscript.CreateObject("Scripting.FileSystemObject")
strMyFolderPath = oFSO.GetParentFolderName(Wscript.ScriptFullName) ' get the folder where the script has been executed 

DQ = chr(34) ' fill a variable with a "  (double quote)

MpPath = "D:\Team MediaPortal\MediaPortal\startMP.vbs"
MpBgImage = "D:\Team MediaPortal\MediaPortal\Skin\ReVision\Media\Background.png"

' initialize The fullscreen window and keep the process running
oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " Text=" & DQ & DQ & " TextSize=60 BgImage=" & DQ & mpBgImage & DQ,,False 
oWshShell.Run DQ & strMyFolderPath & "\WaitForWindow.exe" & DQ & " ForeGroundWindowName=" & DQ & "FullScreenForm" & DQ,,True 

' update the text
oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " Text=" & DQ & "Refreshing 15%" & DQ,,True 

' stop the Tvservice
oWshShell.Run "net stop tvservice",,True

' update the text
oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " Text=" & DQ & "Refreshing 35%" & DQ,,True

' make sure the processes have been stopped
oWshShell.Run "taskkill /T /F /IM MediaPortal.exe",,True
oWshShell.Run "taskkill /T /F /IM TVService.exe",,True

' update the text
oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " Text=" & DQ & "Refreshing 65%" & DQ,,True
 
' reinit the TechnoTrend driver
oWshShell.Run "devcon restart *1131",,True

' update the text
oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " Text=" & DQ & "Refreshing 90%" & DQ,,True

' start the Tvservice
oWshShell.Run "net start tvservice",,True

' start MediaPortal
oWshShell.Run DQ & MpPath & DQ,,False ' Start MediaPortal

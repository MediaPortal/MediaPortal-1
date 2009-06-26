'On Error Resume Next

Set oWshShell = Wscript.CreateObject("Wscript.Shell")
Set oFSO = Wscript.CreateObject("Scripting.FileSystemObject")
strMyFolderPath = oFSO.GetParentFolderName(Wscript.ScriptFullName) ' get the folder where the script has been executed 

DQ = chr(34) ' fill a variable with a "  (double quote)

MpPath = strMyFolderPath & "\MediaPortal.exe"
MpBgImage = strMyFolderPath & "\skin\BlueTwo\Media\background.png"

oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " Text=" & DQ & DQ & " BgImage=" & DQ & mpBgImage & DQ,,False ' initialize The fullscreen window and keep the process running
oWshShell.Run DQ & strMyFolderPath & "\WaitForWindow.exe" & DQ & " ForeGroundWindowName=" & DQ & "FullScreenForm" & DQ,,True 

'WScript.Sleep 1500 ' give the first instance of FullscreenMsg time to establish in the memory <-- this is an alternative to the WaitForWindow tool

' set textcolor to white and the text to "Starting MediaPortal"
' activate the MP splash screen observer. Define that the fullscreen window should close itself if a foreground window that contains "MediaPortal - " in its name appears.
' set close timeout to 3 seconds
oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " Text=" & DQ & "Starting MediaPortal" & DQ & " TextColor=#FFFFFF ObservateMpStartup=true CloseOnForegroundWindowName=" & DQ & "MediaPortal - " & DQ & " CloseTimeOut=6000",,True 

oWshShell.Run DQ & MpPath & DQ,,False ' Start MediaPortal

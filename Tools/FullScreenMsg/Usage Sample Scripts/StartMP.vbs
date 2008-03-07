'On Error Resume Next

Set oWshShell = Wscript.CreateObject("Wscript.Shell")
Set oFSO = Wscript.CreateObject("Scripting.FileSystemObject")
strMyFolderPath = oFSO.GetParentFolderName(Wscript.ScriptFullName) ' get the folder where the script has been executed 

DQ = chr(34) ' fill a variable with a "  (double quote)

MpPath = strMyFolderPath & "\MediaPortal.exe"
MpBgImage = strMyFolderPath & "\skin\BlueTwo\Media\background.png"

oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " Text=" & DQ & DQ,,False // initialize The fullscreen window and keep the process running
oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " BgImage=" & DQ & mpBgImage & DQ,,True // load the back ground image and wait  

WScript.Sleep 500 ' give FullscreenMsg time to receive all information

oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " Text=" & DQ & "Starting MediaPortal" & DQ & " TextColor=#FFFFFF",,True ' set textcolor to white and the text to "Starting MediaPortal"
oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " ObservateMpStartup=true CloseOnForegroundWindowName=" & DQ & "MediaPortal - " & DQ,,True ' activate the MP splash screen observer. Define that the fullscreen window should close itself if a foreground window that contains "MediaPortal - " in its name appears.
oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " CloseTimeOut=3000",,True ' set close timeout to 3 seconds

oWshShell.Run DQ & MpPath & DQ,,False ' Start MediaPortal

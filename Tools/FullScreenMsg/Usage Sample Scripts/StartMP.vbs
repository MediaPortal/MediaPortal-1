'On Error Resume Next

Set oWshShell = Wscript.CreateObject("Wscript.Shell")
Set oFSO = Wscript.CreateObject("Scripting.FileSystemObject")
strMyFolderPath = oFSO.GetParentFolderName(Wscript.ScriptFullName) ' get the folder where the script has been executed 

DQ = chr(34) ' fill a variable with a "  (double quote)

MpPath = strMyFolderPath & "\MediaPortal.exe"
MpBgImage = strMyFolderPath & "\skin\BlueTwo\Media\background.png"

oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " BgImage=" & DQ & mpBgImage & DQ & " Text=" & DQ & DQ,,False 

WScript.Sleep 2000 ' give FullscreenMsg time to receive all information

oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " Text=" & DQ & "Starting MediaPortal" & DQ & " TextColor=#FFFFFF",,True ' set textcolor to white and the text to "Starting MediaPortal"
oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " ObservateMpStartup=true CloseOnForegroundWindowName=" & DQ & "MediaPortal - " & DQ,,True ' activate the MP splash screen observer.
oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " CloseTimeOut=3000",,True ' set close timeout to 3 seconds

oWshShell.Run DQ & MpPath & DQ,,False ' Start MediaPortal

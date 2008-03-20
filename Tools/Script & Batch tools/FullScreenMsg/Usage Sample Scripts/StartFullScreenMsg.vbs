'On Error Resume Next

Set oWshShell = Wscript.CreateObject("Wscript.Shell")
Set oFSO = Wscript.CreateObject("Scripting.FileSystemObject")
strMyFolderPath = oFSO.GetParentFolderName(Wscript.ScriptFullName) ' get the folder where the script has been executed 

DQ = chr(34) ' fill a variable with a "  (double quote)

MpPath = "D:\Team MediaPortal\MediaPortal\MediaPortal.exe"
MpBgImage = "D:\Team MediaPortal\MediaPortal\Skin\ReVision\Media\Background.png"

UserName = oWshShell.ExpandEnvironmentStrings("%UserName%")

If UserName ="MediaPortalUser" Then
 oWshShell.Run DQ & strMyFolderPath & "\FullscreenMsg.exe" & DQ & " Text=" & DQ & "..." & DQ & " TextSize=60 BgImage=" & DQ & mpBgImage & DQ,,False ' initialize The fullscreen window and keep the process running
 oWshShell.Run DQ & strMyFolderPath & "\WaitForWindow.exe" & DQ & " ForeGroundWindowName=" & DQ & "FullScreenForm" & DQ,,True 
End If

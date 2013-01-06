var FONT_DIR

!ifndef CSIDL_FONTS
  !define CSIDL_FONTS '0x14' ;Fonts directory path constant
!endif
!ifndef CSIDL_FLAG_CREATE
  !define CSIDL_FLAG_CREATE 0x8000
!endif

!AddPluginDir "${git_InstallScripts}\FontName-plugin\Plugin"

### Modified Code from FileFunc.nsh    ###
### Original by Instructor and kichik  ###

!ifmacrondef GetFileNameCall
        !macro GetFileNameCall _PATHSTRING _RESULT
                Push `${_PATHSTRING}`
              	Call GetFileName
               	Pop ${_RESULT}
        !macroend
!endif

!ifndef GetFileName
	!define GetFileName `!insertmacro GetFileNameCall`

	Function GetFileName
		Exch $0
		Push $1
		Push $2

		StrCpy $2 $0 1 -1
		StrCmp $2 '\' 0 +3
		StrCpy $0 $0 -1
		goto -3

		StrCpy $1 0
		IntOp $1 $1 - 1
		StrCpy $2 $0 1 $1
		StrCmp $2 '' end
		StrCmp $2 '\' 0 -3
		IntOp $1 $1 + 1
		StrCpy $0 $0 '' $1

		end:
		Pop $2
		Pop $1
		Exch $0
	FunctionEnd
!endif

### End Code From ###

!macro InstallTTF FontFile
  Push $0  
  Push $R0
  Push $R1
  Push $R2
  Push $R3
  
  !define Index 'Line${__LINE__}'
  
; Get the Font's File name
  ${GetFileName} ${FontFile} $0
  !define FontFileName $0

  SetOutPath $FONT_DIR
  IfFileExists "$FONT_DIR\${FontFileName}" ${Index} "${Index}-New"
  
  !ifdef FontBackup
    "${Index}-New:"
    ;Implementation of Font Backup Store
      WriteRegStr HKLM "${FontBackup}" "${FontFileName}" "OK"
      File '${FontFile}'
      goto ${Index}
  !else
    "${Index}-New:"
      File '${FontFile}'
      goto ${Index}
  !endif

${Index}:
  ClearErrors
  ReadRegStr $R0 HKLM "SOFTWARE\Microsoft\Windows NT\CurrentVersion" "CurrentVersion"
  IfErrors "${Index}-9x" "${Index}-NT"

"${Index}-NT:"
  StrCpy $R1 "Software\Microsoft\Windows NT\CurrentVersion\Fonts"
  goto "${Index}-GO"

"${Index}-9x:"
  StrCpy $R1 "Software\Microsoft\Windows\CurrentVersion\Fonts"
  goto "${Index}-GO"

"${Index}-GO:"
  ClearErrors
  !insertmacro FontName "$FONT_DIR\${FontFileName}"
  pop $R2
  IfErrors 0 "${Index}-Add"
    MessageBox MB_OK "$R2"
    goto "${Index}-End"
    
"${Index}-Add:"
  StrCpy $R2 "$R2 (TrueType)"
  ClearErrors
  ReadRegStr $R0 HKLM "$R1" "$R2"
  IfErrors 0 "${Index}-End"
    System::Call "GDI32::AddFontResourceA(t) i ('${FontFileName}') .s"
    WriteRegStr HKLM "$R1" "$R2" "${FontFileName}"
    goto "${Index}-End"

"${Index}-End:"

  !undef Index
  !undef FontFileName
  
  pop $R3
  pop $R2
  pop $R1
  Pop $R0  
  Pop $0
!macroend

!macro InstallFON FontFile FontName
  Push $0  
  Push $R0
  Push $R1

  !define Index 'Line${__LINE__}'
  
; Get the Font's File name
  ${GetFileName} ${FontFile} $0
  !define FontFileName $0
  
  SetOutPath $FONT_DIR
  IfFileExists "$FONT_DIR\${FontFileName}" ${Index} "${Index}-New"

  !ifdef FontBackup
    "${Index}-New:"
    ;Implementation of Font Backup Store
      WriteRegStr HKLM "${FontBackup}" "${FontFileName}" "OK"
      File '${FontFile}'
      goto ${Index}
  !else
    "${Index}-New:"
      File '${FontFile}'
      goto ${Index}
  !endif

${Index}:
  ClearErrors
  ReadRegStr $R0 HKLM "SOFTWARE\Microsoft\Windows NT\CurrentVersion" "CurrentVersion"
  IfErrors "${Index}-9x" "${Index}-NT"

"${Index}-NT:"
  StrCpy $R1 "Software\Microsoft\Windows NT\CurrentVersion\Fonts"
  goto "${Index}-GO"

"${Index}-9x:"
  StrCpy $R1 "Software\Microsoft\Windows\CurrentVersion\Fonts"
  goto "${Index}-GO"

"${Index}-GO:"
  ClearErrors
  ReadRegStr $R0 HKLM "$R1" "${FontName}"
  IfErrors 0 "${Index}-End"
    System::Call "GDI32::AddFontResourceA(t) i ('${FontFileName}') .s"
    WriteRegStr HKLM "$R1" "${FontName}" "${FontFileName}"
    goto "${Index}-End"

"${Index}-End:"

  !undef Index
  !undef FontFileName

  pop $R1
  Pop $R0  
  Pop $0
!macroend

; Uninstaller entries

!macro RemoveTTF FontFile
  Push $0  
  Push $R0
  Push $R1
  Push $R2
  Push $R3
  Push $R4

  !define Index 'Line${__LINE__}'
  
; Get the Font's File name
  ${GetFileName} ${FontFile} $0
  !define FontFileName $0

  IfFileExists "$FONT_DIR\${FontFileName}" ${Index} "${Index}-End"

${Index}:
  ClearErrors
  ReadRegStr $R0 HKLM "SOFTWARE\Microsoft\Windows NT\CurrentVersion" "CurrentVersion"
  IfErrors "${Index}-9x" "${Index}-NT"

"${Index}-NT:"
  StrCpy $R1 "Software\Microsoft\Windows NT\CurrentVersion\Fonts"
  goto "${Index}-GO"

"${Index}-9x:"
  StrCpy $R1 "Software\Microsoft\Windows\CurrentVersion\Fonts"
  goto "${Index}-GO"

  !ifdef FontBackup
  "${Index}-GO:"
  ;Implementation of Font Backup Store
    StrCpy $R2 ''
    ReadRegStr $R2 HKLM "${FontBackup}" "${FontFileName}"
    StrCmp $R2 'OK' 0 "${Index}-Skip"

    ClearErrors
    !insertmacro FontName "$FONT_DIR\${FontFileName}"
    pop $R2
    IfErrors 0 "${Index}-Remove"
    MessageBox MB_OK "$R2"
    goto "${Index}-End"    

  "${Index}-Remove:"
    StrCpy $R2 "$R2 (TrueType)"
    System::Call "GDI32::RemoveFontResourceA(t) i ('${FontFileName}') .s"
    DeleteRegValue HKLM "$R1" "$R2"
    DeleteRegValue HKLM "${FontBackup}" "${FontFileName}"
    EnumRegValue $R4 HKLM "${FontBackup}" 0
    IfErrors 0 "${Index}-NoError"
      MessageBox MB_OK "FONT (${FontFileName}) Removal.$\r$\n(Registry Key Error: $R4)$\r$\nRestart computer and try again. If problem persists contact your supplier."
      Abort "EnumRegValue Error: ${FontFileName} triggered error in EnumRegValue for Key $R4."
  "${Index}-NoError:"
    StrCmp $R4 "" 0 "${Index}-NotEmpty"
      DeleteRegKey HKLM "${FontBackup}" ; This will delete the key if there are no more fonts...
  "${Index}-NotEmpty:"
    Delete /REBOOTOK "$FONT_DIR\${FontFileName}"
    goto "${Index}-End"
  "${Index}-Skip:"
    goto "${Index}-End"
  !else
  "${Index}-GO:"
    
    ClearErrors
    !insertmacro FontName "$FONT_DIR\${FontFileName}"
    pop $R2
    IfErrors 0 "${Index}-Remove"
    MessageBox MB_OK "$R2"
    goto "${Index}-End"

  "${Index}-Remove:"
    StrCpy $R2 "$R2 (TrueType)"
    System::Call "GDI32::RemoveFontResourceA(t) i ('${FontFileName}') .s"
    DeleteRegValue HKLM "$R1" "$R2"
    delete /REBOOTOK "$FONT_DIR\${FontFileName}"
    goto "${Index}-End"
  !endif

"${Index}-End:"

  !undef Index
  !undef FontFileName

  pop $R4
  pop $R3
  pop $R2
  pop $R1
  Pop $R0  
  Pop $0
!macroend

!macro RemoveFON FontFile FontName
  Push $0  
  Push $R0
  Push $R1
  Push $R2
  Push $R3
  Push $R4

  !define Index 'Line${__LINE__}'

; Get the Font's File name
  ${GetFileName} ${FontFile} $0
  !define FontFileName $0

  IfFileExists "$FONT_DIR\${FontFileName}" ${Index} "${Index}-End"

${Index}:
  ClearErrors
  ReadRegStr $R0 HKLM "SOFTWARE\Microsoft\Windows NT\CurrentVersion" "CurrentVersion"
  IfErrors "${Index}-9x" "${Index}-NT"

"${Index}-NT:"
  StrCpy $R1 "Software\Microsoft\Windows NT\CurrentVersion\Fonts"
  goto "${Index}-GO"

"${Index}-9x:"
  StrCpy $R1 "Software\Microsoft\Windows\CurrentVersion\Fonts"
  goto "${Index}-GO"

  !ifdef FontBackup
  "${Index}-GO:"
  ;Implementation of Font Backup Store
    StrCpy $R2 ''
    ReadRegStr $R2 HKLM "${FontBackup}" "${FontFileName}"
    StrCmp $R2 'OK' "${Index}-Remove" "${Index}-Skip"

  "${Index}-Remove:"
    System::Call "GDI32::RemoveFontResourceA(t) i ('${FontFileName}') .s"
    DeleteRegValue HKLM "$R1" "${FontName}"
    DeleteRegValue HKLM "${FontBackup}" "${FontFileName}"
    EnumRegValue $R4 HKLM "${FontBackup}" 0
    IfErrors 0 "${Index}-NoError"
      MessageBox MB_OK "FONT (${FontFileName}) Removal.$\r$\n(Registry Key Error: $R4)$\r$\nRestart computer and try again. If problem persists contact your supplier."
      Abort "EnumRegValue Error: ${FontFileName} triggered error in EnumRegValue for Key $R4."
  "${Index}-NoError:"
    StrCmp $R4 "" 0 "${Index}-NotEmpty"
      DeleteRegKey HKLM "${FontBackup}" ; This will delete the key if there are no more fonts...
  "${Index}-NotEmpty:"
    Delete /REBOOTOK "$FONT_DIR\${FontFileName}"
    goto "${Index}-End"
  "${Index}-Skip:"
    goto "${Index}-End"
  !else
  "${Index}-GO:"
    System::Call "GDI32::RemoveFontResourceA(t) i ('${FontFileName}') .s"
    DeleteRegValue HKLM "$R1" "${FontName}"
    delete /REBOOTOK "$FONT_DIR\${FontFileName}"
    goto "${Index}-End"
  !endif

"${Index}-End:"

  !undef Index
  !undef FontFileName

  pop $R4
  pop $R3
  pop $R2
  pop $R1
  Pop $R0  
  Pop $0
!macroend
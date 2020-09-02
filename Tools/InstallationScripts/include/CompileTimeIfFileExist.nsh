
#**********************************************************************************************************#
#
#                        CompileTimeIfFileExist in compile time
#						
#
#**********************************************************************************************************#

# First script version to check if file exist in compile time #

# usage :
#Section 
#!insertmacro CompileTimeIfFileExist "$%windir%\explorer.exe" itsThere
#!ifdef itsThere
#MessageBox mb_Topmost yes
#!else
#MessageBox mb_Topmost no
#!endif
#SectionEnd

!macro CompileTimeIfFileExist path define
!tempfile tmpinc
!system 'IF EXIST "${path}" echo !define ${define} > "${tmpinc}"'
!include "${tmpinc}"
!delfile "${tmpinc}"
!undef tmpinc
!macroend

# Second script version to check if file exist in compile time #
# https://nsis.sourceforge.io/!ifexist_Macro                   #


#/* ${!IfExist}
#----------------------------------------------------------------------
#    ${!IfExist} "C:\SomeFile.txt"
#        !Error "File Exists!"
#    !else
#        !Error "File is Missing!"
#    !endif */
    !define !IfExist `!insertmacro _!IfExist ""`
 
#* ${!IfNExist}
#---------------------------------------------------------------------
#   ${!IfNExist} "C:\SomeFile.txt"
#       !Error "File is Missing!"
#   !else
#       !Error "File Exists!"
#   !endif */
 
    !define !IfNExist `!insertmacro _!IfExist "n"`
    !macro _!IfExist _OP _FilePath
        !ifdef !IfExistIsTrue
            !undef !IfExistIsTrue
        !endif
        !tempfile "!IfExistTmp"
        !system `IF EXIST "${_FilePath}" Echo !define "!IfExistIsTrue" > "${!IfExistTmp}"`
        !include /NONFATAL "${!IfExistTmp}"
        !delfile /NONFATAL "${!IfExistTmp}"
        !undef !IfExistTmp
        !if${_OP}def !IfExistIsTrue
    !macroend
	
; See http://nsis.sourceforge.net/Check_if_a_file_exists_at_compile_time for documentation
; usage : 
; ${!defineifexist} var_name file_name

!macro !defineifexist _VAR_NAME _FILE_NAME
	!tempfile _TEMPFILE
	!ifdef NSIS_WIN32_MAKENSIS
		; Windows - cmd.exe
		!system 'if exist "${_FILE_NAME}" echo !define ${_VAR_NAME} > "${_TEMPFILE}"'
	!else
		; Posix - sh
		!system 'if [ -e "${_FILE_NAME}" ]; then echo "!define ${_VAR_NAME}" > "${_TEMPFILE}"; fi'
	!endif
	!include '${_TEMPFILE}'
	!delfile '${_TEMPFILE}'
	!undef _TEMPFILE
!macroend
!define !defineifexist "!insertmacro !defineifexist"
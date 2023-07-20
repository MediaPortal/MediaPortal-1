RequestExecutionLevel User
ShowInstDetails Show
#Unicode False


Section 

FontInfo::GetFontName "$Fonts\times.ttf"
DetailPrint $0

SectionEnd


!include LogicLib.nsh
Function EnumNameStringsCallback
; $1 contains hex-encoded platform/encoding information you probably don't care about but we parse some of it here.
StrCpy $3 $1 4
StrCpy $1 $1 "" 4
${If} "0x$3" = 0
	StrCpy $3 "Unicode"
	${If} "0x$1" <= 0x00040000
		StrCpy $1 "UTF-16BE"
	${EndIf}
${ElseIf} "0x$3" = 1
	StrCpy $3 "MacOS"
${ElseIf} "0x$3" = 3
	StrCpy $3 "Windows"
${EndIf}

DetailPrint "$0 ($3 $1) $2"
; StrCpy $0 "" ; Stop enum
FunctionEnd

Section Enum

GetFunctionAddress $0 EnumNameStringsCallback
FontInfo::EnumNameStrings "$Fonts\Arial.ttf" $0

SectionEnd
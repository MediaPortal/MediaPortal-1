!ifdef NSIS_UNICODE
	!ifndef CALLANSIPLUGIN_INCLUDED
	!define CALLANSIPLUGIN_INCLUDED

	!ifndef CP_UTF8
		!define CP_UTF8 65001
	!endif
	!ifndef CP_ACP
		!define CP_ACP 0
	!endif

	!define PushAsANSI '!insertmacro PushAs_ ${CP_ACP}'
	!define PushAsUTF8 '!insertmacro PushAs_ ${CP_UTF8}'
	!macro PushAs_ ENCODING VAR
		Push ${VAR}
		Exch $0
		Push $1
		System::Call "kernel32::WideCharToMultiByte(i${ENCODING},,tr0,i-1,t.r1,i${NSIS_MAX_STRLEN},,)"
		Exch
		Pop $0
		Exch $1
	!macroend

	!define PopAsANSI '!insertmacro PopAs_ ${CP_ACP}'
	!define PopAsUTF8 '!insertmacro PopAs_ ${CP_UTF8}'
	!macro PopAs_ ENCODING VAR
		Exch $0
		Push $1
		System::Call "kernel32::MultiByteToWideChar(i${ENCODING},,tr0,i-1,t.r1,i${NSIS_MAX_STRLEN})"
		Exch
		Pop $0
		Exch $1
		Pop ${VAR}
	!macroend

	!endif ; CALLANSIPLUGIN_INCLUDED
!endif ; NSIS_UNICODE

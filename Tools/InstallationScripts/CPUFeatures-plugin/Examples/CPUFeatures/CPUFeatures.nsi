!define /date BUILD_DATE "%Y-%m-%d"
Caption "CPUFeatures Test-Suite"
BrandingText "Built on ${BUILD_DATE}"

!addincludedir  "..\..\Include"

!ifdef NSIS_UNICODE
	!addplugindir "..\..\Plugins\Release_Unicode"
	OutFile "CPUFeatures-Unicode.exe"
!else
	!addplugindir "..\..\Plugins\Release_ANSI"
	OutFile "CPUFeatures-ANSI.exe"
!endif

; include first in order to enable LogicLib support
!include "LogicLib.nsh"

; include to enable CPUFeatures functions
!include "CPUFeatures.nsh"

RequestExecutionLevel user
ShowInstDetails show

; Query basic CPU information
Section
	DetailPrint "--- Test #1 ---"

	${CPUFeatures.GetFlags} $0
	DetailPrint "CPU Flags: $0"
	
	${CPUFeatures.GetCount} $0
	DetailPrint "CPU Count: $0"
	
	${CPUFeatures.GetFeatures} $0
	DetailPrint "CPU Features: $0"

	${CPUFeatures.GetVendor} $0
	DetailPrint "CPU Vendor: $0"
SectionEnd

; Check individual feature flags
; Find a list of supported feature flags in CPUFeatures.nsh!
Section
	DetailPrint "--- Test #2 ---"

	${CPUFeatures.CheckFeature} "MMX1" $0
	DetailPrint "Has MMX: $0"

	${CPUFeatures.CheckFeature} "MMX2" $0
	DetailPrint "Has MMX2: $0"

	${CPUFeatures.CheckFeature} "SSE1" $0
	DetailPrint "Has SSE: $0"

	${CPUFeatures.CheckFeature} "SSE2" $0
	DetailPrint "Has SSE2: $0"

	${CPUFeatures.CheckFeature} "SSE3" $0
	DetailPrint "Has SSE3: $0"

	${CPUFeatures.CheckFeature} "SSSE3" $0
	DetailPrint "Has SSSE3: $0"

	${CPUFeatures.CheckFeature} "SSE4.2" $0
	DetailPrint "Has SSE4.2: $0"

	${CPUFeatures.CheckFeature} "AVX1" $0
	DetailPrint "Has AVX: $0"

	${CPUFeatures.CheckFeature} "AVX2" $0
	DetailPrint "Has AVX2: $0"

	${CPUFeatures.CheckFeature} "3DNOW" $0
	DetailPrint "Has 3DNOW: $0"

	${CPUFeatures.CheckFeature} "3DNOW_EX" $0
	DetailPrint "Has 3DNOW_EX: $0"

	${CPUFeatures.CheckFeature} "FMA3" $0
	DetailPrint "Has FMA3: $0"

	${CPUFeatures.CheckFeature} "FMA4" $0
	DetailPrint "Has FMA4: $0"

	; Next call is supposed to fail!
	${CPUFeatures.CheckFeature} "SSE7" $0
	DetailPrint "Has SSE7: $0"
SectionEnd

; Check multiple features at once
; Returns only "yes", if *all* features are supported
Section
	DetailPrint "--- Test #3 ---"

	${CPUFeatures.CheckAllFeatures} "MMX1,SSE1" $0
	DetailPrint "Has MMX+SSE: $0"

	${CPUFeatures.CheckAllFeatures} "MMX1,3DNOW" $0
	DetailPrint "Has MMX1+3DNOW: $0"

	${CPUFeatures.CheckAllFeatures} "MMX1,SSE1,SSE2,SSE3,SSSE3" $0
	DetailPrint "Has MMX+SSE+SSE2+SSE3+SSSE3: $0"

	${CPUFeatures.CheckAllFeatures} "MMX1,SSE1,SSE2,SSE3,SSSE3,SSE4" $0
	DetailPrint "Has MMX+SSE+SSE2+SSE3+SSSE3+SSE4: $0"

	; Next call is supposed to fail!
	${CPUFeatures.CheckAllFeatures} "MMX1,SSE1,SSE2,SSE3,SSSE3,SSE7" $0
	DetailPrint "Has MMX+SSE+SSE2+SSE3+SSSE3+SSE7: $0"
SectionEnd

; Use LogicLib to check CPU features
Section
	DetailPrint "--- Test #4 ---"

	${If} ${CPUSupports} "MMX1"
		DetailPrint "This CPU spports MMX"
	${EndIf}
	${If} ${CPUSupports} "SSE1"
		DetailPrint "This CPU spports SSE"
	${EndIf}
	${If} ${CPUSupports} "SSSE3"
		DetailPrint "This CPU spports SSSE3"
	${EndIf}
	${If} ${CPUSupports} "3DNOW"
		DetailPrint "This CPU spports SSSE3"
	${EndIf}
	${If} ${CPUSupports} "AVX1"
		DetailPrint "This CPU spports AVX"
	${EndIf}

	${If} ${CPUSupportsAll} "MMX1,SSE1"
		DetailPrint "This CPU spports MMX+SSE"
	${EndIf}
	${If} ${CPUSupportsAll} "MMX1,3DNOW"
		DetailPrint "This CPU spports MMX+3DNOW"
	${EndIf}
	${If} ${CPUSupportsAll} "MMX1,SSSE3"
		DetailPrint "This CPU spports MMX+SSSE3"
	${EndIf}
	${If} ${CPUSupportsAll} "MMX1,AVX1"
		DetailPrint "This CPU spports MMX+AVX"
	${EndIf}

	${If} ${CPUIsIntel}
		DetailPrint "This CPU is an Intel"
	${EndIf}
	${If} ${CPUIsAMD}
		DetailPrint "This CPU is an AMD"
	${EndIf}
SectionEnd

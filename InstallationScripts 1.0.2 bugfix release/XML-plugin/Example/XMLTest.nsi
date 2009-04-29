Name "XMLTest"
OutFile "XMLTest.exe"

!include "XML.nsh"
!include "Sections.nsh"

Var RADIOBUTTON

Page components
Page instfiles


Section "Read 'BBB' example 1" Read1
	${xml::LoadFile} "test.xml" $0
	MessageBox MB_OK "xml::LoadFile$\n$$0=$0"

	${xml::RootElement} $0 $1
	MessageBox MB_OK "xml::RootElement$\n$$0=$0$\n$$1=$1"

	${xml::FirstChildElement} "" $0 $1
	MessageBox MB_OK "xml::FirstChildElement$\n$$0=$0$\n$$1=$1"

	${xml::NextSiblingElement} "b" $0 $1
	MessageBox MB_OK "xml::NextSiblingElement$\n$$0=$0$\n$$1=$1"

	${xml::FirstChild} "" $0 $1
	MessageBox MB_OK "xml::FirstChild$\n$$0=$0$\n$$1=$1"

	${xml::Unload}
SectionEnd


Section /o "Read 'BBB' example 2" Read2
	${xml::LoadFile} "test.xml" $0
	MessageBox MB_OK "xml::LoadFile$\n$$0=$0"

	${xml::GotoPath} "/a/b" $0
	MessageBox MB_OK "xml::GotoPath$\n$$0=$0"

	${xml::GetText} $0 $1
	MessageBox MB_OK "xml::GetText$\n$$0=$0$\n$$1=$1"

	${xml::Unload}
SectionEnd


Section /o "Read attributes example" ReadAttributes
	${xml::LoadFile} "test.xml" $0
	MessageBox MB_OK "xml::LoadFile$\n$$0=$0"

	${xml::GotoPath} "/a[2]/d" $0
	MessageBox MB_OK "xml::GotoPath$\n$$0=$0"

	${xml::GetAttribute} "attr2" $0 $1 
	MessageBox MB_OK "xml::GetAttribute$\n$$0=$0$\n$$1=$1"

	loop:
	${xml::NextAttribute} $0 $1 $2
	MessageBox MB_YESNO "xml::NextAttribute$\n$$0=$0$\n$$1=$1$\n$$2=$2$\n$\nContinue?" IDYES loop

	#Used only for unload plugin
	${xml::NodeHandle} $0

	${xml::Unload}
SectionEnd


Section /o "Node replace example" NodeReplace
	${xml::LoadFile} "test.xml" $0
	MessageBox MB_OK "xml::LoadFile$\n$$0=$0"

	${xml::RootElement} $0 $1
	MessageBox MB_OK "xml::RootElement$\n$$0=$0$\n$$1=$1"

	${xml::NextSiblingElement} "" $0 $1
	MessageBox MB_OK "xml::NextSiblingElement$\n$$0=$0$\n$$1=$1"

	${xml::NodeHandle} $R0
	MessageBox MB_OK "xml::NodeHandle$\n$$R0=$R0"

	${xml::RootElement} $0 $1
	MessageBox MB_OK "xml::RootElement$\n$$0=$0$\n$$1=$1"

	${xml::FirstChildElement} "" $0 $1
	MessageBox MB_OK "xml::FirstChildElement$\n$$0=$0$\n$$1=$1"

	${xml::ReplaceNode} "$R0" $0
	MessageBox MB_OK "xml::ReplaceNode$\n$$0=$0"

	${xml::SaveFile} "test_saved.xml" $0
	MessageBox MB_OK "xml::SaveFile$\n$$0=$0"

	${xml::Unload}
SectionEnd


Section /o "Search element example" SearchElement
	${xml::LoadFile} "test.xml" $0
	MessageBox MB_OK "xml::LoadFile$\n$$0=$0"

	loop:
	${xml::FindNextElement} "a" $0 $1
	MessageBox MB_OK "xml::FindNextElement$\n$$0=$0$\n$$1=$1"

	${xml::ElementPath} $0
	MessageBox MB_YESNO "xml::ElementPath$\n$$0=$0$\n$\nContinue?" IDYES loop

	${xml::FindCloseElement}
	MessageBox MB_OK "xml::FindCloseElement"

	${xml::Unload}
SectionEnd


Section /o "XPath example" XPath
	${xml::LoadFile} "test.xml" $0
	MessageBox MB_OK "xml::LoadFile$\n$$0=$0"

	${xml::RootElement} $0 $1
	MessageBox MB_OK "xml::RootElement$\n$$0=$0$\n$$1=$1"

	${xml::XPathString} "count(//*/comment())" $0 $1
	MessageBox MB_OK "xml::XPathString$\n$$0=$0$\n$$1=$1"

	${xml::Unload}
SectionEnd


Function .onInit
	StrCpy $RADIOBUTTON ${Read1}
FunctionEnd

Function .onSelChange
	!insertmacro StartRadioButtons $RADIOBUTTON
	!insertmacro RadioButton ${Read1}
	!insertmacro RadioButton ${Read2}
	!insertmacro RadioButton ${ReadAttributes}
	!insertmacro RadioButton ${NodeReplace}
	!insertmacro RadioButton ${SearchElement}
	!insertmacro RadioButton ${XPath}
	!insertmacro EndRadioButtons
FunctionEnd

; nsisXML - Sample script

!ifndef TARGETDIR
!ifdef NSIS_UNICODE
!define TARGETDIR "..\binU"
!else
!define TARGETDIR "..\bin"
!endif
!endif

!addplugindir "${TARGETDIR}"

Name "Sample nsisXML"
OutFile "Sample.exe"
ShowInstDetails show	

Section "Main program"
	nsisXML::create
	nsisXML::createProcessingInstruction "xml" 'version="1.0" encoding="UTF-8" standalone="yes"'
	nsisXML::appendChild
	nsisXML::createElement "main"	; create main document element node
	nsisXML::appendChild
	StrCpy $1 $2			; lets move to this node and create its childs:
	nsisXML::createElementInNS "child" "namespace"
	nsisXML::setAttribute "attrib" "value"
	nsisXML::setText "content"
	nsisXML::appendChild
	nsisXML::createElement "child"
	nsisXML::setAttribute "attrib" "value2"
	nsisXML::setAttribute "active" "yes"
	nsisXML::setText "content2"
	nsisXML::appendChild
	nsisXML::save "Sample.xml"

	MessageBox MB_OK "Sample.xml was created$\nPress OK to continue sample"

	; In this section, we decided to care about memory leaks, so we use "release" adequately
	nsisXML::create
	nsisXML::load "Sample.xml"
	nsisXML::select '/main/child[@attrib="value2"]'
	IntCmp $2 0 notFound
	nsisXML::getAttribute "active"
	DetailPrint "Attribute 'active' is $3"
	nsisXML::getText
	DetailPrint "Tag <child> contains $3"
	nsisXML::parentNode
	nsisXML::removeChild
	nsisXML::release $2
	nsisXML::release $1
	nsisXML::save "Sample.xml"
	nsisXML::release $0
	Goto end
notFound:
	DetailPrint "XPath not resolved"
end:
SectionEnd

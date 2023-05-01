nsisXML -- Small NSIS plugin to manipulate XML data through MSXML
Web site: http://wiz0u.free.fr/prog/nsisXML
-----------------------------------------------------------------

You can typically use it to:
- create, read, modify, write XML files
- create, remove, search XML nodes
- set/get nodes content or attributes

Notes
-----
* For simplicity, I decided to use variables $0 through $3 for most parameters and
	return values, instead of pushing/popping values using the stack
	Therefore, avoid using these variables for something else while dealing with
	nsisXML
* UTF8 text is generated for characters outside 7-bits ASCII range but I'm not
	sure about codepage issue if the installer runs on a different codepage system
	I suggest you don't supply constant strings in NSIS with such characters to
	nsisXML. However strings based on the target system (like paths) should be ok.
* Newly-created XML file won't be nicely indented (but it's still correct XML !)
	If you load, modify and save an XML file that was already indented, it will be
	nicely indented (this is how MSXML works)
* Plugin requires, of course, that MSXML be already installed on the target system
	It should run fine with any version of MSXML and will automatically use the most recent one.
	Check MS Knowledge Base Q269238 for versions of MSXML shipped with Windows & Internet Explorer
	(In summary: every computer that has IE 4.0, or more recent, has MSXML installed)

Variables meaning
-----------------
$0	reference to the XML document root (integer)
$1	reference to a parent node (integer)
$2	reference to a child node (integer)
$3	depends - see functions description

Usage
-----
nsisXML::create
	creates an empty XML document in memory and returns its reference in $0 and $1

nsisXML::load <filename>
	load given XML file into document $0
	REQUIRES that an XML document be already created in $0
	(if XML document $0 is not empty, all content will be discarded)
	sets $0 to 0 if an error occured

nsisXML::loadAndValidate <filename>
	same as load but performs validation on the XML if a schema is present, and
	resolves external definitions (may try to establish an Internet connection)

nsisXML::save <filename>
	saves current document $0 to given file

nsisXML::createProcessingInstruction <target> <data>
	creates a new 'processing instruction' node and return its reference in $2

nsisXML::createElement <tag name>
	creates a new element node with given tag name (initially empty) and return its
	reference in $2

nsisXML::createElementInNS <tag name> <namespace>
	creates a new element node with given tag name (initially empty) inside the given
	namespace, and return its reference in $2

nsisXML::appendChild
	appends node $2 as a child of node $1

nsisXML::insertBefore
	insert node $2 as a child of node $1, to the left of node $3
	($3 must be a reference to a child node of $1)

nsisXML::setAttribute <attribute name> <value>
	sets the given attribute with the given value on node $2

nsisXML::getAttribute <attribute name>
	returns in $3 the value for the given attribute name of node $2
	if attribute is not found, returns an empty string

nsisXML::setText <content text>
	sets the text content of the node $2 (<>the text between the tags</>)

nsisXML::getText
	returns in $3 the text content for node $2

nsisXML::select <XPath expression>
	find the first node matching the given XPath expression and return its
	reference in $1 and $2
	if not found, reference will be 0

nsisXML::parentNode
	returns in $1 the reference on the parent node of node $2

nsisXML::removeChild
	removes node $2 from the childs list of node $1

nsisXML::release <node reference>
	release immediately the memory associated with given node (typically $2, $1 or $0)
	for every nsisXML function that returns a node reference, you may want to release its memory after you're done working with the reference.
	however, please note that in any case, all memory used by MSXML will be released when your installer terminates.
	(so usually this is not a big concern)




Sample script #1
-----------------
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

This script creates the following XML file:
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<main><child attrib="value">content</child><child attrib="value2" active="yes">content2</child></main>

Sample script #2
-----------------
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
	nsisXML::save "Sample.xml"

If applied on the previous "Sample.xml" file, this will print out "value2",
then "content2", and modify Sample.xml to become:
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<main><child attrib="value">content</child></main>

History
-------
2012-08-10: Added "createElementInNS" for xmlns (namespace) support
2010-04-02: Added "release" to prevent memory leaks.
			Automatic use of most recent MSXML available
			DLL is no longer unloaded after each call (you don't need /NOUNLOAD anymore)
2010-02-04: Added variant for Unicode NSIS
2006-03-22: Added "insertBefore"
2006-03-13: Modified "load". Added "loadAndValidation"

License
-------
Copyright (c) 2005-2012 Olivier Marcoux

This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.

Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

    1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.

    2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.

    3. This notice may not be removed or altered from any source distribution.

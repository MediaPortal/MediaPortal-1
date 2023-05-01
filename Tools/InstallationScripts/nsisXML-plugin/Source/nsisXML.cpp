/*
nsisXML -- Small NSIS plugin to manipulate XML data through MSXML
Web site: http://wiz0u.free.fr/prog/nsisXML

Copyright (c) 2005-2012 Olivier Marcoux

This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.

Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

    1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.

    2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.

    3. This notice may not be removed or altered from any source distribution.
*/

#define _WINSOCKAPI_	// this is to prevent conflicts on "select" function name
#include <windows.h>
#include <tchar.h>
#include <shlwapi.h>
#include <comdef.h>

#import "MSXML.DLL"

typedef struct {
  void *exec_flags;
  int  (__stdcall *ExecuteCodeSegment)(int, HWND);
  void (__stdcall *validate_filename)(TCHAR *);
  int  (__stdcall *RegisterPluginCallback)(HMODULE, UINT_PTR (*NSISPLUGINCALLBACK)(enum NSPIM)); // returns 0 on success, 1 if already registered and < 0 on errors
} extra_parameters;

#define NSIS_FUNC(name) \
extern "C" void __declspec(dllexport) name(HWND hwndParent, int string_size, \
                                      TCHAR *variables, stack_t **stacktop, extra_parameters *extra)

typedef struct _stack_t {
  struct _stack_t *next;
  TCHAR text[1]; // this should be the length of string_size
} stack_t;

static INT_PTR __stdcall pop_int(stack_t **&stacktop)
{
  stack_t *th;
  if (!stacktop || !*stacktop) return 0;
  th=(*stacktop);
  INT_PTR result = _ttoi(th->text);
  *stacktop = th->next;
  GlobalFree((HGLOBAL)th);
  return result;
}

static _bstr_t __stdcall pop_bstr(stack_t **&stacktop)
{
  stack_t *th;
  if (!stacktop || !*stacktop) return _bstr_t();
  th=(*stacktop);
  _bstr_t bstr(th->text);
  *stacktop = th->next;
  GlobalFree((HGLOBAL)th);
  return bstr;
}

#define pop(name) _bstr_t name = pop_bstr(stacktop);

#define get(varnum) \
	((LPDISPATCH) _ttoi(variables+varnum*string_size))

void _set(TCHAR *var, LPDISPATCH value)
{
	LPDISPATCH ptr = value;
	if (ptr != NULL) ptr->AddRef();
	wsprintf(var, _T("%d"), ptr);
}
#define set(varnum, value) _set(variables + varnum*string_size, value)

#define settvar(varnum, var) \
	lstrcpy(variables + varnum*string_size, var);
#define var2var(src, dst) \
	lstrcpy(variables + dst*string_size, variables + src*string_size);
#ifdef UNICODE
#define setwvar(varnum, var) settvar(varnum, var)
#else
#define setwvar(varnum, var) \
	WideCharToMultiByte(CP_ACP, 0, var, -1, variables + varnum*string_size, string_size, NULL, NULL);
#endif

#define get_doc() MSXML::IXMLDOMDocumentPtr doc = get(0)
#define get_parent() MSXML::IXMLDOMNodePtr parent = get(1)
#define set_parent(item) set(1, item)
#define get_child(type) MSXML::type ## Ptr child = get(2)
#define set_child(item) set(2, item)
#define get_child2(type) MSXML::type ## Ptr child2 = get(3)

const LPCSTR DOMDocuments[] = // DOMDocument UUID from all the available MSXML DLL variants
{
	"{2933bf90-7b36-11d2-b20e-00c04f983e60}",	//	MSXML::DOMDocument
	"{f6d90f11-9c73-11d3-b32e-00c04f990bb4}",	//	MSXML2::DOMDocument
	"{f5078f1b-c551-11d3-89b9-0000f81fe221}",	//	MSXML2::DOMDocument26
	"{f5078f32-c551-11d3-89b9-0000f81fe221}",	//	MSXML2::DOMDocument30
	"{88d969c0-f192-11d4-a65f-0040963251e5}",	//	MSXML2::DOMDocument40	(first to support Axes like following-sibling)
	"{88d96a05-f192-11d4-a65f-0040963251e5}",	//	MSXML2::DOMDocument60
};

HMODULE g_hInstance;
UINT_PTR NSISCallback(enum NSPIM msg) { return 0; } // we get notified of unloading on termination of the installer process

NSIS_FUNC(create)
{
	if (extra) extra->RegisterPluginCallback(g_hInstance, NSISCallback); // to prevent unloading of this plugin
	MSXML::IXMLDOMDocumentPtr doc = NULL;
	int index = sizeof(DOMDocuments)/sizeof(DOMDocuments[0]); // try to find the most recent DOMDocument available
	while (index--)
	{
		if (doc.CreateInstance(DOMDocuments[index], NULL, CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER) == S_OK)
			break;
	}
	if (doc) doc->async = VARIANT_FALSE;
	set(0, doc);
	var2var(0, 1);
}

NSIS_FUNC(load)
{
	get_doc();
	pop(xmlSource);
	doc->resolveExternals = VARIANT_FALSE;
	doc->validateOnParse  = VARIANT_FALSE;
	if (doc->load(xmlSource) == 0)
		settvar(0, _T("0"));
}

NSIS_FUNC(loadAndValidate)
{
	get_doc();
	pop(xmlSource);
	if (doc->load(xmlSource) == 0)
		settvar(0, _T("0"));
}

NSIS_FUNC(save)
{
	get_doc(); 
	pop(destination);
	doc->save(destination);
}

NSIS_FUNC(createProcessingInstruction)
{
	get_doc(); 
	pop(target);
	pop(data);
	set_child(doc->createProcessingInstruction(target, data));
}

NSIS_FUNC(createElement)
{
	get_doc(); 
	pop(tagName);
	set_child(doc->createElement(tagName));
}

NSIS_FUNC(createElementInNS)
{
	get_doc(); 
	pop(tagName);
	pop(namespaceURI)
	set_child(doc->createNode(long(NODE_ELEMENT), tagName, namespaceURI));
}

NSIS_FUNC(setDocumentElement)
{
	get_doc(); 
	get_child(IXMLDOMElement);
	doc->documentElement = child;
}

NSIS_FUNC(getAttribute)
{
	get_child(IXMLDOMElement); 
	pop(name); 
	_variant_t result = child->getAttribute(name);
	setwvar(3, (result.vt == VT_NULL) ? L"" : result.bstrVal);
}

NSIS_FUNC(setAttribute)
{
	get_child(IXMLDOMElement); 
	pop(name);
	pop(value);
	child->setAttribute(name, value);
}

NSIS_FUNC(setText)
{
	get_child(IXMLDOMNode); 
	pop(text);
	child->text = text;
}

NSIS_FUNC(getText)
{
	get_child(IXMLDOMNode); 
	setwvar(3, child->text);
}

NSIS_FUNC(select)
{
	get_doc();
	pop(queryString);
	try
	{
		set(1, doc->selectSingleNode(queryString));
	}
	catch (_com_error)
	{
		settvar(1, _T("0"));
	}
	var2var(1, 2);
}

NSIS_FUNC(parentNode)
{
	get_child(IXMLDOMNode);
	set_parent(child->parentNode);
}

NSIS_FUNC(appendChild)
{
	get_parent(); 
	get_child(IXMLDOMNode);
	parent->appendChild(child);
}

NSIS_FUNC(insertBefore)
{
	get_parent(); 
	get_child(IXMLDOMNode);
	get_child2(IXMLDOMNode);
	parent->insertBefore(child, _variant_t(child2, true));
}

NSIS_FUNC(removeChild)
{
	get_parent(); 
	get_child(IXMLDOMNode);
	parent->removeChild(child);
}

NSIS_FUNC(release)
{
	LPUNKNOWN node = LPUNKNOWN(pop_int(stacktop));
	node->Release();
}

extern "C" BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID)
{
	g_hInstance = hInstance;
	// we can call CoInitialize here because DLL is not loaded at application
	// startup, but dynamically by Installer
	CoInitialize(NULL); 
	return TRUE;
}

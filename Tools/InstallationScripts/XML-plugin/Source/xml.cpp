/*****************************************************************
 *                    XML NSIS plugin v2.0                       *
 *                                                               *
 * 2008 Shengalts Aleksander aka Instructor (Shengalts@mail.ru)  *
 *****************************************************************/

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include "StackFunc.h"
#include "tinyxml.h"
#include "xpath_static.h"


/* Defines */
#define NSIS_MAX_STRLEN 1024

/* Include private stack functions */
#define StackInsert
#define StackDelete
#define StackClear
#include "StackFunc.h"

typedef struct _xml_stack {
  struct _xml_stack *next;
  struct _xml_stack *prev;
  TiXmlNode *st_node;
} xml_stack;

/* ExDll */
typedef struct _stack_t {
  struct _stack_t *next;
  char text[1];
} stack_t;

stack_t **g_stacktop;
char *g_variables;
unsigned int g_stringsize;

#define EXDLL_INIT()        \
{                           \
  g_stacktop=stacktop;      \
  g_variables=variables;    \
  g_stringsize=string_size; \
}

/* Global variables */
char szBuf[NSIS_MAX_STRLEN];
char szBuf2[NSIS_MAX_STRLEN];
int nGoto=0;
int nElement=1;
TiXmlDocument doc;
TiXmlNode *node=&doc;
TiXmlNode *nodeF=0;
TiXmlNode *nodeF2=0;
TiXmlNode *nodeTmp=0;
TiXmlNode *nodeTmp2=0;
TiXmlText *text=0;
TiXmlElement *element=0;
TiXmlAttribute *attribute=0;
TiXmlAttribute *attributeTmp=0;
TiXmlDeclaration *declaration=0;
TiXmlEncoding TIXML_ENCODING=TIXML_DEFAULT_ENCODING;
xml_stack *pStackElement=NULL;
xml_stack *pStackFirst=NULL;
xml_stack *pStackTop=NULL;

/* Funtions prototypes and macros */
int popstring(char *str, int len);
void pushstring(const char *str, int len);

/* NSIS functions code */
extern "C" void __declspec(dllexport) _SetCondenseWhiteSpace(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if (!lstrcmp(szBuf, "0"))
      doc.SetCondenseWhiteSpace(false);
    else
      doc.SetCondenseWhiteSpace(true);
  }
}

extern "C" void __declspec(dllexport) _SetEncoding(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if (!lstrcmpi(szBuf, "UTF8")) TIXML_ENCODING=TIXML_ENCODING_UTF8;
    else if (!lstrcmpi(szBuf, "LEGACY")) TIXML_ENCODING=TIXML_ENCODING_LEGACY;
  }
}

extern "C" void __declspec(dllexport) _LoadFile(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if (doc.LoadFile(szBuf, TIXML_ENCODING))
      pushstring("0", NSIS_MAX_STRLEN);
    else
      pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _SaveFile(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if ((!*szBuf && doc.SaveFile()) || (*szBuf && doc.SaveFile(szBuf)))
      pushstring("0", NSIS_MAX_STRLEN);
    else
      pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _DeclarationVersion(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    nodeTmp=&doc;

    if ((nodeTmp=nodeTmp->FirstChild()) && (declaration=nodeTmp->ToDeclaration()))
    {
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(declaration->Version(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _DeclarationEncoding(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    nodeTmp=&doc;

    if ((nodeTmp=nodeTmp->FirstChild()) && (declaration=nodeTmp->ToDeclaration()))
    {
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(declaration->Encoding(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _DeclarationStandalone(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    nodeTmp=&doc;

    if ((nodeTmp=nodeTmp->FirstChild()) && (declaration=nodeTmp->ToDeclaration()))
    {
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(declaration->Standalone(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _GetText(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    if (node->ToElement())
    {
      if ((nodeTmp=node->FirstChild()) && nodeTmp->ToText())
      {
        pushstring("0", NSIS_MAX_STRLEN);
        pushstring(nodeTmp->Value(), NSIS_MAX_STRLEN);
        return;
      }
    }
    pushstring("-1", NSIS_MAX_STRLEN);
    pushstring("", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _SetText(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if (node->ToElement())
    {
       if (nodeTmp=node->FirstChild())
      {
        if (nodeTmp->ToText())
          nodeTmp->SetValue(szBuf);
        else
        {
          text=new TiXmlText(szBuf);
          node->InsertBeforeChild(nodeTmp, *text);
        }
      }
      else
      {
        text=new TiXmlText(szBuf);
        node->InsertEndChild(*text);
      }
      pushstring("0", NSIS_MAX_STRLEN);
      return;
    }
    pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _SetCDATA(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if (text=node->ToText())
    {
      if (!lstrcmp(szBuf, "1"))
        text->SetCDATA(true);
      else
        text->SetCDATA(false);
      pushstring("0", NSIS_MAX_STRLEN);
    }
    else
      pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _IsCDATA(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    if (text=node->ToText())
    {
      if (text->CDATA() == TRUE)
        pushstring("1", NSIS_MAX_STRLEN);
      else
        pushstring("0", NSIS_MAX_STRLEN);
    }
    else
      pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _GetNodeValue(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    pushstring(node->Value(), NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _SetNodeValue(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    node->SetValue(szBuf);
  }
}

extern "C" void __declspec(dllexport) _FindNextElement(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    int i;

    popstring(szBuf, NSIS_MAX_STRLEN);

    if (nGoto == 1) goto NextSiblingElement;
    else if (nGoto != 0) goto End;

    nGoto=1;
    StackInsert((stack **)&pStackFirst, (stack **)&pStackTop, (stack **)&pStackElement, -1, sizeof(xml_stack));
    pStackElement->st_node=node;

    while (nElement != 0)
    {
      --nElement;
      nodeF=pStackTop->st_node;
      StackDelete((stack **)&pStackFirst, (stack **)&pStackTop, (stack *)pStackTop);

      if ((!*szBuf && (nodeF2=nodeF->FirstChildElement())) || (*szBuf && (nodeF2=nodeF->FirstChildElement(szBuf))))
      {
        Return:
        node=nodeF2;
        pushstring("0", NSIS_MAX_STRLEN);
        pushstring(node->Value(), NSIS_MAX_STRLEN);
        return;

        NextSiblingElement:
        if ((!*szBuf && (nodeF2=nodeF2->NextSiblingElement())) || (*szBuf && (nodeF2=nodeF2->NextSiblingElement(szBuf))))
          goto Return;
      }
      if (nodeF2=nodeF->FirstChildElement())
      {
        i=0;

        do
        {
          ++i;
          ++nElement;
          StackInsert((stack **)&pStackFirst, (stack **)&pStackTop, (stack **)&pStackElement, i, sizeof(xml_stack));
          pStackElement->st_node=nodeF2;
        }
        while (nodeF2=nodeF2->NextSiblingElement());
      }
    }
    nGoto=-1;

    End:
    pushstring("-1", NSIS_MAX_STRLEN);
    pushstring("", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _FindCloseElement(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  nGoto=0;
  StackClear((stack **)&pStackFirst, (stack **)&pStackTop);
}

extern "C" void __declspec(dllexport) _RootElement(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    if (nodeTmp=doc.RootElement())
    {
      node=nodeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(node->Value(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _FirstChildElement(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if ((!*szBuf && (nodeTmp=node->FirstChildElement())) || (*szBuf && (nodeTmp=node->FirstChildElement(szBuf))))
    {
      node=nodeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(node->Value(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _FirstChild(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if ((!*szBuf && (nodeTmp=node->FirstChild())) || (*szBuf && (nodeTmp=node->FirstChild(szBuf))))
    {
      node=nodeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(node->Value(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _LastChild(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if ((!*szBuf && (nodeTmp=node->LastChild())) || (*szBuf && (nodeTmp=node->LastChild(szBuf))))
    {
      node=nodeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(node->Value(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _Parent(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    if (nodeTmp=node->Parent())
    {
      node=nodeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(node->Value(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _NoChildren(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    if (node->NoChildren() == TRUE)
      pushstring("1", NSIS_MAX_STRLEN);
    else
      pushstring("0", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _NextSiblingElement(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if ((!*szBuf && (nodeTmp=node->NextSiblingElement())) || (*szBuf && (nodeTmp=node->NextSiblingElement(szBuf))))
    {
      node=nodeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(node->Value(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _NextSibling(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if ((!*szBuf && (nodeTmp=node->NextSibling())) || (*szBuf && (nodeTmp=node->NextSibling(szBuf))))
    {
      node=nodeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(node->Value(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _PreviousSibling(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if ((!*szBuf && (nodeTmp=node->PreviousSibling())) || (*szBuf && (nodeTmp=node->PreviousSibling(szBuf))))
    {
      node=nodeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(node->Value(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _InsertAfterNode(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if ((nodeTmp2=(TiXmlNode *)atoi(szBuf)) && (nodeTmp=node->Parent()) && (nodeTmp=nodeTmp->InsertAfterChild(node, *nodeTmp2)))
    {
      node=nodeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
    }
    else
      pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _InsertBeforeNode(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if ((nodeTmp2=(TiXmlNode *)atoi(szBuf)) && (nodeTmp=node->Parent()) && (nodeTmp=nodeTmp->InsertBeforeChild(node, *nodeTmp2)))
    {
      node=nodeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
    }
    else
      pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _InsertEndChild(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if (node->ToElement() && (nodeTmp2=(TiXmlNode *)atoi(szBuf)) && (nodeTmp=node->InsertEndChild(*nodeTmp2)))
    {
      node=nodeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
    }
    else
      pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _ReplaceNode(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if ((nodeTmp2=(TiXmlNode *)atoi(szBuf)) && (nodeTmp=node->Parent()) && (nodeTmp->ReplaceChild(node, *nodeTmp2)))
      pushstring("0", NSIS_MAX_STRLEN);
    else
      pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _RemoveNode(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    if ((nodeTmp=node->Parent()) && nodeTmp->RemoveChild(node))
    {
      node=nodeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
    }
    else
      pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _RemoveAllChild(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    node->Clear();
  }
}

extern "C" void __declspec(dllexport) _CreateText(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if (text=new TiXmlText(szBuf))
    {
      _itoa((int)text, szBuf, 10);
      pushstring(szBuf, NSIS_MAX_STRLEN);
    }
    else
      pushstring("0", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _CreateNode(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    TiXmlElement element("");

    popstring(szBuf, NSIS_MAX_STRLEN);

    if (element.Parse(szBuf, 0, TIXML_ENCODING))
    {
      _itoa((int)element.Clone(), szBuf, 10);
      pushstring(szBuf, NSIS_MAX_STRLEN);
    }
    else
      pushstring("0", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _CloneNode(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    _itoa((int)node->Clone(), szBuf, 10);

    pushstring(szBuf, NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _FreeNode(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if (nodeTmp=(TiXmlNode *)atoi(szBuf))
    {
      nodeTmp->~TiXmlNode();
      nodeTmp=0;
      pushstring("0", NSIS_MAX_STRLEN);
    }
    else
      pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _NodeHandle(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    _itoa((int)node, szBuf, 10);

    pushstring(szBuf, NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _GotoHandle(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if (nodeTmp=(TiXmlNode *)atoi(szBuf))
    {
      node=nodeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
    }
    else
      pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _ElementPath(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    int i;
    szBuf[0]='\0';
    nodeTmp=node;

    if (nodeTmp != &doc && !nodeTmp->ToElement())
      nodeTmp=nodeTmp->Parent();

    if (nodeTmp != &doc)
    {
      do
      {
        nodeTmp2=nodeTmp;
        i=1;

        while (nodeTmp2=nodeTmp2->PreviousSibling(nodeTmp->Value()))
          if (nodeTmp2->ToElement()) ++i;

        if (i != 1)
          sprintf(szBuf2, "/%s[%d]%s", nodeTmp->Value(), i, szBuf);
        else
          sprintf(szBuf2, "/%s%s", nodeTmp->Value(), szBuf);

        lstrcpy(szBuf, szBuf2);
      }
      while ((nodeTmp=nodeTmp->Parent()) && nodeTmp != &doc);
    }
    pushstring(szBuf, NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _GotoPath(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    char *pBuf=&szBuf[0];
    char *pBuf2=&szBuf[0];
    int i;
    BOOL bExit=FALSE;
    nodeTmp=node;

    popstring(szBuf, NSIS_MAX_STRLEN);

    if (*pBuf2 == '\0')
    {
      nodeTmp=&doc;
      bExit=TRUE;
    }
    else if (*pBuf2 == '/')
      nodeTmp=&doc;
    else if (nodeTmp != &doc && !nodeTmp->ToElement())
      goto Error;
    else --pBuf2;

    while (nodeTmp && bExit == FALSE)
    {
      i=1;
      pBuf=++pBuf2;

      while (*pBuf2 != '/' && *pBuf2 != '\0')
      {
        if (*pBuf2 == '[')
        {
          *pBuf2='\0';
          i=atoi(++pBuf2);

          if (i == 0) goto Error;
        }
        else ++pBuf2;
      }

      if (*pBuf2 == '/') *pBuf2='\0';
      else bExit=TRUE;

      if (i > 0)
      {
        if ((!*pBuf && (nodeTmp=nodeTmp->FirstChildElement())) || (*pBuf && (nodeTmp=nodeTmp->FirstChildElement(pBuf))))
        {
          do
          {
            --i;
          }
          while (i != 0 && ((!*pBuf && (nodeTmp=nodeTmp->NextSiblingElement())) || (*pBuf && (nodeTmp=nodeTmp->NextSiblingElement(pBuf)))));
        }
      }
      else
      {
        if ((!*pBuf && (nodeTmp=nodeTmp->LastChild())) || (*pBuf && (nodeTmp=nodeTmp->LastChild(pBuf))))
        {
          do
          {
            if (nodeTmp->ToElement()) ++i;
          }
          while (i != 0 && ((!*pBuf && (nodeTmp=nodeTmp->PreviousSibling())) || (*pBuf && (nodeTmp=nodeTmp->PreviousSibling(pBuf)))));
        }
      }
    }

    if (nodeTmp && bExit == TRUE)
    {
      node=nodeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
      return;
    }

    Error:
    pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _XPathString(HWND hwndParent, int string_size,
                       char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    TIXML_STRING S_res;

    popstring(szBuf, NSIS_MAX_STRLEN);

    if (TinyXPath::o_xpath_string(node, szBuf, S_res))
    {
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(S_res.c_str(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _XPathNode(HWND hwndParent, int string_size,
                       char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if (TinyXPath::o_xpath_node(node, szBuf, nodeTmp))
    {
      node=nodeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
    }
    else
      pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _XPathAttribute(HWND hwndParent, int string_size,
                       char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if (TinyXPath::o_xpath_attribute(node, szBuf, attributeTmp))
    {
      attribute=attributeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
    }
    else
      pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _NodeType(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    int nType;

    nType=node->Type();
    if (nType == 1) pushstring("ELEMENT", NSIS_MAX_STRLEN);
    else if (nType == 2) pushstring("COMMENT", NSIS_MAX_STRLEN);
    else if (nType == 3) pushstring("DOCUMENT", NSIS_MAX_STRLEN);
    else if (nType == 4) pushstring("TEXT", NSIS_MAX_STRLEN);
    else if (nType == 5) pushstring("DECLARATION", NSIS_MAX_STRLEN);
    else pushstring("UNKNOWN", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _Coordinate(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    _itoa(node->Row(), szBuf, 10);
    _itoa(node->Column(), szBuf2, 10);

    pushstring(szBuf2, NSIS_MAX_STRLEN);
    pushstring(szBuf, NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _GetAttribute(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if ((element=node->ToElement()) && (attributeTmp=element->FirstAttribute()))
    {
      do
      {
        if (!lstrcmp(szBuf, attributeTmp->Name()))
        {
          attribute=attributeTmp;
          pushstring("0", NSIS_MAX_STRLEN);
          pushstring(attribute->Value(), NSIS_MAX_STRLEN);
          return;
        }
      }
      while (attributeTmp=attributeTmp->Next());
    }
    pushstring("-1", NSIS_MAX_STRLEN);
    pushstring("", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _SetAttribute(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);
    popstring(szBuf2, NSIS_MAX_STRLEN);

    if (element=node->ToElement())
    {
      element->SetAttribute(szBuf, szBuf2);
      pushstring("0", NSIS_MAX_STRLEN);
    }
    else
      pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _RemoveAttribute(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    if (element=node->ToElement())
    {
      element->RemoveAttribute(szBuf);
      pushstring("0", NSIS_MAX_STRLEN);
    }
    else
      pushstring("-1", NSIS_MAX_STRLEN);
  }
}

extern "C" void __declspec(dllexport) _FirstAttribute(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    if ((element=node->ToElement()) && (attributeTmp=element->FirstAttribute()))
    {
      attribute=attributeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(attribute->Value(), NSIS_MAX_STRLEN);
      pushstring(attribute->Name(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _LastAttribute(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    if ((element=node->ToElement()) && (attributeTmp=element->LastAttribute()))
    {
      attribute=attributeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(attribute->Value(), NSIS_MAX_STRLEN);
      pushstring(attribute->Name(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _NextAttribute(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    if (attribute && (attributeTmp=attribute->Next()))
    {
      attribute=attributeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(attribute->Value(), NSIS_MAX_STRLEN);
      pushstring(attribute->Name(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _PreviousAttribute(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    if (attribute && (attributeTmp=attribute->Previous()))
    {
      attribute=attributeTmp;
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(attribute->Value(), NSIS_MAX_STRLEN);
      pushstring(attribute->Name(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _CurrentAttribute(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    if (attribute)
    {
      pushstring("0", NSIS_MAX_STRLEN);
      pushstring(attribute->Value(), NSIS_MAX_STRLEN);
      pushstring(attribute->Name(), NSIS_MAX_STRLEN);
    }
    else
    {
      pushstring("-1", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
      pushstring("", NSIS_MAX_STRLEN);
    }
  }
}

extern "C" void __declspec(dllexport) _SetAttributeName(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    attribute->SetName(szBuf);
  }
}

extern "C" void __declspec(dllexport) _SetAttributeValue(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
  EXDLL_INIT();
  {
    popstring(szBuf, NSIS_MAX_STRLEN);

    attribute->SetValue(szBuf);
  }
}

extern "C" void __declspec(dllexport) _Unload(HWND hwndParent, int string_size,
                                      char *variables, stack_t **stacktop)
{
}

BOOL WINAPI DllMain(HANDLE hInst, ULONG ul_reason_for_call, LPVOID lpReserved)
{
  return TRUE;
}


/* Functions */
//Function: Removes the element from the top of the NSIS stack and puts it in the buffer
int popstring(char *str, int len)
{
  stack_t *th;

  if (!g_stacktop || !*g_stacktop) return 1;
  th=(*g_stacktop);
  lstrcpyn(str, th->text, len);
  *g_stacktop=th->next;
  GlobalFree((HGLOBAL)th);
  return 0;
}

//Function: Adds an element to the top of the NSIS stack
void pushstring(const char *str, int len)
{
  stack_t *th;

  if (!g_stacktop) return;
  th=(stack_t*)GlobalAlloc(GPTR, sizeof(stack_t) + len);
  lstrcpyn(th->text, str, len);
  th->next=*g_stacktop;
  *g_stacktop=th;
}

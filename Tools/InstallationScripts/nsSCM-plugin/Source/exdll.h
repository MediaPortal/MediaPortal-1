/*********************************************************************************************************
 *
 *  Module Name:	exdll.h
 *
 *  Abstract:		NSIS plugin common header
 *
 *  Author:		
 *
 *  Modified:		Vyacheslav I. Levtchenko (mail-to: sl@r-tt.com, sl@eltrast.ru)
 *
 *  Revision History:	20.10.2003	started
 *
 *  Classes, methods and structures:
 *
 *  TODO:
 *
 *********************************************************************************************************/

#ifndef _EXDLL_H_
#define _EXDLL_H_

// only include this file from one place in your DLL.
// (it is all static, if you use it in two places it will fail)

#define EXDLL_INIT()		\
{				\
  g_stringsize=string_size;	\
  g_stacktop=stacktop;		\
  g_variables=variables;	\
  brk (); }

// For page showing plug-ins
#define WM_NOTIFY_OUTER_NEXT	(WM_USER+0x8)
#define WM_NOTIFY_CUSTOM_READY	(WM_USER+0xd)
#define NOTIFY_BYE_BYE 'x'

#define NSISFunction(funcname) extern "C" void __declspec(dllexport) funcname(HWND hwndParent, int string_size, char *variables, stack_t **stacktop)

#define MALLOC(len)	GlobalAlloc(GPTR, len);
#define FREE(mem)	{ if (mem) GlobalFree (mem), mem = NULL; }

#define STRNEW() (char*)MALLOC((sizeof(char) * g_stringsize) + 1);
#define STRDEL(mem)     FREE (mem);

#define RET_ERROR()	{ clear_stack (); pushstring("error");   return; }
#define RET_SUCCESS()	{ clear_stack (); pushstring("success"); return; }
#define RET(rc)	if (!rc) { RET_ERROR(); } else { RET_SUCCESS(); }

#define RET_DWORD(rc, dd)	\
{ clear_stack ();		\
  pushdword (dd);		\
  if (!rc) pushstring("error");	\
  else     pushstring("success");\
  return; }

#define strcpy(x,y)	lstrcpy(x,y)
#define strncpy(x,y,z)	lstrcpyn(x,y,z)
#define strdup(x)	STRDUP(x);
#define stricmp(x,y)	lstrcmpi(x,y)

enum
{
  INST_0,	// $0
  INST_1,	// $1
  INST_2,	// $2
  INST_3,	// $3
  INST_4,	// $4
  INST_5,	// $5
  INST_6,	// $6
  INST_7,	// $7
  INST_8,	// $8
  INST_9,	// $9
  INST_R0,	// $R0
  INST_R1,	// $R1
  INST_R2,	// $R2
  INST_R3,	// $R3
  INST_R4,	// $R4
  INST_R5,	// $R5
  INST_R6,	// $R6
  INST_R7,	// $R7
  INST_R8,	// $R8
  INST_R9,	// $R9
  INST_CMDLINE,	// $CMDLINE
  INST_INSTDIR,	// $INSTDIR
  INST_OUTDIR,	// $OUTDIR
  INST_EXEDIR,	// $EXEDIR
  INST_LANG,	// $LANGUAGE
  __INST_LAST
};

typedef struct _stack_t
{
  struct _stack_t *next;
  char text[1]; // this should be the length of string_size
} stack_t;

static unsigned int g_stringsize = 0;

/* Call stack (LIFO) variables _cdecl sequence (right-left) */
static stack_t	  **g_stacktop = NULL;

/* Global variables array */
static char	   *g_variables = NULL;

void *operator new (unsigned int num_bytes) { return GlobalAlloc (GPTR, num_bytes); }
void  operator delete (void *p) { if (p) GlobalFree(p), p = NULL; }

// utility functions (not required but often useful)
// 0 on success, 1 on empty stack
static int popstring (char *str)
{
  stack_t *th;

  if (!g_stacktop
   || !*g_stacktop)
    return 1;

  th = *g_stacktop;

  lstrcpy (str, th->text);
  *g_stacktop = th->next;
  GlobalFree ((HGLOBAL)th);

  return 0;
}

static void clear_stack (void)
{
  stack_t *th;

  while (g_stacktop
     && *g_stacktop)
   {
    th = *g_stacktop;
    *g_stacktop = th->next;
    GlobalFree ((HGLOBAL)th);
   }
}

static void pushstring (const char *str)
{
  stack_t *th;

  if (!g_stacktop)
    return;

  th = (stack_t*) GlobalAlloc (GPTR, sizeof(stack_t) + g_stringsize);

  lstrcpyn (th->text, str, g_stringsize);
  th->next = *g_stacktop;
  *g_stacktop = th;
}

static void pushdword (DWORD dd)
{
  stack_t *th;

  if (!g_stacktop)
    return;

  th = (stack_t*) GlobalAlloc (GPTR, sizeof(stack_t) + g_stringsize);

  wsprintf (th->text, "%d", dd);
  th->next = *g_stacktop;
  *g_stacktop = th;
}

static char *getuservariable (int varnum)
{
  if (varnum < 0 
   || varnum >= __INST_LAST)
    return NULL;

  return g_variables + varnum * g_stringsize;
}

static void setuservariable(int varnum, char *var)
{
  if (var != NULL
   && varnum >= 0
   && varnum < __INST_LAST)
    lstrcpy (g_variables + varnum * g_stringsize, var);
}

static char *ns_strstr (const char *string, const char *strCharSet)
{
  size_t chklen, i;
  char *s1, *s2;

  if (lstrlen (string) < lstrlen (strCharSet))
    return 0;

  if (!*strCharSet)
    return (char*)string;

  chklen = lstrlen(string) - lstrlen(strCharSet);

  for (i = 0; i <= chklen; i++)
   {
    s1 = &((char*)string)[i];
    s2 =   (char*)strCharSet;

    while (*s1++ == *s2++)
      if (!*s2)
	return &((char*)string)[i];
   }

  return 0;
}

static unsigned int ns_atoi (char *s)
{
  unsigned int v = 0;

  if (*s == '0' && (s[1] == 'x' || s[1] == 'X'))
   {
    s+=2;

    for (;;)
     {
      int c=*s++;

      if (c >= '0'
       && c <= '9')
	c-='0';
      else
      if (c >= 'a'
       && c <= 'f')
	c-='a'-10;
      else
      if (c >= 'A'
       && c <= 'F')
	c-='A'-10;
      else
	break;

      v <<= 4;
      v+=c;
     }
   }
  else
  if (*s == '0' 
    && s[1] <= '7' 
    && s[1] >= '0')
   {
    s++;

    for (;;)
     {
      int c=*s++;

      if (c >= '0'
       && c <= '7')
	c-='0';
      else
	break;

      v<<=3;
      v+=c;
     }
   }
  else
   {
    for (;;)
     {
      int c=*s++ - '0';

      if (c < 0 
       || c > 9)
	break;

      v*=10;
      v+=c;
     }
   }

  return (int)v;
}

static char *STRDUP (const char *c)
{
  char *t = (char*) MALLOC (strlen(c) + 1);
  strcpy (t,c);
  return t;
}

#endif //_EXDLL_H_

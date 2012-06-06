/* NSIS plug-in for getting a bit of CPU information.
		Version 1.2, July 2003.
		Typed and clicked by Peter Mason, CSIRO DEM MMTG.  mailto://peter.mason@csiro.au.
		The MHz timing was done using code that was pretty much copied from Pavlos Touboulidis' CPUTEST code.
		The lean exception handler wrapped around the timing code was done using Jeremy Gordon's tutorial on writing
		lightweight win32 exception handlers in assembler.   (His web-page is www.GoDevTool.com.)
		The CPUID stuff was done using Intel and AMD's manuals on what CPUID means in their respective microcosms.
		Best viewed with <TAB> == 2 spaces.

		There's only one routine here - tell() - and its output is a string on the NSIS stack.
		This string always has the same fields in exactly the same place.   (Easier to extract values that way.)   It looks like this:

			INTELP=d AMD=add PPRO=b MMX=d SSE=b SSE2=b 3DNOW=d ARCH=dd LEVEL=dd NCPU=dd MHZ=ddddd RAM=dddd

		Here, "d" means a decimal digit (0..9), "a" means an alphabetic character (A..Z) and "b" means a boolean digit (0 or 1).
		ITELP: Values range [0..4].
			0:		Not a genuine Intel CPU (or a very, VERY old one).
			1:		Pentium or Pentium with MMX.   (Check the MMX field if you want to know about the CPU's MMX support.)
			2:		Pentium Pro, II or Celeron.   (May or may not have MMX - PPros don't, the others do.   Check the MMX field.)
			3:		Pentium III or P3 (old) Xeon.   (Always has MMX and SSE.)
			4:		Pentium IV or (new) Xeon.   (Always has MMX, SSE and SSE2.)
		AMD: A bit more complicated...
			000:	Not an authentic AMD CPU (or a very old one).
			Kdd:	An old K-series.   "dd" is either 05 for a K5 or 06 for a K6.
						(Pentium compatible.   K5s have no MMX or 3DNOW.   K6s have standard MMX, and later models have basic 3DNOW.)
			Add:	An Athlon or a Duron.   "dd" is the model number (goes from 01 to 10).
						(Pentium II compatible.   All of these have extended MMX and extended 3DNOW.   None have any SSE.)
			Odd:	An opteron.   "dd" gives the model number.
						(Pentium IV compatible.   This CPU's got everything, it seems.)
		PPRO:		Values range [0..1].
		  0:    Not compatible with the Intel Pentium Pro processor.
		  1:    Compatible with the Intel Pentium Pro processor.
		MMX:		Values range [0..2].
			0:		No MMX support.
			1:		Standard Intel MMX support.
			2:		Standard MMX support plus AMD MMX extensions.
		SSE:		Values range [0..1].
			0:		No SSE support.
			1:		Supports SSE (Intel's Streaming SIMD extensions, P3-style).
		SSE2:		Values range [0..1].
			0:		No SSE2 support.
			1:		Supports SSE2 (Intel's Streaming SIMD extensions 2, P4-style).
		3DNOW:	Values range [0..2].
			0:		No 3DNOW support.
			1:		Standard AMD 3DNOW support.
			2:		Standard 3DNOW support plus AMD 3DNOW extensions.
		ARCH:		Values range [00..10].
			00:		32-bit Intel or compatible
			01:		MIPS (did NT 3.5, apparently)
			02:		DEC Alpha.   (Yes, DEC.   I can't bring myself to call it COMPAQ.)
			03:		PowerPC
			04:		SHX (?)
			05:		ARM (Acorn / Advanced Risc Machine, I presume.   I don't think anyone's going to see this running Windows?)
			06:		64-bit Intel.
			07:		64-bit Alpha
			08:		MSIL (?)
			09:   64-bit AMD
			10:		32-bit Intel doing Win64 (?)
		LEVEL:	"Processor level", like what you see in the main processor environment variable.   Sort-of useless, really.
		NCPU:		The number of processors available.   (Affected by that "Hyper" business that the new XEONs can do, I think.)
		MHZ:		The CPU's internal clock speed in MHz (approx).
		RAM:		The amount of RAM (physical memory) in megabytes (rounded).


Compilation:
/nologo /MT /W3 /vd0 /Og /Os /Oy /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /D "_MBCS" /D "_USRDLL" /D "EXDLL_EXPORTS" /Fo"Release/" /Fd"Release/" /Zl /FD /c
Linking:
kernel32.lib user32.lib advapi32.lib /nologo /entry:"_DllMainCRTStartup" /dll /incremental:no /pdb:"Release/cpudesc.pdb" /machine:I386 /nodefaultlib /out:"Release/cpudesc.dll" /implib:"Release/cpudesc.lib" /MERGE:.rdata=.text /MERGE:.text=.text /MERGE:.reloc=.text /OPT:REF /FILEALIGN:512

*/
#include <windows.h>
#include "../ExDLL/exdll.h"

/*****************************************/
// Gets the MHz timing stored by Windows in the registry.   Returns 0 MHz if there's a problem reading the expected registry value.
// This is used as a fall-back for when the timer routine can't be run.   I don't know if this registry value is stored consistently
// for different versions of Windows.
static int mhzfromreg(void)
{
	HKEY	k;
	DWORD	drv, ndrv=4;
	int		rv=0;
	if( ERROR_SUCCESS == RegOpenKeyEx( HKEY_LOCAL_MACHINE, "HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0", 0, KEY_READ, &k ) ) {
		if( ERROR_SUCCESS == RegQueryValueEx( k, "~MHz", 0, NULL, (LPBYTE)(&drv), &ndrv ) )  rv=(int)drv;
		RegCloseKey(k);
	}
	return rv;
}
/*****************************************/
// This lot pretty much lifted from Pavlos Touboulidis' CPUTEST code.
// (Note _cdecl in case this isn't the default compilation setting.   The assembly calling routine expects this protocol.)
static void _cdecl delay(int foroverhead)
{
	LARGE_INTEGER	c1, c2;
	__int64				x, y;
	if( !QueryPerformanceFrequency(&c1) ) return;
	//x=c1.QuadPart>>4;																					//hard coded for 62.5ms interval
	__asm {																											//avoid bringing in the CRTL for dividing 64-bit c1 by 16 (to get an interval of 62.5 ms)
		push	eax
		mov		eax,c1.HighPart
		shrd	c1.LowPart,eax,4
		shr		eax,4																								//yes, the SHRD above won't have changed EAX.
		mov		c1.HighPart,eax
		pop		eax
	}
	x=c1.QuadPart;
	QueryPerformanceCounter(&c1);
	do {
		QueryPerformanceCounter(&c2);
		y=c2.QuadPart-c1.QuadPart;
	} while( foroverhead ? (y==x) : (y<x) );
	return;
}
/**************/
/*	Unfortunately, there's no decent way (in non-privileged mode) to tell if the CPU is allowed to execute the RDTSC
		instruction in non-privileged mode.   The relevant status bit is bit 2 in "CR4" (control register 4), and we can't
		even look at CR4 in user mode.
		So I have resorted to the indiscrete way of letting it f.. er, throw an exception if it must...
		In the event of an exception (due to disallowed use of RDTSC), the exception will get caught and the MHz value
		pulled out of the registry earlier will silently get returned instead of a timed value.   The exception handler
		is very simple-minded.   It doesn't check what sort of exception occurred, it just "handles" it by jumping to the
		end of the timer code.   (If it gets called, it's assumed that it was due to a disallowed call to RDTSC.)
		BTW, this code seems to produce a good answer on a dual CPU PC.
*/
static void mhzfromtimer(int *mhz)
{
	HANDLE		hproc=GetCurrentProcess(), hthr=GetCurrentThread();
	DWORD			oldpc=GetPriorityClass(hproc);									// old priority class
	DWORD			eax0, edx0, tmhz=*mhz;
	int				oldtp=GetThreadPriority(hthr);									// old thread priority
	//if( GetProcessAffinityMask( hproc, &pam, &sam ) && (pam>1) ) {
	//	tam=SetThreadAffinityMask( hthr, 1 );										//lock onto the primary CPU if 2+ CPUs
	//	Sleep(1);																								//maybe we weren't on the primary CPU?   Hopefully, we will be after this.   (I don't know if SetThreadAffinityMask() does the necessary.)
	//}
	SetPriorityClass( hproc, HIGH_PRIORITY_CLASS );						//that should be sufficient
	SetThreadPriority( hthr, THREAD_PRIORITY_TIME_CRITICAL );	//...and that
		__asm {
			pushad																								//just save all general registers
			push	offset term																			//"safe place" for exception handler's return (our own addition to the structure)
			push offset exh																				//exception handler's start (expected in structure)
			push	dword ptr fs:[0]																//becomes "next handler" (expected in structure)
			mov		fs:[0], esp																			//rig the top ERR structure to be for our handler (we've just built its structure here on the stack)
			RDTSC																									//(Start of the timing job - what we came here for)
			mov		esi,eax
			mov		edi,edx
			push	0
			call	delay																						//delay(0)
			pop		ecx
			RDTSC
			sub		eax,esi
			sbb		edx,edi
			mov		eax0,eax
			mov		edx0,edx																				//that's the main count in edx0:eax0
			RDTSC
			mov		esi,eax
			mov		edi,edx
			push	1
			call	delay																						//delay(1)
			pop		ecx
			RDTSC
			sub		eax,esi
			sbb		edx,edi
			sub		eax0,eax
			sbb		edx0,edx																				//that's the overhead count subtracted from edx0:eax0
			mov		eax,eax0
			mov		edx,edx0
			mov		ecx,62500																				//timed over 62.5 ms
			div		ecx
			mov		tmhz,eax																				//..and THERE'S OUR RESULT
term:	pop		dword ptr fs:[0]																//reinstate old top handler
			add		esp,8																						//(chuck away addresses pushed onto the stack for our handler)
			popad																									//restore all general registers
			jmp		fin
exh:	push	edi																							//Start of exception handler
			push	esi
			mov		edi, [esp+10h]																	//our ERR structure
			mov		esi, [esp+14h]																	//context structure
			mov		[esi+0C4h], edi																	//insert new esp into the context structure
			mov		eax, [edi+8]																		//address of safe place "term" that we stored in our ERR structure
			mov		[esi+0B8h], eax																	//insert new eip
			xor		eax, eax																				//return code 0 means we've handled it
			pop		esi
			pop		edi
			ret																										//End of exception handler
fin:
		}
	*mhz=tmhz;
	SetThreadPriority( hthr, oldtp );													//restore it
	SetPriorityClass( hproc, oldpc );													//...and it
	//if(tam) SetThreadAffinityMask( hthr, tam );								//..and it
	return;
}
/*****************************************/
void __declspec(dllexport) tell(HWND hwndParent, int string_size, char *variables, stack_t **stacktop)
{
	EXDLL_INIT();
	{
		char					ostr[256];
		SYSTEM_INFO		si;
		MEMORYSTATUS	ms;
		unsigned int	mhz=0, ram=0;
		char					arch=0, level=0, pprocompat=0, hasmmx=0, has3dnow=0, hassse=0, hassse2=0, intelpentium=0, amd=0, amdlet='0', ncpu=1, tsc=0;
		GlobalMemoryStatus(&ms);
		if( 0xfff90000 <= (ram=ms.dwTotalPhys) )  ram=4*1024;											//Just say it's 4GB!
		else ram = (ram+512*1024) / (1024*1024);																	//report megabytes (rounded)
		mhz=mhzfromreg();
		GetSystemInfo( &si );
		arch    =(char)si.wProcessorArchitecture;																	//type code (e.g., 0==intel)
		level   =(char)si.wProcessorLevel;																				//CPU "level"
		ncpu    =(char)si.dwNumberOfProcessors;
		if(!ncpu) ncpu=1;
		hasmmx  =IsProcessorFeaturePresent( PF_MMX_INSTRUCTIONS_AVAILABLE );			//just about everything these days?
		has3dnow=IsProcessorFeaturePresent( PF_3DNOW_INSTRUCTIONS_AVAILABLE );		//for some AMDs?
		hassse  =IsProcessorFeaturePresent( PF_XMMI_INSTRUCTIONS_AVAILABLE );			//Pentium III or better?
		hassse2 =IsProcessorFeaturePresent( PF_XMMI64_INSTRUCTIONS_AVAILABLE );		//umm?   (This doesn't seem to work for P4s)
		if( (0==arch) || (6==arch) || (7==arch) || (9==arch) || (10==arch) ) {		//Intel compatible architecture - let's get some better detail
			__asm {
				push	eax
				push	ebx
				push	ecx
				push	edx
				mov		ebx,200000h
				pushfd
				pop		eax
				mov		ecx,eax										;save the flags reg's original contents
				xor		eax,ebx										;try to change bit 21 in the flags register
				push	eax
				popfd														;(that's our attempt to change the flags reg)
				pushfd
				pop		eax												;now let's see if it really got changed
				xor		eax,ecx
				and		eax,ebx										;test just for a change of bit 21 (being a.r. here)
				jz		bye												;if our change didn't stick then we can't execute the CPUID instruction
				xor		eax,eax
				cpuid														;is it a real Intel or AMD CPU?
				cmp		ebx,756E6547h							;"Genu"
				jnz		ckamd
				cmp		edx,49656E69h							;"ineI"
				jnz		bye
				cmp		ecx,6C65746Eh							;"ntel"
				jnz		bye
				mov		byte ptr intelpentium, 1	;assume vanilla pentium for now
				jmp		cont1
ckamd:	cmp		ebx,68747541h							;"Auth"
				jnz		bye
				cmp		edx,69746E65h							;"enti"
				jnz		bye
				cmp		ecx,444D4163h							;"cAMD"
				jnz		bye
				mov		byte ptr amd, 5
				mov		byte ptr amdlet, 'K'			;assume a K5 for the time being
cont1:	xor		eax,eax
				mov		byte ptr hasmmx, al
				mov		byte ptr has3dnow, al
				mov		byte ptr hassse, al
				mov		byte ptr hassse2, al			;we'll be revising our opinions of mmx, 3dnow, sse and sse2
				inc		eax
				cpuid														;get some detail
				mov		cl,10h										;this bit is set if the CPU has the RDTSC instruction
				and		cl,dl
				jz		notsc
				;mov	ecx,CR4										;can't do it - it's a privileged instruction
				;and	cl,4
				;jnz		notsc
				inc		byte ptr tsc							;MAYBE - won't work if bit 2 of CR4 is set, but we could only check that if we were running at privilege level 0
notsc:	mov		ecx,800000h								;this bit is set if the CPU does MMX
				and		ecx,edx
				jz		nommx
				inc		byte ptr hasmmx
nommx:	xor		bl,bl
				mov		ecx,2000000h							;this bit is set if the CPU does SSE
				and		ecx,edx
				jz		nosse1
				inc		bl
				inc		byte ptr hassse						;P3-style SSE
nosse1:	mov		ecx,4000000h							;this bit is set if the CPU does SSE2
				and		ecx,edx
				jz		nosse2
				inc		bl
				inc		byte ptr hassse2					;P4-style SSE2
nosse2:	cmp		byte ptr amd, 0
				jnz		amd2											;further AMD-specific checks elsewhere
				shr		eax,8											;further Intel-specific cheks here...
				and		al,0Fh										;get the family number
				cmp		al,6h
				jb		bye
				inc		byte ptr intelpentium			;so far it looks like an old PPro, old Celeron, old P2 or better
				inc		byte ptr pprocompat
				cmp		bl,0
				jz		bye												;no SSE - it's an old thing and we're done
				inc		byte ptr intelpentium			;if it's got some SSE support then so far it looks like a P3 or better
				cmp		al,0Fh
				jnz		bye
				inc		byte ptr intelpentium			;you know, it looks like a P4
				jmp		bye
amd2:		mov		ecx,1000000h							;this bit is set if the CPU does FXSR
				and		ecx,edx										;dunno about Intel, but with AMD we should apparently check both bits 24 and 25 for SSE.
				jnz		yessse
				mov		byte ptr hassse, 0				;turn off SSE if the CPU doesn't do FXSR (in addition to SSE)
yessse:	mov		dl,0fh										;further AMD checks...
				mov		ebx,eax
				shr		eax,4
				and		al,dl
				cmp		al,dl
				jnz		noxm											;model number is complete as-is
				and		ah,0f0h										;(extended model number)
				add		al,ah											;full model number now in AL
noxm:		shr		ebx,8
				and		bl,dl
				cmp		bl,dl
				jnz		noxf											;family number is complete as-is
				mov		edx,ebx
				shr		edx,12
				and		dl,0ffh										;(extended family number)
				add		bl,dl											;full family number now in bl
noxf:		cmp		bl,5
				ja		amda											;looks like an athlon or better
				cmp		al,6
				jb		bye												;looks like a k5 - we're done
				mov		byte ptr amd, 6						;looks like a k6
				jmp		amd4
amda:		inc		byte ptr pprocompat				;athlons / durons are Pentium II compatible
				cmp		bl,14
				ja		amdo											;looks like an opteron
				mov		byte ptr amdlet, 'A'			;looks like an athlon / duron
				jmp		amd3
amdo:		mov		byte ptr amdlet, 'O'			;(looks like an opteron)
amd3:		mov		byte ptr amd, al					;report the model number for athlons / durons / opterons
amd4:		mov		eax,80000001h
				cpuid														;get some AMD specifics
				mov		ecx,40000h
				and		ecx,edx
				jz		amd5
				inc		byte ptr hasmmx						;it's got AMD MMX extensions
amd5:		mov		ecx,8000000h
				mov		ebx,ecx
				and		ecx,edx
				jz		bye
				inc		byte ptr has3dnow					;it's got basic 3DNOW
				shr		ebx,1
				and		ebx,edx
				jz		bye
				inc		byte ptr has3dnow					;it's got extended 3DNOW
bye:		pop		edx
				pop		ecx
				pop		ebx
				pop		eax
			}
			if(tsc) mhzfromtimer(&mhz);														//get CPU MHz via timing if it appears that we can execute the RDTSC instruction
		}
		wsprintf( ostr, "INTELP=%1d AMD=%c%2.2d PPRO=%1d MMX=%1d SSE=%1d SSE2=%1d 3DNOW=%1d ARCH=%2.2d LEVEL=%2.2d NCPU=%2.2d MHZ=%5.5d RAM=%4.4d",
			intelpentium, amdlet, amd, pprocompat, hasmmx, hassse, hassse2, has3dnow, arch, level, ncpu, mhz, ram );
		pushstring(ostr);
	}
	return;
}
/*****************************************/
BOOL WINAPI _DllMainCRTStartup(HANDLE hInst, ULONG ul_reason_for_call, LPVOID lpReserved)
{
	hInst=hInst;  ul_reason_for_call=ul_reason_for_call;  lpReserved=lpReserved;
	return TRUE;
}
/*****************************************/

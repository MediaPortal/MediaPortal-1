#include <windows.h>
#include "utility.h"

CCpuID g_cpuid;

CCpuID::CCpuID()
{
	DWORD flags = 0;

	__asm
	{
		mov			eax, 1
		cpuid
		test		edx, 0x00800000		// STD MMX
		jz			TEST_SSE
		or			[flags], 1
TEST_SSE:
		test		edx, 0x02000000		// STD SSE
		jz			TEST_SSE2
		or			[flags], 2
		or			[flags], 4
TEST_SSE2:
		test		edx, 0x04000000		// SSE2	
		jz			TEST_3DNOW
		or			[flags], 8
TEST_3DNOW:
		mov			eax, 0x80000001
		cpuid
		test		edx, 0x80000000		// 3D NOW
		jz			TEST_SSEMMX
		or			[flags], 16
TEST_SSEMMX:
		test		edx, 0x00400000		// SSE MMX
		jz			TEST_END
		or			[flags], 2
TEST_END:
	}

	m_flags = (flag_t)flags;
}



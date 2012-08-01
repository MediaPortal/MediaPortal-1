#pragma once

class CCpuID {public: CCpuID(); enum flag_t {mmx=1, ssemmx=2, ssefpu=4, sse2=8, _3dnow=16} m_flags;};
extern CCpuID g_cpuid;

#define countof(array) (sizeof(array)/sizeof(array[0]))
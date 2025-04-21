#define Mode 1

/* blurGauss_pass2: Gaussian blur (Vertical Pass)
Second pass of 2-pass shader, please ensure the 2 passes use the same parameters/Mode. 

by butterw, License: GPL v3
*/

sampler s0: register(s0);
float2  p1: register(c1);

#if Mode==1 //gBlur7, http://dev.theomader.com/gaussian-kernel-calculator/	
	//#define K	float4(0.324225, 0.233638, 0.087348, 0.016901) //sigma=1.2
	#define K	float4(0.266346, 0.215007, 0.113085, 0.038735) //sigma=1.5 (default) 
	//#define K	float4(0.230781, 0.198557, 0.126451, 0.059602) //sigma=1.8
#elif Mode==2
	#define Offsets float2(0.0, 4/3.) //(3 texture, 6 arithmetic)
	#define K 		float2(0.29411764705882354, 0.35294117647058826)
#elif Mode==3	
	#define Offsets float3(0.0, 1.3846153846, 3.2307692308)
	#define K		float3(0.2270270270, 0.3162162162, 0.0702702703)
#elif Mode==4
	#define Offsets float4(0.0, 1.411764705882353, 3.2941176470588234, 5.176470588235294)
	#define K		float4(0.1964825501511404, 0.2969069646728344, 0.09447039785044732, 0.010381362401148057)
#endif

/* --- Main --- */
float4 main(float2 tex: TEXCOORD0): COLOR { //(7 texture, 11 arithmetic)
	float4 c0 = K[0] *tex2D(s0, tex);
#if Mode==1	//(7 texture, 11 arithmetic)
    c0+= K[1] *tex2D(s0, tex +float2(0, p1.y));
    c0+= K[1] *tex2D(s0, tex +float2(0, -p1.y));
    c0+= K[2] *tex2D(s0, tex +p1*float2(0, 2));
    c0+= K[2] *tex2D(s0, tex -p1*float2(0, 2));	
    c0+= K[3] *tex2D(s0, tex +p1*float2(0, 3));
    c0+= K[3] *tex2D(s0, tex -p1*float2(0, 3));	
#elif Mode >1
	[unroll] for (int i=1; i < Mode; i++) {			
        c0+= tex2D(s0, tex + p1.y*float2(0, Offsets[i])) *K[i];
        c0+= tex2D(s0, tex - p1.y*float2(0, Offsets[i])) *K[i];
	}
#endif
	return c0;
}
#define Mode 1

/* blurGauss_pass1: Gaussian blur X (horiz. pass)
by butterw, License: GPL v3

First pass of 2-pass shader. As with all blur shaders, use pre-upscaling for maximum effect.  
blurGauss_pass1 (horiz. pass) >> blurGauss_pass2 (vert. pass)
Mode=1 uses a separable Gaussian Kernel, without hw linear sampling optimization 
default Kernel 7-tap Gaussian, sigma=1.5. A different sigma/Kernel K can be selected in the code. 


tested in mpc-hc v1.9.8.38  
- hw linear sampling (modes 2, 3, 4) doesn't work as expected (uses nearest neighbor sampling instead) !!!
- loading _pass1.hlsl now automatically loads _pass2.hlsl.

you can run the the 2-pass shader multiple times to achieve a stronger blur (ex: 3 times)
1=Blur Gaussian (3x)
PreResize1=.\blurGauss_pass1.hlsl;.\blurGauss_pass1.hlsl;.\blurGauss_pass1.hlsl

Separable Gaussian Kernel with linear sampling (9-tap filter approx.) 
http://rastergrid.com/blog/2010/09/efficient-gaussian-blur-with-linear-sampling/

*/
sampler s0: register(s0);
float2  p1: register(c1);

#if Mode==1 //gBlur7 (7 texture, 11 arithmetic) per pass
	// http://dev.theomader.com/gaussian-kernel-calculator/	
	//#define K	float4(0.324225, 0.233638, 0.087348, 0.016901) //sigma=1.2
	#define K	float4(0.266346, 0.215007, 0.113085, 0.038735) //sigma=1.5 (default) 
	//#define K	float4(0.230781, 0.198557, 0.126451, 0.059602) //sigma=1.8
#elif Mode==2 //hw.blur5 (3 texture, 6 arithmetic, 5 with tex +p1.x*float2(-Offsets[i], 0)) 
	#define Offsets float2(0.0, 4/3.) 
	#define K 		float2(0.29411764705882354, 0.35294117647058826)
#elif Mode==3 //hw.blur9 (5 texture, 8 arithmetic)
	#define Offsets float3(0.0, 1.3846153846, 3.2307692308)
	#define K		float3(0.2270270270, 0.3162162162, 0.0702702703)
#elif Mode==4 //hw.blur13
	#define Offsets float4(0.0, 1.411764705882353, 3.2941176470588234, 5.176470588235294)
	#define K		float4(0.1964825501511404, 0.2969069646728344, 0.09447039785044732, 0.010381362401148057)
#endif

/* --- Main --- */
float4 main(float2 tex: TEXCOORD0): COLOR { //(7 texture, 11 arithmetic)
	float4 c0 = K[0] *tex2D(s0, tex);
#if Mode==1
    c0+= K[1] *tex2D(s0, tex + float2(   p1.x, 0));
    c0+= K[1] *tex2D(s0, tex + float2(  -p1.x, 0));
    c0+= K[2] *tex2D(s0, tex + float2( 2*p1.x, 0));
    c0+= K[2] *tex2D(s0, tex + float2(-2*p1.x, 0));	
    c0+= K[3] *tex2D(s0, tex + float2( 3*p1.x, 0));
    c0+= K[3] *tex2D(s0, tex + float2(-3*p1.x, 0));	

#elif Mode >1	
	[unroll] for (int i=1; i < Mode; i++) {			
        c0+= tex2D(s0, tex +p1.x*float2(Offsets[i], 0)) *K[i];
        c0+= tex2D(s0, tex -p1.x*float2(Offsets[i], 0)) *K[i];
	}
#endif
	return c0;
}
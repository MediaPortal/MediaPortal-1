// $MinimumShaderProfile: ps_2_0
/*
 * (C) 2011 Jan-Willem Krans (janwillem32 <at> hotmail.com)
 * (C) 2011-2013 see Authors.txt
 *
 * This file is part of MPC-HC.
 *
 * MPC-HC is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * MPC-HC is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

// Correct video colorspace BT.601 [SD] to BT.709 [HD] for HD video input
// Use this shader only if BT.709 [HD] encoded video is incorrectly matrixed to full range RGB with the BT.601 [SD] colorspace.
// Run this shader before scaling.

sampler s0 : register(s0);
float4 p0 :  register(c0);

#define width  (p0[0])
#define height (p0[1])

static float4x4 rgb_ycbcr601 = {
	 0.299,     0.587,     0.114,    0.0,
	-0.168736, -0.331264,  0.5,      0.0,
	 0.5,      -0.418688, -0.081312, 0.0,
	 0.0,       0.0,       0.0,      0.0
};
static float4x4 ycbcr709_rgb = {
	1.0,  0.0,       1.5748,   0.0,
	1.0, -0.187324, -0.468124, 0.0,
	1.0,  1.8556,    0.0,      0.0,
	0.0,  0.0,       0.0,      0.0
};

float4 main(float2 tex : TEXCOORD0) : COLOR
{
	float4 c0 = tex2D(s0, tex); // original pixel
	// ATI driver only looks at the height
	if (height < 720) {
		return c0; // this shader does not alter SD video
	}
	c0 = mul(rgb_ycbcr601, c0); // convert RGB to Y'CbCr
	c0 = mul(ycbcr709_rgb, c0); // convert Y'CbCr to RGB
	
	return c0;
}

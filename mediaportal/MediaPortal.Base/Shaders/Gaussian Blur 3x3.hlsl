// $MinimumShaderProfile: ps_2_0
/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2013 see Authors.txt
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

sampler s0 : register(s0);
float4 p1 :  register(c1);

#define dx (p1[0])
#define dy (p1[1])

float4 main(float2 tex : TEXCOORD0) : COLOR
{
	// Pixels definition: original, blurred
	float4 orig;
	float4 blurred;

	// Get neighbor points
	// [ 1,    2, 3 ]
	// [ 4, orig, 5 ]
	// [ 6,    7, 8 ]

	orig = tex2D(s0, tex);
	float4 c1 = tex2D(s0, tex + float2(-dx, -dy));
	float4 c2 = tex2D(s0, tex + float2(  0, -dy));
	float4 c3 = tex2D(s0, tex + float2( dx, -dy));
	float4 c4 = tex2D(s0, tex + float2(-dx,   0));
	float4 c5 = tex2D(s0, tex + float2( dx,   0));
	float4 c6 = tex2D(s0, tex + float2(-dx,  dy));
	float4 c7 = tex2D(s0, tex + float2(  0,  dy));
	float4 c8 = tex2D(s0, tex + float2( dx,  dy));

	// Computation of the blurred image (gaussian filter)
	// to normalize the values, we need to divide by the coeff sum
	// 1/(1+2+1+2+4+2+1+2+1) = 1/16 = 0.0625
	blurred = (c1 + c3 + c6 + c8 + 2 * (c2 + c4 + c5 + c7) + 4 * orig) * 0.0625;
	return blurred;
}

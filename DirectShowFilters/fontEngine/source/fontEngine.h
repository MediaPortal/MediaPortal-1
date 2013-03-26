// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

enum BLEND_MODE
{
  BLEND_NONE = 0,
  BLEND_DIFFUSE = 1,
  BLEND_OVERLAY = 2
};

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the FONTENGINE_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// FONTENGINE_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef FONTENGINE_EXPORTS
#define FONTENGINE_API __declspec(dllexport)
#pragma message("use dllexport")
#else
#define FONTENGINE_API __declspec(dllimport)
#pragma message("use dllimport")
#endif
void FontEngineInitialize(int screenWidth, int screenHeight, int poolFormat);
int  FontEngineAddTexture(int hashCode, bool useAlphaBlend,void* texture);
int  FontEngineAddSurface(int hashCode, bool useAlphaBlend, void* surface);
void FontEngineRemoveTexture(int textureNo);
void FontEngineDrawTexture(int textureNo,float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax, unsigned int color);
void FontEngineDrawTexture2(int textureNo1,float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax, unsigned int color, float m[3][4], int textureNo2, float uoff2, float voff2, float umax2, float vmax2);
void FontEngineDrawMaskedTexture(int textureNo1,float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax, unsigned int color, float m[3][4], int textureNo2, float uoff2, float voff2, float umax2, float vmax2);
void FontEngineDrawMaskedTexture2(int textureNo1,float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax, unsigned int color, float m[3][4], int textureNo2, float uoff2, float voff2, float umax2, float vmax2, int textureNo3, float uoff3, float voff3, float umax3, float vmax3);
void FontEnginePresentTextures();

void FontEngineAddFont(void* device, int fontNumber,void* fontTexture, int firstChar, int endChar, float textureScale, float textureWidth, float textureHeight, float fSpacingPerChar,int maxVertices);
void FontEngineRemoveFont(int fontNumber);
void FontEngineSetCoordinate(int fontNumber, int index, int subindex, float fValue);
void FontEngineDrawText3D(int fontNumber, void* text, int xposStart, int yposStart, DWORD intColor,int maxWidth);
void FontEnginePresent3D(int fontNumber);
void FontEngineSetTexture(void* texture);
void FontEngineDrawSurface( int fx, int fy, int nw, int nh, 
                            int dstX, int dstY, int dstWidth, int dstHeight,void* surface);
void FontEngineSetClipEnable();
void FontEngineSetClipDisable();

void FontEngineSetRenderState(D3DRENDERSTATETYPE state, DWORD dwValue);
void FontEngineSetSamplerState(DWORD dwStage, D3DSAMPLERSTATETYPE d3dSamplerState, DWORD dwValue);
void FontEngineSetTextureStageState(DWORD dwStage, D3DTEXTURESTAGESTATETYPE d3dTextureStageState, DWORD dwValue);

HRESULT FontEngineSetMaximumFrameLatency(UINT maxLatency);

void PrintStatistics();
void Cleanup();

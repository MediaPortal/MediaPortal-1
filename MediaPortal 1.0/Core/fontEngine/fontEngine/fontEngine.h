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
void FontEngineDrawTexture(int textureNo,float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax, int color);
void FontEnginePresentTextures();

void FontEngineAddFont(void* device, int fontNumber,void* fontTexture, int firstChar, int endChar, float textureScale, float textureWidth, float textureHeight, float fSpacingPerChar,int maxVertices);
void FontEngineRemoveFont(int fontNumber);
void FontEngineSetCoordinate(int fontNumber, int index, int subindex, float fValue);
void FontEngineDrawText3D(int fontNumber, void* text, int xposStart, int yposStart, DWORD intColor,int maxWidth);
void FontEnginePresent3D(int fontNumber);
void FontEngineSetTexture(void* texture);
void FontEngineDrawSurface(int fx, int fy, int nw, int nh, 
						   int dstX, int dstY, int dstWidth, int dstHeight,
						   void* surface);

void PrintStatistics();
void Cleanup();
